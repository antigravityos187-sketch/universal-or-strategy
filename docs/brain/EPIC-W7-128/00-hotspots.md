# Phase 0: Hotspot Analysis - EPIC-W7-128

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.74
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:58:38Z

## Target Method
- **Method**: SymmetryGuardReplaceExistingFollowerTarget
- **File**: src/V12_002.Symmetry.Replace.cs
- **Line**: 27
- **Cyclomatic Complexity**: 20 (HIGH - exceeds threshold of 8)
- **Max Nesting Depth**: 5
- **Parameter Count**: 5
- **Lines of Code**: 71
- **Assessment**: HIGH RISK

## Complexity Metrics

### Cyclomatic Complexity Analysis
- **Current CYC**: 20
- **Target CYC**: ≤8 (Jane Street strict standard)
- **Reduction Required**: 12 points (60% reduction)
- **Complexity Category**: HIGH (11+)

### Method Signature
```csharp
private void SymmetryGuardReplaceExistingFollowerTarget(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    ConcurrentDictionary<string, Order> dict
)
```

### Structural Metrics
- **Max Nesting Depth**: 5 levels (indicates deeply nested conditionals)
- **Parameter Count**: 5 (at upper limit for cognitive load)
- **Lines of Code**: 71 (moderate size, but high complexity density)

## Blast Radius

### Impact Analysis
- **Direct Dependents**: 0
- **Importer Count**: 0
- **Overall Risk Score**: 0.0 (LOW)
- **Confirmed Dependencies**: 0
- **Potential Dependencies**: 0

### Interpretation
This method has **ZERO external blast radius**. It is:
- Not imported by other files
- Not called from outside its containing file
- Safe to refactor without cross-file coordination

This is an **IDEAL REFACTORING TARGET** - high complexity with minimal external impact.

## Call Hierarchy

### Callers (Who calls this method)
1. **SymmetryGuardRetargetExistingFollowerBracket** (src/V12_002.Symmetry.Replace.cs:17)
   - Resolution: AST resolved
   - Depth: 1

### Callees (What this method calls)
The method calls 14 other methods:

**PositionInfo Methods** (8 calls):
1. IsRunnerTarget (src/V12_002.PositionInfo.cs:138)
2. IsTargetFilled (src/V12_002.PositionInfo.cs:293)
3. GetTargetContracts (src/V12_002.PositionInfo.cs:277)
4. GetTargetPrice (src/V12_002.PositionInfo.cs:285)
5. GetTargetMode (src/V12_002.PositionInfo.cs:119) - depth 2

**Symmetry Methods** (2 calls):
6. SymmetryTrim (src/V12_002.Symmetry.Replace.cs:343)

**SIMA Methods** (2 calls):
7. StampReaperMoveGrace (src/V12_002.SIMA.cs:199)

**Note**: Some methods appear in both src/ and src-vm-backup/ (duplicate detection)

### Call Graph Depth
- **Maximum Depth Reached**: 2
- **Total Caller Count**: 1
- **Total Callee Count**: 14

## Risk Assessment

### Overall Risk: **MEDIUM-HIGH**

**Risk Factors**:
1. ✅ **Complexity**: CYC 20 (2.5x over threshold) - HIGH RISK
2. ✅ **Nesting**: 5 levels deep - MEDIUM RISK
3. ✅ **Size**: 71 lines - MEDIUM RISK
4. ✅ **Blast Radius**: 0 external dependencies - LOW RISK (GOOD)
5. ✅ **Call Depth**: 2 levels - LOW RISK

**Refactoring Safety**: **HIGH**
- Zero external dependencies means refactoring is isolated
- Single caller makes testing straightforward
- No cross-file coordination required

**Recommended Approach**:
1. Extract nested conditionals into guard clauses
2. Extract complex logic blocks into helper methods
3. Reduce nesting depth from 5 to ≤3
4. Target CYC ≤8 per extracted method

## Jane Street Alignment

This method violates Jane Street HFT principles:
- **Cognitive Simplicity**: CYC 20 is too complex for microsecond-latency reasoning
- **Exhaustive Testing**: 2^20 = 1M+ paths (exponential test explosion)
- **Race Condition Auditing**: Deep nesting makes lock-free verification difficult

**Priority**: HIGH - This is exactly the type of method Jane Street would flag for immediate refactoring.

## Conclusion

**PROCEED WITH EPIC-W7-128**

This method is an **ideal refactoring candidate**:
- High complexity (CYC 20)
- Zero external blast radius
- Single caller (easy to test)
- Clear extraction opportunities

**Estimated Effort**: 3-4 tickets to reduce CYC from 20 to ≤8
