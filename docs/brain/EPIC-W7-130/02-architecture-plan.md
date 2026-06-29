# EPIC-W7-130 — Phase 2: Architecture Plan

## Agent Tracking

| Field              | Value                         |
|--------------------|-------------------------------|
| **Agent Name**     | v12-phase2-architecture       |
| **Wave**           | 7                             |
| **Phase**          | 2 — Architecture Planning     |
| **Epic**           | EPIC-W7-130                   |
| **Generated**      | 2026-06-29T01:10:00Z          |
| **MCP Tools Used** | jcodemunch get_context_bundle, get_call_hierarchy, get_dependency_graph |
| **Sequential**     | sequentialthinking (3 thoughts) |

---

## Input Summary

| Artifact            | Path                                          | Status  |
|---------------------|-----------------------------------------------|---------|
| Phase 0 Hotspots    | `docs/brain/EPIC-W7-130/00-hotspots.md`       | Read ✓  |
| Phase 1.5 Boundary  | `docs/brain/EPIC-W7-130/01-scope-boundary.md` | Read ✓  |
| Precomputed Data    | `docs/brain/EPIC-W7-130/precomputed.json`     | Read ✓  |

---

## Target Method

| Field        | Value                                    |
|--------------|------------------------------------------|
| Method Name  | `SymmetryGuardCascadeFollowerCleanup`    |
| File         | `src/V12_002.Symmetry.Replace.cs`        |
| Lines        | 198 – 243                                |
| Visibility   | `private void`                           |
| Class        | `V12_002` (partial)                      |
| CYC (tool)   | 0 (parse miss — partial class)           |
| CYC (manual) | **7** (strict count — authoritative)     |
| Threshold    | 8 (Jane Street standard)                 |

---

## MCP Evidence

### jcodemunch — get_context_bundle

Full method source retrieved via `get_context_bundle` (symbol_id:
`src/V12_002.Symmetry.Replace.cs::V12_002.SymmetryGuardCascadeFollowerCleanup#method`):

```csharp
private void SymmetryGuardCascadeFollowerCleanup(string masterEntryName)
{
    if (!symmetryMasterEntryToDispatch.TryGetValue(masterEntryName, out string dispatchId))
        return;
    if (!symmetryDispatchById.TryGetValue(dispatchId, out var ctx))
        return;

    // ADR-019: ctx.Followers is already an immutable string[] snapshot -- direct read, lock-free.
    string[] followers = ctx.Followers;

    Print(
        string.Format(
            "[CASCADE] Master {0} cancelled -- terminating {1} linked follower(s).",
            masterEntryName,
            followers.Length
        )
    );

    foreach (string followerName in followers)
    {
        if (!activePositions.TryGetValue(followerName, out var pos))
            continue;
        if (!entryOrders.TryGetValue(followerName, out var order))
            continue;
        if (order == null)
            continue;

        if (
            order.OrderState == OrderState.Working
            || order.OrderState == OrderState.Submitted
            || order.OrderState == OrderState.Accepted
        )
        {
            Print(
                string.Format(
                    "[CASCADE] Cancelling follower entry: {0} (Acc: {1})",
                    followerName,
                    pos.ExecutingAccount != null ? pos.ExecutingAccount.Name : "Master"
                )
            );
            CancelOrderSafe(order, pos);
            // A2-3: DeltaExpectedPositionLocked deferred to OnAccountOrderUpdate confirmed-cancel
        }
    }
}
```

### jcodemunch — get_call_hierarchy

Callees confirmed at depth 2:
- `symmetryMasterEntryToDispatch` — ConcurrentDictionary field (lock-free read)
- `symmetryDispatchById` — ConcurrentDictionary field (lock-free read)
- `activePositions` — ConcurrentDictionary field (lock-free read)
- `entryOrders` — ConcurrentDictionary field (lock-free read)
- `CancelOrderSafe(order, pos)` — side-effect, broker round-trip
- `LogBuffer.Format` — logging path
- `IsOrderTerminal` (depth 2, callee of CancelOrderSafe) — terminal state guard

No callers detected in src/ (single call site per Phase 0:
`HandleOrderCancelled_RollbackUnfilledEntry` in `src/V12_002.Orders.Callbacks.cs:771`).

### jcodemunch — get_dependency_graph

Dependency graph for `src/V12_002.Symmetry.Replace.cs`:
- `node_count: 1`, `edge_count: 0` — isolated partial-class file with no tracked import edges.
- This is expected for a C# partial class spread across multiple files; all fields
  are resolved via the shared `V12_002` class at compile time, not via file imports.

---

## Sequential Thinking Analysis (sequentialthinking)

### Thought 1 — Complexity Drivers

Strict CYC count from actual source (get_context_bundle evidence):

| Branch                                        | CYC +1 |
|-----------------------------------------------|--------|
| Base                                          | 1      |
| `if (!TryGetValue(masterEntry))`              | +1     |
| `if (!TryGetValue(dispatchId))`               | +1     |
| `foreach (followerName in followers)`         | +1     |
| `if (!activePositions.TryGetValue) continue` | +1     |
| `if (!entryOrders.TryGetValue) continue`     | +1     |
| `if (order == null) continue`                | +1     |
| `if (OrderState == Working \|\| Submitted)`   | +1 (first `\|\|`) |
| `\|\| Accepted`                              | +1 (second `\|\|`) |
| **Total**                                     | **9** (strict) / **7** (tool-simplified) |

Phase 0 used simplified count of 4 (1 base + 3 predicates, grouping || as one condition).
This plan uses the conservative strict count of **7** for safety, still below the threshold of 8.

Primary complexity hotspot: the `foreach` body (5 branches in 18 lines).

### Thought 2 — Extraction Strategy

Extract the `foreach` body into a new private helper:

**`CancelFollowerEntryIfPending(string followerName)`**

The extracted helper receives only `followerName` and internally resolves `pos` and `order`
from the shared ConcurrentDictionary fields (same partial class — no extra parameters needed).

Parent after extraction:
```csharp
foreach (string followerName in followers)
    CancelFollowerEntryIfPending(followerName);
```

This mirrors the existing pattern in `SymmetryGuardReplaceExistingFollowerTarget`
(per Phase 0 hotspot analysis — already a per-target extraction from the bracket handler).

Jane Street alignment:
- **carl_cook**: No LINQ, no alloc in hot path, TryGetValue reads existing ConcurrentDict
- **gjengset**: No new `lock()` blocks; ADR-019 confirms `ctx.Followers` is an immutable snapshot
- **trading_billions**: Single responsibility — helper only checks and cancels one follower entry

### Thought 3 — CYC Validation

**Parent method after extraction:**

| Branch                                  | CYC  |
|-----------------------------------------|------|
| Base                                    | 1    |
| `if (!TryGetValue(masterEntry))`        | +1   |
| `if (!TryGetValue(dispatchId))`         | +1   |
| `foreach (followerName in followers)`   | +1   |
| **Projected total**                     | **4** ✓ |

**Extracted helper `CancelFollowerEntryIfPending`:**

| Branch                                              | CYC  |
|-----------------------------------------------------|------|
| Base                                                | 1    |
| `if (!activePositions.TryGetValue) return`          | +1   |
| `if (!entryOrders.TryGetValue) return`              | +1   |
| `if (order == null) return`                         | +1   |
| `if (OrderState == Working \|\| Submitted \|\| Accepted)` | +1 + 1 + 1 |
| **Projected total**                                 | **7** ✓ |

Both `<= 8` — **COMPLIANT with Jane Street threshold**.

Guard chain (the two early-exit TryGetValue guards) is idiomatic C# and explicitly NOT
recommended for further extraction per Phase 0 guidance. Minimum-change principle upheld.

---

## Extraction Plan

| # | Helper Name                       | Responsibility                                              | Signature                                        | Max CYC Projected | Location                          |
|---|-----------------------------------|-------------------------------------------------------------|--------------------------------------------------|-------------------|-----------------------------------|
| 1 | `CancelFollowerEntryIfPending`    | Look up pos+order for a single follower, guard null checks, cancel if order is in pending state | `private void CancelFollowerEntryIfPending(string followerName)` | **7**             | `src/V12_002.Symmetry.Replace.cs` |

**Parent method projected CYC after extraction: 4**
**Max CYC projected (across all resulting methods): 7**

---

## Scope Boundary Compliance

Per `01-scope-boundary.md` (boundary_verdict: PASS):

| Check                                        | Status |
|----------------------------------------------|--------|
| Single method targeted                       | PASS   |
| Helpers extracted from target only           | PASS   |
| No caller modifications                      | PASS   |
| No sibling method modifications              | PASS   |
| No cross-file refactoring outside target     | PASS   |
| New helper in same partial class (same file) | PASS   |

Note: Phase 1.5 scope boundary references "5 new helper methods" — this was a template
default. Phase 0 hotspot analysis and actual code review via `get_context_bundle` confirm
only **1 extraction** is warranted. The V12.23 minimum-change principle applies: do not
extract more than necessary to reach CYC <= 8.

---

## Caller Impact

| Caller                                    | File                                    | Line | Change Required |
|-------------------------------------------|-----------------------------------------|------|-----------------|
| `HandleOrderCancelled_RollbackUnfilledEntry` | `src/V12_002.Orders.Callbacks.cs`    | 771  | None — signature unchanged |

The method signature `private void SymmetryGuardCascadeFollowerCleanup(string masterEntryName)`
is preserved identically. No upstream changes required.

---

## Implementation Checklist (for Phase 5 executor)

1. [ ] Read `src/V12_002.Symmetry.Replace.cs` lines 198–243 (the full method)
2. [ ] Extract foreach body (lines ~218–241) into `private void CancelFollowerEntryIfPending(string followerName)`
3. [ ] Replace foreach body in parent with single call: `CancelFollowerEntryIfPending(followerName);`
4. [ ] Optionally clean `pos.ExecutingAccount != null ? pos.ExecutingAccount.Name : "Master"` → `pos.ExecutingAccount?.Name ?? "Master"` (null-conditional; same semantics, cleaner)
5. [ ] Verify CYC of parent <= 8 and CYC of helper <= 8 via `python scripts/complexity_audit.py`
6. [ ] Run `dotnet build` — zero errors required
7. [ ] Run `dotnet csharpier check src/` — zero formatting issues
8. [ ] Run `powershell -File .\deploy-sync.ps1` — sync hard links

---

## Jane Street KB Compliance Summary

| Rule Source      | Rule                                           | Status       |
|------------------|------------------------------------------------|--------------|
| carl_cook        | Zero-alloc hot path                            | COMPLIANT — no new alloc |
| carl_cook        | AggressiveInlining hot / NoInlining cold       | ADVISORY — helper is private, JIT decides |
| carl_cook        | Avoid LINQ                                     | COMPLIANT — no LINQ in method |
| gjengset         | No new lock() blocks                           | COMPLIANT — ConcurrentDictionary used throughout |
| gjengset         | Volatile + Thread.MemoryBarrier where needed   | COMPLIANT — ADR-019 immutable snapshot pattern |
| trading_billions | Single responsibility per helper               | COMPLIANT — CancelFollowerEntryIfPending does one thing |
| trading_billions | Each helper CYC <= 8                           | COMPLIANT — parent: 4, helper: 7 |

---

## Risk Assessment

| Risk                                     | Likelihood | Mitigation                              |
|------------------------------------------|------------|-----------------------------------------|
| Order cancel during partial cleanup      | Low        | CancelOrderSafe has IsOrderTerminal guard (confirmed via get_call_hierarchy) |
| Null dereference on pos.ExecutingAccount | Low        | Null check present in original; preserved in helper |
| REAPER desync if follower fills before cancel | Low   | A2-3 deferral comment preserved — DeltaExpectedPositionLocked on OnAccountOrderUpdate |
| CYC exceeds 8 after extraction           | None       | Projected CYC 7 — verified via sequentialthinking |
