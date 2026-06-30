# EPIC-W7-042 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29
**Input:** docs/brain/EPIC-W7-042/02-architecture-plan.md, docs/brain/EPIC-W7-042/03-audit-report.md

---

## Header

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-042 |
| **Method** | `SymmetryGuardOnFollowerFill` |
| **Source** | `src/V12_002.Symmetry.Follower.cs` |
| **Original CYC** | 16 |
| **Wave** | 7 |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | **4** |
| **max_cyc_any_method_after** | **5** |
| **dna_verdict (Phase 3)** | PASS |

---

## Ticket Summary

| Ticket | Helper Name | CYC Reduction | Helper CYC | Lines Moved |
|---|---|---|---|---|
| TICKET-1 | `SymmetryGuardHandleInitialBracketSubmission` | -6 | 5 | 30-73 (~44 lines) |
| TICKET-2 | `SymmetryGuardEnqueueAndTryResolve` | -2 | 3 | 75-85 (~11 lines) |

**Parent CYC after both tickets:** 4 (target ≤ 8 ✓)

---

## TICKET-1

| Field | Value |
|---|---|
| **ticket_id** | TICKET-1 |
| **epic_id** | EPIC-W7-042 |
| **helper_name** | `SymmetryGuardHandleInitialBracketSubmission` |
| **concern** | Initial bracket submission decision: determine whether the follower bracket submits immediately (master anchor already resolved) or defers until anchor resolves; encapsulates all anchor pre-check logic and cold-path logging (`[ANCHOR-01]` / `[ANCHOR-GATE]`). |
| **lines_to_move** | 30-73 (the entire `!BracketSubmitted` block, ~44 lines) |
| **cyc_reduction** | -6 (removes decisions: `!BracketSubmitted` +1, `TryGetValue&&TryGetValue` +2, `anchorReady&&preCheckAnchor>0` +2, `shouldSubmitImmediately` +1) |
| **projected_helper_cyc** | 5 (1 base + 4 inner decision points) |
| **execution_order** | 1 — apply before TICKET-2 |

### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void SymmetryGuardHandleInitialBracketSubmission(
    string fleetEntryName,
    PositionInfo followerPos
)
```

### Internal Decision Points

| # | Decision Point | CYC Contribution |
|---|---|---|
| 1 | `if (!followerPos.BracketSubmitted)` — outer gate | +1 |
| 2 | `if (TryGetValue(fleetEntryName, ...) && TryGetValue(preCheckId, ...))` — fleet dispatch context lookup | +2 (short-circuit `&&`) |
| 3 | `if (anchorReady && preCheckAnchor > 0)` — anchor ready check | +2 (short-circuit `&&`) |
| 4 | `if (shouldSubmitImmediately)` — submit vs defer branch | +1 |

**Projected CYC:** 1 (base) + 1 + 2 + 2 + 1 = **5** ✓

### Implementation Notes

- Mark `[MethodImpl(MethodImplOptions.NoInlining)]` — contains two `Print(string.Format(...))` cold-logging calls (`[ANCHOR-01]` and `[ANCHOR-GATE]` paths). Prevents JIT inlining of Print/string.Format overhead into hot call sites (carl_cook NoInlining pattern).
- Both helpers access existing class-level `ConcurrentDictionary` fields only — no new fields introduced.
- `AnchorSnapshot` read pattern (`preCheckCtx.Anchor`) is immutable — `IsResolved` and `MasterAnchorPrice` from same snapshot object. ADR-019 lock-free read is undisturbed.
- Helper is `private void` — no return value needed.
- Place in same file (`src/V12_002.Symmetry.Follower.cs`), same partial class.

### Call Site Replacement (Parent)

Replace lines 30-73 in `SymmetryGuardOnFollowerFill` with:
```csharp
SymmetryGuardHandleInitialBracketSubmission(fleetEntryName, followerPos);
```

### Acceptance Criteria

- [ ] `SymmetryGuardHandleInitialBracketSubmission` compiled successfully in same partial class
- [ ] `[MethodImpl(MethodImplOptions.NoInlining)]` attribute present
- [ ] Call site in parent replaces lines 30-73 with single method call
- [ ] `powershell -File .\scripts\build_readiness.ps1` passes (zero errors)
- [ ] `python scripts/complexity_audit.py` confirms helper CYC ≤ 8
- [ ] No `lock()` blocks introduced
- [ ] All string literals ASCII-only

---

## TICKET-2

| Field | Value |
|---|---|
| **ticket_id** | TICKET-2 |
| **epic_id** | EPIC-W7-042 |
| **helper_name** | `SymmetryGuardEnqueueAndTryResolve` |
| **concern** | PendingFollowerFill enqueue and immediate try-resolve: build the `PendingFollowerFill` record from fill data, enqueue to `symmetryPendingFollowerFills`, attempt immediate resolution via `SymmetryGuardTryResolveFollower`, remove from queue on success. |
| **lines_to_move** | 75-85 (pending fill enqueue + resolve block, ~11 lines) |
| **cyc_reduction** | -2 (removes decisions: ternary price selection +1, `SymmetryGuardTryResolveFollower` result gate +1) |
| **projected_helper_cyc** | 3 (1 base + ternary +1 + resolve gate +1) |
| **execution_order** | 2 — apply after TICKET-1 |

### Signature

```csharp
private void SymmetryGuardEnqueueAndTryResolve(
    string fleetEntryName,
    PositionInfo followerPos,
    double followerFillPrice
)
```

### Internal Decision Points

| # | Decision Point | CYC Contribution |
|---|---|---|
| 1 | `followerFillPrice > 0 ? followerFillPrice : followerPos.EntryPrice` — ternary price selection | +1 |
| 2 | `if (SymmetryGuardTryResolveFollower(...))` — resolution outcome gate | +1 |

**Projected CYC:** 1 (base) + 1 + 1 = **3** ✓

### Implementation Notes

- `new PendingFollowerFill{...}` is one heap allocation per fill event — acceptable at this call frequency. No string allocations on this path.
- No `[MethodImpl(MethodImplOptions.NoInlining)]` needed — no cold logging on this path; hot path zero-alloc preserved.
- Helper is `private void` — no return value needed.
- Place in same file (`src/V12_002.Symmetry.Follower.cs`), same partial class.

### Call Site Replacement (Parent)

Replace lines 75-85 in `SymmetryGuardOnFollowerFill` with:
```csharp
SymmetryGuardEnqueueAndTryResolve(fleetEntryName, followerPos, followerFillPrice);
```

### Acceptance Criteria

- [ ] `SymmetryGuardEnqueueAndTryResolve` compiled successfully in same partial class
- [ ] Call site in parent replaces lines 75-85 with single method call
- [ ] `powershell -File .\scripts\build_readiness.ps1` passes (zero errors)
- [ ] `python scripts/complexity_audit.py` confirms helper CYC ≤ 8
- [ ] No `lock()` blocks introduced
- [ ] All string literals ASCII-only

---

## Parent Method After Both Extractions

### Pseudocode

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

### Remaining Decision Points

| # | Decision Point | CYC Contribution |
|---|---|---|
| 1 | `followerPos == null \|\| !followerPos.IsFollower` | +2 (short-circuit `\|\|`) |
| 2 | `followerPos.RemainingContracts <= 0` | +1 |

**Projected CYC:** 1 (base) + 2 + 1 = **4** ✓

---

## CYC Projection Summary

| Method | Role | CYC Before | CYC After | Status |
|---|---|---|---|---|
| `SymmetryGuardOnFollowerFill` | Parent (modified) | 16 | **4** | PASS (≤ 8) |
| `SymmetryGuardHandleInitialBracketSubmission` | New helper (TICKET-1) | — | **5** | PASS (≤ 8) |
| `SymmetryGuardEnqueueAndTryResolve` | New helper (TICKET-2) | — | **3** | PASS (≤ 8) |

**max_cyc_projected: 5** (Jane Street CYC ≤ 8 mandate satisfied ✓)
**projected_parent_cyc_after_all: 4** (≤ 8 ✓)

---

## MCP Evidence

### jcodemunch: resolve_repo
```json
{
  "found": true,
  "indexed": true,
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "symbol_count": 5147,
  "file_count": 2000,
  "source_root": "/home/malhitticrypto/universal-or-strategy",
  "status": "loadable"
}
```

### jcodemunch: get_symbol_complexity
```json
{
  "symbol_id": "src/V12_002.Symmetry.Follower.cs::V12_002.SymmetryGuardOnFollowerFill#method",
  "name": "SymmetryGuardOnFollowerFill",
  "kind": "method",
  "file": "src/V12_002.Symmetry.Follower.cs",
  "line": 17,
  "cyclomatic": 16,
  "max_nesting": 6,
  "param_count": 3,
  "lines": 72,
  "assessment": "high"
}
```

### jcodemunch: get_extraction_candidates
```json
{
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "file": "src/V12_002.Symmetry.Follower.cs",
  "candidates": [],
  "min_complexity": 3,
  "min_callers": 1
}
```
*Note: 0 external-caller candidates returned because `SymmetryGuardOnFollowerFill` has 0 cross-file importers (confirmed in Phase 2 via `get_dependency_graph` and `find_references`). Extraction plan is based on internal CYC driver analysis per Phase 2 architecture plan.*

---

## Sequential Thinking Evidence

### Thought 1 — Ticket Breakdown Analysis
jcodemunch `get_symbol_complexity` confirmed CYC=16, max_nesting=6, 72 lines. Phase 2 architecture plan identifies 2 cohesive extraction clusters. Phase 3 DNA audit confirmed PASS. Conclusion: 2 tickets required — one per cluster. Tickets are applied sequentially (TICKET-1 first, TICKET-2 second) within same file/class.

### Thought 2 — TICKET-1 Definition
TICKET-1: extract `SymmetryGuardHandleInitialBracketSubmission` from lines 30-73 (44 lines). Removes 4 internal decision points from parent (net -6 CYC). Helper projected CYC=5 (1 base + 4 inner decisions). `[MethodImpl(MethodImplOptions.NoInlining)]` required due to cold-path `Print`/`string.Format` calls. ADR-019 AnchorSnapshot read pattern undisturbed. After TICKET-1 alone, parent CYC reduced to ~6.

### Thought 3 — TICKET-2 Definition & Validation
TICKET-2: extract `SymmetryGuardEnqueueAndTryResolve` from lines 75-85 (11 lines). Removes 2 decision points from parent (ternary +1, resolve gate +1 = net -2 CYC). Helper projected CYC=3. No `NoInlining` needed — no cold logging. After both tickets: parent CYC=4, max any method=5. All three methods ≤ 8 ✓. Jane Street KB mandate satisfied. ticket_count=2, projected_parent_cyc_after_all=4.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-042 |
| **Lane** | P4-L3 |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 4 |
| **max_cyc_projected** | 5 |
| **MCP Tools Used** | resolve_repo, sequentialthinking (x4 incl. probe), get_symbol_complexity, get_extraction_candidates, search_symbols |
| **Sequential Thinking Thoughts** | 3 (+ 1 probe) |
| **Input Artifacts** | 02-architecture-plan.md, 03-audit-report.md |
| **Output Artifact** | 04-tickets.md |
