# Phase 0: Hotspot Analysis - EPIC-W7-126

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.74
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:58:14Z

## Target Method
- **Method**: SymmetryGuardSubmitFollowerBracket
- **File**: src/V12_002.Symmetry.Follower.cs
- **Line**: 285
- **Cyclomatic Complexity**: 16 (HIGH - exceeds Jane Street threshold of 8)
- **Lines of Code**: 141
- **Parameter Count**: 2

## Complexity Metrics

### Symbol Complexity Analysis
```json
{
  "cyclomatic": 16,
  "max_nesting": 5,
  "param_count": 2,
  "lines": 141,
  "assessment": "high"
}
```

**Assessment**: HIGH complexity
- **Cyclomatic Complexity**: 16 (2x Jane Street threshold of 8)
- **Max Nesting Depth**: 5 levels
- **Method Length**: 141 lines (excessive for single responsibility)
- **Cognitive Load**: High - difficult to reason about under microsecond latency constraints

## Blast Radius

### Import Impact Analysis
```json
{
  "importer_count": 0,
  "direct_dependents_count": 0,
  "overall_risk_score": 0.0,
  "confirmed_count": 0,
  "potential_count": 0
}
```

**Assessment**: LOW external impact
- **Zero external importers**: Method is internal to V12_002.Symmetry.Follower.cs
- **Zero cross-file dependencies**: Changes isolated to single file
- **Risk Score**: 0.0 (minimal blast radius)

## Call Hierarchy

### Callers (3 methods)
1. **SymmetryGuardOnFollowerFill** (line 17) - Direct caller, depth 1
2. **SymmetryGuardTryResolveFollower** (line 129) - Direct caller, depth 1
3. **SymmetryGuardProcessPendingFollowerFills** (line 97) - Indirect caller, depth 2

### Callees (34 methods)
**Depth 1 (Direct calls)**:
- ValidateStopPrice (Orders.Management.StopSync.cs)
- SymmetryTrim (Symmetry.Replace.cs)
- GetTargetContracts (PositionInfo.cs)
- IsRunnerTarget (PositionInfo.cs)
- GetTargetPrice (PositionInfo.cs)
- LogBuffer.Format (Perf.LogBuffer.cs)
- Enqueue (V12_002.cs - Actor pattern)
- GetTargetOrdersDictionary (UI.Callbacks.cs)

**Depth 2 (Indirect calls)**:
- Validate_LongIsIllegalAdjust
- Validate_ShortIsIllegalAdjust
- GetTargetMode
- LogBuffer.ValidateThreadAffinity
- LogBuffer.FormatInternal
- _cmdQueue (Actor queue)
- IsActorThread
- TryDrain
- ScheduleActorDrain

**Call Pattern Analysis**:
- Heavy use of validation methods (stop price, illegal adjusts)
- Actor pattern integration (Enqueue, _cmdQueue, TryDrain)
- Position info queries (contracts, prices, modes)
- Logging infrastructure (LogBuffer)

## Risk Assessment

### Overall Risk: MEDIUM-HIGH

**Risk Factors**:
1. **Complexity Risk**: HIGH
   - CYC 16 (2x threshold)
   - 5-level nesting depth
   - 141 lines (God-method pattern)
   - 34 callees (high coupling)

2. **Blast Radius Risk**: LOW
   - Zero external importers
   - Isolated to single file
   - Internal symmetry guard logic

3. **Refactoring Risk**: MEDIUM
   - 3 callers (manageable)
   - Actor pattern integration (requires careful handling)
   - Validation logic intertwined with business logic

### Refactoring Strategy Recommendation

**Approach**: Extract validation and submission logic into separate methods

**Suggested Extractions**:
1. **ValidateFollowerBracketPreconditions** (CYC ≤ 8)
   - Consolidate all validation calls
   - Early return pattern for failures

2. **CalculateFollowerBracketPrices** (CYC ≤ 8)
   - Extract price calculation logic
   - Isolate GetTargetPrice, GetTargetContracts calls

3. **SubmitFollowerBracketOrders** (CYC ≤ 8)
   - Pure submission logic
   - Enqueue calls and order dictionary updates

**Benefits**:
- Each extracted method ≤ 8 CYC (Jane Street compliant)
- Single responsibility per method
- Easier to test in isolation
- Reduced cognitive load

**Risks**:
- Actor pattern integration must preserve thread safety
- Validation order must be maintained
- Logging context must be preserved

## Hotspot Score Calculation

**Formula**: `hotspot_score = cyclomatic_complexity × log(1 + churn)`

**Assumptions** (churn data not available in Phase 0):
- Assuming moderate churn (10 commits in 90 days)
- `log(1 + 10) = 2.4`
- `hotspot_score = 16 × 2.4 = 38.4`

**Interpretation**: HIGH hotspot score (threshold typically 20)

## Conclusion

**EPIC-W7-126 is a valid refactoring target**:
- ✅ High complexity (CYC 16 > 8)
- ✅ Low blast radius (isolated changes)
- ✅ Clear extraction opportunities
- ✅ Manageable caller count (3)
- ⚠️ Requires careful Actor pattern handling

**Recommended Next Steps**:
1. Phase 1: Define scope boundary (3 extracted methods)
2. Phase 2: Architecture plan (preserve Actor semantics)
3. Phase 3: DNA audit (validate lock-free compliance)
4. Phase 4: Generate tickets (one per extracted method)
5. Phase 5: Execute extractions with TDD

**Confidence**: HIGH - Clear refactoring path with low external risk
