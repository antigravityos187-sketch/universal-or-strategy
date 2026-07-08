# Wave 2 Phase 5 Resume Plan

**Created**: 2026-06-13T18:29:21Z
**Status**: Ready to proceed

---

## Current State

### ✅ Complete (3 epics, 9 tickets)
- EPIC-111 (3 tickets)
- EPIC-113 (5 tickets)
- EPIC-114 (1 ticket)

### ⚠️ Blocked (4 epics, 11 remaining tickets)
- EPIC-107: 3 tickets remaining (T4, T5, T6)
- EPIC-108: 5 tickets remaining (T1-T5, need to re-run T1)
- EPIC-109: 2 tickets remaining (T3, T4, need to re-run T2)
- EPIC-112: 2 tickets remaining (T5, T6, need to re-run T4)

---

## Fix Strategy

### Option A: Fix All, Then Resume (Recommended)
**Approach**: Fix all 4 blockers first, then run remaining tickets in one batch
**Pros**: Clean execution, no interruptions
**Cons**: Requires understanding all issues upfront
**Time**: 30-45 minutes fixes + 90-120 minutes execution = 2-2.5 hours

### Option B: Fix One, Resume One (Iterative)
**Approach**: Fix EPIC-107, resume it, then move to next
**Pros**: Incremental progress, easier to debug
**Cons**: More manual intervention
**Time**: Similar to Option A but more interactive

### Option C: Skip Blockers, Complete Successful Ones (Fast Path)
**Approach**: Move complete epics (111, 113, 114) to Phase 6 now
**Pros**: Show progress immediately
**Cons**: Leaves 4 epics incomplete
**Time**: 30-45 minutes for Phase 6 reviews

---

## Recommended Approach: Option A

Fix all 4 blockers, then resume remaining 11 tickets in one autonomous batch.

---

## Fix Details

### 1. EPIC-107 TICKET-3 (Method Visibility)

**Issue**: Method is `private`, tests need `internal`

**Fix Command**:
```bash
# On VM
cd /home/malhitticrypto/universal-or-strategy
sed -i 's/private void EnqueueExpectedPositionUpdate/internal void EnqueueExpectedPositionUpdate/g' src/V12_002.SIMA.Lifecycle.cs

# Verify
grep "internal void EnqueueExpectedPositionUpdate" src/V12_002.SIMA.Lifecycle.cs
```

**Resume**: Re-run T3 execution + validation, then continue with T4, T5, T6

---

### 2. EPIC-108 TICKET-1 (Incomplete Work)

**Issue**: Bob claimed completion but didn't create `IsOrderCancellable` method

**Fix Approach**: Re-run TICKET-1 with explicit verification

**New Script** (`_p5_108_t1_fixed.sh`):
```bash
#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='<key>'

cat > /tmp/phase5_msg_108_t1_fixed.txt << 'EOFMSG'
You are executing TICKET-1 for EPIC-CCN-108.

**CRITICAL**: Previous attempt claimed completion but did NOT create the method.

**Task**: Extract IsOrderCancellable method from V12_002.SIMA.Lifecycle.cs
**Target**: Reduce complexity from 18 to ≤8
**Method Signature**: internal bool IsOrderCancellable(OrderState state)

**Verification Required**:
1. Method MUST exist in source file
2. Method MUST be called from original location
3. Tests MUST compile and pass
4. Complexity MUST be ≤8

**Output**: ticket-1-completion.md with verification proof
EOFMSG

bob --yolo --chat-mode v12-engineer "$(cat /tmp/phase5_msg_108_t1_fixed.txt)" 2>&1 | tee logs/phase5/EPIC-CCN-108-T1-FIXED.log

# Explicit verification
if ! grep -q "IsOrderCancellable" src/V12_002.SIMA.Lifecycle.cs; then
    echo "❌ VERIFICATION FAILED: Method not found in source"
    exit 1
fi

echo "✅ Verification passed: Method exists"
```

**Resume**: Run fixed T1, validate, then continue with T2-T5

---

### 3. EPIC-109 TICKET-2 (Missing Tests)

**Issue**: Extracted method has no unit tests

**Fix Approach**: Add unit tests before re-validating

**Options**:
- **A**: Ask Bob to create tests in separate ticket
- **B**: Manually create minimal test
- **C**: Accept CONDITIONAL PASS and add tests later

**Recommended**: Option A (Bob creates tests)

**New Script** (`_p5_109_t2_tests.sh`):
```bash
#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='<key>'

cat > /tmp/phase5_msg_109_t2_tests.txt << 'EOFMSG'
You are creating unit tests for EPIC-CCN-109 TICKET-2.

**Context**: TICKET-2 extracted a method but validation failed due to missing tests.

**Task**: Create unit tests for the extracted method
**Location**: tests/V12_Performance.Tests/
**Requirements**:
- Test happy path
- Test edge cases
- Test error conditions
- All tests must pass

**Output**: Test file created and all tests passing
EOFMSG

bob --yolo --chat-mode v12-engineer "$(cat /tmp/phase5_msg_109_t2_tests.txt)" 2>&1 | tee logs/phase5/EPIC-CCN-109-T2-TESTS.log
```

**Resume**: Run test creation, re-validate T2, then continue with T3-T4

---

### 4. EPIC-112 TICKET-4 (Complexity Target Miss)

**Issue**: Achieved CYC=13, target was ≤8

**Fix Approach**: Further decompose the extracted method

**New Script** (`_p5_112_t4_decompose.sh`):
```bash
#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='<key>'

cat > /tmp/phase5_msg_112_t4_decompose.txt << 'EOFMSG'
You are fixing EPIC-CCN-112 TICKET-4 complexity issue.

**Context**: TICKET-4 extracted a method but achieved CYC=13 instead of target ≤8.

**Task**: Further decompose the extracted method to achieve CYC ≤8
**Approach**:
1. Identify complex branches in extracted method
2. Extract sub-methods for each complex branch
3. Verify each sub-method has CYC ≤8
4. Verify parent method now has CYC ≤8

**Iterative**: Keep decomposing until ALL methods have CYC ≤8

**Output**: ticket-4-completion.md with complexity measurements
EOFMSG

bob --yolo --chat-mode v12-engineer "$(cat /tmp/phase5_msg_112_t4_decompose.txt)" 2>&1 | tee logs/phase5/EPIC-CCN-112-T4-DECOMPOSE.log
```

**Resume**: Run decomposition, re-validate T4, then continue with T5-T6

---

## Execution Plan

### Step 1: Fix EPIC-107 (Simplest - 2 minutes)
```bash
# Fix method visibility
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && sed -i 's/private void EnqueueExpectedPositionUpdate/internal void EnqueueExpectedPositionUpdate/g' src/V12_002.SIMA.Lifecycle.cs"

# Verify fix
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && grep 'internal void EnqueueExpectedPositionUpdate' src/V12_002.SIMA.Lifecycle.cs"
```

### Step 2: Create Fix Scripts for Other Epics (10 minutes)
- Generate `_p5_108_t1_fixed.sh`
- Generate `_p5_109_t2_tests.sh`
- Generate `_p5_112_t4_decompose.sh`
- Deploy to VM

### Step 3: Run Fix Scripts (30-45 minutes)
- Run EPIC-108 T1 fix
- Run EPIC-109 T2 test creation
- Run EPIC-112 T4 decomposition

### Step 4: Create Resume Script (5 minutes)
- Generate `resume_blocked_epics.sh`
- Includes re-validation of fixed tickets
- Continues with remaining tickets

### Step 5: Launch Resume Script (90-120 minutes autonomous)
- Re-validate fixed tickets
- Continue with remaining tickets
- Stop on any new failures

### Step 6: Phase 6 Reviews (30-45 minutes)
- Run epic-level reviews for all 7 epics
- Generate completion reports
- Prepare for merge

---

## Timeline

| Step | Duration | Description |
|------|----------|-------------|
| 1 | 2 min | Fix EPIC-107 visibility |
| 2 | 10 min | Create fix scripts |
| 3 | 30-45 min | Run fix scripts |
| 4 | 5 min | Create resume script |
| 5 | 90-120 min | Resume autonomous execution |
| 6 | 30-45 min | Phase 6 reviews |
| **Total** | **2.5-3.5 hours** | **Complete Phase 5 + Phase 6** |

---

## Success Criteria

### Phase 5 Complete When:
- ✅ All 30 tickets executed
- ✅ All 30 validations passed (or CONDITIONAL PASS)
- ✅ All 7 epics have all tickets complete
- ✅ No FAIL verdicts remaining

### Phase 6 Complete When:
- ✅ All 7 epic reviews complete
- ✅ All epic-level validations passed
- ✅ Ready for merge to main

---

## Next Action

**Immediate**: Fix EPIC-107 (simplest, 2 minutes)

Shall I proceed with Step 1?