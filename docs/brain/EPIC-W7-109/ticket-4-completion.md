# EPIC-W7-109 — Ticket 4 Completion

## Agent Tracking
- **agent_name**: v12-p5-ticket
- **epic_id**: EPIC-W7-109
- **ticket**: T4
- **wave**: 7
- **cluster**: S1_SIMA

## Ticket Summary
**Extract `ApplyTradeDnaFlags`** — trade-DNA flag stamper on a `PositionInfo` argument.

## Changes Made
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Action**: Added private method `ApplyTradeDnaFlags` in the helper block
- **Lines added**: ~9 lines

## Method Signature
```csharp
private void ApplyTradeDnaFlags(PositionInfo pos, string key)
```

## Complexity
- **CYC**: 4 (OK)
- **LOC**: 9
- **Status**: OK

## DNA Compliance
- [x] No lock()
- [x] ASCII-only strings
- [x] Mutates only the `pos` argument — no shared-state side effects
- [x] Single responsibility: stamp IsMOMOTrade / IsTRENDTrade / IsRetestTrade / IsRMATrade / IsFFMATrade
- [x] IsMOMOTrade=true clears IsRMATrade — mutual-exclusion rule preserved exactly from original

## xUnit Test Stub
```csharp
[Fact]
public void ApplyTradeDnaFlags_MOMOKey_SetsIsMOMOAndClearsIsRMA()
{
    // Arrange:
    var pos = new PositionInfo();
    // Act:
    sut.ApplyTradeDnaFlags(pos, "MOMO_Long_001");
    // Assert:
    Assert.Equal(true, pos.IsMOMOTrade);
    Assert.Equal(false, pos.IsRMATrade);
}

[Fact]
public void ApplyTradeDnaFlags_TRMAKey_SetsIsTRENDAndIsRMA()
{
    var pos = new PositionInfo();
    sut.ApplyTradeDnaFlags(pos, "TRMA_001");
    Assert.Equal(true, pos.IsTRENDTrade);
    Assert.Equal(true, pos.IsRMATrade);
}
```
