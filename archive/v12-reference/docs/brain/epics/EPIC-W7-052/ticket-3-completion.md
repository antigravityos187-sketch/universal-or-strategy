# EPIC-W7-052 Ticket 3 — ScheduleBracketRestoration: Completion

| Field | Value |
|---|---|
| **ticket_id** | T3 |
| **helper_name** | `ScheduleBracketRestoration` |
| **concern** | Guard bracket restoration flag and dispatch TriggerCustomEvent closure |
| **cyc_achieved** | 3 |
| **build_passed** | true |
| **status** | COMPLETE |

## Implementation

Added `private void ScheduleBracketRestoration(string key, PendingStopReplacement pending)`:
- Guard: `if (pending.BracketRestorationNeeded && pending.CapturedTargets != null)`
- Hoists `_tSnap = pending.CapturedTargets` and `_tKey = key` as named locals
- Dispatches: `TriggerCustomEvent(o => RestoreCascadedTargets(_tKey, _tSnap), null)`

Key fix: loop-local variable capture eliminated. Previously `kvp.Key` was captured by lambda — after extraction, `key` and `pending` are named method parameters, preventing the undefined-behavior class of loop-closure capture.

## DNA Checks

- [x] Zero lock() blocks
- [x] ASCII-only
- [x] CYC = 3 (base + BracketRestorationNeeded + CapturedTargets null)
- [x] No loop-local variable capture in TriggerCustomEvent lambda
- [x] Build passes
