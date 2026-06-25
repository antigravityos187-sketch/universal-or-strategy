# Phase 0: Hotspot Analysis - EPIC-W7-048

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.74
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T03:14:23Z

## Target Method
- **Method**: UpdateExistingPendingReplacement
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Line**: 167
- **Cyclomatic Complexity**: 15 (ACTUAL - not 9 as initially reported)
- **Assessment**: HIGH RISK

## Complexity Metrics
- **Cyclomatic Complexity**: 15
- **Max Nesting Depth**: 6
- **Parameter Count**: 5
- **Lines of Code**: 87
- **Assessment**: HIGH (exceeds Jane Street threshold of 8)

### Signature
```csharp
private void UpdateExistingPendingReplacement(
    string entryName,
    PositionInfo pos,
    Order currentStop,
    double validatedStopPrice,
    int newTrailLevel
)
```

## Blast Radius Analysis
- **Direct Dependents**: 0
- **Importer Count**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

**Interpretation**: This method is ISOLATED - no external files import or depend on it. Changes are contained within the file.

## Call Hierarchy

### Callers (1)
1. **UpdateStopOrder** (src/V12_002.Trailing.StopUpdate.cs:84)
   - Resolution: ast_resolved
   - Depth: 1

### Callees (16)
1. **CaptureTargetSnapshot** (src/V12_002.Trailing.StopUpdate.cs:255) - ast_resolved
2. **pendingStopReplacements** (src/V12_002.cs:210) - constant access
3. **LogBuffer.Format** (src/V12_002.Perf.LogBuffer.cs:28) - logging
4. **RefreshTargetSnapshot** (src/V12_002.Trailing.StopUpdate.cs:281) - ast_resolved
5. **MarkStickyDirty** (src/V12_002.StickyState.cs:619) - state management
6. **GetTargetOrdersDictionary** (src/V12_002.UI.Callbacks.cs:1039) - depth 2
7. **LogBuffer.ValidateThreadAffinity** (src/V12_002.Perf.LogBuffer.cs:119) - depth 2
8. **LogBuffer.FormatInternal** (src/V12_002.Perf.LogBuffer.cs:56) - depth 2

## Risk Assessment

### Overall Risk: MEDIUM-HIGH

**Factors**:
1. ✅ **Isolation**: Zero blast radius - changes won't break other files
2. ❌ **Complexity**: CYC=15 (87% above Jane Street threshold of 8)
3. ❌ **Nesting**: Max depth of 6 (cognitive load)
4. ⚠️ **Dependencies**: 16 callees (moderate coupling)
5. ✅ **Single Caller**: Only called by UpdateStopOrder (clear entry point)

### Refactoring Priority: HIGH

**Rationale**:
- Complexity nearly 2x Jane Street standard
- Deep nesting (6 levels) indicates complex control flow
- 87 lines in single method (God-method smell)
- Isolated blast radius makes refactoring SAFE

### Recommended Approach
1. Extract nested conditional blocks into helper methods
2. Reduce nesting depth from 6 to ≤3
3. Target CYC ≤8 per extracted method
4. Preserve single caller relationship (UpdateStopOrder)

## Hotspot Score Calculation
- **Complexity Factor**: 15 (high)
- **Churn Factor**: Unknown (requires git history)
- **Estimated Hotspot Score**: HIGH (complexity alone justifies priority)

## Next Steps
- Proceed to Phase 1 (Scope Definition)
- Focus on extracting conditional logic blocks
- Maintain zero blast radius advantage
