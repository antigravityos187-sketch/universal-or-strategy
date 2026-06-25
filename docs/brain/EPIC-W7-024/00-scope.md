# Phase 1: Scope Definition - EPIC-W7-024

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.75
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:06:19Z

## Epic Context
- **Target Method**: MonitorRmaProximity
- **File**: src/V12_002.Entries.RMA.cs
- **Current CYC**: 9 (target: ≤8)
- **Risk Level**: LOW
- **Priority**: LOW (not a true hotspot)

## Scope Boundary Analysis

### Complexity Assessment
- **Current CYC**: 9 (1 point above Jane Street threshold)
- **Helper Methods Already Extracted**: 4
  1. ShouldMonitorOrder (order filtering)
  2. UpdateProximityAndCalculateDistance (distance calculation)
  3. HandleProximityEntry (entry event handler)
  4. HandleProximityExit (exit event handler)

### Blast Radius
- **Direct Importers**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Impact**: ISOLATED to src/V12_002.Entries.RMA.cs

## IN SCOPE

### Primary Target
- **MonitorRmaProximity method** (CYC 9 → ≤8)
  - Extract remaining conditional logic to reduce CYC by 1-2 points
  - Simplify main orchestration flow
  - Maintain existing helper method calls

### Specific Extractions
1. **Order iteration and filtering logic**
   - Extract loop body to helper method
   - Reduce nesting depth from 4 to ≤3

2. **Conditional branching simplification**
   - Consolidate proximity state checks
   - Extract guard clauses to early returns

### Files to Modify
- src/V12_002.Entries.RMA.cs (single file scope)

### Success Criteria
- MonitorRmaProximity CYC reduced from 9 to ≤8
- Max nesting depth reduced from 4 to ≤3
- All existing helper methods remain functional
- Zero regression in RMA proximity monitoring behavior
- Build passes
- F5 in NinjaTrader successful

## OUT OF SCOPE

### Excluded Work
1. **Helper methods** (already at acceptable complexity)
   - ShouldMonitorOrder
   - UpdateProximityAndCalculateDistance
   - HandleProximityEntry
   - HandleProximityExit

2. **Performance instrumentation**
   - LatencyProbe calls
   - Histogram tracking (_histMonitorRmaProximity)
   - LogBuffer operations

3. **Other RMA methods** in same file
   - Focus ONLY on MonitorRmaProximity
   - Do not refactor adjacent methods

4. **Cross-file changes**
   - No changes to other V12_002.*.cs files
   - No changes to test files
   - No changes to infrastructure

5. **Behavioral changes**
   - No logic modifications
   - No algorithm improvements
   - Pure structural refactoring only

### Deferred Items
- Other high-priority hotspots (CYC 23-38)
- RMA algorithm enhancements
- Test coverage expansion

## Extraction Strategy

### Approach
**MINIMAL SURGICAL EXTRACTION** - This is a borderline case (CYC 9 vs threshold 8)

### Recommended Extraction
1. **Extract loop body** to ProcessOrderProximityCheck(Order order)
   - Encapsulates per-order monitoring logic
   - Reduces main method nesting
   - Target CYC: 2-3

2. **Simplify main orchestration**
   - MonitorRmaProximity becomes simple iterator
   - Delegates to ProcessOrderProximityCheck
   - Target CYC: 2-3

## Risk Mitigation

### Low-Risk Factors
- Zero blast radius (no external callers)
- Already partially refactored (4 helpers exist)
- Single file scope
- Clear extraction boundaries

### Validation Steps
1. Build verification (dotnet build)
2. Hard link sync (deploy-sync.ps1)
3. F5 in NinjaTrader IDE
4. Verify BUILD_TAG in output
5. Manual smoke test of RMA proximity monitoring

## Scope Justification

### Why This Scope?
- **Minimal intervention**: CYC is only 1 point above threshold
- **Surgical precision**: Extract only what needed to hit CYC ≤8
- **Preserve existing work**: 4 helper methods already extracted
- **Zero risk**: No external dependencies to break

### Why Not Broader?
- Helper methods already at acceptable complexity
- No churn detected (not in top 50 hotspots)
- Higher-priority targets exist (CYC 23-38)
- Avoid scope creep (V12.23 mandate)

## Estimated Effort
- **Complexity**: TRIVIAL
- **Time**: 15-30 minutes
- **Tickets**: 1 (single extraction)
- **Risk**: MINIMAL

## Conclusion

This is a **MINIMAL SCOPE** epic targeting a borderline complexity case (CYC 9 vs threshold 8). The extraction is surgical, low-risk, and preserves existing helper methods. Success requires reducing CYC by 1-2 points through simple loop body extraction.

**Recommendation**: Proceed with minimal extraction OR defer in favor of higher-priority hotspots (CYC 23-38).
