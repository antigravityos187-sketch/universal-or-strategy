# Phase 2: Architecture Planning - EPIC-CCN-013

## Target Method Analysis

### Current State
- **Method**: UpdatePanelState
- **File**: src/V12_002.UI.Panel.StateSync.cs
- **Lines**: 13-87 (51 LOC)
- **Complexity**: CYC = 16
- **Access**: private void
- **Parameters**: None (uses class state)

## Extraction Strategy

### Goal
Reduce UpdatePanelState from CYC=16 to CYC≤8 by extracting 3 helper methods.

### Proposed Decomposition

#### Helper Method 1: UpdatePriceDisplay
**Responsibility**: Handle price text and color updates based on market position
**Complexity**: CYC = 4
- Lines 18-28 (price display logic)
- Encapsulates: price formatting, null checks, position-based coloring

#### Helper Method 2: UpdateModeAndConfig
**Responsibility**: Synchronize mode and configuration state
**Complexity**: CYC = 2
- Lines 30-44 (mode + config sync)
- Encapsulates: mode change detection, config revision tracking

#### Helper Method 3: UpdateTargetCountWithGuard
**Responsibility**: Handle target count updates with click guard
**Complexity**: CYC = 3
- Lines 46-58 (target count logic)
- Encapsulates: count validation, guard timing, UI updates

#### Remaining in Main Method: UpdatePanelState
**Complexity**: CYC = 7 (target ≤8 ✅)
- Early exit guard (CYC +1)
- RMA toggles null checks (CYC +2)
- Live position handling (CYC +3)
- Cleanup logic (CYC +1)

### Total Complexity Budget
- Main method: CYC = 7
- Helper 1: CYC = 4
- Helper 2: CYC = 2
- Helper 3: CYC = 3
- **Total**: CYC = 16 (preserved, redistributed)

## Lock-Free Validation

### ✅ Compliance Verified
1. **No lock() statements**: Confirmed by Phase 0 analysis
2. **UI Thread Safety**: All UI updates occur on UI thread (WPF dispatcher model)
3. **Atomic Operations**: String and int assignments are atomic in .NET
4. **No Race Conditions**: Single-threaded UI updates

## Jane Street Compliance
- ✅ Main method: CYC = 7 (within threshold)
- ✅ All helpers: CYC ≤8
- ✅ Cognitive simplicity maintained

## Implementation Plan
1. Extract UpdatePriceDisplay (lines 18-28)
2. Extract UpdateModeAndConfig (lines 30-44)
3. Extract UpdateTargetCountWithGuard (lines 46-58)
4. Verify CYC = 7 for main method

## Success Criteria
- [ ] UpdatePanelState CYC reduced from 16 to ≤8
- [ ] All 3 helper methods have CYC ≤8
- [ ] No compilation errors
- [ ] All existing tests pass
- [ ] No lock() statements introduced

## Next Phase
**Phase 3: DNA & PR Audit** - Arena AI red team review

## Metadata
- **Epic**: EPIC-CCN-013
- **Phase**: 2 (Architecture Planning)
- **Created**: 2026-06-15
- **Status**: READY FOR REVIEW
- **Architect**: Bob Shell (v12-engineer mode)
- **Jane Street Alignment**: ✅ CYC ≤8
- **Lock-Free**: ✅ No locks, UI thread model
