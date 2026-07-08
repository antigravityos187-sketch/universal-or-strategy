# EPIC-W7-044 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-044/02-architecture-plan.md, docs/brain/EPIC-W7-044/03-audit-report.md

---

## Header

| Field | Value |
|-------|-------|
| **Epic** | EPIC-W7-044 |
| **Method** | `SymmetryGuardCascadeFollowerCleanup` |
| **Source File** | `src/V12_002.Symmetry.Replace.cs` |
| **Original CYC** | 11 |
| **Wave** | 7 |
| **Ticket Count** | 4 |
| **Projected Parent CYC After All Tickets** | 3 |
| **DNA Verdict (Phase 3)** | PASS — zero violations |

---

## Ticket Overview

| Ticket | Helper Name | Concern | CYC Impact |
|--------|-------------|---------|-----------|
| T1 | `IsFollowerEntryLive` | Extract OrderState multi-predicate (Working/Submitted/Accepted) into static inline | Adds helper CYC=4; parent unchanged |
| T2 | `TryResolveCascadeContext` | Extract double dictionary resolution + ctx.Followers snapshot into bool out-param resolver | Adds helper CYC=3; parent unchanged |
| T3 | `TryCancelFollowerEntry` | Extract per-follower guard chain + liveness gate + logging + CancelOrderSafe into void helper | Adds helper CYC=6; parent unchanged |
| T4 | *(parent refactor)* | Replace parent body to call helpers; drives parent CYC 11 → 3 | Parent CYC: 11 → 3 |

---

## Ticket Definitions

---

### TICKET-1: Extract `IsFollowerEntryLive`

| Field | Value |
|-------|-------|
| **ticket_id** | EPIC-W7-044-T1 |
| **helper_name** | `IsFollowerEntryLive` |
| **concern** | Extract the OrderState multi-predicate (`Working \|\| Submitted \|\| Accepted`) into a standalone static private method. This is a pure, stateless predicate with no instance state access. Marked `AggressiveInlining` (hot path, zero allocation). |
| **lines_to_move** | Lines ~56–61 of original method body (the `if (order.OrderState == OrderState.Working \|\| order.OrderState == OrderState.Submitted \|\| order.OrderState == OrderState.Accepted)` compound condition) |
| **cyc_reduction** | 0 from parent this ticket (helper added, parent body not yet changed) |
| **projected_helper_cyc** | 4 (base=1 + Working=1 + Submitted=1 + Accepted=1) |

**Action:** Insert the following private static method into `src/V12_002.Symmetry.Replace.cs` (same partial class `V12_002`), positioned before `SymmetryGuardCascadeFollowerCleanup`:

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsFollowerEntryLive(Order order)
{
    return order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Submitted
        || order.OrderState == OrderState.Accepted;
}
```

**Safety:** Additive-only edit. Does not modify existing code. No callers added yet.

**Dependency:** None. Execute first (or concurrently with T2).

---

### TICKET-2: Extract `TryResolveCascadeContext`

| Field | Value |
|-------|-------|
| **ticket_id** | EPIC-W7-044-T2 |
| **helper_name** | `TryResolveCascadeContext` |
| **concern** | Extract the double dictionary resolution chain (`symmetryMasterEntryToDispatch.TryGetValue` + `symmetryDispatchById.TryGetValue`) and `ctx.Followers` immutable snapshot read into a boolean `out`-param resolver. Returns `false` on either miss. Marked `AggressiveInlining` (cold-path gate, zero allocation). |
| **lines_to_move** | Lines ~199–204 of original method body (both `TryGetValue` early-exit guards and `string[] followers = ctx.Followers` assignment) |
| **cyc_reduction** | 0 from parent this ticket (helper added, parent body not yet changed) |
| **projected_helper_cyc** | 3 (base=1 + TryGetValue miss symmetryMasterEntryToDispatch=1 + TryGetValue miss symmetryDispatchById=1) |

**Action:** Insert the following private method into `src/V12_002.Symmetry.Replace.cs` (same partial class `V12_002`), positioned before `SymmetryGuardCascadeFollowerCleanup`:

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool TryResolveCascadeContext(string masterEntryName, out string[] followers)
{
    followers = Array.Empty<string>();
    if (!symmetryMasterEntryToDispatch.TryGetValue(masterEntryName, out string dispatchId))
        return false;
    if (!symmetryDispatchById.TryGetValue(dispatchId, out var ctx))
        return false;
    followers = ctx.Followers;   // ADR-019: immutable snapshot, lock-free read
    return true;
}
```

**Safety:** Additive-only edit. Does not modify existing code. No callers added yet.

**Dependency:** None. Execute concurrently with T1, before T4.

---

### TICKET-3: Extract `TryCancelFollowerEntry`

| Field | Value |
|-------|-------|
| **ticket_id** | EPIC-W7-044-T3 |
| **helper_name** | `TryCancelFollowerEntry` |
| **concern** | Extract the entire per-follower processing body from the `foreach` loop: `activePositions` lookup guard, `entryOrders` lookup guard, `null` order guard, `IsFollowerEntryLive` liveness gate, `Print`/`string.Format` logging with `ExecutingAccount` ternary, and `CancelOrderSafe` call. Marked `NoInlining` (cold logging path with `Print`/`string.Format`; per carl_cook cold-path extraction pattern). Preserves A2-3 deferred delta rollback comment verbatim. |
| **lines_to_move** | Lines ~218–237 of original method body (entire `foreach` body: all guard `continue` checks, the `if (order.OrderState == ...)` block including `Print` and `CancelOrderSafe`, and the A2-3 comment) |
| **cyc_reduction** | 0 from parent this ticket (helper added, parent body not yet changed) |
| **projected_helper_cyc** | 6 (base=1 + activePositions miss=1 + entryOrders miss=1 + null order=1 + IsFollowerEntryLive gate=1 + ExecutingAccount null ternary=1) |

**Action:** Insert the following private method into `src/V12_002.Symmetry.Replace.cs` (same partial class `V12_002`), positioned before `SymmetryGuardCascadeFollowerCleanup`:

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void TryCancelFollowerEntry(string followerName)
{
    if (!activePositions.TryGetValue(followerName, out var pos))
        return;
    if (!entryOrders.TryGetValue(followerName, out var order))
        return;
    if (order == null)
        return;
    if (!IsFollowerEntryLive(order))
        return;

    Print(
        string.Format(
            "[CASCADE] Cancelling follower entry: {0} (Acc: {1})",
            followerName,
            pos.ExecutingAccount != null ? pos.ExecutingAccount.Name : "Master"
        )
    );
    CancelOrderSafe(order, pos);
    // A2-3: DeltaExpectedPositionLocked deferred to OnAccountOrderUpdate confirmed-cancel
    // to prevent REAPER desync if the follower was microseconds from filling (Build 960 audit fix).
}
```

**Safety:** Additive-only edit. Does not modify existing code. No callers added yet.

**Dependency:** T1 (`IsFollowerEntryLive`) must be present in file before this ticket executes.

---

### TICKET-4: Refactor `SymmetryGuardCascadeFollowerCleanup` (Parent)

| Field | Value |
|-------|-------|
| **ticket_id** | EPIC-W7-044-T4 |
| **helper_name** | *(parent — no new helper)* |
| **concern** | Replace the body of `SymmetryGuardCascadeFollowerCleanup` with calls to the three extracted helpers. Drives parent CYC from 11 to 3. Preserves method signature exactly (`private void SymmetryGuardCascadeFollowerCleanup(string masterEntryName)`). Preserves the `[CASCADE] Master … cancelled` log call verbatim. |
| **lines_to_move** | Entire body of `SymmetryGuardCascadeFollowerCleanup` (lines ~199–243); replaced with 7-line refactored body below. |
| **cyc_reduction** | 8 (parent CYC: 11 → 3) |
| **projected_helper_cyc** | N/A (parent method, not a new helper) |

**Action:** Replace the body of `SymmetryGuardCascadeFollowerCleanup` in `src/V12_002.Symmetry.Replace.cs` with:

```csharp
private void SymmetryGuardCascadeFollowerCleanup(string masterEntryName)
{
    if (!TryResolveCascadeContext(masterEntryName, out string[] followers))
        return;

    Print(
        string.Format(
            "[CASCADE] Master {0} cancelled -- terminating {1} linked follower(s).",
            masterEntryName,
            followers.Length
        )
    );

    foreach (string followerName in followers)
    {
        TryCancelFollowerEntry(followerName);
    }
}
```

**Parent CYC After:** 3 (base=1 + `TryResolveCascadeContext` gate=1 + `foreach` loop=1)

**Safety Constraints:**
1. Method signature `private void SymmetryGuardCascadeFollowerCleanup(string masterEntryName)` — FROZEN. Do not alter.
2. Caller `HandleOrderCancelled_RollbackUnfilledEntry` — NOT modified by this ticket.
3. ADR-019 immutable snapshot — preserved inside `TryResolveCascadeContext` (T2).
4. A2-3 deferred delta rollback comment — preserved verbatim inside `TryCancelFollowerEntry` (T3).
5. Two-phase cancel/rollback FSM ordering — `SymmetryGuardCascadeFollowerCleanup` returns before caller's `RollbackExpectedPosition` / `CleanupPosition` sequence runs; extraction does not alter this ordering.

**Dependency:** T2 (`TryResolveCascadeContext`) and T3 (`TryCancelFollowerEntry`) must both be present in file.

---

## CYC Summary After All Tickets

| Symbol | CYC Before | CYC After | Limit | Status |
|--------|-----------|-----------|-------|--------|
| `SymmetryGuardCascadeFollowerCleanup` (parent) | 11 | **3** | 8 | PASS |
| `TryResolveCascadeContext` (new, T2) | — | **3** | 8 | PASS |
| `IsFollowerEntryLive` (new, T1) | — | **4** | 8 | PASS |
| `TryCancelFollowerEntry` (new, T3) | — | **6** | 8 | PASS |
| **Max CYC projected** | — | **6** | **8** | **PASS** |

**projected_parent_cyc_after_all: 3**

---

## Execution Order

```
T1 (IsFollowerEntryLive)       — additive, no deps
T2 (TryResolveCascadeContext)  — additive, no deps (can run concurrently with T1)
T3 (TryCancelFollowerEntry)    — additive, requires T1 present
T4 (parent refactor)           — surgical, requires T2 + T3 present
```

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-044 |
| **Method** | `SymmetryGuardCascadeFollowerCleanup` |
| **Source File** | `src/V12_002.Symmetry.Replace.cs` |
| **Original CYC** | 11 |
| **Ticket Count** | 4 |
| **projected_parent_cyc_after_all** | 3 |
| **Max Helper CYC** | 6 (`TryCancelFollowerEntry`) |
| **DNA Verdict (Phase 3 input)** | PASS — zero violations |
| **MCP Tools Used** | `resolve_repo`, `get_symbol_complexity` (not indexed — CYC confirmed via Phase 2 context bundle), `get_extraction_candidates` (0 automated candidates — manual analysis applied per Phase 2), `sequentialthinking` (4 thoughts: 1 probe + 3 analytical) |
| **Sequential Thinking Thoughts** | 4 (1 probe + 3 analytical) |
| **Output** | docs/brain/EPIC-W7-044/04-tickets.md |
