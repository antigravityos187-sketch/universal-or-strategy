# Phase 1.0: Scope Definition - EPIC-CCN-041

## Target Method
- **Method**: SymmetryGuardPruneDispatches
- **File**: src/V12_002.Symmetry.Replace.cs
- **Current Complexity**: 10
- **Target Complexity**: ≤8 (Jane Street strict standard)

## Extraction Scope (SINGLE METHOD ONLY)

### What's IN Scope
1. **Method Body**: SymmetryGuardPruneDispatches implementation only
2. **Extraction Strategy**: Break into 2-3 helper methods
   - Extract conditional logic blocks into focused helper methods
   - Maintain single responsibility per extracted method
   - Preserve lock-free Actor/FSM pattern
3. **Complexity Reduction**: From CYC=10 to CYC≤8

### What's OUT of Scope
1. **Callers**: No modifications to methods that call SymmetryGuardPruneDispatches
2. **Callees**: No modifications to methods called by SymmetryGuardPruneDispatches
3. **Other Methods**: No changes to other methods in V12_002.Symmetry.Replace.cs
4. **Pre-existing Issues**: No fixing of unrelated compilation errors
5. **Scope Creep**: No "while we're here" improvements

## Boundary Definition

### Single Concern Principle
- **ONE EPIC = ONE CONCERN**: This epic focuses exclusively on reducing the cyclomatic complexity of SymmetryGuardPruneDispatches
- **No Bundling**: No combining with other refactoring tasks
- **No Side Quests**: No fixing adjacent code issues

### Extraction Strategy Details
1. **Identify Decision Points**: Analyze the 10 decision points in the method
2. **Group Related Logic**: Cluster related conditional branches
3. **Extract Helpers**: Create 2-3 private helper methods with clear names
4. **Maintain Atomicity**: Preserve lock-free atomic operations
5. **Preserve Behavior**: Zero functional changes

## Success Criteria

### Mandatory Requirements
1. ✅ **Complexity Reduction**: CYC reduced from 10 to ≤8
2. ✅ **All Tests Pass**: Zero test failures
3. ✅ **No Behavior Changes**: Identical runtime behavior
4. ✅ **Lock-Free Pattern**: Actor/FSM Enqueue model maintained
5. ✅ **ASCII-Only**: No Unicode characters introduced
6. ✅ **Build Success**: Zero compilation errors

### Quality Gates
1. **Pre-Push Validation**: All 13 checks pass
2. **CSharpier Formatting**: Zero formatting issues
3. **Complexity Audit**: complexity_audit.py confirms CYC≤8
4. **Hard-Link Sync**: deploy-sync.ps1 succeeds

## Risk Mitigation

### Low-Risk Factors
- CYC=10 is below V12 threshold (15), indicating manageable complexity
- Single-method scope limits blast radius
- No changes to callers/callees reduces integration risk

### Mitigation Strategies
1. **Manual Code Inspection**: Review method body before extraction
2. **Incremental Extraction**: Extract one helper at a time
3. **Test After Each Step**: Verify tests pass after each extraction
4. **Checkpoint Frequently**: Use Bob CLI checkpointing

## Jane Street Alignment

### Cognitive Simplicity Principles
- Functions with CYC>8 are harder to reason about under microsecond latency constraints
- Single-method extraction maintains focus and testability
- Helper methods should have clear, single-purpose names
- Avoid clever abstractions - prefer explicit, simple logic

### V12 DNA Compliance
- **Correctness by Construction**: Structure code so invalid states are unrepresentable
- **Lock-Free Actor Pattern**: Maintain FSM/Actor Enqueue model
- **ASCII-Only**: No Unicode in string literals
- **Surgical Changes**: Touch only what's necessary

---
**Generated**: 2026-06-15 (Phase 1.0 Scope Definition)
**Status**: READY FOR PHASE 1.5 (Boundary Validation)
