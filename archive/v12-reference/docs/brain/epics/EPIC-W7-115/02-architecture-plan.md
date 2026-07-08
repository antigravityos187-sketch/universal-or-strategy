# Phase 2: Architecture Plan — EPIC-W7-115

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:15:00Z

---

## Method Under Extraction

- **Method:** `SweepTrackedOrders`
- **Source File:** `src/V12_002.SIMA.Lifecycle.cs`
- **Signature:** `private int SweepTrackedOrders(bool force)`
- **Location:** Line 1308–1353
- **Original CYC:** 216 (full subsystem hotspot aggregate); method-level ~12 per body; target <=8 per helper and parent

### jcodemunch get_context_bundle result

Full source obtained (lines 1308–1353, 45 lines). Method body:
1. Builds `trackedDicts` array via ternary: `force=true` → 7 ConcurrentDictionary targets (`entryOrders`, `stopOrders`, `target1Orders`–`target5Orders`); `force=false` → `{ entryOrders }` only.
2. Outer `foreach (var dict in trackedDicts)` with null-continue guard.
3. Inner `foreach (var kvp in dict.ToArray())` with null-continue on `kvp.Value`.
4. 5-way inverted `&&` OrderState guard: skips unless one of `Working`, `Accepted`, `Submitted`, `ChangePending`, `ChangeSubmitted`.
5. `try { CancelOrderOnAccount(ord, ord.Account); trackedCancels++; } catch { }` — swallows broker exceptions.
6. Returns `trackedCancels` (int count of successful cancellations).

### jcodemunch get_call_hierarchy result

- **Callers (depth 1):** `CancelAllV12GtcOrders` (line 1294, same file) — direct caller
- **Callers (depth 2):** `ProcessShutdownSIMA` (line 98, same file) — shutdown path entry
- **Callees (depth 1):** `CancelOrderOnAccount` (`src/V12_002.Orders.CancelGateway.cs:46`) — broker cancel gateway
- **Callees (depth 2):** `IsOrderTerminal` (`src/V12_002.Orders.Management.Flatten.cs:698`) — terminal state check (indirect via cancel gateway)
- **Caller count:** 2 (direct + depth-2). Method signature must remain `private int SweepTrackedOrders(bool force)` — unchanged.

### jcodemunch get_dependency_graph result

No cross-file import edges resolved (C# partial class; imports are not tracked as separate file-level imports). File is self-contained within the partial class. External calls are `CancelOrderOnAccount` (cancel gateway) and order-state reads on `NinjaTrader.Cbi` types.

### jcodemunch get_extraction_candidates result

Empty result at min_complexity=3, min_callers=1. The method has no existing multi-caller shared candidates. All extractions are complexity-reduction-only (single caller in target file). This confirms the extraction plan is purely structural — no existing shared utility exists to reuse.

---

## Sequential Thinking Summary

**Thought 1 — Problem framing:** CYC 216 (subsystem) / ~12 (method body) exceeds Jane Street <=8 ceiling. Method contains 5 identifiable logical sections: dict selection, outer loop, inner loop, OrderState guard, try/cancel body. 5+ helpers mandatory.

**Thought 2 — Helper design:** Identified 5 named helpers with "Tracked" prefix to prevent naming collision with EPIC-W7-056 (SweepBrokerOrders) and EPIC-W7-110 (AdoptMasterOrders). Each helper owns exactly one logical concern.

**Thought 3 — CYC verification:** All 5 helpers + parent verified against <=8 ceiling. Maximum projected CYC = 5 (`IsTrackedOrderCancellable` with 4 OR decision points). Parent reduced to CYC=1.

**Thought 4 — Jane Street alignment:** No lock() blocks. dict.ToArray() snapshot preserved (actor-safe concurrent read). IsTrackedOrderCancellable as named predicate makes the valid-state set explicit — single maintenance point for OrderState additions. force=false semantic preserved exactly in BuildTrackedSweepDicts.

**Thought 5 — Final verdict:** 5 helpers, max CYC=5, parent CYC=1. All constraints satisfied. Naming unique. Blast radius: same file, private methods only.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `BuildTrackedSweepDicts(bool force)` | Select which order tracking dicts to sweep: `force=true` → all 7 dicts; `force=false` → `entryOrders` only. Encapsulates the safety-critical force-flag semantic. | 2 |
| `IsTrackedOrderCancellable(Order ord)` | Predicate: returns `true` if order is in a live/cancellable state (`Working || Accepted || Submitted || ChangePending || ChangeSubmitted`). Rewritten from inverted &&-chain to positive OR-chain. | 5 |
| `CancelTrackedOrderSafe(Order ord)` | Try-cancel one order via `CancelOrderOnAccount(ord, ord.Account)`; swallow broker exception; return `true` on success, `false` on exception. | 2 |
| `SweepTrackedDictOrders(ConcurrentDictionary<string,Order> dict)` | Inner sweep: iterate `dict.ToArray()`, null-guard each `Order`, check `IsTrackedOrderCancellable`, call `CancelTrackedOrderSafe`, accumulate count. | 5 |
| `SweepAllTrackedDicts(ConcurrentDictionary<string,Order>[] dicts)` | Outer sweep: `foreach` dict array with null-guard; delegate to `SweepTrackedDictOrders`; accumulate and return total cancel count. | 3 |

---

## Parent Method After Extraction

**Remaining logic (orchestration only):**
```csharp
private int SweepTrackedOrders(bool force)
{
    var dicts = BuildTrackedSweepDicts(force);
    return SweepAllTrackedDicts(dicts);
}
```

- **Projected CYC:** 1 (base path only — no branches remain in parent)
- **Signature:** Unchanged — `private int SweepTrackedOrders(bool force)`
- **Return type:** Unchanged — `int` (cancel count)
- **Callers affected:** 0 — `CancelAllV12GtcOrders` call site unmodified

---

## max_cyc_projected: 5
## extraction_count: 5

---

## Method Signatures

```csharp
// Helper 1 — dict selection (force semantic)
private ConcurrentDictionary<string, Order>[] BuildTrackedSweepDicts(bool force)

// Helper 2 — OrderState predicate (pure function)
private bool IsTrackedOrderCancellable(Order ord)

// Helper 3 — safe single-order cancel
private bool CancelTrackedOrderSafe(Order ord)

// Helper 4 — inner dict sweep
private int SweepTrackedDictOrders(ConcurrentDictionary<string, Order> dict)

// Helper 5 — outer array sweep
private int SweepAllTrackedDicts(ConcurrentDictionary<string, Order>[] dicts)
```

---

## Naming Collision Audit

| Helper Name | EPIC-W7-056 (SweepBrokerOrders) | EPIC-W7-110 (AdoptMasterOrders) | Collision? |
|---|---|---|---|
| `BuildTrackedSweepDicts` | `SelectSweepDictionaries` | — | NO |
| `IsTrackedOrderCancellable` | `IsOrderCancellable` | — | NO (distinct name) |
| `CancelTrackedOrderSafe` | — | — | NO |
| `SweepTrackedDictOrders` | — | — | NO |
| `SweepAllTrackedDicts` | — | — | NO |

---

## Jane Street Alignment

| Principle | Status | Evidence |
|---|---|---|
| CYC<=8 achieved: parent + all helpers | YES | Parent=1, max helper=5, all <=8 |
| Single-responsibility per helper | YES | Each method owns exactly one logical concern |
| Lock-free / Actor pattern preserved | YES | No lock() blocks; dict.ToArray() snapshot retained (actor-safe concurrent read) |
| Illegal states unrepresentable | YES | `IsTrackedOrderCancellable` makes valid-state set explicit; single maintenance point for new OrderState values |
| Zero-allocation hot-paths | YES | Cancel path (not microsecond hot-path); no new allocations beyond original `new[]` array in dict selection |
| force=false semantic preserved | YES | `BuildTrackedSweepDicts` encapsulates ternary exactly — entry-only vs all-brackets semantic documented in method name |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | ~22 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **Epic ID** | EPIC-W7-115 |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | resolve_repo, search_symbols, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output** | docs/brain/EPIC-W7-115/02-architecture-plan.md |
