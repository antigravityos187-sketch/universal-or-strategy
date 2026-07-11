# EPIC-W7-109 — Ticket 5 Completion

## Agent Tracking
- **agent_name**: v12-p5-ticket
- **epic_id**: EPIC-W7-109
- **ticket**: T5
- **wave**: 7
- **cluster**: S1_SIMA

## Ticket Summary
**Extract `ReconstructMasterActivePositions`** — orchestrator that composes T1–T4 helpers to rebuild master `activePositions` from broker state + adopted stops.

## Changes Made
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Action**: Added private method `ReconstructMasterActivePositions`; parent `HydrateWorkingOrdersFromBroker` lines 334-442 replaced with single call
- **Lines added**: ~42 lines (orchestrator body)
- **Lines removed from parent**: 108 lines

## Method Signature
```csharp
private void ReconstructMasterActivePositions()
```

## Complexity
- **CYC**: 7 (WATCH — within <=8 target)
- **LOC**: 41
- **Status**: WATCH (passes threshold)

## Orchestration Flow
1. `TryGetMasterBrokerPosition` — early-return if no matching broker position
2. Guard: `masterMP == Flat || masterQty <= 0` — second early-return
3. `foreach stopKvp` — iterate adopted stop orders
4. `IsMasterStopKeyEligible` — skip Fleet_ keys and duplicates
5. `BuildMasterPositionInfo` — construct PositionInfo struct
6. `ApplyTradeDnaFlags` — stamp trade classification flags
7. `activePositions[key] = pos` — single write to state

## DNA Compliance
- [x] No lock()
- [x] ASCII-only strings
- [x] try/catch wraps entire body — reconstruction failure is non-fatal (Print + continue)
- [x] Single responsibility: orchestrate master position reconstruction
- [x] Zero new allocations on hot path (ToArray() was already in original)

## Parent CYC Impact
- **Before**: `HydrateWorkingOrdersFromBroker` CYC = 19
- **After**: `HydrateWorkingOrdersFromBroker` CYC = 5
- **Achieved**: 74% CYC reduction on parent

## xUnit Test Stub
```csharp
[Fact]
public void ReconstructMasterActivePositions_NoBrokerPosition_AddsNoActivePositions()
{
    // Arrange: Account.Positions empty
    // Act: sut.ReconstructMasterActivePositions();
    // Assert:
    Assert.Equal(0, sut.activePositions.Count);
}
```
