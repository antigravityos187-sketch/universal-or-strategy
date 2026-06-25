# Phase 0: Hotspot Analysis - EPIC-W7-045

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:42:46Z to 2026-06-23T02:43:04Z

## Target Method
- **Method**: OnKeyDown
- **File**: src/V12_002.UI.Callbacks.cs
- **Line**: 391
- **Cyclomatic Complexity**: 9
- **Max Nesting Depth**: 2
- **Parameter Count**: 2
- **Lines of Code**: 36

## Complexity Metrics

### Symbol Complexity Analysis
- Cyclomatic: 9
- Max Nesting: 2
- Param Count: 2
- Lines: 36
- Assessment: medium

**Assessment**: MEDIUM complexity
- CYC=9 exceeds Jane Street threshold of 8 by 1 point
- Relatively shallow nesting (2 levels)
- Simple parameter signature (2 params)
- Compact implementation (36 lines)

### Hotspot Context
OnKeyDown does NOT appear in the top 50 hotspots by hotspot_score (complexity × log(1 + churn)).
This suggests the method has:
- Low git churn (not frequently modified)
- Moderate complexity (CYC=9)
- Lower refactoring priority compared to top hotspots (scores 120.88 to 43.60)

**Top 3 Hotspots for Reference**:
1. HydrateFromOpenPositions (CYC=34, score=120.88)
2. IsCommandForThisInstrument (CYC=38, score=109.83)
3. HandleTerminated (CYC=30, score=102.04)

## Blast Radius Analysis

### Direct Dependencies
- Importer Count: 0
- Direct Dependents: 0
- Overall Risk Score: 0.0
- Confirmed Count: 0
- Potential Count: 0

**Risk Assessment**: LOW
- Zero confirmed importers
- Zero potential importers
- No external dependencies on this method
- Changes are isolated to the method itself

### Interpretation
OnKeyDown is a UI callback method (event handler) that is:
- Called by the NinjaTrader framework (not by our code)
- Not referenced by any other methods in the codebase
- Safe to refactor without breaking downstream code

## Call Hierarchy

### Callers (Upstream)
**Count**: 0
- No internal callers detected
- Method is invoked by NinjaTrader UI framework as keyboard event handler

### Callees (Downstream)
**Count**: 22 (depth=3)

**Direct Callees (depth=1)**:
1. _keyCommands (constant) - Keyboard command mapping dictionary
2. HandleTargetAction (method) - Processes target-related keyboard actions
3. HandleRunnerAction (method) - Processes runner-related keyboard actions

**Indirect Callees (depth=2)**:
4. ExecuteTargetAction (method) - Executes target actions
5. Enqueue (method) - Actor pattern command queue

**Indirect Callees (depth=3)**:
6. LogBuffer.Format (method) - Performance logging
7. ExecuteTargetActionForPosition (method) - Position-specific target actions
8. _cmdQueue (constant) - Command queue reference
9. IsActorThread (method) - Thread safety check
10. TryDrain (method) - Queue draining
11. ScheduleActorDrain (method) - Async queue processing

## Risk Assessment

### Overall Risk: LOW-MEDIUM

**Factors Supporting LOW Risk**:
- Zero blast radius (no importers)
- UI callback (framework-invoked, not called by our code)
- Low git churn (not in top 50 hotspots)
- Shallow nesting (2 levels)
- Compact size (36 lines)

**Factors Supporting MEDIUM Risk**:
- CYC=9 exceeds Jane Street threshold (8)
- Calls into Actor pattern (Enqueue) - thread safety critical
- 22 downstream callees (moderate coupling)
- UI event handler (user-facing, must be reliable)

### Refactoring Strategy
**Recommended Approach**: Extract conditional branches

The method likely contains:
- Dictionary lookup (_keyCommands)
- Conditional dispatch (if/switch on key codes)
- Action routing (HandleTargetAction vs HandleRunnerAction)

**Extraction Candidates**:
1. Key validation logic -> IsValidKeyCommand()
2. Command lookup logic -> GetKeyCommand()
3. Action dispatch logic -> DispatchKeyAction()

**Expected Outcome**:
- Reduce CYC from 9 to <=8
- Improve testability (smaller units)
- Maintain zero blast radius (internal refactor only)

## Conclusion

OnKeyDown is a **LOW-MEDIUM risk** refactoring target:
- Safe to modify (zero external dependencies)
- Moderate complexity (CYC=9, just above threshold)
- Low churn (stable code)
- UI-critical (requires careful testing)

**Recommendation**: Proceed with extraction-based refactoring to reduce CYC to <=8.
