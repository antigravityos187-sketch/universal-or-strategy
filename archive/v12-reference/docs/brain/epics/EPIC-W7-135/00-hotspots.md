# EPIC-W7-135 — Phase 0: Hotspot Analysis

## Method Signature

| Field       | Value                                         |
|-------------|-----------------------------------------------|
| Method Name | `FindTargetOrderForPosition`                  |
| CYC Score   | **10**                                        |
| File Path   | `src/V12_002.Trailing.Breakeven.cs`           |
| Lines       | 186 – 222                                     |
| Visibility  | `private`                                     |
| Return Type | `Order`                                       |

---

## Blast Radius Summary

`FindTargetOrderForPosition` is a **helper with a single direct caller**:

| Caller | File | Call Site |
|--------|------|-----------|
| `MoveSpecificTarget` | `src/V12_002.Trailing.Breakeven.cs` | Line 356 |

`MoveSpecificTarget` itself is called by the IPC command dispatcher:

| Caller | File | Call Site |
|--------|------|-----------|
| IPC command handler (fleet `MOVE_TARGET`) | `src/V12_002.UI.IPC.Commands.Fleet.cs` | Line 687 |

**Blast scope: low.** Changes to `FindTargetOrderForPosition` affect exactly one direct call site
(`MoveSpecificTarget`, line 356). The IPC layer is one hop further and is unchanged.
A structurally parallel method, `FindTargetOrderForAbsoluteMove` (lines 438–462), uses the
same `foreach`/compound-`&&` pattern and may warrant coordinated cleanup, but is **not** in
scope for this epic.

---

## Top 3 Complexity Drivers

### Driver 1 — Compound boolean guard in `foreach` loop body (lines 208–213)

```csharp
foreach (Order order in searchAcct.Orders)
{
    if (
        order != null
        && order.Name == targetOrderName
        && order.Instrument.FullName == Instrument.FullName
        && (order.OrderState == OrderState.Working || order.OrderState == OrderState.Accepted)
    )
```

**Contribution: +5 CYC** (1 for `foreach`; 3 for three short-circuit `&&` operators; 1 for the
`||` inside the `OrderState` sub-expression). This single compound predicate carries half the
method's total complexity. It can be extracted to a private helper
`IsMatchingWorkingOrder(Order, string)` returning `bool`.

---

### Driver 2 — Ternary with short-circuit condition for account selection (line 204)

```csharp
var searchAcct = (pos.IsFollower && pos.ExecutingAccount != null)
    ? pos.ExecutingAccount
    : Account;
```

**Contribution: +2 CYC** (1 for the ternary branch; 1 for the short-circuit `&&`). The account
resolution logic is duplicated almost verbatim in `FindTargetOrderForAbsoluteMove` (line 446).
Extracting a helper `ResolveSearchAccount(PositionInfo pos)` eliminates both occurrences and
removes the repeated branch.

---

### Driver 3 — Early-exit guard on entry fill state (lines 195–199)

```csharp
if (!pos.EntryFilled)
{
    notFoundReason = $"[V14] MoveSpecificTarget T{targetNum}: Skipping {entryName} - entry not filled";
    return null;
}
```

**Contribution: +1 CYC.** Minor individually, but it is structurally distinct from the order-
search logic. The caller (`MoveSpecificTarget`) already owns a similar `notFoundReason` check
pattern; moving this guard into the caller loop (before the `FindTargetOrderForPosition` call)
would shrink the helper's responsibility to pure order lookup, reducing its CYC to ≤ 4.

---

## Recommended Extraction Plan

| # | Extraction | Target Name | CYC Saved |
|---|-----------|-------------|-----------|
| 1 | Compound order-match predicate | `IsMatchingWorkingOrder(Order, string)` | 5 |
| 2 | Account resolution ternary | `ResolveSearchAccount(PositionInfo)` | 2 |
| 3 | Move entry-fill guard to caller | (inline at call site in `MoveSpecificTarget`) | 1 |

**Recommended extraction count: 2 new helpers + 1 guard relocation = 3 operations.**

Post-refactor target CYC for `FindTargetOrderForPosition`: **≤ 3**
(1 base + 1 `foreach` + 1 `if` for the extracted predicate result).

---

## Agent Tracking

| Field            | Value                          |
|------------------|--------------------------------|
| Agent Name       | v12-phase0-hotspot             |
| Bobcoins Used    | 8                              |
| Execution Time   | ~55s                           |
| Wave             | 7                              |
| Phase            | 0 — Hotspot Analysis           |
| Status           | completed                      |
| Output Artifact  | `docs/brain/EPIC-W7-135/00-hotspots.md` |
