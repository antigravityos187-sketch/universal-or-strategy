# Wave 4 Recovery Plan - Achieving 80/80 (100%)

**Date**: 2026-06-15  
**Objective**: Complete all 11 incomplete epics to achieve 80/80 (100%)  
**Current Status**: 69/80 (86.25%)  
**Target**: 80/80 (100%)

---

## Incomplete Epics Breakdown

### Category 1: EPIC-CCN-016 (Deferred - Scope Mismatch)
**Status**: Phase 5 marked as "deferred" due to scope mismatch  
**Root Cause**: Epic scope didn't match actual code structure  
**Recovery Action**: Re-run Phase 1 (Scope) and Phase 1.5 (Boundary) to re-scope correctly

### Category 2: Phase 5 Failures (7 epics)
**Epics**: EPIC-CCN-003, 015, 030, 031, 033, 042, 055  
**Status**: No Phase 5 completion files created  
**Root Cause**: Unknown - need to check Phase 5 logs  
**Recovery Action**: Re-run Phase 5 for each epic

### Category 3: Phase 6 Failures (3 epics)
**Epics**: EPIC-CCN-012, 027, 045  
**Status**: Phase 5 complete, Phase 6 failed  
**Root Cause**: `bob: command not found` in screen sessions (PATH issue)  
**Recovery Action**: Re-run Phase 6 with fixed PATH or manual verification

---

## Root Cause Analysis

### Why Recovery Loop Didn't Catch These

**Recovery Loop Protocol V12.26** states:
> "NEVER proceed to next phase with <100% completion. Loop failed epics until they catch up with cohort."

**What Happened**:
1. **Phase 5**: Recovery loop WAS applied (see `WAVE4_PHASE5_RECOVERY_REPORT.md`)
   - Initial: 5/80 success
   - After recovery: 79/80 success
   - **BUT**: Final count shows only 72/79 actually succeeded
   - **Gap**: 7 epics (003, 015, 030, 031, 033, 042, 055) were counted as "success" but didn't create completion files

2. **Phase 6**: Recovery loop WAS applied (2 rounds)
   - Round 1: 68/79 success
   - Round 2: 69/79 success
   - **BUT**: 3 epics (012, 027, 045) persistently failed
   - **Gap**: Recovery stopped after 2 rounds instead of continuing until 100%

### Protocol Violations Identified

1. **Phase 5 File Verification Gap**: Success was determined by script exit code, not file existence
2. **Phase 6 Recovery Threshold**: Stopped at 95.8% instead of continuing to 100%
3. **No Cross-Phase Validation**: Phase 6 didn't verify Phase 5 prerequisites before starting

---

## Recovery Strategy

### Step 1: Analyze Phase 5 Failures (7 epics)

**Action**: Check Phase 5 logs on VM to understand why these failed

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && for epic in EPIC-CCN-003 EPIC-CCN-015 EPIC-CCN-030 EPIC-CCN-031 EPIC-CCN-033 EPIC-CCN-042 EPIC-CCN-055; do echo '=== '$epic' ==='; tail -50 logs/phase5/$epic.log 2>/dev/null || echo 'No log'; done"
```

### Step 2: Re-scope EPIC-CCN-016

**Phases to Re-run**:
1. Phase 1 (Scope Definition)
2. Phase 1.5 (Scope Boundary Validation)
3. Phase 2 (Architecture Planning)
4. Phase 3 (DNA & PR Audit)
5. Phase 4 (Ticket Generation)
6. Phase 5 (Execution)
7. Phase 6 (Verification)

**Script**: Create `scripts/wave4/recover_epic_016.sh`

### Step 3: Recover Phase 5 Failures (7 epics)

**Phases to Re-run**:
1. Phase 5 (Execution) - with enhanced logging
2. Phase 6 (Verification) - after Phase 5 succeeds

**Script**: Create `scripts/wave4/recover_phase5_failures.py`

### Step 4: Recover Phase 6 Failures (3 epics)

**Options**:
1. **Option A**: Fix PATH issue and re-run Phase 6 scripts
2. **Option B**: Manual verification using Phase 5 completion files

**Script**: Create `scripts/wave4/recover_phase6_failures.sh`

---

## Enhanced Recovery Scripts

### Script 1: Analyze Phase 5 Failures

```python
#!/usr/bin/env python3
"""Analyze why 7 epics failed Phase 5."""

import subprocess
from pathlib import Path

failed_epics = [
    "EPIC-CCN-003", "EPIC-CCN-015", "EPIC-CCN-030", 
    "EPIC-CCN-031", "EPIC-CCN-033", "EPIC-CCN-042", "EPIC-CCN-055"
]

print("=== PHASE 5 FAILURE ANALYSIS ===\n")

for epic_id in failed_epics:
    print(f"--- {epic_id} ---")
    
    # Check if Phase 4 tickets exist
    tickets_file = Path(f"docs/brain/{epic_id}/04-tickets.md")
    if not tickets_file.exists():
        print(f"❌ Missing prerequisite: {tickets_file}")
        continue
    
    # Check Phase 5 log
    log_file = Path(f"logs/phase5/{epic_id}.log")
    if log_file.exists():
        # Extract last 30 lines with errors
        with open(log_file) as f:
            lines = f.readlines()
            error_lines = [l for l in lines if "ERROR" in l or "FAIL" in l]
            if error_lines:
                print("Errors found:")
                for line in error_lines[-5:]:
                    print(f"  {line.strip()}")
            else:
                print("No explicit errors in log")
                print(f"Last 5 lines:")
                for line in lines[-5:]:
                    print(f"  {line.strip()}")
    else:
        print(f"❌ No log file: {log_file}")
    
    print()
```

### Script 2: Recover Phase 5 Failures

```python
#!/usr/bin/env python3
"""Re-run Phase 5 for 7 failed epics with enhanced verification."""

import subprocess
import time
from pathlib import Path

failed_epics = [
    "EPIC-CCN-003", "EPIC-CCN-015", "EPIC-CCN-030",
    "EPIC-CCN-031", "EPIC-CCN-033", "EPIC-CCN-042", "EPIC-CCN-055"
]

print(f"=== PHASE 5 RECOVERY (7 epics) ===\n")

for i, epic_id in enumerate(failed_epics, 1):
    epic_num = epic_id.split('-')[-1]
    
    print(f"[{i}/7] Launching {epic_id}")
    
    # Launch in screen session with enhanced logging
    cmd = [
        "screen", "-dmS", f"p5-recovery-{epic_num}",
        "bash", "-l", "-c",
        f"./scripts/wave4/_p5_{epic_num}.sh 2>&1 | tee logs/phase5/{epic_id}-recovery.log"
    ]
    
    subprocess.run(cmd, check=True)
    
    # Staggered delay
    if i < len(failed_epics):
        print(f"  Waiting 12 seconds...")
        time.sleep(12)

print(f"\n=== LAUNCHED 7 EPICS ===")
print(f"Monitor: screen -ls | grep p5-recovery")
print(f"Check: ls docs/brain/EPIC-CCN-{{003,015,030,031,033,042,055}}/05-*.md")
```

### Script 3: Fix Phase 6 PATH Issue

```bash
#!/bin/bash
# Fix PATH issue for Phase 6 failures

# Test PATH in screen session
screen -dmS path-test bash -l -c 'echo $PATH > /tmp/screen-path.txt'
sleep 2
cat /tmp/screen-path.txt

# If bob not in PATH, add explicit path to scripts
for num in 012 027 045; do
    sed -i 's|bob --yolo|/home/malhitticrypto/.local/bin/bob --yolo|g' scripts/wave4/_p6_${num}.sh
done

echo "PATH fix applied to Phase 6 scripts"
```

---

## Protocol Hardening

### 1. Enhanced File Verification

**Update**: `scripts/wave4/verify_phase_completion.py`

```python
def verify_phase_completion(epic_id, phase):
    """Verify phase completion by checking file existence, not just exit code."""
    expected_files = {
        0: ["00-hotspots.md"],
        1: ["01-scope.md"],
        1.5: ["01-scope-boundary.md"],
        2: ["02-architecture-plan.md"],
        3: ["03-audit-report.md"],
        4: ["04-tickets.md"],
        5: ["05-*.md", "ticket-*-completion.md"],  # Flexible patterns
        6: ["06-*.md"]
    }
    
    brain_dir = Path(f"docs/brain/{epic_id}")
    patterns = expected_files[phase]
    
    for pattern in patterns:
        files = list(brain_dir.glob(pattern))
        if files:
            return True  # At least one pattern matched
    
    return False  # No files found
```

### 2. Recovery Loop Enhancement

**Update**: `docs/protocol/RECOVERY_LOOP_PROTOCOL.md`

Add section:
```markdown
## Mandatory 100% Completion

Recovery loop MUST continue until 100% completion is achieved:

1. After each phase, verify file existence (not just exit code)
2. If <100%, identify failed epics
3. Analyze root causes
4. Generate recovery scripts
5. Execute recovery
6. REPEAT steps 1-5 until 100%
7. Maximum 5 recovery rounds before escalation to manual intervention

**No exceptions**: 95% is not acceptable. 99% is not acceptable. Only 100%.
```

### 3. Cross-Phase Validation

**New Script**: `scripts/wave4/validate_prerequisites.py`

```python
def validate_prerequisites(epic_id, phase):
    """Validate all prerequisite files exist before starting phase."""
    prerequisites = {
        1: [0],  # Phase 1 requires Phase 0
        1.5: [0, 1],  # Phase 1.5 requires Phases 0, 1
        2: [0, 1, 1.5],
        3: [0, 1, 1.5, 2],
        4: [0, 1, 1.5, 2, 3],
        5: [0, 1, 1.5, 2, 3, 4],
        6: [0, 1, 1.5, 2, 3, 4, 5]
    }
    
    for prereq_phase in prerequisites.get(phase, []):
        if not verify_phase_completion(epic_id, prereq_phase):
            raise ValueError(f"{epic_id}: Missing prerequisite Phase {prereq_phase}")
    
    return True
```

### 4. Skill Updates

**File**: `.bob/skills/gcp-vm-wave-execution/skill.md`

Add section:
```markdown
## Mandatory 100% Completion Protocol

CRITICAL: Recovery loop MUST achieve 100% completion before proceeding to next phase.

**Verification Steps**:
1. Count files: `ls docs/brain/EPIC-CCN-*/XX-*.md | wc -l`
2. Compare to target: Must equal number of epics in wave
3. If <100%: Run `identify_failed_epics.py`
4. Analyze logs: Check for root causes
5. Generate recovery scripts
6. Execute recovery
7. REPEAT until 100%

**No Exceptions**: 95% is failure. 99% is failure. Only 100% is success.
```

### 5. SOP Updates

**File**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`

Add section:
```markdown
## Post-Phase Verification (MANDATORY)

After EVERY phase execution:

1. **File Count Verification**:
   ```bash
   EXPECTED=80  # Or number of epics in wave
   ACTUAL=$(ls docs/brain/EPIC-CCN-*/XX-*.md 2>/dev/null | wc -l)
   if [ $ACTUAL -ne $EXPECTED ]; then
       echo "FAILURE: $ACTUAL/$EXPECTED files created"
       exit 1
   fi
   ```

2. **File Content Verification**:
   - Check file size >1KB
   - Check for error markers in content
   - Verify required sections present

3. **Recovery Loop**:
   - If verification fails, MUST run recovery
   - Maximum 5 recovery rounds
   - Escalate to manual if still <100%
```

---

## Execution Plan

### Phase 1: Analysis (30 minutes)
1. Run `analyze_phase5_failures.py` on VM
2. Check Phase 5 logs for 7 failed epics
3. Document root causes

### Phase 2: EPIC-CCN-016 Re-scoping (2 hours)
1. Re-run Phases 1, 1.5, 2, 3, 4, 5, 6
2. Use manual mode for Phase 1 to ensure correct scope
3. Verify each phase before proceeding

### Phase 3: Phase 5 Recovery (1 hour)
1. Fix any issues identified in Phase 1
2. Launch recovery for 7 epics
3. Monitor until 100% completion
4. Run Phase 6 for newly completed epics

### Phase 4: Phase 6 Recovery (30 minutes)
1. Fix PATH issue in scripts
2. Re-launch Phase 6 for 3 failed epics
3. Monitor until 100% completion

### Phase 5: Final Verification (15 minutes)
1. Count all files: Should be 80 × 7 phases = 560 files
2. Verify no gaps in epic sequence
3. Create final completion report

---

## Success Criteria

- ✅ **80/80 epics complete** (100%)
- ✅ **All phases 0-6 complete** for each epic
- ✅ **560 total files** (80 epics × 7 phases)
- ✅ **No gaps** in epic sequence (001-080)
- ✅ **Protocol hardening** complete (skill, SOP, scripts updated)

---

## Timeline Estimate

- **Analysis**: 30 minutes
- **EPIC-016 Re-scoping**: 2 hours
- **Phase 5 Recovery**: 1 hour
- **Phase 6 Recovery**: 30 minutes
- **Final Verification**: 15 minutes
- **Protocol Updates**: 1 hour
- **TOTAL**: ~5 hours

---

**Next Action**: Run Phase 1 (Analysis) to understand Phase 5 failures