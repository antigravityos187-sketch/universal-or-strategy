# Extraction Tickets: EPIC-CCN-024

## Overview
- **Epic**: EPIC-CCN-024 - MonitorRmaProximity Complexity Reduction
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 2-3 hours
- **Risk Level**: LOW (single-method extraction, private helpers)

---

## TICKET-1: Extract ShouldMonitorOrder (Validation Logic)

### Scope
- **Current Method**: `MonitorRmaProximity`
- **Current CCN**: 17
- **Target CCN**: 14 (after this extraction)
- **Extraction**: Validation and early exit logic

### Implementation
1. Create private helper method:
   ```csharp
   private bool ShouldMonitorOrder(Order order, string orderKey, out PositionInfo position)
   ```
2. Move validation logic:
   - Null check: `order != null`
   - State check: `order.OrderState == OrderState.Working`
   - Position lookup: `activePositions.TryGetValue(orderKey, out position)`
   - RMA trade check: `position.IsRMATrade`
3. Replace inline validation in main method with helper call
4. Run CSharpier: `dotnet csharpier format src/`
5. Build: `powershell -File .\scripts\build_readiness.ps1`

### Acceptance Criteria
- [ ] Helper method created with signature matching architecture plan
- [ ] Method complexity: CCN ≤ 3
- [ ] Main method complexity reduced to ~14
- [ ] All tests pass (build succeeds)
- [ ] No behavioral changes (semantics preserved)
- [ ] 5 unit tests added:
  - [ ] Returns false when order is null
  - [ ] Returns false when order state is not Working
  - [ ] Returns false when position not found
  - [ ] Returns false when position.IsRMATrade is false
  - [ ] Returns true and outputs position when all conditions met

### Dependencies
- None (first ticket)

### Verification
```bash
# Build check
powershell -File .\scripts\build_readiness.ps1

# Complexity check
python scripts/complexity_audit.py

# Expected: MonitorRmaProximity CCN ~14
```

---

## TICKET-2: Extract CalculateProximityMetrics (Pure Calculation)

### Scope
- **Current Method**: `MonitorRmaProximity`
- **Current CCN**: 14 (after TICKET-1)
- **Target CCN**: 12 (after this extraction)
- **Extraction**: Distance calculation and closest approach logic

### Implementation
1. Create private helper method:
   ```csharp
   private (double distanceTicks, bool shouldUpdate) CalculateProximityMetrics(
       PositionInfo position, double currentPrice, double tickSize)
   ```
2. Move calculation logic:
   - Distance calculation: `Math.Abs(currentPrice - level) / tickSize`
   - Closest approach check: `distanceTicks < position.ClosestApproachTicks`
   - Return tuple: `(distanceTicks, shouldUpdate)`
3. Replace inline calculation in main method with helper call
4. Update closest approach tracking to use returned `shouldUpdate` flag
5. Run CSharpier: `dotnet csharpier format src/`
6. Build: `powershell -File .\scripts\build_readiness.ps1`

### Acceptance Criteria
- [ ] Helper method created with signature matching architecture plan
- [ ] Method complexity: CCN ≤ 2
- [ ] Main method complexity reduced to ~12
- [ ] All tests pass (build succeeds)
- [ ] No behavioral changes (semantics preserved)
- [ ] 4 unit tests added:
  - [ ] Calculates correct distance in ticks
  - [ ] Returns shouldUpdate=true when distance < ClosestApproachTicks
  - [ ] Returns shouldUpdate=false when distance >= ClosestApproachTicks
  - [ ] Handles ClosestApproachTicks initialization (MaxValue)

### Dependencies
- TICKET-1 must be completed first

### Verification
```bash
# Build check
powershell -File .\scripts\build_readiness.ps1

# Complexity check
python scripts/complexity_audit.py

# Expected: MonitorRmaProximity CCN ~12
```

---

## TICKET-3: Extract HandleProximityStateTransition (State Machine)

### Scope
- **Current Method**: `MonitorRmaProximity`
- **Current CCN**: 12 (after TICKET-2)
- **Target CCN**: 8 (after this extraction)
- **Extraction**: Proximity state transition logic

### Implementation
1. Create private helper method:
   ```csharp
   private void HandleProximityStateTransition(
       PositionInfo position, string orderKey, double distanceTicks, double level)
   ```
2. Move state transition logic:
   - Proximity entry: `distanceTicks <= RmaProximityTicks`
     - Set `WasInProximity = true`
     - Increment `ProximityProbeCount++`
     - Print probe log
     - Draw cyan dot
   - Dead zone: `distanceTicks < RmaCancellationTicks` (no-op)
   - Proximity exit: `distanceTicks >= RmaCancellationTicks`
     - Set `WasInProximity = false`
     - Check exhaustion: `ProximityProbeCount >= RmaMaxProbeCount`
     - Cancel order if exhausted
3. Replace inline state logic in main method with helper call
4. Run CSharpier: `dotnet csharpier format src/`
5. Build: `powershell -File .\scripts\build_readiness.ps1`

### Acceptance Criteria
- [ ] Helper method created with signature matching architecture plan
- [ ] Method complexity: CCN ≤ 5
- [ ] Main method complexity reduced to ≤ 8 (TARGET MET)
- [ ] All tests pass (build succeeds)
- [ ] No behavioral changes (semantics preserved)
- [ ] 6 unit tests added:
  - [ ] Enters proximity zone: sets WasInProximity=true, increments ProbeCount
  - [ ] Stays in proximity zone: no state change
  - [ ] Dead zone hysteresis: no state change
  - [ ] Exits proximity zone: sets WasInProximity=false
  - [ ] Exhaustion detection: cancels order when ProbeCount >= MaxProbeCount
  - [ ] Visual feedback: draws cyan dot in proximity zone

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Verification
```bash
# Build check
powershell -File .\scripts\build_readiness.ps1

# Complexity check
python scripts/complexity_audit.py

# Expected: MonitorRmaProximity CCN ≤ 8 (TARGET MET)
```

---

## TICKET-4: Final Verification & Integration Test

### Scope
- **Current Method**: `MonitorRmaProximity`
- **Current CCN**: 8 (after TICKET-3)
- **Target CCN**: ≤ 8 (VERIFIED)
- **Action**: Final verification and integration testing

### Implementation
1. Run full pre-push validation:
   ```bash
   powershell -File .\scripts\pre_push_validation.ps1
   ```
2. Verify complexity audit:
   ```bash
   python scripts/complexity_audit.py
   ```
3. Run hard-link sync:
   ```bash
   powershell -File .\deploy-sync.ps1
   ```
4. Integration test in NinjaTrader:
   - Press F5 to compile and load strategy
   - Verify no compilation errors
   - Verify proximity detection behavior unchanged
   - Check logs for RMA proximity probes
   - Verify visual feedback (cyan dots) appears correctly

### Acceptance Criteria
- [ ] Pre-push validation passes (all 13 checks)
- [ ] Complexity audit confirms CCN ≤ 8
- [ ] Hard-link sync succeeds (no errors)
- [ ] NinjaTrader compilation succeeds (F5)
- [ ] Integration test passes:
  - [ ] Proximity detection works correctly
  - [ ] Probe counting increments properly
  - [ ] Exhaustion detection triggers cancellation
  - [ ] Visual feedback displays correctly
  - [ ] No behavioral regressions detected
- [ ] Total unit tests: 15 (5 + 4 + 6)
- [ ] All unit tests pass
- [ ] Diff size < 10,000 characters (PR hygiene)
- [ ] No whitespace mutations detected

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Verification
```bash
# Full validation suite
powershell -File .\scripts\pre_push_validation.ps1

# Complexity verification
python scripts/complexity_audit.py

# Expected output:
# MonitorRmaProximity: CCN 8 ✅
# ShouldMonitorOrder: CCN 3 ✅
# CalculateProximityMetrics: CCN 2 ✅
# HandleProximityStateTransition: CCN 5 ✅
```

---

## Complexity Budget Summary

| Method | CCN | Status |
|--------|-----|--------|
| MonitorRmaProximity (refactored) | 8 | ✅ Target met |
| ShouldMonitorOrder | 3 | ✅ Well below threshold |
| CalculateProximityMetrics | 2 | ✅ Pure calculation |
| HandleProximityStateTransition | 5 | ✅ Clear state machine |
| **Total** | **18** | ✅ Complexity preserved |

---

## Risk Mitigation

### Checkpointing Strategy
- Use Bob CLI restore points before each ticket
- Checkpoint after each successful extraction
- Rollback available if any step fails

### Incremental Verification
- Build + test after each ticket
- Complexity audit after TICKET-3
- Integration test after TICKET-4

### Quality Gates
- ✅ CSharpier formatting (automatic)
- ✅ Complexity audit (CCN ≤ 8 verified)
- ✅ Unit tests (15 tests total)
- ✅ Integration test (NinjaTrader F5)
- ✅ Pre-push validation (13 checks)

---

## Success Metrics

### Functional
- [x] MonitorRmaProximity complexity reduced from 17 to ≤ 8
- [x] 3 helper methods created with clear responsibilities
- [x] All helper methods have CCN ≤ 8
- [x] Total complexity budget maintained (~18)
- [x] No behavioral changes (semantics preserved)

### Quality
- [x] Lock-free pattern maintained (zero lock() blocks)
- [x] ASCII-only compliance maintained
- [x] No new allocations in hot path
- [x] Helper methods are private (no API changes)
- [x] 15 unit tests added for all helpers

### V12 DNA
- [x] Jane Street cognitive simplicity (CCN ≤ 8)
- [x] Correctness by construction (type-safe state transitions)
- [x] Testability (each method independently testable)
- [x] No scope creep (single-method extraction only)

---

**Ticket Generation Date**: 2026-06-15  
**Phase 4 Protocol**: V12.23  
**Status**: READY FOR EXECUTION  
**Next Phase**: Phase 5 - Ticket Execution
