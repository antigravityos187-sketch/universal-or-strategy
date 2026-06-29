# EPIC-W7-056 — Phase 1: Scope Definition

## Single Method in Scope

This refactoring epic targets exactly one **single method**: `SweepBrokerOrders`.  
The scope boundary is drawn tightly around this method and does not extend to any sibling,
caller, or downstream helper unless a new private extraction is introduced as a direct product
of refactoring `SweepBrokerOrders` itself.

| Field                   | Value                                  |
|-------------------------|----------------------------------------|
| **Method**              | `SweepBrokerOrders(bool force)`        |
| **File**                | `src/V12_002.SIMA.Lifecycle.cs`        |
| **Lines (current)**     | 1360–1454                              |
| **Visibility**          | `private`                              |
| **Current CYC**         | 28                                     |
| **Target CYC**          | ≤ 8                                    |
| **Direct Callers**      | 1 (`CancelAllV12GtcOrders`, line 1297) |
| **Grep caller matches** | 2 total (1 definition + 1 call site)   |

---

## Scope Boundary

The **scope boundary** is defined as follows:

- **IN scope:** The body of `SweepBrokerOrders` (lines 1360–1454 of
  `src/V12_002.SIMA.Lifecycle.cs`) and any new private helper methods extracted directly
  from it during Phase 2 (Decomposition).
- **OUT of scope:** All callers (`CancelAllV12GtcOrders`, `ProcessShutdownSIMA`,
  `V12_002.Lifecycle.cs:216`), all sibling sweep methods (`SweepTrackedOrders`), and all
  downstream subsystems listed in the blast-radius analysis (Fleet, Flatten, Execution,
  REAPER, Orders callbacks).

The scope boundary is enforced by the V12.23 rule described below.

---

## Why Other Methods Are NOT in Scope (V12.23)

**V12.23** — *One method per epic, one epic per wave slot.*

The V12 refactoring protocol mandates that each wave epic targets a **single method** in
isolation. Expanding scope beyond `SweepBrokerOrders` in this wave would violate V12.23 for
the following reasons:

1. **`SweepTrackedOrders`** (Phase 1/2 sibling in the same file) is a structurally distinct
   sweep that operates on tracked-order state rather than broker-visible order state. Its
   control flow, prefix logic, and guard clauses differ from `SweepBrokerOrders`. Refactoring
   both simultaneously would interleave two independent decision graphs, making review and
   rollback unreliable. V12.23 prohibits this co-mingling.

2. **`CancelAllV12GtcOrders`** (the sole direct caller) orchestrates the Phase 1→Phase 2
   sweep sequence. Its logic is intentionally thin — it delegates complexity to
   `SweepTrackedOrders` and `SweepBrokerOrders`. Modifying the caller alongside the callee
   in the same wave would obscure whether CYC reductions originated from decomposition or
   from caller restructuring, violating the single-variable principle of V12.23.

3. **All blast-radius subsystems** (`V12_002.SIMA.Fleet.cs`, `V12_002.SIMA.Flatten.cs`,
   `V12_002.SIMA.Execution.cs`, `V12_002.REAPER.Audit.cs`, orders callbacks) are downstream
   consumers of order-state side effects. Their contracts must remain stable as the observable
   behaviour of `SweepBrokerOrders` is preserved unchanged. Modifying them in the same wave
   would break the contract-stability guarantee that V12.23 requires.

---

## Complexity Reduction Plan (Summary)

Three extractions are planned to drive residual CYC from 28 to ≤ 8:

| # | Extracted Method                           | Est. CYC Reduction |
|---|--------------------------------------------|--------------------|
| 1 | `BuildSweepPrefixes(bool force): string[]` | −2                 |
| 2 | `IsCancellableOrderState(Order ord): bool` | −5                 |
| 3 | `IsProtectedBracketOrder(string name): bool` | −8               |

Residual `SweepBrokerOrders` CYC after all extractions: **≈ 13** (outer loop + instrument
guard + prefix match loop + isV12 flag + try/catch).

> **Note:** The hotspot analysis (Phase 0) projects a residual of ≈13 after 3 extractions.
> Additional micro-extractions or loop-guard consolidations may be applied in Phase 2 to reach
> the hard target of ≤ 8. This scope document commits only to the **single method** and the
> **three primary extractions**; exact residual CYC is validated in Phase 3 (Verification).

---

## Caller Graph

```
V12_002.Lifecycle.cs:216 (force=true)  ─┐
ProcessShutdownSIMA()    (force=false) ─┤─► CancelAllV12GtcOrders(bool force)
                                         └─────────────────────────────────────►  SweepBrokerOrders(bool force)  ◄── SCOPE BOUNDARY
```

Direct caller count: **1** (`CancelAllV12GtcOrders`).  
The two upstream call sites do not call `SweepBrokerOrders` directly; they are separated by
one indirection layer and are outside the scope boundary.

---

## Agent Tracking

```
epic_id    : EPIC-W7-056
wave       : 7
phase      : 1
status     : completed
agent      : v12-phase1-scope
timestamp  : 2025-06-13T00:00:00Z
output     : docs/brain/EPIC-W7-056/00-scope.md
method     : SweepBrokerOrders
source     : src/V12_002.SIMA.Lifecycle.cs:1360
cyc_current: 28
cyc_target : <=8
callers    : 1
```
