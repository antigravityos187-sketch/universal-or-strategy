# EPIC-W7-128 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-128/01-scope-boundary.md

---

## Summary

| Field | Value |
|---|---|
| **Epic ID** | EPIC-W7-128 |
| **Method** | `SymmetryGuardReplaceExistingFollowerTarget` |
| **Source File** | `src/V12_002.Symmetry.Replace.cs` |
| **CYC Baseline** | 20 |
| **CYC Target** | ≤ 8 |
| **CYC Projected (parent)** | **7** |
| **Helpers Extracted** | 3 |
| **Max Helper CYC** | 6 (`TryCancelStaleTarget`) |
| **max_cyc_projected** | **7** |

---

## Method Signature (Current)

```csharp
private void SymmetryGuardReplaceExistingFollowerTarget(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    ConcurrentDictionary<string, Order> dict
)
```

**Caller:** [`SymmetryGuardRetargetExistingFollowerBracket`](src/V12_002.Symmetry.Replace.cs:17) — called 5×
(once per target slot T1–T5).
**Upstream:** `SymmetryGuardTryResolveFollower` in `src/V12_002.Symmetry.Follower.cs` L225.

The method signature is **unchanged** by this refactor. All callers remain unmodified.

---

## Complexity Drivers (from Sequential Thinking Step 2)

| # | Driver | Lines | CYC Contribution |
|---|---|---|---|
| 1 | **Duplicated 4-way `OrderState` guard** (`Working\|\|Accepted\|\|Submitted\|\|ChangePending`) used twice in stale-cancel path (L45–50) and replace path (L67–72) | L45–50, L67–72 | +6 (3 per usage × 2) |
| 2 | **Compound entry guard** `isFilled \|\| isRunner \|\| qty <= 0` controlling entire stale-cleanup sub-block | L41 | +3 |
| 3 | **Inline spec-construction block** — OrderState guard + price guard `if (newPrice <= 0)` + ternary `pos.Direction` assignment all nested | L67–96 | +6 |
| 4 | **TryGetValue+null compound check** `dict.TryGetValue(...) && staleTarget != null` | L43 | +2 |
| 5 | **Null guard** `!dict.TryGetValue(fleetEntryName, out var oldTarget) \|\| oldTarget == null` | L59 | +2 |

Total estimated: 20. Matches CYC baseline.

---

## Extraction Plan

### Helper 1 — `IsOrderLive`

| Field | Value |
|---|---|
| **New method name** | `IsOrderLive` |
| **Signature** | `[MethodImpl(MethodImplOptions.AggressiveInlining)] private static bool IsOrderLive(Order order)` |
| **Extracted from lines** | L45–50 AND L67–72 (identical predicate used in both paths) |
| **Body** | Returns `order.OrderState == OrderState.Working \|\| order.OrderState == OrderState.Accepted \|\| order.OrderState == OrderState.Submitted \|\| order.OrderState == OrderState.ChangePending` |
| **CYC projected** | **4** (entry 1 + 3 `\|\|` operators) |
| **Jane Street attribute** | `[AggressiveInlining]` — hot-path boolean predicate, zero-alloc, called on every retarget |
| **LINQ** | None |
| **lock()** | None |

**Responsibility:** Single-responsibility predicate — "is this order in a cancellable live state?"

---

### Helper 2 — `TryCancelStaleTarget`

| Field | Value |
|---|---|
| **New method name** | `TryCancelStaleTarget` |
| **Signature** | `[MethodImpl(MethodImplOptions.NoInlining)] private bool TryCancelStaleTarget(string fleetEntryName, PositionInfo pos, int targetNumber, ConcurrentDictionary<string, Order> dict, bool isFilled, bool isRunner, int qty)` |
| **Extracted from lines** | L41–57 (entire stale-cleanup block including early-return compound guard, TryGetValue, OrderState check, Cancel, TryRemove) |
| **Returns** | `bool` — `true` if stale path was taken (caller should `return` immediately); `false` if not stale/filled/runner (continue to replace path) |
| **CYC projected** | **6** (entry 1 + compound guard `isFilled\|\|isRunner\|\|qty<=0` = +2 + TryGetValue && null = +2 + `IsOrderLive()` call if = +1) |
| **Jane Street attribute** | `[NoInlining]` — cold path; stale cancellation is the exception not the norm |
| **LINQ** | None |
| **lock()** | None |
| **Uses** | Calls `IsOrderLive(staleTarget)` (Helper 1) |

**Responsibility:** Handles the complete stale/filled/runner cleanup path in isolation.

---

### Helper 3 — `BuildFollowerTargetReplaceSpec`

| Field | Value |
|---|---|
| **New method name** | `BuildFollowerTargetReplaceSpec` |
| **Signature** | `[MethodImpl(MethodImplOptions.NoInlining)] private FollowerTargetReplaceSpec? BuildFollowerTargetReplaceSpec(string fleetEntryName, PositionInfo pos, int targetNumber, string targetTag, int qty)` |
| **Extracted from lines** | L74–91 (price guard, direction ternary, `SymmetryTrim` call, `FollowerTargetReplaceSpec` initializer) |
| **Returns** | `FollowerTargetReplaceSpec?` — `null` if `newPrice <= 0` (caller should `return` immediately) |
| **CYC projected** | **3** (entry 1 + `newPrice <= 0` guard +1 + direction ternary +1) |
| **Jane Street attribute** | `[NoInlining]` — cold path; spec construction happens on replace events only |
| **LINQ** | None |
| **lock()** | None |

**Responsibility:** Pure spec construction — builds `FollowerTargetReplaceSpec` from position/target data.

---

## Parent Method After Extraction

```csharp
private void SymmetryGuardReplaceExistingFollowerTarget(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    ConcurrentDictionary<string, Order> dict
)
{
    if (pos.ExecutingAccount == null)                                             // +1
        return;

    string targetTag = "T" + targetNumber;
    bool isRunner = IsRunnerTarget(targetNumber);
    bool isFilled = IsTargetFilled(pos, targetNumber);
    int qty = GetTargetContracts(pos, targetNumber);

    if (TryCancelStaleTarget(fleetEntryName, pos, targetNumber, dict,            // +1
            isFilled, isRunner, qty))
        return;

    if (!dict.TryGetValue(fleetEntryName, out var oldTarget) || oldTarget == null) // +2
        return;

    if (!IsOrderLive(oldTarget))                                                  // +1
        return;

    string signalName = SymmetryTrim(targetTag + "_" + fleetEntryName, 40);
    var tSpec = BuildFollowerTargetReplaceSpec(fleetEntryName, pos,              // 0
                    targetNumber, targetTag, qty);
    if (tSpec == null)                                                            // +1
        return;

    _followerTargetReplaceSpecs[signalName] = tSpec.Value;
    StampReaperMoveGrace();
    pos.ExecutingAccount.Cancel(new[] { oldTarget });
}
// Parent CYC = 7
```

**Parent CYC projected: 7** ✓ (≤ 8)

---

## CYC Budget Table

| Method | CYC Projected | Threshold | Status | Attribute |
|---|---|---|---|---|
| `IsOrderLive` | 4 | ≤ 8 | ✅ PASS | `AggressiveInlining` |
| `TryCancelStaleTarget` | 6 | ≤ 8 | ✅ PASS | `NoInlining` |
| `BuildFollowerTargetReplaceSpec` | 3 | ≤ 8 | ✅ PASS | `NoInlining` |
| `SymmetryGuardReplaceExistingFollowerTarget` (parent) | **7** | ≤ 8 | ✅ PASS | (existing) |
| **max_cyc_projected** | **7** | ≤ 8 | ✅ **PASS** | — |

---

## Jane Street KB Compliance

| Rule | Compliance |
|---|---|
| Zero-alloc hot path | ✅ `IsOrderLive` is `AggressiveInlining`, returns bool, no alloc |
| `AggressiveInlining` hot / `NoInlining` cold | ✅ Correctly applied across all 3 helpers |
| Avoid LINQ | ✅ No LINQ in any helper or parent |
| No new `lock()` blocks | ✅ No lock() added anywhere |
| Single responsibility per helper | ✅ Each helper has exactly one concern |
| Each helper CYC ≤ 8 | ✅ Max helper CYC = 6 |

---

## MCP Evidence

### get_context_bundle
- Symbol ID confirmed: `src/V12_002.Symmetry.Replace.cs::V12_002.SymmetryGuardReplaceExistingFollowerTarget#method`
- Method body fully retrieved (L27–L97, 71 lines)
- Confirmed: no LINQ in body; two duplicated 4-way OrderState guards visible at L45–50 and L67–72
- Confirmed: compound entry guard at L41; spec construction block L74–96

### get_call_hierarchy (depth=2, both directions)
- **Caller confirmed:** `SymmetryGuardRetargetExistingFollowerBracket` (same file, L17) — AST resolved
- **Callees confirmed:** `IsRunnerTarget`, `IsTargetFilled`, `GetTargetContracts`, `GetTargetPrice` (from `V12_002.PositionInfo.cs`), `SymmetryTrim` (same file, L343), `StampReaperMoveGrace` (`src/V12_002.SIMA.cs`, L199)
- Total callees: 14 (including src-vm-backup duplicates)
- No cross-file callers beyond `SymmetryGuardRetargetExistingFollowerBracket`

### get_dependency_graph
- `src/V12_002.Symmetry.Replace.cs` has 0 direct file-level import edges in index
- Confirms file is a partial class — dependencies resolved through C# partial class compilation, not explicit file imports
- Blast radius confined to file as expected

---

## Sequential Thinking Evidence

| Step | Thought | Conclusion |
|---|---|---|
| Thought 2 | CYC Complexity Drivers Analysis | Identified 5 distinct driver groups; duplicated OrderState guard accounts for ~6 CYC, compound entry guard ~3, spec block ~6, compound TryGetValue ~2, null guard ~2 — total maps to baseline CYC=20 |
| Thought 3 | Extraction Strategy Design | 3-helper plan: `IsOrderLive` (shared predicate), `TryCancelStaleTarget` (stale path), `BuildFollowerTargetReplaceSpec` (spec construction). Parent reduced to linear 7-branch orchestrator. |
| Thought 4 | CYC Validation | Verified per-helper CYC counts: IsOrderLive=4, TryCancelStaleTarget=6, BuildFollowerTargetReplaceSpec=3, parent=7. All ≤8. max_cyc_projected=7. |

---

## Implementation Instructions for Phase 5 (v12-engineer)

1. **Add `IsOrderLive` private static method** to `src/V12_002.Symmetry.Replace.cs` (or `src/V12_002.cs` partial), decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`. Body: 4-way OrderState OR predicate.

2. **Add `TryCancelStaleTarget` private method** to same file, decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`. Accepts the 7 parameters listed above. Internally calls `IsOrderLive`. Returns `bool`.

3. **Add `BuildFollowerTargetReplaceSpec` private method** to same file, decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`. Returns `FollowerTargetReplaceSpec?` (nullable struct). Contains price guard, direction ternary, and struct initialization.

4. **Replace body of `SymmetryGuardReplaceExistingFollowerTarget`** with the 7-branch orchestrator form shown in the "Parent Method After Extraction" section above.

5. **Verify:** After changes, run `python scripts/complexity_audit.py` to confirm CYC ≤ 8 for all four methods.

6. **Build:** Run `powershell -File .\scripts\build_readiness.ps1` — must pass with zero errors.

7. **Deploy-sync:** Run `powershell -File .\deploy-sync.ps1` to resync NinjaTrader hard links.

**Scope note (V12.23):** Only `src/V12_002.Symmetry.Replace.cs` is modified. No other files. No interface changes. No caller changes.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 3 |
| **Execution Time** | batch |
| **Phase** | 2 — Architecture Planning |
| **Wave** | 7 |
| **Epic** | EPIC-W7-128 |
| **MCP Tools Used** | `resolve_repo`, `get_context_bundle`, `get_call_hierarchy`, `get_dependency_graph` |
| **Sequential Thinking Steps** | 4 (probe + 3 substantive) |
| **max_cyc_projected** | 7 |
| **Status** | completed |
