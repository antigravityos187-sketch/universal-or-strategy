# EPIC-W7-033 — Phase 1: Scope Definition

## Method in Scope

**Single method:** `private void FlattenSinglePosition(string entryName, PositionInfo pos)`

This document establishes the scope boundary for EPIC-W7-033. Exactly one method is targeted for complexity reduction in this epic: `FlattenSinglePosition`. The scope is intentionally constrained to a single method to minimise blast radius, isolate regression risk, and allow incremental phase-gate review before any adjacent code is touched.

---

## Source Location

| Field | Value |
|---|---|
| **Source File** | `src/V12_002.Orders.Management.Flatten.cs` |
| **Method Lines** | 441–557 |
| **Signature** | `private void FlattenSinglePosition(string entryName, PositionInfo pos)` |
| **Class** | `V12_002` (partial) |

---

## Complexity Metrics

| Metric | Value |
|---|---|
| **Current CYC** | 27 |
| **Target CYC** | ≤ 8 |
| **Required Reduction** | −19 (minimum) |
| **Cognitive Complexity** | 34 (jcodemunch confirmed) |
| **LOC** | 116 |
| **Max Nesting Depth** | 4 |
| **Hotspot Score** | 0.91 — ranked #1 in repository |

The current CYC of **27** is 170% above the project threshold of ≤ 10. The Phase 1 target of **≤ 8** is set conservatively below the threshold to leave headroom for future incremental changes without triggering another hotspot intervention.

---

## Callers

Grep of `src/` for `FlattenSinglePosition` returned **2 matches**:

| Match | File | Line | Role |
|---|---|---|---|
| Definition | `src/V12_002.Orders.Management.Flatten.cs` | 441 | Method declaration |
| Call site | `src/V12_002.Orders.Management.Flatten.cs` | 437 | Called from `FlattenFilledMasterPositions` |

**Direct caller count: 1** (`FlattenFilledMasterPositions` — same file, line 437).

The transitive call graph (from Phase 0 hotspot analysis) shows 5 upstream callers reaching this single method through the flatten pipeline:

```
FlattenSinglePosition
  ← FlattenFilledMasterPositions          [Flatten.cs:437]        ← direct
      ← FlattenAll                         [Flatten.cs:326]
          ← FlattenAllApexAccounts         [SIMA.Flatten.cs:43]
              ← UI.IPC.Commands.Fleet      [UI.IPC.Commands.Fleet.cs:171]
              ← UI.Panel.Handlers (Key.F)  [UI.Panel.Handlers.cs:76]
              ← SIMA.Shadow (sync path)    [SIMA.Shadow.cs:344]
```

Despite the wide transitive fan-in, the **scope boundary** is drawn at `FlattenSinglePosition` exclusively. No caller, no callee, and no sibling method crosses this scope boundary in Phase 1.

---

## Why Other Methods Are NOT in Scope

Per project rule **V12.23** (single-method epic gate), each Wave 7 EPIC targets exactly one method per phase cycle. Expanding scope beyond a single method in Phase 1 would:

1. **Violate V12.23** — the single-method epic gate prohibits co-targeting sibling methods (e.g. `FlattenFilledMasterPositions`, `FlattenAll`, `FlattenPositionByName`) in the same phase-1 scope document.
2. **Inflate blast radius** — `FlattenAll` and its callers cover the full fleet flatten pipeline; changes there require separate EPIC tracking and a dedicated hotspot analysis cycle.
3. **Break incremental validation** — phase-gate sign-off requires a single, independently testable diff. Multi-method scope makes the validation step ambiguous.
4. **Obscure regression attribution** — if a defect surfaces post-refactor, a single method in scope means root cause is unambiguous.

Methods explicitly excluded from Phase 1 scope (each would require its own EPIC under V12.23):

| Method | Reason Excluded |
|---|---|
| `FlattenFilledMasterPositions` | Caller — untouched; only call site is `FlattenSinglePosition` invocation at line 437 |
| `FlattenAll` | Two levels up the call chain; separate concern (orchestration across positions) |
| `FlattenPositionByName` | Sibling flatten path; shares `CleanPendingStopReplacement` pattern but separate invocation context |
| `CancelOrderSafe` | Dependency called by the target method; callee refactoring is out of scope |
| `SubmitOrderUnmanaged` | NT8 framework method; not eligible for extraction within this EPIC |

---

## Planned Extractions (Phase 2 Preview)

Three purely structural extractions will reduce the orchestrator to CYC ≈ 8:

| # | New Method | Target Lines | Estimated CYC Reduction |
|---|---|---|---|
| 1 | `CancelTargetOrdersForPosition(entryName, pos)` | 463–478 | −6 |
| 2 | `ResolveAndSubmitFlattenOrder(entryName, pos)` | 481–552 | −11 |
| 3 | `CleanPendingStopReplacement(entryName)` | 456–459 | −2 |

These extractions involve **zero logic changes** — all branch conditions, state reads, and order-of-operations are preserved verbatim inside the extracted helpers. The orchestrator `FlattenSinglePosition` becomes a sequenced delegation chain.

---

## Scope Boundary — Formal Statement

> The **scope boundary** for EPIC-W7-033 Phase 1 is defined as: the body of the **single method** `FlattenSinglePosition` (lines 441–557, `src/V12_002.Orders.Management.Flatten.cs`) and the three new private helper methods to be introduced by extraction in Phase 2. No existing method outside this boundary will be modified, renamed, or have its signature altered.

This scope boundary is immutable for Phases 1–2. Any required changes to callers (e.g. to accommodate a future signature change) must be tracked under a separate EPIC and approved through the V12.23 gate process.

---

## Constraints and Risks

| Constraint | Detail |
|---|---|
| Order of operations | Stop cancel → pending purge → target cancel → qty resolve → submit order must be preserved |
| Concurrency | `pendingStopReplacements` is a `ConcurrentDictionary`; extracted helpers must not introduce additional locking |
| NT8 threading | `SubmitOrderUnmanaged` must remain on the correct NT8 dispatcher thread; extraction must not move it to a lambda or async context |
| Blast radius | HIGH (0.87) — regression testing must cover Key.F hotkey path, IPC FLATTEN command, and SIMA fleet flatten pipeline |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase1-scope |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Epic** | EPIC-W7-033 |
| **Method** | `FlattenSinglePosition` |
| **Source File** | `src/V12_002.Orders.Management.Flatten.cs` |
| **CYC Current** | 27 |
| **CYC Target** | ≤ 8 |
| **Callers (direct)** | 1 (`FlattenFilledMasterPositions`) |
| **Callers (transitive)** | 5 |
| **Scope** | single method |
| **V12.23 Gate** | satisfied |
| **Output** | `docs/brain/EPIC-W7-033/00-scope.md` |
| **Timestamp** | 2025-07-14T00:00:00Z |
