# EPIC-W7-035 — Phase 1: Scope Definition

## Single Method in Scope

This epic targets a **single method**: `SyncLimitTarget`.

| Field | Value |
|---|---|
| **Method** | `SyncLimitTarget` |
| **Source File** | `src/V12_002.Orders.Management.StopSync.cs` |
| **Definition Line** | 176 |
| **LOC** | 161 (lines 176–336) |
| **Current CYC** | 34 |
| **Target CYC** | ≤ 8 |
| **Access Modifier** | `private` |
| **Class** | `partial class V12_002 : Strategy` |
| **Namespace** | `NinjaTrader.NinjaScript.Strategies` |

---

## Scope Boundary

The **scope boundary** for this epic is strictly limited to the body of `SyncLimitTarget`
and the private helpers that will be extracted from it during Phase 2. No other method,
class, or file falls within the scope boundary of this refactor.

The scope boundary is enforced by the following constraints:

1. All new helpers will be declared `private` within the same `partial class V12_002`.
2. The public-facing call signature of `SyncLimitTarget` itself is frozen — no caller
   modification is required or permitted.
3. No shared state contracts (`targetDict`, `activePositions`, broker API surfaces) are
   altered; only the internal structural decomposition of `SyncLimitTarget` changes.

---

## Callers

Grep over `src/` confirms **1 caller** of `SyncLimitTarget`:

| Caller Method | File | Line |
|---|---|---|
| `RefreshActivePositionOrders` | `src/V12_002.Orders.Management.StopSync.cs` | 85 |

`SyncLimitTarget` is invoked up to N×5 times per `RefreshActivePositionOrders` call
(one invocation per active position × target slot 1–5). The single call site is located
within the same file as the definition; no cross-file caller exists.

---

## Why Other Methods Are NOT in Scope

Rule **V12.23** ("one hotspot per epic, no scope creep") governs this restriction.

The `jcodemunch get_hotspots` analysis identified three additional complexity candidates
within the same source file:

| Method | CYC | Reason Excluded |
|---|---|---|
| `CreateNewStopOrder` | ~18 | Separate hotspot; V12.23 prohibits bundling into this epic |
| `UpdateStopQuantity` | ~15 | Separate hotspot; V12.23 prohibits bundling into this epic |
| `ValidateStopOrderPreconditions` | ~12 | Separate hotspot; V12.23 prohibits bundling into this epic |

Each excluded method would require its own epic, Phase 0 hotspot analysis, and
independent validation cycle. Bundling them here would violate **V12.23**, inflate the
blast radius of this change set, and compromise the ability to attribute regression risk
to a single method extraction. This epic processes a **single method** only.

---

## CYC Reduction Plan

The Phase 0 hotspot analysis identified **3 extractions** sufficient to bring
`SyncLimitTarget` from CYC 34 to estimated CYC ≤ 8:

| # | New Method | Complexity Driver Eliminated | Est. CYC Reduction |
|---|---|---|---|
| 1 | `SetTargetPrice(PositionInfo, int, double)` | Both duplicated `switch (targetNum)` blocks (lines 209–229 and 287–307) | −10 to −12 |
| 2 | `SyncLimitTarget_Reprice(...)` | Reprice arm: delta guard + `ChangeOrder` + catch (lines ~203–243) | −6 to −8 |
| 3 | `SyncLimitTarget_Submit(...)` | New-submit arm: direction ternary + `SubmitOrderUnmanaged` + null guard + catch (lines ~259–334) | −6 to −8 |

Post-extraction, `SyncLimitTarget` becomes a thin coordinator: price calculation →
early-return guard → `hasWorkingOrder` branch → 2 delegation calls. Residual estimated
CYC ≤ 6, comfortably within the ≤ 8 target.

All three extractions are pure structural refactors with zero behaviour change. The
`ref int refreshed` parameter must be threaded through to both `_Reprice` and `_Submit`
to preserve the refresh counter.

---

## Risk Classification

| Dimension | Assessment |
|---|---|
| **Call-site impact** | None — single caller, same file, signature frozen |
| **Cross-file impact** | None — all new helpers are `private` to `partial class V12_002` |
| **Broker API impact** | None — `ChangeOrder` and `SubmitOrderUnmanaged` calls preserved verbatim |
| **Shared state impact** | None — `targetDict` and `pos.Target{n}Price` writes preserved verbatim |
| **Overall risk** | **Low** |

---

## Inputs Consumed

| Artifact | Status |
|---|---|
| `docs/brain/EPIC-W7-035/00-hotspots.md` | Read ✓ |
| `docs/brain/EPIC-W7-035/manifest.json` | Read ✓ |
| `src/V12_002.Orders.Management.StopSync.cs` | grep confirmed (callers) ✓ |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase1-scope |
| **Epic** | EPIC-W7-035 |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Source File** | `src/V12_002.Orders.Management.StopSync.cs` |
| **Method in Scope** | `SyncLimitTarget` (single method) |
| **Current CYC** | 34 |
| **Target CYC** | ≤ 8 |
| **Callers Found** | 1 (`RefreshActivePositionOrders` at line 85) |
| **Scope Boundary** | Enforced — single method, private helpers only, no external file changes |
| **V12.23 Applied** | Yes — 3 other methods explicitly excluded |
| **Output** | `docs/brain/EPIC-W7-035/00-scope.md` |
