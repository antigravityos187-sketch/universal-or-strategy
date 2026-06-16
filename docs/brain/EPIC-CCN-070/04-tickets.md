# Extraction Tickets: EPIC-CCN-070

## Overview
- **Total Tickets**: 1
- **Execution Order**: Sequential (TICKET-1 only)
- **Estimated Effort**: 1.5 hours
- **Epic**: EPIC-CCN-070
- **Target Method**: `HydrateFSMsFromWorkingOrders`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Complexity Reduction**: 9 → 7-8 (Jane Street compliant)

## TICKET-1: Extract CreateAndRegisterFSM Helper Method

### Scope
- **Current Method**: `HydrateFSMsFromWorkingOrders`
- **Current CYC**: 9
- **Target CYC**: 7-8 (main method), 2 (helper method)
- **Extraction**: FSM creation and registration logic (15 LOC)
- **Strategy**: Extract FSM object creation and dictionary registration into dedicated helper method

### Current Code Structure
The target extraction is the FSM creation and registration block within the main loop:

```csharp
// EXTRACTION TARGET (15 LOC)
var fsm = new FollowerBracketFSM(
    entryKey,
    entryOrder,
    pi.ExecutingAccount,
    hydrationState,
    remainingContracts
);

HydrateFSM_LinkBracketOrders(entryKey, fsm, ref ordersIndexed);

if (!string.IsNullOrEmpty(entryOrder.OrderId))
{
    _orderIdToFsmKey.TryAdd(entryOrder.OrderId, entryKey);
}

if (_followerBrackets.TryAdd(entryKey, fsm))
{
    ordersIndexed++;
}
```

### Implementation Steps

#### 1. Create Helper Method (15 minutes)
Add new private method immediately after `HydrateFSMsFromWorkingOrders()`:

```csharp
/// <summary>
/// Creates and registers a FollowerBracketFSM instance for the given entry.
/// Handles FSM initialization, bracket order linking, and order ID indexing.
/// </summary>
/// <param name="entryKey">Entry key for the FSM</param>
/// <param name="entryOrder">Entry order to associate with FSM</param>
/// <param name="pi">Position info containing account details</param>
/// <param name="hydrationState">Initial FSM state</param>
/// <param name="remainingContracts">Remaining contracts for the FSM</param>
/// <param name="ordersIndexed">Reference counter for indexed orders</param>
/// <returns>True if FSM was created and registered; false if already exists</returns>
private bool CreateAndRegisterFSM(
    string entryKey,
    Order entryOrder,
    PositionInfo pi,
    FollowerBracketState hydrationState,
    int remainingContracts,
    ref int ordersIndexed)
{
    var fsm = new FollowerBracketFSM(
        entryKey,
        entryOrder,
        pi.ExecutingAccount,
        hydrationState,
        remainingContracts
    );

    HydrateFSM_LinkBracketOrders(entryKey, fsm, ref ordersIndexed);

    if (!string.IsNullOrEmpty(entryOrder.OrderId))
    {
        _orderIdToFsmKey.TryAdd(entryOrder.OrderId, entryKey);
    }

    if (_followerBrackets.TryAdd(entryKey, fsm))
    {
        ordersIndexed++;
        return true;
    }

    return false;
}
```

#### 2. Update Main Method (10 minutes)
Replace the 15 LOC extraction target with single method call:

```csharp
// Before (15 LOC)
var fsm = new FollowerBracketFSM(...);
HydrateFSM_LinkBracketOrders(...);
if (!string.IsNullOrEmpty(entryOrder.OrderId)) { ... }
if (_followerBrackets.TryAdd(entryKey, fsm)) { ... }

// After (1 LOC)
CreateAndRegisterFSM(entryKey, entryOrder, pi, hydrationState, remainingContracts, ref ordersIndexed);
```

#### 3. Verify Complexity Reduction (5 minutes)
Run complexity audit to confirm:
```bash
python scripts/complexity_audit.py
```

Expected results:
- `HydrateFSMsFromWorkingOrders`: CYC ≤ 8
- `CreateAndRegisterFSM`: CYC = 2

#### 4. Run Quality Gates (30 minutes)
Execute pre-push validation:
```bash
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

Required checks:
- ✅ ASCII-Only (Check #1)
- ✅ Build (Check #2)
- ✅ Unit Tests (Check #3)
- ✅ Lint (Check #4)
- ✅ Formatting (Check #5)
- ✅ PR Hygiene (Check #8)
- ✅ Complexity (Check #9)

#### 5. Sync Hard Links (5 minutes)
```bash
powershell -File .\deploy-sync.ps1
```

#### 6. Manual Verification (15 minutes)
- F5 in NinjaTrader
- Test reconnect scenario
- Verify FSM hydration behavior
- Check ordersIndexed counter accuracy

### Acceptance Criteria
- [ ] Helper method `CreateAndRegisterFSM()` created with correct signature
- [ ] Main method complexity reduced to ≤ 8 (verified by complexity_audit.py)
- [ ] Helper method complexity = 2 (verified by complexity_audit.py)
- [ ] All unit tests pass (100% pass rate)
- [ ] Build succeeds with zero errors
- [ ] No lock() statements introduced (grep verification)
- [ ] ASCII-only compliance maintained (pre-push validation)
- [ ] Hard links synchronized (deploy-sync.ps1)
- [ ] NinjaTrader F5 test passes (manual verification)
- [ ] No behavioral changes (idempotency preserved)
- [ ] Diff size < 10,000 characters (PR hygiene check)
- [ ] CSharpier formatting applied (pre-push validation)
- [ ] Codacy shows "Up to quality standards" (no new issues)

### Dependencies
- None (first and only ticket)

### Lock-Free Validation
- ✅ Uses `ConcurrentDictionary.TryAdd()` for atomic registration
- ✅ Returns bool to indicate success/failure (idempotent)
- ✅ No lock() statements in helper or main method
- ✅ Race condition handling: TryAdd() failure means FSM already registered

### Jane Street Compliance
- ✅ Cognitive simplicity: CYC 9 → 7-8 (main), CYC 2 (helper)
- ✅ Single responsibility: Main orchestrates, helper creates/registers
- ✅ Testability: 2^9=512 paths → 2^7=128 (main) + 2^2=4 (helper)
- ✅ Performance: Zero latency impact (JIT inlining expected)

### Risk Assessment
- **Scope Creep**: MINIMAL (single method extraction only)
- **Regression**: LOW (behavior-preserving transformation)
- **Performance**: NEGLIGIBLE (JIT inlining for small methods)
- **Complexity**: MINIMAL (helper method CYC=2)

### Implementation Notes
1. **Method Placement**: Place helper immediately after main method for code locality
2. **Comment Preservation**: Move any inline comments from extracted code to helper
3. **Atomic Commit**: Keep extraction in single commit for git bisect-ability
4. **Documentation**: XML doc comment explains helper's role in hydration workflow

### Verification Commands
```bash
# Complexity check
python scripts/complexity_audit.py

# Pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# Hard link sync
powershell -File .\deploy-sync.ps1

# Lock-free verification
grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs
```

### Expected Output
```
Complexity Audit Results:
- HydrateFSMsFromWorkingOrders: CYC=7 (PASS)
- CreateAndRegisterFSM: CYC=2 (PASS)

Pre-Push Validation: ALL CHECKS PASSED
Hard Link Sync: SUCCESS
Lock-Free Verification: 0 matches found
```

---

## Ticket Execution Summary

### Total Effort Breakdown
- **TICKET-1**: 1.5 hours
  - Implementation: 25 minutes
  - Quality gates: 30 minutes
  - Verification: 20 minutes
  - Documentation: 15 minutes

### Success Metrics
- **Complexity Reduction**: 9 → 7-8 (22% reduction)
- **Testability Improvement**: 512 paths → 132 paths (74% reduction)
- **Code Locality**: Helper method co-located with main method
- **Maintainability**: Self-documenting helper method name

### Post-Completion Checklist
- [ ] Update manifest.json with Phase 4 completion
- [ ] Commit with message: "EPIC-CCN-070: Extract CreateAndRegisterFSM helper (CYC 9→7)"
- [ ] Push to feature branch
- [ ] Create PR with audit report attached
- [ ] Verify Codacy PR check passes
- [ ] Request code review
- [ ] Merge after approval

---

**Ticket Generation Date**: 2026-06-15  
**Architecture Plan Version**: 2026-06-15  
**Audit Report Version**: 2026-06-15  
**Total Tickets**: 1  
**Estimated Total Effort**: 1.5 hours  
**Complexity Target**: ≤ 8 (Jane Street aligned)  
**Lock-Free Compliance**: VERIFIED  
**Ready for Execution**: YES
