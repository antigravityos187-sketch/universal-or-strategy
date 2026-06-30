# ticket-2-completion.md — EPIC-W7-025 T2

## Ticket
**T2: Extract ComputeFFMAStopDistance(double currentPrice, double candleExtreme)**

## Agent
v12-engineer (V12 Photon Engineer, Phase 5)

## EPIC
EPIC-W7-025 | Cluster: FL-38 S6_SIGNALS | Wave 7

## Source File
[`src/V12_002.Entries.FFMA.cs`](../../src/V12_002.Entries.FFMA.cs)

## Work Performed
Extracted the duplicate stop-distance calculation block (present in both SHORT and LONG paths) into a shared helper `ComputeFFMAStopDistance(double currentPrice, double candleExtreme)`.

### Before (duplicated in both SHORT and LONG setup blocks)
```csharp
double stopDistance = Math.Min(Math.Abs(currentPrice - stopPrice), MaximumStop);
if (stopDistance < tickSize * 2)
    stopDistance = tickSize * 2;
```

### After — new helper
```csharp
private double ComputeFFMAStopDistance(double currentPrice, double candleExtreme)
{
    double stopDistance = Math.Min(Math.Abs(currentPrice - candleExtreme), MaximumStop);
    if (stopDistance < tickSize * 2)
        stopDistance = tickSize * 2;
    return stopDistance;
}
```

### Call sites in TryExecuteFFMAShort / TryExecuteFFMALong
```csharp
double stopDistance = ComputeFFMAStopDistance(currentPrice, High[0]);  // SHORT
double stopDistance = ComputeFFMAStopDistance(currentPrice, Low[0]);   // LONG
```

## Complexity
| Method | CYC |
|--------|-----|
| ComputeFFMAStopDistance | 2 |

## Tests Written
File: [`xunit-tests/W7-025/W7_025_ComputeFFMAStopDistanceTests.cs`](../../xunit-tests/W7-025/W7_025_ComputeFFMAStopDistanceTests.cs)

| Test | Assert | Result |
|------|--------|--------|
| RawDistance_BelowMaxStop_AboveTickFloor_ReturnsRawDistance | Assert.Equal(10.0, result) | PASS |
| RawDistance_ExceedsMaxStop_ClampsToMaximumStop | Assert.Equal(50.0, result) | PASS |
| RawDistance_BelowTickFloor_RaisesToTickFloor | Assert.Equal(0.5, result) | PASS |

Run: `dotnet test xunit-tests/W7-025/W7_025.Tests.csproj` — Passed: 3, Failed: 0

## DNA Compliance
- No lock() blocks
- ASCII-only strings
- CYC <= 8
- Zero logic drift (pure structural extraction, no optimization)

## Build
dotnet build Linting.csproj: **0 errors, 0 warnings**
