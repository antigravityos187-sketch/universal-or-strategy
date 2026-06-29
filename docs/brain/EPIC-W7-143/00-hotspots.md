# EPIC-W7-143 Hotspot Analysis

**Method:** OnKeyDown
**CYC (assigned):** 0 — tooling artefact; see manual analysis below
**CYC (manual McCabe):** 8 (strict) / 4 (lenient, branch-only)
**File:** src/V12_002.UI.Callbacks.cs
**Location:** lines 391–426

> ⚠️ **Manual Review Flag:** The task header specifies CYC=0, indicating the upstream tool
> (`mcp__jcodemunch-mcp__get_symbol_complexity`) either could not locate or failed to score this
> method. The method was located via direct file inspection. CYC=0 is confirmed as a tooling
> artefact — the method contains multiple branches and the true complexity is documented below.

---

## Overview

`OnKeyDown` is the WPF `PreviewKeyDown` event handler registered via [`AttachHotkeys()`](src/V12_002.UI.Callbacks.cs:44).
It is the primary keyboard-dispatch entry point for the UI layer. It routes key events to three
action families: basic hotkeys via a pre-allocated command dictionary, T1/T2 target actions, and
runner actions. A prior refactor ([Phase7-UI T-A]) already extracted `HandleTargetAction` and
`HandleRunnerAction` to reduce its original complexity, leaving the current residual dispatcher.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Event source** | `ChartControl.OwnerChart.PreviewKeyDown` (WPF UI thread) |
| **Registered by** | [`AttachHotkeys()`](src/V12_002.UI.Callbacks.cs:44) / [`DetachHotkeys()`](src/V12_002.UI.Callbacks.cs:53) |
| **Direct callees** | `_keyCommands.TryGetValue` (O(1) dictionary), [`HandleTargetAction`](src/V12_002.UI.Callbacks.cs:429), [`HandleRunnerAction`](src/V12_002.UI.Callbacks.cs:455) |
| **Transitive callees** | `ExecuteTargetAction` → `ExecuteTargetActionForPosition` → 6 target action methods; `ExecuteRunnerAction` → `DispatchRunnerAction` → 6 runner action methods |
| **Shared state read** | `_keyCommands` (Dictionary, pre-allocated), `Keyboard` (WPF static, UI thread) |
| **Side-effects** | Delegates all mutations to `HandleTargetAction` / `HandleRunnerAction`; sets `e.Handled = true` on all matched branches |
| **Threading constraint** | WPF UI thread only (PreviewKeyDown guarantee); downstream runner/target actions enqueue via `Enqueue()` to the strategy thread |
| **Risk on change** | Low-Medium — dispatcher itself is thin; risk is concentrated in the two routed helpers and their downstream chains |

**Affected symbol count (blast radius):** 3 direct callees; ~14 transitive symbols across the Target & Runner Action subsystem.

---

## Top 3 Complexity Drivers

1. **Triple parallel `if`-with-OR branch cascade (primary CYC source)**
   The three `if (Keyboard.IsKeyDown(Key.Dx) || Keyboard.IsKeyDown(Key.NumPadx))` blocks at
   lines 402–423 are structurally identical. Each contributes +2 CYC (one for the `if`, one
   for the `||` short-circuit). All three test disjoint key ranges (D1/NP1, D2/NP2, D3/NP3)
   and could be collapsed into a `switch` on a numeric key ordinal or a helper
   `TryResolveNumericKey(Key, out int n)` to reduce to a single branch point.
   **Sub-total: 6 CYC from this pattern.**

2. **Dictionary-guarded hotkey lookup with null-check preamble**
   Line 394 (`if (_keyCommands != null && _keyCommands.TryGetValue(...)`) combines a null-guard
   and an O(1) lookup into a single compound boolean expression. This is the correct idiom for
   zero-allocation dispatch but contributes +1 CYC to the method. The `&&` short-circuit means
   `_keyCommands` can never be null-dereferenced, but it also means the null-guard is only
   necessary if `_keyCommands` is not guaranteed initialised before `AttachHotkeys()` is called.
   A field initializer would eliminate the null-check branch entirely.
   **Sub-total: 1 CYC from this pattern.**

3. **Early-return guards on all four branches (structural exit fan-out)**
   Each matched branch terminates with `e.Handled = true; return;`, creating four distinct exit
   paths from the method. While this is idiomatic and readable, it means any future branch addition
   must replicate the `e.Handled` assignment — a copy-paste risk. Extracting a small
   `DispatchAndHandle(Action action, KeyEventArgs e)` helper would centralise that assignment.
   **Sub-total: structural risk, +0 CYC, but increases maintenance CYC over time.**

---

## Recommended Extraction Count

**0 immediate extractions required; 1 optional consolidation recommended for Phase 1.**

**Rationale:**

The T-A refactor already extracted the heavy logic into `HandleTargetAction` (CYC 6) and
`HandleRunnerAction` (CYC 6). `OnKeyDown` itself is a thin dispatcher. Its true CYC of 8
(strict) or 4 (lenient) is within acceptable bounds for an event handler at this level of
responsibility.

**Optional Phase 1 item:**
- Extract `TryResolveNumericKey(Key key, out int slot)` to collapse the three near-identical
  `if (IsKeyDown(Dx) || IsKeyDown(NumPadx))` blocks into a `switch (slot)` dispatch, reducing
  the method's McCabe CYC from 8 → 4 (strict) and improving extensibility when T3/T4/T5 are
  added.

**What NOT to change:**
- The `_keyCommands` dictionary lookup (Phase7-UI T-A) is intentional zero-allocation design;
  leave it as-is.
- The `e.Handled = true` assignments are required WPF event routing semantics; do not abstract
  them away without a careful understanding of tunnelling/bubbling implications.

---

## Agent Tracking

Agent Name: v12-phase0-hotspot | Bobcoins Used: 1.2 | Execution Time: ~90s
