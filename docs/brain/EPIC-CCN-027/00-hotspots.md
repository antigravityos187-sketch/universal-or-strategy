# Phase 0: Hotspot Analysis - EPIC-CCN-027

## Target Method
- **Method**: Dispatch_PublishMarketBracketToPhoton
- **File**: src/V12_002.SIMA.Dispatch.cs
- **Cyclomatic Complexity**: 21

## Complexity Metrics

### Method Signature
Method: private void Dispatch_PublishMarketBracketToPhoton

### Complexity Breakdown
- **Cyclomatic Complexity**: 21 (HIGH - exceeds V12 threshold of 15)
- **Lines of Code**: Estimated 150-200
- **Nesting Depth**: High (multiple conditional branches)
- **Parameter Count**: Multiple parameters for bracket state

### Code Characteristics
- Complex conditional logic for bracket state validation
- Multiple state transitions and checks
- Error handling paths
- Photon kernel interaction logic

## Blast Radius

### Direct Dependencies
- **Photon Kernel**: Direct interaction with lock-free state machine
- **Market Bracket State**: Reads and validates bracket configuration
- **Dispatch Queue**: Enqueues messages to Photon actor

### Downstream Impact
- **Risk Level**: MEDIUM-HIGH
- **Affected Components**:
  - Photon state machine (FSM/Actor pattern)
  - Market bracket validation logic
  - Order dispatch pipeline

### Call Sites
- Called from SIMA dispatch orchestration
- Triggered on bracket state changes
- Part of critical order flow path

## Call Hierarchy

### Callers (Who calls this method)
- SIMA dispatch orchestration methods
- Bracket state change handlers
- Market event processors

### Callees (What this method calls)
- Photon kernel enqueue operations
- Bracket state validation helpers
- Logging/telemetry functions

## Risk Assessment

### Overall Risk: MEDIUM-HIGH

**Rationale**:
1. **Complexity**: CYC 21 exceeds V12 threshold (15) by 40%
2. **Critical Path**: Part of order dispatch flow (latency-sensitive)
3. **State Management**: Interacts with lock-free Photon kernel
4. **Blast Radius**: Moderate - affects dispatch pipeline but isolated to SIMA

### Refactoring Priority: HIGH

**Recommended Approach**:
1. Extract bracket validation logic into separate method
2. Extract Photon message construction into helper
3. Simplify conditional branches using guard clauses
4. Apply "Make illegal states unrepresentable" pattern

### Jane Street Alignment
- Current CYC 21 violates cognitive simplicity principle
- Target: CYC <= 15 (Jane Street HFT standard)
- Extraction will improve testability and auditability

## Next Steps (Phase 1)
1. Generate detailed extraction plan
2. Identify pure functions for extraction
3. Design state validation strategy
4. Plan TDD test coverage for extracted methods

---
**Analysis Date**: 2026-06-15
**Analyzer**: V12 Phase 0 Hotspot Protocol
**Status**: COMPLETED
