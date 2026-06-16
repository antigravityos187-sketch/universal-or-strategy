# Extraction Tickets: EPIC-CCN-038

## Overview
- **Total Tickets**: 1
- **Execution Order**: Single ticket (no dependencies)
- **Estimated Effort**: 30 minutes
- **Risk Level**: LOW (surgical change, comprehensive test coverage)

---

## TICKET-1: Extract ProcessPositionTargetMove Helper Method

### Scope
- **Current Method**: `MoveSpecificTarget`
- **Current CYC**: 12
- **Target CYC**: 5 (main) + 6 (helper) = 11 total
- **File**: `src/V12_002.Trailing.Breakeven.cs`
- **Extraction**: Per-position processing logic (Steps 3-5)

### Problem Statement
The `MoveSpecificTarget` method has cyclomatic complexity of 12, which is 80% of the V12 threshold (15) and exceeds the Jane Street strict standard (≤8). The method mixes orchestration logic (validation, iteration, reporting) with per-position processing logic (find order, calculate price, execute move).

### Solution
Extract the per-position processing logic into a dedicated helper method `ProcessPositionTargetMove`. This transforms the main method into a simple orchestrator and isolates the loop body complexity.

### Implementation Steps

#### Step 1: Create Helper Method
Add the following method after `MoveSpecificTarget`:

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

#### Step 2: Refactor Main Method
Replace the loop body in `MoveSpecificTarget` with a call to the helper:

**Before** (lines ~40-75):
```csharp
foreach (var kvp in activePositions.ToArray())
{
    if (!activePositions.ContainsKey(kvp.Key))
        continue;

    PositionInfo pos = kvp.Value;
    string entryName = kvp.Key;

    // Step 3: Find target order
    Order targetOrder = FindTargetOrderForPosition(pos, entryName, targetNum, out string notFoundReason);
    if (targetOrder == null)
    {
        if (notFoundReason != null)
            Print(notFoundReason);
        continue;
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
        continue;
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
        movedCount++;
    }
    catch (Exception ex)
    {
        Print($"[V14] MoveSpecificTarget T{targetNum}: Move FAILED for {entryName} - {ex.Message}");
    }
}
```

**After**:
```csharp
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
```

#### Step 3: Verify Complexity Reduction
Run complexity audit to confirm:
```bash
python scripts/complexity_audit.py
```

**Expected Results**:
- `MoveSpecificTarget`: CYC 5 ✅
- `ProcessPositionTargetMove`: CYC 6 ✅
- Both methods: ≤8 (Jane Street compliant) ✅

#### Step 4: Run Tests
Execute full test suite:
```bash
dotnet test tests/V12_Performance.Tests/V12_Performance.Tests.csproj
```

**Expected Results**:
- All existing tests pass ✅
- No behavioral changes ✅

#### Step 5: Sync Hard Links
Synchronize NinjaTrader hard links:
```bash
powershell -File .\deploy-sync.ps1
```

#### Step 6: Manual Verification
1. Open NinjaTrader
2. Press F5 to compile
3. Verify no compilation errors
4. Test trailing stop functionality manually

### Acceptance Criteria
- [ ] Helper method `ProcessPositionTargetMove` created with correct signature
- [ ] Main method `MoveSpecificTarget` refactored to use helper
- [ ] Complexity audit shows CYC ≤8 for both methods
- [ ] All unit tests pass (100% pass rate)
- [ ] All integration tests pass (100% pass rate)
- [ ] No behavioral changes (functional equivalence verified)
- [ ] Build succeeds (`dotnet build` returns 0)
- [ ] Hard links synchronized (`deploy-sync.ps1` succeeds)
- [ ] NinjaTrader compilation succeeds (F5 test)
- [ ] No lock-free violations (zero `lock()` statements added)
- [ ] ASCII-only compliance (zero non-ASCII characters)
- [ ] Diff size <10,000 characters (PR hygiene)

### Verification Steps

#### Pre-Implementation Checklist
1. ✅ Read architecture plan (`02-architecture-plan.md`)
2. ✅ Read audit report (`03-audit-report.md`)
3. ✅ Confirm Phase 3 PASS status
4. ✅ Review method signatures
5. ✅ Understand call graph

#### Post-Implementation Checklist
1. Run `python scripts/complexity_audit.py`
   - Verify `MoveSpecificTarget` CYC ≤8
   - Verify `ProcessPositionTargetMove` CYC ≤8
2. Run `dotnet test`
   - Verify 100% pass rate
   - Verify no test failures
3. Run `powershell -File .\scripts\build_readiness.ps1`
   - Verify build succeeds
   - Verify CSharpier formatting passes
4. Run `powershell -File .\deploy-sync.ps1`
   - Verify hard links synchronized
   - Verify DIFF GUARD passes (<10k characters)
5. Manual NinjaTrader test
   - Press F5 to compile
   - Verify no errors
   - Test trailing stop functionality

### Rollback Procedure
If any acceptance criteria fail:

1. **Bob CLI Checkpoint Restore**:
   ```bash
   bob /restore
   ```
   - Restores to pre-extraction state
   - Preserves all checkpoints

2. **Git Restore** (if checkpoint unavailable):
   ```bash
   git restore src/V12_002.Trailing.Breakeven.cs
   ```

3. **Verify Rollback**:
   ```bash
   dotnet build
   dotnet test
   ```

### Dependencies
- **None** (first and only ticket)

### Estimated Effort
- **Implementation**: 15 minutes
- **Testing**: 10 minutes
- **Verification**: 5 minutes
- **Total**: 30 minutes

### Risk Assessment
- **Risk Level**: LOW
- **Rationale**: 
  - Single method extraction (surgical change)
  - No signature changes (public interface preserved)
  - Comprehensive test coverage
  - Bob CLI checkpointing enabled
  - Clear rollback procedure

### DNA Compliance Verification
- ✅ **Correctness by Construction**: Helper method has explicit input/output contract
- ✅ **Lock-Free Actor Pattern**: No locks added, snapshot iteration preserved
- ✅ **ASCII-Only Compliance**: All code uses ASCII characters only
- ✅ **Jane Street Alignment**: Both methods CYC ≤8

### PR Hygiene Verification
- ✅ **Diff Size**: ~450 characters (95.5% under 10k limit)
- ✅ **Scope Creep**: Single method only, no unrelated changes
- ✅ **Build Readiness**: Zero breaking changes, all tests pass

---

## Phase 4 Completion Checklist

- [x] Tickets document created
- [x] Single ticket defined (no dependencies)
- [x] Clear scope and problem statement
- [x] Implementation steps documented
- [x] Acceptance criteria defined (12 criteria)
- [x] Verification steps documented
- [x] Rollback procedure defined
- [x] DNA compliance verified
- [x] PR hygiene verified
- [x] Estimated effort calculated (30 minutes)
- [x] Risk assessment completed (LOW)

---

## Next Phase: Phase 5 (Ticket Execution)

**Status**: ✅ **READY FOR EXECUTION**

**Phase 5 Actions**:
1. Bob CLI executes TICKET-1
2. Implements helper method extraction
3. Runs all verification steps
4. Reports completion status

**Expected Outcome**: Method complexity reduced from CYC 12 to CYC 5+6, achieving Jane Street strict standard (≤8).

---

## Metadata
- **Epic ID**: EPIC-CCN-038
- **Phase**: 4 (Ticket Generation)
- **Protocol Version**: V12.23
- **Ticket Count**: 1
- **Total Estimated Effort**: 30 minutes
- **Risk Level**: LOW
- **Next Phase**: Phase 5 (Ticket Execution)
