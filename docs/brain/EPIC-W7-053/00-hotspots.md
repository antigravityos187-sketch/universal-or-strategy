# EPIC-W7-053 — Phase 0: Hotspot Analysis

## Target Method

| Field               | Value                                                         |
|---------------------|---------------------------------------------------------------|
| **Method**          | `InitiateStopReplacement`                                     |
| **CYC (reported)**  | 0 (tool-reported at intake; manual count = 6)                 |
| **File**            | `src/V12_002.Trailing.StopUpdate.cs`                          |
| **Lines**           | 307–369 (63 loc)                                              |
| **Class**           | `V12_002` (partial — Trailing module)                         |
| **Visibility**      | `private void`                                                |
| **Build tag**       | Build 955 / V8.30                                             |

---

## Blast Radius Summary

`InitiateStopReplacement` is called by exactly **one direct caller**:
`UpdateStopOrder` (line 128, same file) — triggered only when `currentStop.OrderState`
is `Working` or `Accepted`.

However the shared state it mutates is touched by a **wide blast surface**:

| Shared Resource                  | Consumers (files)                                                                                                       |
|----------------------------------|-------------------------------------------------------------------------------------------------------------------------|
| `pendingStopReplacements`        | 13 files — `Trailing.StopUpdate`, `Orders.Callbacks`, `Orders.Callbacks.Execution`, `Orders.Callbacks.AccountOrders`, `Orders.Callbacks.Propagation`, `Orders.Management.StopSync`, `Orders.Management.Cleanup`, `Orders.Management.Flatten`, `REAPER.NakedPosition`, `REAPER.Audit`, `UI.Compliance`, `Lifecycle`, `Orders.Callbacks` |
| `pendingReplacementCount`        | `Trailing.StopUpdate`, `Orders.Management.StopSync`, `Orders.Callbacks`                                                |
| `circuitBreakerActive`           | `Trailing.StopUpdate`, `Trailing` (reset), `Orders.Management.StopSync`                                                |
| `circuitBreakerActivatedTime`    | `Trailing.StopUpdate`                                                                                                   |
| `pos.CurrentStopPrice`           | Written by 6+ methods across Trailing, SIMA, Orders subsystems                                                         |
| `pos.CurrentTrailLevel`          | Written by 4+ methods across Trailing, Breakeven, Symmetry subsystems                                                  |
| `CancelOrderForReplace()`        | Also called from `Orders.Management.StopSync`, `UpdateStopQuantity_CancelAndReplace`                                   |

**Downstream callbacks triggered**: `CancelOrderForReplace` → `StampReaperMoveGrace` → `CancelOrderSafe`
→ NinjaTrader broker API → `OnOrderUpdate` / `HandleOrderCancelled` → `RestoreCascadedTargets`.

**Blast radius classification: HIGH** — mutates 4 concurrent shared-state fields, triggers asynchronous
broker callbacks, and the `pendingStopReplacements` dictionary is read by 13+ files on the callback path.

---

## Top 3 Complexity Drivers

### 1 — Inlined Target Snapshot Loop (Build 955 duplication)
Lines 316–336 contain a verbatim copy of the 5-target scan loop already implemented in
`CaptureTargetSnapshot()` (lines 255–279, same file). The loop was intentionally inlined
"before TryAdd" for ordering reasons but introduces structural duplication (≈20 loc clone).
This is the single largest contributor to method length and the primary refactor candidate.

### 2 — Eager Circuit-Breaker State Write Inside TryAdd Branch
Lines 351–360 embed circuit-breaker activation (`circuitBreakerActive = true`,
`circuitBreakerActivatedTime = DateTime.Now`) directly inside the `TryAdd` success branch,
mixing pending-queue bookkeeping with a global safety-mode mutation. The same pattern is
duplicated verbatim in `UpdateExistingPendingReplacement` (lines 191–205). Isolation of
this concern is the second extraction candidate.

### 3 — Level-Name Ternary Formatting at Call Site
Line 367 encodes a nested ternary string formatter
(`newTrailLevel <= 0 ? "Initial" : (newTrailLevel == 1 ? "BE" : "T" + ...)`)
duplicated across `CreateDirectStopOrder` (line 454). A shared helper would eliminate
both duplication and the +2 CYC contribution from the nested conditional.

---

## Recommended Extraction Count

| Extraction                                  | Target name (suggested)           | CYC reduction |
|---------------------------------------------|-----------------------------------|---------------|
| Inline snapshot loop → delegate to existing helper | `CaptureTargetSnapshot()`  | −1 branch     |
| Circuit-breaker activation block            | `TryActivateCircuitBreaker(count)`| −1 branch     |
| Level-name formatter ternary                | `TrailLevelName(int level)`       | −2 ternary    |

**Total recommended extractions: 3**
Post-extraction estimated CYC: 3 (from 6).

---

## Agent Tracking

```
EPIC:       EPIC-W7-053
Wave:       7
Phase:      0 — Hotspot Analysis
Status:     completed
Agent:      Bob (analytical pass — no code modified)
Output:     docs/brain/EPIC-W7-053/00-hotspots.md
Timestamp:  2025-07-15
Source:     src/V12_002.Trailing.StopUpdate.cs
Method:     InitiateStopReplacement
CYC:        0 (tool-reported at intake) / 6 (manual static count)
Blast:      HIGH — 13+ consumer files on pendingStopReplacements
```
