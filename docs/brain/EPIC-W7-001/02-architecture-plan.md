# EPIC-W7-001 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:00:00Z
**Input:** docs/brain/EPIC-W7-001/01-scope-boundary.md

---

## Summary

**Target method:** `ShouldSkipFleet_RunHealthCheck` in `src/V12_002.SIMA.Fleet.cs`
**Original CYC (baseline):** 31
**Current CYC (post T-W1 partial refactor):** 8 (parent), but `LogHealthCheckResult` callee = **12** (violates ≤8 rule)
**Goal:** All methods in the cluster ≤8

---

## Current State (Per jCodemunch Index + Source Verification)

A prior wave (T-W1) already extracted the original CYC=31 monolith into a cluster of helpers.
The parent coordinator now sits at CYC=8, but one extracted helper violates the threshold:

| Method | File | CYC | Status |
|---|---|---|---|
| `ShouldSkipFleet_RunHealthCheck` | `src/V12_002.SIMA.Fleet.cs:478` | 8 | AT BOUNDARY (≤8, no change needed) |
| `IsBrokerPositionFlat` | `src/V12_002.SIMA.Fleet.cs:516` | 6 | PASS ✅ |
| `HasActiveFsmForAccount` | `src/V12_002.SIMA.Fleet.cs:539` | 7 | PASS ✅ |
| `HasActivePositionForAccount` | `src/V12_002.SIMA.Fleet.cs:565` | ~4 | PASS ✅ |
| `LogHealthCheckResult` | `src/V12_002.SIMA.Fleet.cs:581` | **12** | **FAIL ❌ — extraction target** |
| `ShouldSkipFleet_IsConsistencyLockHit` | `src/V12_002.SIMA.Fleet.cs:619` | ~3 | PASS ✅ |

**Root cause of CYC=12 in `LogHealthCheckResult`:**
1. `if (brokerFlat && !hasActiveFsm && !hasActivePosition && !hasDispatchPending)` → 4 boolean short-circuit paths
2. `else if (brokerFlat && (hasActiveFsm || hasActivePosition || hasDispatchPending))` → 3 paths
3. Inline nested ternary in string.Format: `hasActiveFsm ? "FSM active" : (hasDispatchPending ? "..." : "...")` → 2 paths
4. if/elif structure → +2 paths
5. Base path → +1

Total: ~12 confirmed by jCodemunch.

---

## Extraction Plan

**Primary extraction target:** `LogHealthCheckResult` (CYC 12 → CYC 4 after extraction)
**Extraction count:** 5 new private helper methods

### Helper 1: `IsAccountTrulyFlat`

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

- **Responsibility:** Pure predicate — "account is broker-flat with zero active state"
- **Projected CYC:** 5 (4 boolean conditions + base) ✅
- **Jane Street:** AggressiveInlining — hot predicate path, zero-alloc, no heap
- **Parameters:** 4 bools (value types — stack only)

---

### Helper 2: `HasAnyActiveState`

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

- **Responsibility:** Pure predicate — "any of FSM/position/dispatch is active"
- **Projected CYC:** 4 (3 OR conditions + base) ✅
- **Jane Street:** AggressiveInlining — hot predicate, zero-alloc
- **Parameters:** 3 bools (value types — stack only)

---

### Helper 3: `BuildHealthCheckSkipReason`

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

- **Responsibility:** Pure string label selector — returns reason string for health check skip log
- **Projected CYC:** 3 (2 if branches + base) ✅
- **Jane Street:** AggressiveInlining — compile-time string constants, zero heap allocation
- **Note:** Replaced nested ternary with explicit if-returns for clarity (same CYC, better readability per trading_billions single-responsibility principle)

---

### Helper 4: `LogHealthCheck_TrulyFlat`

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private static void LogHealthCheck_TrulyFlat(string accountName, StringBuilder dispatchLog)
{
    dispatchLog.AppendLine(
        string.Format(
            "[DISPATCH] H-13: {0} broker flat, no FSM/position/dispatch -- no action",
            accountName
        )
    );
}
```

- **Responsibility:** Cold-path diagnostic log writer — "truly flat, nothing to do" case
- **Projected CYC:** 2 (1 AppendLine call + base) ✅
- **Jane Street:** NoInlining — cold diagnostic path per carl_cook "extract cold logging out-of-line" pattern

---

### Helper 5: `LogHealthCheck_FlatWithActiveState`

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

- **Responsibility:** Cold-path diagnostic log writer — "flat but active state present" case
- **Projected CYC:** 2 (1 AppendLine call + base) ✅
- **Jane Street:** NoInlining — cold diagnostic path

---

### Refactored: `LogHealthCheckResult` (after extraction)

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

- **Projected CYC:** 4 (base 1 + if +1 + else-if +1 + AND condition +1) ✅
- **Signature unchanged** — callers (`ShouldSkipFleet_RunHealthCheck`) unmodified

---

## Complete Cluster CYC Summary (Post-Extraction)

| Method | CYC Before | CYC After | Status |
|---|---|---|---|
| `ShouldSkipFleet_RunHealthCheck` | 8 | 8 | NO CHANGE (already ≤8) |
| `IsBrokerPositionFlat` | 6 | 6 | NO CHANGE |
| `HasActiveFsmForAccount` | 7 | 7 | NO CHANGE |
| `HasActivePositionForAccount` | ~4 | ~4 | NO CHANGE |
| `LogHealthCheckResult` | **12** | **4** | EXTRACTED |
| `IsAccountTrulyFlat` | n/a | **5** | NEW |
| `HasAnyActiveState` | n/a | **4** | NEW |
| `BuildHealthCheckSkipReason` | n/a | **3** | NEW |
| `LogHealthCheck_TrulyFlat` | n/a | **2** | NEW |
| `LogHealthCheck_FlatWithActiveState` | n/a | **2** | NEW |

**max_cyc_projected: 8** ✅ (ShouldSkipFleet_RunHealthCheck — unchanged, at boundary)
**Extraction count: 5** new helpers extracted from `LogHealthCheckResult`

---

## Jane Street Alignment Notes

### gjengset — Cache line / zero-alloc / false sharing
- All new boolean predicate helpers (`IsAccountTrulyFlat`, `HasAnyActiveState`) operate on value-type params (bools), zero heap allocation, no cache-line pressure.
- `BuildHealthCheckSkipReason` returns interned string literals — zero heap allocation (compile-time constants).
- The ToArray() snapshot in `IsBrokerPositionFlat` is diagnostic/cold path — acceptable heap allocation outside the hot dispatch lane.

### carl_cook — Hot path zero-alloc / AggressiveInlining hot / NoInlining cold
- `IsAccountTrulyFlat`, `HasAnyActiveState`, `BuildHealthCheckSkipReason`: `[AggressiveInlining]` — boolean/string-constant predicate helpers on the hot diagnostic path.
- `LogHealthCheck_TrulyFlat`, `LogHealthCheck_FlatWithActiveState`: `[NoInlining]` — contain `string.Format` + `StringBuilder.AppendLine`, cold diagnostic logging path. Extracted out-of-line per carl_cook pattern.

### trading_billions — Defense in depth / single responsibility / circuit breaker
- Each helper has exactly ONE responsibility (see table above).
- Defense in depth: null safety guard `if (acct == null || acct.Positions == null)` remains in the parent coordinator.
- Single responsibility per helper achieved across all 5 new methods.
- No circuit-breaker logic needed here — the health check is diagnostic-only (RETURNS VOID), not a skip decision path.

---

## Scope Compliance (V12.23)

- Target method: `ShouldSkipFleet_RunHealthCheck` ✅
- All 5 new helpers: private, same class (`V12_002` partial class), same file `src/V12_002.SIMA.Fleet.cs` ✅
- No caller modifications (ShouldSkipFleetAccount unchanged) ✅
- No cross-file changes ✅
- No sibling method modifications ✅

---

## MCP Evidence

### jCodemunch: resolve_repo
- **Result:** `antigravityos187-sketch/universal-or-strategy` — indexed, 5120 symbols, 2000 files, CSharp among languages ✅

### jCodemunch: get_context_bundle
- **Symbol found at:** `src/V12_002.SIMA.Fleet.cs:478`
- **Key finding:** Method body already lean after T-W1 — delegates to `IsBrokerPositionFlat`, `HasActiveFsmForAccount`, `HasActivePositionForAccount`, `LogHealthCheckResult`
- **Index freshness:** `stale_index` — symbol complexity reflects pre-T-W1 state in some metrics

### jCodemunch: get_call_hierarchy
- **Callers (1):** `ShouldSkipFleetAccount` at line 450 — confirmed single caller, not to be modified
- **Callees (10):** `IsBrokerPositionFlat`, `HasActiveFsmForAccount`, `HasActivePositionForAccount`, `LogHealthCheckResult`, `ExpKey`, `_dispatchSyncPendingExpKeys`, `LogBuffer.Format`
- **Key finding:** Call graph is self-contained within `V12_002.SIMA.Fleet.cs` — no cross-file method calls that would complicate extraction

### jCodemunch: get_dependency_graph
- **Result:** `node_count=1, edge_count=0` — `src/V12_002.SIMA.Fleet.cs` has NO import edges to other files
- **Key finding:** File is dependency-isolated — safe to add private helpers without cross-file entanglement

### jCodemunch: get_extraction_candidates
- **Result:** No candidates returned (min_callers=1, min_complexity=5)
- **Interpretation:** jCodemunch extraction candidates require multi-file callers; our extraction is single-file. Manual analysis via get_symbol_complexity used instead.

### jCodemunch: get_symbol_complexity (4 symbols measured)
- `ShouldSkipFleet_RunHealthCheck`: CYC=8, nesting=4, lines=34
- `IsBrokerPositionFlat`: CYC=6, nesting=3, lines=18
- `HasActiveFsmForAccount`: CYC=7, nesting=3, lines=21
- `LogHealthCheckResult`: **CYC=12**, nesting=4, lines=30 — **confirmed extraction target**

---

## Sequential Thinking Evidence

### Thought 1 (probe)
Sequential-thinking MCP confirmed healthy — response received with thoughtHistoryLength=7.

### Thought 2 — Complexity Drivers
Identified that the CYC=12 in `LogHealthCheckResult` is driven by:
(1) 4-condition AND boolean expression in first branch (+4 paths),
(2) 3-condition OR boolean expression in second branch (+3 paths),
(3) nested ternary for string label selection (+2 paths),
(4) if/elif structure (+2 paths),
(5) base path (+1).
Confirmed total matches jCodemunch CYC=12 measurement.

### Thought 3 — Extraction Strategy
Designed 5-helper extraction plan. Key decision: splitting compound boolean conditions into named predicate helpers (`IsAccountTrulyFlat`, `HasAnyActiveState`) reduces LogHealthCheckResult CYC from 12 to 4. String label ternary extracted to `BuildHealthCheckSkipReason` (CYC=3). Two cold-path log writers extracted as `LogHealthCheck_TrulyFlat` and `LogHealthCheck_FlatWithActiveState` (CYC=2 each). All projected CYCs ≤8.

### Thought 4 — Jane Street Alignment
Validated all 5 helpers against gjengset (zero-alloc, no false sharing), carl_cook (AggressiveInlining on hot predicates, NoInlining on cold log writers), and trading_billions (single responsibility per helper, defense-in-depth null guards remain in parent). Full alignment confirmed.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 2.5 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-001 |
| **Input** | docs/brain/EPIC-W7-001/01-scope-boundary.md |
| **Output** | docs/brain/EPIC-W7-001/02-architecture-plan.md |
| **MCP Tools Used** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates, get_symbol_complexity (x4), get_symbol_source (x6) |
| **Sequential Thoughts** | 4 |
| **max_cyc_projected** | 8 |
| **extraction_count** | 5 |
