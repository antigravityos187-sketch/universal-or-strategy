# EPIC-W7-046 — Phase 1: Scope Definition

## Method in Scope

| Field            | Value                                                        |
|------------------|--------------------------------------------------------------|
| **Method Name**  | `HandleChartClick_ConvertPrice`                              |
| **Source File**  | `src/V12_002.UI.Callbacks.cs`                                |
| **Line Range**   | 272 – 353                                                    |
| **Visibility**   | `private bool` (partial class `V12_002 : Strategy`)          |
| **Current CYC**  | 12                                                           |
| **Target CYC**   | ≤ 8                                                          |
| **Wave / Phase** | Wave 7 / Phase 1                                             |

This is a **single method** refactor. The scope boundary is drawn tightly
around `HandleChartClick_ConvertPrice` and nothing else.

---

## Scope Boundary

The **scope boundary** encompasses exactly one method:
`HandleChartClick_ConvertPrice` in `src/V12_002.UI.Callbacks.cs` (lines 272–353).

All analysis, extraction planning, and refactor work in subsequent phases
operates exclusively within this scope boundary. No other methods, classes,
or files are modified as part of this epic unless they are newly extracted
helper methods produced directly by the refactor of this single method.

---

## Callers

A `grep` of `src/` for `HandleChartClick_ConvertPrice` found **2 matches**
across **1 file**:

| # | File                            | Line | Role                                    |
|---|---------------------------------|------|-----------------------------------------|
| 1 | `src/V12_002.UI.Callbacks.cs`   | 242  | **Call site** — invoked by `OnChartClick` |
| 2 | `src/V12_002.UI.Callbacks.cs`   | 272  | **Declaration** of the method itself    |

**Direct caller count: 1** (`OnChartClick`, line 231).  
`OnChartClick` is the WPF `PreviewMouseLeftButtonDown` handler and is the sole
entry point into `HandleChartClick_ConvertPrice`. No external class or
assembly calls this method.

---

## Why Other Methods Are NOT in Scope

Per convention **V12.23** (single-responsibility refactor isolation), each
Wave-7 epic targets exactly one high-CYC method. Neighbouring methods in the
click-handling chain — `OnChartClick`, `HandleChartClick_ValidateMode`,
`HandleChartClick_ExecuteMomo`, `HandleChartClick_ExecuteRma` — all carry
CYC scores below the intervention threshold (CYC < 8) and therefore fall
outside this epic's scope boundary.

Specifically, V12.23 states that blast-radius expansion beyond the nominated
single method requires a separate epic ticket. Pulling sibling methods into
scope here would violate that rule, introduce unrelated change risk on the
live order-execution path, and contaminate the CYC delta measurement used to
confirm refactor success.

The 3 extracted helper methods produced by this refactor
(`IsClickWithinChartBounds`, `ConvertYCoordToPrice`, `ValidatePriceInRange`)
are considered *outputs* of the scope, not additional methods *in* scope.

---

## CYC Reduction Plan (Summary)

| Extraction Target           | Lines Affected | Estimated CYC Reduction |
|-----------------------------|----------------|-------------------------|
| `IsClickWithinChartBounds`  | 289 – 297      | −4                      |
| `ConvertYCoordToPrice`      | 299 – 317      | −3                      |
| `ValidatePriceInRange`      | 338 – 350      | −3                      |

Post-refactor residual CYC of `HandleChartClick_ConvertPrice`: **≤ 3**,
which satisfies the target of ≤ 8 with significant margin.

---

## Agent Tracking

```json
{
  "agent_name":   "v12-phase1-scope",
  "epic":         "EPIC-W7-046",
  "wave":         7,
  "phase":        1,
  "phase_name":   "Scope Definition",
  "status":       "completed",
  "output":       "docs/brain/EPIC-W7-046/00-scope.md",
  "method":       "HandleChartClick_ConvertPrice",
  "source_file":  "src/V12_002.UI.Callbacks.cs",
  "cyc_current":  12,
  "cyc_target":   8,
  "callers_count": 1,
  "scope_confirmed_single_method": true,
  "scope_boundary": "HandleChartClick_ConvertPrice (lines 272-353) only",
  "out_of_scope_rule": "V12.23 — single-responsibility refactor isolation",
  "generated_by": "Bob (jcodemunch wave-7 scope pipeline)",
  "timestamp":    "2025-07-14T00:00:00Z"
}
```
