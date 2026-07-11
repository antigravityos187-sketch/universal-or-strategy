# Phase 4: Ticket Generation -- EPIC-W7-080

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 -- Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-080/02-architecture-plan.md, docs/brain/EPIC-W7-080/03-audit-report.md

---

## Method Under Refactor

- **Method:** `PlacePanel`
- **Source File:** `src/V12_002.UI.Panel.Construction.cs`
- **Original CYC:** 13
- **DNA Verdict:** PASS (Phase 3 audit)
- **extraction_count:** 3 helpers + 1 constant
- **max_cyc_projected:** 5
- **Parent CYC after extraction:** 4

---

## Ticket Summary

| Ticket | Title | CYC Impact |
|--------|-------|------------|
| W7-080-T1 | Introduce const PanelColumnWidth = 210 | No CYC change (constant introduction) |
| W7-080-T2 | Extract TryHijackChartTrader helper | New method CYC=4 |
| W7-080-T3 | Extract TryInjectIntoChartTabGrid helper | New method CYC=3 |
| W7-080-T4 | Extract SchedulePlacementRetry helper | New method CYC=5 (max helper) |
| W7-080-T5 | Refactor parent PlacePanel to dispatch via helpers | Parent CYC 13→4 |
| W7-080-T6 | Verify CYC compliance across all extracted methods | Confirms max_cyc_projected=5, all <=8 |
| W7-080-T7 | Update manifest to reflect Phase 5 readiness | No code change |

---

## ticket W7-080-T1: Introduce const PanelColumnWidth = 210

**Title:** Introduce `private const double PanelColumnWidth = 210` to eliminate magic number

**Description:**
The literal value `210` appears in both the Inject path of `PlacePanel` (adding a
`ColumnDefinition` width) and the `DestroyPanel` heuristic check. This creates hidden coupling
between two methods via a bare magic number. Introduce `private const double PanelColumnWidth = 210`
as a class-level constant in `src/V12_002.UI.Panel.Construction.cs`. All subsequent extraction
tickets (T2-T4) and the parent refactor (T5) will reference this constant.

**File:** `src/V12_002.UI.Panel.Construction.cs`

**Change:**
- Add `private const double PanelColumnWidth = 210;` at the top of the field/constant block
- Replace the bare literal `210` in the Inject path with `PanelColumnWidth`
- Replace the bare literal `210` in `DestroyPanel` with `PanelColumnWidth`

**Acceptance Criteria:**
- `private const double PanelColumnWidth = 210;` present in file
- Zero occurrences of the bare literal `210` remain in panel width context
- `dotnet build src/` passes with zero errors
- No new Roslyn warnings introduced
- ASCII-only: constant name and value use only ASCII characters

**CYC Impact:** No CYC change. Constant introduction does not add branches. Eliminates magic
number coupling between PlacePanel extraction path and DestroyPanel, satisfying the V12 DNA
mandate "make illegal states unrepresentable" (no hidden coupling via bare literals).

---

## ticket W7-080-T2: Extract TryHijackChartTrader helper

**Title:** Extract `private bool TryHijackChartTrader()` from PlacePanel Hijack path (CYC=4)

**Description:**
The Hijack path of `PlacePanel` (lines 246-267) contains a compound pattern-match guard
(`_chartTraderElement != null && Parent is Grid traderGrid`), two span conditionals
(`rSpan > 1`, `cSpan > 1`), Grid column/row/span attachment of `rootContainer`, ChartTrader
visibility collapse, and `_placementMode`/`_placementGrid` write. This is a self-contained
concern and constitutes 4 of the 13 CYC points in the parent method. Extract it into a
private helper that returns `true` on successful hijack placement.

**File:** `src/V12_002.UI.Panel.Construction.cs`

**Change:**
- Extract lines 246-267 into `private bool TryHijackChartTrader()`
- Method returns `true` if hijack placement succeeds, `false` otherwise
- Writes `_placementMode = PanelPlacement.Hijack` and `_placementGrid` on success
- Parent retains only the call site: `if (TryHijackChartTrader()) return;`
- Add xUnit `[Fact]` test: `TryHijackChartTrader_Returns_False_When_ChartTrader_Is_Null`

**Acceptance Criteria:**
- `TryHijackChartTrader()` exists as a `private bool` method in the file
- Method body contains the compound guard, span conditionals, and Grid attachment logic
- Parent `PlacePanel` calls the helper and returns on success
- `dotnet build src/` passes with zero errors
- `python scripts/complexity_audit.py` reports `TryHijackChartTrader` CYC=4
- xUnit `[Fact]` test added (NOT NUnit, NOT MSTest)
- Zero `lock()` blocks introduced
- ASCII-only string literals throughout

**CYC Impact:** Extracts 4 CYC points from parent. `TryHijackChartTrader` cyc=4. All <=8
Jane Street threshold. Partial reduction of parent PlacePanel cyc (13 → intermediate value;
final reduction completed in T5).

---

## ticket W7-080-T3: Extract TryInjectIntoChartTabGrid helper

**Title:** Extract `private bool TryInjectIntoChartTabGrid()` from PlacePanel Inject path (CYC=3)

**Description:**
The Inject path of `PlacePanel` (lines 271-288) calls `FindChartTabGrid`, checks for null,
adds a new `ColumnDefinition` with width `PanelColumnWidth` (introduced in T1), guards on
`RowDefinitions.Count > 1` for row-span, attaches `rootContainer` to the grid, and writes
`_placementMode = PanelPlacement.Injected`. This is a self-contained injection concern with
CYC=3. Extract it into a private helper returning `true` on successful injection.

**File:** `src/V12_002.UI.Panel.Construction.cs`

**Depends on:** W7-080-T1 (requires `PanelColumnWidth` constant)

**Change:**
- Extract lines 271-288 into `private bool TryInjectIntoChartTabGrid()`
- Method returns `true` if injection succeeds, `false` otherwise
- References `PanelColumnWidth` const (not bare literal `210`)
- Writes `_placementMode = PanelPlacement.Injected` and `_placementGrid` on success
- Parent retains only: `if (TryInjectIntoChartTabGrid()) return;`
- Add xUnit `[Fact]` test: `TryInjectIntoChartTabGrid_Returns_False_When_Grid_Not_Found`

**Acceptance Criteria:**
- `TryInjectIntoChartTabGrid()` exists as a `private bool` method in the file
- Method uses `PanelColumnWidth` constant (not literal `210`)
- `dotnet build src/` passes with zero errors
- `python scripts/complexity_audit.py` reports `TryInjectIntoChartTabGrid` CYC=3
- xUnit `[Fact]` test added (NOT NUnit, NOT MSTest)
- Zero `lock()` blocks introduced
- ASCII-only string literals throughout

**CYC Impact:** Extracts 3 CYC points from parent. `TryInjectIntoChartTabGrid` cyc=3. All <=8
Jane Street threshold. Partial reduction continues toward parent cyc target of 4.

---

## ticket W7-080-T4: Extract SchedulePlacementRetry helper

**Title:** Extract `private void SchedulePlacementRetry()` from PlacePanel retry/fallback path (CYC=5)

**Description:**
The retry/fallback path of `PlacePanel` (lines 291-312) guards `_placementRetryCount < 3`,
lazily constructs a `DispatcherTimer`, attaches a Tick lambda containing a compound
`_isTerminating || rootContainer == null` guard, and calls `PlacePanel()` recursively within
the lambda. After retries exhausted, falls back to `UserControlCollection.Add`. This path
has the highest CYC among all helpers (CYC=5) due to the retry guard + timer + compound Tick
guard. Extract it into a private void helper as the max-CYC extraction of this epic.

**File:** `src/V12_002.UI.Panel.Construction.cs`

**Change:**
- Extract lines 291-312 into `private void SchedulePlacementRetry()`
- Encapsulates retry counter guard, lazy DispatcherTimer, Tick lambda with compound guard,
  recursive `PlacePanel()` call, and UserControlCollection fallback
- Parent retains only: `SchedulePlacementRetry();` as final fallback line
- Add xUnit `[Fact]` test: `SchedulePlacementRetry_Does_Not_Schedule_When_RetryCount_Exceeds_Limit`

**Acceptance Criteria:**
- `SchedulePlacementRetry()` exists as a `private void` method in the file
- Method contains retry counter guard, DispatcherTimer construction, and compound Tick guard
- `dotnet build src/` passes with zero errors
- `python scripts/complexity_audit.py` reports `SchedulePlacementRetry` CYC=5
- This is the **max helper CYC** for this epic; must equal `max_cyc_projected=5`
- xUnit `[Fact]` test added (NOT NUnit, NOT MSTest)
- Zero `lock()` blocks introduced (DispatcherTimer is dispatcher-bound; no threading locks)
- ASCII-only string literals throughout

**CYC Impact:** Extracts 5 CYC points from parent. `SchedulePlacementRetry` cyc=5
(max_cyc_projected for this epic). All <=8 Jane Street threshold. Final extraction before
parent refactor in T5.

---

## ticket W7-080-T5: Refactor parent PlacePanel to dispatch via helpers

**Title:** Refactor `PlacePanel()` to call all helpers, reducing parent CYC from 13 to 4

**Description:**
After T2, T3, and T4 extract all three path helpers, the parent `PlacePanel()` method must be
updated to call each helper in sequence, replacing the inline path logic with clean delegation.
The resulting method body is a minimal dispatcher: entry guard, discovery call, and three
helper dispatches. This completes the full extraction and achieves the target parent cyc=4.

**File:** `src/V12_002.UI.Panel.Construction.cs`

**Depends on:** W7-080-T2, W7-080-T3, W7-080-T4

**Final parent body:**
```csharp
private void PlacePanel()
{
    if (rootContainer == null || _placementMode != PanelPlacement.None)
        return;

    _chartTraderElement = FindChartTrader();

    if (TryHijackChartTrader())
        return;

    _chartTraderElement = null;
    if (TryInjectIntoChartTabGrid())
        return;

    SchedulePlacementRetry();
}
```

**Change:**
- Replace all inline path logic in PlacePanel with calls to the three extracted helpers
- Retain entry guard (compound OR) and FindChartTrader() discovery call in parent
- Add xUnit `[Fact]` test: `PlacePanel_Dispatches_To_TryHijack_Then_TryInject_Then_Retry`

**Acceptance Criteria:**
- `PlacePanel()` signature unchanged: `private void PlacePanel()`
- Parent method body matches the final body above (entry guard + 3 helper dispatches)
- `dotnet build src/` passes with zero errors
- `python scripts/complexity_audit.py` reports `PlacePanel` CYC=4 (down from 13)
- xUnit dispatch test added (NOT NUnit, NOT MSTest)
- Zero `lock()` blocks in parent method
- ASCII-only throughout

**CYC Impact:** Parent PlacePanel cyc drops from 13 to 4. Total CYC reduction = 9 points.
Full extraction complete. max_cyc_projected=5 (SchedulePlacementRetry). All 4 methods now
satisfy Jane Street CYC<=8 mandate.

---

## ticket W7-080-T6: Verify CYC compliance across all extracted methods

**Title:** Verify all extracted methods satisfy CYC<=8 and max_cyc_projected=5

**Description:**
Run the complexity audit script and confirm that every method resulting from this epic meets
the Jane Street CYC<=8 threshold. The max_cyc_projected for this epic is 5
(SchedulePlacementRetry). Confirm the audit output matches the architecture plan projections.
Also confirm zero lock() blocks and all xUnit tests pass.

**Commands:**
```
python scripts/complexity_audit.py
grep -r "lock(" src/V12_002.UI.Panel.Construction.cs
dotnet test tests/ --filter "PlacePanel"
```

**Acceptance Criteria:**
- `PlacePanel` CYC=4 in audit output (was 13)
- `TryHijackChartTrader` CYC=4 in audit output
- `TryInjectIntoChartTabGrid` CYC=3 in audit output
- `SchedulePlacementRetry` CYC=5 in audit output (max_cyc_projected)
- All 4 values are <=8 (Jane Street threshold satisfied)
- `grep -r "lock(" src/V12_002.UI.Panel.Construction.cs` returns 0 matches
- `dotnet test` passes all 4 xUnit [Fact] tests for this epic
- `dotnet build src/` passes with zero errors and zero new warnings

**CYC Impact:** Verification only (no code changes). Confirms cyc reduction from 13 to max=5
across all extracted methods. Epic-level CYC compliance achieved.

---

## ticket W7-080-T7: Update manifest to reflect Phase 5 readiness

**Title:** Update `docs/brain/EPIC-W7-080/manifest.json` to mark Phase 4 complete and Phase 5 pending

**Description:**
After tickets T1-T6 are generated and written, update the epic manifest to record Phase 4
completion and set Phase 5 (ticket execution) as the next pending phase. This signals to the
Phase 5 Orchestrator that the epic is ready for code execution.

**File:** `docs/brain/EPIC-W7-080/manifest.json`

**Change:**
- Set `phases.phase_4.status = "completed"`
- Set `phases.phase_4.output = "04-tickets.md"`
- Set `phases.phase_5.status = "pending"`
- Increment `lamport_clock` by 1

**Acceptance Criteria:**
- `manifest.json` is valid JSON
- `phases.phase_4.status == "completed"`
- `phases.phase_4.output == "04-tickets.md"`
- `phases.phase_5.status == "pending"`
- File persists to disk without parse errors

**CYC Impact:** No code change. Manifest update only. Unblocks Phase 5 orchestration for
EPIC-W7-080 (PlacePanel extraction, cyc 13→4, max_cyc_projected=5).

---

## Execution Order

```
T1 (const PanelColumnWidth)
  └── T2 (TryHijackChartTrader, CYC=4)
  └── T3 (TryInjectIntoChartTabGrid, CYC=3)  [depends on T1]
  └── T4 (SchedulePlacementRetry, CYC=5)
        └── T5 (Refactor parent PlacePanel, CYC 13→4)  [depends on T2+T3+T4]
              └── T6 (Verify CYC compliance)
                    └── T7 (Update manifest)
```

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **EPIC** | EPIC-W7-080 |
| **Method** | PlacePanel |
| **Source File** | src/V12_002.UI.Panel.Construction.cs |
| **Original CYC** | 13 |
| **max_cyc_projected** | 5 |
| **extraction_count** | 3 helpers + 1 constant |
| **Parent CYC after extraction** | 4 |
| **Tickets generated** | 7 (T1-T7) |
| **DNA Verdict (Phase 3)** | PASS |
| **Output** | docs/brain/EPIC-W7-080/04-tickets.md |
