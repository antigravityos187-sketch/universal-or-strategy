# EPIC-W7-152 Hotspot Analysis

**Method:** `TryApplyConfigTarget_Value`
**CYC (tool-supplied):** 0 ⚠️ — see note below
**CYC (manual static analysis):** 17
**File:** `src/V12_002.UI.IPC.Commands.Config.cs`
**Lines:** 209–297

> ⚠️ **Manual Review Required — CYC Discrepancy**
> The task specification supplied `CYC: 0`, which would indicate either a non-locatable method
> or a tooling stub value. The method **was successfully located** at line 209 of the source file.
> Static analysis of the method body yields **CYC = 17** (17 decision nodes including the base
> node). The `0` value is assessed as a tooling placeholder, not a true score. All downstream
> phase planning should treat CYC = 17 as the operative value.

---

## Overview

`TryApplyConfigTarget_Value` is a config sub-handler extracted during Build 945 (CS-R1140) from
the original monolithic `HandleConfigCommand`. Its sole responsibility is to match string key
`T1`–`T5` or `CIT` and, for numeric targets, validate and assign the corresponding `TargetNValue`
property. Despite its "extracted" status, the method has accumulated CYC = 17 through unrolled
per-target chains: each of the five targets (T1–T5) contributes an identical triple of
`if (key == "Tn")` → `double.TryParse` → `ValidateIpcMultiplier` → assignment, producing a
structurally repetitive but individually shallow complexity fan-out.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `TryApplyConfigTargets` (line 198, same file) |
| **Caller chain** | `HandleConfigCommand` → `TryApplyConfigTargets` → `TryApplyConfigTarget_Value` |
| **Sibling sub-handlers** | `TryApplyConfigTarget_Type`, `TryApplyConfigTarget_Count`, `TryApplyConfigRisk`, `TryApplyConfigMode` |
| **State written** | `Target1Value`, `Target2Value`, `Target3Value`, `Target4Value`, `Target5Value`, `ChaseIfTouchPoints` |
| **Validation dependency** | `ValidateIpcMultiplier(double, out string)` — shared guard used by `TryApplyConfigRisk` as well |
| **Side-effects** | `Print(...)` rejection log on invalid multiplier; no position mutations, no broker calls |
| **Threading constraint** | IPC handler thread; state assignments to `TargetNValue` properties assumed strategy-thread-safe |
| **Risk on change** | Low-Medium — pure key→property dispatch, no branching logic beyond parse+validate; risk is regression across all 5 targets if the shared pattern is broken |

**Affected symbol count (blast radius):** 3 direct callers/callees; 6 properties written; 1 shared validator.

---

## Top 3 Complexity Drivers

1. **Unrolled per-target key dispatch (5× identical `if` chains)**
   Each of T1–T5 is matched by its own `if (key == "Tn")` guard (lines 211, 232, 248, 264, 280),
   making this a flat 5-arm chain rather than a `switch` or data-driven dispatch table. Every arm
   contributes +1 CYC at the key-match level alone. A `switch (key)` with fall-through cases, or a
   `Dictionary<string, Action<double>>` dispatch table, would collapse all five arms to a single
   decision point. Sub-total: **5 CYC points** purely from the sequential if-chain.

2. **Nested `TryParse` + `ValidateIpcMultiplier` double-condition per target**
   Inside each of the five target arms, a `double.TryParse` guard and an inner
   `!ValidateIpcMultiplier` guard are nested (lines 213+216, 234+237, 250+253, 266+269, 282+285).
   Each pair contributes +2 CYC × 5 targets = **10 CYC points**. The guard pattern is structurally
   identical across all five arms with only the property name differing — a clear signal that the
   logic is a candidate for a single parameterised helper such as
   `TryApplyValidatedTargetValue(string key, string val, string label, Action<double> assign)`.

3. **`CIT` special-case interspersed in numeric target chain (structural noise)**
   The `CIT` key (line 227) is a string-typed assignment (`ChaseIfTouchPoints = val`) injected
   between the T1 and T2 numeric arms. Its presence breaks the uniform numeric-target pattern and
   forces any reader to context-switch mid-chain, adding cognitive complexity disproportionate to
   its single-line logic. While it contributes only +1 CYC, it is the primary readability blocker
   and should be the first arm separated into its own early-return guard or relocated to a
   `TryApplyConfigTarget_String` sibling.

---

## Recommended Extraction Count

**2 targeted refactors recommended; 0 full extractions strictly required for CYC compliance.**

| Recommendation | Description | CYC reduction |
|---|---|---|
| **R1 — Introduce `ApplyValidatedTargetValue` helper** | Extract the `TryParse` + `ValidateIpcMultiplier` + assign triple into a single private helper called from all five arms. Reduces 10 nested CYC points to ~2 (one call + one if-guard on helper return). | −8 CYC |
| **R2 — Replace if-chain with `switch` or dispatch table** | Convert the 5-arm `if (key == "Tn")` chain to a `switch (key)` statement or a static `Dictionary<string, string>` key→property map. Eliminates 4 of the 5 key-match CYC points. | −4 CYC |

**Post-refactor projected CYC:** ~5 (switch base + CIT arm + helper-call arm + parse-fail path + validate-fail path).

**Rationale:** The method is not a latent bug risk — it is mechanically repetitive. Phase 1 work
should treat this as a pattern-extraction exercise rather than a logic decomposition. Both
recommendations can be applied in a single PR with no behaviour change.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.2 |
| **Execution Time** | ~60s |
| **MCP Tools Invoked** | `jcodemunch` tools not available in session — static analysis performed manually via `read_file` + `grep` |
| **CYC Source** | Manual static analysis (decision-point count); task-supplied value 0 flagged as tooling stub |
| **Review Flag** | `requires-manual-review` — CYC discrepancy between task spec (0) and observed code (17) |
