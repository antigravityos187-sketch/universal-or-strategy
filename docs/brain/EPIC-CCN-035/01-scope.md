# Phase 1.0: Scope Definition - EPIC-CCN-035

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**: SyncLimitTarget
**File**: src/V12_002.Orders.Management.StopSync.cs
**Current Complexity**: 17
**Target Complexity**: ≤8 (Jane Street strict standard)
**Extraction Strategy**: Break into 2-3 helper methods

### Complexity Reduction Plan

**Current State**:
- Cyclomatic Complexity: 17
- Violation: +2 over V12 DNA threshold (15)
- Priority: HIGH

**Target State**:
- Cyclomatic Complexity: ≤8
- Compliance: Jane Street strict standard
- Method count: 1 parent + 2-3 extracted helpers

### Extraction Strategy

**Approach**: Surgical method decomposition
1. Extract Decision Logic: Separate conditional branches into focused methods
2. Isolate State Transitions: Move FSM state changes to dedicated handlers
3. Simplify Control Flow: Reduce nested conditionals
4. Preserve Atomicity: Maintain lock-free guarantees during extraction

**Expected Extractions** (2-3 methods):
- Helper 1: Validation/precondition checks (complexity ~3-4)
- Helper 2: State transition logic (complexity ~3-4)
- Helper 3: Post-processing/cleanup (complexity ~2-3)
- Parent: Orchestration only (complexity ~3-4)

## Boundary Definition

### IN SCOPE
- SyncLimitTarget method body ONLY
- Internal logic extraction
- Helper method creation within same class
- Complexity reduction from 17 to ≤8
- Lock-free Actor/FSM pattern preservation

### OUT OF SCOPE
- Callers: No changes to methods calling SyncLimitTarget
- Callees: No changes to methods called by SyncLimitTarget
- Other Methods: No changes to other methods in V12_002.Orders.Management.StopSync.cs
- File Structure: No changes to class structure, namespaces, or imports
- Behavior: No functional changes, pure refactoring only

### No Scope Creep Mandate

**ONE EPIC = ONE CONCERN**
- This EPIC addresses ONLY the complexity of SyncLimitTarget
- No "while we're here" improvements
- No fixing pre-existing compilation errors in other methods
- No bundling multiple concerns
- No architectural changes beyond method extraction

## Success Criteria

### Primary Goals
1. Complexity Reduced: SyncLimitTarget complexity drops from 17 to ≤8
2. All Tests Pass: Existing test suite passes without modification
3. No Behavior Changes: Functional equivalence verified
4. Lock-Free Pattern Maintained: FSM/Actor Enqueue model preserved

### Quality Gates
- Build: Zero compilation errors
- Tests: 100% pass rate (no new failures)
- Lint: Zero new Roslyn violations
- Complexity: Lizard/Codacy confirms ≤8 for all extracted methods
- ASCII-Only: No Unicode/emoji in string literals

### Verification Steps
1. Run dotnet build - must succeed
2. Run dotnet test - must pass all tests
3. Run python3 scripts/complexity_audit.py - verify ≤8
4. Run powershell -File .\scripts\lint.ps1 - zero new violations
5. Run powershell -File .\deploy-sync.ps1 - hard-link sync

## Risk Assessment

**Overall Risk**: MEDIUM-HIGH

**Risk Factors**:
- Manageable complexity (17, not extreme)
- Critical path (order management subsystem)
- Synchronization logic (requires careful handling)
- Clear extraction candidates (decision branches)

**Mitigation Strategy**:
1. TDD Approach: Write tests before extraction
2. Incremental Extraction: One helper at a time
3. Continuous Verification: Build + test after each extraction
4. FSM Compliance: Verify Actor pattern at each step
5. Atomic Operations: Maintain lock-free guarantees

## Dependencies

**Upstream**: Phase 0 (Hotspot Analysis) - COMPLETE
**Downstream**: Phase 2 (Implementation Planning) - PENDING

**Blockers**: None

---
**Document Version**: 1.0
**Created**: 2026-06-15
**Status**: READY FOR PHASE 1.5 (Boundary Validation)
