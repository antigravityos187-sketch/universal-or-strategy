# Phase 0: Hotspot Analysis - EPIC-003

## Epic Overview
- Epic ID: EPIC-003
- Target File: src/V12_002.Orders.Management.StopSync.cs
- Target Methods: SyncLimitTarget (CCN 17), SyncStopTarget (CCN 9)
- Complexity Threshold: <=8 (Jane Street alignment)
- Current Status: Both methods exceed threshold

## Target Methods

### Method 1: SyncLimitTarget
- Cyclomatic Complexity: 17
- Threshold Violation: +9 over limit (17 vs 8)
- Risk Level: HIGH
- Priority: P1 (Critical)

### Method 2: SyncStopTarget
- Cyclomatic Complexity: 9
- Threshold Violation: +1 over limit (9 vs 8)
- Risk Level: MEDIUM
- Priority: P2 (High)

## Complexity Metrics

### SyncLimitTarget Analysis
- Lines of Code: ~150-200 (estimated)
- Branching Points: 17 decision points
- Cognitive Load: HIGH - difficult to reason about under microsecond latency
- Test Coverage: Unknown (requires verification)
- Lock-Free Compliance: Requires audit

Complexity Breakdown:
- Multiple nested conditionals
- State machine logic embedded in single method
- Order validation + limit synchronization co-located
- Error handling paths increase branching

### SyncStopTarget Analysis
- Lines of Code: ~80-120 (estimated)
- Branching Points: 9 decision points
- Cognitive Load: MEDIUM - manageable but exceeds threshold
- Test Coverage: Unknown (requires verification)
- Lock-Free Compliance: Requires audit

Complexity Breakdown:
- Stop order synchronization logic
- Conditional state transitions
- Validation checks increase branching

## Blast Radius Assessment

### Direct Dependencies
- File: V12_002.Orders.Management.StopSync.cs
- Namespace: V12_002.Orders.Management
- Class: StopSync (assumed)

### Potential Impact Areas
1. Order Management System: Core order synchronization logic
2. Limit Order Processing: SyncLimitTarget handles limit order state
3. Stop Order Processing: SyncStopTarget handles stop order state
4. State Machine: Both methods likely interact with FSM/Actor pattern
5. NinjaTrader Integration: Hard-link deployment requires careful testing

### Risk Factors
- Co-location: Both methods in same file increases refactoring risk
- State Coupling: Likely share state variables or dependencies
- Order Lifecycle: Critical path for order execution
- Latency Sensitivity: HFT context requires microsecond precision

## Refactoring Strategy

### Phase 1: Extraction (SyncLimitTarget - CCN 17)
Target: Reduce from CCN 17 to <=8

Extraction Candidates:
1. ValidateLimitOrder() - Extract order validation logic
2. UpdateLimitState() - Extract state transition logic
3. SyncLimitPosition() - Extract position synchronization
4. HandleLimitError() - Extract error handling paths

Expected Outcome: 4-5 focused methods, each CCN <=5

### Phase 2: Extraction (SyncStopTarget - CCN 9)
Target: Reduce from CCN 9 to <=8

Extraction Candidates:
1. ValidateStopOrder() - Extract order validation logic
2. UpdateStopState() - Extract state transition logic
3. HandleStopError() - Extract error handling paths

Expected Outcome: 3-4 focused methods, each CCN <=5

## Risk Assessment

### Overall Risk: HIGH

Risk Factors:
1. Complexity: CCN 17 is 2.1x over threshold (high cognitive load)
2. Co-location: Both methods in same file (blast radius overlap)
3. Critical Path: Order synchronization is core functionality
4. HFT Context: Microsecond latency requirements
5. Hard-Link Deployment: NinjaTrader integration adds deployment risk

Mitigation Strategy:
1. Incremental Extraction: Extract one method at a time
2. Test Coverage: Add unit tests before refactoring
3. Checkpointing: Use Bob CLI checkpointing for rollback safety
4. Verification: Run deploy-sync.ps1 after each extraction
5. Build Validation: Verify compilation after each step

## V12 DNA Compliance Audit

### Lock-Free Pattern
- Status: REQUIRES VERIFICATION
- Action: Scan for lock( statements in target file

### ASCII-Only Compliance
- Status: REQUIRES VERIFICATION
- Action: Scan for non-ASCII characters

### Correctness by Construction
- Status: REQUIRES DESIGN REVIEW
- Action: Verify state machine prevents illegal states
- Focus: Ensure extracted methods maintain invariants

## Next Steps (Phase 1)

1. Forensic Scan: Run lock-free and ASCII compliance checks
2. Test Coverage: Verify existing tests for target methods
3. Design Review: Create mini-spec for extraction strategy
4. Implementation Plan: Generate detailed extraction plan with Mermaid diagrams
5. DNA Audit: Arena AI red team review of plan
6. Execution: Bob CLI surgical extraction with checkpointing

## Success Criteria

- Both methods reduced to CCN <=8
- No lock() statements introduced
- ASCII-only compliance maintained
- Build passes after each extraction
- Hard-link sync successful (deploy-sync.ps1)
- Unit tests pass (or added if missing)
- Code Health Score improves (CodeScene)

## Estimated Effort

- Phase 0 (This Document): 1 hour (COMPLETED)
- Phase 1 (Design): 2-3 hours
- Phase 2 (Planning): 2-3 hours
- Phase 3 (DNA Audit): 1-2 hours
- Phase 4 (Execution): 4-6 hours
- Phase 5 (Verification): 2-3 hours
- Total: 12-18 hours

## References

- V12 DNA: AGENTS.md (Lock-Free, ASCII-Only, Correctness by Construction)
- Jane Street Alignment: docs/intel/jane-street/ (CCN <=15 guideline)
- Complexity Threshold: .codacy.yml (CCN <=15 configured)
- Phase 6 Protocol: AGENTS.md Section 7 (Recursive extraction workflow)
