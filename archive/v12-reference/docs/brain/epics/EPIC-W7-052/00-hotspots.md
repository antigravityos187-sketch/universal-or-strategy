# EPIC-W7-052 · Phase 0 — Hotspot Analysis

## Target Method

| Field            | Value                                                   |
|------------------|---------------------------------------------------------|
| **Method**       | `CleanupStalePendingReplacements`                       |
| **CYC**          | 11                                                      |
| **File**         | `src/V12_002.Trailing.StopUpdate.cs`                    |
| **Class**        | `V12_002` (partial) — `NinjaTrader.NinjaScript.Strategies` |
| **Visibility**   | `private void`                                          |
| **Called from**  | `src/V12_002.Trailing.cs:222` inside `ManageTrailingStops` hot-path |

---

## Cyclomatic Complexity Breakdown (CYC = 11)

The method contains **10 branch points** (+1 base), yielding CYC 11:

| # | Branch / Decision Point |
|---|-------------------------|
| 1 | `foreach` loop over `pendingStopReplacements.ToArray()` |
| 2 | `if (now - kvp.Value.CreatedTime).TotalSeconds > 5` — staleness gate |
| 3 | `if (pendingStopReplacements.TryRemove(...))` — atomic remove success |
| 4 | `if (activePositions.TryGetValue(kvp.Key, out var pos))` — position existence |
| 5 | `&& pos.EntryFilled` — fill guard |
| 6 | `&& pos.RemainingContracts > 0` — non-zero qty guard |
| 7 | `CreateNewStopOrder(...)` call path (isRecovery branch implicitly opens downstream CYC) |
| 8 | `if (pending.BracketRestorationNeeded ...)` — bracket gate |
| 9 | `&& pending.CapturedTargets != null` — null-target guard |
| 10 | `TriggerCustomEvent(...)` closure dispatch |

---

## Top 3 Complexity Drivers

### 1 · Nested Position-Existence Check Inside Staleness Removal (lines 52–76)
A three-clause compound guard (`EntryFilled && RemainingContracts > 0`) plus a
bracket-restoration branch is embedded directly inside the `TryRemove` success
block. This produces 4–5 decision nodes in a single 25-line block, making the
recovery path hard to reason about and test in isolation.

### 2 · Mixed Responsibilities: Cleanup + Emergency Recovery (lines 46–75)
The method simultaneously performs **dictionary cleanup** (remove stale entries,
decrement counter) and **emergency stop re-creation** (call `CreateNewStopOrder`,
optionally re-enqueue `RestoreCascadedTargets` via `TriggerCustomEvent`). These
are orthogonal concerns forced into a single loop body, raising CYC without
adding algorithmic value.

### 3 · Inline Bracket Restoration via Closure Dispatch (lines 70–75)
The `TriggerCustomEvent` lambda captures two locals (`_tSnap`, `_tKey`) to
asynchronously call `RestoreCascadedTargets`. This closure-inside-a-loop pattern
increases cognitive load, creates a latent variable-capture risk, and prevents
the bracket-restoration logic from being unit-tested independently.

---

## Blast Radius Summary

`CleanupStalePendingReplacements` is invoked on every trailing-stop management
cycle (throttled but still hot-path). Its mutations touch:

| Surface | Files Affected |
|---------|---------------|
| `pendingStopReplacements` (ConcurrentDictionary) | 41 files reference this dict or its sibling counters |
| `pendingReplacementCount` (Interlocked counter) | Same 41-file surface; read by circuit-breaker logic across REAPER, SIMA, Orders, UI layers |
| `activePositions` | 41 files; position state shared with REAPER, Orders, Symmetry, SIMA, Lifecycle |
| `CreateNewStopOrder` call chain | 26 files touch `CreateNewStopOrder` / `RestoreCascadedTargets` / `TriggerCustomEvent` |
| `circuitBreakerActive` / `circuitBreakerActivatedTime` | Read immediately after this call in `Trailing.cs:225` |

**Total distinct files in blast radius: ~41 production files** across Trailing,
REAPER, SIMA, Orders, Symmetry, UI, and Lifecycle subsystems.

---

## Recommended Extraction Count

| Extract To | Rationale |
|------------|-----------|
| `RemoveStalePendingEntry(kvp)` | Isolates TryRemove + counter decrement + diagnostic Print |
| `RecoverStopForStaleEntry(kvp, pending)` | Contains the three-clause position check + `CreateNewStopOrder` call |
| `ScheduleBracketRestoration(key, snapshot)` | Wraps the `TriggerCustomEvent` closure; eliminates loop-local capture risk |

**Recommended extraction count: 3 sub-methods**
Expected post-refactor CYC per method: ≤ 4 (cleanup driver ≤ 3, recovery ≤ 4, bracket ≤ 2).

---

## Agent Tracking

```
epic_id       : EPIC-W7-052
wave          : 7
phase         : 0
phase_name    : Hotspot Analysis
output_file   : docs/brain/EPIC-W7-052/00-hotspots.md
cyc_confirmed : 11
source_file   : src/V12_002.Trailing.StopUpdate.cs
method        : CleanupStalePendingReplacements
blast_files   : 41
extractions   : 3
generated_by  : Bob (technical assistant)
```
