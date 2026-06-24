# Phase 1: Scope Boundary - EPIC-W7-116

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: 0.18
- API Key: N/A
- Execution Time: 2026-06-24T01:35:38Z

## Epic Objective
Reduce cyclomatic complexity of `ShadowProcessFollowerStopUpdate` from 13 to ≤8 through surgical extraction.

## IN SCOPE

### Primary Target
- **Method**: `ShadowProcessFollowerStopUpdate`
- **File**: `src/V12_002.SIMA.Shadow.cs`
- **Line**: 246
- **Current CYC**: 13
- **Target CYC**: ≤8
- **Lines of Code**: 46

### Allowed Modifications
1. Extract decision logic into 2-3 helper methods
2. Refactor conditional branches to reduce nesting
3. Add private helper methods within same file
4. Update method signature if needed for clarity
5. Add XML documentation to extracted methods

### Testing Requirements
1. Verify 2 existing callers still function:
   - `ShadowMoveFollowerStops` (line 297)
   - `PropagateAndCacheStopPrice` (line 138)
2. Ensure all 28 callees remain functional
3. Build verification via `dotnet build`
4. NinjaTrader F5 integration test

## OUT OF SCOPE

### Explicitly Excluded
1. **Caller Modifications**: Do NOT modify `ShadowMoveFollowerStops` or `PropagateAndCacheStopPrice`
2. **Callee Modifications**: Do NOT modify any of the 28 called methods
3. **Cross-File Changes**: Do NOT modify any files other than `src/V12_002.SIMA.Shadow.cs`
4. **Signature Changes**: Do NOT change public/internal method signatures
5. **Behavioral Changes**: Do NOT alter business logic or execution flow
6. **Performance Optimization**: Focus on complexity reduction only
7. **Other Shadow Methods**: Do NOT refactor other methods in Shadow subsystem

### Boundary Enforcement
- **File Boundary**: Changes limited to `src/V12_002.SIMA.Shadow.cs` only
- **Method Boundary**: Only `ShadowProcessFollowerStopUpdate` and new extracted helpers
- **Subsystem Boundary**: Shadow subsystem internal changes only
- **API Boundary**: No changes to method signatures visible to callers

## Risk Mitigation

### Zero Blast Radius Advantage
- No external importers = low regression risk
- Only 2 internal callers = easy verification
- Private method = no public API impact

### Complexity Reduction Strategy
1. Extract conditional logic into named helper methods
2. Reduce nesting depth from 3 to ≤2
3. Target 2-3 extractions to achieve CYC ≤8
4. Maintain single responsibility per extracted method

## Success Criteria

### Quantitative
- [ ] `ShadowProcessFollowerStopUpdate` CYC reduced to ≤8
- [ ] All extracted methods have CYC ≤8
- [ ] Zero compilation errors
- [ ] Zero test failures

### Qualitative
- [ ] Code remains readable and maintainable
- [ ] Business logic preserved exactly
- [ ] No behavioral changes introduced
- [ ] Jane Street principles upheld

## Scope Validation
- **Scope Creep Risk**: LOW (zero blast radius, private method)
- **Boundary Violations**: None anticipated
- **Director Approval**: Not required (standard extraction)

## Next Phase
Proceed to Phase 2 (Architecture Planning) to design extraction strategy.
