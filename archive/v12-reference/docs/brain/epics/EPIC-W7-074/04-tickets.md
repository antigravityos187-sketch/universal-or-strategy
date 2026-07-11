# EPIC-W7-074 Phase 4 Tickets — AttachExecutionPanelHandlers

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T04:30:00Z
**Input:** docs/brain/EPIC-W7-074/02-architecture-plan.md + docs/brain/EPIC-W7-074/03-audit-report.md

---

## Epic Summary

| Field | Value |
|---|---|
| **Method** | `AttachExecutionPanelHandlers` |
| **Source** | `src/V12_002.UI.Panel.Handlers.cs` |
| **CYC current** | 12 |
| **CYC target** | <=8 |
| **extraction_count** | 7 |
| **max_cyc_projected** | 2 |
| **Parent CYC after extraction** | 1 |
| **DNA Verdict** | PASS |

### Extraction Overview

The `AttachExecutionPanelHandlers` method (CYC=12) contains 11 repetitive null-guard branches each subscribing a `Button.Click` event with an inline lambda. The extraction strategy replaces 6 inline lambdas with named private handler methods and introduces a `BindClick` guard helper to eliminate the repeated null-check pattern. After full extraction the parent body becomes 11 sequential `BindClick(...)` calls with zero conditionals (CYC=1).

---

## Ticket W7-074-T1: Extract BindClick helper

**Title:** Extract `BindClick(Button btn, RoutedEventHandler handler)` null-safe subscription helper

**Description:**
The 11 inline `if (btn != null) btn.Click += handler;` guards in `AttachExecutionPanelHandlers` are identical in structure. Extract this pattern into a single private helper `BindClick(Button btn, RoutedEventHandler handler)` that performs the null-guard and event subscription. All 11 registration call-sites in the parent are rewritten to use `BindClick`.

This extraction eliminates 11 copies of a null-guard and satisfies the Jane Street "make illegal states unrepresentable" principle — null button references cannot cause NullReferenceException at handler registration time.

**Acceptance Criteria:**
- [ ] `private void BindClick(Button btn, RoutedEventHandler handler)` exists in `src/V12_002.UI.Panel.Handlers.cs`
- [ ] Method body: `if (btn != null) { btn.Click += handler; }`
- [ ] All 11 call-sites in `AttachExecutionPanelHandlers` replaced with `BindClick(button, handler)`
- [ ] No inline null-guard branches remain in the parent method
- [ ] Build passes: `dotnet build` zero errors
- [ ] ASCII-only identifiers and string literals — no Unicode characters
- [ ] No `lock()` blocks introduced

**CYC Impact:** `BindClick` CYC=2 (single null-guard branch). Removes 11 null-guard branches from parent method. Net parent CYC reduction: -11.

---

## Ticket W7-074-T2: Extract OnOrLongClick / OnOrShortClick handlers

**Title:** Extract `OnOrLongClick` and `OnOrShortClick` named handler methods

**Description:**
Two inline lambda closures in `AttachExecutionPanelHandlers` handle OR_LONG and OR_SHORT execution commands. Each calls `PanelCommand(string)` + `ResetExecutionMode()` + `TriggerGlow(color)`. Extract each lambda into a named private `RoutedEventHandler`-compatible method with signature `private void On*Click(object s, RoutedEventArgs e)`.

Named handlers eliminate heap-allocated lambda closures at WPF event subscription time, aligning with the Jane Street zero-allocation hot-paths principle.

**Acceptance Criteria:**
- [ ] `private void OnOrLongClick(object s, RoutedEventArgs e)` exists — body: `PanelCommand("OR_LONG"); ResetExecutionMode(); TriggerGlow(CyanAccent);`
- [ ] `private void OnOrShortClick(object s, RoutedEventArgs e)` exists — body: `PanelCommand("OR_SHORT"); ResetExecutionMode(); TriggerGlow(PinkFg);`
- [ ] `AttachExecutionPanelHandlers` wires: `BindClick(orLongButton, OnOrLongClick)` and `BindClick(orShortButton, OnOrShortClick)`
- [ ] No inline lambda closures remain for these two buttons
- [ ] Build passes: `dotnet build` zero errors
- [ ] ASCII-only string literals (`"OR_LONG"`, `"OR_SHORT"`) — no Unicode
- [ ] `ResetExecutionMode()` routes through `Enqueue` (Actor/FSM lock-free path preserved)

**CYC Impact:** `OnOrLongClick` CYC=1. `OnOrShortClick` CYC=1. Both are straight-line dispatch — zero branches. Reduces parent inline lambda count by 2.

---

## Ticket W7-074-T3: Extract OnMomoClick / OnFfmaClick handlers

**Title:** Extract `OnMomoClick` and `OnFfmaClick` named handler methods

**Description:**
Two inline lambda closures handle MODE_MOMO and MODE_FFMA execution mode commands. Extract each into a named private handler. `OnMomoClick` dispatches `PanelCommand("MODE_MOMO")` + `ResetExecutionMode()` + `TriggerGlow(GreenFg)`. `OnFfmaClick` dispatches `PanelCommand("MODE_FFMA")` + `ResetExecutionMode()` + `TriggerGlow(PinkFg)`.

**Acceptance Criteria:**
- [ ] `private void OnMomoClick(object s, RoutedEventArgs e)` exists — body: `PanelCommand("MODE_MOMO"); ResetExecutionMode(); TriggerGlow(GreenFg);`
- [ ] `private void OnFfmaClick(object s, RoutedEventArgs e)` exists — body: `PanelCommand("MODE_FFMA"); ResetExecutionMode(); TriggerGlow(PinkFg);`
- [ ] `AttachExecutionPanelHandlers` wires: `BindClick(momoButton, OnMomoClick)` and `BindClick(ffmaButton, OnFfmaClick)`
- [ ] No inline lambda closures remain for these two buttons
- [ ] Build passes: `dotnet build` zero errors
- [ ] ASCII-only string literals (`"MODE_MOMO"`, `"MODE_FFMA"`) — no Unicode

**CYC Impact:** `OnMomoClick` CYC=1. `OnFfmaClick` CYC=1. Straight-line dispatch — zero branches. Reduces parent inline lambda count by 2.

---

## Ticket W7-074-T4: Extract OnFfmaManualClick / OnMClick handlers

**Title:** Extract `OnFfmaManualClick` and `OnMClick` named handler methods

**Description:**
Two remaining inline lambda closures handle FFMA_MANUAL_MARKET and MODE_M commands. Extract each into a named private handler. `OnFfmaManualClick` dispatches `PanelCommand("FFMA_MANUAL_MARKET")` + `ResetExecutionMode()` + `TriggerGlow(PinkFg)`. `OnMClick` dispatches `PanelCommand("MODE_M")` + `TriggerGlow(OrangeFg)` (no `ResetExecutionMode` — matches original lambda behaviour).

**Acceptance Criteria:**
- [ ] `private void OnFfmaManualClick(object s, RoutedEventArgs e)` exists — body: `PanelCommand("FFMA_MANUAL_MARKET"); ResetExecutionMode(); TriggerGlow(PinkFg);`
- [ ] `private void OnMClick(object s, RoutedEventArgs e)` exists — body: `PanelCommand("MODE_M"); TriggerGlow(OrangeFg);`
- [ ] `AttachExecutionPanelHandlers` wires: `BindClick(ffmaManualButton, OnFfmaManualClick)` and `BindClick(mButton, OnMClick)`
- [ ] No inline lambda closures remain for these two buttons
- [ ] Build passes: `dotnet build` zero errors
- [ ] ASCII-only string literals (`"FFMA_MANUAL_MARKET"`, `"MODE_M"`) — no Unicode

**CYC Impact:** `OnFfmaManualClick` CYC=1. `OnMClick` CYC=1. Straight-line dispatch — zero branches. Completes the extraction of all 6 inline lambdas. Total inline lambda count after T2+T3+T4 = 0.

---

## Ticket W7-074-T5: Refactor parent AttachExecutionPanelHandlers to call extracted helpers

**Title:** Refactor `AttachExecutionPanelHandlers` body to 11 sequential `BindClick` calls

**Description:**
After T1-T4 complete, the parent method `AttachExecutionPanelHandlers` must be refactored so its body consists exclusively of 11 `BindClick(button, handler)` calls — one per UI button or toggle. All inline null-guards and lambda closures are replaced. The 5 pre-existing named delegates (`OnRetestClick`, `OnRetestRmaToggleClick`, `OnRmaClick`, `OnTrendClick`, `OnTrendRmaToggleClick`) are also migrated to use `BindClick`.

**Final parent body:**
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

**Acceptance Criteria:**
- [ ] `AttachExecutionPanelHandlers` body contains exactly 11 `BindClick` calls and zero other statements
- [ ] Zero inline lambdas remain in the method body
- [ ] Zero inline null-guards (`if (btn != null)`) remain in the method body
- [ ] All 11 handler arguments resolve to named methods (6 new from T1-T4 + 5 pre-existing)
- [ ] Build passes: `dotnet build` zero errors
- [ ] `dotnet csharpier check src/` passes — no formatting violations
- [ ] `grep -n "lock(" src/V12_002.UI.Panel.Handlers.cs` returns zero matches

**CYC Impact:** Parent `AttachExecutionPanelHandlers` CYC after refactor = **1** (zero conditional branches, 11 sequential calls). Reduction from CYC=12 to CYC=1. Target CYC <=8 **EXCEEDED** (1 <= 8).

---

## Ticket W7-074-T6: Verify CYC compliance (target <=8, projected=2)

**Title:** Run complexity audit and confirm all extracted methods meet CYC <=8

**Description:**
Execute `python scripts/complexity_audit.py` against `src/V12_002.UI.Panel.Handlers.cs` to confirm that all new and modified methods comply with the Jane Street CYC <=8 standard. Verify that no method introduced or modified during this epic exceeds CYC=2 (the max_cyc_projected).

**Acceptance Criteria:**
- [ ] `python scripts/complexity_audit.py` executes without error
- [ ] `AttachExecutionPanelHandlers` reports CYC=1
- [ ] `BindClick` reports CYC=2
- [ ] `OnOrLongClick` reports CYC=1
- [ ] `OnOrShortClick` reports CYC=1
- [ ] `OnMomoClick` reports CYC=1
- [ ] `OnFfmaClick` reports CYC=1
- [ ] `OnFfmaManualClick` reports CYC=1
- [ ] `OnMClick` reports CYC=1
- [ ] Zero methods in scope exceed CYC=8
- [ ] `dotnet build` passes with zero errors and zero warnings introduced by this epic
- [ ] `dotnet test` passes (all existing tests green)

**CYC Impact:** Verification only — no code changes. Confirms max_cyc_projected=2 is achieved across all 8 in-scope methods (1 parent + 7 new helpers). cyc reduction for `AttachExecutionPanelHandlers`: 12 -> 1 (delta = -11).

---

## Ticket W7-074-T7: Update manifest

**Title:** Update EPIC-W7-074 manifest.json to reflect Phase 5 ready state

**Description:**
After all tickets T1-T6 pass verification, update `docs/brain/EPIC-W7-074/manifest.json` to record Phase 5 ticket execution as ready. Set all ticket statuses, record completion timestamps, and confirm the epic is unblocked for Phase 5 execution by the v12-engineer agent.

**Acceptance Criteria:**
- [ ] `docs/brain/EPIC-W7-074/manifest.json` `phase_4.status` = `"completed"`
- [ ] `docs/brain/EPIC-W7-074/manifest.json` `phase_4.output` = `"04-tickets.md"`
- [ ] `docs/brain/EPIC-W7-074/manifest.json` `phase_4.ticket_count` = `7`
- [ ] `docs/brain/EPIC-W7-074/manifest.json` `phase_4.helpers_extracted` = `7`
- [ ] `docs/brain/EPIC-W7-074/manifest.json` `phase_4.max_cyc_projected` = `2`
- [ ] `phase_5.status` = `"pending"` (ready for v12-engineer execution)
- [ ] Manifest JSON is valid (parseable by `python -c "import json; json.load(open('docs/brain/EPIC-W7-074/manifest.json'))"`)

**CYC Impact:** Manifest update only — no source code changes.

---

## Ticket Summary

| Ticket | Title | New Methods | CYC Impact |
|---|---|---|---|
| W7-074-T1 | Extract `BindClick` helper | 1 (`BindClick` CYC=2) | Parent -11 null-guards |
| W7-074-T2 | Extract `OnOrLongClick` / `OnOrShortClick` | 2 (CYC=1 each) | Parent -2 lambdas |
| W7-074-T3 | Extract `OnMomoClick` / `OnFfmaClick` | 2 (CYC=1 each) | Parent -2 lambdas |
| W7-074-T4 | Extract `OnFfmaManualClick` / `OnMClick` | 2 (CYC=1 each) | Parent -2 lambdas |
| W7-074-T5 | Refactor parent to 11 `BindClick` calls | 0 | Parent CYC: 12 -> 1 |
| W7-074-T6 | Verify CYC compliance | 0 | Verification: max CYC=2 confirmed |
| W7-074-T7 | Update manifest | 0 | Admin only |

**Total extraction count:** 7 new private helpers (1 guard + 6 named event handlers)
**max_cyc_projected:** 2 (BindClick only)
**Parent CYC after full extraction:** 1
**CYC target <=8:** EXCEEDED (1 <= 8)

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-074 |
| **Method** | `AttachExecutionPanelHandlers` |
| **Inputs** | `02-architecture-plan.md`, `03-audit-report.md` |
| **Output** | `04-tickets.md` |
| **Ticket Count** | 7 |
| **Status** | completed |
