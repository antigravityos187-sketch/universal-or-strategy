# Phase 2: Architecture Plan — EPIC-W7-008

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-008/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `ManageCIT`
- **Source File:** `src/V12_002.Orders.Management.Flatten.cs`
- **Original CYC:** 19
- **Lines:** 68–128 (method body), partial class `V12_002 : Strategy`
- **Signature:** `private void ManageCIT()` — zero parameters, void return

### jcodemunch get_context_bundle result

`get_context_bundle` returned `Symbol(s) not found: ManageCIT` (ambiguous ID); fell back to `search_symbols` with `file_pattern=src/V12_002.Orders.Management.Flatten.cs`. Key findings:

| Symbol | Kind | Line | Signature |
|---|---|---|---|
| `ManageCIT` | method | 68 | `private void ManageCIT()` |
| `ValidateCitConfiguration` | method | 241 | `private bool ValidateCitConfiguration(out double citOffset)` |
| `ExecuteLocalNudge` | method | 133 | `private void ExecuteLocalNudge(string key, Order order, double newLimitPrice, double citOffset)` |

Symbol ID resolved as: `src/V12_002.Orders.Management.Flatten.cs::V12_002.ManageCIT#method`

### jcodemunch get_call_hierarchy result

**Callers (depth=2):** 0 resolved by AST (private method; active callers in `V12_002.BarUpdate.cs` confirmed by Phase 0/1 analysis as 2 active call sites).

**Callees (depth=1, AST-resolved):**

| Callee | File | Line | Resolution |
|---|---|---|---|
| `ValidateCitConfiguration` | `src/V12_002.Orders.Management.Flatten.cs` | 241 | ast_resolved |
| `ShouldChaseOrder` | `src/V12_002.Orders.Management.Flatten.cs` | 199 | ast_resolved |
| `CalculateNudgedPrice` | `src/V12_002.Orders.Management.Flatten.cs` | 228 | ast_resolved |
| `ExecuteFollowerNudge` | `src/V12_002.Orders.Management.Flatten.cs` | 146 | ast_resolved |
| `ExecuteLocalNudge` | `src/V12_002.Orders.Management.Flatten.cs` | 133 | ast_resolved |
| `entryOrders` | `src/V12_002.cs` | 200 | ast_inferred (shared ConcurrentDictionary) |
| `activePositions` | `src/V12_002.cs` | 199 | ast_inferred (shared read surface) |
| `_citNudgedKeys` | `src/V12_002.cs` | 841 | ast_inferred (one-shot guard set) |
| `Enqueue` | `src/V12_002.cs` | 428 | ast_inferred (actor self-requeue, depth=2) |

### jcodemunch get_dependency_graph result

- **Node count:** 1, **Edge count:** 0
- `src/V12_002.Orders.Management.Flatten.cs` is a partial class fragment; no top-level import declarations resolved by graph engine
- All inter-file dependencies flow through the `V12_002` partial class definition in `src/V12_002.cs`
- Blast radius confirmed by Phase 0: `entryOrders` → 22 referencing files; `activePositions` → 41 referencing files

### jcodemunch get_extraction_candidates result

- Returned empty (complexity metadata not populated for this index version)
- Extraction candidates derived from Phase 0 static analysis and sequential thinking (see below)
- Phase 0 analysis is authoritative: 3 targeted extractions confirmed

---

## Sequential Thinking Summary

**5-thought chain executed. Final verdict (Thought 5):**

ManageCIT body CYC 9 must be reduced to <=8. The aggregate cluster CYC of 19 distributes across 9 symbols. Three new private helpers resolve all violations:

1. **`ExecuteCitNudgeWithFaultIsolation`** wraps the dual try/catch block (fault-isolation wrapper pattern). Removes the try/catch branching from ManageCIT body. Own CYC = 4.
2. **`TryNudgeOrder`** unifies the `isFollower` dispatch and `ref int budget` halt contract. Called from within `ExecuteCitNudgeWithFaultIsolation`. Own CYC = 3.
3. **`IsPriceTouchingLimit`** extracts the directional price-touch comparison from `ShouldChaseOrder`. Pure predicate. Own CYC = 3. Reduces `ShouldChaseOrder` from CYC 7 to CYC 5.

ManageCIT body after extraction: foreach loop + 2 guard continues + isFollower lookup + `ExecuteCitNudgeWithFaultIsolation` call → **projected CYC = 6**.

Maximum projected CYC across all 9 methods in cluster: **6**. All <= 8. Jane Street CYC mandate: SATISFIED.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `ExecuteCitNudgeWithFaultIsolation` | Fault-isolation wrapper: wraps `TryNudgeOrder` in try/catch (InvalidOperationException when ChangeOrder + broad Exception). Returns false if budget exhausted, protecting remaining fleet accounts. | 4 |
| `TryNudgeOrder` | Dispatch router: if isFollower → `ExecuteFollowerNudge`; else `ExecuteLocalNudge` + `CalculateNudgedPrice`. Returns false if broker budget halted. | 3 |
| `IsPriceTouchingLimit` | Pure directional price-touch predicate: Buy → `price <= Low[0]`; Sell → `price >= High[0]`. Extracted from `ShouldChaseOrder`. Enables unit testing of the Build 984 regression path. | 3 |

### Full Method Signatures

```csharp
// Fault-isolation wrapper — wraps TryNudgeOrder; returns false on budget exhaustion
private bool ExecuteCitNudgeWithFaultIsolation(
    string key, Order order, double citOffset, bool isFollower, ref int budget);

// Dispatch router — follower vs local nudge path
private bool TryNudgeOrder(
    string key, Order order, double citOffset, bool isFollower, ref int budget);

// Pure predicate — directional price-touch check
private bool IsPriceTouchingLimit(Order order);
```

### ShouldChaseOrder (internal modification — in-scope per Phase 1)

`ShouldChaseOrder` is part of the ManageCIT helper cluster (Phase 1 scope boundary confirmed). The directional price-touch logic (2 CYC) is extracted as `IsPriceTouchingLimit`, reducing `ShouldChaseOrder` from CYC 7 to **projected CYC 5**.

---

## Parent Method After Extraction

**Remaining logic in `ManageCIT` body:**

```
1. foreach (var kvp in entryOrders)
2.   if (!ValidateCitConfiguration(out double citOffset)) continue;   // guard 1
3.   if (!ShouldChaseOrder(kvp.Value)) continue;                      // guard 2
4.   bool isFollower = <activePositions lookup>;
5.   if (!ExecuteCitNudgeWithFaultIsolation(kvp.Key, kvp.Value, citOffset, isFollower, ref budget)) break;
```

- Exception recovery policy: fully delegated to `ExecuteCitNudgeWithFaultIsolation`
- Dispatch routing: fully delegated to `TryNudgeOrder`
- Price-touch predicate: fully delegated to `IsPriceTouchingLimit` (via `ShouldChaseOrder`)
- Self-requeue on budget exhaustion: unchanged — `Enqueue(ctx => ctx.ManageCIT())` remains in `ExecuteFollowerNudge`

**Projected CYC: 6** (base:1 + foreach:+1 + ValidateCitConfiguration guard:+1 + ShouldChaseOrder guard:+1 + isFollower null-conditional:+1 + ExecuteCitNudge result check:+1)

---

## Full Cluster CYC After Extraction

| Method | Before | After | Status |
|---|---|---|---|
| `ManageCIT` body | 9 | 6 | ✅ |
| `ExecuteCitNudgeWithFaultIsolation` | — (new) | 4 | ✅ |
| `TryNudgeOrder` | — (new) | 3 | ✅ |
| `IsPriceTouchingLimit` | — (new) | 3 | ✅ |
| `ShouldChaseOrder` | 7 | 5 | ✅ |
| `ValidateCitConfiguration` | 5 | 5 | ✅ (unchanged) |
| `ExecuteFollowerNudge` | 4 | 4 | ✅ (unchanged) |
| `CalculateNudgedPrice` | 2 | 2 | ✅ (unchanged) |
| `ExecuteLocalNudge` | 1 | 1 | ✅ (unchanged) |
| **Cluster aggregate** | **19** | **33 (distributed)** | ✅ all <=8 |

## max_cyc_projected: 6
## extraction_count: 3

---

## Jane Street Alignment

| Principle | Status |
|---|---|
| **CYC<=8 achieved** | YES — max projected CYC = 6 across all 9 cluster methods |
| **Single-responsibility per helper** | YES — `ExecuteCitNudgeWithFaultIsolation` handles fault isolation only; `TryNudgeOrder` handles dispatch only; `IsPriceTouchingLimit` is a pure predicate only |
| **Lock-free/Actor pattern preserved** | YES — `ref int budget` threaded by value avoids heap boxing; self-requeue via `Enqueue(ctx => ctx.ManageCIT())` in `ExecuteFollowerNudge` unchanged; no `lock()` blocks added or modified |
| **Illegal states unrepresentable** | YES — bool returns on `ExecuteCitNudgeWithFaultIsolation` and `TryNudgeOrder` make success/failure states explicit; no nullable return paths; `IsPriceTouchingLimit` is a pure bool with no side effects |
| **Zero-allocation hot paths** | YES — no heap allocations in any extracted helper; `ref int budget` avoids boxing; `bool` returns are stack-only |
| **Guard clause pattern** | YES — `ValidateCitConfiguration` and `ShouldChaseOrder` guard clauses remain as early `continue` statements in ManageCIT body |
| **Extract loop body** | YES — entire try/catch loop body extracted to `ExecuteCitNudgeWithFaultIsolation` |
| **FSM/dispatch decomposition** | YES — `TryNudgeOrder` is the single dispatch point for follower vs local path |
| **Build 984 regression risk addressed** | YES — `IsPriceTouchingLimit` isolates the directional price-touch logic enabling standalone unit testing |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-008 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Source File** | `src/V12_002.Orders.Management.Flatten.cs` |
| **Method** | `ManageCIT` |
| **Original CYC** | 19 |
| **Max CYC Projected** | 6 |
| **Extraction Count** | 3 |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | resolve_repo, search_symbols (fallback), get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Boundary Verdict** | PASS (from Phase 1.5) |
| **V12.23 No Scope Creep** | ENFORCED |
