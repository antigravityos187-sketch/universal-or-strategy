# Phase 1: Scope Definition — EPIC-W7-148

## Agent Tracking
- Agent Name: v12-phase1-scope
- Phase: 1 (Scope Definition)
- Source Epic: EPIC-W7-148
- Input: 00-hotspots.md, manifest.json

---

## 1. Method Under Refactoring

| Attribute         | Value                                          |
|-------------------|------------------------------------------------|
| Method            | `ProcessQueuedExecution_SyncFlatPosition`      |
| File              | `src/V12_002.UI.Compliance.cs`                 |
| Line              | 729                                            |
| Signature         | `private void ProcessQueuedExecution_SyncFlatPosition(QueuedAccountExecution item)` |
| Current CYC       | 16                                             |
| Target CYC        | ≤ 8 (Jane Street strict standard)             |
| Lines of Code     | 52 (lines 729–780)                             |
| Max Nesting Depth | 7                                              |

### Method Structure (as-read from source)

```
ProcessQueuedExecution_SyncFlatPosition(item)
└── try
    ├── Guard: fleetAcct != null
    │        && expectedPositions != null
    │        && expectedPositions.ContainsKey(ExpKey(fleetAcct.Name))   [3 conditions]
    │   ├── Resolve execOrder from item.EventArgs?.Execution?.Order
    │   ├── Evaluate isEntryFill                                        [2 conditions OR]
    │   ├── if (isEntryFill)
    │   │   └── Print entry-fill skip message
    │   └── else
    │       ├── Resolve brokerPos via LINQ FirstOrDefault               [1 condition]
    │       ├── Evaluate nowFlat                                        [2 conditions OR]
    │       └── if (nowFlat && !IsDispatchSyncPending(...))             [2 conditions AND]
    │           ├── SetExpectedPositionLocked(...)
    │           └── Print flat-cleared message
    └── catch (Exception ex)
        ├── Interlocked.Increment(ref _uiCallbackFailures)
        └── Print error message
```

The CYC of 16 arises from the compound boolean guard (3 terms), the `OrderAction` OR test (2 terms), the `nowFlat` OR test (2 terms), the `nowFlat && !IsDispatchSyncPending` AND test (2 terms), plus the structural branches (try/catch, if/else). Each boolean connective (`&&`, `||`) counts as +1 per McCabe.

---

## 2. IN SCOPE — Extractions

The following **three** private helper methods will be extracted to reduce CYC to ≤ 8.

### Helper 1 — `IsSyncFlatPositionApplicable`

**Signature:**
```csharp
private bool IsSyncFlatPositionApplicable(Account fleetAcct)
```

**Extracted logic:** The three-part guard at lines 734–738:
```csharp
fleetAcct != null
&& expectedPositions != null
&& expectedPositions.ContainsKey(ExpKey(fleetAcct.Name))
```

**CYC contribution removed from parent:** 3 (two `&&` connectives + one method-level branch become encapsulated inside the helper, contributing only 1 decision point to the caller).

---

### Helper 2 — `IsEntryFillOrder`

**Signature:**
```csharp
private bool IsEntryFillOrder(Order execOrder)
```

**Extracted logic:** The null-guard + OrderAction OR test at lines 741–743:
```csharp
execOrder != null
&& (execOrder.OrderAction == OrderAction.Buy || execOrder.OrderAction == OrderAction.SellShort)
```

**CYC contribution removed from parent:** 3 (null check + two `||`/`&&` connectives collapse to a single boolean call-site expression in the caller).

---

### Helper 3 — `TrySyncFlatPositionIfNowFlat`

**Signature:**
```csharp
private void TrySyncFlatPositionIfNowFlat(Account fleetAcct)
```

**Extracted logic:** The exit-fill branch body (lines 755–769): resolving `brokerPos` via LINQ, evaluating `nowFlat`, checking `IsDispatchSyncPending`, calling `SetExpectedPositionLocked`, and printing the flat-cleared message.

**CYC contribution removed from parent:** 4 (LINQ predicate condition, `nowFlat` OR composite, `&&` with `!IsDispatchSyncPending`, and the `if` branch).

---

### Net CYC Reduction

| Source                          | CYC before | CYC after extraction |
|---------------------------------|-----------|----------------------|
| Parent method (caller)          | 16        | ≤ 8                  |
| `IsSyncFlatPositionApplicable`  | —         | ~4 (self-contained)  |
| `IsEntryFillOrder`              | —         | ~3 (self-contained)  |
| `TrySyncFlatPositionIfNowFlat`  | —         | ~5 (self-contained)  |

Each helper stays below the ≤ 8 threshold independently. The parent method's remaining branches (try/catch, the if/else on `IsEntryFill`, and calls to the three helpers) yield CYC ≈ 6–7.

---

## 3. OUT OF SCOPE

The following are **explicitly excluded** from this refactoring:

| Item                                                         | Reason                                           |
|--------------------------------------------------------------|--------------------------------------------------|
| Public/private **signature** of `ProcessQueuedExecution_SyncFlatPosition` | Must remain unchanged — called at line 800     |
| **Behavior change** of any kind                              | Pure structural extraction, zero semantic delta  |
| Caller method `ProcessQueuedExecution` (line 787+)           | Not touched; only the callee is restructured     |
| `ProcessAccountExecutionQueue` (line 427)                    | Upstream caller — untouched                      |
| `OnAccountExecutionUpdate` (line 401)                        | Upstream caller — untouched                      |
| `SetExpectedPositionLocked`, `IsDispatchSyncPending`, `StampAccountFillGrace`, `ExpKey` | Callees — signatures/bodies unchanged |
| `expectedPositions` field                                    | Read-only usage pattern unchanged                |
| `_uiCallbackFailures` counter and catch block                | Error-handling path unchanged                    |
| Any other method in `V12_002.UI.Compliance.cs`               | File is otherwise untouched                      |
| Build, test, or CI configuration                             | No build artifacts modified                      |

---

## 4. Extraction Plan

```
Step 1  Extract IsSyncFlatPositionApplicable(Account fleetAcct)
        — Move 3-part guard expression into new private bool helper.
        — Replace guard in parent with: if (!IsSyncFlatPositionApplicable(fleetAcct.Name)) return;
          (early-return pattern to eliminate nesting level).

Step 2  Extract IsEntryFillOrder(Order execOrder)
        — Move null + OrderAction check into new private bool helper.
        — Replace inline expression in parent with: bool isEntryFill = IsEntryFillOrder(execOrder);

Step 3  Extract TrySyncFlatPositionIfNowFlat(Account fleetAcct)
        — Move the entire else-branch body into new private void helper.
        — Replace else-branch in parent with: TrySyncFlatPositionIfNowFlat(fleetAcct);

Step 4  Verify CYC of parent ≤ 8 and each helper ≤ 8 using complexity tooling.
```

All three helpers are declared `private` in the same class, same file. No new files created.

---

## 5. Risk Assessment

| Risk                                   | Severity | Mitigation                                                  |
|----------------------------------------|----------|-------------------------------------------------------------|
| Logic error during copy-paste of guard | LOW      | Exact copy with no rewrites; boolean operators preserved    |
| `fleetAcct.Name` null-ref inside helper| LOW      | Guard itself checks `fleetAcct != null` first               |
| `ExpKey()` called multiple times       | LOW      | Already called multiple times in original; no state change  |
| Thread-safety of extracted helpers     | LOW      | Helpers only read fields; mutation stays in existing callees|
| Callers broken by signature change     | NONE     | Public signature of parent is unchanged                     |
| Blast radius to other files            | NONE     | Phase 0 confirmed 0 direct dependents outside file          |

**Overall Refactoring Risk: LOW**

---

## 6. Success Criteria

- [ ] `ProcessQueuedExecution_SyncFlatPosition` CYC ≤ 8 after extraction
- [ ] Each extracted helper (`IsSyncFlatPositionApplicable`, `IsEntryFillOrder`, `TrySyncFlatPositionIfNowFlat`) has CYC ≤ 8 independently
- [ ] Method signature at line 729 is byte-for-byte identical to pre-refactor
- [ ] Call site at line 800 (`ProcessQueuedExecution_SyncFlatPosition(item)`) is unchanged
- [ ] All three helpers are `private` and defined within the same class in `src/V12_002.UI.Compliance.cs`
- [ ] No behavior change: same execution paths, same log messages, same side-effects
- [ ] Zero changes to any file outside `src/V12_002.UI.Compliance.cs`
- [ ] Nesting depth of parent method reduced from 7 to ≤ 4
