# Phase 2: Architecture Plan — EPIC-W7-053

## Method Under Extraction

- **Method:** `InitiateStopReplacement`
- **Source File:** `src/V12_002.Trailing.StopUpdate.cs`
- **Lines:** 307–369 (63 loc)
- **Original CYC:** 6 (manual static count; tool-reported 0 due to instrumentation gap at intake)
- **Class:** `V12_002` (partial — Trailing module)
- **Visibility:** `private void`

### jcodemunch get_context_bundle result

Symbol resolved: `src/V12_002.Trailing.StopUpdate.cs::V12_002.InitiateStopReplacement#method`

Key findings from full source retrieval:
- Method body confirmed at lines 307–369, 63 loc
- Uses `ConcurrentDictionary.TryAdd` for atomic pending-queue insertion (lock-free)
- Uses `Interlocked.Increment` for `pendingReplacementCount` (lock-free atomic)
- Contains an inline 5-target snapshot for-loop with compound if-guard (+2 CYC)
- Contains a TryAdd success branch with nested circuit-breaker if-check (+2 CYC)
- Contains a nested ternary level-name formatter on the final Print line (+2 CYC branches)
- **No `lock()` blocks present** — already actor/lock-free pattern compliant
- Four external callees: `GetTargetOrdersDictionary`, `CancelOrderForReplace`, `MarkStickyDirty`, `Print`

### jcodemunch get_call_hierarchy result

- **Callers (depth 1):** `UpdateStopOrder` (same file, line 84) — sole direct caller, confirmed by 00-scope.md
- **Callees (depth 1):**
  - `GetTargetOrdersDictionary` (src/V12_002.UI.Callbacks.cs, ast_inferred)
  - `pendingStopReplacements` (src/V12_002.cs — ConcurrentDictionary field)
  - `CancelOrderForReplace` (src/V12_002.Orders.CancelGateway.cs, ast_inferred)
  - `MarkStickyDirty` (src/V12_002.StickyState.cs, ast_inferred)
  - `LogBuffer.Format` (src/V12_002.Perf.LogBuffer.cs)
- **Callees (depth 2):** `CancelOrderSafe`, `StampReaperMoveGrace`, `IsOrderTerminal`, `ValidateThreadAffinity`, `FormatInternal`
- **Fan-in:** 1 (single caller — low regression risk)
- **Fan-out:** 5 direct callees across 4 files

### jcodemunch get_dependency_graph result

- **Direction:** both (imports + importers)
- **Result:** 1 node, 0 edges — `src/V12_002.Trailing.StopUpdate.cs` has no explicit import edges
  tracked at file level (uses partial class pattern; all dependencies resolved within the single
  compiled assembly). No cross-file import graph edges to protect.

### jcodemunch get_extraction_candidates result

- **Candidates returned:** 0 (no candidates meeting min_complexity=3 + min_callers=1 threshold)
- The method's callees (`GetTargetOrdersDictionary`, `CancelOrderForReplace`, `MarkStickyDirty`)
  are already shared helpers with multi-file callers — they are not extraction candidates themselves.
  The extraction candidates noted in 00-hotspots.md (inline loop, circuit-breaker block, ternary)
  are inlined logic blocks not yet represented as separate symbols in the index.

---

## Sequential Thinking Summary

**Final thought (Thought 5):**

No extraction required. CYC=6 is already ≤8 (V12 Jane Street ceiling). The method already uses
`Interlocked.Increment` (lock-free atomic), `ConcurrentDictionary.TryAdd` (atomic duplicate guard),
and has no `lock()` blocks. Duplicate-key illegal states are avoided by construction via
`ConcurrentDictionary` semantics. The three optional improvements identified in 00-hotspots.md
(inline snapshot loop → delegate to `CaptureTargetSnapshot`, circuit-breaker isolation →
`TryActivateCircuitBreaker`, nested ternary → `TrailLevelName`) are beneficial for SRP and
duplication elimination but are NOT required to achieve CYC≤8 compliance. They are deferred
as optional quality improvements for a future dedicated epic. Architecture plan:
`extraction_count=0`, `max_cyc_projected=6`, Jane Street alignment achieved.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| (No extraction required — method is already CYC=6, which is ≤8) | | |

### Optional Deferred Improvements (NOT in scope for this epic)

The following are flagged as future quality work only. They do not change the CYC compliance
verdict for this epic.

| Helper Method Name (suggested) | Responsibility | CYC reduction |
|---|---|---|
| `CaptureTargetSnapshot()` (existing) | Delegate inline snapshot loop to existing method | −1 branch |
| `TryActivateCircuitBreaker(int count)` | Isolate circuit-breaker state writes from queue bookkeeping | −1 branch |
| `TrailLevelName(int level)` | Extract nested ternary string formatter, eliminate duplication with `CreateDirectStopOrder` | −2 branches |

Post-optional-extraction estimated CYC: 3.

---

## Parent Method After Extraction

- **Remaining logic:** All current logic retained unchanged — the method is not refactored in this epic.
- **Projected CYC:** 6 (unchanged; already CYC-compliant at ≤8)
- **Lock-free state:** Maintained — `Interlocked.Increment` + `ConcurrentDictionary.TryAdd` already in place
- **No surgery required** for CYC compliance

---

## max_cyc_projected: 6
## extraction_count: 0

---

## Jane Street Alignment

| Principle | Status | Evidence |
|---|---|---|
| CYC<=8 achieved | YES | CYC=6 (manual count); already below ceiling |
| Single-responsibility per helper | YES (N/A) | No helpers extracted; method SRP is informational |
| Lock-free/Actor pattern preserved | YES | `Interlocked.Increment`, `ConcurrentDictionary.TryAdd`; zero `lock()` blocks |
| Illegal states unrepresentable | YES | `ConcurrentDictionary.TryAdd` atomically prevents duplicate `entryName` keys; circuit-breaker check guards against threshold activation twice |
| Zero-allocation hot path | YES (informational) | Stop-replacement path is event-driven (not per-tick); necessary allocations (List, PendingStopReplacement) are structurally required |
| Extract Guard Clauses | N/A | No deep nesting requiring guard-clause extraction at CYC=6 |
| FSM Decomposition | N/A | State mutation is atomic at field level; no FSM needed at this CYC |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-053 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | resolve_repo, get_context_bundle (with fallback search), get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **CYC confirmed** | 6 (manual static count from 00-hotspots.md) |
| **Extraction decision** | None required — CYC=6 ≤ 8 already compliant |
| **Source file** | `src/V12_002.Trailing.StopUpdate.cs` |
| **Output** | `docs/brain/EPIC-W7-053/02-architecture-plan.md` |
