# Phase 2: Architecture Plan — EPIC-W7-153

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-153/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `HandleTrimCommand`
- **Source File:** `src/V12_002.UI.IPC.Commands.Config.cs`
- **Lines:** 37–146 (110 lines)
- **Original CYC:** 20
- **Signature:** `private void HandleTrimCommand(string action, string[] parts)`

### jcodemunch get_context_bundle result
Symbol resolved via exact ID `src/V12_002.UI.IPC.Commands.Config.cs::V12_002.HandleTrimCommand#method`. Full source retrieved (110 lines). The method:
1. Parses `percent` from `action` string (`TRIM_50` → 0.5, else 0.25 for `TRIM_25`).
2. Snapshots `activePositions.Values.ToArray()` and iterates.
3. Per position: guards `pos.RemainingContracts > 1`; computes `rawQty` with safety floor; double-guards validity.
4. Routes to **SIMA path** (`Account.CreateOrder` + `Account.Submit`) when `EnableSIMA && pos.IsFollower && pos.ExecutingAccount != null`, with signal name truncation at 50 chars.
5. Routes to **unmanaged path** (`SubmitOrderUnmanaged`) for non-SIMA positions, with inline direction branch (`Long`→Sell / else→BuyToCover).
Imports: NinjaTrader.Cbi, System.Collections.Concurrent, plus standard System namespaces.

### jcodemunch get_call_hierarchy result
- **Callers (depth 2):** 0 direct callers resolved in index (IPC command dispatcher routes via string matching, not direct symbol reference — 2 callers confirmed via 00-hotspots.md: `TryHandleFleet_Trim` in `src/V12_002.UI.IPC.Commands.Fleet.cs:87`).
- **Callees (depth 1):** `LogBuffer.Format` (×2 — src and src-vm-backup variants), resolved as `ast_inferred`.
- **Callees (depth 2):** `LogBuffer.ValidateThreadAffinity`, `LogBuffer.FormatInternal` — log pipeline only.
- **Key finding:** No callers resolved via AST (IPC dispatch is string-based routing); method signature must remain unchanged. Extracted helpers are purely internal.

### jcodemunch get_dependency_graph result
- `src/V12_002.UI.IPC.Commands.Config.cs` — **0 import edges, 0 importer edges** in the index.
- File is a C# partial class fragment; all imports are carried in the enclosing partial class. No cross-file extraction needed — all new helpers remain in the same file.

### jcodemunch get_extraction_candidates result
- No candidates returned (min_complexity=3, min_callers=1). The index does not surface extraction candidates for this file because no external callers are indexed for the helpers. This is expected — analysis proceeds from 00-hotspots.md source-level analysis which provides direct CYC driver detail.

---

## Sequential Thinking Summary

5-thought chain completed. Final conclusion (Thought 5):

> **HYPOTHESIS VERIFIED:** `HandleTrimCommand` (CYC=20) decomposes into 5 private helpers — `ComputeSafeTrimQty` (CYC=3), `BuildTrimSignalName` (CYC=2), `SubmitSimaTrimOrder` (CYC=1), `SubmitUnmanagedTrimOrder` (CYC=1), and `TrimSinglePosition` (CYC=6) — leaving the parent at CYC=3. Maximum projected CYC across all 6 methods = **6**, satisfying the Jane Street ≤8 mandate.
>
> Key design decisions validated:
> - `ComputeSafeTrimQty` receives `int remaining` (value, not volatile reference) — prevents torn reads of `pos.RemainingContracts`.
> - `-1` sentinel from `ComputeSafeTrimQty` makes invalid quantity state unrepresentable at the call site.
> - Guard clause early return in `TrimSinglePosition` replaces the nested if/else chain.
> - `OrderAction` passed by value into `SubmitUnmanagedTrimOrder` eliminates the inline direction branch.
> - All 5 helpers are `private` methods in the same partial class — zero cross-file blast radius.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `ComputeSafeTrimQty(int remaining, double percent)` | Pure function: compute safe trim quantity from snapshot values; returns -1 sentinel when trim is mathematically impossible (prevents invalid qty state) | 3 |
| `BuildTrimSignalName(string signalName)` | Constructs "Trim_" + signalName and truncates to 50 chars if needed; single string concern | 2 |
| `SubmitSimaTrimOrder(PositionInfo pos, OrderAction trimAction, int rawQty, double percent)` | SIMA fleet follower order path: calls BuildTrimSignalName, Account.CreateOrder, Account.Submit, Print with fleet log format | 1 |
| `SubmitUnmanagedTrimOrder(PositionInfo pos, OrderAction trimAction, int rawQty)` | NinjaTrader unmanaged order path: Print with IPC log format, then SubmitOrderUnmanaged; direction branch eliminated by pre-computed OrderAction param | 1 |
| `TrimSinglePosition(PositionInfo pos, double percent)` | Per-position trim orchestration: snapshot RemainingContracts to int, call ComputeSafeTrimQty, guard clause early return, compute trimAction, route to SIMA or unmanaged path | 6 |

---

## Parent Method After Extraction

```csharp
private void HandleTrimCommand(string action, string[] parts)
{
    double percent = action == "TRIM_50" ? 0.5 : 0.25;
    foreach (var pos in activePositions.Values.ToArray())
    {
        if (pos.RemainingContracts > 1)
            TrimSinglePosition(pos, percent);
        else
            Print(string.Format("IPC Trim SKIPPED: {0} has only 1 contract - use FLATTEN to close", pos.SignalName));
    }
}
```

- **Remaining logic:** Percent parsing (ternary, not a CYC branch) + foreach loop + single RemainingContracts guard with log-else. Pure orchestration/routing.
- **Projected CYC:** base(1) + foreach(+1) + if(RemainingContracts>1)(+1) = **3**

---

## max_cyc_projected: 6
## extraction_count: 5

---

## Method Signature Reference

| Method | Signature |
|---|---|
| **Parent (unchanged)** | `private void HandleTrimCommand(string action, string[] parts)` |
| Helper 1 | `private int ComputeSafeTrimQty(int remaining, double percent)` |
| Helper 2 | `private string BuildTrimSignalName(string signalName)` |
| Helper 3 | `private void SubmitSimaTrimOrder(PositionInfo pos, OrderAction trimAction, int rawQty, double percent)` |
| Helper 4 | `private void SubmitUnmanagedTrimOrder(PositionInfo pos, OrderAction trimAction, int rawQty)` |
| Helper 5 | `private void TrimSinglePosition(PositionInfo pos, double percent)` |

---

## CYC Budget Breakdown

| Method | Base | Branches | Total |
|---|---|---|---|
| `HandleTrimCommand` (parent) | 1 | foreach(+1), if(>1)(+1) | **3** |
| `ComputeSafeTrimQty` | 1 | if(remainingAfterTrim<1)(+1), if(rawQty<1)(+1) | **3** |
| `BuildTrimSignalName` | 1 | if(Length>50)(+1) | **2** |
| `SubmitSimaTrimOrder` | 1 | none | **1** |
| `SubmitUnmanagedTrimOrder` | 1 | none | **1** |
| `TrimSinglePosition` | 1 | if(qty<=0)(+1), ternary trimAction(+1), if(EnableSIMA&&IsFollower&&ExecutingAccount!=null)(+3) | **6** |

**Max CYC = 6** (all ≤ 8 ✅)

---

## Jane Street Alignment

| Rule | Status | Detail |
|---|---|---|
| **CYC<=8 achieved** | YES | Max CYC = 6 across all 6 methods |
| **Single-responsibility per helper** | YES | Each helper has exactly one named concern; no helper mixes order submission paths |
| **Lock-free/Actor pattern preserved** | YES | No lock() blocks introduced or modified; IPC command handler executes on designated thread; existing Actor/Enqueue patterns untouched |
| **Illegal states unrepresentable** | YES | (1) `ComputeSafeTrimQty` returns −1 sentinel — qty=0 invalid state cannot reach submission; (2) `TrimSinglePosition` early-returns on qty≤0 guard clause; (3) `int remaining` param (by value) prevents torn reads of volatile `pos.RemainingContracts` |
| **Zero-allocation hot paths** | YES | `ComputeSafeTrimQty` is pure int math (zero alloc); `BuildTrimSignalName` allocs one string (unavoidable for order signal; not in tick-rate hot path) |
| **Extract Guard Clauses** | YES | `TrimSinglePosition` uses early return on `qty <= 0` replacing original nested if/else |
| **Named helper methods (single concern)** | YES | 5 helpers, each named after its single domain responsibility |
| **No scope creep (V12.23)** | YES | All helpers are `private` in same partial class; no cross-file changes; parent signature unchanged |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 2.5 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Epic** | EPIC-W7-153 |
| **jcodemunch tools called** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output** | docs/brain/EPIC-W7-153/02-architecture-plan.md |
| **extraction_count** | 5 |
| **max_cyc_projected** | 6 |
