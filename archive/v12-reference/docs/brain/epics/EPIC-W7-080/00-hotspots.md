# EPIC-W7-080 Hotspot Analysis

**Method:** PlacePanel
**CYC:** 13
**File:** src/V12_002.UI.Panel.Construction.cs

---

## Overview

`PlacePanel` is the three-path WPF visual-tree placement dispatcher that positions the V12
control panel inside the NinjaTrader 8 chart window. It embeds the panel via one of three mutually
exclusive strategies — (1) **Hijack**: overlay the native ChartTrader slot, (2) **Inject**: add a
new column to the ChartTab grid, or (3) **Fallback**: append to `UserControlCollection` — with a
built-in `DispatcherTimer` retry loop (up to 3 attempts) guarding the first two strategies against
a race condition in the WPF visual tree during strategy initialisation.

The method is called from [`CreatePanel()`](src/V12_002.UI.Panel.Construction.cs:236) on first
panel construction and recursively from the timer tick lambda on each retry. A
`_placementMode != PanelPlacement.None` guard prevents double-placement after a successful path.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `CreatePanel` (line 236, same file); `DispatcherTimer.Tick` lambda (line 307, self-recursive) |
| **Caller chain** | `OnStateChange` → `CreatePanel` → `PlacePanel` |
| **Discovery helpers** | `FindChartTrader` (5-strategy fan-out, `src/V12_002.UI.Panel.Helpers.cs:478`) |
| **Grid helper** | `FindChartTabGrid` (`src/V12_002.UI.Panel.Helpers.cs:647`) |
| **State mutated** | `_placementMode`, `_placementGrid`, `_chartTraderElement`, `_placementRetryCount`, `_placementRetryTimer`, `rootContainer` |
| **Side-effects** | Modifies live WPF visual tree (`traderGrid.Children.Add`, `ColumnDefinitions.Add`, `UserControlCollection.Add`); sets `Visibility.Collapsed` on native ChartTrader element |
| **Threading constraint** | Must run on WPF Dispatcher thread; `DispatcherTimer` tick is inherently dispatcher-bound |
| **Risk on change** | High — mutates the host application's live visual tree; incorrect column injection or double-attachment causes permanent layout corruption with no automatic recovery |

**Affected symbol count (blast radius):** 7 state fields + 2 helper methods + 3 visual-tree
collections directly mutated; `DestroyPanel` is the paired teardown (must remain in sync).

---

## Top 3 Complexity Drivers

1. **Three-path conditional placement with compound pattern-match guards** (lines 241–267)
   The outer `if (rootContainer == null || _placementMode != PanelPlacement.None)` guard carries
   a two-clause short-circuit OR (+2 CYC). The Hijack-path entry condition `_chartTraderElement
   != null && _chartTraderElement.Parent is Grid traderGrid` is a compound AND with a C# 7
   pattern-match that evaluates to two independent decision points (+2 CYC). Inside the Hijack
   body, `rSpan > 1` and `cSpan > 1` each add a branch (+2 CYC). Combined, the Hijack block alone
   accounts for ~6 of the 13 CYC points and modifies ChartTrader visibility — a side-effect
   that `DestroyPanel` must unconditionally reverse.

2. **Stateful retry loop with inline lambda and nested condition** (lines 291–312)
   The retry guard `_placementRetryCount < 3` (+1), the null-check `_placementRetryTimer == null`
   controlling lazy timer construction (+1), and the timer tick lambda containing a compound guard
   `_isTerminating || rootContainer == null` (+2) together add 4 CYC. The lambda closes over
   `this` and triggers a recursive `PlacePanel()` call — the back-edge is not visible to most
   static analysis tools, explaining why some counters report 13 vs 14. The lambda is a hidden
   re-entry point that must honour the `_placementMode` guard on every invocation.

3. **Path-2 inject branch with row-span conditional** (lines 271–288)
   The `_placementGrid != null` check (+1) and `_placementGrid.RowDefinitions.Count > 1` span
   guard (+1) contribute the remaining CYC. Critically, `ColumnDefinitions.Add` in this branch
   is only reversed in `DestroyPanel` by a heuristic width-equality check (`Math.Abs(width - 210)
   < 1`), meaning any refactor that changes the injected column width must update both call sites
   atomically — a tight coupling that should be extracted to a named constant.

---

## Recommended Extraction Count

**3 logical helpers recommended for Phase 1 extraction.**

| Candidate Helper | Extracted Logic | CYC Reduction |
|---|---|---|
| `TryHijackChartTrader()` | Lines 246–267: pattern-match guard + rSpan/cSpan conditionals + Hijack state write | −5 CYC |
| `TryInjectIntoChartTabGrid()` | Lines 271–288: PATH 2 grid null-check + RowSpan guard + column inject | −3 CYC |
| `SchedulePlacementRetry()` | Lines 291–312: retry counter, lazy timer construction, lambda closure | −4 CYC |

After extraction the dispatcher `PlacePanel()` would be ≤3 CYC (guard → try path A → try path B
→ schedule retry → fallback), with each extracted helper independently testable and verifiable.
The `210` column-width magic number must become a named constant (`PanelColumnWidth`) shared
between `TryInjectIntoChartTabGrid` and `DestroyPanel` before extraction is complete.

---

## Agent Tracking

Agent Name: bob-phase0-hotspot | Bobcoins Used: 1.0 | Execution Time: ~60s
