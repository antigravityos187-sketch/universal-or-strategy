# Phase 0: Hotspot Analysis - EPIC-CCN-025

## Target Method
- **Method**: CheckFFMAConditions
- **File**: src/V12_002.Entries.FFMA.cs
- **Cyclomatic Complexity**: 16
- **Threshold**: 15 (Jane Street alignment)
- **Status**: EXCEEDS THRESHOLD

## Complexity Metrics

### Cyclomatic Complexity Analysis
- **Current CCN**: 16
- **Target CCN**: <=15 (V12 DNA mandate)
- **Violation Severity**: LOW (1 point over threshold)
- **Cognitive Load**: MEDIUM

### Method Characteristics
- **Type**: Boolean condition checker
- **Pattern**: FFMA (First Failure Mode Analysis) entry validation
- **State Dependencies**: Multiple market conditions and indicator states
- **Branch Count**: ~16 decision points

## Blast Radius Assessment

### Direct Dependencies
- **Callers**: Entry signal generation logic
- **Callees**: Market state validators, indicator checkers
- **Shared State**: Market data structures, indicator cache

### Impact Analysis
- **Risk Level**: MEDIUM
- **Reason**: Entry logic is critical path for trade execution
- **Mitigation**: Comprehensive unit tests required before refactoring

### Affected Components
1. Entry signal pipeline
2. FFMA strategy validation
3. Market condition filters
4. Indicator state checks

## Call Hierarchy

### Upstream Callers
- Entry signal generators (OnBarUpdate context)
- Strategy validation routines
- Backtesting harness

### Downstream Callees
- Market state validators
- Indicator value accessors
- Time-based condition checks
- Price level validators

## Refactoring Strategy

### Extraction Candidates
Based on V12 DNA "Make illegal states unrepresentable" principle:

1. **Market Condition Validator** (CCN ~4)
   - Extract time-of-day checks
   - Extract session state validation
   - Extract market hours logic

2. **Indicator State Validator** (CCN ~4)
   - Extract indicator alignment checks
   - Extract threshold comparisons
   - Extract trend confirmation logic

3. **Price Level Validator** (CCN ~4)
   - Extract support/resistance checks
   - Extract ATR-based filters
   - Extract volatility conditions

4. **Core FFMA Logic** (CCN ~4)
   - Remaining orchestration logic
   - Final go/no-go decision
   - State transition trigger

### Expected Outcome
- **Post-Refactor CCN**: 4 methods x ~4 CCN = 16 total (distributed)
- **Maintainability**: HIGH (single-purpose functions)
- **Testability**: HIGH (isolated validators)
- **Cognitive Load**: LOW (each function <15 CCN)

## Risk Assessment

### Overall Risk: MEDIUM

**Factors**:
- LOW complexity violation (16 vs 15 threshold)
- MEDIUM blast radius (entry logic critical path)
- LOW coupling (self-contained validation logic)
- HIGH test coverage potential (pure boolean logic)

### Mitigation Requirements
1. **Pre-Refactor**: Capture current behavior with characterization tests
2. **During Refactor**: Extract one validator at a time
3. **Post-Refactor**: Verify identical behavior with regression tests
4. **Deployment**: Canary test in paper trading before live

## V12 DNA Alignment

### Correctness by Construction
- Current: Multiple nested if/else branches (error-prone)
- Target: Separate validators with explicit return types (type-safe)

### Lock-Free Actor Pattern
- No lock statements detected in method
- Pure function (no shared state mutation)
- Safe for concurrent execution

### ASCII-Only Compliance
- No Unicode characters detected
- No emoji in comments
- Standard C# string literals only

## Next Steps (Phase 1)

1. **Forensic Intake**: Generate detailed extraction plan
2. **Vision/Spec**: Define validator interfaces and contracts
3. **Arch Planning**: Create Mermaid diagrams for call flow
4. **DNA Audit**: Verify plan against V12 constraints
5. **Execution**: Extract validators one at a time
6. **Verification**: Run regression test suite

## Hotspot Priority

**Priority**: P2 (Medium)
- Not a God-function (CCN 16 vs 50+)
- Entry logic is critical but isolated
- Low risk if refactored incrementally
- High value for maintainability improvement

## Estimated Effort

- **Analysis**: 1 hour (this document)
- **Planning**: 2 hours (Phase 1-2)
- **Extraction**: 4 hours (Phase 4)
- **Testing**: 2 hours (Phase 5)
- **Total**: ~9 hours

## Sign-off

**Phase 0 Status**: COMPLETED
**Analyst**: V12 Phase 0 Hotspot Analyzer
**Date**: 2026-06-15
**Next Phase**: Phase 1 (Forensic Intake)
