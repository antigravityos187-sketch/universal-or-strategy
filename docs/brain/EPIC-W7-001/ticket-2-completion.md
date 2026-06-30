# Ticket 2 Completion -- EPIC-W7-001

**epic_id:** EPIC-W7-001
**ticket_id:** T2
**helper_name:** HasAnyActiveState
**concern_extracted:** Pure 3-condition OR predicate -- at least one of FSM / position / dispatch-pending is active
**source_file:** src/V12_002.SIMA.Fleet.cs
**parent_method:** LogHealthCheckResult
**cyc_achieved:** 4
**build_passed:** true
**tests_written:** 4
**decorator:** AggressiveInlining

## Method Signature

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool HasAnyActiveState(
    bool hasActiveFsm,
    bool hasActivePosition,
    bool hasDispatchPending)
{
    return hasActiveFsm || hasActivePosition || hasDispatchPending;
}
```

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | wave7-phase5-lane-FL-04 |
| Wave | 7 |
| Epic ID | EPIC-W7-001 |
| Ticket ID | T2 |
| Phase | 5 |
| cyc_achieved | 4 |
| build_passed | true |
| tests_written | 4 |
