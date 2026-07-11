# EPIC-W7-099 | Phase 0 — Hotspot Analysis

**Wave:** 7 | **Phase:** 0 | **Status:** Completed  
**Source File:** `src/V12_002.Orders.Management.Cleanup.cs`  
**Target Method:** `PurgePositionIfEligible`  
**CYC (Cyclomatic Complexity):** 11  
**Date:** 2025-07-17

---

## 1. Symbol Under Analysis

| Attribute        | Value                                              |
|------------------|----------------------------------------------------|
| Method           | `PurgePositionIfEligible(string, int)`             |
| Containing Type  | `partial class V12_002 : Strategy`                 |
| Namespace        | `NinjaTrader.NinjaScript.Strategies`               |
| Lines            | 207–243 (37 LOC)                                   |
| Visibility       | `private`                                          |
| CYC Score        | **11**                                             |
| Decision Nodes   | 10 branching points (if, &&, lambda predicate)     |

---

## 2. Cyclomatic Complexity Breakdown

The method contains two logically independent blocks, each gated by a compound boolean:

### Block A — Primary Purge (lines 211–217)
```
if (followerExpected == 0                         // +1
    && !HasActiveOrPendingOrderForEntry(entryName) // +1
{
    if (removed)                                   // +1
        SymmetryGuardForgetEntry(entryName);
}
```

### Block B — FIX-ZP-02 Secondary Follower Purge (lines 222–242)
```
if (followerExpected == 0                          // +1
    && activePositions.TryGetValue(...)            // +1
    && followerCheck.IsFollower                    // +1
    && followerCheck.ExecutingAccount != null      // +1
{
    FirstOrDefault(p => p.Instrument == Instrument) // +1 (lambda)
    if (brokerPos != null                          // +1
        && brokerPos.MarketPosition == Flat)       // +1
    {
        if (removedFZP)                            // +1
    }
}
```

**CYC = 1 (base) + 10 decision nodes = 11**

---

## 3. Hotspot Risk Assessment

| Risk Dimension        | Finding                                                                                 | Severity |
|-----------------------|-----------------------------------------------------------------------------------------|----------|
| **Dual-path logic**   | Two independent purge paths with overlapping guard conditions create hidden coupling.   | HIGH     |
| **Compound guards**   | FIX-ZP-02 block uses 4-clause `&&` guard — any clause order change alters semantics.    | HIGH     |
| **Silent no-ops**     | If `followerExpected != 0`, both blocks are entirely skipped with no trace/log.         | MEDIUM   |
| **Broker LINQ scan**  | `Positions.FirstOrDefault(...)` is a linear scan over broker account — called per purge on the NinjaTrader UI thread. | MEDIUM |
| **Duplicate guard**   | `followerExpected == 0` is tested twice (line 211, line 222) — Block A removal does not prevent Block B execution, causing a second TryRemove attempt on an already-removed key. | MEDIUM |
| **Missing null check**| `followerCheck.ExecutingAccount.Positions` — `.Positions` could be null if account is disconnected mid-purge. | LOW-MEDIUM |
| **No test coverage**  | Both FIX-ZP-02 and META-GUARD paths depend on live broker state, making unit isolation impossible without mocking. | HIGH |

---

## 4. Blast Radius

### Direct Callers (methods that invoke `PurgePositionIfEligible`)

| Caller                | File                                          | Line |
|-----------------------|-----------------------------------------------|------|
| `CleanupPosition`     | `V12_002.Orders.Management.Cleanup.cs`        | 78   |

### Callers of `CleanupPosition` (transitive blast)

| Caller                        | File                                              | Line |
|-------------------------------|---------------------------------------------------|------|
| `SyncPositionState` (Flatten) | `V12_002.Orders.Management.Flatten.cs`            | 56   |
| `OnOrderUpdate` (Callbacks)   | `V12_002.Orders.Callbacks.cs`                     | 518, 539, 650, 774 |
| `OnExecutionUpdate`           | `V12_002.Orders.Callbacks.Execution.cs`           | 124  |
| `OnAccountItemUpdate`         | `V12_002.Orders.Callbacks.AccountOrders.cs`       | 939  |
| `ProcessSymmetryReplacement`  | `V12_002.Symmetry.Replace.cs`                     | 130  |
| `IPC FleetCommand handler`    | `V12_002.UI.IPC.Commands.Fleet.cs`                | 356  |

### Shared State Mutated

| State Object             | Impact                                                                     |
|--------------------------|----------------------------------------------------------------------------|
| `activePositions`        | `TryRemove` called up to **twice** per invocation (Block A + Block B).     |
| `SymmetryGuardForgetEntry` | Called up to twice; symmetric side-effects must be idempotent.           |
| `expectedPositions`      | Read-only within this method; populated by SIMA/REAPER subsystems.         |
| Broker `Positions` list  | Read-only LINQ scan; no mutation, but incurs broker thread-crossing cost.  |

### Downstream Subsystems at Risk

- **REAPER Repair Hook** (`V12_002.REAPER.Repair.cs`) — depends on `activePositions` being intact when `expectedPositions != 0`; premature purge here would cause REAPER to attempt repair on a now-absent entry.
- **SymmetryGuard** (`V12_002.Symmetry.cs`, `V12_002.Symmetry.Replace.cs`) — double-call to `SymmetryGuardForgetEntry` could corrupt symmetry tracking if not idempotent.
- **SIMA Fleet** (`V12_002.SIMA.Fleet.cs`, `V12_002.SIMA.Execution.cs`) — follower lifecycle depends on `activePositions` being present until broker confirms flat.
- **UI Snapshot / Compliance** (`V12_002.UI.Snapshot.cs`, `V12_002.UI.Compliance.cs`) — reads `activePositions` for display; race-window if purge races with UI refresh.

---

## 5. Refactoring Recommendations

1. **Extract `IsMetaGuardActive(string entryName)`** — centralize the `followerExpected == 0` + `IsFollower` + `ExecutingAccount != null` guard into a single readable predicate. This eliminates the repeated compound guard pattern and reduces CYC by ~3.

2. **Merge Block A and Block B** into a single conditional tree to prevent the double-`TryRemove` scenario. Block B should only execute if Block A did NOT remove the entry (i.e., guard with `activePositions.ContainsKey` after Block A).

3. **Add a null guard on `ExecutingAccount.Positions`** before calling `FirstOrDefault` to prevent NullReferenceException during account disconnection.

4. **Add a `Print` trace on the META-GUARD skip path** (when `followerExpected != 0`) to provide observability — currently both blocks silently no-op.

5. **Target CYC ≤ 5** post-refactor, matching the pattern already established for `RemoveGhostOrderRef` (refactored from CYC 37 → 5 using dispatcher sub-methods in Phase 7).

---

## 6. Phase 0 Summary

| Item                        | Value                                             |
|-----------------------------|---------------------------------------------------|
| CYC Confirmed               | **11**                                            |
| Hotspot Tier                | Tier 1 (CYC ≥ 10, multi-subsystem blast radius)   |
| Recommended Refactor Phase  | Phase 1 (decompose guards + merge purge paths)    |
| Estimated CYC Post-Refactor | 4–5                                               |
| Blocking Issues             | None — analysis only; no production change made   |
| Output Artifact             | `docs/brain/EPIC-W7-099/00-hotspots.md`           |
