# EPIC-W7-020 — Phase 1: Scope Definition

## Single Method in Scope

**`HandleSecondaryOrderFilled`**
File: `src/V12_002.Orders.Callbacks.cs` (definition at line 571)

This is a **single method** scope. The refactoring target is the `HandleSecondaryOrderFilled`
logical unit — the parent routing method and its three Phase-7-NEW-1-extracted sub-handlers
(`_Target`, `_Stop`, `_TerminalCleanup`), which together form a single coherent responsibility:
dispatching and processing all non-entry order fills for NinjaTrader 8.

---

## Complexity Budget

| Metric | Value |
|---|---|
| **Current CYC** | 34 (pre-extraction aggregate baseline) |
| **Target CYC** | ≤ 8 (per-method ceiling after planned extractions) |
| **CYC Reduction Required** | ≥ 26 points across the logical unit |
| **Strategy** | 2 additional targeted extractions (see `00-hotspots.md`) |

The CYC=34 figure is the *aggregate pre-extraction complexity* for the entire logical unit.
Post-Phase-7-NEW-1 the parent router sits at ≈4, but the sub-handlers still carry elevated
complexity (`_Target` ≈8, `_Stop` ≈6). The target of ≤8 **per method** is the governing
constraint.

---

## Scope Boundary

The **scope boundary** is drawn at the `HandleSecondaryOrderFilled` logical unit boundary.

### What IS in scope

| Symbol | File | Lines | Role |
|---|---|---|---|
| `HandleSecondaryOrderFilled` | `src/V12_002.Orders.Callbacks.cs` | 571–597 | Parent router (in scope as single method owner) |
| `HandleSecondaryOrderFilled_Target` | `src/V12_002.Orders.Callbacks.cs` | 427–477 | T1–T5 target fill sub-handler |
| `HandleSecondaryOrderFilled_Stop` | `src/V12_002.Orders.Callbacks.cs` | 489–546 | Stop fill / position teardown sub-handler |
| `HandleSecondaryOrderFilled_TerminalCleanup` | `src/V12_002.Orders.Callbacks.cs` | 554–569 | Ghost reference removal sub-handler |

The three sub-handlers are included within the scope boundary because they were extracted
from `HandleSecondaryOrderFilled` in Phase 7 NEW-1. They are the direct implementation of
the **single method**'s original body and cannot be meaningfully treated as separate
responsibilities. All four symbols reside in the same file and class partition.

### What is NOT in scope

All other methods in the codebase — including all V12.23 subsystem methods — are **outside
the scope boundary** for this epic. Specifically:

- **V12.23 methods are not in scope.** V12.23 refers to the broader V12 version-23
  subsystem surface including all `Orders.Management.*`, `SIMA.*`, `UI.*`, `Symmetry.*`,
  and `Flatten.*` files. These files host transitive callees
  (`ApplyTargetFill`, `UpdateStopQuantity`, `CleanupPosition`, `GetTargetOrdersDictionary`,
  etc.) that are *called by* `HandleSecondaryOrderFilled` but are not themselves complexity
  targets. They sit outside the scope boundary because:
  1. Their CYC scores are below the remediation threshold.
  2. Modifying them would expand blast radius unnecessarily across 8+ source files.
  3. The refactoring objective is targeted: reduce the complexity of **this single method**
     without touching the API surface of its callees.
  4. V12.23 subsystem methods have their own ownership and change-gate requirements
     independent of this epic.

---

## Callers Analysis

Caller count established by static grep over all `*.cs` source files.

| Caller | File | Line | Call Type |
|---|---|---|---|
| `HandleOrderState_Filled` | `src/V12_002.Orders.Callbacks.cs` | 218 | Direct call (else-branch, non-entry fill path) |

**Total direct callers: 1**

`HandleOrderState_Filled` is the sole caller of `HandleSecondaryOrderFilled`. It is reached
via the NT8 hot-path chain:

```
OnOrderUpdate → Enqueue → ProcessOnOrderUpdate
             → HandleOrderState_Filled → HandleSecondaryOrderFilled
```

There are no cross-file callers. This means all refactoring changes are **fully contained**
within `src/V12_002.Orders.Callbacks.cs`. No external call sites will be broken or require
signature updates.

Internal call sites within the logical unit (sub-handler dispatch from the parent router):

| Internal Call Site | Line |
|---|---|
| `HandleSecondaryOrderFilled_Target(...)` | 579 |
| `HandleSecondaryOrderFilled_Stop(...)` | 585 |
| `HandleSecondaryOrderFilled_TerminalCleanup(...)` | 591 |

These are internal to the scope boundary and not counted as external callers.

---

## Why V12.23 Methods Are Not In Scope

The V12.23 designation covers all subsystem helpers that `HandleSecondaryOrderFilled`
calls transitively. These methods are **explicitly excluded** from this epic's scope for
the following reasons:

1. **Complexity is localised.** CYC=34 is attributable to decision trees *within* the
   `HandleSecondaryOrderFilled` logical unit. The transitive callees are mostly thin helpers
   (CYC ≤ 4 each).

2. **Change containment.** Keeping V12.23 methods out of scope limits the blast radius to a
   single file (`src/V12_002.Orders.Callbacks.cs`) and avoids cascading test obligations
   across the Order, SIMA, UI, and Symmetry subsystems.

3. **Single responsibility.** The epic's mission is to bring `HandleSecondaryOrderFilled`
   to CYC ≤ 8 per method. Refactoring callees is a separate concern that requires its own
   epic, hotspot analysis, and approval gate.

4. **Regression risk.** `CleanupPosition` and `UpdateStopQuantity` are flagged CRITICAL in
   the blast-radius analysis. Touching them outside a dedicated epic without full regression
   coverage would be imprudent.

---

## Planned Extractions (Preview for Phase 2)

| # | New Method | Source Sub-handler | Estimated CYC Δ |
|---|---|---|---|
| 1 | `ProcessTargetFillForPosition(key, pos, tNum, order, averageFillPrice)` | `_Target` inner loop body | ≈8 → ≤4 |
| 2 | `TryResolveStopOrder(order, orderName, snapshot, out string entryKey)` | `_Stop` dual-path resolver | ≈6 → ≤3 |

Both extractions are fully contained within the scope boundary. No callee APIs are modified.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase1-scope |
| **Epic** | EPIC-W7-020 |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Source File** | `src/V12_002.Orders.Callbacks.cs` |
| **Method** | `HandleSecondaryOrderFilled` |
| **Current CYC** | 34 |
| **Target CYC** | ≤ 8 (per method) |
| **Callers Count** | 1 (`HandleOrderState_Filled`) |
| **Scope Type** | single method (logical unit incl. extracted sub-handlers) |
| **Output File** | `docs/brain/EPIC-W7-020/00-scope.md` |
| **V12.23 In Scope** | No — explicitly excluded (see rationale above) |
