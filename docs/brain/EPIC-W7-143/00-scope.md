# Phase 1: Scope Definition - EPIC-W7-143

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.15
- **API Key**: jCodemunch MCP
- **Execution Time**: ~15 seconds

## Epic Overview
- **Target Method**: OnKeyDown
- **File**: src/V12_002.UI.Callbacks.cs
- **Line**: 391
- **Current CYC**: 9
- **Target CYC**: ≤8
- **Blast Radius**: ZERO (isolated UI callback)

## Scope Boundary Validation

### IN SCOPE ✅

#### Primary Target
- **OnKeyDown method** (src/V12_002.UI.Callbacks.cs:391)
  - Extract keyboard command dispatch logic
  - Reduce CYC from 9 to ≤8
  - Maintain framework callback signature

#### Extraction Candidates
Based on call hierarchy analysis, the following command handlers are IN SCOPE for extraction:
1. **Target command dispatch logic** - Extract to dedicated method
2. **Runner command dispatch logic** - Extract to dedicated method
3. **Any switch/if-else chains** - Extract each branch to separate handler

#### Supporting Methods (Read-Only Analysis)
- `HandleTargetAction` (src/V12_002.UI.Callbacks.cs:429) - ANALYZE ONLY
- `HandleRunnerAction` (src/V12_002.UI.Callbacks.cs:455) - ANALYZE ONLY
- `_keyCommands` constant (src/V12_002.UI.Callbacks.cs:42) - REFERENCE ONLY

### OUT OF SCOPE ❌

#### Framework Integration
- **NinjaTrader event wiring** - Do not modify framework callback registration
- **KeyEventArgs handling** - Do not change parameter signature
- **UI thread context** - Do not modify threading model

#### Downstream Methods
- `ExecuteTargetAction` (src/V12_002.UI.Callbacks.cs:490) - OUT OF SCOPE
- `ExecuteTargetActionForPosition` (src/V12_002.UI.Callbacks.cs:508) - OUT OF SCOPE
- `Enqueue` (src/V12_002.cs:428) - OUT OF SCOPE (Actor pattern core)
- `TryDrain` (src/V12_002.cs:503) - OUT OF SCOPE (Actor pattern core)
- `ScheduleActorDrain` (src/V12_002.cs:481) - OUT OF SCOPE (Actor pattern core)

#### Other Files
- **V12_002.cs** - OUT OF SCOPE (main strategy file)
- **V12_002.Perf.LogBuffer.cs** - OUT OF SCOPE (logging infrastructure)
- **Any test files** - OUT OF SCOPE (will be created in Phase 5)

## Extraction Strategy

### Approach: Command Handler Extraction
The OnKeyDown method likely contains branching logic for different keyboard commands. The strategy is to:

1. **Identify command dispatch pattern** (switch/if-else chain)
2. **Extract each command handler** to dedicated private method
3. **Preserve callback signature** (OnKeyDown remains the entry point)
4. **Maintain Actor/FSM delegation** (calls to HandleTargetAction/HandleRunnerAction)

### Expected Transformations

#### Before (CYC=9)
Method contains 36 lines with branching logic and CYC=9 due to multiple if/else or switch branches.

#### After (CYC≤8)
Simplified dispatch logic with CYC≤8 by extracting command handlers to dedicated methods.

## Risk Assessment

### Refactoring Safety: MAXIMUM
- ✅ **Zero blast radius** - No downstream dependencies
- ✅ **Framework callback** - Isolated entry point
- ✅ **Moderate complexity** - CYC=9 (only 1 point over threshold)
- ✅ **Low nesting** - max_nesting=2 (not deeply nested)
- ✅ **Reasonable size** - 36 lines (not a god-method)

### Testing Strategy
- **Unit Tests**: Test each extracted command handler in isolation
- **Integration Test**: F5 in NinjaTrader IDE to verify keyboard commands still work
- **Rollback Plan**: Git revert if issues arise (zero blast radius makes this safe)

## Success Criteria

### Phase 2 (Architecture Planning)
- [ ] Read full source of OnKeyDown method
- [ ] Identify exact branching pattern (switch vs if-else)
- [ ] Map each branch to command handler name
- [ ] Design extraction plan with method signatures

### Phase 5 (Ticket Execution)
- [ ] Extract command handlers to dedicated methods
- [ ] Verify CYC reduction to ≤8
- [ ] Add unit tests for each handler
- [ ] Run deploy-sync.ps1
- [ ] F5 in NinjaTrader IDE (verify keyboard commands work)

### Phase 6 (Final Review)
- [ ] Confirm CYC ≤8 via complexity_audit.py
- [ ] Verify zero compilation errors
- [ ] Verify zero blast radius impact
- [ ] Update EPIC-W7-143 completion report

## Scope Boundary Justification

### Why This Scope?
1. **Minimal Surface Area**: Only OnKeyDown method is modified
2. **Zero Ripple Effect**: No downstream dependencies to break
3. **Clear Success Metric**: CYC reduction from 9 to ≤8
4. **Safe Learning Target**: Low-risk practice for higher-priority hotspots

### Why Not Broader?
- **HandleTargetAction/HandleRunnerAction** are already separate methods (good design)
- **Actor/FSM pattern** is V12 DNA-compliant (do not touch)
- **Framework integration** is stable (do not modify)

### Why Not Narrower?
- **CYC=9** exceeds Jane Street threshold (must fix)
- **36 lines** suggests extractable logic exists
- **Zero blast radius** makes this a no-brainer refactoring target

## Conclusion

This epic has a **TIGHTLY SCOPED** extraction target:
- **IN SCOPE**: OnKeyDown method only (src/V12_002.UI.Callbacks.cs:391)
- **OUT OF SCOPE**: All downstream methods, framework integration, Actor/FSM core
- **RISK**: MINIMAL (zero blast radius, isolated callback)
- **PRIORITY**: LOW (learning exercise, not critical hotspot)

**Recommendation**: Proceed to Phase 2 (Architecture Planning) to read full source and design extraction plan.
