# EPIC-W7-060 — Phase 1: Scope Definition

## Overview

This document defines the exact scope boundary for EPIC-W7-060. The refactoring
engagement covers a **single method** only: `SweepTrackedOrders` in
`src/V12_002.SIMA.Lifecycle.cs`. No other method is included in this epic's scope.

---

## Method in Scope

| Attribute         | Value                                                   |
|-------------------|---------------------------------------------------------|
| Method            | `SweepTrackedOrders`                                    |
| Signature         | `private int SweepTrackedOrders(bool force)`            |
| Class             | `V12_002` (partial)                                     |
| Namespace         | `NinjaTrader.NinjaScript.Strategies`                    |
| File              | `src/V12_002.SIMA.Lifecycle.cs`                         |
| Lines             | 1308–1353                                               |
| Access            | `private`                                               |

---

## Complexity Targets

| Metric                  | Current (Phase 0 confirmed) | Target  |
|-------------------------|-----------------------------|---------|
| Cyclomatic Complexity   | **0**                       | **≤ 8** |

The current CYC of **0** was confirmed by the Phase 0 hotspot analysis
(`00-hotspots.md`). The method is a flat sweep loop with a single linear
execution path — no decision points that would independently increment the
cyclomatic complexity baseline. The `force` ternary at line 1313 is an
r-value initialisation expression, not a control-flow branch within the loop
body. CYC=0 is therefore structurally sound, and the target ceiling of ≤ 8
provides adequate headroom for any observability improvement applied in later
phases (e.g., converting the bare `catch {}` at line 1349 to a logged catch).

---

## Caller Count

`SweepTrackedOrders` has **1 direct caller** within the codebase:

| # | Caller                         | File                              | Line | Context                          |
|---|--------------------------------|-----------------------------------|------|----------------------------------|
| 1 | `CancelAllV12GtcOrders(bool)`  | `src/V12_002.SIMA.Lifecycle.cs`   | 1296 | Phase 1 of two-phase GTC sweep   |

Two additional **indirect** callers reach the method through
`CancelAllV12GtcOrders`:

| # | Indirect Caller            | File                              | Line |
|---|----------------------------|-----------------------------------|------|
| 1 | `ProcessShutdownSIMA()`    | `src/V12_002.SIMA.Lifecycle.cs`   | 100  |
| 2 | Strategy `OnTermination`   | `src/V12_002.Lifecycle.cs`        | 216  |

Both indirect callers are cold-path lifecycle events (shutdown / SIMA-disable).
The blast radius is contained within the two-phase GTC sweep subsystem.

---

## Scope Boundary

The **scope boundary** for EPIC-W7-060 is drawn around the single method
`SweepTrackedOrders` in `src/V12_002.SIMA.Lifecycle.cs` (lines 1308–1353).

Everything outside this boundary — including the direct caller
`CancelAllV12GtcOrders`, the indirect lifecycle callers, the downstream sink
`CancelOrderOnAccount`, and all seven tracked-order dictionaries
(`entryOrders`, `stopOrders`, `target1Orders` … `target5Orders`) — is **read-only
context** for this epic. No mutations to those symbols are permitted unless a
future phase explicitly widens the scope boundary via a new scope document.

---

## Why Other Methods Are NOT in Scope

### Rule V12.23 — Single-Method Scope Constraint

Per rule **V12.23** of the V12 refactoring protocol, an epic targeting a method
with CYC < 5 must restrict its scope to that **single method** unless the Phase 0
hotspot analysis explicitly flags a blast-radius spillover that cannot be resolved
within the method boundary. No such spillover was flagged for `SweepTrackedOrders`:

- `CancelAllV12GtcOrders` is a simple two-line coordinator; it requires no change.
- `ProcessShutdownSIMA` and `OnTermination` are lifecycle orchestrators that pass
  an unmodified `force` argument downstream; they require no change.
- `CancelOrderOnAccount` is the downstream sink; it is out of scope per V12.23
  and is treated as an opaque service boundary.

V12.23 therefore prohibits widening the scope boundary to any of these neighbours
for this epic. The **single method** constraint is both intentional and mandatory.

---

## Agent Tracking

```
Agent Name:   v12-phase1-scope
Epic:         EPIC-W7-060
Wave:         7
Phase:        1
Produced:     00-scope.md
Method:       SweepTrackedOrders
File:         src/V12_002.SIMA.Lifecycle.cs
CYC Current:  0
CYC Target:   <=8
Callers:      1 direct, 2 indirect
Scope Rule:   V12.23 (single method)
```

---

## Phase 1 Conclusion

- **Single method in scope:** `SweepTrackedOrders` (`src/V12_002.SIMA.Lifecycle.cs`)
- **Scope boundary confirmed:** lines 1308–1353; no neighbouring methods included
- **CYC:** 0 (current) → ≤ 8 (target ceiling)
- **Caller count:** 1 direct caller (`CancelAllV12GtcOrders`)
- **V12.23 constraint applied:** other methods excluded from scope
- **Output artifact:** `docs/brain/EPIC-W7-060/00-scope.md`
