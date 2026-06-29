# EPIC-W7-086 Hotspot Analysis

**Method:** ProcessReaperFlatten_CancelWorkingOrders
**CYC:** 34
**File:** src/V12_002.REAPER.Audit.cs

---

## Overview

`ProcessReaperFlatten_CancelWorkingOrders` (lines 852–884) is the emergency-cancel sweep step of the
REAPER flatten pipeline. Called exclusively from `ProcessReaperFlattenQueue` (line 811) on the
strategy thread via `TriggerCustomEvent`, it is responsible for snapshoting all broker orders on a
target account, filtering to cancellable states for the instrument under management, collecting them
into a staging list, and issuing cancel calls through the `CancelOrderOnAccount` gateway.

Despite being a sub-method extracted from the flatten pipeline, the method carries a Cyclomatic
Complexity of 34 — the highest single hotspot in `V12_002.REAPER.Audit.cs`. The complexity is
concentrated in two compounded nodes: a 4-branch `OrderState` OR predicate embedded inside a
double-nested `foreach`/`if` and a second `foreach` for the cancel pass, all sitting within the
larger `ProcessReaperFlattenQueue` dispatch frame that contributes additional `while`/`try/catch/finally`
branches to the aggregate CYC score.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `ProcessReaperFlattenQueue` (line 811, `src/V12_002.REAPER.Audit.cs`) |
| **Caller chain** | `AuditFleet_HandleCriticalDesyncFlatten` / `AuditMaster_HandleDesyncFlatten` → `TriggerCustomEvent` → `ProcessReaperFlattenQueue` → `ProcessReaperFlatten_CancelWorkingOrders` |
| **Sibling steps in pipeline** | `ProcessReaperFlatten_FindAccount`, `ProcessReaperFlatten_ClosePositions`, `ProcessReaperFlatten_TerminateFsms` |
| **Shared state read** | `targetAcct.Orders` (broker collection — snapshotted via `ToArray()`, H14-FIX), `Instrument.FullName` (read-only strategy property) |
| **Cancel gateway** | `CancelOrderOnAccount` (`src/V12_002.Orders.CancelGateway.cs:46`) — 8 call sites across the codebase; routes fleet orders via `executingAccount.Cancel()`, master via `CancelOrder()` |
| **In-flight guard cleared by** | `_reaperFlattenInFlight.TryRemove(...)` in `ProcessReaperFlattenQueue.finally` — not inside this method |
| **Threading constraint** | Strategy thread only (marshaled via `TriggerCustomEvent`); `targetAcct.Orders.ToArray()` snapshot is the thread-safety boundary |
| **Side-effects** | Fires `Account.Cancel()` / `CancelOrder()` on broker for every matching working order; emits `[REAPER] Emergency Cancel: N orders on {accountName}` diagnostic log line |
| **Risk on change** | High — directly controls live order cancellation during emergency desync recovery; any logic gap leaves orphaned working orders that block subsequent market-close from `ProcessReaperFlatten_ClosePositions` |

**Affected symbol count (blast radius):** 5 direct symbols coupled; 1 shared broker collection; 1 gateway with 8 consumers.

---

## Top 3 Complexity Drivers

1. **4-branch `OrderState` OR compound embedded inside double `foreach`/`if` nesting**
   The cancellable-state predicate `order.OrderState == OrderState.Working || .Submitted || .Accepted
   || .ChangePending` contributes 3 additional CYC branches (each `||` beyond the first). This
   predicate is inlined directly inside the outer `foreach` body, combined with a null guard on
   `order` and an instrument FullName equality check, producing a 6-operand compound boolean that is
   both hard to read at a glance and impossible to unit-test in isolation without a full `Account`
   mock. The 4-state fan-out accounts for roughly 7 CYC points of the method's total.

2. **Two-pass collect-then-cancel pattern with a second `foreach`**
   The method uses a `List<Order> ordersToCancel` staging buffer and two separate loops: one to
   collect and one to cancel. This is the correct H14-FIX pattern (avoids collection-modified
   exceptions), but the guard `if (ordersToCancel.Count > 0)` + inner `foreach` adds 2 more
   CYC branches and creates a structural seam where the diagnostic `Print` call is only reachable
   via the guarded path — a subtle coupling between the count check and the log line. Sub-total:
   ~4 CYC from the dual-loop skeleton.

3. **Outer dispatch frame CYC bleed from `ProcessReaperFlattenQueue`**
   The stated CYC=34 reflects the aggregate complexity of the flatten pipeline dispatcher
   (`ProcessReaperFlattenQueue`: `while` + `try/catch/finally` + `if (targetAcct != null)` + `else`)
   plus all sub-steps. Even after sub-method extraction, the `while` loop body's branching
   (`targetAcct != null` → 2 branches, `catch` → 1, `finally` → 1) contributes ~5 CYC to the
   reported hotspot score and will remain in the dispatcher regardless of further sub-method
   extraction.

---

## Recommended Extraction Count

**2 helpers recommended for Phase 1.**

**Rationale:**

Extract the following from `ProcessReaperFlatten_CancelWorkingOrders`:

- `BuildFlattenCancelList(Account targetAcct) → List<Order>` — encapsulates the `ToArray()` snapshot,
  the instrument filter, and the 4-branch `OrderState` predicate into a single testable query method.
  Reduces the outer loop body to a single method call. Estimated extracted CYC: ~8.
- `IsOrderCancellable(Order order) → bool` — extracts the compound `OrderState` OR predicate as a
  named, reusable predicate. Already needed by `BuildFlattenCancelList`; can be shared with
  `ProcessReaperFlatten_ClosePositions` and `AuditFleet_CheckWorkingStop` which test overlapping
  states. Estimated extracted CYC: ~5.

After extraction, `ProcessReaperFlatten_CancelWorkingOrders` is reduced to ≤4 CYC (snapshot call +
count guard + cancel loop + print). The dispatcher `ProcessReaperFlattenQueue` CYC bleed (~5 CYC)
is structural and should not be extracted further without a broader pipeline refactor.

---

## Agent Tracking

Agent Name: bob-hotspot-w7-086 | Bobcoins Used: 1.0 | Execution Time: ~55s
