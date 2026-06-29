# EPIC-W7-059 — Phase 1: Scope Definition

## Overview

This document defines the **scope boundary** for EPIC-W7-059. The refactor targets a
**single method** cluster — the logical `AdoptMasterWorkingOrders` pipeline — which is the
highest-complexity adoption path in the SIMA Lifecycle subsystem.

---

## Method in Scope

| Field              | Value                                  |
|--------------------|----------------------------------------|
| **Logical name**   | `AdoptMasterWorkingOrders`             |
| **Live symbol(s)** | `HydrateWorkingOrdersFromBroker` (orchestrator, lines 309–457) + `AdoptMasterOrders` (leaf, lines 1195–1254) |
| **Source file**    | `src/V12_002.SIMA.Lifecycle.cs`        |
| **Current CYC**    | 34 (cluster total across both bodies)  |
| **Target CYC**     | ≤ 8 per method after extraction        |
| **Wave**           | 7                                      |
| **Epic ID**        | EPIC-W7-059                            |

The backlog ticket names the target `AdoptMasterWorkingOrders` (CYC=34). No symbol by that
exact name exists in the compiled codebase. Name resolution (documented in `00-hotspots.md`)
maps this logical name to the two-body cluster above, which together constitute the full
adoption pipeline and account for the measured CYC=34. Phase 1 scopes to this entire cluster
as a **single method** unit of work.

---

## Caller Count

Grep across `src/` (`HydrateWorkingOrdersFromBroker | AdoptMasterOrders`) produced
**2 call sites** into the public entry point of this cluster:

| # | File                               | Line | Context                                                              |
|---|-------------------------------------|------|----------------------------------------------------------------------|
| 1 | `src/V12_002.SIMA.Lifecycle.cs`    | 196  | `HydrateWorkingOrdersFromBroker()` — startup path via `EnumerateApexAccounts` |
| 2 | `src/V12_002.Lifecycle.cs`         | 337  | `Enqueue(ctx => ctx.HydrateWorkingOrdersFromBroker())` — reconnect path via `ProcessOnConnectionStatusUpdate` |

`AdoptMasterOrders` is called internally at line 320 of the same file; it is **not** a
public call site and is counted as an internal implementation detail of the cluster.
Total **external callers: 2**.

---

## Scope Boundary

The **scope boundary** is drawn around the `AdoptMasterWorkingOrders` logical cluster only.
Concretely, the following symbols fall inside the boundary:

- `HydrateWorkingOrdersFromBroker` — orchestrator (primary extraction target)
- `AdoptMasterOrders` — leaf adoption method (secondary extraction target)
- Inline master-position reconstruction block (lines 340–442) — to be extracted as
  `ReconstructMasterActivePosition()`

Everything outside this cluster is **out of scope** for this epic. The boundary is enforced
as follows:

- **AdoptFleetOrders** — called by the orchestrator but its own body is not modified; it is
  a dependency, not a target.
- **HydrateFSMsFromWorkingOrders** — called after adoption completes; FSM rebuild logic is
  not part of this epic.
- **ClassifyOrderByPrefix**, **RouteOrderToTargetDict**, **IsValidOrderState**,
  **RebuildFleetPositionFromEntry** — pure helpers consumed by the cluster; their bodies
  are not changed unless required to resolve the `Unknown`-state divergence risk noted
  in `00-hotspots.md`.

---

## Why Other Methods Are NOT in Scope (V12.23 Rule)

Per project rule **V12.23**, each epic targets exactly one logical method cluster at a time.
Broadening scope beyond the nominated cluster violates this rule for the following reasons:

1. **Blast radius control** — the adoption pipeline mutates 7 shared `ConcurrentDictionary`
   instances and the `_orderAdoptionComplete` REAPER gate. Introducing concurrent changes to
   caller code (e.g., `EnumerateApexAccounts` or `ProcessOnConnectionStatusUpdate`) alongside
   the extraction work would make regression attribution impossible.
2. **FSM rebuild independence** — `HydrateFSMsFromWorkingOrders` and its downstream position
   pass are separately nominated in future wave epics; touching them here would create
   overlapping change sets across epics.
3. **Pure helpers are stable** — `ClassifyOrderByPrefix`, `RouteOrderToTargetDict`, and
   `IsValidOrderState` have CYC ≤ 8 individually and do not contribute to the CYC=34 total;
   there is no complexity justification to include them in this epic.
4. **Threading contract isolation** — `AdoptMasterOrders` carries an explicit
   `ACTOR-SERIALIZED` contract; isolating all extraction work to the cluster boundary ensures
   the threading invariant remains trivially auditable.

---

## Extraction Preview (from Phase 0)

| Extraction                                | New Method                         | Est. CYC Δ |
|-------------------------------------------|------------------------------------|------------|
| Master position reconstruction (340–442)  | `ReconstructMasterActivePosition()` | −10        |
| Position-to-stop matching loop (361–434)  | `TryMatchStopKeyForMasterPosition()` | −4        |
| Master order state-guard alignment        | align with `IsValidOrderState` + `Unknown` overload | −3 |

**Projected post-refactor cluster CYC:** ≤ 15 total (≤ 8 per method, ≤ 10 per helper).

---

## Key Constraints Carried Forward to Phase 2

- `_orderAdoptionComplete = true` assignment must remain unconditional; a `finally`-block
  pattern is strongly recommended to survive any early-return introduced during extraction.
- `OrderState.Unknown` acceptance in `AdoptMasterOrders` (Build 994 NT8 Sim workaround) must
  be preserved; it must not be silently dropped when aligning with `IsValidOrderState`.
- Both caller paths (startup via `EnumerateApexAccounts` and reconnect via
  `ProcessOnConnectionStatusUpdate`) must continue to work without preconditions on each other.

---

## Agent Tracking

| Field            | Value                  |
|------------------|------------------------|
| Agent Name       | v12-phase1-scope       |
| Phase            | 1 — Scope Definition   |
| Epic             | EPIC-W7-059            |
| Wave             | 7                      |
| Input artifacts  | 00-hotspots.md         |
| Output artifact  | 00-scope.md            |
