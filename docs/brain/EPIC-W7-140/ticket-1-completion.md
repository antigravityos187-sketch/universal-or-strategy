# EPIC-W7-140 Ticket 1 — TrySnapshotReplacementTargets / TryEnqueuePendingReplacement: Completion

| Field | Value |
|---|---|
| **ticket_id** | 1 |
| **helper_name** | `TryEnqueuePendingReplacement` + `BuildReplacementSnapshot` |
| **concern** | Snapshot target orders + enqueue pending replacement record with circuit-breaker |
| **cyc_achieved** | 4 |
| **build_passed** | true |
| **status** | COMPLETE |

## Implementation

`BuildReplacementSnapshot(string entryName)` — CYC=7:
- Iterates _tB=1..5, applies 4-clause compound guard, accumulates TargetSnapshot list
- Returns array or null

`TryEnqueuePendingReplacement(...)` — CYC=4:
- Calls BuildReplacementSnapshot, builds PendingStopReplacement record
- TryAdd to pendingStopReplacements (Actor/Enqueue pattern)
- Interlocked.Increment + circuit-breaker threshold check
- Returns bool — duplicate-key path now explicit instead of silently swallowed

## DNA Checks

- [x] Zero lock() blocks (ConcurrentDictionary.TryAdd + Interlocked.Increment)
- [x] ASCII-only
- [x] CYC = 4 (base + TryAdd + threshold + circuitBreakerActive)
- [x] Build passes
