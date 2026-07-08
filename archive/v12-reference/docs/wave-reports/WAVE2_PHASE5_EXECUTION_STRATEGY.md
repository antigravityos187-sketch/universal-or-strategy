# Wave 2 Phase 5 Execution Strategy - Gated Sequential Workflow

**Date**: 2026-06-13  
**Critical Insight**: Validate after EVERY ticket before proceeding to next

## Execution Flow (Per Epic)

```
TICKET-1 → VALIDATE-1 → [PASS/FAIL]
              ↓ PASS
TICKET-2 → VALIDATE-2 → [PASS/FAIL]
              ↓ PASS
TICKET-3 → VALIDATE-3 → [PASS/FAIL]
              ↓ PASS
...
TICKET-N → VALIDATE-N → [PASS/FAIL]
              ↓ PASS
EPIC-REVIEW
```

## Failure Handling Strategy

### Scenario 1: All Tickets Pass (Happy Path)
```
Deploy: 10 tickets + 10 validators
Result: All pass
Action: Proceed to epic review
```

### Scenario 2: Some Tickets Fail
```
Deploy: 10 tickets + 10 validators
Result: Tickets 3, 5, 7, 9 fail validation
Action: 
  1. STOP deployment
  2. Fix failed tickets (3, 5, 7, 9)
  3. Deploy ONLY: 4 tickets + 4 validators (rework)
  4. Wait for all 4 to pass
  5. Resume with remaining tickets
```

### Scenario 3: Cascading Failures
```
Deploy: 10 tickets
Result: Ticket 3 fails
Impact: Tickets 4-10 may depend on Ticket 3
Action:
  1. STOP at Ticket 3
  2. Fix Ticket 3
  3. Re-validate Ticket 3
  4. Resume from Ticket 4 (may need rework if dependent)
```

## Why This Strategy?

### Early Failure Detection
- ✅ Catch issues immediately after each ticket
- ✅ Prevent cascading failures (Ticket 5 won't fail due to Ticket 2 bug)
- ✅ Reduce rework (fix 1 ticket vs. fix 10 tickets)

### Dependency Management
- ✅ Tickets execute in order (1→2→3→4→5)
- ✅ Each ticket validated before next starts
- ✅ Dependencies guaranteed to be correct

### Avoid Messiness
- ✅ No "implement 10, then discover 4 are broken"
- ✅ No "fix 4, but now 2 more are broken due to changes"
- ✅ Clean linear progression

## Implementation Strategy

### Batch Size: 10 Tickets
**Rationale**: Balance between parallelization and failure isolation

**Per Epic**:
1. Deploy first 10 tickets (or all if <10)
2. Each ticket followed immediately by its validator
3. If any fail, STOP and fix before proceeding
4. Deploy next batch only after current batch passes

### Launcher Script Structure

```bash
# EPIC-CCN-107 (6 tickets)
# Batch 1: All 6 tickets (< 10)

# Launch Ticket 1
screen -dmS p5_107_t1 bash -l _p5_107_t1.sh
wait_for_completion p5_107_t1

# Validate Ticket 1
screen -dmS p5v_107_t1 bash -l _p5v_107_t1.sh
wait_for_completion p5v_107_t1
check_validation_result p5v_107_t1
# If FAIL: STOP, fix, re-run, then continue

# Launch Ticket 2
screen -dmS p5_107_t2 bash -l _p5_107_t2.sh
wait_for_completion p5_107_t2

# Validate Ticket 2
screen -dmS p5v_107_t2 bash -l _p5v_107_t2.sh
wait_for_completion p5v_107_t2
check_validation_result p5v_107_t2
# If FAIL: STOP, fix, re-run, then continue

# ... repeat for all 6 tickets

# Epic Review (after all tickets pass)
screen -dmS p6_107 bash -l _p6_107.sh
```

## Validation Pass/Fail Criteria

### PASS Criteria
- ✅ Code compiles
- ✅ Tests pass
- ✅ Complexity reduced (before/after CYC)
- ✅ No regressions
- ✅ Follows V12 DNA principles

### FAIL Criteria
- ❌ Compilation errors
- ❌ Test failures
- ❌ Complexity increased
- ❌ Regressions detected
- ❌ V12 DNA violations

## Rework Protocol

### When Validation Fails
1. **Capture failure details** from `ticket-X-verification.md`
2. **Create rework ticket** with specific fixes needed
3. **Re-run ticket execution** with fixes
4. **Re-run validation** to confirm pass
5. **Proceed to next ticket** only after pass

### Rework Script Naming
- Original: `_p5_107_t3.sh`
- Rework: `_p5_107_t3_rework1.sh`
- Re-validation: `_p5v_107_t3_rework1.sh`

## Cost Implications

### Happy Path (All Pass)
- 30 tickets × 2.5 bobcoins = 75 bobcoins (execution)
- 30 validators × 1.5 bobcoins = 45 bobcoins (validation)
- **Total**: ~120 bobcoins

### With Failures (20% fail rate)
- 30 tickets × 2.5 = 75 bobcoins
- 30 validators × 1.5 = 45 bobcoins
- 6 rework tickets × 2.5 = 15 bobcoins
- 6 rework validators × 1.5 = 9 bobcoins
- **Total**: ~144 bobcoins

## Time Implications

### Sequential Execution (Per Epic)
- Ticket 1: 30 min
- Validate 1: 15 min
- Ticket 2: 30 min
- Validate 2: 15 min
- ...
- **Total per epic**: ~4-6 hours (6 tickets)

### Parallel Across Epics
- All 7 epics run in parallel
- **Total time**: ~4-6 hours (not 28-42 hours)

## Deployment Strategy

### Phase 5A: Ticket Execution + Validation (Gated)
- Deploy 30 ticket scripts + 30 validator scripts
- Launcher enforces sequential execution with validation gates
- STOP on any failure, fix, resume

### Phase 6: Epic Review (After All Tickets Pass)
- Deploy 7 epic review scripts
- Run only after all tickets in epic pass validation

## Next Steps

1. Update `generate_phase5_scripts.py` to create gated launcher
2. Generate 30 ticket scripts + 30 validator scripts
3. Generate 1 gated launcher (enforces sequential + validation)
4. Deploy all scripts to VM
5. Launch gated workflow
6. Monitor for failures, fix as needed