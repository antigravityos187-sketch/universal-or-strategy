# Phase 1: Scope Definition - EPIC-W7-049

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:29:58Z

## Epic Overview
- **Target Method**: ManageTrail_RunPerTradeBranches
- **File**: src/V12_002.Trailing.cs
- **Line**: 240
- **Current CYC**: 11
- **Target CYC**: ≤8 per extracted method
- **Blast Radius**: LOW (0 external dependencies)

## Scope Boundary Definition

### IN SCOPE

#### Primary Extraction Target
1. **ManageTrail_RunPerTradeBranches** (src/V12_002.Trailing.cs:240)
   - Extract 3 trail handler branches into separate methods
   - Maintain dispatcher pattern in original method
   - Target CYC ≤8 per extracted method

#### Branch Extraction Targets
1. **TREND_E1 Branch**
   - Current: Inline switch case
   - Target: Extract to HandleTrail_TREND_E1()
   - Calls: TrailHandler_TREND_E1

2. **TREND_E2 Branch**
   - Current: Inline switch case
   - Target: Extract to HandleTrail_TREND_E2()
   - Calls: TrailHandler_TREND_E2

3. **RETEST Branch**
   - Current: Inline switch case
   - Target: Extract to HandleTrail_RETEST()
   - Calls: TrailHandler_RETEST

#### Files to Modify
- **src/V12_002.Trailing.cs** (PRIMARY)
  - Refactor ManageTrail_RunPerTradeBranches
  - Add 3 new extracted methods
  - Maintain single entry point

### OUT OF SCOPE

#### Excluded from This Epic
1. **Caller Method**
   - ManageTrailingStops (src/V12_002.Trailing.cs:39)
   - Reason: Separate concern, no complexity issue

2. **Trail Handler Methods**
   - TrailHandler_TREND_E1 (src/V12_002.Trailing.cs:257)
   - TrailHandler_TREND_E2 (src/V12_002.Trailing.cs:312)
   - TrailHandler_RETEST (src/V12_002.Trailing.cs:342)
   - Reason: Already extracted, separate methods

3. **Deep Callees (Depth 2-3)**
   - LogBuffer methods
   - UpdateStopOrder
   - ValidateStopPrice
   - Stop replacement handlers
   - Reason: Separate concerns, no direct complexity contribution

4. **Other Trailing Methods**
   - Any other methods in V12_002.Trailing.cs
   - Reason: Not part of this hotspot

5. **Test Files**
   - No test modifications in this epic
   - Reason: Tests will be added in separate TDD epic

## Extraction Strategy

### Approach: Strategy Pattern Dispatcher

BEFORE (CYC 11):
- Single method with inline switch logic
- All branch handling in one place
- CYC 11 exceeds Jane Street threshold

AFTER (CYC ≤3 dispatcher + 3 methods CYC ≤8 each):
- Dispatcher delegates to specialized handlers
- Each handler manages one trail mode
- CYC 3 dispatcher + 3 handlers ≤8 each

### Complexity Reduction Target
- **Before**: 1 method @ CYC 11
- **After**: 4 methods (1 dispatcher @ CYC 3, 3 handlers @ CYC ≤8 each)
- **Net Reduction**: CYC 11 → CYC 3 (dispatcher)

## Risk Mitigation

### Low Risk Factors
- ✅ Zero external dependencies (blast radius = 0)
- ✅ Single caller (ManageTrailingStops)
- ✅ Stable code (not in top 50 hotspots)
- ✅ Clear branch boundaries (switch statement)

### Mitigation Strategies
1. **Preserve Behavior**: Extract branches as-is, no logic changes
2. **Single File**: All changes in V12_002.Trailing.cs
3. **Atomic Commits**: One commit per extracted branch
4. **Build Verification**: Run deploy-sync.ps1 after each commit

## Success Criteria

### Phase 1 (Scope Definition)
- ✅ Scope boundaries clearly defined
- ✅ IN SCOPE: 1 method, 3 branch extractions
- ✅ OUT OF SCOPE: Callers, callees, tests
- ✅ Extraction strategy documented

### Epic Completion Criteria
1. **Complexity**: ManageTrail_RunPerTradeBranches CYC ≤3
2. **Extracted Methods**: 3 new methods, each CYC ≤8
3. **Build**: Zero compilation errors
4. **Sync**: deploy-sync.ps1 successful
5. **Verification**: F5 in NinjaTrader successful

## Jane Street Alignment

### Relevant Patterns
- **Strategy Pattern**: Dispatcher delegates to specialized handlers
- **Single Responsibility**: Each handler manages one trail mode
- **Guard Clauses**: Early returns in extracted methods

### KB Queries for Phase 2
- python scripts/query_kb.py "complexity reduction"
- python scripts/query_kb.py "strategy pattern"
- python scripts/query_kb.py "switch statement refactoring"

## Boundary Validation Notes

### Why This Scope is Correct
1. **Focused**: Single method, clear extraction targets
2. **Measurable**: CYC 11 → CYC 3 (73% reduction)
3. **Achievable**: Low blast radius, stable code
4. **Isolated**: No ripple effects to other methods
5. **Jane Street Aligned**: Targets CYC ≤8 mandate

### Why Excluded Items are OUT OF SCOPE
1. **Callers**: ManageTrailingStops has no complexity issue
2. **Callees**: Trail handlers already extracted
3. **Deep Callees**: Separate concerns, no direct impact
4. **Tests**: Separate TDD epic (not blocking)

---

**Scope Definition Complete**: 2026-06-24T19:29:58Z
**Next Phase**: Phase 1.5 (Scope Boundary Validation)
