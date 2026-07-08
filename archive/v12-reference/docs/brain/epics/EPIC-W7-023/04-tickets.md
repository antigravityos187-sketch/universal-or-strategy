# EPIC-W7-023 — Phase 4: Ticket Generation

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29
**Inputs:**
- `docs/brain/EPIC-W7-023/02-architecture-plan.md`
- `docs/brain/EPIC-W7-023/03-audit-report.md`

---

## Overview

**Target Method:** `HandleFlatPositionUpdate(string acctName)`
**File:** `src/V12_002.Orders.Callbacks.Execution.cs`
**CYC Before:** 19
**CYC Target:** ≤ 8 (Jane Street strict standard)
**ticket_count:** 3
**projected_parent_cyc_after_all:** 2
**max_cyc_projected:** 7
**dna_verdict:** PASS (from Phase 3 audit — zero violations)

---

## Sequential Thinking Validation Summary

Three thoughts were used to validate the ticket breakdown:

- **Thought 1:** Confirmed ticket_count = 3. One ticket per semantic cluster, one concern per helper.
- **Thought 2:** Mapped exact lines_to_move, helper names, and per-ticket CYC delta for all three tickets.
- **Thought 3:** Verified that every new helper (CYC ≤ 7) AND the refactored parent (CYC = 2) will satisfy CYC ≤ 8 post-extraction. max_cyc_projected = 7. All pass.

---

## Ticket Definitions

---

### TICKET-1: Extract `HandleFlatPosition_SyncExpected`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-023-T1 |
| **helper_name** | `HandleFlatPosition_SyncExpected` |
| **signature** | `private void HandleFlatPosition_SyncExpected(string acctName)` |
| **concern** | Expected Position Sync Guard — decide whether to reset `expectedPositions` for the flat account. Checks `hasPendingEntry`, `hasActivePositionForAcct`, and `hasSyncPending` guards. Either skips with a `Print` log or calls `SetExpectedPositionLocked`. |
| **source_cluster** | Cluster A (lines 72–97) |
| **lines_to_move** | 72–97 (~26 lines) |
| **cyc_reduction** | −8 branches removed from parent (base absorbed into helper: `!IsNullOrEmpty`, `!hasPendingEntry`, two OR short-circuits, outer ternary, nested ternary, plus their base) |
| **projected_helper_cyc** | 7 |
| **vm_backup_reference** | `src-vm-backup/V12_002.Orders.Callbacks.Execution.cs` line 73 |

**Branch accounting for helper (CYC = 7):**

| Branch | +1 |
|---|---|
| base | 1 |
| `!string.IsNullOrEmpty(acctName)` | +1 |
| `!hasPendingEntry` | +1 |
| `hasPendingEntry \|\| hasActivePositionForAcct` (OR short-circuit 1) | +1 |
| `\|\| hasSyncPending` (OR short-circuit 2) | +1 |
| outer ternary `hasPendingEntry ? ... : (...)` | +1 |
| nested ternary `hasActivePositionForAcct ? ... : ...` | +1 |
| **Total** | **7** |

**Body outline (reference):**
```csharp
private void HandleFlatPosition_SyncExpected(string acctName)
{
    if (!string.IsNullOrEmpty(acctName))
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
}
```

**Verification steps:**
1. `mcp__jcodemunch-mcp__get_symbol_complexity` on `HandleFlatPosition_SyncExpected` → assert CYC ≤ 8
2. `dotnet build` → zero errors
3. `dotnet csharpier check src/` → zero formatting issues

---

### TICKET-2: Extract `HandleFlatPosition_ReconcileOrphans`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-023-T2 |
| **helper_name** | `HandleFlatPosition_ReconcileOrphans` |
| **signature** | `private bool HandleFlatPosition_ReconcileOrphans()` |
| **concern** | Orphan Reconciliation Early Return — detect external-close / strategy-restart condition (`activePositions.Count == 0`), trigger `ReconcileOrphanedOrders`, and return `true` to signal caller should return early. |
| **source_cluster** | Cluster B (lines 98–102) |
| **lines_to_move** | 98–102 (~5 lines) |
| **cyc_reduction** | −1 branch from parent (count == 0 check removed; parent gains +1 for `if (bool return)` — net 0 change to parent from this ticket alone, but enables clean orchestrator when all three tickets applied) |
| **projected_helper_cyc** | 2 |
| **vm_backup_reference** | `src-vm-backup/V12_002.Orders.Callbacks.Execution.cs` line 138 |

**Branch accounting for helper (CYC = 2):**

| Branch | +1 |
|---|---|
| base | 1 |
| `activePositions.Count == 0` | +1 |
| **Total** | **2** |

**Body outline (reference):**
```csharp
private bool HandleFlatPosition_ReconcileOrphans()
{
    if (activePositions.Count == 0)
    {
        Print("EXTERNAL CLOSE/RESTART DETECTED - Scanning for orphaned bracket orders...");
        ReconcileOrphanedOrders("Position went flat");
        return true;
    }
    return false;
}
```

**Verification steps:**
1. `mcp__jcodemunch-mcp__get_symbol_complexity` on `HandleFlatPosition_ReconcileOrphans` → assert CYC ≤ 8
2. `dotnet build` → zero errors
3. `dotnet csharpier check src/` → zero formatting issues

---

### TICKET-3: Extract `HandleFlatPosition_CleanupActivePositions`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-023-T3 |
| **helper_name** | `HandleFlatPosition_CleanupActivePositions` |
| **signature** | `private void HandleFlatPosition_CleanupActivePositions()` |
| **concern** | Active Position Cleanup — scan `activePositions` for orphaned filled-but-flat entries, cancel their orders via `CancelOrphanedOrdersForPosition`, collect cleanup keys, run second loop calling `CleanupPosition`, print completion if any cleanup occurred. |
| **source_cluster** | Cluster C (lines 103–120) |
| **lines_to_move** | 103–120 (~18 lines) |
| **cyc_reduction** | −6 branches removed from parent (foreach1, ContainsKey guard, EntryFilled, RemainingContracts short-circuit, foreach2, count guard) |
| **projected_helper_cyc** | 7 |
| **vm_backup_reference** | `src-vm-backup/V12_002.Orders.Callbacks.Execution.cs` line 151 |

**Branch accounting for helper (CYC = 7):**

| Branch | +1 |
|---|---|
| base | 1 |
| `foreach (var kvp in activePositions.ToArray())` | +1 |
| `!activePositions.ContainsKey(kvp.Key)` continue guard | +1 |
| `pos.EntryFilled` | +1 |
| `&& pos.RemainingContracts > 0` (short-circuit AND) | +1 |
| `foreach (string key in positionsToCleanup)` | +1 |
| `positionsToCleanup.Count > 0` | +1 |
| **Total** | **7** |

**Body outline (reference):**
```csharp
private void HandleFlatPosition_CleanupActivePositions()
{
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
}
```

**Verification steps:**
1. `mcp__jcodemunch-mcp__get_symbol_complexity` on `HandleFlatPosition_CleanupActivePositions` → assert CYC ≤ 8
2. `dotnet build` → zero errors
3. `dotnet csharpier check src/` → zero formatting issues

---

## Refactored Parent (Post All Tickets)

| Field | Value |
|---|---|
| **method** | `HandleFlatPositionUpdate(string acctName)` |
| **projected_parent_cyc_after_all** | 2 |
| **body** | 3-line orchestrator |

```csharp
private void HandleFlatPositionUpdate(string acctName) // [B967-FIX-01]
{
    HandleFlatPosition_SyncExpected(acctName);
    if (HandleFlatPosition_ReconcileOrphans())
        return;
    HandleFlatPosition_CleanupActivePositions();
}
```

**Branch accounting (CYC = 2):**

| Branch | +1 |
|---|---|
| base | 1 |
| `if (HandleFlatPosition_ReconcileOrphans())` | +1 |
| **Total** | **2** |

---

## CYC Reduction Summary

| Symbol | CYC Before | CYC After | Delta | Meets ≤ 8 |
|---|---|---|---|---|
| `HandleFlatPositionUpdate` | 19 | 2 | −17 | ✓ |
| `HandleFlatPosition_SyncExpected` | N/A (new) | 7 | — | ✓ |
| `HandleFlatPosition_ReconcileOrphans` | N/A (new) | 2 | — | ✓ |
| `HandleFlatPosition_CleanupActivePositions` | N/A (new) | 7 | — | ✓ |
| **max_cyc_projected** | — | **7** | — | ✓ |

---

## Execution Order for Phase 5

Phase 5 tickets MUST be applied in order (each builds on the previous):

1. **T1 first** — Extract `HandleFlatPosition_SyncExpected`, place before parent
2. **T2 second** — Extract `HandleFlatPosition_ReconcileOrphans`, place after T1
3. **T3 third** — Extract `HandleFlatPosition_CleanupActivePositions`, place after T2
4. **Replace parent body** — Swap `HandleFlatPositionUpdate` body with 3-line orchestrator

Reference implementation: `src-vm-backup/V12_002.Orders.Callbacks.Execution.cs` (lines 73, 138, 151).

---

## Scope Compliance Reminder

- [x] Only `HandleFlatPositionUpdate` modified (body replaced)
- [x] 3 new `private` helpers added to same partial class, same file
- [x] No public interface changes
- [x] Caller `ProcessOnPositionUpdate` untouched
- [x] No cross-file changes
- [x] V12.23 No Scope Creep: ONE EPIC = ONE CONCERN ✓

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-023 |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~30s |
| **Tools Used** | jCodemunch (resolve_repo, get_symbol_complexity, get_extraction_candidates), Sequential Thinking (3 thoughts) |
| **ticket_count** | 3 |
| **max_cyc_projected** | 7 |
| **projected_parent_cyc_after_all** | 2 |
| **dna_verdict** | PASS |
| **Input** | docs/brain/EPIC-W7-023/02-architecture-plan.md + docs/brain/EPIC-W7-023/03-audit-report.md |
| **Output** | docs/brain/EPIC-W7-023/04-tickets.md |
