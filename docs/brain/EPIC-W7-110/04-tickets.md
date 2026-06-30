# EPIC-W7-110 — Phase 4 Tickets

**Method**: `AdoptMasterOrders`
**Source**: `src/V12_002.SIMA.Lifecycle.cs`
**Line Range**: 1195–1254
**CYC**: 22 → projected 5 (parent), max helper CYC: 7
**Lane**: P4-L7
**Wave**: 7
**DNA Verdict**: PASS (Phase 3)
**Extraction Count**: 3 helpers + 1 parent refactor

---

## Ticket Summary

| # | Ticket | Type | Projected CYC | Jane Street |
|---|--------|------|---------------|-------------|
| 1 | Extract `IsValidMasterOrderState` | extraction | ≤7 | PASS |
| 2 | Extract `DeriveMasterOrderKey` | extraction | ≤3 | PASS |
| 3 | Extract `RouteOrderToMasterDict` | extraction | ≤7 | PASS |
| 4 | Refactor parent + xUnit tests | refactor+test | ≤5 | PASS |

---

## Ticket 1 — Extract `IsValidMasterOrderState`

**Type**: extraction
**Target CYC**: ≤7
**Priority**: P0 (must execute before Ticket 4)
**File**: `src/V12_002.SIMA.Lifecycle.cs`

### Description

Extract the 6-clause `&&` `OrderState` guard from `AdoptMasterOrders` into a new private static helper method `IsValidMasterOrderState(Order ord)`.

The guard currently spans lines ~1207–1214 and checks:
- `OrderState.Working`
- `OrderState.Accepted`
- `OrderState.Submitted`
- `OrderState.ChangePending`
- `OrderState.ChangeSubmitted`
- `OrderState.Unknown` ← intentional, NT8 Sim reconnect (Build 994)

The helper intentionally diverges from any fleet `IsValidOrderState` by including `OrderState.Unknown`. This **must be preserved** and documented with the Build 994 comment.

### Target Signature

```csharp
/// <summary>
/// Returns true if the order state is one the SIMA master-account hydration path
/// should adopt. Intentionally includes Unknown for NT8 Sim reconnect (Build 994).
/// </summary>
private static bool IsValidMasterOrderState(Order ord)
{
    return ord.OrderState == OrderState.Working
        || ord.OrderState == OrderState.Accepted
        || ord.OrderState == OrderState.Submitted
        || ord.OrderState == OrderState.ChangePending
        || ord.OrderState == OrderState.ChangeSubmitted
        || ord.OrderState == OrderState.Unknown;
}
```

### Complexity Budget

| Metric | Value |
|--------|-------|
| Internal OR branches | 6 |
| Base path | 1 |
| **Projected CYC** | **7** |

### Acceptance Criteria

- [ ] Method `IsValidMasterOrderState(Order ord)` added as `private static bool` in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Method encapsulates all 6 `OrderState` checks (Working, Accepted, Submitted, ChangePending, ChangeSubmitted, Unknown)
- [ ] Build 994 comment preserved explaining `Unknown` inclusion
- [ ] No `lock()` blocks introduced
- [ ] All string literals remain ASCII-only
- [ ] File compiles with zero errors after change
- [ ] CYC of new helper ≤ 7 (verified via `python scripts/complexity_audit.py`)

---

## Ticket 2 — Extract `DeriveMasterOrderKey`

**Type**: extraction
**Target CYC**: ≤3
**Priority**: P0 (must execute before Ticket 4)
**File**: `src/V12_002.SIMA.Lifecycle.cs`

### Description

Extract the order-name-to-dictionary-key derivation logic from `AdoptMasterOrders` into a new private static helper `DeriveMasterOrderKey(string name)`.

The original logic (line ~1224) is:
```csharp
string key = name.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase)
    ? name.Substring(5)
    : name.Substring(2);
```

**Bug fix included**: The original `Substring(2)` is incorrect for 3-character prefixes `T1_`–`T5_`. The fix adds a second branch: if the name starts with `T` followed by a digit and `_`, use `Substring(3)`. This is a latent off-by-one fix identified during architecture analysis.

### Target Signature

```csharp
/// <summary>
/// Derives the ConcurrentDictionary key from an order name.
/// Stop_ prefix: Substring(5). T1_-T5_ prefixes: Substring(3).
/// All others: Substring(2).
/// </summary>
private static string DeriveMasterOrderKey(string name)
{
    if (name.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase))
        return name.Substring(5);
    if (name.Length >= 3 && name[1] >= '1' && name[1] <= '5' && name[2] == '_')
        return name.Substring(3);
    return name.Substring(2);
}
```

### Complexity Budget

| Metric | Value |
|--------|-------|
| Branch conditions | 2 |
| Base path | 1 |
| **Projected CYC** | **3** |

### Acceptance Criteria

- [ ] Method `DeriveMasterOrderKey(string name)` added as `private static string` in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Handles `Stop_` prefix → `Substring(5)`
- [ ] Handles 3-char `T1_`–`T5_` prefixes → `Substring(3)` (off-by-one fix)
- [ ] Default → `Substring(2)`
- [ ] Pure function — no state access, no side effects, no allocation beyond the `Substring` already present
- [ ] No `lock()` blocks introduced
- [ ] All string literals remain ASCII-only
- [ ] File compiles with zero errors after change
- [ ] CYC of new helper ≤ 3

---

## Ticket 3 — Extract `RouteOrderToMasterDict`

**Type**: extraction
**Target CYC**: ≤7
**Priority**: P0 (must execute before Ticket 4)
**File**: `src/V12_002.SIMA.Lifecycle.cs`

### Description

Extract the 6-arm `switch` routing block from `AdoptMasterOrders` into a new private instance helper `RouteOrderToMasterDict(string classification, string key, Order ord)`.

The switch (lines ~1229–1249) routes to one of six `ConcurrentDictionary` fields:
- `"stop"` → `stopOrders[key] = ord`
- `"target1"` → `target1Orders[key] = ord`
- `"target2"` → `target2Orders[key] = ord`
- `"target3"` → `target3Orders[key] = ord`
- `"target4"` → `target4Orders[key] = ord`
- `"target5"` → `target5Orders[key] = ord`

This must be an **instance method** (not static) because it writes to class-level `ConcurrentDictionary` fields. Operations remain lock-free — single-writer ConcurrentDictionary on the actor/strategy thread.

### Target Signature

```csharp
/// <summary>
/// Routes an adopted master order to the appropriate ConcurrentDictionary.
/// Called on the strategy actor thread — single-writer, lock-free.
/// </summary>
private void RouteOrderToMasterDict(string classification, string key, Order ord)
{
    switch (classification)
    {
        case "stop":    stopOrders[key]    = ord; break;
        case "target1": target1Orders[key] = ord; break;
        case "target2": target2Orders[key] = ord; break;
        case "target3": target3Orders[key] = ord; break;
        case "target4": target4Orders[key] = ord; break;
        case "target5": target5Orders[key] = ord; break;
    }
}
```

### Complexity Budget

| Metric | Value |
|--------|-------|
| switch arms | 6 |
| Base path | 1 |
| **Projected CYC** | **7** |

### Acceptance Criteria

- [ ] Method `RouteOrderToMasterDict(string classification, string key, Order ord)` added as `private void` (instance) in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] All 6 classification cases handled: `stop`, `target1`, `target2`, `target3`, `target4`, `target5`
- [ ] No default case added (matches original no-op behavior for unrecognized classifications)
- [ ] No `lock()` blocks introduced — ConcurrentDictionary writes remain lock-free
- [ ] All string literals remain ASCII-only
- [ ] File compiles with zero errors after change
- [ ] CYC of new helper ≤ 7

---

## Ticket 4 — Refactor `AdoptMasterOrders` Parent + xUnit Tests

**Type**: refactor+test
**Target CYC**: ≤5 (parent method)
**Priority**: P1 (depends on Tickets 1, 2, 3)
**Files**:
- `src/V12_002.SIMA.Lifecycle.cs` (modify parent)
- `tests/V12_Performance.Tests/Core/AdoptMasterOrdersTests.cs` (create)

### Description

After Tickets 1–3 add the three helper methods, refactor `AdoptMasterOrders` to delegate to all three helpers. The parent becomes a pure orchestration method.

Then add xUnit `[Fact]` tests for all three extracted helpers.

### Target Parent Body

```csharp
private int AdoptMasterOrders()
{
    int adoptedCount = 0;
    foreach (Order ord in Account.Orders.ToArray())
    {
        if (ord.Instrument?.FullName != Instrument?.FullName)
            continue;
        if (!IsValidMasterOrderState(ord))
            continue;
        string name = ord.Name ?? string.Empty;
        string classification = ClassifyOrderByPrefix(name);
        if (classification == null || classification == "entry")
            continue;
        string key = DeriveMasterOrderKey(name);
        RouteOrderToMasterDict(classification, key, ord);
        adoptedCount++;
    }
    return adoptedCount;
}
```

### Required xUnit Tests

| Test | Helper | Assertion |
|------|--------|-----------|
| `IsValidMasterOrderState_Working_ReturnsTrue` | `IsValidMasterOrderState` | OrderState.Working → true |
| `IsValidMasterOrderState_Unknown_ReturnsTrue` | `IsValidMasterOrderState` | OrderState.Unknown → true (NT8 Sim) |
| `IsValidMasterOrderState_Filled_ReturnsFalse` | `IsValidMasterOrderState` | OrderState.Filled → false |
| `DeriveMasterOrderKey_StopPrefix_Returns_Substring5` | `DeriveMasterOrderKey` | "Stop_abc" → "abc" |
| `DeriveMasterOrderKey_T1Prefix_Returns_Substring3` | `DeriveMasterOrderKey` | "T1_abc" → "abc" (off-by-one fix) |
| `DeriveMasterOrderKey_DefaultPrefix_Returns_Substring2` | `DeriveMasterOrderKey` | "SLabc" → "abc" |

### Complexity Budget (parent after extraction)

| Branch | CYC contribution |
|--------|-----------------|
| Base path | 1 |
| foreach loop | +1 |
| Instrument null-guard | +1 |
| IsValidMasterOrderState call (1 branch) | +1 |
| classification null/entry guard | +1 |
| **Parent CYC total** | **5** |

### Acceptance Criteria

- [ ] `AdoptMasterOrders` body replaced with orchestration skeleton calling all 3 helpers
- [ ] Parent CYC = 5 (verified via `python scripts/complexity_audit.py`)
- [ ] Public signature `private int AdoptMasterOrders()` unchanged
- [ ] Return value behavior identical to original (adoptedCount incremented per adopted order)
- [ ] xUnit test file `AdoptMasterOrdersTests.cs` created with all 6 `[Fact]` tests listed above
- [ ] No NUnit or MSTest attributes used — xUnit only
- [ ] `dotnet build` passes with zero errors
- [ ] `dotnet test` passes with zero failures
- [ ] No `lock()` blocks introduced
- [ ] `python scripts/complexity_audit.py` shows all 4 symbols (parent + 3 helpers) ≤ 8
- [ ] `dotnet csharpier check src/` passes (no formatting issues)

---

## Execution Order

```
Ticket 1 (IsValidMasterOrderState)
Ticket 2 (DeriveMasterOrderKey)       ← can run parallel with Ticket 1
Ticket 3 (RouteOrderToMasterDict)     ← can run parallel with Tickets 1 & 2
        ↓  (all three complete)
Ticket 4 (parent refactor + tests)
```

Tickets 1, 2, 3 are **independent** and can be executed in parallel. Ticket 4 is a **hard dependency** on all three.

---

## CYC Reduction Summary

| Symbol | Before | After |
|--------|--------|-------|
| `AdoptMasterOrders` (parent) | 22 | **5** |
| `IsValidMasterOrderState` (new) | — | **7** |
| `DeriveMasterOrderKey` (new) | — | **3** |
| `RouteOrderToMasterDict` (new) | — | **7** |
| **Max CYC across all symbols** | 22 | **7** ✅ |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-110 |
| **Method** | AdoptMasterOrders |
| **Source** | src/V12_002.SIMA.Lifecycle.cs |
| **Original CYC** | 22 |
| **Ticket Count** | 4 |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity |
| **sequential-thinking calls** | 3 (probe + 3 analysis thoughts) |
| **DNA Verdict (Phase 3)** | PASS |
| **Generated** | 2026-06-29 |
| **Output** | docs/brain/EPIC-W7-110/04-tickets.md |
