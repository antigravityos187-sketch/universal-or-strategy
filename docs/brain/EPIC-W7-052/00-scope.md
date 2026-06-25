# Phase 1: Scope Definition - EPIC-W7-052

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: TBD
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:30:23Z

## Epic Objective
Reduce cyclomatic complexity of CleanupStalePendingReplacements from CYC 11 to ≤8 by extracting sub-workflows into helper methods.

## Target Method
- **Method**: CleanupStalePendingReplacements
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Line**: 37
- **Current CYC**: 11
- **Target CYC**: ≤8
- **Lines of Code**: 44
- **Max Nesting Depth**: 7

## Scope Boundary Analysis

### IN SCOPE ✅

#### Primary Extraction Target
- **CleanupStalePendingReplacements** method (CYC 11 → ≤8)
  - Extract validation phase
  - Extract cleanup iteration logic
  - Extract order creation phase
  - Extract target restoration phase
  - Extract submission phase

#### Allowed Modifications
1. Extract helper methods from CleanupStalePendingReplacements
2. Preserve all 26 callees (no behavioral changes)
3. Maintain orchestration pattern (coordinator role)
4. Add private helper methods to V12_002.Trailing.StopUpdate.cs
5. Preserve method signature (no parameters, void return)

#### Success Criteria
- Main method CYC ≤8
- Each extracted helper CYC ≤8
- All 26 callees preserved
- Zero behavioral changes
- Build passes
- F5 in NinjaTrader successful

### OUT OF SCOPE ❌

#### Forbidden Modifications
1. DO NOT modify any of the 26 callees
2. DO NOT change method signature of CleanupStalePendingReplacements
3. DO NOT modify calling context (method has 0 callers)
4. DO NOT refactor other methods in V12_002.Trailing.StopUpdate.cs
5. DO NOT change file structure (keep as partial class)

#### Deferred Work
- Performance optimization (not a complexity concern)
- Dead code removal (method has 0 callers but may be called via reflection)
- Logging improvements (preserve existing LogBuffer calls)
- Error handling changes (preserve existing patterns)

## Extraction Strategy

### Phase Identification
Based on hotspot analysis, the method orchestrates:

1. **Validation Phase** - ValidateStopOrderPreconditions
2. **Cleanup Iteration Phase** - pendingStopReplacements iteration
3. **Order Creation Phase** - CreateNewStopOrder
4. **Target Restoration Phase** - RestoreCascadedTargets
5. **Submission Phase** - SubmitStopOrderToBroker

### Extraction Plan
- Extract each phase into private helper method
- Target CYC ≤8 per helper
- Main method becomes coordinator (CYC ≤8)
- Preserve all call semantics

## Risk Assessment

### Complexity Risk: HIGH → LOW (after extraction)
- Current CYC 11 exceeds threshold by 37.5%
- Extraction will reduce to ≤8

### Blast Radius Risk: LOW (unchanged)
- 0 callers = isolated method
- Safe to refactor

### Maintenance Risk: MEDIUM → LOW (after extraction)
- High fan-out (26 callees) currently
- Extraction will organize into phases

## Verification Plan

### Build Verification
dotnet build src/V12_002.csproj

### Complexity Verification
python scripts/complexity_audit.py --file src/V12_002.Trailing.StopUpdate.cs --threshold 8

### Integration Verification
- F5 in NinjaTrader IDE
- Verify BUILD_TAG appears

### Hard Link Sync
powershell -File .\deploy-sync.ps1

## Dependencies

### Required Tools
- jCodemunch MCP (code analysis)
- Sequential Thinking MCP (scope validation)
- complexity_audit.py (CYC verification)

### Required Files
- src/V12_002.Trailing.StopUpdate.cs (target file)
- docs/brain/EPIC-W7-052/00-hotspots.md (input)

## Next Phase (Phase 1.5)
- Validate scope boundaries
- Confirm no scope creep
- Verify extraction plan feasibility
- Generate Phase 2 architecture plan
