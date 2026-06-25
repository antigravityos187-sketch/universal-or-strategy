# Phase 1: Scope Definition - EPIC-W7-002

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T18:58:09Z
- **Input Artifact**: 00-hotspots.md

## Epic Overview
- **Epic ID**: EPIC-W7-002
- **Target Method**: SymmetryGuardTryResolveFollowersForDispatch
- **File**: src/V12_002.Symmetry.Replace.cs
- **Line**: 134
- **Current CYC**: 16
- **Target CYC**: ≤8

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
- **Method**: `SymmetryGuardTryResolveFollowersForDispatch` (CYC 16)
  - **Location**: src/V12_002.Symmetry.Replace.cs:134
  - **Reason**: Exceeds Jane Street threshold (CYC >8) by 8 points
  - **Blast Radius**: 0.0 (zero dependents - safe to refactor)

#### Extraction Targets
Based on hotspot analysis, the following nested logic blocks are IN SCOPE for extraction:

1. **Follower Resolution Logic** (estimated CYC 4-6)
   - Nested conditionals for follower state validation
   - Dictionary lookups and state checks
   - Target: Extract to `ResolveFollowerState()`

2. **Dispatch Validation Logic** (estimated CYC 3-5)
   - Symmetry dispatch validation
   - Fleet entry validation
   - Target: Extract to `ValidateDispatchEligibility()`

3. **Pending Fill Processing** (estimated CYC 3-5)
   - Pending follower fill checks
   - Active position validation
   - Target: Extract to `ProcessPendingFollowerFills()`

#### Helper Method Interfaces (Preserve)
The following 5 helper methods are called by the target and MUST maintain their current signatures:
- `SymmetryGuardTryResolveFollower`
- `SymmetryGuardSkipFollower`
- `SymmetryGuardApplyMasterAnchor`
- `SymmetryGuardRetargetExistingFollowerBracket`
- `SymmetryGuardSubmitFollowerBracket`

#### State Dictionaries (Read-Only Access)
The following state dictionaries are accessed and MUST remain unchanged:
- `symmetryDispatchById`
- `symmetryFleetEntryToDispatch`
- `symmetryPendingFollowerFills`
- `activePositions`

### OUT OF SCOPE

#### Excluded Methods
The following methods are OUT OF SCOPE (not to be modified):
- `SymmetryGuardTryResolveFollower` (helper - already exists)
- `SymmetryGuardSkipFollower` (helper - already exists)
- `SymmetryGuardApplyMasterAnchor` (helper - already exists)
- `SymmetryGuardRetargetExistingFollowerBracket` (helper - already exists)
- `SymmetryGuardSubmitFollowerBracket` (helper - already exists)

#### Excluded Files
- All files except `src/V12_002.Symmetry.Replace.cs`
- Test files (tests will be added in Phase 5)
- State dictionary implementations

#### Excluded Concerns
- Performance optimization (maintain current performance)
- API changes (preserve all public/internal interfaces)
- State management refactoring (preserve existing state dictionaries)
- Caller modifications (zero callers detected, none to modify)

## Extraction Strategy

### Complexity Reduction Plan
- **Current CYC**: 16
- **Target CYC**: ≤8 per method
- **Expected Tickets**: 3 extraction tickets
- **Reduction Required**: 8 points (50% reduction)

### Ticket Breakdown (Estimated)
1. **Ticket 1**: Extract follower resolution logic (CYC 4-6)
2. **Ticket 2**: Extract dispatch validation logic (CYC 3-5)
3. **Ticket 3**: Extract pending fill processing (CYC 3-5)

### Success Criteria
- Main method CYC ≤8
- All extracted methods CYC ≤8
- Zero blast radius maintained (no new dependents)
- All helper method signatures preserved
- Build passes
- No new dependencies introduced

## Risk Assessment

### Favorable Factors
- **Zero Blast Radius**: No direct dependents (risk score 0.0)
- **Clear Helper Structure**: 5 existing helper methods to delegate to
- **No External Callers**: Safe to modify internal logic
- **Moderate Length**: 58 lines (manageable)

### Risk Factors
- **High Complexity**: 16 decision points
- **Deep Nesting**: 4 levels (cognitive load)
- **High Coupling**: 20 callees (must preserve interfaces)
- **Dynamic Dispatch**: No detected callers (may be called via reflection)

### Mitigation Strategy
1. Extract nested conditionals to helper methods
2. Use early returns to reduce nesting depth
3. Preserve all existing helper method interfaces
4. Maintain state dictionary access patterns
5. Add comprehensive unit tests in Phase 5

## Jane Street Alignment

### Cognitive Simplicity
- **Principle**: Functions with CYC ≤8 for microsecond-latency reasoning
- **Current State**: 16 decision points exceed threshold
- **Target State**: All methods ≤8 for tractable reasoning

### Correctness by Construction
- **Principle**: Make illegal states unrepresentable
- **Application**: Extract validation logic to prevent invalid state transitions
- **Benefit**: Compiler-enforced correctness

### Testing Strategy
- **Current**: 16 decision points = 2^16 = 65,536 potential paths (intractable)
- **Target**: Methods with CYC ≤8 = 2^8 = 256 paths per method (exhaustive testing feasible)
- **Benefit**: Race condition auditing becomes tractable

## Scope Validation

### Boundary Checks
- ✅ Single method targeted (no scope creep)
- ✅ Zero blast radius (safe to refactor)
- ✅ Clear extraction boundaries identified
- ✅ Helper method interfaces preserved
- ✅ State dictionaries unchanged
- ✅ No caller modifications required

### Complexity Validation
- ✅ Current CYC (16) exceeds threshold (8) by 8 points
- ✅ Reduction target (50%) is achievable
- ✅ Estimated ticket count (3) is reasonable
- ✅ Each extracted method targets CYC ≤8

### Risk Validation
- ✅ Zero dependents (blast radius 0.0)
- ✅ No breaking changes to public APIs
- ✅ Existing helper methods preserved
- ✅ State management unchanged

## Conclusion

**SCOPE APPROVED**: This epic targets a single method with clear extraction boundaries. The zero blast radius provides a safety net, while the high complexity (16) justifies the effort. All extracted methods will target CYC ≤8, aligning with Jane Street strict standards.

**Next Phase**: Proceed to Phase 1.5 (Scope Boundary Validation) to verify no scope creep and confirm extraction boundaries.
