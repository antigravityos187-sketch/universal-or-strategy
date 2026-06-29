# EPIC-W7-030 — Phase 1: Scope Definition

## Method in Scope

| Field | Value |
|-------|-------|
| **Method** | `ValidateOrphanedMasterOrders(string reason)` |
| **Source File** | `src/V12_002.Orders.Management.Cleanup.cs` · Lines 457–479 |
| **Current CYC** | 0 (post-EPIC-CCN-18 dispatcher shell; all conditional branches extracted during prior refactoring wave, original CYC 19 → 4 → 0) |
| **Target CYC** | ≤ 8 |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |

---

## Scope Boundary

This phase enforces a strict **scope boundary**: exactly one **single method** is under
examination — `ValidateOrphanedMasterOrders`. No peer method, helper, caller, or
downstream dependency crosses into this phase's work boundary.

The scope boundary is defined by the following criteria:

1. The method is identified as the sole Wave 7 hotspot in
   `src/V12_002.Orders.Management.Cleanup.cs` by the Phase 0 analysis.
2. Its CYC of 0 is the subject of this epic's complexity tracking.
3. All work items in subsequent phases are anchored to this single method's behaviour
   and architectural risks — not to any adjacent symbol.

---

## Callers

Grep of `src/` for `ValidateOrphanedMasterOrders` returned **2 matches**:

| Role | Symbol | Location |
|------|--------|----------|
| Definition | `ValidateOrphanedMasterOrders(string reason)` | `src/V12_002.Orders.Management.Cleanup.cs:457` |
| Direct caller | `ReconcileOrphanedOrders` (call site) | `src/V12_002.Orders.Management.Cleanup.cs:662` |

**Callers count: 1** — `ReconcileOrphanedOrders` is the sole direct caller, confirmed
by grep output (1 definition match + 1 call-site match, both within the same source
file). No cross-file callers exist. The upstream trigger chain is:

```
OnPositionUpdate  →  ReconcileOrphanedOrders  →  ValidateOrphanedMasterOrders
```

---

## CYC Summary

| Metric | Value |
|--------|-------|
| Current CYC | **0** |
| Target CYC | **≤ 8** |
| Extraction count recommended | **0** (Phase 0 determination) |
| CYC gap to close | None — current CYC is already below target |

`ValidateOrphanedMasterOrders` is a fully-extracted dispatcher shell. The Phase 0
hotspot analysis (grounded in jcodemunch MCP tooling) confirmed CYC = 0 with
0 decision points in the dispatcher body itself. The four EPIC-CCN-18 helpers
(`ShouldValidateOrder`, `HasV12OrderPrefix`, `ExtractEntryNameFromOrderName`,
`IsOrphanedOrder`) represent the complete prior decomposition; no further structural
extraction is possible.

---

## Why Other Methods Are NOT in Scope

Per **V12.23** project convention, each EPIC phase targets exactly one method.
Cross-method scope creep is prohibited regardless of blast radius or shared-state
coupling. The following symbols appear in the blast radius but are explicitly excluded
from this epic's scope boundary:

| Symbol | File | Reason Excluded |
|--------|------|-----------------|
| `ReconcileOrphanedOrders` | `V12_002.Orders.Management.Cleanup.cs:653` | Caller only; its own CYC is not the subject of this epic |
| `ShouldValidateOrder` | `V12_002.Orders.Management.Cleanup.cs` | EPIC-CCN-18 extracted helper; frozen per V12.23 |
| `HasV12OrderPrefix` | `V12_002.Orders.Management.Cleanup.cs` | EPIC-CCN-18 extracted helper; frozen per V12.23 |
| `ExtractEntryNameFromOrderName` | `V12_002.Orders.Management.Cleanup.cs` | EPIC-CCN-18 extracted helper; frozen per V12.23 |
| `IsOrphanedOrder` | `V12_002.Orders.Management.Cleanup.cs` | EPIC-CCN-18 extracted helper; frozen per V12.23 |
| `CancelOrderOnAccount` | `V12_002.Orders.CancelGateway.cs:46` | Cross-subsystem gateway; shared across 8+ call sites; out of scope per V12.23 single-method rule |
| `OnPositionUpdate` | `V12_002.Orders.Callbacks.Execution.cs:105` | Upstream trigger; separate callback lifecycle; not a target of this wave |

V12.23 mandates that the **single method** nominated in the epic manifest is the
exclusive unit of analysis. All adjacent symbols — whether callers, callees, helpers,
or shared-state consumers — retain their existing epic assignments or await independent
wave nomination.

---

## Risk Notes (Informational — Not in Scope for Phase 2 Extraction)

The Phase 0 hotspot analysis identified three architectural risk drivers that are
orthogonal to CYC and outside the scope boundary of structural extraction:

1. **Shared mutable `activePositions` read during live order iteration** — concurrency
   latency risk; requires architectural guard, not method extraction.
2. **Order-name parsing heuristic in `ExtractEntryNameFromOrderName`** — silent coupling;
   addressed only if a prefix registry work item is separately nominated.
3. **`CancelOrderOnAccount` gateway cross-subsystem surface** — disproportionate blast
   radius; requires gateway interface isolation, not extraction within this method.

These risks are recorded here for traceability but do not alter the scope boundary.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase1-scope |
| **Epic** | EPIC-W7-030 |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Source File** | `src/V12_002.Orders.Management.Cleanup.cs` |
| **Method** | `ValidateOrphanedMasterOrders` |
| **CYC Current** | 0 |
| **CYC Target** | ≤ 8 |
| **Callers Count** | 1 (`ReconcileOrphanedOrders`) |
| **Scope Confirmed** | single method — `ValidateOrphanedMasterOrders` |
| **Generated** | 2025-07-14 |
