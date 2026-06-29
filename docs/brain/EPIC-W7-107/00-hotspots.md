# EPIC-W7-107 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field       | Value                                                  |
|-------------|--------------------------------------------------------|
| Method Name | `HydrateFromOpenPositions`                             |
| CYC Score   | **34**                                                 |
| File Path   | `src/V12_002.SIMA.Lifecycle.cs`                        |
| Line Range  | 625 – 780                                              |
| Signature   | `private int HydrateFromOpenPositions(ConcurrentDictionary<string, Order> stopOrders, ConcurrentDictionary<string, Order> target1Orders … target5Orders, ref int ordersIndexed, ref int fsmCreated)` |

---

## Blast Radius Summary

The method writes into two strategy-wide concurrent dictionaries (`_followerBrackets`,
`_orderIdToFsmKey`) and one failure-tracking dictionary (`_positionPassFailedFirstSeen`).
These three fields are read or mutated by **16 files** across the codebase:

| Layer              | Affected Files                                                                                                                                                                    |
|--------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| SIMA subsystem     | `V12_002.SIMA.Lifecycle.cs`, `V12_002.SIMA.Dispatch.cs`, `V12_002.SIMA.Execution.cs`, `V12_002.SIMA.Fleet.cs`, `V12_002.SIMA.Shadow.cs`                                         |
| REAPER subsystem   | `V12_002.REAPER.cs`, `V12_002.REAPER.Audit.cs`, `V12_002.REAPER.Repair.cs`                                                                                                       |
| Symmetry subsystem | `V12_002.Symmetry.Follower.cs`, `V12_002.Symmetry.BracketFSM.cs`                                                                                                                 |
| Orders callbacks   | `V12_002.Orders.Callbacks.AccountOrders.cs`, `V12_002.Orders.Callbacks.Propagation.cs`                                                                                           |
| Core / UI          | `V12_002.cs`, `V12_002.Lifecycle.cs`, `V12_002.MetadataGuard.cs`, `V12_002.UI.IPC.Commands.Fleet.cs`                                                                             |

**Call chain:** `HydrateFSMsFromWorkingOrders` (line 866) → `HydrateFromOpenPositions` →
`_followerBrackets.TryAdd` / `_orderIdToFsmKey[…]` / `_positionPassFailedFirstSeen[…]`.

Any extraction must preserve atomic idempotency guarantees: the method guards against
duplicate FSMs via `_followerBrackets.ContainsKey` before writing.

---

## Top 3 Complexity Drivers

### 1 — Outer account iteration loop with three guard `continue` branches (lines 637–694)
```
foreach (Account acct in Account.All)          // loop entry (+1)
{
    if (!IsFleetAccount(acct)) continue;        // guard branch (+1)
    if (_followerBrackets.Values.Any(…)) continue; // LINQ predicate (+1, branch +1)
    Position acctPos = acct.Positions.FirstOrDefault(…); // LINQ (+1)
    if (acctPos == null) continue;              // guard branch (+1)
    // inner stop-order scan loop (see #2)
    if (recoveredKey == null) { … continue; }  // branch (+1)
    if (_followerBrackets.ContainsKey(…)) continue; // idempotent guard (+1)
}
```
Seven decision points **inside a single loop** before the FSM is even constructed.
This alone contributes ~8 to CYC.

### 2 — Repeated stop-order linear scan with nested null guards (lines 660–675)
```
foreach (var stopKvp in stopOrders.ToArray())  // nested loop (+1)
{
    Order stopCand = stopKvp.Value;
    if (stopCand == null) continue;             // null guard (+1)
    if (stopCand.Account == null) continue;     // null guard (+1)
    if (string.Equals(…)) { … break; }         // match branch (+1)
}
```
This inner O(n) scan is repeated per account iteration. Four decision points; the
linear scan itself is a latency risk on large account sets. Contributes ~4 to CYC.

### 3 — Five structurally-identical target-order linking blocks (lines 719–763)
```
if (target1Orders.TryGetValue(…) && targetOrd != null)  // branch (+1), && short-circuit (+1)
{
    fsm.Targets[0] = targetOrd;
    if (!string.IsNullOrEmpty(targetOrd.OrderId)) { … ordersIndexed++; } // branch (+1)
}
// × 5 for target1 … target5
```
Five copy-pasted blocks each carrying 3 decision points → **15 CYC points** in total.
This is the single largest contributor and the most mechanical refactoring target.

---

## Complexity Budget Breakdown (estimated)

| Driver                                       | Estimated CYC contribution |
|----------------------------------------------|---------------------------|
| Account loop + guard continuations           | ~8                        |
| Inner stop-order scan + null guards          | ~4                        |
| ×5 target-order linking blocks               | ~15                       |
| Null checks on stop order linkage (706–714)  | ~3                        |
| Base path / method entry                     | 1                         |
| Rounding / LINQ internal branches            | ~3                        |
| **Total**                                    | **≈ 34**                  |

---

## Recommended Extraction Count

**3 targeted extractions** are sufficient to reduce CYC below the threshold of 10:

| # | Extraction Target                              | Proposed Name                              | CYC Removed |
|---|------------------------------------------------|--------------------------------------------|-------------|
| 1 | Five target-order linking blocks (719–763)     | `LinkAllTargetOrders(fsm, key, t1…t5)`     | ~13–15      |
| 2 | Inner stop-order scan loop (660–675)           | `TryRecoverStopOrder(stopOrders, acct)`    | ~4          |
| 3 | Account FSM skip predicates (643–647, 651–655) | `ShouldSkipAccountForHydration(acct)`      | ~3          |

Applying all three extractions leaves `HydrateFromOpenPositions` with a residual CYC
of approximately **6–8**, well under the 10-point threshold.

---

## Agent Tracking

| Field            | Value                    |
|------------------|--------------------------|
| Agent Name       | v12-phase0-hotspot       |
| Bobcoins Used    | 9                        |
| Execution Time   | ~55s                     |
