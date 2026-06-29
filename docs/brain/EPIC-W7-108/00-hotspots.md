# EPIC-W7-108 — Phase 0: Hotspot Analysis

## Method Name

`DrainPhotonQueuesOnShutdown`

## CYC (Cyclomatic Complexity)

**0 — Method does not exist as a standalone symbol.**

> **⚠️ REQUIRES MANUAL REVIEW**
>
> The method `DrainPhotonQueuesOnShutdown` could not be located as a named symbol anywhere in
> `src/V12_002.SIMA.Lifecycle.cs` or any other `.cs` file in the repository. A codebase-wide
> grep confirms zero occurrences of the identifier as a method definition.
>
> **Root cause (confirmed):** The drain logic exists but is **inlined** inside
> `ProcessShutdownSIMA` (lines 104–136 of `src/V12_002.SIMA.Lifecycle.cs`). The extraction of
> `DrainPhotonQueuesOnShutdown` from `ProcessShutdownSIMA` was the subject of EPIC-CCN-114 and
> was documented as complete in `docs/brain-vm-backup/EPIC-CCN-114/`, but the actual source file
> does **not** contain the extracted method — the code was never committed, or the commit was
> subsequently reverted / merged back inline.
>
> **Duplicate epic note:** EPIC-W7-055 targets the identical method name with CYC=8 (derived from
> static branch analysis of the inline drain blocks in `ProcessShutdownSIMA`) and has a completed
> Phase 0 artifact at `docs/brain/EPIC-W7-055/00-hotspots.md`. EPIC-W7-108 appears in the Wave 7
> epic list with `"cyc": 0` and an unqualified source path (`V12_002.SIMA.Lifecycle.cs`, missing
> the `src/` prefix), both of which are consistent with a data-entry anomaly or an entry that was
> generated before the planned extraction was executed.

## File Path

Recorded source: `V12_002.SIMA.Lifecycle.cs` (Wave 7 epic list — missing `src/` prefix)
Canonical source: `src/V12_002.SIMA.Lifecycle.cs`

Implementation location (inline, not yet extracted):
`src/V12_002.SIMA.Lifecycle.cs` — lines 104–136 (`ProcessShutdownSIMA`, drain blocks)

## Blast Radius Summary

Because the method does not exist as a standalone symbol, the blast radius is assessed against
the **inline drain blocks** within `ProcessShutdownSIMA`:

- **Direct caller chain:** `ProcessShutdownSIMA` ← `ProcessApplySimaState(enabled=false)` ←
  IPC toggle, UI panel toggle, and the strategy `Terminated` path in `src/V12_002.Lifecycle.cs`.
- **Sibling duplication hazard:** `DrainAllDispatchQueuesOnAbort` in
  `src/V12_002.SIMA.Fleet.cs` (lines ~287–323) implements a near-identical Photon-ring + legacy-
  queue drain with three structural divergences: uses `AddExpectedPositionDeltaLocked` (locked
  variant) instead of `AddExpectedPositionDelta`, additionally calls `TrackPhotonDequeue()` and
  `Interlocked.Decrement(ref _pendingFleetDispatchCount)`, and calls
  `TryResetCircuitBreakerIfBelow`. Any correctness fix applied to one site must be mirrored to
  the other — this is the primary blast radius risk.
- **Mutable state written:** `_photonPool` (slot releases), `_photonSideband[]` (zeroed),
  `_pendingFleetDispatches` (drained), `_photonDispatchRing` (drained), `expectedPositions` dict
  (delta rollback via `AddExpectedPositionDelta`), dispatch-sync barrier map (via
  `ClearDispatchSyncPending`).
- **REAPER dependency:** Incomplete drain leaves `expectedPositions` with stale reserved deltas,
  triggering false REAPER CRITICAL DESYNC on the next SIMA enable cycle.
- **External callers:** None outside the SIMA lifecycle path; blast radius is bounded to the
  SIMA enable/disable cycle and strategy termination.

## Top 3 Complexity Drivers

(Assessed against the inline drain blocks in `ProcessShutdownSIMA`, lines 104–136, which
constitute the de-facto body of the unextracted `DrainPhotonQueuesOnShutdown`.)

### 1 — Photon-ring drain logic inlined in two files with divergent semantics (+4 CYC each, divergence risk)

The Photon-ring drain block (Lifecycle.cs lines 107–123) and its twin in `DrainAllDispatchQueuesOnAbort`
(Fleet.cs lines ~291–308) implement the same sideband-aware delta-rollback-and-pool-release pattern
but with subtle operational differences (locked vs. unlocked delta call; missing `TrackPhotonDequeue`
and `_pendingFleetDispatchCount` decrement in the Lifecycle copy). Four of the eight McCabe
predicate nodes in `ProcessShutdownSIMA`'s drain section belong to this ring-drain block.
Any future change (e.g., adding XorShadow verification on shutdown) requires editing two files
with matching but non-identical semantics — the absence of extraction is the root divergence hazard.

**Extraction target:** `DrainPhotonRing()` — shared ring-drain body callable by both callers —
would reduce each caller's CYC by ~4 and eliminate the dual-maintenance burden.

### 2 — Compound boolean guards producing hidden branches inside the ring-drain loop body (+3 CYC)

Three nested conditional checks within the Photon ring loop body each contribute an independent
McCabe predicate:

| Expression | CYC contribution |
|---|---|
| `(_sbIdx >= 0 && _sbIdx < _photonSideband.Length)` ternary | +1 (compound null-AND) |
| `if (ringSlot.ReservedDelta != 0 && _expectedKey != null)` | +1 (compound AND guard) |
| `if (_expectedKey != null)` standalone | +1 |

These guards exist because pool slot index and sideband key can independently be invalid, but
they are expressed as raw inline comparisons with no named predicate or guard method. The loop
body reads as a 7-condition gauntlet for what is logically three operations:
*rollback delta → clear sync barrier → release pool slot*.

**Extraction target:** `ReleasePhotonSlot(ref FleetDispatchSlot slot)` helper to collapse inner
`if (_sbIdx >= 0) { ... if (_sbIdx < ...) }` nesting.

### 3 — Legacy queue drain duplicating rollback pattern without structural reuse (+2 CYC)

The second `while` block (lines 129–134) drains `_pendingFleetDispatches` with a simpler
`if (ignored.ReservedDelta != 0)` guard — a structural subset of the ring-drain rollback. Both
loops call `AddExpectedPositionDelta` + `ClearDispatchSyncPending` with no shared helper. As a
pre-Photon compatibility path, its rollback semantics must stay consistent with the ring path;
inlining both means a missing `ClearDispatchSyncPending` call in either loop would be invisible
to a reviewer focused on the other.

**Extraction target:** `RollbackAndClearDispatchSlot(string expectedKey, int reservedDelta)` —
2-line helper reused by both loop bodies to make the invariant explicit and auditable.

## Recommended Extraction Count

**3 extractions recommended** (identical to EPIC-W7-055 analysis — this is the same method body):

| # | Extracted helper | Lines eliminated | CYC reduction |
|---|---|---|---|
| 1 | `DrainPhotonRing()` — shared ring-drain body for both `ProcessShutdownSIMA` and `DrainAllDispatchQueuesOnAbort` | Removes 17-line ring block from each site | −4 per caller (−8 total across both files) |
| 2 | `ReleasePhotonSlot(ref FleetDispatchSlot slot)` — sideband-index guard + pool release + sideband zero | Collapses inner `if (_sbIdx >= 0) { if (_sbIdx < ...) }` nesting | −2 |
| 3 | `RollbackAndClearDispatchSlot(string expectedKey, int reservedDelta)` — delta rollback + sync-clear | 2-line helper reused by both ring and legacy loop bodies | −1 |

**Projected post-refactor CYC for `ProcessShutdownSIMA`:** ≈ 2 (1 base + 1 loop guard each for ring
and legacy queues; all inner branches absorbed into helpers) — well below the threshold of 10.

> **Coordination note:** This epic (EPIC-W7-108) and EPIC-W7-055 target the same logical unit.
> Before proceeding to Phase 1, confirm with the Wave 7 coordinator whether EPIC-W7-108 should
> be merged into EPIC-W7-055, or whether EPIC-W7-108 should be the active ticket and W7-055
> closed as superseded. Duplicate execution of both would produce conflicting commits.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.2 |
| **Execution Time** | ~90s |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Epic** | EPIC-W7-108 |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **CYC Confirmed** | 0 (method absent as standalone symbol; inline body CYC ≈ 8 per static analysis) |
| **Status** | ⚠️ Requires manual review — duplicate epic / method not yet extracted |
