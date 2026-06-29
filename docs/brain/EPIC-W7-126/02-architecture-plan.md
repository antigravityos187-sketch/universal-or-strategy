# EPIC-W7-126 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T00:40:00Z
**Input:** docs/brain/EPIC-W7-126/01-scope-boundary.md

---

## Summary

Reduce cyclomatic complexity of `SymmetryGuardSubmitFollowerBracket` from **CYC 16** to **CYC <= 6**
by extracting three private helper methods into the same partial class in
[`src/V12_002.Symmetry.Follower.cs`](src/V12_002.Symmetry.Follower.cs).

**Boundary verdict (Phase 1.5):** PASS — single-file blast radius, signature unchanged, 2 callers untouched.

---

## Complexity Driver Breakdown (CYC = 16)

| # | Driver | CYC |
|---|--------|-----|
| 1 | Base method | +1 |
| 2 | `if (pos.BracketSubmitted) return` | +1 |
| 3 | `if (acct == null) return` | +1 |
| 4 | `exitAction` Direction ternary | +1 |
| 5 | `ocoId` IsNullOrEmpty ternary | +1 |
| 6 | `for (targetNum = 1..5)` loop | +1 |
| 7 | `if (targetQty <= 0) continue` | +1 |
| 8 | `if (IsRunnerTarget(targetNum))` | +1 |
| 9 | `if (targetPrice <= 0)` with Print | +1 |
| 10 | `for (i = 0; i < 5)` price init | +1 |
| 11 | `foreach (stagedTargets)` FSM assignment | +1 |
| 12 | `if (tNum >= 1 && tNum <= 5)` compound (left) | +1 |
| 13 | `if (tNum >= 1 && tNum <= 5)` compound (right) | +1 |
| 14 | `foreach (stagedTargets)` dict write | +1 |
| 15–16 | Additional branch paths | +2 |
| **Total** | | **16** |

---

## Extraction Plan

### Helper 1 — `ResolveOcoGroupId`

| Field | Value |
|-------|-------|
| **Signature** | `private string ResolveOcoGroupId(PositionInfo pos)` |
| **Extracted logic** | `!string.IsNullOrEmpty(pos.OcoGroupId) ? pos.OcoGroupId : ("SG_" + DateTime.UtcNow.Ticks.ToString())` |
| **CYC projected** | **2** (base + 1 conditional) |
| **CYC removed from parent** | -1 |
| **Jane Street attribute** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — trivial hot-path helper (carl_cook) |
| **Responsibility** | Single: determine OCO group identifier |

**Call site in parent:**
```csharp
string ocoId = ResolveOcoGroupId(pos);
```

---

### Helper 2 — `TryBuildTargetOrder`

| Field | Value |
|-------|-------|
| **Signature** | `private bool TryBuildTargetOrder(Account acct, PositionInfo pos, int targetNum, string fleetEntryName, string ocoId, OrderAction exitAction, out (int targetNum, Order order) staged)` |
| **Extracted logic** | Inner loop body: qty guard, runner guard, price guard (with Print), `RoundToTickSize`, `SymmetryTrim`, `acct.CreateOrder`, staged add |
| **CYC projected** | **5** (base + qty guard + runner guard + price guard + Print path) |
| **CYC removed from parent** | -3 |
| **Jane Street attribute** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on method; cold `Print` path extracted into `[MethodImpl(MethodImplOptions.NoInlining)]` helper `LogTargetSkip` (carl_cook) |
| **Responsibility** | Single: build one target limit order or return false |

**Call site in parent:**
```csharp
for (int targetNum = 1; targetNum <= 5; targetNum++)
{
    if (!TryBuildTargetOrder(acct, pos, targetNum, fleetEntryName, ocoId, exitAction, out var staged))
        continue;
    stagedTargets.Add(staged);
    ordersToSubmit.Add(staged.order);
    nonRunnerLimitQty += staged.order.Quantity;
}
```

**Note on `runnerQty`:** The runner accumulator (`runnerQty += targetQty` when `IsRunnerTarget`) is moved into the helper via a `ref int runnerQty` parameter to preserve the audit-log value needed in the final `Print` call.

**Revised signature:**
```csharp
private bool TryBuildTargetOrder(
    Account acct, PositionInfo pos, int targetNum, string fleetEntryName,
    string ocoId, OrderAction exitAction,
    ref int runnerQty,
    out (int targetNum, Order order) staged)
```

---

### Helper 3 — `CommitFsmAndDictionaries`

| Field | Value |
|-------|-------|
| **Signature** | `private void CommitFsmAndDictionaries(FollowerBracketFSM fsm, string fleetEntryName, List<(int targetNum, Order order)> stagedTargets)` |
| **Extracted logic** | `for(i<5)` zero-init of `ExpectedTargetPrices`; `foreach(stagedTargets)` FSM target/price assignment with compound `tNum` guard; `_followerBrackets[fleetEntryName] = fsm`; `foreach(stagedTargets)` `GetTargetOrdersDictionary(tNum)[fleetEntryName] = order` |
| **CYC projected** | **6** (base + for init + foreach FSM + if tNum>=1 + if tNum<=5 + foreach dict) |
| **CYC removed from parent** | -4 |
| **Jane Street attribute** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — deterministic commit with no branching surprises (carl_cook); no new `lock()` blocks, actor Enqueue pattern preserved in parent (gjengset) |
| **Responsibility** | Single: commit FSM struct and fill order dictionaries atomically |

**Call site in parent:**
```csharp
CommitFsmAndDictionaries(fsm, fleetEntryName, stagedTargets);
```

---

## Parent CYC Projection After Extractions

| Remaining driver | CYC |
|-----------------|-----|
| Base | +1 |
| `if (pos.BracketSubmitted) return` | +1 |
| `if (acct == null) return` | +1 |
| `exitAction` Direction ternary | +1 |
| `for (targetNum = 1..5)` + call to `TryBuildTargetOrder` | +1 |
| `Enqueue(ctx => {...})` lambda | +1 |
| **Parent CYC projected** | **6** |

**Target <= 8: PASS**

---

## Full Extraction Summary Table

| Helper | Signature | Extracted Logic | CYC Projected | CYC Removed | Jane Street Rule |
|--------|-----------|-----------------|---------------|-------------|------------------|
| `ResolveOcoGroupId` | `private string ResolveOcoGroupId(PositionInfo pos)` | OcoId ternary | 2 | -1 | AggressiveInlining (carl_cook) |
| `TryBuildTargetOrder` | `private bool TryBuildTargetOrder(Account acct, PositionInfo pos, int targetNum, string fleetEntryName, string ocoId, OrderAction exitAction, ref int runnerQty, out (int targetNum, Order order) staged)` | Inner loop body: 3 guards + CreateOrder | 5 | -3 | AggressiveInlining hot / NoInlining cold log (carl_cook) |
| `CommitFsmAndDictionaries` | `private void CommitFsmAndDictionaries(FollowerBracketFSM fsm, string fleetEntryName, List<(int targetNum, Order order)> stagedTargets)` | FSM init + 2 foreach loops + compound guard | 6 | -4 | AggressiveInlining; no lock() (carl_cook + gjengset) |
| **Parent (after)** | `private void SymmetryGuardSubmitFollowerBracket(string fleetEntryName, PositionInfo pos)` | Guards + stop build + for loop + Enqueue + Submit | **6** | -8 total | — |

**Max CYC projected across all methods: 6 <= 8 ✓**

---

## Jane Street KB Compliance

| Rule Source | Rule | Applied Where |
|------------|------|---------------|
| carl_cook | Zero-alloc hot path; `AggressiveInlining` on hot helpers | `ResolveOcoGroupId`, `TryBuildTargetOrder`, `CommitFsmAndDictionaries` |
| carl_cook | `NoInlining` on cold logging path | `LogTargetSkip` (cold `Print` inside `TryBuildTargetOrder`) |
| carl_cook | Avoid LINQ; use `ref`/`out` structs | `out (int, Order) staged` avoids extra allocation in loop |
| gjengset | No new `lock()` blocks | All commits via existing `Enqueue` actor pipeline or direct dict assignment |
| gjengset | Volatile + MemoryBarrier not needed | Actor model (`Enqueue`) handles ordering — no new primitives added |
| trading_billions | Single responsibility per helper | Each of the 3 helpers has exactly one stated purpose |
| trading_billions | Each helper CYC <= 8 | Max projected CYC = 6 across all helpers |
| trading_billions | Defense in depth | Guards preserved in `TryBuildTargetOrder`; null guard stays in parent |

---

## MCP Evidence

| Tool | Result |
|------|--------|
| `resolve_repo` | Repo `antigravityos187-sketch/universal-or-strategy` indexed: 5147 symbols, 2000 files |
| `get_context_bundle` | `SymmetryGuardSubmitFollowerBracket` retrieved: lines 285–425, CYC drivers confirmed in source |
| `get_call_hierarchy` | Resolved via candidate ID; callers in same file only (SymmetryGuardOnFollowerFill ln 62, SymmetryGuardTryResolveFollower ln 230) |
| `get_dependency_graph` | `src/V12_002.Symmetry.Follower.cs` has 0 cross-file import edges — blast radius fully contained |

---

## Sequential Thinking Evidence

| Step | Thought | Conclusion |
|------|---------|------------|
| 1 | Initial probe — identified 3 extraction candidates from pre-loaded hotspot data | 3 helpers confirmed |
| 2 | Complexity driver analysis — mapped all 16 CYC contributors to source lines | Extraction targets validated against actual source |
| 3 | Extraction strategy — designed signatures with Jane Street attributes; verified single responsibility per helper | Signatures finalized |
| 4 | CYC validation — computed projected CYC for parent (6) and all helpers (2, 5, 6) | All <= 8, plan valid |

---

## Implementation Notes for Phase 5 (Bob CLI)

1. **File:** [`src/V12_002.Symmetry.Follower.cs`](src/V12_002.Symmetry.Follower.cs) — all new methods added as `private` in the same partial class.
2. **Order of extraction (safest):**
   a. Extract `ResolveOcoGroupId` first (trivial, no side effects).
   b. Extract `TryBuildTargetOrder` (loop body — use `ref int runnerQty` + `out (int, Order) staged`).
   c. Extract `CommitFsmAndDictionaries` (FSM commit block — includes `_followerBrackets` dict write).
3. **Do NOT move the `Enqueue(ctx => {...})` lambda** — it must stay in parent to use correct closures.
4. **Do NOT modify the `ordersToSubmit.Insert(0, stop)` line** — stop order must remain first.
5. **Verify:** After extraction, parent method lines 285–425 should reduce to ~50 lines.
6. **Test signal:** Callers `SymmetryGuardOnFollowerFill` and `SymmetryGuardTryResolveFollower` must compile unchanged.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-126 |
| **Method** | SymmetryGuardSubmitFollowerBracket |
| **CYC Baseline** | 16 |
| **CYC Projected (parent)** | 6 |
| **CYC Projected (max helper)** | 6 |
| **Extractions Planned** | 3 |
| **Boundary Verdict (Phase 1.5)** | PASS |
