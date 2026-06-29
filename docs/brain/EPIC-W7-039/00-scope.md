# EPIC-W7-039 — Phase 1: Scope Definition

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-039 |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Method** | `ManageTrailingStops` |
| **CYC (current)** | 13 |
| **CYC (target)** | ≤ 8 |
| **Source File** | `src/V12_002.Trailing.cs` |
| **Reported** | 2025-05-27 |
| **Artifact Version** | REDO (V3.0) |

---

## Single Method in Scope

This epic targets a **single method**: [`ManageTrailingStops()`](src/V12_002.Trailing.cs:39), defined at line 39 of [`src/V12_002.Trailing.cs`](src/V12_002.Trailing.cs).

The scope boundary is precisely drawn around that one method. All analysis, decomposition planning, extraction implementation, and validation work in subsequent phases exists solely to reduce the cyclomatic complexity of `ManageTrailingStops` from its current CYC **13** to the target CYC **≤ 8**, without altering observable behaviour on any execution path.

No other methods — regardless of CYC score, blast-radius proximity, or co-location in the same file — fall inside the scope boundary for this epic.

---

## Caller Count and Call Sites

`grep` over `src/` identified **1 direct runtime caller** of `ManageTrailingStops`:

| File | Line | Nature |
|---|---|---|
| [`src/V12_002.BarUpdate.cs`](src/V12_002.BarUpdate.cs:327) | 327 | `Enqueue(ctx => ctx.ManageTrailingStops())` — enqueued on every tick when `activePositions.Count > 0` |

Three additional references appear in the codebase, but none of them *call* the method:

| File | Line | Nature |
|---|---|---|
| [`src/V12_002.Trailing.Breakeven.cs`](src/V12_002.Trailing.Breakeven.cs:115) | 115 | Comment only — describes expected behaviour of the method |
| [`src/V12_002.UI.Callbacks.cs`](src/V12_002.UI.Callbacks.cs:1229) | 1229 | Comment only — notes interaction with the method's armed path |
| [`src/V12_002.SIMA.Shadow.cs`](src/V12_002.SIMA.Shadow.cs:15) | 15 | XML doc comment — describes the method as the caller of `ShadowEngineCheck` |
| [`src/V12_002.Orders.Callbacks.Execution.cs`](src/V12_002.Orders.Callbacks.Execution.cs:628) | 628 | Comment only — distinguishes steady-state trailing (this method) from immediate fill-callback path |

**Total direct runtime callers: 1** (`V12_002.BarUpdate.cs:327`).

The single call site is an enqueue-lambda — this means all refactoring work happens exclusively inside the method body and its direct helpers; the call site signature is unchanged by any extraction.

---

## Why Other Methods Are NOT in Scope

The V12.23 rule governs this epic's scope containment: each epic targets one method, one CYC reduction goal, and one source unit. Introducing additional methods into scope — even high-CYC co-inhabitants of the same file such as `UpdateStopOrder` (CYC ~18, [`src/V12_002.Trailing.StopUpdate.cs`](src/V12_002.Trailing.StopUpdate.cs)) or `ManageTrail_ApplyPointBasedCascade` (CYC ~7) — would:

1. **Violate the single-method constraint of V12.23.** The V12.23 discipline requires that each epic touch exactly one primary method as its unit of work. Widening scope mid-epic breaks the traceability between the epic identifier, the complexity metric, and the resulting artifact.
2. **Expand the blast radius beyond what is assessed.** Phase 0 (hotspot analysis) sized the risk at HIGH for `ManageTrailingStops` specifically. Adding `UpdateStopOrder` would introduce a second HIGH-blast-radius method whose threading and order-sequencing risks have not been assessed under this epic.
3. **Invalidate the CYC target metric.** The target CYC ≤ 8 was calibrated for the orchestrator method only. A combined scope would require a separate CYC target negotiation and a new hotspot artifact.
4. **Risk regression in live-order infrastructure.** `UpdateStopOrder` is shared by 6 cross-file dependents (`V12_002.Trailing.StopUpdate.cs`, `V12_002.Symmetry.Replace.cs`, `V12_002.Orders.Callbacks.Propagation.cs`, `V12_002.UI.IPC.Commands.Mode.cs`, plus fill-callback paths). Touching it under EPIC-W7-039 would mix two distinct risk profiles under a single validation gate.

Therefore, the scope boundary is fixed: **only `ManageTrailingStops` in `src/V12_002.Trailing.cs`**. All helper methods introduced or reorganised during extractions (Phase 2) are considered implementation artefacts of this single method's decomposition, not independently scoped targets.

---

## CYC Reduction Path

| Stage | CYC | Method |
|---|---|---|
| Current (baseline) | **13** | `ManageTrailingStops` as-is |
| Post Phase 2 (target) | **≤ 8** | `ManageTrailingStops` after 3 planned extractions |
| Projected post-extraction (optimistic) | **5–6** | Per Phase 0 sequential thinking analysis |

The three planned extractions from Phase 0 are:

1. **EMA handler file** — Move `TrailHandler_TREND_E1/E2` and `TrailHandler_RETEST` into `V12_002.Trailing.EMAHandlers.cs`; `ManageTrail_RunPerTradeBranches` becomes a thin dispatcher (CYC ≤ 4).
2. **Throttle value-object** — Encapsulate `ManageTrail_AdaptiveThrottleTick` and all throttle/circuit-breaker state fields in a `TrailingThrottle` struct; orchestrator call site becomes `if (!_throttle.ShouldProcess()) return;` (zero CYC contribution).
3. **Fleet sync file** — Relocate `ManageTrail_RunFleetSymmetrySync`, `FleetSync_FindLeaderMaxLevels`, and `FleetSync_SyncFollowersToLevel` into `V12_002.Trailing.FleetSync.cs`; only the `if (EnableSIMA)` guard remains in the orchestrator (CYC contribution = 1).

---

## Risk Summary (Inherited from Phase 0)

| Risk | Severity | Note |
|---|---|---|
| Threading — dual `activePositions.ToArray()` snapshots (main loop + fleet sync) | HIGH | Extractions must not merge snapshots; pre-existing race must not be widened |
| Stop-order call ordering — `UpdateStopOrder` called from 5 paths via orchestrator | HIGH | Fleet sync must continue to run *after* the main loop |
| Blast radius — fill-callback path shares `UpdateStopOrder` | MEDIUM | Phase 3 validation must cover the `V12_002.Orders.Callbacks.Execution.cs:628` path |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase1-scope |
| **Epic** | EPIC-W7-039 |
| **Wave** | 7 |
| **Phase** | 1 |
| **Artifact Version** | REDO (V3.0) |
| **Bobcoins Used** | 6 |
| **Execution Time** | ~18 seconds |
| **Tools Used** | `read_file` ×2, `grep` ×1, `write_file` ×1, `apply_diff` ×1 |
| **Status** | ✅ Completed |
