# Phase 2: Architecture Plan — EPIC-W7-107

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-107/01-scope-boundary.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `HydrateFromOpenPositions` |
| **Source File** | [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:625) |
| **Original CYC** | 34 |
| **Lines** | 625 – 780 |
| **Signature** | `private int HydrateFromOpenPositions(ConcurrentDictionary<string,Order> stopOrders, …target1..5Orders, ref int ordersIndexed, ref int fsmCreated)` |

### jcodemunch `get_context_bundle` result
Symbol ID resolved via `search_symbols` fallback: `src/V12_002.SIMA.Lifecycle.cs::V12_002.HydrateFromOpenPositions#method` (line 625, byte_length 6536). Context bundle not available via direct call (symbol lookup required search fallback). Full source read directly from file lines 625-780.

### jcodemunch `get_call_hierarchy` result
- **Direct callers (depth=1):** `HydrateFSMsFromWorkingOrders` (line 787, same file)
- **Indirect callers (depth=2):** `HydrateWorkingOrdersFromBroker` (line 309, same file)
- **Callees (depth=1):** `IsFleetAccount`, `stopOrders` (ConcurrentDictionary param), `_followerBrackets`, `target1Orders..target5Orders`, `LogBuffer.Format`
- **Callees (depth=2):** `LogBuffer.ValidateThreadAffinity`, `LogBuffer.FormatInternal`
- **Caller count:** 2 (direct chain); 1 direct caller per 00-scope.md

### jcodemunch `get_dependency_graph` result
- **Imports:** none resolved (C# partial class — imports are at project level, not file-to-file)
- **Importers:** none resolved
- **Blast radius:** confined to `src/V12_002.SIMA.Lifecycle.cs` + private helpers in same partial class

### jcodemunch `get_extraction_candidates` result
- No candidates returned (index requires min_callers=1 cross-file; helpers are same-file private methods below threshold)
- Extraction plan derived from source analysis and hotspot complexity breakdown

---

## Sequential Thinking Summary

**5-thought chain completed.** Final thought conclusion:

`HydrateFromOpenPositions` (CYC=34) decomposes cleanly into **6 private helper methods** plus a refactored parent. The three dominant complexity drivers identified in Phase 0 are each addressed by dedicated helpers:

1. The **×5 copy-pasted target-order linking blocks** (~15 CYC) are collapsed into a single `LinkTargetOrdersToFsm` method using an indexed for-loop over a `ConcurrentDictionary<string,Order>[]` array parameter — eliminating 11+ CYC points in one extraction.
2. The **inner stop-order scan** (~4 CYC) is isolated in `TryRecoverStopOrder` (CYC=5).
3. The **account-skip predicates** are split into `HasExistingFsmForAccount` (CYC=2) and `TryGetAccountOpenPosition` (CYC=3).
4. **FSM construction** is isolated in `BuildPositionRecoveryFSM` (CYC=1).
5. **Stop order linkage** is isolated in `LinkStopOrderToFsm` (CYC=3).

After extraction the parent method retains only orchestration: one foreach loop + six guard branches + helper calls = **CYC=7**. All helpers and the parent project to CYC≤8. All Jane Street rules are satisfied.

---

## Extraction Plan

| # | Helper Method Name | Responsibility | Projected CYC | Lines Extracted |
|---|---|---|---|---|
| 1 | `HasExistingFsmForAccount(Account acct)` → `bool` | Returns true if `_followerBrackets` already contains an FSM whose `AccountName` matches `acct.Name` (LINQ Any predicate) | **2** | 643–648 |
| 2 | `TryGetAccountOpenPosition(Account acct, out Position pos)` → `bool` | Resolves a non-Flat position on the current `Instrument` for `acct`; sets `pos` and returns false if none | **3** | 651–655 |
| 3 | `TryRecoverStopOrder(ConcurrentDictionary<string,Order> stopOrders, Account acct, out string recoveredKey, out Order recoveredStop)` → `bool` | Scans `stopOrders` for the first entry whose account matches `acct`; sets `recoveredKey`/`recoveredStop` | **5** | 658–675 |
| 4 | `BuildPositionRecoveryFSM(Account acct, string recoveredKey, Position acctPos)` → `FollowerBracketFSM` | Constructs a new `FollowerBracketFSM` with `State=Active`, `RemainingContracts=Math.Abs(qty)` from recovered position data | **1** | 696–704 |
| 5 | `LinkStopOrderToFsm(FollowerBracketFSM fsm, Order recoveredStop, string recoveredKey, ref int ordersIndexed)` → `void` | Attaches `recoveredStop` to `fsm.StopOrder`; indexes `recoveredStop.OrderId` in `_orderIdToFsmKey` if non-empty | **3** | 706–715 |
| 6 | `LinkTargetOrdersToFsm(FollowerBracketFSM fsm, string recoveredKey, ConcurrentDictionary<string,Order>[] targetOrderSets, ref int ordersIndexed)` → `void` | Replaces the ×5 copy-pasted target-order blocks with a for-loop over `targetOrderSets`; sets `fsm.Targets[i]` and indexes each non-empty `OrderId` | **4** | 717–763 |

---

## Parent Method After Extraction

**Remaining logic (orchestration only):**
```csharp
private int HydrateFromOpenPositions(
    ConcurrentDictionary<string, Order> stopOrders,
    ConcurrentDictionary<string, Order> target1Orders,
    ConcurrentDictionary<string, Order> target2Orders,
    ConcurrentDictionary<string, Order> target3Orders,
    ConcurrentDictionary<string, Order> target4Orders,
    ConcurrentDictionary<string, Order> target5Orders,
    ref int ordersIndexed,
    ref int fsmCreated
)
{
    int positionFsmCreated = 0;
    var targetOrderSets = new[] { target1Orders, target2Orders, target3Orders, target4Orders, target5Orders };
    foreach (Account acct in Account.All)
    {
        if (!IsFleetAccount(acct))
            continue;
        if (HasExistingFsmForAccount(acct))
            continue;
        if (!TryGetAccountOpenPosition(acct, out Position acctPos))
            continue;
        if (!TryRecoverStopOrder(stopOrders, acct, out string recoveredKey, out Order recoveredStop))
        {
            Print(string.Format("[SIMA] Phase 5 Position Pass: WARNING -- open position on {0} but no stopOrders key found. FSM not created. REAPER grace window started.", acct.Name));
            _positionPassFailedFirstSeen[acct.Name] = DateTime.UtcNow;
            continue;
        }
        if (_followerBrackets.ContainsKey(recoveredKey))
            continue;
        var fsm = BuildPositionRecoveryFSM(acct, recoveredKey, acctPos);
        LinkStopOrderToFsm(fsm, recoveredStop, recoveredKey, ref ordersIndexed);
        LinkTargetOrdersToFsm(fsm, recoveredKey, targetOrderSets, ref ordersIndexed);
        _followerBrackets.TryAdd(recoveredKey, fsm);
        positionFsmCreated++;
        fsmCreated++;
        Print(string.Format("[SIMA] Phase 5 Position Pass: Created FSM for {0} (key={1})", acct.Name, recoveredKey));
    }
    return positionFsmCreated;
}
```

**Projected CYC:** **7**
- 1 (base) + 1 (foreach) + 1 (IsFleetAccount guard) + 1 (HasExistingFsmForAccount guard) + 1 (TryGetAccountOpenPosition guard) + 1 (TryRecoverStopOrder guard) + 1 (ContainsKey guard) = **7**

---

## max_cyc_projected: 7
## extraction_count: 6

---

## CYC Reduction Summary

| Symbol | Before | After |
|---|---|---|
| `HydrateFromOpenPositions` (parent) | 34 | **7** |
| `HasExistingFsmForAccount` | — | **2** |
| `TryGetAccountOpenPosition` | — | **3** |
| `TryRecoverStopOrder` | — | **5** |
| `BuildPositionRecoveryFSM` | — | **1** |
| `LinkStopOrderToFsm` | — | **3** |
| `LinkTargetOrdersToFsm` | — | **4** |
| **Max projected CYC** | | **7** |

---

## Jane Street Alignment

| Rule | Status | Evidence |
|---|---|---|
| CYC<=8 achieved | **YES** | Max CYC=7 (parent); all helpers 1–5 |
| Single-responsibility per helper | **YES** | Each helper does exactly one named operation |
| Lock-free / Actor pattern preserved | **YES** | No `lock()` blocks; `ConcurrentDictionary.TryAdd` used throughout |
| Illegal states unrepresentable | **YES** | `BuildPositionRecoveryFSM` enforces required fields at construction; `out` params in Try* methods ensure key/value are always paired |
| Zero-allocation hot paths | **YES** | `TryGetValue` with `out` param, `string.Equals` with `StringComparison` (no culture alloc); `targetOrderSets` array is lifecycle-init path only |
| Extract Guard Clauses | **YES** | All 5 early-return `continue` guards remain in parent as flat guard chain |
| Extract Loop Body | **YES** | Inner stop-order scan loop extracted to `TryRecoverStopOrder`; ×5 target blocks extracted to `LinkTargetOrdersToFsm` loop |
| Replace ×N copy-paste with loop | **YES** | 5 identical target-linking blocks → 1 indexed for-loop over `ConcurrentDictionary[]` |
| FSM Decomposition | **YES** | FSM construction isolated in `BuildPositionRecoveryFSM`; state linkage in dedicated `Link*` helpers |
| Method signature unchanged | **YES** | `HydrateFromOpenPositions` signature identical; all call sites preserved |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | `resolve_repo`, `search_symbols`, `get_call_hierarchy`, `get_dependency_graph`, `get_extraction_candidates` |
| **sequential-thinking calls** | 5 |
| **Source lines read** | 625–780 (156 lines) |
| **Extraction count** | 6 |
| **max_cyc_projected** | 7 |
