# Phase 1: Scope Boundary - EPIC-W7-028

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:30:37Z
- **Input**: 00-hotspots.md
- **Output**: 01-scope-boundary.md

## Target Method
- **Method**: ProcessFlattenWorkItem_CancelOrders
- **File**: src/V12_002.SIMA.Flatten.cs
- **Line**: 191
- **Current CYC**: 17
- **Target CYC**: ≤8 per extracted method

## Scope Definition

### IN SCOPE

#### Primary Extraction Target
1. **ProcessFlattenWorkItem_CancelOrders** (CYC 17)
   - Extract nested conditional logic
   - Break into 2-3 helper methods with CYC ≤8 each
   - Preserve public method signature (5 callers depend on it)

#### Extraction Candidates
Based on complexity analysis, extract:
1. **Order validation logic** (nested if/else chains)
2. **Cancel order execution logic** (order submission flow)
3. **Error handling and logging** (exception paths)

#### Dependencies to Preserve
1. **LogBuffer calls** (6 callees)
   - LogBuffer.Format
   - LogBuffer.ValidateThreadAffinity
   - LogBuffer.FormatInternal
2. **Method signature** (2 parameters)
3. **Return type and behavior**

#### Callers to Verify (5 methods)
1. PumpFlattenOps (line 124)
2. PerformFallbackFlatten (line 328)
3. FlattenAllApexAccounts (line 38)
4. ChainNextFlattenOp (line 376)
5. ClosePositionsOnlyApexAccounts (line 516)

### OUT OF SCOPE

#### Excluded from This Epic
1. **Caller methods** - Do NOT modify the 5 calling methods
2. **LogBuffer infrastructure** - Do NOT modify logging methods
3. **Other methods in V12_002.SIMA.Flatten.cs** - Only target ProcessFlattenWorkItem_CancelOrders
4. **Public API changes** - Do NOT change method signature
5. **Behavioral changes** - Preserve exact logic flow

#### Deferred to Future Epics
1. **Caller refactoring** - If callers also have high CYC, create separate epics
2. **Logging optimization** - LogBuffer improvements are separate concern
3. **Test coverage expansion** - Beyond unit tests for extracted methods

### Boundary Validation

#### What Changes
- Internal implementation of ProcessFlattenWorkItem_CancelOrders
- Addition of 2-3 private helper methods
- Code organization within the method

#### What Stays the Same
- Method signature (name, parameters, return type)
- Public behavior and side effects
- Caller contracts
- LogBuffer call patterns
- Error handling outcomes

## Risk Mitigation

### Zero Blast Radius Confirmation
- **Direct Dependents**: 0 external files
- **Importer Count**: 0 external modules
- **Overall Risk Score**: 0.0 (LOW)
- **Interpretation**: All callers are file-local, making this a SAFE refactoring target

### Caller Impact Analysis
All 5 callers are in the same file (src/V12_002.SIMA.Flatten.cs):
- No cross-file coordination needed
- No API versioning concerns
- No external contract breakage risk

### Test Strategy
1. **Before Refactoring**: Add unit tests for current behavior
2. **During Refactoring**: Verify tests still pass
3. **After Refactoring**: Add tests for extracted helper methods

## Success Criteria

### Complexity Reduction
- **Before**: CYC = 17
- **After**: All methods CYC ≤8
- **Target**: 2-3 extracted methods, each CYC ≤8

### Behavioral Preservation
- All 5 callers continue to work unchanged
- LogBuffer calls remain identical
- Error handling paths preserved
- No regression in functionality

### Code Quality
- CSharpier formatting compliance
- ASCII-only compliance
- No lock() statements (V12 DNA mandate)
- Jane Street complexity threshold met

## Next Steps (Phase 2)

1. **Architecture Planning**: Design extraction strategy
2. **Method Decomposition**: Identify logical boundaries for extraction
3. **Test Design**: Create test cases for extracted methods
4. **Implementation Plan**: Define ticket breakdown

## Conclusion

**Scope Status**: WELL-DEFINED

This epic has a clear, bounded scope:
- Single method refactoring (ProcessFlattenWorkItem_CancelOrders)
- Zero external dependencies
- File-local impact only
- Measurable success criteria (CYC ≤8)

**Recommendation**: Proceed to Phase 2 (Architecture Planning) with HIGH confidence.
