# Phase 2: Architecture Planning - EPIC-CCN-067

## Epic Metadata

- **Epic ID**: EPIC-CCN-067
- **Target Method**: SymmetryFindDispatchForMasterFill
- **File**: src/V12_002.Symmetry.cs
- **Current Complexity**: CYC=9, LOC=28
- **Target Complexity**: CYC≤8 (Target: CYC=2 for main method)
- **Phase**: 2 - Architecture Planning
- **Date**: 2026-06-15
- **Architect**: Bob Shell (Plan Mode)

## 1. Extraction Strategy

### Current Method Analysis

**Method**: SymmetryFindDispatchForMasterFill
- **Signature**: private SymmetryDispatchContext SymmetryFindDispatchForMasterFill(string tradeType, MarketPosition direction, DateTime fillTimeUtc)
- **Current Complexity**: CYC=9
- **Lines of Code**: 28
- **Purpose**: Pure query method that searches symmetryDispatchById dictionary for the oldest valid dispatch candidate matching trade type, direction, and within TTL window

**Complexity Breakdown**:
- 1 foreach loop (CYC+1)
- 4 filter conditions (CYC+4):
  - Null check + IsResolved check
  - Direction match
  - TradeType match (normalized)
  - TTL validation
- 1 selection condition (CYC+1): oldest candidate comparison
- **Total**: CYC=9

### Proposed Extraction

**Strategy**: Separate filtering logic from selection logic into two focused helper methods.

**Target Complexity Distribution**:
- **Main Method**: CYC=2 (foreach loop + selection call)
- **Helper 1 (IsValidDispatchCandidate)**: CYC=4 (4 filter conditions)
- **Helper 2 (SelectOldestCandidate)**: CYC=1 (single comparison)
- **Total**: CYC=7 (meets ≤8 requirement)

**Rationale**:
- Achieves target CYC=2 for main method (cognitive simplicity)
- Each helper has single, clear responsibility
- Maintains pure functional style (no side effects)
- Preserves defensive copy pattern (ToArray())
- No performance degradation (JIT inlining candidates)

## 2. Method Signatures

### Original Method (Unchanged)

private SymmetryDispatchContext SymmetryFindDispatchForMasterFill(
    string tradeType,
    MarketPosition direction,
    DateTime fillTimeUtc
)

**Parameters**:
- tradeType: Trade type identifier (will be normalized)
- direction: Market position (Long/Short)
- fillTimeUtc: Fill timestamp for TTL validation

**Return**: SymmetryDispatchContext (or null if no valid candidate found)

**Access Modifier**: private (unchanged)

### Helper Method 1: IsValidDispatchCandidate

private bool IsValidDispatchCandidate(
    SymmetryDispatchContext ctx,
    string normalizedTradeType,
    MarketPosition direction,
    DateTime fillTimeUtc
)

**Purpose**: Consolidates all 4 filter conditions into single predicate method.

**Parameters**:
- ctx: Candidate dispatch context to validate
- normalizedTradeType: Pre-normalized trade type (from SymmetryNormalizeTradeType)
- direction: Required market position
- fillTimeUtc: Fill timestamp for TTL validation

**Return**: bool - true if candidate passes all filters, false otherwise

**Access Modifier**: private (encapsulation preserved)

**Complexity**: CYC=4 (4 filter conditions)

### Helper Method 2: SelectOldestCandidate

private SymmetryDispatchContext SelectOldestCandidate(
    SymmetryDispatchContext current,
    SymmetryDispatchContext candidate
)

**Purpose**: Isolates selection logic for finding oldest candidate.

**Parameters**:
- current: Current best candidate (may be null)
- candidate: New candidate to compare

**Return**: SymmetryDispatchContext - the older of the two candidates

**Access Modifier**: private (encapsulation preserved)

**Complexity**: CYC=1 (single comparison)

## 3. Call Graph

### Method Invocation Hierarchy

SymmetryFindDispatchForMasterFill (main)
├── SymmetryNormalizeTradeType (existing helper)
│   └── Returns: normalized trade type string
├── IsValidDispatchCandidate (new helper)
│   └── Called: for each candidate in loop
│   └── Returns: bool (filter result)
└── SelectOldestCandidate (new helper)
    └── Called: when valid candidate found
    └── Returns: SymmetryDispatchContext (best candidate)

### Execution Flow

1. **Main Method** receives: tradeType, direction, fillTimeUtc
2. **Normalize** trade type via SymmetryNormalizeTradeType(tradeType) → normalizedTradeType
3. **Initialize** best = null
4. **Iterate** through symmetryDispatchById.ToArray() (defensive copy)
5. **For each candidate**:
   - Call IsValidDispatchCandidate(ctx, normalizedTradeType, direction, fillTimeUtc)
   - If valid (returns true):
     - Call SelectOldestCandidate(best, ctx) → update best
   - If invalid (returns false):
     - Continue to next candidate
6. **Return** best (or null if no valid candidates)

### Shared State

**None** - All methods are pure functions:
- Main method: reads from symmetryDispatchById (via defensive copy)
- IsValidDispatchCandidate: pure predicate (no state access)
- SelectOldestCandidate: pure comparison (no state access)

**Thread Safety**: Preserved via defensive copy pattern (ToArray())

## 4. Lock-Free Validation

### Original Method Analysis

✅ **PASS**: No lock() statements in original method
✅ **PASS**: Uses defensive copy pattern (ToArray())
✅ **PASS**: Pure query method (read-only, no state mutations)
✅ **PASS**: No side effects
✅ **PASS**: Thread-safe via defensive copy

### Refactored Method Validation

✅ **PASS**: No locks will be introduced
✅ **PASS**: Defensive copy pattern preserved in main method
✅ **PASS**: Helper methods are pure (no state access)
✅ **PASS**: No shared mutable state between methods
✅ **PASS**: Thread safety maintained

### V12 DNA Compliance

- **Lock-Free Actor Pattern**: ✅ No locks, defensive copy preserved
- **Atomic Primitives**: ✅ Not applicable (pure query method)
- **FSM/Actor Enqueue**: ✅ Not applicable (read-only operation)
- **Correctness by Construction**: ✅ Pure functions with clear contracts

## 5. Jane Street Compliance

### Cognitive Simplicity

✅ **PASS**: Complexity reduction from CYC=9 to CYC=2 (main method)
✅ **PASS**: Each helper has single, clear responsibility
✅ **PASS**: Filter logic consolidated (IsValidDispatchCandidate)
✅ **PASS**: Selection logic isolated (SelectOldestCandidate)
✅ **PASS**: Main method orchestrates workflow (high-level intent)

**Rationale**: Functions with CYC>8 are harder to reason about under microsecond latency constraints. Reducing main method to CYC=2 dramatically improves cognitive load.

### Microsecond Latency Constraints

✅ **PASS**: No performance degradation expected
✅ **PASS**: Method call overhead negligible (JIT inlining candidates)
✅ **PASS**: No allocations introduced
✅ **PASS**: Defensive copy pattern unchanged (same ToArray() call)
✅ **PASS**: Hot-path co-location preserved (all methods in same class)

**Performance Analysis**:
- Helper methods are small (≤10 lines each)
- JIT compiler will likely inline both helpers
- No additional allocations beyond existing ToArray()
- Same number of iterations (no algorithmic change)

### Testing Standards

✅ **PASS**: Existing tests will validate behavior preservation
✅ **PASS**: No new test cases required (pure refactoring)
✅ **PASS**: Complexity reduction improves testability
✅ **PASS**: Helper methods can be unit tested independently (if needed)

**Test Strategy**:
- Existing integration tests cover SymmetryFindDispatchForMasterFill
- Refactoring preserves exact behavior (no logic changes)
- Pre-push validation will catch regressions

### Jane Street Knowledge Base Validation

**Query Results**: No specific FSM extraction patterns found in KB.

**Applied Principles** (from V12 DNA):
- ✅ Cognitive simplicity over clever abstractions
- ✅ Single responsibility principle
- ✅ Pure functions (no side effects)
- ✅ Microsecond latency awareness
- ✅ Testability through simplicity

## 6. Implementation Plan

### Step 1: Extract IsValidDispatchCandidate

**Action**: Create private helper method with 4 filter conditions.

**Verification**:
- CYC=4 (4 if statements)
- Pure function (no side effects)
- Clear single responsibility (filtering)

### Step 2: Extract SelectOldestCandidate

**Action**: Create private helper method with comparison logic.

**Verification**:
- CYC=1 (single if statement)
- Pure function (no side effects)
- Clear single responsibility (selection)

### Step 3: Refactor Main Method

**Action**: Replace inline logic with helper method calls.

**Verification**:
- CYC=2 (foreach loop + if statement)
- Signature unchanged
- Behavior preserved
- Defensive copy pattern maintained

### Step 4: Verification

**Actions**:
1. Run dotnet build (zero errors)
2. Run dotnet test (100% pass)
3. Run dotnet csharpier format src/ (formatting)
4. Run python3 scripts/complexity_audit.py (verify CYC≤8)
5. Run powershell -File .\scripts\pre_push_validation.ps1 -Fast (quality gates)

**Success Criteria**:
- ✅ Build succeeds
- ✅ All tests pass
- ✅ Main method CYC=2
- ✅ Helper methods CYC≤4
- ✅ No new lint violations
- ✅ Pre-push validation passes

## 7. Risk Assessment

### Implementation Risk: MINIMAL

**Rationale**:
- Simple extraction pattern (well-understood refactoring)
- No algorithmic changes
- No new dependencies
- Blast radius: 28 lines in single method

**Mitigation**:
- Existing test suite provides coverage
- Pre-push validation catches issues
- Single commit, easy revert

### Regression Risk: LOW

**Rationale**:
- Pure refactoring (no behavior changes)
- Method signature unchanged
- Defensive copy pattern preserved
- No state mutations

**Validation**:
- Existing integration tests
- Pre-push validation (13 checks)
- Manual F5 test in NinjaTrader

### Performance Risk: ZERO

**Rationale**:
- No allocations introduced
- JIT inlining candidates (small methods)
- Same algorithmic complexity
- Hot-path co-location preserved

**Validation**:
- No performance tests required (pure refactoring)
- Microsecond latency constraints maintained

## 8. Approval Checklist

### V12.23 Protocol Compliance

- ✅ Single method scope (no scope creep)
- ✅ Boundary validation PASS (Phase 1.5)
- ✅ No changes to callers or callees
- ✅ No changes to class-level state
- ✅ Helper methods are private (encapsulation)

### V12 DNA Compliance

- ✅ Lock-free pattern preserved
- ✅ ASCII-only compliance (no Unicode)
- ✅ Correctness by construction (pure functions)
- ✅ Defensive copy pattern maintained

### Jane Street Alignment

- ✅ Cognitive simplicity (CYC 9→2)
- ✅ Microsecond latency constraints met
- ✅ Testing standards maintained
- ✅ Single responsibility principle

### Quality Gates

- ✅ Target complexity achieved (CYC≤8)
- ✅ No new dependencies
- ✅ No breaking changes
- ✅ Rollback plan (single commit revert)

## 9. Next Phase

**Phase 3**: DNA & PR Audit (Adjudicator)
- **Agent**: Arena AI (Red Team)
- **Goal**: Verify plan against V12 constraints
- **Gate**: PASS/FAIL (fail triggers Phase 2 rework)

**Phase 4**: Recursive Execution (Engineer)
- **Agent**: Bob CLI (v12-engineer) for implementation
- **Safety**: Mandatory checkpointing enabled
- **Verification**: Compare against this architecture plan

## Architecture Plan Signature

- **Protocol**: V12 Phase 2 Architecture Planning
- **Epic**: EPIC-CCN-067
- **Method**: SymmetryFindDispatchForMasterFill
- **Complexity**: 9 → 2 (main) + 4 (filter) + 1 (select) = 7 total
- **Status**: READY FOR PHASE 3 AUDIT
- **Date**: 2026-06-15
- **Architect**: Bob Shell (Plan Mode)
