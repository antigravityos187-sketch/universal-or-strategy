# EPIC-W7-005 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:30:00Z
**Inputs:** `docs/brain/EPIC-W7-005/02-architecture-plan.md`, `docs/brain/EPIC-W7-005/03-audit-report.md`

---

## Summary

**Original Method:** `ClassifyAndRouteFleetOrder` (CYC=16, decomposed by Wave 4/6)
**Live Successor Targets in `src/V12_002.SIMA.Lifecycle.cs`:**

| Method | Line | CYC Now | Target CYC |
|--------|------|---------|------------|
| `ClassifyOrderByPrefix` | 1262 | 20 | ≤8 |
| `AdoptSingleOrder` | 1058 | 11 | ≤8 |
| `AdoptOrdersFromAccount` | 930 | 10 | ≤8 |

**ticket_count:** 4
**projected_parent_cyc_after_all:** 6 (worst-case parent: `AdoptOrdersFromAccount`)
**max_cyc_projected:** 6

---

## Ticket Definitions

---

### TICKET 1 — Table-Driven Refactor of `ClassifyOrderByPrefix`

| Field | Value |
|-------|-------|
| **ticket_id** | T1 |
| **parent_method** | `ClassifyOrderByPrefix` (line 1262) |
| **new_symbol** | `_fleetPrefixTable` (static field) |
| **helper_name** | `_fleetPrefixTable` (static readonly lookup field, not a method) |
| **concern** | Replace 8-branch if/else-if prefix classification with a static zero-allocation lookup array — pure prefix→string classification with no state, no side effects |
| **lines_to_move** | Entire body of `ClassifyOrderByPrefix` (~lines 1263–1286): 8 `if (orderName.StartsWith(...))` branches. New static field `_fleetPrefixTable` introduced above the method. Body replaced with null guard + foreach over `_fleetPrefixTable` + single `StartsWith` check. |
| **cyc_reduction** | 16 (20 → 4) |
| **projected_helper_cyc** | 0 (data field — no cyclomatic complexity) |
| **projected_parent_cyc_after** | 4 (null guard=1 + foreach=1 + StartsWith=1 + baseline=1) |

**New static field:**
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

**Jane Street patterns:** `carl_cook` (zero-alloc ValueTuple array, no GC), `trading_billions` (single responsibility, pure function)

---

### TICKET 2 — Extract `RebuildOrSyncPositionEntry` from `AdoptSingleOrder`

| Field | Value |
|-------|-------|
| **ticket_id** | T2 |
| **parent_method** | `AdoptSingleOrder` (line 1058, CYC=11) |
| **helper_name** | `RebuildOrSyncPositionEntry` |
| **concern** | Own all `activePositions` tracking — if(isEntryDict) rebuild vs else TryGetValue + force-sync TotalContracts + ExecutingAccount. No order routing, no logging. |
| **lines_to_move** | The if/else block inside `AdoptSingleOrder` handling `activePositions` management: `if (entryOrders)` → rebuild position entry vs `else` → TryGetValue + force-sync TotalContracts + ExecutingAccount (~20–25 LOC within the method body) |
| **cyc_reduction** | ~8 (11 → ~3; position-tracking if/else chain eliminated from parent) |
| **projected_helper_cyc** | 4 (isEntryDict check=1 + !ContainsKey guard=1 + TryGetValue=1 + baseline=1) |
| **projected_parent_cyc_after** | 7 (intermediate after T2; T3 will reduce further to 3) |

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

**Dependency:** T2 must be applied before T3 (both target `AdoptSingleOrder`; position block is the deeper nesting — extract first).

**Jane Street patterns:** `trading_billions` (single responsibility: position tracking only), `carl_cook` (`[AggressiveInlining]` — hot path, pure logic, no alloc)

---

### TICKET 3 — Extract `LogOrderAdoption` from `AdoptSingleOrder`

| Field | Value |
|-------|-------|
| **ticket_id** | T3 |
| **parent_method** | `AdoptSingleOrder` (post-T2, CYC~7) |
| **helper_name** | `LogOrderAdoption` |
| **concern** | Own the single cold `Print()` diagnostic call: `[SIMA HYDRATE] Adopted working order {0} into {1}`. No logic, no mutations, no routing. |
| **lines_to_move** | The `Print()` call plus any `string.Format` string-building for `dictName` inside `AdoptSingleOrder` (~3–5 LOC). After T2 has already moved the position-tracking block, the remaining Print call in the parent body is the only cold-path allocation source. |
| **cyc_reduction** | ~4 (7 → 3; cold Print/string.Format block removed from parent) |
| **projected_helper_cyc** | 1 (baseline only — no branches, no conditionals) |
| **projected_parent_cyc_after** | 3 (final: RouteOrderToTargetDict call=0 + null check=1 + dict assignment=0 + RebuildOrSyncPositionEntry call=0 + LogOrderAdoption call=0 + adoptedCount++=0 + baseline=1 = 3) |

**New method signature:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private void LogOrderAdoption(string orderName, string dictName)
```

**Dependency:** Apply after T2 (T3 targets the already-simplified post-T2 body of `AdoptSingleOrder`).

**Jane Street patterns:** `carl_cook` (`[NoInlining]` — cold path; keeps Print/string.Format JIT overhead out of hot-path instruction cache)

---

### TICKET 4 — Extract `IsOrderEligibleForAdoption` from `AdoptOrdersFromAccount`

| Field | Value |
|-------|-------|
| **ticket_id** | T4 |
| **parent_method** | `AdoptOrdersFromAccount` (line 930, CYC=10) |
| **helper_name** | `IsOrderEligibleForAdoption` |
| **concern** | Consolidate per-order guard chain: null name check + `IsValidOrderState` check + instrument match + account membership. Returns `false` to skip an order. No routing, no mutation, no logging. |
| **lines_to_move** | The inline guard chain inside the `foreach (var ord in acct.Orders)` loop of `AdoptOrdersFromAccount` (~8–12 LOC): null name check, `IsValidOrderState(ord)`, instrument match, account membership. These guards currently run inline and drive the 4-deep nesting. |
| **cyc_reduction** | 4 (10 → 6; 4 inline guard branches extracted into predicate) |
| **projected_helper_cyc** | 4 (null name=1 + IsValidOrderState=1 + instrument check=1 + baseline=1) |
| **projected_parent_cyc_after** | 6 (foreach=1 + eligibility guard=1 + null classification guard=1 + AdoptSingleOrder call=0 + baseline=1 + 2 remaining structural conditionals = 6) |

**New method signature:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private bool IsOrderEligibleForAdoption(Order ord, Account acct)
```

**Independent of T2/T3** — targets a different parent method (`AdoptOrdersFromAccount`, not `AdoptSingleOrder`). Can be applied in parallel with T2/T3 if desired.

**Jane Street patterns:** `trading_billions` (defense-in-depth as isolated guard layer), `carl_cook` (`[AggressiveInlining]` — pure boolean predicate on per-order hot path, no alloc)

---

## Post-Extraction CYC Summary

| Symbol | CYC Before | CYC After | All Tickets Applied | Status |
|--------|-----------|-----------|---------------------|--------|
| `ClassifyOrderByPrefix` | 20 | 4 | T1 | ✅ ≤8 |
| `AdoptSingleOrder` | 11 | 3 | T2 + T3 | ✅ ≤8 |
| `AdoptOrdersFromAccount` | 10 | 6 | T4 | ✅ ≤8 |
| `_fleetPrefixTable` (new field) | — | 0 | T1 | ✅ data |
| `RebuildOrSyncPositionEntry` (new) | — | 4 | T2 | ✅ ≤8 |
| `LogOrderAdoption` (new) | — | 1 | T3 | ✅ ≤8 |
| `IsOrderEligibleForAdoption` (new) | — | 4 | T4 | ✅ ≤8 |

**projected_parent_cyc_after_all:** 6 (worst-case parent: `AdoptOrdersFromAccount` post-T4)
**max_cyc_projected:** 6 — Jane Street strict threshold (CYC ≤ 8) met by all symbols ✅

---

## Execution Order

| Step | Ticket | Target Method | Dependency |
|------|--------|--------------|------------|
| 1 | T1 | `ClassifyOrderByPrefix` | None (independent) |
| 2 | T2 | `AdoptSingleOrder` | None (independent from T1, T4) |
| 3 | T3 | `AdoptSingleOrder` | **After T2** (targets post-T2 body) |
| 4 | T4 | `AdoptOrdersFromAccount` | None (independent from T1/T2/T3) |

T1 and T4 are fully independent. T2 must precede T3 (both target `AdoptSingleOrder`).

---

## Sequential Thinking Evidence

**Thought 1 — Ticket Count:** Original `ClassifyAndRouteFleetOrder` was decomposed by Wave 4/6 into 3 live helpers, all exceeding CYC=8. Architecture plan specifies 4 extraction/refactor operations: (1) table-driven refactor of `ClassifyOrderByPrefix`, (2) `RebuildOrSyncPositionEntry` from `AdoptSingleOrder`, (3) `LogOrderAdoption` from `AdoptSingleOrder`, (4) `IsOrderEligibleForAdoption` from `AdoptOrdersFromAccount`. Per protocol "one ticket = one extracted helper = one concern", ticket_count=4.

**Thought 2 — Lines and CYC Math per Ticket:** T1 replaces 8-branch if/else-if with static lookup (CYC 20→4, reduction=16). T2 extracts position-tracking block from `AdoptSingleOrder` (~20-25 LOC, helper CYC=4, parent intermediate CYC=7). T3 extracts cold Print block from `AdoptSingleOrder` post-T2 (~3-5 LOC, helper CYC=1, parent final CYC=3). T4 extracts per-order guard chain from `AdoptOrdersFromAccount` (~8-12 LOC, helper CYC=4, parent CYC=6).

**Thought 3 — CYC ≤ 8 Verification:** All 7 symbols post-extraction: ClassifyOrderByPrefix=4, AdoptSingleOrder=3, AdoptOrdersFromAccount=6, _fleetPrefixTable=0, RebuildOrSyncPositionEntry=4, LogOrderAdoption=1, IsOrderEligibleForAdoption=4. Max = 6. All ≤8. ✅

**Thought 4 — Final Validation:** Ticket ordering confirmed (T2 before T3 for AdoptSingleOrder). T1 and T4 are independent. ticket_count=4 correct. DNA cleared in Phase 3 (PASS, violations=[]). Scope: single file `src/V12_002.SIMA.Lifecycle.cs`. 04-tickets.md ready to write.

---

## jCodemunch MCP Evidence

| Tool | Finding |
|------|---------|
| `resolve_repo` | Repo indexed, 5147 symbols, status=loadable ✅ |
| `get_symbol_complexity("ClassifyOrderByPrefix")` | Symbol not in live index (method exists in src/ but not indexed at query time — consistent with Phase 2 finding that only src-vm-backup copy was indexed). CYC=20 confirmed from Phase 2 MCP evidence and architecture plan. |
| `get_extraction_candidates(src/V12_002.SIMA.Lifecycle.cs, min_complexity=8)` | 0 candidates returned (jcodemunch heuristic requires multi-file callers — private helpers fail this filter). Manual extraction plan from Phase 2 used as authoritative source. |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Epic ID** | EPIC-W7-005 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Method (original)** | ClassifyAndRouteFleetOrder (CYC=16, decomposed by Wave 4/6) |
| **Live Successor Targets** | ClassifyOrderByPrefix (CYC=20), AdoptSingleOrder (CYC=11), AdoptOrdersFromAccount (CYC=10) |
| **Source File** | src/V12_002.SIMA.Lifecycle.cs |
| **ticket_count** | 4 |
| **projected_parent_cyc_after_all** | 6 (AdoptOrdersFromAccount) |
| **max_cyc_projected** | 6 |
| **MCP Tools Used** | `resolve_repo`, `get_symbol_complexity`, `get_extraction_candidates`, `search_symbols`, `sequentialthinking` (4 thoughts) |
| **Bobcoins Used** | ~8 MCP calls |
| **Execution Time** | ~60 seconds |
| **Output** | docs/brain/EPIC-W7-005/04-tickets.md |
| **Status** | Phase 4 Complete |

---
*Generated by v12-phase4-tickets — Wave 7, Phase 4*
*Protocol: EPIC-W7-005 / 04-tickets.md / V12.23*
