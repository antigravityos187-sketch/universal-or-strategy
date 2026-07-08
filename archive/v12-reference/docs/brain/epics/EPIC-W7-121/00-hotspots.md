# EPIC-W7-121 Hotspot Analysis

**Method:** SymmetryGuardCascadeFollowerCleanup
**CYC:** 10
**File:** src/V12_002.Symmetry.Replace.cs

---

## Overview

`SymmetryGuardCascadeFollowerCleanup` (lines 198–243) is the cascade terminator for all follower
entry orders linked to a cancelled master. When a master entry order is cancelled and SIMA is
enabled, this method resolves the dispatch context, iterates every follower in the immutable
`ctx.Followers` snapshot, and issues `CancelOrderSafe` for any still-working entry order.
The deferred delta-rollback pattern (Build 960 audit fix) means it intentionally does **not**
call `DeltaExpectedPositionLocked` — that responsibility is delegated to
`HandleMatchedFollower_DeltaRollback` in `V12_002.Orders.Callbacks.AccountOrders.cs` on
confirmed-cancel receipt.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `HandleOrderCancelled_RollbackUnfilledEntry` (line 771, `src/V12_002.Orders.Callbacks.cs`) |
| **Call condition** | `EnableSIMA && !kvp.Value.IsFollower` — master-only, SIMA-gated |
| **Caller chain** | `OnOrderUpdate` → `HandleOrderCancelled_RollbackUnfilledEntry` → `SymmetryGuardCascadeFollowerCleanup` |
| **Deferred continuation** | `HandleMatchedFollower_DeltaRollback` (`src/V12_002.Orders.Callbacks.AccountOrders.cs` line 691) — fires on confirmed cancel via `OnAccountOrderUpdate` |
| **Shared state read** | `symmetryMasterEntryToDispatch` (ConcurrentDictionary), `symmetryDispatchById` (ConcurrentDictionary), `ctx.Followers` (immutable `string[]` snapshot per ADR-019) |
| **Shared state write** | `entryOrders` (read-only here), `activePositions` (read-only here) |
| **Side-effects** | `CancelOrderSafe(order, pos)` — broker-side cancel; `Print(...)` — diagnostic only |
| **Threading constraint** | Strategy thread only (called from `OnOrderUpdate`); snapshot guard on `ctx.Followers` makes iteration lock-free |
| **Risk on change** | High — any extraction must preserve the `continue`-on-null pattern for all three dict lookups; breaking early exits causes zombie follower orders |

**Affected symbol count (blast radius):** 5 symbols directly coupled; 3 shared concurrent state bags; 1 deferred continuation in a separate file.

---

## Top 3 Complexity Drivers

### 1. Multi-condition `OrderState` guard with 3-way OR (lines 225–230)

The active-order check fans across three `OrderState` values (`Working`, `Submitted`, `Accepted`)
joined by `||`. In McCabe CYC terms this is the single highest-density branch cluster in the
method, contributing **3 CYC points** (one per OR-joined condition beyond the first). The pattern
mirrors the four-state variant in `SymmetryGuardReplaceExistingFollowerTarget` (lines 46–50) but
deliberately omits `ChangePending` because a follower entry awaiting a change-pending confirmation
should not be force-cancelled during cascade — an intentional semantic difference that must be
preserved in any extraction.

### 2. Triple-null early-exit chain inside the `foreach` loop (lines 218–223)

Three sequential guard conditions (`!activePositions.TryGetValue`, `!entryOrders.TryGetValue`,
`order == null`) each add **+1 CYC** via `continue` exits. Although individually trivial, all
three are nested inside the `foreach` loop body (which itself is +1), creating a four-level
decision tree over a single loop iteration. Extracting this as a helper must return a nullable
`(PositionInfo pos, Order order)?` tuple to preserve the null semantics without adding a bool
parameter that masks the reason for skipping a follower.

### 3. Ternary account-name selector inside the `Print` call (line 235) + dual dict-miss early returns (lines 200–203)

The ternary `pos.ExecutingAccount != null ? pos.ExecutingAccount.Name : "Master"` adds **+1 CYC**
inside an already-nested conditional. Combined with the two top-level early returns on
`TryGetValue` failure (lines 200–201 and 202–203), which together contribute **+2 CYC** before
the loop even starts, these structural guards account for 3 of the method's 10 CYC points
while doing no business logic. They are prime candidates for a single named guard helper
(`TryResolveCascadeContext`) that returns a nullable `(string dispatchId, string[] followers)?`.

---

## Recommended Extraction Count

**3 helpers recommended.**

| # | Proposed Helper | Absorbs |
|---|---|---|
| 1 | `TryResolveCascadeContext(string masterEntryName, out string[] followers)` | The two top-of-method `TryGetValue` guards + `ctx.Followers` snapshot read (lines 200–206). Eliminates 2 CYC from dispatcher. |
| 2 | `TryGetFollowerOrderForCancel(string followerName, out PositionInfo pos, out Order order)` | The three-guard `continue` chain inside the `foreach` (lines 218–223). Returns false if any guard fails, eliminating 3 nested CYC points from the loop body. |
| 3 | `GetFollowerAccountName(PositionInfo pos)` | The `pos.ExecutingAccount != null ?` ternary (line 235). Trivial but eliminates the last inline branch from the loop body and makes the `Print` call a pure format call. |

**Post-extraction dispatcher CYC target: ≤ 4** (base + `foreach` + OrderState compound condition reduced to a single delegating call).

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase0-hotspot |
| Bobcoins Used | 1.0 |
| Execution Time | ~55s |
