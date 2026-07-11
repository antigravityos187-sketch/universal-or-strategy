# EPIC-W7-139 Ticket 1 — IsStalePendingReplacement: Completion

| Field | Value |
|---|---|
| **ticket_id** | 1 |
| **helper_name** | `IsStalePendingReplacement` |
| **concern** | Staleness detection — pendingStopReplacements.TryGetValue + DateTime age arithmetic + threshold comparison |
| **cyc_achieved** | 2 |
| **build_passed** | true |
| **status** | COMPLETE |

## Implementation

Added `private bool IsStalePendingReplacement(string entryName)`:
- `if (!pendingStopReplacements.TryGetValue(entryName, out var pendingRecord)) return false;`
- `double pendingAgeSeconds = (DateTime.UtcNow - pendingRecord.CreatedTime).TotalSeconds;`
- `return pendingAgeSeconds > STALE_PENDING_FAST_PATH_SEC;`

Pure predicate: read-only, zero allocation, no side effects.
