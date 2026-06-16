# Extraction Tickets: EPIC-CCN-051

## Overview
- **Epic ID**: EPIC-CCN-051
- **Target Method**: UpdateStopOrder
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Current Complexity**: 11
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 4.5 hours (1.5h per ticket)

---

## TICKET-1: Extract CheckAndHandleStalePending

### Scope
- **Current Method**: `UpdateStopOrder`
- **Current CYC**: 11
- **Target CYC After Extraction**: 9 (reduction of 2)
- **Helper CYC**: 2
- **Extraction**: Stale pending replacement detection and handling logic

### Purpose
Isolate stale pending replacement detection and timeout handling into a focused helper method. This removes nested conditional logic and timeout calculations from the main method.

### Implementation Steps

1. **Create Helper Method**
   ```csharp
   private bool CheckAndHandleStalePending(
       string entryName, 
       PositionInfo pos, 
       double validatedStopPrice, 
       int newTrailLevel)
   {
       // Check if pending replacement exists
       if (!pendingStopReplacements.TryGetValue(entryName, out var pending))
       {
           return false; // No pending replacement
       }

       // Calculate pending age
       var pendingAge = DateTime.UtcNow - pending.InitiatedAt;
       
       // Handle stale pending if timeout exceeded
       if (pendingAge.TotalSeconds > PENDING_TIMEOUT_SECONDS)
       {
           HandleStalePendingReplacement(entryName, pos, validatedStopPrice, newTrailLevel);
           return true; // Stale handling occurred (early exit)
       }

       return false; // Pending is fresh, continue normal flow
   }
   ```

2. **Update UpdateStopOrder Call Site**
   - Replace stale pending check logic with helper call
   - Maintain early exit behavior if stale handling occurred
   ```csharp
   // Before: Inline stale pending check (5 lines)
   // After: Single helper call
   if (CheckAndHandleStalePending(entryName, pos, validatedStopPrice, newTrailLevel))
   {
       return; // Early exit if stale pending handled
   }
   ```

3. **Verify Extraction**
   - Run: `dotnet build`
   - Run: `powershell -File .\deploy-sync.ps1`
   - Run: `python scripts/complexity_audit.py`
   - Verify: UpdateStopOrder CYC reduced to 9

4. **Git Checkpoint**
   ```bash
   git add src/V12_002.Trailing.StopUpdate.cs
   git commit -m "EPIC-CCN-051: Extract CheckAndHandleStalePending (CYC: 2)"
   ```

### Acceptance Criteria
- [ ] Helper method created with signature matching architecture plan
- [ ] Helper CYC = 2 (verified by complexity_audit.py)
- [ ] UpdateStopOrder CYC reduced to 9 (verified by complexity_audit.py)
- [ ] No behavioral changes (black-box equivalence maintained)
- [ ] Build succeeds (dotnet build)
- [ ] Hard-link sync passes (deploy-sync.ps1)
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained
- [ ] Git checkpoint created

### Dependencies
- None (first ticket in sequence)

### Verification Commands
```bash
# Build check
dotnet build

# Hard-link sync
powershell -File .\deploy-sync.ps1

# Complexity audit
python scripts/complexity_audit.py

# Expected output:
# UpdateStopOrder: CYC 9 (was 11)
# CheckAndHandleStalePending: CYC 2
```

---

## TICKET-2: Extract RouteStopOrderUpdate

### Scope
- **Current Method**: `UpdateStopOrder`
- **Current CYC**: 9 (after TICKET-1)
- **Target CYC After Extraction**: 6 (reduction of 3)
- **Helper CYC**: 3
- **Extraction**: Order state routing logic (3 conditional branches)

### Purpose
Centralize order state routing logic into a focused helper method. This removes complex conditional branching based on order state from the main method.

### Implementation Steps

1. **Create Helper Method**
   ```csharp
   private void RouteStopOrderUpdate(
       string entryName, 
       PositionInfo pos, 
       Order currentStop, 
       double validatedStopPrice, 
       int newTrailLevel)
   {
       // Route to UpdateExistingPendingReplacement if order is CancelPending/Submitted
       if (currentStop.OrderState == OrderState.CancelPending || 
           currentStop.OrderState == OrderState.Submitted)
       {
           UpdateExistingPendingReplacement(entryName, pos, validatedStopPrice, newTrailLevel);
           return;
       }

       // Route to InitiateStopReplacement if order is Working/Accepted
       if (currentStop.OrderState == OrderState.Working || 
           currentStop.OrderState == OrderState.Accepted)
       {
           InitiateStopReplacement(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
           return;
       }

       // Route to CreateDirectStopOrder if no existing stop or not cancellable
       CreateDirectStopOrder(entryName, pos, validatedStopPrice, newTrailLevel);
   }
   ```

2. **Update UpdateStopOrder Call Site**
   - Replace order state routing logic with helper call
   - Maintain routing behavior
   ```csharp
   // Before: Inline order state routing (10 lines)
   // After: Single helper call
   RouteStopOrderUpdate(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
   ```

3. **Verify Extraction**
   - Run: `dotnet build`
   - Run: `powershell -File .\deploy-sync.ps1`
   - Run: `python scripts/complexity_audit.py`
   - Verify: UpdateStopOrder CYC reduced to 6

4. **Git Checkpoint**
   ```bash
   git add src/V12_002.Trailing.StopUpdate.cs
   git commit -m "EPIC-CCN-051: Extract RouteStopOrderUpdate (CYC: 3)"
   ```

### Acceptance Criteria
- [ ] Helper method created with signature matching architecture plan
- [ ] Helper CYC = 3 (verified by complexity_audit.py)
- [ ] UpdateStopOrder CYC reduced to 6 (verified by complexity_audit.py)
- [ ] No behavioral changes (black-box equivalence maintained)
- [ ] Build succeeds (dotnet build)
- [ ] Hard-link sync passes (deploy-sync.ps1)
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained
- [ ] Git checkpoint created

### Dependencies
- **TICKET-1** must be completed first (sequential extraction)

### Verification Commands
```bash
# Build check
dotnet build

# Hard-link sync
powershell -File .\deploy-sync.ps1

# Complexity audit
python scripts/complexity_audit.py

# Expected output:
# UpdateStopOrder: CYC 6 (was 9)
# RouteStopOrderUpdate: CYC 3
```

---

## TICKET-3: Extract HandleUpdateError

### Scope
- **Current Method**: `UpdateStopOrder`
- **Current CYC**: 6 (after TICKET-2)
- **Target CYC After Extraction**: 5 (reduction of 1, final target ≤8 achieved)
- **Helper CYC**: 2
- **Extraction**: Error handling and circuit breaker logic

### Purpose
Isolate error handling and circuit breaker logic into a focused helper method. This removes nested error handling and flatten attempt counting from the main method.

### Implementation Steps

1. **Create Helper Method**
   ```csharp
   private void HandleUpdateError(
       string entryName, 
       PositionInfo pos, 
       Exception ex)
   {
       // Log error details
       LogError($"UpdateStopOrder failed for {entryName}: {ex.Message}");

       // Check circuit breaker state
       if (!activePositions.TryGetValue(entryName, out var activePos))
       {
           return; // Position no longer active, skip flatten
       }

       // Increment flatten attempt counter (circuit breaker)
       var flattenAttempts = Interlocked.Increment(ref activePos.FlattenAttempts);
       
       // Execute emergency flatten if not blocked
       if (flattenAttempts <= MAX_FLATTEN_ATTEMPTS)
       {
           try
           {
               FlattenPositionByName(entryName);
           }
           catch (Exception flattenEx)
           {
               LogError($"Emergency flatten failed for {entryName}: {flattenEx.Message}");
           }
       }
   }
   ```

2. **Update UpdateStopOrder Catch Block**
   - Replace inline error handling with helper call
   - Maintain circuit breaker behavior
   ```csharp
   // Before: Inline error handling (8 lines)
   // After: Single helper call
   catch (Exception ex)
   {
       HandleUpdateError(entryName, pos, ex);
   }
   ```

3. **Verify Extraction**
   - Run: `dotnet build`
   - Run: `powershell -File .\deploy-sync.ps1`
   - Run: `python scripts/complexity_audit.py`
   - Verify: UpdateStopOrder CYC reduced to 5 (FINAL TARGET ACHIEVED)

4. **Git Checkpoint**
   ```bash
   git add src/V12_002.Trailing.StopUpdate.cs
   git commit -m "EPIC-CCN-051: Extract HandleUpdateError (CYC: 2) - FINAL"
   ```

### Acceptance Criteria
- [ ] Helper method created with signature matching architecture plan
- [ ] Helper CYC = 2 (verified by complexity_audit.py)
- [ ] UpdateStopOrder CYC reduced to 5 (verified by complexity_audit.py)
- [ ] **FINAL TARGET ACHIEVED**: UpdateStopOrder CYC ≤8 ✅
- [ ] No behavioral changes (black-box equivalence maintained)
- [ ] Build succeeds (dotnet build)
- [ ] Hard-link sync passes (deploy-sync.ps1)
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained
- [ ] Git checkpoint created

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed second

### Verification Commands
```bash
# Build check
dotnet build

# Hard-link sync
powershell -File .\deploy-sync.ps1

# Complexity audit
python scripts/complexity_audit.py

# Expected output:
# UpdateStopOrder: CYC 5 (was 6) ✅ TARGET ACHIEVED
# HandleUpdateError: CYC 2
```

---

## Final Verification (After All 3 Tickets)

### Comprehensive Verification Checklist

1. **Build Readiness**
   ```bash
   powershell -File .\scripts\build_readiness.ps1
   ```
   - Expected: Build succeeds, no errors

2. **Complexity Audit**
   ```bash
   python scripts/complexity_audit.py
   ```
   - Expected: All methods ≤8 CYC
   - UpdateStopOrder: 5 ✅
   - CheckAndHandleStalePending: 2 ✅
   - RouteStopOrderUpdate: 3 ✅
   - HandleUpdateError: 2 ✅

3. **Stress Test**
   ```bash
   powershell -File .\scripts\test_stress.ps1
   ```
   - Expected: All tests pass, no regressions

4. **Manual Verification (F5 Test)**
   - Open NinjaTrader
   - Load V12_002 strategy
   - Press F5 (compile and run)
   - Expected: No compilation errors, strategy loads successfully

### Success Criteria (Epic-Level)
- [ ] All 3 tickets completed
- [ ] UpdateStopOrder CYC reduced from 11 → 5 (55% reduction)
- [ ] All helper methods ≤8 CYC (Jane Street compliance)
- [ ] No lock() statements introduced (lock-free pattern maintained)
- [ ] ASCII-only compliance maintained
- [ ] Build succeeds (build_readiness.ps1)
- [ ] Hard-link sync passes (deploy-sync.ps1)
- [ ] Stress tests pass (test_stress.ps1)
- [ ] NinjaTrader F5 test passes (manual verification)
- [ ] Git history shows 3 clean checkpoints

---

## Complexity Reduction Summary

### Before Refactoring
```
UpdateStopOrder: CYC 11 (5 major decision points)
├─ Stale pending check (nested if + timeout)
├─ Order state routing (3 conditional branches)
└─ Error handling (nested if + circuit breaker)
```

### After Refactoring (Progressive)
```
TICKET-1: UpdateStopOrder: CYC 11 → 9 (CheckAndHandleStalePending extracted)
TICKET-2: UpdateStopOrder: CYC 9 → 6 (RouteStopOrderUpdate extracted)
TICKET-3: UpdateStopOrder: CYC 6 → 5 (HandleUpdateError extracted) ✅ FINAL
```

### Final State
```
UpdateStopOrder: CYC 5 (main method) ✅
├─ CheckAndHandleStalePending: CYC 2 ✅
├─ RouteStopOrderUpdate: CYC 3 ✅
└─ HandleUpdateError: CYC 2 ✅

Total Distributed CYC: 5 + 2 + 3 + 2 = 12
Main Method CYC: 5 ✅ (Target: ≤8)
All Helpers: ≤3 ✅ (Target: ≤8)
```

### Cognitive Load Reduction
- **Original**: 1 method with 11 CYC (high cognitive load)
- **Refactored**: 4 methods with max 5 CYC each (low cognitive load)
- **Improvement**: 55% reduction in main method complexity

### Testing Complexity Reduction
- **Original**: 2^5 = 32 possible paths (exponential)
- **Refactored**: 2^2 + 2^3 + 2^2 = 16 paths (linear sum)
- **Improvement**: 50% reduction in test path explosion

---

## Risk Mitigation

### Checkpointing Strategy
- **Frequency**: After each ticket (3 checkpoints total)
- **Purpose**: Enables surgical rollback if any helper causes regression
- **Command**: `git add src/V12_002.Trailing.StopUpdate.cs && git commit -m "EPIC-CCN-051: [Ticket]"`

### Verification Strategy
- **Per-Ticket**: Build + deploy-sync + complexity audit
- **Final**: Build readiness + stress test + F5 manual test
- **Rollback**: Git revert to last checkpoint if regression detected

### Blast Radius
- **Scope**: Single method extraction (UpdateStopOrder only)
- **Callers**: 2 identified (UI.IPC.Commands.Mode, Symmetry.Replace)
- **Signature**: Unchanged (no caller impact)
- **Behavior**: Black-box equivalent (no functional changes)

---

## Phase 4 Sign-off

### Ticket Generation Complete
- **Total Tickets**: 3 (sequential execution)
- **Complexity Target**: ≤8 CYC (Jane Street strict standard)
- **Estimated Effort**: 4.5 hours (1.5h per ticket)
- **Risk Level**: LOW (surgical extraction, black-box equivalence)

### Next Phase Authorization
- **Phase 5 (Ticket Execution)**: AUTHORIZED
- **Engineer**: Bob CLI (v12-engineer)
- **Execution Order**: TICKET-1 → TICKET-2 → TICKET-3 (sequential)

### Architect Sign-off
- **Architect**: Bob CLI (v12-engineer)
- **Date**: 2026-06-15
- **Status**: ✅ APPROVED FOR PHASE 5
- **Next Action**: Execute TICKET-1 (Extract CheckAndHandleStalePending)

---

**End of Ticket Generation Document**
