# Wave 4 CRLF Line Ending Incident Report

**Date**: 2026-06-16T00:32:00Z  
**Severity**: P2 (Blocking - caused immediate script failures)  
**Status**: RESOLVED  
**Impact**: 4 Phase 6 scripts failed to execute (EPIC-CCN-003, 015, 030, 045)

## Incident Summary

Regenerated Phase 6 scripts created on Windows with Python had CRLF line endings, causing immediate failure on Linux VM with error: `/bin/bash^M: bad interpreter: No such file or directory`

## Timeline

| Time (UTC) | Event |
|------------|-------|
| 00:24:39 | Phase 6 scripts regenerated using `regenerate_p6_scripts.py` on Windows |
| 00:25:47 | Scripts uploaded to VM with V12.27 verification (4/4 confirmed) |
| 00:28:47 | Phase 6 launched for 4 epics |
| 00:31:58 | CHECK 1: All 4 scripts failed instantly (0 sessions, 0 files) |
| 00:32:16 | Log inspection revealed CRLF error (`^M` character) |
| 00:32:43 | Created `fix_line_endings.sh` to convert CRLF → LF |
| 00:32:58 | Uploaded fix script to VM |
| 00:33:15 | Executed fix script - all 4 scripts converted to LF |
| 00:33:25 | Re-launched Phase 6 (attempt #3) |
| 00:34:19 | CHECK 1: 4 screen sessions running (SUCCESS) |

**Total Downtime**: ~6 minutes (from launch to successful re-launch)

## Root Cause

**Primary**: Python `Path.write_text()` on Windows defaults to CRLF line endings  
**Secondary**: No line ending validation in upload verification protocol (V12.27)

### Why It Happened

1. **Script Generation**: `regenerate_p6_scripts.py` used `Path.write_text()` which respects OS default
2. **Windows Default**: Windows uses CRLF (`\r\n`) for line endings
3. **Linux Requirement**: Bash scripts require LF (`\n`) only
4. **Silent Failure**: Upload verification (V12.27) only checks file count, not content validity
5. **Instant Failure**: Bash interpreter rejects CRLF shebang immediately

### Error Manifestation

```bash
bash: ./scripts/wave4/_p6_003.sh: /bin/bash^M: bad interpreter: No such file or directory
```

The `^M` character is the visible representation of `\r` (carriage return).

## Impact Analysis

### Immediate Impact
- ✅ **Contained**: Only 4 scripts affected (regenerated batch)
- ✅ **Fast Detection**: Caught in CHECK 1 (T+1min)
- ✅ **Fast Resolution**: Fixed in 6 minutes
- ✅ **No Data Loss**: All files persisted correctly

### Potential Impact (If Undetected)
- ❌ Could have affected all 79 Phase 6 scripts if regeneration was wave-wide
- ❌ Would have required re-upload of all scripts
- ❌ Could have delayed wave completion by hours

## Resolution

### Immediate Fix (Applied)

Created `fix_line_endings.sh` on VM:
```bash
#!/bin/bash
cd /home/malhitticrypto/universal-or-strategy/scripts/wave4

for f in _p6_003.sh _p6_015.sh _p6_030.sh _p6_045.sh; do
    sed -i 's/\r$//' "$f"  # Strip trailing \r
    echo "Fixed: $f"
done

chmod +x _p6_003.sh _p6_015.sh _p6_030.sh _p6_045.sh
```

**Verification**:
```bash
file _p6_003.sh
# Output: Bourne-Again shell script, ASCII text executable (no CRLF)
```

### Long-Term Prevention

**Protocol Update Required**: V12.29 - Line Ending Validation

#### 1. Update Script Generation Template

**File**: `building-blocks/autonomous-refactoring/PHASE_SCRIPT_TEMPLATE.py`

Add explicit LF line ending enforcement:
```python
#!/usr/bin/env python3
"""Generate phase scripts with MANDATORY Unix LF line endings."""

from pathlib import Path

def write_script_unix_lf(path: Path, content: str):
    """Write script with Unix LF line endings, regardless of OS."""
    # Normalize to LF only
    content_lf = content.replace('\r\n', '\n').replace('\r', '\n')
    
    # Write in binary mode to prevent OS line ending conversion
    path.write_bytes(content_lf.encode('utf-8'))
    
    print(f"Generated: {path} (Unix LF)")

# Usage in script generation
script_content = """#!/bin/bash
# Phase X script
set -e
...
"""

output_path = Path(f'scripts/wave4/_pX_001.sh')
write_script_unix_lf(output_path, script_content)
```

#### 2. Update Upload Verification Protocol (V12.27 → V12.29)

**File**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`

Add Step 5.5 (Line Ending Verification):
```bash
# After upload, verify line endings on VM
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy/scripts/wave{N} && \
  file _p{X}_*.sh | grep -c 'CRLF' && echo 'ERROR: CRLF detected' || echo 'OK: Unix LF'"

# Expected output: "OK: Unix LF"
# If CRLF detected: STOP and fix before launch
```

#### 3. Add Pre-Upload Local Validation

**File**: `scripts/wave4/validate_scripts_before_upload.ps1`

```powershell
#!/usr/bin/env pwsh
# Validate scripts have Unix LF line endings before upload

$scripts = Get-ChildItem "scripts/wave4/_p*.sh"
$crlf_count = 0

foreach ($script in $scripts) {
    $content = [System.IO.File]::ReadAllBytes($script.FullName)
    $has_crlf = $content -contains 13  # \r (carriage return)
    
    if ($has_crlf) {
        Write-Host "ERROR: CRLF detected in $($script.Name)" -ForegroundColor Red
        $crlf_count++
    }
}

if ($crlf_count -gt 0) {
    Write-Host "`nFAILURE: $crlf_count scripts have CRLF line endings" -ForegroundColor Red
    Write-Host "Run: dos2unix scripts/wave4/_p*.sh" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "SUCCESS: All scripts have Unix LF line endings" -ForegroundColor Green
    exit 0
}
```

#### 4. Update Building-Blocks Method Documentation

**File**: `building-blocks/autonomous-refactoring/GETTING_STARTED.md`

Add section:
```markdown
### Line Ending Requirements (V12.29)

**CRITICAL**: All bash scripts MUST use Unix LF line endings (`\n`), never Windows CRLF (`\r\n`).

**Enforcement**:
1. Generate scripts using `write_script_unix_lf()` helper (binary mode write)
2. Validate locally before upload: `validate_scripts_before_upload.ps1`
3. Verify on VM after upload: `file _p*.sh | grep -c 'CRLF'`
4. If CRLF detected: Fix with `sed -i 's/\r$//' script.sh`

**Why This Matters**:
- Bash interpreter rejects CRLF shebang (`#!/bin/bash^M`)
- Silent failure: Scripts appear uploaded but won't execute
- Wave-wide impact: Could block all 80 epics if undetected
```

## Lessons Learned

### What Went Well ✅
1. **Fast Detection**: V2.0 monitoring caught failure in CHECK 1 (T+1min)
2. **Clear Error Message**: Log showed exact issue (`^M` character)
3. **Fast Resolution**: Fixed in 6 minutes with sed command
4. **Contained Scope**: Only 4 scripts affected (not entire wave)
5. **No Data Loss**: All files persisted through fix

### What Went Wrong ❌
1. **No Line Ending Validation**: V12.27 only checks file count, not content
2. **OS-Dependent Generation**: Python script used OS default line endings
3. **No Pre-Upload Check**: Scripts uploaded without local validation
4. **Regeneration Risk**: Building-blocks method bypassed for 4 scripts

### Action Items

| # | Action | Owner | Priority | Status |
|---|--------|-------|----------|--------|
| 1 | Update script generation template with `write_script_unix_lf()` | Wave Lead | P0 | TODO |
| 2 | Add line ending check to V12.27 upload verification | Wave Lead | P0 | TODO |
| 3 | Create `validate_scripts_before_upload.ps1` | Wave Lead | P1 | TODO |
| 4 | Update building-blocks documentation with line ending requirements | Wave Lead | P1 | TODO |
| 5 | Add line ending validation to pre-push checks | Wave Lead | P2 | TODO |
| 6 | Document incident in Wave 4 completion report | Wave Lead | P2 | TODO |

## Prevention Checklist (V12.29)

Before every script generation:
- [ ] Use `write_script_unix_lf()` helper (binary mode write)
- [ ] Run `validate_scripts_before_upload.ps1` locally
- [ ] Verify on VM: `file _p*.sh | grep -c 'CRLF'` (expect 0)
- [ ] Test pilot script before wave launch
- [ ] Document any line ending issues in session notes

## Related Incidents

- **Wave 4 Upload Gap** (V12.27): 7 scripts never uploaded (silent glob failure)
- **Wave 4 Filename Pattern** (V12.26): Prerequisite checks rejected valid files
- **Wave 4 API Key Revocation** (V12.28): Revoked key blocked 3 epics

**Pattern**: Silent failures in script generation/upload pipeline require multi-layer validation.

## References

- **Fix Script**: `scripts/wave4/fix_line_endings.sh`
- **Regeneration Script**: `scripts/wave4/regenerate_p6_scripts.py` (needs update)
- **Upload Verification**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md` (V3.1)
- **Building-Blocks Method**: `building-blocks/autonomous-refactoring/GETTING_STARTED.md`

## Status

**RESOLVED**: All 4 scripts fixed and re-launched successfully at 00:33:25 UTC.  
**Monitoring**: CHECK 1 confirmed 4 screen sessions running (00:34:19 UTC).  
**Next Check**: CHECK 2 at T+4min (00:38 UTC).

---

**Incident Report Version**: 1.0  
**Last Updated**: 2026-06-16T00:34:00Z  
**Maintainer**: Wave 4 Execution Lead  
**Status**: 🟢 RESOLVED - Prevention protocol pending (V12.29)