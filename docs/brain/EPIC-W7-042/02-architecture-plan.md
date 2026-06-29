# EPIC-W7-042 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29
**Input:** docs/brain/EPIC-W7-042/01-scope-boundary.md

---

## Summary

Extract 2 private helpers from `SymmetryGuardOnFollowerFill` (CYC 16) to bring all methods to CYC <= 8.
No interface changes. No cross-file impact. Same-file private helpers only (V12.23 compliant).

---

## Target Method

| Field | Value |
|---|---|
| **Method** | `SymmetryGuardOnFollowerFill` |
| **File** | `src/V12_002.Symmetry.Follower.cs` |
| **Line** | 17 |
| **CYC Baseline** | 16 |
| **Max Nesting** | 6 |
| **Lines** | 72 |
| **Params** | 3 (`string fleetEntryName`, `PositionInfo followerPos`, `double followerFillPrice`) |
| **Callers** | 1 (upstream only — not modified by this epic) |

---

## CYC Driver Analysis

The following decision points in the method body drive the CYC score to 16:

| # | Decision Point | Location | CYC Contribution |
|---|---|---|---|
| 1 | `followerPos == null \|\| !followerPos.IsFollower` | Line 23 | +2 (short-circuit `\|\|`) |
| 2 | `followerPos.RemainingContracts <= 0` | Line 27 | +1 |
| 3 | `!followerPos.BracketSubmitted` | Line 30 | +1 |
| 4 | `TryGetValue(fleetEntryName, ...) && TryGetValue(preCheckId, ...)` | Line 37-40 | +2 (short-circuit `&&`) |
| 5 | `anchorReady && preCheckAnchor > 0` | Line 47 | +2 (short-circuit `&&`) |
| 6 | `if (shouldSubmitImmediately)` | Line 60 | +1 |
| 7 | `followerFillPrice > 0 ? ... : ...` | Line 78 | +1 |
| 8 | `if (SymmetryGuardTryResolveFollower(...))` | Line 84 | +1 |

**Total CYC: ~16** (baseline confirmed by jcodemunch index)

**Two cohesive clusters identified:**
- **Cluster A** (decisions 3-6): The entire `!BracketSubmitted` block — anchor pre-check + bracket submission decision
- **Cluster B** (decisions 7-8): PendingFollowerFill enqueue + try-resolve

---

## Extraction Plan

### Extraction Count: 2

---

### Helper 1: `SymmetryGuardHandleInitialBracketSubmission`

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void SymmetryGuardHandleInitialBracketSubmission(
    string fleetEntryName,
    PositionInfo followerPos
)
```

**Responsibility:** Determine whether the follower bracket should be submitted immediately
(master anchor already resolved) or deferred until anchor resolves. Encapsulates all
anchor pre-check logic and cold-path logging.

**Source lines extracted:** Lines 30-73 (`!BracketSubmitted` block)

**Internal decision points:**
1. `if (!followerPos.BracketSubmitted)` — outer gate
2. `if (TryGetValue && TryGetValue)` — fleet dispatch context lookup (&&)
3. `if (anchorReady && preCheckAnchor > 0)` — anchor ready check (&&)
4. `if (shouldSubmitImmediately)` — submit vs defer decision

**Projected CYC:** 1 (base) + 4 decision points = **5** ✓

**Jane Street — NoInlining:** Contains two `Print(string.Format(...))` cold-logging calls:
- `[ANCHOR-01]` path: pre-applying master anchor
- `[ANCHOR-GATE]` path: delaying bracket

Both `Print` calls are cold (execute once per fill event, not in tight loop). Mark
`[MethodImpl(MethodImplOptions.NoInlining)]` to keep JIT from inlining Print overhead
into hot call sites (carl_cook pattern: extract cold logging out-of-line).

**ADR-019 Lock-free preservation:** Reads `AnchorSnapshot` as an immutable snapshot
(`preCheckCtx.Anchor` — single field read). `IsResolved` and `MasterAnchorPrice` come
from the same snapshot object. No change to the Interlocked.CompareExchange publish path.

---

### Helper 2: `SymmetryGuardEnqueueAndTryResolve`

```csharp
private void SymmetryGuardEnqueueAndTryResolve(
    string fleetEntryName,
    PositionInfo followerPos,
    double followerFillPrice
)
```

**Responsibility:** Build the `PendingFollowerFill` record from fill data, enqueue it to
`symmetryPendingFollowerFills`, attempt immediate resolution, and remove from the queue
on success.

**Source lines extracted:** Lines 75-85

**Internal decision points:**
1. `followerFillPrice > 0 ? followerFillPrice : followerPos.EntryPrice` — ternary price selection
2. `if (SymmetryGuardTryResolveFollower(...))` — resolution outcome gate

**Projected CYC:** 1 (base) + 2 decision points = **3** ✓

**Jane Street — Zero-alloc note:** `new PendingFollowerFill{...}` is one heap allocation
per fill event — acceptable at this call frequency. No string allocations on this path.

---

### Parent After Extraction: `SymmetryGuardOnFollowerFill`

```csharp
private bool SymmetryGuardOnFollowerFill(
    string fleetEntryName,
    PositionInfo followerPos,
    double followerFillPrice
)
```

**Remaining responsibilities:**
1. Null + IsFollower guard (early return false)
2. Initialize `RemainingContracts` if zero
3. Delegate to `SymmetryGuardHandleInitialBracketSubmission`
4. Delegate to `SymmetryGuardEnqueueAndTryResolve`
5. Return true

**Remaining decision points:**
1. `followerPos == null || !followerPos.IsFollower` — null/type guard (||)
2. `followerPos.RemainingContracts <= 0` — init guard

**Projected CYC:** 1 (base) + 2 (||) + 1 (if) = **4** ✓

---

## CYC Projection Summary

| Method | Role | CYC Before | CYC After | Status |
|---|---|---|---|---|
| `SymmetryGuardOnFollowerFill` | Parent (modified) | 16 | **4** | ✓ PASS |
| `SymmetryGuardHandleInitialBracketSubmission` | New helper | — | **5** | ✓ PASS |
| `SymmetryGuardEnqueueAndTryResolve` | New helper | — | **3** | ✓ PASS |

**max_cyc_projected: 5** (all methods ≤ 8 ✓)

---

## Pseudocode: Parent After Extraction

```csharp
private bool SymmetryGuardOnFollowerFill(
    string fleetEntryName,
    PositionInfo followerPos,
    double followerFillPrice
)
{
    if (followerPos == null || !followerPos.IsFollower)
        return false;

    followerPos.EntryFilled = true;
    if (followerPos.RemainingContracts <= 0)
        followerPos.RemainingContracts = Math.Max(1, followerPos.TotalContracts);

    SymmetryGuardHandleInitialBracketSubmission(fleetEntryName, followerPos);
    SymmetryGuardEnqueueAndTryResolve(fleetEntryName, followerPos, followerFillPrice);

    return true;
}
```

---

## Jane Street KB Alignment

| Pattern | Source | Application |
|---|---|---|
| `NoInlining` on cold logging | carl_cook | `SymmetryGuardHandleInitialBracketSubmission` marked NoInlining (contains Print + string.Format) |
| Hot path zero-alloc | carl_cook | Parent and Helper 2 have no string allocation on hot path |
| No shared mutable state across helpers | gjengset | No new fields introduced; both helpers access existing class-level ConcurrentDictionary fields only |
| Immutable snapshot / lock-free read | gjengset | AnchorSnapshot read pattern unchanged (ADR-019 preserved) |
| Single responsibility per helper | trading_billions | Helper 1 = bracket submission decision only; Helper 2 = enqueue + resolve only |
| Defense in depth (guard in parent) | trading_billions | Null / IsFollower guard stays in parent — input validation before any helper call |
| Rate-limit / circuit breaker | trading_billions | `!BracketSubmitted` gate + anchor-not-ready path in Helper 1 naturally defers bracket until anchor is resolved |

---

## MCP Evidence

### jcodemunch: resolve_repo
```json
{
  "found": true, "indexed": true,
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "symbol_count": 5147, "file_count": 2000,
  "source_root": "/home/malhitticrypto/universal-or-strategy"
}
```

### jcodemunch: get_symbol_complexity
```json
{
  "symbol_id": "src/V12_002.Symmetry.Follower.cs::V12_002.SymmetryGuardOnFollowerFill#method",
  "cyclomatic": 16, "max_nesting": 6, "param_count": 3,
  "lines": 72, "assessment": "high"
}
```

### jcodemunch: get_call_hierarchy
- **Callers:** 0 direct callers resolved at depth=2
- **Callees (depth 1):** `SymmetryGuardApplyMasterAnchor`, `SymmetryGuardSubmitFollowerBracket`, `SymmetryGuardTryResolveFollower`, `symmetryFleetEntryToDispatch`, `symmetryDispatchById`, `symmetryPendingFollowerFills`
- **No cross-file callers of this method** confirmed

### jcodemunch: get_dependency_graph
- File `src/V12_002.Symmetry.Follower.cs` has **0 import edges and 0 importer edges** in the index
- All dependencies are within the same partial class (class-level fields, same assembly)

### jcodemunch: get_extraction_candidates
- `SymmetryGuardOnFollowerFill`: CYC=16, max_nesting=6, param_count=3 — primary extraction target ✓
- Other high-CYC methods in same file (`SymmetryGuardTryResolveFollower` CYC=20, `SymmetryGuardSubmitFollowerBracket` CYC=16) are **out of scope** per V12.23 / 01-scope-boundary.md

---

## Sequential Thinking Evidence

**Thought 1 — CYC Driver Analysis:**
Identified 8 decision points with two cohesive extraction clusters:
- Cluster A (lines 30-73): `!BracketSubmitted` + anchor pre-check + submit decision (4 decision points, nesting depth 6)
- Cluster B (lines 75-85): PendingFollowerFill enqueue + try-resolve (2 decision points)

**Thought 2 — Extraction Plan Validated:**
- Helper 1: `SymmetryGuardHandleInitialBracketSubmission` → CYC 5
- Helper 2: `SymmetryGuardEnqueueAndTryResolve` → CYC 3
- Parent after extraction: CYC 4
- All three: ≤ 8 ✓. Extraction count = 2.

**Thought 3 — Jane Street Alignment Confirmed:**
- `NoInlining` on Helper 1 (contains Print/string.Format cold logging)
- Zero-alloc on hot path preserved
- Lock-free AnchorSnapshot pattern (ADR-019) undisturbed
- Single responsibility verified per helper
- Defense in depth: null guard remains in parent

---

## Implementation Notes for Phase 5

1. Extract the `!BracketSubmitted` block (lines 30-73) into `SymmetryGuardHandleInitialBracketSubmission` — add `[MethodImpl(MethodImplOptions.NoInlining)]` attribute
2. Extract the pending fill + resolve block (lines 75-85) into `SymmetryGuardEnqueueAndTryResolve`
3. Replace extracted blocks in parent with two method calls
4. Both helpers are `private void` — no return value needed
5. All helpers go in same file (`src/V12_002.Symmetry.Follower.cs`), same partial class
6. No signature change to `SymmetryGuardOnFollowerFill` — callers unaffected
7. Verify build passes: `powershell -File .\scripts\build_readiness.ps1`
8. Verify CYC after: run `python scripts/complexity_audit.py` — confirm all three methods ≤ 8

---

## Scope Compliance (V12.23)

| Check | Status |
|---|---|
| Single method targeted | PASS |
| Helpers extracted from subject only | PASS |
| No caller modifications | PASS |
| No sibling method modifications | PASS |
| No cross-file refactoring | PASS |
| max_cyc_projected ≤ 8 | PASS (max=5) |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-042 |
| **Extraction Count** | 2 |
| **max_cyc_projected** | 5 |
| **MCP Tools Used** | resolve_repo, search_symbols, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates, get_symbol_complexity |
| **Sequential Thinking Thoughts** | 3 |
