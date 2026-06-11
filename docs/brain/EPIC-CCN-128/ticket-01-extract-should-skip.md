# Ticket 1: Extract ShouldSkipTargetReplacement

**Epic**: EPIC-CCN-128  
**File**: `src/V12_002.Symmetry.Replace.cs`  
**Method**: `SymmetryGuardReplaceExistingFollowerTarget`  
**Priority**: P1 (Foundation for subsequent tickets)  
**Estimated Time**: 30 minutes

---

## Objective

Extract early exit condition logic into a dedicated helper method to reduce cognitive complexity and improve testability.

---

## Current State

**Lines**: 36-38 (embedded in main method)  
**Logic**: Checks if target replacement should be skipped due to filled/runner/zero-quantity conditions

```csharp
bool isRunner = IsRunnerTarget(targetNumber);
bool isFilled = IsTargetFilled(pos, targetNumber);
int qty = GetTargetContracts(pos, targetNumber);

if (isFilled || isRunner || qty <= 0)
{
    // Cleanup logic...
    return;
}
```

---

## Target State

**New Method Signature**:
```csharp
private bool ShouldSkipTargetReplacement(
    PositionInfo pos,
    int targetNumber,
    int qty,
    out bool isFilled,
    out bool isRunner
)
```

**Implementation**:
```csharp
private bool ShouldSkipTargetReplacement(
    PositionInfo pos,
    int targetNumber,
    int qty,
    out bool isFilled,
    out bool isRunner
)
{
    isFilled = IsTargetFilled(pos, targetNumber);
    isRunner = IsRunnerTarget(targetNumber);
    return isFilled || isRunner || qty <= 0;
}
```

**Complexity**: CYC 3 (3 OR conditions)  
**Lines**: 6

---

## Caller Update

**Before**:
```csharp
string targetTag = "T" + targetNumber;
bool isRunner = IsRunnerTarget(targetNumber);
bool isFilled = IsTargetFilled(pos, targetNumber);
int qty = GetTargetContracts(pos, targetNumber);

if (isFilled || isRunner || qty <= 0)
{
    // Cleanup logic...
    return;
}
```

**After**:
```csharp
string targetTag = "T" + targetNumber;
int qty = GetTargetContracts(pos, targetNumber);

if (ShouldSkipTargetReplacement(pos, targetNumber, qty, out bool isFilled, out bool isRunner))
{
    CleanupStaleTargetOrder(fleetEntryName, pos, dict);
    return;
}
```

---

## Unit Tests

**Test File**: `tests/V12_Performance.Tests/Symmetry/ShouldSkipTargetReplacementTests.cs`

```csharp
[Theory]
[InlineData(true, false, 1, true)]   // isFilled=true → skip
[InlineData(false, true, 1, true)]   // isRunner=true → skip
[InlineData(false, false, 0, true)]  // qty=0 → skip
[InlineData(false, false, -1, true)] // qty<0 → skip
[InlineData(false, false, 1, false)] // All false → continue
[InlineData(true, true, 0, true)]    // Multiple true → skip
[InlineData(true, false, 0, true)]   // isFilled + qty=0 → skip
[InlineData(false, true, 0, true)]   // isRunner + qty=0 → skip
public void ShouldSkipTargetReplacement_VariousConditions_ReturnsExpected(
    bool mockIsFilled,
    bool mockIsRunner,
    int qty,
    bool expectedSkip
)
{
    // Arrange
    var pos = CreateMockPosition();
    int targetNumber = 1;
    
    // Mock IsTargetFilled and IsRunnerTarget
    // (Implementation depends on test framework)
    
    // Act
    bool result = _sut.ShouldSkipTargetReplacement(
        pos, targetNumber, qty, out bool isFilled, out bool isRunner
    );
    
    // Assert
    Assert.Equal(expectedSkip, result);
    Assert.Equal(mockIsFilled, isFilled);
    Assert.Equal(mockIsRunner, isRunner);
}
```

---

## V12 DNA Compliance

- ✅ **Lock-Free**: No locks introduced
- ✅ **ASCII-Only**: No Unicode characters
- ✅ **CYC ≤ 8**: CYC 3 (target: 8)
- ✅ **Correctness by Construction**: Out parameters make state explicit
- ✅ **Single Responsibility**: Decision logic only

---

## Verification Steps

1. **Extract Method**:
   - Add `ShouldSkipTargetReplacement` method to `V12_002.Symmetry.Replace.cs`
   - Update caller to use new method
   - Preserve exact logic (zero drift)

2. **Build**:
   ```powershell
   dotnet build
   ```

3. **Deploy**:
   ```powershell
   powershell -File .\deploy-sync.ps1
   ```

4. **Unit Tests**:
   ```powershell
   dotnet test --filter "FullyQualifiedName~ShouldSkipTargetReplacementTests"
   ```

5. **Integration Test**:
   - F5 in NinjaTrader IDE
   - Verify BUILD_TAG appears in output
   - Verify no compilation errors

6. **Complexity Audit**:
   ```powershell
   python scripts/complexity_audit.py --file src/V12_002.Symmetry.Replace.cs
   ```

---

## Success Criteria

- ✅ Method extracted with CYC 3
- ✅ All 8 unit tests pass
- ✅ `deploy-sync.ps1` executes successfully
- ✅ BUILD_TAG verified in NinjaTrader output
- ✅ No compilation errors
- ✅ Zero logic drift (behavior unchanged)

---

## Rollback Plan

If verification fails:
```powershell
git checkout HEAD -- src/V12_002.Symmetry.Replace.cs
powershell -File .\deploy-sync.ps1
```

---

**Status**: PENDING  
**Dependencies**: None  
**Blocks**: Ticket 2, 3, 4, 5
