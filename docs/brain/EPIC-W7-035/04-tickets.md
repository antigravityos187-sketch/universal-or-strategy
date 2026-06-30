# Phase 4: Implementation Tickets — EPIC-W7-035

**Epic:** EPIC-W7-035
**Method:** SyncLimitTarget
**Source:** src/V12_002.Orders.Management.StopSync.cs
**Original CYC:** 34
**Wave:** 7 | **Phase:** 4 — Ticket Generation

---

## ticket_count: 3

---

## Ticket 1
- **ticket_id:** 1
- **helper_name:** `SetTargetPrice`
- **concern:** Price-slot stamping — assign `pos.Target{n}Price = price` for targetNum 1–5; guard default (invalid targetNum). Eliminates BOTH duplicated `switch(targetNum)` blocks in the original method.
- **lines_to_move:** The two duplicated `switch(targetNum)` blocks embedded in both the reprice arm and the submit arm of SyncLimitTarget (lines 176–336). Both assign `pos.Target{n}Price` for slots 1–5 and are merged into a single private helper.
- **signature:** `private void SetTargetPrice(PositionInfo pos, int targetNum, double price)`
- **cyc_reduction:** ~10 (eliminates two 5-case switch blocks from parent)
- **projected_helper_cyc:** 7

---

## Ticket 2
- **ticket_id:** 2
- **helper_name:** `SyncLimitTarget_Reprice`
- **concern:** Reprice path — execute repricing of an existing working order: delta-price guard → `ChangeOrder` → `SetTargetPrice` → `Print` → `refreshed++`. One `try/catch`. Called only when `hasWorkingOrder == true`.
- **lines_to_move:** The `hasWorkingOrder == true` arm of SyncLimitTarget (approximately lines 245–285 of the original method body). Includes: `if (Math.Abs(existingOrder.LimitPrice - newPrice) < tickSize)` early-return guard, `ChangeOrder` broker API call, `SetTargetPrice` call, `Print` log line, and `refreshed++` increment — all wrapped in a single `try/catch`.
- **signature:** `private void SyncLimitTarget_Reprice(string entryName, PositionInfo pos, int targetNum, Order existingOrder, double newPrice, ref int refreshed)`
- **cyc_reduction:** ~4 (removes delta guard, two-path branch, and try/catch from parent)
- **projected_helper_cyc:** 4

---

## Ticket 3
- **ticket_id:** 3
- **helper_name:** `SyncLimitTarget_Submit`
- **concern:** Submit path — submit a new unmanaged limit order: resolve `exitAction` direction ternary → `SubmitOrderUnmanaged` → null guard → `targetDict` write → `SetTargetPrice` → `Print` → `refreshed++`. One `try/catch`. Called only when `hasWorkingOrder == false`.
- **lines_to_move:** The `hasWorkingOrder == false` arm of SyncLimitTarget (approximately lines 287–330 of the original method body). Includes: `exitAction` ternary (`Long→Sell` / `else BuyToCover`), `SubmitOrderUnmanaged` broker API call, `if (newLimit != null)` null guard with `targetDict[entryName] = newLimit` write, `SetTargetPrice` call, `Print` log lines, and `refreshed++` increment — all wrapped in a single `try/catch`.
- **signature:** `private void SyncLimitTarget_Submit(string entryName, PositionInfo pos, int targetNum, int targetQty, ConcurrentDictionary<string, Order> targetDict, double newPrice, ref int refreshed)`
- **cyc_reduction:** ~16 (removes direction ternary, try/catch, null guard, and multiple Print branches from parent)
- **projected_helper_cyc:** 4

---

## projected_parent_cyc_after_all: 4

**Residual SyncLimitTarget body (post all 3 extractions):**
```
baseline(1)
+ if (newPrice <= 0) { return; }      → +1
+ if (hasWorkingOrder)                → +1
    SyncLimitTarget_Reprice(...)
+ else                                → +1
    SyncLimitTarget_Submit(...)
= CYC 4
```

**Post-extraction CYC table:**

| Symbol | Projected CYC | Status |
|---|---|---|
| `SetTargetPrice` | 7 | ✅ ≤ 8 |
| `SyncLimitTarget_Reprice` | 4 | ✅ ≤ 8 |
| `SyncLimitTarget_Submit` | 4 | ✅ ≤ 8 |
| `SyncLimitTarget` (parent, post-extraction) | 4 | ✅ ≤ 8 |
| **Max projected CYC** | **7** | ✅ Jane Street threshold met |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-035 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **jcodemunch tools called** | resolve_repo, search_symbols, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket breakdown thoughts) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 4 |
| **Original CYC** | 34 |
