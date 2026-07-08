# EPIC-W7-032 — Phase 1: Scope Definition

| Field | Value |
|---|---|
| **Epic ID** | EPIC-W7-032 |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Status** | Completed |

---

## 1 · Single Method in Scope

This epic targets a **single method**: `RestoreCascadedTargets`.

| Attribute | Value |
|---|---|
| **Method** | `RestoreCascadedTargets` |
| **File** | `src/V12_002.Orders.Management.StopSync.cs` |
| **Definition line** | 981 |
| **Current CYC** | 23 |
| **Target CYC** | ≤ 8 (parent orchestrator after extraction) |
| **Required reduction** | −15 cyclomatic complexity points |

The scope boundary for this epic is drawn precisely at the method signature of
`RestoreCascadedTargets` in `src/V12_002.Orders.Management.StopSync.cs`. No other
method, file, or subsystem falls within the Phase 1–3 refactor scope.

---

## 2 · Scope Boundary

The **scope boundary** is defined as follows:

- **In scope:** The body of `RestoreCascadedTargets` (lines 981–1098,
  `src/V12_002.Orders.Management.StopSync.cs`) and the five private helper methods
  that will be extracted from it during Phase 3 execution. All helpers will live in
  the same file as the parent method; no new files are created.

- **Out of scope:** Every caller file, every consumer of `GetTargetOrdersDictionary`,
  all other methods in `V12_002.Orders.Management.StopSync.cs`, and all methods in the
  broader V12.23 build. The three caller files listed in §3 below are read-only
  reference points — they are **not modified** during any phase of this epic.

---

## 3 · Callers (Read-Only Reference)

`RestoreCascadedTargets` is dispatched exclusively through `TriggerCustomEvent` at
**3 external call-sites** across 3 files. These are confirmed by `grep` on the full
`src/` tree and are consistent with the blast-radius evidence in `00-hotspots.md`.

| # | Caller file | Line | Context |
|---|---|---|---|
| 1 | `src/V12_002.Orders.Callbacks.cs` | 715 | Stop-replacement OCO callback |
| 2 | `src/V12_002.Orders.Callbacks.AccountOrders.cs` | 749 | Account-level order state callback |
| 3 | `src/V12_002.Trailing.StopUpdate.cs` | 74 | Trailing-stop update path |

**Callers count: 3**

Because all call-sites use `TriggerCustomEvent(o => RestoreCascadedTargets(...), null)`,
the dispatch contract is unchanged by any internal refactor. The callers do not need to
be modified to achieve the CYC ≤ 8 target.

---

## 4 · Why Other Methods Are NOT in Scope

The V12.23 build (`src/V12_002.Orders.Management.StopSync.cs`) contains multiple
methods that also carry measurable cyclomatic complexity, including
`RefreshActivePositionOrders`, `UpdateStopQuantity`, `CreateNewStopOrder`, and
`ValidateStopPrice` (noted in the file's line-1 build comment as a future refactor
candidate group exceeding 400 lines).

These methods are excluded from EPIC-W7-032 for the following reasons:

1. **Wave-7 prioritisation** — Wave 7 targets the single highest-CYC method first.
   `RestoreCascadedTargets` at CYC 23 is the confirmed hotspot from Phase 0 analysis.
   No other method in the file has been measured at or above CYC 23 in the current
   build.

2. **Blast-radius containment** — Scoping to a single method limits the change surface
   during Phase 3 execution. Multi-method refactors in the same file raise the risk of
   merge conflicts and unintended side-effects across the 14 indirect consumers of
   `GetTargetOrdersDictionary`.

3. **Epic atomicity** — Each Wave-7 epic is designed to be independently reviewable
   and deployable. Bundling additional methods into EPIC-W7-032 would violate the
   single-method epic atomicity contract established in the Wave-7 planning brief.

4. **V12.23 build constraint** — The V12.23 build comment on line 1 of the source
   file explicitly flags the full method group as a *future* refactor candidate, not
   a current one. Acting on that group now would pre-empt a future planned epic and
   introduce unreviewed scope expansion.

---

## 5 · Complexity Reduction Plan (Summary)

The CYC 23 → ≤ 8 reduction will be achieved by extracting five private helpers as
identified in `00-hotspots.md §3`:

| Helper | Responsibility | Est. CYC |
|---|---|---|
| `ValidateRestorePreConditions` | Null-array + no-position + not-filled guards | 4 |
| `ShouldRestoreTarget` | Per-snapshot Cancelled/Rejected state filter | 3 |
| `SubmitFollowerTarget` | Fleet path: `CreateOrder` + null-check + `Submit` | 5 |
| `SubmitLocalTarget` | Local path: `SubmitOrderUnmanaged` Long/Short fork | 4 |
| `RegisterRestoredTarget` | Dict write + Print fork | 4 |

Post-extraction, the parent `RestoreCascadedTargets` becomes a thin orchestrator with
estimated CYC **6**, satisfying the ≤ 8 target. Every helper also satisfies ≤ 8
individually. The full extraction plan and correctness constraints are documented in
`00-hotspots.md §3` and `§5 Thought 3`.

---

## 6 · Agent Tracking

```
Agent Name:       v12-phase1-scope
Bobcoins Used:    3
Execution Time:   ~18s
Analysis Method:  00-hotspots.md review + manifest.json read + grep src/ for callers
```
