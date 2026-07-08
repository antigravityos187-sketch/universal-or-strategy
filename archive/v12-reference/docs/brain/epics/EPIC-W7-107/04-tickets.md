# EPIC-W7-107 — Phase 4 Tickets

**Method**: HydrateFromOpenPositions
**Source**: V12_002.SIMA.Lifecycle.cs
**CYC**: 34
**Lane**: P4-L7

---

## Ticket Summary

| # | Ticket | Type | CYC Target | Lines |
|---|--------|------|-----------|-------|
| 1 | Extract HasExistingFsmForAccount | extraction | ≤2 | 643–648 |
| 2 | Extract TryGetAccountOpenPosition | extraction | ≤3 | 651–655 |
| 3 | Extract TryRecoverStopOrder | extraction | ≤5 | 658–675 |
| 4 | Extract BuildPositionRecoveryFSM | extraction | ≤1 | 696–704 |
| 5 | Extract LinkStopOrderToFsm | extraction | ≤3 | 706–715 |
| 6 | Extract LinkTargetOrdersToFsm | extraction | ≤4 | 717–763 |
| 7 | Refactor HydrateFromOpenPositions orchestrator | refactor | ≤7 | 625–780 |

**Total tickets**: 7 | **CYC reduction**: 34 → 7

---

## Ticket 1 — Extract HasExistingFsmForAccount

**Type**: extraction
**Source File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:643)
**Lines Extracted**: 643–648
**Target CYC**: ≤2
**Depends On**: none

**Description**:
Extract the inline guard predicate that checks whether `_followerBrackets` already contains an FSM
for the given `Account` into a named private helper `HasExistingFsmForAccount`. The logic uses a
LINQ `Any` predicate over `_followerBrackets.Values` comparing `fsm.AccountName == acct.Name`.

**Signature**:
```csharp
private bool HasExistingFsmForAccount(Account acct)
```

**Acceptance Criteria**:
- [ ] Private method `HasExistingFsmForAccount(Account acct)` added to `V12_002.SIMA.Lifecycle.cs`
- [ ] Method returns `true` when `_followerBrackets` contains an FSM with `AccountName == acct.Name`
- [ ] Method returns `false` when no matching FSM exists
- [ ] Inline predicate at lines 643–648 removed from parent body
- [ ] `dotnet build` passes with zero errors
- [ ] No lock() blocks introduced
- [ ] CYC of extracted method ≤2 (verified via complexity_audit.py)

---

## Ticket 2 — Extract TryGetAccountOpenPosition

**Type**: extraction
**Source File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:651)
**Lines Extracted**: 651–655
**Target CYC**: ≤3
**Depends On**: none

**Description**:
Extract the inline position-lookup logic into `TryGetAccountOpenPosition`. This helper iterates
`acct.Positions`, filters for the current `Instrument` and a non-Flat state, sets the `out Position pos`
parameter on first match, and returns `false` (setting `pos = null`) when no open position is found.

**Signature**:
```csharp
private bool TryGetAccountOpenPosition(Account acct, out Position pos)
```

**Acceptance Criteria**:
- [ ] Private method `TryGetAccountOpenPosition(Account acct, out Position pos)` added
- [ ] Returns `true` and sets `pos` to the open position when found
- [ ] Returns `false` and sets `pos = null` (or default) when no open position exists
- [ ] Inline position-lookup at lines 651–655 removed from parent body
- [ ] Parent guard `if (!TryGetAccountOpenPosition(acct, out Position acctPos)) continue;` compiles
- [ ] `dotnet build` passes with zero errors
- [ ] CYC of extracted method ≤3

---

## Ticket 3 — Extract TryRecoverStopOrder

**Type**: extraction
**Source File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:658)
**Lines Extracted**: 658–675
**Target CYC**: ≤5
**Depends On**: none

**Description**:
Extract the stop-order scan loop into `TryRecoverStopOrder`. The helper iterates the
`ConcurrentDictionary<string,Order> stopOrders` key-value pairs looking for the first entry
whose account name matches `acct.Name`. On match it sets both `out string recoveredKey` and
`out Order recoveredStop` and returns `true`. Returns `false` if no match found.
This is the highest-complexity extraction (CYC=5) due to the inner scan loop plus null guards.

**Signature**:
```csharp
private bool TryRecoverStopOrder(
    ConcurrentDictionary<string, Order> stopOrders,
    Account acct,
    out string recoveredKey,
    out Order recoveredStop)
```

**Acceptance Criteria**:
- [ ] Private method `TryRecoverStopOrder` with above signature added
- [ ] Sets `recoveredKey` and `recoveredStop` on first matching entry
- [ ] Returns `false` with `recoveredKey=null` and `recoveredStop=null` when no match
- [ ] Inner stop-order scan loop at lines 658–675 removed from parent body
- [ ] Parent warning branch `Print(string.Format("[SIMA] Phase 5 ..."))` preserved in parent
- [ ] `dotnet build` passes with zero errors
- [ ] No lock() blocks; `ConcurrentDictionary` enumeration pattern preserved
- [ ] CYC of extracted method ≤5

---

## Ticket 4 — Extract BuildPositionRecoveryFSM

**Type**: extraction
**Source File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:696)
**Lines Extracted**: 696–704
**Target CYC**: ≤1
**Depends On**: none

**Description**:
Extract the FSM construction block into `BuildPositionRecoveryFSM`. The helper constructs and
returns a new `FollowerBracketFSM` with `AccountName = acct.Name`, `Key = recoveredKey`,
`State = FsmState.Active`, and `RemainingContracts = Math.Abs(acctPos.Quantity)` from the
recovered position data. This is a pure construction method with no branches (CYC=1).

**Signature**:
```csharp
private FollowerBracketFSM BuildPositionRecoveryFSM(
    Account acct,
    string recoveredKey,
    Position acctPos)
```

**Acceptance Criteria**:
- [ ] Private method `BuildPositionRecoveryFSM` with above signature added
- [ ] Returns a non-null `FollowerBracketFSM` with all required fields populated
- [ ] `State` initialized to `FsmState.Active` (or equivalent active enum value)
- [ ] `RemainingContracts` set to `Math.Abs(acctPos.Quantity)`
- [ ] FSM construction block at lines 696–704 removed from parent body
- [ ] Parent call `var fsm = BuildPositionRecoveryFSM(acct, recoveredKey, acctPos);` compiles
- [ ] `dotnet build` passes with zero errors
- [ ] CYC of extracted method ≤1

---

## Ticket 5 — Extract LinkStopOrderToFsm

**Type**: extraction
**Source File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:706)
**Lines Extracted**: 706–715
**Target CYC**: ≤3
**Depends On**: Ticket 4 (BuildPositionRecoveryFSM must exist first for context)

**Description**:
Extract the stop-order linkage block into `LinkStopOrderToFsm`. The helper attaches
`recoveredStop` to `fsm.StopOrder`, and when `recoveredStop.OrderId` is non-empty it registers
`_orderIdToFsmKey[recoveredStop.OrderId] = recoveredKey` and increments the `ref int ordersIndexed`
counter. The `ref` parameter is required to propagate the index count back to the parent.

**Signature**:
```csharp
private void LinkStopOrderToFsm(
    FollowerBracketFSM fsm,
    Order recoveredStop,
    string recoveredKey,
    ref int ordersIndexed)
```

**Acceptance Criteria**:
- [ ] Private method `LinkStopOrderToFsm` with above signature added
- [ ] `fsm.StopOrder` assigned `recoveredStop`
- [ ] `_orderIdToFsmKey` entry added when `recoveredStop.OrderId` is non-empty
- [ ] `ordersIndexed` incremented via `ref` parameter
- [ ] Stop-order linkage block at lines 706–715 removed from parent body
- [ ] Parent call `LinkStopOrderToFsm(fsm, recoveredStop, recoveredKey, ref ordersIndexed);` compiles
- [ ] `dotnet build` passes with zero errors
- [ ] No lock() blocks
- [ ] CYC of extracted method ≤3

---

## Ticket 6 — Extract LinkTargetOrdersToFsm

**Type**: extraction
**Source File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:717)
**Lines Extracted**: 717–763
**Target CYC**: ≤4
**Depends On**: Ticket 4 (FollowerBracketFSM type reference)

**Description**:
Extract the ×5 copy-pasted target-order linking blocks into a single `LinkTargetOrdersToFsm`
helper that uses an indexed for-loop over a `ConcurrentDictionary<string,Order>[]` array.
This is the highest-impact extraction: eliminates ~46 lines of copy-paste (lines 717–763)
and approximately 11+ CYC points. Each iteration sets `fsm.Targets[i]` and indexes the
non-empty `OrderId` into `_orderIdToFsmKey`. The parent passes `targetOrderSets` (the array
built from target1Orders…target5Orders) to this helper.

**Signature**:
```csharp
private void LinkTargetOrdersToFsm(
    FollowerBracketFSM fsm,
    string recoveredKey,
    ConcurrentDictionary<string, Order>[] targetOrderSets,
    ref int ordersIndexed)
```

**Acceptance Criteria**:
- [ ] Private method `LinkTargetOrdersToFsm` with above signature added
- [ ] Iterates `targetOrderSets` via indexed for-loop (0 to Length-1)
- [ ] Sets `fsm.Targets[i]` for each matched target order
- [ ] Indexes non-empty `OrderId` entries into `_orderIdToFsmKey`
- [ ] `ordersIndexed` incremented per order via `ref` parameter
- [ ] ×5 copy-paste blocks at lines 717–763 removed from parent body
- [ ] Parent call `LinkTargetOrdersToFsm(fsm, recoveredKey, targetOrderSets, ref ordersIndexed);` compiles
- [ ] `dotnet build` passes with zero errors
- [ ] No lock() blocks
- [ ] CYC of extracted method ≤4

---

## Ticket 7 — Refactor HydrateFromOpenPositions to Orchestrator (CYC=7)

**Type**: refactor
**Source File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:625)
**Lines Modified**: 625–780 (parent method body replacement)
**Target CYC**: ≤7
**Depends On**: Tickets 1, 2, 3, 4, 5, 6 (all helpers must exist)

**Description**:
Replace the body of `HydrateFromOpenPositions` with the orchestrator form shown in the architecture
plan. All extracted inline logic is removed and replaced with calls to the 6 helper methods.
The method signature is unchanged. A `targetOrderSets` array is constructed at the top of the loop
to consolidate the 5 target-order dictionaries into a single parameter for `LinkTargetOrdersToFsm`.
After refactor, the parent method retains only: 1 foreach loop, 5 guard-clause continues,
1 ContainsKey check, and 3 helper calls = CYC=7.

**Final Parent Body**:
```csharp
private int HydrateFromOpenPositions(
    ConcurrentDictionary<string, Order> stopOrders,
    ConcurrentDictionary<string, Order> target1Orders,
    ConcurrentDictionary<string, Order> target2Orders,
    ConcurrentDictionary<string, Order> target3Orders,
    ConcurrentDictionary<string, Order> target4Orders,
    ConcurrentDictionary<string, Order> target5Orders,
    ref int ordersIndexed,
    ref int fsmCreated)
{
    int positionFsmCreated = 0;
    var targetOrderSets = new[] { target1Orders, target2Orders, target3Orders, target4Orders, target5Orders };
    foreach (Account acct in Account.All)
    {
        if (!IsFleetAccount(acct)) continue;
        if (HasExistingFsmForAccount(acct)) continue;
        if (!TryGetAccountOpenPosition(acct, out Position acctPos)) continue;
        if (!TryRecoverStopOrder(stopOrders, acct, out string recoveredKey, out Order recoveredStop))
        {
            Print(string.Format("[SIMA] Phase 5 Position Pass: WARNING -- open position on {0} but no stopOrders key found.", acct.Name));
            _positionPassFailedFirstSeen[acct.Name] = DateTime.UtcNow;
            continue;
        }
        if (_followerBrackets.ContainsKey(recoveredKey)) continue;
        var fsm = BuildPositionRecoveryFSM(acct, recoveredKey, acctPos);
        LinkStopOrderToFsm(fsm, recoveredStop, recoveredKey, ref ordersIndexed);
        LinkTargetOrdersToFsm(fsm, recoveredKey, targetOrderSets, ref ordersIndexed);
        _followerBrackets.TryAdd(recoveredKey, fsm);
        positionFsmCreated++;
        fsmCreated++;
        Print(string.Format("[SIMA] Phase 5 Position Pass: Created FSM for {0} (key={1})", acct.Name, recoveredKey));
    }
    return positionFsmCreated;
}
```

**Acceptance Criteria**:
- [ ] Parent method body replaced with orchestrator form above
- [ ] Method signature of `HydrateFromOpenPositions` unchanged (all 8 parameters preserved)
- [ ] `HydrateFSMsFromWorkingOrders` (caller at line 787) NOT modified
- [ ] All 6 helper calls present: `HasExistingFsmForAccount`, `TryGetAccountOpenPosition`, `TryRecoverStopOrder`, `BuildPositionRecoveryFSM`, `LinkStopOrderToFsm`, `LinkTargetOrdersToFsm`
- [ ] `dotnet build` passes with zero errors
- [ ] CYC of `HydrateFromOpenPositions` verified ≤7 via `python scripts/complexity_audit.py`
- [ ] All 6 helper CYCs verified ≤8
- [ ] No lock() blocks in any new or modified code
- [ ] xUnit test: `HydrateFromOpenPositions` creates FSM for valid fleet account with open position and matching stop order

---

## CYC Projection Summary

| Symbol | Before | After |
|--------|--------|-------|
| `HydrateFromOpenPositions` (parent) | 34 | **7** |
| `HasExistingFsmForAccount` | — | **2** |
| `TryGetAccountOpenPosition` | — | **3** |
| `TryRecoverStopOrder` | — | **5** |
| `BuildPositionRecoveryFSM` | — | **1** |
| `LinkStopOrderToFsm` | — | **3** |
| `LinkTargetOrdersToFsm` | — | **4** |
| **Max projected CYC** | | **7** |

Jane Street threshold: ≤8. All symbols comply.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-107 |
| **Generated** | 2026-06-29T01:20:00Z |
| **jcodemunch tools called** | `resolve_repo`, `get_symbol_complexity`, `get_extraction_candidates` |
| **sequential-thinking calls** | 6 (1 probe + 5 planning) |
| **Ticket count** | 7 |
| **CYC before** | 34 |
| **CYC after (max)** | 7 |
| **DNA verdict** | PASS (from Phase 3) |
