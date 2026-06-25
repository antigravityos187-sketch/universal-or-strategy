# Phase 1: Scope Definition - EPIC-W7-133

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Mode**: plan
- **Phase**: 1 (Scope Definition)
- **Input**: 00-hotspots.md
- **Execution Time**: 2026-06-24T19:42:23Z

## Epic Overview
- **Target Method**: MoveStop_SinglePosition
- **File**: src/V12_002.Trailing.Breakeven.cs
- **Current CYC**: 21 (HIGH - 2.6x over threshold)
- **Target CYC**: ≤8 per method (Jane Street strict standard)
- **Blast Radius**: ZERO (safe to refactor)
- **Single Caller**: MoveStopsToBreakevenWithOffset

## Extraction Strategy

### Primary Goal
Decompose MoveStop_SinglePosition (CYC 21) into 4-5 focused methods, each with CYC ≤8, following vertical slice extraction to preserve call semantics.

### Extraction Targets

#### 1. ValidateStopMoveRequest (Estimated CYC: 4-5)
**What to Extract**:
- Entry validation (entryName, pos null checks)
- Position state validation (pos.Position, pos.MarketPosition)
- Price validation (lastKnownPrice sanity checks)
- Offset validation (offsetPoints range checks)

**Boundary**:
- **IN**: entryName, pos, offsetPoints, lastKnownPrice
- **OUT**: bool (isValid) + early return on failure
- **Stays in Original**: All logic after validation passes

**Rationale**: Validation logic typically has 4-6 decision points (null checks, range checks, state checks). Extracting this reduces parent CYC by ~5-7.

#### 2. CalculateNewStopPrice (Estimated CYC: 2-3)
**What to Extract**:
- Stop price calculation logic
- Long vs Short position handling
- Offset application (pos.AveragePrice ± offsetPoints)
- Price rounding/normalization

**Boundary**:
- **IN**: pos (PositionInfo), offsetPoints, lastKnownPrice
- **OUT**: double (newStopPrice)
- **Stays in Original**: Stop order update logic

**Rationale**: Price calculation is pure logic with minimal branching (long/short switch). Low CYC (2-3), high cohesion.

#### 3. PrepareStopOrderUpdate (Estimated CYC: 5-6)
**What to Extract**:
- Existing stop order lookup (stopOrders dictionary)
- Pending replacement check (pendingStopReplacements)
- Stop order state validation (ValidateStopPrice)
- Stale replacement handling (HandleStalePendingReplacement)
- Update vs Replace decision logic

**Boundary**:
- **IN**: entryName, pos, newStopPrice
- **OUT**: StopOrderUpdateContext (struct with: existingOrder, pendingReplacement, updateAction)
- **Stays in Original**: Actual order submission logic

**Rationale**: Pre-update checks have 5-7 decision points (dictionary lookups, state checks, stale handling). Extracting this reduces parent CYC by ~4-6.

#### 4. ExecuteStopOrderUpdate (Estimated CYC: 4-5)
**What to Extract**:
- UpdateStopOrder call
- UpdateExistingPendingReplacement call
- InitiateStopReplacement call
- CreateDirectStopOrder call
- Error handling (HandleUpdateException)

**Boundary**:
- **IN**: StopOrderUpdateContext, entryName, pos, newStopPrice
- **OUT**: bool (success)
- **Stays in Original**: Post-update state management

**Rationale**: Order submission logic has 4-6 decision points (update vs replace, error handling). Extracting this reduces parent CYC by ~3-4.

#### 5. MoveStop_SinglePosition (Orchestrator - Target CYC: 3-4)
**What Remains**:
- Call ValidateStopMoveRequest → early return if invalid
- Call CalculateNewStopPrice
- Call PrepareStopOrderUpdate
- Call ExecuteStopOrderUpdate
- MarkStickyDirty (state persistence)
- LogBuffer.Format (performance logging)

**Boundary**:
- **IN**: entryName, pos, offsetPoints, lastKnownPrice (unchanged)
- **OUT**: void (unchanged)
- **Role**: Orchestrator - delegates to extracted methods

**Rationale**: Orchestrator pattern with 3-4 decision points (validation result, update result, logging). Achieves CYC ≤8 target.

## Scope Boundaries

### What Gets Extracted
1. **Validation Logic**: All null checks, range checks, state checks → ValidateStopMoveRequest
2. **Price Calculation**: Stop price computation, offset application → CalculateNewStopPrice
3. **Pre-Update Checks**: Order lookup, pending replacement handling → PrepareStopOrderUpdate
4. **Order Submission**: UpdateStopOrder, InitiateStopReplacement, error handling → ExecuteStopOrderUpdate

### What Stays in Original
1. **Orchestration**: High-level flow control (call extracted methods)
2. **State Persistence**: MarkStickyDirty call
3. **Performance Logging**: LogBuffer.Format call
4. **Method Signature**: Unchanged (preserves caller contract)

### What Does NOT Change
1. **Caller**: MoveStopsToBreakevenWithOffset (no changes required)
2. **Callees**: All 46 callees remain unchanged (internal refactoring only)
3. **Public API**: No external-facing changes
4. **Behavior**: Functionally equivalent (refactoring, not rewriting)

## Dependencies

### Internal Dependencies (Within File)
- stopOrders (dictionary)
- pendingStopReplacements (dictionary)
- UpdateStopOrder (method)
- ValidateStopPrice (method)
- HandleStalePendingReplacement (method)
- UpdateExistingPendingReplacement (method)
- InitiateStopReplacement (method)
- CreateDirectStopOrder (method)
- HandleUpdateException (method)
- MarkStickyDirty (method)
- LogBuffer.Format (method)

### External Dependencies
- **NONE** (zero blast radius confirmed)

### Data Structures
- PositionInfo (parameter type)
- StopOrderUpdateContext (NEW - struct to pass between extracted methods)

## Risk Assessment

### Refactoring Risks: LOW
1. **Blast Radius**: ZERO (no external dependencies)
2. **Caller Impact**: NONE (single caller, unchanged signature)
3. **Behavioral Change**: NONE (functionally equivalent)
4. **Test Coverage**: Existing tests remain valid (black-box behavior unchanged)

### Complexity Risks: MEDIUM
1. **Extraction Count**: 4 methods (manageable)
2. **Context Passing**: StopOrderUpdateContext struct (new data structure)
3. **Call Chain Depth**: +1 level (orchestrator → extracted methods)
4. **Cognitive Load**: Reduced per method, increased file-level (trade-off)

### Mitigation Strategies
1. **Preserve Semantics**: Extract vertically (preserve call order)
2. **Single Responsibility**: Each extracted method has one clear purpose
3. **Minimal Context**: Pass only required parameters (avoid God objects)
4. **Incremental Extraction**: Extract one method at a time, verify build after each
5. **Test After Each**: Run existing tests after each extraction

## Success Criteria

### Quantitative Metrics
- [ ] MoveStop_SinglePosition CYC reduced from 21 to ≤8
- [ ] All extracted methods have CYC ≤8
- [ ] Zero compilation errors
- [ ] Zero test failures
- [ ] deploy-sync.ps1 executes successfully
- [ ] F5 in NinjaTrader loads strategy without errors

### Qualitative Metrics
- [ ] Each extracted method has single responsibility
- [ ] Method names clearly describe purpose
- [ ] No behavioral changes (functionally equivalent)
- [ ] Code is more readable (reduced nesting)
- [ ] Easier to test (smaller units)

### Jane Street Alignment
- [ ] All methods meet CYC ≤8 strict standard
- [ ] Cognitive simplicity achieved (easier to reason about)
- [ ] Exhaustive testing feasible (reduced path explosion)
- [ ] Race condition auditing simplified (smaller units)

## Exclusions (Out of Scope)

### What This Epic Does NOT Include
1. **Behavioral Changes**: No logic changes, only structural refactoring
2. **Performance Optimization**: No performance tuning (preserve existing behavior)
3. **New Features**: No new functionality added
4. **Caller Refactoring**: MoveStopsToBreakevenWithOffset remains unchanged
5. **Callee Refactoring**: 46 callees remain unchanged (separate epics if needed)
6. **Test Additions**: No new tests (existing tests must pass)
7. **Documentation Updates**: No user-facing documentation changes

### Future Work (Separate Epics)
- EPIC-W7-134: Refactor MoveStopsToBreakevenWithOffset (caller, CYC unknown)
- EPIC-W7-135+: Refactor callees if they exceed CYC ≤8 threshold

## Phase 2 Preparation

### Architecture Planning Inputs
1. **Extraction Targets**: 4 methods defined above
2. **Estimated CYC**: ValidateStopMoveRequest (4-5), CalculateNewStopPrice (2-3), PrepareStopOrderUpdate (5-6), ExecuteStopOrderUpdate (4-5), Orchestrator (3-4)
3. **Data Structures**: StopOrderUpdateContext struct (to be designed in Phase 2)
4. **Dependencies**: 11 internal methods, 2 dictionaries, 1 parameter type
5. **Risk Profile**: LOW refactoring risk, MEDIUM complexity risk

### Next Phase Actions
1. Design StopOrderUpdateContext struct
2. Define exact method signatures for all 4 extracted methods
3. Map decision points to extracted methods (verify CYC estimates)
4. Create extraction sequence (order of operations)
5. Define rollback strategy (if extraction fails mid-way)

## Approval Gate

### Ready for Phase 1.5 (Scope Boundary Validation)
- [x] Extraction targets defined (4 methods)
- [x] Boundaries clear (what stays, what goes)
- [x] Dependencies identified (11 internal, 0 external)
- [x] Risks assessed (LOW refactoring, MEDIUM complexity)
- [x] Success criteria defined (quantitative + qualitative)
- [x] Exclusions documented (out of scope)

**Status**: READY FOR PHASE 1.5 VALIDATION
