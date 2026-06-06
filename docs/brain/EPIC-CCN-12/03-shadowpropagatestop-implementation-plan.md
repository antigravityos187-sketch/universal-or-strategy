# EPIC-CCN-12 Stage 2: ShadowPropagateStopMoves Implementation Plan

**Date**: 2026-06-02  
**Epic**: EPIC-CCN-12  
**Target Method**: `ShadowPropagateStopMoves`  
**Stage**: Arch Planning (Implementation)

---

## Executive Summary

**Extraction Approach**: BOTTOM-UP TDD (5 phases)  
**Total Estimated Time**: 4-5 hours  
**Risk Level**: MEDIUM (cache coherence, no existing tests)  
**Rollback Strategy**: Git checkpoints after each phase  
**Success Criteria**: CYC 20 → 4, 17 tests passing, zero logic drift

---

## 1. Phase-by-Phase Extraction Plan

### Phase 0: Pre-Flight Checks (15 minutes)

#### Objectives
- Verify codebase compiles cleanly
- Create feature branch
- Set up test infrastructure
- Establish baseline metrics

#### Actions
1. **Compile Check**
   ```powershell
   dotnet build src/V12_002.csproj
   ```
   **Expected**: Zero errors, zero warnings

2. **Create Feature Branch**
   ```powershell
   git checkout -b feature/src-epic-ccn-12-shadowpropagatestop
   ```
   **Branch Pattern**: `feature/src-*` (SRC-ONLY per V12.18)

3. **Baseline Complexity Audit**
   ```powershell
   python scripts/complexity_audit.py
   ```
   **Expected**: `ShadowPropagateStopMoves` CYC = 20

4. **Create Test File**
   ```powershell
   New-Item -ItemType File -Path "tests/V12_Performance.Tests/Shadow/ShadowPropagateStopMovesTests.cs"
   ```

5. **Git Checkpoint**
   ```powershell
   git add .
   git commit -m "chore(epic-ccn-12): Phase 0 - Pre-flight checks complete"
   ```

#### Verification
- ✅ Build passes
- ✅ Feature branch created
- ✅ Test file exists
- ✅ Baseline CYC = 20

#### Estimated Time
**15 minutes**

---

### Phase 1: Extract `ValidateLeaderPosition` (45 minutes)

#### Objectives
- Extract validation helper (CYC 5)
- Write 3 unit tests
- Verify CYC reduction (20 → 16)

#### Step 1.1: Write Tests First (TDD) (20 minutes)

**Test File**: `tests/V12_Performance.Tests/Shadow/ShadowPropagateStopMovesTests.cs`

```csharp
[Test]
public void ValidateLeaderPosition_ValidLeader_ReturnsTrue()
{
    // Arrange
    var pos = new PositionInfo
    {
        IsFollower = false,
        EntryFilled = true,
        RemainingContracts = 10
    };
    var stopOrder = new Order { StopPrice = 4500.00 };
    _activePositions["LEADER_1"] = pos;
    _stopOrders["LEADER_1"] = stopOrder;

    // Act
    Order outStop;
    bool result = _strategy.ValidateLeaderPosition(pos, "LEADER_1", out outStop);

    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(stopOrder, outStop);
}

[Test]
public void ValidateLeaderPosition_FollowerPosition_ReturnsFalse()
{
    // Arrange
    var pos = new PositionInfo { IsFollower = true };

    // Act
    Order outStop;
    bool result = _strategy.ValidateLeaderPosition(pos, "FOLLOWER_1", out outStop);

    // Assert
    Assert.IsFalse(result);
    Assert.IsNull(outStop);
}

[Test]
public void ValidateLeaderPosition_UnfilledPosition_ReturnsFalse()
{
    // Arrange
    var pos = new PositionInfo
    {
        IsFollower = false,
        EntryFilled = false,
        RemainingContracts = 0
    };

    // Act
    Order outStop;
    bool result = _strategy.ValidateLeaderPosition(pos, "LEADER_1", out outStop);

    // Assert
    Assert.IsFalse(result);
    Assert.IsNull(outStop);
}
```

**Run Tests** (should fail - method doesn't exist yet):
```powershell
dotnet test tests/V12_Performance.Tests/V12_Performance.Tests.csproj --filter "FullyQualifiedName~ValidateLeaderPosition"
```

#### Step 1.2: Extract Helper Method (15 minutes)

**Location**: [`src/V12_002.SIMA.Shadow.cs`](src/V12_002.SIMA.Shadow.cs) (after line 79)

```csharp
/// <summary>
/// Validates leader position eligibility for stop propagation.
/// Returns true if position is a filled leader with a valid stop order.
/// </summary>
/// <param name="pos">Position to validate</param>
/// <param name="entryKey">Entry key for stop order lookup</param>
/// <param name="leaderStop">Output: leader stop order if valid</param>
/// <returns>True if position is eligible for propagation</returns>
private bool ValidateLeaderPosition(
    PositionInfo pos,
    string entryKey,
    out Order leaderStop
)
{
    leaderStop = null;

    if (pos == null || pos.IsFollower)
        return false;
    if (!pos.EntryFilled || pos.RemainingContracts <= 0)
        return false;

    if (!stopOrders.TryGetValue(entryKey, out leaderStop))
        return false;
    if (leaderStop == null || leaderStop.StopPrice <= 0)
        return false;

    return true;
}
```

#### Step 1.3: Refactor Main Method (5 minutes)

**Replace lines 37-46** with:
```csharp
Order leaderStop;
if (!ValidateLeaderPosition(kvp.Value, kvp.Key, out leaderStop))
    continue;
```

**Before** (lines 37-46):
```csharp
PositionInfo pos = kvp.Value;
if (pos == null || pos.IsFollower)
    continue;
if (!pos.EntryFilled || pos.RemainingContracts <= 0)
    continue;

Order leaderStop;
if (!stopOrders.TryGetValue(kvp.Key, out leaderStop))
    continue;
if (leaderStop == null || leaderStop.StopPrice <= 0)
    continue;
```

**After** (3 lines):
```csharp
Order leaderStop;
if (!ValidateLeaderPosition(kvp.Value, kvp.Key, out leaderStop))
    continue;
```

#### Step 1.4: Verify (5 minutes)

1. **Run Tests**:
   ```powershell
   dotnet test tests/V12_Performance.Tests/V12_Performance.Tests.csproj --filter "FullyQualifiedName~ValidateLeaderPosition"
   ```
   **Expected**: 3/3 tests pass

2. **Complexity Audit**:
   ```powershell
   python scripts/complexity_audit.py | Select-String "ShadowPropagateStopMoves"
   ```
   **Expected**: CYC = 16 (was 20, reduced by 4)

3. **Build Check**:
   ```powershell
   dotnet build src/V12_002.csproj
   ```
   **Expected**: Zero errors

4. **Git Checkpoint**:
   ```powershell
   git add .
   git commit -m "refactor(epic-ccn-12): Phase 1 - Extract ValidateLeaderPosition (CYC 20->16)"
   ```

#### Expected CYC Reduction
**20 → 16** (-4)

#### Estimated Time
**45 minutes**

---

### Phase 2: Extract `DetectStopPriceChange` (40 minutes)

#### Objectives
- Extract detection helper (CYC 2)
- Write 3 unit tests
- Verify CYC reduction (16 → 15)

#### Step 2.1: Write Tests First (TDD) (15 minutes)

```csharp
[Test]
public void DetectStopPriceChange_PriceChangedByOneTick_ReturnsTrue()
{
    // Arrange
    _leaderLastStopPrice["LEADER_1"] = 4500.00;
    double currentPrice = 4500.25; // +1 tick

    // Act
    double lastKnown;
    bool result = _strategy.DetectStopPriceChange("LEADER_1", currentPrice, out lastKnown);

    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(4500.00, lastKnown);
}

[Test]
public void DetectStopPriceChange_PriceChangedByNoise_ReturnsFalse()
{
    // Arrange
    _leaderLastStopPrice["LEADER_1"] = 4500.00;
    double currentPrice = 4500.10; // +0.4 ticks (< 0.5 threshold)

    // Act
    double lastKnown;
    bool result = _strategy.DetectStopPriceChange("LEADER_1", currentPrice, out lastKnown);

    // Assert
    Assert.IsFalse(result);
}

[Test]
public void DetectStopPriceChange_NoCachedPrice_ReturnsTrue()
{
    // Arrange
    double currentPrice = 4500.00;

    // Act
    double lastKnown;
    bool result = _strategy.DetectStopPriceChange("LEADER_1", currentPrice, out lastKnown);

    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(0.0, lastKnown);
}
```

#### Step 2.2: Extract Helper Method (15 minutes)

**Location**: [`src/V12_002.SIMA.Shadow.cs`](src/V12_002.SIMA.Shadow.cs) (after `ValidateLeaderPosition`)

```csharp
/// <summary>
/// Detects if leader stop price changed beyond noise threshold.
/// Uses half-tick threshold to filter out insignificant price movements.
/// </summary>
/// <param name="entryKey">Entry key for cache lookup</param>
/// <param name="currentStopPrice">Current stop price from order</param>
/// <param name="lastKnownPrice">Output: last known price from cache</param>
/// <returns>True if price changed beyond threshold</returns>
private bool DetectStopPriceChange(
    string entryKey,
    double currentStopPrice,
    out double lastKnownPrice
)
{
    _leaderLastStopPrice.TryGetValue(entryKey, out lastKnownPrice);

    // Only propagate if price actually changed (beyond half-tick noise)
    if (Math.Abs(currentStopPrice - lastKnownPrice) < tickSize * 0.5)
        return false;

    return true;
}
```

#### Step 2.3: Refactor Main Method (5 minutes)

**Replace lines 49-54** with:
```csharp
double lastKnownPrice;
if (!DetectStopPriceChange(kvp.Key, leaderStop.StopPrice, out lastKnownPrice))
    continue;
```

#### Step 2.4: Verify (5 minutes)

1. **Run Tests**: 3/3 pass
2. **Complexity Audit**: CYC = 15 (was 16, reduced by 1)
3. **Build Check**: Zero errors
4. **Git Checkpoint**:
   ```powershell
   git commit -m "refactor(epic-ccn-12): Phase 2 - Extract DetectStopPriceChange (CYC 16->15)"
   ```

#### Expected CYC Reduction
**16 → 15** (-1)

#### Estimated Time
**40 minutes**

---

### Phase 3: Extract `PropagateAndCacheStopPrice` (40 minutes)

#### Objectives
- Extract action helper (CYC 2)
- Write 3 unit tests
- Verify CYC reduction (15 → 14)

#### Step 3.1: Write Tests First (TDD) (15 minutes)

```csharp
[Test]
public void PropagateAndCacheStopPrice_PropagationSucceeds_CacheUpdated()
{
    // Arrange
    _strategy.StubShadowMoveFollowerStops(true);

    // Act
    _strategy.PropagateAndCacheStopPrice("LEADER_1", 4500.25);

    // Assert
    Assert.IsTrue(_leaderLastStopPrice.ContainsKey("LEADER_1"));
    Assert.AreEqual(4500.25, _leaderLastStopPrice["LEADER_1"]);
}

[Test]
public void PropagateAndCacheStopPrice_PropagationFails_CacheNotUpdated()
{
    // Arrange
    _strategy.StubShadowMoveFollowerStops(false);

    // Act
    _strategy.PropagateAndCacheStopPrice("LEADER_1", 4500.25);

    // Assert
    Assert.IsFalse(_leaderLastStopPrice.ContainsKey("LEADER_1"));
}

[Test]
public void PropagateAndCacheStopPrice_MultiplePropagations_CacheReflectsLatest()
{
    // Arrange
    _strategy.StubShadowMoveFollowerStops(true);

    // Act
    _strategy.PropagateAndCacheStopPrice("LEADER_1", 4500.00);
    _strategy.PropagateAndCacheStopPrice("LEADER_1", 4500.25);
    _strategy.PropagateAndCacheStopPrice("LEADER_1", 4500.50);

    // Assert
    Assert.AreEqual(4500.50, _leaderLastStopPrice["LEADER_1"]);
}
```

#### Step 3.2: Extract Helper Method (15 minutes)

**Location**: [`src/V12_002.SIMA.Shadow.cs`](src/V12_002.SIMA.Shadow.cs) (after `DetectStopPriceChange`)

```csharp
/// <summary>
/// Propagates stop price to followers and updates cache on success.
/// Cache is only updated if propagation succeeds (all followers ready).
/// </summary>
/// <param name="leaderEntryKey">Leader entry key</param>
/// <param name="newStopPrice">New stop price to propagate</param>
private void PropagateAndCacheStopPrice(
    string leaderEntryKey,
    double newStopPrice
)
{
    // Find and update all follower positions linked to this leader entry
    if (ShadowMoveFollowerStops(leaderEntryKey, newStopPrice))
        _leaderLastStopPrice[leaderEntryKey] = newStopPrice;
}
```

#### Step 3.3: Refactor Main Method (5 minutes)

**Replace lines 57-58** with:
```csharp
PropagateAndCacheStopPrice(kvp.Key, leaderStop.StopPrice);
```

#### Step 3.4: Verify (5 minutes)

1. **Run Tests**: 3/3 pass
2. **Complexity Audit**: CYC = 14 (was 15, reduced by 1)
3. **Build Check**: Zero errors
4. **Git Checkpoint**:
   ```powershell
   git commit -m "refactor(epic-ccn-12): Phase 3 - Extract PropagateAndCacheStopPrice (CYC 15->14)"
   ```

#### Expected CYC Reduction
**15 → 14** (-1)

#### Estimated Time
**40 minutes**

---

### Phase 4: Extract `ValidateCachedEntry` (60 minutes)

#### Objectives
- Extract validation helper (CYC 9)
- Write 5 unit tests
- Verify CYC reduction (14 → 6)

#### Step 4.1: Write Tests First (TDD) (25 minutes)

```csharp
[Test]
public void ValidateCachedEntry_ValidEntry_ReturnsTrue()
{
    // Arrange
    var pos = new PositionInfo
    {
        IsFollower = false,
        EntryFilled = true,
        RemainingContracts = 10
    };
    var stopOrder = new Order { StopPrice = 4500.00 };
    _activePositions["LEADER_1"] = pos;
    _stopOrders["LEADER_1"] = stopOrder;

    // Act
    PositionInfo outPos;
    Order outStop;
    bool result = _strategy.ValidateCachedEntry("LEADER_1", out outPos, out outStop);

    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(pos, outPos);
    Assert.AreEqual(stopOrder, outStop);
}

[Test]
public void ValidateCachedEntry_PositionClosed_ReturnsFalse()
{
    // Arrange
    _leaderLastStopPrice["LEADER_1"] = 4500.00;

    // Act
    PositionInfo outPos;
    Order outStop;
    bool result = _strategy.ValidateCachedEntry("LEADER_1", out outPos, out outStop);

    // Assert
    Assert.IsFalse(result);
}

[Test]
public void ValidateCachedEntry_PositionBecameFollower_ReturnsFalse()
{
    // Arrange
    var pos = new PositionInfo { IsFollower = true };
    _activePositions["LEADER_1"] = pos;

    // Act
    PositionInfo outPos;
    Order outStop;
    bool result = _strategy.ValidateCachedEntry("LEADER_1", out outPos, out outStop);

    // Assert
    Assert.IsFalse(result);
}

[Test]
public void ValidateCachedEntry_StopOrderRemoved_ReturnsFalse()
{
    // Arrange
    var pos = new PositionInfo
    {
        IsFollower = false,
        EntryFilled = true,
        RemainingContracts = 10
    };
    _activePositions["LEADER_1"] = pos;

    // Act
    PositionInfo outPos;
    Order outStop;
    bool result = _strategy.ValidateCachedEntry("LEADER_1", out outPos, out outStop);

    // Assert
    Assert.IsFalse(result);
}

[Test]
public void ValidateCachedEntry_StopPriceInvalid_ReturnsFalse()
{
    // Arrange
    var pos = new PositionInfo
    {
        IsFollower = false,
        EntryFilled = true,
        RemainingContracts = 10
    };
    var stopOrder = new Order { StopPrice = 0.0 };
    _activePositions["LEADER_1"] = pos;
    _stopOrders["LEADER_1"] = stopOrder;

    // Act
    PositionInfo outPos;
    Order outStop;
    bool result = _strategy.ValidateCachedEntry("LEADER_1", out outPos, out outStop);

    // Assert
    Assert.IsFalse(result);
}
```

#### Step 4.2: Extract Helper Method (20 minutes)

**Location**: [`src/V12_002.SIMA.Shadow.cs`](src/V12_002.SIMA.Shadow.cs) (after `PropagateAndCacheStopPrice`)

```csharp
/// <summary>
/// Validates that a cached stop price entry is still valid.
/// Returns false if position closed, stop removed, or became follower.
/// </summary>
/// <param name="entryKey">Entry key to validate</param>
/// <param name="livePos">Output: live position if valid</param>
/// <param name="liveStop">Output: live stop order if valid</param>
/// <returns>True if cache entry is still valid</returns>
private bool ValidateCachedEntry(
    string entryKey,
    out PositionInfo livePos,
    out Order liveStop
)
{
    livePos = null;
    liveStop = null;

    if (!activePositions.TryGetValue(entryKey, out livePos))
        return false;
    if (livePos == null)
        return false;
    if (livePos.IsFollower)
        return false;
    if (!livePos.EntryFilled)
        return false;
    if (livePos.RemainingContracts <= 0)
        return false;
    if (!stopOrders.TryGetValue(entryKey, out liveStop))
        return false;
    if (liveStop == null)
        return false;
    if (liveStop.StopPrice <= 0)
        return false;

    return true;
}
```

#### Step 4.3: Refactor Main Method (10 minutes)

**Replace lines 65-74** with:
```csharp
PositionInfo livePos;
Order liveStop;
if (!ValidateCachedEntry(cacheKvp.Key, out livePos, out liveStop))
{
    _leaderLastStopPrice.TryRemove(cacheKvp.Key, out _);
}
```

**Before** (10 lines):
```csharp
PositionInfo livePos;
Order liveStop;
if (
    !activePositions.TryGetValue(cacheKvp.Key, out livePos)
    || livePos == null
    || livePos.IsFollower
    || !livePos.EntryFilled
    || livePos.RemainingContracts <= 0
    || !stopOrders.TryGetValue(cacheKvp.Key, out liveStop)
    || liveStop == null
    || liveStop.StopPrice <= 0
)
{
    _leaderLastStopPrice.TryRemove(cacheKvp.Key, out _);
}
```

**After** (6 lines):
```csharp
PositionInfo livePos;
Order liveStop;
if (!ValidateCachedEntry(cacheKvp.Key, out livePos, out liveStop))
{
    _leaderLastStopPrice.TryRemove(cacheKvp.Key, out _);
}
```

#### Step 4.4: Verify (5 minutes)

1. **Run Tests**: 5/5 pass
2. **Complexity Audit**: CYC = 6 (was 14, reduced by 8)
3. **Build Check**: Zero errors
4. **Git Checkpoint**:
   ```powershell
   git commit -m "refactor(epic-ccn-12): Phase 4 - Extract ValidateCachedEntry (CYC 14->6)"
   ```

#### Expected CYC Reduction
**14 → 6** (-8)

#### Estimated Time
**60 minutes**

---

### Phase 5: Extract `CleanupStaleCache` (50 minutes)

#### Objectives
- Extract cleanup helper (CYC 3)
- Write 3 unit tests
- Verify CYC reduction (6 → 4)

#### Step 5.1: Write Tests First (TDD) (20 minutes)

```csharp
[Test]
public void CleanupStaleCache_AllEntriesValid_NoEvictions()
{
    // Arrange
    var pos = new PositionInfo
    {
        IsFollower = false,
        EntryFilled = true,
        RemainingContracts = 10
    };
    var stopOrder = new Order { StopPrice = 4500.00 };
    _activePositions["LEADER_1"] = pos;
    _stopOrders["LEADER_1"] = stopOrder;
    _leaderLastStopPrice["LEADER_1"] = 4500.00;

    // Act
    _strategy.CleanupStaleCache();

    // Assert
    Assert.IsTrue(_leaderLastStopPrice.ContainsKey("LEADER_1"));
    Assert.AreEqual(1, _leaderLastStopPrice.Count);
}

[Test]
public void CleanupStaleCache_OneStaleEntry_Evicted()
{
    // Arrange
    _leaderLastStopPrice["LEADER_1"] = 4500.00;
    _leaderLastStopPrice["LEADER_2"] = 4510.00;
    var pos2 = new PositionInfo
    {
        IsFollower = false,
        EntryFilled = true,
        RemainingContracts = 10
    };
    var stop2 = new Order { StopPrice = 4510.00 };
    _activePositions["LEADER_2"] = pos2;
    _stopOrders["LEADER_2"] = stop2;

    // Act
    _strategy.CleanupStaleCache();

    // Assert
    Assert.IsFalse(_leaderLastStopPrice.ContainsKey("LEADER_1"));
    Assert.IsTrue(_leaderLastStopPrice.ContainsKey("LEADER_2"));
}

[Test]
public void CleanupStaleCache_MultipleStaleEntries_AllEvicted()
{
    // Arrange
    _leaderLastStopPrice["LEADER_1"] = 4500.00;
    _leaderLastStopPrice["LEADER_2"] = 4510.00;
    _leaderLastStopPrice["LEADER_3"] = 4520.00;

    // Act
    _strategy.CleanupStaleCache();

    // Assert
    Assert.AreEqual(0, _leaderLastStopPrice.Count);
}
```

#### Step 5.2: Extract Helper Method (15 minutes)

**Location**: [`src/V12_002.SIMA.Shadow.cs`](src/V12_002.SIMA.Shadow.cs) (after `ValidateCachedEntry`)

```csharp
/// <summary>
/// Evicts stale entries from leader stop price cache.
/// Runs after propagation pass to remove entries for closed positions.
/// </summary>
private void CleanupStaleCache()
{
    foreach (var cacheKvp in _leaderLastStopPrice.ToArray())
    {
        PositionInfo livePos;
        Order liveStop;
        if (!ValidateCachedEntry(cacheKvp.Key, out livePos, out liveStop))
        {
            _leaderLastStopPrice.TryRemove(cacheKvp.Key, out _);
        }
    }
}
```

#### Step 5.3: Refactor Main Method (10 minutes)

**Replace lines 61-78** with:
```csharp
// Phase 2: Cleanup stale cache entries
CleanupStaleCache();
```

**Before** (18 lines):
```csharp
foreach (var cacheKvp in _leaderLastStopPrice.ToArray())
{
    PositionInfo livePos;
    Order liveStop;
    if (!ValidateCachedEntry(cacheKvp.Key, out livePos, out liveStop))
    {
        _leaderLastStopPrice.TryRemove(cacheKvp.Key, out _);
    }
}
```

**After** (2 lines):
```csharp
// Phase 2: Cleanup stale cache entries
CleanupStaleCache();
```

#### Step 5.4: Verify (5 minutes)

1. **Run Tests**: 3/3 pass
2. **Complexity Audit**: CYC = 4 (was 6, reduced by 2)
3. **Build Check**: Zero errors
4. **Git Checkpoint**:
   ```powershell
   git commit -m "refactor(epic-ccn-12): Phase 5 - Extract CleanupStaleCache (CYC 6->4)"
   ```

#### Expected CYC Reduction
**6 → 4** (-2)

#### Estimated Time
**50 minutes**

---

### Phase 6: Final Verification (30 minutes)

#### Objectives
- Run full test suite (17 tests)
- Verify final CYC = 4
- Run pre-push validation
- Update BUILD_TAG

#### Step 6.1: Full Test Suite (10 minutes)

```powershell
dotnet test tests/V12_Performance.Tests/V12_Performance.Tests.csproj --filter "FullyQualifiedName~ShadowPropagate"
```

**Expected**: 17/17 tests pass

#### Step 6.2: Complexity Audit (5 minutes)

```powershell
python scripts/complexity_audit.py | Select-String "Shadow"
```

**Expected Output**:
```
ShadowPropagateStopMoves: CYC 4 (was 20, reduced by 16)
ValidateLeaderPosition: CYC 5
DetectStopPriceChange: CYC 2
PropagateAndCacheStopPrice: CYC 2
ValidateCachedEntry: CYC 9
CleanupStaleCache: CYC 3
```

#### Step 6.3: Pre-Push Validation (10 minutes)

```powershell
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

**Expected**: All checks pass

#### Step 6.4: Update BUILD_TAG (5 minutes)

**File**: [`src/V12_002.cs`](src/V12_002.cs)

**Find**:
```csharp
private const string BUILD_TAG = "1111.XXX";
```

**Replace with**:
```csharp
private const string BUILD_TAG = "1111.XXX-epic-ccn-12";
```

#### Step 6.5: Final Commit

```powershell
git add .
git commit -m "refactor(epic-ccn-12): Phase 6 - Final verification complete (CYC 20->4)"
```

#### Estimated Time
**30 minutes**

---

## 2. TDD Test Implementation

### Test File Structure

**Location**: `tests/V12_Performance.Tests/Shadow/ShadowPropagateStopMovesTests.cs`

**Total Lines**: ~600 lines (17 tests × ~35 lines each)

### Test Class Template

```csharp
using NUnit.Framework;
using NinjaTrader.Cbi;
using System.Collections.Concurrent;

namespace V12_Performance.Tests.Shadow
{
    [TestFixture]
    public class ShadowPropagateStopMovesTests
    {
        private V12_002_TestHarness _strategy;
        private ConcurrentDictionary<string, PositionInfo> _activePositions;
        private ConcurrentDictionary<string, Order> _stopOrders;
        private ConcurrentDictionary<string, double> _leaderLastStopPrice;
        private double _tickSize;

        [SetUp]
        public void Setup()
        {
            _strategy = new V12_002_TestHarness();
            _activePositions = new ConcurrentDictionary<string, PositionInfo>();
            _stopOrders = new ConcurrentDictionary<string, Order>();
            _leaderLastStopPrice = new ConcurrentDictionary<string, double>();
            _tickSize = 0.25; // ES tick size

            // Inject dependencies into strategy
            _strategy.InjectDependencies(
                _activePositions,
                _stopOrders,
                _leaderLastStopPrice,
                _tickSize
            );
        }

        [TearDown]
        public void TearDown()
        {
            _activePositions.Clear();
            _stopOrders.Clear();
            _leaderLastStopPrice.Clear();
        }

        // 17 test methods (see Phase 1-5 for details)
    }
}
```

### Test Harness Requirements

**File**: `tests/V12_Performance.Tests/Harness/V12_002_TestHarness.cs`

```csharp
public class V12_002_TestHarness : V12_002
{
    private bool _shadowMoveFollowerStopsStub = true;

    public void InjectDependencies(
        ConcurrentDictionary<string, PositionInfo> activePositions,
        ConcurrentDictionary<string, Order> stopOrders,
        ConcurrentDictionary<string, double> leaderLastStopPrice,
        double tickSize
    )
    {
        this.activePositions = activePositions;
        this.stopOrders = stopOrders;
        this._leaderLastStopPrice = leaderLastStopPrice;
        this.tickSize = tickSize;
    }

    public void StubShadowMoveFollowerStops(bool returnValue)
    {
        _shadowMoveFollowerStopsStub = returnValue;
    }

    // Override ShadowMoveFollowerStops for testing
    protected override bool ShadowMoveFollowerStops(string leaderEntryKey, double newStopPrice)
    {
        return _shadowMoveFollowerStopsStub;
    }

    // Expose private methods for testing
    public new bool ValidateLeaderPosition(PositionInfo pos, string entryKey, out Order leaderStop)
        => base.ValidateLeaderPosition(pos, entryKey, out leaderStop);

    public new bool DetectStopPriceChange(string entryKey, double currentStopPrice, out double lastKnownPrice)
        => base.DetectStopPriceChange(entryKey, currentStopPrice, out lastKnownPrice);

    public new void PropagateAndCacheStopPrice(string leaderEntryKey, double newStopPrice)
        => base.PropagateAndCacheStopPrice(leaderEntryKey, newStopPrice);

    public new bool ValidateCachedEntry(string entryKey, out PositionInfo livePos, out Order liveStop)
        => base.ValidateCachedEntry(entryKey, out livePos, out liveStop);

    public new void CleanupStaleCache()
        => base.CleanupStaleCache();
}
```

---

## 3. Validation Checklist

### Pre-Extraction Checks

- [ ] Codebase compiles cleanly (zero errors)
- [ ] Baseline CYC = 20 confirmed
- [ ] Feature branch created (`feature/src-epic-ccn-12-shadowpropagatestop`)
- [ ] Test file created
- [ ] Test harness implemented

### During-Extraction Checks (Per Phase)

- [ ] Tests written first (TDD)
- [ ] Tests fail before extraction (red)
- [ ] Helper method extracted
- [ ] Main method refactored
- [ ] Tests pass after extraction (green)
- [ ] CYC reduced as expected
- [ ] Build passes (zero errors)
- [ ] Git checkpoint created

### Post-Extraction Checks

- [ ] All 17 tests pass
- [ ] Final CYC = 4 confirmed
- [ ] Pre-push validation passes
- [ ] BUILD_TAG updated
- [ ] No logic drift (behavior identical)
- [ ] ASCII-only compliance verified
- [ ] Lock-free pattern preserved

---

## 4. Rollback Plan

### Checkpoint Strategy

**Git Checkpoints**: After each phase (6 total)

```
Phase 0: Pre-flight checks
Phase 1: ValidateLeaderPosition extracted
Phase 2: DetectStopPriceChange extracted
Phase 3: PropagateAndCacheStopPrice extracted
Phase 4: ValidateCachedEntry extracted
Phase 5: CleanupStaleCache extracted
Phase 6: Final verification
```

### Recovery Procedure

#### Scenario 1: Test Failure During Phase

**Symptoms**: Tests fail after extraction

**Recovery**:
1. Review test failure output
2. Check for typos in helper method
3. Verify main method refactoring
4. If unfixable within 15 minutes:
   ```powershell
   git reset --hard HEAD~1
   ```
5. Restart phase from checkpoint

**Recovery Time**: 5-15 minutes

---

#### Scenario 2: Build Failure

**Symptoms**: Compilation errors after extraction

**Recovery**:
1. Check for missing braces, semicolons
2. Verify method signature matches calls
3. If unfixable within 10 minutes:
   ```powershell
   git reset --hard HEAD~1
   ```
4. Restart phase from checkpoint

**Recovery Time**: 5-10 minutes

---

#### Scenario 3: CYC Reduction Not Achieved

**Symptoms**: Complexity audit shows unexpected CYC

**Recovery**:
1. Re-run complexity audit
2. Check if all decision points extracted
3. Verify main method refactoring complete
4. If CYC still wrong:
   ```powershell
   git reset --hard HEAD~1
   ```
5. Review extraction plan, restart phase

**Recovery Time**: 10-20 minutes

---

#### Scenario 4: Logic Drift Detected

**Symptoms**: Behavior changed after extraction

**Recovery**:
1. Run full test suite
2. Compare before/after behavior
3. Check for missing conditions
4. If drift confirmed:
   ```powershell
   git reset --hard HEAD~1
   ```
5. Review extraction carefully, restart phase

**Recovery Time**: 15-30 minutes

---

#### Scenario 5: Pre-Push Validation Failure

**Symptoms**: ASCII gate or build check fails

**Recovery**:
1. Check for Unicode characters
2. Verify build passes locally
3. Run `dotnet csharpier format src/`
4. If still failing:
   ```powershell
   git reset --hard <last-good-commit>
   ```
5. Review all changes, restart from last good checkpoint

**Recovery Time**: 10-20 minutes

---

### Full Rollback (Nuclear Option)

**When**: Multiple phases fail, unrecoverable state

**Procedure**:
```powershell
git checkout main
git branch -D feature/src-epic-ccn-12-shadowpropagatestop
git checkout -b feature/src-epic-ccn-12-shadowpropagatestop
```

**Recovery Time**: 5 minutes (restart from Phase 0)

---

## 5. Risk Mitigation

### Risk 1: Cache Coherence Issues

**Probability**: MEDIUM  
**Impact**: HIGH (data corruption)

**Mitigation**:
- Use `ConcurrentDictionary` atomic operations
- Preserve read → propagate → write order
- Test cache update scenarios thoroughly

**Contingency**:
- Add logging to track cache operations
- Add assertions to detect stale reads

---

### Risk 2: Test Harness Complexity

**Probability**: MEDIUM  
**Impact**: MEDIUM (delayed testing)

**Mitigation**:
- Use real `ConcurrentDictionary` (no mocking)
- Stub only `ShadowMoveFollowerStops()`
- Keep test harness minimal

**Contingency**:
- Simplify test harness if too complex
- Use integration tests as fallback

---

### Risk 3: Logic Drift During Extraction

**Probability**: LOW  
**Impact**: HIGH (behavior change)

**Mitigation**:
- Pure structural extraction (no logic changes)
- Preserve exact validation order
- Test after each phase

**Contingency**:
- Rollback to last checkpoint
- Review extraction carefully
- Add more granular tests

---

### Risk 4: Performance Regression

**Probability**: LOW  
**Impact**: MEDIUM (slower execution)

**Mitigation**:
- No new allocations (same `ToArray()` calls)
- No additional dictionary lookups
- Preserve snapshot iteration pattern

**Contingency**:
- Profile before/after with benchmarks
- Optimize if regression detected

---

### Risk 5: Merge Conflicts

**Probability**: LOW  
**Impact**: MEDIUM (delayed merge)

**Mitigation**:
- Work on dedicated feature branch
- Rebase frequently on main
- Keep changes isolated to Shadow module

**Contingency**:
- Resolve conflicts manually
- Use `git mergetool` if needed

---

## 6. Timeline Summary

| Phase | Task | Time | Cumulative |
|-------|------|------|------------|
| 0 | Pre-flight checks | 15 min | 15 min |
| 1 | Extract `ValidateLeaderPosition` | 45 min | 60 min |
| 2 | Extract `DetectStopPriceChange` | 40 min | 100 min |
| 3 | Extract `PropagateAndCacheStopPrice` | 40 min | 140 min |
| 4 | Extract `ValidateCachedEntry` | 60 min | 200 min |
| 5 | Extract `CleanupStaleCache` | 50 min | 250 min |
| 6 | Final verification | 30 min | 280 min |
| **Total** | | **4h 40m** | |

**Buffer**: +20 minutes (contingency)  
**Total Estimated Time**: **5 hours**

---

## 7. Success Metrics

### Functional Metrics
- ✅ CYC reduced from 20 to 4 (80% reduction)
- ✅ 17 tests passing (113% of minimum 15)
- ✅ Zero logic drift (behavior identical)
- ✅ 100% branch coverage in helpers

### Non-Functional Metrics
- ✅ Build passes (zero errors, zero warnings)
- ✅ Pre-push validation passes (all checks)
- ✅ ASCII-only compliance verified
- ✅ Lock-free pattern preserved (ADR-019)

### Process Metrics
- ✅ 6 git checkpoints created
- ✅ TDD approach followed (tests first)
- ✅ Bottom-up extraction order maintained
- ✅ Rollback plan tested (if needed)

---

## 8. Post-Extraction Tasks

### Immediate (Same Session)
1. Run full test suite (all 17 tests)
2. Run complexity audit (verify CYC = 4)
3. Run pre-push validation (all checks)
4. Update BUILD_TAG
5. Create PR (feature → main)

### Follow-Up (Next Session)
1. Address PR review comments
2. Merge PR after approval
3. Monitor production for issues
4. Update EPIC-CCN-12 status (complete)

### Future Work
1. Extract `ShadowMoveFollowerStops()` (CYC 15) in separate epic
2. Add performance benchmarks
3. Add metrics tracking (optional)

---

**End of Stage 2 Implementation Plan**

**Next Stage**: DNA & PR Audit (Stage 3) - Verify V12 compliance and PR health