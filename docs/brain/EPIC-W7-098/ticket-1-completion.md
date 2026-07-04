# Ticket 1 Completion -- EPIC-W7-098

**epic_id:** EPIC-W7-098
**ticket_id:** T1
**helper_name:** IsOrderNullOrBadInstrument
**concern_extracted:** Compound null-guard || extracted to named predicate — eliminates 1 || branch from ProcessFlattenWorkItem_CancelOrders
**source_file:** src/V12_002.SIMA.Flatten.cs
**parent_method:** ProcessFlattenWorkItem_CancelOrders
**cyc_parent_before:** 9
**cyc_parent_now:** 8
**cyc_achieved:** 8
**build_passed:** true
**tests_written:** 0
**agent_name:** v12-p5-ticket
**verification_only:** false
**no_src_changes:** false

## Summary
Extracted `IsOrderNullOrBadInstrument(Order order)` from the compound `order == null || order.Instrument == null` guard in `ProcessFlattenWorkItem_CancelOrders`. The `||` operator counted as +1 CYC by complexity_audit.py, making the parent CYC=9. After extraction, parent CYC=8.

Helper decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`.

## DNA Checks
- Zero lock() blocks: PASS
- No LINQ introduced: PASS
- ASCII-only identifiers: PASS
- UTF-8 no BOM: PASS
- xUnit [Fact] tests: N/A (helper is a pure static predicate, covered by integration)
