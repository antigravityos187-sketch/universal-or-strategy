# Phase 1.0: Scope Definition - EPIC-CCN-067

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: SymmetryFindDispatchForMasterFill
- File: src/V12_002.Symmetry.cs
- Line Range: 326-353 (28 lines)
- Current Complexity: 9
- Target Complexity: ≤8 (Jane Street strict standard)

### Extraction Strategy

Approach: Break into 2-3 helper methods to reduce cyclomatic complexity from 9 to ≤8.

Proposed Decomposition:

1. Extract Filter Predicate Method (CYC reduction: -4)
   - Name: IsValidDispatchCandidate
   - Purpose: Consolidate all early-exit filter conditions
   - Logic: Combine null/resolved check, direction check, trade type check, TTL check
   - Returns: bool indicating if context is a valid candidate
   - Complexity: 5 (4 conditions + base)

2. Extract Best Candidate Selection (CYC reduction: -2)
   - Name: SelectOldestCandidate
   - Purpose: Isolate best candidate selection logic
   - Logic: Compare timestamps and select oldest
   - Returns: Updated best candidate or current best
   - Complexity: 2 (OR condition + base)

3. Simplified Main Method (Remaining CYC: 3)
   - Loop iteration: +1
   - Call to IsValidDispatchCandidate: +1
   - Call to SelectOldestCandidate: +1
   - Base: 1
   - Total: 4 (well below threshold)

### Complexity Calculation

Before Extraction (CYC = 9):
- Base: 1
- foreach loop: +1
- if (ctx == null OR ctx.Anchor.IsResolved): +2
- if (ctx.Direction != direction): +1
- if (!string.Equals(...)): +1
- if (fillTimeUtc - ctx.CreatedUtc > SymmetryDispatchTtl): +1
- if (best == null OR ctx.CreatedUtc < best.CreatedUtc): +2

After Extraction (CYC = 4):
- Base: 1
- foreach loop: +1
- if (!IsValidDispatchCandidate(...)): +1
- best = SelectOldestCandidate(...): +1

## Boundary Definition

### IN SCOPE (ONLY)
- SymmetryFindDispatchForMasterFill method body (lines 326-353)
- Extract 2 helper methods within same class
- Maintain exact same behavior (pure refactoring)
- Preserve defensive copy pattern (ToArray())
- Keep method signature unchanged

### OUT OF SCOPE (STRICTLY FORBIDDEN)
- Callers: SymmetryOnExecutionUpdate (line 283)
- Callees: SymmetryNormalizeTradeType
- Other methods in V12_002.Symmetry.cs
- Dictionary state: symmetryDispatchById
- Constants: SymmetryDispatchTtl
- Any behavior changes or optimizations
- LINQ conversions or pattern changes
- Thread safety modifications

### No Scope Creep
- ONE EPIC = ONE CONCERN: Reduce complexity of single method
- No "While We're Here": Do not fix unrelated issues
- No Bundling: Do not combine with other refactoring tasks
- No Pre-existing Errors: Do not fix compilation errors outside scope

## Success Criteria

### Functional Requirements
1. Complexity reduced from 9 to ≤8 (target: 4)
2. All existing tests pass (zero regressions)
3. No behavior changes (pure refactoring)
4. Method signature unchanged
5. Defensive copy pattern preserved

### V12 DNA Compliance
1. Lock-free Actor/FSM pattern maintained (no locks introduced)
2. ASCII-only compliance (no Unicode in strings)
3. Pure query method (read-only, no side effects)
4. Thread safety via defensive copying preserved

### Quality Gates
1. Build passes: dotnet build (zero errors)
2. Tests pass: dotnet test (100% pass rate)
3. Lint clean: powershell -File .\scripts\lint.ps1 (zero violations)
4. Complexity audit: python3 scripts/complexity_audit.py (CYC ≤8)
5. Pre-push validation: powershell -File .\scripts\pre_push_validation.ps1 -Fast

### Documentation
1. XML doc comments for new helper methods
2. Inline comments preserved from original
3. No documentation changes outside method scope

## Risk Assessment

### Complexity Risk: MINIMAL
- Current State: CYC=9, already below V12 threshold (15)
- Refactoring Type: Simple extraction (low-risk pattern)
- Blast Radius: Single method, single file
- Rollback: Easy (single commit, clear scope)

### Jane Street Risk: ZERO
- P0 Violations: 0 (already compliant)
- Pattern Alignment: Pure query, defensive copying, early-exit
- Cognitive Load: Reduction from 9 to 4 improves readability

### Thread Safety Risk: ZERO
- No Lock Introduction: Extraction preserves lock-free pattern
- Defensive Copy: ToArray() pattern maintained
- Atomic Operations: Not applicable (read-only method)

## Verification Plan

### Pre-Extraction
1. Run complexity audit: Confirm CYC=9 baseline
2. Run tests: Establish green baseline
3. Snapshot file: Create restore point

### Post-Extraction
1. Run complexity audit: Verify CYC≤8 achieved
2. Run tests: Confirm zero regressions
3. Run build: Verify zero compilation errors
4. Run lint: Verify zero new violations
5. Manual review: Verify behavior preservation

### Rollback Trigger
- Any test failure
- Any compilation error
- Complexity not reduced to ≤8
- Behavior change detected

## Timeline Estimate

- Extraction: 15 minutes (straightforward decomposition)
- Testing: 5 minutes (existing test suite)
- Verification: 10 minutes (quality gates)
- Total: 30 minutes

## Approval Status

Status: PENDING BOUNDARY VALIDATION (Phase 1.5)

Next Step: Create 01-scope-boundary.md for V12.23 mandatory boundary check.
