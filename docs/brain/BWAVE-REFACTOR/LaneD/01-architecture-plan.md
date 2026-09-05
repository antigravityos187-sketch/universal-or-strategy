# Ph1 Architecture Plan — BWAVE-REFACTOR Lane D
## Prepared by: ptt-architect

---

## Current State Assessment

### D-1: DW-B37 test name inversions (BwaveCycLaneBTests.cs)
**Status: ALREADY COMPLETE.** All 5 renames have been applied in prior work:
- `IsPttBeRetryTriggerOrder_ReturnsTrue_WhenNameIsPttQxT` — present (line ~437)
- `IsNativeExitName_ReturnsFalse_WhenNameIsTarget` — present (line ~551)
- `ResolveMultipliers_ReturnsNull_WhenMultipliersNull` — present as Skipped test (line ~712)
- `SelectRefPriceByDirection_ReturnsAsk_WhenLong` — present (line ~729)
- `SelectRefPriceByDirection_ReturnsBid_WhenShort` — present (line ~758)
Old names are absent. **No work needed.** Acceptance already met.

### D-2: SA1507/SA1508 CSharpier formatting
**Status: REQUIRES FORMATTING.**
- `src/PropTraderTools/CopyEngineTests.cs` — CSharpier reports "Was not formatted" (unrelated location ~line 4108, NOT lines 6843/6920/6921 which look structurally clean).
- `src/PropTraderTools/Tests/BwaveCycLaneCTests.cs` — CSharpier reports "Was not formatted" (missing blank line before `[Fact]` around line 1033).

**Plan:** Run `csharpier format` on both files. This is the safe, correct approach for SA1507/SA1508 blank-line violations. CSharpier will add/remove blank lines per StyleCop rules without altering logic.

**Constraint:** CopyEngineTests.cs is very large (7565 lines). The diff will touch formatting only. Build must pass after.

### D-3: DW-WARN-B131 xUnit2004
**Status: REQUIRES FIX.**
- `src/PropTraderTools/Tests/B131Tests.cs` line 165:
  `Assert.Equal(true, (bool)field.GetValue(null)!);`
  → Replace with: `Assert.True((bool)field.GetValue(null)!);`
- Only 1 occurrence. Simple, surgical replacement.

### D-4: DW-B37 test hardening (BwaveCycLaneBTests.cs)
**Status: PARTIALLY DONE — additions needed.**

**DW-B37-01 (line 142 area):**
The test `TryRecordBeTargetFill_DoesNothing_WhenStateIsNotFilled` is already `[Fact(Skip = "NT8-HOST-REQUIRED...")]`. The ticket asks to "exercise the Order-based code path" or "add a companion test." Since `CopyRule.Create` requires `Account` (NT8 runtime), a structural test verifying `WouldRecordBeTargetFill` method signature exists is the correct approach per ticket constraints.
**Action:** Add a companion structural test `TryRecordBeTargetFill_SeamExists_WouldRecordBeTargetFill` in `BwaveCycLaneBT2Tests` that verifies the method exists with the expected parameter signature via reflection.

**DW-B37-03 (line ~444):**
`ExecuteBeRetryAndRearm_CallsBreakEven` is Skipped. Ticket requires:
1. Rename it to clarify it only tests the predicate: `IsBeRetryEligible_VerifiesPredicate_NotExecution`
2. Add companion structural test confirming `TryFireFollowerBeRetry` exists with expected `OrderEventArgs` signature via reflection.
**Action:** Rename the skipped test + add `TryFireFollowerBeRetry_Exists_WithOrderEventArgsParam` structural test in `BwaveCycLaneBT5Tests`.

**DW-B37-05 (line ~707):**
`ResolveMultipliers_ReturnsNull_WhenMultipliersNull` is already Skipped. Ticket asks to verify `CopyRule.Create` exists with expected signature.
**Action:** Add `CopyRule_Create_Exists_WithExpectedSignature` structural test in `BwaveCycLaneBT7Tests` using reflection to confirm `CopyRule.Create` is accessible as an internal static method.

---

## Implementation Plan

### Step 1 — D-1
No action required. Acceptance already met.

### Step 2 — D-2
1. Run `csharpier format src/PropTraderTools/CopyEngineTests.cs`
2. Run `csharpier format src/PropTraderTools/Tests/BwaveCycLaneCTests.cs`
3. Verify `csharpier check` passes for both files.

### Step 3 — D-3
Surgical replace in `src/PropTraderTools/Tests/B131Tests.cs`:
```
Assert.Equal(true, (bool)field.GetValue(null)!);
```
→
```
Assert.True((bool)field.GetValue(null)!);
```

### Step 4 — D-4
In `src/PropTraderTools/Tests/BwaveCycLaneBTests.cs`:
- **BwaveCycLaneBT2Tests**: Add structural test for `WouldRecordBeTargetFill` seam.
- **BwaveCycLaneBT5Tests**: Rename `ExecuteBeRetryAndRearm_CallsBreakEven` → `IsBeRetryEligible_VerifiesPredicate_NotExecution`. Add structural test for `TryFireFollowerBeRetry`.
- **BwaveCycLaneBT7Tests**: Add structural test for `CopyRule.Create`.

### Step 5 — Build Verification
Run `dotnet build src/PropTraderTools/PropTraderTools.csproj 2>&1` — must be 0 errors, 0 warnings.

---

## Risk Assessment
- D-2 CSharpier on large CopyEngineTests.cs (7565 lines) — only formatting changes, no logic risk. Low risk.
- D-4 structural tests — all read-only reflection, cannot break runtime behavior. Low risk.
- D-3 single-line replacement — trivially safe.
