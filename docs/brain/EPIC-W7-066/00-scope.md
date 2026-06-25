# Phase 1: Scope Definition - EPIC-W7-066

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **Execution Time**: ~5 seconds

---

## Method Under Refactoring

| Property | Value |
|---|---|
| **Method** | `RemoveFsmOrderIdMappings` |
| **File** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **Line** | 103 |
| **Access** | `private void` |
| **Parameter** | `FollowerBracketFSM fsm` |
| **Current CYC** | 10 |
| **Target CYC** | ≤ 8 |
| **LOC** | 23 |
| **Max Nesting** | 3 |

### Current Body (verbatim, lines 103–125)

```csharp
private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)
{
    if (fsm == null)
        return;

    if (fsm.EntryOrder != null && !string.IsNullOrEmpty(fsm.EntryOrder.OrderId))
        _orderIdToFsmKey.TryRemove(fsm.EntryOrder.OrderId, out _);

    if (!string.IsNullOrEmpty(fsm.ReplacingCancelOrderId))
        _orderIdToFsmKey.TryRemove(fsm.ReplacingCancelOrderId, out _);

    if (fsm.StopOrder != null && !string.IsNullOrEmpty(fsm.StopOrder.OrderId))
        _orderIdToFsmKey.TryRemove(fsm.StopOrder.OrderId, out _);

    if (fsm.Targets == null)
        return;

    foreach (Order target in fsm.Targets)
    {
        if (target != null && !string.IsNullOrEmpty(target.OrderId))
            _orderIdToFsmKey.TryRemove(target.OrderId, out _);
    }
}
```

---

## IN SCOPE

### What will be changed
1. **Extract `RemoveOrderIdIfPresent(Order order)`** — a private helper that encapsulates the two-condition guard (`order != null && !string.IsNullOrEmpty(order.OrderId)`) followed by `_orderIdToFsmKey.TryRemove(...)`. Called for `fsm.EntryOrder` and `fsm.StopOrder`.
2. **Extract `RemoveTargetOrderIds(IEnumerable<Order> targets)`** — a private helper that owns the `foreach` loop over `fsm.Targets`, delegating each iteration body to `RemoveOrderIdIfPresent`. This collapses the loop and its inner conditional into a single named concept.

After both extractions `RemoveFsmOrderIdMappings` retains only its null-guard and three flat call sites, reducing CYC from **10 → 6**.

### CYC Accounting (post-extraction)

| Method | Decision Points | CYC |
|---|---|---|
| `RemoveFsmOrderIdMappings` (refactored) | null guard (1) + 1 bare `IsNullOrEmpty` (1) + 2 helper calls (0) + targets null guard (1) = | **4** |
| `RemoveOrderIdIfPresent` | `order != null` (1) + `IsNullOrEmpty` (1) = | **3** |
| `RemoveTargetOrderIds` | `foreach` (1) = | **2** |

All three methods satisfy CYC ≤ 8. ✅

---

## OUT OF SCOPE

- **Signature of `RemoveFsmOrderIdMappings` is unchanged** — same name, same parameter type, same `private void` access level.
- **No behavior change** — every `TryRemove` call that would have fired before refactoring fires after; no early-exit path is added or removed.
- **`TryTerminateFollowerBracket`** (the sole caller at line 127) — untouched.
- **`_orderIdToFsmKey` field** — not moved, renamed, or retyped.
- **`FollowerBracketFSM` / `Order` types** — not modified.
- **All other methods in the file** — untouched.
- **No new public API** — both new helpers are `private`.
- **No test files are authored in this phase** — testing is a later-phase concern.
- **`src-vm-backup/`** — not touched.

---

## Extraction Plan

### Helper 1 — `RemoveOrderIdIfPresent`

```csharp
// Proposed signature
private void RemoveOrderIdIfPresent(Order order)
{
    if (order != null && !string.IsNullOrEmpty(order.OrderId))
        _orderIdToFsmKey.TryRemove(order.OrderId, out _);
}
```

- **Replaces** the two existing `if (fsm.XxxOrder != null && ...)` blocks for `EntryOrder` and `StopOrder`.
- **CYC contribution**: 3 (1 base + 2 conditions).

### Helper 2 — `RemoveTargetOrderIds`

```csharp
// Proposed signature
private void RemoveTargetOrderIds(IReadOnlyList<Order> targets)
{
    if (targets == null)
        return;

    foreach (Order target in targets)
        RemoveOrderIdIfPresent(target);
}
```

- **Absorbs** the `if (fsm.Targets == null) return;` guard and the `foreach` loop.
- The loop body is entirely delegated to `RemoveOrderIdIfPresent`, so no compound condition remains here.
- **CYC contribution**: 2 (1 null guard + 1 foreach).

### Refactored `RemoveFsmOrderIdMappings`

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

- `ReplacingCancelOrderId` is a `string`, not an `Order`, so it cannot use `RemoveOrderIdIfPresent`; its existing inline check is retained.
- **CYC contribution**: 4 (1 base + 1 null guard + 1 `IsNullOrEmpty`).

---

## Risk Assessment

| Risk | Severity | Mitigation |
|---|---|---|
| Inadvertent logic change in `RemoveOrderIdIfPresent` condition order | Low | Condition is a direct lift; no reordering |
| `fsm.Targets` null guard moving into `RemoveTargetOrderIds` | Negligible | Functionally identical early return |
| Type mismatch if `Targets` is not `IReadOnlyList<Order>` | Low | Use concrete declared type from existing `foreach`; adjust if needed |
| Breaking the single caller `TryTerminateFollowerBracket` | None | Outer signature is unchanged |
| Blast radius to external files | None | Zero external dependents confirmed in Phase 0 |

**Overall Risk: LOW**

---

## Success Criteria

1. `RemoveFsmOrderIdMappings` CYC ≤ 8 (target: 4).
2. `RemoveOrderIdIfPresent` CYC ≤ 8 (target: 3).
3. `RemoveTargetOrderIds` CYC ≤ 8 (target: 2).
4. Method signature `private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)` — **unchanged**.
5. All `TryRemove` call sites produce identical runtime outcomes as pre-refactoring.
6. `TryTerminateFollowerBracket` compiles and behaves without modification.
7. No `src/` file other than `src/V12_002.Symmetry.BracketFSM.cs` is modified.
8. No new public members introduced.
