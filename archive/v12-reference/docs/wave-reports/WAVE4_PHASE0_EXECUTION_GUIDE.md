# Wave 4 Phase 0 Execution Guide

**Date**: 2026-06-15
**Version**: 1.0 (With Jane Street Integration)
**Status**: Ready for Execution

---

## Executive Summary

✅ **All Phase 0 scripts generated with Jane Street validation**
- 80 individual epic scripts (`_p0_001.sh` through `_p0_080.sh`)
- Master launcher with 12-second staggered delays
- Python execution wrapper with violation detection
- Local testing successful (EPIC-CCN-001)

---

## What's Different from Previous Waves

### Jane Street Integration (NEW)

**Previous Waves**: No Jane Street validation
- Phase 0 completed WITHOUT checking 299 P0 violations
- Violations discovered AFTER refactoring
- Required rework and re-validation

**Wave 4**: Jane Street validation MANDATORY
- Every Phase 0 execution checks violations in target file
- Violation count included in hotspot analysis
- Risk level elevated if violations >5
- Ensures violations are fixed DURING refactoring, not after

### Execution Model

**Script Flow**:
```
_p0_001.sh
  ↓
execute_phase0_with_jane_street.py
  ↓
jane_street_utils.py (load violations)
  ↓
Generate: 00-hotspots.md + manifest.json
```

**Output Files**:
- `docs/brain/EPIC-CCN-001/00-hotspots.md` - Includes violation count
- `docs/brain/EPIC-CCN-001/manifest.json` - Tracks violations found

---

## Pre-Execution Checklist

### Local Environment

- [x] Scripts generated (`scripts/wave4/_p0_*.sh`)
- [x] Python wrapper created (`scripts/wave4/execute_phase0_with_jane_street.py`)
- [x] Jane Street utils available (`scripts/jane_street_utils.py`)
- [x] Violations file exists (`jane_street_p0_violations.json`)
- [x] Local test passed (EPIC-CCN-001)

### VM Environment (User Action Required)

- [ ] VM accessible: `gcloud compute ssh v12-test-golden-v2`
- [ ] Build passes: `dotnet build`
- [ ] Git status clean: `git status` (no uncommitted `src/` changes)
- [ ] Branch strategy: GitButler virtual branches active
- [ ] Python 3 available: `python3 --version`
- [ ] Jane Street violations file uploaded

---

## Step-by-Step Execution

### Step 1: Upload Scripts to VM

```bash
# Upload Phase 0 scripts (80 files)
gcloud compute scp scripts/wave4/_p0_*.sh v12-test-golden-v2:~/universal-or-strategy/

# Upload master launcher
gcloud compute scp scripts/wave4/launch_phase0_all.sh v12-test-golden-v2:~/universal-or-strategy/

# Upload Python wrapper
gcloud compute scp scripts/wave4/execute_phase0_with_jane_street.py v12-test-golden-v2:~/universal-or-strategy/scripts/wave4/

# Upload Jane Street utils (if not already on VM)
gcloud compute scp scripts/jane_street_utils.py v12-test-golden-v2:~/universal-or-strategy/scripts/

# Upload violations file (if not already on VM)
gcloud compute scp jane_street_p0_violations.json v12-test-golden-v2:~/universal-or-strategy/
```

**Expected Time**: 2-3 minutes

### Step 2: Set Permissions

```bash
gcloud compute ssh v12-test-golden-v2 --command='chmod +x ~/universal-or-strategy/_p0_*.sh ~/universal-or-strategy/launch_phase0_all.sh'
```

**Expected Time**: 5 seconds

### Step 3: Pilot Test (EPIC-CCN-001 Only)

```bash
# Test single epic first
gcloud compute ssh v12-test-golden-v2 --command='cd ~/universal-or-strategy && ./_p0_001.sh'
```

**Expected Output**:
```
[OK] Phase 0 complete for EPIC-CCN-001
   - Hotspots: docs/brain/EPIC-CCN-001/00-hotspots.md
   - Manifest: docs/brain/EPIC-CCN-001/manifest.json
   - Jane Street Violations: 0
```

**Verification**:
```bash
# Check files created
gcloud compute ssh v12-test-golden-v2 --command='ls -lh ~/universal-or-strategy/docs/brain/EPIC-CCN-001/'

# Expected: 00-hotspots.md, manifest.json
```

**If Pilot Fails**: STOP. Debug before launching full wave.

### Step 4: Launch Full Wave (All 80 Epics)

```bash
# Launch all 80 epics with 12-second delays
gcloud compute ssh v12-test-golden-v2 --command='cd ~/universal-or-strategy && ./launch_phase0_all.sh'
```

**Expected Output**:
```
[2026-06-15 02:30:00] Starting Phase 0 launch for 80 epics
[2026-06-15 02:30:00] Using 12-second delays between launches
[2026-06-15 02:30:00] Launching EPIC-CCN-001 (1/80)
[2026-06-15 02:30:12] Launching EPIC-CCN-002 (2/80)
...
[2026-06-15 02:46:00] All 80 epics launched for Phase 0
[2026-06-15 02:46:00] Total launch time: 16 minutes
```

**Launch Time**: 16 minutes (80 epics × 12 seconds)

### Step 5: Monitor Execution

**Polling Protocol** (MANDATORY):
1. **Wait 1 minute** after launch completes
2. **Check screen sessions**: `screen -ls`
3. **Poll every 4 minutes** until all complete

**Commands**:
```bash
# Check running sessions (expect "No Sockets found" when done)
gcloud compute ssh v12-test-golden-v2 --command='screen -ls'

# Count completed files (expect 80)
gcloud compute ssh v12-test-golden-v2 --command='ls ~/universal-or-strategy/docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l'

# Check for errors
gcloud compute ssh v12-test-golden-v2 --command='grep -i "error\|failed\|exception" ~/universal-or-strategy/logs/phase0/*.log | head -20'
```

**Expected Duration**: 10-15 minutes (parallel execution)

### Step 6: Verify Completion

```bash
# Verify all 80 files created
gcloud compute ssh v12-test-golden-v2 --command='ls ~/universal-or-strategy/docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l'

# Expected: 80

# Verify all manifests created
gcloud compute ssh v12-test-golden-v2 --command='ls ~/universal-or-strategy/docs/brain/EPIC-CCN-*/manifest.json 2>/dev/null | wc -l'

# Expected: 80

# Extract Jane Street violation counts
gcloud compute ssh v12-test-golden-v2 --command='grep "Jane Street Violations:" ~/universal-or-strategy/docs/brain/EPIC-CCN-*/00-hotspots.md'
```

**Success Criteria**:
- ✅ 80 hotspot files created
- ✅ 80 manifest files created
- ✅ All files include Jane Street violation counts
- ✅ No errors in logs

---

## Jane Street Validation Results

### Expected Violation Distribution

Based on the 299 P0 violations across the codebase:

**Files with Violations**:
- `V12_002.Symmetry.Replace.cs` - 0 violations (tested)
- `V12_002.UI.Compliance.cs` - TBD
- `V12_002.SIMA.Lifecycle.cs` - TBD
- `V12_002.Orders.Callbacks.cs` - TBD
- (Other files TBD)

**Post-Execution Analysis**:
```bash
# Count epics with violations
gcloud compute ssh v12-test-golden-v2 --command='grep -c "Jane Street Violations: [1-9]" ~/universal-or-strategy/docs/brain/EPIC-CCN-*/00-hotspots.md'

# List high-risk epics (>5 violations)
gcloud compute ssh v12-test-golden-v2 --command='grep -B 5 "Jane Street Risk: HIGH" ~/universal-or-strategy/docs/brain/EPIC-CCN-*/00-hotspots.md'
```

---

## Troubleshooting

### Issue: Script Fails with "ModuleNotFoundError: jane_street_utils"

**Cause**: Python wrapper can't find jane_street_utils.py

**Fix**:
```bash
# Verify file exists on VM
gcloud compute ssh v12-test-golden-v2 --command='ls -lh ~/universal-or-strategy/scripts/jane_street_utils.py'

# If missing, upload it
gcloud compute scp scripts/jane_street_utils.py v12-test-golden-v2:~/universal-or-strategy/scripts/
```

### Issue: Script Fails with "FileNotFoundError: jane_street_p0_violations.json"

**Cause**: Violations file not on VM

**Fix**:
```bash
# Upload violations file
gcloud compute scp jane_street_p0_violations.json v12-test-golden-v2:~/universal-or-strategy/
```

### Issue: "UnicodeEncodeError" in Output

**Cause**: Windows console encoding (already fixed in script)

**Status**: ✅ Fixed - using ASCII "[OK]" instead of Unicode checkmark

### Issue: Files Not Created

**Cause**: Script execution failed silently

**Diagnosis**:
```bash
# Check logs for specific epic
gcloud compute ssh v12-test-golden-v2 --command='cat ~/universal-or-strategy/logs/phase0/EPIC-CCN-001.log'
```

**Fix**: Debug based on error message in log

---

## Post-Execution Actions

### Immediate (After Phase 0 Complete)

1. **Sync Files to Local**:
```bash
# Download all hotspot files
gcloud compute scp --recurse v12-test-golden-v2:~/universal-or-strategy/docs/brain/ ./docs/

# Download logs
gcloud compute scp --recurse v12-test-golden-v2:~/universal-or-strategy/logs/phase0/ ./logs/
```

2. **Analyze Violation Distribution**:
```bash
# Count epics by violation count
grep "Jane Street Violations:" docs/brain/EPIC-CCN-*/00-hotspots.md | \
  awk -F': ' '{print $2}' | sort | uniq -c
```

3. **Update Roadmap**:
```bash
# Mark Phase 0 complete for all 80 epics
python scripts/update_roadmap_status.py --phase 0 --status completed
```

### Deferred (Before Phase 1)

1. **Validate Jane Street Integration**:
   - Verify all epics have violation counts
   - Confirm high-risk epics flagged correctly
   - Document any files with >10 violations

2. **Review High-Risk Epics**:
   - Identify epics with >5 violations
   - Plan additional scrutiny for these during Phase 2

3. **Update Wave 4 Lessons Learned**:
   - Document Jane Street integration effectiveness
   - Note any violations missed by file-level detection
   - Recommend improvements for future waves

---

## Success Metrics

### Phase 0 Completion

- ✅ 80/80 epics completed
- ✅ 80/80 hotspot files created
- ✅ 80/80 manifest files created
- ✅ Jane Street violations documented for all epics
- ✅ No P0 blockers

### Jane Street Integration

- ✅ Violation detection working
- ✅ Risk levels assigned correctly
- ✅ High-risk epics flagged (>5 violations)
- ✅ Zero false negatives (all violations caught)

### Execution Efficiency

- ✅ Launch time: 16 minutes (as expected)
- ✅ Execution time: 10-15 minutes (parallel)
- ✅ Total time: ~30 minutes (vs 80 minutes sequential)
- ✅ No manual intervention required

---

## Next Steps

After Phase 0 completion:

1. **Phase 1 (Scope + Boundary)**: Use same pattern
   - Copy Phase 0 scripts
   - Update to call Phase 1 MCP server
   - Add Jane Street KB query for scope validation

2. **Phase 2 (Architecture)**: Automated Jane Street hooks
   - Firebase KB integration in MCP server
   - Automatic pattern matching
   - Extraction plan validation

3. **Phases 3-6**: Continue pattern
   - Each phase includes Jane Street validation
   - Cumulative violation tracking
   - Final verification in Phase 6

---

## Critical Reminders

### V12.23 No Scope Creep Protocol

**ONE EPIC = ONE CONCERN**
- ❌ Fixing pre-existing compilation errors during epic
- ❌ Adding "while we're here" improvements
- ❌ Bundling multiple concerns in one commit

### Jane Street Mandate

**ALL 299 violations MUST be fixed**
- Every file touched must have zero violations after refactoring
- No new violations introduced
- Verification in Phase 5.V and Phase 6

### Polling Protocol

**12-second delays, 1 min + 4 min intervals**
- Launch: 12 seconds between epics
- First poll: 1 minute after launch complete
- Subsequent polls: Every 4 minutes
- Cost optimization: 88% reduction vs 30-second polling

---

## Document Version

**Version**: 1.0
**Last Updated**: 2026-06-15T02:24:00Z
**Status**: Ready for Execution
**Maintainer**: V12 Orchestration Team

---

**Ready to proceed with Wave 4 Phase 0 execution!** 🚀