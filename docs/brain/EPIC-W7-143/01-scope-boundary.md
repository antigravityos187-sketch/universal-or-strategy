# Phase 1: Scope Boundary - EPIC-W7-143

## Epic Metadata
- **Epic ID**: EPIC-W7-143
- **Target Method**: OnKeyDown
- **File**: src/V12_002.UI.Callbacks.cs
- **Line**: 391
- **Current CYC**: 9
- **Target CYC**: ≤8
- **Blast Radius**: ZERO (framework callback, no direct callers)

## Scope Definition

### IN SCOPE ✅

#### Primary Target
- **OnKeyDown method** (src/V12_002.UI.Callbacks.cs:391)
  - Extract keyboard command dispatch logic
  - Reduce branching complexity from CYC=9 to CYC≤8
  - Maintain framework callback signature

#### Extraction Strategy
- Extract individual command handlers from switch/if-else chains
- Create dedicated methods for each keyboard command
- Preserve existing behavior (no logic changes)
- Maintain Actor/FSM pattern integration

#### Testing Requirements
- Unit tests for extracted command handlers
- Integration test via UI automation (if feasible)
- Verify no regression in keyboard command handling

#### Documentation
- Update method XML comments
- Document extracted handlers
- Add inline comments for command dispatch logic

### OUT OF SCOPE ❌

#### Existing Dependencies (DO NOT MODIFY)
- **HandleTargetAction** (src/V12_002.UI.Callbacks.cs:429)
  - Already called by OnKeyDown
  - Do not refactor this method
  
- **HandleRunnerAction** (src/V12_002.UI.Callbacks.cs:455)
  - Already called by OnKeyDown
  - Do not refactor this method

- **ExecuteTargetAction** (src/V12_002.UI.Callbacks.cs:490)
  - Indirect callee (depth 2)
  - Do not modify

- **Actor/FSM Infrastructure**
  - Enqueue, TryDrain, ScheduleActorDrain methods
  - These are V12 DNA-compliant lock-free patterns
  - Do not touch

#### Related UI Callbacks (DEFER)
- Other keyboard/mouse event handlers in V12_002.UI.Callbacks.cs
- These may have similar complexity issues but are separate epics

#### Framework Integration (DO NOT CHANGE)
- NinjaTrader event handler signature
- Framework callback registration
- Event propagation logic

#### Performance Optimizations (DEFER)
- Command lookup optimization
- Caching strategies
- These are separate concerns

### Boundary Validation

#### What Makes This Scope Safe?
1. **ZERO blast radius** - No downstream dependencies to break
2. **Framework callback** - Signature is fixed by NinjaTrader
3. **Isolated logic** - Command dispatch is self-contained
4. **No cross-cutting concerns** - Doesn't affect other UI handlers

#### What Could Cause Scope Creep?
1. ❌ Refactoring HandleTargetAction or HandleRunnerAction
2. ❌ Modifying Actor/FSM infrastructure
3. ❌ Changing other UI callback methods
4. ❌ Adding new keyboard commands (feature work)

#### Scope Enforcement
- **ONE METHOD ONLY**: OnKeyDown
- **ONE FILE ONLY**: src/V12_002.UI.Callbacks.cs
- **ONE CONCERN**: Reduce CYC from 9 to ≤8
- **NO LOGIC CHANGES**: Pure extraction refactoring

## Risk Assessment

### Refactoring Risk: MINIMAL
- ✅ Zero blast radius (no callers)
- ✅ Framework callback (signature fixed)
- ✅ Moderate complexity (CYC=9, only 1 over threshold)
- ✅ Low nesting (max_nesting=2)
- ✅ Reasonable size (36 lines)

### Rollback Plan
1. Git revert if issues arise
2. No downstream code to fix (zero blast radius)
3. Easy to test in isolation

## Success Criteria

### Functional Requirements
- [ ] OnKeyDown CYC reduced from 9 to ≤8
- [ ] All keyboard commands still work correctly
- [ ] No regression in UI responsiveness
- [ ] Actor/FSM integration preserved

### Code Quality Requirements
- [ ] CSharpier formatting passes
- [ ] No new Roslyn warnings
- [ ] ASCII-only compliance maintained
- [ ] XML documentation complete

### Testing Requirements
- [ ] Unit tests for extracted handlers (minimum 80% coverage)
- [ ] Integration test passes (F5 in NinjaTrader)
- [ ] BUILD_TAG verification successful

### Documentation Requirements
- [ ] Method XML comments updated
- [ ] Inline comments for command dispatch
- [ ] EPIC completion report written

## Extraction Plan Preview

### Expected Extractions (Estimated)
Based on typical keyboard command handlers, expect to extract:
1. Command validation logic → `ValidateKeyCommand()`
2. Individual command handlers → `HandleCommand_X()` methods
3. Command lookup logic → `GetCommandHandler()`

### Expected CYC Reduction
- **Before**: CYC=9 (1 branching point over threshold)
- **After**: CYC≤8 (each extracted method ≤8)
- **Method Count**: +2 to +4 new methods (depending on command count)

## Phase 1 Completion Checklist

- [x] Hotspot analysis reviewed (Phase 0)
- [x] Scope boundary defined (IN SCOPE vs OUT OF SCOPE)
- [x] Risk assessment completed
- [x] Success criteria documented
- [ ] Manifest updated to show Phase 1 complete
- [ ] Ready for Phase 2 (Architecture Planning)

## Notes

This is a **LOW-PRIORITY, LOW-RISK** epic suitable as a learning exercise. The method exceeds CYC≤8 by only 1 point and has zero blast radius, making it ideal for practicing extraction techniques without risk of breaking production code.

The epic serves as a template for higher-priority hotspots (CYC=13-43) identified in Phase 0 analysis.
