# EPIC-W7-127 — Phase 2: Architecture Plan

**Agent Name:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-127/01-scope-boundary.md

---

## Summary

| Field | Value |
|---|---|
| **Epic ID** | EPIC-W7-127 |
| **Target Method** | `SymmetryGuardOnFollowerFill` |
| **Source File** | `src/V12_002.Symmetry.Follower.cs` |
| **CYC Baseline** | 16 |
| **CYC Target** | <= 8 |
| **max_cyc_projected** | **6** |
| **Parent CYC (after extraction)** | **3** |
| **Helpers Extracted** | 3 |
| **Scope Boundary** | PASS (V12.23) |

---

## MCP Evidence

### get_context_bundle

- **Symbol confirmed:** `src/V12_002.Symmetry.Follower.cs::V12_002.SymmetryGuardOnFollowerFill#method`
- **Lines:** 17–88 (72 lines)
- **Signature:** `private bool SymmetryGuardOnFollowerFill(string fleetEntryName, PositionInfo followerPos, double followerFillPrice)`
- **Source freshness:** fresh
- **Key source facts:**
  - Opens with null/IsFollower guard returning `false` early
  - Sets `followerPos.EntryFilled = true` unconditionally
  - Guard block `if (!followerPos.BracketSubmitted)` wraps a double-map TryGetValue ANCHOR-01 path
  - AnchorSnapshot read via `Interlocked.CompareExchange` (ADR-019 lock-free — must be preserved)
  - Closes with `PendingFollowerFill` construction, `ConcurrentDictionary` write, and conditional TryRemove

### get_call_hierarchy

- **Callers (depth=1):** 0 direct callers indexed (called via event dispatch from `HandleFleetEntryFill`)
- **Callees (depth=1, direct):**
  - `symmetryFleetEntryToDispatch` (ConcurrentDictionary, read)
  - `symmetryDispatchById` (ConcurrentDictionary, read)
  - `SymmetryGuardApplyMasterAnchor` — `src/V12_002.Symmetry.Follower.cs:248`
  - `SymmetryGuardSubmitFollowerBracket` — `src/V12_002.Symmetry.Follower.cs:285`
  - `SymmetryGuardTryResolveFollower` — `src/V12_002.Symmetry.Follower.cs:129`
  - `symmetryPendingFollowerFills` (ConcurrentDictionary, write + conditional remove)
- **Callees (depth=2, transitive):**
  - `SymmetryGuardSkipFollower`, `SymmetryGuardRetargetExistingFollowerBracket`, `ValidateStopPrice`, `Enqueue` (FSM actor), `GetTargetContracts`, `GetTargetOrdersDictionary`
- **Threading note:** Called on strategy thread via actor queue drain; `ConcurrentDictionary` writes visible to REAPER audit thread per ADR-019

### get_dependency_graph

- **File imports/importers:** No resolved import edges in index (C# partial-class architecture — all files compile into one assembly; relationships are symbol-level not file-level)
- **Symbol-level coupling confirmed via call hierarchy:** 9 directly coupled symbols across 6 files

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Drivers

Three CYC concentration zones identified:

| Zone | Lines | Complexity Source | CYC Contribution |
|---|---|---|---|
| **A** | 22–25 | `\|\|` null+flag guard + `RemainingContracts <= 0` init | ~3 |
| **B** | 30–70 | `!BracketSubmitted` block: double TryGetValue `&&`, anchorReady `&&`, preCheckAnchor>0, shouldSubmitImmediately fork | ~10 |
| **C** | 72–88 | PendingFollowerFill ternary + TryResolve conditional remove | ~3 |

Zone B is the dominant contributor (~10 CYC). Extracting all 3 zones reduces parent to CYC=3.

### Thought 2 — Extraction Strategy

Three helpers, each matching a CYC zone, with Jane Street-aligned attributes:

- **Helper 1** (`ValidateAndInitFollowerPos`): hot path null/flag guard + init — `[AggressiveInlining]`
- **Helper 2** (`TryApplyPreCheckAnchorAndSubmit`): ANCHOR-01 double-map + anchor readiness + submit/defer — `[AggressiveInlining]` (hot, called on every fill attempt)
- **Helper 3** (`EnqueueAndTryResolveFollower`): queue construction + write + conditional remove — `[NoInlining]` (queue mutation path, less hot, isolates side-effect from JIT view)

### Thought 3 — CYC Validation

All helpers and parent confirmed <= 8:

| Symbol | CYC Calculation | Projected CYC |
|---|---|---|
| `SymmetryGuardOnFollowerFill` (parent) | 1 + 1 (ValidateAndInit check) + 1 (!BracketSubmitted) | **3** |
| `ValidateAndInitFollowerPos` | 1 + 1 (null \|\|) + 1 (!IsFollower) + 1 (<=0) | **4** |
| `TryApplyPreCheckAnchorAndSubmit` | 1 + 1 (&&TryGetValue1) + 1 (&&TryGetValue2) + 1 (anchorReady) + 1 (>0) + 1 (shouldSubmit) | **6** |
| `EnqueueAndTryResolveFollower` | 1 + 1 (ternary) + 1 (TryResolve if) | **3** |

**max_cyc_projected = 6** ✅ (all <= 8)

---

## Extraction Table

| # | Helper Method | Visibility | Lines Extracted | Projected CYC | JIT Attribute | Returns | Side Effects |
|---|---|---|---|---|---|---|---|
| 1 | `ValidateAndInitFollowerPos(PositionInfo followerPos)` | `private` | 22–25 | 4 | `[AggressiveInlining]` | `bool` | Sets `EntryFilled=true`, may set `RemainingContracts` |
| 2 | `TryApplyPreCheckAnchorAndSubmit(string fleetEntryName, PositionInfo followerPos)` | `private` | 30–70 (interior of !BracketSubmitted block) | 6 | `[AggressiveInlining]` | `void` | Calls `SymmetryGuardApplyMasterAnchor`, `SymmetryGuardSubmitFollowerBracket`, `Print` |
| 3 | `EnqueueAndTryResolveFollower(string fleetEntryName, PositionInfo followerPos, double followerFillPrice)` | `private` | 72–88 | 3 | `[NoInlining]` | `void` | Writes to `symmetryPendingFollowerFills`, calls `SymmetryGuardTryResolveFollower`, calls `TryRemove` |

---

## Method Signatures

### Parent (post-extraction)

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool SymmetryGuardOnFollowerFill(
    string fleetEntryName,
    PositionInfo followerPos,
    double followerFillPrice
)
// CYC projected: 3
```

### Helper 1

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool ValidateAndInitFollowerPos(PositionInfo followerPos)
// CYC projected: 4
// Responsibility: null/flag guard + EntryFilled + RemainingContracts init
```

### Helper 2

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private void TryApplyPreCheckAnchorAndSubmit(
    string fleetEntryName,
    PositionInfo followerPos
)
// CYC projected: 6
// Responsibility: ANCHOR-01 double-map lookup + AnchorSnapshot read (ADR-019 lock-free)
//                 + anchor readiness check + SubmitFollowerBracket or defer Print
```

### Helper 3

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void EnqueueAndTryResolveFollower(
    string fleetEntryName,
    PositionInfo followerPos,
    double followerFillPrice
)
// CYC projected: 3
// Responsibility: PendingFollowerFill construction + ConcurrentDictionary write
//                 + TryResolveFollower + conditional TryRemove
```

---

## Post-Extraction Parent Body (Pseudocode)

```csharp
private bool SymmetryGuardOnFollowerFill(
    string fleetEntryName,
    PositionInfo followerPos,
    double followerFillPrice
)
{
    if (!ValidateAndInitFollowerPos(followerPos))
        return false;

    if (!followerPos.BracketSubmitted)
        TryApplyPreCheckAnchorAndSubmit(fleetEntryName, followerPos);

    EnqueueAndTryResolveFollower(fleetEntryName, followerPos, followerFillPrice);

    return true;
}
```

---

## V12 DNA Compliance

| Rule | Status | Evidence |
|---|---|---|
| No `lock()` blocks added | PASS | All extractions preserve ConcurrentDictionary lock-free writes |
| `AggressiveInlining` on hot helpers | PASS | Helpers 1 and 2 on hot fill path |
| `NoInlining` on cold helpers | PASS | Helper 3 (queue mutation, less hot) |
| No LINQ | PASS | Source contains no LINQ; extractions introduce none |
| All helpers CYC <= 8 | PASS | Max projected = 6 (Helper 2) |
| Parent CYC <= 8 | PASS | Parent projected = 3 |
| ADR-019 lock-free AnchorSnapshot preserved | PASS | AnchorSnapshot read via `Interlocked.CompareExchange` stays in Helper 2 |
| V12.23 No Scope Creep | PASS | 3 private helpers, same file, no caller modification |
| ASCII-only | PASS | No Unicode/emoji in any proposed string literal |
| Caller signature unchanged | PASS | `HandleFleetEntryFill` call site unaffected |

---

## Risk Register

| Risk | Severity | Mitigation |
|---|---|---|
| Temporal coupling between Helper 2 (sets state on `followerPos`) and Helper 3 (reads `followerPos.EntryPrice`) | Medium | Preserve call order: Helper 2 BEFORE Helper 3 in parent; document ordering constraint |
| `SymmetryGuardTryResolveFollower` (downstream, CYC~9) triggers same shared state | Low | Out of scope per 00-hotspots.md — do NOT modify |
| `shouldSubmitImmediately` local var must move into Helper 2 as local | Low | Resolved by making Helper 2 own the `bool shouldSubmitImmediately` flag internally |
| REAPER audit thread visibility of `symmetryPendingFollowerFills` write | Low | ConcurrentDictionary write in Helper 3 preserves lock-free semantics; no change to ordering |

---

## Files Touched

| File | Change Type |
|---|---|
| `src/V12_002.Symmetry.Follower.cs` | Modify — extract 3 private helpers, simplify parent body |

No other files are modified. No interface, signature, or cross-file changes.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Phase** | 2 |
| **Wave** | 7 |
| **Bobcoins Used** | 1.0 |
| **MCP Tools Used** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph |
| **Sequential Thinking Steps** | 4 (1 probe + 3 analysis) |
| **max_cyc_projected** | 6 |
| **parent_cyc_projected** | 3 |
| **CYC Reduction** | 16 → 3 (parent), 13 points reduced |
