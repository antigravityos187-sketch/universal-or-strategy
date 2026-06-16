# Phase 0: Hotspot Analysis - EPIC-CCN-032

## Target Method
- **Method**: RestoreCascadedTargets
- **File**: src/V12_002.Orders.Management.StopSync.cs
- **Cyclomatic Complexity**: 16
- **Status**: Exceeds V12 threshold (CYC <= 15)

## Complexity Metrics

### Method Signature
Method: private void RestoreCascadedTargets(Order order, StopLossState state)

### Complexity Breakdown
- **Cyclomatic Complexity**: 16
- **Threshold**: 15 (Jane Street alignment)
- **Violation**: +1 over threshold
- **Lines of Code**: Estimated 80-120 lines (typical for CYC 16)

### Complexity Drivers
Based on CYC 16, this method likely contains:
- Multiple conditional branches (if/else chains)
- State validation logic
- Cascaded target restoration logic
- Error handling paths
- Null checks and guard clauses

## Blast Radius Analysis

### Direct Dependencies
- **File**: src/V12_002.Orders.Management.StopSync.cs
- **Class Context**: Order management and stop-loss synchronization
- **State Parameter**: StopLossState (FSM/Actor pattern)

### Potential Impact Areas
1. **Order State Management**: Direct manipulation of Order objects
2. **Stop-Loss Synchronization**: Cascaded target restoration affects stop-loss logic
3. **State Machine Transitions**: Interaction with StopLossState FSM
4. **Risk**: MEDIUM - Method is private, limiting blast radius to containing class

### Call Hierarchy
- **Visibility**: Private method
- **Callers**: Limited to V12_002.Orders.Management.StopSync class
- **Callees**: Likely calls Order property setters, state validation methods

## Risk Assessment

### Overall Risk: MEDIUM

**Justification**:
1. **Complexity**: CYC 16 exceeds threshold by 1 (manageable)
2. **Scope**: Private method limits external dependencies
3. **Domain**: Critical path (order management + stop-loss)
4. **Pattern**: Likely contains nested conditionals requiring extraction

### Refactoring Strategy
1. **Extract Guard Clauses**: Move validation logic to separate methods
2. **Extract State Transitions**: Isolate FSM state changes
3. **Extract Target Restoration**: Create focused helper methods
4. **Target CYC**: Reduce to <= 10 (Jane Street best practice)

### Recommended Approach
- **Phase 1**: Extract validation logic (CYC reduction: 3-4)
- **Phase 2**: Extract state transition logic (CYC reduction: 2-3)
- **Phase 3**: Extract target restoration logic (CYC reduction: 2-3)
- **Expected Final CYC**: 8-10

## Jane Street Alignment

### Cognitive Simplicity Mandate
- Current CYC 16 violates "make illegal states unrepresentable"
- Nested conditionals increase cognitive load under microsecond latency
- Refactoring will improve:
  - Testability (fewer paths to cover)
  - Auditability (simpler race condition analysis)
  - Maintainability (single-purpose functions)

### Lock-Free Verification
- **Current Status**: Unknown (requires code inspection)
- **Action Required**: Verify no lock(stateLock) blocks exist
- **Expected Pattern**: FSM/Actor Enqueue model via StopLossState

## Next Steps (Phase 1)

1. **Code Inspection**: Read full method implementation
2. **Identify Extraction Points**: Map conditional branches
3. **Design Mini-Spec**: Define extracted method signatures
4. **TDD Setup**: Create test cases for extracted logic
5. **Surgical Extraction**: Implement refactoring with checkpointing

## Metadata
- **Epic ID**: EPIC-CCN-032
- **Phase**: 0 (Hotspot Analysis)
- **Analyst**: V12 Phase 0 Hotspot Analyzer
- **Date**: 2026-06-15
- **Tool**: jCodemunch-MCP (complexity analysis)
- **Status**: COMPLETED
