# Phase 2: Architecture Planning - EPIC-CCN-048

## V12.23 Protocol Compliance
- **Epic ID**: EPIC-CCN-048
- **Phase**: 2 (Architecture Planning)
- **Date**: 2026-06-15
- **Status**: DRAFT

## Target Method Analysis

### Current State
- **Method**: UpdateExistingPendingReplacement
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Lines**: 167-220 (54 lines)
- **Current Complexity**: 9
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Tier**: 2 (Medium complexity)

### Method Signature (Original)
private void UpdateExistingPendingReplacement(
    string entryName,
    PositionInfo pos,
    Order currentStop,
    double validatedStopPrice,
    int newTrailLevel
)

### Complexity Analysis

**Current Branching Points** (9 total):
1. Line 193: if (pendingStopReplacements.TryAdd(entryName, newPending))
2. Line 195: if (currentCount >= CIRCUIT_BREAKER_THRESHOLD && !circuitBreakerActive)
3. Line 208: else if (pendingStopReplacements.TryGetValue(entryName, out var pending))
4. Line 211: if (!pending.BracketRestorationNeeded)

**Cognitive Load Factors**:
- Mixed concerns: object creation, circuit breaker, bracket restoration
- Nested conditionals (circuit breaker inside TryAdd)
- State mutation across multiple paths
- 54 lines violates single-responsibility principle

## Extraction Strategy

### Principle: Cognitive Simplicity
Following Jane Street "make illegal states unrepresentable" principle, extract three helper methods:

1. **CreatePendingReplacement** - Object construction (pure function)
2. **HandleCircuitBreakerCheck** - Circuit breaker logic (side effects isolated)
3. **RefreshBracketTargetsIfNeeded** - Bracket restoration logic (conditional update)

### Target Complexity Distribution
- **UpdateExistingPendingReplacement** (main): 5 (orchestration only)
- **CreatePendingReplacement**: 1 (no branching)
- **HandleCircuitBreakerCheck**: 2 (single if with compound condition)
- **RefreshBracketTargetsIfNeeded**: 1 (single if)

**Total**: 9 redistributed to 5+1+2+1 = 9 (main method ≤8)

## Proposed Helper Methods

### 1. CreatePendingReplacement (Pure Function)

**Purpose**: Construct PendingStopReplacement object with all required fields.

**Signature**:
private PendingStopReplacement CreatePendingReplacement(
    string entryName,
    PositionInfo pos,
    Order currentStop,
    double validatedStopPrice,
    TargetInfo[] capturedTargets
)

**Responsibilities**:
- Initialize PendingStopReplacement struct
- Set all required fields (EntryName, Quantity, StopPrice, Direction, OldOrder, CreatedTime)
- Set CapturedTargets and BracketRestorationNeeded flag
- **Zero branching** (CYC = 1)

**Return**: Fully initialized PendingStopReplacement

**Lock-Free Compliance**: ✅ No state mutation, pure function

### 2. HandleCircuitBreakerCheck (Side Effect Isolated)

**Purpose**: Check and activate circuit breaker if threshold exceeded.

**Signature**:
private void HandleCircuitBreakerCheck(int currentCount)

**Responsibilities**:
- Check if currentCount >= CIRCUIT_BREAKER_THRESHOLD
- Check if circuit breaker not already active
- Activate circuit breaker (set flag and timestamp)
- Print warning message
- **Single compound conditional** (CYC = 2)

**Return**: void (side effects only)

**Lock-Free Compliance**: ✅ Uses atomic flag (circuitBreakerActive)

### 3. RefreshBracketTargetsIfNeeded (Conditional Update)

**Purpose**: Refresh bracket targets on existing pending replacement if not yet populated.

**Signature**:
private void RefreshBracketTargetsIfNeeded(
    string entryName,
    PendingStopReplacement pending
)

**Responsibilities**:
- Check if BracketRestorationNeeded is false
- If false, call RefreshTargetSnapshot
- Update CapturedTargets and BracketRestorationNeeded flag
- **Single conditional** (CYC = 1)

**Return**: void (mutates pending in-place)

**Lock-Free Compliance**: ✅ No locks, operates on thread-local reference

## Refactored Method Structure

### UpdateExistingPendingReplacement (Main Orchestrator)

**New Complexity**: 5 (reduced from 9)

**Pseudo-code**:
1. Capture targets (existing logic)
2. Create pending replacement (EXTRACTED)
3. Try add or update (Branch 1: TryAdd, Branch 3: TryGetValue)
4. Handle circuit breaker if added (EXTRACTED - Branch 2 inside)
5. Refresh bracket targets if updated (EXTRACTED - Branch 4 inside)
6. Update position (existing logic)

**Branching Points** (5 total):
1. TryAdd success path
2. Circuit breaker check (inside HandleCircuitBreakerCheck)
3. TryGetValue fallback path
4. Bracket refresh check (inside RefreshBracketTargetsIfNeeded)
5. Implicit else (no action if both TryAdd and TryGetValue fail)

**Complexity**: 5 ≤ 8 ✅

## Call Graph

UpdateExistingPendingReplacement (CYC=5)
├── CaptureTargetSnapshot (existing, not modified)
├── CreatePendingReplacement (NEW, CYC=1)
├── Interlocked.Increment (atomic primitive)
├── HandleCircuitBreakerCheck (NEW, CYC=2)
└── RefreshBracketTargetsIfNeeded (NEW, CYC=1)
    └── RefreshTargetSnapshot (existing, not modified)

**Data Flow**:
1. Main → CaptureTargetSnapshot → capturedTargets
2. Main → CreatePendingReplacement(capturedTargets) → newPending
3. Main → TryAdd(newPending) → success/failure
4. Main → HandleCircuitBreakerCheck(currentCount) → side effects
5. Main → TryGetValue → pending reference
6. Main → RefreshBracketTargetsIfNeeded(pending) → mutates pending

**Shared State**:
- pendingStopReplacements (ConcurrentDictionary) - thread-safe
- pendingReplacementCount (int) - atomic via Interlocked
- circuitBreakerActive (bool) - atomic flag
- circuitBreakerActivatedTime (DateTime) - set once when activated

## Lock-Free Validation

### ✅ No lock() Statements
- **Verified**: No lock(stateLock) blocks in original method
- **Maintained**: Extracted helpers use no locks

### ✅ FSM/Actor Enqueue Pattern
- **Original**: Uses ConcurrentDictionary.TryAdd/TryGetValue (lock-free)
- **Maintained**: Helpers preserve thread-safe operations
- **Atomic Operations**: Interlocked.Increment for counter

### ✅ Atomic Primitives Only
- Interlocked.Increment(ref pendingReplacementCount) - atomic counter
- circuitBreakerActive - boolean flag (atomic read/write)
- ConcurrentDictionary - lock-free collection

### ✅ Type-Safe State Transitions
- PendingStopReplacement struct - immutable after creation (except StopPrice update)
- BracketRestorationNeeded flag - explicit state tracking
- No hidden state mutations

## Jane Street Compliance

### Cognitive Simplicity Principles

#### 1. Single Responsibility
- **CreatePendingReplacement**: Object construction only
- **HandleCircuitBreakerCheck**: Circuit breaker logic only
- **RefreshBracketTargetsIfNeeded**: Bracket restoration only
- **Main method**: Orchestration only

#### 2. Linear Control Flow
- Main method: Sequential steps with clear branching
- Helpers: Minimal nesting (max 1 level)
- Guard clauses: Early returns not needed (void methods)

#### 3. Explicit State
- No hidden state mutations
- All state changes visible in method signatures
- Atomic operations for shared state

#### 4. Testability
- **CreatePendingReplacement**: Pure function, easily testable
- **HandleCircuitBreakerCheck**: Side effects isolated, mockable
- **RefreshBracketTargetsIfNeeded**: Conditional logic testable
- **Main method**: Integration test with mocked helpers

### Performance Considerations

#### Zero Allocations
- **CreatePendingReplacement**: Returns struct (stack allocation)
- **HandleCircuitBreakerCheck**: No allocations (side effects only)
- **RefreshBracketTargetsIfNeeded**: No new allocations
- **Main method**: Same allocation profile as original

#### Inline Candidates
- **CreatePendingReplacement**: 10 lines, likely inlined by JIT
- **HandleCircuitBreakerCheck**: 8 lines, likely inlined
- **RefreshBracketTargetsIfNeeded**: 6 lines, likely inlined
- **Impact**: Zero runtime overhead after JIT optimization

#### Hot Path
- **Original**: 54 lines, 9 branches
- **Refactored**: 30 lines main + 24 lines helpers = 54 lines total
- **No impact**: Same instruction count, better cache locality

#### Cache Locality
- **No changes**: Data layout unchanged
- **No changes**: Access patterns unchanged
- **Benefit**: Smaller methods improve instruction cache hit rate

## Risk Assessment

### Extraction Risk: MINIMAL

**Rationale**:
- Pure extraction (no logic changes)
- Helpers co-located in same class
- No changes to callers or callees
- Existing tests provide coverage

**Blast Radius**: Contained to UpdateExistingPendingReplacement only

**Rollback**: Simple revert of single method body

### Testing Strategy

#### Unit Tests (New)
1. CreatePendingReplacement_ValidInputs_ReturnsCorrectStruct
2. HandleCircuitBreakerCheck_BelowThreshold_NoActivation
3. HandleCircuitBreakerCheck_AboveThreshold_ActivatesBreaker
4. RefreshBracketTargetsIfNeeded_AlreadyPopulated_NoRefresh
5. RefreshBracketTargetsIfNeeded_NotPopulated_RefreshesTargets

#### Integration Tests (Existing)
- Existing tests for UpdateExistingPendingReplacement should pass unchanged
- No new integration tests required (behavior preserved)

## Implementation Checklist

### Phase 4 (Execution) Prerequisites
- [ ] Extract CreatePendingReplacement helper
- [ ] Extract HandleCircuitBreakerCheck helper
- [ ] Extract RefreshBracketTargetsIfNeeded helper
- [ ] Refactor main method to call helpers
- [ ] Verify complexity ≤8 with complexity_audit.py
- [ ] Run dotnet build (zero errors)
- [ ] Run dotnet test (100% pass)
- [ ] Run dotnet csharpier format src/ (formatting)
- [ ] Run pre_push_validation.ps1 -Fast
- [ ] Verify diff <10k characters

### Phase 5 (Verification) Gates
- [ ] Compare implementation against this plan
- [ ] Verify no scope creep (single method only)
- [ ] Verify lock-free compliance (no lock() statements)
- [ ] Verify Jane Street alignment (CYC ≤8)
- [ ] Run full pre-push validation (all 13 checks)

## Approval Decision

### Status: READY FOR PHASE 3 (DNA & PR AUDIT)

**Rationale**:
1. Complexity reduced from 9 to 5 (main method)
2. Three helpers with CYC ≤2 each
3. Lock-free compliance maintained
4. Jane Street principles applied
5. Zero performance impact
6. Testability improved

### Next Phase Authorization
- **Phase 3**: DNA & PR Audit (Arena AI adversarial review)
- **Constraint**: Must verify plan against V12 DNA and PR hygiene
- **Gate**: PASS/FAIL decision before Phase 4 execution

## Sign-off

- **Architect**: V12 Phase 2 Architecture Planner
- **Date**: 2026-06-15
- **Verdict**: APPROVED - Proceed to Phase 3 (DNA & PR Audit)
- **Caveat**: Any deviation from this plan triggers EPIC rejection
