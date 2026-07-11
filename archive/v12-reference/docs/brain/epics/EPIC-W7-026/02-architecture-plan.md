# Phase 2: Architecture Plan — EPIC-W7-026

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-07-01T00:00:00Z
**Input:** docs/brain/EPIC-W7-026/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `ProcessQueuedAccountOrder`
- **Source File:** [`src/V12_002.Orders.Callbacks.AccountOrders.cs`](src/V12_002.Orders.Callbacks.AccountOrders.cs)
- **Lines:** 1054–1101
- **Original CYC:** 17

### jcodemunch get_context_bundle result

Symbol resolved via ID `src/V12_002.Orders.Callbacks.AccountOrders.cs::V12_002.ProcessQueuedAccountOrder#method`. Full source confirmed (47-line private void method). Body contains two sequential early-return guards (EventArgs null + instrument filter), a GHOST-AUDIT Print call, an unconditional cancellation pre-filter gate, a single-allocation snapshot (`activePositions.ToArray()`), a `foreach` scan loop with compound 3-predicate filter and `TryFindOrderInPosition` identity search, and a matched/unmatched dispatch branch routing to `HandleMatchedFollowerOrder` or `ExecuteFollowerCascadeCleanup`.

### jcodemunch get_call_hierarchy result

**Callers (depth 1):** `ProcessAccountOrderQueue` (line 182, same file — drain loop, single call per dequeued item).
**Callers (depth 2):** `ProcessAccountOrder_EnqueueTerminalUpdate` (line 154) — enqueues into the drain queue; does not call `ProcessQueuedAccountOrder` directly.
**Callees (depth 1):** `ProcessFollowerCancellationUnconditional` (line 1002), `activePositions` constant, `TryFindOrderInPosition` (line 262), `HandleMatchedFollowerOrder` (line 472), `ExecuteFollowerCascadeCleanup` (line 793) — 5 direct callees, all confirmed by Phase 0.
**Callees (depth 2):** `HandleMatchedFollower_PendingCancelReplace`, `HandleMatchedFollower_TargetReplaceCancel`, `HandleMatchedFollower_StopReplacement`, `HandleMatchedFollower_PendingCleanupPurge`, `ExecuteFollowerCascade_SuppressMasterReplace`, `ExecuteFollowerCascade_ResolveFollowers`, `ExecuteFollowerCascade_CleanupUnfilled`, `ExecuteFollowerCascade_EmergencyFlattenFilled` — downstream FSM handlers, none modified by this epic.

### jcodemunch get_dependency_graph result

File-level edge count: 0 (imports: none, importers: none). The file is a C# partial class within a single assembly; all dependencies resolve within the same binary. Cross-file blast radius is zero — confirmed. Scope is bounded entirely to `src/V12_002.Orders.Callbacks.AccountOrders.cs`.

### jcodemunch get_extraction_candidates result

Tool returned 0 candidates (min_complexity=3, min_callers=1). This is expected — the jCodemunch index does not populate per-method caller counts for C# partial class files in this repo. The extraction plan is grounded in direct source analysis from Phase 0 and Phase 1 artifacts, which are authoritative. No blockers.

---

## Sequential Thinking Summary

Five-thought chain completed (chain length reached 5/5, `nextThoughtNeeded: false`).

**Thought 1** — Decomposed body-local vs. transitive CYC. Body-local decision nodes = 8 (two guards, cancellation gate, foreach, stale-key guard, 3-predicate compound filter, TryFindOrderInPosition result, matched gate). Transitive = 9 (from `ProcessFollowerCancellationUnconditional` and `ExecuteFollowerCascadeCleanup` callees). Target: remove 7 body-local nodes via 3 extractions → parent CYC ~4.

**Thought 2** — Designed precise method signatures for all three helpers. `IsValidQueuedOrderForThisInstrument` (CYC 3), `TryMatchFollowerPositionInSnapshot` with out params (CYC 7), `DispatchMatchedFollowerResult` (CYC 4). Parent after extraction: CYC 4 (2 early-return branches + base 1 + 1 guard call node).

**Thought 3** — Verified all Jane Street rules: CYC ≤ 8 for all helpers and parent ✓, single-responsibility ✓, no lock() blocks ✓, illegal states unrepresentable ✓, zero-allocation hot path (snapshot passed by array ref) ✓, guard clauses extracted ✓, loop body extracted ✓.

**Thought 4** — Corroborated jCodemunch call hierarchy (1 direct caller, 5 direct callees) against Phase 0 data. Confirmed zero cross-file blast radius. Extraction candidates tool returning empty is a known limitation of the index for C# partials; source-based plan is authoritative.

**Thought 5** — Final synthesis: max_cyc_projected = 7 (`TryMatchFollowerPositionInSnapshot`). extraction_count = 3. All helpers ≤ 8, parent CYC = 4. Plan verified correct and complete.

---

## Extraction Plan

| Helper Method Name | Responsibility | Signature | Projected CYC |
|---|---|---|---|
| `IsValidQueuedOrderForThisInstrument` | Merges the two sequential early-return guards (EventArgs/Order null check + instrument filter) into a single named predicate. Replaces 2 if-return chains with 1 guard call in the parent. | `private bool IsValidQueuedOrderForThisInstrument(QueuedAccountOrderUpdate item)` | **3** |
| `TryMatchFollowerPositionInSnapshot` | Extracts the full snapshot scan loop: stale-key guard, compound IsFollower/null/account filter, and `TryFindOrderInPosition` identity search. Populates `matchedEntry` and `matchedPos` via out params. Returns true on first match. | `private bool TryMatchFollowerPositionInSnapshot(QueuedAccountOrderUpdate item, Order order, KeyValuePair<string, PositionInfo>[] snapshot, out string matchedEntry, out PositionInfo matchedPos)` | **7** |
| `DispatchMatchedFollowerResult` | Routes to `HandleMatchedFollowerOrder` (matched path) or `ExecuteFollowerCascadeCleanup` (unmatched/orphan path) based on match state. Makes the cascade fallback explicit and auditable. | `private void DispatchMatchedFollowerResult(string matchedEntry, PositionInfo matchedPos, Order order, string acctName, string reason, KeyValuePair<string, PositionInfo>[] snapshot)` | **4** |

---

## Parent Method After Extraction

**Remaining logic in `ProcessQueuedAccountOrder` after extraction:**

```
private void ProcessQueuedAccountOrder(QueuedAccountOrderUpdate item)
{
    if (!IsValidQueuedOrderForThisInstrument(item))      // guard helper call
        return;
    Order order = item.EventArgs.Order;
    string reason = order.OrderState.ToString().ToUpper();
    string acctName = item.Account != null ? item.Account.Name : "UNKNOWN";
    Print(string.Format("[GHOST-AUDIT] ...", order.Name, reason, acctName));

    if (ProcessFollowerCancellationUnconditional(order, acctName, reason))  // cancellation gate
        return;

    var snapshot = activePositions.ToArray();            // single allocation (Build 935 [R-01])
    TryMatchFollowerPositionInSnapshot(item, order, snapshot,
        out string matchedEntry, out PositionInfo matchedPos);
    DispatchMatchedFollowerResult(matchedEntry, matchedPos, order, acctName, reason, snapshot);
}
```

**Decision nodes remaining:** 2 (guard return + cancellation gate return) + base 1 = **CYC 4**
**Projected CYC: 4**

---

## max_cyc_projected: 7
## extraction_count: 3

---

## Jane Street Alignment

| Rule | Status | Evidence |
|---|---|---|
| CYC ≤ 8 achieved | **YES** | Parent: 4, Helpers: 3, 7, 4 — max 7 |
| Single-responsibility per helper | **YES** | Guard validation / scan loop / dispatch routing — each does one thing |
| Lock-free / Actor pattern preserved | **YES** | No lock() in source; all state access via NinjaTrader strategy-thread contract; no new synchronization primitives introduced |
| Illegal states unrepresentable | **YES** | `IsValidQueuedOrderForThisInstrument` prevents processing null orders; out params default to null/empty on no-match; `DispatchMatchedFollowerResult` handles both matched and unmatched exhaustively with no silent fallthrough |
| Zero-allocation hot path | **YES** | Snapshot array (`activePositions.ToArray()`) allocated once in parent (Build 935 [R-01]); passed by array reference to all helpers; no boxing or new collections in extracted helpers |
| Extract Guard Clauses | **YES** | `IsValidQueuedOrderForThisInstrument` collapses 2 sequential guards into 1 named call |
| Extract Loop Body | **YES** | `TryMatchFollowerPositionInSnapshot` is the full loop body extraction |
| Single-method scope (V12.23) | **YES** | Extractions are private helpers in same partial class; no external interface changes |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Epic** | EPIC-W7-026 |
| **Source File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **Method in Scope** | `ProcessQueuedAccountOrder` |
| **Original CYC** | 17 |
| **max_cyc_projected** | 7 |
| **extraction_count** | 3 |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-07-01T00:00:00Z |
| **jcodemunch tools called** | `resolve_repo`, `get_context_bundle`, `get_call_hierarchy`, `get_dependency_graph`, `get_extraction_candidates` |
| **sequential-thinking calls** | 5 |
