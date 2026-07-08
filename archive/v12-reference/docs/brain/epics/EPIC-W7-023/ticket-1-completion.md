# Ticket 1 Completion -- EPIC-W7-023

**epic_id:** EPIC-W7-023
**ticket_id:** T1
**helper_name:** HandleFlatPosition_SyncExpected
**concern_extracted:** Expected Position Sync Guard -- decide whether to reset expectedPositions for the flat account. Checks hasPendingEntry, hasActivePositionForAcct, and hasSyncPending guards. Either skips with Print log or calls SetExpectedPositionLocked.
**source_file:** src/V12_002.Orders.Callbacks.Execution.cs
**parent_method:** HandleFlatPositionUpdate
**cyc_achieved:** 7
**build_passed:** true
**tests_written:** 5

## Method Signature

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
            string skipReason = hasPendingEntry
                ? "pending entry in flight"
                : (hasActivePositionForAcct ? "activePositions metadata present" : "dispatch sync pending");
            Print(
                $"[OnPositionUpdate] H-14 SKIP: {flatExpKey} broker=Flat but {skipReason} -- not resetting expectedPositions"
            );
        }
        else
        {
            SetExpectedPositionLocked(flatExpKey, 0);
            Print($"[OnPositionUpdate] expectedPositions cleared for {flatExpKey} (position flat)");
        }
    }
}
```

## CYC Branch Accounting

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

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | wave7-phase5-lane-FL-09 |
| Wave | 7 |
| Epic ID | EPIC-W7-023 |
| Ticket ID | T1 |
| Phase | 5 |
| cyc_achieved | 7 |
| build_passed | true |
| tests_written | 5 |
