# Phase 2: Architecture Plan — EPIC-W7-074

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-074/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `AttachExecutionPanelHandlers`
- **Source File:** [`src/V12_002.UI.Panel.Handlers.cs`](../../src/V12_002.UI.Panel.Handlers.cs:96)
- **Original CYC:** 12
- **Lines:** 96–149 (53 lines)

### jcodemunch get_context_bundle result

Symbol resolved: `src/V12_002.UI.Panel.Handlers.cs::V12_002.AttachExecutionPanelHandlers#method`
Signature: `private void AttachExecutionPanelHandlers()`
Body: 53 lines — 11 null-guard if-branches, each conditionally subscribing a `Button.Click` event.
Pattern: 6 inline lambdas (each calls `PanelCommand` + optional `ResetExecutionMode` + `TriggerGlow`) and 5 existing named delegates (`OnRetestClick`, `OnRetestRmaToggleClick`, `OnRmaClick`, `OnTrendClick`, `OnTrendRmaToggleClick`).
Imports: System.Windows.Controls (Button), System.Windows (RoutedEventHandler), WPF event infrastructure.

### jcodemunch get_call_hierarchy result

- **Callers (depth=1):** 1 — `AttachPanelHandlers` at [`src/V12_002.UI.Panel.Handlers.cs:42`](../../src/V12_002.UI.Panel.Handlers.cs:42) (ast_resolved)
- **Direct callees (depth=1):** `PanelCommand` (line 935), `ResetExecutionMode` (line 558), `TriggerGlow` (src/V12_002.UI.Panel.Lifecycle.cs:114)
- **Indirect callees (depth=2):** `Enqueue` (src/V12_002.cs:428), `ClearClickTraderBorderIfInactive` (src/V12_002.UI.Callbacks.cs:219), `UpdateRmaButtonVisual` (line 869), `_glowTimer` (Panel.Lifecycle.cs:16)
- State mutations flow through `ResetExecutionMode` → `Enqueue` (Actor/FSM pattern — confirmed lock-free)

### jcodemunch get_dependency_graph result

- **File edges:** 0 import/importer edges detected (partial class — C# partial classes are not resolved as file-level imports by the index)
- **Blast radius:** Confirmed within single source file + lifecycle file for `TriggerGlow`; no new cross-file dependencies introduced by extraction

### jcodemunch get_extraction_candidates result

- **Candidates returned:** 0 (min_complexity=3, min_callers=1)
- **Interpretation:** No existing sub-functions meet the threshold for extraction; the method is a monolithic registration body. Confirms this is a greenfield private-helper extraction with no pre-existing decomposition to leverage.

---

## Sequential Thinking Summary

**Thought 1 — Problem analysis:** Method complexity is entirely structural — 11 repetitive null-guards and 6 inline lambda closures. No business logic, no nested loops, no switch chains. Primary strategy: Extract Guard Clauses (R1) + Extract Named Helper Methods (R2).

**Thought 2 — Helper design:** 6 inline lambdas become 6 named `private void On*Click(object s, RoutedEventArgs e)` handlers. Each has CYC=1 (no branches — straight-line command dispatch). Plus 1 `BindClick(Button, RoutedEventHandler)` guard helper (CYC=2). 7 total new helpers.

**Thought 3 — Parent CYC:** After extraction, parent body = 11 `BindClick(...)` calls. Zero conditionals remain. CYC=1.

**Thought 4 — Jane Street compliance:** All 5 JS rules satisfied. CYC<=8 achieved (max=2). SRP per helper. Lock-free Actor path preserved via existing `ResetExecutionMode` → `Enqueue` chain. Named handlers eliminate 6 heap-allocated lambda closures.

**Thought 5 — Final verdict:** extraction_count=7, max_cyc_projected=2. Architecture safe to implement in Phase 5.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `BindClick(Button btn, RoutedEventHandler handler)` | Null-safe event subscription — if btn != null, attaches handler to btn.Click | 2 |
| `OnOrLongClick(object s, RoutedEventArgs e)` | OR_LONG command: PanelCommand("OR_LONG") + ResetExecutionMode() + TriggerGlow(CyanAccent) | 1 |
| `OnOrShortClick(object s, RoutedEventArgs e)` | OR_SHORT command: PanelCommand("OR_SHORT") + ResetExecutionMode() + TriggerGlow(PinkFg) | 1 |
| `OnMomoClick(object s, RoutedEventArgs e)` | MOMO mode command: PanelCommand("MODE_MOMO") + ResetExecutionMode() + TriggerGlow(GreenFg) | 1 |
| `OnFfmaClick(object s, RoutedEventArgs e)` | FFMA mode command: PanelCommand("MODE_FFMA") + ResetExecutionMode() + TriggerGlow(PinkFg) | 1 |
| `OnFfmaManualClick(object s, RoutedEventArgs e)` | FFMA manual market command: PanelCommand("FFMA_MANUAL_MARKET") + ResetExecutionMode() + TriggerGlow(PinkFg) | 1 |
| `OnMClick(object s, RoutedEventArgs e)` | M mode command: PanelCommand("MODE_M") + TriggerGlow(OrangeFg) | 1 |

---

## Parent Method After Extraction

**Remaining logic:** 11 sequential `BindClick(button, handlerMethod)` calls — one per UI button/toggle. No conditional branches. Pure registration dispatch.

```csharp
private void AttachExecutionPanelHandlers()
{
    BindClick(orLongButton, OnOrLongClick);
    BindClick(orShortButton, OnOrShortClick);
    BindClick(retestButton, OnRetestClick);
    BindClick(retestRmaToggle, OnRetestRmaToggleClick);
    BindClick(rmaButton, OnRmaClick);
    BindClick(momoButton, OnMomoClick);
    BindClick(ffmaButton, OnFfmaClick);
    BindClick(ffmaManualButton, OnFfmaManualClick);
    BindClick(mButton, OnMClick);
    BindClick(trendButton, OnTrendClick);
    BindClick(trendRmaToggle, OnTrendRmaToggleClick);
}
```

- **Projected CYC:** 1

---

## max_cyc_projected: 2
## extraction_count: 7

---

## Jane Street Alignment

| Rule | Status | Evidence |
|---|---|---|
| CYC<=8 achieved | YES | Parent CYC=1, BindClick CYC=2, all handlers CYC=1 — max=2 |
| Single-responsibility per helper | YES | BindClick: null-safe bind only. Each On*Click: single command dispatch only. |
| Lock-free/Actor pattern preserved | YES | ResetExecutionMode() routes through Enqueue (src/V12_002.cs:428) — no new lock() blocks introduced |
| Illegal states unrepresentable | YES | BindClick null-guard prevents null-reference at registration; no invalid handler state possible |
| Zero-allocation hot paths | YES | Named handlers replace 6 heap-allocated lambda closures; BindClick requires no closure capture |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 2.0 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **Method** | AttachExecutionPanelHandlers |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output** | docs/brain/EPIC-W7-074/02-architecture-plan.md |
