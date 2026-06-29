# Phase 2: Architecture Plan — EPIC-W7-147

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-147/01-scope-boundary.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `ProcessQueuedExecution_HandleFleetOCO` |
| **Source File** | `src/V12_002.UI.Compliance.cs` |
| **Lines** | 698–727 |
| **Original CYC** | 15 |
| **Target CYC** | <= 8 |
| **Signature** | `private void ProcessQueuedExecution_HandleFleetOCO(QueuedAccountExecution item)` |

### jcodemunch get_context_bundle result

`get_context_bundle` returned `Symbol(s) not found` for the bare name; resolved via `search_symbols` fallback with `file_pattern=src/V12_002.UI.Compliance.cs`. Symbol confirmed at line 698, kind=method, signature `private void ProcessQueuedExecution_HandleFleetOCO(QueuedAccountExecution item)`. Full source retrieved via `get_symbol_source` (lines 698–727, 30 lines). Source contains: outer `try/catch`, compound 4-`&&`+1-`||` guard, inner `if (StartsWith("Stop_"))` branch, `else if (StartsWith("T") && Length>2 && [2]=='_')` 3-operand branch.

### jcodemunch get_call_hierarchy result

`get_call_hierarchy` (depth=2, direction=both) resolved via disambiguated symbol ID `src/V12_002.UI.Compliance.cs::V12_002.ProcessQueuedExecution_HandleFleetOCO#method`:
- **Callers (depth=1):** `ProcessQueuedExecution` (line 787, same file) — ast_resolved
- **Callers (depth=2):** `ProcessAccountExecutionQueue` (line 427, same file) — ast_resolved
- **Callees (depth=1):** `IsFleetAccount`, `HandleFleetStopFill` (line 519), `HandleFleetTargetFill` (line 624), `LogBuffer.Format`
- **Callees (depth=2):** `CancelOrphanedTargets`, `ExtractEntryKeyFromStopName`, `FinalizeStopFilledPosition`, `ApplyTargetFill`, `CancelOrderOnAccount`, `LogBuffer.ValidateThreadAffinity`, `LogBuffer.FormatInternal`, `_nakedPositionFirstSeen`, `activePositions`

Caller count: 1 direct (ProcessQueuedExecution), 2 total (chain). Signature must remain unchanged.

### jcodemunch get_dependency_graph result

`get_dependency_graph` (file=src/V12_002.UI.Compliance.cs, direction=both, depth=1): node_count=1, edge_count=0, imports=[], importers=[]. The file is a C# partial class — import relationships are not tracked via file-level `using` edges in this index. Self-contained node. No cross-file import blast radius from adding private helpers to the same partial class.

### jcodemunch get_extraction_candidates result

`get_extraction_candidates` (file=src/V12_002.UI.Compliance.cs, min_complexity=3, min_callers=1) returned empty candidates list. This is expected: the analyzer requires methods called from multiple distinct callers; `ProcessQueuedExecution_HandleFleetOCO` has 1 direct caller and the private sub-helpers are called from within the class only. Extraction plan is derived directly from source analysis and hotspot report.

---

## Sequential Thinking Summary

sequentialthinking chain (5 thoughts completed):

**Thought 1 — Source Analysis:** Full source confirmed 30 lines (698–727). CYC=15 contributors: try/catch (+1 catch), 4-`&&` compound guard (+3 short-circuit), embedded `||` (+1), `if (StartsWith("Stop_"))` (+1), `else if` (+1) with 2 additional `&&` (+2) = base 1 + 9 branch points = CYC ~10–15 (Lizard/Codacy tool counts each short-circuit operand; hotspot confirms 15). Call hierarchy shows 1 direct caller; signature locked.

**Thought 2 — Extraction 1:** `IsOcoOrderActionable(QueuedAccountExecution item)` encapsulates the 4-predicate null+fleet+state guard into a single bool. Helper CYC = 5 (1 base + 3 `&&` operands + 1 `||` operand) ≤ 8 ✓. Removes ~4 CYC from dispatcher.

**Thought 3 — Extraction 2:** `GetOcoOrderFleetType(string ocoName) → OcoFleetOrderType` enum (`Stop`, `Target`, `Unknown`) encapsulates `StartsWith("Stop_")` + multi-condition `else if` into a classifier. Helper CYC = 5 (1 base + 1 Stop branch + 1 Target else-if + 1 Length guard + 1 char index guard) ≤ 8 ✓. Zero-allocation: returns a value-type enum. Removes ~4 CYC from dispatcher.

**Thought 4 — Extraction 3:** `DispatchOcoFleetOrder(OcoFleetOrderType orderType, QueuedAccountExecution item, Order ocoOrder, Account ocoAcct, string ocoName)` executes the switch dispatch. Helper CYC = 4 (1 base + 1 case Stop + 1 case Target + 1 default) ≤ 8 ✓. Dispatcher after all 3 extractions: CYC = 1(base) + 1(catch) + 1(if IsOcoOrderActionable) = 3 ≤ 8 ✓.

**Thought 5 — Final Verification:** All 4 methods (parent + 3 helpers) project CYC ≤ 5. max_cyc_projected = 5. Jane Street alignment: CYC ≤ 8 ✓, single-responsibility per helper ✓, no lock() blocks ✓, `OcoFleetOrderType` enum makes illegal dispatch states unrepresentable ✓, `GetOcoOrderFleetType` returns value type (zero heap allocation) ✓.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `IsOcoOrderActionable` | `private bool IsOcoOrderActionable(QueuedAccountExecution item)` | Validates that the OCO item has a non-null order, non-null account, is a fleet account, and is in a Filled or PartFilled state. Encapsulates the 4-`&&`+1-`||` compound guard. | 5 |
| `GetOcoOrderFleetType` | `private OcoFleetOrderType GetOcoOrderFleetType(string ocoName)` | Classifies OCO order name as `Stop` (prefix `Stop_`), `Target` (prefix `T{n}_`), or `Unknown`. Returns a value-type enum — zero allocation. Encapsulates `StartsWith("Stop_")` + multi-condition `else if`. | 5 |
| `DispatchOcoFleetOrder` | `private void DispatchOcoFleetOrder(OcoFleetOrderType orderType, QueuedAccountExecution item, Order ocoOrder, Account ocoAcct, string ocoName)` | Performs switch dispatch to `HandleFleetStopFill` or `HandleFleetTargetFill` based on the classified order type. Single-responsibility: dispatch only, no classification logic. | 4 |

### Supporting Enum (nested private)

```csharp
private enum OcoFleetOrderType { Stop, Target, Unknown }
```

Added to the same partial class in `src/V12_002.UI.Compliance.cs`. Makes unclassified order types unrepresentable as a runtime dispatch error.

---

## Parent Method After Extraction

**Remaining logic in `ProcessQueuedExecution_HandleFleetOCO` after extraction:**

```csharp
private void ProcessQueuedExecution_HandleFleetOCO(QueuedAccountExecution item)
{
    try
    {
        if (IsOcoOrderActionable(item))
        {
            Order ocoOrder = item.EventArgs.Execution?.Order;
            Account ocoAcct = item.Account;
            string ocoName = ocoOrder.Name ?? "";
            OcoFleetOrderType orderType = GetOcoOrderFleetType(ocoName);
            DispatchOcoFleetOrder(orderType, item, ocoOrder, ocoAcct, ocoName);
        }
    }
    catch (Exception ex)
    {
        Print(string.Format("[1104.1 OCO] Fleet OCO error: {0}", ex.Message));
    }
}
```

- **Remaining logic:** guard check, local variable extraction, type classification call, dispatch call, error logging
- **Projected CYC:** 3 (1 base + 1 if + 1 catch)

---

## max_cyc_projected: 5
## extraction_count: 3

---

## Callers Preserved

| Caller | File | Line | Impact |
|---|---|---|---|
| `ProcessQueuedExecution` | `src/V12_002.UI.Compliance.cs` | 787 | No change — method signature unchanged |
| `ProcessAccountExecutionQueue` (indirect) | `src/V12_002.UI.Compliance.cs` | 427 | No change — call chain unaffected |

---

## Jane Street Alignment

| Principle | Status | Evidence |
|---|---|---|
| CYC <= 8 achieved | YES | Parent: 3, Helper 1: 5, Helper 2: 5, Helper 3: 4 |
| Single-responsibility per helper | YES | Each helper has exactly one concern: guard, classify, dispatch |
| Lock-free / Actor pattern preserved | YES | No lock() blocks introduced; all state mutations delegated to existing handlers |
| Illegal states unrepresentable | YES | `OcoFleetOrderType` enum with `Unknown` case; no string-based dispatch |
| Zero-allocation hot paths | YES | `GetOcoOrderFleetType` returns value-type enum; no heap allocation |
| No scope creep (V12.23) | YES | All helpers private to same partial class; no cross-file changes |
| Caller signature unchanged | YES | `ProcessQueuedExecution_HandleFleetOCO(QueuedAccountExecution item)` signature preserved |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 3 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-147 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 3 |
| **max_cyc_projected** | 5 |
