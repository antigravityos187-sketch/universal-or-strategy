# Phase 2: Architecture Plan — EPIC-W7-056

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-056/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `SweepBrokerOrders`
- **Source File:** `src/V12_002.SIMA.Lifecycle.cs`
- **Lines:** 1360–1454
- **Visibility:** `private int`
- **Original CYC:** 28

### jcodemunch get_context_bundle result

Symbol resolved: `src/V12_002.SIMA.Lifecycle.cs::V12_002.SweepBrokerOrders#method`
Signature: `private int SweepBrokerOrders(bool force)`
Lines: 1360–1454 (95 lines)
Key findings:
- Dual-mode prefix array construction via ternary (`force=true` → 14 prefixes, `force=false` → 7 prefixes)
- Triple-nested iteration: `Account.All` → `acct.Orders.ToArray()` → `v12Prefixes[pi]`
- Five-state `OrderState` fan-out guard clause (Working, Accepted, Submitted, ChangePending, ChangeSubmitted)
- Inner `for` loop prefix scan with `StartsWith` and early `break` to set `isV12` flag
- `[FIX-FF]` bracket exclusion guard on `!force` path: 8 `StartsWith` checks (Stop_, S_, T1_–T5_, Target_)
- Dual `try/catch` nesting (outer per-account, inner per-cancel call)
- Returns `int brokerCancels` count

### jcodemunch get_call_hierarchy result

- **Callers (depth 1):** `CancelAllV12GtcOrders` at `src/V12_002.SIMA.Lifecycle.cs:1294`
- **Callers (depth 2):** `ProcessShutdownSIMA` at `src/V12_002.SIMA.Lifecycle.cs:98`
- **Callees (depth 1):** `IsFleetAccount` (src/V12_002.cs:864), `LogBuffer.Format`
- **Callees (depth 2):** `LogBuffer.ValidateThreadAffinity`, `LogBuffer.FormatInternal`
- Call resolution confidence: `ast_resolved` for direct callers; `ast_inferred` for callees

### jcodemunch get_dependency_graph result

- File is self-contained: no cross-file import edges detected
- `node_count=1`, `edge_count=0` — no explicit file-level imports or importers in index
- All NinjaTrader APIs are resolved via global using statements (not cross-file compile edges)

### jcodemunch get_extraction_candidates result

- Returned: empty (no pre-computed extraction candidates — complexity metadata not populated at index time)
- Manual analysis applied using full source from `get_context_bundle` — see Sequential Thinking below

---

## Sequential Thinking Summary

**5-thought chain completed.** Final verdict (Thought 5):

The extraction plan for `SweepBrokerOrders` (CYC 28) produces **7 new private helper methods**. Every method — parent AND all helpers — is projected at CYC <= 8.

CYC breakdown before extraction:
- 1 base + 1 ternary (prefix build) + 1 foreach accounts + 1 IsFleetAccount + 1 outer try + 1 foreach orders
- + 1 instrument check + 5 OrderState comparisons + 1 prefix for-loop + 1 StartsWith + 1 isV12 check
- + 1 !force guard + 8 bracket StartsWith + 1 isBracketOrder check + 1 inner try + 1 inner catch + 1 outer catch
- = **CYC 28** (confirmed)

Extraction strategy applied:
- **Extract Guard Clauses** → `IsCancellableOrderState`, bracket predicate hierarchy
- **Extract Named Helper Methods** → `BuildSweepPrefixes`, `HasMatchingV12Prefix`
- **Extract Loop Body** → `TryCancelV12Order` (entire per-order processing block)
- The bracket exclusion (8 conditions) is split into two sub-predicates to keep each at CYC<=8

Jane Street verdict: **APPROVED** — all methods CYC<=8, single-responsibility, zero new allocations per order.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `BuildSweepPrefixes` | `private static string[] BuildSweepPrefixes(bool force)` | Returns 14-element prefix array (force=true) or 7-element entry-signal-only array (force=false). Encapsulates the dual-mode ternary at lines 1365-1383. | 2 |
| `IsCancellableOrderState` | `private static bool IsCancellableOrderState(Order ord)` | Returns true if ord.OrderState is one of the 5 cancellable states: Working, Accepted, Submitted, ChangePending, ChangeSubmitted. Isolates the 5-way OR guard clause. | 6 |
| `IsStopSideProtectedPrefix` | `private static bool IsStopSideProtectedPrefix(string ordName)` | Returns true if ordName starts with Stop_, S_, or Target_ (stop-loss and target bracket prefixes). Handles 3 of the 8 [FIX-FF] bracket prefix checks. | 4 |
| `IsTakeProfitProtectedPrefix` | `private static bool IsTakeProfitProtectedPrefix(string ordName)` | Returns true if ordName starts with T1_, T2_, T3_, T4_, or T5_ (take-profit tier bracket prefixes). Handles 5 of the 8 [FIX-FF] bracket prefix checks. | 6 |
| `IsProtectedBracketOrder` | `private static bool IsProtectedBracketOrder(string ordName)` | Composes IsStopSideProtectedPrefix || IsTakeProfitProtectedPrefix. Single-responsibility facade for the [FIX-FF] bracket exclusion logic. | 2 |
| `HasMatchingV12Prefix` | `private static bool HasMatchingV12Prefix(string ordName, string[] prefixes)` | Iterates the prefix array and returns true on first case-insensitive StartsWith match. Encapsulates the isV12 flag-setting for-loop. Zero-alloc scan. | 3 |
| `TryCancelV12Order` | `private static bool TryCancelV12Order(Account acct, Order ord, bool force, string[] prefixes, string instrumentFullName)` | Orchestrates all per-order decisions: instrument match, cancellable state, V12 prefix match, bracket exclusion on soft-disable, and acct.Cancel call. Returns true if order was cancelled. Encapsulates the entire inner foreach body. | 8 |

---

## Parent Method After Extraction

**Remaining logic in `SweepBrokerOrders` after all 7 extractions:**

```csharp
private int SweepBrokerOrders(bool force)
{
    int brokerCancels = 0;
    string[] prefixes = BuildSweepPrefixes(force);
    string instrumentFullName = Instrument?.FullName ?? string.Empty;
    foreach (Account acct in Account.All)
    {
        if (!IsFleetAccount(acct))
            continue;
        try
        {
            foreach (Order ord in acct.Orders.ToArray())
            {
                if (TryCancelV12Order(acct, ord, force, prefixes, instrumentFullName))
                    brokerCancels++;
            }
        }
        catch { }
    }
    return brokerCancels;
}
```

- **Remaining logic:** Account iteration, fleet filter, per-account try/catch, order iteration, delegate to `TryCancelV12Order`, increment counter, return
- **Projected CYC:** 7
  - 1 (base) + 1 (foreach accounts) + 1 (IsFleetAccount continue) + 1 (outer try/catch) + 1 (foreach orders) + 1 (TryCancelV12Order result) + 1 (outer catch block) = **7**

---

## max_cyc_projected: 8
## extraction_count: 7

---

## Projected CYC Summary

| Symbol | Projected CYC | Within Limit |
|---|---|---|
| `BuildSweepPrefixes` | 2 | YES |
| `IsCancellableOrderState` | 6 | YES |
| `IsStopSideProtectedPrefix` | 4 | YES |
| `IsTakeProfitProtectedPrefix` | 6 | YES |
| `IsProtectedBracketOrder` | 2 | YES |
| `HasMatchingV12Prefix` | 3 | YES |
| `TryCancelV12Order` | 8 | YES |
| `SweepBrokerOrders` (parent) | 7 | YES |
| **MAX** | **8** | **YES** |

---

## Jane Street Alignment

- **CYC<=8 achieved:** YES — all 8 methods (7 helpers + 1 parent) are <= 8; max = 8
- **Single-responsibility per helper:** YES
  - `BuildSweepPrefixes`: only builds prefix array
  - `IsCancellableOrderState`: only checks order state membership
  - `IsStopSideProtectedPrefix`: only checks 3 stop/target prefixes
  - `IsTakeProfitProtectedPrefix`: only checks 5 TP-tier prefixes
  - `IsProtectedBracketOrder`: only composes the two bracket predicates
  - `HasMatchingV12Prefix`: only scans for prefix match
  - `TryCancelV12Order`: only orchestrates single-order cancel decision
- **Lock-free/Actor pattern preserved:** YES — no `lock()` blocks introduced; try/catch pattern retained (NinjaTrader broker API requirement)
- **Illegal states unrepresentable:** YES — `IsCancellableOrderState` encapsulates the valid cancellable state set; `IsProtectedBracketOrder` encapsulates the bracket protection set; callers cannot accidentally skip these checks as they are named and testable predicates
- **Zero-allocation hot paths:** YES — `BuildSweepPrefixes` allocates the array once per sweep invocation (not per-order); all 6 predicate helpers (2–7) are pure functions with no heap allocations; `HasMatchingV12Prefix` scans the existing array in-place

---

## Implementation Notes for Phase 5 (Bob CLI)

1. Add all 7 helpers as `private static` methods in the same partial class as `SweepBrokerOrders`
2. `TryCancelV12Order` requires `instrumentFullName` as parameter — caller extracts `Instrument?.FullName ?? string.Empty` before the account loop (avoids repeated null-conditional evaluation per order)
3. `StringComparison.OrdinalIgnoreCase` must be preserved in all `StartsWith` calls — do not substitute `ToLower()` (allocation violation)
4. The `[FIX-FF]` comment block must be preserved in `IsProtectedBracketOrder` or its immediate callers for audit trail continuity
5. The outer `try/catch` per-account loop is preserved in parent — it is a NinjaTrader requirement, not dead error handling
6. All 7 helpers must compile with zero new warnings; CSharpier format before commit

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-056 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Original CYC** | 28 |
| **max_cyc_projected** | 8 |
| **extraction_count** | 7 |
