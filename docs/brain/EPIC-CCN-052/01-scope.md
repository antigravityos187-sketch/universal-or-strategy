# Phase 1.0: Scope Definition - EPIC-CCN-052

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**: CleanupStalePendingReplacements
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Current Complexity**: 9
- **Target Complexity**: <=8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

## Boundary Definition

### IN SCOPE
- **Method Body Only**: CleanupStalePendingReplacements implementation
- **Complexity Reduction**: Extract logical blocks into helper methods
- **Pattern Preservation**: Maintain lock-free Actor/FSM pattern
- **ASCII Compliance**: Verify all string literals are ASCII-only

### OUT OF SCOPE
- Callers: No changes to methods that call CleanupStalePendingReplacements
- Callees: No changes to methods called by CleanupStalePendingReplacements
- Other Methods: No changes to other methods in V12_002.Trailing.StopUpdate.cs
- Pre-existing Issues: No fixing compilation errors outside this method
- Scope Creep: No "while we're here" improvements

### NO SCOPE CREEP MANDATE
**ONE EPIC = ONE CONCERN**
- This epic addresses ONLY the complexity of CleanupStalePendingReplacements
- Any other concerns discovered during analysis must be logged as separate epics
- No bundling of multiple refactoring concerns

## Success Criteria

### Functional Requirements
1. Complexity Reduced: From 9 to <=8
2. All Tests Pass: Zero test failures
3. No Behavior Changes: Identical runtime behavior
4. Lock-Free Pattern: Actor/FSM pattern maintained

### Quality Gates
1. Build Success: dotnet build passes
2. Lint Clean: powershell -File .\scripts\lint.ps1 passes
3. Format Check: dotnet csharpier check src/ passes
4. ASCII Compliance: Zero non-ASCII characters

### V12 DNA Compliance
1. No Locks: Zero lock(...) statements introduced
2. Atomic Operations: Use Interlocked or Actor pattern only
3. Correctness by Construction: Make illegal states unrepresentable
4. Jane Street Alignment: Cognitive simplicity over clever abstractions

## Risk Assessment

### Low Risk Factors
- **Complexity**: 9 (below MEDIUM threshold of 15)
- **Jane Street Violations**: 0
- **Blast Radius**: TBD (requires jCodemunch analysis)

### Mitigation Strategy
1. **Checkpointing**: Enabled via Bob CLI
2. **Incremental Extraction**: One helper at a time
3. **Test After Each Step**: Verify behavior preservation
4. **Rollback Plan**: Use /restore if issues arise

## Next Steps

1. **Phase 1.5**: Boundary Validation (V12.23 Protocol - MANDATORY)
2. **Phase 2**: Implementation Planning
3. **Phase 3**: DNA & PR Audit
4. **Phase 4**: Surgical Extraction
5. **Phase 5**: Verification & Sign-off
