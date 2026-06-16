# [EPIC-7-QUALITY-003] Maintainability: Add Missing Test Coverage for Critical Paths

## Priority: P2 MEDIUM

## Labels
`testing`, `P2`, `epic-7-quality`, `maintainability`

## Summary
24 missing test cases for critical paths: circuit breaker thresholds, state rollback verification, and dispatch logic.

## Test Coverage Gaps

### Circuit Breaker (8 tests)
- Trip threshold validation
- Reset threshold validation
- State transition edge cases
- Concurrent trip/reset scenarios

### State Rollback (8 tests)
- Dictionary cleanup verification
- Partial rollback scenarios
- Rollback failure handling
- State consistency checks

### Dispatch Logic (8 tests)
- SIMA dispatch edge cases
- Order callback error paths
- Concurrent dispatch scenarios
- Performance regression tests

## Affected Components
- `src/V12_002.SIMA.Dispatch.cs`
- `src/V12_002.UI.IPC.cs`
- Circuit breaker FSM
- Order management callbacks

## Required Actions
1. Create test specifications for 24 test cases
2. Implement unit tests (xUnit)
3. Implement integration tests
4. Add performance benchmarks (BenchmarkDotNet)
5. Verify coverage with AMAL Harness

## Acceptance Criteria
- [ ] 24 test cases implemented and passing
- [ ] Unit test coverage >80% for affected files
- [ ] Integration tests cover end-to-end scenarios
- [ ] Performance benchmarks establish baselines
- [ ] AMAL Harness reports no regressions

## Effort Estimate
Large (16-24 hours)

## References
- Audit: `docs/brain/DEFERRED_WORK_AUDIT.md` (Lines 127-178)
- PRs affected: #2, #6, #8
- Testing pyramid: `docs/protocol/TESTING_PYRAMID.md`

## Status
🔴 **OPEN** - Not Started

## Assigned To
_Unassigned_

## Created
2026-05-24T04:14:00Z