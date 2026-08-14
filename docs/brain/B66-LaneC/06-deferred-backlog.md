# B66-LaneC Deferred Backlog

---

## DW-B66-C-02 -- DispatchCopy dedup key = 0.0 for all StopLimit entries

**Status**: OPEN
**Priority**: P1
**Target block**: B67+
**Created**: B66-LaneC Phase 1 architecture plan
**Location**: `src/PropTraderTools/CopyEngine.cs` line 805

### Description

`DispatchCopy` Gate 5 passes `order.LimitPrice` as the dedup key to `IsDedup`:

```csharp
// Current (Gate 5, line 805):
if (IsDedup(order.OrderId.ToString(), order.LimitPrice))
    return;
```

For `OrderType.StopLimit`, `LimitPrice` is always 0 (NT8 confirmed -- see B66-LaneC plan Section 2,
Fact 1). This means every StopLimit entry on every instrument shares dedup key `0.0`:

- First StopLimit entry: `IsDedup("orderId-A", 0.0)` -- cache miss, dispatches correctly.
- Second StopLimit entry: `IsDedup("orderId-B", 0.0)` -- cache hit on key `0.0` regardless of
  instrument or order ID, wrongly blocked as duplicate.

### Impact

After the B66-LaneC drag-sync fixes land (Defects 1-3), drag-sync for StopLimit orders will work.
However, on accounts with two or more concurrent StopLimit entry orders, the second (and any
subsequent) StopLimit copy dispatch will be silently swallowed by the dedup gate. The first
StopLimit entry copies correctly; others do not.

Severity: P1 -- affects correctness when trader has multiple concurrent StopLimit entries.
Workaround: None (silent failure, no error surfaced to UI).

### Root Cause

Gate 5 was written when only `Limit` and `Market` orders were supported (B62). `LimitPrice` was
always a real, non-zero price for Limit orders. The field choice was correct for that scope.
The StopLimit extension (this block) exposes the invariant violation.

### Fix Approach

Replace line 805 with a price that is meaningful for the order type:

```csharp
// Fixed Gate 5 (B67+):
double dedupPrice = order.OrderType == OrderType.StopLimit ? order.StopPrice : order.LimitPrice;
if (IsDedup(order.OrderId.ToString(), dedupPrice))
    return;
```

If `DispatchCopy` CYC is already >= 7 pre-change, extract the selection into a dedicated helper:

```csharp
// Optional helper if DispatchCopy CYC would exceed 8:
private static double GetDedupPrice(Order order)
    => order.OrderType == OrderType.StopLimit ? order.StopPrice : order.LimitPrice;
```

### Why Deferred (not fixed in B66-LaneC)

Scope creep risk per AGENTS.md Section 11: ONE EPIC = ONE CONCERN.

- `IsDedup` and `DispatchCopy` Gate 5 are intersected by ALL copy paths: Market, Limit, StopLimit,
  StopMarket. Any change here risks regressions in the tested Limit and Market paths.
- The drag-sync fixes (Defects 1-3) are independent of this dedup issue. They operate on the
  `OnOrderUpdate` event path, not the `DispatchCopy` path. Shipping them together is unnecessary.
- A dedicated PR for this fix with its own targeted test coverage is architecturally safer.
- Blast radius of `IsDedup` signature change requires verification across all dispatch call sites.

### Test Requirements (when fixed in B67+)

Minimum 2 additional tests:
- `T_B67_01_DispatchCopy_SecondStopLimitEntry_NotDeduped` -- second StopLimit on same account
  dispatches correctly (dedup key is StopPrice, not shared 0.0).
- `T_B67_02_DispatchCopy_LimitEntry_DedupUnaffected` -- Limit order dedup still uses LimitPrice
  (regression guard).

### Dependencies

- B66-LaneC Defects 1-3 must be merged first (provides the entry-copy foundation for StopLimit).
- Engineer must measure `DispatchCopy` CYC before the fix; if CYC >= 7, extract `GetDedupPrice`.

---

*Deferred backlog entry created by ptt-architect during B66-LaneC Phase 1 plan rewrite.*
