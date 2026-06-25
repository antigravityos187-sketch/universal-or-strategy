# Phase 1: Scope Definition - EPIC-W7-130

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: TBD
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:41:47Z

## Epic Overview
- **Target Method**: SymmetryGuardCascadeFollowerCleanup
- **File**: src/V12_002.Symmetry.Replace.cs
- **Current CYC**: 11
- **Target CYC**: ≤8
- **Reduction Required**: -3 points (27% reduction)

## Scope Boundary Analysis

### IN SCOPE

#### Primary Extraction Target
1. **SymmetryGuardCascadeFollowerCleanup** (CYC=11, lines 198-243)
   - **Rationale**: Exceeds Jane Street threshold by 3 points
   - **Blast Radius**: ZERO (no external dependencies)
   - **Callers**: 0 (isolated refactoring)
   - **Callees**: 18 (indicates multiple responsibilities)

#### Extraction Candidates (Based on 18 Callees)
The method has 18 callees suggesting multiple responsibilities. Likely extraction targets:

1. **Follower Order Cancellation Logic**
   - Pattern: Iterating through follower orders and calling CancelOrderSafe
   - Estimated CYC contribution: 3-4 points
   - Extract to: CancelFollowerOrders()

2. **Symmetry Dispatch Cleanup Logic**
   - Pattern: Removing entries from symmetryDispatchById
   - Estimated CYC contribution: 2-3 points
   - Extract to: CleanupSymmetryDispatch()

3. **Position Validation Logic**
   - Pattern: Checking activePositions and order states
   - Estimated CYC contribution: 2-3 points
   - Extract to: ValidateFollowerPositions()

#### Thread Safety Validation
- **ValidateThreadAffinity**: Already called (line context needed)
- **Lock-Free Pattern**: Verify FSM/Actor Enqueue model compliance
- **Atomic Operations**: Check for any lock(stateLock) violations

### OUT OF SCOPE

#### Excluded from This Epic
1. **Caller Analysis**: 0 callers means no upstream refactoring needed
2. **Blast Radius Mitigation**: Zero external dependencies = no impact files
3. **Related Symmetry Methods**: Focus only on SymmetryGuardCascadeFollowerCleanup
4. **Test File Modifications**: Tests will be added in Phase 5, not extracted here
5. **Other High-CYC Methods**: This epic targets ONLY EPIC-W7-130

#### Architectural Constraints
1. **No Lock Introduction**: Must maintain lock-free Actor pattern
2. **No Unicode**: ASCII-only compliance (already enforced)
3. **No Signature Changes**: Keep method signature identical for compatibility
4. **No External API Changes**: Internal refactoring only

## Extraction Strategy

### Approach: Vertical Slice Extraction
Given the 18 callees and max nesting depth of 6, use vertical slice extraction:

1. **Extract Follower Cancellation** (Target CYC reduction: -3)
   - Move nested loop for follower order cancellation
   - Single responsibility: Cancel all follower orders

2. **Extract Dispatch Cleanup** (Target CYC reduction: -2)
   - Move symmetryDispatchById removal logic
   - Single responsibility: Clean up dispatch tracking

3. **Extract Position Validation** (Target CYC reduction: -2)
   - Move activePositions validation logic
   - Single responsibility: Validate follower positions

### Expected Outcome
- **Original Method CYC**: 11 to 6 (after extraction)
- **New Method 1 CYC**: ≤3 (CancelFollowerOrders)
- **New Method 2 CYC**: ≤2 (CleanupSymmetryDispatch)
- **New Method 3 CYC**: ≤2 (ValidateFollowerPositions)
- **Total CYC Reduction**: -5 points (45% reduction)
- **Jane Street Compliance**: All methods ≤8

## Risk Mitigation

### Low Risk Factors
- Zero blast radius (no external dependencies)
- No callers (isolated refactoring)
- Not in top 50 hotspots (lower churn)
- Clear extraction boundaries (18 callees)

### Medium Risk Factors
- Deep nesting (max_nesting=6) requires careful extraction
- 18 callees suggest complex internal logic
- 0 callers may indicate dead code or reflection-based invocation

### Mitigation Strategy
1. **Phase 2**: Verify actual usage via code search and reflection analysis
2. **Phase 3**: Add comprehensive unit tests before extraction
3. **Phase 5**: Surgical extraction with immediate build verification
4. **Phase 5.V**: Runtime verification in NinjaTrader IDE

## Success Criteria

### Phase 1 Completion
- Scope boundary defined (IN SCOPE vs OUT OF SCOPE)
- Extraction candidates identified (3 methods)
- Risk assessment completed (LOW-MEDIUM)
- Strategy documented (Vertical Slice Extraction)

### Epic Completion (Phase 6)
- SymmetryGuardCascadeFollowerCleanup CYC ≤8
- All extracted methods CYC ≤8
- Zero compilation errors
- F5 in NinjaTrader successful
- BUILD_TAG verification passed

## Next Phase
**Phase 1.5**: Scope Boundary Validation (Jane Street gate)
- Verify extraction boundaries are correct
- Confirm no scope creep
- Validate risk assessment
