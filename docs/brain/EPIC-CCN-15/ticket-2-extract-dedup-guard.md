# Ticket 2: Extract Deduplication Guard

**Epic**: EPIC-CCN-15  
**Priority**: P1 (Independent, high complexity)  
**Estimated CYC**: 12 → 8 (33% reduction)  
**File**: `src/V12_002.Orders.Callbacks.Execution.cs`

---

## Objective

Extract deduplication logic (lines 241-278) to a new method `CheckExecutionDeduplication`. This method prevents double-decrement when both `OnOrderUpdate` and `OnExecutionUpdate` fire for the same execution.

---

## Current Code (Lines 241-278)

```csharp
// V12.962 INLINE ACTOR: Dedup guard -- lock-free, serial execution guaranteed by _drainToken.
// V12.Phase7 [C-01]: Prevent double-decrement if OnOrderUpdate + OnExecutionUpdate both fire.
if (!string.IsNullOrEmpty(executionId))
{
    // V14.2 [ADR-011]: Zero-allocation dedup via FNV-1a hash ring
    long _execHash = FnvHash64(executionId);
    if (_executionIdRing.ContainsOrAdd(_execHash))
    {
        Print(
            string.Format("[DEDUP] Skipping duplicate execution {0} for {1}", executionId, orderName)
        );
        return;
    }
}
else
{
    // V14.2 [ADR-011]: Fallback dedup when executionId is missing
    // Uses execution.Order properties -- FIX-D5: correct variable mapping
    string uniqueOrderId = !string.IsNullOrEmpty(execution.Order.OrderId)
        ? execution.Order.OrderId
        : execution.Order.Name;
    int dedupFilledQty = execution.Order.Filled > 0 ? execution.Order.Filled : Math.Max(0, quantity);
    string _fallbackKey = string.Format("{0}|{1}", uniqueOrderId, dedupFilledQty);
    long _fallbackHash = FnvHash64(_fallbackKey);
    if (_executionIdFallbackRing.ContainsOrAdd(_fallbackHash))
    {
        Print(
            string.Format(
                "[DEDUP] Skipping duplicate execution (fallback) orderId={0}",
                execution.Order.OrderId
            )
        );
        return;
    }
}
```

---

## Target Code (New Method)

**Location**: Insert after `ExtractEntryNameFromOrder` method

```csharp
/// <summary>
/// V12.CCN-15 [T2]: Deduplication guard using FNV-1a hash rings.
/// Prevents double-decrement if OnOrderUpdate + OnExecutionUpdate both fire.
/// V12.962 INLINE ACTOR: Lock-free, serial execution guaranteed by _drainToken.
/// </summary>
/// <param name="executionId">Execution ID from broker (may be null/empty)</param>
/// <param name="execution">Execution object for fallback dedup</param>
/// <param name="orderName">Order name for logging</param>
/// <param name="quantity">Execution quantity for fallback dedup</param>
/// <returns>True if duplicate (skip processing), false if new (proceed)</returns>
private bool CheckExecutionDeduplication(string executionId, Execution execution, string orderName, int quantity)
{
    if (!string.IsNullOrEmpty(executionId))
    {
        // V14.2 [ADR-011]: Zero-allocation dedup via FNV-1a hash ring
        long execHash = FnvHash64(executionId);
        if (_executionIdRing.ContainsOrAdd(execHash))
        {
            Print(string.Format("[DEDUP] Skipping duplicate execution {0} for {1}", executionId, orderName));
            return true;
        }
    }
    else
    {
        // V14.2 [ADR-011]: Fallback dedup when executionId is missing
        // Uses execution.Order properties -- FIX-D5: correct variable mapping
        string uniqueOrderId = !string.IsNullOrEmpty(execution.Order.OrderId)
            ? execution.Order.OrderId
            : execution.Order.Name;
        int dedupFilledQty = execution.Order.Filled > 0 ? execution.Order.Filled : Math.Max(0, quantity);
        string fallbackKey = string.Format("{0}|{1}", uniqueOrderId, dedupFilledQty);
        long fallbackHash = FnvHash64(fallbackKey);
        
        if (_executionIdFallbackRing.ContainsOrAdd(fallbackHash))
        {
            Print(string.Format("[DEDUP] Skipping duplicate execution (fallback) orderId={0}", execution.Order.OrderId));
            return true;
        }
    }
    
    return false; // Not a duplicate, proceed with processing
}
```

---

## Refactor Site (Replace Inline Logic)

**Location**: Lines 241-278 in `ProcessOnExecutionUpdate`

**Before**:
```csharp
// V12.962 INLINE ACTOR: Dedup guard -- lock-free, serial execution guaranteed by _drainToken.
// V12.Phase7 [C-01]: Prevent double-decrement if OnOrderUpdate + OnExecutionUpdate both fire.
if (!string.IsNullOrEmpty(executionId))
{
    // ... (37 lines of dedup logic)
}
```

**After**:
```csharp
// V12.CCN-15 [T2]: Deduplication guard
if (CheckExecutionDeduplication(executionId, execution, orderName, quantity))
    return;
```

---

## Implementation Steps

1. **Insert new method** after `ExtractEntryNameFromOrder`
2. **Replace inline logic** at lines 241-278 with method call
3. **Verify ASCII-only**: All string literals use straight quotes
4. **Run complexity audit**: `python scripts/complexity_audit.py`
   - Verify `ProcessOnExecutionUpdate` CYC reduced
   - Verify `CheckExecutionDeduplication` CYC ≤8
5. **Run deploy-sync**: `powershell -File .\deploy-sync.ps1`
6. **Bump BUILD_TAG** in `src/V12_002.cs`

---

## F5 Verification

**Test Scenario**: Trigger duplicate execution (OnOrderUpdate + OnExecutionUpdate)

**Setup**:
1. Place entry order (Buy 1 MES @ Market)
2. Wait for fill
3. Both callbacks fire for same execution

**Expected Behavior**:
1. First callback: Processes execution normally
2. Second callback: "[DEDUP] Skipping duplicate execution..." log appears
3. Position quantity correct (no double-decrement)
4. No errors in log

**Verification Command**:
```
F5 in NinjaTrader → Place entry → Check log for "[DEDUP]" message
```

---

## Success Criteria

- [ ] New method `CheckExecutionDeduplication` added
- [ ] Inline logic replaced with method call (lines 241-278)
- [ ] ASCII-only compliance verified (no Unicode)
- [ ] Complexity audit passes:
  - [ ] `ProcessOnExecutionUpdate` CYC reduced by ~10
  - [ ] `CheckExecutionDeduplication` CYC ≤8
- [ ] Deploy-sync passes (ASCII gate)
- [ ] BUILD_TAG bumped
- [ ] F5 verification passes:
  - [ ] "[DEDUP]" log appears on duplicate
  - [ ] Position quantity correct (no double-decrement)
- [ ] Build + tests pass

---

## Risk Assessment

**Risk Level**: HIGH

**Rationale**:
- Hash ring logic is subtle (collision edge cases)
- Fallback dedup uses complex key construction
- Critical for preventing double-decrement bugs

**Mitigation**:
1. **Exact Copy**: Zero logic drift, preserve all comments
2. **Unit Tests**: Add tests for primary/fallback dedup paths
3. **F5 Verification**: Trigger duplicate execution scenario

---

## Unit Tests (Add to Test Suite)

```csharp
[Test]
public void CheckExecutionDeduplication_PrimaryPath_ReturnsTrueOnDuplicate()
{
    // Arrange
    string executionId = "EXEC123";
    Execution execution = CreateMockExecution();
    string orderName = "Entry_Test1";
    int quantity = 1;
    
    // Act
    bool firstCall = strategy.CheckExecutionDeduplication(executionId, execution, orderName, quantity);
    bool secondCall = strategy.CheckExecutionDeduplication(executionId, execution, orderName, quantity);
    
    // Assert
    Assert.IsFalse(firstCall, "First call should return false (not duplicate)");
    Assert.IsTrue(secondCall, "Second call should return true (duplicate)");
}

[Test]
public void CheckExecutionDeduplication_FallbackPath_ReturnsTrueOnDuplicate()
{
    // Arrange
    string executionId = ""; // Empty to trigger fallback
    Execution execution = CreateMockExecution(orderId: "ORD123", filled: 1);
    string orderName = "Entry_Test1";
    int quantity = 1;
    
    // Act
    bool firstCall = strategy.CheckExecutionDeduplication(executionId, execution, orderName, quantity);
    bool secondCall = strategy.CheckExecutionDeduplication(executionId, execution, orderName, quantity);
    
    // Assert
    Assert.IsFalse(firstCall, "First call should return false (not duplicate)");
    Assert.IsTrue(secondCall, "Second call should return true (duplicate)");
}
```

---

## Rollback Plan

If F5 verification fails:
1. Revert commit: `git reset --hard HEAD~1`
2. Restore inline dedup logic
3. Report issue to Director
4. Investigate hash collision or key construction bug

---

**Ticket Created**: 2026-06-09T03:38:00Z  
**Assigned To**: Bob Shell (v12-engineer)  
**Status**: Ready for Execution (Depends on Ticket 1)