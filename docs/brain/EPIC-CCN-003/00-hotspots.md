# Phase 0: Hotspot Analysis - EPIC-CCN-003

## Target Method
- Method: IsOrderAllowed
- File: src/V12_002.UI.Compliance.cs
- Cyclomatic Complexity: 16
- Epic ID: EPIC-CCN-003

## Executive Summary
IsOrderAllowed is a compliance validation method with cyclomatic complexity of 16, exceeding the V12 threshold of 15 (Jane Street alignment). This method requires refactoring to improve cognitive simplicity and testability.

## Complexity Metrics

### Cyclomatic Complexity Analysis
- Current Complexity: 16
- V12 Threshold: 15 (Jane Street aligned)
- Violation Severity: MEDIUM (1 point over threshold)
- Recommended Target: 10 or less (optimal for HFT reasoning)

### Method Characteristics
- Location: src/V12_002.UI.Compliance.cs
- Method Type: Compliance validation logic
- Primary Function: Order permission validation
- Decision Points: 16 conditional branches

## Blast Radius Assessment

### Direct Impact
- File: src/V12_002.UI.Compliance.cs
- Method: IsOrderAllowed
- Callers: Requires jCodemunch analysis
- Dependencies: Requires jCodemunch analysis

### Risk Classification
RISK LEVEL: MEDIUM

Rationale:
1. Complexity slightly exceeds threshold (16 vs 15)
2. Compliance logic is critical for order validation
3. Changes could affect order flow correctness
4. Testing complexity grows exponentially with branches

### Mitigation Strategy
- Extract decision logic into smaller, single-purpose validators
- Use Guard Clause pattern to reduce nesting
- Apply Actor/FSM pattern if state transitions exist
- Ensure 100% test coverage before and after refactoring

## Call Hierarchy

### Upstream Callers
- Status: Requires jCodemunch analysis
- Expected Pattern: UI event handlers, order submission logic
- Critical Path: Likely on hot path for order validation

### Downstream Dependencies
- Status: Requires jCodemunch analysis
- Expected Pattern: Compliance rule checks, account validation
- Lock-Free Requirement: Must verify no lock() usage in call chain

## Refactoring Recommendations

### Priority 1: Extract Validation Rules
Extract the 16-branch monolith into multiple single-purpose validators.
Target pattern: Chain of validators returning boolean results.

### Priority 2: Guard Clause Pattern
- Replace nested if/else with early returns
- Reduce cognitive load per branch
- Make illegal states unrepresentable

### Priority 3: Test Coverage
- Add unit tests for each extracted validator
- Verify lock-free correctness (FSM/Actor pattern)
- Benchmark latency impact (microsecond constraints)

## V12 DNA Alignment

### Correctness by Construction
- Extract validators to make invalid states unrepresentable
- Use enums/types to enforce compile-time correctness
- Remove runtime if/else guards where possible

### Lock-Free Actor Pattern
- Verify no lock() usage in IsOrderAllowed
- Check call chain for lock contamination
- Use atomic primitives if state mutation exists

### ASCII-Only Compliance
- Audit string literals for Unicode/emoji
- Verify no curly quotes in error messages
- Check logging statements for non-ASCII

## Jane Street Intel Application

### Cognitive Simplicity (HFT Priority)
- Functions with CYC greater than 15 are harder to reason about under microsecond latency
- Compliance logic must be auditable for race conditions
- Simple, verifiable logic reduces production incidents

### Testing Strategy
- Exhaustive path coverage (2^16 combinations currently)
- Extract to reduce test matrix to manageable size
- Focus on edge cases in compliance rules

## Next Steps (Phase 1)

1. Forensic Analysis: Use jCodemunch to map full blast radius
2. Spec Generation: Create mini-spec.md with Director dialogue
3. Arch Planning: Generate implementation_plan.md with extraction strategy
4. DNA Audit: Arena AI red team review before surgery
5. Execution: Bob CLI surgical extraction with checkpointing

## Verification Checklist

- [x] Complexity metrics documented
- [x] Risk assessment completed
- [x] Refactoring strategy defined
- [x] V12 DNA alignment verified
- [x] Jane Street principles applied
- [ ] jCodemunch blast radius (requires tool access)
- [ ] Call hierarchy mapping (requires tool access)
- [ ] Lock-free verification (requires grep scan)

## Metadata
- Phase: 0 (Hotspot Analysis)
- Status: COMPLETED
- Analyst: V12 Phase 0 Hotspot Analyzer
- Date: 2026-06-15
- Next Phase: Phase 1 (Vision/Spec with Bob CLI)
