# Phase 1: Scope Definition - EPIC-W7-117

## Agent Tracking
- Agent Name: v12-phase1-scope
- Mode: plan
- Bobcoins Used: 0.18
- Execution Time: 2026-06-24T19:39:50Z

## Epic Overview
- **Target Method**: ValidateCachedEntry
- **File**: src/V12_002.SIMA.Shadow.cs
- **Line**: 158
- **Current CYC**: 9
- **Target CYC**: ≤8 (Jane Street strict standard)
- **Risk Level**: LOW (zero blast radius)

## Scope Boundaries

### What Will Be Extracted

#### Primary Extraction Target
**Method**: `ValidateCachedEntry` (lines 158-180)
- **Current Complexity**: CYC=9
- **Extraction Strategy**: Decompose validation chain into focused helper methods

#### Proposed Extractions

1. **ValidatePositionState** (NEW)
   - **Purpose**: Validate position existence and basic state
   - **Logic**: Check position exists, not null, not follower, entry filled, has contracts
   - **Parameters**: `PositionInfo pos`
   - **Returns**: `bool`
   - **Estimated CYC**: 3

2. **ValidateStopOrder** (NEW)
   - **Purpose**: Validate stop order existence and price validity
   - **Logic**: Check stop order exists, not null, stop price > 0
   - **Parameters**: `Order stopOrder`
   - **Returns**: `bool`
   - **Estimated CYC**: 2

3. **ValidateCachedEntry** (REFACTORED)
   - **Purpose**: Orchestrate validation checks
   - **Logic**: Call helper methods and return combined result
   - **Parameters**: Same as current (5 params)
   - **Returns**: `bool`
   - **Estimated CYC**: 3

### What Will Remain in Original Method

**After Refactoring**:
```csharp
private static bool ValidateCachedEntry(
    string entryKey,
    ConcurrentDictionary<string, PositionInfo> activePositions,
    ConcurrentDictionary<string, Order> stopOrders)
{
    PositionInfo livePos;
    Order liveStop;

    if (!activePositions.TryGetValue(entryKey, out livePos))
        return false;
    
    if (!ValidatePositionState(livePos))
        return false;
    
    if (!stopOrders.TryGetValue(entryKey, out liveStop))
        return false;
    
    if (!ValidateStopOrder(liveStop))
        return false;

    return true;
}
```

### What Stays Unchanged

1. **Method Signature**: No changes to parameters or return type
2. **Caller Sites**: No changes required
   - ShadowPropagateStopMoves (line 60)
3. **Dictionary Lookups**: TryGetValue calls remain in orchestrator
4. **Static Modifier**: All methods remain static (no instance state)

## Dependencies

### Internal Dependencies
- **Types Used**:
  - `PositionInfo` (custom type from V12_002)
  - `Order` (NinjaTrader.Cbi)
  - `ConcurrentDictionary<TKey, TValue>` (System.Collections.Concurrent)

- **Properties Accessed**:
  - `PositionInfo.IsFollower`
  - `PositionInfo.EntryFilled`
  - `PositionInfo.RemainingContracts`
  - `Order.StopPrice`

### External Dependencies
- **None**: Method is self-contained, no external service calls

### Caller Impact
- **Zero Impact**: Method signature unchanged
- **Callers**: 1 direct caller (ShadowPropagateStopMoves)
- **Call Pattern**: Used in cache cleanup loop

## Risk Assessment

### Refactoring Risks

#### LOW RISK
1. **Blast Radius**: Zero (isolated method, no external dependencies)
2. **Caller Count**: 1 (single call site)
3. **Stability**: Not in top 50 hotspots (low churn)
4. **Test Coverage**: Method is static, easily testable

#### MEDIUM RISK
1. **Logic Preservation**: Must maintain exact validation semantics
2. **Short-Circuit Behavior**: Must preserve early-exit pattern
3. **Null Handling**: Must maintain null-safety guarantees

#### MITIGATION STRATEGIES
1. **Preserve Semantics**: Extract methods maintain identical logic
2. **Early Exit**: Use guard clauses in helper methods
3. **Unit Tests**: Add tests for all validation paths
4. **Static Analysis**: Verify CYC reduction via complexity_audit.py

## Success Criteria

### Functional Requirements
- ✅ **Correctness**: All validation logic preserved exactly
- ✅ **Behavior**: Identical return values for all input combinations
- ✅ **Performance**: No performance degradation (static methods, inline candidates)

### Complexity Requirements
- ✅ **Target CYC**: ValidateCachedEntry ≤8 (currently 9)
- ✅ **Helper CYC**: All extracted methods ≤8
- ✅ **Total Reduction**: CYC 9 → 3 (orchestrator) + 3 (position) + 2 (stop) = 8 effective

### Quality Requirements
- ✅ **Build**: Zero compilation errors
- ✅ **Tests**: All existing tests pass
- ✅ **Lint**: Zero new Roslyn warnings
- ✅ **ASCII**: No Unicode characters introduced

### Documentation Requirements
- ✅ **XML Comments**: All extracted methods documented
- ✅ **Purpose**: Clear explanation of validation responsibility
- ✅ **Parameters**: Documented with validation rules
- ✅ **Returns**: Boolean semantics explained

## Extraction Strategy

### Phase 2 Architecture Plan
1. **Extract ValidatePositionState**
   - Single responsibility: position state validation
   - Guard clauses for null/invalid states
   - CYC ≤3

2. **Extract ValidateStopOrder**
   - Single responsibility: stop order validation
   - Guard clauses for null/invalid prices
   - CYC ≤2

3. **Refactor ValidateCachedEntry**
   - Orchestrate validation calls
   - Maintain early-exit semantics
   - CYC ≤3

### Code Organization
- **Location**: Same file (V12_002.SIMA.Shadow.cs)
- **Placement**: Helper methods immediately after ValidateCachedEntry
- **Visibility**: Private static (same as original)
- **Ordering**: Maintain logical flow (position → stop → orchestrator)

## Boundary Validation

### In-Scope
- ✅ ValidateCachedEntry method body (lines 158-180)
- ✅ Validation logic decomposition
- ✅ Helper method extraction
- ✅ XML documentation updates

### Out-of-Scope
- ❌ Caller modifications (ShadowPropagateStopMoves)
- ❌ PositionInfo type changes
- ❌ Order type changes
- ❌ Dictionary structure changes
- ❌ Other methods in Shadow.cs file

### Scope Creep Prevention
- **One Epic = One Concern**: Only ValidateCachedEntry refactoring
- **No Adjacent Fixes**: Do not modify other methods
- **No Type Changes**: Do not alter PositionInfo or Order
- **No Infrastructure**: Do not change dictionary patterns

## Next Phase Inputs

### For Phase 1.5 (Boundary Validation)
- This scope definition document
- Confirmation of extraction targets
- Validation of boundary constraints

### For Phase 2 (Architecture Planning)
- Approved scope boundaries
- Detailed extraction plan
- Method signatures for new helpers
- XML documentation templates

## Conclusion

**Scope Status**: DEFINED ✅

**Complexity Reduction Path**: CYC 9 → 8 (via decomposition into 3 focused methods)

**Risk Level**: LOW (isolated, static, single caller, zero blast radius)

**Ready for Phase 1.5**: YES (boundary validation gate)
