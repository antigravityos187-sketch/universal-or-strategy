# Ticket 2 Completion: EPIC-W7-159

## Agent Tracking
- **epic_id**: EPIC-W7-159
- **ticket_id**: ticket-2
- **lane**: FL-26
- **source_file**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **phase**: 5 (Ticket Execution)
- **agent**: V12 Photon Engineer (v12-engineer)
- **completed_at**: 2026-06-29

## Work Summary
Extracted `CalculateSIMAEntryQty` helper from `TryHandleFleet_LongShort` SIMA path.

## Changes
- Added `CalculateSIMAEntryQty()` method
- Extracts ATR stop calculation, fallback to MinimumStop, and quantity sizing logic
- Includes try/catch fallback per original exempt table (empty catch with fallback value)

## Metrics
| Metric | Before | After |
|--------|--------|-------|
| `TryHandleFleet_LongShort` CYC | 11 | 7 |
| `CalculateSIMAEntryQty` CYC | N/A | 3 |

## Verification
- **status**: success
- **cyc_achieved**: 7
- **build_passed**: true
- **lock_check**: 0 matches
- **ascii_check**: pass
