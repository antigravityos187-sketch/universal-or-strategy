# EPIC-CCN-15: Ticket Summary

**Epic**: EPIC-CCN-15  
**Date**: 2026-06-09  
**Status**: Phase 5 - Execution Ready

---

## Ticket Overview

| # | Ticket | Priority | CYC Reduction | Depends On | Status |
|---|--------|----------|---------------|------------|--------|
| 1 | Extract Entry Name Helper | P0 | 4 → 4 (foundation) | None | Ready |
| 2 | Extract Deduplication Guard | P1 | 12 → 8 (33%) | T1 | Ready |
| 3 | Extract Stop Loss Handler | P2 | 18 → 8 (56%) | T1 | Ready |
| 4 | Extract Target Fill Handler | P3 | 20 → 8 (60%) | T1 | Ready |
| 5 | Extract Trim Handler | P4 | 15 → 8 (47%) | T1 | Ready |

**Total CYC Reduction**: 67 → 8 (88%)

---

## Execution Order

1. **Ticket 1** (Foundation) → F5 verification
2. **Ticket 2** (Independent) → F5 verification
3. **Ticket 3** (Stop handler) → F5 verification
4. **Ticket 4** (Target handler) → F5 verification
5. **Ticket 5** (Trim handler) → F5 verification

---

## Sub-Extractions

| Parent | Sub-Extraction | CYC | Purpose |
|--------|----------------|-----|---------|
| T3 | `CancelTargetOrdersForEntry` | 5 | OCO cancellation loop |
| T4 | `CleanupTerminalTargetFill` | 3 | Terminal state cleanup |

---

## F5 Verification Checkpoints

### Ticket 1: Entry Name Helper
- **Test**: Place entry order
- **Expected**: Entry name parsed correctly in logs

### Ticket 2: Deduplication Guard
- **Test**: Trigger duplicate execution
- **Expected**: "[DEDUP]" log appears, no double-decrement

### Ticket 3: Stop Loss Handler
- **Test**: Hit stop loss
- **Expected**: Targets cancelled, "OCO: Cancelled X targets" log

### Ticket 4: Target Fill Handler
- **Test**: Hit T1 target
- **Expected**: Stop quantity reduced, "TARGET FILLED" log

### Ticket 5: Trim Handler
- **Test**: Execute trim order
- **Expected**: Stop quantity reduced, "STOP INTEGRITY" log

---

## Success Criteria (Final Verification)

After completing all 5 tickets:

- [ ] `ProcessOnExecutionUpdate` CYC ≤8 (target achieved)
- [ ] All extracted methods CYC ≤8
- [ ] Zero locks (grep -r "lock(" returns empty)
- [ ] ASCII-only (no Unicode violations)
- [ ] All F5 verifications passed
- [ ] Build + tests pass
- [ ] Documentation committed
- [ ] BUILD_TAG bumped (5 times, once per ticket)

---

## Risk Summary

| Ticket | Risk Level | Mitigation |
|--------|------------|------------|
| T1 | LOW | Pure function, no state mutation |
| T2 | HIGH | Hash ring logic, unit tests required |
| T3 | HIGH | OCO race conditions, sub-extraction |
| T4 | HIGH | First-Writer-Wins guard, sub-extraction |
| T5 | MEDIUM | Stop quantity critical, unit tests |

---

## Estimated Timeline

- **Ticket 1**: 10 minutes (simple lambda conversion)
- **Ticket 2**: 20 minutes (complex dedup logic)
- **Ticket 3**: 25 minutes (stop handler + sub-extraction)
- **Ticket 4**: 30 minutes (target handler + sub-extraction)
- **Ticket 5**: 20 minutes (trim handler)

**Total**: ~105 minutes (1.75 hours)

---

## Next Steps

1. **Execute Ticket 1**: Extract entry name helper
2. **STOP for F5**: Wait for Director confirmation
3. **Execute Ticket 2**: Extract dedup guard
4. **STOP for F5**: Wait for Director confirmation
5. **Execute Ticket 3**: Extract stop handler
6. **STOP for F5**: Wait for Director confirmation
7. **Execute Ticket 4**: Extract target handler
8. **STOP for F5**: Wait for Director confirmation
9. **Execute Ticket 5**: Extract trim handler
10. **STOP for F5**: Wait for Director confirmation
11. **Final Verification**: Run all success criteria checks
12. **Document Completion**: Create completion report

---

**Summary Created**: 2026-06-09T03:41:00Z  
**Ready for Execution**: Phase 5 - Ticket 1