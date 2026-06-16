# Phase 1.5: Scope Boundary Validation - EPIC-CCN-107

## Epic Context
- **Epic ID**: EPIC-CCN-107
- **Target Method**: HydrateFromOpenPositions
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Current Complexity**: 31 (CYC)
- **Target Complexity**: ≤ 15 (Jane Street aligned)
- **Violation Severity**: 2.07x over threshold

## Scope Boundary Definition

### Single Method Focus (V12.23 No Scope Creep Protocol)
**IN SCOPE**: HydrateFromOpenPositions method ONLY

**OUT OF SCOPE**:
- Caller methods (defer to separate epics)
- Callee methods (unless trivial inline candidates)
- Related state machine methods
- Position management infrastructure
- Order tracking systems

### Extraction Strategy

#### Primary Extraction Target
**Method**: HydrateFromOpenPositions
**Current State**: Monolithic orchestrator with embedded logic
**Target State**: Thin orchestrator delegating to extracted helpers

#### Extraction Breakdown

1. **Extract Position Validation Logic**
   - **New Method**: ValidatePositionForHydration
   - **Responsibility**: Validate NT position data before hydration
   - **Complexity Reduction**: ~5 CYC
   - **Rationale**: Separate validation concerns from orchestration

2. **Extract State Transition Logic**
   - **New Method**: ApplyPositionStateTransition
   - **Responsibility**: Execute FSM state updates based on position
   - **Complexity Reduction**: ~8 CYC
   - **Rationale**: Isolate state mutation logic (Actor/FSM pattern)

3. **Extract Order Mapping Logic**
   - **New Method**: MapOrderIdToPosition
   - **Responsibility**: Track order ID to position relationships
   - **Complexity Reduction**: ~6 CYC
   - **Rationale**: Separate tracking concerns from orchestration

4. **Extract Risk Calculation Logic**
   - **New Method**: CalculatePositionRisk
   - **Responsibility**: Compute P&L and position size metrics
   - **Complexity Reduction**: ~4 CYC
   - **Rationale**: Isolate financial calculations

#### Post-Refactoring Structure
```
HydrateFromOpenPositions (CYC: 8-12)
├── ValidatePositionForHydration (CYC: ≤5)
├── ApplyPositionStateTransition (CYC: ≤8)
├── MapOrderIdToPosition (CYC: ≤6)
└── CalculatePositionRisk (CYC: ≤4)
```

### Boundary Constraints

#### What to Extract
- Decision logic embedded in HydrateFromOpenPositions
- Validation checks (null checks, state guards)
- State transition logic (FSM updates)
- Order tracking logic
- Risk calculation logic

#### What to Keep in Original Method
- High-level orchestration flow
- Method signature (preserve API contract)
- Top-level error handling
- Logging/telemetry calls
- Actor/FSM enqueue calls (coordination only)

#### What NOT to Touch
- Caller methods (out of scope)
- NinjaTrader Position API usage
- Existing helper methods (unless inlined)
- State machine infrastructure
- Lock-free Actor pattern implementation

## Success Criteria

### Quantitative Metrics
1. **Complexity Target**: HydrateFromOpenPositions CYC ≤ 15
2. **Extracted Methods**: 4 new methods created
3. **Individual Method Complexity**: Each extracted method CYC ≤ 8
4. **Total Complexity**: Sum of all methods ≤ 35 (allow 4 CYC overhead)

### Qualitative Criteria
1. **Single Responsibility**: Each method has one clear purpose
2. **Testability**: Extracted methods are unit-testable in isolation
3. **Readability**: Main method reads like high-level orchestration
4. **V12 DNA Compliance**: Lock-free Actor pattern preserved
5. **ASCII-Only**: All string literals are ASCII-compliant

### Verification Gates
- [ ] Build succeeds without errors
- [ ] All existing tests pass
- [ ] New unit tests cover extracted methods
- [ ] Complexity metrics verified with tooling
- [ ] Manual code review confirms single-method scope

## Risk Assessment

### Risk Level: MEDIUM

#### Risk Factors
1. **State Mutation Complexity**: FSM updates require careful extraction
2. **Position Data Dependencies**: NT Position object usage must be preserved
3. **Race Condition Risk**: Lock-free pattern must remain intact
4. **Test Coverage**: Existing test suite may be insufficient

#### Mitigation Strategies
1. **Incremental Extraction**: Extract one method at a time with verification
2. **TDD Approach**: Write tests before extraction
3. **Actor Pattern Preservation**: Ensure all state mutations use Enqueue
4. **Regression Testing**: Run full test suite after each extraction

#### Rollback Plan
If complexity target not met:
1. Revert to previous commit
2. Re-analyze hotspot with finer granularity
3. Consider alternative extraction boundaries
4. Escalate to V12 Phase 7 Lead for guidance

## V12 DNA Compliance Checklist

### Lock-Free Pattern (CRITICAL)
- [ ] No lock(stateLock) blocks introduced
- [ ] All state mutations use FSM Actor Enqueue
- [ ] No synchronous state reads during mutations
- [ ] Thread-safety preserved through message passing

### ASCII-Only Compliance
- [ ] All string literals checked for non-ASCII characters
- [ ] No Unicode/emoji in error messages
- [ ] Log messages are ASCII-only

### Photon Kernel Alignment
- [ ] Method names follow V12 naming conventions
- [ ] Error handling uses V12 patterns
- [ ] Logging uses V12 telemetry infrastructure

## Implementation Constraints

### File Modification Scope
**ONLY MODIFY**: src/V12_002.SIMA.Lifecycle.cs

**DO NOT MODIFY**:
- Any other source files
- Test files (except adding new tests)
- Configuration files
- Build scripts

### Code Style Requirements
- Follow existing C# coding standards in file
- Maintain consistent indentation (tabs/spaces as per file)
- Preserve existing comment style
- Use XML documentation comments for new methods

## Next Phase Transition

### Phase 2 (Planning) Entry Criteria
- [x] Scope boundary document completed
- [x] Extraction strategy defined
- [x] Success criteria established
- [x] Risk assessment completed
- [ ] User approval of scope boundary

### Phase 2 Deliverables Preview
1. Detailed implementation plan with step-by-step instructions
2. Test strategy for each extracted method
3. Complexity reduction verification plan
4. Rollback procedures

---

**Document Version**: 1.0
**Phase**: 1.5 (Scope Boundary Validation)
**Status**: PENDING_APPROVAL
**Created**: 2026-06-13
**Protocol**: V12.23 No Scope Creep
