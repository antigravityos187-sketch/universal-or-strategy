# Phase 0: Hotspot Analysis - EPIC-W7-155

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.79
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T03:03:54Z

## Target Method
- **Method**: TryHandleFleetCommand
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Line**: 37
- **Cyclomatic Complexity**: 20
- **Assessment**: HIGH

## Complexity Metrics

### Symbol Complexity Analysis
- **Cyclomatic Complexity**: 20 (exceeds Jane Street threshold of 8)
- **Max Nesting Depth**: 2 (acceptable)
- **Parameter Count**: 3 (acceptable)
- **Lines of Code**: 45
- **Assessment**: HIGH complexity

### Hotspot Score
- **Hotspot Score**: 43.9445
- **Rank**: #44 out of top 50 hotspots
- **Churn**: 8 commits in last 90 days
- **Risk Level**: HIGH (complexity x log(1 + churn))

## Blast Radius

### Direct Impact
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

### Analysis
The method has ZERO direct dependents, indicating it is likely called through a dispatcher or command router pattern. This is a LOW RISK refactoring target from a blast radius perspective.

## Call Hierarchy

### Callers (Depth 0)
NONE - This method has no direct callers in the indexed codebase. It is likely invoked through:
- IPC command routing
- Reflection-based dispatch
- Event handlers

### Callees (Depth 1-3)
The method has 185 callees across 3 depth levels, indicating HIGH FAN-OUT:

#### Depth 1 (Direct Calls - 18 methods)
1. TryHandleFleet_Trim - Trim command handler
2. TryHandleFleet_Lock50 - Lock 50% command handler
3. TryHandleFleet_FlattenOnly - Flatten-only command handler
4. TryHandleFleet_Flatten - Full flatten command handler
5. TryHandleFleet_CancelAll - Cancel all orders handler
6. TryHandleFleet_ResetMemory - Memory reset handler
7. TryHandleFleet_LongShort - Long/short position handler
8. TryHandleFleet_OrLong - OR long entry handler
9. TryHandleFleet_OrShort - OR short entry handler
10. TryHandleFleet_TrendManualLimit - Trend manual limit handler
11. TryHandleFleet_RetestManualLimit - Retest manual limit handler
12. TryHandleFleet_FfmaManualLimit - FFMA manual limit handler
13. TryHandleFleet_FfmaManualMarket - FFMA manual market handler
14. TryHandleFleet_CloseTarget - Close target handler
15. TryHandleFleet_MoveTarget - Move target handler
16. TryHandleFleet_FleetState - Fleet state query handler
17. TryHandleFleet_ToggleAccount - Account toggle handler
18. TryHandleFleet_SetShadow - Shadow mode setter

## Risk Assessment

### Overall Risk: MEDIUM-HIGH

#### Risk Factors
1. LOW Blast Radius: Zero direct dependents
2. HIGH Complexity: CYC=20 (2.5x Jane Street threshold)
3. HIGH Fan-Out: 185 callees across 3 levels
4. MEDIUM Churn: 8 commits in 90 days
5. LOW Nesting: Max depth 2 (acceptable)

### Refactoring Strategy
RECOMMENDED: Extract Command Router Pattern

The method is a command dispatcher with 18+ sub-handlers. Refactoring approach:

1. Extract Command Map: Create a dictionary mapping command types to handler methods
2. Single Responsibility: Each handler already exists as a separate method
3. Reduce Cyclomatic Complexity: Replace if/else chain with dictionary lookup
4. Preserve Behavior: No logic changes, pure structural refactoring

### Expected Outcome
- Before: CYC=20, 45 lines, 18+ branches
- After: CYC<=8, ~15 lines, dictionary-based dispatch
- Risk: LOW (zero dependents, existing handlers unchanged)

## Recommendations

### Phase 1 (Scope Definition)
1. Verify command routing pattern
2. Identify all command types handled
3. Document command-to-handler mappings

### Phase 2 (Architecture Planning)
1. Design command registry pattern
2. Plan dictionary-based dispatcher
3. Ensure backward compatibility

### Phase 3 (DNA Audit)
1. Verify no lock-free violations
2. Check ASCII-only compliance
3. Validate Jane Street patterns

### Phase 4 (Ticket Generation)
1. Ticket 1: Extract command registry
2. Ticket 2: Replace if/else with dictionary lookup
3. Ticket 3: Add unit tests for dispatcher

### Phase 5 (Execution)
- Use Bob CLI (v12-engineer) for surgical refactoring
- Target CYC<=8 per method
- Maintain zero blast radius impact

## Conclusion

TryHandleFleetCommand is a HIGH-PRIORITY refactoring target due to:
- Complexity exceeding Jane Street threshold (20 vs 8)
- Command dispatcher anti-pattern (18+ branches)
- Zero blast radius (safe to refactor)

The refactoring is LOW RISK and HIGH VALUE for code health improvement.
