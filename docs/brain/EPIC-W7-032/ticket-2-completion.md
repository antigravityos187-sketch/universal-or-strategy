# EPIC-W7-032 Ticket 2 Completion

ticket_id: T2
helper_name: ShouldRestoreTarget
epic_id: EPIC-W7-032
method: RestoreCascadedTargets
file: src/V12_002.Orders.Management.StopSync.cs
build_passed: true
cyc_gate: PASS
cyc_gate_output: "CYC_GATE: PASS  EPIC-W7-032  RestoreCascadedTargets  CYC=7"
cyc_achieved: 7
final_cyc: 7
wave_ready: true
status: completed

## Summary

`ShouldRestoreTarget` already existed as a `private static bool` helper at line 1005, extracted and
correctly called inside the `foreach` in `RestoreCascadedTargets` (line 1068).

The helper satisfies all ticket spec requirements:
- Null guard on `snap` -> returns false
- Null guard on `snap.CapturedOrder` -> returns false
- Returns true only for `OrderState.Cancelled` or `OrderState.Rejected`
- Any other state (Filled, Working, etc.) falls through to `return false`

## Gate Results

- **dotnet csharpier format src/**: Formatted 83 files in 755ms
- **dotnet build Linting.csproj**: Build succeeded — 0 Warning(s), 0 Error(s)
- **CYC gate**: CYC_GATE: PASS  EPIC-W7-032  RestoreCascadedTargets  CYC=7

## DNA Compliance

- No lock() blocks introduced
- ASCII-only string literals
- Single concern: pure predicate, no instance state
- Helper in same class as parent method
