# Extraction Tickets: EPIC-CCN-028

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 6-8 hours
- **Target Method**: `ProcessFlattenWorkItem_CancelOrders`
- **Current Complexity**: 18 (CYC)
- **Target Complexity**: ≤8 (CYC)

## TICKET-1: Create Result Structs

### Scope
- **Current Method**: `ProcessFlattenWorkItem_CancelOrders`
- **Current CYC**: 18
- **Target CYC**: N/A (infrastructure ticket)
- **Extraction**: Create ValidationResult and CancellationResult structs

### Implementation
1. Add `ValidationResult` struct to `src/V12_002.SIMA.Flatten.cs`:
   - `bool IsValid` - Indicates if validation passed
   - `string FailureReason` - Describes validation failure (empty if valid)
2. Add `CancellationResult` struct to same file:
   - `bool Success` - Indicates if cancellation succeeded
   - `int CancelledCount` - Number of orders successfully cancelled
   - `List<string> Errors` - Collection of error messages (empty if success)
3. Place structs near the top of the class (after fields, before methods)
4. Use ASCII-only string literals
5. Add XML documentation comments for each struct and field

### Acceptance Criteria
- [ ] ValidationResult struct created with IsValid and FailureReason fields
- [ ] CancellationResult struct created with Success, CancelledCount, and Errors fields
- [ ] XML documentation added for both structs
- [ ] ASCII-only compliance verified
- [ ] Build succeeds (`dotnet build`)
- [ ] No behavioral changes (infrastructure only)

### Dependencies
- None (first ticket)

### Estimated Effort
- 30 minutes

---

## TICKET-2: Extract ValidateCancellationRequest Helper

### Scope
- **Current Method**: `ProcessFlattenWorkItem_CancelOrders`
- **Current CYC**: 18
- **Target CYC**: ≤3 (for extracted helper)
- **Extraction**: Extract pre-condition validation logic into dedicated helper

### Implementation
1. Create private method `ValidateCancellationRequest(FlattenWorkItem item)`:
   - Returns: `ValidationResult`
   - Complexity Target: CYC ≤3
2. Extract validation logic from main method:
   - Null/empty checks on work item
   - Verify order state allows cancellation
   - Check FSM state compatibility
3. Return `ValidationResult` with appropriate IsValid and FailureReason
4. Update main method to call helper and handle validation result
5. Add unit tests for all validation paths:
   - Test null work item
   - Test invalid order state
   - Test FSM state incompatibility
   - Test valid work item

### Acceptance Criteria
- [ ] ValidateCancellationRequest method created with CYC ≤3
- [ ] Method is private and stateless (no shared state)
- [ ] Returns ValidationResult struct
- [ ] Main method updated to use helper
- [ ] Unit tests added with 100% branch coverage
- [ ] All tests pass (`dotnet test`)
- [ ] Build succeeds
- [ ] No behavioral changes verified
- [ ] Complexity verified with `python scripts/complexity_audit.py`

### Dependencies
- TICKET-1 must be completed first (requires ValidationResult struct)

### Estimated Effort
- 2 hours

---

## TICKET-3: Extract ExecuteOrderCancellations Helper

### Scope
- **Current Method**: `ProcessFlattenWorkItem_CancelOrders`
- **Current CYC**: 18 → ~10 (after TICKET-2)
- **Target CYC**: ≤5 (for extracted helper)
- **Extraction**: Extract core order cancellation execution logic

### Implementation
1. Create private method `ExecuteOrderCancellations(FlattenWorkItem item)`:
   - Returns: `CancellationResult`
   - Complexity Target: CYC ≤5
   - Parameter: Validated FlattenWorkItem (assumes validation passed)
2. Extract cancellation execution logic from main method:
   - Iterate through orders in work item
   - Invoke NinjaTrader cancellation API for each order
   - Collect cancellation results
   - Handle immediate execution errors
   - Build CancellationResult with success count and errors
3. Update main method to call helper after validation
4. Add unit tests for all execution paths:
   - Test successful cancellation (all orders)
   - Test partial failure (some orders fail)
   - Test total failure (all orders fail)
   - Test empty order list

### Acceptance Criteria
- [ ] ExecuteOrderCancellations method created with CYC ≤5
- [ ] Method is private and stateless (no shared state)
- [ ] Returns CancellationResult struct
- [ ] Main method updated to use helper
- [ ] Unit tests added with 100% branch coverage
- [ ] All tests pass (`dotnet test`)
- [ ] Build succeeds
- [ ] No behavioral changes verified
- [ ] Complexity verified with `python scripts/complexity_audit.py`
- [ ] Lock-free compliance verified (no lock() statements)

### Dependencies
- TICKET-1 must be completed first (requires CancellationResult struct)
- TICKET-2 should be completed first (sequential extraction recommended)

### Estimated Effort
- 2.5 hours

---

## TICKET-4: Extract LogCancellationOutcome Helper

### Scope
- **Current Method**: `ProcessFlattenWorkItem_CancelOrders`
- **Current CYC**: ~10 (after TICKET-3) → ≤8
- **Target CYC**: ≤2 (for extracted helper)
- **Extraction**: Extract logging and diagnostics logic

### Implementation
1. Create private method `LogCancellationOutcome(CancellationResult result)`:
   - Returns: void
   - Complexity Target: CYC ≤2
   - Parameter: CancellationResult from execution
2. Extract logging logic from main method:
   - Log successful cancellations (count, order IDs if available)
   - Log failures with error details
   - Use existing V12 logging infrastructure
   - Simple success/failure branching only
3. Update main method to call helper after execution
4. Verify main method complexity is now ≤8
5. Add unit tests:
   - Test logging for successful cancellation
   - Test logging for failed cancellation
   - Verify correct log levels used

### Acceptance Criteria
- [ ] LogCancellationOutcome method created with CYC ≤2
- [ ] Method is private and stateless (no shared state)
- [ ] Uses existing V12 logging infrastructure
- [ ] Main method updated to use helper
- [ ] Main method complexity reduced to ≤8 (verified with complexity_audit.py)
- [ ] Unit tests added with 100% branch coverage
- [ ] All tests pass (`dotnet test`)
- [ ] Build succeeds
- [ ] No behavioral changes verified
- [ ] ASCII-only compliance verified in log messages

### Dependencies
- TICKET-1 must be completed first (requires CancellationResult struct)
- TICKET-3 should be completed first (sequential extraction recommended)

### Estimated Effort
- 1.5 hours

---

## Final Verification Checklist

After completing all tickets:

- [ ] Run `dotnet csharpier format src/` to enforce formatting
- [ ] Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast` for quality gates
- [ ] Run `python scripts/complexity_audit.py` to verify CYC ≤8 for main method
- [ ] Run `powershell -File .\deploy-sync.ps1` to sync NinjaTrader hard links
- [ ] Verify zero lock() statements: `grep -r "lock(" src/V12_002.SIMA.Flatten.cs`
- [ ] Run full test suite: `dotnet test`
- [ ] Verify FSM state transitions remain correct (integration tests)
- [ ] Check Codacy dashboard for new issues

## Complexity Reduction Summary

| Component | Before | After | Reduction |
|-----------|--------|-------|-----------|
| ProcessFlattenWorkItem_CancelOrders | 18 | ≤8 | 56% |
| ValidateCancellationRequest | N/A | ≤3 | N/A |
| ExecuteOrderCancellations | N/A | ≤5 | N/A |
| LogCancellationOutcome | N/A | ≤2 | N/A |

## Jane Street Alignment

- ✅ **Cognitive Simplicity**: All methods CYC ≤8
- ✅ **Correctness by Construction**: Type-safe result structs
- ✅ **Lock-Free**: Zero lock() statements
- ✅ **Testability**: 100% branch coverage achievable
- ✅ **Microsecond-Latency**: Pure functions, minimal allocation

## Metadata
- **Epic ID**: EPIC-CCN-028
- **Phase**: 4 (Ticket Generation)
- **Status**: COMPLETE
- **Date**: 2026-06-15
- **Total Tickets**: 4
- **Estimated Total Effort**: 6-8 hours
- **Next Phase**: 5 (Ticket Execution)
