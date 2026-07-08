# Phase 4: Ticket Generation -- EPIC-W7-075

## Summary

| Field | Value |
|---|---|
| **Epic ID** | EPIC-W7-075 |
| **Wave** | 7 |
| **Method** | `OnSubmitClick` |
| **Source File** | `src/V12_002.UI.Panel.Handlers.cs` |
| **Original CYC** | 34 |
| **max_cyc_projected** | 7 |
| **extraction_count** | 6 |
| **ticket_count** | 9 |
| **dna_verdict** | PASS |

---

## Ticket Overview

This document contains 9 tickets covering the full extraction of `OnSubmitClick` (CYC=34)
into 6 single-responsibility private helpers plus parent refactor, verification, and manifest
update. Each ticket is scoped to exactly one concern per V12.23 no-scope-creep mandate.

---

## W7-075-T1: Extract ReadSubmitDirection

**Title:** Extract `ReadSubmitDirection` helper from `OnSubmitClick`

**Description:**
Extract the UI direction-reading logic from `OnSubmitClick` in
`src/V12_002.UI.Panel.Handlers.cs` into a new private helper method
`ReadSubmitDirection()`. This helper reads the `directionCombo` UI control and
returns the selected item's content string, defaulting to `"OR LONG"` when the
combo is null or has no selection.

**Signature:** `private string ReadSubmitDirection()`

**Acceptance Criteria:**
- `ReadSubmitDirection` exists as a `private string` method in `V12_002` partial class.
- Returns `"OR LONG"` when `directionCombo` is null or has no selected item.
- Returns `ComboBoxItem.Content.ToString()` when a valid item is selected.
- The direction-reading branch logic is removed from `OnSubmitClick` body.
- Build passes with zero errors and zero new warnings.
- xUnit `[Fact]` tests: `ReadSubmitDirection_NullCombo_ReturnsDefault` and
  `ReadSubmitDirection_ValidItem_ReturnsContent` both green.

**CYC Impact:** Removes 3 branch points from `OnSubmitClick`. Helper CYC=3 (<=8 PASS).
Parent CYC after this ticket: approximately 31.

---

## W7-075-T2: Extract ReadSubmitPrice

**Title:** Extract `ReadSubmitPrice` helper from `OnSubmitClick`

**Description:**
Extract the price input reading logic from `OnSubmitClick` into a new private helper
`ReadSubmitPrice()`. This helper reads `priceInput.Text`, applies a null guard, and
returns the trimmed string or `string.Empty` when the input is absent.

**Signature:** `private string ReadSubmitPrice()`

**Acceptance Criteria:**
- `ReadSubmitPrice` exists as a `private string` method in `V12_002` partial class.
- Returns `string.Empty` when `priceInput` is null.
- Returns `priceInput.Text.Trim()` when a valid text input exists.
- Price-reading logic removed from `OnSubmitClick` body.
- Build passes with zero errors and zero new warnings.
- xUnit `[Fact]` test: `ReadSubmitPrice_NullInput_ReturnsEmpty` green.

**CYC Impact:** Removes 2 branch points from `OnSubmitClick`. Helper CYC=2 (<=8 PASS).
Parent CYC after this ticket: approximately 29.

---

## W7-075-T3: Extract ResolveSubmitMode

**Title:** Extract `ResolveSubmitMode` helper from `OnSubmitClick`

**Description:**
Extract the order mode resolution logic from `OnSubmitClick` into a new private helper
`ResolveSubmitMode()`. This helper resolves the mode from `_panelLastSyncedMode` with a
fallback to `GetCurrentConfigMode()`, and normalizes `"OR"` to `"ORB"`.

**Signature:** `private string ResolveSubmitMode()`

**Acceptance Criteria:**
- `ResolveSubmitMode` exists as a `private string` method in `V12_002` partial class.
- Returns `_panelLastSyncedMode` when it is non-null and non-empty.
- Falls back to `GetCurrentConfigMode()` when `_panelLastSyncedMode` is absent.
- Normalizes `"OR"` input to `"ORB"` output.
- Mode resolution logic removed from `OnSubmitClick` body.
- Build passes with zero errors and zero new warnings.
- xUnit `[Fact]` tests: `ResolveSubmitMode_EmptyLastSynced_CallsGetCurrent` and
  `ResolveSubmitMode_ORMode_RemapsToORB` both green.

**CYC Impact:** Removes 3 branch points from `OnSubmitClick`. Helper CYC=3 (<=8 PASS).
Parent CYC after this ticket: approximately 26.

---

## W7-075-T4: Extract ResolveSubmitSymbol

**Title:** Extract `ResolveSubmitSymbol` helper from `OnSubmitClick`

**Description:**
Extract the instrument/symbol resolution logic from `OnSubmitClick` into a new private
helper `ResolveSubmitSymbol()`. This helper traverses the
`Instrument.MasterInstrument` object chain to extract the symbol name string,
returning `string.Empty` on any null in the chain.

**Signature:** `private string ResolveSubmitSymbol()`

**Acceptance Criteria:**
- `ResolveSubmitSymbol` exists as a `private string` method in `V12_002` partial class.
- Returns `string.Empty` when `Instrument` is null.
- Returns `string.Empty` when `Instrument.MasterInstrument` is null.
- Returns the symbol name string on the happy path.
- Symbol resolution logic removed from `OnSubmitClick` body.
- Build passes with zero errors and zero new warnings.
- xUnit `[Fact]` test: `ResolveSubmitSymbol_NullInstrument_ReturnsEmpty` green.

**CYC Impact:** Removes 3 branch points from `OnSubmitClick`. Helper CYC=3 (<=8 PASS).
Parent CYC after this ticket: approximately 23.

---

## W7-075-T5: Extract ClassifyDirectionFlag

**Title:** Extract `ClassifyDirectionFlag` helper from `OnSubmitClick`

**Description:**
Extract the direction-to-flag classification logic from `OnSubmitClick` into a new
private helper `ClassifyDirectionFlag(string direction)`. This helper maps the
human-readable direction string (e.g. `"OR SHORT"`) to a binary flag string
(`"SHORT"` or `"LONG"`), normalizing the value exactly once before it reaches
`BuildSubmitCommand`.

**Signature:** `private string ClassifyDirectionFlag(string direction)`

**Acceptance Criteria:**
- `ClassifyDirectionFlag` exists as a `private string` method in `V12_002` partial class.
- Returns `"SHORT"` when `direction` contains `"SHORT"`.
- Returns `"LONG"` for all other values (default).
- Direction classification logic removed from `OnSubmitClick` body.
- Build passes with zero errors and zero new warnings.
- xUnit `[Fact]` tests: `ClassifyDirectionFlag_SHORT_ReturnsShort` and
  `ClassifyDirectionFlag_LONG_ReturnsLong` both green.

**CYC Impact:** Removes 2 branch points from `OnSubmitClick`. Helper CYC=2 (<=8 PASS).
Parent CYC after this ticket: approximately 21.

---

## W7-075-T6: Extract BuildSubmitCommand

**Title:** Extract `BuildSubmitCommand` helper from `OnSubmitClick` (most complex extraction)

**Description:**
Extract the command-string construction logic from `OnSubmitClick` into a new private
helper `BuildSubmitCommand(string mode, string dir, string symbol, string price)`.
This is the most complex extraction in the epic (CYC=7). The helper is a pure
command-string factory implementing a 4-way mode dispatch (TREND_MANUAL_LIMIT, ORB,
OR_LONG, OR_SHORT) with an optional price suffix. No I/O, no shared state access,
no lock() -- pure functional transformation compliant with the lock-free Actor pattern.

**Signature:** `private string BuildSubmitCommand(string mode, string dir, string symbol, string price)`

**Acceptance Criteria:**
- `BuildSubmitCommand` exists as a `private string` method in `V12_002` partial class.
- Implements 4-way mode dispatch without exceeding CYC=7.
- Appends price suffix only when `price` is non-empty and non-zero.
- Formats `TREND_MANUAL_LIMIT` pipe-delimited command string correctly.
- Returns `OR_LONG` or `OR_SHORT` command strings correctly.
- No I/O, no field reads, no lock() calls inside the method body.
- All string literals are ASCII-only (no Unicode, no curly quotes).
- Command-string factory logic fully removed from `OnSubmitClick` body.
- Build passes with zero errors and zero new warnings.
- xUnit `[Fact]` tests: `BuildSubmitCommand_TrendMode_FormatsCorrectly`,
  `BuildSubmitCommand_ORLong_NoPrice_OmitsPrice`, and
  `BuildSubmitCommand_ORLong_WithPrice_AppendPrice` all green.

**CYC Impact:** Removes 7+ branch points from `OnSubmitClick`. Helper CYC=7 (<=8 PASS).
This is the hardest extraction; max_cyc_projected=7 for the entire epic.
Parent CYC after this ticket: approximately 14.

---

## W7-075-T7: Refactor Parent OnSubmitClick to Orchestrate Helpers

**Title:** Refactor `OnSubmitClick` body to pure sequential orchestration (CYC 34 -> 1)

**Description:**
After all 6 helpers are extracted (T1-T6), replace the `OnSubmitClick` body with a
pure sequential call chain delegating to all 6 helpers. The parent method becomes a
zero-predicate orchestrator with CYC=1. No conditionals, no loops, no null checks
remain in the parent body.

**Post-Extraction Body:**
```csharp
private void OnSubmitClick(object sender, RoutedEventArgs e)
{
    string direction = ReadSubmitDirection();
    string price     = ReadSubmitPrice();
    string mode      = ResolveSubmitMode();
    string symbol    = ResolveSubmitSymbol();
    string dir       = ClassifyDirectionFlag(direction);
    string cmd       = BuildSubmitCommand(mode, dir, symbol, price);
    PanelCommand(cmd);
    TriggerGlow(GreenFg);
}
```

**Acceptance Criteria:**
- `OnSubmitClick` body contains exactly the 8 sequential statements above.
- Zero `if`, `switch`, `?:`, `&&`, `||` operators remain in `OnSubmitClick`.
- CYC of `OnSubmitClick` verified = 1 by `scripts/complexity_audit.py`.
- `PanelCommand` -> `Enqueue` Actor pattern preserved (no lock() introduced).
- `TriggerGlow(GreenFg)` call preserved.
- Build passes with zero errors and zero new warnings.
- All 11 planned xUnit `[Fact]` tests green.

**CYC Impact:** Parent `OnSubmitClick` CYC: 34 -> 1. Net CYC reduction = 33.
Depends on: T1, T2, T3, T4, T5, T6 all completed.

---

## W7-075-T8: Verify CYC Compliance

**Title:** Verify CYC compliance for all 7 symbols post-extraction

**Description:**
Run `scripts/complexity_audit.py` and verify that all 7 symbols introduced or
modified in this epic meet the Jane Street CYC<=8 threshold. Confirm that
`BuildSubmitCommand` specifically reports CYC=7 (the max_cyc_projected value) and
that `OnSubmitClick` reports CYC=1.

**Acceptance Criteria:**
- `scripts/complexity_audit.py` output shows zero methods in target file with CYC > 8.
- `OnSubmitClick` reports CYC=1.
- `BuildSubmitCommand` reports CYC<=7.
- `ReadSubmitDirection` reports CYC<=3.
- `ReadSubmitPrice` reports CYC<=2.
- `ResolveSubmitMode` reports CYC<=3.
- `ResolveSubmitSymbol` reports CYC<=3.
- `ClassifyDirectionFlag` reports CYC<=2.
- `dotnet build` passes with zero errors.
- `dotnet csharpier check src/` passes with zero formatting issues.
- `grep -r "lock(" src/V12_002.UI.Panel.Handlers.cs` returns zero matches.

**CYC Impact:** Verification only -- no code changes. Confirms max_cyc_projected=7.
Depends on: T7 completed.

---

## W7-075-T9: Update Manifest

**Title:** Update `docs/brain/EPIC-W7-075/manifest.json` to reflect Phase 5 completion

**Description:**
After code changes are committed and verified, update the EPIC-W7-075 manifest to
record Phase 5 completion artifacts. Set `phase_5.status = "completed"`, record the
source file modified, the 6 helpers extracted, the parent CYC achieved, and the
timestamp.

**Acceptance Criteria:**
- `docs/brain/EPIC-W7-075/manifest.json` updated with `phase_5.status = "completed"`.
- `phase_5.output` references the ticket completion report.
- `phase_5.helpers_extracted = 6` recorded.
- `phase_5.parent_cyc_achieved = 1` recorded.
- `phase_5.max_cyc_verified = 7` recorded.
- `epic.status` set to `"completed"` after all phase statuses are confirmed complete.
- Git commit includes only `src/V12_002.UI.Panel.Handlers.cs` and the manifest update
  (no unrelated file modifications per V12.23 no-scope-creep).

**CYC Impact:** Documentation only -- no code changes.
Depends on: T8 completed.

---

## Execution Order and Dependencies

```
T1 (ReadSubmitDirection)  --|
T2 (ReadSubmitPrice)      --|
T3 (ResolveSubmitMode)    --|--> T7 (Parent Refactor) --> T8 (Verify) --> T9 (Manifest)
T4 (ResolveSubmitSymbol)  --|
T5 (ClassifyDirectionFlag)--|
T6 (BuildSubmitCommand)   --|
```

T1-T6 are independent and can be executed in any order (or in parallel).
T7 depends on T1-T6 all complete. T8 depends on T7. T9 depends on T8.

---

## CYC Reduction Summary

| Symbol | CYC Before | CYC After | Delta |
|---|---|---|---|
| `OnSubmitClick` (parent) | 34 | 1 | -33 |
| `ReadSubmitDirection` | -- | 3 | new |
| `ReadSubmitPrice` | -- | 2 | new |
| `ResolveSubmitMode` | -- | 3 | new |
| `ResolveSubmitSymbol` | -- | 3 | new |
| `ClassifyDirectionFlag` | -- | 2 | new |
| `BuildSubmitCommand` | -- | 7 | new |

**max_cyc_projected = 7** (BuildSubmitCommand -- Jane Street CYC<=8 PASS)
**parent CYC projected = 1** (pure sequential orchestration)
**Net CYC reduction in parent = 33**

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic ID** | EPIC-W7-075 |
| **Wave** | 7 |
| **Phase** | 4 |
| **Lane** | P4-L5 |
| **Bobcoins Used** | 0.8 |
| **Execution Time** | 2026-06-29T05:00:00Z |
| **ticket_count** | 9 |
| **helpers_extracted** | 6 |
| **max_cyc_projected** | 7 |
| **Output** | docs/brain/EPIC-W7-075/04-tickets.md |
| **status** | completed |
