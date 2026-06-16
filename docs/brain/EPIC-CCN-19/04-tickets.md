# EPIC-CCN-19: Ticket Generation

**Epic ID**: EPIC-CCN-19  
**Target**: CheckFFMAConditions (CYC 16 → ≤8)  
**Phase**: 4 - Ticket Generation  
**Created**: 2026-06-09T20:10:18Z

---

## Ticket Overview

**Total Tickets**: 1  
**Execution Strategy**: Single-ticket extraction (all helpers in one pass)  
**Estimated Duration**: 45 minutes

---

## Ticket 1: Extract CheckFFMAConditions Helpers

### Ticket ID
**EPIC-CCN-19-T1**

### Title
Extract 4 helper methods from CheckFFMAConditions (CYC 16 → 3)

### Description
Refactor `CheckFFMAConditions()` by extracting 4 helper methods to reduce cyclomatic complexity from 16 to 3. This is a pure structural extraction with zero logic changes.

### Target File
`src/V12_002.Entries.FFMA.cs`

### Target Method
`CheckFFMAConditions()` (lines 43-106)

### Complexity Metrics
- **Current CYC**: 16
- **Target CYC**: 3
- **Reduction**: -13 (81%)

---

## Ticket 1 Specification

### Acceptance Criteria

**Functional**:
- ✅ CheckFFMAConditions() CYC reduced from 16 to 3
- ✅ IsFFMAReadyToCheck() extracted with CYC 3
- ✅ GetFFMAMarketData() extracted with CYC 1
- ✅ CheckFFMAShortSetup() extracted with CYC 4
- ✅ CheckFFMALongSetup() extracted with CYC 4
- ✅ FFMA SHORT/LONG behavior unchanged (byte-for-byte logic preservation)

**Quality**:
- ✅ All 19 unit tests passing (TDD approach)
- ✅ ASCII-only compliance maintained
- ✅ No new Jane Street P0 violations
- ✅ BUILD_TAG updated in `src/V12_002.cs`
- ✅ Hard links synchronized via `deploy-sync.ps1`
- ✅ F5 verification passed (NinjaTrader compilation + load)

**Documentation**:
- ✅ XML documentation added for all 4 helpers
- ✅ Ticket completion report generated
- ✅ Manifest updated with completion status

---

### Implementation Steps

**Step 1: Setup Test File (5 minutes)**
```bash
# Create test file
mkdir -p tests/V12_Performance.Tests/Entries
touch tests/V12_Performance.Tests/Entries/FFMAConditionsTests.cs
```

**Step 2: Write TDD Tests (15 minutes)**

Create 19 unit tests:

**IsFFMAReadyToCheck() - 4 tests**
```csharp
[Fact] public void IsFFMAReadyToCheck_WhenDisarmed_ReturnsFalse()
[Fact] public void IsFFMAReadyToCheck_WhenIndicatorsNull_ReturnsFalse()
[Fact] public void IsFFMAReadyToCheck_WhenInsufficientBars_ReturnsFalse()
[Fact] public void IsFFMAReadyToCheck_WhenAllConditionsMet_ReturnsTrue()
```

**GetFFMAMarketData() - 5 tests**
```csharp
[Fact] public void GetFFMAMarketData_ReturnsCorrectEMA9Value()
[Fact] public void GetFFMAMarketData_ReturnsCorrectRSIValue()
[Fact] public void GetFFMAMarketData_CalculatesDistanceCorrectly()
[Fact] public void GetFFMAMarketData_DetectsGreenCandleCorrectly()
[Fact] public void GetFFMAMarketData_DetectsRedCandleCorrectly()
```

**CheckFFMAShortSetup() - 5 tests**
```csharp
[Fact] public void CheckFFMAShortSetup_WhenConditionsNotMet_ReturnsFalse()
[Fact] public void CheckFFMAShortSetup_WhenRSILow_ReturnsFalse()
[Fact] public void CheckFFMAShortSetup_WhenGreenCandle_ReturnsFalse()
[Fact] public void CheckFFMAShortSetup_WhenAllConditionsMet_ExecutesEntry()
[Fact] public void CheckFFMAShortSetup_ValidatesMinimumStopDistance()
```

**CheckFFMALongSetup() - 5 tests**
```csharp
[Fact] public void CheckFFMALongSetup_WhenConditionsNotMet_ReturnsFalse()
[Fact] public void CheckFFMALongSetup_WhenRSIHigh_ReturnsFalse()
[Fact] public void CheckFFMALongSetup_WhenRedCandle_ReturnsFalse()
[Fact] public void CheckFFMALongSetup_WhenAllConditionsMet_ExecutesEntry()
[Fact] public void CheckFFMALongSetup_ValidatesMinimumStopDistance()
```

**Step 3: Extract IsFFMAReadyToCheck() (3 minutes)**
```csharp
/// <summary>
/// V12.Phase7: Validates FFMA readiness (armed, indicators initialized, sufficient bars)
/// </summary>
/// <returns>True if FFMA checks should proceed, false otherwise</returns>
private bool IsFFMAReadyToCheck()
{
    if (!isFFMAModeArmed || !FFMAEnabled)
        return false;
    
    if (ema9 == null || rsiIndicator == null || currentATR <= 0)
        return false;
    
    if (CurrentBar < 20)
        return false;
    
    return true;
}
```

**Step 4: Extract GetFFMAMarketData() (3 minutes)**
```csharp
/// <summary>
/// V12.Phase7: Extracts current market data for FFMA analysis
/// </summary>
/// <returns>Tuple containing EMA9, RSI, price, distance, and candle color flags</returns>
private (double ema9Value, double rsiValue, double currentPrice, double distanceFromEMA, 
         bool isGreenCandle, bool isRedCandle) GetFFMAMarketData()
{
    double ema9Value = ema9[0];
    double rsiValue = rsiIndicator[0];
    double currentPrice = Close[0];
    double distanceFromEMA = currentPrice - ema9Value;
    bool isGreenCandle = Close[0] > Open[0];
    bool isRedCandle = Close[0] < Open[0];
    
    return (ema9Value, rsiValue, currentPrice, distanceFromEMA, isGreenCandle, isRedCandle);
}
```

**Step 5: Extract CheckFFMAShortSetup() (5 minutes)**
```csharp
/// <summary>
/// V12.Phase7: Checks SHORT setup conditions and executes entry if triggered
/// SHORT: RSI > 80 + price 10+ pts above 9 EMA + RED candle
/// </summary>
private bool CheckFFMAShortSetup(double rsiValue, double distanceFromEMA, 
                                  bool isRedCandle, double currentPrice)
{
    if (!(rsiValue > FFMARSIOverbought && distanceFromEMA >= FFMAEMADistance && isRedCandle))
        return false;

    Print(string.Format(
        "FFMA SHORT TRIGGERED: RSI={0:F1} > {1} | Distance={2:F2}pts > {3}pts | RED candle",
        rsiValue, FFMARSIOverbought, distanceFromEMA, FFMAEMADistance));

    double stopPrice = High[0];
    double stopDistance = Math.Min(Math.Abs(currentPrice - stopPrice), MaximumStop);
    
    if (stopDistance < tickSize * 2)
        stopDistance = tickSize * 2;

    int contracts = CalculatePositionSize(stopDistance);
    ExecuteFFMAEntry(MarketPosition.Short, contracts);
    return true;
}
```

**Step 6: Extract CheckFFMALongSetup() (5 minutes)**
```csharp
/// <summary>
/// V12.Phase7: Checks LONG setup conditions and executes entry if triggered
/// LONG: RSI < 20 + price 10+ pts below 9 EMA + GREEN candle
/// </summary>
private bool CheckFFMALongSetup(double rsiValue, double distanceFromEMA, 
                                 bool isGreenCandle, double currentPrice)
{
    if (!(rsiValue < FFMARSIOversold && distanceFromEMA <= -FFMAEMADistance && isGreenCandle))
        return false;

    Print(string.Format(
        "FFMA LONG TRIGGERED: RSI={0:F1} < {1} | Distance={2:F2}pts (below by {3}pts) | GREEN candle",
        rsiValue, FFMARSIOversold, distanceFromEMA, FFMAEMADistance));

    double stopPrice = Low[0];
    double stopDistance = Math.Min(Math.Abs(currentPrice - stopPrice), MaximumStop);
    
    if (stopDistance < tickSize * 2)
        stopDistance = tickSize * 2;

    int contracts = CalculatePositionSize(stopDistance);
    ExecuteFFMAEntry(MarketPosition.Long, contracts);
    return true;
}
```

**Step 7: Refactor CheckFFMAConditions() (3 minutes)**
```csharp
private void CheckFFMAConditions()
{
    if (!IsFFMAReadyToCheck())
        return;

    try
    {
        var marketData = GetFFMAMarketData();
        
        if (CheckFFMAShortSetup(marketData.rsiValue, marketData.distanceFromEMA, 
            marketData.isRedCandle, marketData.currentPrice))
            return;
        
        if (CheckFFMALongSetup(marketData.rsiValue, marketData.distanceFromEMA, 
            marketData.isGreenCandle, marketData.currentPrice))
            return;
    }
    catch (Exception ex)
    {
        Print("ERROR CheckFFMAConditions: " + ex.Message);
    }
}
```

**Step 8: Verification (6 minutes)**
```bash
# Run complexity audit
python scripts/complexity_audit.py

# Expected output:
# CheckFFMAConditions: CYC 3 (was 16)
# IsFFMAReadyToCheck: CYC 3
# GetFFMAMarketData: CYC 1
# CheckFFMAShortSetup: CYC 4
# CheckFFMALongSetup: CYC 4

# Run unit tests
dotnet test tests/V12_Performance.Tests/Entries/FFMAConditionsTests.cs

# Expected: 19/19 tests passing
```

**Step 9: Build & Deploy (5 minutes)**
```bash
# Update BUILD_TAG
# Edit src/V12_002.cs, increment BUILD_TAG

# Sync hard links
powershell -File .\deploy-sync.ps1

# Expected: 83/83 files synchronized

# F5 verification
# Open NinjaTrader 8
# Press F5
# Verify: Zero compilation errors, strategy loads, BUILD_TAG displays
```

---

### Rollback Plan

**If Tests Fail**:
1. Review test expectations vs implementation
2. Fix implementation (not tests)
3. Re-run tests until green

**If Complexity Audit Fails**:
1. Re-check CYC calculations
2. Further extract if any method >8
3. Re-run audit

**If F5 Fails**:
1. Check compilation errors
2. Verify `deploy-sync.ps1` ran successfully
3. Revert to last known good state if needed

**If Logic Drift Detected**:
1. Compare extracted code byte-for-byte with original
2. Revert any unintended changes
3. Re-extract with zero modifications

---

### Success Criteria

**Functional**:
- ✅ CheckFFMAConditions() CYC = 3 (verified by complexity_audit.py)
- ✅ All 4 helpers CYC ≤4 (verified by complexity_audit.py)
- ✅ FFMA behavior unchanged (verified by integration tests)

**Quality**:
- ✅ 19/19 unit tests passing (verified by dotnet test)
- ✅ F5 verification passed (verified manually)
- ✅ BUILD_TAG updated (verified in NinjaTrader output)
- ✅ Hard links synchronized (verified by deploy-sync.ps1)

**Documentation**:
- ✅ XML docs added for all helpers
- ✅ Ticket completion report generated
- ✅ Manifest updated

---

### Estimated Duration

| Step | Duration | Cumulative |
|------|----------|------------|
| Setup test file | 5 min | 5 min |
| Write TDD tests | 15 min | 20 min |
| Extract IsFFMAReadyToCheck | 3 min | 23 min |
| Extract GetFFMAMarketData | 3 min | 26 min |
| Extract CheckFFMAShortSetup | 5 min | 31 min |
| Extract CheckFFMALongSetup | 5 min | 36 min |
| Refactor CheckFFMAConditions | 3 min | 39 min |
| Verification | 6 min | 45 min |
| Build & Deploy | 5 min | 50 min |
| **Total** | **50 min** | |

**Buffer**: 10 minutes (for unexpected issues)  
**Total with Buffer**: 60 minutes (1 hour)

---

## Execution Context

### Parallel Execution (Optional)
**Cluster**: SIMA (Cluster 1)  
**Worktree**: `C:\WSGTA\universal-or-epic-cluster-1`  
**Branch**: `epic-cluster-1`

**Concurrent Tickets** (if running in parallel):
- EPIC-CCN-20-T1: Orders cluster
- EPIC-CCN-21-T1: Lifecycle cluster

### Sequential Execution (Default)
**Branch**: `gitbutler/workspace`  
**Working Directory**: `C:\WSGTA\universal-or-strategy`

---

## Approval Gates

### Phase 4 Gate: Ticket Generation
- ✅ Ticket specification complete
- ✅ Implementation steps defined
- ✅ Acceptance criteria clear
- ✅ Rollback plan documented
- ✅ Success criteria measurable
- ✅ Duration estimated

**Status**: ✅ **APPROVED** - Proceed to Phase 5.1 (Ticket 1 Execution)

---

**Phase Status**: Phase 4 COMPLETE  
**Next Phase**: Ticket 1 Execution (Phase 5.1)  
**Ready for Execution**: ✅ YES - All planning phases complete
