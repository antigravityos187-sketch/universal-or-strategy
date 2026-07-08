# EPIC-W7-034 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-034/01-scope-boundary.md

---

## Summary

Reduce `ManageCIT` from CYC=11 to CYC=4 by extracting one private helper `ProcessCitOrder`.
The helper absorbs all per-order dispatch logic (isFollower resolution, nudge dispatch, budget
check, exception handling) and reduces to CYC=8. Both units satisfy the Jane Street threshold
of ≤8.

---

## Target Method

| Field              | Value                                          |
|--------------------|------------------------------------------------|
| Method             | `ManageCIT`                                    |
| File               | `src/V12_002.Orders.Management.Flatten.cs`     |
| Lines              | 68–128                                         |
| CYC (baseline)     | 11                                             |
| CYC (target)       | ≤8 for ALL units                               |
| Extractions needed | 1                                              |

---

## Complexity Drivers (MCP Evidence — context_bundle + call_hierarchy)

`ManageCIT` (CYC=11) contains 10 explicit decision points + base:

| # | Branch                                                        | Location      |
|---|---------------------------------------------------------------|---------------|
| 1 | `if (!ValidateCitConfiguration(...))`                         | line ~72      |
| 2 | `foreach (var kvp in entryOrders.ToArray())`                  | line ~78      |
| 3 | `if (!ShouldChaseOrder(order, key))`                          | line ~84      |
| 4 | `pos != null` (&&-operand 1)                                  | line ~89      |
| 5 | `pos.IsFollower` (&&-operand 2)                               | line ~89      |
| 6 | `pos.ExecutingAccount != null` (&&-operand 3)                 | line ~89      |
| 7 | `if (isFollower)`                                             | line ~94      |
| 8 | `if (!ExecuteFollowerNudge(...))`                             | line ~96      |
| 9 | `catch (InvalidOperationException when ...)`                  | line ~118     |
|10 | `catch (Exception)`                                           | line ~123     |

**Primary extraction target:** Branches 4–10 (per-order dispatch block) are a cohesive
unit of logic: "determine follower/local routing, execute the appropriate nudge, handle
errors." This maps cleanly to a single extracted method.

---

## Extraction Plan

### Helper 1: `ProcessCitOrder`

```csharp
/// <summary>
/// Processes a single CIT order entry: resolves follower/local routing,
/// calculates nudge price, dispatches ExecuteFollowerNudge or ExecuteLocalNudge,
/// marks the one-shot nudge guard, and absorbs per-order exceptions.
/// Returns false when the broker budget is exhausted (caller stops iteration).
/// </summary>
private bool ProcessCitOrder(
    string key,
    Order order,
    double citOffset,
    ref int citBrokerBudget
)
```

**Placement:** Same partial class, `src/V12_002.Orders.Management.Flatten.cs`, immediately
after `ManageCIT` closing brace (~line 130).

**Body owns:**
- `activePositions.TryGetValue(key, out PositionInfo pos)` — state read
- `bool isFollower = pos != null && pos.IsFollower && pos.ExecutingAccount != null`
- `double newLimitPrice = CalculateNudgedPrice(order.OrderAction, order.LimitPrice, citOffset)`
- `if (isFollower)` → `ExecuteFollowerNudge(...)` + budget-exhausted return false
- `else` → `ExecuteLocalNudge(...)`
- `_citNudgedKeys.TryAdd(key, true)` — one-shot guard
- `catch (InvalidOperationException ex) when (ex.Message.Contains("ChangeOrder"))`
- `catch (Exception ex)`
- Returns `true` on success, `false` on budget-exhausted (replaces inline `return`)

---

### ManageCIT (post-extraction)

```csharp
private void ManageCIT()
{
    if (!ValidateCitConfiguration(out double citOffset))
        return;

    int _citBrokerBudget = MaxBrokerCallsPerCycle;
    foreach (var kvp in entryOrders.ToArray())
    {
        string key = kvp.Key;
        Order order = kvp.Value;

        if (!ShouldChaseOrder(order, key))
            continue;

        if (!ProcessCitOrder(key, order, citOffset, ref _citBrokerBudget))
            return; // budget exhausted
    }
}
```

**Note:** The budget-exhausted `return` in ManageCIT is a new branch (added by the
refactor at the ManageCIT level). This replaces the `return` that was previously buried
inside the isFollower arm deep in the loop body. Net effect: cleaner control flow,
same semantic behavior.

---

## CYC Projection Table

| Unit              | CYC (before) | CYC (after) | ≤8? | Branch accounting                                                                |
|-------------------|:------------:|:-----------:|:---:|----------------------------------------------------------------------------------|
| `ManageCIT`       | 11           | 4           | ✓   | base(1) + ValidateCitConfiguration(1) + foreach(1) + ShouldChaseOrder(1) = 4   |
| `ProcessCitOrder` | —            | 8           | ✓   | base(1) + pos!=null(1) + IsFollower(1) + ExecutingAcct!=null(1) + isFollower(1) + !ExecuteFollowerNudge(1) + catch-IOE(1) + catch-Ex(1) = 8 |

**max_cyc_projected = 8** ✓

---

## Jane Street KB Alignment

| Principle                         | How applied                                                                                      |
|-----------------------------------|--------------------------------------------------------------------------------------------------|
| **carl_cook: cold path out-of-line** | Exception handling + error logging stays in `ProcessCitOrder`; ManageCIT hot path is clean      |
| **carl_cook: no LINQ**            | No new LINQ introduced; existing `.ToArray()` call retained as-is                               |
| **trading_billions: single responsibility** | ManageCIT = iterate + guard; ProcessCitOrder = dispatch + nudge + error handling        |
| **trading_billions: CYC ≤ 8**    | ManageCIT=4, ProcessCitOrder=8. Both ≤8 ✓                                                       |
| **gjengset: no new lock() blocks** | All state access via existing `ConcurrentDictionary` methods (TryAdd, TryGetValue). Zero locks added. |

---

## MCP Evidence

| Tool                    | Finding                                                                                         |
|-------------------------|-------------------------------------------------------------------------------------------------|
| `resolve_repo`          | Repo indexed: 5147 symbols, `antigravityos187-sketch/universal-or-strategy`                    |
| `get_context_bundle`    | Full source retrieved (lines 68–128). CYC=11 confirmed by branch count.                        |
| `get_call_hierarchy`    | 0 callers in scope. 13 callees: `ValidateCitConfiguration`, `ShouldChaseOrder`, `CalculateNudgedPrice`, `ExecuteFollowerNudge`, `ExecuteLocalNudge`, `_citNudgedKeys`, `entryOrders`, `activePositions`. |
| `get_dependency_graph`  | 0 import edges, 0 importer edges. File is a standalone partial class with no cross-file imports. |
| `search_symbols`        | Symbol ID confirmed: `src/V12_002.Orders.Management.Flatten.cs::V12_002.ManageCIT#method`       |

---

## Sequential Thinking Evidence

| Thought | Conclusion                                                                                          |
|---------|-----------------------------------------------------------------------------------------------------|
| T1      | Identified 10 decision branches. Per-order dispatch block (branches 4–10) is the extraction target. |
| T2      | Named helper `ProcessCitOrder(string key, Order order, double citOffset, ref int citBrokerBudget)`. Signature, placement, and responsibility defined. Jane Street alignment verified. |
| T3      | CYC validated: ManageCIT=4 ≤8 ✓, ProcessCitOrder=8 ≤8 ✓. One extraction sufficient. No lock() blocks. |

---

## Scope Boundary Compliance

- **Boundary verdict from Phase 1.5:** PASS
- **Methods touched:** `ManageCIT` (modified) + `ProcessCitOrder` (new, same file)
- **Caller count:** 0 in scope (call hierarchy: 0 direct callers from indexed files)
- **External signature of `ManageCIT`:** Unchanged (`private void ManageCIT()`)
- **Cross-file impact:** None

---

## Execution Notes for Phase 5 (v12-engineer)

1. In `src/V12_002.Orders.Management.Flatten.cs`, replace the loop body of `ManageCIT`
   with a call to `ProcessCitOrder(key, order, citOffset, ref _citBrokerBudget)`.
2. Add `if (!ProcessCitOrder(...)) return;` in the ManageCIT foreach to preserve
   budget-exhausted early-exit behavior.
3. Insert `ProcessCitOrder` as a new `private bool` method immediately after
   `ManageCIT` closes (~line 130), within the same partial class.
4. Run `dotnet build` — zero errors expected (no interface changes).
5. Run `python scripts/complexity_audit.py` — verify ManageCIT ≤8 and ProcessCitOrder ≤8.
6. Run `dotnet csharpier format src/` — formatting compliance.
7. Run `powershell -File .\deploy-sync.ps1` — NinjaTrader hard-link sync.

---

## Agent Tracking

| Field              | Value                       |
|--------------------|-----------------------------|
| **Agent Name**     | v12-phase2-architecture     |
| **Phase**          | 2                           |
| **Wave**           | 7                           |
| **Epic**           | EPIC-W7-034                 |
| **Bobcoins Used**  | 1.0                         |
| **Execution Time** | batch                       |
| **Output**         | 02-architecture-plan.md     |
