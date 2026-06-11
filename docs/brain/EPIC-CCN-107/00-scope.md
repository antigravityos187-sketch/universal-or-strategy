# Phase 1: Scope Definition - EPIC-CCN-107

## Epic Metadata
- **Epic ID**: EPIC-CCN-107
- **Target Method**: `HydrateFromOpenPositions`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Lines**: 1044-1165 (122 lines)
- **Current CYC**: 31
- **Target CYC**: ≤ 8 (74% reduction)
- **Risk Level**: HIGH

## Method Purpose
Phase 5 Position Pass: Creates FSMs for accounts with open positions but terminal entry orders. Handles edge case where entry order is cancelled/rejected but position remains open. Implements REAPER grace window (5 minutes) for failed stop order key recovery.

## Scope Boundary (V12.23 Protocol)

### IN SCOPE ✅
1. **Single Method Extraction**: `HydrateFromOpenPositions` only
2. **Structural Refactoring**: Extract helper methods with CYC ≤ 8
3. **Zero Logic Drift**: Pure structural movement, no behavior changes
4. **Parameter Preservation**: All 7 parameters remain unchanged
5. **Return Type**: `int` (positionFsmCreated count)

### OUT OF SCOPE ❌
1. **Adjacent Methods**: No changes to `HydrateFSMsFromWorkingOrders`, `OrchestrateFSMHydration`, or other lifecycle methods
2. **Caller Modifications**: No changes to `HydrateWorkingOrdersFromBroker` (line 1000)
3. **Dictionary Structures**: No changes to `_followerBrackets`, `_orderIdToFsmKey`, or bracket order dictionaries
4. **FSM State Machine**: No changes to `FollowerBracketFSM` struct or state transitions
5. **REAPER Grace Window**: No changes to `_positionPassFailedFirstSeen` logic
6. **Logging Format**: Preserve exact log messages for operational continuity

## Complexity Analysis

### Current Structure (CYC 31)
```
HydrateFromOpenPositions (CYC 31)
├─ foreach Account.All (1)
├─ if !IsFleetAccount (1)
├─ if _followerBrackets.Values.Any (1)
├─ Position acctPos = ... (1)
├─ if acctPos == null (1)
├─ foreach stopOrders.ToArray() (1)
│  ├─ if stopCand == null (1)
│  ├─ if stopCand.Account == null (1)
│  └─ if string.Equals (1)
├─ if recoveredKey == null (1)
├─ if _followerBrackets.ContainsKey (1)
├─ if recoveredStop != null (1)
│  └─ if !string.IsNullOrEmpty (1)
├─ if target1Orders.TryGetValue (1)
│  └─ if !string.IsNullOrEmpty (1)
├─ if target2Orders.TryGetValue (1)
│  └─ if !string.IsNullOrEmpty (1)
├─ if target3Orders.TryGetValue (1)
│  └─ if !string.IsNullOrEmpty (1)
├─ if target4Orders.TryGetValue (1)
│  └─ if !string.IsNullOrEmpty (1)
├─ if target5Orders.TryGetValue (1)
│  └─ if !string.IsNullOrEmpty (1)
└─ Total: 31 decision points
```

### Extraction Strategy
**Target Structure (CYC ≤ 8 per method)**:
1. `HydrateFromOpenPositions` (CYC 5) - Main orchestrator
2. `ShouldSkipAccountForPositionPass` (CYC 3) - Guard checks
3. `RecoverStopOrderKeyForAccount` (CYC 4) - Stop order key recovery
4. `CreateFSMFromOpenPosition` (CYC 3) - FSM construction
5. `LinkBracketOrdersToFSM` (CYC 6) - Target order linking

## Dependencies

### Read Dependencies
- `Account.All` - Fleet account enumeration
- `_followerBrackets` - Existing FSM registry
- `stopOrders`, `target1Orders`-`target5Orders` - Bracket order dictionaries
- `IsFleetAccount()` - Fleet account filter
- `_positionPassFailedFirstSeen` - REAPER grace window tracker

### Write Dependencies
- `_followerBrackets` - FSM registration
- `_orderIdToFsmKey` - Order ID indexing
- `_positionPassFailedFirstSeen` - Grace window tracking
- `ordersIndexed`, `fsmCreated` - Counters (ref parameters)

### No External Callers
- Method is called ONLY by `HydrateFSMsFromWorkingOrders` (line 1042)
- Single call site simplifies refactoring

## Risk Assessment

### Complexity Risks
- **Nested Loops**: Outer account loop + inner stop order scan
- **Conditional Chains**: 5x target order linking blocks (repetitive pattern)
- **State Mutations**: Multiple dictionary updates per iteration
- **Error Handling**: Grace window logic for missing stop orders

### Mitigation Strategy
1. **Extract Guard Checks**: Consolidate skip conditions into single helper
2. **Extract Recovery Logic**: Isolate stop order key recovery loop
3. **Extract FSM Construction**: Separate FSM creation from linking
4. **Extract Linking Logic**: Consolidate 5x target order blocks into loop

### Jane Street Alignment
- **Cognitive Simplicity**: CYC 31 → 8 improves reasoning under latency constraints
- **Exhaustive Testing**: Smaller methods enable complete path coverage
- **Race Condition Auditing**: Simpler control flow reduces concurrency bugs

## Success Criteria

### Functional Requirements
1. ✅ All 122 lines of logic preserved (zero drift)
2. ✅ All 7 parameters unchanged
3. ✅ Return value semantics preserved
4. ✅ REAPER grace window behavior unchanged
5. ✅ FSM registration logic unchanged
6. ✅ Order ID indexing unchanged

### Quality Gates
1. ✅ CYC ≤ 8 for all extracted methods
2. ✅ `dotnet build` passes (zero errors)
3. ✅ `deploy-sync.ps1` ASCII gate passes
4. ✅ F5 in NinjaTrader successful
5. ✅ Pre-push validation passes (13 checks)

### Documentation Requirements
1. ✅ Update `src/AGENTS.md` "Recent Major Refactors" table
2. ✅ Commit message: `[EPIC-CCN-107] ticket-1: extract HydrateFromOpenPositions helpers -- CYC 31->8 [BUILD_TAG]`
3. ✅ Phase 1 completion logged in manifest.json

## Blast Radius

### Direct Impact
- **File**: `src/V12_002.SIMA.Lifecycle.cs` (single file)
- **Method**: `HydrateFromOpenPositions` (lines 1044-1165)
- **Caller**: `HydrateFSMsFromWorkingOrders` (line 1042)

### Indirect Impact
- **REAPER Audit**: Depends on FSM hydration completeness
- **Fleet Lifecycle**: Startup/reconnect path integrity
- **Order Adoption**: Phase 5 position pass correctness

### Zero Impact
- **Master Account**: Separate hydration path (lines 993-1038)
- **Entry Order Pass**: Separate FSM creation path (lines 1168-1280)
- **Fleet Dispatch**: No interaction with dispatch ring
- **Compliance Hub**: No P/L tracking changes

## Validation Plan

### Pre-Refactor
1. Run `python scripts/complexity_audit.py` (baseline CYC 31)
2. Run `dotnet build` (verify clean build)
3. Verify `HydrateFSMsFromWorkingOrders` calls at line 1042

### Post-Refactor
1. Run `python scripts/complexity_audit.py` (verify CYC ≤ 8)
2. Run `dotnet build` (zero errors)
3. Run `powershell -File .\deploy-sync.ps1` (ASCII gate)
4. F5 in NinjaTrader (verify BUILD_TAG)
5. Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast`

### Regression Testing
1. **Scenario 1**: Restart with open positions (no entry orders)
   - Expected: FSMs created, REAPER audit passes
2. **Scenario 2**: Restart with missing stop order keys
   - Expected: Grace window triggered, no false alarms
3. **Scenario 3**: Restart with terminal entry orders
   - Expected: Position pass creates Active FSMs

## Phase 1 Status
✅ **COMPLETE** - Scope defined, boundaries validated, ready for Phase 2 (Architecture Planning)

---

**Next Phase**: Phase 2 (Architecture Planning) - Define extraction plan with method signatures and call graph
