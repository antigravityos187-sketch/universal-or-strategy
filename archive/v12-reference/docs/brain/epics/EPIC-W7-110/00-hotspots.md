# EPIC-W7-110 Hotspot Analysis

**Method:** AdoptMasterOrders
**CYC:** 22
**File:** src/V12_002.SIMA.Lifecycle.cs (line 1195)

---

## Overview

`AdoptMasterOrders` is the master-account counterpart to `AdoptFleetOrders`. It iterates
`Account.Orders` (the strategy's primary account), classifies each working bracket order by
name prefix, derives a tracking-dictionary key, and routes the order into one of six
`ConcurrentDictionary<string, Order>` dicts (`stopOrders`, `target1Orders`–`target5Orders`).
Called once per SIMA enable or reconnect from `HydrateWorkingOrdersFromBroker` (line 320),
it runs exclusively on the strategy thread (Actor serialized, cold path only).

The CYC of 22 comes from three entangled complexity sources: a 6-clause compound state guard,
a 6-arm `switch` dispatch block, and dual null-conditional (`?.`) chains on instrument identity
— all nested inside a single `foreach` loop. The method currently duplicates significant logic
already extracted into `IsValidOrderState` (used by `AdoptOrdersFromAccount`) but does not
reuse it, and its key-derivation ternary is a partial duplicate of `RouteOrderToTargetDict`.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `HydrateWorkingOrdersFromBroker` (line 320, `src/V12_002.SIMA.Lifecycle.cs`) |
| **Caller chain** | `EnumerateApexAccounts` → `HydrateWorkingOrdersFromBroker` → `AdoptMasterOrders` |
| **Callee helpers** | `ClassifyOrderByPrefix` (line 1262) — shared with `AdoptOrdersFromAccount` |
| **Shared state written** | `stopOrders`, `target1Orders`, `target2Orders`, `target3Orders`, `target4Orders`, `target5Orders` (6 ConcurrentDictionary bags) |
| **Shared state read** | `Account.Orders` (broker-thread collection — snapshotted via `ToArray()`), `Instrument.FullName` |
| **Downstream consumers of written state** | `HydrateFSMsFromWorkingOrders`, `SweepTrackedOrders`, `OnAccountOrderUpdate`, trailing-stop update paths (`V12_002.Orders.Management.StopSync.cs`, `V12_002.Trailing.StopUpdate.cs`) |
| **Parallel fleet path** | `AdoptFleetOrders` / `AdoptOrdersFromAccount` — same dict targets, fleet accounts only |
| **Threading constraint** | Strategy thread only; all dict writes are single-writer ConcurrentDictionary ops (safe) |
| **Risk on change** | Medium-high — incorrect key derivation or missed classification silently drops orders from tracking, causing REAPER false desync or unprotected master positions at reconnect |

**Affected symbol count (blast radius):** 9 symbols directly coupled; 6 shared concurrent state bags written.

---

## Top 3 Complexity Drivers

### 1. Six-clause compound `OrderState` guard (CYC +6)

Lines 1207–1214 express a single "skip if not live" guard across six `&&`-chained inequality
comparisons — one for each of `Working`, `Accepted`, `Submitted`, `ChangePending`,
`ChangeSubmitted`, and `Unknown`. Each clause contributes +1 to CYC. The `Unknown` state is an
extra branch absent from `IsValidOrderState` (which handles only the first five), meaning the
master path intentionally diverges from the fleet path. This dual-path divergence is undocumented
at the call site and makes future state additions error-prone: any new `OrderState` must be
added to both guards independently.

**Extraction opportunity:** Extract an `IsValidMasterOrderState(Order ord)` (or extend
`IsValidOrderState` with an `includeUnknown` flag) to encapsulate the 6-clause guard and bring
CYC back in line with the fleet equivalent. Estimated CYC reduction: **−5**.

### 2. Six-arm `switch` dispatch with inline dictionary writes (CYC +6)

Lines 1229–1249 route the classified order to a target dict via a `switch` on a string
classification token. Each `case` arm writes directly to a named field (`stopOrders[key] = ord`,
etc.) instead of routing through the already-available `RouteOrderToTargetDict` helper (line 994).
The six arms add 6 CYC points and create a verbatim duplication of the routing table that exists
in `RouteOrderToTargetDict`. Any new order class (e.g., `target6`) must be added in three
places: `ClassifyOrderByPrefix`, `RouteOrderToTargetDict`, and this inline `switch`.

**Extraction opportunity:** Replace the inline `switch` with a call to `RouteOrderToTargetDict`,
which already returns the target dict reference and derived key. Estimated CYC reduction: **−5**.

### 3. Nested null-conditional chains + ternary key derivation inside loop (CYC +5)

Lines 1202 and 1224 each contain compound null-conditional or ternary expressions:

- `ord.Instrument?.FullName != Instrument?.FullName` (line 1202): two `?.` null-conditionals
  each contribute +1 CYC under strict branch-counting, plus the inequality guard itself.
- `name.StartsWith("Stop_", ...) ? name.Substring(5) : name.Substring(2)` (line 1224): the
  ternary adds +1 CYC, and the `Start​sWith` guard is a partial duplicate of what
  `ClassifyOrderByPrefix` already resolves. For `target1`–`target5` prefixes (`T1_`–`T5_`), the
  key is always `Substring(3)`, not `Substring(2)`, meaning the current key derivation is
  incorrect for non-stop classifications (off-by-one silent bug — all target keys include a
  stray leading character unless the classifier corrects it downstream).

**Extraction opportunity:** Replace the ternary with a call to `RouteOrderToTargetDict` (which
already embeds correct per-prefix `Substring` logic), eliminating both the ternary branch and
the latent key-derivation bug. Estimated CYC reduction: **−2**.

---

## Recommended Extraction Count

**3 targeted extractions recommended.**

| # | Extraction | Estimated CYC Reduction | Risk |
|---|---|---|---|
| 1 | Extract `IsValidMasterOrderState` (or extend `IsValidOrderState` with `bool includeUnknown`) | −5 | Low |
| 2 | Replace inline `switch` with `RouteOrderToTargetDict` call | −5 | Low–Medium |
| 3 | Replace ternary key derivation + dict write with unified `RouteOrderToTargetDict` + `TryAdd` pattern | −2 | Medium (fix latent substring bug) |

**Post-extraction target CYC: ≤10** (baseline 1 + foreach 1 + instrument guard 1 + null-check 1 +
`ClassifyOrderByPrefix` call 1 + `IsValidMasterOrderState` call 1 + null-coalesce 1 + adoptedCount
increment 1 + two residual guards ≤ 2 = 10 max).

---

## Agent Tracking

Agent Name: v12-phase0-hotspot | Bobcoins Used: 1.2 | Execution Time: ~90s
