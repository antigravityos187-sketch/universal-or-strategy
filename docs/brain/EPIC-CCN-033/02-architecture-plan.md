# Phase 2: Architecture Planning - EPIC-CCN-033

## Target Method Analysis

### Current State
- Method: FlattenSinglePosition
- File: src/V12_002.Orders.Management.Flatten.cs
- Current CCN: 16 (exceeds threshold by 1)
- Current LOC: 76
- Target CCN: ≤8 (Jane Street strict standard)

### Method Signature (Original)
private void FlattenSinglePosition(string entryName, PositionInfo pos)

## Extraction Strategy

### Complexity Reduction Plan
Goal: Reduce CCN from 16 to ≤8 through surgical extraction of 3 helper methods.

Rationale: The method currently handles 4 distinct responsibilities:
1. Stop and target order cancellation (CCN ~5)
2. Position quantity validation and calculation (CCN ~4)
3. Market order creation and submission (CCN ~2)
4. Orchestration and logging (CCN ~5)

By extracting responsibilities 1-3 into separate methods, the main method will focus solely on orchestration, reducing its CCN to ~7.

### Extracted Helper Methods

#### 1. CancelStopAndTargetOrders
Purpose: Consolidate all stop and target order cancellation logic.

Signature: private void CancelStopAndTargetOrders(string entryName, PositionInfo pos)

Responsibilities:
- Request stop cancellation via RequestStopCancelLifecycleSafe(entryName)
- Clear pending stop replacements from pendingStopReplacements dictionary
- Iterate through target orders (T1-T5) and cancel working/accepted/submitted orders
- Decrement pendingReplacementCount atomically when clearing replacements

Estimated CCN: 3 (one conditional for TryRemove, nested conditionals in target loop)

Lock-Free Compliance:
- Uses TryRemove on ConcurrentDictionary (lock-free)
- Uses Interlocked.Decrement for atomic counter update
- Uses TryGetValue on concurrent dictionaries
- No lock() statements

#### 2. ValidateAndCalculateFlattenQuantity
Purpose: Validate position state and calculate safe flatten quantity.

Signature: private int ValidateAndCalculateFlattenQuantity(PositionInfo pos)

Responsibilities:
- Read live position quantity from Position.Quantity (with exception handling)
- Compare cached pos.RemainingContracts with live quantity
- Apply V10 FLATTEN FIX logic (trust cached contracts if live is 0)
- Log diagnostic information for troubleshooting
- Return safe flatten quantity

Estimated CCN: 4 (try-catch, position null check, market position check, quantity comparison)

Lock-Free Compliance:
- Read-only access to Position property (NinjaTrader API, thread-safe)
- No shared mutable state
- Exception handling for safe property access
- No lock() statements

#### 3. SubmitFlattenMarketOrder
Purpose: Create and submit the market order to close the position.

Signature: private void SubmitFlattenMarketOrder(string entryName, PositionInfo pos, int flattenQty)

Responsibilities:
- Validate flattenQty > 0 before submission
- Determine order direction based on pos.Direction (Long to ExitLong, Short to ExitShort)
- Call SubmitOrderUnmanaged with appropriate parameters
- Store order reference in flattenOrders dictionary
- Log order submission

Estimated CCN: 2 (quantity check, direction conditional)

Lock-Free Compliance:
- Uses ConcurrentDictionary for flattenOrders storage
- Calls NinjaTrader API (assumed thread-safe)
- No shared mutable state beyond concurrent collections
- No lock() statements

## Call Graph

### Sequential Execution Flow
FlattenSinglePosition(entryName, pos)
- Log flatten intent
- CancelStopAndTargetOrders(entryName, pos)
  - RequestStopCancelLifecycleSafe(entryName)
  - pendingStopReplacements.TryRemove(entryName, out _)
  - Loop T1-T5: CancelOrderSafe(tOrder, pos)
- ValidateAndCalculateFlattenQuantity(pos) returns flattenQty
  - Read Position.Quantity with exception handling
  - Compare cached vs live quantities
  - Log diagnostic information
- SubmitFlattenMarketOrder(entryName, pos, flattenQty)
  - Validate flattenQty > 0
  - SubmitOrderUnmanaged(...)
  - flattenOrders[entryName] = order

### Data Flow
1. Input: entryName (string), pos (PositionInfo)
2. Cancellation Phase: entryName and pos passed to CancelStopAndTargetOrders
3. Validation Phase: pos passed to ValidateAndCalculateFlattenQuantity, returns flattenQty (int)
4. Submission Phase: entryName, pos, and flattenQty passed to SubmitFlattenMarketOrder

### Shared State
- Read-Only: Position property (NinjaTrader API)
- Concurrent Collections (lock-free):
  - pendingStopReplacements (ConcurrentDictionary)
  - targetOrders1-5 (ConcurrentDictionary)
  - flattenOrders (ConcurrentDictionary)
- Atomic Counters: pendingReplacementCount (Interlocked operations)

## Complexity Analysis

### Before Extraction
- FlattenSinglePosition CCN: 16
- Breakdown:
  - Stop cancellation logic: 2 branches
  - Target cancellation loop: 5 branches (loop + nested conditionals)
  - Position validation: 4 branches (try-catch + null checks + comparisons)
  - Order submission: 3 branches (quantity check + direction + dictionary storage)
  - Orchestration: 2 branches

### After Extraction
- FlattenSinglePosition CCN: ~7 (orchestration + 3 method calls)
- CancelStopAndTargetOrders CCN: 3
- ValidateAndCalculateFlattenQuantity CCN: 4
- SubmitFlattenMarketOrder CCN: 2
- Total CCN: 16 (unchanged, but distributed across 4 methods)
- Max Method CCN: 7 (meets Jane Street ≤8 threshold)

## Lock-Free Validation

### V12 DNA Compliance Checklist
- No lock() statements: All extracted methods use lock-free primitives
- FSM/Actor Enqueue pattern: Methods operate on immutable parameters
- Atomic primitives: Interlocked.Decrement for counter updates
- Concurrent collections: ConcurrentDictionary.TryRemove, TryGetValue
- Thread-safe APIs: NinjaTrader SubmitOrderUnmanaged, Position property

### Race Condition Analysis
- Cancellation Phase: Lock-free dictionary operations prevent race conditions
- Validation Phase: Read-only access to Position (no mutation)
- Submission Phase: Concurrent dictionary storage is thread-safe
- No shared mutable state between extracted methods (parameters passed explicitly)

## Jane Street Alignment

### Cognitive Simplicity Principles
1. Single Responsibility: Each extracted method has one clear purpose
   - Cancellation: Stop and target order cleanup
   - Validation: Position quantity calculation
   - Submission: Market order creation
2. CCN ≤8 Target: All methods meet Jane Street strict complexity threshold
3. Explicit Data Flow: No hidden dependencies, all parameters passed explicitly
4. Verifiable Logic: Simple conditionals enable exhaustive testing

### Make Illegal States Unrepresentable
- Separation of Concerns: Validation cannot accidentally submit orders
- Type Safety: flattenQty (int) explicitly passed to submission method
- Fail-Fast: Quantity validation happens before order submission
- No Implicit State: All methods operate on explicit parameters

### HFT Microsecond-Latency Considerations
- Minimal Allocations: No new objects created in hot path
- Inline Candidates: Small methods (CCN ≤8) are JIT inline-friendly
- Cache Locality: Sequential execution reduces branch mispredictions
- Lock-Free: Zero contention on critical path

## Testing Strategy

### Unit Test Coverage (TDD)
1. CancelStopAndTargetOrders:
   - Test stop cancellation request
   - Test pending replacement removal (atomic decrement)
   - Test target order cancellation loop (T1-T5)
   - Test concurrent dictionary operations

2. ValidateAndCalculateFlattenQuantity:
   - Test cached quantity when live is 0 (V10 FIX)
   - Test exception handling for Position access
   - Test quantity comparison logic
   - Test diagnostic logging

3. SubmitFlattenMarketOrder:
   - Test quantity validation (flattenQty > 0)
   - Test direction logic (Long to ExitLong, Short to ExitShort)
   - Test order submission and dictionary storage
   - Test zero quantity early return

### Integration Test Coverage
- Test full FlattenSinglePosition flow with extracted methods
- Verify CCN reduction (16 to 7)
- Validate lock-free compliance (no deadlocks under load)
- Stress test concurrent flatten requests

## Implementation Plan

### Phase 3: Incremental Extraction (Bob CLI)
1. Extract CancelStopAndTargetOrders:
   - Create method with signature
   - Move cancellation logic
   - Update main method to call helper
   - Run tests, commit checkpoint

2. Extract ValidateAndCalculateFlattenQuantity:
   - Create method with signature
   - Move validation logic
   - Update main method to call helper
   - Run tests, commit checkpoint

3. Extract SubmitFlattenMarketOrder:
   - Create method with signature
   - Move submission logic
   - Update main method to call helper
   - Run tests, commit checkpoint

4. Verify CCN Reduction:
   - Run python scripts/complexity_audit.py
   - Confirm FlattenSinglePosition CCN ≤8
   - Run powershell build_readiness.ps1

### Rollback Strategy
- Checkpointing: Enabled via .bob/settings.json
- Git Commits: After each successful extraction
- Restore Points: Bob CLI /restore command available
- Test Verification: Tests must pass before each commit

## Success Criteria

### Phase 2 Completion
- Extraction strategy defined (3 helper methods)
- Method signatures designed with proper types
- Call graph documented (sequential flow)
- Lock-free compliance validated (no lock() statements)
- Jane Street alignment verified (CCN ≤8, cognitive simplicity)
- Testing strategy outlined (unit + integration)

### Phase 3 Readiness
- Architecture plan approved by Director
- Helper method signatures finalized
- Extraction order determined (cancellation to validation to submission)
- Checkpointing enabled for safe incremental extraction

---

Phase: 2.0 (Architecture Planning)
Status: COMPLETE
Next Phase: 3.0 (DNA & PR Audit via Arena AI)
Date: 2026-06-15
Architect: Bob CLI (v12-engineer mode)
