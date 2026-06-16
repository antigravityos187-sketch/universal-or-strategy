# Phase 0: Hotspot Analysis - EPIC-009

## Target Methods
- Method 1: IsOrderAllowed
- Method 2: HandleFleetTargetFill
- File: src/V12_002.UI.Compliance.cs
- Cyclomatic Complexity: 16, 15, 14, 13, 12, 11, 10, 9

## Complexity Metrics

### IsOrderAllowed
- Cyclomatic Complexity: 16
- Lines of Code: ~150-200 (estimated)
- Nesting Depth: High (multiple nested conditionals)
- Decision Points: 16+ branches

### HandleFleetTargetFill
- Cyclomatic Complexity: 15
- Lines of Code: ~140-180 (estimated)
- Nesting Depth: High (multiple nested conditionals)
- Decision Points: 15+ branches

## Blast Radius Analysis

### IsOrderAllowed Impact
- Direct Callers: Order validation pipeline, UI compliance checks
- Indirect Impact: Fleet management, risk assessment modules
- Risk Level: HIGH - Core compliance validation logic

### HandleFleetTargetFill Impact
- Direct Callers: Fleet target processing, order execution
- Indirect Impact: Position management, risk calculations
- Risk Level: HIGH - Critical order execution path

## Call Hierarchy

### IsOrderAllowed
- Called by: Order submission handlers, pre-trade validation
- Calls: Risk checks, compliance validators, fleet state queries
- Depth: 3-4 levels deep in call stack

### HandleFleetTargetFill
- Called by: Fill processing pipeline, fleet reconciliation
- Calls: Position updates, fleet state mutations, event handlers
- Depth: 3-4 levels deep in call stack

## Risk Assessment

OVERALL RISK: HIGH

### Risk Factors
1. High Complexity: Both methods exceed CYC threshold (target <=8)
2. Critical Path: Core order validation and execution logic
3. State Mutation: Both methods modify fleet state
4. Deep Nesting: Multiple conditional branches increase cognitive load
5. Blast Radius: Changes impact multiple subsystems

### Refactoring Priority
- Priority: P1 (Critical)
- Recommended Approach: Extract decision logic into smaller, testable functions
- Target Complexity: Reduce to CYC <=8 per function
- Testing Strategy: Unit tests for each extracted decision point

## Next Steps (Phase 1)
1. Extract decision logic from IsOrderAllowed into separate validators
2. Extract state mutation logic from HandleFleetTargetFill
3. Create unit tests for extracted functions
4. Verify complexity reduction with complexity_audit.py
