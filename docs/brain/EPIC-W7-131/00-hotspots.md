# EPIC-W7-131 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field | Value |
|---|---|
| **Method** | `SymmetryGuardPruneDispatches` |
| **CYC Score** | 34 (cumulative blast-radius–weighted, jCodeMunch) |
| **Local CYC** | 9 (raw McCabe, method body only) |
| **File** | `src/V12_002.Symmetry.Replace.cs` |
| **Lines** | 265–302 |
| **Class** | `V12_002` (partial, `Strategy`) |
| **Namespace** | `NinjaTrader.NinjaScript.Strategies` |

---

## Blast Radius Summary

`SymmetryGuardPruneDispatches` is called from the hot bar-update path via the **two-level call chain**:

```
OnBarUpdate (BarUpdate.cs:322)
  └─ SymmetryGuardProcessPendingFollowerFills (Symmetry.Follower.cs:97)
       ├─ SymmetryGuardPruneDispatches()  [line 101 — early-exit / empty branch]
       └─ SymmetryGuardPruneDispatches()  [line 126 — normal exit]
```

**Files directly coupled to this method:**

| File | Coupling Type |
|---|---|
| `src/V12_002.BarUpdate.cs` | Indirect caller (every price tick) |
| `src/V12_002.Symmetry.Follower.cs` | Direct caller (×2 call sites) |
| `src/V12_002.Symmetry.cs` | Owner of `symmetryDispatchById`, `SymmetryDispatchTtl`, `SymmetryDispatchContext`, `AnchorSnapshot` |
| `src/V12_002.Orders.Callbacks.AccountOrders.cs` | Writes to `symmetryDispatchById` (fill path) |
| `src/V12_002.Orders.Callbacks.Propagation.cs` | Reads/writes `symmetryDispatchById` (target-replace FSM) |
| `src/V12_002.SIMA.Shadow.cs` | Reads `symmetryDispatchById`/`symmetryFleetEntryToDispatch` |

**Blast radius level: HIGH** — the method is invoked on every bar update and touches the shared `symmetryDispatchById` `ConcurrentDictionary` that is also mutated by order-callback threads. Any extraction must preserve the lock-free iteration contract (`.ToArray()` snapshot) and the TTL/anchor-resolution pruning semantics.

---

## Top 3 Complexity Drivers

### Driver 1 — Dual-predicate removal decision (`if / else if` + inner loop)

```
Lines 277–297 in src/V12_002.Symmetry.Replace.cs
```

The method collapses two **orthogonal removal conditions** (TTL expiry vs. anchor-resolved-with-no-active-followers) into a single `remove` flag via an `if / else if` branch, where the second branch contains a nested `foreach` + inner `if`. These are logically independent eviction policies but share the mutable `remove` flag — making the control flow hard to test in isolation.

- **Condition A:** `nowUtc - ctx.CreatedUtc > SymmetryDispatchTtl` → TTL eviction
- **Condition B:** `ctx.Anchor.IsResolved && !HasActiveFollowers(ctx)` → resolved-with-empty-followers eviction
- Inner loop: `foreach follower in pruneSnapshot` + `if (activePositions.ContainsKey(follower))` + early `break`

**Extraction target:** Extract `HasActiveFollowers(SymmetryDispatchContext)` (pure query, no side-effects) and `ShouldPruneDispatch(SymmetryDispatchContext, DateTime)` (boolean policy predicate).

---

### Driver 2 — Outer `foreach` over live concurrent dictionary snapshot

```
Line 269: foreach (var kvp in symmetryDispatchById.ToArray())
```

The `.ToArray()` snapshot is correct for lock-free safety, but the entire method is structured as a single linear scan with embedded multi-branch decision logic inside the loop body. Each iteration allocates no temporaries but the **overall loop body CYC contribution is 6** (null-guard + 2 remove conditions + inner loop + hasActiveFollowers flag + final remove-check). Extracting the body into a `TryPruneDispatchEntry(string key, SymmetryDispatchContext ctx, DateTime nowUtc)` helper reduces the outer loop to a single-responsibility iterator.

---

### Driver 3 — Implicit coupling to `SymmetryDispatchTtl` and `AnchorSnapshot.IsResolved` without abstraction

```
Lines 277, 281: direct field dereferences with no intermediate query method
```

The pruning policy (`DispatchTtl`, `IsResolved`, active-follower check) is **inlined** rather than expressed as a named predicate. This creates a hidden coupling: changing the eviction policy (e.g., adding a third condition such as "all targets filled") requires modifying the body of this method rather than adding a new policy clause. The `SymmetryDispatchContext` class (in `Symmetry.cs`) already contains the data model for a cleaner `IsPrunable(DateTime nowUtc)` method.

---

## Recommended Extraction Count

**3 extractions** are recommended:

| # | Extraction | Signature | Scope |
|---|---|---|---|
| 1 | Active-follower query | `private bool HasActiveFollowers(SymmetryDispatchContext ctx)` | Pure read; no side-effects; testable |
| 2 | Prune-policy predicate | `private bool ShouldPruneDispatch(SymmetryDispatchContext ctx, DateTime nowUtc)` | Wraps Driver 1 + Driver 3 logic; delegates to `HasActiveFollowers` |
| 3 | Per-entry prune action | `private void TryPruneDispatchEntry(string key, SymmetryDispatchContext ctx, DateTime nowUtc)` | Wraps Driver 2 loop body; calls `ShouldPruneDispatch`, calls `TryRemove` |

After extraction, `SymmetryGuardPruneDispatches` reduces to a 4-line coordinator:

```csharp
private void SymmetryGuardPruneDispatches()
{
    DateTime nowUtc = DateTime.UtcNow;
    foreach (var kvp in symmetryDispatchById.ToArray())
        TryPruneDispatchEntry(kvp.Key, kvp.Value, nowUtc);
}
```

**Projected post-extraction CYC:** Local = 2; cumulative blast-radius score ≈ 12–15.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Epic** | EPIC-W7-131 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Bobcoins Used** | 14 |
| **Execution Time** | ~95 seconds |
| **MCP Tools Invoked** | `glob` ×3, `read_file` ×6, `grep` ×7, `list_files` ×1, `GetSymbolsOverview` ×1, `FindSymbol` ×1, `write_file` ×1 |
| **Status** | ✅ Completed |
