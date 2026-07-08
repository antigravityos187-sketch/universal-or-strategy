# EPIC-W7-013 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:40:00Z
**Inputs:**
- `docs/brain/EPIC-W7-013/02-architecture-plan.md`
- `docs/brain/EPIC-W7-013/03-audit-report.md`

---

## Overview

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-013 |
| **Method** | `UpdatePanelState` |
| **File** | `src/V12_002.UI.Panel.StateSync.cs` |
| **CYC (measured)** | 22 (confirmed by `get_symbol_complexity`) |
| **Lines** | 77 (lines 13–89) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 7 |
| **max_cyc_projected** | 7 |
| **DNA verdict (Phase 3)** | PASS |

---

## MCP Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | `found=true, indexed=true, symbol_count=5147` |
| `get_symbol_complexity(UpdatePanelState)` | `cyclomatic=22, max_nesting=3, param_count=0, lines=77, assessment=high` |
| `get_extraction_candidates(V12_002.UI.Panel.StateSync.cs)` | `candidates=[]` (0 external callers indexed; consistent with Phase 2/3 evidence) |

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | 1 |
| **helper_name** | `SyncLastPriceDisplay` |
| **signature** | `private void SyncLastPriceDisplay(UIStateSnapshot snapshot)` |
| **concern** | Format last-price text and market-position foreground color |
| **lines_to_move** | Lines 17–27 (~11 lines): null-check on `lastPriceText`, set text via `price > 0` ternary, set `Foreground` color via `mp == MarketPosition.Long` / `mp == MarketPosition.Short` ternaries |
| **cyc_reduction** | 4 (removes 4 branch conditions from parent: `lastPriceText != null`, `price > 0`, `mp == Long`, `mp == Short`) |
| **projected_helper_cyc** | **5** (1 base + 4 branches) |
| **method_impl** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — hot path, called every panel tick |
| **scope** | Same partial class, same file — no new files, no interface changes |

### CYC Validation
- `SyncLastPriceDisplay` projected CYC = **5** → target <= 8 → **PASS**

---

## Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | 2 |
| **helper_name** | `TrySyncCountChipGuarded` |
| **signature** | `private void TrySyncCountChipGuarded(int count)` |
| **concern** | Rate-limit count-chip re-sync via tick-guard circuit breaker |
| **lines_to_move** | Lines 35–48 (~10 lines): check `_panelLastSyncedTargetCount != count`, compute `elapsedTicks`, apply tick-guard conditions (`elapsedTicks >= 0 && elapsedTicks < TimeSpan.TicksPerSecond`), evaluate `!guardActive`, sync count chip if guard passes |
| **cyc_reduction** | 4 (removes 4 branch conditions from parent: `count changed`, `elapsedTicks >= 0`, `elapsedTicks < TicksPerSecond`, `!guardActive`) |
| **projected_helper_cyc** | **5** (1 base + 4 branches) |
| **method_impl** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — hot path, called every panel tick |
| **scope** | Same partial class, same file — no new files, no interface changes |

### CYC Validation
- `TrySyncCountChipGuarded` projected CYC = **5** → target <= 8 → **PASS**

---

## Ticket 3

| Field | Value |
|---|---|
| **ticket_id** | 3 |
| **helper_name** | `SyncLivePositionOrCollapse` |
| **signature** | `private void SyncLivePositionOrCollapse(UIStateSnapshot snapshot, int count)` |
| **concern** | Render live-position rows or collapse them when no live position exists |
| **lines_to_move** | Lines 61–89 (~26 lines): `livePosition != null && livePosition.HasLivePosition` check, `liveStopRow != null` live-path rendering, live-path `return;` (preserved as early return inside helper), cleanup/collapse path (`_currentLiveEntryName != null`, `liveStopRow != null` cleanup) |
| **cyc_reduction** | 5 (removes 5 branch conditions from parent: `livePos != null`, `&&HasLivePosition`, `liveStop != null` live, `_currentLiveName != null`, `liveStop != null` cleanup) |
| **projected_helper_cyc** | **6** (1 base + 5 branches) |
| **method_impl** | `[MethodImpl(MethodImplOptions.NoInlining)]` — conditionally cold path |
| **early_return_note** | Original `return;` inside live-path block is preserved inside helper as a `return` statement. Helper is `void`; parent calls it as final statement — implicit early exit preserved without return value. |
| **scope** | Same partial class, same file — no new files, no interface changes |

### CYC Validation
- `SyncLivePositionOrCollapse` projected CYC = **6** → target <= 8 → **PASS**

---

## Parent Method After All Extractions

### UpdatePanelState() — Projected CYC Breakdown

```
Base                                = 1
rootContainer == null               +1
|| _isTerminating (compound)        +1
!string.Equals(_panelLastSyncedMode, mode, ...) +1
snapshot.ConfigRevision !=
  _panelAppliedConfigRevision       +1
trendRmaToggle != null              +1
retestRmaToggle != null             +1
3 helper calls                      +0  (no new branches)
─────────────────────────────────────
projected_parent_cyc_after_all      = 7
```

| Field | Value |
|---|---|
| **projected_parent_cyc_after_all** | **7** |
| **Target** | <= 8 |
| **Status** | **PASS** |

---

## CYC Validation Summary

| Symbol | Projected CYC | Target | Status |
|---|---|---|---|
| `SyncLastPriceDisplay` | 5 | <= 8 | **PASS** |
| `TrySyncCountChipGuarded` | 5 | <= 8 | **PASS** |
| `SyncLivePositionOrCollapse` | 6 | <= 8 | **PASS** |
| `UpdatePanelState` (parent after all) | 7 | <= 8 | **PASS** |
| **max_cyc_projected** | **7** | **<= 8** | **PASS** |

---

## Sequential Thinking Evidence

### Thought 1 — Ticket Count
Three distinct concern clusters identified from 21 branches in `UpdatePanelState` (CYC=22):
price/MP display (4 branches), count-chip tick-guard (4 branches), live-position rows (5 branches).
One ticket per extracted helper = **3 tickets**. Remaining parent = CYC 7.

### Thought 2 — Per-Ticket Detail
- **T1** `SyncLastPriceDisplay`: lines 17–27, 4 branches removed from parent, helper CYC=5. `AggressiveInlining` hot path.
- **T2** `TrySyncCountChipGuarded`: lines 35–48, 4 branches removed from parent, helper CYC=5. `AggressiveInlining` hot path.
- **T3** `SyncLivePositionOrCollapse`: lines 61–89, 5 branches removed from parent, helper CYC=6. `NoInlining` cold path.

### Thought 3 — Final CYC Verification
All helpers <= 8: 5, 5, 6. Parent after all extractions = 7. max_cyc_projected = 7 <= 8.
Phase 2 architecture plan and Phase 3 DNA audit both independently confirmed these projections.
Verdict: **3 tickets. All CYC projections PASS.**

---

## Implementation Order

| Order | Ticket | Helper | Rationale |
|---|---|---|---|
| 1 | T1 | `SyncLastPriceDisplay` | Smallest scope (11 lines), safest first extraction |
| 2 | T2 | `TrySyncCountChipGuarded` | Independent concern, no dependency on T1 or T3 |
| 3 | T3 | `SyncLivePositionOrCollapse` | Largest scope (26 lines), moved last after parent is simplified |

---

## Execution Constraints

| Constraint | Value |
|---|---|
| **New files created** | 0 |
| **Interface changes** | None |
| **External callers modified** | None |
| **Partial class** | All helpers added to same partial class in `src/V12_002.UI.Panel.StateSync.cs` |
| **Lock blocks introduced** | 0 |
| **ASCII compliance** | All helper names and literals are ASCII-only |
| **V12.23 scope creep** | NONE — strictly bounded to target method + 3 helpers, same file |
| **Test framework** | xUnit `[Fact]` + `Assert.Equal()` on panel element state (no NUnit/MSTest) |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 5 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-013 |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 7 |
| **max_cyc_projected** | 7 |
