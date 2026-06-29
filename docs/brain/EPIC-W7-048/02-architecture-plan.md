# Phase 2: Architecture Plan — EPIC-W7-048

## Method Under Extraction

- **Method:** `UpdateExistingPendingReplacement`
- **Source File:** [`src/V12_002.Trailing.StopUpdate.cs`](src/V12_002.Trailing.StopUpdate.cs:167)
- **Lines:** 167–253
- **Original CYC:** 0 (tool artefact — lambdas not counted); structural CYC ~5–7 per Phase 0 analysis
- **Visibility:** `private` within `V12_002` partial class

### jcodemunch get_context_bundle result
Symbol not found by name (ambiguous — src/ vs src-vm-backup/); resolved via `get_symbol_source` with full symbol ID `src/V12_002.Trailing.StopUpdate.cs::V12_002.UpdateExistingPendingReplacement#method`. Full source retrieved (lines 167–253). Signature: `private void UpdateExistingPendingReplacement(string entryName, PositionInfo pos, Order currentStop, double validatedStopPrice, int newTrailLevel)`.

### jcodemunch get_call_hierarchy result
- **Callers (depth 1):** `UpdateStopOrder` (line 84, same file) — sole caller, ast_resolved
- **Callees (depth 1):** `CaptureTargetSnapshot`, `RefreshTargetSnapshot`, `MarkStickyDirty`, `pendingStopReplacements` (ConcurrentDictionary field), `LogBuffer.Format`
- **Callees (depth 2):** `GetTargetOrdersDictionary`, `LogBuffer.ValidateThreadAffinity`, `LogBuffer.FormatInternal`
- **Total callee count:** 16 (including src-vm-backup mirror symbols)

### jcodemunch get_dependency_graph result
- **Node count:** 1 (file is self-contained with no tracked import edges)
- **Edge count:** 0 — `src/V12_002.Trailing.StopUpdate.cs` participates in the partial class pattern; dependency edges are not resolvable by the import graph at the file level

### jcodemunch get_extraction_candidates result
- **Candidates found:** 0 (no symbols met min_complexity=3 + min_callers=1 threshold by tool metrics, consistent with CYC=0 artefact — tool did not count lambda decision paths)
- Extraction plan derived from structural analysis in Sequential Thinking below

---

## Sequential Thinking Summary

**5-thought chain completed. Final conclusion (Thought 5):**

The method's tool-reported CYC of 0 is a measurement artefact caused by CYC analyzers that do not count lambda sub-expressions inside `ConcurrentDictionary.AddOrUpdate` delegates. Structural analysis counts 5–7 decision paths:

1. `if (TryAdd)` branch  
2. Circuit breaker compound condition (`>=` threshold `&&` `!active`) — 2 sub-conditions  
3. Update-factory ternary (`!BracketRestorationNeeded ? ... : ...`)  
4. `_b950Needed` compound (`&&` `&&`) — 2 sub-conditions  
5. Null-coalescing `??` and `||` in struct initializer

**Hypothesis confirmed:** Extract 2 helpers. Both project to CYC ≤ 8. Parent reduces to CYC ≈ 4. All Jane Street mandates satisfied. Scope boundary upheld per V12.23 — both helpers are private methods in the same partial class.

---

## Extraction Plan

| Helper Method Name | Responsibility | Signature | Projected CYC |
|---|---|---|---|
| `TryActivateCircuitBreaker` | Checks `pendingReplacementCount` against threshold and atomically sets `circuitBreakerActive`; emits diagnostic Print if activated. Isolates the non-atomic breaker check currently duplicated with `InitiateStopReplacement`. | `private void TryActivateCircuitBreaker(int currentCount)` | 3 |
| `BuildRefreshedPendingReplacement` | Refreshes target snapshot via `RefreshTargetSnapshot` if `BracketRestorationNeeded` is not yet set; conditionally derives new `BracketRestorationNeeded`; constructs and returns updated `PendingStopReplacement` struct. Isolates the entire AddOrUpdate update-factory lambda body. | `private PendingStopReplacement BuildRefreshedPendingReplacement(string entryName, PendingStopReplacement pending, double validatedStopPrice)` | 6 |

### TryActivateCircuitBreaker — Implementation Sketch

```csharp
private void TryActivateCircuitBreaker(int currentCount)
{
    if (currentCount >= CIRCUIT_BREAKER_THRESHOLD && !circuitBreakerActive)
    {
        circuitBreakerActive = true;
        circuitBreakerActivatedTime = DateTime.Now;
        Print(
            string.Format(
                "V8.30: CIRCUIT BREAKER ACTIVATED - {0} pending replacements (threshold: {1})",
                currentCount,
                CIRCUIT_BREAKER_THRESHOLD
            )
        );
    }
}
```

### BuildRefreshedPendingReplacement — Implementation Sketch

```csharp
private PendingStopReplacement BuildRefreshedPendingReplacement(
    string entryName,
    PendingStopReplacement pending,
    double validatedStopPrice
)
{
    var refreshedTargets = !pending.BracketRestorationNeeded
        ? RefreshTargetSnapshot(entryName)
        : pending.CapturedTargets;
    var restorationNeeded =
        !pending.BracketRestorationNeeded
        && refreshedTargets != null
        && refreshedTargets.Length > 0;
    return new PendingStopReplacement
    {
        EntryName = pending.EntryName,
        Quantity = pending.Quantity,
        StopPrice = validatedStopPrice,
        Direction = pending.Direction,
        OldOrder = pending.OldOrder,
        CreatedTime = pending.CreatedTime,
        CapturedTargets = refreshedTargets ?? pending.CapturedTargets,
        BracketRestorationNeeded = restorationNeeded || pending.BracketRestorationNeeded,
    };
}
```

---

## Parent Method After Extraction

**Remaining logic in `UpdateExistingPendingReplacement` after extraction:**

1. Call `CaptureTargetSnapshot(entryName)` to build `_b955TargetsA`
2. Construct `newPending` struct inline (unchanged)
3. Call `pendingStopReplacements.TryAdd(entryName, newPending)`
   - On success: `Interlocked.Increment` → call `TryActivateCircuitBreaker(currentCount)`
   - On failure: `AddOrUpdate` with add-factory (inline, trivial) and update-factory replaced by single `BuildRefreshedPendingReplacement(...)` expression
4. Assign `pos.CurrentStopPrice`, `pos.CurrentTrailLevel`
5. Call `MarkStickyDirty()`
6. Emit `Print(...)` diagnostic

**Projected CYC:** 4  
(1 base + 1 for `if(TryAdd)` + 2 for compound `&&` in `newPending` initializer `_b955TargetsA != null && _b955TargetsA.Length > 0`)

---

## max_cyc_projected: 6
## extraction_count: 2

---

## Jane Street Alignment

| Mandate | Status |
|---|---|
| CYC ≤ 8 achieved (all methods) | **YES** — parent: 4, TryActivateCircuitBreaker: 3, BuildRefreshedPendingReplacement: 6 |
| Single-responsibility per helper | **YES** — each helper has exactly one concern |
| Lock-free / Actor pattern preserved | **YES** — `ConcurrentDictionary.TryAdd`/`AddOrUpdate` pattern unchanged; no `lock()` introduced |
| Illegal states unrepresentable | **YES** — `BracketRestorationNeeded` derived deterministically from snapshot nullability; `PendingStopReplacement` is a struct (cannot be null) |
| Zero-allocation hot path | **YES** — `PendingStopReplacement` is a value type (struct); no heap allocations introduced by extraction |
| Guard clause extraction | **YES** — `BracketRestorationNeeded` two-level conditional isolated in `BuildRefreshedPendingReplacement` |
| Extract loop body | N/A — no loops present |
| FSM decomposition | N/A — no FSM pattern in this method |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-048 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Bobcoins Used** | 2.5 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **jcodemunch tools called** | get_context_bundle (→ fallback: get_symbol_source), get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **CYC reported (tool)** | 0 (artefact) |
| **CYC structural estimate** | ~5–7 |
| **max_cyc_projected** | 6 |
| **extraction_count** | 2 |
