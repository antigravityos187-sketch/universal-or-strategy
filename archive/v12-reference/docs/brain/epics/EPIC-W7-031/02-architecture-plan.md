# EPIC-W7-031 — Phase 2: Architecture Plan

**Agent Name:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T00:35:10Z
**Input:** docs/brain/EPIC-W7-031/01-scope-boundary.md

---

## Summary

`AuditMaster_HandleNakedPosition` in [`src/V12_002.REAPER.Audit.cs`](src/V12_002.REAPER.Audit.cs) has CYC=19. The method handles four distinct responsibilities: snapshot-order stop detection, grace-window initialization with logging, naked-stop enqueue and dispatch, and cleanup when a stop is found. Three private helper methods will be extracted to reduce each unit to CYC≤8 (Jane Street strict standard).

---

## Extraction Plan

| # | Helper Name | Extracted Logic | Projected CYC | Jane Street Rule |
|---|-------------|-----------------|---------------|-----------------|
| 1 | `AuditMaster_HasWorkingStopOrder` | LINQ `.Any()` predicate evaluating instrument match + stop order state/type/action OR-conditions | 6 | carl_cook: extract LINQ predicate out-of-line; zero-alloc hot path |
| 2 | `AuditMaster_InitNakedPositionGrace` | First-seen dictionary insert + `Print(string.Format(...))` grace-window log | 1 | carl_cook: cold logging extracted out-of-line (NoInlining cold path) |
| 3 | `AuditMaster_DispatchNakedStop` | `EnqueueReaperMasterNakedStop(...)` guard + `TriggerCustomEvent` call + `catch` handler + in-flight cleanup | 4 | trading_billions: single responsibility; defense in depth; isolate exception handler |
| — | `AuditMaster_HandleNakedPosition` (parent, post-extract) | Orchestration: qty guard, order snapshot, call helpers, cleanup TryRemove | 7 | trading_billions: parent remains orchestrator only; CYC≤8 |

**max_cyc_projected = 7** (parent after extraction) — satisfies CYC≤8 target.

---

## Method Signatures

```csharp
// HELPER 1 — Stop-order detection predicate (extracted from inline LINQ lambda)
// [MethodImpl(MethodImplOptions.NoInlining)]  -- cold relative to hot path decisions
private bool AuditMaster_HasWorkingStopOrder(Order[] masterOrders)

// HELPER 2 — Grace window initialization + cold log (NoInlining cold path)
// [MethodImpl(MethodImplOptions.NoInlining)]
private void AuditMaster_InitNakedPositionGrace(int masterActualQty, int graceSeconds)

// HELPER 3 — Enqueue + dispatch naked stop with exception handling
// [MethodImpl(MethodImplOptions.NoInlining)]
private void AuditMaster_DispatchNakedStop(
    Position masterPos,
    int masterActualQty,
    string masterExpectedKey,
    DateTime masterFirstSeen)

// PARENT (signature UNCHANGED — caller contract preserved)
private void AuditMaster_HandleNakedPosition(
    Position masterPos,
    int masterActualQty,
    string masterExpectedKey)
```

---

## Parent Post-Extraction Body (Pseudocode)

```csharp
private void AuditMaster_HandleNakedPosition(Position masterPos, int masterActualQty, string masterExpectedKey)
{
    if (masterActualQty != 0)                                          // branch 1
    {
        var masterOrders = Account.Orders.ToArray();                   // H13-FIX snapshot
        bool masterHasWorkingStop = AuditMaster_HasWorkingStopOrder(masterOrders); // delegate to helper 1

        if (!masterHasWorkingStop)                                     // branch 2
        {
            int graceSeconds = (NakedPositionGraceSec >= 5)           // ternary branch 3
                ? NakedPositionGraceSec : 5;

            if (!_nakedPositionFirstSeen.TryGetValue(Account.Name, out DateTime masterFirstSeen)) // branch 4
            {
                _nakedPositionFirstSeen[Account.Name] = DateTime.UtcNow;
                AuditMaster_InitNakedPositionGrace(masterActualQty, graceSeconds); // helper 2
            }
            else if (EnqueueReaperMasterNakedStop(masterPos, masterActualQty, masterExpectedKey, masterFirstSeen)) // branch 5 (else-if)
            {
                AuditMaster_DispatchNakedStop(masterPos, masterActualQty, masterExpectedKey, masterFirstSeen); // helper 3
            }
        }
        else                                                           // branch 6 (else cleanup)
        {
            _nakedPositionFirstSeen.TryRemove(Account.Name, out _);
        }
    }
}
// Parent CYC = 1(base) + 1(qty!=0) + 1(ternary) + 1(!hasStop) + 1(!firstSeen) + 1(else-if) + 1(else cleanup) = 7
```

---

## CYC Validation

| Unit | Branch Count | Projected CYC | Pass CYC<=8? |
|------|-------------|---------------|-------------|
| `AuditMaster_HandleNakedPosition` (parent) | base(1) + qty_guard(1) + grace_ternary(1) + no_stop(1) + first_seen_miss(1) + else_if(1) + else_cleanup(1) | **7** | YES |
| `AuditMaster_HasWorkingStopOrder` | base(1) + any_lambda(1) + instrument_eq(1) + state_OR(1) + type_OR(1) + action_OR(1) | **6** | YES |
| `AuditMaster_InitNakedPositionGrace` | base(1) | **1** | YES |
| `AuditMaster_DispatchNakedStop` | base(1) + enqueue_guard(1) + try_normal(1) + catch(1) | **4** | YES |

**All units CYC≤8. max_cyc_projected=7.**

---

## Scope Boundary Compliance

- **File modified:** `src/V12_002.REAPER.Audit.cs` only
- **New helpers:** 3 private methods added to same partial class (same file)
- **Callers unchanged:** `AuditMasterAccountIfNeeded` (1 direct caller) — signature unchanged
- **No cross-file changes:** No interface changes, no public API changes
- **V12.23 compliance:** ONE EPIC = ONE CONCERN ✓

---

## MCP Evidence

| Tool | Result Summary |
|------|---------------|
| `resolve_repo` | Repo `antigravityos187-sketch/universal-or-strategy` indexed; 5147 symbols, 2000 files |
| `search_symbols` | `AuditMaster_HandleNakedPosition` confirmed at `src/V12_002.REAPER.Audit.cs:624`, signature `private void AuditMaster_HandleNakedPosition(Position, int, string)` |
| `get_context_bundle` | Full source retrieved (lines 624–679, 56 lines); docstring confirms B935 extraction lineage; 4 distinct responsibilities identified |
| `get_call_hierarchy` | 2 callers: `AuditMasterAccountIfNeeded` (depth=1), `AuditApexPositions` (depth=2); 22 callees including `EnqueueReaperMasterNakedStop`, `ProcessReaperNakedStopQueue`, `_nakedPositionFirstSeen`, `_reaperNakedStopInFlight` |
| `get_dependency_graph` | `src/V12_002.REAPER.Audit.cs` has 0 import edges in index — partial class; changes are fully self-contained |

---

## Sequential Thinking Evidence

| Thought | Conclusion |
|---------|-----------|
| **Thought 1 — Complexity Drivers** | CYC=19 sourced from: 4 LINQ OR-conditions in stop-order predicate (+4), 3 nested if/else-if/else blocks (+3), try/catch (+2), ternary (+1), .Any() lambda (+1), outer qty guard (+1). 4 distinct responsibilities confirmed. |
| **Thought 2 — Extraction Strategy** | 3 helpers planned: (1) stop-order predicate extraction removes 4 OR CYC from parent, (2) cold logging isolated as NoInlining, (3) dispatch+exception-handler isolated. Parent reduced to CYC=7 (orchestration only). |
| **Thought 3 — CYC Validation** | All 4 units verified CYC≤8. max=7 (parent). Jane Street rules: carl_cook zero-alloc + cold log NoInlining; gjengset lock-free ConcurrentDictionary preserved; trading_billions single-responsibility + defense-in-depth. Plan confirmed sound. |

---

## Jane Street KB Alignment

| Rule Source | Rule Applied |
|-------------|-------------|
| `carl_cook` | LINQ `.Any()` predicate extracted to dedicated method — removes LINQ complexity from hot-path parent; cold `Print` logging isolated with `NoInlining` |
| `gjengset` | No new `lock()` blocks introduced; existing `ConcurrentDictionary.TryGetValue`/`TryRemove`/indexer remain as lock-free primitives |
| `trading_billions` | Each extracted helper has single responsibility; parent is orchestrator only; all units CYC≤8; exception handler isolated to `DispatchNakedStop` (defense in depth) |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **MCP Tools Used** | resolve_repo, search_symbols, get_context_bundle, get_call_hierarchy, get_dependency_graph, sequentialthinking (4 thoughts) |
| **CYC Baseline** | 19 |
| **CYC Target** | ≤8 |
| **max_cyc_projected** | 7 |
| **Extractions Planned** | 3 |
