# EPIC-W7-089 — Phase 4: Ticket Generation

**Agent Name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T04:00:00Z
**Input:** docs/brain/EPIC-W7-089/02-architecture-plan.md + docs/brain/EPIC-W7-089/03-audit-report.md

---

## Header

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-089 |
| **Method** | `CancelWatchdogWorkingOrders` |
| **Source File** | `src/V12_002.Safety.Watchdog.cs` |
| **CYC Before** | 10 |
| **Wave** | 7 |
| **Phase** | 4 |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 3 |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 0.8 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **MCP: resolve_repo** | antigravityos187-sketch/universal-or-strategy — loadable, 5147 symbols |
| **MCP: sequential-thinking** | 3 thoughts completed — extraction plan validated |
| **DNA Verdict (Phase 3)** | PASS — 0 violations |

---

## Ticket Summary

| Ticket | Helper Name | Concern | Projected Helper CYC | CYC Reduction |
|---|---|---|---|---|
| T1 | `IsOrderCancelable` | 5-way OrderState OR-chain classifier (hot path) | 5 | -5 from parent predicate block |
| T2 | `CollectCancelableOrders` | ToArray snapshot + null guard + instrument filter + state check | 6 | -7 from parent collect block |
| T3 | `LogWatchdogCancelCount` | Cold-path Print logger + orchestrator wire-up | 1 | moves Print body out-of-line |

**Execution order:** T1 → T2 → T3 (T2 depends on T1's `IsOrderCancelable`; orchestrator wire-up is final step of T3)

---

## Ticket T1 — Extract `IsOrderCancelable`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-089-T1 |
| **helper_name** | `IsOrderCancelable` |
| **concern** | Extract the 5-way `OrderState` OR-chain into a dedicated pure predicate. This is a hot-path classification ticket: the inline boolean compound (`Working \|\| Submitted \|\| Accepted \|\| ChangePending \|\| ChangeSubmitted`) drives 5 of the 10 CYC branches in the parent. Extraction reduces cyclomatic complexity at the call site and enables reuse by `CollectCancelableOrders` in T2. |
| **lines_to_move** | ~5 (the 5-condition OR expression from the inner foreach body) |
| **cyc_reduction** | 5 branches removed from parent (4 OR short-circuits + the enclosing if) |
| **projected_helper_cyc** | 5 |

### Implementation Instructions

1. Add a new `private static bool IsOrderCancelable(Order order)` method in the same partial class (`src/V12_002.Safety.Watchdog.cs`).
2. Decorate with `[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]` — hot-path predicate, zero allocation, per `carl_cook` inlining mandate.
3. Move the 5-condition OR body verbatim:
   ```csharp
   return order.OrderState == OrderState.Working
       || order.OrderState == OrderState.Submitted
       || order.OrderState == OrderState.Accepted
       || order.OrderState == OrderState.ChangePending
       || order.OrderState == OrderState.ChangeSubmitted;
   ```
4. Do NOT change `CancelWatchdogWorkingOrders` in this ticket — that is T2's responsibility. T1 only adds the helper.
5. Verify: build passes, no new lock() blocks, ASCII-only strings.

### Acceptance Criteria

- [ ] `IsOrderCancelable` method exists in `src/V12_002.Safety.Watchdog.cs`
- [ ] `[AggressiveInlining]` attribute present
- [ ] Method is `private static bool`
- [ ] 5-way OR-chain matches original logic exactly
- [ ] Build passes (`dotnet build src/`)
- [ ] CSharpier format check passes (`dotnet csharpier check src/`)

---

## Ticket T2 — Extract `CollectCancelableOrders`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-089-T2 |
| **helper_name** | `CollectCancelableOrders` |
| **concern** | Extract the collect-then-cancel snapshot block into a dedicated list-building helper. This ticket is the primary complexity reduction ticket: it removes the `foreach` over `masterAccount.Orders.ToArray()`, the null guard (`order == null \|\| order.Instrument == null`), the instrument name filter, and the `IsOrderCancelable` call from the parent method body. Preserves the H14-FIX `ToArray()` snapshot pattern (thread-safe enumeration on strategy thread, same as W7-086). |
| **lines_to_move** | ~10 (entire first foreach block including ToArray, null guard, instrument filter, IsOrderCancelable check, list accumulation) |
| **cyc_reduction** | 7 branches removed from parent (foreach + null-guard-if + null-guard-OR + instrument-filter-if + IsOrderCancelable-if + list init collapsed into call) |
| **projected_helper_cyc** | 6 |

### Dependency

**Requires T1 completed** — `CollectCancelableOrders` calls `IsOrderCancelable` internally.

### Implementation Instructions

1. Add `private static List<Order> CollectCancelableOrders(Account masterAccount, string instrumentName)` in same file.
2. Body (preserving H14-FIX ToArray pattern, no LINQ):
   ```csharp
   List<Order> result = new List<Order>();
   foreach (Order order in masterAccount.Orders.ToArray())
   {
       if (order == null || order.Instrument == null)
           continue;
       if (order.Instrument.FullName != instrumentName)
           continue;
       if (IsOrderCancelable(order))
           result.Add(order);
   }
   return result;
   ```
3. Replace the corresponding block in `CancelWatchdogWorkingOrders` with a single call:
   ```csharp
   List<Order> ordersToCancel = CollectCancelableOrders(masterAccount, instrumentName);
   ```
4. No LINQ (no `.Where()`, `.Select()`) — pure foreach per `carl_cook` mandate.
5. Verify: build passes, parent method now has `ordersToCancel` populated via helper call.

### Acceptance Criteria

- [ ] `CollectCancelableOrders` method exists in `src/V12_002.Safety.Watchdog.cs`
- [ ] Method is `private static List<Order>`
- [ ] `ToArray()` snapshot preserved (H14-FIX)
- [ ] No LINQ in helper body
- [ ] `CancelWatchdogWorkingOrders` uses `CollectCancelableOrders(...)` call
- [ ] Build passes
- [ ] CSharpier format check passes

---

## Ticket T3 — Extract `LogWatchdogCancelCount` + Orchestrator Wire-Up

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-089-T3 |
| **helper_name** | `LogWatchdogCancelCount` |
| **concern** | Extract the cold-path `Print` logging call into a dedicated out-of-line helper per `carl_cook` cold-logging mandate, then wire the final orchestrator skeleton using all 3 helpers. This ticket completes the extraction: after T3, `CancelWatchdogWorkingOrders` is a clean orchestrator at CYC=3. |
| **lines_to_move** | ~2 (the `Print(...)` call from the conditional log block at the bottom of the original method) |
| **cyc_reduction** | Moves Print body out-of-line; orchestrator retains the `if (ordersToCancel.Count > 0)` guard (CYC stays at 3 after T2 reduction) |
| **projected_helper_cyc** | 1 |

### Implementation Instructions

1. Add `private void LogWatchdogCancelCount(int count)` in same file.
2. Decorate with `[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]` — cold path, out-of-line per `carl_cook` mandate.
3. Body:
   ```csharp
   Print("[WATCHDOG] Cancelled " + count + " master order(s) on strategy thread.");
   ```
4. In `CancelWatchdogWorkingOrders`, replace the conditional Print block with:
   ```csharp
   if (ordersToCancel.Count > 0)
       LogWatchdogCancelCount(ordersToCancel.Count);
   ```
5. Final orchestrator skeleton after all 3 extractions:
   ```csharp
   private void CancelWatchdogWorkingOrders(Account masterAccount, string instrumentName)
   {
       List<Order> ordersToCancel = CollectCancelableOrders(masterAccount, instrumentName);
       foreach (Order orderToCancel in ordersToCancel)
           CancelOrderOnAccount(orderToCancel, masterAccount);
       if (ordersToCancel.Count > 0)
           LogWatchdogCancelCount(ordersToCancel.Count);
   }
   ```
6. Verify orchestrator CYC = 3 (base + foreach + if).
7. ASCII-only string literal in `LogWatchdogCancelCount` — no Unicode, no curly quotes.

### Acceptance Criteria

- [ ] `LogWatchdogCancelCount` method exists in `src/V12_002.Safety.Watchdog.cs`
- [ ] `[NoInlining]` attribute present
- [ ] Method is `private void`
- [ ] String literal is ASCII-only
- [ ] `CancelWatchdogWorkingOrders` orchestrator matches final skeleton (CYC=3)
- [ ] Build passes
- [ ] CSharpier format check passes
- [ ] `powershell -File .\deploy-sync.ps1` executed to re-synchronize NinjaTrader hard links

---

## CYC Projection After All Tickets

| Symbol | Before | After | Delta |
|---|---|---|---|
| `CancelWatchdogWorkingOrders` | 10 | **3** | -7 |
| `IsOrderCancelable` (new) | — | **5** | new |
| `CollectCancelableOrders` (new) | — | **6** | new |
| `LogWatchdogCancelCount` (new) | — | **1** | new |
| **max_cyc_projected** | 10 | **6** | ✅ ≤ 8 mandate |

**projected_parent_cyc_after_all = 3**

---

## Jane Street Compliance Summary

| Rule | Applied In | Status |
|---|---|---|
| `[AggressiveInlining]` hot-path predicate | T1 | ✅ |
| `[NoInlining]` cold-path logger | T3 | ✅ |
| Zero LINQ — foreach only | T2 | ✅ |
| H14-FIX ToArray snapshot preserved | T2 | ✅ |
| ASCII-only string literals | T3 | ✅ |
| No lock() blocks | All | ✅ |
| Single responsibility per helper | T1/T2/T3 | ✅ |
| CYC ≤ 8 per helper | All | ✅ (max=6) |
| Same-file private helpers | All | ✅ |
| V12.23 No Scope Creep | All | ✅ |
