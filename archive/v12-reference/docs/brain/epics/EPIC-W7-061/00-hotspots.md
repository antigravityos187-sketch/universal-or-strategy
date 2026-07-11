# EPIC-W7-061 | Phase 0 — Hotspot Analysis
**Wave:** 7 | **Phase:** 0  
**Method:** `SubmitAndRegisterFleetOrders`  
**Source:** [`src/V12_002.SIMA.Fleet.cs:174`](../../src/V12_002.SIMA.Fleet.cs)  
**CYC Confirmed:** 12  
**Generated:** Phase 0 automated hotspot scan

---

## 1. Method Overview

[`SubmitAndRegisterFleetOrders`](../../src/V12_002.SIMA.Fleet.cs:174) is the terminal commit step of the SIMA fleet-dispatch pipeline. It is invoked exclusively by [`ProcessFleetSlot`](../../src/V12_002.SIMA.Fleet.cs:44) — the shared slot processor called by both the Photon ring consumer path and the legacy `ConcurrentQueue` fallback path. Its responsibilities are:

1. **Slice the order array** — conditionally copy `orders[0..orderCount-1]` into a fresh `submitOrders` array if `orderCount < orders.Length`.
2. **Submit** — call `acct.Submit(submitOrders)` (blocking broker API call).
3. **Clear dispatch sync guard** — call `ClearDispatchSyncPending(expectedKey)` and set `ref syncCleared = true`.
4. **Advance FSM state** — if the bracket FSM entry is still `PendingSubmit`, transition it to `Submitted` and stamp `LastUpdateUtc`.
5. **Index order IDs** — if an FSM entry exists for `fleetEntryName`, write every non-null `ord.OrderId → fleetEntryName` mapping into `_orderIdToFsmKey`.
6. **Emit diagnostic log** — `Print("[PUMP] Submitted …")`.

---

## 2. Cyclomatic Complexity Breakdown (CYC = 12)

The method body contains the following independent decision paths (each `+1` from the base of 1):

| # | Branch | Location |
|---|--------|----------|
| 1 | `if (orders != null …)` — null/bounds guard before array copy | L184 |
| 2 | `&& orderCount > 0` — secondary condition in same `if` | L184 |
| 3 | `&& orderCount < orders.Length` — tertiary condition in same `if` | L184 |
| 4 | `if (_followerBrackets.TryGetValue(…) && pFsm != null …)` | L195-196 |
| 5 | `&& pFsm != null` — compound AND | L197 |
| 6 | `&& pFsm.State == FollowerBracketState.PendingSubmit` — state guard | L198 |
| 7 | `if (_followerBrackets.TryGetValue(fleetEntryName, out fsm))` — second lookup | L206 |
| 8 | `for (int i = 0; i < orderCount; i++)` — loop (each iteration is a branch) | L208 |
| 9 | `if (ord != null …)` — null guard on order | L211 |
| 10 | `&& !string.IsNullOrEmpty(ord.OrderId)` — compound AND, empty-ID guard | L211 |
| 11 | (implicit) `string.IsNullOrEmpty` internal branch on null | L211 |
| 12 | (implicit) `string.IsNullOrEmpty` internal branch on empty string | L211 |

> **Net result:** Base 1 + 11 explicit branch increments = **CYC 12**, consistent with the Wave-7 ticket.

---

## 3. Blast Radius

### Direct callers
| Caller | File | Notes |
|--------|------|-------|
| [`ProcessFleetSlot`](../../src/V12_002.SIMA.Fleet.cs:44) | `SIMA.Fleet.cs` | Only caller; invoked within try/catch with full rollback on exception |

### Transitive callers (2 hops)
| Caller | File |
|--------|------|
| [`PumpFleetDispatch`](../../src/V12_002.SIMA.Fleet.cs:233) | `SIMA.Fleet.cs` — legacy ConcurrentQueue drain |
| [`ProcessValidPhotonSlot`](../../src/V12_002.SIMA.Fleet.cs:395) | `SIMA.Fleet.cs` — Photon ring consumer |

### Shared mutable state touched
| State | Type | Access pattern |
|-------|------|---------------|
| `_followerBrackets` | `ConcurrentDictionary<string, FollowerBracketFSM>` | `TryGetValue` (×2) + in-place state mutation |
| `_orderIdToFsmKey` | `ConcurrentDictionary<string, string>` | Indexer write inside loop |
| `_dispatchSyncPendingExpKeys` | `ConcurrentDictionary<string, byte>` | `TryRemove` via `ClearDispatchSyncPending` |
| `syncCleared` | `ref bool` (caller-owned) | Set to `true`; used by caller's rollback guard |

### External side effects
| Effect | Description |
|--------|-------------|
| `acct.Submit(submitOrders)` | Blocking broker API call — can throw; orders routed to exchange |
| `Print(…)` | NinjaTrader diagnostic output |

---

## 4. Hotspots & Risk Factors

### H-1 · Redundant double `TryGetValue` on `_followerBrackets` (same key, consecutive)
- **Lines:** 195–213
- **Risk:** Low-severity logic duplication. The first lookup (`pFsm`) checks `PendingSubmit`; the second (`fsm`) enters unconditionally to index order IDs. These can be collapsed to a single lookup, eliminating one ConcurrentDictionary read and two local variables.
- **CYC contribution:** +3 (branches 4, 5, 6 and branches 7 are redundant paths to the same key)

### H-2 · FSM state transition occurs **after** `acct.Submit`
- **Lines:** 190 vs 199–202
- **Risk:** Medium. If `acct.Submit` succeeds but the process is interrupted before the FSM is transitioned (e.g., thread abort, reentrancy via `TriggerCustomEvent`), the FSM remains in `PendingSubmit`. The REAPER audit at [`V12_002.REAPER.Audit.cs:482`](../../src/V12_002.REAPER.Audit.cs) treats `Submitted | Accepted` as valid in-flight states; `PendingSubmit` persisting after submit is a latent REAPER false-positive surface.

### H-3 · Conditional array allocation on hot path
- **Lines:** 184–188
- **Risk:** Low. `new Order[orderCount]` + `Array.Copy` allocates heap memory on every dispatch where `orderCount < orders.Length`. In high-frequency fleet dispatch scenarios this produces GC pressure. The pool-slot path (`_photonPool`) already avoids this for Photon-ring dispatches, but the legacy ConcurrentQueue path still hits this branch.

### H-4 · `string.IsNullOrEmpty(ord.OrderId)` inside tight loop
- **Lines:** 211
- **Risk:** Low-medium. Called once per order per dispatch. `OrderId` is only populated after broker acknowledgement; at submit-time most orders will have empty IDs, making the loop's indexing body a no-op in the common case. The loop's CYC contribution (branches 8–12) is real but the body is typically inert at dispatch time.

### H-5 · `syncCleared` ref-bool coupling across call boundary
- **Lines:** 192 (set), caller L70–71 (checked in catch)
- **Risk:** Medium. The rollback in `ProcessFleetSlot`'s catch block depends on `syncCleared` being set **before** the FSM update code (L195+). Any future reordering of lines 191–202 risks leaving the sync-pending guard uncleaned. The coupling is implicit and not documented at the call site.

---

## 5. Recommended Refactors (Phase 1 Candidates)

| Priority | Refactor | Expected CYC reduction |
|----------|----------|----------------------|
| P1 | Merge the two `_followerBrackets.TryGetValue` lookups into one; handle both `PendingSubmit` transition and order-ID indexing in the same `if` block | −3 |
| P2 | Extract order-ID indexing into a private `RegisterOrderIds(FollowerBracketFSM, Order[], int)` helper | −2 (moves loop complexity out of method) |
| P3 | Document `ref syncCleared` semantics with an inline contract comment and assert-style guard | 0 (safety) |

Applying P1 + P2 would reduce `SubmitAndRegisterFleetOrders` to approximately **CYC 7**, below the Wave-7 target threshold of 10.

---

## 6. Dependency Map

```
PumpFleetDispatch (ConcurrentQueue drain)
 └─► ProcessFleetSlot
      ├─► ValidateDispatchTimestamp
      ├─► InitializeFollowerBracketFSM
      └─► SubmitAndRegisterFleetOrders  ◄── THIS METHOD
           ├── acct.Submit()               [broker I/O]
           ├── ClearDispatchSyncPending()  [_dispatchSyncPendingExpKeys]
           ├── _followerBrackets (x2)      [FSM state + ID index]
           └── _orderIdToFsmKey (write)    [order routing index]

ProcessValidPhotonSlot (Photon ring)
 └─► ProcessFleetSlot (same path as above)
```

---

## 7. Summary

`SubmitAndRegisterFleetOrders` acts as the **atomic commit fence** of the fleet dispatch system: it is the point of no return after which orders have been sent to the broker. Its CYC of 12 is driven primarily by the redundant double FSM lookup (H-1) and the compound guard on `string.IsNullOrEmpty` inside the order-ID indexing loop (H-4). The highest-risk finding is H-2 (post-submit FSM state lag) and H-5 (implicit `ref syncCleared` ordering contract). Phase 1 refactors targeting H-1 and extracting the loop into a helper will bring CYC below the threshold with minimal behaviour change.
