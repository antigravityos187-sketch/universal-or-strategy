# EPIC-W7-001 — Phase 4: Ticket Generation

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:10:00Z
**Inputs:**
- `docs/brain/EPIC-W7-001/02-architecture-plan.md`
- `docs/brain/EPIC-W7-001/03-audit-report.md`

---

## Summary

**Target method:** `ShouldSkipFleet_RunHealthCheck` in `src/V12_002.SIMA.Fleet.cs`
**Extraction target:** `LogHealthCheckResult` — CYC 12 → CYC 4 after all tickets complete
**ticket_count:** 6
**Execution order:** T1–T5 may run in any order (independent); T6 must run last (depends on T1–T5)

---

## Cluster CYC Status

| Method | CYC Before | CYC After | Ticket(s) |
|---|---|---|---|
| `ShouldSkipFleet_RunHealthCheck` | 8 | 8 | NONE — at boundary, unchanged |
| `IsBrokerPositionFlat` | 6 | 6 | NONE — pass |
| `HasActiveFsmForAccount` | 7 | 7 | NONE — pass |
| `HasActivePositionForAccount` | ~4 | ~4 | NONE — pass |
| `LogHealthCheckResult` | **12** | **4** | T6 (wiring + tests) |
| `IsAccountTrulyFlat` | n/a | **5** | T1 (new helper) |
| `HasAnyActiveState` | n/a | **4** | T2 (new helper) |
| `BuildHealthCheckSkipReason` | n/a | **3** | T3 (new helper) |
| `LogHealthCheck_TrulyFlat` | n/a | **2** | T4 (new helper) |
| `LogHealthCheck_FlatWithActiveState` | n/a | **2** | T5 (new helper) |

**projected_parent_cyc_after_all:** 4 (`LogHealthCheckResult` after T6)
**max_cyc_in_cluster_after_all:** 8 (`ShouldSkipFleet_RunHealthCheck`, unchanged, at boundary)

---

## Tickets

---

### Ticket T1 — Extract `IsAccountTrulyFlat`

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **helper_name** | `IsAccountTrulyFlat` |
| **concern** | Pure boolean predicate — "account is broker-flat with zero active state (no FSM, no position, no dispatch pending)" |
| **source_method** | `LogHealthCheckResult` in `src/V12_002.SIMA.Fleet.cs:581` |
| **lines_to_move** | The 4-condition AND boolean expression `brokerFlat && !hasActiveFsm && !hasActivePosition && !hasDispatchPending` from `LogHealthCheckResult`'s first `if`-branch condition. Extracted into a new `private static` method accepting 4 `bool` parameters (all value-type, stack-only). |
| **cyc_reduction** | 4 (4 compound AND short-circuit conditions collapse to a single method-call leaf in `LogHealthCheckResult`) |
| **projected_helper_cyc** | 5 (conservative McCabe: 4 boolean short-circuit conditions + base path) |
| **decorator** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — hot predicate path |
| **scope** | Private static, same class (`V12_002` partial class), same file `src/V12_002.SIMA.Fleet.cs` |
| **dependencies** | None — independent, can run first |

**Planned method signature:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsAccountTrulyFlat(
    bool brokerFlat,
    bool hasActiveFsm,
    bool hasActivePosition,
    bool hasDispatchPending)
{
    return brokerFlat && !hasActiveFsm && !hasActivePosition && !hasDispatchPending;
}
```

**Jane Street alignment:** Zero-alloc (4 bool value-type params, stack only). `AggressiveInlining` per carl_cook hot-predicate pattern.

---

### Ticket T2 — Extract `HasAnyActiveState`

| Field | Value |
|---|---|
| **ticket_id** | T2 |
| **helper_name** | `HasAnyActiveState` |
| **concern** | Pure boolean predicate — "at least one of FSM / position / dispatch-pending is active" |
| **source_method** | `LogHealthCheckResult` in `src/V12_002.SIMA.Fleet.cs:581` |
| **lines_to_move** | The 3-condition OR expression `hasActiveFsm \|\| hasActivePosition \|\| hasDispatchPending` from `LogHealthCheckResult`'s `else if` branch condition. Extracted into a new `private static` method accepting 3 `bool` parameters. |
| **cyc_reduction** | 3 (3 OR short-circuit conditions collapse to a single method-call leaf) |
| **projected_helper_cyc** | 4 (3 OR conditions + base path) |
| **decorator** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — hot predicate path |
| **scope** | Private static, same class, same file |
| **dependencies** | None — independent |

**Planned method signature:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool HasAnyActiveState(
    bool hasActiveFsm,
    bool hasActivePosition,
    bool hasDispatchPending)
{
    return hasActiveFsm || hasActivePosition || hasDispatchPending;
}
```

**Jane Street alignment:** Zero-alloc. `AggressiveInlining` per carl_cook hot-predicate pattern.

---

### Ticket T3 — Extract `BuildHealthCheckSkipReason`

| Field | Value |
|---|---|
| **ticket_id** | T3 |
| **helper_name** | `BuildHealthCheckSkipReason` |
| **concern** | String label selector — returns a human-readable reason string for the health check skip log message |
| **source_method** | `LogHealthCheckResult` in `src/V12_002.SIMA.Fleet.cs:581` |
| **lines_to_move** | The nested ternary expression `hasActiveFsm ? "FSM active" : (hasDispatchPending ? "..." : "...")` from `LogHealthCheckResult`'s `else if` branch body. Replaced with explicit `if`-return pattern in the extracted helper (same CYC, better readability per trading_billions single-responsibility principle). |
| **cyc_reduction** | 2 (nested ternary with 2 decision points collapses to a single method-call) |
| **projected_helper_cyc** | 3 (2 if-return branches + base path) |
| **decorator** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — returns interned compile-time string constants |
| **scope** | Private static, same class, same file |
| **dependencies** | None — independent |

**Planned method signature:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static string BuildHealthCheckSkipReason(
    bool hasActiveFsm,
    bool hasDispatchPending,
    bool hasActivePosition)
{
    if (hasActiveFsm) return "FSM active";
    if (hasDispatchPending) return "dispatch pending";
    return "activePos present";
}
```

**Jane Street alignment:** Returns interned string literals — zero heap allocation. `AggressiveInlining` per carl_cook. ASCII-only string literals ✅.

---

### Ticket T4 — Extract `LogHealthCheck_TrulyFlat`

| Field | Value |
|---|---|
| **ticket_id** | T4 |
| **helper_name** | `LogHealthCheck_TrulyFlat` |
| **concern** | Cold-path diagnostic log writer — "account is truly flat, no FSM/position/dispatch, no action required" case |
| **source_method** | `LogHealthCheckResult` in `src/V12_002.SIMA.Fleet.cs:581` |
| **lines_to_move** | The `dispatchLog.AppendLine(string.Format(...))` call inside the first `if` block body of `LogHealthCheckResult` (format string: `"[DISPATCH] H-13: {0} broker flat, no FSM/position/dispatch -- no action"`). Extracted out-of-line as a cold-path diagnostic writer. |
| **cyc_reduction** | 1 (one branch body extracted out-of-line; condition expression remains in parent) |
| **projected_helper_cyc** | 2 (1 AppendLine call + base path) |
| **decorator** | `[MethodImpl(MethodImplOptions.NoInlining)]` — cold diagnostic path |
| **scope** | Private static, same class, same file |
| **dependencies** | None — independent |

**Planned method signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private static void LogHealthCheck_TrulyFlat(
    string accountName,
    StringBuilder dispatchLog)
{
    dispatchLog.AppendLine(
        string.Format(
            "[DISPATCH] H-13: {0} broker flat, no FSM/position/dispatch -- no action",
            accountName
        )
    );
}
```

**Jane Street alignment:** `NoInlining` per carl_cook "extract cold logging out-of-line" pattern. ASCII-only string literals ✅.

---

### Ticket T5 — Extract `LogHealthCheck_FlatWithActiveState`

| Field | Value |
|---|---|
| **ticket_id** | T5 |
| **helper_name** | `LogHealthCheck_FlatWithActiveState` |
| **concern** | Cold-path diagnostic log writer — "flat but active state present, skip reset" case |
| **source_method** | `LogHealthCheckResult` in `src/V12_002.SIMA.Fleet.cs:581` |
| **lines_to_move** | The `dispatchLog.AppendLine(string.Format(...))` call inside the `else if` block body of `LogHealthCheckResult` (format string: `"[DISPATCH] H-13 SKIP: {0} Flat but {1} -- not resetting"`). Extracted out-of-line as a cold-path diagnostic writer. |
| **cyc_reduction** | 1 (one branch body extracted out-of-line; condition expression remains in parent) |
| **projected_helper_cyc** | 2 (1 AppendLine call + base path) |
| **decorator** | `[MethodImpl(MethodImplOptions.NoInlining)]` — cold diagnostic path |
| **scope** | Private static, same class, same file |
| **dependencies** | None — independent |

**Planned method signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private static void LogHealthCheck_FlatWithActiveState(
    string accountName,
    string skipReason,
    StringBuilder dispatchLog)
{
    dispatchLog.AppendLine(
        string.Format(
            "[DISPATCH] H-13 SKIP: {0} Flat but {1} -- not resetting",
            accountName,
            skipReason
        )
    );
}
```

**Jane Street alignment:** `NoInlining` per carl_cook cold-path pattern. ASCII-only string literals ✅.

---

### Ticket T6 — Refactor `LogHealthCheckResult` + xUnit Tests

| Field | Value |
|---|---|
| **ticket_id** | T6 |
| **helper_name** | n/a (parent refactor + test ticket) |
| **concern** | (1) Wire the 5 extracted helpers (T1–T5) into the refactored `LogHealthCheckResult` body. (2) Author xUnit `[Fact]` tests for all 5 new helpers and the refactored parent. |
| **source_method** | `LogHealthCheckResult` in `src/V12_002.SIMA.Fleet.cs:581` |
| **lines_to_move** | Entire `LogHealthCheckResult` body REPLACED with delegation calls to `IsAccountTrulyFlat`, `HasAnyActiveState`, `BuildHealthCheckSkipReason`, `LogHealthCheck_TrulyFlat`, `LogHealthCheck_FlatWithActiveState`. Signature of `LogHealthCheckResult` UNCHANGED (callers unmodified). |
| **cyc_reduction** | 8 (LogHealthCheckResult: 12 → 4; net CYC removed from parent = 8) |
| **projected_helper_cyc** | n/a (parent wiring; projected LogHealthCheckResult CYC = 4) |
| **dependencies** | **Requires T1, T2, T3, T4, T5 all complete** |

**Refactored `LogHealthCheckResult` body:**
```csharp
private void LogHealthCheckResult(
    string accountName,
    bool brokerFlat,
    bool hasActiveFsm,
    bool hasActivePosition,
    bool hasDispatchPending,
    StringBuilder dispatchLog)
{
    if (IsAccountTrulyFlat(brokerFlat, hasActiveFsm, hasActivePosition, hasDispatchPending))
    {
        LogHealthCheck_TrulyFlat(accountName, dispatchLog);
    }
    else if (brokerFlat && HasAnyActiveState(hasActiveFsm, hasActivePosition, hasDispatchPending))
    {
        string reason = BuildHealthCheckSkipReason(hasActiveFsm, hasDispatchPending, hasActivePosition);
        LogHealthCheck_FlatWithActiveState(accountName, reason, dispatchLog);
    }
}
```

**xUnit test coverage required (all `[Fact]`, xUnit only — no NUnit/MSTest):**
- `IsAccountTrulyFlat`: test all 4 boolean path combinations (trulyFlat=true, brokerNotFlat, hasActiveFsm, hasActivePosition, hasDispatchPending)
- `HasAnyActiveState`: test all 3 OR paths + all-false path
- `BuildHealthCheckSkipReason`: test each of the 3 return values (FSM active, dispatch pending, activePos present)
- `LogHealthCheck_TrulyFlat`: test AppendLine writes correct format string
- `LogHealthCheck_FlatWithActiveState`: test AppendLine writes correct format string with substitution
- `LogHealthCheckResult`: integration test — test both if-branches and fall-through (no-op) path

**Jane Street alignment:** Signature unchanged — zero caller impact. `ShouldSkipFleetAccount` (sole caller) unmodified. Defense-in-depth null guard in parent coordinator (`ShouldSkipFleet_RunHealthCheck`) remains in place.

---

## Execution Order

```
T1 (IsAccountTrulyFlat)
T2 (HasAnyActiveState)       <- can run in parallel with T1, T3, T4, T5
T3 (BuildHealthCheckSkipReason)
T4 (LogHealthCheck_TrulyFlat)
T5 (LogHealthCheck_FlatWithActiveState)
        |
        v (all T1-T5 must be complete)
T6 (Refactor LogHealthCheckResult + xUnit tests)
```

---

## CYC Projection Verification

| Method | CYC Before | CYC After | <= 8? |
|---|---|---|---|
| `ShouldSkipFleet_RunHealthCheck` | 8 | 8 | ✅ (unchanged) |
| `LogHealthCheckResult` | 12 | 4 | ✅ (T6) |
| `IsAccountTrulyFlat` | n/a | 5 | ✅ (T1) |
| `HasAnyActiveState` | n/a | 4 | ✅ (T2) |
| `BuildHealthCheckSkipReason` | n/a | 3 | ✅ (T3) |
| `LogHealthCheck_TrulyFlat` | n/a | 2 | ✅ (T4) |
| `LogHealthCheck_FlatWithActiveState` | n/a | 2 | ✅ (T5) |

**projected_parent_cyc_after_all:** 4
**max_cyc_in_cluster_after_all:** 8

All methods comply with CYC ≤ 8 mandate. ✅

---

## MCP Evidence

### jCodemunch: resolve_repo
```json
{
  "found": true,
  "indexed": true,
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "symbol_count": 5147,
  "file_count": 2000,
  "status": "loadable"
}
```

### jCodemunch: get_symbol_complexity
- **Result:** Symbol not found in index (stale index — consistent with Phase 2 finding)
- **Fallback:** Phase 2 `get_symbol_complexity` results used:
  - `ShouldSkipFleet_RunHealthCheck`: CYC=8, nesting=4, lines=34
  - `LogHealthCheckResult`: CYC=12, nesting=4, lines=30

### jCodemunch: get_extraction_candidates
- **Result:** `candidates=[]` (min_callers=1, min_complexity=5)
- **Interpretation:** Single-file extraction; consistent with Phase 2 finding (jCodemunch extraction candidates require multi-file callers). Manual analysis via architecture plan used.

### Sequential Thinking
- 4 thoughts completed (thoughtHistoryLength advanced to 11)
- Thought 1: Ticket count determination → 6 tickets
- Thought 2: Per-ticket line mapping, helper signatures, CYC reduction analysis
- Thought 3: Full CYC verification grid — all 7 methods ≤ 8 post-extraction confirmed
- Thought 4: Final ticket synthesis and execution ordering

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.2 |
| **Execution Time** | batch (parallel MCP calls) |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-001 |
| **Inputs** | docs/brain/EPIC-W7-001/02-architecture-plan.md, docs/brain/EPIC-W7-001/03-audit-report.md |
| **Output** | docs/brain/EPIC-W7-001/04-tickets.md |
| **MCP Tools Used** | resolve_repo, sequentialthinking (x4+probe), get_symbol_complexity, get_extraction_candidates |
| **Sequential Thoughts** | 4 (substantive) + 1 (probe) |
| **ticket_count** | 6 |
| **projected_parent_cyc_after_all** | 4 |
| **max_cyc_in_cluster_after_all** | 8 |
