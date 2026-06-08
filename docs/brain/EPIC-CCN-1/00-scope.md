# Epic: EPIC-CCN-1 -- Scope Alignment (REVISION)

**CRITICAL CONTEXT**: This is a REVISION of the original scope document. Previous analysis was based on STALE DATA (19 days old) and incorrectly assumed extracted helpers existed. This document reflects CURRENT CODE STATE as of 2026-06-08.

---

## Code Area

**Target File**: [`src/V12_002.SIMA.Lifecycle.cs`](../../../src/V12_002.SIMA.Lifecycle.cs)  
**Target Method**: [`HydrateFSMsFromWorkingOrders()`](../../../src/V12_002.SIMA.Lifecycle.cs:464-759)  
**Line Range**: 464-759 (296 lines total)  
**Caller**: [`HydrateWorkingOrdersFromBroker()`](../../../src/V12_002.SIMA.Lifecycle.cs:445) at line 445

---

## Validated Problem

### Complexity Metrics (VERIFIED via complexity_audit.py)
- **Cyclomatic Complexity**: **71** (Jane Street threshold: ≤8)
- **Lines of Code**: 296 (actual), not 188 as previously stated
- **Violation Severity**: **BLOCKING** (9x over threshold)
- **M5 Dispatch Candidate**: YES (flagged for Actor/FSM extraction)

### Architectural Reality
This is a **FULL GOD-METHOD EXTRACTION**, not "final polish":
- ❌ NO extracted helpers exist
- ❌ NO sub-methods present
- ❌ 100% inline logic in single 296-line method
- ✅ Zero `lock()` statements (V12 DNA compliant)
- ✅ Private method (isolated blast radius)
- ✅ Single caller (low coupling)

### Method Responsibility
**Phase 5 of SIMA Lifecycle**: Rebuilds `_followerBrackets` FSM dictionary and `_orderIdToFsmKey` index from already-adopted working orders during broker reconnection. Idempotent -- safe to call on every reconnect.

**Two-Pass Algorithm**:
1. **Entry Pass** (lines 469-638): Iterate `entryOrders`, create FSMs for follower positions, link stop/target orders
2. **Position Pass** (lines 641-745): Handle accounts with open positions but terminal entry orders (recovery path)

---

## Extraction Boundaries (IDENTIFIED)

### Pass 1: Entry Order Processing (169 lines)
**Lines 469-638**: Main loop over `entryOrders.ToArray()`

**Sub-Extraction Candidates**:
1. **MapBrokerStateToFSM** (~40 lines, 469-509)
   - Maps `OrderState` enum to `FollowerBracketState`
   - Calculates `hydratedRemainingContracts` from live position
   - CYC: ~8 (nested if/else chain)

2. **CreateFollowerBracketFSM** (~30 lines, 511-541)
   - Instantiates `FollowerBracketFSM` struct
   - Sets initial state, contracts, timestamps
   - Links entry order reference

3. **LinkStopOrder** (~15 lines, 543-557)
   - Retrieves stop order from `stopOrders` dictionary
   - Updates `_orderIdToFsmKey` index
   - Increments `ordersIndexed` counter

4. **LinkTargetOrders** (~80 lines, 559-629)
   - **REPETITIVE PATTERN**: 5x identical blocks for target1-5
   - Each block: TryGetValue → assign to fsm.Targets[N] → index OrderId
   - **HIGH EXTRACTION VALUE**: Collapse to single parameterized helper

5. **RegisterFSM** (~10 lines, 631-638)
   - Adds FSM to `_followerBrackets` dictionary
   - Indexes entry order ID
   - Increments `fsmCreated` counter

### Pass 2: Position Recovery Processing (104 lines)
**Lines 641-745**: Loop over `Account.All` for orphaned positions

**Sub-Extraction Candidates**:
6. **ScanForRecoveryKey** (~40 lines, 661-701)
   - Searches `stopOrders` for account match
   - Handles REAPER grace window for CancelPending stops
   - Updates `_positionPassFailedFirstSeen` dictionary

7. **CreateRecoveryFSM** (~30 lines, 707-737)
   - Similar to CreateFollowerBracketFSM but for terminal entries
   - Links stop + targets (same repetitive pattern as Pass 1)
   - Registers FSM and logs recovery

---

## Scope Boundaries

### IN SCOPE
- ✅ Extract 7 sub-methods from `HydrateFSMsFromWorkingOrders`
- ✅ Reduce CYC from 71 → ≤8 per method
- ✅ Eliminate 5x target-linking repetition (DRY violation)
- ✅ Maintain idempotency guarantees
- ✅ Preserve REAPER grace window logic (Build 999 feature)
- ✅ Keep all state mutations in original method (no new shared state)

### OUT OF SCOPE
- ❌ Refactoring `HydrateWorkingOrdersFromBroker` (caller method, CYC 19)
- ❌ Modifying FSM state machine logic (separate epic)
- ❌ Changing `_followerBrackets` or `_orderIdToFsmKey` data structures
- ❌ Altering REAPER integration points
- ❌ Performance optimization (this is correctness-first)

---

## Risk Level

**ISOLATED** (Low Risk)

**Rationale**:
- Private method (no external callers)
- Single caller within same file
- Zero blast radius (confirmed via jCodemunch)
- No lock() statements to migrate
- Idempotent design (safe to re-run)
- Self-contained state mutations (no cross-file dependencies)

**Mitigation**:
- Extraction preserves exact control flow
- Sub-methods remain private to same class
- No signature changes to public API
- Caller invocation unchanged

---

## V12 DNA Constraints

### Mandatory Compliance
1. **CYC Target**: ≤8 per method (Jane Street GODMODE)
2. **Lock-Free**: ✅ ALREADY COMPLIANT (zero lock() statements)
3. **ASCII-Only**: ✅ ALREADY COMPLIANT (verified in source)
4. **Extraction Floor**: ≥15 LOC per sub-method (all candidates qualify)
5. **Actor/FSM Model**: Method is ACTOR-SERIALIZED (called on strategy thread)

### Jane Street Violations (FLAGGED)
- ❌ **Cognitive Complexity**: CYC 71 = impossible to reason about under microsecond constraints
- ❌ **DRY Violation**: 5x identical target-linking blocks (maintenance hazard)
- ❌ **God-Method Anti-Pattern**: 296 lines = exponential test path growth
- ❌ **Hidden State Mutations**: 4 dictionaries mutated inline (hard to audit)

### Success Criteria
- [ ] All extracted methods: CYC ≤8
- [ ] Parent method: CYC ≤8 (orchestration only)
- [ ] Zero new lock() statements introduced
- [ ] Zero Unicode in string literals
- [ ] All sub-methods ≥15 LOC
- [ ] `deploy-sync.ps1` passes after extraction
- [ ] Complexity audit shows green for entire file

---

## Dependencies & State Mutations

### Read Dependencies (Dictionaries)
- `entryOrders` (ConcurrentDictionary<string, Order>)
- `activePositions` (ConcurrentDictionary<string, PositionInfo>)
- `stopOrders` (ConcurrentDictionary<string, Order>)
- `target1Orders` through `target5Orders` (5x ConcurrentDictionary<string, Order>)
- `Account.All` (NinjaTrader global collection)

### Write Dependencies (Mutated State)
- `_followerBrackets` (ConcurrentDictionary<string, FollowerBracketFSM>) - FSM registry
- `_orderIdToFsmKey` (ConcurrentDictionary<string, string>) - reverse index
- `_positionPassFailedFirstSeen` (ConcurrentDictionary<string, DateTime>) - REAPER grace tracking

### External Calls
- `Print()` - NinjaTrader logging (3 call sites)
- `DateTime.UtcNow` - timestamp generation
- `Math.Max()` / `Math.Abs()` - quantity calculations

---

## Extraction Strategy

### Phase 1: Target-Linking Consolidation
**Priority**: HIGH (eliminates 80 lines of duplication)

Extract repetitive target-linking pattern into:
```csharp
private void LinkTargetOrderToFSM(
    FollowerBracketFSM fsm,
    string entryKey,
    int targetIndex,
    Dictionary<string, Order> targetDict,
    ref int ordersIndexed
)
```

### Phase 2: FSM Creation Helpers
Extract FSM instantiation logic:
- `MapBrokerStateToFSM()` - state mapping + quantity calculation
- `CreateFollowerBracketFSM()` - struct initialization
- `CreateRecoveryFSM()` - position-pass variant

### Phase 3: Order Linking Helpers
Extract order association logic:
- `LinkStopOrder()` - stop order indexing
- `RegisterFSM()` - dictionary insertion + logging

### Phase 4: Recovery Path Extraction
Extract position-pass logic:
- `ScanForRecoveryKey()` - orphaned position detection
- `HandlePositionPassFailure()` - REAPER grace window

---

## Testing Implications

### Current Test Coverage
**ZERO** - No tests exist for `HydrateFSMsFromWorkingOrders` (verified via test audit)

### Post-Extraction Test Requirements
1. **Unit Tests** (per extracted method):
   - `MapBrokerStateToFSM`: 5 OrderState branches
   - `LinkTargetOrderToFSM`: null handling, index updates
   - `ScanForRecoveryKey`: grace window logic

2. **Integration Test** (full hydration cycle):
   - Reconnect scenario with mixed order states
   - Position-pass recovery path
   - Idempotency verification (double-call safety)

3. **Regression Test**:
   - Compare FSM state before/after extraction
   - Verify `_orderIdToFsmKey` index integrity

---

## Sentinel Audit Findings (INCORPORATED)

**Original Failure Root Cause**: Scope document was based on 19-day-old index data. The method had NOT been refactored; all "extracted helpers" were phantom assumptions.

**Corrective Actions Taken**:
1. ✅ Re-indexed file with `index_file` (fresh AST parse)
2. ✅ Verified CYC 71 with `complexity_audit.py`
3. ✅ Confirmed zero lock() statements with PowerShell Select-String
4. ✅ Analyzed actual 296-line method source via `get_symbol_source`
5. ✅ Validated zero blast radius with `get_blast_radius`
6. ✅ Identified 7 extraction candidates from live code structure

**Lesson Learned**: ALWAYS verify code state with jCodemunch tools before planning. Never assume prior refactoring exists.

---

## Next Steps (BLOCKED AT INTAKE GATE)

**DO NOT PROCEED** until Director confirms:
1. Scope boundaries are correct (7 sub-methods, 296-line parent)
2. Risk assessment is accurate (isolated, low-risk)
3. Extraction strategy aligns with V12 DNA (CYC ≤8, no locks)
4. Success criteria are measurable and complete

**After Director Approval**:
- Advance to `/epic-scope-boundary` (Phase 1.5 - MANDATORY anti-pattern gate)
- Then `/epic-plan` (Phase 2 - detailed analysis + approach)

---

**[INTAKE-GATE] Scope alignment complete. Awaiting Director confirmation before planning.**