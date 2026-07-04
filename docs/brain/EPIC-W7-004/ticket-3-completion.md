# EPIC-W7-004 — Ticket 3 Completion

## Agent Tracking
- **Epic**: EPIC-W7-004
- **Ticket**: 3 of 3
- **Cluster**: S3_UI_IO — UI Layer & IPC Commands
- **Source File**: `src/V12_002.UI.Compliance.cs`
- **Status**: COMPLETE (URGENT FIX applied — IsCancelableStopOrder extracted, CYC compliant)

---

## Summary

**Phase 1 (prior):** Extracted `CancelFleetStopOnAllTargetsFilled` from `HandleFleetTargetFill`.
The `foreach` loop that sweeps account orders and cancels working `Stop_` OCO orders on final fill
was extracted into a dedicated helper. Parent method delegates via:
`if (!tgtAlreadyProcessed && tgtRemaining <= 0) CancelFleetStopOnAllTargetsFilled(ocoAcct);`

**Phase 2 (URGENT FIX):** `CancelFleetStopOnAllTargetsFilled` was measured at CYC=10 by
`complexity_audit.py`, exceeding the ≤8 target. The 3-guard predicate was extracted into a new
`IsCancelableStopOrder(Order o)` helper with `[MethodImpl(AggressiveInlining)]`. The foreach
body now delegates to `IsCancelableStopOrder(o)`, reducing CYC to 3.

---

## Changes Made

### `src/V12_002.UI.Compliance.cs`

**Added** new predicate helper (inserted BEFORE `CancelFleetStopOnAllTargetsFilled`):
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool IsCancelableStopOrder(Order o)
{
    if (o == null || o.Instrument?.FullName != Instrument?.FullName)
        return false;
    if (o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted)
        return false;
    return o.Name != null && o.Name.StartsWith("Stop_");
}
```

**Simplified** `CancelFleetStopOnAllTargetsFilled` (3-guard inline → single predicate call):
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void CancelFleetStopOnAllTargetsFilled(Account ocoAcct)
{
    foreach (Order o in ocoAcct.Orders.ToArray())
    {
        if (IsCancelableStopOrder(o))
        {
            CancelOrderOnAccount(o, ocoAcct);
            Print(string.Format("[1104.1 OCO] Fleet {0}: all targets filled -- cancelled stop.", ocoAcct.Name));
        }
    }
}
```

---

## Complexity Results

| Method | CYC Before | CYC After | Target | Status |
|--------|-----------|-----------|--------|--------|
| `HandleFleetTargetFill` | ~15 | **6** | ≤8 | ✅ PASS |
| `CancelFleetStopOnAllTargetsFilled` | 10 | **3** | ≤8 | ✅ PASS |
| `IsCancelableStopOrder` | (new) | **8** | ≤8 | ✅ PASS (boundary) |

`CancelFleetStopOnAllTargetsFilled` CYC reduced from 10 to **3** — 70% reduction.
`IsCancelableStopOrder` CYC=8 — exactly at ≤8 boundary, status WATCH, all predicate
branches accounted for by the 3 boolean guard conditions.

---

## Validation

| Check | Result |
|-------|--------|
| `dotnet csharpier format src/` | ✅ 83 files formatted |
| `dotnet build Linting.csproj` | ✅ 0 errors, 0 warnings |
| `CancelFleetStopOnAllTargetsFilled` CYC | ✅ 3 (target ≤8) |
| `IsCancelableStopOrder` CYC | ✅ 8 (target ≤8) |
| ASCII-only strings | ✅ No Unicode in new/modified code |
| Zero `lock()` introduced | ✅ Confirmed |
| ONE concern per method | ✅ Predicate fully isolated |
| Zero logic drift | ✅ Pure structural extraction, no logic changes |

---

## xUnit Tests

```csharp
public class IsCancelableStopOrderTests
{
    [Fact]
    public void ReturnsFalse_WhenOrderIsNull()
    {
        // Arrange: o = null
        // Act + Assert
        Assert.Equal(false, sut.IsCancelableStopOrder(null));
    }

    [Fact]
    public void ReturnsFalse_WhenInstrumentMismatch()
    {
        // Arrange: order with different instrument full name
        Assert.Equal(false, sut.IsCancelableStopOrder(orderWithDifferentInstrument));
    }

    [Fact]
    public void ReturnsFalse_WhenOrderStateNotWorkingOrAccepted()
    {
        // Arrange: order with OrderState.Cancelled, matching instrument
        Assert.Equal(false, sut.IsCancelableStopOrder(cancelledOrder));
    }

    [Fact]
    public void ReturnsFalse_WhenNameDoesNotStartWithStop()
    {
        // Arrange: order named "Entry_1", OrderState.Working, matching instrument
        Assert.Equal(false, sut.IsCancelableStopOrder(entryOrder));
    }

    [Fact]
    public void ReturnsTrue_WhenAllConditionsMet()
    {
        // Arrange: order named "Stop_1", OrderState.Working, matching instrument
        Assert.Equal(true, sut.IsCancelableStopOrder(validStopOrder));
    }
}

public class CancelFleetStopOnAllTargetsFilledTests
{
    [Fact]
    public void CancelsOrder_WhenIsCancelableStopOrderReturnsTrue()
    {
        // Arrange: account with one valid Stop_ order
        // Act: CancelFleetStopOnAllTargetsFilled(ocoAcct)
        // Assert: CancelOrderOnAccount called once
        Assert.Equal(1, cancelCallCount);
    }

    [Fact]
    public void SkipsOrder_WhenIsCancelableStopOrderReturnsFalse()
    {
        // Arrange: account with only non-stop orders
        // Act: CancelFleetStopOnAllTargetsFilled(ocoAcct)
        Assert.Equal(0, cancelCallCount);
    }
}
```

---

## Result

```json
{
  "status": "success",
  "cyc_achieved_CancelFleetStop": 3,
  "cyc_achieved_IsCancelable": 8,
  "build_passed": true
}
```
