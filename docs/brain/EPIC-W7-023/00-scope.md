# Phase 1: Scope Definition - EPIC-W7-023

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.85
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:26:11Z

## Epic Overview
- **Target Method**: HandleFlatPositionUpdate
- **File**: src/V12_002.Orders.Callbacks.Execution.cs
- **Current CYC**: 19
- **Target CYC**: <=8
- **Reduction Needed**: 11 points

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
- **Method**: HandleFlatPositionUpdate (line 69, CYC 19)
- **Extraction Strategy**: Extract 4 helper methods from main logic

#### Helper Methods to Extract
1. **ShouldSkipFlatPositionCleanup(acctName)**
   - **Purpose**: Guard validation - check if cleanup is needed
   - **Target CYC**: <=3
   - **Logic**: Consolidate early return conditions
   - **Lines**: Guard clauses at method start

2. **SyncExpectedPositionsOnFlat(acctName)**
   - **Purpose**: State synchronization - sync expectedPositions
   - **Target CYC**: <=2
   - **Logic**: SetExpectedPositionLocked call with logging
   - **Lines**: ExpectedPosition sync block

3. **DetectOrphanedPositions(acctName)**
   - **Purpose**: Orphan detection - find positions needing cleanup
   - **Target CYC**: <=6
   - **Logic**: Loop through activePositions, build cleanup list
   - **Lines**: Position iteration and validation block
   - **Returns**: List<Position> positionsToCleanup

4. **ExecutePositionCleanup(positionsToCleanup)**
   - **Purpose**: Cleanup execution - execute cleanup and log results
   - **Target CYC**: <=3
   - **Logic**: Loop through cleanup list, call CleanupPosition
   - **Lines**: Cleanup execution loop

#### Main Method After Refactoring
- **HandleFlatPositionUpdate** (orchestrator)
- **Target CYC**: <=5
- **Logic**: Call 4 helper methods in sequence
- **Responsibilities**: High-level orchestration only

### OUT OF SCOPE

#### Excluded from This Epic
1. **Caller Method**: ProcessOnPositionUpdate
   - **Reason**: Separate epic, different complexity profile
   - **Note**: Single caller, no changes needed

2. **Downstream Callees** (49 symbols)
   - **Reason**: Already extracted/refactored, or below CYC threshold
   - **Examples**: ExpKey(), IsDispatchSyncPending(), HasPendingEntryOrderForAccount()
   - **Note**: These are called BY our target, not modified

3. **Supporting Infrastructure**
   - **Reason**: Out of scope for this complexity reduction
   - **Examples**: IsOrderTerminal(), StampAccountFillGrace(), ValidateOrphanedMasterOrders()

4. **Test Files**
   - **Reason**: Test creation is Phase 5 responsibility
   - **Note**: Tests will be added during ticket execution

5. **Other Methods in Same File**
   - **Reason**: Each method gets its own epic
   - **Note**: V12_002.Orders.Callbacks.Execution.cs has other methods

6. **Behavioral Changes**
   - **Reason**: This is a pure refactoring epic
   - **Note**: Zero behavioral changes allowed

### Scope Validation

#### Blast Radius Confirmation
- **External Importers**: 0 files
- **Direct Dependents**: 0 symbols
- **Overall Risk Score**: 0.0 (LOW)
- **Conclusion**: Minimal risk, ideal for refactoring

#### Complexity Reduction Math
- **Current**: HandleFlatPositionUpdate = CYC 19
- **After Extraction**:
  - ShouldSkipFlatPositionCleanup = CYC 3
  - SyncExpectedPositionsOnFlat = CYC 2
  - DetectOrphanedPositions = CYC 6
  - ExecutePositionCleanup = CYC 3
  - HandleFlatPositionUpdate (orchestrator) = CYC 5
- **Total Max CYC**: 6 (DetectOrphanedPositions)
- **All Methods**: <=8

#### Jane Street Alignment
- **Cognitive Simplicity**: Each method has single responsibility
- **Testability**: Smaller methods easier to test exhaustively
- **Race Condition Auditing**: Simpler logic easier to audit
- **Lock-Free Pattern**: No lock() usage (already compliant)

## Extraction Order

### Dependency Analysis
All 4 helper methods are **independent** - no dependencies between them.

### Recommended Execution Order
1. **Ticket 1**: Extract ShouldSkipFlatPositionCleanup (simplest, CYC 3)
2. **Ticket 2**: Extract SyncExpectedPositionsOnFlat (simple, CYC 2)
3. **Ticket 3**: Extract DetectOrphanedPositions (most complex, CYC 6)
4. **Ticket 4**: Extract ExecutePositionCleanup (simple, CYC 3)

**Rationale**: Start with simplest extractions to build confidence, tackle most complex (DetectOrphanedPositions) third, finish with simple cleanup.

## Success Criteria

### Phase 1 (This Document)
- Scope boundaries clearly defined (IN SCOPE vs OUT OF SCOPE)
- 4 helper methods identified with target CYC
- Extraction order determined
- Blast radius confirmed (zero external impact)

### Phase 2 (Architecture Planning)
- Extract Method refactoring pattern documented
- Guard Clause pattern for early returns
- Preserve all logging and error handling
- Maintain exact behavioral equivalence

### Phase 3 (DNA Audit)
- No lock() usage (already compliant)
- ASCII-only strings (already compliant)
- CYC <=8 per method (target of refactoring)
- Correctness by Construction principles

### Phase 4 (Ticket Generation)
- 4 atomic tickets (one per helper method)
- Each ticket includes: method signature, logic to extract, test requirements
- Tickets executable in parallel (no dependencies)

### Phase 5 (Execution)
- All 4 helper methods extracted
- Main method reduced to orchestrator (CYC <=5)
- All methods CYC <=8
- Zero behavioral changes
- All tests pass
- Build succeeds
- deploy-sync.ps1 completes

### Phase 6 (Final Review)
- Complexity audit confirms CYC <=8 for all methods
- No regressions in test suite
- Code review approval
- Merge to main

## Risk Mitigation

### Low Risk Factors
- Zero external blast radius
- Single internal caller
- Well-isolated in callback file
- Clear functional boundaries

### Medium Risk Factors
- High churn (24 commits in 90 days)
- 49 downstream callees (complex internal logic)

### Mitigation Strategy
- **Atomic Commits**: One helper method per commit
- **Test Coverage**: Unit tests for each extracted method
- **Behavioral Verification**: Integration tests confirm no changes
- **Rollback Plan**: Git revert if any test fails

## Next Phase
Proceed to **Phase 2: Architecture Planning** to document:
- Detailed extraction patterns
- Method signatures
- Parameter passing strategy
- Logging preservation approach
- Error handling preservation approach
