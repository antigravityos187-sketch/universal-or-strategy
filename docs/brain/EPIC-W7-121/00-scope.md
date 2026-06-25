# Phase 1: Scope Definition - EPIC-W7-121

**Agent**: v12-phase1-scope
**Date**: 2026-06-24T19:40:57Z
**Target Method**: SymmetryGuardCascadeFollowerCleanup
**File**: V12_002.Symmetry.Replace.cs
**Current Complexity**: 10
**Target Complexity**: ≤ 8

## Scope Boundary Analysis

### Method Structure
The `SymmetryGuardCascadeFollowerCleanup` method (lines 198-243) contains 46 lines with the following logical sections:

1. **Guard Conditions** (lines 200-203): Dispatch lookup validation
2. **Follower Snapshot** (line 206): Immutable array retrieval
3. **Cascade Logging** (lines 208-215): Master cancellation notification
4. **Follower Iteration** (lines 217-243): Order cancellation loop with nested conditions

### Complexity Breakdown
- **Base complexity**: 1 (method entry)
- **Guard conditions**: +2 (two early returns)
- **Follower iteration**: +1 (foreach loop)
- **Position lookup**: +1 (if continue)
- **Order lookup**: +1 (if continue)
- **Null check**: +1 (if continue)
- **Order state validation**: +3 (three OR conditions)
- **Total**: 10

## IN SCOPE

### Primary Extraction Target
**Method**: `SymmetryGuardCascadeFollowerCleanup` (CYC 10)
- **Lines**: 198-243
- **Reason**: Exceeds Jane Street threshold by +2

### Extraction Candidates

#### 1. ValidateDispatchContext (Priority: HIGH)
**Lines**: 200-203
**Complexity Reduction**: -2
**Purpose**: Extract guard condition validation
**Signature**: `private bool ValidateDispatchContext(string masterEntryName, out string dispatchId, out SymmetryDispatchContext ctx)`
**Rationale**: Consolidates two TryGetValue checks into single validation method

#### 2. CancelFollowerEntryOrder (Priority: HIGH)
**Lines**: 219-243 (inner loop body)
**Complexity Reduction**: -5
**Purpose**: Extract follower cancellation logic
**Signature**: `private void CancelFollowerEntryOrder(string followerName, string masterEntryName)`
**Rationale**: Encapsulates position lookup, order validation, state checking, and cancellation

### Expected Outcome
- **Original method**: CYC 10 → CYC 3 (guard + loop + log)
- **ValidateDispatchContext**: CYC 3 (two checks + return)
- **CancelFollowerEntryOrder**: CYC 5 (three lookups + state check)
- **All methods**: ≤ 8 ✅

## OUT OF SCOPE

### Adjacent Methods (No Changes)
- `SymmetryGuardRetargetExistingFollowerBracket` (lines 17-25)
- `SymmetryGuardReplaceExistingFollowerTarget` (lines 27-97)
- `SymmetryGuardSkipFollower` (lines 99-132)
- `SymmetryGuardTryResolveFollowersForDispatch` (lines 134-191)
- `SymmetryGuardForgetEntry` (lines 245-263)
- `SymmetryGuardPruneDispatches` (lines 265-302)

**Rationale**: These methods are not part of EPIC-W7-121 scope. Each will be addressed in separate epics if they exceed CYC threshold.

### Data Structures (No Changes)
- `symmetryMasterEntryToDispatch` (ConcurrentDictionary)
- `symmetryDispatchById` (ConcurrentDictionary)
- `activePositions` (ConcurrentDictionary)
- `entryOrders` (ConcurrentDictionary)

**Rationale**: Data structures are shared across Symmetry module. Changes would affect blast radius beyond this epic.

### Helper Methods (No Changes)
- `CancelOrderSafe` (existing helper)
- `Print` (logging infrastructure)

**Rationale**: These are stable, tested utilities used throughout the codebase.

## Boundary Validation

### Scope Creep Prevention
✅ **ONE EPIC = ONE CONCERN**: Only `SymmetryGuardCascadeFollowerCleanup` is targeted
✅ **No Adjacent Refactoring**: Other methods in file remain untouched
✅ **No Infrastructure Changes**: Existing data structures and helpers preserved
✅ **Clear Success Criteria**: CYC 10 → ≤ 8 with two extractions

### Risk Assessment
- **Blast Radius**: LOW - Method is self-contained within Symmetry Replace module
- **Test Coverage**: MEDIUM - Requires unit tests for extracted methods
- **Integration Risk**: LOW - Method signature unchanged, behavior preserved

## Jane Street Alignment

### Violated Principles (Current State)
1. **Cognitive Simplicity**: CYC 10 exceeds threshold for microsecond-latency reasoning
2. **Testability**: 10 paths create exponential test case growth
3. **Single Responsibility**: Method handles validation + iteration + cancellation

### Compliance Path (Target State)
1. **ValidateDispatchContext**: Isolates guard logic (CYC 3)
2. **CancelFollowerEntryOrder**: Encapsulates cancellation logic (CYC 5)
3. **Main method**: Orchestrates flow only (CYC 3)
4. **Result**: All methods ≤ 8, improved testability, reduced cognitive load

## Success Criteria

### Phase 1 Completion
- ✅ Scope boundaries clearly defined
- ✅ IN SCOPE: 1 method, 2 extractions identified
- ✅ OUT OF SCOPE: Adjacent methods, data structures, helpers
- ✅ Complexity targets validated (10 → 3+3+5 = all ≤ 8)

### Epic Completion (Future Phases)
- [ ] Phase 2: Architecture plan with extraction signatures
- [ ] Phase 3: DNA audit and PR review
- [ ] Phase 4: Ticket generation (2 tickets expected)
- [ ] Phase 5: Implementation and verification
- [ ] Phase 6: Final review and NinjaTrader F5 test

## Next Phase

**Phase 2 (Architecture Planning)** will:
1. Design exact method signatures for extractions
2. Plan state management approach (immutable snapshots)
3. Verify lock-free Actor pattern compliance
4. Generate Mermaid diagrams for call flow

---

**Scope Definition Complete**: Ready for Phase 2 Architecture Planning
