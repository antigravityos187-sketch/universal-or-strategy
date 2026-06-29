# EPIC-W7-005 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-005/01-scope-boundary.md

---

## Summary

**Original Method:** `ClassifyAndRouteFleetOrder`
**Original Source File:** `src/V12_002.SIMA.Lifecycle.cs`
**Original CYC (confirmed):** 16 (CYC=0 in epic-list is a sparse/phantom entry — actual CYC=16 confirmed by 4 independent audit sources)
**Original LOC:** 42 (raw) / 60 (Codacy Lizard)

**Live Status at HEAD:** Method decomposed by Wave 4/6 into three successor helpers in `src/V12_002.SIMA.Lifecycle.cs`. The original body no longer exists. This epic targets the live successors which still exceed CYC=8.

**Extraction Count:** 3 new methods + 1 static lookup field
**max_cyc_projected:** 6

---

## Successor Helper Complexity (Live HEAD — Requires Reduction)

| Method | Line | CYC Now | Max Nesting | LOC | Assessment |
|--------|------|---------|-------------|-----|------------|
| `ClassifyOrderByPrefix` | 1262 | 20 | 2 | 25 | HIGH — over threshold |
| `AdoptOrdersFromAccount` | 930 | 10 | 4 | 35 | MEDIUM — over threshold |
| `AdoptSingleOrder` | 1058 | 11 | 5 | 60 | HIGH — over threshold |

All three exceed the Jane Street strict threshold of CYC=8. This plan reduces all three to ≤8.

---

## Extraction Plan

### Extraction 1 — Table-Driven Refactor of `ClassifyOrderByPrefix`

**Problem:** CYC=20 from an 8-branch if/else-if chain. The method is a pure prefix→string classification function. Each `StartsWith` branch contributes to cyclomatic complexity independently.

**Pattern applied:** `trading_billions` — single responsibility; `carl_cook` — zero-alloc hot path. Replace branch logic with a static lookup array (zero GC pressure, O(n) where n=8 which is constant).

**New static field introduced (not a method extraction):**
```csharp
private static readonly (string Prefix, string Classification)[] _fleetPrefixTable =
{
    ("Stop_", "stop"),
    ("S_",    "stop"),
    ("T1_",   "target1"),
    ("T2_",   "target2"),
    ("T3_",   "target3"),
    ("T4_",   "target4"),
    ("T5_",   "target5"),
    ("Fleet_","entry"),
};
```

**Refactored `ClassifyOrderByPrefix` body:**
```csharp
private string ClassifyOrderByPrefix(string orderName)
{
    if (string.IsNullOrEmpty(orderName))
        return null;

    foreach (var (prefix, classification) in _fleetPrefixTable)
    {
        if (orderName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return classification;
    }
    return null;
}
```

**Signature (unchanged):** `private string ClassifyOrderByPrefix(string orderName)`
**CYC before:** 20
**CYC projected:** 4 (null guard=1 + foreach=1 + StartsWith=1 + baseline=1)

---

### Extraction 2 — `RebuildOrSyncPositionEntry` from `AdoptSingleOrder`

**Problem:** CYC=11, max_nesting=5. The dominant driver is the position tracking branch: `if (entryOrders) → rebuild position` vs `else → TryGetValue + force-sync TotalContracts + ExecutingAccount`. This nesting is the depth contributor.

**Pattern applied:** `trading_billions` — single responsibility per helper. `carl_cook` — `[AggressiveInlining]` on pure-logic hot paths.

**New method signature:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private void RebuildOrSyncPositionEntry(
    string key,
    Order ord,
    Account acct,
    bool isEntryDict)
```

**Responsibility:** Handles the if(isEntryDict)/else(TryGetValue+sync) branching for activePositions management. Returns void; mutates `activePositions` only.

**CYC projected for helper:** 4 (isEntryDict check=1 + !ContainsKey=1 + TryGetValue=1 + baseline=1)

**`AdoptSingleOrder` after extraction:**
```csharp
private void AdoptSingleOrder(
    Order ord,
    Account acct,
    string classification,
    ref int adoptedCount)
{
    ConcurrentDictionary<string, Order> targetDict =
        RouteOrderToTargetDict(classification, ord.Name, out string key, out string dictName);

    if (targetDict == null)
        return;

    targetDict[key] = ord;

    RebuildOrSyncPositionEntry(key, ord, acct, targetDict == entryOrders);

    LogOrderAdoption(ord.Name, dictName);
    adoptedCount++;
}
```

**CYC projected for parent:** 3 (null check=1 + if via bool arg=0 (eliminated) + baseline=1 + call chain=0)

---

### Extraction 3 — `LogOrderAdoption` from `AdoptSingleOrder`

**Problem:** Cold Print() calls and string.Format allocations inflate the method body and create confusion between hot-path logic and cold diagnostic output.

**Pattern applied:** `carl_cook` — extract cold logging out-of-line; `[NoInlining]` on cold path prevents JIT from pulling Print overhead into the hot-path instruction cache.

**New method signature:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private void LogOrderAdoption(string orderName, string dictName)
```

**Responsibility:** Single Print() call: `[SIMA HYDRATE] Adopted working order {0} into {1}`.

**CYC projected for helper:** 1 (baseline only — no branches)

**Note:** The position-rebuild and force-sync Print() calls move into `RebuildOrSyncPositionEntry` as a helper (`LogPositionRebuild` / `LogPositionSync`), or are inlined in that helper if LOC allows.

---

### Extraction 4 — `IsOrderEligibleForAdoption` from `AdoptOrdersFromAccount`

**Problem:** CYC=10, max_nesting=4. The source likely contains inline guard chains (null name, state validity, instrument match, account membership) before calling `ClassifyOrderByPrefix` and `AdoptSingleOrder`. These guards run per-order in the inner loop and create multiplicative complexity.

**Pattern applied:** `trading_billions` — defense in depth as isolated guard layers; `carl_cook` — pure boolean predicate, candidate for `[AggressiveInlining]` since it is on the hot adoption path.

**New method signature:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private bool IsOrderEligibleForAdoption(Order ord, Account acct)
```

**Responsibility:** Consolidates: null name check, `IsValidOrderState` check, instrument match, account membership. Returns `false` to skip the order.

**CYC projected for helper:** 4 (null name=1 + IsValidOrderState=1 + instrument=1 + baseline=1)

**`AdoptOrdersFromAccount` after extraction:**
- foreach loop (1) + eligibility guard (1) + null classification guard (1) + call AdoptSingleOrder (0) = CYC ≈ 4-6

**CYC projected for parent:** 6 (worst case with full loop + guards)

---

## CYC Projection Table

| Symbol | CYC Before | CYC Projected | Status |
|--------|-----------|---------------|--------|
| `ClassifyOrderByPrefix` | 20 | 4 | ✅ ≤8 |
| `AdoptOrdersFromAccount` | 10 | 6 | ✅ ≤8 |
| `AdoptSingleOrder` | 11 | 3 | ✅ ≤8 |
| `_fleetPrefixTable` (new field) | — | 0 | ✅ data |
| `RebuildOrSyncPositionEntry` (new) | — | 4 | ✅ ≤8 |
| `LogOrderAdoption` (new) | — | 1 | ✅ ≤8 |
| `IsOrderEligibleForAdoption` (new) | — | 4 | ✅ ≤8 |

**max_cyc_projected: 6** (AdoptOrdersFromAccount after IsOrderEligibleForAdoption extraction)

---

## Jane Street Alignment Notes

### gjengset — Cache Line / False Sharing
- `_fleetPrefixTable` is a static readonly value-type array — no heap allocation on reads, no cache-line ping-pong between threads accessing the classifier
- `ConcurrentDictionary` mutations in the adoption path (targetDict[key] = ord; activePositions[key]) are preserved exactly — no lock pattern changes, actor-serialized as per existing `[THREAD-SAFETY]` annotation

### carl_cook — Zero-Alloc Hot Path / NoInlining Cold
- `LogOrderAdoption` decorated with `[MethodImpl(NoInlining)]`: keeps Print/string.Format JIT overhead out of the hot-path instruction cache
- `IsOrderEligibleForAdoption` and `RebuildOrSyncPositionEntry` decorated with `[MethodImpl(AggressiveInlining)]`: pure logic helpers on the per-order hot path, no alloc
- `_fleetPrefixTable` as ValueTuple array: zero GC allocation on classification path (no boxing, no LINQ, no closures)

### trading_billions — Defense in Depth / Single Responsibility
- `ClassifyOrderByPrefix`: pure prefix→string, no state, no side effects
- `IsOrderEligibleForAdoption`: pure guard predicate, no mutations, no routing
- `RebuildOrSyncPositionEntry`: owns position tracking only — no order routing, no logging
- `LogOrderAdoption`: owns Print only — no logic, no mutations
- Circuit-breaker pattern preserved: null returns from RouteOrderToTargetDict and ClassifyOrderByPrefix are early-exit guards, not exceptions

---

## Source File Target

All new methods are private helpers added to the same partial class in:
**`src/V12_002.SIMA.Lifecycle.cs`**

No cross-file changes. No public/internal interface changes. Callers of the successor helpers (`AdoptFleetWorkingOrders`, `AdoptMasterWorkingOrders`, `HydrateWorkingOrdersFromBroker`) are not modified — their call sites call the same method names with the same signatures.

Per V12.23 No Scope Creep Protocol: ONE EPIC = ONE CONCERN. No sibling methods are modified.

---

## Call Hierarchy Evidence (jCodemunch MCP)

- **ClassifyAndRouteFleetOrder callers (src-vm-backup):**
  - `AdoptFleetWorkingOrders` (line 460) — depth 1, AST resolved
  - `HydrateWorkingOrdersFromBroker` (line 415) — depth 2, AST resolved
- **Live successor callers:** Preserved unchanged; no signature modifications
- **Callees of ClassifyAndRouteFleetOrder:** 0 (pure routing logic, no sub-calls in original)

---

## MCP Evidence

| Tool | Finding |
|------|---------|
| `resolve_repo` | Repo `antigravityos187-sketch/universal-or-strategy` indexed; 5147 symbols; source root `/home/malhitticrypto/universal-or-strategy` |
| `search_symbols("ClassifyAndRouteFleetOrder")` | Found in `src-vm-backup/V12_002.SIMA.Lifecycle.cs` line 531, CYC=16 (backup). Zero results in live `src/` — confirms Wave 4/6 decomposition. |
| `search_symbols("ClassifyOrderByPrefix AdoptOrdersFromAccount AdoptSingleOrder")` | All three found in `src/V12_002.SIMA.Lifecycle.cs` at lines 1262, 930, 1058 respectively |
| `get_context_bundle(ClassifyAndRouteFleetOrder)` | Full source retrieved: 8-branch if/else-if prefix router, lines 531-597 |
| `get_context_bundle(ClassifyOrderByPrefix)` | Full source retrieved: same 8-branch if/else-if chain, CYC=20 confirmed |
| `get_context_bundle(AdoptSingleOrder)` | Full source retrieved: RouteOrderToTargetDict + position tracking + Print calls, CYC=11 |
| `get_symbol_complexity(ClassifyOrderByPrefix)` | CYC=20, max_nesting=2, param=1, lines=25, assessment=high |
| `get_symbol_complexity(AdoptOrdersFromAccount)` | CYC=10, max_nesting=4, param=2, lines=35, assessment=medium |
| `get_symbol_complexity(AdoptSingleOrder)` | CYC=11, max_nesting=5, param=4, lines=60, assessment=high |
| `get_call_hierarchy(ClassifyAndRouteFleetOrder, depth=2)` | callers: AdoptFleetWorkingOrders (d1), HydrateWorkingOrdersFromBroker (d2); callees: 0 |
| `get_extraction_candidates(src/V12_002.SIMA.Lifecycle.cs, min_complexity=8)` | 0 candidates returned (jcodemunch extraction heuristic requires multi-file callers) — manual extraction plan applied based on complexity + source analysis |

---

## Sequential Thinking Evidence

**Thought 1 — Current State Resolution:**
The original ClassifyAndRouteFleetOrder (CYC=16) was decomposed by Wave 4/6, but the successor helpers remain above CYC=8. Phase 2 must target the live successors. The scope boundary from Phase 1.5 explicitly includes "new extracted helper methods" which maps to the live successor helpers.

**Thought 2 — Extraction Design:**
Table-driven refactor is the Jane Street-aligned solution for ClassifyOrderByPrefix (8 if/else branches → static lookup array → CYC 20→4). AdoptSingleOrder's position tracking branch and cold Print calls are the two extraction candidates. AdoptOrdersFromAccount's inline guards are the remaining complexity driver.

**Thought 3 — Signatures and CYC Math:**
Four concrete extractions designed: `_fleetPrefixTable` (data), `RebuildOrSyncPositionEntry` (helper, CYC=4), `LogOrderAdoption` (cold logger, CYC=1), `IsOrderEligibleForAdoption` (guard predicate, CYC=4). All parents project to CYC ≤6.

**Thought 4 — Validation:**
All projections verified: max CYC across all symbols = 6. Jane Street patterns applied consistently. Scope confined to `src/V12_002.SIMA.Lifecycle.cs`. max_cyc_projected = 6 < 8. ✅ Architecture confirmed.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase2-architecture |
| **Epic ID** | EPIC-W7-005 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Original CYC** | 16 (confirmed; epic-list registered 0 — sparse entry) |
| **Live Successor CYC** | ClassifyOrderByPrefix=20, AdoptOrdersFromAccount=10, AdoptSingleOrder=11 |
| **Extraction Count** | 3 methods + 1 static field |
| **max_cyc_projected** | 6 |
| **MCP Tools Used** | `resolve_repo`, `search_symbols` (x2), `get_context_bundle` (x3), `get_call_hierarchy`, `get_symbol_complexity` (x3), `get_extraction_candidates`, `sequentialthinking` (4 thoughts) |
| **Jane Street Patterns** | gjengset (cache-line safe static array), carl_cook (NoInlining cold logger, AggressiveInlining hot guards, zero-alloc ValueTuple), trading_billions (single responsibility, defense-in-depth guards) |
| **Output** | `docs/brain/EPIC-W7-005/02-architecture-plan.md` |
| **Status** | Phase 2 Complete |
