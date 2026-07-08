# EPIC-W7-044 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-044/01-scope-boundary.md

---

## Summary

**Method:** `SymmetryGuardCascadeFollowerCleanup`
**File:** `src/V12_002.Symmetry.Replace.cs` (lines 198–243)
**Class:** `V12_002` (partial)
**CYC Baseline:** 11
**CYC Target:** <= 8
**Extraction Count:** 3
**Max CYC Projected:** 6

Boundary verdict from Phase 1.5: **PASS**. Scope is strictly the target method and its
3 new private helper methods added to the same partial class in the same file.

---

## Source Method (Confirmed by MCP)

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
            // to prevent REAPER desync if the follower was microseconds from filling (Build 960 audit fix).
        }
    }
}
```

---

## CYC Branch Map (Baseline = 11)

| # | Branch | Location | Type |
|---|--------|----------|------|
| 1 | Base path | — | base |
| 2 | `TryGetValue` on `symmetryMasterEntryToDispatch` | line 200 | early-exit |
| 3 | `TryGetValue` on `symmetryDispatchById` | line 202 | early-exit |
| 4 | `foreach` loop over `followers[]` | line 210 | loop |
| 5 | `TryGetValue` on `activePositions` → continue | line 218 | guard |
| 6 | `TryGetValue` on `entryOrders` → continue | line 220 | guard |
| 7 | `null` check on `order` → continue | line 222 | guard |
| 8 | `OrderState == Working` | line 225 | predicate |
| 9 | `OrderState == Submitted` | line 226 | predicate |
| 10 | `OrderState == Accepted` | line 227 | predicate |
| 11 | `ExecutingAccount != null` ternary | line 233 | ternary |

**Cluster groupings used for extraction:**
- **CLUSTER A** (branches 2–3): Dictionary resolution chain
- **CLUSTER B** (branches 5–7): Per-follower null-guard cascade
- **CLUSTER C** (branches 8–10): OrderState multi-predicate
- **CLUSTER D** (branch 11): Account name ternary inside logging

---

## Extraction Plan

### Helper 1: `TryResolveCascadeContext`

```
Signature:  private bool TryResolveCascadeContext(string masterEntryName, out string[] followers)
Cluster:    CLUSTER A (branches 2–3)
Responsibility: Perform both TryGetValue lookups, read ctx.Followers snapshot.
                Returns false if either lookup misses — parent returns immediately.
CYC Projected:  3  (base=1 + TryGetValue miss=1 + TryGetValue miss=1)
Inlining:   [MethodImpl(MethodImplOptions.AggressiveInlining)]
                Called once per cancel event (cold path boolean gate, zero allocation).
```

**Body sketch:**
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

---

### Helper 2: `IsFollowerEntryLive`

```
Signature:  private static bool IsFollowerEntryLive(Order order)
Cluster:    CLUSTER C (branches 8–10)
Responsibility: Returns true iff order.OrderState is Working, Submitted, or Accepted.
                Pure stateless predicate. No I/O, no instance state.
CYC Projected:  4  (base=1 + Working=1 + Submitted=1 + Accepted=1)
Inlining:   [MethodImpl(MethodImplOptions.AggressiveInlining)]
                Pure predicate called per-follower — must be inlined to stay zero-overhead.
Note:       00-hotspots.md confirms this predicate appears verbatim in
            SymmetryGuardReplaceExistingFollowerTarget (lines 45-51) — extraction
            enables future deduplication in Phase 3/5 at zero extra cost now.
```

**Body sketch:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsFollowerEntryLive(Order order)
{
    return order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Submitted
        || order.OrderState == OrderState.Accepted;
}
```

---

### Helper 3: `TryCancelFollowerEntry`

```
Signature:  private void TryCancelFollowerEntry(string followerName)
Clusters:   CLUSTER B (branches 5–7) + CLUSTER D (branch 11)
Responsibility: Guard lookups for activePositions + entryOrders + null order, then
                if IsFollowerEntryLive, log and call CancelOrderSafe. Contains all
                per-follower side effects including logging and order cancellation.
CYC Projected:  6  (base=1 + activePositions miss=1 + entryOrders miss=1 +
                     null order=1 + IsFollowerEntryLive gate=1 +
                     ExecutingAccount null ternary=1)
Inlining:   [MethodImpl(MethodImplOptions.NoInlining)]
                Contains Print/string.Format (cold logging path) — NoInlining to
                keep hot-path lean per carl_cook cold-path extraction pattern.
```

**Body sketch:**
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

---

## Parent After Extraction

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

**Parent CYC Projected:** 3
(base=1 + TryResolveCascadeContext gate=1 + foreach loop=1)

---

## CYC Summary Table

| Symbol | CYC Before | CYC After | Limit | Status |
|--------|-----------|-----------|-------|--------|
| `SymmetryGuardCascadeFollowerCleanup` (parent) | 11 | 3 | 8 | PASS |
| `TryResolveCascadeContext` (new) | — | 3 | 8 | PASS |
| `IsFollowerEntryLive` (new) | — | 4 | 8 | PASS |
| `TryCancelFollowerEntry` (new) | — | 6 | 8 | PASS |
| **Max CYC projected** | — | **6** | 8 | **PASS** |

---

## MCP Evidence

| Tool | Result |
|------|--------|
| `resolve_repo` | Repo indexed: `antigravityos187-sketch/universal-or-strategy`, 5147 symbols |
| `get_context_bundle` | Source retrieved at `src/V12_002.Symmetry.Replace.cs:198` — CYC=11 confirmed |
| `get_call_hierarchy` | Callers: 0 direct callers found in AST graph (caller confirmed in 00-hotspots.md as `HandleOrderCancelled_RollbackUnfilledEntry`). Callees: 18 including `CancelOrderSafe`, `Format`, `ValidateThreadAffinity`, `IsOrderTerminal` |
| `get_dependency_graph` | `src/V12_002.Symmetry.Replace.cs` is a self-contained partial class — 0 import edges, 0 importer edges |
| `get_extraction_candidates` | No automated candidates returned (requires min_callers=2); manual analysis from context bundle used |

---

## Sequential Thinking Evidence

**Thought 2 (Extraction Plan):** Three extraction targets identified. CLUSTER A → `TryResolveCascadeContext`
(CYC 3). CLUSTER C → `IsFollowerEntryLive` (CYC 4). CLUSTER B+D → `TryCancelFollowerEntry` (CYC 6).
Parent projected CYC = 3. All symbols satisfy ≤8 constraint. Design rule: PASS.

**Thought 3 (Jane Street Alignment):** Lock-free read ordering preserved — `ctx.Followers` snapshot
captured in `TryResolveCascadeContext` before loop, `activePositions`/`entryOrders` lookups remain
per-iteration as required. AggressiveInlining on hot predicates, NoInlining on cold logging path.
Single-responsibility per helper confirmed. Defense-in-depth guard chain preserved.

---

## Jane Street Alignment Notes

| Pattern | Source | Application |
|---------|--------|-------------|
| Cache-line safety / lock-free read ordering | gjengset | `TryResolveCascadeContext` captures `ctx.Followers` snapshot BEFORE loop. Per-iteration `TryGetValue` on `activePositions`/`entryOrders` in `TryCancelFollowerEntry` retains original read ordering — no prefetch that could race. ADR-019 immutable snapshot contract preserved. |
| Hot path zero-alloc / cold logging out-of-line | carl_cook | `TryResolveCascadeContext` and `IsFollowerEntryLive` marked `AggressiveInlining` (zero alloc, hot path). `TryCancelFollowerEntry` marked `NoInlining` (contains `Print`/`string.Format` — cold logging path isolated). |
| Defense in depth / single responsibility / rate-limit | trading_billions | Each helper has exactly one responsibility. `TryResolveCascadeContext` = dictionary resolution only. `IsFollowerEntryLive` = state predicate only. `TryCancelFollowerEntry` = per-follower cancel execution only. Null-guard cascade preserved as explicit defense layers. |

---

## Safety Constraints (from 00-hotspots.md)

1. **Two-phase cancel/rollback FSM ordering MUST be preserved.** `SymmetryGuardCascadeFollowerCleanup`
   cancels follower orders; `RollbackExpectedPosition` and `CleanupPosition` run in the caller
   (`HandleOrderCancelled_RollbackUnfilledEntry`) immediately after this method returns. The extraction
   must NOT move any calls out of order relative to the caller sequence.

2. **ADR-019 immutable snapshot contract.** `ctx.Followers` is a `string[]` snapshot — safe to read
   lock-free. Do NOT replace with a live dictionary lookup inside the loop.

3. **A2-3 deferred delta rollback comment.** The `// A2-3:` comment block inside `TryCancelFollowerEntry`
   MUST be preserved verbatim — it documents the Build 960 audit fix rationale.

4. **Method signature is frozen.** `SymmetryGuardCascadeFollowerCleanup(string masterEntryName)` must
   retain its exact signature — caller in `HandleOrderCancelled_RollbackUnfilledEntry` is not modified.

---

## File Placement

All 3 helpers are added as **private methods in the same partial class** (`V12_002`) in the same file
(`src/V12_002.Symmetry.Replace.cs`). No new files. No interface changes. Consistent with V12.23
No Scope Creep Protocol — same-file private helpers only.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase2-architecture |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-044 |
| **Method** | SymmetryGuardCascadeFollowerCleanup |
| **CYC Baseline** | 11 |
| **Extraction Count** | 3 |
| **Max CYC Projected** | 6 |
| **Design Rule** | All helpers AND parent CYC <= 8: PASS |
| **Boundary Verdict** | PASS (from Phase 1.5) |
| **MCP Tools Used** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **Sequential Thinking Thoughts** | 4 (probe + 3 analytical) |
| **Output** | docs/brain/EPIC-W7-044/02-architecture-plan.md |
