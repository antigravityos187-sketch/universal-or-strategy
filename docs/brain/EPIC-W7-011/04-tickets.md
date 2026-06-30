# Phase 4: Tickets — EPIC-W7-011

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:40:00Z
**Inputs:**
- `docs/brain/EPIC-W7-011/02-architecture-plan.md`
- `docs/brain/EPIC-W7-011/03-audit-report.md`

---

## Method Under Extraction

- **Method:** `DestroyPanel`
- **Source File:** `src/V12_002.UI.Panel.Construction.cs`
- **Signature:** `private void DestroyPanel()`
- **Lines:** 320–509 (190 lines)
- **CYC (tool-reported):** 19 (structural branches + exception handlers; `get_symbol_complexity` at line 320)
- **CYC (manual McCabe):** 8 (confirmed by Phase 0 hotspot analysis and Phase 2 architecture plan)
- **DNA Verdict:** PASS (Phase 3 audit — 0 violations)

---

## Sequential Thinking Summary

**Thought 1 — Ticket Count:**
`DestroyPanel` has three separable concerns: placement teardown dispatch, per-arm placement teardown (3 arms), and bulk WPF field nullification. The architecture plan identifies 5 helper methods. One ticket = one extracted helper = one concern. `ticket_count = 5`.

**Thought 2 — Per-Ticket Breakdown:**
- T1 `TeardownPlacedPanel`: switch-dispatch orchestrator; moves outer try/catch + switch(_placementMode) block; CYC reduction from parent = 5; helper CYC = 5.
- T2 `TeardownFallbackPlacement`: Fallback arm body; moves inner try/catch + UserControlCollection.Remove; CYC reduction via T1 = 2; helper CYC = 2.
- T3 `TeardownInjectedPlacement`: Injected arm body; moves _placementGrid.Children.Remove + ColumnDefinition 210px heuristic removal; CYC reduction via T1 = 5; helper CYC = 5.
- T4 `TeardownHijackPlacement`: Hijack arm body; moves grid non-null + Contains + Remove; CYC reduction via T1 = 2; helper CYC = 2.
- T5 `ClearPanelWidgetRefs`: bulk ~45 WPF field nullification; zero branch lines; CYC reduction = 0 (line count only); helper CYC = 1.

**Thought 3 — CYC Verification:**
Parent after all extractions = CYC 3 (null guard if + timer if + baseline). All 5 helpers <= 5. max_cyc_projected = 5. All <= Jane Street threshold of 8. VERIFIED.

---

## ticket_count: 5

---

## Ticket 1

```
ticket_id:            1
helper_name:          TeardownPlacedPanel
signature:            private void TeardownPlacedPanel()
concern:              Restores _chartTraderElement visibility and dispatches teardown to the
                      correct per-arm helper via switch(_placementMode); owns the outer
                      try/catch that wraps all placement removal logic.
lines_to_move:        Lines ~330–430 — the outer try { switch (_placementMode) { case Fallback:
                      ... case Injected: ... case Hijack: ... default: ... } } catch { } block,
                      plus the _chartTraderElement.Visibility restore line immediately before it.
                      After extraction, each case arm body becomes a call to the corresponding
                      per-arm helper (T2/T3/T4).
cyc_reduction:        5 (removes switch decision points + outer try/catch from parent)
projected_helper_cyc: 5  [<= 8 PASS]
xunit_test:           [Fact] DestroyPanel_TeardownPlacedPanel_DispatchesToCorrectArm — verifies
                      the switch dispatches to the expected per-arm helper for each PanelPlacement
                      enum value including PanelPlacement.None (default arm).
```

---

## Ticket 2

```
ticket_id:            2
helper_name:          TeardownFallbackPlacement
signature:            private void TeardownFallbackPlacement()
concern:              Removes rootContainer from the WPF UserControlCollection inside an inner
                      try/catch; logs any non-fatal removal failure without re-throwing.
lines_to_move:        Lines within the Fallback case arm body — the inner try { UserControlCollection
                      .Remove(rootContainer); } catch (Exception ex) { /* non-fatal log */ } block.
                      This entire block moves to the new helper; the Fallback case arm in
                      TeardownPlacedPanel becomes a single call: TeardownFallbackPlacement();
cyc_reduction:        2 (removes inner try/catch decision paths; extracted through TeardownPlacedPanel)
projected_helper_cyc: 2  [<= 8 PASS]
xunit_test:           [Fact] TeardownFallbackPlacement_DoesNotThrow_WhenRemoveFails — verifies the
                      inner catch swallows removal exceptions without propagating.
```

---

## Ticket 3

```
ticket_id:            3
helper_name:          TeardownInjectedPlacement
signature:            private void TeardownInjectedPlacement()
concern:              Removes rootContainer from _placementGrid.Children; also removes the last
                      injected ColumnDefinition when its width matches the 210px heuristic.
lines_to_move:        Lines within the Injected case arm body — null guard on _placementGrid,
                      _placementGrid.Children.Remove(rootContainer), the ColumnDefinition width
                      comparison loop/check (width == 210.0 heuristic), and conditional
                      ColumnDefinitions.Remove call. This entire block moves to the new helper;
                      the Injected case arm in TeardownPlacedPanel becomes a single call:
                      TeardownInjectedPlacement();
cyc_reduction:        5 (removes null guard + Children remove conditional + ColumnDefinition check
                      decision paths; extracted through TeardownPlacedPanel)
projected_helper_cyc: 5  [<= 8 PASS]
xunit_test:           [Fact] TeardownInjectedPlacement_RemovesColumnDef_WhenWidthMatches210 —
                      verifies the ColumnDefinition is removed when the 210px heuristic matches,
                      and not removed when it does not.
```

---

## Ticket 4

```
ticket_id:            4
helper_name:          TeardownHijackPlacement
signature:            private void TeardownHijackPlacement()
concern:              Removes rootContainer from _placementGrid.Children when _placementGrid is
                      non-null and already contains rootContainer.
lines_to_move:        Lines within the Hijack case arm body — null guard on _placementGrid,
                      _placementGrid.Children.Contains(rootContainer) check, and
                      _placementGrid.Children.Remove(rootContainer) call. This entire block moves
                      to the new helper; the Hijack case arm in TeardownPlacedPanel becomes a
                      single call: TeardownHijackPlacement();
cyc_reduction:        2 (removes null guard + contains check decision paths; extracted through
                      TeardownPlacedPanel)
projected_helper_cyc: 2  [<= 8 PASS]
xunit_test:           [Fact] TeardownHijackPlacement_SkipsRemove_WhenGridIsNull — verifies no
                      exception and no remove call when _placementGrid is null.
```

---

## Ticket 5

```
ticket_id:            5
helper_name:          ClearPanelWidgetRefs
signature:            private void ClearPanelWidgetRefs()
concern:              Nullifies all ~45 WPF widget field references (identity, execution, target,
                      compliance, market-data, and mode/SV sections) and resets
                      _panelLastSyncedMode, _panelLastSyncedTargetCount, and
                      _panelAppliedConfigRevision to their zero/default values.
lines_to_move:        Lines ~450–509 — the bulk sequential block of <field> = null; assignments
                      for all WPF widget fields plus the three scalar/enum reset statements at the
                      end. Zero branch lines in this block. The block moves verbatim to the new
                      helper; DestroyPanel's call site becomes a single call: ClearPanelWidgetRefs();
cyc_reduction:        0 (no branch decision paths removed; extraction reduces line count only)
projected_helper_cyc: 1  [<= 8 PASS]
xunit_test:           [Fact] ClearPanelWidgetRefs_NullifiesAllWidgetFields — verifies that after
                      the call all tracked WPF widget fields are null and scalar counters are at
                      default/zero values.
```

---

## projected_parent_cyc_after_all: 3

**Parent `DestroyPanel` remaining logic after all 5 extractions:**

```csharp
private void DestroyPanel()
{
    if (rootContainer == null)          // +1 (guard)
        return;

    DetachPanelHandlers();

    TeardownPlacedPanel();              // extracted T1

    rootContainer = null;
    contentBody = null;
    floatingAnchor = null;
    panelScrollViewer = null;
    mainStack = null;
    _chartTraderElement = null;
    _placementGrid = null;
    _placementMode = PanelPlacement.None;
    if (_placementRetryTimer != null)   // +1 (timer guard)
    {
        _placementRetryTimer.Stop();
        _placementRetryTimer = null;
    }
    _placementRetryCount = 0;

    ClearPanelWidgetRefs();             // extracted T5
}
// baseline = 1; guard if = +1; timer if = +1 => CYC = 3
```

**Projected parent CYC = 3** (null guard +1, timer null-check +1, baseline 1) — well within Jane Street threshold of 8.

---

## CYC Summary Table

| Method | Role | Projected CYC | Threshold | Status |
|---|---|---|---|---|
| `DestroyPanel` (parent) | Orchestrator after extraction | 3 | 8 | **PASS** |
| `TeardownPlacedPanel` | Switch-dispatch for placement modes | 5 | 8 | **PASS** |
| `TeardownFallbackPlacement` | Fallback arm teardown | 2 | 8 | **PASS** |
| `TeardownInjectedPlacement` | Injected arm teardown | 5 | 8 | **PASS** |
| `TeardownHijackPlacement` | Hijack arm teardown | 2 | 8 | **PASS** |
| `ClearPanelWidgetRefs` | Bulk WPF field nullification | 1 | 8 | **PASS** |

**max_cyc_projected = 5** — all methods CYC <= 8. Jane Street mandate satisfied.

---

## jCodemunch Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | `found=true, indexed=true, symbol_count=5147` |
| `get_symbol_complexity(DestroyPanel)` | `cyclomatic=19, max_nesting=6, lines=190, assessment=high` (tool uses structural branch + exception handler count; manual McCabe=8 confirmed by Phase 0/Phase 2) |
| `get_extraction_candidates(src/V12_002.UI.Panel.Construction.cs)` | `candidates=0` (expected: Dispatcher lambda boundary prevents AST caller resolution — zero result is tool-limitation artifact, not evidence against extraction) |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-011 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | 2.0 |
| **Execution Time** | 2026-06-29T01:40:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket breakdown thoughts) |
| **ticket_count** | 5 |
| **max_cyc_projected** | 5 |
| **projected_parent_cyc** | 3 |
| **Output File** | docs/brain/EPIC-W7-011/04-tickets.md |
