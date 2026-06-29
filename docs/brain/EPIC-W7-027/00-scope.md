# EPIC-W7-027 — Phase 1: Scope Definition

## Single Method in Scope

| Field             | Value                                  |
|-------------------|----------------------------------------|
| **Method**        | `Dispatch_PublishMarketBracketToPhoton` |
| **Source File**   | `src/V12_002.SIMA.Dispatch.cs`         |
| **Lines**         | 612–753                                |
| **Current CYC**   | 9                                      |
| **Target CYC**    | ≤ 8 (mandatory ceiling for Wave 7)     |
| **Recommended**   | ≤ 4 (orchestration skeleton after 3 extractions) |

This phase narrows the entire epic to a **single method**: `Dispatch_PublishMarketBracketToPhoton`.
No other method is included in the refactor scope at this time.

---

## Scope Boundary

The **scope boundary** is drawn at the declaration of `Dispatch_PublishMarketBracketToPhoton`
(line 612) through its closing brace (line 753) in `src/V12_002.SIMA.Dispatch.cs`.

- All code changes in Phase 2 (Refactor Implementation) are confined to this method and the
  private helpers extracted from it.
- Callers and downstream callees remain unchanged in their signatures unless a helper extraction
  necessitates a new private method declaration in the same file — which is permitted within the
  scope boundary.
- No changes are permitted to `Dispatch_ProcessFleetLoop`, `PublishPhoton_StopOrder`,
  `PublishPhoton_TargetOrders`, `RegisterTrackingDictionaries`, `InitializeFollowerBracketFSM`,
  `SymmetryGuardRegisterFollower`, `AddExpectedPositionDeltaLocked`, or `EnqueueToPhotonRing`
  unless those changes are strictly mechanical renames driven by an extraction inside the boundary.

---

## Callers

Grep of `src/` confirms exactly **1 caller**:

| # | Caller Method              | File                            | Line |
|---|----------------------------|---------------------------------|------|
| 1 | `Dispatch_ProcessFleetLoop` | `src/V12_002.SIMA.Dispatch.cs` | 277  |

The method is called once, inside the market-entry follower loop, triggered for every follower
whose entry condition is satisfied. There are no additional call sites in the codebase.
Line 1101 in the same file contains a log string that references the method name but does not
constitute a call site.

---

## Why Other Methods Are NOT in Scope

This epic operates under **V12.23** single-method isolation discipline.

Under V12.23, each epic targets one method per wave phase to:

1. **Contain rollback risk** — A single-method scope means a revert (if validation fails in
   Phase 3) is a one-function rollback with no collateral damage.
2. **Preserve blast-radius contracts** — The 12 downstream symbols identified in Phase 0
   (`PublishPhoton_StopOrder`, `RegisterTrackingDictionaries`, `InitializeFollowerBracketFSM`,
   etc.) each carry their own CYC budget and will be addressed in subsequent wave epics if their
   own CYC threshold is breached.
3. **Avoid cascading refactor drift** — Methods like `Dispatch_ProcessFleetLoop` (CYC 11) and
   `TryIncrementDispatchCountWithCircuitBreaker` (CYC 8) are adjacent hotspots, but touching
   them in the same epic would violate the V12.23 scope boundary contract and introduce
   unquantified test surface.
4. **Respect the `ref`-parameter rollback surface** — The parent loop's catch block at lines
   315–343 depends on the three `ref` flags (`syncPending`, `registeredForCleanup`,
   `reservedDelta`) being managed exclusively by `Dispatch_PublishMarketBracketToPhoton`. Any
   co-refactor of the parent loop during the same epic would make correctness of the rollback
   surface unverifiable in isolation.

In summary: all methods other than `Dispatch_PublishMarketBracketToPhoton` are **explicitly
out of scope** for EPIC-W7-027 per the V12.23 single-method isolation rule.

---

## Complexity Baseline

| Metric                      | Value     |
|-----------------------------|-----------|
| Current CYC                 | 9         |
| Target CYC (ceiling)        | ≤ 8       |
| Recommended post-refactor   | ≤ 4       |
| Exit points in method       | 3         |
| `ref` parameters            | 3 (`syncPending`, `registeredForCleanup`, `reservedDelta`) |
| Total parameters            | 15        |
| Downstream callees          | 12        |
| Identified extraction count | 3         |

Top complexity drivers (from Phase 0):
1. Dual early-return paths with `ref` side-effect contract (CYC +3)
2. Conditional exit-action inversion threaded through 3 helpers (CYC +2)
3. Zero-allocation Photon pool lifecycle embedded inline (CYC +2)

---

## Planned Extractions (Preview for Phase 2)

| # | Proposed Helper                      | Responsibility                                      | Estimated CYC |
|---|--------------------------------------|-----------------------------------------------------|---------------|
| 1 | `BuildExitActionAndStopOrder(...)`   | Stop creation + null-guard; returns abort signal    | ≤ 3           |
| 2 | `ReservePositionAndRegisterState(...)` | Tracking dicts, FSM init, symmetry guard, delta    | ≤ 3           |
| 3 | `DispatchToPhotonRing(...)`          | Pool claim, slot populate, circuit-breaker, enqueue | ≤ 3           |

These extractions are scoped exclusively within the scope boundary defined above.

---

## Agent Tracking

| Field              | Value                                   |
|--------------------|-----------------------------------------|
| **Agent Name**     | v12-phase1-scope                        |
| **Epic**           | EPIC-W7-027                             |
| **Wave**           | 7                                       |
| **Phase**          | 1 — Scope Definition                   |
| **Source File**    | `src/V12_002.SIMA.Dispatch.cs`          |
| **Method**         | `Dispatch_PublishMarketBracketToPhoton` |
| **CYC Confirmed**  | 9                                       |
| **Target CYC**     | ≤ 8                                     |
| **Callers Found**  | 1 (`Dispatch_ProcessFleetLoop` line 277)|
| **V12.23 Rule**    | single method isolation enforced        |
| **Output**         | `docs/brain/EPIC-W7-027/00-scope.md`   |
