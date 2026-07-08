# EPIC-W7-015 Ticket 1 Completion

**Method**: CancelAll_ProcessSingleFleetAccount
**File**: src/V12_002.UI.IPC.Commands.Fleet.cs
**Status**: COMPLETED
**CYC Before**: 19
**CYC After**: 6
**Helpers Extracted**: CancelAll_IsOrderCancellable (CYC=7), CancelAll_IsBracketOrder (CYC=7)
**Behavior Change**: None -- structural refactor only. LINQ simplification: .Where().ToList().Any() -> .Any() with compound predicate (same result, zero allocation improvement)
**DNA**: No lock() blocks, ASCII-only, UTF-8 no BOM

## Agent Tracking

- **Wave**: 7
- **Phase**: 5 (Ticket Execution)
- **Epic**: EPIC-W7-015
- **Ticket**: 1

## Changes Made

### CancelAll_ProcessSingleFleetAccount (lines 326-348)
- Replaced `.Where().ToList().Any()` chain with single `.Any()` compound predicate (zero allocation)
- Replaced nested if-blocks with two early-continue guards
- Delegates to extracted predicates: CancelAll_IsOrderCancellable, CancelAll_IsBracketOrder
- Preserved Build 1104.1 comment for bracket preservation logic

### CancelAll_IsOrderCancellable (lines 351-363) [NEW]
- Pure predicate: returns false for null order, false for wrong instrument
- Returns true for any of the 5 working states (Working/Accepted/Submitted/ChangePending/ChangeSubmitted)
- CYC = 7 (if-null + if-instrument + 5 OR branches)

### CancelAll_IsBracketOrder (lines 365-376) [NEW]
- Pure static predicate: returns true if order name starts with any bracket prefix
- Prefixes: Stop_, S_, T1_, T2_, T3_, T4_, T5_
- CYC = 7 (7 OR branches)

## CYC Verification

| Method | CYC |
|--------|-----|
| CancelAll_ProcessSingleFleetAccount | 6 |
| CancelAll_IsOrderCancellable | 7 |
| CancelAll_IsBracketOrder | 7 |

All <= 8. Jane Street CYC<=8 mandate satisfied.

## Jane Street Alignment

- Zero allocation: LINQ .Any() compound predicate replaces .Where().ToList().Any()
- Single-responsibility: each method has exactly one concern
- Cognitive simplicity: no nested if-blocks, flat guard clauses
- Pure predicates: both helpers are side-effect free
