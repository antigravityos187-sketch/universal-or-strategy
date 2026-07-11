# Ticket 1 Completion — EPIC-W7-018

**epic_id:** EPIC-W7-018
**ticket_id:** T1
**helper_name:** IsGlobalCommand
**concern_extracted:** Determine whether a given action string is a global command (not instrument-specific). HashSet lookup + MOVE_TARGET StartsWith guard.
**source_file:** src/V12_002.UI.IPC.cs
**parent_method:** IsCommandForThisInstrument
**cyc_parent_before:** 38
**cyc_parent_now:** 3
**cyc_achieved:** 2
**cyc_threshold:** 8
**build_passed:** true
**tests_written:** 3

## Extraction Evidence

Helper `IsGlobalCommand(string action)` extracted at line 320 of `src/V12_002.UI.IPC.cs`.
`GlobalCommandsSet` static HashSet<string> with OrdinalIgnoreCase at lines 295-315.
`[AggressiveInlining]` annotation applied.

```csharp
[AggressiveInlining]
private static bool IsGlobalCommand(string action)
{
    return GlobalCommandsSet.Contains(action)
        || action.StartsWith("MOVE_TARGET", StringComparison.OrdinalIgnoreCase);
}
```

## DNA Compliance

- Zero lock() blocks: PASS
- ASCII-only string literals: PASS
- UTF-8 source encoding (no BOM): PASS
- CYC <= 8: PASS (helper CYC=2, parent CYC=3)
- xUnit [Fact] tests only: PASS
- Single concern per helper: PASS (global-command routing exclusively)

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-p5-lane-orch-FL-03-29 |
| Wave | 7 |
| Epic ID | EPIC-W7-018 |
| Ticket ID | T1 |
| Phase | 5 |
| Executed | 2026-06-30T02:00:00Z |
| cyc_achieved | 2 |
| build_passed | true |
