# Phase 1: Scope Definition - EPIC-W7-151

## Agent Tracking
- Agent Name: v12-phase1-scope
- Execution Time: 2026-06-24T02:00:00Z

---

## Method Under Refactoring

| Attribute         | Value                               |
|-------------------|-------------------------------------|
| Method            | `TrackTradeEntry`                   |
| File              | `src/V12_002.UI.Compliance.cs`      |
| Line              | 67                                  |
| Current CYC       | 9                                   |
| Target CYC        | ≤ 8                                 |
| Parameters        | `Account acct`, `Execution execution` |
| Return type       | `void`                              |

### Current Method Body (lines 67–90)

```csharp
private void TrackTradeEntry(Account acct, Execution execution)
{
    if (acct == null || execution == null || execution.Order == null)
        return;
    if (execution.Order.OrderState != OrderState.Filled)
        return;

    OrderAction action = execution.Order.OrderAction;
    if (action != OrderAction.Buy && action != OrderAction.SellShort)
        return;

    if (EnableSIMA && !IsFleetAccount(acct))
        return;

    DateTime nowInZone = GetComplianceNow();
    EnsureAccountComplianceTracking(acct.Name, nowInZone);

    accountTradeCount.AddOrUpdate(acct.Name, 1, (k, v) => v + 1);
    accountDailyTradeCount.AddOrUpdate(acct.Name, 1, (k, v) => v + 1);

    int dayKey = GetTradingDayKey(nowInZone);
    var days = accountTradingDays.GetOrAdd(acct.Name, _ => new ConcurrentDictionary<int, byte>());
    days.TryAdd(dayKey, 1);
}
```

### CYC Breakdown (why CYC = 9)

| Decision point                                               | +CYC |
|--------------------------------------------------------------|------|
| Base path                                                    | 1    |
| `acct == null`                                               | +1   |
| `execution == null`                                          | +1   |
| `execution.Order == null`                                    | +1   |
| `execution.Order.OrderState != OrderState.Filled`            | +1   |
| `action != OrderAction.Buy`                                  | +1   |
| `action != OrderAction.SellShort` (compound `&&`)            | +1   |
| `EnableSIMA`                                                 | +1   |
| `!IsFleetAccount(acct)`                                      | +1   |
| **Total**                                                    | **9** |

---

## IN SCOPE — Extractions to Bring CYC to ≤ 8

### Proposed Helper: `IsTrackableExecution`

**Responsibility:** Consolidate the four guard clauses (null checks + filled-state + entry-action check) that together account for 6 of the 8 decision points into a single boolean predicate.

```csharp
// Proposed signature
private bool IsTrackableExecution(Account acct, Execution execution)
```

**Logic absorbed:**
- `acct == null || execution == null || execution.Order == null` → return false
- `execution.Order.OrderState != OrderState.Filled` → return false
- `action != OrderAction.Buy && action != OrderAction.SellShort` → return false

**CYC contribution inside helper:** 6 (isolated away from `TrackTradeEntry`)

**Resulting `TrackTradeEntry` CYC after extraction:**

| Decision point              | +CYC |
|-----------------------------|------|
| Base path                   | 1    |
| `!IsTrackableExecution(...)` | +1   |
| `EnableSIMA`                | +1   |
| `!IsFleetAccount(acct)`     | +1   |
| **Total**                   | **4** |

> CYC 4 is well within the ≤ 8 target.  
> Only **one** helper extraction is required.

---

## OUT OF SCOPE

| Item                                                         | Reason                                        |
|--------------------------------------------------------------|-----------------------------------------------|
| Public/internal signature of `TrackTradeEntry`              | Must remain `private void TrackTradeEntry(Account, Execution)` — no callers updated |
| Observable behavior of `TrackTradeEntry`                    | Identical input/output behavior required; refactor is structural only |
| Callers: `ProcessQueuedExecution`, `ProcessAccountExecutionQueue`, `OnAccountExecutionUpdate` | Zero call-site changes; callers are not touched |
| `UpdateEquityDrawdown`, `UpdateAccountMetricsFromAccount`, and all other methods in the file | Untouched; single-method scope |
| Data structures (`accountTradeCount`, `accountDailyTradeCount`, `accountTradingDays`, etc.) | No changes to field declarations or types |
| `IsFleetAccount`, `GetComplianceNow`, `EnsureAccountComplianceTracking`, `GetTradingDayKey` | Existing callees remain unchanged |
| Unit tests / test projects                                   | Phase 1 scope only; test additions are Phase 3 |
| Build, CI configuration, NuGet references                   | Not touched at any phase                      |

---

## Extraction Plan

### Step 1 — Create `IsTrackableExecution` (new private method)

Insert immediately before `TrackTradeEntry` (line 67) or immediately after it (line 91) — either placement is acceptable. The method is `private`, file-local, no new dependencies introduced.

```csharp
private bool IsTrackableExecution(Account acct, Execution execution)
{
    if (acct == null || execution == null || execution.Order == null)
        return false;
    if (execution.Order.OrderState != OrderState.Filled)
        return false;
    OrderAction action = execution.Order.OrderAction;
    return action == OrderAction.Buy || action == OrderAction.SellShort;
}
```

### Step 2 — Rewrite `TrackTradeEntry` body

Replace the four guard clauses with a single delegation to the new helper:

```csharp
private void TrackTradeEntry(Account acct, Execution execution)
{
    if (!IsTrackableExecution(acct, execution))
        return;

    if (EnableSIMA && !IsFleetAccount(acct))
        return;

    DateTime nowInZone = GetComplianceNow();
    EnsureAccountComplianceTracking(acct.Name, nowInZone);

    accountTradeCount.AddOrUpdate(acct.Name, 1, (k, v) => v + 1);
    accountDailyTradeCount.AddOrUpdate(acct.Name, 1, (k, v) => v + 1);

    int dayKey = GetTradingDayKey(nowInZone);
    var days = accountTradingDays.GetOrAdd(acct.Name, _ => new ConcurrentDictionary<int, byte>());
    days.TryAdd(dayKey, 1);
}
```

**Net diff:** −7 lines in `TrackTradeEntry`, +9 lines (new helper). No other lines changed.

---

## Risk Assessment

| Risk                              | Likelihood | Severity | Mitigation                                              |
|-----------------------------------|------------|----------|---------------------------------------------------------|
| Behavioral regression in guard logic | Low      | High     | Helper is a pure boolean extraction; logic is identical |
| Null-dereference order change     | Very Low   | Medium   | Short-circuit evaluation preserved (`&&` / `\|\|` order kept) |
| Merge conflict with concurrent edits | Very Low | Low     | Zero external blast radius; single-file, single-method scope |
| Missed edge case in OrderAction check | Very Low | Medium  | `Buy \|\| SellShort` is logically equivalent to `!= Buy && != SellShort` negated |

**Overall Risk: LOW** — consistent with Phase 0 assessment.

---

## Success Criteria

| Criterion                                                      | Verification method              |
|----------------------------------------------------------------|----------------------------------|
| `TrackTradeEntry` CYC ≤ 8 after refactor                      | Static analysis / manual count   |
| `IsTrackableExecution` exists as a new `private bool` method  | Code review                      |
| Method signature of `TrackTradeEntry` unchanged               | Diff check: `private void TrackTradeEntry(Account, Execution)` |
| No changes to any caller (`ProcessQueuedExecution`, etc.)     | Diff check: callers untouched    |
| No changes outside `src/V12_002.UI.Compliance.cs`             | `git diff --name-only`           |
| All existing tests pass (no new failures)                     | CI green (Phase 2 gate)          |
