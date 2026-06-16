# Phase 0: Hotspot Analysis - EPIC-015

## Epic Overview
- **Epic ID**: EPIC-015
- **Target File**: src/V12_002.UI.Panel.StateSync.cs
- **Target Methods**: UpdatePanelState, SyncPanelConfigFromSnapshot
- **Current Complexity**: 16, 15, 10, 9 (CCN)
- **Target Complexity**: ≤8 (Jane Street alignment)

## Target Methods

### Method 1: UpdatePanelState
- **Cyclomatic Complexity**: 16
- **Status**: HIGH PRIORITY - Exceeds threshold by 8 points
- **Category**: State synchronization logic

### Method 2: SyncPanelConfigFromSnapshot
- **Cyclomatic Complexity**: 15
- **Status**: HIGH PRIORITY - Exceeds threshold by 7 points
- **Category**: Configuration snapshot synchronization

### Supporting Methods
- **Method 3**: Complexity 10 (exceeds by 2)
- **Method 4**: Complexity 9 (exceeds by 1)

## Complexity Metrics

### UpdatePanelState (CCN: 16)
**Complexity Breakdown**:
- Base complexity: 1
- Conditional branches: ~15
- Likely patterns: Multiple if/else chains, switch statements, nested conditionals

**Cognitive Load**: HIGH
- 16 decision points = 2^16 possible execution paths
- Difficult to test exhaustively
- High risk for race conditions in lock-free code

### SyncPanelConfigFromSnapshot (CCN: 15)
**Complexity Breakdown**:
- Base complexity: 1
- Conditional branches: ~14
- Likely patterns: Configuration validation, snapshot version checks, property sync logic

**Cognitive Load**: HIGH
- 15 decision points = 2^15 possible execution paths
- Complex state reconciliation logic

## Blast Radius Analysis

### Direct Dependencies
**UpdatePanelState** likely called by:
- Panel initialization routines
- State change event handlers
- UI refresh mechanisms

**SyncPanelConfigFromSnapshot** likely called by:
- Configuration restore operations
- Panel state persistence layer
- Snapshot management system

### Impact Assessment
**Risk Level**: MEDIUM-HIGH

**Reasoning**:
1. UI State Management - critical UI state synchronization
2. Lock-Free Context - must maintain atomicity without locks
3. Snapshot Integrity - affects system-wide panel state
4. Testing Gap - no existing tests for these methods

**Blast Radius Estimate**:
- Direct callers: 5-10 methods
- Indirect impact: Panel rendering, state persistence, configuration management
- Failure mode: UI inconsistency, lost configuration, race conditions

## Risk Assessment

### Overall Risk: MEDIUM-HIGH

**Risk Factors**:
1. Complexity - both methods significantly exceed CCN threshold
2. Lock-Free Requirement - must maintain atomicity without locks
3. UI Critical Path - affects user-visible panel behavior
4. Test Coverage - zero tests for these methods
5. Blast Radius - moderate impact on panel subsystem

**Mitigation Strategy**:
1. Extract state machine logic to FSM/Actor pattern
2. Decompose configuration sync into atomic operations
3. Add TDD tests before refactoring
4. Use checkpointing during extraction

## Refactoring Strategy

### Phase 1: Extract State Machine (UpdatePanelState)
**Target**: Reduce CCN from 16 to ≤8

**Approach**:
1. Identify state transitions (likely 4-6 states)
2. Extract to FSM/Actor with Enqueue pattern
3. Separate validation logic into pure functions
4. Move UI update logic to separate methods

**Expected Outcome**:
- Main method: CCN ≤5 (dispatch only)
- State handlers: CCN ≤3 each
- Validation functions: CCN ≤2 each

### Phase 2: Decompose Config Sync (SyncPanelConfigFromSnapshot)
**Target**: Reduce CCN from 15 to ≤8

**Approach**:
1. Extract field-by-field sync into individual methods
2. Use property pattern matching
3. Separate validation from application logic
4. Create atomic update primitives

**Expected Outcome**:
- Main method: CCN ≤4 (orchestration only)
- Field sync methods: CCN ≤2 each
- Validation methods: CCN ≤3 each

## Jane Street Alignment

### Cognitive Simplicity Principles
**Current State**: VIOLATES Jane Street standards
- Functions with CCN >15 are cognitively complex
- Difficult to reason about under latency constraints
- Exponential test path growth

**Target State**: ALIGNED with Jane Street standards
- CCN ≤8 enables exhaustive testing
- Simple, verifiable logic for lock-free correctness
- Make illegal states unrepresentable via FSM

## Next Steps (Phase 1)

1. Vision/Spec (Bob CLI) - Generate mini-spec.md with Director dialogue
2. Arch Planning (Bob CLI) - Create implementation_plan.md with Mermaid diagrams
3. DNA & PR Audit (Arena AI) - Verify plan against V12 constraints
4. TDD Test Creation (MANDATORY) - Write tests before refactoring

## Success Criteria

- CCN reduced from 16→≤8 (UpdatePanelState)
- CCN reduced from 15→≤8 (SyncPanelConfigFromSnapshot)
- CCN reduced from 10→≤8 (supporting method 3)
- CCN reduced from 9→≤8 (supporting method 4)
- Zero lock() statements introduced
- TDD tests added for all extracted methods
- Build passes (dotnet build)
- Pre-push validation passes (all 13 checks)

---

**Phase 0 Status**: COMPLETED
**Next Phase**: Phase 1 (Vision/Spec) - Bob CLI session required
**Risk Level**: MEDIUM-HIGH
**Estimated Effort**: 2-3 sprints (with TDD test creation)
