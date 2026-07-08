# Phase 2: Architecture Plan — EPIC-W7-052

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T02:15:00Z
**Input:** docs/brain/EPIC-W7-052/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `CleanupStalePendingReplacements`
- **Source File:** `src/V12_002.Trailing.StopUpdate.cs`
- **Class:** `V12_002` (partial) — `NinjaTrader.NinjaScript.Strategies`
- **Visibility:** `private void`
- **Original CYC:** 11 (10 branch points + 1 base)

### jcodemunch get_context_bundle result

Symbol resolved: `src/V12_002.Trailing.StopUpdate.cs::V12_002.CleanupStalePendingReplacements#method`

Key findings:
- Lines 37–80, 44 lines total
- Signature: `private void CleanupStalePendingReplacements()`
- Accesses class fields: `pendingStopReplacements` (ConcurrentDictionary), `pendingReplacementCount` (Interlocked counter), `activePositions`
- Calls: `Print`, `Interlocked.Decrement`, `CreateNewStopOrder` (with `isRecovery: true`), `TriggerCustomEvent` (with lambda capturing `_tSnap`, `_tKey`), `RestoreCascadedTargets` (via closure)
- Contains loop-local variable capture risk in TriggerCustomEvent lambda (`_tSnap`, `_tKey` captured inside foreach)
- All string literals are ASCII-only

### jcodemunch get_call_hierarchy result

- **Callers (depth 1):** 0 direct callers resolved via AST (called from `ManageTrailingStops` at `src/V12_002.Trailing.cs:222` — confirmed via Phase 1 analysis; not resolved by AST due to partial class boundary)
- **Callees (depth 1):**
  - `pendingStopReplacements` (ConcurrentDictionary field — `V12_002.cs:210`)
  - `activePositions` (ConcurrentDictionary field — `V12_002.cs:199`)
  - `LogBuffer.Format` (via Print — `Perf.LogBuffer.cs:28`)
  - `CreateNewStopOrder` (`Orders.Management.StopSync.cs:673`)
  - `RestoreCascadedTargets` (`Orders.Management.StopSync.cs:981`)
- **Callees (depth 2):**
  - `ValidateStopOrderPreconditions`, `SubmitStopOrderToBroker` (via `CreateNewStopOrder`)
  - `FlattenPositionByName`, `Enqueue`, `SymmetryTrim`, `GetTargetOrdersDictionary` (via `RestoreCascadedTargets`)
  - `LogBuffer.ValidateThreadAffinity`, `LogBuffer.FormatInternal` (via `Print`)

### jcodemunch get_dependency_graph result

- `src/V12_002.Trailing.StopUpdate.cs` is a **standalone partial class file** with no explicit file-level imports or importers resolved by the graph (partial class pattern — all dependencies shared at class level via `V12_002.cs`)
- Node count: 1, Edge count: 0
- Cross-file dependencies are class-level (partial class), not file-level imports

### jcodemunch get_extraction_candidates result

- No candidates returned (tool requires min_callers=1 at file level; partial class callers not resolved at file level by the graph). Phase 0 hotspot analysis and manual source review provide the extraction candidates.

---

## Sequential Thinking Summary

**Final Thought (5/5):**

Three helper methods are extracted from `CleanupStalePendingReplacements`. The parent is reduced to a linear foreach orchestrator. All projected CYCs are ≤ 4, satisfying both the ≤ 8 Jane Street mandate and the Phase 0 stretch goal of ≤ 4.

The `out` parameter pattern on `RemoveStalePendingEntry` makes it impossible to use a `pending` reference that was never successfully removed — illegal state made unrepresentable at compile time. `ScheduleBracketRestoration` is called from inside `RecoverStopForStaleEntry` (not from the parent), because bracket restoration is logically conditioned on the recovery path executing — this eliminates the loop-local lambda capture risk by hoisting local variables into named method parameters. All helpers are `private`, ASCII-only, lock-free, and single-responsibility.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `RemoveStalePendingEntry` | `private bool RemoveStalePendingEntry(string key, out PendingReplacement pending)` | TryRemove from `pendingStopReplacements`, `Interlocked.Decrement` on `pendingReplacementCount`, Print stale-removed diagnostic log | 2 |
| `RecoverStopForStaleEntry` | `private void RecoverStopForStaleEntry(string key, PendingReplacement pending)` | Guard check (`activePositions.TryGetValue` + `pos.EntryFilled` + `pos.RemainingContracts > 0`), compute `replacementQty`, call `CreateNewStopOrder(isRecovery: true)`, call `ScheduleBracketRestoration` | 4 |
| `ScheduleBracketRestoration` | `private void ScheduleBracketRestoration(string key, PendingReplacement pending)` | Guard (`pending.BracketRestorationNeeded && pending.CapturedTargets != null`), dispatch `TriggerCustomEvent` closure for `RestoreCascadedTargets` — eliminates loop-local capture risk | 3 |

---

## Parent Method After Extraction

**Remaining logic:**

```csharp
private void CleanupStalePendingReplacements()
{
    DateTime now = DateTime.Now;
    foreach (var kvp in pendingStopReplacements.ToArray())
    {
        if ((now - kvp.Value.CreatedTime).TotalSeconds > 5)
        {
            if (RemoveStalePendingEntry(kvp.Key, out var pending))
            {
                RecoverStopForStaleEntry(kvp.Key, pending);
            }
        }
    }
}
```

- **Remaining logic:** Snapshot iteration, staleness time check, delegating remove + recovery to named helpers
- **Projected CYC:** 4 (base:1 + foreach:1 + staleness-if:1 + RemoveStalePendingEntry-bool:1)

---

## max_cyc_projected: 4
## extraction_count: 3

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC <= 8 achieved | YES — max projected CYC is 4 across all 4 methods |
| Single-responsibility per helper | YES — each helper has exactly one named concern |
| Lock-free / Actor pattern preserved | YES — ConcurrentDictionary.TryRemove and Interlocked.Decrement retained; no lock() introduced |
| Illegal states unrepresentable | YES — `out PendingReplacement pending` on `RemoveStalePendingEntry` bool prevents use of unremoved pending; loop-local lambda capture eliminated by parameter passing into `ScheduleBracketRestoration` |
| Extract Guard Clauses | YES — three-clause compound guard extracted into `RecoverStopForStaleEntry` |
| Extract Loop Body | YES — foreach body reduced to 3 lines: staleness check + 2 helper calls |
| String literals ASCII-only | YES — all Print() format strings verified ASCII-only |
| xUnit tests required (Phase 5) | `Test_RemoveStalePendingEntry_RemovesEntry_And_DecrementsCounter`, `Test_RecoverStopForStaleEntry_CreatesStopOrder_WhenPositionExists`, `Test_ScheduleBracketRestoration_DispatchesTrigger_WhenBracketNeeded` |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic ID** | EPIC-W7-052 |
| **Wave** | 7 |
| **Phase** | 2 |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T02:15:00Z |
| **jcodemunch tools called** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **max_cyc_projected** | 4 |
| **extraction_count** | 3 |
