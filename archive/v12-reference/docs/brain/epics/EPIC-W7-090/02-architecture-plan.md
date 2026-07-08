# Phase 2: Architecture Plan -- EPIC-W7-090

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2
**Generated:** 2026-06-29T03:00:00Z
**Input:** docs/brain/EPIC-W7-090/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `OnWatchdogTimer`
- **Source File:** `src/V12_002.Safety.Watchdog.cs`
- **Original CYC:** 11 (true McCabe count from source; 00-scope.md listed 244 from erroneous precomputed.json aggregate; 00-hotspots.md confirmed flat structure but CYC=0 was overly conservative -- actual branch count from source = ~11 including boolean-condition subclauses)
- **Signature:** `private void OnWatchdogTimer(object state)`
- **Lines:** 36-89 (54 LOC)

### jcodemunch get_context_bundle result

Symbol resolved: `src/V12_002.Safety.Watchdog.cs::V12_002.OnWatchdogTimer#method`
Key findings from get_context_bundle:
- Method is 54 LOC with 4 flat early-return guard blocks followed by a two-stage CAS escalation state machine
- Imports: System, System.Collections.Generic, System.Linq, System.Threading, NinjaTrader.Cbi, NinjaTrader.NinjaScript.Strategies
- No docstring; all logic is inline
- Stage-0 path: `Interlocked.CompareExchange(ref _watchdogStage, 1, 0)` -> `Enqueue(ctx => ctx.ExecuteWatchdogLeadAccountFlatten())`
- Stage-1 path: `Interlocked.CompareExchange(ref _watchdogStage, 2, 1)` -> `ExecuteWatchdogDirectFallback()`
- try/catch wraps the Enqueue call with rollback via `Interlocked.Exchange(ref _watchdogStage, 0)`

### jcodemunch get_call_hierarchy result

Direction: both | Depth: 2
**Callers (depth 1):** 0 direct callers (method is a `System.Threading.Timer` callback -- invoked via delegate, not by a named symbol)
**Callees (depth 1):**
- `HasWatchdogLeadAccountWorkingOrder` (src/V12_002.Safety.Watchdog.cs:112) -- ast_resolved
- `Enqueue` (src/V12_002.cs:428) -- ast_inferred
- `ExecuteWatchdogDirectFallback` (src/V12_002.Safety.Watchdog.cs:244) -- ast_resolved

**Callees (depth 2):**
- `IsOrderTerminal` (src/V12_002.Orders.Management.Flatten.cs:698)
- `_cmdQueue` constant (src/V12_002.cs:359)
- `IsActorThread` (src/V12_002.cs:439)
- `TryDrain` (src/V12_002.cs:503)
- `ScheduleActorDrain` (src/V12_002.cs:481)
- `CancelDirectFallbackOrders` (src/V12_002.Safety.Watchdog.cs:268)
- `FlattenDirectFallbackPositions` (src/V12_002.Safety.Watchdog.cs:297)

### jcodemunch get_dependency_graph result

File: `src/V12_002.Safety.Watchdog.cs` | Direction: both | Depth: 1
- **imports:** [] (no explicit file-level import edges resolved -- partial class, compiled with main V12_002.cs)
- **importers:** [] (timer-delegate registration in StartWatchdog, not an import edge)
- **node_count:** 1 | **edge_count:** 0
- Assessment: File is a C# partial class; dependency resolution is handled at compile/partial-class level, not by file import edges. All coupling is intra-class.

### jcodemunch get_extraction_candidates result

Candidates returned: 0 (min_complexity=3, min_callers=1)
Assessment: The index did not surface pre-existing extraction candidates meeting the threshold. This is consistent with the method being a single-caller timer callback. The extraction plan is derived from manual CYC analysis of the method body.

---

## Sequential Thinking Summary

**5-thought chain completed (sequentialthinking calls: 5)**

**Final thought (Thought 5):**
Extraction plan validated. True CYC ~11 exceeds Jane Street threshold of <=8. Three private helper methods extracted to decompose the method into: (1) a guard block (WatchdogShouldSuppressEscalation, CYC=6), (2) a stage-0 CAS escalation block (TryEscalateToStageOne, CYC=4), and (3) a stage-1 CAS escalation block (TryEscalateToStageTwo, CYC=3). Parent shell reduces to CYC=3. All projected CYC values are <=8. Jane Street alignment confirmed: lock-free Actor/Enqueue pattern preserved, all string literals ASCII-only, single-responsibility per helper, no illegal state transitions introduced.

---

## Extraction Plan

| Helper Method Name | Responsibility | Parameters | Return Type | Projected CYC |
|---|---|---|---|---|
| `WatchdogShouldSuppressEscalation` | Handles all 4 early-exit guard conditions: terminatingState check (with stage reset), zero-heartbeat guard, heartbeat-age-within-timeout guard (with stage reset), no-working-order guard (with stage reset). Returns true if escalation should be suppressed. | `(out long heartbeatAge)` or `()` | `bool` | **6** |
| `TryEscalateToStageOne` | CAS 0->1 escalation: checks stage==0, attempts CompareExchange(1,0), on success calls Print + Enqueue(ExecuteWatchdogLeadAccountFlatten), catch rolls back to stage 0 with Print. Returns true if stage==0 path was entered (caller should return regardless of CAS outcome). | `(int stage)` | `bool` | **4** |
| `TryEscalateToStageTwo` | CAS 1->2 escalation: checks stage!=1 guard, attempts CompareExchange(2,1), on success calls Print + ExecuteWatchdogDirectFallback. | `(int stage)` | `void` | **3** |

---

## Parent Method After Extraction

**Remaining logic in `OnWatchdogTimer`:**

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

- **Projected CYC:** 3 (base=1, if-suppress=1, if-stageOne=1)

---

## max_cyc_projected: 6
## extraction_count: 3

---

## xUnit Test Requirements

Each extracted helper requires a `[Fact]` xUnit test:
1. `WatchdogShouldSuppressEscalation_WhenTerminating_ReturnsTrue`
2. `WatchdogShouldSuppressEscalation_WhenHeartbeatHealthy_ReturnsTrue`
3. `WatchdogShouldSuppressEscalation_WhenNoWorkingOrder_ReturnsTrue`
4. `TryEscalateToStageOne_WhenStageZero_EnqueuesAndReturnsTrue`
5. `TryEscalateToStageOne_WhenStageNonZero_ReturnsFalse`
6. `TryEscalateToStageTwo_WhenStageOne_ExecutesFallback`
7. `TryEscalateToStageTwo_WhenStageNotOne_DoesNothing`

---

## Jane Street Alignment

| Check | Status | Notes |
|---|---|---|
| CYC<=8 achieved | YES | max_cyc_projected=6; all helpers <=6; parent=3 |
| Single-responsibility per helper | YES | Guards in one helper; each CAS stage in its own helper |
| Lock-free/Actor pattern preserved | YES | No lock() blocks added; CAS/Interlocked unchanged; Enqueue pattern intact |
| Illegal states unrepresentable | YES | CAS transitions (0->1->2) are atomic; no new state paths introduced |
| ASCII-only string literals | YES | All Print strings are ASCII; no Unicode or curly quotes |
| Extract Guard Clauses pattern | YES | WatchdogShouldSuppressEscalation consolidates all early-exit guards |
| Named helper methods, private scope | YES | All helpers are private, single-purpose |
| No lock() blocks | YES | Zero lock() usage; all synchronization via Interlocked primitives |
| xUnit [Fact] tests required | YES | 7 test cases specified above |
| ONE method per epic | YES | Only OnWatchdogTimer decomposed |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 2.5 |
| **Execution Time** | 2026-06-29T03:00:00Z |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-090 |
| **Output** | docs/brain/EPIC-W7-090/02-architecture-plan.md |
