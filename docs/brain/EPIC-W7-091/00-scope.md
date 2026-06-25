# Phase 1: Scope Definition — EPIC-W7-091

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.41
- **Execution Time**: 2026-06-24T08:00:00Z
- **Input**: `00-hotspots.md`

---

## Method Under Refactoring

| Attribute        | Value                                  |
|------------------|----------------------------------------|
| **Method**       | `CancelDirectFallbackOrders`           |
| **File**         | `src/V12_002.Safety.Watchdog.cs`       |
| **Line**         | 268                                    |
| **Signature**    | `private void CancelDirectFallbackOrders(Account masterAccount, string instrumentName)` |
| **Current CYC**  | 10                                     |
| **Target CYC**   | ≤ 8                                    |
| **LOC**          | 28 (lines 268–295)                     |
| **Reduction**    | −2 CYC points                          |

### Method Body (Annotated Decision Points)

```csharp
// Lines 268–295 — CYC decision points labelled [D1]…[D10]
private void CancelDirectFallbackOrders(Account masterAccount, string instrumentName)
{
    List<Order> ordersToCancel = new List<Order>();

    foreach (Order order in masterAccount.Orders.ToArray())   // [D1] loop
    {
        if (order == null || order.Instrument == null)         // [D2] null guard (||)
            continue;
        if (order.Instrument.FullName != instrumentName)       // [D3] instrument filter
            continue;
        if (
            order.OrderState == OrderState.Working             // [D4]
            || order.OrderState == OrderState.Submitted        // [D5]
            || order.OrderState == OrderState.Accepted         // [D6]
            || order.OrderState == OrderState.ChangePending    // [D7]
            || order.OrderState == OrderState.ChangeSubmitted  // [D8]
        )
        {
            ordersToCancel.Add(order);
        }
    }

    if (ordersToCancel.Count > 0)                             // [D9] guard before Cancel
    {
        masterAccount.Cancel(ordersToCancel.ToArray());
        Print("[WATCHDOG] Direct fallback cancelled "          // (not a branch)
            + ordersToCancel.Count + " master order(s).");
    }
}
// [D10] = method entry node (baseline)
```

**CYC breakdown**: 1 (method) + 1 (foreach) + 2 (null-guard `||`) + 1 (instrument filter)
+ 5 (five `||` terms in state predicate) = **10**

---

## IN SCOPE — Planned Extractions

### Extraction 1 — `IsOrderCancellable`

| Attribute        | Value                                        |
|------------------|----------------------------------------------|
| **Kind**         | Private predicate helper                     |
| **Signature**    | `private bool IsOrderCancellable(Order order, string instrumentName)` |
| **Removes**      | [D2] null guard, [D3] instrument filter, [D4]–[D8] state predicate (5 `||` terms) |
| **CYC removed**  | 7 (2 guards + 5 state branches)              |
| **CYC added**    | 7 (same logic lives in new method)           |
| **Net effect on `CancelDirectFallbackOrders`** | −7 CYC (guards replaced by single predicate call) |

**Body sketch:**
```csharp
private bool IsOrderCancellable(Order order, string instrumentName)
{
    if (order == null || order.Instrument == null)
        return false;
    if (order.Instrument.FullName != instrumentName)
        return false;
    return order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Submitted
        || order.OrderState == OrderState.Accepted
        || order.OrderState == OrderState.ChangePending
        || order.OrderState == OrderState.ChangeSubmitted;
}
```

**Residual CYC of `CancelDirectFallbackOrders` after extraction:**

| Decision points remaining        | Count |
|----------------------------------|-------|
| Method entry                     | 1     |
| `foreach` loop                   | 1     |
| `if (ordersToCancel.Count > 0)` | 1     |
| `if (!IsOrderCancellable(…))`   | 1     |
| **Total**                        | **4** |

> CYC = 4, which satisfies the ≤ 8 threshold with **4 points of headroom**.  
> No second extraction is required.

---

## OUT OF SCOPE

| Item                                      | Reason                                          |
|-------------------------------------------|-------------------------------------------------|
| Signature of `CancelDirectFallbackOrders` | Must remain unchanged — callers must not break  |
| Logic / behavior of the method            | Pure structural refactor; no semantic change    |
| `ExecuteWatchdogDirectFallback` (line 244)| Caller; untouched                               |
| `OnWatchdogTimer` (line 36)               | Indirect caller; untouched                      |
| `FlattenDirectFallbackPositions` (line 297)| Sibling method; untouched                      |
| Any other file in `src/`                  | Zero blast radius confirmed; no cross-file edits |
| Test files                                | No new tests are required by this phase         |

---

## Extraction Plan

### Step 1 — Define `IsOrderCancellable`
- Insert new `private bool IsOrderCancellable(Order order, string instrumentName)` method
  immediately after `CancelDirectFallbackOrders` (i.e., before `FlattenDirectFallbackPositions`).
- Copy the three-part guard logic (null check, instrument match, state check) verbatim from
  the loop body into the new helper.

### Step 2 — Simplify the `foreach` body in `CancelDirectFallbackOrders`
- Replace the three existing `if`/`continue` blocks and the state-check `if` block with
  a single inverted-guard call:
  ```csharp
  if (!IsOrderCancellable(order, instrumentName))
      continue;
  ordersToCancel.Add(order);
  ```
- The `if (ordersToCancel.Count > 0)` block and `Print` call are **unchanged**.

### Proposed Helper Method Names

| Helper                  | Purpose                                      | Final CYC |
|-------------------------|----------------------------------------------|-----------|
| `IsOrderCancellable`    | Validates null-safety, instrument match, and active-state membership for a single order | 8 |

> `IsOrderCancellable` CYC = 1 (method) + 1 (null-guard `||`) + 1 (instrument filter)
> + 5 (state `||` chain) = **8** — exactly at the Jane Street limit.

---

## Risk Assessment

| Risk Factor                     | Rating   | Notes                                                  |
|---------------------------------|----------|--------------------------------------------------------|
| Blast radius                    | ZERO     | Confirmed by Phase 0 — no external importers           |
| Caller breakage                 | NONE     | Signature of `CancelDirectFallbackOrders` unchanged    |
| Behavior change                 | NONE     | Logic is moved verbatim; identical execution paths     |
| New method complexity           | LOW      | `IsOrderCancellable` CYC = 8 (at limit, not above)    |
| Naming collision                | MINIMAL  | Name is local to the `V12_002` partial class          |
| Churn risk                      | MINIMAL  | Method is not in top-50 hotspots; rarely modified      |
| Net diff size                   | SMALL    | ~6 lines removed from `CancelDirectFallbackOrders`, ~14 lines added for helper |

**Overall Risk: LOW**

---

## Success Criteria

| # | Criterion                                                                          | Verifiable by          |
|---|------------------------------------------------------------------------------------|------------------------|
| 1 | `CancelDirectFallbackOrders` CYC ≤ 8 after refactor                               | Lizard / jCodemunch    |
| 2 | `IsOrderCancellable` CYC ≤ 8                                                       | Lizard / jCodemunch    |
| 3 | Public signature of `CancelDirectFallbackOrders` is byte-for-byte identical        | `git diff` inspection  |
| 4 | All existing callers (`ExecuteWatchdogDirectFallback`) compile without change       | `dotnet build`         |
| 5 | No other file in `src/` is modified                                                | `git diff --stat`      |
| 6 | Codacy issue `76348462-84b3-409a-90d3-955e90abfb87` resolves (CYC ≤ 8)            | Codacy re-scan         |
| 7 | Behaviour is unchanged: identical order-cancellation semantics for all OrderStates | Code review / diff     |
