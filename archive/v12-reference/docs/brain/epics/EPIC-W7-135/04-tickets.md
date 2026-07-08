# EPIC-W7-135 — Phase 4: Implementation Tickets

## Agent Tracking

| Field             | Value                                             |
|-------------------|---------------------------------------------------|
| **Agent Name**    | v12-phase4-tickets                                |
| **Wave**          | 7                                                 |
| **Phase**         | 4 — Ticket Generation                             |
| **Generated**     | 2026-06-29                                        |
| **Input**         | docs/brain/EPIC-W7-135/02-architecture-plan.md   |
| **DNA Verdict**   | PASS (docs/brain/EPIC-W7-135/03-audit-report.md) |
| **Output**        | docs/brain/EPIC-W7-135/04-tickets.md             |
| **Status**        | completed                                         |
| **Bobcoins Used** | 0.5                                               |

---

## MCP Evidence

| Tool                       | Result                                                                          |
|----------------------------|---------------------------------------------------------------------------------|
| `resolve_repo`             | repo=antigravityos187-sketch/universal-or-strategy, indexed=true, symbols=5147  |
| `get_symbol_complexity`    | Symbol lookup attempted; not found in index (partial-class indexing gap)        |
| `get_extraction_candidates`| candidates=[] (index gap — CYC=10 confirmed by Phase 2 get_context_bundle)     |
| `sequentialthinking`       | 4 thoughts: input validation, extraction strategy, acceptance criteria, final check |

**Authoritative CYC source**: Phase 2 `get_context_bundle` confirmed CYC=10 for `FindTargetOrderForPosition`. Index gap on partial class does not invalidate the extraction plan.

---

## Ticket Summary

| Ticket | Type       | Helper                   | CYC Removed from Parent | Projected Helper CYC | CYC Target |
|--------|------------|--------------------------|-------------------------|----------------------|------------|
| T1     | extraction | `IsMatchingWorkingOrder` | 6                       | 6                    | <= 8       |
| T2     | extraction | `ResolveSearchAccount`   | 3                       | 3                    | <= 8       |

**ticket_count: 2**
**Parent `FindTargetOrderForPosition` CYC after all extractions: 4** (well within <= 8 mandate)
**max_cyc_projected: 6**

---

## Ticket T1 — Extract `IsMatchingWorkingOrder`

| Field               | Value                                                                                   |
|---------------------|-----------------------------------------------------------------------------------------|
| **ticket_id**       | T1                                                                                      |
| **type**            | extraction                                                                              |
| **file**            | `src/V12_002.Trailing.Breakeven.cs`                                                     |
| **cyc_before**      | 10 (parent, inline predicate contributes 6 branches)                                   |
| **cyc_target**      | helper <= 8 (projected 6); parent after extraction: 4                                  |
| **helper_name**     | `IsMatchingWorkingOrder`                                                                |
| **visibility**      | `private bool`                                                                          |
| **signature**       | `private bool IsMatchingWorkingOrder(Order order, string targetOrderName, string instrumentFullName)` |
| **placement**       | Immediately below `FindTargetOrderForPosition` in same partial class (`V12_002`)        |
| **jane_street**     | carl_cook (zero-alloc pure predicate), gjengset (no lock, pure read), trading_billions |

### What to Extract

Move the compound `if` predicate from the `foreach` body in `FindTargetOrderForPosition` (lines 208–213):

```csharp
// BEFORE (inline inside foreach body):
if (order != null
    && order.Name == targetOrderName
    && order.Instrument.FullName == Instrument.FullName
    && (order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Accepted))
{
    return order;
}
```

### New Helper Body

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

### Call Site Change (inside `FindTargetOrderForPosition` foreach loop)

```csharp
// AFTER:
if (IsMatchingWorkingOrder(order, targetOrderName, Instrument.FullName))
{
    return order;
}
```

Note: Pass `Instrument.FullName` as a `string` at the call site — do NOT pass the `Instrument` object (null-ref safety).

### Acceptance Criteria

- [ ] New `private bool IsMatchingWorkingOrder(Order order, string targetOrderName, string instrumentFullName)` method exists in `src/V12_002.Trailing.Breakeven.cs`
- [ ] `order != null` is the first clause (null guard preserved)
- [ ] Predicate tests `order.Name == targetOrderName`, `order.Instrument.FullName == instrumentFullName`, and `(OrderState.Working || OrderState.Accepted)`
- [ ] CYC target for helper: **<= 8** (projected: 6)
- [ ] Zero heap allocations — no LINQ, no boxing, no new objects
- [ ] No `lock()` blocks introduced
- [ ] Parent `foreach` body calls `IsMatchingWorkingOrder(order, targetOrderName, Instrument.FullName)` (string, not Instrument object)
- [ ] `MoveSpecificTarget` caller (line 335) is NOT modified
- [ ] Build passes with zero errors and zero new warnings

---

## Ticket T2 — Extract `ResolveSearchAccount`

| Field               | Value                                                                        |
|---------------------|------------------------------------------------------------------------------|
| **ticket_id**       | T2                                                                           |
| **type**            | extraction                                                                   |
| **file**            | `src/V12_002.Trailing.Breakeven.cs`                                          |
| **cyc_before**      | 10 (parent, ternary account selection contributes 3 branches)               |
| **cyc_target**      | helper <= 8 (projected 3); parent retains CYC=4 after both extractions      |
| **helper_name**     | `ResolveSearchAccount`                                                       |
| **visibility**      | `private Account`                                                            |
| **signature**       | `private Account ResolveSearchAccount(PositionInfo pos)`                    |
| **placement**       | Immediately below `IsMatchingWorkingOrder` in same partial class (`V12_002`) |
| **jane_street**     | carl_cook ([AggressiveInlining] — CYC 3, hot-path candidate), gjengset (pure read, no lock), trading_billions |

### What to Extract

Move the account-selection ternary from `FindTargetOrderForPosition` (line ~204):

```csharp
// BEFORE (inline in parent):
var searchAcct = (pos.IsFollower && pos.ExecutingAccount != null)
    ? pos.ExecutingAccount
    : Account;
```

### New Helper Body

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private Account ResolveSearchAccount(PositionInfo pos)
{
    return (pos.IsFollower && pos.ExecutingAccount != null)
        ? pos.ExecutingAccount
        : Account;
}
```

### Call Site Change (inside `FindTargetOrderForPosition`)

```csharp
// AFTER:
foreach (var order in ResolveSearchAccount(pos).Orders)
```

### Acceptance Criteria

- [ ] New `private Account ResolveSearchAccount(PositionInfo pos)` method exists in `src/V12_002.Trailing.Breakeven.cs`
- [ ] `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute applied above the method
- [ ] Returns `pos.ExecutingAccount` when `pos.IsFollower && pos.ExecutingAccount != null`, otherwise returns `Account`
- [ ] CYC target for helper: **<= 8** (projected: 3)
- [ ] Pure function — no state mutation, no side effects, no `lock()` blocks
- [ ] Parent `foreach` uses `ResolveSearchAccount(pos).Orders` directly
- [ ] `MoveSpecificTarget` caller (line 335) is NOT modified
- [ ] Build passes with zero errors and zero new warnings

---

## Parent Method After All Extractions

```
FindTargetOrderForPosition — CYC after extraction = 4
  +1  (base)
  +1  if (!pos.EntryFilled)                          — retained early-exit guard (scope boundary)
  +1  foreach (ResolveSearchAccount(pos).Orders)     — loop
  +1  if (IsMatchingWorkingOrder(order, ...))        — delegated match
```

**cyc_after_extraction: 4** — passes Jane Street strict CYC <= 8 mandate.

---

## Scope Boundary Constraints

| Constraint                                    | Status  |
|-----------------------------------------------|---------|
| `if (!pos.EntryFilled)` guard stays in parent | LOCKED  |
| `MoveSpecificTarget` (line 335) unchanged     | LOCKED  |
| No cross-file changes                         | LOCKED  |
| No sibling method modifications               | LOCKED  |
| Both helpers in same partial class/file       | REQUIRED|
| Pass `Instrument.FullName` string (not object)| REQUIRED|

---

## Execution Order

T2 MUST be applied before T1 (or concurrently), because T2 changes the `var searchAcct` line that T1's `foreach` body depends upon. Recommended order: **T2 first, then T1**.
