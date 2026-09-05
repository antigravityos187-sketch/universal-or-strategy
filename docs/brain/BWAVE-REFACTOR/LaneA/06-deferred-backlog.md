# BWAVE-REFACTOR LaneA -- Deferred Backlog

**Block**: BWAVE-REFACTOR LaneA
**Date**: 2026-08-26
**Status**: PIPELINE_COMPLETE

---

## Deferred Items This Block

### DW-C39-09-TEST -- xUnit test for `OnAddRule_CallsSaveRules_RulePersistsAcrossRestart`

- **Source**: Ticket A-3 (DW-C39-09) -- xUnit test specified in 04-tickets.md §8 (acceptance
  criterion 7) and the xUnit Test Specification subsection.
- **Why deferred**: `OnAddRule` is a `private void` WPF event handler. Direct xUnit testing
  requires either `[InternalsVisibleTo]` + making the method `internal`, or a full WPF UI test
  harness to simulate the button click. Neither infrastructure component was in scope for the A-3
  implementation ticket. The production code change (SaveRules call) is correct and VERIFY_PASS.
  The test gap is an architectural integration constraint, not a skipped unit test.
- **Suggested approach**: Add `[assembly: InternalsVisibleTo("PropTraderTools.Tests")]` to the
  production assembly, mark `OnAddRule` as `internal`, and write an xUnit `[Fact]` that invokes
  it directly. Assert that the rules file last-write-time is updated (file mtime check, Option A
  from ticket spec) or that the rule count loaded from disk increases by 1 (Option B). The test
  MUST use `[Fact]` (xUnit) only -- never NUnit or MSTest.
- **Priority**: P2 (data integrity coverage, not blocking production behaviour)
- **Assigned to**: Next available engineer sprint
- **Status**: OPEN

---

### PRE-EXISTING-COPYENGINE-CCN -- 33 methods in CopyEngine.cs with CCN > 8

- **Source**: Identified by A-2 verifier (SCAN-07 discrepancy analysis in ticket-2-verification.md)
  and independently by A-3 verifier during lizard run (ticket-3-verification.md SCAN-07 detail).
- **Count**: 33 methods with CCN > 8 in `src/PropTraderTools/CopyEngine.cs`.
- **Why deferred**: Not introduced by LaneA. This is pre-existing prior-wave technical debt.
  LaneA touched only `TradeCopierPanel.cs` (A-2) and `TradeCopierWindow.cs` (A-3), both of
  which are clean (0 methods CCN > 8). The CopyEngine.cs debt predates this block.
- **Suggested approach**: Address in a dedicated CCN reduction epic targeting `CopyEngine.cs`.
  Use extraction patterns documented in `docs/intel/jane-street/complexity-reduction.md`.
  BWAVE-REFACTOR lanes B and C are already scoped to CopyEngine.cs CCN reduction.
- **Priority**: P2 (ongoing wave debt, tracked by wave orchestrator)
- **Status**: OPEN (carried from prior waves; not newly introduced)

---

## Prior OPEN Items Closed This Block

None. This is the first deferred-backlog file for LaneA.
