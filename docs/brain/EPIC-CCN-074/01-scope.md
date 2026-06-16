# Phase 1.0: Scope Definition - EPIC-CCN-074

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: `AttachExecutionPanelHandlers`
- **File**: `src/V12_002.UI.Panel.Handlers.cs`
- **Current Complexity**: 12 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

### Complexity Reduction Plan

**Current State**:
- CYC = 12 (MEDIUM risk, approaching HIGH threshold of 15)
- Method likely contains multiple handler attachment operations
- Each handler attachment adds conditional logic (if/null checks)

**Target State**:
- CYC ≤ 8 (Jane Street cognitive simplicity standard)
- Main method orchestrates handler attachments
- Helper methods encapsulate individual handler logic

**Extraction Strategy**:
1. **Identify Handler Groups**: Group related event handler attachments
2. **Extract Helper Methods**: Create 2-3 methods like:
   - `AttachPrimaryExecutionHandlers()` - Core execution events
   - `AttachSecondaryExecutionHandlers()` - Supporting events
   - `AttachExecutionStateHandlers()` - State change events
3. **Maintain Atomicity**: Each helper method is atomic, no shared state
4. **Preserve Lock-Free Pattern**: No locks, use Actor/FSM Enqueue model

## Boundary Definition

### IN SCOPE ✅
- **Method Body Only**: `AttachExecutionPanelHandlers` implementation
- **Internal Logic**: Event handler attachment code within method
- **Helper Method Creation**: New private methods for extracted logic
- **Complexity Reduction**: Refactor to achieve CYC ≤8

### OUT OF SCOPE ❌
- **Callers**: Methods that invoke `AttachExecutionPanelHandlers`
- **Callees**: Event handler implementations being attached
- **Other Methods**: Any other method in `V12_002.UI.Panel.Handlers.cs`
- **File-Wide Changes**: No changes to class structure, fields, or properties
- **Pre-existing Issues**: No fixing unrelated compilation errors
- **Scope Creep**: No "while we're here" improvements

### No Scope Creep Enforcement
- **ONE EPIC = ONE CONCERN**: This epic ONLY reduces complexity of `AttachExecutionPanelHandlers`
- **No Bundling**: Do not combine with other refactoring tasks
- **No Side Quests**: Do not fix unrelated issues discovered during analysis

## Success Criteria

### Functional Requirements
1. ✅ **Complexity Reduced**: CYC reduced from 12 to ≤8
2. ✅ **All Tests Pass**: Existing test suite passes without modification
3. ✅ **No Behavior Changes**: Identical runtime behavior before/after
4. ✅ **Lock-Free Pattern**: Actor/FSM Enqueue model maintained (no locks)

### Quality Requirements
1. ✅ **ASCII-Only**: No Unicode, emoji, or curly quotes in code
2. ✅ **Build Success**: `dotnet build` completes with zero errors
3. ✅ **Lint Clean**: `powershell -File .\scripts\lint.ps1` passes
4. ✅ **Format Clean**: `dotnet csharpier check src/` passes

### Documentation Requirements
1. ✅ **Code Comments**: Helper methods have XML doc comments
2. ✅ **Extraction Rationale**: Comments explain why logic was extracted
3. ✅ **Complexity Metrics**: Document before/after CYC values

## Risk Assessment

### Low Risk Factors
- **Single Method**: Isolated change, minimal blast radius
- **No API Changes**: Method signature unchanged
- **No State Changes**: No modifications to class fields/properties
- **UI Handler Logic**: Well-understood event attachment patterns

### Mitigation Strategies
1. **Checkpoint Before**: Create restore point before any changes
2. **Incremental Extraction**: Extract one helper at a time, test after each
3. **Regression Testing**: Run full test suite after each extraction
4. **Manual Verification**: F5 in NinjaTrader to verify UI behavior

## Phase 1.0 Status
- **Status**: COMPLETED
- **Date**: 2026-06-15
- **Analyst**: Bob Shell (v12-engineer mode)
- **Next Phase**: Phase 1.5 (Boundary Validation)
