# Phase 1.0: Scope Definition - EPIC-CCN-046

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: HandleChartClick_ConvertPrice
- **File**: src/V12_002.UI.Callbacks.cs
- **Current Complexity**: 9
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

### Complexity Reduction Plan

**Current State**:
- 9 decision points across price conversion logic
- Conditional statements for price conversion
- State validation checks
- Error handling branches
- UI callback coordination

**Target State**:
- Main method: ≤5 decision points (orchestration only)
- Helper method 1: Price validation logic (≤3 decision points)
- Helper method 2: Conversion calculation (≤3 decision points)
- Helper method 3: UI update coordination (≤2 decision points)

**Extraction Strategy**:
1. Extract price validation logic into `ValidatePriceForConversion()`
2. Extract conversion calculation into `CalculateConvertedPrice()`
3. Extract UI update logic into `UpdateChartWithConvertedPrice()`
4. Main method becomes orchestrator: validate → calculate → update

## Boundary Definition

### IN SCOPE (SINGLE METHOD ONLY)
- ✅ HandleChartClick_ConvertPrice method body
- ✅ Internal logic extraction into private helper methods
- ✅ Complexity reduction from 9 to ≤8
- ✅ Maintaining lock-free Actor/FSM pattern
- ✅ Preserving exact behavior (no logic changes)

### OUT OF SCOPE (STRICT BOUNDARY)
- ❌ Callers of HandleChartClick_ConvertPrice
- ❌ Callees (downstream methods)
- ❌ Other methods in V12_002.UI.Callbacks.cs
- ❌ Chart rendering logic
- ❌ Price calculation utilities
- ❌ State management infrastructure
- ❌ Event dispatcher system
- ❌ Pre-existing compilation errors
- ❌ "While we're here" improvements

### No Scope Creep Mandate
**ONE EPIC = ONE CONCERN**
- This EPIC addresses ONLY HandleChartClick_ConvertPrice complexity
- No bundling of multiple refactoring concerns
- No fixing unrelated issues discovered during extraction
- No architectural changes beyond method extraction

## Success Criteria

### Functional Requirements
1. ✅ Complexity reduced from 9 to ≤8
2. ✅ All existing tests pass (if tests exist)
3. ✅ No behavior changes (bit-for-bit identical output)
4. ✅ Lock-free Actor/FSM pattern maintained
5. ✅ ASCII-only compliance verified

### Technical Requirements
1. ✅ Helper methods are private (encapsulation)
2. ✅ Method names follow V12 naming conventions
3. ✅ No new dependencies introduced
4. ✅ No performance regression
5. ✅ CSharpier formatting compliance

### Quality Gates
1. ✅ `dotnet build` succeeds (zero errors)
2. ✅ `dotnet test` passes (100% pass rate)
3. ✅ `dotnet csharpier check src/` passes
4. ✅ `python scripts/complexity_audit.py` shows CYC ≤8
5. ✅ `powershell -File .\scripts\pre_push_validation.ps1 -Fast` passes

### V12 DNA Compliance
1. ✅ No `lock()` statements introduced
2. ✅ No Unicode/emoji in string literals
3. ✅ Curly braces on all control structures
4. ✅ No whitespace mutation in unrelated lines
5. ✅ Hard-link sync via `deploy-sync.ps1`

## Risk Assessment

### Risk Level: LOW
**Rationale**:
- Single method extraction (minimal blast radius)
- Method already below V12 threshold (9 < 15)
- UI callback layer (isolated from core logic)
- No lock-based concurrency detected
- Standard event handler pattern

### Mitigation Strategy
1. **Checkpoint before extraction**: Git commit before changes
2. **Incremental extraction**: One helper method at a time
3. **Test after each extraction**: Verify behavior preservation
4. **Rollback plan**: Git reset if tests fail

## Implementation Constraints

### Jane Street Alignment
- **Cognitive Simplicity**: Each helper method should have single, clear purpose
- **Verifiable Logic**: Simple enough to reason about under latency constraints
- **Test Exhaustiveness**: All code paths must be testable
- **Race Condition Audit**: No new concurrency risks

### V12 Protocol Compliance
- **Phase 6 Recursive Protocol**: Follow Stage 0-6 workflow
- **Pre-Push Validation**: Run all 13 quality gates
- **Branch Strategy**: Use `feature/epic-ccn-046` branch
- **PR Hygiene**: Diff <10k characters, rebase on main

## Approval Status

**Phase 1.0 Status**: READY FOR BOUNDARY VALIDATION
**Next Step**: Create 01-scope-boundary.md (Phase 1.5)

---

**Document Version**: 1.0
**Created**: 2026-06-15
**Protocol**: V12.23 Phase 1.0
