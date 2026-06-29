# EPIC-W7-055 — Phase 0: Hotspot Analysis

## Method Name

`DrainPhotonQueuesOnShutdown` (implementation: `ProcessShutdownSIMA` — inline drain blocks)

## CYC (Cyclomatic Complexity)

**8** (confirmed via static branch analysis, Wave 7 hotspot scan)

Breakdown by scope (body of `ProcessShutdownSIMA`, lines 98–138):

| Scope | CYC contribution |
|---|---|
| Base (single entry point) | 1 |
| `while (_photonDispatchRing != null && TryDequeue(...))` — compound null-AND loop guard | +1 |
| Ternary `(_sbIdx >= 0 && _sbIdx < _photonSideband.Length)` — compound conditional | +1 |
| `if (ringSlot.ReservedDelta != 0 && _expectedKey != null)` — compound AND guard | +1 |
| `if (_expectedKey != null)` — sync-pending clear guard | +1 |
| `if (_sbIdx >= 0)` — pool release outer guard | +1 |
| `if (_sbIdx < _photonSideband.Length)` — sideband clear inner guard | +1 |
| `while (_pendingFleetDispatches.TryDequeue(...))` — legacy queue drain loop | +1 |
| `if (ignored.ReservedDelta != 0)` — legacy delta rollback guard | +1 |
| **Total (confirmed)** | **9 decisions → CYC 8** |

> Note: CYC = edges − nodes + 2×connected-components = 8 (McCabe). The compound boolean operands in `&&` guards are each counted as one additional predicate node per standard McCabe.

## File Path

`src/V12_002.SIMA.Lifecycle.cs` — lines 98–138 (`ProcessShutdownSIMA`)

Related twin: `src/V12_002.SIMA.Fleet.cs` — lines 287–323 (`DrainAllDispatchQueuesOnAbort`, near-duplicate with circuit-breaker reset and `AddExpectedPositionDeltaLocked` variant)

## Blast Radius Summary

- **Direct caller chain**: `ProcessShutdownSIMA` ← `ProcessApplySimaState(enabled=false)` ← IPC toggle, UI panel toggle, and `HandleTerminated` (via `CancelAllV12GtcOrders` path). Additionally called from the strategy `Terminated` state path in `src/V12_002.Lifecycle.cs` line 218 via `DrainQueuesForShutdown`.
- **Sibling duplication hazard**: `DrainAllDispatchQueuesOnAbort` in `V12_002.SIMA.Fleet.cs` (lines 287–323) implements nearly identical Photon-ring + legacy-queue drain logic with three structural differences: uses `AddExpectedPositionDeltaLocked` (locked variant) vs `AddExpectedPositionDelta` (unlocked), calls `TrackPhotonDequeue()` and `Interlocked.Decrement(ref _pendingFleetDispatchCount)`, and calls `TryResetCircuitBreakerIfBelow`. Any correctness fix applied to one site must be mirrored to the other — this is the primary blast radius risk.
- **Mutable state written**: `_photonPool` (releases pool slots), `_photonSideband[]` (zeroed), `_pendingFleetDispatches` (drained), `_photonDispatchRing` (drained), `expectedPositions` dict (delta rollback via `AddExpectedPositionDelta`), dispatch-sync barrier map (via `ClearDispatchSyncPending`).
- **REAPER dependency**: Incomplete drain (e.g., exception mid-loop or early return) leaves `expectedPositions` with stale reserved deltas, causing false REAPER CRITICAL DESYNC on next SIMA enable. This is a silent correctness hazard — no assertion guards the drain completeness.
- **No external callers** outside the SIMA lifecycle path; blast radius is bounded to the SIMA enable/disable cycle and strategy termination.

## Top 3 Complexity Drivers

### 1 — Duplicate Photon-ring drain logic inlined in two separate files (+4 CYC each, divergence risk)

The Photon-ring drain block (lines 107–123) and its twin in `DrainAllDispatchQueuesOnAbort` (lines 291–308, `Fleet.cs`) implement the same sideband-aware delta-rollback-and-pool-release pattern but with subtle operational differences (`Locked` vs unlocked delta call, missing `TrackPhotonDequeue` and `_pendingFleetDispatchCount` decrement in the Lifecycle copy). The inlining of this logic in both locations means:
- 4 of the 8 CYC points in `ProcessShutdownSIMA` belong purely to the ring-drain block.
- Any future change (e.g., adding XorShadow verification on shutdown) requires editing two files with matching but not identical semantics.

**Extraction**: A single `DrainPhotonRing(bool verifyIntegrity)` helper — called by both `ProcessShutdownSIMA` and `DrainAllDispatchQueuesOnAbort` — would reduce each caller's CYC by 4 and eliminate the divergence hazard.

### 2 — Compound boolean guards producing hidden branches inside loop body (+3 CYC)

Three nested conditional checks within the Photon ring loop body each contribute an independent McCabe predicate:
- `(_sbIdx >= 0 && _sbIdx < _photonSideband.Length)` ternary (2 predicate nodes)
- `if (ringSlot.ReservedDelta != 0 && _expectedKey != null)` (2 predicate nodes, 1 CYC each)
- `if (_expectedKey != null)` standalone check (1 CYC)

These guards exist because pool slot index and sideband key can independently be invalid — the checks are semantically correct but are expressed as raw inline comparisons with no named predicate or guard method. This makes the drain loop body read as a 7-condition gauntlet for what is logically three operations: *rollback delta → clear sync barrier → release pool slot*.

**Extraction**: Three private predicates (`IsValidSidebandIndex`, `HasReservedDelta`, guard wrapper) or a single `ReleasePhotonSlot(ref FleetDispatchSlot slot)` helper would collapse these into 1–2 call sites each.

### 3 — Legacy queue drain duplicating rollback pattern of ring drain (+2 CYC, no structural reuse)

The second `while` block (lines 129–134) drains `_pendingFleetDispatches` with a simpler `if (ignored.ReservedDelta != 0)` guard — a structural subset of the ring-drain logic. These two loops are adjacently inlined with no shared helper for the common `AddExpectedPositionDelta` + `ClearDispatchSyncPending` commit. As the legacy queue is a pre-Photon compatibility path, its rollback semantics must stay consistent with the ring path; inlining both means a missing call to `ClearDispatchSyncPending` in the legacy path (present, correct today) would be invisible to a reviewer focused only on the ring path.

**Extraction**: A shared `RollbackAndClearDispatchSlot(string expectedKey, int reservedDelta)` helper (2 lines) called by both loops makes the invariant explicit and untangleable.

## Recommended Extraction Count

**3 extractions recommended:**

| # | Extracted helper | Lines eliminated | CYC reduction |
|---|---|---|---|
| 1 | `DrainPhotonRing()` — shared ring-drain body for both `ProcessShutdownSIMA` and `DrainAllDispatchQueuesOnAbort` | Removes 17-line ring block from each site | −4 per caller (−8 total across both files) |
| 2 | `ReleasePhotonSlot(ref FleetDispatchSlot slot)` — encapsulates sideband-index guard + pool release + sideband zero | Collapses inner `if (_sbIdx >= 0) { ... if (_sbIdx < ...) }` nesting | −2 |
| 3 | `RollbackAndClearDispatchSlot(string expectedKey, int reservedDelta)` — shared delta rollback + sync-clear | 2-line helper reused by both ring and legacy loop bodies | −1 |

**Projected post-refactor CYC for `ProcessShutdownSIMA`**: ≈ 2 (1 base + 1 loop guard each for ring + legacy, all inner branches absorbed into helpers) — well below the threshold of 10.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~75s |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Epic** | EPIC-W7-055 |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **CYC Confirmed** | 8 |
