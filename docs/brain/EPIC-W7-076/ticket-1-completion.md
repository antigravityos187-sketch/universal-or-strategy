# Ticket 1 Completion — EPIC-W7-076

**epic_id:** EPIC-W7-076
**ticket_id:** 1
**helper_name:** CollapseControlIfPresent
**concern_extracted:** 9 inline if-null-collapse guards replaced with helper calls — CYC 11→2
**source_file:** src/V12_002.UI.Panel.Handlers.cs
**parent_method:** CollapseAllExecutionControls
**cyc_parent_before:** 11
**cyc_parent_now:** 2
**cyc_achieved:** 2
**build_passed:** true
**tests_written:** 0
**agent_name:** v12-p5-ticket
**verification_only:** false
**no_src_changes:** false

## Summary
Replaced 9 inline `if (x != null) x.Visibility = Visibility.Collapsed` checks with calls to
`CollapseControlIfPresent(x)`. Each inline `if` added +1 CYC; moving them into the helper
reduces the parent from CYC=11 to CYC=2 (1 remaining if for manualEntryRow + base=1).

The helper `CollapseControlIfPresent(System.Windows.UIElement control)` is declared `private static`
and uses the minimal common base type (`UIElement`) that exposes the `.Visibility` property,
shared by both `Grid` (execRetestRow, execTrendRow, manualEntryRow) and `Button` (rmaButton,
momoButton, ffmaButton, ffmaManualButton, mButton, orLongButton, orShortButton).

The `manualEntryRow` assignment (`Visibility.Visible`) intentionally remains inline in the parent
because it sets Visible (not Collapsed) — semantically distinct and correctly excluded from the helper.

## Complexity Audit Results
| Method                        | LOC | CYC | Status |
|-------------------------------|-----|-----|--------|
| CollapseAllExecutionControls  |  12 |   2 | OK     |
| CollapseControlIfPresent      |   3 |   2 | OK     |

## DNA Checks
- Zero lock() blocks: PASS
- ASCII-only identifiers: PASS
- UTF-8 no BOM: PASS
- xUnit tests: N/A (pure UI visibility helper)
