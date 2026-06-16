# Phase 2: Architecture Planning - EPIC-CCN-057

## V12.23 Protocol Compliance
This document defines the extraction strategy for reducing ShouldProtectBracketOrder complexity from 10 to ≤8.

## Target Method Analysis

### Current Implementation
- **Method**: ShouldProtectBracketOrder
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Lines**: 1451-1476 (26 lines total, 16 LOC)
- **Current Complexity**: 10
- **Target Complexity**: ≤8 (Jane Street strict standard)

### Complexity Breakdown
Base complexity: 1
if (force) return false: +1 = 2
|| orderName.StartsWith("Stop_", ...): +1 = 3
|| orderName.StartsWith("S_", ...): +1 = 4
|| orderName.StartsWith("T1_", ...): +1 = 5
|| orderName.StartsWith("T2_", ...): +1 = 6
|| orderName.StartsWith("T3_", ...): +1 = 7
|| orderName.StartsWith("T4_", ...): +1 = 8
|| orderName.StartsWith("T5_", ...): +1 = 9
|| orderName.StartsWith("Target_", ...): +1 = 10
Total Cyclomatic Complexity: 10

## Extraction Strategy

### Proposed Refactoring
Extract the bracket order detection logic (8 OR conditions) into a dedicated helper method.

**Result**:
- **Original method** (ShouldProtectBracketOrder): CYC 3
  - Base: 1
  - if (force): +1
  - if (IsBracketOrderName(...)): +1
- **Helper method** (IsBracketOrderName): CYC 8
  - Base: 1
  - 7 || operators: +7

### Complexity Validation
✅ Original method: 3 ≤ 8 (Jane Street compliant)
✅ Helper method: 8 ≤ 8 (Jane Street compliant)
✅ Total reduction: 10 → max(3, 8) = 8

## Method Signatures

### Original Method (Modified)
private bool ShouldProtectBracketOrder(string orderName, bool force, string accountName)

### Proposed Helper Method
private bool IsBracketOrderName(string orderName)

## Call Graph

### Data Flow
ShouldProtectBracketOrder(orderName, force, accountName)
    ├─► if (force) → return false [early exit]
    ├─► IsBracketOrderName(orderName) → bool isBracketOrder
    │       └─► Check 8 prefix patterns (Stop_, S_, T1-T5_, Target_)
    └─► if (isBracketOrder) → Print + return true else → return false

### Method Relationships
- **Caller**: SweepBrokerOrders (line 1416)
- **Callee (new)**: IsBracketOrderName (to be created)
- **Shared State**: None (pure function, no side effects)

## Implementation Plan

### Step 1: Create Helper Method
**Location**: Insert immediately after ShouldProtectBracketOrder (after line 1476)

### Step 2: Refactor Original Method
Replace lines 1456-1464 with single call to helper

### Step 3: Update XML Documentation
Add EPIC-CCN-057 reference to original method's XML doc

## Lock-Free Validation

### Compliance Check
✅ **No lock() statements**: Both methods are pure functions with no locking
✅ **No shared mutable state**: All parameters are value types or immutable strings
✅ **No race conditions**: No state mutations, thread-safe by design
✅ **Atomic operations**: Not required (no state changes)
✅ **FSM/Actor pattern**: Not applicable (pure helper functions)

### Thread Safety Analysis
- **ShouldProtectBracketOrder**: Thread-safe (no state, no side effects except logging)
- **IsBracketOrderName**: Thread-safe (pure function, no side effects)
- **Print() call**: Assumed thread-safe (NinjaTrader framework method)

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
✅ **Original method**: CYC 3 (well below threshold)
✅ **Helper method**: CYC 8 (at threshold, acceptable)
✅ **Single Responsibility**: Each method has one clear purpose
✅ **Readability**: Method names clearly describe intent

### Correctness by Construction
✅ **Semantic Equivalence**: Extracted logic is identical to original
✅ **No Behavior Changes**: Pure refactoring, zero functional changes
✅ **Type Safety**: All parameters strongly typed
✅ **Immutability**: String parameters are immutable

### Testing Strategy
✅ **Existing Tests**: Must pass without modification
✅ **No New Tests Required**: Scope boundary constraint
✅ **Regression Safety**: Semantic equivalence guarantees correctness

## Risk Assessment

### Blast Radius
- **Files Modified**: 1 (V12_002.SIMA.Lifecycle.cs)
- **Methods Modified**: 1 (ShouldProtectBracketOrder)
- **Methods Added**: 1 (IsBracketOrderName)
- **Callers Affected**: 0 (no signature changes)
- **Callees Affected**: 0 (no external dependencies)

### Rollback Strategy
1. Git checkpoint before extraction
2. Atomic commit for helper method creation
3. Atomic commit for original method refactoring
4. Immediate rollback if any test fails
5. Bob CLI restore point available

### Validation Criteria
- ✅ Build succeeds (zero errors)
- ✅ All existing tests pass (100% pass rate)
- ✅ Complexity audit shows CYC ≤8 for both methods
- ✅ CSharpier formatting passes
- ✅ No lock() statements introduced
- ✅ ASCII-only compliance maintained

## Next Steps

### Phase 3: DNA & PR Audit (Adjudicator)
- Arena AI red team review
- Verify lock-free compliance
- Validate Jane Street alignment
- PR health check (diff size, ASCII-only)

### Phase 4: Implementation (Engineer)
- Bob CLI surgical extraction
- Create IsBracketOrderName helper
- Refactor ShouldProtectBracketOrder
- Run CSharpier formatting
- Verify complexity reduction

### Phase 5: Verification (Forensics)
- Run complexity_audit.py
- Verify CYC ≤8 for both methods
- Run build_readiness.ps1
- Run all tests (100% pass required)
- Compare against implementation plan

### Phase 6: Sign-off (Director)
- Run deploy-sync.ps1
- F5 in NinjaTrader
- Verify BUILD_TAG
- Update EPIC-CCN-057 manifest
- Close ticket

## Approval Status

**Architecture Plan Status**: ✅ READY FOR REVIEW

**Compliance Checklist**:
- ✅ Complexity reduction strategy defined (10 → 8)
- ✅ Method signatures documented
- ✅ Call graph and data flow mapped
- ✅ Lock-free validation completed
- ✅ Jane Street alignment verified
- ✅ Risk assessment completed
- ✅ Implementation steps defined

**Ready for Phase 3**: Adjudicator review (Arena AI)
