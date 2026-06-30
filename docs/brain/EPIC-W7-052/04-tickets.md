# Phase 4: Ticket Definitions — EPIC-W7-052

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T02:45:00Z
**Inputs:** docs/brain/EPIC-W7-052/02-architecture-plan.md, docs/brain/EPIC-W7-052/03-audit-report.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `CleanupStalePendingReplacements` |
| **Source File** | `src/V12_002.Trailing.StopUpdate.cs` |
| **Class** | `V12_002` (partial) — `NinjaTrader.NinjaScript.Strategies` |
| **Original CYC** | 11 |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 4 |

---

## Sequential Thinking Validation

All 3 tickets validated via `mcp__sequential-thinking__sequentialthinking` (thoughts 1–3):

- **Thought 1:** Identified 3 concerns requiring extraction — one ticket per helper.
- **Thought 2:** Mapped lines to move, helper signatures, and CYC reduction per ticket.
- **Thought 3:** Verified parent CYC=4 and all helpers CYC<=4 after all extractions. All <= 8 mandate satisfied.

---

## Ticket Definitions

---

### Ticket 1 — Extract `RemoveStalePendingEntry`

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **helper_name** | `RemoveStalePendingEntry` |
| **concern** | Remove a keyed entry from `pendingStopReplacements` (ConcurrentDictionary.TryRemove), decrement `pendingReplacementCount` via `Interlocked.Decrement`, and emit the stale-removed diagnostic Print log. Returns `bool` success; carries the removed value via `out PendingReplacement pending` — making it impossible to reference a pending that was never successfully removed (illegal state unrepresentable). |
| **signature** | `private bool RemoveStalePendingEntry(string key, out PendingReplacement pending)` |
| **lines_to_move** | The `pendingStopReplacements.TryRemove(kvp.Key, out var pending)` call, the `Interlocked.Decrement(ref pendingReplacementCount)` call, and the `Print(LogBuffer.Format("Stale pending replacement removed: {0}", key))` diagnostic log — previously inline inside the staleness-if block of the parent foreach. |
| **cyc_reduction** | 3 (removes TryRemove-bool branch, decrement, and Print inline branching from parent scope) |
| **projected_helper_cyc** | 2 |
| **jane_street_notes** | `out` parameter pattern enforces illegal-state-unrepresentable at compile time. Lock-free: ConcurrentDictionary.TryRemove + Interlocked.Decrement. ASCII-only Print format string. |

**Acceptance Criteria:**
- `RemoveStalePendingEntry` compiles as `private bool` with `out PendingReplacement` parameter
- Parent foreach calls `if (RemoveStalePendingEntry(kvp.Key, out var pending))` — no inline TryRemove in parent
- Build passes: `dotnet build src/`
- xUnit test: `Test_RemoveStalePendingEntry_RemovesEntry_And_DecrementsCounter` — verifies TryRemove called and `pendingReplacementCount` decremented when key exists; returns false when key absent

---

### Ticket 2 — Extract `RecoverStopForStaleEntry`

| Field | Value |
|---|---|
| **ticket_id** | T2 |
| **helper_name** | `RecoverStopForStaleEntry` |
| **concern** | Validate that the stale entry's position is still active, filled, and has remaining contracts (three-clause compound guard), compute `replacementQty`, call `CreateNewStopOrder(isRecovery: true)` to submit the recovery order, and delegate bracket restoration to `ScheduleBracketRestoration`. Single responsibility: recovery orchestration for a stale pending replacement. |
| **signature** | `private void RecoverStopForStaleEntry(string key, PendingReplacement pending)` |
| **lines_to_move** | The `activePositions.TryGetValue(pending.PositionKey, out var pos)` guard, the `pos.EntryFilled` guard, the `pos.RemainingContracts > 0` guard, the `int replacementQty = pos.RemainingContracts` assignment, the `CreateNewStopOrder(key, replacementQty, isRecovery: true)` call, and the `ScheduleBracketRestoration(key, pending)` call — all previously inline in the staleness-if block after the TryRemove call. |
| **cyc_reduction** | 4 (three guard clauses + one conditional path removed from parent scope) |
| **projected_helper_cyc** | 4 |
| **jane_street_notes** | Compound guard decomposed into three separate early-return checks. `ScheduleBracketRestoration` called from within this helper (not from parent) — eliminates loop-local lambda capture risk by hoisting loop variables into named method parameters. Lock-free: no lock() blocks. |

**Acceptance Criteria:**
- `RecoverStopForStaleEntry` compiles as `private void` receiving `string key, PendingReplacement pending`
- Parent foreach body calls `RecoverStopForStaleEntry(kvp.Key, pending)` — no inline guard logic in parent
- `ScheduleBracketRestoration` is called from within `RecoverStopForStaleEntry`, not from parent
- Build passes: `dotnet build src/`
- xUnit test: `Test_RecoverStopForStaleEntry_CreatesStopOrder_WhenPositionExists` — verifies `CreateNewStopOrder` called with `isRecovery: true` when position is active, filled, and has remaining contracts; verifies no call when guard fails

---

### Ticket 3 — Extract `ScheduleBracketRestoration`

| Field | Value |
|---|---|
| **ticket_id** | T3 |
| **helper_name** | `ScheduleBracketRestoration` |
| **concern** | Guard whether bracket restoration is required (`pending.BracketRestorationNeeded && pending.CapturedTargets != null`), then dispatch the `TriggerCustomEvent` closure that calls `RestoreCascadedTargets`. Eliminates the loop-local lambda variable capture risk by receiving `key` and `pending` as named method parameters instead of closing over loop variables. |
| **signature** | `private void ScheduleBracketRestoration(string key, PendingReplacement pending)` |
| **lines_to_move** | The `if (pending.BracketRestorationNeeded && pending.CapturedTargets != null)` guard and the `TriggerCustomEvent((_tSnap, _tKey) => RestoreCascadedTargets(_tSnap, _tKey), ...)` lambda dispatch — previously inline inside the recovery logic block, subject to loop-local variable capture through `kvp`. |
| **cyc_reduction** | 2 (BracketRestorationNeeded guard + CapturedTargets-null guard removed from RecoverStopForStaleEntry scope) |
| **projected_helper_cyc** | 3 |
| **jane_street_notes** | Hoisting loop variables `key` and `pending` into named parameters eliminates undefined-behavior-class loop capture bug. Guard uses short-circuit `&&` — only one branch point per guard clause. ASCII-only. Lock-free. |

**Acceptance Criteria:**
- `ScheduleBracketRestoration` compiles as `private void` receiving `string key, PendingReplacement pending`
- No loop-local variable capture in the TriggerCustomEvent lambda — only named parameters used
- `RecoverStopForStaleEntry` calls `ScheduleBracketRestoration(key, pending)` — no inline bracket guard in that helper
- Build passes: `dotnet build src/`
- xUnit test: `Test_ScheduleBracketRestoration_DispatchesTrigger_WhenBracketNeeded` — verifies `TriggerCustomEvent` dispatched when `BracketRestorationNeeded=true` and `CapturedTargets != null`; verifies no dispatch when guard fails

---

## Execution Order

| Order | Ticket | Reason |
|---|---|---|
| 1st | T3 — `ScheduleBracketRestoration` | No dependencies; called from T2 helper |
| 2nd | T2 — `RecoverStopForStaleEntry` | Depends on T3 being defined |
| 3rd | T1 — `RemoveStalePendingEntry` | Parent orchestrator reduction — last to finalize parent method |

---

## CYC Summary

| Method | Original CYC | Projected CYC | Status |
|---|---|---|---|
| `CleanupStalePendingReplacements` (parent) | 11 | 4 | PASS (<=8) |
| `RemoveStalePendingEntry` (T1 helper) | — | 2 | PASS (<=8) |
| `RecoverStopForStaleEntry` (T2 helper) | — | 4 | PASS (<=8) |
| `ScheduleBracketRestoration` (T3 helper) | — | 3 | PASS (<=8) |
| **Max across all methods** | **11** | **4** | **PASS** |

**CYC reduction:** 11 → 4 (63.6% reduction on parent method)

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC <= 8 for all methods | YES — max projected CYC is 4 |
| Single-responsibility per helper | YES — each ticket has exactly one named concern |
| Lock-free / Actor pattern preserved | YES — ConcurrentDictionary.TryRemove + Interlocked.Decrement; no lock() introduced |
| Illegal states unrepresentable | YES — `out PendingReplacement pending` bool pattern; loop-local lambda capture eliminated |
| ASCII-only string literals | YES — all Print() format strings verified ASCII-only (Phase 3 audit) |
| xUnit tests required ([Fact], Assert.Equal) | YES — 3 named tests specified, one per helper |
| No scope creep (V12.23) | YES — 1 method refactored + 3 private helpers added, same file only |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic ID** | EPIC-W7-052 |
| **Wave** | 7 |
| **Phase** | 4 |
| **Bobcoins Used** | 0.9 |
| **Execution Time** | 2026-06-29T02:45:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket-breakdown thoughts) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 4 |
