# Wave 2 Phase 1 API Key Issue - Complete Analysis

**Date**: 2026-06-13  
**Status**: RESOLVED - Phase 1 completed successfully after fixing script generation bugs  
**Session**: Brainstorming analysis (no edits)

---

## Executive Summary

**The Good News**: Phase 1 is now **COMPLETE** (9/9 epics, 100% success). The API authentication issue was NOT an API key problem - it was a **script generation bug** that has been fixed.

**The Real Problem**: Phase 1 scripts were generated from scratch instead of copying Phase 0's proven pattern, introducing 3 critical bugs that caused authentication failures.

---

## What Actually Happened (Timeline)

### Phase 0: Success (8/9 epics)
- **Pattern**: Hardcoded API keys directly in scripts
- **Result**: 89% success rate (EPIC-CCN-112 failed due to jCodemunch timeout)
- **Key Learning**: The `--yolo` flag fix enabled file persistence

### Phase 1: Initial Failure (0/9 epics)
- **Pattern**: Used `jq` to extract API keys from JSON files dynamically
- **Result**: HTTP 401 "API Key verification failed"
- **Misdiagnosis**: Thought keys were revoked or rate-limited

### Phase 1: Root Cause Discovery
After 3 failed attempts, discovered the real issues:

1. **Bug #1**: Used `jq -r '.key'` instead of `jq -r '.apikey'`
   - Wrong JSON field name
   - Extracted `null` instead of actual key
   - Bob Shell received empty/null API key → 401 error

2. **Bug #2**: Used `bash -c "bash $script_name; exec bash"` in launcher
   - Non-login shell didn't load PATH
   - `bob: command not found` errors
   - Scripts couldn't even start

3. **Bug #3**: Generated scripts from scratch instead of copying Phase 0
   - Lost the proven hardcoded API key pattern
   - Introduced unnecessary complexity (jq extraction)
   - Violated the "copy working phase" principle

### Phase 1: Fix Applied
- **Solution**: Regenerated scripts using `generate_phase1_scripts.py`
  - Loads API keys from JSON files **locally** (not on VM)
  - **Hardcodes** keys into generated scripts (like Phase 0)
  - Uses correct field `.apikey`
  - Matches Phase 0 structure exactly
- **Launcher Fix**: Changed to `bash -l "$script_name"` (login shell)
- **Result**: ✅ 9/9 epics completed successfully

---

## API Key Allocation Analysis

### Current Allocation (Phase 1)

From `scripts/wave2/generate_phase1_scripts.py` (lines 11-21):

```python
EPICS = [
    ("107", "b (2).json", "HydrateFromOpenPositions", 31),
    ("108", "b.json", "ProcessOnExecutionUpdate", 67),
    ("109", "bob (1).json", "HydrateFSMsFromWorkingOrders", 45),
    ("110", "bob (2).json", "HandleFlatPositionUpdate", 37),
    ("111", "bob (3).json", "AdoptFleetOrders", 37),
    ("112", "bob (4).json", "ClassifyOrderByPrefix", 17),
    ("113", "bob (5).json", "SweepBrokerOrders", 28),
    ("114", "bob (6).json", "FlattenSinglePosition", 27),
    ("115", "bob.json", "ExecuteRetestEntry", 26),
]
```

### API Key Files Available

```
docs/API/
├── b (2).json       → EPIC-CCN-107
├── b.json           → EPIC-CCN-108
├── bob (1).json     → EPIC-CCN-109
├── bob (2).json     → EPIC-CCN-110
├── bob (3).json     → EPIC-CCN-111
├── bob (4).json     → EPIC-CCN-112
├── bob (5).json     → EPIC-CCN-113
├── bob (6).json     → EPIC-CCN-114
├── bob.json         → EPIC-CCN-115
└── sean.carter.jr@atomicmail.io.json (unused)
```

### Verification: Each Epic Has Unique API Key ✅

**Critical Check**: Are there any duplicate API key assignments?

Looking at the allocation:
- 9 epics
- 9 unique JSON files
- Each epic gets its own API key
- **No duplicates detected**

**Proof**: Phase 1 completed successfully with this allocation, confirming no quota contention.

---

## The "Similar Issue Before" Reference

You mentioned: *"we had to deal with something similar before"*

**You're referring to**: The **V12.25 Multi-Agent API Key Allocation Protocol** documented in `AGENTS.md` (lines 8-65).

### That Protocol Says:

```markdown
## ⚠️ CRITICAL: Multi-Agent API Key Allocation Protocol (V12.25)

**MANDATORY**: When launching ANY multi-agent workflow (Wave 2, parallel epics, distributed tasks), you MUST:

### 1. Check Previous Success
- **ALWAYS** read the last successful deployment script for that workflow
- **NEVER** create API allocations from scratch
- **COPY** the proven allocation exactly

### 2. Validate Before Launch
```python
# MANDATORY validation before ANY multi-agent launch
api_values = list(API_ALLOCATION.values())
if len(api_values) != len(set(api_values)):
    duplicates = [x for x in api_values if api_values.count(x) > 1]
    raise ValueError(f"DUPLICATE API KEYS DETECTED: {duplicates}")
print(f"✓ Validated {len(api_values)} unique API keys")
```

### 4. Violation Consequences
- **Quota Contention**: Agents compete for same API key
- **Stalled Execution**: Both agents blocked, no progress
- **Silent Failure**: API balances don't move, no error messages
```

### How This Applies to Phase 1

**Phase 1 violated rule #1**: "NEVER create API allocations from scratch"

The generator script (`generate_phase1_scripts.py`) was created fresh instead of copying Phase 0's pattern. This introduced the jq extraction bug that looked like an API authentication issue but was actually a script generation bug.

**The fix followed the protocol**: Copy Phase 0's proven pattern (hardcoded keys), just change the phase-specific content.

---

## Why The Error Message Was Misleading

### What We Saw
```
HTTP 401: Unauthorized
{"message":"API Key verification failed: API Key revoked or access denied","error":"unauthorized"}
```

### What We Thought
- Keys were revoked
- Rate limiting after Phase 0
- API service issue
- Keys expired

### What Actually Happened
```bash
# Phase 1 script (BROKEN)
export BOBSHELL_API_KEY=$(jq -r '.key' "$HOME/.bob/api-keys/b.json")
# Result: BOBSHELL_API_KEY="" (empty string, because field is .apikey not .key)

# Bob Shell receives empty API key → HTTP 401
```

**The 401 error was correct** - Bob Shell API rejected an empty/null API key. The error message was accurate, but we misinterpreted the root cause.

---

## The SOP That Prevents This

After Phase 1 debugging, we created: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP.md`

### Core Rule
**"NEVER generate phase scripts from scratch. ALWAYS copy the previous working phase and modify only what's necessary."**

### The Proven Pattern (Phase 0 Baseline)
```bash
#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_...'  # HARDCODED
mkdir -p docs/brain/EPIC-CCN-107
mkdir -p logs/phase0

cat > /tmp/phase0_msg_107.txt << 'EOFMSG'
[TASK-SPECIFIC INSTRUCTIONS HERE]
EOFMSG

bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_107.txt)" 2>&1 | tee logs/phase0/EPIC-CCN-107.log
echo "DONE_EXIT=$?"
```

### What Changes Between Phases
**ONLY these elements**:
- Script name: `_p0_*.sh` → `_p1_*.sh` → `_p2_*.sh`
- Log directory: `logs/phase0/` → `logs/phase1/` → `logs/phase2/`
- Message file: `/tmp/phase0_msg_*.txt` → `/tmp/phase1_msg_*.txt`
- Output file: `00-hotspots.md` → `00-scope.md` → `02-architecture-plan.md`
- Manifest phase: `"0"` → `"1"` → `"2"`
- Task description: Hotspot Analysis → Scope Definition → Architecture Planning
- Chat mode: `v12-phase0-hotspot` → `plan` → `plan` → `advanced`

**Everything else stays IDENTICAL**:
- ✅ API key loading (hardcoded)
- ✅ Directory structure
- ✅ Bob Shell invocation pattern
- ✅ Logging pattern
- ✅ Error handling

---

## Phase 1.5 Status

### Current State
You ran this command before the session froze:
```powershell
Get-ChildItem _p1_5_*.sh | ForEach-Object { 
    (Get-Content $_.FullName) -replace 'phase1','phase1_5' -replace 'Phase 1','Phase 1.5' -replace 'Scope Definition','Scope Boundary Validation' -replace '00-scope\.md','01-scope-boundary.md' -replace '"1"','"1.5"' | Set-Content $_.FullName 
}
```

### Issue Detected
Looking at `_p1_5_107.sh` line 33:
```bash
**Phase**: 1 (Scope Boundary Validation)
```

Should be:
```bash
**Phase**: 1.5 (Scope Boundary Validation)
```

**Problem**: The find-and-replace pattern `'"1"'` → `'"1.5"'` only matches quoted numbers. Line 33 has no quotes around the 1.

### Fix Needed
```powershell
# Fix the unquoted phase number
Get-ChildItem _p1_5_*.sh | ForEach-Object { 
    (Get-Content $_.FullName) -replace '\*\*Phase\*\*: 1 \(','**Phase**: 1.5 (' | Set-Content $_.FullName 
}
```

### Next Steps for Phase 1.5
1. ✅ Phase 1.5 scripts created (9 files)
2. ❌ Phase number fix needed (line 33 in all scripts)
3. ⏳ Launcher script needs to be created
4. ⏳ Deploy to VM
5. ⏳ Launch Phase 1.5

---

## Key Insights

### 1. The API Keys Were Never The Problem
- All 9 API keys are valid and have balance
- Each epic has a unique key (no duplicates)
- The 401 error was caused by empty/null key extraction, not revoked keys

### 2. Script Generation Was The Problem
- Phase 1 generator created scripts from scratch
- Introduced jq extraction instead of hardcoded keys
- Used wrong JSON field name (`.key` vs `.apikey`)
- Violated the "copy working phase" principle

### 3. The Fix Was Simple
- Load API keys locally during script generation
- Hardcode keys into generated scripts (like Phase 0)
- Use correct JSON field `.apikey`
- Match Phase 0 structure exactly

### 4. The SOP Prevents Recurrence
- Mandatory "copy working phase" rule
- Validation checklist before deployment
- Emergency recovery procedures
- Documented in `WAVE_PHASE_SCRIPT_GENERATION_SOP.md`

---

## Recommendations

### For Phase 1.5 and Beyond

1. **Follow the SOP religiously**
   - Copy Phase 1 scripts to Phase 1.5
   - Use find-and-replace for phase-specific changes
   - Don't regenerate from scratch

2. **Validate API key allocation**
   - Check for duplicates before launch
   - Verify each epic has unique key
   - Use the validation script from V12.25 protocol

3. **Test one script locally first**
   - Before deploying all 9 scripts
   - Verify structure matches working phase
   - Check for syntax errors

4. **Monitor for Greptile errors**
   - Non-blocking 403 errors are expected
   - Don't affect epic completion
   - Consider removing Greptile MCP in Wave 3+

### For Future Waves

1. **Create Phase 2+ scripts by copying Phase 1**
   - Don't use generator scripts
   - Manual copy + find-and-replace is safer
   - Faster and less error-prone

2. **Document working allocations**
   - Each successful phase becomes the template
   - Reference in comments: "Copied from Phase 1 (9/9 success)"
   - Build on success, don't start over

3. **Consider Wave 3+ consolidation**
   - Remove Greptile MCP (403 errors)
   - Optimize jCodemunch usage
   - Reduce API call overhead

---

## Conclusion

**The "API key issue" was actually a script generation bug.** The keys were always valid - we just weren't extracting them correctly.

**Phase 1 is now complete** (9/9 epics) after fixing the generator script to hardcode API keys like Phase 0.

**Phase 1.5 is ready** except for one minor fix (unquoted phase number on line 33).

**The SOP ensures this never happens again** by mandating "copy working phase, don't regenerate."

---

## Files Referenced

- `WAVE2_PHASE1_STATUS.md` - Original problem report
- `WAVE2_PHASE0_COMPLETION_REPORT.md` - Phase 0 success baseline
- `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP.md` - Prevention protocol
- `scripts/wave2/generate_phase1_scripts.py` - Fixed generator
- `AGENTS.md` (lines 8-65) - V12.25 API Key Allocation Protocol
- `.bob/skills/gcp-vm-wave-execution/skill.md` - Updated skill docs

---

**Status**: Ready for Phase 1.5 after minor fix to line 33 in all `_p1_5_*.sh` scripts.