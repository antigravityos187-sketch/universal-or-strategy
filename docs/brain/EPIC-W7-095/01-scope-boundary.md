# Phase 1: Scope Boundary - EPIC-W7-095

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: N/A
- **Execution Time**: 2026-06-24T01:49:50Z

## Epic Context
- **Target Method**: ProcessSingleFleetRMAAccount
- **File**: src/V12_002.SIMA.Execution.cs
- **Line**: 511
- **Current CYC**: 25 (Target: ≤ 8)
- **Lines**: 168 (Target: ≤ 50)
- **Parameters**: 9 (Target: ≤ 5)

## Scope Definition

### IN SCOPE ✅

#### 1. State Access Extraction (Priority: HIGH)
**Rationale**: 6 dictionary lookups create noise and increase nesting
- Extract `activeFleetAccounts` lookup
- Extract `activePositions` lookup
- Extract `entryOrders` lookup
- Extract `_followerBrackets` lookup
- Extract `expectedPositions` lookup (depth 2)
- Extract `_dispatchSyncPendingExpKeys` lookup (depth 2)

**Target**: Create `GetFleetAccountState()` helper method (CYC ≤ 3)

#### 2. Symmetry Registration Logic (Priority: HIGH)
**Rationale**: Core business logic that can be isolated
- Extract `SymmetryGuardRegisterFollower()` calls
- Extract `GetStableHash()` computation
- Extract symmetry validation logic

**Target**: Create `RegisterFleetSymmetry()` helper method (CYC ≤ 5)

#### 3. Position Management Logic (Priority: MEDIUM)
**Rationale**: Complex position delta calculations
- Extract `AddExpectedPositionDeltaLocked()` logic
- Extract position validation
- Extract fill grace stamping

**Target**: Create `UpdateExpectedPosition()` helper method (CYC ≤ 5)

#### 4. Dispatch Synchronization Logic (Priority: MEDIUM)
**Rationale**: Dispatch state management can be isolated
- Extract `MarkDispatchSyncPending()` calls
- Extract `ClearDispatchSyncPending()` calls
- Extract dispatch key generation via `ExpKey()`

**Target**: Create `ManageDispatchSync()` helper method (CYC ≤ 4)

#### 5. Parameter Reduction (Priority: HIGH)
**Rationale**: 9 parameters indicate multiple responsibilities
- Group related parameters into `FleetRMAContext` struct:
  - `account` (Account)
  - `instrument` (Instrument)
  - `quantity` (int)
  - `orderAction` (OrderAction)
  - `limitPrice` (double)
  - `stopPrice` (double)
  - `oco` (string)
  - `signalName` (string)
  - `fromEntrySignalName` (string)

**Target**: Reduce to 2-3 parameters (context object + essential flags)

### OUT OF SCOPE ❌

#### 1. Logging Infrastructure
**Rationale**: Cross-cutting concern, not part of core complexity
- `LogBuffer.Format()` calls remain in place
- Logging statements are noise, not complexity drivers

#### 2. Caller Refactoring
**Rationale**: Single caller (ExecuteRMAEntryV2) is separate epic
- `ExecuteRMAEntryV2` refactoring is out of scope
- Focus only on ProcessSingleFleetRMAAccount internals

#### 3. Callee Implementation Changes
**Rationale**: Do not modify called methods, only extract calls
- `SymmetryGuardRegisterFollower()` implementation unchanged
- `AddExpectedPositionDeltaLocked()` implementation unchanged
- `MarkDispatchSyncPending()` implementation unchanged
- Only extract and organize the calls, do not change their internals

#### 4. State Dictionary Refactoring
**Rationale**: Global state management is architectural concern
- `activeFleetAccounts` dictionary structure unchanged
- `activePositions` dictionary structure unchanged
- `entryOrders` dictionary structure unchanged
- Only extract lookups, do not change dictionary design

#### 5. Error Handling Additions
**Rationale**: No new error handling unless required for extraction
- Maintain existing error handling patterns
- Do not add defensive checks unless extraction requires them

#### 6. Test File Creation
**Rationale**: Testing is Phase 5.V (Verification)
- No test files created in this epic
- Testing deferred to verification phase

## Extraction Strategy

### Phase 2 Architecture Plan Will Define:
1. **Helper Method Signatures**: Exact signatures for 4 extracted methods
2. **Context Object Design**: FleetRMAContext struct layout
3. **Call Sequence**: Order of helper method calls
4. **State Flow**: How state flows between extracted methods
5. **Error Propagation**: How errors bubble up from helpers

### Target Metrics After Extraction:
- **ProcessSingleFleetRMAAccount CYC**: ≤ 8 (from 25)
- **Helper Method CYC**: Each ≤ 5
- **Parameter Count**: ≤ 3 (from 9)
- **Lines per Method**: ≤ 50 (from 168)
- **Max Nesting Depth**: ≤ 4 (from 5)

## Risk Mitigation

### Low Risk Factors (Advantages):
1. ✅ Zero blast radius (no external dependents)
2. ✅ Single caller (clear entry point)
3. ✅ Well-isolated method (safe to refactor)

### Medium Risk Factors (Manageable):
1. ⚠️ 32 callees (high internal coupling) - Mitigate by extracting in logical groups
2. ⚠️ 7 commits/90 days (moderate churn) - Mitigate by thorough testing
3. ⚠️ Hotspot #27 (complexity × churn) - Mitigate by reducing complexity

### Mitigation Strategy:
1. Extract in small, testable chunks (4 helper methods)
2. Maintain existing behavior (no logic changes)
3. Preserve call order and state flow
4. Use context object to reduce parameter coupling

## Success Criteria

### Phase 1 Complete When:
- ✅ Scope boundary clearly defined (IN SCOPE vs OUT OF SCOPE)
- ✅ 4 extraction targets identified
- ✅ Parameter reduction strategy defined
- ✅ Risk mitigation strategy documented

### Epic Complete When (Phase 6):
- ✅ ProcessSingleFleetRMAAccount CYC ≤ 8
- ✅ All helper methods CYC ≤ 5
- ✅ Parameter count ≤ 3
- ✅ Build passes (no compilation errors)
- ✅ F5 in NinjaTrader successful
- ✅ deploy-sync.ps1 executed successfully

## Next Steps (Phase 2)

1. Design `FleetRMAContext` struct layout
2. Define 4 helper method signatures
3. Plan call sequence and state flow
4. Create architecture diagrams (Mermaid)
5. Document error propagation strategy

---

**Phase 1 Status**: ✅ COMPLETE
**Generated**: 2026-06-24T01:49:50Z
**Agent**: v12-phase1-scope
