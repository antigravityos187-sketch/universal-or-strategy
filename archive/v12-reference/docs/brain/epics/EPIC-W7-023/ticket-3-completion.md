# Ticket 3 Completion -- EPIC-W7-023

**epic_id:** EPIC-W7-023
**ticket_id:** T3
**helper_name:** HandleFlatPosition_CleanupActivePositions
**concern_extracted:** Active Position Cleanup -- scan activePositions for orphaned filled-but-flat entries, cancel their orders via CancelOrphanedOrdersForPosition, collect cleanup keys, run second loop calling CleanupPosition, print completion if any cleanup occurred.
**source_file:** src/V12_002.Orders.Callbacks.Execution.cs
**parent_method:** HandleFlatPositionUpdate
**cyc_achieved:** 7
**build_passed:** true
**tests_written:** 4

## Method Signature

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

## CYC Branch Accounting

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

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | wave7-phase5-lane-FL-09 |
| Wave | 7 |
| Epic ID | EPIC-W7-023 |
| Ticket ID | T3 |
| Phase | 5 |
| cyc_achieved | 7 |
| build_passed | true |
| tests_written | 4 |
