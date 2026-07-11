# EPIC-W7-129 — Phase 2: Architecture Plan

**Agent Name:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-129/01-scope-boundary.md

---

## Target Method

| Field | Value |
|---|---|
| **Method** | `SymmetryGuardTryResolveFollower` |
| **File** | `src/V12_002.Symmetry.Follower.cs` |
| **Lines** | 129–246 |
| **CYC Baseline** | 16 |
| **CYC Target** | ≤ 8 |
| **Signature** | `private bool SymmetryGuardTryResolveFollower(string fleetEntryName, PositionInfo pos, PendingFollowerFill pending, DateTime nowUtc)` |

---

## MCP Evidence

### jCodemunch: get_symbol_source
- Symbol ID resolved: `src/V12_002.Symmetry.Follower.cs::V12_002.SymmetryGuardTryResolveFollower#method`
- Lines 129–246 confirmed (118 lines)
- Three complexity driver clusters identified in source: dispatch-context tri-OR guard (lines 135–157), slippage evaluation (lines 183–200), bracket routing fork (lines 208–233)

### jCodemunch: get_call_hierarchy (depth=2, direction=both)
- **Direct callers (2):**
  - `SymmetryGuardOnFollowerFill` — `src/V12_002.Symmetry.Follower.cs` line 17
  - `SymmetryGuardProcessPendingFollowerFills` — `src/V12_002.Symmetry.Follower.cs` line 97
- **Key direct callees:**
  - `SymmetryGuardSkipFollower` — `src/V12_002.Symmetry.Replace.cs` line 99
  - `SymmetryGuardApplyMasterAnchor` — `src/V12_002.Symmetry.Follower.cs` line 248
  - `SymmetryGuardRetargetExistingFollowerBracket` — `src/V12_002.Symmetry.Replace.cs` line 17
  - `SymmetryGuardSubmitFollowerBracket` — `src/V12_002.Symmetry.Follower.cs` line 285
- **Shared state accessed:** `symmetryFleetEntryToDispatch` (ConcurrentDictionary), `symmetryDispatchById` (ConcurrentDictionary) — both lock-free per ADR-019

### jCodemunch: get_dependency_graph
- `src/V12_002.Symmetry.Follower.cs` has zero declared import edges (partial-class pattern — all types resolved at compile time via same-namespace partial class split)
- Blast radius fully contained within the partial class; no cross-file import changes required

---

## Complexity Driver Analysis (Sequential Thinking — Step 2)

| # | Driver | Lines | CYC Contribution | Extraction Candidate |
|---|---|---|---|---|
| 1 | Tri-clause `\|\|` dispatch lookup guard (`!TryGetValue dispatchId \|\| !TryGetValue ctx \|\| ctx == null`) + timeout inside missing-context block | 135–157 | ~5 | **EXTRACT → `SymmetryGuardResolveDispatchContext`** |
| 2 | Dual-ternary slippage initializers (`tickSize > 0 ?`, `pointValue > 0 ?`) + OR breach predicate + `if (breach)` | 183–200 | ~5 | **EXTRACT → `SymmetryGuardEvaluateSlippage`** |
| 3 | `if (pos.BracketSubmitted)` outer fork + `&&`-compound anchor-aligned check + `if (alreadyAnchored)` inner branch | 208–233 | ~4 | **RETAIN inline** — parent CYC ≤8 after extractions 1+2 |

---

## Extraction Plan

### Helper 1: `SymmetryGuardResolveDispatchContext`

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool SymmetryGuardResolveDispatchContext(
    string fleetEntryName,
    PendingFollowerFill pending,
    DateTime nowUtc,
    out SymmetryDispatchContext ctx,
    out bool timedOut
)
```

**Responsibility:** Encapsulates the tri-clause ConcurrentDictionary lookup and the no-context timeout-skip guard. Returns `true` if a valid context was resolved; returns `false` if the caller should wait or skip based on `timedOut`.

**Extracted Logic (lines 135–157):**
```csharp
ctx = null;
timedOut = false;
if (
    !symmetryFleetEntryToDispatch.TryGetValue(fleetEntryName, out var dispatchId)
    || !symmetryDispatchById.TryGetValue(dispatchId, out ctx)
    || ctx == null
)
{
    if (nowUtc - pending.QueuedUtc >= SymmetryAnchorWait)
    {
        SymmetryGuardSkipFollower(fleetEntryName, pos, pending.FleetFillPrice, 0, 0, "Missing dispatch context");
        timedOut = true;
    }
    return false;
}
return true;
```

**CYC Breakdown:**
- Base: 1
- Tri-OR guard (`||`, `||`): +3
- Timeout check: +1
- **Total: 5 ≤ 8 ✓**

**Inlining:** `AggressiveInlining` — this is on the fill callback hot path (called from `SymmetryGuardOnFollowerFill` on every fleet fill).

---

### Helper 2: `SymmetryGuardEvaluateSlippage`

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool SymmetryGuardEvaluateSlippage(
    string fleetEntryName,
    PositionInfo pos,
    PendingFollowerFill pending,
    double masterAnchor,
    out double slippageTicks,
    out double slippageUsdPerContract
)
```

**Responsibility:** Computes slippage in ticks and USD-per-contract from the fill-vs-anchor delta, then evaluates the dual-threshold breach predicate. Returns `true` if no breach (safe to proceed); returns `false` if breach was detected and `SymmetryGuardSkipFollower` has been called internally.

**Extracted Logic (lines 183–200):**
```csharp
double slippagePoints = Math.Abs(pending.FleetFillPrice - masterAnchor);
slippageTicks = tickSize > 0 ? slippagePoints / tickSize : 0.0;
slippageUsdPerContract = pointValue > 0 ? slippagePoints * pointValue : 0.0;

bool breach =
    slippageTicks > SymmetryMaxSlippageTicks
    || slippageUsdPerContract > SymmetryMaxSlippageUsdPerContract;
if (breach)
{
    SymmetryGuardSkipFollower(
        fleetEntryName, pos, pending.FleetFillPrice,
        slippageTicks, slippageUsdPerContract,
        string.Format("Slippage Buffer breach vs Master {0:F2}", masterAnchor)
    );
    return false;
}
return true;
```

**CYC Breakdown:**
- Base: 1
- `tickSize > 0 ?` ternary: +1
- `pointValue > 0 ?` ternary: +1
- `||` in breach predicate: +1
- `if (breach)`: +1
- **Total: 5 ≤ 8 ✓**

**Inlining:** `AggressiveInlining` — slippage computation is on the fill-callback hot path.

---

## Post-Extraction Parent CYC Projection (Sequential Thinking — Step 4)

**Residual branches in `SymmetryGuardTryResolveFollower` after extractions:**

| Branch | +CYC |
|---|---|
| Base | 1 |
| `if (!SymmetryGuardResolveDispatchContext(...))` | +1 |
| `if (!isResolved)` | +1 |
| `if (nowUtc - pending.QueuedUtc >= SymmetryAnchorWait)` inside `!isResolved` | +1 |
| `if (!SymmetryGuardEvaluateSlippage(...))` | +1 |
| `if (pos.BracketSubmitted)` | +1 |
| `&&` in `alreadyAnchored` compound | +1 |
| `if (alreadyAnchored)` | +1 |
| **Parent total** | **8 ≤ 8 ✓** |

---

## Extraction Table (Summary)

| # | Helper Name | File | Extracted Lines | Extracted CYC | Target CYC | Inlining |
|---|---|---|---|---|---|---|
| 1 | `SymmetryGuardResolveDispatchContext` | `src/V12_002.Symmetry.Follower.cs` (same partial class) | 135–157 | 5 | ≤ 5 | `AggressiveInlining` |
| 2 | `SymmetryGuardEvaluateSlippage` | `src/V12_002.Symmetry.Follower.cs` (same partial class) | 183–200 | 5 | ≤ 5 | `AggressiveInlining` |
| — | **Parent (post-extraction)** | `src/V12_002.Symmetry.Follower.cs` | 129–246 (thinned) | — | **≤ 8** | — |

**max_cyc_projected: 8**

---

## Jane Street KB Compliance

| Rule | Application | Status |
|---|---|---|
| **carl_cook: zero-alloc hot path** | Helpers use `out` params (no heap allocation); slippage uses `double` arithmetics — no boxing | ✓ PASS |
| **carl_cook: AggressiveInlining hot / NoInlining cold** | Both helpers annotated `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — they sit on the fill callback hot path | ✓ PASS |
| **carl_cook: avoid LINQ** | No LINQ in extracted or residual code — pure arithmetic and `TryGetValue` dictionary ops | ✓ PASS |
| **gjengset: no new lock() blocks** | Dispatch state uses `ConcurrentDictionary` (lock-free); `AnchorSnapshot` published via `Interlocked.CompareExchange` (ADR-019) | ✓ PASS |
| **gjengset: volatile reads for shared flags** | `snapshot.IsResolved` read from an immutable `AnchorSnapshot` struct obtained atomically — no volatile wrapping needed | ✓ PASS |
| **trading_billions: single responsibility per helper** | Helper 1: context resolution only. Helper 2: slippage evaluation only. | ✓ PASS |
| **trading_billions: each helper CYC ≤ 8** | Helper 1 = 5, Helper 2 = 5, Parent = 8 | ✓ PASS |

---

## Callers — No Signature Change Required

| Caller | File | Line | Impact |
|---|---|---|---|
| `SymmetryGuardOnFollowerFill` | `src/V12_002.Symmetry.Follower.cs` | 17 | None — parent signature unchanged |
| `SymmetryGuardProcessPendingFollowerFills` | `src/V12_002.Symmetry.Follower.cs` | 97 | None — parent signature unchanged |
| `SymmetryGuardTryResolveFollowersForDispatch` | `src/V12_002.Symmetry.Replace.cs` | 187 | None — parent signature unchanged |

Extraction is **internal-only**: no public or internal interface changes. V12.23 No Scope Creep Protocol: PASS.

---

## Implementation Checklist (for Phase 5 / v12-engineer)

- [ ] Add `SymmetryGuardResolveDispatchContext` as private method in `src/V12_002.Symmetry.Follower.cs`
- [ ] Add `SymmetryGuardEvaluateSlippage` as private method in `src/V12_002.Symmetry.Follower.cs`
- [ ] Refactor `SymmetryGuardTryResolveFollower` body to call both helpers
- [ ] Verify slippage `out` params (`slippageTicks`, `slippageUsdPerContract`) are forwarded to the existing `Print` call at method end
- [ ] Run `dotnet csharpier format src/` after edit
- [ ] Run `powershell -File .\scripts\build_readiness.ps1` — zero errors required
- [ ] Run `python scripts/complexity_audit.py` — verify parent CYC ≤ 8, helpers ≤ 8
- [ ] Run `powershell -File .\deploy-sync.ps1` to re-sync NinjaTrader hard links

---

## Sequential Thinking Evidence

| Thought | Focus | Conclusion |
|---|---|---|
| 1 (probe) | Initialization | Jane Street rules confirmed, discovery approach set |
| 2 | Complexity driver analysis | 3 clusters identified: A(5 CYC), B(5 CYC), C(4 CYC) |
| 3 | Extraction strategy | 2-helper plan with out-param signatures; no struct alloc needed; inlining rationale confirmed |
| 4 | CYC validation | Parent post-extraction = 8 ✓; Helper 1 = 5 ✓; Helper 2 = 5 ✓; max_cyc_projected = 8 |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-129 |
| **Input artifacts** | `00-hotspots.md`, `01-scope-boundary.md`, `manifest.json` |
| **Output artifact** | `02-architecture-plan.md` |
| **MCP Tools Used** | `resolve_repo`, `get_symbol_source`, `get_call_hierarchy` (depth=2, both), `get_dependency_graph` |
| **Sequential Thinking Steps** | 4 (probe + complexity drivers + extraction strategy + CYC validation) |
| **max_cyc_projected** | 8 |
| **Helpers Designed** | 2 (`SymmetryGuardResolveDispatchContext`, `SymmetryGuardEvaluateSlippage`) |
| **Bobcoins Used** | ~2.5 |
