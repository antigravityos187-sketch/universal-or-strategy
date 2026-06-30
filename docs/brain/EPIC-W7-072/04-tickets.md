# Phase 4: Ticket Generation — EPIC-W7-072

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T04:30:00Z
**Input:** docs/brain/EPIC-W7-072/02-architecture-plan.md + docs/brain/EPIC-W7-072/03-audit-report.md

---

## Epic Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-072 |
| **Method** | `ProcessAccountOrder_UpdateMasterExpected` |
| **Source File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **Lines** | 81–115 |
| **Original CYC** | 12 |
| **Target CYC (parent)** | 6 |
| **Extraction Count** | 2 helpers |
| **DNA Verdict** | PASS (Phase 3) |
| **Ticket Count** | 5 |

---

## Ticket Overview

| Ticket | Title | CYC Impact | Blocking? |
|---|---|---|---|
| T1 | TDD Baseline — Write xUnit tests for original method | None (safety net) | Yes — must precede extraction |
| T2 | Extract `HandleMasterStopFill()` | Parent CYC contribution reduced | Yes — T1 must pass |
| T3 | Extract `HandleMasterTargetFill(Order order)` | Parent CYC contribution reduced | Yes — T2 must pass |
| T4 | Verify Parent CYC = 6 post-extraction | Confirms cyc target met | Yes — T3 must pass |
| T5 | Build + CI Gate: complexity audit + deploy-sync | Final validation | Yes — T4 must pass |

---

## T1 — TDD Baseline: Write xUnit Tests for Original Method

### Description

Before any extraction begins, write comprehensive xUnit [Fact] tests that exercise all 12 decision paths in the original `ProcessAccountOrder_UpdateMasterExpected` method. These tests serve as the regression safety net that must pass before and after every extraction ticket.

The method has CYC=12, which maps to these observable branches:
- `order.OrderState` is neither `Filled` nor `PartFilled` → no-op (outer guard)
- `order.OrderState == Filled` AND `order.Name.StartsWith("Stop_")` → `TryRemove` + `Enqueue(SetExpectedPositionLocked(mExpKey, 0))`
- `order.OrderState == PartFilled` AND `order.Name.StartsWith("Stop_")` → same stop-fill path
- `order.OrderState == Filled` AND name starts with `"T"` and contains `"_"` AND `expectedPositions == null` → lambda no-op
- `order.OrderState == Filled` AND name matches target AND `TryGetValue` fails → lambda no-op
- `order.OrderState == Filled` AND name matches target AND `currentExp > 0` → `Math.Max` path
- `order.OrderState == Filled` AND name matches target AND `currentExp < 0` → `Math.Min` path
- `order.OrderState == Filled` AND name matches target AND `currentExp == 0` → `SetExpectedPositionLocked(mExpKey, 0)` with no arithmetic

### Acceptance Criteria

- [ ] Test file created at `tests/V12_Performance.Tests/Core/AccountOrders/ProcessAccountOrder_UpdateMasterExpectedTests.cs`
- [ ] Uses xUnit `[Fact]` attributes — NEVER NUnit `[Test]` or MSTest `[TestMethod]`
- [ ] Uses `Assert.Equal`, `Assert.True`, `Assert.False` — no FluentAssertions or NUnit assertions
- [ ] All 12 cyc paths have at least one dedicated test
- [ ] All tests green before extraction starts
- [ ] No compilation errors after adding test file

### Estimated CYC Reduction

None — this ticket establishes the safety net only.

---

## T2 — Extract `HandleMasterStopFill()`

### Description

Surgically extract the stop-fill branch body from `ProcessAccountOrder_UpdateMasterExpected` into a new private helper `HandleMasterStopFill()` in `src/V12_002.Orders.Callbacks.AccountOrders.cs`.

**Current inline code** (lines ~90–93 within the method):
```csharp
_nakedPositionFirstSeen.TryRemove(Account.Name, out _);
var mExpKey = ExpKey(Account.Name);
Enqueue(ctx => ctx.SetExpectedPositionLocked(mExpKey, 0));
```

**Extracted helper** (exact signature from architecture plan):
```csharp
private void HandleMasterStopFill()
{
    _nakedPositionFirstSeen.TryRemove(Account.Name, out _);
    var mExpKey = ExpKey(Account.Name);
    Enqueue(ctx => ctx.SetExpectedPositionLocked(mExpKey, 0));
}
```

**Parent call site** replaces the inline block with:
```csharp
HandleMasterStopFill();
```

**Threading contract**: `_nakedPositionFirstSeen.TryRemove` executes on the broker thread (ConcurrentDictionary — thread-safe). `Enqueue` marshals the `SetExpectedPositionLocked` mutation to the strategy thread. This contract is unchanged by extraction.

### Acceptance Criteria

- [ ] New method `HandleMasterStopFill()` added to `src/V12_002.Orders.Callbacks.AccountOrders.cs`
- [ ] Method is `private void` with no parameters
- [ ] Parent `ProcessAccountOrder_UpdateMasterExpected` calls `HandleMasterStopFill()` in place of the inline block
- [ ] `HandleMasterStopFill` CYC = 1 (confirmed by complexity audit)
- [ ] All T1 tests still pass after extraction
- [ ] `dotnet csharpier check src/` passes (zero formatting issues)
- [ ] No `lock(` introduced (Actor/Enqueue model preserved)

### Estimated CYC Reduction

Stop-fill branch responsibility moved to helper. Helper CYC = 1. Parent cyc delta = stop-fill logic removed from parent scope.

---

## T3 — Extract `HandleMasterTargetFill(Order order)`

### Description

Surgically extract the target-fill branch body from `ProcessAccountOrder_UpdateMasterExpected` into a new private helper `HandleMasterTargetFill(Order order)` in `src/V12_002.Orders.Callbacks.AccountOrders.cs`.

**Current inline code** (lines ~95–113 within the method):
```csharp
int filledQty = order.Filled;
var mExpKey = ExpKey(Account.Name);
Enqueue(ctx =>
{
    if (
        ctx.expectedPositions != null
        && ctx.expectedPositions.TryGetValue(mExpKey, out int currentExp)
    )
    {
        int newExp = 0;
        if (currentExp > 0)
            newExp = Math.Max(0, currentExp - filledQty);
        else if (currentExp < 0)
            newExp = Math.Min(0, currentExp + filledQty);

        ctx.SetExpectedPositionLocked(mExpKey, newExp);
    }
});
```

**Extracted helper** (exact signature from architecture plan):
```csharp
private void HandleMasterTargetFill(Order order)
{
    int filledQty = order.Filled;
    var mExpKey = ExpKey(Account.Name);
    Enqueue(ctx =>
    {
        if (
            ctx.expectedPositions != null
            && ctx.expectedPositions.TryGetValue(mExpKey, out int currentExp)
        )
        {
            int newExp = 0;
            if (currentExp > 0)
                newExp = Math.Max(0, currentExp - filledQty);
            else if (currentExp < 0)
                newExp = Math.Min(0, currentExp + filledQty);

            ctx.SetExpectedPositionLocked(mExpKey, newExp);
        }
    });
}
```

**Parent call site** replaces the inline block with:
```csharp
HandleMasterTargetFill(order);
```

**Lambda capture preserved**: `filledQty` and `mExpKey` are captured by value (int and struct-like key) on the broker thread; `ctx` is the strategy-thread actor context passed by Enqueue. This pattern is identical to the original — no new allocations introduced.

### Acceptance Criteria

- [ ] New method `HandleMasterTargetFill(Order order)` added to `src/V12_002.Orders.Callbacks.AccountOrders.cs`
- [ ] Method is `private void` with one `Order order` parameter
- [ ] Parent calls `HandleMasterTargetFill(order)` in place of the inline block
- [ ] `HandleMasterTargetFill` CYC = 5 (confirmed by complexity audit)
- [ ] Lambda capture pattern is bit-for-bit identical to original (no behavioral change)
- [ ] All T1 tests still pass after extraction
- [ ] `dotnet csharpier check src/` passes (zero formatting issues)
- [ ] No `lock(` introduced

### Estimated CYC Reduction

Direction-aware delta arithmetic moved to helper. Helper CYC = 5. Parent net cyc after both extractions = 6 (down from 12).

---

## T4 — Verify Parent CYC = 6 Post-Extraction

### Description

After T2 and T3 are complete, verify that the parent method `ProcessAccountOrder_UpdateMasterExpected` exactly matches the architecture plan body and achieves CYC = 6.

**Expected parent body** (from 02-architecture-plan.md):
```csharp
private void ProcessAccountOrder_UpdateMasterExpected(Order order)
{
    if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.PartFilled)
    {
        if (order.Name.StartsWith("Stop_"))
            HandleMasterStopFill();
        else if (order.Name.StartsWith("T") && order.Name.Contains("_"))
            HandleMasterTargetFill(order);
    }
}
```

**CYC breakdown** (6 decision points):
| Decision Point | Delta |
|---|---|
| Base path | +1 |
| `\|\|` in outer fill-state guard | +1 |
| `if (Filled\|\|PartFilled)` | +1 |
| `if (StartsWith("Stop_"))` | +1 |
| `else if (StartsWith("T"))` | +1 |
| `&&` compound in `else if` | +1 |
| **Total** | **6** |

### Acceptance Criteria

- [ ] `python scripts/complexity_audit.py` reports `ProcessAccountOrder_UpdateMasterExpected` CYC = 6
- [ ] `python scripts/complexity_audit.py` reports `HandleMasterStopFill` CYC = 1
- [ ] `python scripts/complexity_audit.py` reports `HandleMasterTargetFill` CYC = 5
- [ ] max_cyc across all 3 methods = 6 (below Jane Street threshold of 8)
- [ ] Parent body matches architecture plan exactly — no residual inline logic
- [ ] All T1 tests still pass

### Estimated CYC Reduction

Confirms 50% cyc reduction: original CYC 12 → parent CYC 6. Two helpers: CYC 1 and CYC 5. All three methods satisfy CYC ≤ 8 threshold.

---

## T5 — Build + CI Gate: Complexity Audit + Deploy-Sync

### Description

Final validation gate. Run all required checks to confirm the extraction is production-ready.

### Acceptance Criteria

- [ ] `powershell -File .\scripts\build_readiness.ps1` exits with code 0 (zero compilation errors)
- [ ] `python scripts/complexity_audit.py` reports zero methods in scope above CYC = 8
- [ ] `dotnet csharpier check src/` exits with code 0 (zero formatting issues)
- [ ] `powershell -File .\deploy-sync.ps1` completes successfully (NinjaTrader hard-link sync)
- [ ] `grep -r "lock(" src/V12_002.Orders.Callbacks.AccountOrders.cs` returns zero matches
- [ ] All T1 xUnit tests pass via `dotnet test`

### Estimated CYC Reduction

No additional cyc reduction — this ticket confirms and locks the CYC = 6/5/1 targets achieved in T2–T4.

---

## Sequential Thinking Summary

4 thoughts executed:
1. Identified method's structural concerns (fill-state guard, stop-fill branch, target-fill branch)
2. Mapped architecture plan's 2 helpers to 5-ticket workflow (TDD → extract → extract → verify → CI)
3. Confirmed ticket sequencing and acceptance criteria from architecture plan + audit report
4. Verified manifest structure and final ticket count = 5

---

## CYC Reduction Summary

| Method | Before | After |
|---|---|---|
| `ProcessAccountOrder_UpdateMasterExpected` | 12 | 6 |
| `HandleMasterStopFill` (new) | — | 1 |
| `HandleMasterTargetFill` (new) | — | 5 |
| **max_cyc** | **12** | **6** |
| **Jane Street threshold** | — | **8** |

**Total cyc reduction: 50%** — All methods at CYC ≤ 8. Headroom = 2 units below threshold.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T04:30:00Z |
| **jCodemunch tools called** | resolve_repo |
| **sequential-thinking calls** | 4 |
| **Wave** | 7 |
| **Phase** | 4 |
| **Method** | ProcessAccountOrder_UpdateMasterExpected |
| **Output** | docs/brain/EPIC-W7-072/04-tickets.md |
| **ticket_count** | 5 |
| **dna_verdict_input** | PASS |
| **max_cyc_projected** | 6 |
