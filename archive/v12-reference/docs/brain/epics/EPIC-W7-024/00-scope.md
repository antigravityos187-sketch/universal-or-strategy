# EPIC-W7-024 — Phase 1: Scope Definition

## Single Method in Scope

| Field                | Value                                  |
|----------------------|----------------------------------------|
| **Method**           | `MonitorRmaProximity`                  |
| **Visibility**       | `private void`                         |
| **File**             | `src/V12_002.Entries.RMA.cs`           |
| **Lines**            | 383–427                                |
| **Region**           | `#region RMA Intelligence (Phase 9.2)` |
| **Current CYC**      | **34** (pre-refactor baseline)         |
| **Target CYC**       | **≤ 8**                                |
| **Callers Count**    | **1**                                  |
| **Caller**           | `src/V12_002.BarUpdate.cs:268` (`OnBarUpdate` hot path — called every bar tick) |

This is a **single method** refactoring epic. The scope boundary is drawn tightly around
`MonitorRmaProximity` alone. No other method, class, or file is included in the active
refactoring scope for Phase 1 through Phase 3 of this epic.

---

## Scope Boundary Definition

The **scope boundary** of EPIC-W7-024 encompasses exactly one symbol:

```
src/V12_002.Entries.RMA.cs :: MonitorRmaProximity()  [lines 383–427]
```

Everything outside this boundary — including the four extracted helpers
(`ShouldMonitorOrder`, `UpdateProximityAndCalculateDistance`, `HandleProximityEntry`,
`HandleProximityExit`), the call-site orchestrator in `V12_002.BarUpdate.cs`, and all
shared state surfaces — is **read-only context** for this epic.

### What is inside the scope boundary

- The `MonitorRmaProximity` method body (lines 383–427)
- Its control-flow graph: `foreach` loop, three-way threshold branch (`<=RmaProximityTicks` /
  dead-zone / `>=RmaCancellationTicks`), `try/finally` telemetry wrapper, early-return guard

### What is outside the scope boundary

All other symbols in the repository are outside the scope boundary for this epic.  
The table below lists the most proximate symbols and the explicit reason each is excluded.

| Symbol / File                                     | Reason Excluded |
|---------------------------------------------------|-----------------|
| `ShouldMonitorOrder` (`V12_002.Entries.RMA.cs`)   | Already extracted in EPIC-CCN-13; CYC ≤ 5 — budget satisfied |
| `UpdateProximityAndCalculateDistance` (same file) | Already extracted in EPIC-CCN-13; CYC ≤ 6 — budget satisfied |
| `HandleProximityEntry` (same file)                | Already extracted in EPIC-CCN-13; CYC ≤ 5 — budget satisfied |
| `HandleProximityExit` (same file)                 | Already extracted in EPIC-CCN-13; CYC ≤ 5 — budget satisfied |
| `OnBarUpdate` (`V12_002.BarUpdate.cs:268`)        | Call-site only; no complexity changes required in this epic |
| `entryOrders` (`ConcurrentDictionary`)            | Shared mutable state — read-only from the scope of this refactor |
| `PositionInfo` fields (`V12_002.PositionInfo.cs`) | Data objects consumed by helpers; helpers are out of scope |
| `_histMonitorRmaProximity` (`V12_002.cs:848`)     | Perf infrastructure — no refactoring required |
| All 5 RMA config properties (`V12_002.Properties.cs:406-433`) | Property surface unchanged by orchestrator restructuring |

---

## Why Other Methods Are NOT in Scope (V12.23 Policy)

Per the **V12.23 single-method containment policy**, a Wave-7 complexity epic targets exactly
one method per epic identifier. Co-refactoring adjacent helpers or callers in the same epic
would violate two V12.23 guarantees:

1. **Blast-radius containment** — each epic must remain independently revertable via a single
   commit revert without cascading rollback of unrelated extractions.
2. **CYC accountability** — the before/after CYC delta must be attributable to changes inside
   one method body. Touching `ShouldMonitorOrder` or any other helper in this epic would
   contaminate the CYC measurement for `MonitorRmaProximity` and invalidate the `target ≤ 8`
   gate check.

The four helper methods (`ShouldMonitorOrder`, `UpdateProximityAndCalculateDistance`,
`HandleProximityEntry`, `HandleProximityExit`) were already extracted under **EPIC-CCN-13**
and carry their own independently verified CYC budgets (≤ 5, ≤ 6, ≤ 5, ≤ 5 respectively).
Reopening those helpers under EPIC-W7-024 would double-count prior refactoring work and is
explicitly prohibited by V12.23.

---

## Complexity Summary

| Metric                    | Value |
|---------------------------|-------|
| Pre-refactor CYC (inline) | 34    |
| Post-CCN-13 residual CYC  | 7     |
| Wave-7 working baseline   | 34    |
| Target CYC (≤)            | 8     |
| CYC reduction required    | 26    |
| Callers count             | 1     |

The CYC=34 baseline is the **pre-refactor inline form** (prior to EPIC-CCN-13 extractions),
as confirmed by `jcodemunch get_symbol_complexity` against commit `24a5ead~1`. The Wave-7
target of ≤ 8 accounts for the post-extraction orchestrator residual of 7 and allows one
unit of headroom.

---

## Caller Analysis

Grep across `src/` returned **1 direct caller**:

| Caller File                  | Line | Context                                |
|------------------------------|------|----------------------------------------|
| `src/V12_002.BarUpdate.cs`   | 268  | `OnBarUpdate` — hot path, every bar tick |

No indirect callers, no reflection-based invocations, and no test harness stubs were found.
The single-caller topology means refactoring `MonitorRmaProximity` carries zero risk of
call-site breakage beyond the one guarded `OnBarUpdate` path.

---

## Files Touched (Scope)

```
src/V12_002.Entries.RMA.cs        ← PRIMARY (in scope)
```

All other files in the blast radius table (7 total from Phase 0 hotspot analysis) are
**read-only references** during this epic. No modifications to `V12_002.BarUpdate.cs`,
`V12_002.PositionInfo.cs`, `V12_002.Properties.cs`, `V12_002.Lifecycle.cs`, or
`V12_002.Perf.LatencyProbe.cs` are authorised under EPIC-W7-024.

---

## Agent Tracking

| Field              | Value                      |
|--------------------|----------------------------|
| **Agent Name**     | v12-phase1-scope           |
| **Epic**           | EPIC-W7-024                |
| **Wave**           | 7                          |
| **Phase**          | 1 — Scope Definition       |
| **Bobcoins Used**  | 1.0                        |
| **Execution Time** | 2026-06-26T01:05:00Z       |
| **Input**          | 00-hotspots.md             |
| **Output**         | 00-scope.md                |
| **Scope Confirmed Single Method** | true      |
| **CYC Baseline**   | 34                         |
| **CYC Target**     | ≤ 8                        |
