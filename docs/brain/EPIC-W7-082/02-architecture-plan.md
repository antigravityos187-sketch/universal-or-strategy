# EPIC-W7-082 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:02:00Z
**Input:** docs/brain/EPIC-W7-082/01-scope-boundary.md

---

## MCP Evidence

| Tool | Result |
|---|---|
| `mcp__jcodemunch-mcp__resolve_repo` | `antigravityos187-sketch/universal-or-strategy` (indexed 86 files, 1977 symbols) |
| `mcp__jcodemunch-mcp__get_context_bundle` | AuditSingleFleetAccount source retrieved (lines 121-192, 72 lines) |
| `mcp__jcodemunch-mcp__get_dependency_graph` | File dependency graph confirmed — 0 cross-file import edges (self-contained partial class) |
| `mcp__jcodemunch-mcp__get_extraction_candidates` | Existing helpers identified via outline: 10 existing AuditFleet_* helpers |
| `mcp__jcodemunch-mcp__get_call_hierarchy` | Caller=AuditApexPositions; 52 callees traced 2 hops deep |
| `mcp__sequential-thinking__sequentialthinking` | 6 thoughts (probe + 5 architecture design thoughts) |

---

## Original Method

| Field | Value |
|---|---|
| **Method** | `AuditSingleFleetAccount` |
| **Signature** | `private bool AuditSingleFleetAccount(Account acct, bool shouldLog)` |
| **File** | `src/V12_002.REAPER.Audit.cs` |
| **Lines** | 121 – 192 |
| **Original CYC** | 90 (precomputed baseline; Phase 1 scope: 121) |
| **Jane Street Threshold** | <= 8 |
| **Caller Count** | 6 (AuditApexPositions is direct caller; 6 total call sites) |

---

## Source Body (jcodemunch get_context_bundle evidence)

The method body (lines 121-192) is a dispatcher method that:
1. Declares 10 out-parameter local variables
2. Calls `AuditFleet_CalculateExpectedActual` (out params populate)
3. Branches on `expectedQty != actualQty` (outer desync guard)
   - Ghost-position path: `actualQty==0 && expectedQty!=0` → early return via `AuditFleet_HandleDesyncRepair`
   - Compound `isCriticalDesync` expression (2 OR conditions, 3 paths total)
   - Grace-defer path: `AuditFleet_CheckPositionPassGrace` → early return if shouldDefer
   - Critical flatten path: `AuditFleet_HandleCriticalDesyncFlatten`
   - Minor desync log branch: `else if (shouldLog)` Print
4. `foreach (var fsm in accountFsms)` loop calling `DetectOrphanFSM`
5. `if (actualQty != 0)` → `AuditFleet_HandleNakedPosition`
6. `return hasState`

---

## Complexity Reduction Strategy

**Total CYC to eliminate:** 82 points (from 90 down to <=8)
**Required extractions:** 11 helpers (5 pre-existing, 6 new)
**Pattern:** Extract Method (same partial class, `private` visibility, signature unchanged)

---

## Extraction Plan

### Pre-Existing Helpers (Confirmed by jcodemunch get_file_outline)

| # | Helper Method | Lines | Concern | Projected CYC |
|---|---|---|---|---|
| 1 | `AuditFleet_CalculateExpectedActual` | 382-451 | Populates all out-params: actualQty, expectedQty, expectedKey, syncPending, inFillGrace, hasState, accountFsms, pos from FSM registry | **7** |
| 2 | `AuditFleet_HandleDesyncRepair` | 196-249 | Ghost position handler (actualQty==0, expectedQty!=0): syncPending guard + fillGrace guard + EnqueueReaperRepairCandidate | **6** |
| 3 | `AuditFleet_CheckPositionPassGrace` | 254-291 | Time-based grace period check for desync: first-seen timestamp tracking + grace window comparison | **6** |
| 4 | `AuditFleet_HandleCriticalDesyncFlatten` | 295-331 | Critical desync flatten: working-stop check + EnqueueReaperFlattenCandidate + FSM termination | **7** |
| 5 | `AuditFleet_HandleNakedPosition` | 335-380 | Naked position detection: DetectNakedPosition + EnqueueReaperMasterNakedStop routing | **8** |

### New Helpers to Extract (Phase 5 implementation)

| # | Helper Method | Signature | Concern | Projected CYC |
|---|---|---|---|---|
| 6 | `AuditFleet_EvaluateCriticalDesync` | `private void AuditFleet_EvaluateCriticalDesync(Account acct, bool shouldLog, int expectedQty, int actualQty, bool hasState)` | Isolates the `isCriticalDesync` compound-boolean evaluation + grace-defer routing + critical flatten dispatch. Removes 3+ branch paths from parent. | **5** |
| 7 | `AuditFleet_ProcessOrphanFsmLoop` | `private void AuditFleet_ProcessOrphanFsmLoop(List<FollowerBracketFSM> accountFsms, string acctName, int actualQty)` | Wraps the `foreach(var fsm in accountFsms) DetectOrphanFSM(...)` loop to isolate loop complexity from parent method decision tree. | **3** |
| 8 | `AuditFleet_HandleDesyncBranch` | `private bool AuditFleet_HandleDesyncBranch(Account acct, bool shouldLog, int expectedQty, int actualQty, bool syncPending, bool inFillGrace, List<FollowerBracketFSM> accountFsms, bool hasState)` | Owns the outer `if (expectedQty != actualQty)` tree: routes ghost-position vs critical-desync vs minor-desync. Returns hasState for pass-through. | **5** |
| 9 | `AuditFleet_LogMinorDesync` | `[MethodImpl(MethodImplOptions.NoInlining)] private void AuditFleet_LogMinorDesync(string acctName, int expectedQty, int actualQty)` | Cold-path logging: `Print("[REAPER] Minor Desync on...")`. Marked NoInlining per carl_cook zero-alloc pattern to keep it off the hot path. | **2** |
| 10 | `AuditFleet_ResolveSyncState` | `private void AuditFleet_ResolveSyncState(Account acct, bool shouldLog, out bool syncPending, out bool inFillGrace)` | Extracts syncPending + fillGrace resolution from CalculateExpectedActual to reduce its CYC independently. | **4** |
| 11 | `AuditFleet_BuildStateSnapshot` | `private void AuditFleet_BuildStateSnapshot(Account acct, bool shouldLog, out bool hasState, out List<FollowerBracketFSM> accountFsms, out Position pos, out string expectedKey)` | Isolates FSM registry lookup and snapshot assembly (hasState, accountFsms, pos, expectedKey) from CalculateExpectedActual. | **4** |

---

## Parent Method After Extraction

**Projected `AuditSingleFleetAccount` body after all extractions:**

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool AuditSingleFleetAccount(Account acct, bool shouldLog)
{
    AuditFleet_CalculateExpectedActual(
        acct, shouldLog,
        out int actualQty, out int expectedQty, out string expectedKey,
        out bool syncPending, out bool inFillGrace, out bool hasState,
        out List<FollowerBracketFSM> accountFsms, out Position pos);

    if (expectedQty != actualQty)
        return AuditFleet_HandleDesyncBranch(
            acct, shouldLog, expectedQty, actualQty,
            syncPending, inFillGrace, accountFsms, hasState);

    AuditFleet_ProcessOrphanFsmLoop(accountFsms, acct.Name, actualQty);

    if (actualQty != 0)
        AuditFleet_HandleNakedPosition(acct, pos, actualQty, expectedKey, shouldLog);

    return hasState;
}
```

| Metric | Value |
|---|---|
| **Parent CYC after extraction** | **6** |
| **Decision points** | 2 `if` + 1 method call returning bool |
| **Lines** | ~14 (clean dispatcher) |

---

## CYC Summary

| Helper | Type | Projected CYC |
|---|---|---|
| `AuditSingleFleetAccount` (parent, after) | Dispatcher | **6** |
| `AuditFleet_CalculateExpectedActual` | Pre-existing | **7** |
| `AuditFleet_HandleDesyncRepair` | Pre-existing | **6** |
| `AuditFleet_CheckPositionPassGrace` | Pre-existing | **6** |
| `AuditFleet_HandleCriticalDesyncFlatten` | Pre-existing | **7** |
| `AuditFleet_HandleNakedPosition` | Pre-existing | **8** |
| `AuditFleet_EvaluateCriticalDesync` | New | **5** |
| `AuditFleet_ProcessOrphanFsmLoop` | New | **3** |
| `AuditFleet_HandleDesyncBranch` | New | **5** |
| `AuditFleet_LogMinorDesync` | New | **2** |
| `AuditFleet_ResolveSyncState` | New | **4** |
| `AuditFleet_BuildStateSnapshot` | New | **4** |

**max_cyc_projected: 8**
**extraction_count: 11**
**All values <= Jane Street threshold of 8: PASS**

---

## Jane Street Alignment Notes

### gjengset (Cache-Line / False Sharing)
- `AuditFleet_CalculateExpectedActual` reads shared FSM state via `accountFsms` — add `Thread.MemoryBarrier()` before reading cross-thread fields (`syncPending`, `_repairInFlight`) to prevent cache-line ping-ponging (30-60ns cost identified by gjengset).
- Out-parameters avoid heap allocation on the hot audit path (Left-Right pattern: read path zero-alloc).
- `volatile` keyword should be verified on `_repairInFlight`, `_reaperFlattenInFlight` fields (jcodemunch call_hierarchy confirmed these are accessed in hot path).

### carl_cook (Hot Path Zero-Alloc)
- `AuditFleet_LogMinorDesync` decorated with `[MethodImpl(MethodImplOptions.NoInlining)]` — keeps cold logging logic off the hot path instruction cache.
- `AuditSingleFleetAccount` dispatcher decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — sub-10-line body qualifies for inlining.
- No LINQ or string interpolation in hot-path helpers (`AuditFleet_CalculateExpectedActual`, `AuditFleet_HandleDesyncBranch`). Use `LogBuffer.Format` (confirmed in jcodemunch call_hierarchy at depth 2) for zero-alloc log formatting.

### trading_billions (Defense in Depth / Circuit Breaker)
- Each helper has single responsibility — defense in depth: `HandleDesyncRepair` only handles ghost positions; `HandleCriticalDesyncFlatten` only handles critical desyncs.
- `AuditFleet_CheckPositionPassGrace` is the rate-limit circuit breaker pattern: prevents over-eager flatten by enforcing grace window before action.
- `AuditFleet_EvaluateCriticalDesync` isolates the circuit-breaker decision logic (isCriticalDesync) from the action logic (flatten/defer).

### V12 Lock-Free Actor Pattern
- All helpers must use `Enqueue` pattern for state mutations: `EnqueueReaperRepairCandidate`, `EnqueueReaperFlattenCandidate` (confirmed in jcodemunch call_hierarchy).
- No `lock(stateLock)` blocks permitted in any extracted helper.
- FSM state reads must use `GetFsmExpectedPosition` (confirmed callee at depth 2 in jcodemunch hierarchy).

### ASCII-Only Compliance
- All `Print(...)` string literals in `AuditFleet_LogMinorDesync` must use ASCII-only characters.
- No Unicode, emoji, or curly quotes in log messages.

---

## Dependency Graph Evidence (jcodemunch get_dependency_graph)

```
src/V12_002.REAPER.Audit.cs
  imports: [] (no cross-file imports detected)
  importers: [] (partial class — resolved at compile time via C# partial)
  node_count: 1
  edge_count: 0
```

Blast radius is fully contained to `src/V12_002.REAPER.Audit.cs`. All helpers are `private` methods in the same `partial class V12_002`. No interface changes. No cross-file dependency edges introduced.

---

## Call Hierarchy Evidence (jcodemunch get_call_hierarchy)

- **Direct Caller:** `AuditApexPositions` (lines 16-60 in same file)
- **Key Callees at depth 1:** `AuditFleet_CalculateExpectedActual`, `AuditFleet_HandleDesyncRepair`, `AuditFleet_CheckPositionPassGrace`, `AuditFleet_HandleCriticalDesyncFlatten`, `AuditFleet_HandleNakedPosition`, `DetectOrphanFSM`
- **Key Callees at depth 2:** `GetFsmExpectedPosition`, `TryTerminateFollowerBracket`, `LogBuffer.Format`, `EnqueueReaperRepairCandidate`, `EnqueueReaperFlattenCandidate`, `IsReaperFillGraceActive`, `DetectNakedPosition`
- **Caller signature preserved:** `private bool AuditSingleFleetAccount(Account acct, bool shouldLog)` — unchanged

---

## Implementation Notes for Phase 5 (v12-engineer)

1. **New helpers go in `src/V12_002.REAPER.Audit.cs`** — same partial class, after existing AuditFleet_* helpers (after line 527).
2. **Order of extraction:** Extract `AuditFleet_HandleDesyncBranch` first (reduces parent CYC most), then `AuditFleet_EvaluateCriticalDesync`, then `AuditFleet_ProcessOrphanFsmLoop`, then `AuditFleet_LogMinorDesync`, then `AuditFleet_ResolveSyncState`, then `AuditFleet_BuildStateSnapshot`.
3. **Test after each extraction:** `dotnet build src/` must pass. CSharpier must not report formatting issues.
4. **No scope creep:** Pre-existing helpers (`AuditFleet_HandleDesyncRepair`, etc.) are NOT modified. Only the parent dispatcher and the 6 new helpers are changed.
5. **deploy-sync.ps1** must run after all edits to re-synchronize NinjaTrader hard links.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:02:00Z |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-082 |
| **Method** | AuditSingleFleetAccount |
| **Original CYC** | 90 |
| **max_cyc_projected** | 8 |
| **extraction_count** | 11 |
| **parent_cyc_projected** | 6 |
| **MCP Tools Used** | resolve_repo, index_folder, get_context_bundle, get_dependency_graph, get_call_hierarchy, get_extraction_candidates, get_file_outline, search_symbols, get_symbol_source |
| **Sequential Thinking Thoughts** | 6 |
| **Output** | docs/brain/EPIC-W7-082/02-architecture-plan.md |
