---
# TICKET EPIC-CCN-1-01: LinkTargetOrderToFSM Duplication Elimination
# Epic: EPIC-CCN-1
# Sequence: 1 of 7
# Depends on: NONE
---

## Objective
Extract the duplicated target-order-to-FSM linking logic (80 lines → 20 lines) into a single reusable parameterized method to eliminate the 5x duplication pattern across Pass 1 and Pass 2

## Scope
IN scope:
- Extract `LinkTargetOrderToFSM()` method from lines 543-588 pattern
- Replace 5x Pass 1 target blocks (lines 543-588) with parameterized calls
- Replace 5x Pass 2 target blocks (lines 684-729) with parameterized calls
- File: `src/V12_002.SIMA.Lifecycle.cs`

OUT of scope:
- Stop order linking (separate ticket)
- FSM registration logic (separate ticket)
- State mapping or FSM creation (separate tickets)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-1/01-analysis.md`](01-analysis.md) -- Section "Risk Hotspots #5: Duplication Elimination Risk"
- Approach: [`docs/brain/EPIC-CCN-1/02-approach.md`](02-approach.md) -- Section "2. Target State / Sub-Methods to Create #4"

## Implementation Instructions

### Extract New Method
Create private method with signature:
```csharp
private void LinkTargetOrderToFSM(
    ref FollowerBracketFSM fsm,
    string entryKey,
    int targetIndex,
    ConcurrentDictionary<string, Order> targetDict,
    ref int ordersIndexed
)
```

**Logic** (from lines 543-588 pattern):
1. TryGetValue from `targetDict` using `entryKey`
2. If found, assign to `fsm.Targets[targetIndex]`
3. If OrderId is not null/empty, index in `_orderIdToFsmKey[order.OrderId] = entryKey`
4. Increment `ordersIndexed` counter

**Estimated LOC**: 15-20 lines

### Replace Pass 1 Target Blocks (Lines 543-588)
Replace 5 identical blocks with:
```csharp
LinkTargetOrderToFSM(ref fsm, entryKey, 0, target1Orders, ref ordersIndexed);
LinkTargetOrderToFSM(ref fsm, entryKey, 1, target2Orders, ref ordersIndexed);
LinkTargetOrderToFSM(ref fsm, entryKey, 2, target3Orders, ref ordersIndexed);
LinkTargetOrderToFSM(ref fsm, entryKey, 3, target4Orders, ref ordersIndexed);
LinkTargetOrderToFSM(ref fsm, entryKey, 4, target5Orders, ref ordersIndexed);
```

### Replace Pass 2 Target Blocks (Lines 684-729)
Replace 5 identical blocks with same pattern:
```csharp
LinkTargetOrderToFSM(ref fsm, recoveredKey, 0, target1Orders, ref ordersIndexed);
LinkTargetOrderToFSM(ref fsm, recoveredKey, 1, target2Orders, ref ordersIndexed);
LinkTargetOrderToFSM(ref fsm, recoveredKey, 2, target3Orders, ref ordersIndexed);
LinkTargetOrderToFSM(ref fsm, recoveredKey, 3, target4Orders, ref ordersIndexed);
LinkTargetOrderToFSM(ref fsm, recoveredKey, 4, target5Orders, ref ordersIndexed);
```

## V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (extraction floor)
- [ ] Residual method CYC reduction verified
- [ ] No logic drift -- pure structural movement only

## Post-Edit Verification (Mandatory)
```powershell
# 1. Re-establish hard links (MANDATORY after every src/ edit)
powershell -File .\deploy-sync.ps1

# 2. Complexity verification
python scripts/complexity_audit.py

# 3. Lock regression (must return ZERO)
grep -r "lock(" src/

# 4. ASCII gate (must return ZERO)
grep -Prn "[^\x00-\x7F]" src/
```

## Acceptance Criteria
- [ ] `LinkTargetOrderToFSM()` method created with correct signature
- [ ] Pass 1: 5 target blocks replaced with parameterized calls (80 lines → 5 lines)
- [ ] Pass 2: 5 target blocks replaced with parameterized calls (45 lines → 5 lines)
- [ ] `_orderIdToFsmKey` index integrity maintained (same OrderId mappings)
- [ ] `ordersIndexed` counter increments correctly
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows CYC ≤3 for `LinkTargetOrderToFSM`
- [ ] lock() audit: ZERO matches
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible

## Estimated Effort
**30 minutes** (LOW risk, pure refactoring, no logic changes)