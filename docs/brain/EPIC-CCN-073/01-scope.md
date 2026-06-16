# Phase 1.0: Scope Definition - EPIC-CCN-073

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**: DeserializeSnapshot
- **File**: src/V12_002.StickyState.cs
- **Current Complexity**: 9 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

## Method Signature
private void DeserializeSnapshot(string json)

## Complexity Breakdown
Current CYC=9 indicates multiple decision points:
- Likely includes: null checks, JSON parsing, error handling, state validation
- Target: Extract validation logic and error handling into separate methods

## Proposed Extraction Strategy

### Option 1: Validation + Parsing Split
1. Extract validation logic to ValidateSnapshotJson(string json) (CYC ≤3)
2. Extract parsing logic to ParseSnapshotData(string json) (CYC ≤3)
3. Keep orchestration in DeserializeSnapshot (CYC ≤3)

### Option 2: Error Handling Extraction
1. Extract error handling to HandleDeserializationError(Exception ex) (CYC ≤2)
2. Extract state application to ApplyDeserializedState(object data) (CYC ≤3)
3. Keep core logic in DeserializeSnapshot (CYC ≤4)

## Boundary Definition

### IN SCOPE
- DeserializeSnapshot method body ONLY
- Internal logic refactoring
- Helper method extraction within same class
- Maintaining exact same public behavior

### OUT OF SCOPE
- Callers of DeserializeSnapshot (no signature changes)
- Callees (JSON libraries, state setters)
- Other methods in V12_002.StickyState.cs
- Serialization counterpart methods
- Test modifications (unless new helpers need coverage)

### NO SCOPE CREEP
- No "while we're here" improvements to adjacent code
- No fixing pre-existing compilation errors in other methods
- No bundling multiple concerns (ONE EPIC = ONE CONCERN)
- No refactoring of caller code paths

## Success Criteria

### Functional Requirements
- Complexity reduced from CYC=9 to CYC≤8
- All existing tests pass (zero regressions)
- No behavior changes (bit-for-bit equivalent output)
- Lock-free Actor/FSM pattern maintained (no new locks)

### Code Quality Requirements
- ASCII-only compliance maintained
- Helper methods follow V12 naming conventions
- No new compiler warnings introduced
- CSharpier formatting compliance

### Testing Requirements
- Existing unit tests pass without modification
- Integration tests pass (if applicable)
- Manual F5 test in NinjaTrader (smoke test)

## Risk Assessment

**Complexity Risk**: LOW
- Method is already below threshold (9 < 15)
- Refactoring is optimization, not emergency fix
- Small, focused change scope

**Behavioral Risk**: LOW
- Pure refactoring (extract method pattern)
- No algorithm changes
- Testable via existing test suite

**Integration Risk**: LOW
- Private method (no external callers)
- No signature changes
- No state machine modifications

## Verification Plan

1. **Pre-Surgery**: Run dotnet build + dotnet test (baseline)
2. **Post-Extraction**: Run dotnet build + dotnet test (verify zero delta)
3. **Complexity Check**: Run python scripts/complexity_audit.py (verify CYC≤8)
4. **Manual Test**: F5 in NinjaTrader, verify snapshot load/save works

## Dependencies

**Blocked By**: None (method is self-contained)
**Blocks**: None (optimization task, not critical path)
**Related**: EPIC-CCN-10 (overall complexity reduction initiative)

## Estimated Effort

- **Analysis**: 15 minutes (read method, identify extraction points)
- **Implementation**: 30 minutes (extract 2-3 helpers, verify tests)
- **Verification**: 15 minutes (build, test, complexity audit)
- **Total**: ~1 hour

## Notes

- Method is below Jane Street threshold (15) but can be optimized to strict standard (≤8)
- No Jane Street violations found in database for this file
- jCodemunch tools unavailable - manual code review required
- Blast radius analysis pending - assume low risk due to private method scope
