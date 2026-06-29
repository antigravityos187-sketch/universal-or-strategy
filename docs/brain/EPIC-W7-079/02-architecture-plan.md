# Phase 2: Architecture Plan — EPIC-W7-079

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:20:00Z

---

## Method Under Extraction

- **Method:** `CreateSection0_Identity`
- **Source File:** `src/V12_002.UI.Panel.Construction.cs`
- **Original CYC:** 0 at structural entry (per 00-hotspots.md); real complexity is structural — 194-line UI factory spanning lines 511–705. Phase 1 scope doc records CYC = 1 (baseline functional count). All complexity lives inside lambda closures and a foreach loop.
- **Lines:** 511–705 (194 lines)
- **Return type:** `private Border CreateSection0_Identity()`

### jCodemunch `get_context_bundle` Result

Symbol confirmed at `src/V12_002.UI.Panel.Construction.cs:511`. Full source retrieved (511–705). Method builds three distinct UI sub-panels sequentially: hub status LED row, fleet popup row, and manual entry row. No branches at top-level flow — all control flow is inside lambdas (`selectAllCheck.Checked`, `selectAllCheck.Unchecked`, `cb.Checked`, `cb.Unchecked`) and a `foreach` over `fleetAccounts`. Calls 28 distinct callees (helper constructors, field accessors, `UpdateFleetButtonText`, `PanelCommand`, `GetFleetAccountsSnapshot`).

### jCodemunch `get_call_hierarchy` Result

- **Callers (depth 1):** 1 — `CreatePanel` at `src/V12_002.UI.Panel.Construction.cs:163`
- **Key callees (depth 1):** `CreateSectionBorder`, `CreateSectionHeader`, `CreateCombo`, `GetFleetAccountsSnapshot` (in `V12_002.UI.IPC.cs:202`), `activeFleetAccounts` (ConcurrentDictionary in `V12_002.cs:195`), `UpdateFleetButtonText` (line 1511), `PanelCommand` (in `V12_002.UI.Panel.Handlers.cs:935`), `CreateTextBox`, `CreateButton`
- **Depth-2 callees via Enqueue:** `IsFleetAccount`, `Enqueue` — confirming Actor/lock-free dispatch pattern in downstream PanelCommand path

### jCodemunch `get_dependency_graph` Result

No indexed cross-file import edges (partial class pattern — all V12_002 partials exist in the same logical class). New private helpers added to the same file are fully safe. No blast radius beyond `src/V12_002.UI.Panel.Construction.cs`.

### jCodemunch `get_extraction_candidates` Result

Returned empty (no candidates meeting min_complexity=3 + min_callers=1 thresholds). Consistent with the lambda-driven complexity — AST cyclomatic detection does not capture lambda branch complexity. Extraction is driven by SRP violation and method length (Jane Street rules #2 and #4), not raw CYC count. Manual extraction plan is required.

---

## Sequential Thinking Summary

**Thought 1:** Confirmed CYC = 0 structural / 1 functional. 194-line method. Three distinct sub-panel regions identified from source body. 1 caller confirmed. Extraction by SRP decomposition justified.

**Thought 2:** Designed 4 private helper methods with names, responsibilities, and projected CYC: `BuildHubStatusRow` (CYC 1), `BuildFleetPopupRow` (CYC 3), `BuildFleetCheckboxPanel` (CYC 5), `BuildManualEntryRow` (CYC 2).

**Thought 3:** Parent compositor after extraction retains no branches — projected CYC = 1. Max CYC across all new symbols = 5. All values <= 8. Jane Street verdict: PASS.

**Thought 4:** Thread-safety fix (H-3): snapshot `activeFleetAccounts` via `ToArray()` before foreach. Ordering fix (H-2): assign `fleetCheckboxPanel` before registering `selectAllCheck` event handlers — eliminates the null-guard ordering dependency. Both fixes are internal to the extraction scope (no new external methods, no caller changes).

**Thought 5:** Final verdict — 4 helpers extracted, parent becomes 12-line compositor, max CYC = 5, all Jane Street rules satisfied, V12.23 no-scope-creep maintained.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `private Grid BuildHubStatusRow()` | Constructs hub-status LED (`hubStatusLed` Border 8x8) + leader account ComboBox (`leaderAccountCombo`) in a 2-column Grid. Pure construction, no branches. Assigns `hubStatusLed` and `leaderAccountCombo` instance fields. | 1 |
| `private Grid BuildFleetPopupRow()` | Constructs `fleetSelectButton`, `fleetPopup`, `popupBorder`, `popupStack`, `selectAllCheck` (with Checked/Unchecked lambdas), separator Border. Calls `BuildFleetCheckboxPanel()`. Wires popup to button. Assigns `fleetSelectButton`, `fleetPopup` fields. **Fix H-2:** assigns `fleetCheckboxPanel` before registering `selectAllCheck` event handlers. | 3 |
| `private void BuildFleetCheckboxPanel()` | Initialises `fleetCheckboxPanel = new StackPanel()`, clears `selectedFleetAccounts`, snapshots `activeFleetAccounts` via `ToArray()` (**Fix H-3** thread safety), foreach over `fleetAccounts` — creates CheckBox per account with Checked/Unchecked lambdas that call `PanelCommand` via Actor/Enqueue path. Adds all checkboxes to `fleetCheckboxPanel`. | 5 |
| `private Grid BuildManualEntryRow()` | Constructs 3-column Grid with `directionCombo` (CreateCombo), `priceInput` (CreateTextBox with `lastKnownPrice > 0` ternary), and `submitButton` (CreateButton). Assigns `manualEntryRow`, `directionCombo`, `priceInput`, `submitButton` fields. | 2 |

---

## Parent Method After Extraction

### Remaining Logic

```csharp
private Border CreateSection0_Identity()
{
    Border section = CreateSectionBorder();
    StackPanel stack = new StackPanel
    {
        Margin = new Thickness(2, 2, 2, 1),
        HorizontalAlignment = HorizontalAlignment.Stretch,
    };
    stack.Children.Add(CreateSectionHeader("SECTION 0: IDENTITY"));
    stack.Children.Add(BuildHubStatusRow());
    stack.Children.Add(BuildFleetPopupRow());
    stack.Children.Add(BuildManualEntryRow());
    section.Child = stack;
    return section;
}
```

- **Remaining logic:** Pure compositor — delegates each sub-panel construction to the 4 extracted helpers. No conditional logic remains.
- **Projected CYC:** 1

---

## max_cyc_projected: 5
## extraction_count: 4

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC <= 8 achieved (max = 5) | YES |
| Single-responsibility per helper | YES |
| Lock-free/Actor pattern preserved (PanelCommand -> Enqueue chain confirmed) | YES |
| Illegal states unrepresentable (fleetCheckboxPanel assigned before selectAllCheck handlers) | YES |
| Zero-allocation hot path (UI construction path — no hot-path allocation concern) | N/A |
| No scope creep (V12.23 — same-file private helpers only, 1 caller unmodified) | YES |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | ~15 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **jcodemunch tools called** | search_symbols, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Phase** | 2 |
| **Wave** | 7 |
| **Input** | docs/brain/EPIC-W7-079/01-scope-boundary.md |
| **Output** | docs/brain/EPIC-W7-079/02-architecture-plan.md |
