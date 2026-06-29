# Phase 2 Architecture Plan — EPIC-W7-100
## Method: ClosePositionsOnlyApexAccounts
## Source: src/V12_002.SIMA.Flatten.cs
## Agent: v12-phase2-architecture

---

## Complexity Analysis

- **Tool-reported CYC:** 0 (measurement artifact — tool cannot compute CYC for this method)
- **Manual CYC:** 10

**CYC drivers (manual count from source lines 516–589):**
| # | Branch | Type | +Delta |
|---|--------|------|--------|
| 1 | Base complexity | baseline | 1 |
| 2 | `if (!EnableSIMA)` early return | if | +1 |
| 3 | `foreach (Account acct in snapshot)` | loop | +1 |
| 4 | `if (!IsFleetAccount(acct)) continue` | if | +1 |
| 5 | `if (!masterCovered && ...)` condition | if | +1 |
| 6 | `&&` logical-AND in master condition | logical-op | +1 |
| 7 | `if (!_pendingFlattenOps.IsEmpty)` | if | +1 |
| 8 | `catch (InvalidOperationException ex) when (...)` | catch | +1 |
| 9 | `when (ex.Message.Contains("TriggerCustomEvent"))` | exception filter | +1 |
| 10 | `catch (Exception ex)` | catch | +1 |
| **Total** | | | **10** |

**Threshold:** Jane Street standard ≤ 8. Manual CYC = 10 → **EXTRACTION REQUIRED.**

---

## Extraction Plan

| Helper Method | Signature | CYC | Attribute | Rationale |
|---|---|---|---|---|
| `EnqueueFleetAccountFlattenOps` | `private void EnqueueFleetAccountFlattenOps(Account[] snapshot, ref int enqueued)` | 3 | `[MethodImpl(MethodImplOptions.NoInlining)]` | Isolates fleet enumeration loop (foreach + IsFleetAccount guard + Enqueue). Single responsibility: populate queue for all fleet accounts. |
| `EnqueueMasterAccountFallbackFlatten` | `private void EnqueueMasterAccountFallbackFlatten(ref int enqueued)` | 3 | `[MethodImpl(MethodImplOptions.NoInlining)]` | Isolates master-account fallback guard (`!masterCovered && Positions.Count > 0` + Enqueue). Single responsibility: ensure master account is covered. |
| `TriggerOrFallbackFlattenExecution` | `private void TriggerOrFallbackFlattenExecution()` | 5 | `[MethodImpl(MethodImplOptions.NoInlining)]` | Isolates the trigger/catch/fallback block: `if (!IsEmpty)` + `try { TriggerCustomEvent }` + 2 catch handlers + `else` path. Single responsibility: fire or fallback-flatten. |

**Residual parent CYC after extraction:**
- `if (!EnableSIMA)` early return: +1
- Calls to 3 helpers (no branching in caller): 0
- **Residual parent CYC = 2** ✓

---

## max_cyc_projected: 5 (must be <= 8) ✓

---

## Jane Street Alignment

- **carl_cook:** `FlattenWorkItem` is a struct — zero-alloc enqueue preserved. No LINQ introduced. Print/logging calls are in the extracted cold-path helpers (`[NoInlining]` applied). No new heap allocations.
- **gjengset:** No new `lock()` blocks introduced. `isFlattenRunning` field writes remain in same locations (inside `TriggerOrFallbackFlattenExecution` catch handlers). No synchronization changes.
- **trading_billions:** Each extracted helper has a single responsibility. CYC of each helper ≤ 8 (max = 5). Defense in depth maintained: fallback flatten preserved in `TriggerOrFallbackFlattenExecution`. Rate-limit circuit breaker (`isFlattenRunning` guard) preserved in residual parent.

---

## MCP Evidence

- **resolve_repo:** `antigravityos187-sketch/universal-or-strategy` — indexed, 5147 symbols, 2000 files, loadable.
- **get_symbol_source:** Symbol confirmed at `src/V12_002.SIMA.Flatten.cs` line 516–589 (74 lines). Full source retrieved and manually analyzed.
- **get_call_hierarchy:** 0 callers found (private method, not referenced by indexed symbols); 23 callees across depth-2 traversal including `IsFleetAccount`, `_pendingFlattenOps.Enqueue`, `PumpFlattenOps`, `PerformFallbackFlatten`, `Print/Format`.
- **get_dependency_graph:** `src/V12_002.SIMA.Flatten.cs` has 0 import edges and 0 importer edges in the dependency graph (partial class — dependencies resolved at compile time, not via file-level imports).

---

## Sequential Thinking Evidence

- **Thought 1:** Manual branch counting from actual source: 10 decision points (if, foreach, &&, two catch handlers, exception filter). CYC = 10 > 8 threshold. Extraction required. Scope-boundary CYC=106 was a file/method-aggregate estimate; actual method CYC is 10.
- **Thought 2:** Three extraction targets identified: (1) fleet enumeration loop → CYC=3; (2) master fallback guard → CYC=3; (3) trigger/catch/fallback block → CYC=5. Residual parent → CYC=2. All helpers single-responsibility per trading_billions mandate. No lock() introduced (gjengset). Struct FlattenWorkItem preserved zero-alloc (carl_cook).
- **Thought 3:** Validation — all helpers CYC ≤ 8 (max=5), parent CYC=2. V12.23 compliance: all 3 helpers are private methods in same partial class, no public interface changes, no caller modifications. max_cyc_projected = 5. PASS.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Epic** | EPIC-W7-100 |
| **Method** | ClosePositionsOnlyApexAccounts |
| **Source File** | src/V12_002.SIMA.Flatten.cs |
| **Tool CYC** | 0 (artifact) |
| **Manual CYC** | 10 |
| **Helpers Extracted** | 3 |
| **max_cyc_projected** | 5 |
| **Phase** | 2 |
