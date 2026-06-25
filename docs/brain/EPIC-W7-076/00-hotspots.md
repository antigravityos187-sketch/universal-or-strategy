# Phase 0: Hotspot Analysis - EPIC-W7-076

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.74
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:48:51Z

## Target Method
- **Method**: CollapseAllExecutionControls
- **File**: src/V12_002.UI.Panel.Handlers.cs
- **Line**: 665
- **Cyclomatic Complexity**: 11
- **Symbol ID**: src/V12_002.UI.Panel.Handlers.cs::V12_002.CollapseAllExecutionControls#method

## Complexity Metrics
- **Cyclomatic Complexity**: 11 (HIGH - exceeds Jane Street threshold of 8)
- **Max Nesting Depth**: 1 (LOW)
- **Parameter Count**: 0 (LOW)
- **Lines of Code**: 23
- **Assessment**: HIGH complexity

### Analysis
The method has a cyclomatic complexity of 11, which exceeds the V12 DNA mandate of CYC ≤ 8 (Jane Street strict standard). This indicates the method contains multiple conditional branches that should be extracted into smaller, single-purpose helper methods.

The low nesting depth (1) and zero parameters suggest the complexity comes from sequential conditional logic rather than deeply nested structures, making it a good candidate for extraction.

## Blast Radius
- **Direct Dependents**: 0
- **Importer Count**: 0
- **Overall Risk Score**: 0.0 (ISOLATED)
- **Confirmed Impact Files**: 0
- **Potential Impact Files**: 0

### Analysis
The blast radius analysis shows this method is **completely isolated** with no external dependencies. This is an **ideal refactoring target** because:
- No other files import or depend on this method
- Changes will not ripple through the codebase
- Low risk of breaking external contracts
- Can be refactored independently

## Call Hierarchy

### Callers (2 total)
1. **UpdateContextualUI** (depth 1)
   - File: src/V12_002.UI.Panel.Handlers.cs
   - Line: 654
   - Resolution: ast_resolved

2. **SelectConfigMode** (depth 2)
   - File: src/V12_002.UI.Panel.Handlers.cs
   - Line: 591
   - Resolution: ast_resolved

### Callees (0 total)
This method does not call any other methods - it appears to be a leaf node in the call graph.

### Analysis
The method is called by 2 internal methods within the same file (UI.Panel.Handlers.cs):
- **UpdateContextualUI**: Direct caller at depth 1
- **SelectConfigMode**: Indirect caller at depth 2

Both callers are in the same file, which means refactoring will be contained within a single compilation unit. The method makes no outbound calls, suggesting it performs direct UI manipulation without delegating to helper methods.

## Risk Assessment

### Overall Risk: **LOW**

**Justification**:
1. ✅ **Isolated Blast Radius**: Zero external dependencies
2. ✅ **Same-File Callers**: Both callers in same file (easy to update)
3. ✅ **No Callees**: Leaf node - no downstream impact
4. ⚠️ **High Complexity**: CYC=11 exceeds threshold (reason for refactoring)
5. ✅ **Low Nesting**: Nesting depth of 1 suggests linear logic

**Refactoring Strategy**:
- Extract conditional branches into helper methods
- Target CYC ≤ 8 per method (Jane Street standard)
- Keep all changes within src/V12_002.UI.Panel.Handlers.cs
- Update 2 caller sites after extraction

**Confidence**: HIGH - This is a textbook low-risk refactoring candidate.

## Recommendations

1. **Extract Method Pattern**: Break the 11-branch logic into 3-4 helper methods
2. **Naming Convention**: Use descriptive names like CollapseRetestControls(), CollapseFfmaControls(), etc.
3. **Testing**: Add unit tests for each extracted helper method
4. **Verification**: Run deploy-sync.ps1 after changes to sync NinjaTrader hard links

## Next Steps
- Proceed to Phase 1 (Scope Definition)
- Define extraction boundaries
- Identify specific conditional branches to extract
