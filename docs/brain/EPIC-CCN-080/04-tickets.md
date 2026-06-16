# Extraction Tickets: EPIC-CCN-080

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 4-6 hours
- **Target Method**: PlacePanel (src/V12_002.UI.Panel.Construction.cs)
- **Current Complexity**: CYC = 13
- **Target Complexity**: CYC ≤ 8 per method

## TICKET-1: Extract TryHijackChartTrader

### Scope
- **Current Method**: `PlacePanel`
- **Lines**: 241-267
- **Current CYC**: 13 (entire method)
- **Target CYC**: ≤ 8
- **Extraction**: Chart Trader hijack logic into dedicated helper method

### Implementation
1. Create new private method `TryHijackChartTrader()` with return type `bool`
2. Extract lines 241-267 (Chart Trader discovery and hijack logic)
3. Move logic:
   - Find Chart Trader element via FindChartTrader()
   - Extract grid position (column, row, spans)
   - Apply position to rootContainer
   - Add rootContainer to trader grid
   - Collapse original Chart Trader element
   - Set _placementMode = PanelPlacement.Hijack
4. Return `true` on success, `false` if Chart Trader not found
5. Update PlacePanel to call `if (TryHijackChartTrader()) return;`
6. Run checkpoint after extraction

### Acceptance Criteria
- [ ] TryHijackChartTrader() method created with correct signature
- [ ] Chart Trader hijack logic fully extracted (lines 241-267)
- [ ] Method returns bool (true = success, false = not found)
- [ ] PlacePanel calls new method with early exit on success
- [ ] All tests pass (100% pass rate)
- [ ] Build succeeds (zero errors)
- [ ] No behavioral changes (identical runtime behavior)
- [ ] Checkpoint created for rollback safety

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Build check
powershell -File .\scripts\build_readiness.ps1

# Complexity audit
python scripts/complexity_audit.py

# Test suite
dotnet test
```

---

## TICKET-2: Extract TryInjectIntoChartTabGrid

### Scope
- **Current Method**: `PlacePanel`
- **Lines**: 269-288
- **Current CYC**: ~9 (after TICKET-1)
- **Target CYC**: ≤ 8
- **Extraction**: Chart Tab Grid injection logic into dedicated helper method

### Implementation
1. Create new private method `TryInjectIntoChartTabGrid()` with return type `bool`
2. Extract lines 269-288 (Chart Tab Grid discovery and injection logic)
3. Move logic:
   - Find Chart Tab Grid via FindChartTabGrid(ChartControl)
   - Create new ColumnDefinition (width 210)
   - Calculate panel column index
   - Set rootContainer grid position
   - Apply row span if multiple rows exist
   - Set horizontal alignment and width
   - Add rootContainer to grid
   - Set _placementMode = PanelPlacement.Injected
4. Return `true` on success, `false` if grid not found
5. Update PlacePanel to call `if (TryInjectIntoChartTabGrid()) return;`
6. Run checkpoint after extraction

### Acceptance Criteria
- [ ] TryInjectIntoChartTabGrid() method created with correct signature
- [ ] Chart Tab Grid injection logic fully extracted (lines 269-288)
- [ ] Method returns bool (true = success, false = not found)
- [ ] PlacePanel calls new method with early exit on success
- [ ] All tests pass (100% pass rate)
- [ ] Build succeeds (zero errors)
- [ ] No behavioral changes (identical runtime behavior)
- [ ] Checkpoint created for rollback safety

### Dependencies
- TICKET-1 must be completed first

### Verification Commands
```powershell
# Build check
powershell -File .\scripts\build_readiness.ps1

# Complexity audit
python scripts/complexity_audit.py

# Test suite
dotnet test
```

---

## TICKET-3: Extract SchedulePlacementRetry

### Scope
- **Current Method**: `PlacePanel`
- **Lines**: 290-299+
- **Current CYC**: ~6 (after TICKET-2)
- **Target CYC**: ≤ 8
- **Extraction**: Retry/fallback logic into dedicated helper method

### Implementation
1. Create new private method `SchedulePlacementRetry()` with return type `void`
2. Extract lines 290-299+ (retry scheduling and fallback logic)
3. Move logic:
   - Check retry count < 3
   - Increment _placementRetryCount
   - Initialize _placementRetryTimer if null
   - Set timer interval to 500ms
   - Attach retry handler
   - Start timer
   - Print retry status
4. No return value (void method - always schedules retry or accepts fallback)
5. Update PlacePanel to call `SchedulePlacementRetry();` as final fallback
6. Run checkpoint after extraction

### Acceptance Criteria
- [ ] SchedulePlacementRetry() method created with correct signature
- [ ] Retry/fallback logic fully extracted (lines 290-299+)
- [ ] Method has void return type (no early exit)
- [ ] PlacePanel calls new method as final fallback
- [ ] All tests pass (100% pass rate)
- [ ] Build succeeds (zero errors)
- [ ] No behavioral changes (identical runtime behavior)
- [ ] Checkpoint created for rollback safety

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Verification Commands
```powershell
# Build check
powershell -File .\scripts\build_readiness.ps1

# Complexity audit
python scripts/complexity_audit.py

# Test suite
dotnet test
```

---

## TICKET-4: Refactor PlacePanel to Orchestrator

### Scope
- **Current Method**: `PlacePanel`
- **Current CYC**: ~6 (after TICKET-3)
- **Target CYC**: ≤ 4
- **Refactor**: Simplify PlacePanel to orchestrator pattern

### Implementation
1. Simplify PlacePanel to orchestrator structure:
   ```csharp
   private void PlacePanel()
   {
       // Early exit if already placed or no container
       if (rootContainer == null || _placementMode != PanelPlacement.None)
           return;
       
       // Strategy 1: Try Chart Trader hijack
       _chartTraderElement = FindChartTrader();
       if (TryHijackChartTrader())
           return;
       
       // Strategy 2: Try Chart Tab Grid injection
       _chartTraderElement = null;
       if (TryInjectIntoChartTabGrid())
           return;
       
       // Strategy 3: Schedule retry/fallback
       SchedulePlacementRetry();
   }
   ```
2. Remove all extracted logic (should already be in helper methods)
3. Verify orchestrator has only 4 branches (67% reduction from 13)
4. Run final checkpoint

### Acceptance Criteria
- [ ] PlacePanel simplified to orchestrator pattern
- [ ] Method complexity reduced to CYC ≤ 4
- [ ] All three helper methods called in correct order
- [ ] Early exit on success for strategies 1 and 2
- [ ] Strategy 3 always executes (no early exit)
- [ ] All tests pass (100% pass rate)
- [ ] Build succeeds (zero errors)
- [ ] Complexity audit passes (CYC ≤ 8 for all methods)
- [ ] No behavioral changes (identical runtime behavior)
- [ ] Final checkpoint created

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Verification Commands
```powershell
# Build check
powershell -File .\scripts\build_readiness.ps1

# Complexity audit (verify all methods ≤8)
python scripts/complexity_audit.py

# Test suite
dotnet test

# Pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# Hard-link sync (MANDATORY after src/ changes)
powershell -File .\deploy-sync.ps1
```

---

## Post-Implementation Checklist

### Quality Gates
- [ ] All 4 tickets completed sequentially
- [ ] PlacePanel complexity reduced from 13 to ≤4
- [ ] All helper methods complexity ≤8
- [ ] Build passes (zero errors)
- [ ] Tests pass (100% pass rate)
- [ ] Complexity audit passes
- [ ] Pre-push validation passes (fast mode)
- [ ] Hard-link sync completed

### Runtime Verification
- [ ] F5 in NinjaTrader (manual smoke test)
- [ ] Panel placement works (Chart Trader hijack)
- [ ] Panel placement works (Chart Tab Grid injection)
- [ ] Panel placement works (retry/fallback)
- [ ] No visual regressions
- [ ] No runtime errors

### Documentation
- [ ] Update manifest.json with Phase 4 completion
- [ ] Document final complexity metrics
- [ ] Archive extraction artifacts

## Success Metrics

### Complexity Reduction
- **Before**: PlacePanel CYC = 13
- **After**: 
  - PlacePanel (orchestrator) CYC = 4 (67% reduction)
  - TryHijackChartTrader CYC = 5-6
  - TryInjectIntoChartTabGrid CYC = 3-4
  - SchedulePlacementRetry CYC = 2-3
- **Total Distributed**: 14-17 across 4 methods
- **Per-Method Max**: 6 (well under Jane Street threshold of 8)

### Jane Street Alignment
✅ **Cognitive Simplicity**: Each method has single responsibility
✅ **Testability**: Isolated units enable exhaustive testing
✅ **Microsecond Reasoning**: Simpler functions fit in working memory
✅ **Debugging Efficiency**: Clear failure isolation points

### V12 DNA Compliance
✅ **Correctness by Construction**: Preserved invariants
✅ **Lock-Free**: No synchronization primitives
✅ **ASCII-Only**: No Unicode characters
✅ **Hard-Link Integrity**: deploy-sync.ps1 executed

## Risk Assessment

### Extraction Risk: LOW
- Logic is self-contained with clear boundaries
- Checkpointing enabled for rollback safety
- Sequential execution reduces coordination complexity

### Regression Risk: LOW
- No API changes
- No caller/callee modifications
- Test suite validates behavior preservation

### Complexity Risk: NONE
- All helpers stay well under CYC=8 threshold
- Complexity audit confirms compliance
- Jane Street principles aligned

## Notes

### Implementation Order
The tickets MUST be executed sequentially (1→2→3→4) because:
1. Each extraction reduces PlacePanel complexity incrementally
2. Checkpoints enable rollback if issues arise
3. Tests validate behavior after each step
4. Final refactor depends on all helpers being extracted

### Rollback Strategy
If any ticket fails:
1. Use Bob CLI `/restore` command to rollback to last checkpoint
2. Review failure reason
3. Adjust extraction strategy if needed
4. Retry ticket with corrected approach

### Testing Strategy
- Run tests after EACH ticket (not just at the end)
- Verify complexity reduction after EACH extraction
- Checkpoint after EACH successful extraction
- This enables fast failure detection and easy rollback
