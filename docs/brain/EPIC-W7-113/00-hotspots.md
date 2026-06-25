# Phase 0: Hotspot Analysis - EPIC-W7-113

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:55:53Z

## Target Method
- **Method**: HydrateFSMsFromWorkingOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 787
- **Cyclomatic Complexity**: 13
- **Lines of Code**: 105

## Complexity Metrics (from get_symbol_complexity)
- **Cyclomatic Complexity**: 13
- **Max Nesting Depth**: 4
- **Parameter Count**: 0
- **Lines**: 105
- **Assessment**: HIGH

## Hotspot Analysis (from get_hotspots)
- **Hotspot Score**: 46.22
- **Rank**: #36 out of top 50 hotspots
- **Churn (90 days)**: 34 commits
- **Risk Category**: HIGH

### Context
This method ranks in the top 50 hotspots with a score of 46.22, indicating it is both complex (CYC=13) and frequently modified (34 commits in 90 days). The combination of complexity and churn makes this a prime refactoring candidate.

## Blast Radius (from get_blast_radius)
- **Direct Dependents**: 0
- **Importer Count**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

### Analysis
**LOW BLAST RADIUS** - This method has zero direct dependents, making it an excellent refactoring candidate. Changes to this method will not ripple through the codebase, reducing regression risk.

## Call Hierarchy (from get_call_hierarchy)

### Callers (2)
1. **HydrateWorkingOrdersFromBroker** (src/V12_002.SIMA.Lifecycle.cs:309)
   - Resolution: ast_resolved
   - Depth: 1

2. **EnumerateApexAccounts** (src/V12_002.SIMA.Lifecycle.cs:140)
   - Resolution: ast_resolved
   - Depth: 2

### Callees (33)
The method calls 33 downstream symbols, including:
- **MapOrderStateToFSMState** (complexity 13)
- **FindLivePosition**
- **ResolveRemainingContracts**
- **BuildFSM**
- **LinkTargetOrderToFSM**
- **RegisterFSM**
- **HydrateFromOpenPositions** (complexity 34 - HIGH hotspot)
- Multiple order collection accessors (entryOrders, stopOrders, target1-5Orders)
- LogBuffer.Format methods

### Key Dependencies
- Accesses multiple order collections (entryOrders, stopOrders, activePositions, _followerBrackets)
- Calls several FSM lifecycle methods (BuildFSM, RegisterFSM, LinkTargetOrderToFSM)
- Interacts with position tracking (FindLivePosition, ResolveRemainingContracts)

## Risk Assessment

### Overall Risk: MEDIUM-LOW

**Factors:**
1. ✅ **LOW Blast Radius**: Zero direct dependents - changes are isolated
2. ⚠️ **HIGH Complexity**: CYC=13 exceeds Jane Street threshold (≤8)
3. ⚠️ **HIGH Churn**: 34 commits in 90 days indicates active development
4. ✅ **Clear Callers**: Only 2 callers, both in same file
5. ⚠️ **Many Callees**: 33 downstream calls suggest complex orchestration logic

### Refactoring Recommendation
**PROCEED WITH CAUTION** - This is a good refactoring candidate due to low blast radius, but the 33 callees suggest the method orchestrates complex FSM hydration logic. Extraction should focus on:
1. Breaking down the 105-line method into smaller, focused helpers
2. Reducing cyclomatic complexity from 13 to ≤8 per extracted method
3. Preserving the orchestration logic while simplifying individual steps

### Jane Street Alignment
- **Current CYC**: 13
- **Target CYC**: ≤8 (Jane Street strict standard)
- **Gap**: 5 complexity points to reduce
- **Strategy**: Extract 2-3 helper methods to distribute complexity

## Next Steps (Phase 1)
1. Define scope boundary - identify extraction candidates within the 105 lines
2. Analyze control flow to find natural break points
3. Ensure extracted methods maintain single responsibility
4. Verify no hidden dependencies on mutable state
