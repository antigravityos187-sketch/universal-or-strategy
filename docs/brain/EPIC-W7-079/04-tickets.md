# Phase 4: Tickets — EPIC-W7-079

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:30:00Z
**Input:** docs/brain/EPIC-W7-079/02-architecture-plan.md + docs/brain/EPIC-W7-079/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Method** | `CreateSection0_Identity` |
| **Source File** | `src/V12_002.UI.Panel.Construction.cs` |
| **Lines** | 511–705 (194 lines) |
| **Extraction Count** | 4 helpers |
| **max_cyc_projected** | 5 |
| **Parent CYC after extraction** | 1 |
| **DNA Verdict** | PASS |
| **Ticket Count** | 7 |

---

## Ticket W7-079-T1: Extract BuildHubStatusRow

**Title:** Extract `BuildHubStatusRow()` from `CreateSection0_Identity`

**Description:**
Extract the hub-status row construction block from `CreateSection0_Identity` (lines ~515–545) into a new private method `private Grid BuildHubStatusRow()`. This extraction isolates the construction of the `hubStatusLed` Border (8×8 px) and the `leaderAccountCombo` ComboBox into a dedicated helper, fulfilling the single-responsibility principle per Jane Street Rule #2.

**Scope:** `src/V12_002.UI.Panel.Construction.cs` only. No other files modified. Same partial-class context.

**Acceptance Criteria:**
- `private Grid BuildHubStatusRow()` exists in `src/V12_002.UI.Panel.Construction.cs`
- Method assigns `hubStatusLed` and `leaderAccountCombo` instance fields
- Method returns a 2-column `Grid` containing both controls
- `CreateSection0_Identity` no longer contains the inline hub-status construction block; replaced by `BuildHubStatusRow()` call
- Build passes (`dotnet build`) with zero errors
- No `lock()` blocks introduced

**CYC Impact:** `BuildHubStatusRow` cyc=1 (pure construction, no branches). Parent cyc reduction: partial (inline block removed).

---

## Ticket W7-079-T2: Extract BuildFleetPopupRow

**Title:** Extract `BuildFleetPopupRow()` from `CreateSection0_Identity`

**Description:**
Extract the fleet popup row construction block from `CreateSection0_Identity` into a new private method `private Grid BuildFleetPopupRow()`. This block constructs `fleetSelectButton`, `fleetPopup`, `popupBorder`, `popupStack`, `selectAllCheck` with its Checked/Unchecked lambdas, and the separator Border. It wires the popup to the button. The method calls `BuildFleetCheckboxPanel()` (extracted in T3) to populate the popup's inner panel.

**Fix H-2 (ordering):** Within this extraction, ensure `fleetCheckboxPanel` is assigned (via `BuildFleetCheckboxPanel()`) BEFORE `selectAllCheck` Checked/Unchecked event handlers are registered. This eliminates the null-guard ordering dependency and makes illegal states unrepresentable.

**Scope:** `src/V12_002.UI.Panel.Construction.cs` only.

**Acceptance Criteria:**
- `private Grid BuildFleetPopupRow()` exists in `src/V12_002.UI.Panel.Construction.cs`
- `BuildFleetCheckboxPanel()` is called inside this method BEFORE the `selectAllCheck` event handlers are wired
- Method assigns `fleetSelectButton` and `fleetPopup` instance fields
- `CreateSection0_Identity` no longer contains the inline fleet popup block; replaced by `BuildFleetPopupRow()` call
- Build passes with zero errors
- No `lock()` blocks introduced; Actor/Enqueue pattern preserved for `PanelCommand` dispatch chain

**CYC Impact:** `BuildFleetPopupRow` cyc=3 (two lambda closures — selectAllCheck.Checked, selectAllCheck.Unchecked). Well within V12 threshold of ≤8.

---

## Ticket W7-079-T3: Extract BuildFleetCheckboxPanel

**Title:** Extract `BuildFleetCheckboxPanel()` from `CreateSection0_Identity`

**Description:**
Extract the fleet checkbox panel initialisation block from `CreateSection0_Identity` into a new private method `private void BuildFleetCheckboxPanel()`. This is the most complex helper (max cyc helper at cyc=5). It initialises `fleetCheckboxPanel = new StackPanel()`, clears `selectedFleetAccounts`, snapshots `activeFleetAccounts` using `.ToArray()` for thread safety (Fix H-3), then iterates over `fleetAccounts` via `foreach` creating a `CheckBox` per account with Checked/Unchecked lambdas that dispatch via the Actor/Enqueue path through `PanelCommand`.

**Fix H-3 (thread safety):** Snapshot `activeFleetAccounts` with `ToArray()` before the `foreach` loop to avoid ConcurrentDictionary enumeration races. This is an internal fix within the extraction scope — no external API changes.

**Scope:** `src/V12_002.UI.Panel.Construction.cs` only. `void` return — method initialises the `fleetCheckboxPanel` instance field directly.

**Acceptance Criteria:**
- `private void BuildFleetCheckboxPanel()` exists in `src/V12_002.UI.Panel.Construction.cs`
- Method initialises `fleetCheckboxPanel` (StackPanel), clears `selectedFleetAccounts`
- `activeFleetAccounts` is snapshotted via `ToArray()` before the `foreach` — Fix H-3 present
- Per-account CheckBox event handlers use the `PanelCommand` → `Enqueue` lock-free Actor path — no `lock()` introduced
- `CreateSection0_Identity` (or `BuildFleetPopupRow`) no longer contains this inline block
- Build passes with zero errors

**CYC Impact:** `BuildFleetCheckboxPanel` cyc=5 (foreach + 4 lambda branches: cb.Checked, cb.Unchecked, selectAllCheck delegates). This is the max_cyc_projected=5 helper — still well within the cyc≤8 threshold.

---

## Ticket W7-079-T4: Extract BuildManualEntryRow

**Title:** Extract `BuildManualEntryRow()` from `CreateSection0_Identity`

**Description:**
Extract the manual entry row construction block from `CreateSection0_Identity` into a new private method `private Grid BuildManualEntryRow()`. This block constructs a 3-column Grid containing `directionCombo` (via `CreateCombo`), `priceInput` (via `CreateTextBox` with a `lastKnownPrice > 0` ternary for the default value), and `submitButton` (via `CreateButton`). Assigns `manualEntryRow`, `directionCombo`, `priceInput`, and `submitButton` instance fields.

**Scope:** `src/V12_002.UI.Panel.Construction.cs` only.

**Acceptance Criteria:**
- `private Grid BuildManualEntryRow()` exists in `src/V12_002.UI.Panel.Construction.cs`
- Method assigns `manualEntryRow`, `directionCombo`, `priceInput`, `submitButton` instance fields
- Method returns the 3-column `Grid`
- The `lastKnownPrice > 0` ternary is preserved inside the method for `priceInput` default value
- `CreateSection0_Identity` no longer contains the inline manual-entry block; replaced by `BuildManualEntryRow()` call
- Build passes with zero errors
- No `lock()` blocks introduced

**CYC Impact:** `BuildManualEntryRow` cyc=2 (1 ternary branch for lastKnownPrice > 0). Well within V12 threshold of ≤8.

---

## Ticket W7-079-T5: Refactor Parent CreateSection0_Identity to Pure Compositor

**Title:** Refactor `CreateSection0_Identity` to call all 4 extracted helpers

**Description:**
After completing T1–T4, refactor `CreateSection0_Identity` to be a thin 12-line compositor that delegates all sub-panel construction to the 4 extracted helpers. The resulting method should match the architecture plan's target signature exactly:

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

Note: `BuildFleetCheckboxPanel` is called internally by `BuildFleetPopupRow` — it does not appear as a direct call in the compositor.

**Scope:** `src/V12_002.UI.Panel.Construction.cs` only. Caller `CreatePanel` (line 163) is explicitly unmodified.

**Acceptance Criteria:**
- `CreateSection0_Identity` body matches the 12-line compositor form above
- No inline construction logic remains in the parent method
- The single caller `CreatePanel` at line 163 is unmodified
- Build passes with zero errors
- `dotnet csharpier check src/` passes with zero formatting issues

**CYC Impact:** Parent `CreateSection0_Identity` cyc=1 after extraction (pure sequence, no branches). This is the primary CYC reduction objective of this epic.

---

## Ticket W7-079-T6: Verify CYC Compliance

**Title:** Verify CYC compliance for all 5 symbols post-extraction

**Description:**
Run a complexity audit to confirm that all 5 symbols introduced or modified by this epic meet the V12 Jane Street CYC threshold of ≤8.

**Symbols to verify:**

| Symbol | Expected CYC |
|---|---|
| `CreateSection0_Identity` | 1 |
| `BuildHubStatusRow` | 1 |
| `BuildFleetPopupRow` | 3 |
| `BuildFleetCheckboxPanel` | 5 |
| `BuildManualEntryRow` | 2 |
| **max_cyc_projected** | **5** |

**Verification steps:**
1. Run `python scripts/complexity_audit.py` — confirm all 5 symbols show CYC ≤8
2. Run `dotnet build` — confirm zero errors, zero warnings introduced by this epic
3. Run `dotnet csharpier check src/` — confirm zero formatting issues
4. Run `grep -r "lock(" src/V12_002.UI.Panel.Construction.cs` — must return zero matches
5. Confirm ASCII-only compliance: no Unicode, emoji, or curly quotes in modified file

**Acceptance Criteria:**
- All 5 symbols pass cyc≤8 threshold
- No `lock()` blocks in `src/V12_002.UI.Panel.Construction.cs`
- Build: zero errors
- CSharpier: zero issues
- ASCII: compliant

**CYC Impact:** Verification-only ticket. No code changes. Confirms max_cyc_projected=5 is met across all extracted symbols.

---

## Ticket W7-079-T7: Update Manifest

**Title:** Update `docs/brain/EPIC-W7-079/manifest.json` to mark phase_5 ready

**Description:**
Update the EPIC-W7-079 manifest to record Phase 4 completion and signal that Phase 5 (ticket execution) is ready to begin. Set the phase_4 status to completed and reference the tickets output file. Mark all 7 tickets as pending execution for Phase 5.

**Scope:** `docs/brain/EPIC-W7-079/manifest.json` only.

**Acceptance Criteria:**
- `phase_4.status` = `"completed"`
- `phase_4.output` = `"04-tickets.md"`
- `phase_5.status` = `"pending"` (ready for wave Phase 5 Orchestrator)
- Ticket references T1–T7 recorded in manifest
- Manifest is valid JSON

**CYC Impact:** Documentation-only ticket. No source code changes. Confirms epic metadata is current for Phase 5 orchestration pickup.

---

## Ticket Summary

| Ticket | Title | CYC Target | Type |
|---|---|---|---|
| W7-079-T1 | Extract BuildHubStatusRow | cyc=1 | extraction |
| W7-079-T2 | Extract BuildFleetPopupRow | cyc=3 | extraction |
| W7-079-T3 | Extract BuildFleetCheckboxPanel | cyc=5 (max) | extraction |
| W7-079-T4 | Extract BuildManualEntryRow | cyc=2 | extraction |
| W7-079-T5 | Refactor parent to compositor | cyc=1 | refactor |
| W7-079-T6 | Verify CYC compliance | all ≤8 | verification |
| W7-079-T7 | Update manifest | N/A | documentation |

**max_cyc_projected: 5** | **extraction_count: 4** | **All CYC values ≤8: YES** | **DNA Verdict: PASS**

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Input** | docs/brain/EPIC-W7-079/02-architecture-plan.md, docs/brain/EPIC-W7-079/03-audit-report.md |
| **Output** | docs/brain/EPIC-W7-079/04-tickets.md |
| **Ticket Count** | 7 |
| **Execution Time** | 2026-06-29T01:30:00Z |
