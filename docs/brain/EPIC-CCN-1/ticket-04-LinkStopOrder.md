---
# TICKET EPIC-CCN-1-04: LinkStopOrder Extraction
# Epic: EPIC-CCN-1
# Sequence: 4 of 7
# Depends on: ticket-03-CreateFollowerBracketFSM.md
---

## Objective
Extract the stop order linking and indexing logic for Pass 1 into a dedicated method to improve readability and maintain consistency with target order linking pattern

## Scope
IN scope:
- Extract `LinkStopOrder()` method from lines 531-540
- Link stop order to FSM struct
- Update `_orderIdToFsmKey` reverse index
- Increment `ordersIndexed` counter
- File: `src/V12_002.SIMA.Lifecycle.cs`

OUT of scope:
- Target order linking (already extracted in ticket-01)
- Pass 2 stop order linking (remains inline due to different semantics with recoveredStop)
- FSM registration logic (separate ticket)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-1/01-analysis.md`](01-analysis.md) -- Section "Risk Hotspots #4: Order ID Indexing Integrity"
- Approach: [`docs/brain/EPIC-CCN-1/02-approach.md`](02-approach.md) -- Section "2. Target State / Sub-Methods to Create #3"

## Implementation Instructions

### Extract New Method
Create private method with signature:
```csharp
private void LinkStopOrder(
    ref FollowerBracketFSM fsm,
    string entryKey,
    ref int ordersIndexed
)
```

**Linking Logic** (from lines 531-540):
1. TryGetValue from `stopOrders` dictionary using `entryKey`
2. If found, assign to `fsm.StopOrder`
3. If `order.OrderId` is not null/empty:
   - Index in `_orderIdToFsmKey[order.OrderId] = entryKey`
   - Increment `ordersIndexed` counter

**Estimated LOC**: 15-20 lines

### Update Parent Method Call Site (Lines 531-540)
Replace inline logic with:
```csharp
LinkStopOrder(ref fsm, entryKey, ref ordersIndexed);
```

### Note on Pass 2
**DO NOT** extract Pass 2 stop order linking (lines 662-670). It has different semantics:
- Uses `recoveredStop` variable (not dictionary lookup)
- Different control flow (already has the stop order from ScanForRecoveryKey)
- Keep inline for clarity

## V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (extraction floor)
- [ ] Method CYC target: ≤3 (1 TryGetValue + 1 null check + 1 string check)
- [ ] No logic drift -- preserve exact indexing behavior

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
- [ ] `LinkStopOrder()` method created with correct signature
- [ ] Stop order retrieved from `stopOrders` dictionary
- [ ] `fsm.StopOrder` field assigned correctly
- [ ] `_orderIdToFsmKey` index updated for stop order
- [ ] `ordersIndexed` counter incremented correctly
- [ ] Pass 1 call site updated to use helper
- [ ] Pass 2 stop linking remains inline (NOT extracted)
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows CYC ≤3 for `LinkStopOrder`
- [ ] lock() audit: ZERO matches
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible

## Estimated Effort
**20 minutes** (LOW risk, simple dictionary operation)