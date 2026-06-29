# Phase 2: Architecture Plan — EPIC-W7-072

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T04:00:00Z
**Input:** docs/brain/EPIC-W7-072/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `ProcessAccountOrder_UpdateMasterExpected`
- **Source File:** `src/V12_002.Orders.Callbacks.AccountOrders.cs`
- **Lines:** 81–115
- **Signature:** `private void ProcessAccountOrder_UpdateMasterExpected(Order order)`
- **Original CYC:** 12

### jcodemunch get_context_bundle result

Full source confirmed at lines 81–115. The method body contains:
- Outer fill-state guard: `if (order.OrderState == Filled || PartFilled)`
- Stop-fill branch: `if (order.Name.StartsWith("Stop_"))` → `_nakedPositionFirstSeen.TryRemove` + `Enqueue(ctx => SetExpectedPositionLocked(mExpKey, 0))`
- Target-fill branch: `else if (order.Name.StartsWith("T") && order.Name.Contains("_"))` → captures `filledQty`/`mExpKey` on broker thread, enqueues lambda with `null`-guard, `TryGetValue`, and direction-aware `if/else-if` signed-delta arithmetic (`Math.Max`/`Math.Min`)
- All state mutations are deferred via `Enqueue` (Actor pattern); only `_nakedPositionFirstSeen.TryRemove` executes directly on the broker thread (ConcurrentDictionary — thread-safe)

### jcodemunch get_call_hierarchy result

- **Callers (depth 1):** `OnAccountOrderUpdate` (`src/V12_002.Orders.Callbacks.AccountOrders.cs:37`) — 1 direct caller
- **Callees (depth 1):** `_nakedPositionFirstSeen` (ConcurrentDictionary field), `ExpKey` (`src/V12_002.SIMA.cs:209`), `Enqueue` (`src/V12_002.cs:428`)
- **Callees (depth 2):** `_cmdQueue`, `IsActorThread`, `TryDrain`, `ScheduleActorDrain` (all via `Enqueue`)
- **Threading:** Broker-thread caller; all expectedPositions mutations marshalled to strategy thread via `Enqueue`

### jcodemunch get_dependency_graph result

- **File-level imports:** 0 external file edges (no resolved import graph edges for this partial-class file — all dependencies resolved within the same partial class compilation unit)
- **Importers:** None at file-graph level (caller relationship is intra-compilation-unit, not a file import)

### jcodemunch get_extraction_candidates result

- No candidates returned (min_callers=1, min_complexity=3) — the index did not resolve intra-file callers for this partial class pattern. Source analysis via context_bundle was used directly for extraction planning.

---

## Sequential Thinking Summary

**Thought 5 (final verdict):**
All three resulting methods satisfy Jane Street mandates. CYC<=8 achieved across the board — parent dispatcher CYC=6, `HandleMasterTargetFill` CYC=5, `HandleMasterStopFill` CYC=1, max=6 which is well under the threshold of 8. Single-responsibility per helper is preserved: `HandleMasterStopFill` does exactly one thing (clear naked grace and zero expected position), `HandleMasterTargetFill` does exactly one thing (compute and enqueue direction-aware signed delta for expected position), and the parent does exactly one thing (guard on fill state and dispatch). The Actor/Enqueue model is fully preserved — all state mutations remain deferred via `Enqueue`; the broker-thread-safe `TryRemove` remains on the broker thread. Illegal states remain unrepresentable: the fill-state guard stays in the parent before any helper is called, so helpers always operate in a valid-state context. Zero-allocation hot paths: `HandleMasterStopFill` allocates nothing new; `HandleMasterTargetFill` re-uses the existing lambda capture pattern — no new heap objects introduced by the extraction itself.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `HandleMasterStopFill` | Clears `_nakedPositionFirstSeen` grace for master account and enqueues `SetExpectedPositionLocked(mExpKey, 0)` to zero the master expected position on a stop fill. No conditionals — pure sequential execution of the two stop-fill side effects. | 1 |
| `HandleMasterTargetFill` | Captures `filledQty` and `mExpKey` on the broker thread; enqueues a lambda that null-guards `expectedPositions`, reads `currentExp` via `TryGetValue`, then applies direction-aware signed-delta arithmetic (`Math.Max(0, currentExp - filledQty)` for long; `Math.Min(0, currentExp + filledQty)` for short) before calling `SetExpectedPositionLocked`. | 5 |

### Helper Signatures

```csharp
private void HandleMasterStopFill()
{
    _nakedPositionFirstSeen.TryRemove(Account.Name, out _);
    var mExpKey = ExpKey(Account.Name);
    Enqueue(ctx => ctx.SetExpectedPositionLocked(mExpKey, 0));
}
```

CYC breakdown: base=1, no decision points → **CYC = 1**

```csharp
private void HandleMasterTargetFill(Order order)
{
    int filledQty = order.Filled;
    var mExpKey = ExpKey(Account.Name);
    Enqueue(ctx =>
    {
        if (
            ctx.expectedPositions != null
            && ctx.expectedPositions.TryGetValue(mExpKey, out int currentExp)
        )
        {
            int newExp = 0;
            if (currentExp > 0)
                newExp = Math.Max(0, currentExp - filledQty);
            else if (currentExp < 0)
                newExp = Math.Min(0, currentExp + filledQty);

            ctx.SetExpectedPositionLocked(mExpKey, newExp);
        }
    });
}
```

CYC breakdown: base=1, `&&` in lambda null-guard +1, Enqueue lambda branch +1, `if (currentExp > 0)` +1, `else if (currentExp < 0)` +1 → **CYC = 5**

---

## Parent Method After Extraction

### Remaining Logic

```csharp
private void ProcessAccountOrder_UpdateMasterExpected(Order order)
{
    if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.PartFilled)
    {
        if (order.Name.StartsWith("Stop_"))
            HandleMasterStopFill();
        else if (order.Name.StartsWith("T") && order.Name.Contains("_"))
            HandleMasterTargetFill(order);
    }
}
```

### CYC Breakdown for Parent After Extraction

| Decision Point | CYC Delta |
|---|---|
| Base path | +1 |
| `\|\|` in outer fill-state guard | +1 |
| `if (Filled \|\| PartFilled)` | +1 |
| `if (StartsWith("Stop_"))` | +1 |
| `else if (StartsWith("T"))` | +1 |
| `&&` compound in `else if` | +1 |
| **Total** | **6** |

- **Projected CYC: 6**

---

## max_cyc_projected: 6
## extraction_count: 2

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved (max=6) | **YES** |
| Single-responsibility per helper | **YES** — `HandleMasterStopFill` owns stop-fill side effects only; `HandleMasterTargetFill` owns direction-aware delta logic only |
| Lock-free/Actor pattern preserved | **YES** — `Enqueue` pattern unchanged; `TryRemove` on broker thread unchanged (ConcurrentDictionary) |
| Illegal states unrepresentable | **YES** — fill-state guard (`Filled\|\|PartFilled`) remains in parent; helpers are only reachable in valid fill states |
| Zero-allocation hot paths | **YES** — `HandleMasterStopFill` introduces no allocations; `HandleMasterTargetFill` reuses the pre-existing lambda capture pattern |
| Extract Guard Clauses applied | **YES** — outer guard retained in parent; helpers are free of redundant guards |
| Named helpers each CYC<=8 | **YES** — CYC 1 and CYC 5, both <= 8 |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T04:00:00Z |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Wave** | 7 |
| **Phase** | 2 |
| **Method** | ProcessAccountOrder_UpdateMasterExpected |
| **Output** | docs/brain/EPIC-W7-072/02-architecture-plan.md |
