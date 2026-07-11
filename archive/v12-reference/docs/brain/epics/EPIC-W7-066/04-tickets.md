# Phase 4: Ticket Generation — EPIC-W7-066

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-066 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Method** | `RemoveFsmOrderIdMappings` |
| **Source File** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **Original CYC** | 10 |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 3 |
| **max_cyc_projected** | 3 |

---

## Sequential Thinking Validation

### Thought 1 — Ticket Count Decision
The architecture plan (Phase 2) identifies exactly 2 private helper methods to extract from `RemoveFsmOrderIdMappings` (CYC=10). V12.23 No-Scope-Creep protocol mandates ONE EPIC = ONE CONCERN; each helper extraction is a distinct atomic concern:
- **Ticket 1**: Extract `RemoveOrderIdIfPresent(Order order)` — collapses the repeated null+`IsNullOrEmpty` guard pattern for individual `Order` fields.
- **Ticket 2**: Extract `RemoveTargetOrderIds(IEnumerable<Order> targets)` — extracts the `foreach` loop body per Jane Street "Extract Loop Body" rule.
- **Result**: `ticket_count = 2`

### Thought 2 — Per-Ticket Detail: Lines, Helper Name, Projected CYC
- **Ticket 1** moves the 2-branch guard body (`order != null && !IsNullOrEmpty(order.OrderId)` + `TryRemove`) into a dedicated helper. Projected CYC = 3.
- **Ticket 2** moves the null guard on `fsm.Targets` + `foreach` iteration into a dedicated helper that delegates per-element to Ticket 1's helper. Projected CYC = 3.
- Both helpers are `private void`, same file, zero blast radius outside `src/V12_002.Symmetry.BracketFSM.cs`.

### Thought 3 — CYC Verification: all methods <= 8 post-extraction
| Method | Projected CYC | <= 8? |
|---|---|---|
| `RemoveOrderIdIfPresent(Order order)` | 3 | YES |
| `RemoveTargetOrderIds(IEnumerable<Order> targets)` | 3 | YES |
| `RemoveFsmOrderIdMappings` (parent after all extractions) | 3 | YES |

**max_cyc_projected = 3. Jane Street CYC <= 8 threshold: CONFIRMED for all methods.**

---

## Tickets

---

### Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-066-T1 |
| **helper_name** | `RemoveOrderIdIfPresent` |
| **helper_signature** | `private void RemoveOrderIdIfPresent(Order order)` |
| **concern** | Eliminate the repeated 2-branch null+`IsNullOrEmpty` guard pattern that appears for `EntryOrder`, `StopOrder`, and each `Targets` element. Centralise the guard so it is structurally impossible for a null or empty-string `OrderId` to reach `_orderIdToFsmKey.TryRemove`. |
| **lines_to_move** | Original lines 17–18 (EntryOrder block), 21–22 (StopOrder block), 26–27 (Targets element block) — guard+TryRemove pattern duplicated 3×; extracted once into helper |
| **call_sites_replaced** | `EntryOrder` block at original line 17, `StopOrder` block at original line 21, `Targets` inner body at original line 26 — all replaced with `RemoveOrderIdIfPresent(<field>)` |
| **cyc_reduction** | 4 (removes 2 branches per EntryOrder guard + 2 branches per StopOrder guard from parent; Targets element guard moves to Ticket 2) |
| **projected_helper_cyc** | **3** |
| **projected_parent_cyc_after_t1** | 6 (10 - 4) |

#### Projected Helper Body (Ticket 1)

```csharp
private void RemoveOrderIdIfPresent(Order order)
{
    if (order != null && !string.IsNullOrEmpty(order.OrderId))
        _orderIdToFsmKey.TryRemove(order.OrderId, out _);
}
```

#### Jane Street Alignment

| Rule | Status |
|---|---|
| Single-responsibility | YES — one concern: guard a single Order field and remove its id |
| Lock-free | YES — `ConcurrentDictionary.TryRemove` only; no `lock()` block |
| Illegal states unrepresentable | YES — structurally prevents null/empty OrderId reaching TryRemove |
| Zero-allocation | YES — stack-frame only, no heap allocations |
| Guard clause (early return pattern) | YES — compound `&&` guard |
| ASCII-only | YES |
| xUnit tests at Phase 5 | YES — `[Fact]` with `Assert.Equal` / `Assert.False` |

---

### Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-066-T2 |
| **helper_name** | `RemoveTargetOrderIds` |
| **helper_signature** | `private void RemoveTargetOrderIds(IEnumerable<Order> targets)` |
| **concern** | Extract the `foreach` loop body per Jane Street "Extract Loop Body" rule. Encapsulate the null guard on `fsm.Targets` and the per-element iteration, delegating each element to `RemoveOrderIdIfPresent`. |
| **lines_to_move** | Original lines 23–28 (null guard `if (fsm.Targets == null) return` + `foreach` loop + inner null+empty guard body) |
| **call_sites_replaced** | Original lines 23–28 in parent replaced with single call `RemoveTargetOrderIds(fsm.Targets)` |
| **cyc_reduction** | 3 (removes null-targets branch + foreach iteration branch + inner element guard branch from parent count) |
| **projected_helper_cyc** | **3** |
| **depends_on** | EPIC-W7-066-T1 (calls `RemoveOrderIdIfPresent` internally) |
| **projected_parent_cyc_after_t2** | **3** (6 - 3) |

#### Projected Helper Body (Ticket 2)

```csharp
private void RemoveTargetOrderIds(IEnumerable<Order> targets)
{
    if (targets == null)
        return;
    foreach (Order target in targets)
        RemoveOrderIdIfPresent(target);
}
```

#### Jane Street Alignment

| Rule | Status |
|---|---|
| Single-responsibility | YES — one concern: iterate targets collection and delegate per-element removal |
| Extract Loop Body | YES — `foreach` body extracted per Jane Street rule |
| Lock-free | YES — delegates to `RemoveOrderIdIfPresent` which uses `ConcurrentDictionary.TryRemove` |
| Illegal states unrepresentable | YES — null guard on `targets` prevents null enumeration |
| Zero-allocation | YES — `foreach` (no LINQ), stack-frame only |
| Guard clause (early return) | YES — `if (targets == null) return` |
| ASCII-only | YES |
| xUnit tests at Phase 5 | YES — `[Fact]` with null-targets case, empty-targets case, populated-targets case |

---

## Parent Method After All Extractions

```csharp
private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)
{
    if (fsm == null)
        return;
    RemoveOrderIdIfPresent(fsm.EntryOrder);
    if (!string.IsNullOrEmpty(fsm.ReplacingCancelOrderId))
        _orderIdToFsmKey.TryRemove(fsm.ReplacingCancelOrderId, out _);
    RemoveOrderIdIfPresent(fsm.StopOrder);
    RemoveTargetOrderIds(fsm.Targets);
}
```

| Field | Value |
|---|---|
| **projected_parent_cyc_after_all** | **3** |
| **CYC reduction** | 10 → 3 (70% reduction, -7 CYC points) |
| **Remaining branches** | 1 null-fsm early return + 1 `ReplacingCancelOrderId` empty-string guard (bare string field, cannot use `RemoveOrderIdIfPresent` which requires an `Order`) |

---

## Execution Order

1. **T1 first**: Add `RemoveOrderIdIfPresent` helper; replace EntryOrder and StopOrder inline guards with `RemoveOrderIdIfPresent(...)` calls.
2. **T2 second**: Add `RemoveTargetOrderIds` helper (references T1 helper); replace Targets null-guard+foreach block with `RemoveTargetOrderIds(fsm.Targets)` call.

Both tickets target `src/V12_002.Symmetry.BracketFSM.cs` only. No other files modified.

---

## jcodemunch Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | `found=true, indexed=true, symbol_count=5147` |
| `get_symbol_complexity(RemoveFsmOrderIdMappings)` | Not found in index (private method, partial class — expected); complexity confirmed from Phase 2 source analysis as CYC=10 |
| `get_extraction_candidates(src/V12_002.Symmetry.BracketFSM.cs)` | `candidates=[]` (method is private, no cross-file callers meeting min_callers=2 threshold — expected for intra-file private method) |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-066 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:25:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 validation thoughts) |
| **Input** | `docs/brain/EPIC-W7-066/02-architecture-plan.md`, `docs/brain/EPIC-W7-066/03-audit-report.md` |
| **Output** | `docs/brain/EPIC-W7-066/04-tickets.md` |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 3 |
| **max_cyc_projected** | 3 |
| **CYC Before** | 10 |
| **CYC After** | 3 |
