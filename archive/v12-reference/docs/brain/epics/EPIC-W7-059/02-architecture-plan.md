# EPIC-W7-059 — Phase 2: Architecture Plan

**Agent Name:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-059/01-scope-boundary.md

---

## Summary

| Field                  | Value                                         |
|------------------------|-----------------------------------------------|
| **Epic**               | EPIC-W7-059                                   |
| **Method**             | `AdoptMasterWorkingOrders`                    |
| **File**               | `src/V12_002.SIMA.Lifecycle.cs`               |
| **CYC Baseline**       | 34 (cluster aggregate)                        |
| **CYC Target**         | <= 8 per method                               |
| **max_cyc_projected**  | **4**                                         |
| **Extractions**        | 2 new private helper methods                  |
| **Risk Level**         | HIGH (26 points over Jane Street threshold 8) |

---

## Method Signature

```csharp
private void AdoptMasterWorkingOrders(ref int adoptedCount)
```

**Location:** `src-vm-backup/V12_002.SIMA.Lifecycle.cs`, line 711–758

**Docstring:** Phase 2: Adopt working orders from master account into tracking dictionaries.
Master account does not use FSM — bracket orders only.

---

## MCP Evidence

### Symbol Resolution
- **Symbol ID:** `src-vm-backup/V12_002.SIMA.Lifecycle.cs::V12_002.AdoptMasterWorkingOrders#method`
- **Resolved via:** `search_symbols` → `get_context_bundle`
- **Source lines:** 711–758 (48 lines)

### Call Hierarchy (depth=2)
**Callers (do NOT modify):**
- `HydrateWorkingOrdersFromBroker` (depth=1) — direct orchestrator
- `EnumerateApexAccounts` (depth=2) — upstream lifecycle caller

**Callees (existing helpers, do NOT modify):**
- `IsOrderStateAdoptable` (line 690) — validates order state for adoption
- `ClassifyMasterOrderByPrefix` (line 768) — routes order to target dict by prefix
- `GetOrderDictionaryByName` (line 795, depth=2) — dict resolver
- `LogBuffer.Format` — logging infrastructure

### Dependency Graph
- `src/V12_002.SIMA.Lifecycle.cs` — no external import edges (self-contained partial class)
- All helpers reside in same file/class; no cross-file coordination needed

### Actual Source (AdoptMasterWorkingOrders)
```csharp
private void AdoptMasterWorkingOrders(ref int adoptedCount)
{
    try
    {
        Account masterBroker996h = Account;
        foreach (Order ord in masterBroker996h.Orders.ToArray())
        {
            if (ord.Instrument?.FullName != Instrument?.FullName)
                continue;
            if (!IsOrderStateAdoptable(ord.OrderState, includeMasterUnknown: true))
                continue;

            string name = ord.Name ?? string.Empty;
            string key, dictName;
            ConcurrentDictionary<string, Order> targetDict = ClassifyMasterOrderByPrefix(
                name, out key, out dictName
            );

            if (targetDict == null || key == null)
                continue;

            targetDict[key] = ord;
            adoptedCount++;
            Print(string.Format(
                "[SIMA HYDRATE] {0} (Master): Adopted {1} -> {2}[{3}]",
                Account.Name, name, dictName, key
            ));
        }
    }
    catch (Exception ex)
    {
        Print(string.Format(
            "[SIMA HYDRATE] WARNING: Could not adopt orders for {0} (Master): {1}",
            Account.Name, ex.Message
        ));
    }
}
```

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Drivers for CYC=34

The CYC=34 is a cluster aggregate from Lizard across the `HydrateWorkingOrdersFromBroker`
logical grouping. The actual `AdoptMasterWorkingOrders` method has these branches:

- `foreach` over orders: +1
- `if` instrument filter (2 continues): +1
- `if` state adoptable guard: +1
- `if` targetDict/key null guard (||): +1
- `catch (Exception)`: +1
- Base: 1

Raw method CYC ≈ 6–7. Supporting helpers (`IsOrderStateAdoptable` with 6 if-returns = CYC 7,
`ClassifyMasterOrderByPrefix` with foreach+if = CYC 3) contribute to the cluster total.
The primary complexity driver inside the foreach body is the multi-step filter → classify →
write → log pipeline executed per order, with multiple early-continue guards and a try/catch
wrapping the entire loop.

### Thought 2 — Extraction Strategy

The foreach body mixes three concerns:
1. **Filtering** — instrument match + adoptable state check (predicate logic)
2. **Processing** — classification + null guard + dict write + counter increment (mutation)
3. **Logging** — Print call with formatted diagnostics (observability)

Extraction plan:
- **`ShouldAdoptMasterOrder(Order ord)`** — absorbs the two guard predicates (instrument
  filter + `IsOrderStateAdoptable` call). Consolidates 2 early-continues into 1 predicate.
- **`ProcessAdoptedMasterOrder(Order ord, ref int adoptedCount)`** — absorbs
  `ClassifyMasterOrderByPrefix` call + null guard + dict write + `adoptedCount++` + `Print`.

Parent `AdoptMasterWorkingOrders` after extraction becomes:
```csharp
try {
    foreach (Order ord in Account.Orders.ToArray()) {
        if (!ShouldAdoptMasterOrder(ord)) continue;
        ProcessAdoptedMasterOrder(ord, ref adoptedCount);
    }
} catch (Exception ex) { PrintMasterAdoptionWarning(ex); }
```

`IsOrderStateAdoptable` and `ClassifyMasterOrderByPrefix` **already exist** as helpers
and are **not extracted** (they are callee dependencies, not complexity sources in parent).

### Thought 3 — CYC Validation

| Method                       | CYC Calculation                                      | CYC Projected |
|------------------------------|------------------------------------------------------|---------------|
| `AdoptMasterWorkingOrders`   | base(1) + foreach(1) + if(!Should)(1) + catch(1)     | **4**         |
| `ShouldAdoptMasterOrder`     | base(1) + if(instrument!=)(1) + if(!adoptable)(1)    | **3**         |
| `ProcessAdoptedMasterOrder`  | base(1) + if(null-guard)(1)                          | **2**         |

All helpers AND parent: CYC <= 8 ✓
max_cyc_projected = **4**

---

## Extraction Plan

| Helper                        | Absorbs                                           | Est. CYC | Visibility |
|-------------------------------|---------------------------------------------------|----------|------------|
| `ShouldAdoptMasterOrder`      | Instrument filter check + IsOrderStateAdoptable call | 3     | `private`  |
| `ProcessAdoptedMasterOrder`   | ClassifyMasterOrderByPrefix + null guard + dict write + adoptedCount++ + Print | 2 | `private` |

**Parent after extraction:**

| Method                       | Retains                                           | Est. CYC |
|------------------------------|---------------------------------------------------|----------|
| `AdoptMasterWorkingOrders`   | foreach loop + single if(!Should) + catch wrapper | 4        |

**Pre-existing helpers (unchanged):**
- `IsOrderStateAdoptable` — called by `ShouldAdoptMasterOrder`; CYC 7 (already <= 8)
- `ClassifyMasterOrderByPrefix` — called by `ProcessAdoptedMasterOrder`; CYC 3

---

## Method Signatures for Extracted Helpers

```csharp
/// <summary>
/// Predicate: returns true if the order should be adopted into master tracking dictionaries.
/// Validates instrument match and order state eligibility.
/// </summary>
private bool ShouldAdoptMasterOrder(Order ord)

/// <summary>
/// Classify, write, and log a single master order into its target tracking dictionary.
/// Increments adoptedCount on successful adoption.
/// </summary>
private void ProcessAdoptedMasterOrder(Order ord, ref int adoptedCount)
```

---

## Jane Street KB Alignment

| Principle            | Source        | Application                                                       |
|----------------------|---------------|-------------------------------------------------------------------|
| Zero-alloc hot path  | carl_cook     | No new allocations; `ref int adoptedCount` avoids boxing          |
| Extract cold logging | carl_cook     | `Print` calls isolated in `ProcessAdoptedMasterOrder` (cold path) |
| No new lock()        | gjengset      | `ConcurrentDictionary` used; no new lock blocks introduced        |
| Single responsibility| trading_billions | `ShouldAdoptMasterOrder` = predicate only; `ProcessAdoptedMasterOrder` = write+log |
| CYC <= 8 per helper  | trading_billions | All projected CYCs: 4, 3, 2 — all below threshold 8             |

---

## V12.23 Scope Compliance

| Check                                | Status  |
|--------------------------------------|---------|
| Single method targeted               | PASS    |
| Callers not modified                 | PASS    |
| No cross-file changes                | PASS    |
| Helpers are same-class private       | PASS    |
| No sibling method modifications      | PASS    |
| Boundary matches 01-scope-boundary   | PASS    |
| max_cyc_projected <= 8               | PASS (4)|

**Callers confirmed untouched:**
- `EnumerateApexAccounts` — upstream caller, signature unchanged
- `ProcessOnConnectionStatusUpdate` — upstream caller, signature unchanged

---

## Agent Tracking

| Field              | Value                        |
|--------------------|------------------------------|
| **Agent Name**     | v12-phase2-architecture      |
| **Bobcoins Used**  | 0.8                          |
| **Execution Time** | batch                        |
| **Phase**          | 2                            |
| **Wave**           | 7                            |
| **MCP Tools Used** | resolve_repo, search_symbols, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_symbol_source |
| **Sequential Thinking Thoughts** | 3 (probe + 3 deep analysis) |
