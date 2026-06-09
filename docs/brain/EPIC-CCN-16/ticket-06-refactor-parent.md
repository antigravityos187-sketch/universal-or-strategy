---
# TICKET EPIC-CCN-16-06: Refactor HydrateFSMsFromWorkingOrders
# Epic: EPIC-CCN-16
# Sequence: 6 of 6
# Depends on: ALL previous tickets (1-5)
---

## Objective
Refactor parent method to orchestrate extracted methods, reducing complexity from CYC 45 to ≤8.

## Scope
IN scope:
- Refactor [`HydrateFSMsFromWorkingOrders`](src/V12_002.SIMA.Lifecycle.cs:464) to use extracted methods
- Replace inline logic with method calls
- Preserve method signature and XML documentation
- Preserve logging statements

OUT of scope:
- Caller method `HydrateWorkingOrdersFromBroker`
- Extracted methods (already created in tickets 1-5)
- Helper methods (IsFleetAccount, Print, LinkTargetOrderToFSM)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-16/02-analysis.md`](docs/brain/EPIC-CCN-16/02-analysis.md) -- Migration Path section
- Approach: [`docs/brain/EPIC-CCN-16/03-approach.md`](docs/brain/EPIC-CCN-16/03-approach.md) -- Method Signatures section 6

## Implementation Instructions

### Refactored Method Structure
Replace the entire method body (lines 464-759) with the orchestration code below.

**Preserve Method Signature and XML Doc** (lines 456-463):
```csharp
/// <summary>
/// Phase 5: Rebuilds _followerBrackets and _orderIdToFsmKey from already-adopted
/// working orders. Called from HydrateWorkingOrdersFromBroker() before the
/// adoption-complete gate is set. Idempotent -- safe to call on every reconnect.
/// </summary>
/// <remarks>
/// ACTOR-SERIALIZED: Must be called on strategy thread (via EnumerateApexAccounts).
/// THREAD-SAFETY: Single-write operations to ConcurrentDictionary are safe.
/// </remarks>
private void HydrateFSMsFromWorkingOrders()
```

**New Method Body**:
```csharp
{
    int fsmCreated = 0;
    int ordersIndexed = 0;

    Print("[SIMA] Phase 5 FSM Hydration: Starting entry order pass...");

    // Entry Order Pass
    foreach (var kvp in entryOrders.ToArray())
    {
        string entryKey = kvp.Key;
        Order entryOrder = kvp.Value;
        if (entryOrder == null) continue;

        // Skip master account entries
        PositionInfo pi;
        if (!activePositions.TryGetValue(entryKey, out pi) || !pi.IsFollower) continue;
        if (pi.ExecutingAccount == null) continue;

        // Idempotent guard
        if (_followerBrackets.ContainsKey(entryKey)) continue;

        // Map state
        FollowerBracketState? state = MapOrderStateToFSMState(entryOrder.OrderState);
        if (state == null) continue; // Terminal state - skip FSM creation

        // Resolve contracts
        int remainingContracts = ResolveRemainingContracts(entryOrder, pi, state.Value);

        // Build FSM
        var fsm = BuildFSM(entryKey, pi, entryOrder, state.Value, remainingContracts);

        // Link stop order
        Order stopOrd;
        if (stopOrders.TryGetValue(entryKey, out stopOrd) && stopOrd != null)
        {
            fsm.StopOrder = stopOrd;
            if (!string.IsNullOrEmpty(stopOrd.OrderId))
            {
                _orderIdToFsmKey[stopOrd.OrderId] = entryKey;
                ordersIndexed++;
            }
        }

        // Link target orders (using existing helper)
        LinkTargetOrderToFSM(ref fsm, entryKey, 0, target1Orders, ref ordersIndexed);
        LinkTargetOrderToFSM(ref fsm, entryKey, 1, target2Orders, ref ordersIndexed);
        LinkTargetOrderToFSM(ref fsm, entryKey, 2, target3Orders, ref ordersIndexed);
        LinkTargetOrderToFSM(ref fsm, entryKey, 3, target4Orders, ref ordersIndexed);
        LinkTargetOrderToFSM(ref fsm, entryKey, 4, target5Orders, ref ordersIndexed);

        // Register FSM
        RegisterFSM(entryKey, fsm, entryOrder, ref ordersIndexed, ref fsmCreated);
    }

    Print(string.Format(
        "[SIMA] Phase 5 FSM Hydration (Entry Pass): {0} FSMs created, {1} order IDs indexed.",
        fsmCreated, ordersIndexed
    ));

    // Position Pass
    int positionFsmCreated = HydrateFromOpenPositions(
        stopOrders, target1Orders, target2Orders, target3Orders, target4Orders, target5Orders,
        ref ordersIndexed, ref fsmCreated
    );

    Print(string.Format(
        "[SIMA] Phase 5 FSM Hydration (Position Pass): {0} Active FSMs created from open positions.",
        positionFsmCreated
    ));

    Print(string.Format(
        "[SIMA] Phase 5 FSM Hydration: {0} FSMs created, {1} order IDs indexed.",
        fsmCreated, ordersIndexed
    ));
}
```

### Key Changes
1. **Lines 488-503** → `MapOrderStateToFSMState(entryOrder.OrderState)`
2. **Lines 505-518** → `ResolveRemainingContracts(entryOrder, pi, state.Value)`
3. **Lines 520-528** → `BuildFSM(entryKey, pi, entryOrder, state.Value, remainingContracts)`
4. **Lines 530-588** → Existing `LinkTargetOrderToFSM` calls (5x)
5. **Lines 590-598** → `RegisterFSM(entryKey, fsm, entryOrder, ref ordersIndexed, ref fsmCreated)`
6. **Lines 602-743** → `HydrateFromOpenPositions(...)`

### Verification Points
- Method signature unchanged (line 463)
- XML documentation preserved (lines 456-462)
- All logging statements preserved
- Idempotency guard preserved (line 484)
- Entry order pass logic preserved
- Position pass logic preserved
- Counter variables (fsmCreated, ordersIndexed) flow correctly

## V12 DNA Guardrails
- [x] Zero new lock() statements (orchestration only, no locks needed)
- [x] Zero non-ASCII characters in string literals (all logging is ASCII)
- [x] All sub-methods >= 15 LOC (parent method is ~75 lines, down from 295)
- [x] Residual method CYC target: ≤8 (orchestration only)
- [x] No logic drift -- pure structural movement only

## Post-Edit Verification (Mandatory)
```powershell
# 1. Re-establish hard links (MANDATORY after every src/ edit)
powershell -File .\deploy-sync.ps1

# 2. Complexity verification (CRITICAL - verify CYC ≤8)
python scripts/complexity_audit.py

# 3. Lock regression (must return ZERO)
grep -r "lock(" src/

# 4. ASCII gate (must return ZERO)
grep -Prn "[^\x00-\x7F]" src/
```

## Acceptance Criteria
- [ ] Method signature unchanged (line 463)
- [ ] XML documentation preserved (lines 456-462)
- [ ] Method body reduced from 295 lines to ~75 lines
- [ ] All extracted methods called correctly
- [ ] Existing `LinkTargetOrderToFSM` helper used (5 calls)
- [ ] All logging statements preserved
- [ ] Idempotency guard preserved
- [ ] Entry order pass logic preserved
- [ ] Position pass logic preserved
- [ ] Counter variables flow correctly
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows `HydrateFSMsFromWorkingOrders` CYC ≤8 ✅ TARGET MET
- [ ] lock() audit: ZERO matches
- [ ] Build passes: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible

## Success Metrics
**Before Extraction**:
- CYC: 45
- Lines: 295
- Methods: 1

**After Extraction**:
- CYC: ≤8 (82.2% reduction) ✅
- Lines: ~75 (74.6% reduction)
- Methods: 6 (5 new + 1 refactored)

**Jane Street Compliance**: ✅ All methods ≤15 (target: ≤8)