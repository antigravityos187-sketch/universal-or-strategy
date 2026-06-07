# [EPIC-7-QUALITY-002] Error-Prone: Complete Circuit Breaker Rollback Logic

## Priority: P1 HIGH

## Labels
`bug`, `P1`, `epic-7-quality`, `error-prone`

## Summary
12 incomplete rollback logic instances in circuit breaker state management, causing dictionary registration leaks.

## Affected Files
- `src/V12_002.SIMA.Dispatch.cs` (primary)
- Circuit breaker state cleanup paths

## Technical Debt
- **Category:** Error-prone
- **Impact:** State corruption on circuit breaker trips
- **Risk:** Dictionary leaks, memory growth over time

## Root Cause
Circuit breaker rollback logic cleans up state but fails to remove dictionary registrations, causing:
- Orphaned entries in dispatch registry
- Memory leaks on repeated trip/reset cycles
- Potential state corruption

## Required Actions
1. Add dictionary cleanup to rollback paths
2. Verify all circuit breaker state transitions
3. Add unit tests for rollback scenarios
4. Add integration tests for trip/reset cycles

## Acceptance Criteria
- [ ] Dictionary cleanup added to all rollback paths
- [ ] Unit tests cover rollback logic (100% coverage)
- [ ] Integration tests verify trip/reset cycles
- [ ] No memory leaks in stress tests
- [ ] cubic-dev-ai scan passes (0 incomplete rollback warnings)

## Effort Estimate
Small (4-6 hours)

## References
- Audit: `docs/brain/DEFERRED_WORK_AUDIT.md` (Lines 91-125)
- PRs affected: #2, #6
- Related: Circuit breaker FSM in `src/V12_002.SIMA.Dispatch.cs`

## Status
🔴 **OPEN** - Not Started

## Assigned To
_Unassigned_

## Created
2026-05-24T04:14:00Z