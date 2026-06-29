# EPIC-W7-034 — Phase 1: Scope Definition

## Summary

This document establishes the scope boundary for EPIC-W7-034. Exactly one single method is
targeted for cyclomatic-complexity reduction in this epic. No other methods are included.

---

## Method in Scope

| Field           | Value                                             |
|-----------------|---------------------------------------------------|
| **Method**      | `ManageCIT`                                       |
| **Class**       | `V12_002` (partial)                               |
| **Source File** | `src/V12_002.Orders.Management.Flatten.cs`        |
| **Current CYC** | 11                                                |
| **Target CYC**  | ≤ 8                                               |
| **Lines**       | 61 (L68–L128)                                     |
| **Max Nesting** | 5                                                 |
| **Wave**        | 7                                                 |

This is a single method scope. The scope boundary is drawn tightly around `ManageCIT` and does
not extend to any helper, caller, or co-located method in the same partial class or file.

---

## Caller Inventory

Grep of `src/` for `ManageCIT` returned **4 call sites** across **2 source files**:

| # | File                                          | Line | Pattern                              |
|---|-----------------------------------------------|------|--------------------------------------|
| 1 | `src/V12_002.BarUpdate.cs`                    | 265  | Direct inline call: `ManageCIT()`    |
| 2 | `src/V12_002.BarUpdate.cs`                    | 328  | Actor queue: `Enqueue(ctx => ctx.ManageCIT())` |
| 3 | `src/V12_002.Orders.Management.Flatten.cs`    | 163  | Actor queue: `Enqueue(ctx => ctx.ManageCIT())` |
| 4 | `src/V12_002.Orders.Management.Flatten.cs`    | 189  | Comment reference (non-call)         |

**Distinct caller files: 2** (`BarUpdate.cs`, `Orders.Management.Flatten.cs`).

The two queue-based call sites use a lambda delegate pattern (`Enqueue(ctx => ctx.ManageCIT())`).
Static analysis tools (jcodemunch blast-radius) report zero importers for this reason; the
functional caller count is confirmed by grep as shown above.

---

## Why `ManageCIT` Is the Single Method in Scope

`ManageCIT` is selected as the single method in scope because:

1. **CYC 11 exceeds the V12.23 project threshold of CYC ≤ 8.** The project convention
   (V12.23) requires that all methods introduced or touched in Wave 7 meet a CYC ceiling of 8.
   `ManageCIT` at CYC 11 is the only method in `src/V12_002.Orders.Management.Flatten.cs`
   that breaches this threshold and was flagged by the Phase 0 hotspot analysis.

2. **Blast radius is contained.** Static analysis reports zero exposed importers. Functional
   callers are two files, both already identified. Refactoring `ManageCIT` in isolation does not
   require touching any other method's signature or contract.

3. **Co-located methods are NOT in scope.** `FlattenSinglePosition` (CYC=27, same file) is a
   significantly larger hotspot identified in Phase 0. It is intentionally excluded from this
   epic. Including it would widen the scope boundary beyond what is achievable in a single
   targeted wave-7 task and would risk destabilising the fleet-follower flattening path
   simultaneously with CIT nudging changes. Per V12.23, each epic targets one complexity
   reduction unit at a time. `FlattenSinglePosition` and any other method in the file remain
   outside the scope boundary of EPIC-W7-034.

4. **Helper methods are NOT in scope.** `ValidateCitConfiguration` and `ExecuteFollowerNudge`
   are called by `ManageCIT` and are complexity contributors identified in Phase 0 Driver 1 and
   Driver 2. However, they are helpers subordinate to `ManageCIT`'s own decision tree. Observability
   improvements to `ValidateCitConfiguration` (adding trace logs) may be performed as a
   side-effect of the extraction work, but that helper is not itself a refactoring target. It
   does not independently breach V12.23 thresholds and is therefore outside the scope boundary.

---

## Complexity Reduction Contract

| Metric          | Current | Target |
|-----------------|---------|--------|
| CYC             | 11      | ≤ 8    |
| Max Nesting     | 5       | ≤ 4    |
| Method length   | 61 loc  | ≤ 45 loc (indicative) |

The CYC ≤ 8 target is the binding contract for this epic. Nesting and line-count reductions are
desirable secondary outcomes of the planned extractions but are not individually gating.

---

## Scope Boundary Statement

The scope boundary for EPIC-W7-034 is: **`ManageCIT` in
`src/V12_002.Orders.Management.Flatten.cs`, and nothing else.**

Any work item that touches a method other than `ManageCIT` — including its callers, its helpers,
or any co-located method — is outside the scope boundary and must be tracked under a separate epic.

---

## Agent Tracking

| Field              | Value                                          |
|--------------------|------------------------------------------------|
| **Agent Name**     | v12-phase1-scope                               |
| **Epic**           | EPIC-W7-034                                    |
| **Wave**           | 7                                              |
| **Phase**          | 1 — Scope Definition                           |
| **Input Docs**     | `00-hotspots.md`, `manifest.json`              |
| **Grep Target**    | `src/` for `ManageCIT`                         |
| **Grep Matches**   | 7 lines across 3 files (2 distinct caller files) |
| **Output**         | `docs/brain/EPIC-W7-034/00-scope.md`           |
