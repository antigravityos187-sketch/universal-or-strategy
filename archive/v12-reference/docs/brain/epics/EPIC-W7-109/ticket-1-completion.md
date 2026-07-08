# EPIC-W7-109 — Ticket 1 Completion

## Agent Tracking
- **agent_name**: v12-p5-ticket
- **epic_id**: EPIC-W7-109
- **ticket**: T1
- **wave**: 7
- **cluster**: S1_SIMA

## Ticket Summary
**Extract `TryGetMasterBrokerPosition`** — pure broker-position lookup helper.

## Changes Made
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Action**: Added private method `TryGetMasterBrokerPosition` immediately after `HydrateWorkingOrdersFromBroker` (line ~358)
- **Lines added**: ~28 lines

## Method Signature
```csharp
private bool TryGetMasterBrokerPosition(
    out MarketPosition masterMP,
    out int masterQty,
    out double masterAvgPrice)
```

## Complexity
- **CYC**: 6 (WATCH — within <=8 target)
- **LOC**: 20
- **Status**: OK

## DNA Compliance
- [x] No lock()
- [x] ASCII-only strings
- [x] Pure query — zero side effects on strategy state
- [x] out-params pattern (zero-allocation, no heap allocation for result)
- [x] Single responsibility: finds matching broker position for current instrument

## xUnit Test Stub
```csharp
[Fact]
public void TryGetMasterBrokerPosition_NoMatchingPosition_ReturnsFalse()
{
    // Arrange: Account.Positions empty or all Flat
    // Act: bool result = sut.TryGetMasterBrokerPosition(out var mp, out var qty, out var avg);
    // Assert:
    Assert.Equal(false, result);
    Assert.Equal(MarketPosition.Flat, mp);
    Assert.Equal(0, qty);
    Assert.Equal(0.0, avg);
}
```
