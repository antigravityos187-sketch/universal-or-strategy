# BWAVE-REFACTOR Lane B -- Ticket Review
# Phase 3.5 Output
# Author: ptt-ticket-reviewer
# Source tickets: docs/brain/BWAVE-REFACTOR/LaneB/04-tickets.md
# Source plan:    docs/brain/BWAVE-REFACTOR/LaneB/02-architecture-plan.md
# Written: 2026-09-06

---

## Overall Result: TICKET_REVIEW_PASS

All 5 tickets pass all 7 checks. Gate clearance statement: the engineer may proceed with T1.
Sequential execution is mandatory (T1 -> T2 -> T3 -> T4 -> T5; do not start the next ticket
until the prior ticket passes all 7 scans).

---

## Check Matrix Summary

| Check | T1 | T2 | T3 | T4 | T5 |
|-------|----|----|----|----|-----|
| 1. Traceability | PASS | PASS | PASS | PASS | PASS |
| 2. 7-Scan Checklist | PASS | PASS | PASS | PASS | PASS |
| 3. JS Pre-Check | PASS | PASS | PASS | PASS | PASS* |
| 4. CYC Pre-Check | PASS | PASS | PASS | PASS | PASS |
| 5. NT8 Constraints | PASS | PASS | PASS | PASS | PASS |
| 6. Completeness | PASS | PASS | PASS | PASS | PASS |
| 7. Test Coverage | PASS | PASS | PASS | PASS | PASS |

*T5 JS-002 note: two helpers return null for reference types (`ResolveNullFollowerSlot`,
`ExtractLegSuffix`). Both are explicitly grandfathered by the ticket with documented rationale
(NT8 pattern / .NET 4.8 nullable string). PASS with mandatory engineer annotation requirement
-- see T5 detail.

---

## Ticket 1 (T1) -- Tier A: CCN >= 20 (6 methods)

### Check 1: Traceability

PASS.

- All 6 Tier A methods from the plan (Section 4, Ticket 1) are present: ArmPendingBe(27),
  ResubmitOneCollateralLeg(25), SnapshotBeTargets(24), TryCleanupReArmedAtmBracket(23),
  SyncAtmFollowerTarget(21), SyncFollowerBracket(20).
- Each extraction maps to a specific plan section (02-architecture-plan.md §5.1).
- No phantom work: all 17 new helpers listed in T1 appear in the plan's §5.1 extraction designs.
- No missing work from plan §5.1.

### Check 2: 7-Scan Checklist

PASS. All 7 scans present with exact pass conditions:
- SCAN-01 (lizard CCN): present, scoped to 6 T1 methods + all new helpers. PASS condition correct.
- SCAN-02 (grep lock): present, exact pattern `lock\s*\(`, PASS=zero matches.
- SCAN-03 (grep async void): present, exact pattern `async\s+void`, PASS=zero matches.
- SCAN-04 (return null in new helpers only): present, grandfathers CaptureLinkedTargetPrice.
- SCAN-05 (dotnet build --no-incremental): present, PASS=zero errors zero warnings.
- SCAN-06 (ASCII-only): present, byte-level scan, PASS=Count=0.
- SCAN-07 (dotnet test --no-build): present, PASS=all 5 new [Fact] pass.

### Check 3: JS Pre-Check

PASS.

- JS-021 (no lock): explicitly listed in JS rule table; all 17 T1 helpers are pure extractions,
  no lock() in any described logic. PASS.
- JS-001 (no throw in hot path): CreateAndSubmitCollateralStop/Target absorb existing try/catch,
  explicitly told not to add throw. PASS.
- JS-002 (no return null): all new helpers return bool, void, or use out-param. No new reference-type
  null returns in T1 helpers. PASS.
- JS-033 (no async void): all helpers are synchronous; ticket explicitly states "no async modifier".
  PASS.
- ASCII-only: ticket specifies ASCII-only for all new helper names and string literals. PASS.
- CYC<=8: all helper expected CCN values are within range (max declared is IsBeTargetStateOk<=7,
  IsCleanupAtmEligible<=8). PASS.

### Check 4: CYC Pre-Check

PASS.

Parent residual CCN values after extraction:
- ArmPendingBe: CCN<=7 (ticket §1 parent residual formula verified against plan §5.1).
- ResubmitOneCollateralLeg: CCN<=4. PASS.
- SnapshotBeTargets: CCN<=7. PASS.
- TryCleanupReArmedAtmBracket: CCN<=2. PASS.
- SyncAtmFollowerTarget: CCN<=2. PASS (plan says <=3, ticket says <=2 -- both <=8, no conflict).
- SyncFollowerBracket: CCN<=5. PASS.

No helper has an expected CCN >8. IsCleanupAtmEligible is declared <=8 (exactly at limit),
which is acceptable per spec (target is <=8, not <8).

### Check 5: NT8 Constraints

PASS.

- No new public/internal method signatures added. Test seams (`IsImmediateBeEligibleTestable`,
  `IsBeTargetStateOkTestable`) are `internal static` -- these are test-seam wrappers, not API.
  The restriction is on public/internal *parent* signatures, which are frozen. PASS.
- No AtmStrategyCreate, no AtmStrategyChangeStopTarget. PASS.
- CreateOrder calls preserve `NinjaTrader.Core.Globals.MaxDate` and `(NinjaTrader.Cbi.CustomOrder)null`
  explicitly noted in NT8 Constraints section. PASS.
- IsCleanupAtmEligible out-param type exact match requirement documented with R-03 warning. PASS.
- No FontFamily, no hardcoded hex color, no DateTime.Now described. PASS.
- All order names carry PTT- prefix (CreateAndSubmitCollateralStop: `"PTT-STP-Drag-" + suffix`,
  CreateAndSubmitCollateralTarget: `"PTT-TGT-Drag-" + suffix`). PASS.

### Check 6: Completeness

PASS.

Each of the 6 methods has:
- [x] Spec requirement IDs: BWAVE-REFACTOR-LaneB-T1
- [x] Target method signatures with line numbers
- [x] Extraction instructions with absorb descriptions and expected CCN
- [x] Parent residual CCN formula
- [x] New helper signatures section
- [x] JS rule table
- [x] [Fact] test names (5 tests)
- [x] 7-scan checklist (all 7 scans)
- [x] NT8 constraints section
- [x] Acceptance criteria checklist

### Check 7: Test Coverage

PASS.

5 [Fact] tests provided:
- IsBeTargetStateOk_Working_ReturnsTrue (static seam)
- IsBeTargetStateOk_CancelSubmitted_ReturnsTrue
- IsBeTargetStateOk_Filled_ReturnsFalse
- IsImmediateBeEligible_NullPosition_ReturnsFalse (static seam)
- IsImmediateBeEligible_ZeroTickSize_ReturnsFalse

Coverage: covers all static helpers that have test seams (IsBeTargetStateOk via seam, IsImmediateBeEligible
via seam). Instance helpers (FireImmediateBe, CancelLiveCollateral*, CreateAndSubmit*, ClassifyBeTarget,
IsCleanupAtmEligible, TryCancelNativeAtmTarget, EvaluateCleanupRemoval, IsAtmTargetSyncEligible,
CancelBlockAAtmTarget, BlockBCreateAtmTarget, HandleAtm*, HandleNonAtmSync) are not unit-testable
without NT8 runtime -- this is the established project pattern for instance methods. PASS per
project convention (structural existence tests only when NT8 runtime not available).

NT8 note in test guidance acknowledged: if Position/Instrument cannot be constructed, restructure
seam to accept primitives. Engineer has clear guidance. PASS.

### T1 VERDICT: TICKET_REVIEW_PASS

---

## Ticket 2 (T2) -- Tier B: CCN 16-19 (4 methods)

### Check 1: Traceability

PASS.

- All 4 Tier B methods from plan §4 Ticket 2 present: FlattenOneAccount(19),
  MoveStopToBreakEven(18), ReplaceFollowerCopyOnAtmCancel(18), CancelQxBrackets 3-param(16).
- All 9 new helpers trace to plan §5.2 extraction designs.
- Consolidation note for CommitStaleCancelBatch/CommitQxCancelBatch carried forward from plan R-04.
  No phantom work.

### Check 2: 7-Scan Checklist

PASS. All 7 scans present:
- SCAN-01: scoped to 4 T2 methods + new helpers.
- SCAN-02 through SCAN-07: all present with correct pass conditions.
- SCAN-07 specifies: all 3 new T2 tests pass PLUS all T1 tests continue to pass. PASS.

### Check 3: JS Pre-Check

PASS.

- JS-021: all helpers explicitly listed. PASS.
- JS-002: `FindFollowerRuleForOrder` returns `CopyRule?` (nullable struct). Ticket explicitly
  clarifies that returning null for a nullable struct is compliant (differs from reference-type
  null). Correct reading of JS-002. PASS.
- JS-009 (shared mutable Dictionary): `RegisterBeRetrySlotIfNeeded` uses `_pendingFollowerBeSlots`
  which is explicitly identified as ConcurrentDictionary. Compliant. PASS.
- JS-001: SubmitMarketFlattenOrder, CommitStaleCancelBatch absorb existing try/catch. PASS.
- JS-033: no async. PASS.

### Check 4: CYC Pre-Check

PASS.

- FlattenOneAccount: residual CCN<=2. PASS.
- MoveStopToBreakEven: residual CCN<=4 (<=5 per plan; both <=8). PASS.
- ReplaceFollowerCopyOnAtmCancel: residual CCN<=4. PASS.
- CancelQxBrackets 3-param: residual CCN<=5. PASS.
- All helper expected CCN <=7. No helper described with >8 branches.

### Check 5: NT8 Constraints

PASS.

- SubmitMarketFlattenOrder: `DateTime.MaxValue` (NOT DateTime.Now), null as last args, order name
  `"PTT-Flatten"` -- all explicitly required in NT8 section. PASS.
- CommitStaleCancelBatch: `acc.Cancel(stale.ToArray())` correct AddOnBase pattern; NOT acc.Change.
  PASS.
- FindFollowerRuleForOrder: runs on NT8 account bg thread, same as parent. PASS.
- No AtmStrategyCreate, no AtmStrategyChangeStopTarget. PASS.
- No new public/internal signatures. PASS.

### Check 6: Completeness

PASS. All required sections present: spec IDs, target signatures with line numbers, extraction
instructions, new helper signatures, JS rule table, [Fact] names (3 tests), 7-scan checklist,
NT8 constraints, acceptance criteria.

### Check 7: Test Coverage

PASS.

3 [Fact] tests provided:
- IsQxCancelEligible3_NullSnapshot_PassesThrough (reflection/seam structural)
- IsQxCancelEligible3_OrderNotInSnapshot_ReturnsFalse
- IsAccountFlattenable_NullAccount_ReturnsFalse (structural existence)

`IsQxCancelEligible3` is `private static` -- guidance correctly suggests adding a test seam
`IsQxCancelEligible3Testable`. IsAccountFlattenable is instance -- structural test acceptable
per project NT8 pattern. Coverage is consistent with T1 approach. PASS.

### T2 VERDICT: TICKET_REVIEW_PASS

---

## Ticket 3 (T3) -- Tier C: CCN 13-15 (5 methods)

### Check 1: Traceability

PASS.

- All 5 Tier C methods from plan §4 Ticket 3 present: TryReplacePttBeBrackets(14),
  CancelQxBrackets 2-param(14), TryFirePositionState(13), CountLeaderTargets(13),
  ResubmitTargetAfterCascade(13).
- All 11 new helpers trace to plan §5.3 extraction designs.
- Consolidation note for CommitQxCancelBatch / CommitStaleCancelBatch carried from plan R-04. PASS.

### Check 2: 7-Scan Checklist

PASS. All 7 scans present:
- SCAN-01: scoped to 5 T3 methods + new helpers.
- SCAN-04: `return null` in new helpers only -- zero matches required.
- SCAN-07: T1+T2+T3 tests pass. PASS.

### Check 3: JS Pre-Check

PASS.

- JS-021: all helpers explicitly listed. PASS.
- JS-002: all new T3 helpers return bool or void. No new reference-type null returns. PASS.
- JS-001: CancelStaleTargetDrag, CreateAndSubmitCascadeTarget absorb existing try/catch only.
  PASS.
- JS-033: no async. PASS.

### Check 4: CYC Pre-Check

PASS.

- TryReplacePttBeBrackets: residual CCN<=5. PASS.
- CancelQxBrackets 2-param: residual CCN<=4. PASS.
- TryFirePositionState: residual CCN<=5. PASS.
- CountLeaderTargets: residual CCN<=5. PASS.
- ResubmitTargetAfterCascade: residual CCN<=2. PASS.
- All helpers <=7 expected CCN. PASS.

### Check 5: NT8 Constraints

PASS.

- CreateAndSubmitCascadeTarget: `NinjaTrader.Core.Globals.MaxDate` and
  `(NinjaTrader.Cbi.CustomOrder)null` explicitly required. DW-B142-QTY-DESYNC-01 preserved
  (leaderOrder.Quantity). PASS.
- HasActiveQxOrders: `acc.Orders.ToList()` snapshot pattern specified. PASS.
- IsNativeLeaderTarget: static, no NT8 API. PASS.
- No AtmStrategyCreate, no AtmStrategyChangeStopTarget. PASS.

### Check 6: Completeness

PASS. All required sections present.

### Check 7: Test Coverage

PASS.

4 [Fact] tests provided:
- IsPositionStateTriggerState_Filled_ReturnsFalse (via seam)
- IsPositionStateTriggerState_Cancelled_ReturnsTrue
- IsNativeLeaderTarget_NullOrder_ReturnsFalse (via seam)
- IsQxCancelEligible2_NullInstrument_ReturnsFalse (structural/seam)

Ambiguity note on IsPositionStateTriggerState convention (returns true for trigger states vs
non-trigger states): ticket explicitly instructs engineer to choose and document the convention.
The test name `_Filled_ReturnsFalse` suggests "returns false for trigger states" (i.e., returns
true when NOT a trigger = should skip). But test guidance says "if returns true for trigger states,
assert true". This is a deliberate ambiguity deferred to engineer with documentation requirement.
This does NOT constitute a FAIL because the guidance is internally consistent -- engineer resolves
convention and adapts test assertion. PASS.

### T3 VERDICT: TICKET_REVIEW_PASS

---

## Ticket 4 (T4) -- Tier D: CCN 10-12 (6 methods)

### Check 1: Traceability

PASS.

- All 6 Tier D methods from plan §4 Ticket 4 present: OnOrderUpdate(12),
  CancelAllAccountOrders(12), BuildQxSnapshot(11), DrainThenDispatch(11),
  FindFollowerBracketOrder IEnumerable overload(11), MatchesLeaderName(11).
- All 7 new helpers trace to plan §5.4 extraction designs.
- Note: T4 DrainThenDispatch description correctly identifies the signature revision from the
  plan: `IssueDrainCancels` accepts `List<Order> entryCandidates` not `Account acc` as second
  param (plan §5.4 note preserved in ticket). No phantom work.

### Check 2: 7-Scan Checklist

PASS. All 7 scans present:
- SCAN-01: scoped to 6 T4 methods + new helpers.
- SCAN-04: ExtractLegSuffix returning null for nullable string explicitly permitted.
- SCAN-07: T1+T2+T3+T4 tests pass. PASS.

### Check 3: JS Pre-Check

PASS.

- JS-021: all helpers listed. PASS.
- JS-002: `ExtractLegSuffix` returns `string` (nullable reference type in .NET 4.8) -- may return
  null for "no trailing digit". Ticket acknowledges this and notes: "acceptable in .NET 4.8 context;
  or return string.Empty as sentinel". The JS-002 concern is flagged; the ticket does NOT mandate
  null return -- it gives the engineer the option of string.Empty sentinel (which would be fully
  compliant). The SCAN-04 pass condition also explicitly permits this. PASS with annotation: engineer
  SHOULD prefer `string.Empty` over `null` for `ExtractLegSuffix` to fully comply with JS-002.
  This is advisory (P1), not blocking (P0).
- JS-001: HandleDrainTerminalState delegates to existing helpers only. PASS.
- JS-033: no async. PASS.

### Check 4: CYC Pre-Check

PASS.

- OnOrderUpdate: residual CCN<=8 (exactly at limit). PASS.
- CancelAllAccountOrders: residual CCN<=5. PASS.
- BuildQxSnapshot: residual CCN<=5. PASS.
- DrainThenDispatch: residual CCN<=4. PASS.
- FindFollowerBracketOrder IEnumerable: residual CCN<=7. PASS.
- MatchesLeaderName: residual CCN<=4. PASS.
- All helpers <=5 expected CCN. PASS.

Note: `MatchesBracketTypeTestable` is declared as an `internal static` method that inlines the
logic using primitives rather than delegating to `MatchesBracketType`. The ticket correctly
anticipates that `Order` cannot be constructed without NT8 runtime and provides the primitive-param
form. This is correct NT8/test-seam practice.

### Check 5: NT8 Constraints

PASS.

- IssueDrainCancels: `follower.Cancel(new Order[]{e})` correct AddOnBase cancel pattern. PASS.
- BuildQxSnapshot remains `internal static` after extraction. `IsQxSnapshotStateOk` is `private
  static`. Visibility model correct. PASS.
- OnOrderUpdate is NT8 event handler; HandleDrainTerminalState must NOT be async -- explicitly
  required. PASS.
- No AtmStrategyCreate, no AtmStrategyChangeStopTarget. PASS.

### Check 6: Completeness

PASS. All required sections present with 8 [Fact] tests listed.

### Check 7: Test Coverage

PASS.

8 [Fact] tests provided:
- IsCancelAllStateOk_Working_ReturnsTrue
- IsCancelAllStateOk_Filled_ReturnsFalse
- IsQxSnapshotStateOk_TriggerPending_ReturnsTrue
- IsQxSnapshotStateOk_Rejected_ReturnsFalse
- MatchesBracketType_StopMarket_IsStop_ReturnsTrue
- MatchesBracketType_Limit_IsStop_ReturnsFalse
- ExtractLegSuffix_Stop1_Returns1
- ExtractLegSuffix_NoDigit_ReturnsNull

All 4 static helpers that have test seams (IsCancelAllStateOk, IsQxSnapshotStateOk via seams,
MatchesBracketTypeTestable primitive form, ExtractLegSuffix via seam) are covered.
HandleDrainTerminalState (instance) and IssueDrainCancels (instance) are not unit-testable
without NT8 runtime -- consistent with project pattern. PASS.

### T4 VERDICT: TICKET_REVIEW_PASS

---

## Ticket 5 (T5) -- Tier E: CCN = 9 (11 methods)

### Check 1: Traceability

PASS.

- All 11 Tier E methods from plan §4 Ticket 5 present: HasNakedPosition(9), RuleToDto(9),
  IsFollowerAccount(9), AllAccounts(9), CaptureLinkedTargetPrice(9), MirrorClose(9),
  BuildUpdatedMultipliers(9), CaptureOtherLegTargetPrices(9), HandleEntryChange(9),
  HandleBracketChange(9), CreateFollowerReplacementStop(9).
- All 11 new helpers trace to plan §5.5 extraction designs.

One plan discrepancy noted (ADVISORY, not blocking):
- Plan §5.5 RuleToDto names the helper `ExtractAtmTemplateMap` returning `Dictionary<string,string>`.
  Ticket T5 names it `BuildAtmModeNames` returning `string[]` and provides explicit rationale
  ("actual code at L6213-6219 builds a string[] array, not a Dictionary"). This is a deliberate
  deviation from the plan, justified by a code inspection. The ticket explicitly documents this
  and confirms JS-009 PASS reasoning still holds. No phantom work. PASS.

- Plan §5.5 IsFollowerAccount lists the test seam signature as
  `internal static bool IsNativeLeaderTargetTestable(OrderState s, string oInstrFN, OrderType t, string name, string checkInstrFN)`.
  That is actually the T3 CountLeaderTargets seam signature, misplaced in the plan's IsFollowerAccount
  section. The ticket does not carry this over -- T5 has no test seam for MatchesFollowerSlot
  (it's a private static helper without an explicit seam specified). This is a plan typo, not a
  ticket defect. Tests for T5 test the static helpers via seams added in the test implementation
  guidance. PASS.

- Plan §5.5 AllAccounts states extract `IsFollowerForInstrument(Account acc, CopyRule rule) -> bool`.
  Ticket T5 revises this to `ResolveNullFollowerSlot(CopyRule rule, int i) -> Account` with
  documented rationale (iterator pattern limitation makes the original extraction impractical).
  This revision is a reasonable architectural refinement within the ticket. PASS.

- Plan §5.5 UpdateLegTargetPrice is described as `private static`. Ticket T5 NT8 Constraints
  section overrides to `private` (non-static) because it calls `IsTargetOrderLive(o)` which is
  a private instance method. Ticket updates the helper signature table accordingly. Correct. PASS.

### Check 2: 7-Scan Checklist

PASS. All 7 scans present:
- SCAN-01: scope is ENTIRE CopyEngine.cs (final ticket -- all 32 original methods + all new helpers
  must be at CCN<=8). PASS condition: zero output from lizard for the full file.
- SCAN-04: `ResolveNullFollowerSlot` returning null for reference type permitted; explicit grandfathering.
- SCAN-07: T1+T2+T3+T4+T5 tests pass. PASS.

### Check 3: JS Pre-Check

PASS with annotations.

- JS-021: all helpers listed. PASS.
- JS-002 (nullable reference returns -- advisory P1):
  * `ResolveNullFollowerSlot` returns `Account` (reference type) and may return null. Ticket
    grandfathers this as "existing NT8 pattern" consistent with FindBePosition etc. This is a
    P1 advisory, not a P0 block. The pattern is consistent with pre-existing grandfathered nulls
    in the codebase. PASS (with annotation: engineer must add `// NT8 pattern: null=not found`
    comment inline per project convention).
  * `ExtractAtmTemplateMap` -> `BuildAtmModeNames` returns `string[]` (reference type). If the
    array can never be null (it is always constructed with `new string[...]`), JS-002 is satisfied.
    Ticket does not describe a null return path for this helper. PASS.
  * `PickBestTargetPrice` returns `double?` (nullable value type). Not a JS-002 violation.
- JS-001: SubmitReplacementStopOrder absorbs existing try/catch only. PASS.
- JS-033: no async. PASS.
- ASCII-only: all new helper names are ASCII. PASS.

ADVISORY (not blocking): Engineer should prefer non-null returns wherever possible:
  - `ResolveNullFollowerSlot`: add comment `// NT8 pattern: null = slot could not be resolved`.
  - `ExtractLegSuffix` (T4): prefer `string.Empty` over `null` if sentinel semantics allow.

### Check 4: CYC Pre-Check

PASS.

- HasNakedPosition: residual CCN<=4. PASS.
- RuleToDto: residual CCN<=3. PASS.
- IsFollowerAccount: residual CCN<=4 (plan says <=3 for MatchesFollowerSlot + parent; ticket
  says <=4 -- both <=8). PASS.
- AllAccounts: residual CCN<=6. PASS.
- CaptureLinkedTargetPrice: residual CCN<=5. PASS.
- MirrorClose: residual CCN<=3. PASS.
- BuildUpdatedMultipliers: residual CCN<=5. PASS.
- CaptureOtherLegTargetPrices: residual CCN<=4. PASS.
- HandleEntryChange: residual CCN<=7. PASS.
- HandleBracketChange: residual CCN<=7. PASS.
- CreateFollowerReplacementStop: residual CCN<=2. PASS.
- All helpers <=5 expected CCN. PASS.

### Check 5: NT8 Constraints

PASS.

- MirrorCloseOneAccount: order name `"PTT-Mirror-Close"` required. No Submit call (preserves
  existing behavior exactly). Explicitly documented. PASS.
- SubmitReplacementStopOrder: order name `"PTT-STP-Drag"` (unsuffixed -- matches original
  CreateFollowerReplacementStop). PASS.
- ResolveNullFollowerSlot: uses ConcurrentDictionary TryGetValue/TryAdd (lock-free). PASS.
- UpdateLegTargetPrice: revised to `private` (non-static) due to IsTargetOrderLive instance
  call. NT8 constraint section explicitly notes this. PASS.
- Post-T5 sync gate: `ptt-sync-and-verify.ps1` + F5 NT8 compilation required in acceptance
  criteria. PASS.

### Check 6: Completeness

PASS. All required sections present. 8 [Fact] tests listed. Post-T5 verification gate with
4 final verification commands present.

### Check 7: Test Coverage

PASS.

8 [Fact] tests provided:
- ResolveMultiplierLength_CountZeroNullExisting_ReturnsZero (seam)
- ResolveMultiplierLength_CountPositive_ReturnsCount
- IsPriceDeltaSignificant_ZeroTickSize_ReturnsFalse (seam)
- IsPriceDeltaSignificant_SmallDelta_ReturnsTrue
- RoundToTick_ZeroTickSize_ReturnsRawPrice (seam)
- RoundToTick_PositiveTickSize_ReturnsRoundedPrice
- PickBestTargetPrice_PttHasValue_ReturnsPtt (seam)
- PickBestTargetPrice_PttNull_ReturnsAtm

All static helpers with pure-computation logic (ResolveMultiplierLength, IsPriceDeltaSignificant,
RoundToTick, PickBestTargetPrice) have seam-based tests with concrete input/output assertions.
Instance helpers (IsNakedConditionMet reads acc.Orders -- NT8 runtime dependent;
MirrorCloseOneAccount, SubmitReplacementStopOrder -- NT8 dependent) are not directly tested.
Consistent with established project pattern. PASS.

### T5 VERDICT: TICKET_REVIEW_PASS

---

## Aggregate Coverage Check

### 32-Method CCN Coverage (from plan baseline table)

| Tier | CCN | Methods | Ticket | Covered |
|------|-----|---------|--------|---------|
| A | 20-27 | 6 | T1 | YES |
| B | 16-19 | 4 | T2 | YES |
| C | 13-15 | 5 | T3 | YES |
| D | 10-12 | 6 | T4 | YES |
| E | 9 | 11 | T5 | YES |
| **Total** | | **32** | | **32/32 = 100%** |

All 32 methods from the plan baseline are covered by exactly one ticket each. No duplicates.
No gaps. PASS.

### Dismissed Items (Do Not Touch) -- Verified Present in Plan, Absent from Tickets

- `(long)(int)Environment.TickCount` -- not touched. PASS.
- `ActiveOrders .ToList()` -- not touched (DrainThenDispatch extraction notes ActiveOrders
  usage as a pre-existing field; helper receives pre-filtered List<Order>). PASS.
- `Features/*.cs` -- not referenced in any ticket. PASS.
- `_drainOwnedOrderIds ConcurrentDictionary` -- not modified. PASS.

### File Routing Check

All `.cs` modifications target `src/PropTraderTools/CopyEngine.cs` within `C:\WSGTA\ptt-lane-b\`.
New test file: `src/PropTraderTools/Tests/BwaveRefactorLaneBTests.cs` within `C:\WSGTA\ptt-lane-b\`.
No Director workspace paths referenced for `.cs` files. PASS.

---

## Gate Clearance Statement

All 5 tickets pass all 7 checks. The engineer is cleared to begin T1.

**Execution order**: T1 -> T2 -> T3 -> T4 -> T5 (sequential; each ticket must pass all 7 scans
before the next begins).

**Pre-work verification** (engineer runs before starting T1):
1. Confirm `git status --short src/PropTraderTools/` is clean.
2. Run `dotnet build --no-incremental` to confirm clean baseline.
3. Run `Select-String -Path src/PropTraderTools/CopyEngine.cs -Pattern "lock\s*\("` -> zero matches.
4. For each new helper name in T1 (Table: Name Collision Registry), run:
   `Select-String -Path src/PropTraderTools/CopyEngine.cs -Pattern "private.*<HelperName>"` -> zero matches.

**TICKET_REVIEW_PASS**
