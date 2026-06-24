# Phase 1: Scope Boundary - EPIC-W7-107

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:35:01Z
- **Input**: 00-hotspots.md
- **Output**: 01-scope-boundary.md

## Target Method
- **Method**: HydrateFromOpenPositions
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 625
- **Current CYC**: 34
- **Target CYC**: ≤8 per extracted method

## Scope Definition

### IN SCOPE

#### 1. Parameter Validation Extraction
**Rationale**: 14 parameters is excessive. Extract validation logic to reduce cognitive load.
- Extract null checks and precondition validation
- Create `ValidateHydrationParameters()` helper
- Target CYC: ≤3

#### 2. Order Collection Iteration Logic
**Rationale**: Iterating through 6 order collections (stop, target1-5) creates deep nesting.
- Extract order collection processing to `ProcessOrderCollection()`
- Reduce nesting from 5 levels to ≤3
- Target CYC: ≤5 per collection handler

#### 3. Fleet Account Handling
**Rationale**: IsFleetAccount checks appear multiple times, creating branching complexity.
- Extract fleet-specific logic to `HydrateFleetAccount()`
- Consolidate fleet bracket management
- Target CYC: ≤6

#### 4. FSM State Hydration
**Rationale**: Core responsibility - linking orders to FSM state.
- Extract FSM linking logic to `LinkOrdersToFSM()`
- Separate master vs follower logic
- Target CYC: ≤7

#### 5. Logging/Formatting Reduction
**Rationale**: 22 callees includes multiple logging calls - extract to reduce noise.
- Consolidate logging to single helper method
- Reduce inline LogBuffer.Format calls
- Target CYC: ≤2

### OUT OF SCOPE

#### 1. Caller Methods (3 upstream)
**Rationale**: Zero blast radius means we don't need to modify callers.
- HydrateFSMsFromWorkingOrders (depth 1)
- HydrateWorkingOrdersFromBroker (depth 2)
- EnumerateApexAccounts (depth 3)
- **Action**: Leave unchanged - they will call refactored method transparently

#### 2. Callee Methods (22 downstream)
**Rationale**: Existing helper methods are already extracted - don't refactor them.
- IsFleetAccount
- Order collections (stopOrders, target1-5Orders)
- _followerBrackets
- LogBuffer methods
- **Action**: Use as-is - focus on orchestration logic only

#### 3. Method Signature Changes
**Rationale**: Maintain backward compatibility with zero blast radius.
- Keep all 14 parameters (for now)
- Don't change return type
- Don't change method name
- **Action**: Future epic can address parameter reduction after extraction

#### 4. Test File Creation
**Rationale**: Phase 5.V (Verification) handles test generation.
- **Action**: Defer to Phase 5.V

#### 5. Adjacent Methods in Same File
**Rationale**: One epic = one concern (No Scope Creep Protocol V12.23).
- Don't touch other methods in V12_002.SIMA.Lifecycle.cs
- **Action**: Each method gets its own epic

## Extraction Strategy

### Primary Goal
Reduce CYC from 34 to ≤8 by extracting 4-5 helper methods, each with CYC ≤8.

### Extraction Order (Tickets)
1. **Ticket 1**: Extract parameter validation → `ValidateHydrationParameters()` (CYC ≤3)
2. **Ticket 2**: Extract order collection iteration → `ProcessOrderCollection()` (CYC ≤5)
3. **Ticket 3**: Extract fleet account handling → `HydrateFleetAccount()` (CYC ≤6)
4. **Ticket 4**: Extract FSM state linking → `LinkOrdersToFSM()` (CYC ≤7)
5. **Ticket 5**: Consolidate logging → `LogHydrationEvent()` (CYC ≤2)

### Success Criteria
- Main method CYC reduced from 34 to ≤8
- Each extracted method has CYC ≤8
- Zero compilation errors
- Zero test failures
- F5 in NinjaTrader successful
- BUILD_TAG verification passed

## Risk Mitigation

### Zero Blast Radius Advantage
- No external dependencies to break
- No callers to update
- Safe to refactor aggressively

### Potential Risks
1. **Parameter coupling**: 14 parameters may create tight coupling in extracted methods
   - **Mitigation**: Pass only required parameters to each helper
2. **State mutation**: Method may mutate shared state
   - **Mitigation**: Audit for side effects before extraction
3. **Order of operations**: Extraction may change execution order
   - **Mitigation**: Preserve exact logic flow in extracted methods

## Jane Street Alignment

### Cognitive Simplicity
- Current CYC 34 = impossible to reason about under microsecond latency
- Target CYC ≤8 = each method fits in working memory
- Extraction enables exhaustive testing (2^8 = 256 paths vs 2^34 = 17B paths)

### Correctness by Construction
- Extract validation logic to make invalid states unrepresentable
- Use early returns to eliminate nested if/else
- Prefer guard clauses over deep nesting

### Lock-Free Actor Pattern
- Audit for lock() statements (should be zero)
- Ensure FSM state mutations use Enqueue model
- Verify atomic primitives for shared state

## Phase 1 Completion

**Status**: ✅ SCOPE DEFINED

**Next Phase**: Phase 2 (Architecture Planning)
- Input: This scope boundary document
- Output: 02-architecture-plan.md with detailed extraction design
- Agent: v12-phase2-architecture

**Approval**: Ready for Phase 2
