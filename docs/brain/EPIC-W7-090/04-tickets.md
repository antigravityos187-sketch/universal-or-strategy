# Phase 4: Ticket Generation -- EPIC-W7-090

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4
**Generated:** 2026-06-29T04:00:00Z
**Input:** docs/brain/EPIC-W7-090/02-architecture-plan.md + docs/brain/EPIC-W7-090/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Method** | `OnWatchdogTimer` |
| **Source File** | `src/V12_002.Safety.Watchdog.cs` |
| **Original CYC** | 11 |
| **Ticket Count** | 3 |
| **Projected Parent CYC After All Extractions** | 3 |
| **Max Helper CYC** | 6 |
| **DNA Audit** | PASS (03-audit-report.md) |

Each ticket defines one surgical extraction of a private helper method from `OnWatchdogTimer`. Executing all 3 tickets reduces the parent method cyc from 11 to 3, satisfying the Jane Street CYC<=8 mandate.

---

## Ticket 1

**ticket_id:** EPIC-W7-090-T1
**helper_name:** `WatchdogShouldSuppressEscalation`
**concern:** Extract all 4 early-exit guard conditions from `OnWatchdogTimer` into a single boolean predicate helper. Guards cover: (1) terminatingState check with `_watchdogStage` reset to 0 via `Interlocked.Exchange`, (2) zero-heartbeat guard, (3) heartbeat-age-within-timeout guard with stage reset, (4) no-working-order guard (`HasWatchdogLeadAccountWorkingOrder`) with stage reset. Returns `true` if escalation should be suppressed; caller returns immediately on `true`.

**lines_to_move:** Lines 36–65 (approximately 30 LOC — the 4 guard if-branches at the top of `OnWatchdogTimer`)

**cyc_reduction:** 8 (removes 8 branch points from the parent; parent retains CYC=3 after all extractions are applied)

**projected_helper_cyc:** 6

**signature:** `private bool WatchdogShouldSuppressEscalation()`

**xunit_tests:**
- `WatchdogShouldSuppressEscalation_WhenTerminating_ReturnsTrue`
- `WatchdogShouldSuppressEscalation_WhenHeartbeatHealthy_ReturnsTrue`
- `WatchdogShouldSuppressEscalation_WhenNoWorkingOrder_ReturnsTrue`

**constraints:**
- No `lock()` blocks. All synchronization via `Interlocked` primitives only.
- All string literals ASCII-only (no Unicode, emoji, curly quotes).
- Helper is `private` scope, same file (`src/V12_002.Safety.Watchdog.cs`).
- No cross-file edits required.

---

## Ticket 2

**ticket_id:** EPIC-W7-090-T2
**helper_name:** `TryEscalateToStageOne`
**concern:** Extract the CAS 0->1 escalation block from `OnWatchdogTimer` into a dedicated helper. Logic: short-circuit if `stage != 0`; attempt `Interlocked.CompareExchange(ref _watchdogStage, 1, 0)`; on success call `Print(...)` + `Enqueue(ctx => ctx.ExecuteWatchdogLeadAccountFlatten())`; catch block rolls back stage to 0 via `Interlocked.Exchange(ref _watchdogStage, 0)` with `Print`. Returns `bool` — `true` if the stage-0 path was entered (regardless of CAS outcome); parent calls `if (TryEscalateToStageOne(stage)) return;`.

**lines_to_move:** Lines 66–78 (approximately 13 LOC — the stage-0 CAS escalation block)

**cyc_reduction:** (contributes to parent's total reduction alongside T1 and T3; helper carries 4 CYC that would otherwise remain inline)

**projected_helper_cyc:** 4

**signature:** `private bool TryEscalateToStageOne(int stage)`

**xunit_tests:**
- `TryEscalateToStageOne_WhenStageZero_EnqueuesAndReturnsTrue`
- `TryEscalateToStageOne_WhenStageNonZero_ReturnsFalse`

**constraints:**
- `Enqueue` call and `Interlocked.CompareExchange` must be preserved exactly — lock-free Actor pattern must not be altered.
- Catch block rollback (`Interlocked.Exchange(ref _watchdogStage, 0)`) must be retained inside the helper.
- No `lock()` blocks. ASCII-only strings. `private` scope, same file.

---

## Ticket 3

**ticket_id:** EPIC-W7-090-T3
**helper_name:** `TryEscalateToStageTwo`
**concern:** Extract the CAS 1->2 escalation block from `OnWatchdogTimer` into a dedicated helper. Logic: short-circuit guard if `stage != 1`; attempt `Interlocked.CompareExchange(ref _watchdogStage, 2, 1)`; on success call `Print(...)` + `ExecuteWatchdogDirectFallback()`. Returns `void` — parent calls `TryEscalateToStageTwo(stage);` as the final statement with no return needed.

**lines_to_move:** Lines 79–89 (approximately 11 LOC — the stage-1 CAS escalation block)

**cyc_reduction:** (contributes to parent's total reduction alongside T1 and T2; helper carries 3 CYC that would otherwise remain inline)

**projected_helper_cyc:** 3

**signature:** `private void TryEscalateToStageTwo(int stage)`

**xunit_tests:**
- `TryEscalateToStageTwo_WhenStageOne_ExecutesFallback`
- `TryEscalateToStageTwo_WhenStageNotOne_DoesNothing`

**constraints:**
- `ExecuteWatchdogDirectFallback()` call must be preserved — no substitution.
- No `lock()` blocks. ASCII-only strings. `private` scope, same file.
- This is the terminal escalation path; no return value required.

---

## Projected Parent CYC After All Extractions

After completing all 3 tickets the parent `OnWatchdogTimer` reduces to the following shell (CYC=3):

```csharp
private void OnWatchdogTimer(object state)
{
    if (WatchdogShouldSuppressEscalation())
        return;

    int stage = Volatile.Read(ref _watchdogStage);
    if (TryEscalateToStageOne(stage))
        return;

    TryEscalateToStageTwo(stage);
}
```

**projected_parent_cyc_after_all:** 3 (base=1, if-suppress=1, if-stageOne=1)

**CYC reduction summary:**

| Symbol | Before | After |
|---|---|---|
| `OnWatchdogTimer` (parent) | 11 | 3 |
| `WatchdogShouldSuppressEscalation` | — | 6 |
| `TryEscalateToStageOne` | — | 4 |
| `TryEscalateToStageTwo` | — | 3 |
| **Max CYC in scope** | **11** | **6** |

All projected CYC values satisfy the Jane Street CYC<=8 mandate. The cyc extraction model distributes complexity into single-responsibility helpers, making each path independently testable.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T04:00:00Z |
| **jcodemunch tools called** | resolve_repo |
| **sequential-thinking calls** | 4 |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-090 |
| **Ticket Count** | 3 |
| **projected_parent_cyc_after_all** | 3 |
| **Output** | docs/brain/EPIC-W7-090/04-tickets.md |
