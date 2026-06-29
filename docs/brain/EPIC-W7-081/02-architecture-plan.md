# EPIC-W7-081 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T00:58:00Z
**Input:** docs/brain/EPIC-W7-081/01-scope-boundary.md

---

## Method Under Analysis

| Field | Value |
|---|---|
| **Method** | `AuditMaster_HandleNakedPosition` |
| **File** | `src/V12_002.REAPER.Audit.cs` |
| **Lines** | 624 – 679 (55 lines) |
| **Signature** | `private void AuditMaster_HandleNakedPosition(Position masterPos, int masterActualQty, string masterExpectedKey)` |
| **Callers** | 1 (`AuditMasterAccountIfNeeded`, same file line 701) |
| **Current CYC** | 6 (structural analysis: 1 base + 5 decision points) |
| **Precomputed CYC** | 0 (tool did not capture; 00-scope.md reports 1 as floor) |
| **Jane Street Threshold** | 8 |
| **Target CYC** | ≤ 8 (already within threshold; extracting for single-responsibility) |

---

## MCP Tool Evidence

The following jcodemunch MCP tools were used during Phase 2 analysis:

| Tool | Finding |
|---|---|
| `get_context_bundle` | Retrieved full source (lines 624-679), imports, docstring; confirmed method is 55 lines |
| `get_call_hierarchy` | Confirmed 1 direct caller (`AuditMasterAccountIfNeeded`), 26 callees (depth 3); upstream caller unchanged |
| `get_dependency_graph` | `src/V12_002.REAPER.Audit.cs` has 0 inter-file import edges; self-contained partial class |
| `get_extraction_candidates` | Returned empty (file complexity below tool threshold); manual CYC analysis used instead |
| `search_symbols` | Located symbol at line 624 in both `src/` and `src-vm-backup/`; confirmed single active instance |

---

## CYC Baseline Analysis

Structural CYC count from source (lines 624-679):

```
1  (method entry base)
+1 if (masterActualQty != 0)
+1 if (!masterHasWorkingStop)
+1 if (!_nakedPositionFirstSeen.TryGetValue(...))      // first-seen check
+1 else if (EnqueueReaperMasterNakedStop(...))          // grace expired, enqueue
+1 catch (Exception tcEx)                              // TriggerCustomEvent failure path
─────
= 6 total CYC
```

The method has three cohesive concerns interleaved with deep nesting:
- **Concern A** (lines 630-636): Snapshot broker orders + detect working stop
- **Concern B** (lines 639-651): First-seen tracking + grace window start logging
- **Concern C** (lines 653-671): Enqueue stop + trigger event + error recovery

---

## Sequential Thinking Summary

Phase 2 used 4 sequential thinking steps to derive and validate the extraction plan:

**Step 1 — CYC Analysis**: Counted structural decision points; derived CYC=6. Identified 3 logical concerns (A, B, C) at different nesting depths. Confirmed method is within ≤8 threshold but violates single-responsibility.

**Step 2 — Helper Design**: Evaluated two extraction strategies. Strategy 1: 3 separate helpers (one per concern). Strategy 2: 2 helpers (detect + combined no-stop handler). Selected Strategy 1 for maximum single-responsibility alignment (trading_billions pattern). Verified each helper's projected CYC independently.

**Step 3 — Jane Street KB Alignment**: Applied gjengset (preserve H13-FIX ToArray() snapshot; preserve ConcurrentDictionary atomic ops — no lock()), carl_cook (mark hot-path helper [AggressiveInlining]; keep cold Print logging in its own helper with [NoInlining]), trading_billions (each helper = one responsibility; preserve _reaperNakedStopInFlight circuit-breaker in TriggerNakedStopIfGraceExpired).

**Step 4 — Final Verification**: Confirmed all projected CYC values ≤ 8. Reconciled "5 new helpers" from Phase 1.5 scope: this epic adds 3 helpers; prior epics (B935) added 2 (`AuditMaster_CalculatePositionState`, `AuditMaster_HandleDesyncFlatten`). Total = 5 helpers for this method family. Plan verified. max_cyc_projected = 3.

---

## Extraction Plan

### Helper Method 1: `AuditMaster_HasWorkingStopOrder`

| Field | Value |
|---|---|
| **Signature** | `private bool AuditMaster_HasWorkingStopOrder()` |
| **Location** | `src/V12_002.REAPER.Audit.cs` (same file, same partial class) |
| **Extracts** | Lines 630-636 — snapshot `Account.Orders.ToArray()` and evaluate LINQ predicate |
| **Responsibility** | Single: detect whether any working/accepted stop order exists for the current instrument |
| **Projected CYC** | 1 (no branches; single snapshot + LINQ Any() expression) |
| **Attributes** | `[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining)]` — hot path, called every audit cycle |

**Body sketch:**
```csharp
// H13-FIX: Snapshot to prevent InvalidOperationException from UI thread updates.
var masterOrders = Account.Orders.ToArray();
return masterOrders.Any(o =>
    o.Instrument?.FullName == Instrument?.FullName
    && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
    && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
    && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover)
);
```

---

### Helper Method 2: `AuditMaster_StartNakedGraceWindow`

| Field | Value |
|---|---|
| **Signature** | `private void AuditMaster_StartNakedGraceWindow(int masterActualQty, int graceSeconds)` |
| **Location** | `src/V12_002.REAPER.Audit.cs` (same file, same partial class) |
| **Extracts** | Lines 643-651 — record first-seen timestamp + emit grace-window log |
| **Responsibility** | Single: register the initial naked-position observation and log the grace window start |
| **Projected CYC** | 1 (no branches; sequential assignment + Print call) |
| **Attributes** | `[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]` — cold path, only executes once per naked position detection |

**Body sketch:**
```csharp
_nakedPositionFirstSeen[Account.Name] = DateTime.UtcNow;
Print(string.Format(
    "[REAPER][NAKED_POSITION] {0} (Master): {1}ct naked -- starting {2}s grace window.",
    Account.Name,
    masterActualQty,
    graceSeconds
));
```

---

### Helper Method 3: `AuditMaster_TriggerNakedStopIfGraceExpired`

| Field | Value |
|---|---|
| **Signature** | `private void AuditMaster_TriggerNakedStopIfGraceExpired(Position masterPos, int masterActualQty, string masterExpectedKey, DateTime masterFirstSeen)` |
| **Location** | `src/V12_002.REAPER.Audit.cs` (same file, same partial class) |
| **Extracts** | Lines 640 + 653-671 — grace seconds calculation, `EnqueueReaperMasterNakedStop`, `TriggerCustomEvent` with error recovery |
| **Responsibility** | Single: determine if the grace period is expired, enqueue the emergency stop, and trigger processing with in-flight circuit-breaker on failure |
| **Projected CYC** | 3 (ternary grace check +1; if-enqueue +1; catch +1) |
| **Attributes** | `[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]` — cold path (emergency stop path) |

**Body sketch:**
```csharp
int graceSeconds = (NakedPositionGraceSec >= 5) ? NakedPositionGraceSec : 5;
if (EnqueueReaperMasterNakedStop(masterPos, masterActualQty, masterExpectedKey, masterFirstSeen))
{
    try
    {
        TriggerCustomEvent(e => ProcessReaperNakedStopQueue(), null);
    }
    catch (Exception tcEx)
    {
        _reaperNakedStopInFlight.TryRemove(masterExpectedKey, out _);
        Print(string.Format(
            "[REAPER][NAKED_STOP] TriggerCustomEvent failed for {0} (Master): {1} -- in-flight cleared.",
            Account.Name,
            tcEx.Message
        ));
    }
}
```

---

## Parent Method After Extraction

**New body of `AuditMaster_HandleNakedPosition`:**

```csharp
private void AuditMaster_HandleNakedPosition(
    Position masterPos, int masterActualQty, string masterExpectedKey)
{
    if (masterActualQty != 0)
    {
        if (!AuditMaster_HasWorkingStopOrder())
        {
            DateTime masterFirstSeen;
            if (!_nakedPositionFirstSeen.TryGetValue(Account.Name, out masterFirstSeen))
            {
                int graceSeconds = (NakedPositionGraceSec >= 5) ? NakedPositionGraceSec : 5;
                AuditMaster_StartNakedGraceWindow(masterActualQty, graceSeconds);
            }
            else
            {
                AuditMaster_TriggerNakedStopIfGraceExpired(
                    masterPos, masterActualQty, masterExpectedKey, masterFirstSeen);
            }
        }
        else
        {
            _nakedPositionFirstSeen.TryRemove(Account.Name, out _);
        }
    }
}
```

| Field | Value |
|---|---|
| **Projected CYC** | 3 (base 1 + qty-check +1 + hasWorkingStop-check +1; delegates else branches to helpers) |

---

## CYC Summary Table

| Symbol | Projected CYC | Jane Street Threshold | Status |
|---|---|---|---|
| `AuditMaster_HandleNakedPosition` (parent) | 3 | 8 | PASS |
| `AuditMaster_HasWorkingStopOrder` | 1 | 8 | PASS |
| `AuditMaster_StartNakedGraceWindow` | 1 | 8 | PASS |
| `AuditMaster_TriggerNakedStopIfGraceExpired` | 3 | 8 | PASS |

**max_cyc_projected: 3** (well under limit of 8)
**extraction_count: 3**

---

## Jane Street KB Alignment

### gjengset — Cache line / False sharing / Left-Right pattern

- `Account.Orders.ToArray()` snapshot **must be preserved** in `AuditMaster_HasWorkingStopOrder()`. This H13-FIX prevents `InvalidOperationException` when NinjaTrader's UI thread mutates `Account.Orders` during iteration (false-sharing / concurrent collection race equivalent).
- `_nakedPositionFirstSeen` uses `ConcurrentDictionary<string, DateTime>` with `TryGetValue` / `TryRemove` — atomic, lock-free ops. **No `lock()` block may be introduced.** This is the Left-Right equivalent: concurrent readers + single writer, no synchronization primitive overhead.
- `_reaperNakedStopInFlight` uses `TryRemove` (atomic). The circuit-breaker in the catch block must remain ConcurrentDictionary atomic, not restructured to a lock.

### carl_cook — Hot-path zero-alloc / Cold logging out-of-line

- `AuditMaster_HasWorkingStopOrder()` is on the **hot path** (invoked every audit cycle). Mark `[AggressiveInlining]` to eliminate call overhead. The LINQ `.Any()` with snapshot is zero-alloc beyond the array copy (required for thread-safety).
- `AuditMaster_StartNakedGraceWindow()` is **cold path** (once per new naked position): mark `[NoInlining]` to keep the instruction cache footprint of the hot path minimal. Cold `Print(string.Format(...))` logging is fully out-of-line.
- `AuditMaster_TriggerNakedStopIfGraceExpired()` is **cold path** (emergency stop): mark `[NoInlining]`. The `TriggerCustomEvent` call and catch block are cold-path side effects.

### trading_billions — Defense in depth / Single responsibility / Circuit breaker

- Each extracted helper has exactly one responsibility (detect / start-grace / trigger-stop). No helper does two things.
- Parent method retains the outer `masterActualQty != 0` guard as the first line of defense — delegates all logic to typed helpers.
- `_reaperNakedStopInFlight.TryRemove` in the catch block is the **rate-limit circuit breaker**: if `TriggerCustomEvent` fails, the in-flight token is cleared so the next audit cycle can retry. This pattern must be preserved intact in `AuditMaster_TriggerNakedStopIfGraceExpired`.

---

## Scope Boundary Compliance

| Check | Status |
|---|---|
| Parent method signature unchanged | PASS |
| All helpers are `private` in same partial class | PASS |
| Caller `AuditMasterAccountIfNeeded` not modified | PASS |
| No cross-file changes | PASS |
| V12.23 No Scope Creep | PASS |
| max_cyc_projected ≤ 8 | PASS (3) |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T00:58:00Z |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-081 |
| **Method** | AuditMaster_HandleNakedPosition |
| **MCP Tools Used** | resolve_repo, index_folder, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates, search_symbols |
| **Sequential Thinking Steps** | 4 (probe + 4 design thoughts) |
| **extraction_count** | 3 |
| **max_cyc_projected** | 3 |
| **Output** | docs/brain/EPIC-W7-081/02-architecture-plan.md |
