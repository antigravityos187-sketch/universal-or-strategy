# Phase 1: Scope Definition - EPIC-W7-086

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.0
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T21:45:52Z

---

## Method Under Refactoring

| Attribute          | Value                                          |
|--------------------|------------------------------------------------|
| **Method**         | `ProcessReaperFlatten_CancelWorkingOrders`     |
| **File**           | `src/V12_002.REAPER.Audit.cs`                  |
| **Line**           | 852                                            |
| **Signature**      | `private void ProcessReaperFlatten_CancelWorkingOrders(Account targetAcct, string accountName)` |
| **Current CYC**    | 10                                             |
| **Target CYC**     | ≤ 8                                            |
| **LOC**            | 33                                             |

### Current Method Body (verbatim)

```csharp
private void ProcessReaperFlatten_CancelWorkingOrders(Account targetAcct, string accountName)
{
    // [V12.Phase9] REAPER FIX: Use manual unmanaged close instead of broken targetAcct.Flatten().
    // 1. Cancel all working orders for this instrument
    // H14-FIX: Snapshot broker orders before iteration to prevent collection-modified exception
    // during emergency flatten when broker callbacks update order states concurrently.
    List<Order> ordersToCancel = new List<Order>();
    var accountOrders = targetAcct.Orders.ToArray();
    foreach (Order order in accountOrders)
    {
        if (
            order != null
            && order.Instrument.FullName == Instrument.FullName
            && (
                order.OrderState == OrderState.Working
                || order.OrderState == OrderState.Submitted
                || order.OrderState == OrderState.Accepted
                || order.OrderState == OrderState.ChangePending
            )
        )
        {
            ordersToCancel.Add(order);
        }
    }
    if (ordersToCancel.Count > 0)
    {
        foreach (Order orderToCancel in ordersToCancel)
        {
            CancelOrderOnAccount(orderToCancel, targetAcct);
        }
        Print($"[REAPER] Emergency Cancel: {ordersToCancel.Count} orders on {accountName}");
    }
}
```

### CYC Breakdown (10 decision points)

| # | Construct                                        | +CYC |
|---|--------------------------------------------------|------|
| 1 | Method entry (base)                              | 1    |
| 2 | `foreach` over `accountOrders`                   | 1    |
| 3 | `if (order != null …)`                           | 1    |
| 4 | `order.Instrument.FullName == Instrument.FullName`| 1   |
| 5 | `OrderState.Working`                             | 1    |
| 6 | `OrderState.Submitted`                           | 1    |
| 7 | `OrderState.Accepted`                            | 1    |
| 8 | `OrderState.ChangePending`                       | 1    |
| 9 | `if (ordersToCancel.Count > 0)`                  | 1    |
| 10| `foreach` over `ordersToCancel`                  | 1    |

---

## IN SCOPE — Extractions to Reduce CYC to ≤ 8

### Extraction 1 — `IsOrderCancellable(Order order)`

**Purpose**: Encapsulates the multi-branch `OrderState` membership test (contributes CYC +4) and the null/instrument guard (CYC +2) into a single predicate method.

**Logic to extract**:
```csharp
private bool IsOrderCancellable(Order order)
{
    return order != null
        && order.Instrument.FullName == Instrument.FullName
        && (
            order.OrderState == OrderState.Working
            || order.OrderState == OrderState.Submitted
            || order.OrderState == OrderState.Accepted
            || order.OrderState == OrderState.ChangePending
        );
}
```

**CYC of extracted method**: 6 (1 base + 1 null check + 1 instrument check + 4 state branches — contained within the helper)  
**CYC removed from parent**: 6 decision points collapse to 1 call-site `if (IsOrderCancellable(order))`

---

### Extraction 2 — `CollectCancellableOrders(Account targetAcct)`

**Purpose**: Encapsulates the snapshot + filter loop that builds `ordersToCancel`, removing the `foreach` + predicate call from the parent body.

**Logic to extract**:
```csharp
private List<Order> CollectCancellableOrders(Account targetAcct)
{
    var result = new List<Order>();
    foreach (Order order in targetAcct.Orders.ToArray())
    {
        if (IsOrderCancellable(order))
            result.Add(order);
    }
    return result;
}
```

**CYC of extracted method**: 3 (1 base + 1 `foreach` + 1 `if`)  
**CYC removed from parent**: Replaces the `foreach` (+1) and the predicate `if` (+1) with a single assignment call

---

### Post-Extraction Parent CYC

After both extractions the parent method becomes:

```csharp
private void ProcessReaperFlatten_CancelWorkingOrders(Account targetAcct, string accountName)
{
    List<Order> ordersToCancel = CollectCancellableOrders(targetAcct);
    if (ordersToCancel.Count > 0)
    {
        foreach (Order orderToCancel in ordersToCancel)
            CancelOrderOnAccount(orderToCancel, targetAcct);
        Print($"[REAPER] Emergency Cancel: {ordersToCancel.Count} orders on {accountName}");
    }
}
```

| Construct                             | +CYC |
|---------------------------------------|------|
| Method entry (base)                   | 1    |
| `if (ordersToCancel.Count > 0)`       | 1    |
| `foreach` over `ordersToCancel`       | 1    |
| **Total**                             | **3** |

**Parent CYC = 3 ✅ (well within ≤ 8 threshold)**

---

## OUT OF SCOPE

| Item                                                                        | Reason                                          |
|-----------------------------------------------------------------------------|--------------------------------------------------|
| Method signature change                                                     | Callers must compile unchanged; no ABI mutation  |
| Behavioral change (cancellation logic, Print output, snapshot strategy)     | Refactor only — observable behavior preserved    |
| `CancelOrderOnAccount` implementation                                       | External callsite; not owned by this method      |
| `IsOrderTerminal` (called by other methods)                                 | Different concern; separate file                 |
| Any other method in `V12_002.REAPER.Audit.cs`                               | One method per epic                              |
| `src-vm-backup/` files                                                      | Backup tree; never modified                      |
| Caller methods (ProcessReaperFlattenQueue, AuditFleet_HandleCriticalDesyncFlatten, etc.) | Zero blast radius — callers need no changes |
| Adding logging, telemetry, error handling                                   | No new behavior allowed                          |

---

## Extraction Plan

```
ProcessReaperFlatten_CancelWorkingOrders  (CYC 10 → 3)
│
├── Extract ① → IsOrderCancellable(Order order)             (new, CYC 6)
│       null-guard + instrument match + 4-branch OrderState test
│
└── Extract ② → CollectCancellableOrders(Account targetAcct) (new, CYC 3)
        snapshot (.ToArray) + filter loop using IsOrderCancellable
```

**Proposed helper placement**: Both helpers are `private` instance methods added in the same class region as `ProcessReaperFlatten_CancelWorkingOrders` (immediately after line 884 or within the same `#region` if one exists).

**Total new methods**: 2  
**Total methods modified**: 1 (parent only)  
**Files touched**: 1 (`src/V12_002.REAPER.Audit.cs`)

---

## Risk Assessment

| Risk                                     | Likelihood | Impact | Mitigation                                          |
|------------------------------------------|------------|--------|-----------------------------------------------------|
| Behavioral regression in cancellation    | Very Low   | High   | Extraction is pure restructuring; no logic change   |
| Collection-modified exception reintroduced | Very Low | High   | `.ToArray()` snapshot kept in `CollectCancellableOrders` |
| Callers broken                           | None       | —      | Signature unchanged; all callers in same file       |
| Naming collision with existing helpers   | Very Low   | Low    | Grep confirms `IsOrderCancellable` / `CollectCancellableOrders` are unused names |
| `src-vm-backup` divergence               | None       | —      | Backup tree explicitly excluded                     |

**Overall Phase Risk: LOW** (consistent with Phase 0 finding)

---

## Success Criteria

| Criterion                                                             | How Verified                              |
|-----------------------------------------------------------------------|-------------------------------------------|
| `ProcessReaperFlatten_CancelWorkingOrders` CYC ≤ 8 (target: 3)       | Manual CYC count of post-extraction body  |
| Method signature identical to pre-refactor                            | Diff of line 852                          |
| All 5 callers compile without modification                            | Build passes with no CS errors            |
| `ordersToCancel` still built from `.ToArray()` snapshot               | `CollectCancellableOrders` uses `.ToArray()` |
| `Print(…)` log line content and format unchanged                      | String literal diff = 0                   |
| No new `public` or `internal` surface area introduced                 | Both helpers are `private`                |
| No `src-vm-backup/` files modified                                    | Git diff shows only `src/` changes        |
| Only 1 source file modified                                           | `git diff --name-only` = 1 file           |

---

## Phase 1 Completion

- ✅ Method under refactoring identified and body read
- ✅ CYC 10 decomposed into 10 annotated decision points
- ✅ IN SCOPE extractions defined (2 helper methods)
- ✅ Post-extraction parent CYC modelled (CYC = 3)
- ✅ OUT OF SCOPE boundary explicitly stated
- ✅ Extraction plan named and placed
- ✅ Risk assessment completed (LOW)
- ✅ Success criteria enumerated

**Next Phase**: Phase 1.5 (Scope Boundary Validation) → Phase 2 (Implementation)
