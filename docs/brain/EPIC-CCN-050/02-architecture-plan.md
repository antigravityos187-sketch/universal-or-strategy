# Phase 2: Architecture Planning - EPIC-CCN-050

## V12.23 Protocol: Single-Method Extraction Architecture

This document defines the architectural plan for extracting helper methods from FleetSync_SyncFollowersToLevel to reduce cyclomatic complexity from 9 to 8 or less.

## Target Method Analysis

### Current State
- Method: FleetSync_SyncFollowersToLevel
- File: src/V12_002.Trailing.cs
- Line: 142
- Complexity: 9 (CYC)
- LOC: 34
- Tier: 2 (Medium complexity)

### Method Signature
private void FleetSync_SyncFollowersToLevel(
    KeyValuePair<string, PositionInfo>[] positionSnapshot,
    int leaderLongMaxLevel,
    int leaderShortMaxLevel
)

### Current Logic Flow
1. Iterate through position snapshot
2. Validation Block (4 conditions):
   - Check if follower
   - Check if entry filled and bracket submitted
   - Check if active position exists
   - Calculate target level based on direction
3. Guard Conditions (2 conditions):
   - Skip if no leader exists (targetLevel == 0)
   - Skip if follower already at higher level
4. Stop Price Calculation:
   - Calculate new stop price for target level
5. Better Stop Validation (1 condition):
   - Determine if new stop is more protective
6. Update Block:
   - Update stop order
   - Log sync action

## Extraction Strategy

### Complexity Reduction Target
- Current: CYC 9
- Target: CYC 8 or less (Jane Street strict standard)
- Reduction Required: At least 1 point

### Proposed Helper Methods

#### Helper 1: ShouldSyncFollower
Purpose: Consolidate validation logic to reduce branching in main method.

Signature:
private bool ShouldSyncFollower(
    PositionInfo follower,
    string entryName,
    int targetLevel
)

Responsibility:
- Validate follower eligibility (IsFollower, EntryFilled, BracketSubmitted)
- Check active position existence
- Validate target level (not zero)
- Validate current level (not already at or above target)

Returns: true if follower should be synced, false otherwise

Complexity: CYC 5 (4 validation conditions + 1 base)

#### Helper 2: IsStopPriceImprovement
Purpose: Encapsulate better stop logic for clarity and testability.

Signature:
private bool IsStopPriceImprovement(
    PositionInfo follower,
    double newStopPrice
)

Responsibility:
- Compare new stop price against current stop price
- Apply direction-specific logic (Long: higher is better, Short: lower is better)

Returns: true if new stop price is more protective, false otherwise

Complexity: CYC 2 (1 ternary condition + 1 base)

### Refactored Method Structure

New Complexity: CYC 4 (1 ternary + 2 if statements + 1 base) - TARGET MET

## Call Graph

FleetSync_SyncFollowersToLevel (CYC 4)
├── ShouldSyncFollower (CYC 5)
│   ├── Validates follower state
│   ├── Checks active position
│   ├── Validates target level
│   └── Validates current level
├── CalculateStopForLevel (existing, unchanged)
├── IsStopPriceImprovement (CYC 2)
│   └── Compares stop prices by direction
└── UpdateStopOrder (existing, unchanged)

### Data Flow
1. Input: Position snapshot, leader max levels
2. Per Follower:
   - Calculate target level (inline)
   - Validate via ShouldSyncFollower returns bool
   - Calculate sync stop price via CalculateStopForLevel (existing)
   - Validate improvement via IsStopPriceImprovement returns bool
   - Update via UpdateStopOrder (existing)
3. Output: Side effects (stop order updates, logging)

### Shared State
- Read-Only Access:
  - activePositions dictionary (via ShouldSyncFollower)
  - follower.CurrentStopPrice (via IsStopPriceImprovement)
  - follower.CurrentTrailLevel (via ShouldSyncFollower)
- No Shared Mutable State: All helpers are pure validation functions

## Lock-Free Validation

### Compliance Checklist
- No lock() statements: Method uses read-only iteration over snapshot
- FSM/Actor Pattern: Method called within Actor Enqueue context (inherited from caller)
- Atomic Primitives: No explicit atomics needed (snapshot-based read)
- Immutable Snapshot: positionSnapshot is array copy, not live dictionary

### Concurrency Safety
- Snapshot Isolation: Method operates on KeyValuePair array snapshot
- No Race Conditions: All reads are from immutable snapshot
- Side Effects: UpdateStopOrder is existing method (assumed safe by caller context)

## Jane Street Compliance

### Cognitive Simplicity (CYC 8 or less)
- Main Method: CYC 4 (well below threshold)
- Helper 1: CYC 5 (below threshold)
- Helper 2: CYC 2 (trivial)
- Total Reduction: 9 to 4 (56% reduction)

### Microsecond-Latency Constraints
- No Algorithmic Changes: Pure extraction, behavior preserved
- No Performance Regression: Inline candidates for JIT optimization
- Hot Path Optimization: Validation short-circuits early (fail-fast)

### Testing Standards
- Exhaustive Testing Feasible: Each helper has clear input/output contract
- Unit Test Targets:
  - ShouldSyncFollower: 5 test cases (each validation condition + happy path)
  - IsStopPriceImprovement: 4 test cases (Long better/worse, Short better/worse)
  - FleetSync_SyncFollowersToLevel: Integration test (existing behavior preserved)

### Jane Street KB Insights
Query Result: testing document found (will_wilson_why_testing_hard_2026)

Relevant Principles:
- Testability: Extracted helpers are pure functions (deterministic, no side effects)
- Isolation: Each helper can be tested independently
- Clarity: Method names describe intent (ShouldSyncFollower, IsStopPriceImprovement)

## Implementation Constraints

### Scope Boundary (V12.23 Protocol)
- Single Method: Only FleetSync_SyncFollowersToLevel modified
- No Caller Changes: Signature unchanged, behavior preserved
- No Callee Changes: CalculateStopForLevel, UpdateStopOrder unchanged
- No Sibling Changes: Other methods in V12_002.Trailing.cs untouched

### Access Modifiers
- Main Method: private (unchanged)
- Helper 1: private (internal to class)
- Helper 2: private (internal to class)

### Parameter Types
- Existing Types: PositionInfo, MarketPosition, string, int, double
- No New Types: No enums, structs, or classes introduced
- No Breaking Changes: All types already in use

## Verification Criteria

### Pre-Extraction Baseline
- Complexity audit: Confirm CYC 9 for FleetSync_SyncFollowersToLevel
- Build: Confirm zero compilation errors
- Tests: Confirm 100% pass rate (baseline)

### Post-Extraction Validation
- Complexity audit: Confirm CYC 8 or less for all methods
- Build: Confirm zero compilation errors
- Tests: Confirm 100% pass rate (no regressions)
- Diff audit: Confirm only FleetSync_SyncFollowersToLevel modified
- Lock-free audit: Confirm zero lock() statements in modified code

### Success Criteria
1. Complexity: Main method CYC 8 or less (Target: 4)
2. Behavior: Existing tests pass (no regressions)
3. Scope: Only target method modified (no scope creep)
4. Lock-Free: No lock() statements introduced
5. Jane Street: Cognitive simplicity maintained (CYC 8 or less)

## Risk Assessment

### Low Risk Factors
- Pure extraction (no algorithmic changes)
- Clear validation boundaries
- Existing test coverage (FSMActorTests.cs)
- Single-method scope (isolated blast radius)

### Mitigation Strategies
- Regression Risk: Run full test suite before/after
- Performance Risk: Benchmark hot path (if applicable)
- Logic Risk: Manual code review of extracted helpers

## Next Steps

With Phase 2 architecture planning APPROVED, proceed to:
1. Phase 3: DNA & PR Audit (Arena AI red team review)
2. Phase 4: Recursive Execution (Bob CLI v12-engineer mode)
3. Phase 5: Verification/Review (compare against this plan)
4. Phase 6: Sign-off (deploy-sync.ps1 + F5 test)

---

Approval Status: READY FOR PHASE 3 REVIEW
Architect: Bob Shell (Plan Mode)
Date: 2026-06-15
Epic: EPIC-CCN-050
