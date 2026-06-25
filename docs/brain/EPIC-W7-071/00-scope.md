# Phase 1: Scope Definition - EPIC-W7-071

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.94
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:33:36Z

## Target Method
- **Method**: ShadowProcessFollowerStopUpdate
- **File**: src/V12_002.SIMA.Shadow.cs
- **Line**: 246
- **Current CYC**: 13
- **Target CYC**: <=8
- **Lines of Code**: 46

## Scope Boundaries

### IN SCOPE

**Primary Target**: Method body refactoring only
- Extract 3-5 helper methods from the 46-line method body
- Reduce cyclomatic complexity from 13 to <=8
- Maintain all existing functionality
- Keep all changes within src/V12_002.SIMA.Shadow.cs

**Extraction Candidates** (4 logical concerns):

1. **Validation Logic**
   - ValidateStopPrice calls
   - Input parameter validation
   - Precondition checks
   - Target: 1 extracted method

2. **State Management**
   - pendingStopReplacements dictionary operations
   - HandleStalePendingReplacement logic
   - UpdateExistingPendingReplacement logic
   - Target: 1-2 extracted methods

3. **Error Handling**
   - HandleUpdateException calls
   - Exception path logic
   - Error recovery flows
   - Target: 1 extracted method

4. **Order Operations**
   - UpdateStopOrder orchestration
   - InitiateStopReplacement logic
   - CreateDirectStopOrder calls
   - Target: 1 extracted method

**Success Criteria**:
- Final CYC <=8 for ShadowProcessFollowerStopUpdate
- All extracted methods have CYC <=8
- Zero external API changes (method signature unchanged)
- All 28 existing callees remain functional
- Build passes after extraction
- F5 in NinjaTrader successful

### OUT OF SCOPE

**Explicitly Excluded**:

1. **Caller Methods** (DO NOT MODIFY)
   - ShadowMoveFollowerStops (line 297)
   - PropagateAndCacheStopPrice (line 138)
   - Rationale: Zero blast radius means callers are unaffected

2. **Callee Methods** (DO NOT MODIFY)
   - All 28 existing callees are already extracted
   - ValidateStopPrice, UpdateStopOrder, InitiateStopReplacement, etc.
   - Rationale: These are stable, tested methods

3. **Cross-File Changes** (DO NOT MODIFY)
   - No changes to other .cs files
   - No changes to interfaces or contracts
   - Rationale: Zero external dependencies

4. **Signature Changes** (DO NOT MODIFY)
   - Method name: ShadowProcessFollowerStopUpdate
   - Parameters: (SIMA_FSM fsm, double newStopPrice, string reason)
   - Return type: void
   - Rationale: Maintain API compatibility

5. **Test Files** (DO NOT MODIFY)
   - No test changes required (zero external blast radius)
   - Rationale: Internal refactoring only

## Extraction Strategy

### Approach: Surgical Decomposition

**Phase 2 will define the exact extraction plan, but the strategy is**:

1. **Identify Decision Points**: Map all 13 cyclomatic complexity points
2. **Group by Concern**: Cluster related decision points into 4 logical groups
3. **Extract Helpers**: Create 3-5 private helper methods (one per concern)
4. **Simplify Main Method**: Reduce to orchestration logic only
5. **Verify CYC**: Ensure main method and all helpers are <=8

### Risk Mitigation

**Low Risk Factors**:
- Zero external blast radius (no external dependencies)
- Single file scope (isolated changes)
- Existing callees are stable (no need to modify)
- Clear logical concerns (4 distinct groups)

**Medium Risk Factors**:
- High internal coupling (28 callees)
- State management complexity (pendingStopReplacements)
- Error handling paths (multiple exception scenarios)

**Mitigation Strategy**:
- Extract one concern at a time
- Verify build after each extraction
- Test in NinjaTrader after each extraction
- Use Bob CLI v12-engineer mode for surgical precision

## Boundary Validation

### Scope Creep Prevention

**STOP if any of these occur**:
- Modifying caller methods (ShadowMoveFollowerStops, PropagateAndCacheStopPrice)
- Modifying callee methods (ValidateStopPrice, UpdateStopOrder, etc.)
- Changing method signature (name, parameters, return type)
- Cross-file changes (modifying other .cs files)
- Adding new public APIs (all extractions must be private)

**Scope Adherence Checklist**:
- All changes in src/V12_002.SIMA.Shadow.cs only
- Method signature unchanged
- All extracted methods are private
- No caller modifications
- No callee modifications
- CYC <=8 for all methods

## Verification Plan

### Build Verification
```
dotnet build
powershell -File .\deploy-sync.ps1
```

### NinjaTrader Verification
- F5 in NinjaTrader IDE
- Verify BUILD_TAG appears
- Check for compilation errors
- Verify strategy loads

### Complexity Verification
```
python scripts/complexity_audit.py --file src/V12_002.SIMA.Shadow.cs --threshold 8
```

## Conclusion

**EPIC-W7-071 Scope is APPROVED**

The scope is well-defined with clear boundaries:
- IN SCOPE: Method body extraction (3-5 helpers, CYC 13 to <=8)
- OUT OF SCOPE: Callers, callees, cross-file changes, signature changes
- Risk: MEDIUM-LOW (high complexity, low blast radius)
- Strategy: Surgical decomposition by logical concern

**Next Phase**: Proceed to Phase 1.5 (Scope Boundary Validation) to verify no scope creep risks.
