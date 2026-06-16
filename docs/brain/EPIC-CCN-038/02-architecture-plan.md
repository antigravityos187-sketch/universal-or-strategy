# Phase 2: Architecture Planning - EPIC-CCN-038

## Method Analysis

### Target Method
- **Method**: `MoveSpecificTarget`
- **File**: `src/V12_002.Trailing.Breakeven.cs`
- **Current Complexity**: 12 (80% of V12 threshold)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Lines of Code**: 81 lines
- **Access Modifier**: `private`

### Current Structure
```csharp
private void MoveSpecificTarget(int targetNum, double profitPoints)
{
    // Step 1: Validate request
    // Step 2: Iterate through all active positions
    // Step 3: Find target order (per position)
    // Step 4: Calculate and validate new price (per position)
    // Step 5: Execute move (per position)
    // Step 6: Summary reporting
}
```

### Complexity Drivers
1. **Main Loop**: `foreach` over `activePositions.ToArray()` (complexity +1)
2. **Position Existence Check**: `if (!activePositions.ContainsKey(kvp.Key))` (complexity +1)
3. **Null Order Check**: `if (targetOrder == null)` (complexity +1)
4. **Null Reason Check**: `if (notFoundReason != null)` (complexity +1)
5. **Price Validation Check**: `if (!CalculateAndValidateNewTargetPrice(...))` (complexity +1)
6. **Rejection Reason Check**: `if (rejectionReason != null)` (complexity +1)
7. **Follower Branch**: `if (pos.IsFollower && pos.ExecutingAccount != null)` (complexity +1)
8. **Try-Catch Block**: Exception handling (complexity +1)
9. **Summary Reporting Check**: `if (movedCount > 0)` (complexity +1)

**Total Complexity**: 12 (9 decision points + 3 from nested structure)

## Extraction Strategy

### Approach: Single Helper Method Extraction
Extract the per-position processing logic (Steps 3-5) into a dedicated helper method. This isolates the loop body complexity and transforms the main method into a simple orchestrator.

### Rationale
- **Cognitive Simplicity**: Main method becomes a clear orchestrator (validate → loop → report)
- **Single Responsibility**: Helper method handles all per-position logic
- **Testability**: Helper can be unit tested independently
- **Maintainability**: Changes to position processing logic are isolated

### Expected Complexity Reduction
- **Main Method**: 12 → 5-6 (validation + simple loop + reporting)
- **Helper Method**: 6-7 (find + calculate + execute + error handling)
- **Both Methods**: ≤8 (Jane Street compliant)

## Method Signatures

### Original Method (Preserved)
```csharp
/// <summary>
/// [Phase7-S5-T10] Moves a specific target number to a new profit level for all active positions.
/// </summary>
/// <param name="targetNum">Target number to move (1-4)</param>
/// <param name="profitPoints">Profit points to add to entry price</param>
private void MoveSpecificTarget(int targetNum, double profitPoints)
```

**Signature Preservation**: ✅ Public interface unchanged

### Proposed Helper Method
```csharp
/// <summary>
/// [EPIC-CCN-038] Processes target move for a single position.
/// Encapsulates Steps 3-5: find order, calculate price, execute move.
/// </summary>
/// <param name="pos">Position information</param>
/// <param name="entryName">Position entry name (dictionary key)</param>
/// <param name="targetNum">Target number to move (1-4)</param>
/// <param name="profitPoints">Profit points to add to entry price</param>
/// <returns>True if target was successfully moved, false otherwise</returns>
private bool ProcessPositionTargetMove(
    PositionInfo pos,
    string entryName,
    int targetNum,
    double profitPoints
)
```

**Design Decisions**:
- **Return Type**: `bool` for success/failure counting
- **Access Modifier**: `private` (internal helper)
- **Parameters**: All data needed for per-position processing
- **Responsibility**: Find order → Calculate price → Execute move

## Call Graph

### Before Extraction
```
MoveSpecificTarget
├─→ ValidateMoveTargetRequest (existing helper)
├─→ [LOOP] foreach position
│   ├─→ FindTargetOrderForPosition (existing helper)
│   ├─→ CalculateAndValidateNewTargetPrice (existing helper)
│   ├─→ ExecuteFollowerTargetMove (existing helper)
│   └─→ ExecuteMasterTargetMove (existing helper)
└─→ Print (summary reporting)
```

### After Extraction
```
MoveSpecificTarget
├─→ ValidateMoveTargetRequest (existing helper)
├─→ [LOOP] foreach position
│   └─→ ProcessPositionTargetMove (NEW helper)
│       ├─→ FindTargetOrderForPosition (existing helper)
│       ├─→ CalculateAndValidateNewTargetPrice (existing helper)
│       ├─→ ExecuteFollowerTargetMove (existing helper)
│       └─→ ExecuteMasterTargetMove (existing helper)
└─→ Print (summary reporting)
```

### Data Flow
1. **Main Method** validates request and iterates positions
2. **Helper Method** receives position data and target parameters
3. **Helper Method** calls existing helpers for find/calculate/execute
4. **Helper Method** returns success/failure boolean
5. **Main Method** counts successes and reports summary

### Shared State
- **Read-Only Access**: `activePositions` (snapshot via `.ToArray()`)
- **No Shared Mutable State**: All data passed via parameters
- **Lock-Free**: No synchronization primitives needed

## Implementation Plan

### Step 1: Extract Helper Method
Create `ProcessPositionTargetMove` with the following structure:
```csharp
private bool ProcessPositionTargetMove(
    PositionInfo pos,
    string entryName,
    int targetNum,
    double profitPoints
)
{
    // Step 3: Find target order
    Order targetOrder = FindTargetOrderForPosition(pos, entryName, targetNum, out string notFoundReason);
    if (targetOrder == null)
    {
        if (notFoundReason != null)
            Print(notFoundReason);
        return false;
    }

    // Step 4: Calculate and validate new price
    if (!CalculateAndValidateNewTargetPrice(
        pos,
        profitPoints,
        targetNum,
        out double newTargetPrice,
        out string rejectionReason
    ))
    {
        if (rejectionReason != null)
            Print(rejectionReason);
        return false;
    }

    // Step 5: Execute move (follower FSM vs master ChangeOrder)
    try
    {
        if (pos.IsFollower && pos.ExecutingAccount != null)
        {
            ExecuteFollowerTargetMove(pos, entryName, targetNum, targetOrder, newTargetPrice);
        }
        else
        {
            ExecuteMasterTargetMove(pos, entryName, targetNum, targetOrder, newTargetPrice);
        }
        return true;
    }
    catch (Exception ex)
    {
        Print($"[V14] MoveSpecificTarget T{targetNum}: Move FAILED for {entryName} - {ex.Message}");
        return false;
    }
}
```

### Step 2: Simplify Main Method
Refactor `MoveSpecificTarget` to use the helper:
```csharp
private void MoveSpecificTarget(int targetNum, double profitPoints)
{
    // Step 1: Validate request
    if (!ValidateMoveTargetRequest(targetNum, out string errorMsg))
    {
        Print(errorMsg);
        return;
    }

    int movedCount = 0;

    // Step 2: Iterate through all active positions
    foreach (var kvp in activePositions.ToArray())
    {
        if (!activePositions.ContainsKey(kvp.Key))
            continue;

        PositionInfo pos = kvp.Value;
        string entryName = kvp.Key;

        // Step 3-5: Process position (delegated to helper)
        if (ProcessPositionTargetMove(pos, entryName, targetNum, profitPoints))
        {
            movedCount++;
        }
    }

    // Step 6: Summary reporting
    if (movedCount > 0)
    {
        Print($"[V14] MoveSpecificTarget T{targetNum}: Moved {movedCount} target(s) to +{profitPoints}pt profit");
    }
    else
    {
        Print($"[V14] MoveSpecificTarget T{targetNum}: No targets were moved (no active working orders found)");
    }
}
```

### Step 3: Verify Complexity Reduction
**Main Method Complexity** (after extraction):
1. Validation check: `if (!ValidateMoveTargetRequest(...))` (+1)
2. Loop: `foreach (var kvp in activePositions.ToArray())` (+1)
3. Position exists check: `if (!activePositions.ContainsKey(kvp.Key))` (+1)
4. Helper success check: `if (ProcessPositionTargetMove(...))` (+1)
5. Summary check: `if (movedCount > 0)` (+1)

**Total Main Method Complexity**: 5 ✅ (≤8 Jane Street compliant)

**Helper Method Complexity**:
1. Null order check: `if (targetOrder == null)` (+1)
2. Null reason check: `if (notFoundReason != null)` (+1)
3. Price validation check: `if (!CalculateAndValidateNewTargetPrice(...))` (+1)
4. Rejection reason check: `if (rejectionReason != null)` (+1)
5. Follower branch: `if (pos.IsFollower && pos.ExecutingAccount != null)` (+1)
6. Try-catch block: Exception handling (+1)

**Total Helper Method Complexity**: 6 ✅ (≤8 Jane Street compliant)

## Lock-Free Validation

### Current Lock-Free Patterns
✅ **No `lock()` Statements**: Method uses lock-free iteration
✅ **Snapshot Iteration**: `activePositions.ToArray()` creates safe snapshot
✅ **FSM/Actor Pattern**: Delegates to `ExecuteFollowerTargetMove` (FSM Enqueue)
✅ **Atomic Primitives**: No shared mutable state in method body

### Post-Extraction Lock-Free Compliance
✅ **Helper Method**: No locks, no shared mutable state
✅ **Main Method**: Preserves snapshot iteration pattern
✅ **Existing Helpers**: No changes to lock-free execution paths
✅ **Data Flow**: All data passed via parameters (no shared state)

### FSM/Actor Integration
- **Follower Path**: `ExecuteFollowerTargetMove` uses FSM Enqueue pattern
- **Master Path**: `ExecuteMasterTargetMove` uses direct ChangeOrder
- **No Changes**: Extraction preserves existing FSM integration

## Jane Street Compliance

### Cognitive Simplicity ✅
- **Main Method**: Clear orchestrator pattern (validate → loop → report)
- **Helper Method**: Single responsibility (process one position)
- **Both Methods**: CYC ≤8 (strict Jane Street standard)
- **Reasoning**: Each method has one clear purpose, easy to understand

### Microsecond-Latency Requirements ✅
- **No Additional Overhead**: Helper method is inlined by JIT compiler
- **No Allocations**: All parameters passed by value or reference
- **No Locks**: Preserves lock-free hot path
- **No Branching Changes**: Same execution paths, just reorganized

### Testing Standards ✅
- **Unit Testable**: Helper method can be tested independently
- **Edge Cases**: Each method has clear boundary conditions
- **Regression Suite**: Existing tests validate behavior preservation
- **Integration Tests**: Main method behavior unchanged

### Jane Street Knowledge Base Insights
**Query**: "testing cognitive simplicity"
**Document**: "Why Testing Is Hard and How to Fix It" (Will Wilson)

**Key Principles Applied**:
1. **Simplicity**: Each method has one clear responsibility
2. **Testability**: Helper method isolates per-position logic
3. **Maintainability**: Changes to position processing are isolated
4. **Cognitive Load**: Reduced decision points per method

## V12 DNA Alignment

### Correctness by Construction ✅
- **Clear Contracts**: Helper method has explicit input/output contract
- **Type Safety**: All parameters strongly typed
- **Return Value**: Boolean clearly indicates success/failure
- **No Implicit State**: All data passed explicitly

### ASCII-Only Compliance ✅
- **No Unicode**: All string literals use ASCII characters
- **No Emoji**: No decorative characters in code
- **No Curly Quotes**: Standard ASCII quotes only

### Hard-Link Integrity ✅
- **Post-Modification**: Run `powershell -File .\deploy-sync.ps1`
- **Verification**: Confirm NinjaTrader hard links synchronized
- **Build Test**: F5 in NinjaTrader to verify compilation

## Testing Strategy

### Unit Tests (New)
Create `tests/V12_Performance.Tests/Trailing/MoveSpecificTargetTests.cs`:

```csharp
[TestClass]
public class MoveSpecificTargetTests
{
    [TestMethod]
    public void ProcessPositionTargetMove_ValidPosition_ReturnsTrue()
    {
        // Arrange: Valid position with working target order
        // Act: Call ProcessPositionTargetMove
        // Assert: Returns true, target moved
    }

    [TestMethod]
    public void ProcessPositionTargetMove_NullTargetOrder_ReturnsFalse()
    {
        // Arrange: Position with no matching target order
        // Act: Call ProcessPositionTargetMove
        // Assert: Returns false, no move attempted
    }

    [TestMethod]
    public void ProcessPositionTargetMove_InvalidPrice_ReturnsFalse()
    {
        // Arrange: Position with invalid new target price
        // Act: Call ProcessPositionTargetMove
        // Assert: Returns false, no move attempted
    }

    [TestMethod]
    public void ProcessPositionTargetMove_FollowerPath_UsesFSM()
    {
        // Arrange: Follower position with executing account
        // Act: Call ProcessPositionTargetMove
        // Assert: ExecuteFollowerTargetMove called (FSM path)
    }

    [TestMethod]
    public void ProcessPositionTargetMove_MasterPath_UsesChangeOrder()
    {
        // Arrange: Master position (non-follower)
        // Act: Call ProcessPositionTargetMove
        // Assert: ExecuteMasterTargetMove called (direct path)
    }

    [TestMethod]
    public void ProcessPositionTargetMove_ExceptionThrown_ReturnsFalse()
    {
        // Arrange: Position that triggers exception in execute
        // Act: Call ProcessPositionTargetMove
        // Assert: Returns false, exception caught and logged
    }
}
```

### Integration Tests (Existing)
Verify existing tests still pass:
- `MoveSpecificTarget_ValidRequest_MovesTargets`
- `MoveSpecificTarget_NoActivePositions_NoMoves`
- `MoveSpecificTarget_InvalidTargetNum_PrintsError`

### Regression Tests
- **Before Extraction**: Run full test suite, capture baseline
- **After Extraction**: Run full test suite, verify identical behavior
- **Complexity Audit**: Run `python scripts/complexity_audit.py`
- **Build Verification**: Run `powershell -File .\scripts\build_readiness.ps1`

## Risk Assessment

### Low Risk ✅
- **Single Method**: Only MoveSpecificTarget body modified
- **Signature Preserved**: Public interface unchanged
- **Behavior Preserved**: Exact functional equivalence
- **Existing Helpers**: No changes to called methods

### Medium Risk ⚠️
- **Critical Path**: Method is part of trailing stop logic
- **Production Impact**: Used in live trading scenarios
- **Testing Required**: Comprehensive unit + integration tests

### Mitigation Strategies
1. **Checkpointing**: Bob CLI auto-checkpoint before changes
2. **Incremental Testing**: Test after each extraction step
3. **Rollback Plan**: Git restore if tests fail
4. **Manual Verification**: F5 in NinjaTrader after deployment

## Success Criteria

### Phase 2 Completion ✅
- [x] Architecture plan created
- [x] Method signatures defined
- [x] Call graph documented
- [x] Complexity reduction validated
- [x] Lock-free compliance verified
- [x] Jane Street alignment confirmed
- [x] Testing strategy defined

### Phase 3 Prerequisites
- [ ] Architecture plan reviewed by Director
- [ ] DNA & PR Audit passed (Arena AI)
- [ ] Test cases approved
- [ ] Extraction strategy validated

## Next Steps

1. **Phase 3: DNA & PR Audit** (Arena AI)
   - Verify plan against V12 DNA constraints
   - Validate PR health (diff size, scope boundaries)
   - Approve or reject architecture plan

2. **Phase 4: Recursive Execution** (Bob CLI)
   - Extract `ProcessPositionTargetMove` helper method
   - Refactor `MoveSpecificTarget` to use helper
   - Run complexity audit to verify CYC ≤8

3. **Phase 5: Verification/Review** (Bob CLI + Orchestrator)
   - Compare implementation against this plan
   - Run full test suite
   - Verify lock-free compliance

4. **Phase 6: Sign-off** (Director)
   - Run `powershell -File .\deploy-sync.ps1`
   - F5 in NinjaTrader
   - Verify BUILD_TAG

## Metadata
- **Epic ID**: EPIC-CCN-038
- **Phase**: 2 (Architecture Planning)
- **Protocol Version**: V12.23
- **Planning Date**: 2026-06-15
- **Planner**: Bob Shell (v12-engineer mode)
- **Next Phase**: Phase 3 (DNA & PR Audit)
