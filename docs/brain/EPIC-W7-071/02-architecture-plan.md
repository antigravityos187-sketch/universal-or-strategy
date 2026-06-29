# Phase 2: Architecture Plan — EPIC-W7-071

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-071/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `ShadowProcessFollowerStopUpdate`
- **Source File:** `src/V12_002.SIMA.Shadow.cs`
- **Signature:** `private bool ShadowProcessFollowerStopUpdate(string followerEntryName, double newStopPrice, out bool waitingOnFollower)`
- **Lines:** 246–291
- **Original CYC:** 13

### jCodemunch get_context_bundle result

Symbol resolved at `src/V12_002.SIMA.Shadow.cs` line 246–291. Full source confirmed. The method:
1. Performs dual TryGetValue lookups (`_followerBrackets`, `activePositions`) into `bool hasFsm` and `bool hasFollowerPos`.
2. Returns `false` early if both lookups miss (unknown follower).
3. Returns `true + waitingOnFollower=true` if position not ready (not filled / bracket not submitted).
4. Returns `true + waitingOnFollower=true` if FSM not Active or has no StopOrder.
5. Returns `true` (no-op) if current stop is within half a tick of target.
6. Prints log line and delegates to `UpdateStopOrder` — the two-phase replace FSM.

### jCodemunch get_call_hierarchy result

- **Direct callers (depth=1):** `ShadowMoveFollowerStops` (same file, line 297)
- **Depth-2 callers:** `PropagateAndCacheStopPrice` (same file, line 138)
- **Key callees (depth=1):** `_followerBrackets` (read), `activePositions` (read), `UpdateStopOrder` (write — two-phase replace FSM), `LogBuffer.Format`
- **Depth-2 callees:** `HandleStalePendingReplacement`, `UpdateExistingPendingReplacement`, `InitiateStopReplacement`, `CreateDirectStopOrder`, `HandleUpdateException`, `ValidateStopPrice`
- **Caller count:** 2 (depth-1: ShadowMoveFollowerStops; depth-2: PropagateAndCacheStopPrice)
- **Callee count:** 28 transitively (all via UpdateStopOrder two-phase FSM)

### jCodemunch get_dependency_graph result

- **File:** `src/V12_002.SIMA.Shadow.cs`
- **Direction:** both | **Depth:** 1
- **Importers:** 0 indexed (C# partial class — file-level imports not resolved by index)
- **Imports:** 0 indexed
- **Note:** C# partial class pattern — all dependencies are same-assembly, resolved at compile time. Cross-file calls confirmed via call hierarchy above.

### jCodemunch get_extraction_candidates result

- **Candidates returned:** 0 (min_complexity=3, min_callers=1)
- **Interpretation:** `ShadowProcessFollowerStopUpdate` has no sub-methods meeting the multi-caller threshold since it is the leaf hotspot. The extraction design is therefore driven by CYC decomposition of its internal guard clauses (identified via get_context_bundle + hotspot analysis), not by multi-caller sub-function promotion.

---

## Sequential Thinking Summary

**Final thought (Thought 5):**

Approved extraction plan for `ShadowProcessFollowerStopUpdate` (CYC 13 → 5 after extraction):

Five private helpers extracted, each with exactly one responsibility:
1. `IsFollowerUnknown` — unknown-follower early-exit predicate (CYC 2)
2. `IsFollowerPositionNotReady` — position-not-ready predicate (CYC 3)
3. `IsFsmNotReady` — FSM-not-active predicate (CYC 3)
4. `IsStopPriceAtTarget` — half-tick proximity predicate (CYC 2)
5. `ExecuteFollowerStopPropagation` — log + delegate to UpdateStopOrder (CYC 1)

Parent after extraction retains 4 guard if/return blocks (each a single named-predicate call — no &&/|| in parent) plus 1 unconditional call to ExecuteFollowerStopPropagation. Parent CYC = 5.

All constraints satisfied: every symbol CYC <= 8, single-responsibility per helper, zero-allocation, lock-free, no scope creep, three-valued return semantics of parent unchanged.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `IsFollowerUnknown(bool hasFsm, bool hasFollowerPos)` | Returns true when both `_followerBrackets` and `activePositions` lookups missed — signals completely unknown follower | 2 |
| `IsFollowerPositionNotReady(bool hasFollowerPos, PositionInfo followerPos)` | Returns true when PositionInfo is absent, entry not yet filled, or bracket not yet submitted | 3 |
| `IsFsmNotReady(bool hasFsm, FollowerBracketFSM fsm)` | Returns true when FSM is null, not in Active state, or has no live StopOrder | 3 |
| `IsStopPriceAtTarget(Order stopOrder, double newStopPrice)` | Returns true when current stop price is within half a tick of the target — half-tick no-op guard | 2 |
| `ExecuteFollowerStopPropagation(string followerEntryName, PositionInfo followerPos, double newStopPrice, FollowerBracketFSM fsm)` | Emits the `[SHADOW] Propagating stop` log line and calls `UpdateStopOrder` to initiate two-phase replace FSM | 1 |

---

## Parent Method After Extraction

**Remaining logic:**
```
waitingOnFollower = false;
Lookup hasFsm + hasFollowerPos (2 TryGetValue reads — unchanged)
if (IsFollowerUnknown(hasFsm, hasFollowerPos)) return false;
if (IsFollowerPositionNotReady(hasFollowerPos, followerPos)) { waitingOnFollower = true; return true; }
if (IsFsmNotReady(hasFsm, fsm)) { waitingOnFollower = true; return true; }
if (IsStopPriceAtTarget(fsm.StopOrder, newStopPrice)) return true;
ExecuteFollowerStopPropagation(followerEntryName, followerPos, newStopPrice, fsm);
return true;
```

- **Projected CYC:** 5
  - Base: 1
  - `if (IsFollowerUnknown(...))`: +1
  - `if (IsFollowerPositionNotReady(...))`: +1
  - `if (IsFsmNotReady(...))`: +1
  - `if (IsStopPriceAtTarget(...))`: +1
  - Sequential call to `ExecuteFollowerStopPropagation`: +0
  - **Total: 5**

---

## max_cyc_projected: 5
## extraction_count: 5

---

## Jane Street Alignment

| Principle | Status |
|---|---|
| CYC<=8 achieved | YES — max CYC across all 6 symbols (parent + 5 helpers) = 5 |
| Single-responsibility per helper | YES — each helper encapsulates exactly one named predicate or one action |
| Lock-free/Actor pattern preserved | YES — no lock() blocks introduced; all reads are TryGetValue on ConcurrentDictionary; UpdateStopOrder called identically as before |
| Illegal states unrepresentable | YES — three-valued return semantics (false=unknown, true+waiting=not-ready, true=updated-or-noop) preserved; named predicates make state machine order explicit |
| Zero-allocation hot path | YES — all helpers take stack-bound parameters; no closures, no LINQ, no heap allocations |
| Extract guard clauses (early returns) | YES — all four guard clauses are isolated to named predicates + early return in parent |
| No scope creep (V12.23) | YES — all new methods are `private`, same partial class, same file |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 2.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jCodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **max_cyc_projected** | 5 |
| **extraction_count** | 5 |
| **Output** | docs/brain/EPIC-W7-071/02-architecture-plan.md |
