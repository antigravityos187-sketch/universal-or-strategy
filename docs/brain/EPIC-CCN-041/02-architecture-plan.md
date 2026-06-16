# Phase 2: Architecture Planning - EPIC-CCN-041

## Target Method Analysis

### Current State
- **Method**: SymmetryGuardPruneDispatches
- **File**: src/V12_002.Symmetry.Replace.cs
- **Current Complexity**: 10 (CYC)
- **Lines of Code**: 20
- **Tier**: 2 (Medium complexity)

### Complexity Breakdown
The method contains 10 decision points:
1. Null check on ctx
2. TTL expiration check
3. Anchor resolution check
4. Loop iteration over followers
5. Active position check
6. Break condition in loop
7. Active followers check
8. Final removal decision
9. Dictionary removal operation
10. Implicit continue in foreach

## Extraction Strategy

### Goal
Reduce complexity from CYC=10 to CYC≤8 (Jane Street strict standard) through extraction of 3 private helper methods.

### Proposed Helper Methods

#### 1. IsDispatchExpired
**Purpose**: Encapsulate TTL expiration logic
**Complexity Reduction**: -2 decision points

**Rationale**:
- Single responsibility: TTL validation only
- Pure function: no side effects
- Testable: clear input/output contract

#### 2. HasActiveFollowers
**Purpose**: Encapsulate active followers detection logic
**Complexity Reduction**: -4 decision points

**Rationale**:
- Extracts nested loop logic
- Early return pattern (Jane Street preference)
- Lock-free: uses immutable snapshot + ConcurrentDictionary.ContainsKey
- Testable: clear boolean output

#### 3. ShouldRemoveDispatch
**Purpose**: Centralize removal decision logic
**Complexity Reduction**: -3 decision points

**Rationale**:
- Orchestrates the two helper methods
- Clear decision tree structure
- Guard clause pattern (null check first)
- Testable: all branches covered

### Refactored Main Method
**New Complexity**: CYC=3 (foreach + if + TryRemove)
**Reduction**: 10 to 3 (70% reduction, well below target of ≤8)

## Method Signatures

### Original Method
- **Access**: Private
- **Return**: void
- **Parameters**: None
- **Side Effects**: Mutates symmetryDispatchById (ConcurrentDictionary)

### Helper Method 1: IsDispatchExpired
- **Access**: Private
- **Return**: bool (true if expired)
- **Parameters**: SymmetryDispatchContext ctx, DateTime nowUtc
- **Side Effects**: None (pure function)

### Helper Method 2: HasActiveFollowers
- **Access**: Private
- **Return**: bool (true if any follower is active)
- **Parameters**: string[] followers (immutable snapshot)
- **Side Effects**: None (read-only access to activePositions)

### Helper Method 3: ShouldRemoveDispatch
- **Access**: Private
- **Return**: bool (true if dispatch should be removed)
- **Parameters**: SymmetryDispatchContext ctx (nullable), DateTime nowUtc
- **Side Effects**: None (orchestration only)

## Call Graph

SymmetryGuardPruneDispatches (CYC=3) calls ShouldRemoveDispatch (CYC=5) which calls IsDispatchExpired (CYC=1) and HasActiveFollowers (CYC=2)

### Data Flow
1. Main Method captures DateTime.UtcNow once
2. Main Method iterates symmetryDispatchById.ToArray() (immutable snapshot)
3. Main Method calls ShouldRemoveDispatch(ctx, nowUtc) for each entry
4. ShouldRemoveDispatch guards null, calls IsDispatchExpired
5. ShouldRemoveDispatch if not expired, calls HasActiveFollowers
6. HasActiveFollowers iterates immutable followers[] snapshot
7. HasActiveFollowers checks activePositions.ContainsKey() (lock-free)
8. Main Method if true, calls symmetryDispatchById.TryRemove() (atomic)

### Shared State
- **Read-Only**: activePositions (ConcurrentDictionary, thread-safe reads)
- **Mutated**: symmetryDispatchById (ConcurrentDictionary, atomic TryRemove)
- **Immutable Snapshots**: ctx.Followers (string[]), symmetryDispatchById.ToArray()

## Lock-Free Validation

### No lock() Statements
- **Verification**: Zero lock(stateLock) blocks in original or extracted methods
- **Pattern**: Uses immutable snapshots + ConcurrentDictionary atomic operations

### FSM/Actor Enqueue Pattern
- **Context**: This method is called from FSM/Actor context (ADR-019 reference in code)
- **Validation**: Method operates on immutable snapshots (ctx.Followers, ToArray())
- **Atomic Operations**: TryRemove() is lock-free atomic operation

### Atomic Primitives Only
- **ConcurrentDictionary.ContainsKey**: Thread-safe read without lock
- **ConcurrentDictionary.TryRemove**: Atomic compare-and-swap operation
- **Immutable Arrays**: string[] snapshots prevent race conditions

### Lock-Free Guarantees
1. Snapshot Isolation: ToArray() creates immutable copy for iteration
2. No Shared Mutable State: Each helper operates on local parameters
3. Atomic Removal: TryRemove() handles concurrent modifications safely
4. Early Exit: HasActiveFollowers returns immediately on first match (no lock needed)

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
- **Target**: CYC ≤8 (Jane Street strict standard)
- **Achieved**: Main method CYC=3, helpers CYC≤5
- **Rationale**: Functions with CYC >8 are harder to reason about under microsecond-latency constraints

### Correctness by Construction
- **Null Safety**: Guard clause in ShouldRemoveDispatch prevents null dereference
- **Immutable Snapshots**: ctx.Followers and ToArray() prevent race conditions
- **Pure Functions**: IsDispatchExpired and HasActiveFollowers have no side effects
- **Atomic Operations**: TryRemove() ensures thread-safe mutation

### Microsecond-Latency Reasoning
- **Early Exit**: HasActiveFollowers returns on first match (O(1) best case)
- **Single Pass**: Main method iterates dictionary once
- **No Allocations**: Helpers use stack-allocated parameters
- **Cache-Friendly**: Sequential array iteration in HasActiveFollowers

## Implementation Plan

### Step 1: Extract IsDispatchExpired
1. Create private method IsDispatchExpired
2. Replace inline TTL check with method call
3. Run tests: dotnet test
4. Verify complexity: python3 scripts/complexity_audit.py

### Step 2: Extract HasActiveFollowers
1. Create private method HasActiveFollowers
2. Replace nested loop logic with method call
3. Run tests: dotnet test
4. Verify complexity: python3 scripts/complexity_audit.py

### Step 3: Extract ShouldRemoveDispatch
1. Create private method ShouldRemoveDispatch
2. Orchestrate IsDispatchExpired and HasActiveFollowers
3. Simplify main method to single if-statement
4. Run tests: dotnet test
5. Verify complexity: python3 scripts/complexity_audit.py

### Step 4: Final Validation
1. Run full test suite: dotnet test
2. Run complexity audit: python3 scripts/complexity_audit.py
3. Verify CYC≤8 for all methods
4. Run build: powershell -File .\scripts\build_readiness.ps1
5. Sync hard links: powershell -File .\deploy-sync.ps1

## Success Criteria

### Functional Requirements
- Zero behavioral changes (output identical to original)
- All existing tests pass
- No new compilation errors

### Complexity Requirements
- Main method CYC ≤8 (target: CYC=3)
- All helper methods CYC ≤8
- Total complexity reduced by ≥20%

### V12 DNA Requirements
- No lock() statements introduced
- Maintains FSM/Actor Enqueue pattern
- Uses atomic primitives only
- ASCII-only compliance (no Unicode)

### Jane Street Alignment
- Cognitive simplicity (CYC ≤8)
- Correctness by construction (immutable snapshots)
- Microsecond-latency reasoning (early exit, single pass)
- Testable (pure functions, clear contracts)

## Approval Decision

### Status: READY FOR PHASE 3 (DNA & PR Audit)

### Rationale
1. Clear Extraction Boundaries: 3 helper methods with single responsibilities
2. Complexity Reduction: 10 to 3 (70% reduction, exceeds target)
3. Lock-Free Compliance: No locks, uses immutable snapshots + atomic operations
4. Jane Street Aligned: CYC≤8, cognitive simplicity, testable design
5. Low Risk: Incremental extraction with test verification after each step

### Next Phase
Proceed to Phase 3: DNA & PR Audit (Adjudicator review)

---
**Generated**: 2026-06-15 (Phase 2: Architecture Planning)
**Status**: APPROVED - Ready for Phase 3 (DNA & PR Audit)
**Complexity Target**: CYC ≤8 (Jane Street strict standard)
**V12 DNA**: COMPLIANT (Lock-free, Atomic, ASCII-only)
