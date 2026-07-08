# EPIC-W7-156 — Phase 0: Hotspot Analysis

## Target Method

| Field       | Value                                              |
|-------------|----------------------------------------------------|
| Method      | `CancelAll_ProcessSingleFleetAccount`              |
| CYC Score   | **18**                                             |
| File        | `src/V12_002.UI.IPC.Commands.Fleet.cs`             |
| Lines       | 300 – 343                                          |
| Signature   | `private int CancelAll_ProcessSingleFleetAccount(Account acct, bool masterHasPosition)` |

---

## Blast Radius Summary

**Direct callers (1):**
- [`CancelAll_ProcessFleetOrders()`](src/V12_002.UI.IPC.Commands.Fleet.cs:275) — iterates `Account.All`, gates on `IsFleetAccount`, and accumulates the returned cancel count.

**Call chain from IPC surface:**
```
TryHandleFleetCommand()
  └─ TryHandleFleet_CancelAll()
       └─ CancelAll_ProcessFleetAccounts()
            └─ CancelAll_ProcessFleetOrders()
                 └─ CancelAll_ProcessSingleFleetAccount()  ← TARGET
```

**Shared state touched:**
| Symbol | File | Risk |
|--------|------|------|
| `_followerBrackets` (`ConcurrentDictionary`) | `src/V12_002.cs:829` | Read-only here; mutated by FSM lifecycle files — race window if cancel races a fill |
| `FollowerBracketState.Active` | `src/V12_002.Symmetry.BracketFSM.cs:21` | State comparison gate for bracket preservation |
| `CancelOrderOnAccount()` | `src/V12_002.Orders.CancelGateway.cs` | Side-effecting broker call; reaches `V12_002.UI.Compliance.cs` OCO teardown |
| `acct.Orders` (live collection) | NT8 runtime | Iterated without `.ToArray()` snapshot — concurrent modification hazard |

**Downstream impact files (read `_followerBrackets` or `IsFleetAccount`):**
`V12_002.SIMA.cs`, `V12_002.SIMA.Flatten.cs`, `V12_002.SIMA.Fleet.cs`, `V12_002.SIMA.Lifecycle.cs`,
`V12_002.SIMA.Dispatch.cs`, `V12_002.SIMA.Shadow.cs`, `V12_002.Symmetry.BracketFSM.cs`,
`V12_002.Symmetry.Follower.cs`, `V12_002.UI.Compliance.cs`

**Risk classification:** `HIGH` — cancel logic executing against live broker order collection; any extracted helper must preserve exact predicate semantics for stop/target name prefix filtering and the dual-gate (`acctHasActiveFsm && masterHasPosition`).

---

## Top 3 Complexity Drivers

### Driver 1 — Order-state compound `||` chain (CYC +5)

```csharp
// Lines 310–316
order.OrderState == OrderState.Working
|| order.OrderState == OrderState.Accepted
|| order.OrderState == OrderState.Submitted
|| order.OrderState == OrderState.ChangePending
|| order.OrderState == OrderState.ChangeSubmitted
```

Five independent `||` branches each add 1 to CYC. This is a reused pattern — an identical block appears in `TryHandleFleet_CancelAll()` (line 200) and `CancelAll_ProcessMasterAccount()` (line 251).

**Extraction candidate:** `IsOrderCancellable(Order order)` — a single predicate helper that consolidates the instrument match, null guard, and all five state checks. Removes **5 CYC** from this method and can be shared across all three call sites.

---

### Driver 2 — Bracket-name prefix `||` filter chain (CYC +7)

```csharp
// Lines 321–328
oName.StartsWith("Stop_")
|| oName.StartsWith("S_")
|| oName.StartsWith("T1_")
|| oName.StartsWith("T2_")
|| oName.StartsWith("T3_")
|| oName.StartsWith("T4_")
|| oName.StartsWith("T5_")
```

Seven `||` predicates each add 1 to CYC. The same prefix set appears verbatim in `TryHandleFleet_CancelAll()` (lines 214–221), making this a DRY violation as well as a complexity hotspot.

**Extraction candidate:** `IsBracketManagementOrder(string orderName)` — encapsulates all seven prefix checks. Removes **7 CYC** from this method and eliminates the duplicate in the non-SIMA branch. Can also be used in REAPER audit sweeps.

---

### Driver 3 — Nested FSM + position dual-gate (CYC +2, semantic risk HIGH)

```csharp
// Lines 321–335  (outer `if` for name-prefix + inner guard)
if (oName.StartsWith("Stop_") || ... || oName.StartsWith("T5_"))
{
    // Build 1104.1: Preserve brackets ONLY if FSM is active AND Master has position.
    if (acctHasActiveFsm && masterHasPosition)
        continue;
}
```

The outer bracket-name check wraps an inner `&&` condition, creating a two-level nesting. The semantic coupling is tight: the `continue` path (preserve the order) depends on **both** FSM state and broker position truth together. Separating these incorrectly would introduce a correctness bug.

**Extraction candidate:** `ShouldPreserveBracketOrder(string orderName, bool acctHasActiveFsm, bool masterHasPosition)` — returns `bool`; the caller does `if (ShouldPreserveBracketOrder(...)) continue;`. This flattens the nesting from 3-deep to 2-deep and makes the preservation logic independently testable.

---

## CYC Budget After Recommended Extractions

| Extraction | CYC Removed | Post-extraction CYC |
|------------|-------------|---------------------|
| `IsOrderCancellable()`         | −5  | 13 |
| `IsBracketManagementOrder()`   | −7  | 6  |
| `ShouldPreserveBracketOrder()` | −2  | **4** |

Target post-refactor CYC: **≤ 5** (well inside the ≤ 10 threshold).

---

## Recommended Extraction Count

**3 helper methods** (all within the same partial class file or a new
`V12_002.Orders.CancelHelpers.cs` partial):

1. `IsOrderCancellable(Order order, string instrumentFullName)` — shared predicate
2. `IsBracketManagementOrder(string orderName)` — shared prefix filter
3. `ShouldPreserveBracketOrder(string orderName, bool acctHasActiveFsm, bool masterHasPosition)` — FSM/position preservation gate

Cross-file deduplication targets: `TryHandleFleet_CancelAll()` and `CancelAll_ProcessMasterAccount()` (same file) both contain near-identical copies of drivers 1 and 2 and should be updated to use the same helpers in the same refactor pass.

---

## Agent Tracking

| Field            | Value                        |
|------------------|------------------------------|
| Agent Name       | `v12-phase0-hotspot`         |
| Bobcoins Used    | 14                           |
| Execution Time   | ~45s                         |
| Wave             | 7                            |
| Phase            | 0                            |
| Epic             | EPIC-W7-156                  |
| CYC Confirmed    | 18                           |
| Status           | ✅ completed                  |
