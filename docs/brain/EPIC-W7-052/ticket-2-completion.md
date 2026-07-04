# EPIC-W7-052 Ticket 2 — RecoverStopForStaleEntry: Completion

| Field | Value |
|---|---|
| **ticket_id** | T2 |
| **helper_name** | `RecoverStopForStaleEntry` |
| **concern** | Recovery orchestration — validate position active/filled/has-contracts, call CreateNewStopOrder, delegate bracket restoration |
| **cyc_achieved** | 4 |
| **build_passed** | true |
| **status** | COMPLETE |

## Implementation

Added `private void RecoverStopForStaleEntry(string key, PendingStopReplacement pending)`:
- Guard 1: `if (!activePositions.TryGetValue(key, out var pos)) return;`
- Guard 2: `if (!pos.EntryFilled) return;`
- Guard 3: `if (pos.RemainingContracts <= 0) return;`
- Calls `CreateNewStopOrder(key, replacementQty, pending.StopPrice, pending.Direction, isRecovery: true)`
- Delegates to `ScheduleBracketRestoration(key, pending)` — no inline bracket logic

Three early-return guard clauses = Jane Street defense-in-depth. No loop-variable capture.

## DNA Checks

- [x] Zero lock() blocks
- [x] ASCII-only
- [x] CYC = 4 (base + 3 guards)
- [x] ScheduleBracketRestoration called from within helper (not from parent)
- [x] Build passes
