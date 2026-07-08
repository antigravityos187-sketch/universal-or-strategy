# EPIC-W7-052 Ticket 1 — RemoveStalePendingEntry: Completion

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **helper_name** | `RemoveStalePendingEntry` |
| **concern** | Atomic TryRemove from pendingStopReplacements + Interlocked.Decrement + Print log |
| **cyc_achieved** | 2 |
| **build_passed** | true |
| **status** | COMPLETE |

## Implementation

Added `private bool RemoveStalePendingEntry(string key, out PendingStopReplacement pending)`:
- Calls `pendingStopReplacements.TryRemove(key, out pending)` — returns false immediately on miss
- `Interlocked.Decrement(ref pendingReplacementCount)` — lock-free counter update
- Print diagnostic log (ASCII-only)
- Returns true on successful removal

`out` parameter makes it impossible to reference a `pending` that was never successfully removed — illegal state unrepresentable.

Parent `CleanupStalePendingReplacements` calls: `if (RemoveStalePendingEntry(kvp.Key, out var pending))`

## DNA Checks

- [x] Zero lock() blocks
- [x] ASCII-only Print format string
- [x] CYC = 2 (base + TryRemove branch)
- [x] UTF-8 no BOM
- [x] Build passes
