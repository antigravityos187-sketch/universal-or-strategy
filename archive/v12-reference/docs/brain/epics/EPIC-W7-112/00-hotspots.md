# EPIC-W7-112 Hotspot Analysis

**Method:** `ClassifyOrderByPrefix`
**CYC:** 20 (aggregate scope: `ClassifyOrderByPrefix` + `RouteOrderToTargetDict` + inline switch in `AdoptMasterOrders`)
**File:** `src/V12_002.SIMA.Lifecycle.cs` (line 1262)

---

## Overview

`ClassifyOrderByPrefix` (line 1262) is a pure classification function that maps an order-name string
to one of eight string tokens: `"stop"`, `"target1"`–`"target5"`, `"entry"`, or `null`.
It is the gateway to the entire broker-order adoption subsystem: every call path through
`AdoptOrdersFromAccount` and `AdoptMasterOrders` passes through it before routing orders into
tracking dictionaries via `RouteOrderToTargetDict`.

The reported CYC of **20** reflects the aggregate complexity of the classification cluster:

| Method | CYC contribution |
|---|---|
| `ClassifyOrderByPrefix` (null-guard + 8 prefix branches) | 10 |
| `RouteOrderToTargetDict` (7-case `switch` + ternary inside `"stop"`) | 8 |
| Inline `switch` in `AdoptMasterOrders` (duplicate 6-case routing, lines 1229–1249) | 2 (delta above extracted helper) |
| **Total** | **20** |

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct callers** | `AdoptOrdersFromAccount` (line 950), `AdoptMasterOrders` (line 1219) |
| **Caller chain** | `EnumerateApexAccounts` → `HydrateWorkingOrdersFromBroker` → `AdoptFleetOrders` → `AdoptOrdersFromAccount` → `ClassifyOrderByPrefix` |
| **Secondary consumer** | `RouteOrderToTargetDict` (line 994) — receives classification token and performs a 7-arm switch to route to `stopOrders`, `target1Orders`–`target5Orders`, or `entryOrders` |
| **Shared state written** | `stopOrders`, `target1Orders`, `target2Orders`, `target3Orders`, `target4Orders`, `target5Orders`, `entryOrders` (all `ConcurrentDictionary<string, Order>`) |
| **Shared state read** | `activePositions` (in `AdoptSingleOrder`, downstream of routing) |
| **External dependency** | `Account.Orders` (broker thread — snapshot guard via `.ToArray()` applied at call site) |
| **Side-effects** | Adoption counter (`adoptedCount` ref), `activePositions` struct mutations (via `AdoptSingleOrder`), structured log via `Print()` |
| **Threading constraint** | Strategy thread only (called from actor-serialized `EnumerateApexAccounts`; per doc comment line 898) |
| **Risk on change** | **Medium-High** — any prefix addition/removal must be mirrored in four locations: `ClassifyOrderByPrefix`, `RouteOrderToTargetDict`, the inline switch in `AdoptMasterOrders`, and the `v12Prefixes` arrays inside `SweepBrokerOrders`. No compile-time enforcement of that invariant. |

**Affected symbol count (blast radius):** 7 methods directly coupled; 7 shared concurrent dictionaries.

---

## Top 3 Complexity Drivers

### 1. 8-way prefix if/else chain in `ClassifyOrderByPrefix` (CYC +9)

The classification body (lines 1264–1285) is a flat `if / else if × 7 / else` chain against
string literal prefixes using `StringComparison.OrdinalIgnoreCase`. Each branch is a
`StartsWith` predicate returning a string constant, contributing **+1 CYC per branch**.
The guard `if (string.IsNullOrEmpty(orderName))` at the top adds another +1, bringing the
method's standalone CYC to **10**. The chain is not table-driven: adding a ninth order type
(e.g. `"OCO_"`) requires manually extending three separate switch/chain structures across
the file, and the lack of a central prefix registry makes completeness audits error-prone.

### 2. Duplicate routing switch in `RouteOrderToTargetDict` vs `AdoptMasterOrders` (CYC +10)

`RouteOrderToTargetDict` (lines 1005–1046) mirrors the classification token set with a
7-arm `switch` that assigns `targetDict`, `key`, and `dictName`. The `"stop"` arm contains
an additional ternary (`StartsWith("Stop_") ? Substring(5) : Substring(2)`) that adds +1
more. Separately, `AdoptMasterOrders` (lines 1229–1249) contains its own inline 6-arm
`switch` on the same classification tokens but computes the dictionary key with a different
substring offset (`Substring(5)` vs `Substring(2)` logic). This duplication means the
routing logic exists in **two non-trivially divergent forms**, each contributing to the
aggregate CYC and each carrying independent regression risk.

### 3. Null/state guard matrix at call sites (CYC +1 each, scattered)

Both `AdoptOrdersFromAccount` (lines 931–964) and `AdoptMasterOrders` (lines 1200–1254) repeat
the same 5-condition `OrderState` guard: `Working || Accepted || Submitted || ChangePending ||
ChangeSubmitted`. Each `||` operand that influences control flow adds +1 CYC at the call site
(extracted into `IsValidOrderState` for fleet, but inlined for master). The master path also
adds `OrderState.Unknown` as a sixth state (line 1208), creating a silent divergence from the
fleet path. This asymmetry between master and fleet adoption guards is a source of correctness
risk independent of the classification chain itself.

---

## Recommended Extraction Count

**3 targeted extractions recommended.**

| # | Extraction | Rationale | Expected CYC delta |
|---|---|---|---|
| 1 | Replace the `if/else if` chain in `ClassifyOrderByPrefix` with a **static read-only dictionary** `_prefixMap` (prefix → token) iterated via `foreach` | Eliminates 7 `else if` branches; classification becomes a single loop with one `if` return, reducing CYC from 10 → 3 | −7 |
| 2 | Extract `RouteOrderToTargetDict`'s `switch` body into a **static `_classificationRoutes` dictionary** mapping token → `(Func<string,string> keyExtractor, ConcurrentDictionary<string,Order> dict, string dictName)` | Removes 7 switch arms from `RouteOrderToTargetDict` and eliminates the need for the duplicate switch in `AdoptMasterOrders` | −8 |
| 3 | Unify `AdoptMasterOrders` inline routing to use `RouteOrderToTargetDict` (already extracted) rather than its own switch | Removes the divergent duplicate and closes the `"entry"` exclusion gap via a single exclusion guard before the shared routing call | −2 |

**Net CYC after extractions: ~3** (well below the CYC ≤ 10 threshold).

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~90s |
