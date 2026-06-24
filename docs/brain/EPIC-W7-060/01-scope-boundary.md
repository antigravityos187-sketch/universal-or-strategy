# Phase 1: Scope Boundary - EPIC-W7-060

## Epic Metadata
- **Epic ID**: EPIC-W7-060
- **Target Method**: SweepTrackedOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Current CYC**: 11
- **Target CYC**: ≤8
- **Phase**: 1 (Scope Definition)

## Scope Definition

### IN SCOPE ✅

#### Primary Extraction Target
- **Method**: `SweepTrackedOrders` (line 1308, 46 lines, CYC 11)
- **Purpose**: Cancel orders held in strategy tracking dictionaries
- **Extraction Strategy**: Extract nested conditional logic into helper methods

#### Specific Extractions
1. **Order State Validation Logic**
   - Extract terminal state checks
   - Reduce nesting from 4 to ≤2 levels
   - Target CYC reduction: 3-4 points

2. **Dictionary Iteration Logic**
   - Extract order collection processing
   - Simplify loop structures
   - Target CYC reduction: 2-3 points

3. **Cancellation Dispatch Logic**
   - Extract order cancellation calls
   - Isolate error handling
   - Target CYC reduction: 2-3 points

#### Callers (Must Verify After Refactoring)
- `CancelAllV12GtcOrders` (line 1294, same file)
- `ProcessShutdownSIMA` (line 98, same file)

#### Callees (Dependencies to Preserve)
- `CancelOrderOnAccount` (src/V12_002.Orders.CancelGateway.cs, line 46)
- `IsOrderTerminal` (src/V12_002.Orders.Management.Flatten.cs, line 698)

### OUT OF SCOPE ❌

#### Caller Methods (No Changes)
- **CancelAllV12GtcOrders**: Do NOT modify this caller
- **ProcessShutdownSIMA**: Do NOT modify this caller
- Rationale: Scope creep prevention, single-concern refactoring

#### Callee Methods (No Changes)
- **CancelOrderOnAccount**: Do NOT modify order cancellation gateway
- **IsOrderTerminal**: Do NOT modify state validation logic
- Rationale: These are stable, tested dependencies

#### Other Files
- **V12_002.Orders.CancelGateway.cs**: Out of scope
- **V12_002.Orders.Management.Flatten.cs**: Out of scope
- **Any other V12_002.*.cs files**: Out of scope

#### Behavioral Changes
- **Order Cancellation Semantics**: Must remain identical
- **Error Handling**: Must preserve existing behavior
- **Logging**: Must preserve existing log statements
- **State Transitions**: Must preserve FSM state logic

### Scope Boundary Validation

#### Complexity Budget
- **Starting CYC**: 11
- **Target CYC**: ≤8
- **Required Reduction**: 3+ points
- **Method**: Extract 2-3 helper methods

#### Blast Radius Confirmation
- **External Importers**: 0 (confirmed safe)
- **Direct Dependents**: 0 (confirmed safe)
- **Risk Score**: 0.0 (minimal)
- **Callers**: 2 (both in same file)

#### Jane Street Alignment
- ✅ **Cognitive Simplicity**: CYC ≤8 for microsecond-latency reasoning
- ✅ **Testability**: Smaller methods easier to test exhaustively
- ✅ **Correctness by Construction**: Simpler logic reduces race conditions
- ✅ **Single Responsibility**: Each extracted method has one clear purpose

## Extraction Plan

### Helper Method 1: ValidateOrderForCancellation
**Purpose**: Check if order is in terminal state
**CYC Reduction**: 2-3 points
**Signature**: `private bool ValidateOrderForCancellation(Order order)`

### Helper Method 2: ProcessOrderCancellation
**Purpose**: Execute cancellation and handle errors
**CYC Reduction**: 2-3 points
**Signature**: `private void ProcessOrderCancellation(Order order, string context)`

### Helper Method 3: IterateTrackedOrders (Optional)
**Purpose**: Simplify dictionary iteration logic
**CYC Reduction**: 1-2 points
**Signature**: `private IEnumerable<Order> GetCancellableOrders()`

## Success Criteria

### Quantitative
- ✅ Final CYC ≤8 (currently 11)
- ✅ Max nesting ≤2 (currently 4)
- ✅ Method length ≤30 lines (currently 46)
- ✅ Zero new compilation errors
- ✅ Zero new test failures

### Qualitative
- ✅ Order cancellation semantics preserved
- ✅ Error handling behavior unchanged
- ✅ Logging statements preserved
- ✅ No changes to caller methods
- ✅ No changes to callee methods

## Risk Mitigation

### Pre-Refactoring Checks
1. ✅ Verify build passes: `dotnet build`
2. ✅ Verify tests pass: `dotnet test`
3. ✅ Verify no uncommitted changes in src/

### Post-Refactoring Verification
1. ✅ Run complexity audit: `python scripts/complexity_audit.py`
2. ✅ Run build: `dotnet build`
3. ✅ Run tests: `dotnet test`
4. ✅ Sync hard links: `powershell -File ./deploy-sync.ps1`
5. ✅ F5 in NinjaTrader IDE

## Conclusion

**SCOPE APPROVED**: Proceed to Phase 2 (Architecture Planning)

This scope definition ensures:
- **Focused refactoring**: Only SweepTrackedOrders method
- **No scope creep**: Callers and callees explicitly excluded
- **Clear success criteria**: CYC ≤8, nesting ≤2, length ≤30
- **Risk mitigation**: Low blast radius, comprehensive verification

**Next Phase**: Architecture Planning (Phase 2)
