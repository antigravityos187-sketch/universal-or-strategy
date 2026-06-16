# Phase 2: Architecture Planning - EPIC-CCN-035

## V12.23 Protocol Compliance

This document defines the extraction architecture for reducing SyncLimitTarget complexity from 17 to ≤8.

## Target Method Analysis

### Current State
- **Method**: SyncLimitTarget
- **File**: src/V12_002.Orders.Management.StopSync.cs
- **Line Range**: 176-304 (128 lines)
- **Cyclomatic Complexity**: 17
- **Primary Issues**:
  - Duplicated switch statement (lines 218-233 and 289-304)
  - Two distinct execution paths (reprice vs new order)
  - Mixed concerns (validation, repricing, submission, state update)

### Original Method Signature
private void SyncLimitTarget(
    string entryName,
    PositionInfo pos,
    int targetNum,
    int targetQty,
    ConcurrentDictionary<string, Order> targetDict,
    Order existingOrder,
    bool hasWorkingOrder,
    ref int refreshed
)

## Extraction Strategy

### Complexity Reduction Target
- **Current**: 17 (single method)
- **Target**: ≤8 per method (Jane Street strict standard)
- **Proposed Distribution**:
  - SyncLimitTarget (orchestrator): ≤5
  - UpdateTargetPrice (helper): ≤2
  - RepriceExistingOrder (helper): ≤6
  - SubmitNewTargetOrder (helper): ≤7
- **Total Budget**: 20 (acceptable for 4 methods vs 17 for 1 monolith)

### Extraction Boundaries

#### Helper 1: UpdateTargetPrice
**Purpose**: Eliminate duplicated switch statement for updating PositionInfo target prices.

**Signature**: private void UpdateTargetPrice(PositionInfo pos, int targetNum, double newPrice)

**Responsibility**:
- Update pos.Target1Price through pos.Target5Price based on targetNum
- Single switch statement (5 cases)
- No side effects beyond PositionInfo mutation

**Complexity**: ≤2 (simple switch with no nested logic)

**Extracted From**: Lines 218-233 and 289-304 (duplicated code)

#### Helper 2: RepriceExistingOrder
**Purpose**: Handle repricing logic for existing working orders.

**Signature**: private void RepriceExistingOrder(Order existingOrder, double newPrice, PositionInfo pos, int targetNum, string entryName, ref int refreshed)

**Responsibility**:
- Check if price change exceeds tick size threshold
- Call ChangeOrder() API
- Update position target price via UpdateTargetPrice()
- Increment refreshed counter on success
- Handle exceptions with logging

**Complexity**: ≤6 (if-else, try-catch, single API call)

**Extracted From**: Lines 203-253

#### Helper 3: SubmitNewTargetOrder
**Purpose**: Handle new order submission when no working order exists.

**Signature**: private void SubmitNewTargetOrder(PositionInfo pos, int targetNum, int targetQty, double newPrice, string entryName, ConcurrentDictionary<string, Order> targetDict)

**Responsibility**:
- Determine order action (Sell vs BuyToCover) based on position direction
- Call SubmitOrderUnmanaged() API
- Store order in targetDict on success
- Update position target price via UpdateTargetPrice()
- Handle exceptions with logging

**Complexity**: ≤7 (if-else for direction, try-catch, single API call)

**Extracted From**: Lines 254-304

## Call Graph

SyncLimitTarget (orchestrator)
├─> CalculateTargetPriceFromPos() [existing, unchanged]
├─> RepriceExistingOrder()
│   ├─> ChangeOrder() [NinjaTrader API]
│   └─> UpdateTargetPrice()
└─> SubmitNewTargetOrder()
    ├─> SubmitOrderUnmanaged() [NinjaTrader API]
    └─> UpdateTargetPrice()

### Data Flow

1. **SyncLimitTarget** (entry point):
   - Validates calculated price
   - Branches on hasWorkingOrder flag
   - Delegates to appropriate helper

2. **RepriceExistingOrder** (hasWorkingOrder = true):
   - Receives: existingOrder, newPrice, pos, targetNum, entryName, ref refreshed
   - Calls: ChangeOrder() if price delta ≥ tickSize
   - Calls: UpdateTargetPrice() on success
   - Returns: void (mutates pos and refreshed)

3. **SubmitNewTargetOrder** (hasWorkingOrder = false):
   - Receives: pos, targetNum, targetQty, newPrice, entryName, targetDict
   - Calls: SubmitOrderUnmanaged() with direction-specific action
   - Calls: UpdateTargetPrice() on success
   - Returns: void (mutates pos and targetDict)

4. **UpdateTargetPrice** (shared utility):
   - Receives: pos, targetNum, newPrice
   - Mutates: pos.Target1Price through pos.Target5Price
   - Returns: void

### Shared State

- **PositionInfo pos**: Mutated by all helpers (thread-safe via caller context)
- **ConcurrentDictionary targetDict**: Thread-safe by design
- **ref int refreshed**: Only modified in caller thread (no concurrency risk)

## Lock-Free Validation

### Current State
✅ **No lock() statements**: Method uses NinjaTrader API calls only
✅ **Thread-safe collections**: ConcurrentDictionary<string, Order> used
✅ **Atomic operations**: ref int refreshed modified in single thread context

### Post-Extraction State
✅ **UpdateTargetPrice**: Pure mutation of caller-owned PositionInfo (no locks)
✅ **RepriceExistingOrder**: Calls thread-safe ChangeOrder() API (no locks)
✅ **SubmitNewTargetOrder**: Calls thread-safe SubmitOrderUnmanaged() API (no locks)
✅ **No new concurrency risks**: All helpers operate on caller-provided state

### FSM/Actor Pattern Compliance
- Method operates within NinjaTrader event-driven model (implicit Actor pattern)
- No explicit FSM state machine (order lifecycle managed by NinjaTrader)
- All state mutations are synchronous within single thread context
- **Verdict**: Lock-free compliance maintained ✅

## Jane Street Alignment

### Cognitive Simplicity Principle
✅ **Single Responsibility**: Each helper has one clear purpose
✅ **Complexity ≤8**: All methods meet strict threshold
✅ **DRY Compliance**: Eliminates duplicated switch statement
✅ **Testability**: Helpers can be unit tested independently

### HFT Microsecond-Latency Requirements
✅ **No additional allocations**: Helpers reuse existing objects
✅ **No lock contention**: Lock-free design preserved
✅ **Minimal call overhead**: 3 private methods (inlined by JIT)
✅ **Predictable execution**: No dynamic dispatch or reflection

### Testing Strategy (Jane Street Standard)
- **UpdateTargetPrice**: Test all 5 target numbers + invalid case
- **RepriceExistingOrder**: Test price delta threshold, API success/failure, exception handling
- **SubmitNewTargetOrder**: Test Long/Short directions, API success/failure, exception handling
- **Integration**: Test SyncLimitTarget orchestration with mocked helpers

## Implementation Plan

### Step 1: Extract UpdateTargetPrice
- Create private method with switch statement
- Replace duplicated code at lines 218-233 and 289-304
- Verify: Complexity ≤2, no behavioral change

### Step 2: Extract RepriceExistingOrder
- Create private method with repricing logic
- Replace lines 203-253 with single method call
- Verify: Complexity ≤6, no behavioral change

### Step 3: Extract SubmitNewTargetOrder
- Create private method with submission logic
- Replace lines 254-304 with single method call
- Verify: Complexity ≤7, no behavioral change

### Step 4: Verify SyncLimitTarget Orchestration
- Confirm reduced complexity ≤5
- Verify functional equivalence (no behavioral changes)
- Run complexity audit: python scripts/complexity_audit.py

## Success Criteria

### Mandatory Checks
- [ ] SyncLimitTarget complexity ≤8 (target: ≤5)
- [ ] All helper methods complexity ≤8
- [ ] Zero lock() statements introduced
- [ ] Zero behavioral changes (functional equivalence)
- [ ] Zero changes to callers/callees
- [ ] Zero changes to method signature
- [ ] Build passes: dotnet build
- [ ] Tests pass: dotnet test
- [ ] Complexity audit passes: python scripts/complexity_audit.py

### Quality Gates
- [ ] CSharpier formatting: dotnet csharpier check src/
- [ ] No whitespace mutations in diff
- [ ] Diff size <10k characters (PR hygiene)
- [ ] Unit tests added for extracted helpers

## Risk Assessment

### Extraction Risk: LOW

**Factors**:
- ✅ Clear extraction boundaries (no ambiguous logic)
- ✅ No cross-method dependencies
- ✅ Duplicated code elimination (low risk)
- ✅ Private methods only (no API surface changes)
- ✅ Incremental extraction (one helper at a time)

### Mitigation Controls
1. **Incremental commits**: One helper extraction per commit
2. **Continuous validation**: Run complexity audit after each extraction
3. **Diff review**: Verify only target method + helpers modified
4. **Test coverage**: Add unit tests for each helper before extraction
5. **Rollback plan**: Git revert if complexity target not met

## Approval Decision

### Status: ✅ READY FOR IMPLEMENTATION

**Rationale**:
1. **Clear architecture**: 3 focused helpers with single responsibilities
2. **Complexity target met**: All methods ≤8 (Jane Street aligned)
3. **Lock-free compliance**: No concurrency risks introduced
4. **Testability**: Helpers can be unit tested independently
5. **Low risk**: Incremental extraction with clear boundaries

### Next Phase
- **Phase 3**: TDD Implementation (APPROVED to proceed)
- **Blocker**: None
- **Dependencies**: Phase 1.5 (Scope Boundary) - COMPLETE

---
**Document Version**: 1.0
**Created**: 2026-06-15
**Validated By**: Sequential Thinking Analysis + Jane Street KB
**Status**: APPROVED - PROCEED TO PHASE 3
