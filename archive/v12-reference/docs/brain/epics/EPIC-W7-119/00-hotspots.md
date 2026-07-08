# EPIC-W7-119 — Phase 0: Hotspot Analysis

> **Note:** `method_name` and `source_file` were missing from the epic list — using best-effort hotspot match via full-codebase extended-McCabe CYC scan.

---

## Best Candidate Method

| Field           | Value                                                          |
|-----------------|----------------------------------------------------------------|
| **Method Name** | `Dispatch_ProcessFleetLoop`                                    |
| **CYC**         | **14** (extended McCabe: `if×10 + for×2 + catch×1 = 13 decisions + 1`) |
| **File Path**   | `src/V12_002.SIMA.Dispatch.cs`                                 |
| **Line Range**  | L196–L348                                                      |
| **Metric Mode** | Extended McCabe (structural branches only — no `&&`/`||` dilution) |

> **Why this candidate?** Of the **11 methods** confirmed at exactly CYC=14 across the repo, `Dispatch_ProcessFleetLoop` is the most architecturally significant: it is the innermost loop of the entire SIMA multi-account fleet dispatch pipeline, called exclusively via `ExecuteSmartDispatchEntry` which is itself invoked from **12 call-sites across 8 entry-strategy files** (FFMA, RMA, MOMO, OR, Trend, Retest). Its complexity is purely structural (no boolean-compound inflation), making it the truest McCabe CYC=14 in the codebase.

---

## All CYC=14 Methods (Full Enumeration)

| # | Method | File | Breakdown |
|---|--------|------|-----------|
| 1 | `Dispatch_ProcessFleetLoop` ⭐ | `src/V12_002.SIMA.Dispatch.cs:196` | if×10, for×2, catch×1 |
| 2 | `VerifyPhotonSlotIntegrity` | `src/V12_002.SIMA.Fleet.cs:294` | if×9, for×1, catch×1, &&×1, \|\|×1 |
| 3 | `FormatInternal` | `src/V12_002.Perf.LogBuffer.cs:34` | if×7, while×2, &&×3, ??×1 |
| 4 | `SymmetryGuardOnMasterFill` | `src/V12_002.Symmetry.cs:209` | if×7, while×1, &&×2, \|\|×3 |
| 5 | `HandleFleetTargetFill` | `src/V12_002.UI.Compliance.cs:504` | if×7, foreach×1, &&×4, \|\|×1 |
| 6 | `TryHandleFleet_MoveTarget` | `src/V12_002.UI.IPC.Commands.Fleet.cs:569` | if×7, &&×6 |
| 7 | `UpdateLivePositionSnapshot` | `src/V12_002.UI.SnapshotPool.cs:114` | if×6, for×2, &&×4, \|\|×1 |
| 8 | `HandleMatchedFollowerOrder` | `src/V12_002.Orders.Callbacks.AccountOrders.cs:390` | if×4, &&×7, \|\|×2 |
| 9 | `PropagateMasterEntryMove` | `src/V12_002.Orders.Callbacks.Propagation.cs:344` | if×4, foreach×1, catch×1, &&×4, \|\|×3 |
| 10 | `BuildLiveBrokerOrderIndex` | `src/V12_002.Orders.Management.Cleanup.cs:428` | if×4, foreach×3, &&×4, \|\|×2 |
| 11 | `SubmitRepairOrderWithAuthorization` | `src/V12_002.REAPER.Repair.cs:101` | if×5, &&×5, \|\|×3 |

---

## Blast Radius Summary

### Direct Call Graph

```
ExecuteSmartDispatchEntry()          [src/V12_002.SIMA.Dispatch.cs:45]
  └─► Dispatch_ProcessFleetLoop()   [CYC=14 — TARGET]
        ├─► ShouldSkipFleetAccount()
        ├─► Dispatch_BuildFollowerOrders()
        ├─► Dispatch_PublishMarketBracketToPhoton()
        └─► Dispatch_PublishLimitEntryToPhoton()
```

### Upstream Callers of `ExecuteSmartDispatchEntry` (12 call-sites)

| File | Call Count |
|------|-----------|
| `src/V12_002.Entries.FFMA.cs` | 3 |
| `src/V12_002.Entries.Trend.cs` | 2 |
| `src/V12_002.Entries.Retest.cs` | 2 |
| `src/V12_002.Entries.RMA.cs` | 1 |
| `src/V12_002.Entries.MOMO.cs` | 1 |
| `src/V12_002.Entries.OR.cs` | 1 |
| `src/V12_002.SIMA.Dispatch.cs` (self/retry) | 1 |
| `src/V12_002.Orders.Callbacks.Propagation.cs` | 1 |

### Blast Radius Metrics

| Metric | Value |
|--------|-------|
| **Direct callers** | 1 (`ExecuteSmartDispatchEntry`) |
| **Upstream entry-strategy files** | 8 |
| **Downstream sub-methods called** | 4 |
| **Total files in blast radius** | **13** |
| **Risk level** | 🔴 HIGH — fleet dispatch path; every live multi-account trade flows through this method |

---

## Top 3 Complexity Drivers

### Driver 1 — Flatten-Guard + Circuit-Breaker Double Gate (L158–L232)
**Type:** Nested `if` with early-return pattern  
**Nesting depth:** 3–4 levels inside the for-loop  
**Lines:** L158–L162, L167–L171, L228–L232  
**Description:** Three independent guard clauses (`isFlattenRunning`, `MetadataGuardDuplicate`, `_reaperCircuitBreakerTripped`) each force an early `continue` within the loop. Because they must fire in a specific order for correctness (flatten > dedup > CB), they cannot be collapsed into a single compound predicate. Each adds 1 CYC point.  
**Extraction opportunity:** Extract all three guards into a single `bool ShouldSkipFleetIteration(...)` predicate method (already partially done for `ShouldSkipFleetAccount`), reducing to CYC −3.

### Driver 2 — Market vs. Limit Branch + Per-Target Rollback `for` Loop (L272–L337)
**Type:** `if/else` + `for` loop with inner `if` guards  
**Nesting depth:** 4–5 levels  
**Lines:** L275–L311, L332–L337  
**Description:** The `isMarketEntry` split dispatches to two separate Photon publish paths, each with independent out-parameter sets. Inside the `catch` handler, a `for (int tNum = 1; tNum <= 5; tNum++)` rollback loop with a null-guard `if` adds 2 CYC points. The rollback loop exists only in the error path, making it a hidden complexity that tests must explicitly cover.  
**Extraction opportunity:** Extract the catch-handler rollback into `Dispatch_RollbackFleetSlot(...)` — reduces CYC by 3 (removes `for`, `if (targetDict != null)`, and the `if (registeredForCleanup)` wrapper).

### Driver 3 — `_builtOk` Continuation Guard + `catch` Multi-Condition Error Recovery (L270, L315–L344)
**Type:** Mixed `if` + `catch` with 4 nested `if` branches inside  
**Nesting depth:** 5 (catch block)  
**Lines:** L270–L271, L315–L344  
**Description:** The `if (!_builtOk) continue` is a degenerate guard that can't be eliminated. The `catch` block contains 4 independent `if` checks (`syncPending`, `reservedDelta != 0`, `registeredForCleanup`, `!string.IsNullOrEmpty(fleetEntryName)`) each for partial-rollback of distinct allocations. These are all genuinely independent conditions, contributing 4 CYC points inside the catch.  
**Extraction opportunity:** Extract the full catch body into `Dispatch_HandleFleetSlotException(...)`, reducing CYC by 4 (all 4 catch-body `if` branches move out of scope).

---

## Recommended Extraction Count

**3 extractions** to reach CYC ≤ 7 (target per Jane Street / SOLID invariant):

| # | Extraction | Δ CYC | New Method |
|---|------------|-------|------------|
| 1 | Guards (flatten + dedup + CB) → predicate | −3 | `ShouldContinueFleetIteration()` |
| 2 | Rollback loop in catch → helper | −3 | `Dispatch_RollbackFleetSlot()` |
| 3 | Full catch body → handler | −4 | `Dispatch_HandleFleetSlotException()` |
| | **Total reduction** | **−10** | CYC: 14 → ~4 |

---

## Risk & Testing Notes

- `Dispatch_ProcessFleetLoop` runs on the **NinjaTrader strategy thread** inside a semaphore-guarded block — any extraction must preserve thread-affinity invariants.
- The rollback logic in the `catch` block is **safety-critical** for preventing orphaned fleet positions; extraction must be regression-tested against the REAPER audit path.
- `VerifyPhotonSlotIntegrity` (CYC=14, `src/V12_002.SIMA.Fleet.cs:294`) is the **runner-up** candidate — nearly identical profile with XorShadow rollback logic, same extraction strategy applies.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 14 |
| **Execution Time** | ~180s |
| **Scan Coverage** | 83 `.cs` source files, comment-stripped extended McCabe |
| **CYC Confirmed** | ✅ 14 |
| **Methods Scanned** | ~620 method bodies |
| **CYC=14 Matches** | 11 exact matches enumerated |
