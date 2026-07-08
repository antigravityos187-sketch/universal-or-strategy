# EPIC-W7-013 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-013/01-scope-boundary.md

---

## Original Method

| Field | Value |
|---|---|
| **Method** | `UpdatePanelState` |
| **File** | `src/V12_002.UI.Panel.StateSync.cs` |
| **Lines** | 13–89 (77 lines) |
| **CYC (measured)** | 22 |
| **Max Nesting** | 3 |
| **Params** | 0 |
| **Target CYC** | <= 8 (all helpers + parent) |

### Branch Inventory

| # | Condition | Branch Count |
|---|---|---|
| 1 | `rootContainer == null \|\| _isTerminating` | 2 (compound \|\|) |
| 2 | `if (lastPriceText != null)` | 1 |
| 3 | `price > 0` ternary | 1 |
| 4 | `mp == MarketPosition.Long` ternary | 1 |
| 5 | `mp == MarketPosition.Short` ternary | 1 |
| 6 | `if (!string.Equals(_panelLastSyncedMode, mode, ...))` | 1 |
| 7 | `if (snapshot.ConfigRevision != _panelAppliedConfigRevision)` | 1 |
| 8 | `if (_panelLastSyncedTargetCount != count)` | 1 |
| 9 | `elapsedTicks >= 0` (compound &&) | 1 |
| 10 | `elapsedTicks < TimeSpan.TicksPerSecond` (compound &&) | 1 |
| 11 | `if (!guardActive)` | 1 |
| 12 | `if (trendRmaToggle != null)` | 1 |
| 13 | `if (retestRmaToggle != null)` | 1 |
| 14 | `if (livePosition != null` (compound &&) | 1 |
| 15 | `&& livePosition.HasLivePosition)` (compound &&) | 1 |
| 16 | `if (liveStopRow != null)` (live path) | 1 |
| 17 | `if (_currentLiveEntryName != null)` (cleanup) | 1 |
| 18 | `if (liveStopRow != null)` (cleanup) | 1 |
| **Total** | | **21 = CYC 22** |

---

## Extraction Plan

| Helper Name | Responsibility | Lines Moved | Projected CYC |
|---|---|---|---|
| `SyncLastPriceDisplay(UIStateSnapshot snapshot)` | Format last-price text and market-position foreground color | ~11 (lines 17–27) | **5** |
| `TrySyncCountChipGuarded(int count)` | Rate-limit count-chip re-sync via tick-guard circuit breaker | ~10 (lines 35–48) | **5** |
| `SyncLivePositionOrCollapse(UIStateSnapshot snapshot, int count)` | Render live-position rows or collapse them when no live position | ~26 (lines 61–89) | **6** |
| `UpdatePanelState()` (parent after extraction) | Orchestrate: early-exit guard, snapshot acquisition, mode/config sync, RMA toggle, telemetry delegates, then call helpers | ~30 (lines 13–89 remainder) | **7** |

**max_cyc_projected: 7**

---

## Helper Signatures

```csharp
// Helper 1 — price display (same partial class, private)
private void SyncLastPriceDisplay(UIStateSnapshot snapshot)

// Helper 2 — count chip with tick-guard rate limiter (same partial class, private)
private void TrySyncCountChipGuarded(int count)

// Helper 3 — live position render / collapse (same partial class, private)
// Returns void; early 'return' in live path is preserved inside this helper via return statement
private void SyncLivePositionOrCollapse(UIStateSnapshot snapshot, int count)
```

---

## Parent After Extraction — UpdatePanelState()

```
CYC breakdown:
  Base = 1
  || _isTerminating          +1   (compound, from: rootContainer == null ||)
  rootContainer == null      +1
  !Equals mode               +1
  ConfigRevision != applied  +1
  trendRmaToggle != null     +1
  retestRmaToggle != null    +1
  3 helper calls             +0   (no branches added)
  ─────────────────────────────
  Total                      = 7
```

**Parent projected CYC = 7 <= 8 PASS**

---

## CYC Validation Summary

| Symbol | Projected CYC | Target | Status |
|---|---|---|---|
| `SyncLastPriceDisplay` | 5 | <= 8 | PASS |
| `TrySyncCountChipGuarded` | 5 | <= 8 | PASS |
| `SyncLivePositionOrCollapse` | 6 | <= 8 | PASS |
| `UpdatePanelState` (parent) | 7 | <= 8 | PASS |
| **max_cyc_projected** | **7** | **<= 8** | **PASS** |

---

## Jane Street Alignment Notes

| Principle | Application |
|---|---|
| **carl_cook — zero-alloc hot path** | No new allocations introduced; all helpers receive existing snapshot struct reference. `SyncLastPriceDisplay` operates on pre-acquired snapshot fields only. |
| **carl_cook — AggressiveInlining hot / NoInlining cold** | `SyncLastPriceDisplay` and `TrySyncCountChipGuarded` are hot-path helpers (called every panel tick) — mark with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`. `SyncLivePositionOrCollapse` is conditionally cold — mark with `[MethodImpl(MethodImplOptions.NoInlining)]`. |
| **carl_cook — avoid LINQ** | No LINQ added; all logic is direct field access and conditionals. |
| **gjengset — no new lock() blocks** | No locks introduced. All state mutations remain the same atomic field assignments already present (`_panelLastSyncedMode`, `_panelLastSyncedTargetCount`, `_currentLiveEntryName`). |
| **trading_billions — single responsibility per helper** | Each helper owns exactly one concern: price display, count-chip guard, or live-position row. |
| **trading_billions — each helper CYC <= 8** | Max projected CYC = 7. All three helpers <= 6. |
| **trading_billions — rate-limit circuit breaker** | `TrySyncCountChipGuarded` isolates the tick-guard circuit breaker into its own method, making the rate-limit logic explicit and independently testable. |

---

## Implementation Notes

1. **Partial class placement**: All 3 helpers are `private void` methods added to the same partial class in `src/V12_002.UI.Panel.StateSync.cs`. No new files required.

2. **Early-return preservation**: The `return;` inside the `if (livePosition != null && livePosition.HasLivePosition)` block must be preserved inside `SyncLivePositionOrCollapse`. Since the helper is `void`, the `return` terminates the helper. The parent `UpdatePanelState` calls `SyncLivePositionOrCollapse` as the last statement — the implicit early exit is preserved because after this call, there is no more logic in the parent (the cleanup path `if (_currentLiveEntryName != null)` is also moved inside the helper). No return value needed.

3. **Snapshot parameter**: `UIStateSnapshot snapshot` is passed by value (struct copy already performed by `GetUiSnapshot()`). No allocation concern.

4. **count parameter**: `int count` is computed in parent before the first call to `TrySyncCountChipGuarded` and reused by `SyncLivePositionOrCollapse`. Pass by value.

5. **Scope compliance**: V12.23 No Scope Creep — all helpers are private, same file, same partial class. No interface changes. No caller modifications.

---

## MCP Evidence

| Tool | Input | Result |
|---|---|---|
| `resolve_repo` | `/home/malhitticrypto/universal-or-strategy` | `found=true, indexed=true, symbol_count=5147` |
| `get_symbol_source` | `src/V12_002.UI.Panel.StateSync.cs::V12_002.UpdatePanelState#method` | Source confirmed: lines 13–89, CYC=22, private void UpdatePanelState() |
| `get_call_hierarchy` | `UpdatePanelState` (src path), depth=2, direction=both | 0 callers, 55 callee edges resolved. No upstream callers in index (consistent with Phase 1.5 — 2 callers not in indexed files). |
| `get_dependency_graph` | `src/V12_002.UI.Panel.StateSync.cs`, direction=both, depth=2 | 1 node, 0 edges — file has no indexed import/export edges (C# partial class; all deps are in-assembly). |
| `get_symbol_source` (bare name) | `UpdatePanelState` | Returned ambiguity: 2 candidates (src + src-vm-backup). Resolved to src/ path. |

---

## Sequential Thinking Evidence

**Thought 1 — Initial Probe:**
Identified 5 concerns in UpdatePanelState: early-exit guard, price/MP display, mode+config sync, count-chip guard, live-position row. Jane Street single-responsibility mandate requires decomposition into <= 8 CYC helpers.

**Thought 2 — Complexity Drivers:**
Mapped all 21 branches to 3 extraction clusters:
- Cluster 1 (Price Display): 4 branches (null-check, price>0, mp==Long, mp==Short)
- Cluster 2 (Count Chip Guard): 4 branches (count-changed, tick>=0, tick<TicksPerSec, !guardActive)
- Cluster 3 (Live Position): 5 branches (livePos!=null, &&HasLive, liveStop!=null, _currentLiveName!=null, liveStop!=null-cleanup)
Remaining parent: 6 branches (early-exit||, mode check, config check, 2x toggle null)

**Thought 3 — Extraction Strategy:**
Named and spec'd all 3 helpers with exact signatures, line ranges, and CYC projections:
SyncLastPriceDisplay=CYC5, TrySyncCountChipGuarded=CYC5, SyncLivePositionOrCollapse=CYC6.
Parent after extraction = CYC7. All <= 8.

**Thought 4 — CYC Validation:**
Final verification: max(5,5,6,7) = 7. All helpers PASS. Parent PASS.
max_cyc_projected = 7. Architecture ready.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 4 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-013 |
| **max_cyc_projected** | 7 |
| **boundary_verdict** | PASS (from Phase 1.5) |
