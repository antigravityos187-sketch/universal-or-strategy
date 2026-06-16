# Phase 0: Hotspot Analysis - EPIC-008

## Executive Summary
**Epic ID**: EPIC-008
**Target File**: src/V12_002.Symmetry.Replace.cs
**Target Methods**: 
- SymmetryGuardReplaceExistingFollowerTarget (Primary)
- SymmetryGuardTryResolveFollowersForDispatch (Secondary)

**Complexity Metrics**: 18, 14, 12, 11, 10 (Target: all ≤8)
**Risk Level**: HIGH - Multiple methods exceed V12 DNA threshold (CYC ≤15)

## Target Methods Analysis

### Method 1: SymmetryGuardReplaceExistingFollowerTarget
- Cyclomatic Complexity: 18
- V12 DNA Violation: YES (threshold: 15)
- Jane Street Alignment: CRITICAL - Exceeds cognitive simplicity threshold
- Primary Risk: Complex branching logic with multiple state transitions

**Complexity Breakdown**:
- Multiple conditional branches for follower state validation
- Nested logic for symmetry guard replacement
- State mutation patterns requiring lock-free Actor model conversion

### Method 2: SymmetryGuardTryResolveFollowersForDispatch
- Cyclomatic Complexity: 14
- V12 DNA Violation: NO (below threshold 15, but close)
- Jane Street Alignment: MARGINAL - Approaching cognitive complexity limit
- Secondary Risk: Dispatch resolution logic with multiple exit paths

**Additional Complexity Hotspots** (from file):
- Complexity 12: Likely helper method for follower resolution
- Complexity 11: State validation or guard logic
- Complexity 10: Supporting dispatch or replacement logic

## Blast Radius Assessment

### Direct Dependencies
**File**: src/V12_002.Symmetry.Replace.cs
- Part of Symmetry subsystem (V12_002 namespace)
- Likely called by order management and position tracking logic
- Interacts with follower order state machine

### Potential Impact Zones
1. Order Entry Flow: Symmetry guard replacement during order submission
2. Position Management: Follower order dispatch and tracking
3. State Synchronization: FSM/Actor state transitions for symmetry logic

### Risk Factors
- High Coupling: Symmetry logic is central to V12 order management
- State Mutation: Methods likely contain legacy lock() blocks (BANNED)
- Cognitive Load: CYC 18 makes race condition auditing difficult

## Call Hierarchy Analysis

### Inbound Callers (Who calls these methods?)
**Likely Callers**:
- Order entry handlers (OnBarUpdate, OnOrderUpdate)
- Position management logic
- Symmetry state machine transitions

**Risk**: Changes to these methods will ripple through order flow hot paths.

### Outbound Callees (What do these methods call?)
**Likely Callees**:
- Follower order creation/modification APIs
- State validation helpers
- Symmetry guard state accessors

**Risk**: Extraction must preserve call semantics to avoid breaking downstream logic.

## Refactoring Strategy

### Extraction Targets
1. SymmetryGuardReplaceExistingFollowerTarget (CYC 18 → ≤8):
   - Extract follower validation logic
   - Extract guard replacement logic
   - Extract state transition logic
   - Convert to FSM/Actor Enqueue pattern

2. SymmetryGuardTryResolveFollowersForDispatch (CYC 14 → ≤8):
   - Extract dispatch resolution logic
   - Extract follower lookup logic
   - Simplify exit path branching

### V12 DNA Compliance Checklist
- [ ] Remove all lock(stateLock) blocks → FSM/Actor Enqueue
- [ ] Ensure ASCII-only string literals (no Unicode/emoji)
- [ ] Apply Make illegal states unrepresentable pattern
- [ ] Verify atomic state transitions
- [ ] Add TDD tests for extracted methods

### Jane Street Alignment
- Cognitive Simplicity: Break CYC 18 into 3-4 methods with CYC ≤8 each
- Testability: Each extracted method should have <10 test cases for full coverage
- Race Condition Audit: Simpler methods = easier lock-free verification

## Risk Assessment: HIGH

### Critical Risks
1. Complexity Violation: CYC 18 is 20% above V12 DNA threshold (15)
2. Lock-Free Conversion: Likely contains legacy lock() blocks
3. Blast Radius: Central to order management hot path
4. Testing Gap: No existing TDD tests for these methods

### Mitigation Strategy
1. Phase 1 (Vision/Spec): Map current state machine logic
2. Phase 2 (Arch Planning): Design FSM/Actor extraction with Mermaid diagrams
3. Phase 3 (DNA Audit): Red team review for lock-free correctness
4. Phase 4 (Execution): Surgical extraction with mandatory checkpointing
5. Phase 5 (Verification): TDD test suite + F5 NinjaTrader validation

## Recommended Next Steps

1. Immediate: Run grep -r lock( src/V12_002.Symmetry.Replace.cs to confirm lock usage
2. Phase 1: Bob CLI (v12-engineer) dialogue to generate mini-spec.md
3. Phase 2: Generate implementation_plan.md with extraction sequence
4. Phase 3: Arena AI red team audit before surgery
5. Phase 4: Execute extraction with Bob CLI + Codex CLI backup

## Appendix: File Context

**File**: src/V12_002.Symmetry.Replace.cs
- Namespace: V12_002 (Core order management)
- Subsystem: Symmetry (Follower order coordination)
- Complexity Hotspots: 5 methods with CYC ≥10

**Related Files** (likely):
- src/V12_002.Symmetry.cs (Base symmetry logic)
- src/V12_002.OrderManagement.cs (Order entry/exit)
- src/V12_002.FSM.cs (State machine core)

**Analysis Completed**: 2026-06-14
**Analyst**: V12 Phase 0 Hotspot Analyzer
**Status**: Ready for Phase 1 (Vision/Spec)
