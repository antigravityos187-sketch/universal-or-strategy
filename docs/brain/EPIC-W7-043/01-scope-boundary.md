# Phase 1: Scope Boundary - EPIC-W7-043

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: N/A
- **Execution Time**: 2026-06-24T01:31:01Z

## Epic Target
- **Method**: SymmetryGuardSubmitFollowerBracket
- **File**: src/V12_002.Symmetry.Follower.cs
- **Line**: 285
- **Current CYC**: 16
- **Target CYC**: <=8
- **Reduction Required**: 8 decision points

## IN SCOPE

### Primary Extraction Target
**SymmetryGuardSubmitFollowerBracket** (CYC=16, 141 lines, nesting=5)

### Extraction Strategy
Based on hotspot analysis showing 34 callees and deep nesting:

1. **Validation Logic Extraction** (Priority 1)
   - Extract validation calls (ValidateStopPrice, Validate_LongIsIllegalAdjust, Validate_ShortIsIllegalAdjust)
   - Consolidate into single validation method
   - Expected CYC reduction: 3-4 points

2. **Position Info Queries** (Priority 2)
   - Extract position info calls (GetTargetContracts, IsRunnerTarget, GetTargetPrice, GetTargetMode)
   - Create position info aggregator method
   - Expected CYC reduction: 2-3 points

3. **Conditional Flattening** (Priority 3)
   - Flatten 5-level nesting using early returns
   - Convert nested if/else to guard clauses
   - Expected CYC reduction: 2-3 points

### Scope Boundaries
- **File**: src/V12_002.Symmetry.Follower.cs ONLY
- **Method**: SymmetryGuardSubmitFollowerBracket ONLY
- **Callers**: No changes to callers (SymmetryGuardOnFollowerFill, SymmetryGuardTryResolveFollower, SymmetryGuardProcessPendingFollowerFills)
- **Callees**: No changes to downstream methods (34 callees remain unchanged)

### Success Criteria
- SymmetryGuardSubmitFollowerBracket CYC <=8
- All 3 callers continue to work unchanged
- All 34 callees continue to work unchanged
- Zero compilation errors
- Zero test failures
- F5 in NinjaTrader successful

## OUT OF SCOPE

### Explicitly Excluded
1. **Caller Methods** (3 methods)
   - SymmetryGuardOnFollowerFill (line 17)
   - SymmetryGuardTryResolveFollower (line 129)
   - SymmetryGuardProcessPendingFollowerFills (line 97)
   - **Rationale**: Blast radius analysis shows these are separate concerns

2. **Callee Methods** (34 methods)
   - All validation methods (ValidateStopPrice, etc.)
   - All position info methods (GetTargetContracts, etc.)
   - All symmetry methods (SymmetryTrim)
   - All logging methods (LogBuffer.*)
   - All actor methods (Enqueue, IsActorThread, etc.)
   - All UI methods (GetTargetOrdersDictionary)
   - **Rationale**: These are stable, tested, and used elsewhere

3. **Other Symmetry.Follower Methods**
   - Any method not directly called by SymmetryGuardSubmitFollowerBracket
   - **Rationale**: Separate epic scope

4. **Cross-File Changes**
   - No changes to other V12_002.*.cs files
   - No changes to test files
   - **Rationale**: Isolated refactoring

### Scope Creep Prevention
- **ONE EPIC = ONE METHOD**: SymmetryGuardSubmitFollowerBracket ONLY
- **NO "While We're Here" Fixes**: Pre-existing issues in other methods are OUT OF SCOPE
- **NO Caller/Callee Modifications**: Only extract logic within target method
- **NO Cross-Module Changes**: Symmetry.Follower module boundary is hard limit

## Risk Mitigation

### Low Blast Radius Advantage
- **0 external dependents**: Changes are fully isolated
- **3 internal callers**: Easy to verify behavior preservation
- **34 callees unchanged**: No downstream impact

### Complexity Reduction Strategy
1. **Phase 1**: Extract validation logic (CYC 16->12)
2. **Phase 2**: Extract position info queries (CYC 12->9)
3. **Phase 3**: Flatten conditionals (CYC 9->8)

### Verification Gates
- After each extraction: dotnet build (must pass)
- After each extraction: deploy-sync.ps1 (must succeed)
- After all extractions: F5 in NinjaTrader (must load)

## Jane Street Alignment

### Current State
- **CYC**: 16 (FAILS Jane Street standard <=8)
- **Nesting**: 5 (FAILS Jane Street standard <=3)
- **Lines**: 141 (ACCEPTABLE but high)

### Target State
- **CYC**: <=8 (MEETS Jane Street HFT standard)
- **Nesting**: <=3 (MEETS Jane Street standard)
- **Lines**: <100 per method (MEETS Jane Street guideline)

### Extraction Principles
- **Correctness by Construction**: Extract to make illegal states unrepresentable
- **Single Responsibility**: Each extracted method does ONE thing
- **Cognitive Simplicity**: Each method fits in working memory (<=8 decision points)

## Dependencies

### Prerequisites
- jCodemunch index current (verified in Phase 0)
- Git status clean (no uncommitted src/ changes)
- Build passes (dotnet build)
- GitButler virtual branch active

### Artifacts Required
- 00-hotspots.md (completed in Phase 0)
- This file: 01-scope-boundary.md

### Next Phase Input
- This scope boundary document feeds Phase 2 (Architecture Planning)

## Approval Status
- **Scope Defined**: YES
- **Boundaries Clear**: YES
- **Risk Assessed**: YES
- **Ready for Phase 2**: YES
