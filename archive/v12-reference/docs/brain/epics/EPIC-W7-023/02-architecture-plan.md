# EPIC-W7-023 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29
**Input:** docs/brain/EPIC-W7-023/01-scope-boundary.md

---

## Overview

**Target Method:** `HandleFlatPositionUpdate(string acctName)`
**File:** `src/V12_002.Orders.Callbacks.Execution.cs`
**CYC Before:** 19 (measured by jCodemunch tooling)
**CYC Target:** ≤ 8 per Jane Street strict standard
**Extraction Count:** 3 helpers
**max_cyc_projected:** 7

---

## Complexity Analysis

### Branch Inventory (CYC=19 Validation)

| # | Branch Point | Type | Location |
|---|---|---|---|
| 1 | Base path | Base | entry |
| 2 | `!string.IsNullOrEmpty(flatAcctName)` | if-guard | L72 |
| 3 | `!hasPendingEntry` guard | if-guard | L79 |
| 4 | `hasPendingEntry \|\| hasActivePositionForAcct \|\| hasSyncPending` | compound OR | L83 |
| 5 | `\|\| hasActivePositionForAcct` | short-circuit OR | L83 |
| 6 | `\|\| hasSyncPending` | short-circuit OR | L83 |
| 7 | `hasPendingEntry ? "pending entry..." : (...)` | ternary | L84 |
| 8 | `hasActivePositionForAcct ? "activePositions..." : "dispatch sync pending"` | nested ternary | L85 |
| 9 | `activePositions.Count == 0` | early-return guard | L98 |
| 10 | `foreach (var kvp in activePositions.ToArray())` | loop | L104 |
| 11 | `!activePositions.ContainsKey(kvp.Key)` | continue guard | L106 |
| 12 | `pos.EntryFilled && pos.RemainingContracts > 0` | compound AND | L109 |
| 13 | `&& pos.RemainingContracts > 0` | short-circuit AND | L109 |
| 14 | `foreach (string key in positionsToCleanup)` | loop | L115 |
| 15 | `positionsToCleanup.Count > 0` | post-loop guard | L118 |

Total: 1 (base) + 14 branches = **CYC ≥ 15**. Tool-measured CYC = **19** (accepted as ground truth — jCodemunch McCabe path analysis counts all exception paths and logical sub-expressions).

---

## Semantic Cluster Identification

### Cluster A — Expected Position Sync Guard (lines 72–97)

**Responsibility:** Decide whether to reset `expectedPositions` for the flat account. Checks three guard conditions (pending entry, unfilled position, sync pending) and either skips or clears the expected position.

**Includes:**
- `ExpKey(flatAcctName)` call
- `IsDispatchSyncPending(flatExpKey)` call
- `HasPendingEntryOrderForAccount(flatAcctName)` call
- `HasUnfilledPositionForAccount(flatAcctName)` conditional call
- Compound boolean guard + ternary skip-reason construction
- `SetExpectedPositionLocked` or `Print` dispatch

**Jane Street Tag:** Single responsibility — "should I sync expected positions?"

---

### Cluster B — Orphan Reconciliation Early Return (lines 98–102)

**Responsibility:** Detect external-close / strategy-restart condition (`activePositions.Count == 0`) and trigger `ReconcileOrphanedOrders`, then return early. This is a guard clause pattern.

**Includes:**
- `activePositions.Count == 0` check
- `Print` call with restart message
- `ReconcileOrphanedOrders("Position went flat")` call
- `return`

**Jane Street Tag:** Defense-in-depth restart guard. Minimal CYC.

---

### Cluster C — Active Position Cleanup (lines 103–120)

**Responsibility:** Scan `activePositions`, cancel orphaned orders for filled positions that went flat, collect cleanup keys, run cleanup loop.

**Includes:**
- `List<string> positionsToCleanup` allocation
- `foreach` over `activePositions.ToArray()`
- `ContainsKey` re-validation guard
- `pos.EntryFilled && pos.RemainingContracts > 0` compound guard
- `CancelOrphanedOrdersForPosition(kvp.Key, pos)` call
- Second `foreach` for `CleanupPosition(key)`
- Final `Print` if any cleanup occurred

**Jane Street Tag:** Single responsibility — "cancel and clean up orphaned positions"

---

## Extracted Method Signatures

### Helper 1: `HandleFlatPosition_SyncExpected`

```csharp
private void HandleFlatPosition_SyncExpected(string acctName)
```

**Purpose:** Sync expected positions for an account that went flat.
**CYC Projected:** 7
**Branch breakdown:**
- `!IsNullOrEmpty(acctName)` → +1
- `!hasPendingEntry` → +1
- `hasPendingEntry || hasActivePositionForAcct || hasSyncPending` → +2
- Outer ternary (skipReason) → +1
- Nested ternary → +1
- Base → 1
- Total = **7 ≤ 8** ✓

**Body outline:**
```
if (!IsNullOrEmpty(acctName))
{
    string flatExpKey = ExpKey(acctName);
    bool hasSyncPending = IsDispatchSyncPending(flatExpKey);
    bool hasPendingEntry = HasPendingEntryOrderForAccount(acctName);
    bool hasActivePositionForAcct = false;
    if (!hasPendingEntry)
        hasActivePositionForAcct = HasUnfilledPositionForAccount(acctName);
    if (hasPendingEntry || hasActivePositionForAcct || hasSyncPending)
    {
        string skipReason = hasPendingEntry ? "pending entry in flight"
            : (hasActivePositionForAcct ? "activePositions metadata present" : "dispatch sync pending");
        Print($"[OnPositionUpdate] H-14 SKIP: {flatExpKey} broker=Flat but {skipReason} -- not resetting expectedPositions");
    }
    else
    {
        SetExpectedPositionLocked(flatExpKey, 0);
        Print($"[OnPositionUpdate] expectedPositions cleared for {flatExpKey} (position flat)");
    }
}
```

---

### Helper 2: `HandleFlatPosition_ReconcileOrphans`

```csharp
private bool HandleFlatPosition_ReconcileOrphans()
```

**Purpose:** Detect external-close/restart (empty `activePositions`) and reconcile orphaned orders. Returns `true` if caller should return early.
**CYC Projected:** 2
**Branch breakdown:**
- `activePositions.Count == 0` → +1
- Base → 1
- Total = **2 ≤ 8** ✓

**Body outline:**
```
if (activePositions.Count == 0)
{
    Print("EXTERNAL CLOSE/RESTART DETECTED - Scanning for orphaned bracket orders...");
    ReconcileOrphanedOrders("Position went flat");
    return true;
}
return false;
```

---

### Helper 3: `HandleFlatPosition_CleanupActivePositions`

```csharp
private void HandleFlatPosition_CleanupActivePositions()
```

**Purpose:** Scan active positions for orphaned filled-but-flat entries, cancel their orders, and clean up.
**CYC Projected:** 7
**Branch breakdown:**
- `foreach` loop 1 → +1
- `!activePositions.ContainsKey(kvp.Key)` → +1
- `pos.EntryFilled` → +1
- `&& pos.RemainingContracts > 0` → +1
- `foreach` loop 2 → +1
- `positionsToCleanup.Count > 0` → +1
- Base → 1
- Total = **7 ≤ 8** ✓

**Body outline:**
```
List<string> positionsToCleanup = new List<string>();
foreach (var kvp in activePositions.ToArray())
{
    if (!activePositions.ContainsKey(kvp.Key))
        continue;
    PositionInfo pos = kvp.Value;
    if (pos.EntryFilled && pos.RemainingContracts > 0)
    {
        Print("EXTERNAL CLOSE DETECTED - Position went flat. Cancelling orphaned orders...");
        CancelOrphanedOrdersForPosition(kvp.Key, pos);
        positionsToCleanup.Add(kvp.Key);
    }
}
foreach (string key in positionsToCleanup)
    CleanupPosition(key);
if (positionsToCleanup.Count > 0)
    Print("Cleanup complete - Strategy still running, ready for new entries.");
```

---

### Refactored Parent: `HandleFlatPositionUpdate`

```csharp
private void HandleFlatPositionUpdate(string acctName) // [B967-FIX-01]
```

**CYC Projected:** 2
**Branch breakdown:**
- `if (HandleFlatPosition_ReconcileOrphans())` → +1
- Base → 1
- Total = **2 ≤ 8** ✓

**Body outline:**
```
HandleFlatPosition_SyncExpected(acctName);
if (HandleFlatPosition_ReconcileOrphans())
    return;
HandleFlatPosition_CleanupActivePositions();
```

---

## CYC Reduction Summary

| Symbol | CYC Before | CYC After | Delta | Meets Target |
|--------|------------|-----------|-------|---|
| `HandleFlatPositionUpdate` | 19 | 2 | -17 | ✓ |
| `HandleFlatPosition_SyncExpected` | N/A | 7 | new | ✓ |
| `HandleFlatPosition_ReconcileOrphans` | N/A | 2 | new | ✓ |
| `HandleFlatPosition_CleanupActivePositions` | N/A | 7 | new | ✓ |
| **max_cyc_projected** | — | **7** | — | ✓ |

---

## Jane Street KB Alignment

| Rule | Applied |
|---|---|
| `carl_cook`: zero-alloc hot path | `HandleFlatPosition_SyncExpected` and `ReconcileOrphans` allocate nothing. `CleanupActivePositions` retains `List<string>` on cold path (acceptable — external-close is rare) |
| `carl_cook`: extract cold logging out-of-line | All `Print()` calls remain in helpers — they are cold-path diagnostics |
| `carl_cook`: avoid LINQ | No LINQ used. `.ToArray()` on `activePositions` preserved from original |
| `gjengset`: no new `lock()` blocks | Zero new lock blocks introduced |
| `gjengset`: volatile + MemoryBarrier | State access patterns unchanged; no new volatile concerns |
| `trading_billions`: single responsibility | Each helper has exactly one job (sync, reconcile, cleanup) |
| `trading_billions`: CYC ≤ 8 per helper | All ≤ 8 ✓ |
| `trading_billions`: defense in depth | `ReconcileOrphans` preserves restart-detection guard pattern |

---

## Blueprint Reference

The vm-backup copy at `src-vm-backup/V12_002.Orders.Callbacks.Execution.cs` already contains this exact 3-helper decomposition:
- `HandleFlatPosition_SyncExpected` (line 73)
- `HandleFlatPosition_ReconcileOrphans` (line 138)
- `HandleFlatPosition_CleanupActivePositions` (line 151)

The Phase 5 implementation engineer should use the vm-backup as a direct reference. The task is to bring `src/` into alignment with `src-vm-backup/`.

---

## Execution Plan (for Phase 5)

1. **Read** `src/V12_002.Orders.Callbacks.Execution.cs` lines 69–128 (current `HandleFlatPositionUpdate`)
2. **Extract** `HandleFlatPosition_SyncExpected` — move cluster A code into new private method
3. **Extract** `HandleFlatPosition_ReconcileOrphans` — move cluster B code into new private method returning `bool`
4. **Extract** `HandleFlatPosition_CleanupActivePositions` — move cluster C code into new private method
5. **Replace** body of `HandleFlatPositionUpdate` with 3-line orchestrator (see refactored parent above)
6. **Verify** CYC via `mcp__jcodemunch-mcp__get_symbol_complexity` on all 4 methods
7. **Build** with `dotnet build` — zero errors required
8. **Run** `dotnet csharpier check src/` — zero formatting issues

---

## Scope Compliance

Per `01-scope-boundary.md` (boundary_verdict: PASS):
- [x] Only `HandleFlatPositionUpdate` modified
- [x] 3 new private helpers added to same partial class — same file
- [x] No public interface changes
- [x] Caller `ProcessOnPositionUpdate` untouched
- [x] No cross-file changes

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Phase** | 2 |
| **Wave** | 7 |
| **Bobcoins Used** | 1.0 |
| **Tools Used** | jCodemunch (resolve_repo, search_symbols, get_symbol_complexity, get_symbol_source, get_call_hierarchy, get_dependency_graph), Sequential Thinking (3 thoughts) |
| **max_cyc_projected** | 7 |
| **extraction_count** | 3 |
| **Scope Boundary Input** | docs/brain/EPIC-W7-023/01-scope-boundary.md |
| **Output** | docs/brain/EPIC-W7-023/02-architecture-plan.md |
