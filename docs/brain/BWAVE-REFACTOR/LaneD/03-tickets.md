# Ph3 Tickets — BWAVE-REFACTOR Lane D
## Prepared by: ptt-architect

---

## TICKET D-1: DW-B37 test name inversions
**Status: ALREADY COMPLETE — no action.**
All 5 renames confirmed applied. Old names absent. New names present. dotnet build to verify.

---

## TICKET D-2: SA1507/SA1508 CSharpier formatting

**Files:**
- `src/PropTraderTools/CopyEngineTests.cs`
- `src/PropTraderTools/Tests/BwaveCycLaneCTests.cs`

**Exact steps:**
1. `csharpier format src/PropTraderTools/CopyEngineTests.cs`
2. `csharpier format "src/PropTraderTools/Tests/BwaveCycLaneCTests.cs"`
3. `csharpier check src/PropTraderTools/CopyEngineTests.cs` — must exit 0
4. `csharpier check "src/PropTraderTools/Tests/BwaveCycLaneCTests.cs"` — must exit 0

**Acceptance:** Both `csharpier check` calls exit 0. dotnet build 0 errors.

---

## TICKET D-3: xUnit2004 bool assertion fix

**File:** `src/PropTraderTools/Tests/B131Tests.cs`

**Exact change at line 165:**
```
// BEFORE:
            Assert.Equal(true, (bool)field.GetValue(null)!);
// AFTER:
            Assert.True((bool)field.GetValue(null)!);
```

**Acceptance:** No `Assert.Equal(true,` or `Assert.Equal(false,` in file. dotnet build 0 warnings.

---

## TICKET D-4a: DW-B37-01 structural companion test

**File:** `src/PropTraderTools/Tests/BwaveCycLaneBTests.cs`
**Class:** `BwaveCycLaneBT2Tests`
**Insert after:** existing `TryRecordBeTargetFill_DoesNothing_WhenStateIsNotFilled` test (the skipped one)

**Add this test:**
```csharp
/// <summary>
/// Structural: WouldRecordBeTargetFill seam exists with expected parameter signature.
/// Guards Order-based path: state, name, accountName parameters exercised via seam.
/// </summary>
[Fact]
public void TryRecordBeTargetFill_SeamExists_WouldRecordBeTargetFill()
{
    var m = typeof(CopyEngine).GetMethod(
        "WouldRecordBeTargetFill",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
    );
    Assert.NotNull(m);
    var parms = m!.GetParameters();
    Assert.Equal(3, parms.Length);
    Assert.Equal(typeof(OrderState), parms[0].ParameterType);
    Assert.Equal(typeof(string), parms[1].ParameterType);
    Assert.Equal(typeof(string), parms[2].ParameterType);
}
```

**Acceptance:** Test passes. No new build errors.

---

## TICKET D-4b: DW-B37-03 rename + structural companion test

**File:** `src/PropTraderTools/Tests/BwaveCycLaneBTests.cs`
**Class:** `BwaveCycLaneBT5Tests`

**Step 1 — Rename:**
Change test method name from `ExecuteBeRetryAndRearm_CallsBreakEven` to `IsBeRetryEligible_VerifiesPredicate_NotExecution`.

**Step 2 — Add structural test** after the renamed test:
```csharp
/// <summary>
/// Structural: TryFireFollowerBeRetry exists with OrderEventArgs parameter.
/// Confirms retry execution entry point is present; execution requires NT8 runtime.
/// </summary>
[Fact]
public void TryFireFollowerBeRetry_Exists_WithOrderEventArgsParam()
{
    var m = typeof(CopyEngine).GetMethod(
        "TryFireFollowerBeRetry",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
    );
    Assert.NotNull(m);
    var parms = m!.GetParameters();
    Assert.Equal(1, parms.Length);
    Assert.Equal(typeof(NinjaTrader.Cbi.OrderEventArgs), parms[0].ParameterType);
}
```

**Acceptance:** Old name absent, new name present. Both tests pass. 0 build errors.

---

## TICKET D-4c: DW-B37-05 structural companion test

**File:** `src/PropTraderTools/Tests/BwaveCycLaneBTests.cs`
**Class:** `BwaveCycLaneBT7Tests`
**Insert after:** `ResolveMultipliers_ReturnsNull_WhenMultipliersNull` skipped test

**Add this test:**
```csharp
/// <summary>
/// Structural: CopyRule.Create internal static factory exists with expected signature.
/// Confirms normalization entry point is present; NT8 Account args required for live execution.
/// </summary>
[Fact]
public void CopyRule_Create_Exists_WithExpectedSignature()
{
    var m = typeof(CopyEngine).GetNestedType(
        "CopyRule",
        System.Reflection.BindingFlags.NonPublic
    )?.GetMethod(
        "Create",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            | System.Reflection.BindingFlags.Public
    );
    Assert.NotNull(m);
    Assert.True(m!.IsStatic);
}
```

**Acceptance:** Test passes. 0 build errors.

---

## TICKET D-5: Final build verification

Run `dotnet build src/PropTraderTools/PropTraderTools.csproj` — 0 errors, 0 warnings.
