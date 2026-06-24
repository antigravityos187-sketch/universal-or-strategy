# Phase 1: Scope Boundary - EPIC-W7-159

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:50:39Z
- **Input**: 00-hotspots.md
- **Output**: 01-scope-boundary.md

## Epic Summary
**Target**: TryHandleFleet_LongShort method in src/V12_002.UI.IPC.Commands.Fleet.cs
**Current CYC**: 21
**Target CYC**: ≤8 per extracted method
**Blast Radius**: ZERO (no direct dependents)

## IN SCOPE

### Primary Extraction Targets
1. **ATR Calculation Logic**
   - Extract calls to CalculateATRStopDistance
   - Consolidate ATR-based risk calculations
   - Target CYC: ≤5

2. **Position Sizing Logic**
   - Extract calls to CalculatePositionSize
   - Consolidate position size calculations
   - Target CYC: ≤5

3. **Order Execution Logic**
   - Extract ExecuteMultiAccountBracket calls
   - Extract ExecuteMultiAccountMarket calls
   - Consolidate multi-account order execution
   - Target CYC: ≤6

4. **Duplicate Check Logic**
   - Extract MetadataGuardDuplicate validation
   - Simplify early-exit conditions
   - Target CYC: ≤3

### Refactoring Constraints
- **PRESERVE**: Actor pattern (Enqueue calls) - V12 DNA compliant
- **PRESERVE**: Thread safety checks (IsActorThread)
- **PRESERVE**: Logging patterns (LogBuffer.Format)
- **PRESERVE**: State management (AddExpectedPositionDeltaLocked)
- **MAINTAIN**: Single entry point (TryHandleFleetCommand)

### Success Criteria
- Main method CYC reduced from 21 to ≤8
- All extracted methods have CYC ≤8
- Zero compilation errors
- Zero test failures
- Actor pattern integrity maintained
- Hard link sync successful (deploy-sync.ps1)

## OUT OF SCOPE

### Explicitly Excluded
1. **Caller Method**: TryHandleFleetCommand (line 37)
   - Reason: Not the complexity hotspot
   - Action: Leave unchanged

2. **Callee Methods** (40 methods called):
   - CalculateATRStopDistance - use as-is
   - CalculatePositionSize - use as-is
   - ExecuteMultiAccountBracket - use as-is
   - ExecuteMultiAccountMarket - use as-is
   - MetadataGuardDuplicate - use as-is
   - Enqueue - use as-is (Actor pattern)
   - All other callees - use as-is

3. **Adjacent Methods** in V12_002.UI.IPC.Commands.Fleet.cs:
   - TryHandleFleet_Long
   - TryHandleFleet_Short
   - TryHandleFleet_Close
   - Reason: Separate epics if needed

4. **Test Files**:
   - No test modifications (tests do not exist yet)
   - Reason: Test creation is separate epic

5. **Documentation**:
   - No XML doc comment updates
   - Reason: Focus on complexity reduction only

6. **Performance Optimization**:
   - No algorithmic changes
   - No caching additions
   - Reason: Correctness-preserving refactor only

### Boundary Violations to Reject
- Modifying Actor pattern implementation
- Changing method signatures of callees
- Refactoring other Fleet command handlers
- Adding new features or logic
- Optimizing performance
- Changing logging behavior

## Extraction Strategy

### Phase 2 Architecture Plan Will Define:
1. Exact extraction boundaries (line ranges)
2. New method signatures
3. Parameter passing strategy
4. Return value handling
5. Error propagation approach

### Phase 4 Ticket Breakdown:
- **Ticket 1**: Extract duplicate check logic (CYC ≤3)
- **Ticket 2**: Extract ATR calculation logic (CYC ≤5)
- **Ticket 3**: Extract position sizing logic (CYC ≤5)
- **Ticket 4**: Extract order execution logic (CYC ≤6)
- **Ticket 5**: Verify main method CYC ≤8

## Risk Mitigation

### Zero Blast Radius Advantage
- No downstream dependencies to break
- Single caller makes testing straightforward
- Isolated within Fleet command handling

### Actor Pattern Preservation
- All Enqueue calls must remain unchanged
- Thread safety checks must be preserved
- Queue processing logic must be maintained

### Rollback Plan
- Git branch: epic-w7-159-scope
- Checkpoint: Before each ticket execution
- Rollback: Revert to last passing build

## Phase 1 Conclusion
**SCOPE APPROVED**: Proceed to Phase 2 (Architecture Planning)

**Scope Summary**:
- IN SCOPE: 4 extraction targets (duplicate check, ATR calc, position sizing, order execution)
- OUT OF SCOPE: Caller method, 40 callee methods, adjacent Fleet handlers, tests, docs
- TARGET: Reduce CYC from 21 to ≤8
- RISK: LOW (zero blast radius, single caller)
