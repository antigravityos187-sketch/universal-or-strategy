# Phase 1: Scope Definition - EPIC-W7-046

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:29:22Z

## Epic Summary
**Target**: HandleChartClick_ConvertPrice method complexity reduction
**File**: src/V12_002.UI.Callbacks.cs
**Current CYC**: 12 → **Target CYC**: ≤8
**Risk Level**: LOW-MEDIUM (isolated method, single caller)

## IN SCOPE

### Primary Target
- **Method**: `HandleChartClick_ConvertPrice` (line 272)
  - Extract nested conditional logic into helper methods
  - Reduce nesting depth from 5 to ≤3
  - Split 82-line method into smaller units (<50 lines each)
  - Target: CYC ≤8 per extracted method

### Extraction Candidates
1. **Price conversion logic** (nested conditionals)
2. **Validation logic** (parameter checks)
3. **Coordinate transformation** (chart-to-price conversion)
4. **Error handling paths** (logging and early returns)

### Constraints
- Preserve existing behavior (no logic changes)
- Maintain logging patterns (LogBuffer calls)
- Keep single caller pattern (OnChartClick)
- Preserve method signature (4 parameters)

## OUT OF SCOPE

### Caller Context
- **OnChartClick** (line 231) - UNCHANGED
  - Single caller remains untouched
  - No signature changes required

### Callee Dependencies
- **LogBuffer.Format** - UNCHANGED
- **LogBuffer.ValidateThreadAffinity** - UNCHANGED
- **LogBuffer.FormatInternal** - UNCHANGED

### Related Methods
- Other UI callback methods in V12_002.UI.Callbacks.cs
- Chart rendering logic
- Event handling infrastructure

## Scope Boundaries

### What Changes
✅ HandleChartClick_ConvertPrice method body
✅ New private helper methods (extracted logic)
✅ Method-level complexity metrics

### What Stays Unchanged
❌ Method signature (4 parameters)
❌ Caller (OnChartClick)
❌ Callees (LogBuffer methods)
❌ Public API surface
❌ Other UI callback methods

## Success Criteria

### Complexity Targets
- **Primary Method CYC**: ≤8 (currently 12)
- **Extracted Methods CYC**: ≤8 each
- **Max Nesting Depth**: ≤3 (currently 5)
- **Method Length**: <50 lines (currently 82)

### Quality Gates
- ✅ Build passes (zero compilation errors)
- ✅ All tests pass (if applicable)
- ✅ Logging behavior preserved
- ✅ No behavioral changes (pure refactor)

## Risk Mitigation

### Low Risk Factors
- Zero blast radius (no external dependencies)
- Single caller (OnChartClick only)
- Isolated to UI layer
- No cross-file changes required

### Medium Risk Factors
- Nesting depth 5 (requires careful extraction)
- 82 lines (substantial method body)
- Multiple conditional paths

### Mitigation Strategy
1. Extract one logical block at a time
2. Verify build after each extraction
3. Preserve exact logging behavior
4. Test with F5 in NinjaTrader after completion

## Verification Plan

### Build Verification
```bash
dotnet build src/V12_002.csproj
```

### Complexity Verification
```bash
python scripts/complexity_audit.py --file src/V12_002.UI.Callbacks.cs --threshold 8
```

### Hard Link Sync
```bash
powershell -File ./deploy-sync.ps1
```

### NinjaTrader Test
- F5 in NinjaTrader IDE
- Verify BUILD_TAG appears
- Test chart click functionality

## Scope Validation

**Scope Creep Prevention**: This epic targets ONLY HandleChartClick_ConvertPrice. Any other methods discovered during extraction are OUT OF SCOPE and require separate epics.

**Boundary Enforcement**: No changes to caller (OnChartClick) or callees (LogBuffer). Extraction is purely internal to the target method.

**Success Definition**: CYC ≤8 for primary method and all extracted helpers, with zero behavioral changes.
