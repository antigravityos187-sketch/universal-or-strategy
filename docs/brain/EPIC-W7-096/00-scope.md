# Phase 1: Scope Definition - EPIC-W7-096

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:36:58Z

## Epic Overview
- **Epic ID**: EPIC-W7-096
- **Target Method**: ExecuteMultiAccountBracket
- **File**: src/V12_002.SIMA.Execution.cs
- **Current CYC**: 16 to **Target CYC**: 8 or less
- **Current Nesting**: 8 to **Target Nesting**: 3 or less
- **Current LOC**: 147 to **Target LOC**: less than 50

## Scope Boundary Definition

### IN SCOPE

#### Primary Extraction Target
- **Method**: ExecuteMultiAccountBracket (lines 163-310)
- **Complexity**: CYC 16, Nesting 8, LOC 147
- **Blast Radius**: 0 dependents (ISOLATED - safe to refactor)

#### Extraction Candidates (4 Helper Methods)

1. **ValidateFleetAccounts**
   - **Purpose**: Extract fleet account validation logic
   - **Lines**: approximately 15-20 lines
   - **Logic**: IsFleetAccount checks, account filtering
   - **Target CYC**: 3 or less
   - **Rationale**: Reduces nesting depth, isolates validation concern

2. **CreateBracketOrders**
   - **Purpose**: Extract bracket order creation logic
   - **Lines**: approximately 40-50 lines
   - **Logic**: Order object construction, parameter assignment
   - **Target CYC**: 5 or less
   - **Rationale**: Separates order creation from execution logic

3. **TrackPositionDeltas**
   - **Purpose**: Extract position delta tracking logic
   - **Lines**: approximately 20-30 lines
   - **Logic**: AddExpectedPositionDeltaLocked calls, ExpKey generation
   - **Target CYC**: 4 or less
   - **Rationale**: Isolates state mutation, improves testability

4. **HandleBracketErrors**
   - **Purpose**: Extract error handling and logging logic
   - **Lines**: approximately 15-20 lines
   - **Logic**: LogBuffer.Format calls, error state management
   - **Target CYC**: 3 or less
   - **Rationale**: Separates error handling from happy path

#### Preserved Dependencies (14 Callees)
- IsFleetAccount (method)
- LogBuffer (class)
- AddExpectedPositionDeltaLocked (method)
- ExpKey (method)
- expectedPositions (constant)
- LogBuffer.Format (method)
- StampAccountFillGrace (method)
- 7 additional callees to be preserved

### OUT OF SCOPE

#### Excluded from This Epic
1. **Caller Modifications**: No callers detected (blast radius = 0)
2. **Signature Changes**: Preserve method signature (5 parameters)
3. **External Dependencies**: No changes to called methods (14 callees)
4. **Other Methods**: No changes to other methods in V12_002.SIMA.Execution.cs
5. **Test File Changes**: No test modifications (tests will be added in separate epic)
6. **Performance Optimization**: Focus on complexity reduction, not performance
7. **Logging Framework Changes**: Use existing LogBuffer pattern
8. **State Management Changes**: Preserve existing expectedPositions dictionary usage

#### Boundary Constraints
- **No Cross-File Changes**: All extractions within V12_002.SIMA.Execution.cs
- **No API Changes**: Method signature remains unchanged
- **No Behavioral Changes**: Preserve exact execution semantics
- **No Dependency Injection**: Use existing class-level dependencies

## Extraction Strategy

### Approach: Bottom-Up Extraction
1. Extract leaf logic first (validation, error handling)
2. Extract mid-level logic (position tracking)
3. Extract high-level logic (order creation)
4. Refactor main method to orchestrate extracted helpers

### Complexity Reduction Path
- **Step 1**: Extract ValidateFleetAccounts - CYC 16 to 13
- **Step 2**: Extract HandleBracketErrors - CYC 13 to 10
- **Step 3**: Extract TrackPositionDeltas - CYC 10 to 7
- **Step 4**: Extract CreateBracketOrders - CYC 7 to 4
- **Final**: ExecuteMultiAccountBracket orchestrates 4 helpers (CYC 4 or less)

### Nesting Reduction Path
- **Current**: 8 levels of nesting
- **After Extraction**: 3 or less levels per method
- **Technique**: Early returns, guard clauses, helper delegation

## Risk Mitigation

### Low-Risk Factors
- Zero Blast Radius: No external callers to break
- Low Churn: Not in top 50 hotspots (stable code)
- Isolated Method: Changes contained within single file
- No API Changes: Signature preservation maintains compatibility

### Medium-Risk Factors
- High Complexity: CYC 16 requires careful extraction
- Deep Nesting: 8 levels requires multiple extraction passes
- Large Method: 147 lines requires significant restructuring

### Mitigation Strategy
1. **Atomic Tickets**: Each extraction is a separate, testable ticket
2. **Build Verification**: Compile after each extraction
3. **Semantic Preservation**: No behavioral changes, only structural
4. **Rollback Plan**: Git commits per ticket for easy revert

## Success Criteria

### Phase 1 (Scope Definition)
- Scope boundaries defined (IN SCOPE vs OUT OF SCOPE)
- 4 extraction candidates identified
- Complexity reduction path documented
- Risk assessment completed

### Phase 2 (Architecture Planning)
- Extraction sequence finalized
- Helper method signatures designed
- Data flow diagrams created
- Mermaid diagrams generated

### Phase 3 (DNA Audit)
- V12 DNA compliance verified
- Lock-free pattern compliance checked
- ASCII-only compliance verified
- Jane Street alignment confirmed

### Phase 4 (Ticket Generation)
- 4 atomic tickets created (one per extraction)
- Ticket dependencies mapped
- Execution order defined
- Verification criteria per ticket

### Phase 5 (Execution)
- All 4 tickets executed successfully
- Build passes after each ticket
- CYC 8 or less achieved for all methods
- Nesting 3 or less achieved for all methods

### Phase 6 (Final Review)
- All success criteria met
- No compilation errors
- No behavioral regressions
- Documentation updated

## Verification Checklist

### Pre-Extraction
- Method signature documented
- All 14 callees identified
- Current behavior captured
- Baseline complexity metrics recorded

### Post-Extraction
- CYC reduced from 16 to 8 or less (per method)
- Nesting reduced from 8 to 3 or less (per method)
- LOC reduced from 147 to less than 50 (main method)
- All 14 callees preserved
- Build passes (zero compilation errors)
- Semantic equivalence verified

## Next Steps

1. **Phase 1.5**: Scope boundary validation (MANDATORY gate)
2. **Phase 2**: Architecture planning (extraction sequence design)
3. **Phase 3**: DNA audit (V12 compliance verification)
4. **Phase 4**: Ticket generation (4 atomic tickets)

## Metadata

- **Epic ID**: EPIC-W7-096
- **Phase**: 1 (Scope Definition)
- **Status**: COMPLETED
- **Timestamp**: 2026-06-24T19:36:58Z
- **Input**: docs/brain/EPIC-W7-096/00-hotspots.md
- **Output**: docs/brain/EPIC-W7-096/00-scope.md
- **Agent**: v12-phase1-scope
- **Mode**: v12-phase1-scope
