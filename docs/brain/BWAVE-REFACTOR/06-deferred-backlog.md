# BWAVE-REFACTOR -- Deferred Work Backlog

# Maintained by: ptt-plan-reviewer

# Format: one block per epic/lane reviewed

---

## Block: BWAVE-REFACTOR Lane B

Date: 2026-09-06
Epic: CopyEngine.cs CCN<=8 extraction (all 32 methods + 3 residual)
Final verdict: FINAL_PASS

### Deferred Items

| ID       | Item                                                                                                                                                                                                                                                                       | Priority | Target Block | Status |
| -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ------------ | ------ |
| DW-LB-01 | `ActiveOrders .ToList()` -- replace with ConcurrentBag snapshot or direct iteration to avoid allocation on high-frequency NT8 account-bg-thread path. Inherited from DW-NEXT-A-07. Lane B explicitly deferred it.                                                          | P2       | B-future     | OPEN   |
| DW-LB-02 | `Features/*.cs` CCN violations -- CopyEngine.cs extraction is complete but Features/ files (PttTrim, PttFlatten, PttBreakEven, PttBreakEvenSwap, PttGlobalBreakEven, PttCancel, PttCopier, PttBreakEven) were Lane C scope and remain unaudited for lizard CCN compliance. | P1       | Lane C       | OPEN   |
| DW-LB-03 | BWAVE-NEXT LaneBRepair backlog items unrelated to CCN in CopyEngine.cs -- deferred by plan §10 (e.g., correctness repairs from prior LaneBRepair backlog). These were out-of-scope for the extraction-only goal of this epic.                                              | P2       | B-future     | OPEN   |
| DW-LB-04 | `ResolveNullFollowerSlot` returns null for a reference type (Account). Grandfathered as NT8 iterator-method pattern. Future work should evaluate Option<Account> or a non-null sentinel pattern to fully satisfy JS-002.                                                   | P2       | B-future     | OPEN   |
| DW-LB-05 | `ExtractLegSuffix_NoDigit_ReturnsNull` test name is misleading: the implementation returns `string.Empty` (not null), but the test method name was preserved from the ticket spec. Rename in a future test-cleanup pass.                                                   | P3       | B-future     | OPEN   |
| DW-LB-06 | F5 NinjaTrader 8 compilation gate -- ptt-sync-and-verify.ps1 confirmed 18/18 OK (0 MISMATCH). F5 press in NinjaTrader 8 is the mandatory final compile step. Orchestrator must confirm F5 was green before marking this epic 100% closed.                                  | P0       | Immediate    | OPEN   |
| DW-LB-07 | Pre-existing `xUnit2004` warning in `src/PropTraderTools/Tests/B131Tests.cs` (L165: Assert.Equal used for boolean check instead of Assert.True). Present since B131. Should be fixed in a dedicated test-cleanup ticket. Not introduced by this epic.                      | P3       | B-future     | OPEN   |

### Rationale for Deferred Items

DW-LB-01: ActiveOrders.ToList() creates a heap allocation per call on the NT8 account background
thread. The plan and ticket review explicitly preserved the existing pattern. A future lane
should investigate zero-alloc enumeration or pre-snapshot approach.

DW-LB-02: Features/*.cs were intentionally excluded from Lane B scope per plan §2 (deferred
section). A Lane C epic should run lizard over Features/ and apply the same CCN<=8 extraction
strategy.

DW-LB-03: LaneBRepair items include non-CCN correctness gaps identified in prior repair blocks.
These require their own targeted epic, not bundled with the extraction-only work of Lane B.

DW-LB-04: The NT8 Account iterator pattern (ConcurrentDictionary fallback to null) is endemic
to the codebase. Resolving it comprehensively requires a codebase-wide nullability refactor
that goes beyond what a single method extraction can achieve.

DW-LB-05: Test name preservation was explicitly instructed by the ticket spec to avoid
unnecessary churn. The assertion is correct; only the name is misleading.

DW-LB-06: F5 gate is the orchestrator's contractual responsibility per plan §11 (Component
Summary). Reviewer cannot run NT8 compilation independently.

DW-LB-07: xUnit2004 is a code quality advisory, not a blocking error. The pre-existing warning
was present at all stages T1-T5 and should be addressed in a test-quality housekeeping ticket.

### Items Inherited from Prior Blocks

None. This is the first entry in the BWAVE-REFACTOR deferred-backlog file.
Prior LaneBRepair backlog items that remain open (DW-NEXT-A-07 etc.) are tracked in
docs/brain/BWAVE-REFACTOR/LaneB/02-architecture-plan.md §10 and in the BWAVE-NEXT roadmap.
