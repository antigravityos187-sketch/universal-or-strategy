# Phase 1: Scope Definition - EPIC-W7-107

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.12
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:38:23Z
- **Input**: docs/brain/EPIC-W7-107/00-hotspots.md

## Target Method
- **Method**: HydrateFromOpenPositions
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 625
- **Current CYC**: 34
- **Target CYC**: ≤8 per extracted method
- **LOC**: 156

## Scope Boundary Decision

### IN SCOPE ✅

#### 1. Parameter Validation Logic
- **Lines**: ~625-640 (estimated)
- **Reason**: 14 parameters need validation/null checks
- **Extraction Target**: ValidateHydrationParameters()
- **Expected CYC**: 3-5

#### 2. Order Collection Iteration
- **Lines**: ~640-700 (estimated)
- **Reason**: Iterates through 6 order collections (stop, target1-5)
- **Extraction Target**: IterateOrderCollections()
- **Expected CYC**: 6-8

#### 3. Fleet Account Handling
- **Lines**: ~700-750 (estimated)
- **Reason**: IsFleetAccount checks and _followerBrackets logic
- **Extraction Target**: ProcessFleetAccounts()
- **Expected CYC**: 5-7

#### 4. FSM State Hydration
- **Lines**: ~750-780 (estimated)
- **Reason**: Core FSM state restoration from open positions
- **Extraction Target**: HydrateFSMState()
- **Expected CYC**: 6-8

### OUT OF SCOPE ❌

#### 1. Caller Methods
- **HydrateFSMsFromWorkingOrders** (line 787)
- **HydrateWorkingOrdersFromBroker** (line 309)
- **EnumerateApexAccounts** (line 140)
- **Reason**: Zero blast radius - callers are stable and do not need changes

#### 2. Callee Methods (22 methods)
- **IsFleetAccount**, **LogBuffer.Format**, etc.
- **Reason**: These are utility methods used correctly; no refactoring needed

#### 3. Order Collection Classes
- **stopOrders**, **target1Orders**, etc.
- **Reason**: Data structures are stable; only iteration logic needs extraction

#### 4. Logging Infrastructure
- **LogBuffer.Format**, **LogBuffer.ValidateThreadAffinity**
- **Reason**: Logging is cross-cutting; keep as-is for consistency

## Extraction Strategy

### Phase 5 Ticket Breakdown (4 tickets)

**Ticket 1**: Extract Parameter Validation
- Extract parameter null checks and validation
- Target CYC: 3-5
- Risk: LOW (pure validation logic)

**Ticket 2**: Extract Order Collection Iteration
- Extract loop logic for 6 order collections
- Target CYC: 6-8
- Risk: LOW (iteration pattern)

**Ticket 3**: Extract Fleet Account Handling
- Extract IsFleetAccount checks and follower bracket logic
- Target CYC: 5-7
- Risk: MEDIUM (fleet-specific logic)

**Ticket 4**: Extract FSM State Hydration
- Extract core FSM state restoration
- Target CYC: 6-8
- Risk: MEDIUM (core business logic)

### Success Criteria
- Original method CYC reduced from 34 to ≤8
- All extracted methods have CYC ≤8
- Zero blast radius maintained (no caller changes)
- All tests pass
- Build succeeds

## Risk Assessment

### Scope Boundary Risks: **LOW**

**Mitigations**:
1. ✅ Zero blast radius - no external dependencies to break
2. ✅ Clear extraction points - well-defined logic boundaries
3. ✅ Stable callers - no changes needed upstream
4. ✅ Stable callees - no changes needed downstream

### Scope Creep Prevention
- **BANNED**: Refactoring caller methods (out of scope)
- **BANNED**: Refactoring callee utility methods (out of scope)
- **BANNED**: Changing order collection data structures (out of scope)
- **REQUIRED**: Focus ONLY on HydrateFromOpenPositions method body

## Jane Street Alignment

### Complexity Reduction (P0)
- Current CYC 34 → Target CYC ≤8
- Aligns with Jane Street strict standard (CYC ≤8)
- Microsecond-latency reasoning requirement

### Correctness by Construction (P1)
- Extract validation logic to make invalid states unrepresentable
- Parameter validation prevents null reference errors

### Single Responsibility (P1)
- Each extracted method has one clear purpose
- Reduces cognitive load for maintenance

## Conclusion

**Scope is APPROVED for Phase 2 (Architecture Planning)**

This scope definition provides:
- Clear IN SCOPE boundaries (4 extraction targets)
- Clear OUT OF SCOPE boundaries (callers, callees, data structures)
- Low-risk extraction strategy
- Jane Street alignment
- Zero blast radius preservation

Proceed to Phase 2 to design extraction architecture and ticket specifications.
