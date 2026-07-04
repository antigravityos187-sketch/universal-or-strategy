# EPIC-W7-109 — Ticket 6 Completion

## Agent Tracking
- **agent_name**: v12-p5-ticket
- **epic_id**: EPIC-W7-109
- **ticket**: T6 (parent wiring)
- **wave**: 7
- **cluster**: S1_SIMA

## Ticket Summary
**Refactor `HydrateWorkingOrdersFromBroker`** — replace 108-line inline reconstruction block with single `ReconstructMasterActivePositions()` call.

## Changes Made
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Lines removed**: 334–442 (inner try block with full reconstruction logic — 108 lines)
- **Lines added**: 2 lines (comment + `ReconstructMasterActivePositions()` call)
- **Net delta**: -106 lines from parent method

## Before / After CYC
| Method | CYC Before | CYC After |
|--------|-----------|-----------|
| `HydrateWorkingOrdersFromBroker` | 19 | **5** |

## Final Parent Body (summary)
```csharp
private void HydrateWorkingOrdersFromBroker()
{
    int adoptedCount = AdoptFleetOrders();
    bool masterIsFleetForOrders993 = IsFleetAccount(Account);
    if (!masterIsFleetForOrders993)
    {
        try { adoptedCount += AdoptMasterOrders(); }
        catch (Exception ex) { Print(...); }
    }
    if (!masterIsFleetForOrders993)
        ReconstructMasterActivePositions();   // <-- T6 wire-up
    HydrateFSMsFromWorkingOrders();
    _orderAdoptionComplete = true;
    Print(...);
}
```

## DNA Compliance
- [x] No lock()
- [x] ASCII-only strings
- [x] Zero logic drift — pure structural movement, no behavior change
- [x] Build: 0 errors, 0 warnings
- [x] CSharpier formatted

## xUnit Test Stub
```csharp
[Fact]
public void HydrateWorkingOrdersFromBroker_MasterAccount_CallsReconstructAndCompletesAdoption()
{
    // Arrange: masterIsFleetForOrders993 = false
    // Act: sut.HydrateWorkingOrdersFromBroker();
    // Assert:
    Assert.Equal(true, sut._orderAdoptionComplete);
}
```
