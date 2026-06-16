# Phase 1.0: Scope Definition - EPIC-CCN-062

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**:
- **Method Name**: ProcessFleetSlot
- **File**: src/V12_002.SIMA.Fleet.cs
- **Current Complexity**: 11 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 focused helper methods

## Complexity Analysis

**Current State**:
- CYC = 11 (73% of V12 threshold of 15)
- Status: Below threshold but approaching warning zone
- Risk: MEDIUM - preventive refactoring recommended

**Target State**:
- CYC ≤ 8 per method (Jane Street cognitive simplicity standard)
- Total complexity budget: ~11 (distributed across extracted methods)
- Expected breakdown: Main method (5-6) + 2-3 helpers (2-3 each)

## Boundary Definition

### IN SCOPE
- **ProcessFleetSlot method body only**
- Extract 2-3 helper methods from conditional branches
- Maintain existing method signature
- Preserve all existing behavior
- Keep lock-free Actor/FSM pattern intact

### OUT OF SCOPE
- Callers of ProcessFleetSlot (no changes)
- Callees invoked by ProcessFleetSlot (no changes)
- Other methods in V12_002.SIMA.Fleet.cs (no changes)
- Pre-existing compilation errors (not our concern)
- Performance optimizations (separate epic)
- Logging improvements (separate epic)
- Variable naming refactors (separate epic)

### NO SCOPE CREEP
- **ONE EPIC = ONE CONCERN**: Complexity reduction only
- No "while we're here" improvements
- No bundling multiple refactoring concerns
- No fixing unrelated issues

## Success Criteria

### Functional Requirements
1. All existing tests pass (zero regressions)
2. No behavior changes (pure refactoring)
3. Method signature unchanged (API compatibility)
4. Lock-free Actor/FSM pattern maintained

### Quality Requirements
1. ProcessFleetSlot complexity reduced from 11 to ≤8
2. Each extracted helper method has CYC ≤8
3. Total complexity budget maintained (~11 distributed)
4. ASCII-only compliance (no Unicode)

### Process Requirements
1. Pre-push validation passes (all 13 checks)
2. CSharpier formatting applied
3. Complexity audit confirms ≤8 threshold
4. Build succeeds with zero errors

## Extraction Strategy

### Approach
- **Pattern**: Extract conditional branches into focused helper methods
- **Naming**: Use descriptive names that reflect business logic
- **Cohesion**: Each helper should have single responsibility
- **Coupling**: Minimize parameter passing (use class state where appropriate)

### Expected Helpers (2-3 methods)
1. Helper for primary conditional branch (CYC 2-3)
2. Helper for secondary conditional branch (CYC 2-3)
3. Optional third helper if needed (CYC 2-3)

### Verification
- Run `python3 scripts/complexity_audit.py` after extraction
- Confirm ProcessFleetSlot shows CYC ≤8
- Confirm all helpers show CYC ≤8

## Jane Street Alignment

**Cognitive Simplicity Principle**:
- Functions with CYC >8 are harder to reason about under microsecond latency constraints
- Simple, verifiable logic enables exhaustive testing
- "Make illegal states unrepresentable" requires decomposed logic

**HFT Context**:
- SIMA.Fleet is in critical path (fleet management)
- Complexity reduction improves audit-ability for race conditions
- Simpler methods = faster code review cycles

## Risk Mitigation

**Low Risk Refactoring**:
- Pure extraction (no logic changes)
- Existing tests provide safety net
- Complexity is moderate (not a God-function)
- Single-method scope limits blast radius

**Rollback Plan**:
- Git restore point before changes
- Bob CLI checkpointing enabled
- Can revert via `git reset --hard` if needed

## Timeline Estimate

- **Phase 1 (Scope)**: 15 minutes (this document)
- **Phase 2 (Planning)**: 30 minutes (implementation plan)
- **Phase 3 (Audit)**: 15 minutes (Arena AI review)
- **Phase 4 (Execution)**: 45 minutes (extraction + tests)
- **Phase 5 (Verification)**: 30 minutes (validation)
- **Total**: ~2.5 hours

## Approval Gate

**Status**: PENDING (awaiting Phase 1.5 boundary validation)

**Next Step**: Create `01-scope-boundary.md` for V12.23 mandatory boundary check.
