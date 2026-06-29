# Phase 2: Architecture Plan -- EPIC-W7-047

## Method Under Extraction

- **Method:** `CancelOrphanedTargets`
- **Source File:** `src/V12_002.UI.Compliance.cs`
- **Lines:** 553-578
- **Original CYC:** 13
- **Target CYC:** <= 8 (Jane Street mandatory)

### Source (confirmed from jcodemunch get_symbol_source)

```csharp
private int CancelOrphanedTargets(Account account)
{
    int cancelledTargets = 0;
    foreach (Order o in account.Orders.ToArray())
    {
        if (o == null || o.Instrument?.FullName != Instrument?.FullName)
            continue;
        if (o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted)
            continue;
        if (
            o.Name != null
            && (
                o.Name.StartsWith("T1_")
                || o.Name.StartsWith("T2_")
                || o.Name.StartsWith("T3_")
                || o.Name.StartsWith("T4_")
                || o.Name.StartsWith("T5_")
            )
        )
        {
            CancelOrderOnAccount(o, account);
            cancelledTargets++;
        }
    }
    return cancelledTargets;
}
```

### jcodemunch get_context_bundle result

Symbol `CancelOrphanedTargets` not found via bundle lookup (no imports to bundle);
retrieved via `search_symbols` + `get_symbol_source`.
Signature: `private int CancelOrphanedTargets(Account account)`
Docstring confirms purpose: cancel all working T1-T5 target orders for fleet account on stop fill.

### jcodemunch get_call_hierarchy result

- **Callers (depth 2):**
  - `HandleFleetStopFill` (line 519, same file) -- depth 1 direct caller
  - `ProcessQueuedExecution_HandleFleetOCO` (line 698, same file) -- depth 2 ancestor
- **Callees (depth 1):**
  - `CancelOrderOnAccount` (`src/V12_002.Orders.CancelGateway.cs`, line 46) -- live cancel dispatch
  - `IsOrderTerminal` (`src/V12_002.Orders.Management.Flatten.cs`) -- inferred at depth 2

Call chain confirmed: `OnAccountExecutionUpdate -> ProcessAccountExecutionQueue -> ProcessQueuedExecution_HandleFleetOCO -> HandleFleetStopFill -> CancelOrphanedTargets -> CancelOrderOnAccount`

### jcodemunch get_dependency_graph result

`src/V12_002.UI.Compliance.cs` has **0 indexed import edges** (node_count=1, edge_count=0).
The file is self-contained at the import-graph level; no cross-file dependency changes needed.

### jcodemunch get_extraction_candidates result

No candidates returned (min_complexity=3, min_callers=1). This is expected: the index lacks
per-method complexity data for this file at scan time. The extraction plan is driven by manual
CYC analysis of the confirmed source above.

---

## Sequential Thinking Summary

**5-thought chain completed.** Final verdict (Thought 5):

Two helper extractions are required to bring CancelOrphanedTargets from CYC 13 to CYC 3:

1. **`IsTargetOrderPrefix(string name)`** (CYC 6) -- isolates the 5-arm T1_..T5_ `||` chain.
   Extracting this alone removes 4 CYC points from the inner predicate block.

2. **`IsOrphanedTarget(Order o)`** (CYC 7) -- composes null guard, instrument match, state gate,
   and a call to `IsTargetOrderPrefix`. Without this second extraction, the combined predicate
   in the loop body still yields CYC ~11 (exceeds limit). With it, the loop body becomes a
   single `if (!IsOrphanedTarget(o)) continue;`, reducing the parent to CYC 3.

Max CYC across all methods post-extraction: **7** (well within the <= 8 ceiling).
Jane Street alignment: **PASS** on all axes.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `IsTargetOrderPrefix` | `private bool IsTargetOrderPrefix(string name)` | Returns true if `name` starts with T1_ through T5_; encapsulates the 5-arm `||` prefix filter | 6 |
| `IsOrphanedTarget` | `private bool IsOrphanedTarget(Order o)` | Full order qualification predicate: null guard, instrument match, state gate (Working/Accepted), and prefix test via `IsTargetOrderPrefix` | 7 |

### Helper Method Bodies (reference)

**IsTargetOrderPrefix:**
```csharp
private bool IsTargetOrderPrefix(string name)
{
    return name != null
        && (
            name.StartsWith("T1_")
            || name.StartsWith("T2_")
            || name.StartsWith("T3_")
            || name.StartsWith("T4_")
            || name.StartsWith("T5_")
        );
}
```

**IsOrphanedTarget:**
```csharp
private bool IsOrphanedTarget(Order o)
{
    if (o == null || o.Instrument?.FullName != Instrument?.FullName)
        return false;
    if (o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted)
        return false;
    return IsTargetOrderPrefix(o.Name);
}
```

---

## Parent Method After Extraction

```csharp
private int CancelOrphanedTargets(Account account)
{
    int cancelledTargets = 0;
    foreach (Order o in account.Orders.ToArray())
    {
        if (!IsOrphanedTarget(o))
            continue;
        CancelOrderOnAccount(o, account);
        cancelledTargets++;
    }
    return cancelledTargets;
}
```

- **Remaining logic:** foreach loop over `.ToArray()` snapshot, single predicate dispatch via
  `IsOrphanedTarget`, cancel submission via `CancelOrderOnAccount`, and counter increment.
- **Projected CYC:** 3 (base=1 + foreach=1 + if=1)

---

## max_cyc_projected: 7
## extraction_count: 2

---

## Test Plan (xUnit [Fact])

| Test Name | Covers |
|---|---|
| `IsTargetOrderPrefix_ReturnsTrue_ForT1ThroughT5Prefixes` | All 5 valid prefixes return true |
| `IsTargetOrderPrefix_ReturnsFalse_ForNullOrOtherPrefixes` | null, "T6_", "TP_", "" return false |
| `IsOrphanedTarget_ReturnsFalse_WhenOrderIsNull` | null order guard |
| `IsOrphanedTarget_ReturnsFalse_WhenInstrumentMismatch` | different instrument FullName |
| `IsOrphanedTarget_ReturnsFalse_WhenOrderStateIsNotActive` | Cancelled/Filled states |
| `IsOrphanedTarget_ReturnsTrue_WhenAllConditionsMet` | Working state + T1_ prefix + instrument match |

---

## Jane Street Alignment

- **CYC<=8 achieved:** YES (parent=3, IsTargetOrderPrefix=6, IsOrphanedTarget=7; max=7)
- **Single-responsibility per helper:** YES
  - `IsTargetOrderPrefix` owns only the prefix enumeration concern
  - `IsOrphanedTarget` owns only the full order qualification predicate
  - `CancelOrphanedTargets` owns only the dispatch/count loop
- **Lock-free/Actor pattern preserved:** YES -- `.ToArray()` snapshot pattern preserved exactly;
  no lock blocks introduced; cancel submission delegates to existing `CancelOrderOnAccount`
- **Illegal states unrepresentable:** YES -- future T6 extension requires exactly one `||` line
  in `IsTargetOrderPrefix`; no other site needs changing. The predicate isolation makes
  the filter contract explicit and extensible without silent omission risk.
- **ASCII-only strings:** YES -- T1_..T5_ literals are all ASCII
- **No scope creep (V12.23):** YES -- both helpers are private to same class/partial class;
  caller `HandleFleetStopFill` and downstream `CancelOrderOnAccount` are untouched

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-047 |
| **Wave** | 7 |
| **Phase** | 2 -- Architecture Planning |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **jcodemunch tools called** | resolve_repo, search_symbols, get_symbol_source, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **max_cyc_projected** | 7 |
| **extraction_count** | 2 |
