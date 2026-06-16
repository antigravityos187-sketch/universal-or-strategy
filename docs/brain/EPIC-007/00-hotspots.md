# Phase 0: Hotspot Analysis - EPIC-007

## Epic Overview
- **Epic ID**: EPIC-007
- **Target File**: src/V12_002.SIMA.Shadow.cs
- **Methods**: ShadowPropagateStopMoves, ShadowProcessFollowerStopUpdate
- **Current Complexity**: 20, 12
- **Target Complexity**: <=8 per method

## Target Methods

### Method 1: ShadowPropagateStopMoves
- **Cyclomatic Complexity**: 20
- **Risk Level**: HIGH
- **Primary Concern**: Complex conditional logic for stop loss propagation across shadow positions

### Method 2: ShadowProcessFollowerStopUpdate
- **Cyclomatic Complexity**: 12
- **Risk Level**: MEDIUM
- **Primary Concern**: Follower position stop update logic with multiple state checks

## Complexity Analysis

### ShadowPropagateStopMoves Breakdown
- **Complexity Score**: 20 (2.5x over threshold of 8)
- **Branching Points**: Multiple nested conditionals for:
  - Position state validation
  - Stop loss type checking
  - Shadow position synchronization
  - Error handling paths

### ShadowProcessFollowerStopUpdate Breakdown
- **Complexity Score**: 12 (1.5x over threshold of 8)
- **Branching Points**: Conditional logic for:
  - Follower position validation
  - Stop update type determination
  - State synchronization
  - Edge case handling

## Blast Radius Assessment

### ShadowPropagateStopMoves
- **Direct Callers**: Methods that trigger stop loss propagation in shadow trading
- **Downstream Impact**: Shadow position state management, stop loss synchronization
- **Risk**: Changes could affect multi-position stop loss coordination

### ShadowProcessFollowerStopUpdate
- **Direct Callers**: Follower position update handlers
- **Downstream Impact**: Follower stop loss tracking, position state consistency
- **Risk**: Changes could affect follower position synchronization

## Call Hierarchy

### ShadowPropagateStopMoves
- Called by: Shadow position management methods
- Calls: Stop loss calculation, position state update methods
- Integration Points: SIMA shadow trading subsystem

### ShadowProcessFollowerStopUpdate
- Called by: Follower position event handlers
- Calls: Stop update validation, state synchronization methods
- Integration Points: Follower position tracking subsystem

## Refactoring Strategy

### Extraction Targets for ShadowPropagateStopMoves (CYC 20 -> <=8)
1. **ValidateShadowPositionForStopPropagation** (CYC ~4)
   - Extract position state validation logic
   - Consolidate null checks and state verification

2. **CalculateStopLossForShadowPosition** (CYC ~5)
   - Extract stop loss calculation logic
   - Isolate price computation and type handling

3. **ApplyShadowStopLossUpdate** (CYC ~4)
   - Extract stop loss application logic
   - Separate update execution from validation

4. **Core orchestration** (CYC ~7)
   - Coordinate extracted methods
   - Handle high-level flow control

### Extraction Targets for ShadowProcessFollowerStopUpdate (CYC 12 -> <=8)
1. **ValidateFollowerStopUpdate** (CYC ~3)
   - Extract follower position validation
   - Consolidate state checks

2. **DetermineStopUpdateType** (CYC ~4)
   - Extract stop update type logic
   - Isolate type determination rules

3. **Core orchestration** (CYC ~5)
   - Coordinate extracted methods
   - Handle update flow

## Risk Assessment

### Overall Risk: MEDIUM-HIGH
- **Complexity Risk**: HIGH (Method 1 at 2.5x threshold)
- **Integration Risk**: MEDIUM (Shadow trading subsystem coupling)
- **Testing Risk**: MEDIUM (Requires shadow position test scenarios)

### Mitigation Strategy
1. Extract validation logic first (lowest risk)
2. Extract calculation logic second (medium risk)
3. Refactor core orchestration last (highest risk)
4. Maintain atomic commits with rollback points
5. Verify shadow position behavior after each extraction

## Jane Street Alignment
- **Cognitive Simplicity**: Current CYC 20 violates "make illegal states unrepresentable"
- **Testability**: High complexity makes exhaustive path testing impractical
- **Auditability**: Nested conditionals obscure race condition analysis
- **Target**: CYC <=8 aligns with HFT cognitive load constraints

## Next Steps (Phase 1)
1. Generate mini-spec for extraction strategy
2. Create Mermaid diagrams for method decomposition
3. Define extraction boundaries and interfaces
4. Plan test coverage for extracted methods

## Verification Criteria
- [ ] All extracted methods have CYC <=8
- [ ] Shadow position behavior unchanged (integration tests pass)
- [ ] No new lock() statements introduced
- [ ] ASCII-only compliance maintained
- [ ] Hard-link sync verified (deploy-sync.ps1)
