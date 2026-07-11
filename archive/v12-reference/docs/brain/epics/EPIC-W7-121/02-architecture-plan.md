# EPIC-W7-121 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:00:00Z
**Input:** docs/brain/EPIC-W7-121/01-scope-boundary.md

---

## Summary

Extract `SymmetryGuardCascadeFollowerCleanup` (CYC=10) into **3 private helper methods** to
reduce all cyclomatic complexity values to <= 8 (Jane Street strict standard).

- **Original method CYC:** 10
- **Extraction count:** 3
- **max_cyc_projected:** 7
- **Boundary verdict (from Phase 1.5):** PASS

---

## Original Method Analysis

**File:** `src/V12_002.Symmetry.Replace.cs` — line 198
**Class:** `V12_002` (partial)
**Signature:** `private void SymmetryGuardCascadeFollowerCleanup(string masterEntryName)`

### CYC=10 Branch Inventory

| # | Branch | Type | CYC contribution |
|---|--------|------|-----------------|
| 1 | `if (!symmetryMasterEntryToDispatch.TryGetValue(...))` | early return | +1 |
| 2 | `if (!symmetryDispatchById.TryGetValue(...))` | early return | +1 |
| 3 | `foreach (string followerName in followers)` | loop | +1 |
| 4 | `if (!activePositions.TryGetValue(...))` | continue | +1 |
| 5 | `if (!entryOrders.TryGetValue(...))` | continue | +1 |
| 6 | `if (order == null)` | continue | +1 |
| 7 | `order.OrderState == OrderState.Working \|\| ...Submitted` | compound OR | +1 |
| 8 | `... \|\| ...Accepted` | compound OR continuation | +1 |
| 9 | `pos.ExecutingAccount != null ? ... : "Master"` | ternary | +1 |
| base | — | — | +1 |
| **Total** | | | **10** |

### Logical Sub-Concerns Identified

1. **Dispatch context resolution** (branches 1-2): Two-hop `TryGetValue` lookup chain
   `masterEntryName → dispatchId → SymmetryDispatchContext`
2. **Cascade start log** (cold path): Single `Print` call announcing cascade to N followers
3. **Per-follower cancellation** (branches 4-9 inside foreach): Guard chain + conditional cancel + account log

---

## Extraction Plan

### Helper 1: `TryResolveSymmetryCascadeContext`

```csharp
private bool TryResolveSymmetryCascadeContext(
    string masterEntryName,
    out SymmetryDispatchContext ctx)
```

**Responsibility:** Perform the two-hop dispatch lookup. Returns `false` (and sets `ctx = default`)
on any miss, allowing the parent to early-return with a single guard. Preserves ADR-019 lock-free
contract — no new locking introduced; both `TryGetValue` calls are already lock-free on
`ConcurrentDictionary`.

**Extracted branches:**
- `if (!symmetryMasterEntryToDispatch.TryGetValue(masterEntryName, out dispatchId)) return false;`
- `if (!symmetryDispatchById.TryGetValue(dispatchId, out ctx)) return false;`

**Projected CYC:** 3 (base 1 + 2 if-return branches)  ✓ <= 8

---

### Helper 2: `LogCascadeCancellationStart`

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private void LogCascadeCancellationStart(string masterEntryName, int followerCount)
```

**Responsibility:** Emit the `[CASCADE] Master X cancelled -- terminating N follower(s)` diagnostic
log. Pure side-effect (Print call). No state mutation, no heap allocation beyond the format string.

**Extracted logic:**
```csharp
Print(string.Format(
    "[CASCADE] Master {0} cancelled -- terminating {1} linked follower(s).",
    masterEntryName,
    followerCount));
```

**Annotation:** `[MethodImpl(MethodImplOptions.NoInlining)]` — cold-path logging per carl_cook pattern.
The JIT will never inline this into the hot dispatch loop.

**Projected CYC:** 1 (no branches — pure Print call)  ✓ <= 8

---

### Helper 3: `TryCancelFollowerEntry`

```csharp
private void TryCancelFollowerEntry(string followerName)
```

**Responsibility:** For one named follower: look up its active position and entry order, validate
state eligibility, cancel if the order is in a cancellable state, and log the cancellation.
Single-responsibility: "cancel one follower entry if eligible." Contains all per-follower
complexity that was previously inline inside the foreach body.

**Extracted branches:**
- `if (!activePositions.TryGetValue(followerName, out var pos)) return;`
- `if (!entryOrders.TryGetValue(followerName, out var order)) return;`
- `if (order == null) return;`
- `if (order.OrderState == OrderState.Working || order.OrderState == OrderState.Submitted || order.OrderState == OrderState.Accepted)` (+2 for compound OR)
- `pos.ExecutingAccount != null ? pos.ExecutingAccount.Name : "Master"` (+1 ternary)

**Projected CYC:** 7 (base 1 + 3 guard-returns + 2 OR-branches + 1 ternary)  ✓ <= 8

Note: The A2-3 audit comment ("DeltaExpectedPositionLocked deferred to OnAccountOrderUpdate
confirmed-cancel to prevent REAPER desync") stays as an inline comment inside `TryCancelFollowerEntry`.

---

## Parent Method After Extraction

```csharp
private void SymmetryGuardCascadeFollowerCleanup(string masterEntryName)
{
    if (!TryResolveSymmetryCascadeContext(masterEntryName, out var ctx))
        return;

    string[] followers = ctx.Followers; // ADR-019: immutable snapshot, lock-free

    LogCascadeCancellationStart(masterEntryName, followers.Length);

    foreach (string followerName in followers)
        TryCancelFollowerEntry(followerName);
}
```

**Remaining branches in parent:**
- `if (!TryResolveSymmetryCascadeContext(...))` → +1
- `foreach` → +1

**Projected parent CYC:** 3 (base 1 + 2)  ✓ <= 8

---

## CYC Summary Table

| Method | Projected CYC | Status |
|--------|--------------|--------|
| `SymmetryGuardCascadeFollowerCleanup` (parent, after extraction) | 3 | ✓ PASS |
| `TryResolveSymmetryCascadeContext` | 3 | ✓ PASS |
| `LogCascadeCancellationStart` | 1 | ✓ PASS |
| `TryCancelFollowerEntry` | 7 | ✓ PASS |
| **max_cyc_projected** | **7** | **✓ <= 8** |

---

## Jane Street Alignment Notes

### gjengset: Lock-Free / Left-Right Pattern
- `ctx.Followers` is an immutable `string[]` snapshot per ADR-019. All helpers read it directly
  without any lock or `MemoryBarrier`. Extraction preserves this guarantee — no new synchronization
  primitives are introduced.
- `symmetryMasterEntryToDispatch` and `symmetryDispatchById` are `ConcurrentDictionary` — their
  `TryGetValue` calls are inherently lock-free. `TryResolveSymmetryCascadeContext` consolidates both
  lookups without touching their thread-safety contract.

### carl_cook: Hot Path Zero-Alloc + Cold Logging Out-of-Line
- `LogCascadeCancellationStart` is annotated `[MethodImpl(NoInlining)]` — the JIT will never merge
  the string formatting overhead into the hot dispatch loop.
- `TryCancelFollowerEntry` uses `TryGetValue` with `out var` — pure stack allocation. No heap
  allocations introduced. The ternary for account name references existing heap objects; no new
  allocation.
- The `Print(string.Format(...))` log inside `TryCancelFollowerEntry` is guarded by the
  OrderState condition so it only fires when an order is actually cancelled (cold path).

### trading_billions: Single Responsibility + Defense in Depth
- Each helper has exactly one concern:
  - `TryResolveSymmetryCascadeContext` → resolve dispatch context
  - `LogCascadeCancellationStart` → emit cascade start diagnostic
  - `TryCancelFollowerEntry` → cancel one eligible follower
- The full guard chain (two TryGetValue lookups + null check + OrderState check) is preserved in
  depth across the extracted helpers. No guard is weakened or removed. Defense in depth is intact.

---

## MCP Evidence

### jcodemunch: resolve_repo
- **Repo:** `antigravityos187-sketch/universal-or-strategy`
- **Status:** loadable, indexed
- **Symbol count:** 5147, **File count:** 2000

### jcodemunch: get_context_bundle
- Symbol resolved at `src/V12_002.Symmetry.Replace.cs:198`
- Full source body retrieved (see Original Method Analysis above)
- Confirmed: `private void SymmetryGuardCascadeFollowerCleanup(string masterEntryName)`

### jcodemunch: get_call_hierarchy
- **Callers (direct):** 0 detected by AST (callers are internal invocation sites in the same
  partial class — confirmed present per Phase 1.5 scope boundary which found 2 callers)
- **Callees (depth 2, 18 entries):**
  - `symmetryMasterEntryToDispatch`, `symmetryDispatchById` (constants/fields, `V12_002.Symmetry.cs`)
  - `activePositions`, `entryOrders` (constants/fields, `V12_002.cs`)
  - `CancelOrderSafe` (method, `V12_002.Orders.CancelGateway.cs:18`)
  - `LogBuffer.Format`, `LogBuffer.ValidateThreadAffinity`, `LogBuffer.FormatInternal`
  - `V12_002.IsOrderTerminal` (depth-2, `V12_002.Orders.Management.Flatten.cs:698`)

### jcodemunch: get_dependency_graph
- `src/V12_002.Symmetry.Replace.cs` has **0 import edges** (node_count=1, edge_count=0)
- File is standalone within the import graph — no transitive file-level dependencies to break

### jcodemunch: get_extraction_candidates
- No candidates returned (min_callers=1, min_complexity=5) — the method is private and
  only called within the class. Extraction is driven by CYC reduction mandate, not caller demand.

---

## Sequential Thinking Evidence

### Thought 1 — Current structure analysis
Mapped all 9 branch sources contributing to CYC=10. Identified 3 coherent logical sub-concerns:
(A) two-hop dispatch context resolution, (B) cascade-start log emission, (C) per-follower
cancellation guard loop body. Confirmed sub-concern C carries the highest branch density (5 branches).

### Thought 2 — Extraction design
Named all 3 helpers with method signatures. Projected CYCs: parent→3, TryResolveSymmetryCascadeContext→3,
LogCascadeCancellationStart→1, TryCancelFollowerEntry→7. Verified max projected CYC=7 <= 8.
Applied `[NoInlining]` to cold-path logger per carl_cook pattern.

### Thought 3 — Validation pass
Confirmed all 4 projected CYCs <= 8. Verified Jane Street alignment across all 3 KB sources
(gjengset lock-free preservation, carl_cook cold-path extraction + zero-alloc, trading_billions
single-responsibility + defense-in-depth). Plan finalized.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-121 |
| **Method** | SymmetryGuardCascadeFollowerCleanup |
| **Original CYC** | 10 |
| **max_cyc_projected** | 7 |
| **Extraction count** | 3 |
| **Boundary verdict** | PASS |
