# EPIC-W7-135 — Phase 2: Architecture Plan

## Agent Tracking

| Field            | Value                                               |
|------------------|-----------------------------------------------------|
| **Agent Name**   | v12-phase2-architecture                             |
| **Wave**         | 7                                                   |
| **Phase**        | 2 — Architecture Planning                           |
| **Generated**    | 2026-06-29                                          |
| **Input**        | docs/brain/EPIC-W7-135/01-scope-boundary.md        |
| **Output**       | docs/brain/EPIC-W7-135/02-architecture-plan.md     |
| **Status**       | completed                                           |

---

## MCP Evidence Summary

| Tool                   | Call                                                                                           | Key Finding                                              |
|------------------------|-----------------------------------------------------------------------------------------------|----------------------------------------------------------|
| **jcodemunch**         | `get_context_bundle` (src/V12_002.Trailing.Breakeven.cs::V12_002.FindTargetOrderForPosition)  | Full method source confirmed: CYC = 10                   |
| **jcodemunch**         | `get_call_hierarchy` depth=2                                                                   | 1 direct caller: `MoveSpecificTarget` (line 335), 0 callees |
| **jcodemunch**         | `get_dependency_graph` src/V12_002.Trailing.Breakeven.cs                                      | Zero cross-file import edges — self-contained refactor   |

**Sequential Thinking**: Three `sequentialthinking` thoughts were executed:
1. CYC validation from source (confirmed = 10)
2. Extraction strategy selection (Strategy B: 2 helpers, no caller modification)
3. CYC projection validation and Jane Street compliance check

---

## Target Method

| Field        | Value                                |
|--------------|--------------------------------------|
| Method       | `FindTargetOrderForPosition`         |
| File         | `src/V12_002.Trailing.Breakeven.cs`  |
| Lines        | 186 – 222                            |
| Visibility   | `private`                            |
| Return Type  | `Order`                              |
| CYC Before   | **10**                               |
| CYC Target   | **<= 8**                             |
| Threshold    | 8 (Jane Street strict)               |

---

## CYC Decomposition (Pre-Refactor)

```
Method: FindTargetOrderForPosition — CYC 10

  +1  (base)
  +1  if (!pos.EntryFilled)              — Driver 3: entry-fill guard
  +1  ternary (pos.IsFollower)           — Driver 2: account selection
  +1  && (pos.ExecutingAccount != null)  — Driver 2: account selection
  +1  foreach (searchAcct.Orders)        — loop
  +1  if (outer compound predicate)      — Driver 1: order match guard
  +1  && order.Name == targetOrderName   — Driver 1
  +1  && order.Instrument.FullName == .. — Driver 1
  +1  && order.OrderState == Working     — Driver 1
  +1  || order.OrderState == Accepted    — Driver 1
  ─────────────────────────────────────
  = 10 total
```

---

## Extraction Plan

| # | Helper Name              | Responsibility                                                              | Signature                                                                                              | Estimated CYC |
|---|--------------------------|-----------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------|---------------|
| 1 | `IsMatchingWorkingOrder` | Tests whether an order matches the target name, instrument, and active state | `private bool IsMatchingWorkingOrder(Order order, string targetOrderName, string instrumentFullName)` | 6             |
| 2 | `ResolveSearchAccount`   | Selects the correct account to search (follower vs master)                  | `private Account ResolveSearchAccount(PositionInfo pos)`                                               | 3             |

**max_cyc_projected: 6**

---

## Post-Refactor CYC Projections

### `FindTargetOrderForPosition` (refactored parent)

```
+1  (base)
+1  if (!pos.EntryFilled)                          — retained (Driver 3, in-scope guard)
+1  foreach (ResolveSearchAccount(pos).Orders)     — loop
+1  if (IsMatchingWorkingOrder(order, ...) == true) — delegated to helper
─────────────────────────────────────────────────
= 4  (well within <= 8)
```

### `IsMatchingWorkingOrder` (new helper)

```csharp
private bool IsMatchingWorkingOrder(
    Order order,
    string targetOrderName,
    string instrumentFullName)
{
    return order != null
        && order.Name == targetOrderName
        && order.Instrument.FullName == instrumentFullName
        && (order.OrderState == OrderState.Working
            || order.OrderState == OrderState.Accepted);
}
```

```
+1  (base)
+1  && order != null
+1  && order.Name
+1  && order.Instrument.FullName
+1  && order.OrderState == Working
+1  || order.OrderState == Accepted
─────────────────────────────────
= 6  (<= 8) ✓
```

### `ResolveSearchAccount` (new helper)

```csharp
private Account ResolveSearchAccount(PositionInfo pos)
{
    return (pos.IsFollower && pos.ExecutingAccount != null)
        ? pos.ExecutingAccount
        : Account;
}
```

```
+1  (base)
+1  ternary
+1  &&
────────
= 3  (<= 8) ✓
```

---

## CYC Summary Table

| Symbol                       | CYC Before | CYC After | Delta | Passes <= 8? |
|------------------------------|------------|-----------|-------|--------------|
| `FindTargetOrderForPosition` | 10         | 4         | -6    | YES ✓        |
| `IsMatchingWorkingOrder`     | N/A (new)  | 6         | +6    | YES ✓        |
| `ResolveSearchAccount`       | N/A (new)  | 3         | +3    | YES ✓        |

**max_cyc_projected: 6**

---

## Implementation Notes for Phase 5

1. **Refactor `FindTargetOrderForPosition`** (lines 186–222):
   - Replace inline ternary (line 204) with call to `ResolveSearchAccount(pos)`
   - Replace compound `if` guard (lines 208–213) with call to `IsMatchingWorkingOrder(order, targetOrderName, Instrument.FullName)`
   - Retain `if (!pos.EntryFilled)` early-exit guard — scope boundary prohibits moving it to caller

2. **Add `IsMatchingWorkingOrder`** — private method in same partial class (`V12_002`):
   - Takes `Order order`, `string targetOrderName`, `string instrumentFullName`
   - Pure predicate — no state mutation, no allocations

3. **Add `ResolveSearchAccount`** — private method in same partial class (`V12_002`):
   - Takes `PositionInfo pos`
   - Returns `Account` — pure function, no side effects

4. **Placement**: Both helpers should be placed immediately below `FindTargetOrderForPosition` in the file to maintain locality.

5. **No caller changes**: `MoveSpecificTarget` (line 335) is not touched — V12.23 compliant.

---

## Scope Boundary Compliance

| Check                             | Status |
|-----------------------------------|--------|
| Single method targeted            | PASS   |
| Helpers extracted from subject    | PASS   |
| No caller modifications           | PASS   |
| No sibling method modifications   | PASS   |
| No cross-file refactoring         | PASS   |
| All helpers CYC <= 8              | PASS   |
| Parent CYC <= 8 after refactor    | PASS   |

---

## Jane Street Compliance Notes

| Principle       | Application                                                                                                   |
|-----------------|---------------------------------------------------------------------------------------------------------------|
| **carl_cook**   | Both helpers are zero-alloc: no LINQ, no boxing, no allocations. `ResolveSearchAccount` is an `[AggressiveInlining]` candidate (CYC 3, hot path). `IsMatchingWorkingOrder` is a pure boolean predicate with no heap pressure. |
| **gjengset**    | Zero new `lock()` blocks. Both helpers are pure reads — no state mutation, no synchronization required. `Order` and `Account` are read-only in this context. |
| **trading_billions** | Single responsibility enforced: `IsMatchingWorkingOrder` tests order eligibility only; `ResolveSearchAccount` resolves account only. Parent `FindTargetOrderForPosition` orchestrates the search only. CYC <= 8 for all. |

---

## Dependency Graph (jcodemunch get_dependency_graph)

- `src/V12_002.Trailing.Breakeven.cs` has **0 cross-file import edges** in the dependency graph.
- All extracted helpers remain in the same file — zero blast radius beyond the single source file.
- `MoveSpecificTarget` (sole direct caller via `get_call_hierarchy`) is unchanged.

---

## Risk Assessment

| Risk                             | Likelihood | Mitigation                                    |
|----------------------------------|------------|-----------------------------------------------|
| Caller behavior change           | None       | Method signature unchanged; guards retained   |
| NinjaTrader `Order` null-safety  | Low        | `order != null` check is first clause in helper |
| `Instrument.FullName` pass-by    | Low        | Pass `Instrument.FullName` string at call site, not `Instrument` object |
| Build failure                    | Very low   | Both helpers are private, same class; no interface changes |
