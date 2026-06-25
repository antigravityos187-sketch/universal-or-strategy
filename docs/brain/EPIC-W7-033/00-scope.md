# Phase 1: Scope Definition - EPIC-W7-033

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:27:22Z
- **Input**: docs/brain/EPIC-W7-033/00-hotspots.md

## Epic Objective
Reduce cyclomatic complexity of `FlattenSinglePosition` from 27 to ≤8 through surgical extraction of decision logic into focused helper methods.

## Target Method
- **Method**: FlattenSinglePosition
- **File**: src/V12_002.Orders.Management.Flatten.cs
- **Line**: 441
- **Current CYC**: 27
- **Target CYC**: ≤8
- **Reduction Required**: 19 points (70% reduction)

## IN SCOPE

### Primary Extraction Targets
1. **Stop Order Validation Logic**
   - Extract conditional checks for stop order state
   - Validate stop order existence and terminal state
   - Target CYC: ≤3 per extracted method

2. **Target Order Cancellation Logic**
   - Extract target order retrieval and cancellation flow
   - Consolidate duplicate cancellation calls
   - Target CYC: ≤4 per extracted method

3. **Position State Validation Logic**
   - Extract position state checks (filled, active)
   - Validate position eligibility for flattening
   - Target CYC: ≤3 per extracted method

4. **Emergency Flatten Logic**
   - Extract emergency flatten decision tree
   - Separate emergency vs normal flatten paths
   - Target CYC: ≤4 per extracted method

### Control Flow Simplification
- Replace nested if/else with early returns
- Use guard clauses for preconditions
- Eliminate redundant conditional branches

### Testing Requirements
- Unit tests for each extracted helper method
- Integration test for FlattenSinglePosition
- Regression test for flatten behavior

## OUT OF SCOPE

### Excluded from This Epic
1. **Caller Methods**
   - FlattenFilledMasterPositions (caller at depth 1)
   - FlattenAll (caller at depth 2)
   - These remain unchanged unless blocking

2. **Callee Methods**
   - LogBuffer.Format (logging infrastructure)
   - RequestStopCancelLifecycleSafe (order cancellation gateway)
   - GetTargetOrdersDictionary (target order retrieval)
   - CancelOrderSafe (order cancellation gateway)
   - IsOrderTerminal (order state validation)
   - All 20 callees remain unchanged

3. **State Management**
   - pendingStopReplacements dictionary
   - stopOrders dictionary
   - activePositions dictionary
   - No changes to state tracking structures

4. **External Dependencies**
   - No changes to method signatures
   - No changes to public API
   - No changes to order lifecycle management

5. **Performance Optimization**
   - No algorithmic changes
   - No caching strategies
   - Focus is complexity reduction, not performance

## Scope Boundaries

### File Boundaries
- **IN SCOPE**: src/V12_002.Orders.Management.Flatten.cs (lines 441-558)
- **OUT OF SCOPE**: All other files in src/

### Method Boundaries
- **IN SCOPE**: FlattenSinglePosition method body only
- **OUT OF SCOPE**: All other methods in the file

### Behavioral Boundaries
- **PRESERVE**: Exact flatten behavior (no logic changes)
- **PRESERVE**: Order cancellation semantics
- **PRESERVE**: Position state transitions
- **PRESERVE**: Logging output format

## Risk Mitigation

### Low Blast Radius Confirmed
- 0 external dependents
- 0 confirmed files affected
- 0 potential files affected
- Overall risk score: 0.0 (LOW)

### Safety Constraints
1. **No Signature Changes**: Method signature remains identical
2. **No Behavioral Changes**: Extracted logic must be semantically equivalent
3. **No State Changes**: State management structures unchanged
4. **No API Changes**: Public interface unchanged

### Rollback Plan
- Git branch: gitbutler/workspace (virtual branch)
- Rollback command: `git reset --hard HEAD~1`
- Verification: `dotnet build && powershell -File .\deploy-sync.ps1`

## Success Criteria

### Complexity Targets
- [ ] FlattenSinglePosition: CYC ≤8 (currently 27)
- [ ] All extracted methods: CYC ≤8
- [ ] No method exceeds CYC 8 in the file

### Testing Targets
- [ ] Unit tests for all extracted methods (minimum 1 per method)
- [ ] Integration test for FlattenSinglePosition passes
- [ ] No regression in flatten behavior

### Build Targets
- [ ] `dotnet build` passes with zero errors
- [ ] `powershell -File .\deploy-sync.ps1` executes successfully
- [ ] F5 in NinjaTrader IDE successful

### Quality Targets
- [ ] ASCII-only compliance maintained
- [ ] No lock() statements introduced
- [ ] CSharpier formatting passes
- [ ] Pre-push validation passes

## Extraction Strategy

### Phase 1: Extract Decision Logic (4 methods)
1. `ValidateStopOrderForFlatten(Order stopOrder)` → CYC ≤3
2. `CancelTargetOrdersForPosition(Position position)` → CYC ≤4
3. `ValidatePositionStateForFlatten(Position position)` → CYC ≤3
4. `ExecuteEmergencyFlatten(Position position)` → CYC ≤4

### Phase 2: Simplify Control Flow
1. Replace nested if/else with early returns
2. Use guard clauses for null checks
3. Consolidate duplicate cancellation calls

### Phase 3: Verify
1. Run complexity audit: `python scripts/complexity_audit.py --threshold 8`
2. Run unit tests: `dotnet test`
3. Run integration test: F5 in NinjaTrader IDE

## Scope Validation

### Scope Creep Prevention
- **ONE EPIC = ONE CONCERN**: Only FlattenSinglePosition complexity reduction
- **NO PRE-EXISTING FIXES**: Do not fix unrelated compilation errors
- **NO ADJACENT IMPROVEMENTS**: Do not refactor caller/callee methods
- **NO WHITESPACE MUTATIONS**: Preserve formatting outside extraction zone

### Director Approval Required For
- Changing method signatures
- Modifying caller methods (FlattenFilledMasterPositions, FlattenAll)
- Modifying callee methods (any of the 20 dependencies)
- Changing state management structures
- Expanding scope beyond FlattenSinglePosition

## Estimated Effort
- **Complexity Reduction**: 4 extracted methods × 30 min = 2 hours
- **Testing**: 4 unit tests + 1 integration test = 1 hour
- **Verification**: Build + deploy + F5 = 30 minutes
- **Total**: ~3.5 hours

## Dependencies
- **Prerequisite**: Hotspot analysis complete (00-hotspots.md)
- **Blocker**: None identified
- **Follow-up**: Phase 2 (Architecture Planning)

## Scope Sign-Off
- **Scope Defined**: 2026-06-24T19:27:22Z
- **Scope Approved**: Pending Phase 1.5 validation
- **Ready for Phase 2**: Pending scope boundary validation
