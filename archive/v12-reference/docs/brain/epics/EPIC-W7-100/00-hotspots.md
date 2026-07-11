# EPIC-W7-100 Hotspot Analysis

**Method:** `ClosePositionsOnlyApexAccounts`
**CYC (tool-reported at ticket creation):** 0 *(tool returned no score — see Manual Analysis below)*
**CYC (manual structural count):** ~9
**File:** `src/V12_002.SIMA.Flatten.cs`
**Lines:** 516–589

> ⚠️ **Manual Review Required:** The automated CYC tool reported 0, indicating the method was not
> indexed at the time of ticket creation. A manual branch-count was performed against the source
> (lines 516–589) and yielded CYC ≈ 9. This document reflects the manual analysis as the
> authoritative score for planning purposes.

---

## Overview

`ClosePositionsOnlyApexAccounts` is the *positions-only* variant of the full `FlattenAllApexAccounts`
fleet operation. It enqueues a `FlattenWorkItem` (with `ZombieSweepOnly = true`, `CancelOnly = false`)
for every fleet account and the master-account fallback, then kicks the shared `PumpFlattenOps`
async pump via `TriggerCustomEvent`. It preserves all pending/working orders and only closes live
positions — hence "positions only." It is the target of the `FLATTEN_ONLY` IPC command handled in
[`TryHandleFleet_FlattenOnly`](src/V12_002.UI.IPC.Commands.Fleet.cs:106).

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `TryHandleFleet_FlattenOnly` (line 115, `src/V12_002.UI.IPC.Commands.Fleet.cs`) |
| **Caller trigger** | IPC action string `"FLATTEN_ONLY"` dispatched from external UI/command channel |
| **Shared queue written** | `_pendingFlattenOps` (`ConcurrentQueue<FlattenWorkItem>`) — also written by `FlattenAllApexAccounts`, race-risk if both are inflight |
| **Guard mutated** | `isFlattenRunning` (bool, strategy thread) — set `true` on entry, released only in catch-paths; normal release delegated to `PumpFlattenOps`/`ChainNextFlattenOp` |
| **Pump called** | `PumpFlattenOps` (same file, line 124) via `TriggerCustomEvent` |
| **Fallback called** | `PerformFallbackFlatten` (same file, line 328) on both catch branches |
| **State read** | `Account.All` (broker snapshot), `IsFleetAccount` (same class), `Account.Positions.Count` (broker) |
| **Work item routing** | `ZombieSweepOnly = true` → `ProcessFlattenWorkItem_CancelOrders` will only cancel zombie-tagged orders (T1_/T2_/EMERGENCY_STOP_ prefixes), then `ProcessFlattenWorkItem_ClosePositions` runs normally |
| **Threading constraint** | Strategy thread only; `TriggerCustomEvent` marshals pump to strategy thread |
| **Risk on change** | Medium — the `isFlattenRunning` guard is released in catch branches but NOT in the happy path (pump owns it); any extraction must preserve this asymmetric release ownership |

**Affected symbol count (blast radius):** 7 symbols directly coupled; 1 shared concurrent queue; 1 shared bool guard.

---

## Top 3 Complexity Drivers

### 1. Compound guard on master-account fallback (line 546)
```csharp
if (!masterCovered && Account.Positions.Count > 0)
```
This is a **compound boolean condition** (`&&`), contributing +2 CYC on its own. Combined with the
preceding `bool masterCovered = IsFleetAccount(Account)` evaluation and the fact that the master
account is handled as a *special case* diverging from the fleet-account `foreach` path, this creates
a parallel-path decision tree. The condition is asymmetric with `FlattenAllApexAccounts` (which only
checks `!masterCovered`, not `Positions.Count > 0`), making the two methods subtly behaviorally
divergent — a latent source of confusion and a candidate for documented contract extraction.

### 2. Dual-catch exception handling with fallback dispatch (lines 569–582)
The method contains two `catch` blocks — one `InvalidOperationException when (...)` filter-catch
and one broad `catch (Exception)` — each independently calling `PerformFallbackFlatten` and
manually resetting `isFlattenRunning`. This pattern duplicates the exact same two-line recovery
sequence, adding +2 CYC for the catch paths plus +1 for the `when` filter predicate. The duplication
is structural: the identical pattern appears in `FlattenAllApexAccounts` and `ChainNextFlattenOp`,
meaning any future modification to the recovery strategy requires changes in three places.

### 3. `foreach` + `IsFleetAccount` filter loop with `continue` guard (lines 527–542)
The inner `foreach (Account acct in snapshot)` loop with an early-`continue` on `!IsFleetAccount(acct)`
contributes +2 CYC (loop + guard branch). Though individually modest, the loop body enqueues a
`FlattenWorkItem` inline (anonymous object initializer) — meaning any future need to vary enqueue
behaviour (e.g., per-instrument filtering, per-account throttle) will add CYC directly to this
already-compound control flow rather than to a dedicated helper.

---

## Recommended Extraction Count

**2 targeted extractions recommended for Phase 1.**

| # | Proposed Helper | Rationale | Estimated CYC reduction |
|---|---|---|---|
| 1 | `EnqueueFleetAccountsForPositionsClose(Account[] snapshot)` | Extracts the `foreach` + master-fallback enqueue block (lines 527–559) into a named helper, eliminating the compound guard and loop from the primary method body. Returns `int enqueued`. | −3 CYC from primary method |
| 2 | `TriggerPumpWithFallback(string callerContext)` | Extracts the `if (!IsEmpty) { try TriggerCustomEvent ... catch ... catch ... } else { release guard }` pattern shared across `FlattenAllApexAccounts`, `ClosePositionsOnlyApexAccounts`, and `ChainNextFlattenOp` (3 identical sites). Eliminates the duplicated dual-catch + `isFlattenRunning = false` recovery. | −3 CYC from primary method; −6 CYC total across 3 call sites |

After extraction, the primary method body would be reduced to ~4 CYC (base + `EnableSIMA` guard +
`Print` calls), well within the CYC ≤ 10 threshold. Phase 2 should validate that `isFlattenRunning`
guard ownership is explicitly documented in XML-doc on `TriggerPumpWithFallback`.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~60s |
| **CYC Source** | Manual structural analysis (tool returned 0 — method not indexed at ticket creation) |
| **Manual Review Flag** | YES — CYC must be confirmed by `mcp__jcodemunch-mcp__get_symbol_complexity` once method is indexed |
