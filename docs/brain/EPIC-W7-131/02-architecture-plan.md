# EPIC-W7-131 — Phase 2: Architecture Plan

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-131 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Generated** | 2026-06-29 |
| **Input** | docs/brain/EPIC-W7-131/01-scope-boundary.md |
| **Output** | docs/brain/EPIC-W7-131/02-architecture-plan.md |

---

## MCP Evidence

### jcodemunch get_context_bundle

Tool: `jcodemunch` / `get_context_bundle`
Symbol resolved: `src/V12_002.Symmetry.Replace.cs::V12_002.SymmetryGuardPruneDispatches#method`
Source lines: 265–302
Signature: `private void SymmetryGuardPruneDispatches()`
Callees detected: `symmetryDispatchById` (src/V12_002.Symmetry.cs:118), `activePositions` (src/V12_002.cs:199)

Full source retrieved via get_context_bundle confirms:
- Method body is 38 lines
- Outer `foreach` iterates `.ToArray()` snapshot of `symmetryDispatchById` (ADR-019 lock-free contract)
- Inner `foreach` iterates `ctx.Followers` immutable string[] snapshot
- `symmetryDispatchById.TryRemove` is the only side-effect (lock-free ConcurrentDictionary op)

### jcodemunch get_dependency_graph

Tool: `jcodemunch` / `get_dependency_graph`
File: `src/V12_002.Symmetry.Replace.cs`
Direction: both (imports + importers)
Result: 1 node, 0 import/export edges in graph (partial class — intra-assembly deps not expressed as file imports)

This confirms the extraction is fully self-contained within the partial class. All new helper methods live in `src/V12_002.Symmetry.Replace.cs` with no new cross-file dependencies.

### sequential sequentialthinking

Tool: `sequential` / `sequentialthinking`
Thoughts executed: 3

- Thought 1: Mapped all CYC branches — confirmed raw local CYC=9, identified 4 primary decision points
- Thought 2: Designed extraction strategy — 3 helpers each <= CYC 8, lock-free contract preserved
- Thought 3: CYC validation — confirmed max_cyc_projected=4 (all helpers and parent <= 8)

---

## Target Method Analysis

### Source (retrieved via get_context_bundle)

```csharp
private void SymmetryGuardPruneDispatches()
{
    DateTime nowUtc = DateTime.UtcNow;

    foreach (var kvp in symmetryDispatchById.ToArray())   // +1 loop
    {
        SymmetryDispatchContext ctx = kvp.Value;
        if (ctx == null) continue;                         // +1 null-guard

        bool remove = false;

        if (nowUtc - ctx.CreatedUtc > SymmetryDispatchTtl) // +1 TTL check
        {
            remove = true;
        }
        else if (ctx.Anchor.IsResolved)                    // +1 anchor-resolved
        {
            bool hasActiveFollowers = false;
            string[] pruneSnapshot = ctx.Followers;
            foreach (string follower in pruneSnapshot)     // +1 inner loop
            {
                if (activePositions.ContainsKey(follower)) // +1 ContainsKey
                {
                    hasActiveFollowers = true;
                    break;
                }
            }
            if (!hasActiveFollowers) remove = true;        // +1 negate flag
        }

        if (remove)                                        // +1 remove check
            symmetryDispatchById.TryRemove(kvp.Key, out _);
    }
}
```

**Local CYC breakdown:** 1 (base) + 1 (foreach outer) + 1 (null guard) + 1 (if TTL) + 1 (else if anchor) + 1 (foreach inner) + 1 (if ContainsKey) + 1 (if !hasActive) + 1 (if remove) = **9**
**Blast-radius-weighted CYC:** 34 (hot path: OnBarUpdate → SymmetryGuardProcessPendingFollowerFills → target ×2)

---

## Complexity Drivers

| # | Driver | Lines | Impact |
|---|---|---|---|
| 1 | Dual-predicate `remove` flag collapsing TTL + anchor/followers into one branch | 277–294 | Forces reader to track two orthogonal policies simultaneously |
| 2 | Nested inner `foreach` + `ContainsKey` + `break` inside outer loop body | 285–292 | No abstraction boundary; untestable in isolation |
| 3 | Inline eviction policy without named predicates | 277, 281 | Any policy change requires modifying the loop body directly |

---

## Extraction Plan

| # | Helper Name | Signature | Responsibility | Estimated CYC |
|---|---|---|---|---|
| 1 | `HasActiveFollowers` | `private bool HasActiveFollowers(SymmetryDispatchContext ctx)` | Pure read: iterate `ctx.Followers` snapshot, check `activePositions.ContainsKey` per follower | **3** |
| 2 | `ShouldPruneDispatch` | `private bool ShouldPruneDispatch(SymmetryDispatchContext ctx, DateTime nowUtc)` | Boolean eviction policy: TTL check OR (anchor resolved AND no active followers) | **4** |
| 3 | `TryPruneDispatchEntry` | `private void TryPruneDispatchEntry(string key, SymmetryDispatchContext ctx, DateTime nowUtc)` | Per-entry action: null-guard + call ShouldPruneDispatch + call TryRemove | **3** |
| — | `SymmetryGuardPruneDispatches` (parent, post-extraction) | `private void SymmetryGuardPruneDispatches()` | Coordinator: snapshot + iterate + delegate to TryPruneDispatchEntry | **2** |

**max_cyc_projected: 4** (ShouldPruneDispatch is the most complex helper — all <= 8)

---

## Post-Extraction Method Shapes

### Parent (coordinator — CYC 2)

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private void SymmetryGuardPruneDispatches()
{
    DateTime nowUtc = DateTime.UtcNow;
    foreach (var kvp in symmetryDispatchById.ToArray())
        TryPruneDispatchEntry(kvp.Key, kvp.Value, nowUtc);
}
```

### Helper 1: HasActiveFollowers (CYC 3)

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool HasActiveFollowers(SymmetryDispatchContext ctx)
{
    string[] pruneSnapshot = ctx.Followers;
    foreach (string follower in pruneSnapshot)
    {
        if (activePositions.ContainsKey(follower))
            return true;
    }
    return false;
}
```

### Helper 2: ShouldPruneDispatch (CYC 4)

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool ShouldPruneDispatch(SymmetryDispatchContext ctx, DateTime nowUtc)
{
    if (nowUtc - ctx.CreatedUtc > SymmetryDispatchTtl)
        return true;
    if (ctx.Anchor.IsResolved && !HasActiveFollowers(ctx))
        return true;
    return false;
}
```

### Helper 3: TryPruneDispatchEntry (CYC 3)

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private void TryPruneDispatchEntry(string key, SymmetryDispatchContext ctx, DateTime nowUtc)
{
    if (ctx == null)
        return;
    if (ShouldPruneDispatch(ctx, nowUtc))
        symmetryDispatchById.TryRemove(key, out _);
}
```

---

## CYC Validation Table

| Symbol | Decision Points | CYC | <= 8? |
|---|---|---|---|
| `HasActiveFollowers` | base(1) + foreach(1) + if-ContainsKey(1) | **3** | YES |
| `ShouldPruneDispatch` | base(1) + if-TTL(1) + if-anchor-and-followers(1) + implicit short-circuit(1) | **4** | YES |
| `TryPruneDispatchEntry` | base(1) + if-null(1) + if-should-prune(1) | **3** | YES |
| `SymmetryGuardPruneDispatches` | base(1) + foreach-outer(1) | **2** | YES |

**All helpers and parent CYC <= 8. max_cyc_projected = 4.**

---

## Jane Street Compliance Notes

| Mandate | Applied? | Notes |
|---|---|---|
| No LINQ | YES | No `.Where`, `.Any`, `.First`, `.Select` — manual foreach only |
| No new `lock()` blocks | YES | All operations use lock-free primitives: `ConcurrentDictionary.ToArray()`, `ContainsKey`, `TryRemove`; `ctx.Followers` is immutable string[] snapshot (ADR-019) |
| `AggressiveInlining` on hot helpers | YES | All 3 helpers + parent receive `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — all are hot-path (called every bar update) |
| `NoInlining` on cold/logging paths | N/A | No cold logging paths in these helpers |
| Structs with `ref`/`in`/`out` | N/A | SymmetryDispatchContext is a class (reference type); `out _` already used on TryRemove |
| Zero-alloc hot path | YES | Parent retains `.ToArray()` snapshot (required by ADR-019 lock-free contract); no new allocations in helpers |
| CYC <= 8 per helper | YES | All 4 symbols at CYC 2–4 |
| Single responsibility per helper | YES | HasActiveFollowers = query only; ShouldPruneDispatch = policy predicate; TryPruneDispatchEntry = action executor |
| Defense in depth | YES | Null-guard preserved in TryPruneDispatchEntry (same semantics as original `continue`) |

---

## Lock-Free Contract Preservation (ADR-019)

The original method's concurrency safety relies on three invariants:

1. **`symmetryDispatchById.ToArray()`** — snapshot before iteration prevents mutation-during-enumeration. **Preserved:** snapshot call stays in parent coordinator.
2. **`ConcurrentDictionary.ContainsKey`** — thread-safe read without lock. **Preserved:** called identically in `HasActiveFollowers`.
3. **`ConcurrentDictionary.TryRemove`** — atomic remove without lock. **Preserved:** called identically in `TryPruneDispatchEntry`.
4. **`ctx.Followers` immutable string[]** — lock-free inner loop. **Preserved:** `HasActiveFollowers` receives ctx by reference, reads immutable array.

No new synchronization primitives introduced. No behavioral change.

---

## Scope Boundary Confirmation

Per `01-scope-boundary.md` (boundary_verdict: PASS):
- Target file: `src/V12_002.Symmetry.Replace.cs` only
- New helpers: Private methods in same partial class (`V12_002`, partial `Strategy`)
- No caller modifications (3 callers: SymmetryGuardProcessPendingFollowerFills ×2 + OnBarUpdate indirect)
- No cross-file changes
- V12.23 No Scope Creep: ONE EPIC = ONE CONCERN ✅

---

## Phase 3 Handoff Summary

- **Target file:** `src/V12_002.Symmetry.Replace.cs`
- **Method to refactor:** `SymmetryGuardPruneDispatches` (lines 265–302)
- **New methods to add:** `HasActiveFollowers`, `ShouldPruneDispatch`, `TryPruneDispatchEntry`
- **CYC before:** 34 (blast-radius-weighted), 9 (local)
- **CYC after:** max_cyc_projected = 4 (all helpers), 2 (parent)
- **Build impact:** Zero — no signature changes, no interface changes, no caller changes
