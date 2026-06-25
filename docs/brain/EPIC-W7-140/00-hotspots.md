# Phase 0: Hotspot Analysis - EPIC-W7-140

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 1.73
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T03:33:43Z

## Target Method
- **Method**: InitiateStopReplacement
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Line**: 307
- **Cyclomatic Complexity**: 13
- **Signature**: 
```csharp
private void InitiateStopReplacement(
    string entryName,
    PositionInfo pos,
    Order currentStop,
    double validatedStopPrice,
    int newTrailLevel
)
```

## Complexity Metrics
- **Cyclomatic Complexity**: 13 (HIGH - exceeds threshold of 8)
- **Max Nesting Depth**: 5
- **Parameter Count**: 5
- **Lines of Code**: 63
- **Assessment**: HIGH complexity

## Blast Radius Analysis
- **Importer Count**: 0 (internal method, not imported)
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0 (LOW)
- **Confirmed Impact Files**: 0
- **Potential Impact Files**: 0

**Analysis**: This is an internal private method with no external dependencies. Changes are isolated to the containing file.

## Call Hierarchy

### Callers (1)
1. **UpdateStopOrder** (src/V12_002.Trailing.StopUpdate.cs:84)
   - Resolution: ast_resolved
   - Depth: 1

### Callees (20)
Key dependencies include:
1. **GetTargetOrdersDictionary** - UI callbacks for target order management
2. **pendingStopReplacements** - State tracking constant
3. **LogBuffer.Format** - Logging infrastructure
4. **CancelOrderForReplace** - Order cancellation gateway
5. **MarkStickyDirty** - Persistent state management
6. **IsOrderTerminal** - Order state validation
7. **StampReaperMoveGrace** - SIMA lifecycle coordination
8. **CancelOrderSafe** - Safe order cancellation wrapper

**Depth Reached**: 2 (out of requested 3)

## Risk Assessment

### Overall Risk: MEDIUM

**Rationale**:
- ✅ **LOW Blast Radius**: Private method with no external callers beyond UpdateStopOrder
- ⚠️ **HIGH Complexity**: CYC=13 exceeds Jane Street threshold of 8 by 62%
- ✅ **Isolated Scope**: Changes contained within trailing stop update subsystem
- ⚠️ **Deep Nesting**: Max nesting depth of 5 indicates complex conditional logic
- ✅ **Single Entry Point**: Only called by UpdateStopOrder, making refactoring safer

### Complexity Breakdown
The method orchestrates stop order replacement with multiple responsibilities:
1. Pending replacement state management
2. Order cancellation coordination
3. Sticky state persistence
4. Logging and diagnostics
5. REAPER grace period stamping

### Recommended Approach
**Extract Method Pattern** - Break into smaller, single-responsibility methods:
- Extract pending replacement lookup logic (CYC ~3)
- Extract cancellation coordination logic (CYC ~3)
- Extract state persistence logic (CYC ~2)
- Keep orchestration logic in main method (CYC ~5)

Target: Reduce from CYC=13 to CYC≤8 across all extracted methods.

## Jane Street Alignment
- **Current**: CYC=13 (FAILS Jane Street strict standard of ≤8)
- **Target**: CYC≤8 per method after extraction
- **Cognitive Load**: HIGH - 5 levels of nesting makes reasoning difficult
- **Testing Complexity**: Exponential path growth (2^13 = 8,192 potential paths)

## Next Steps
Proceed to Phase 1 (Scope Definition) to plan extraction strategy.
