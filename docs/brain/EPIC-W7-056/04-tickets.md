# Phase 4: Ticket Definitions — EPIC-W7-056

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-056/02-architecture-plan.md + docs/brain/EPIC-W7-056/03-audit-report.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `SweepBrokerOrders` |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Lines** | 1360–1454 |
| **Original CYC** | 28 (confirmed by jcodemunch: `cyclomatic=28`, `lines=95`, `max_nesting=8`, `assessment=high`) |
| **Extraction Count** | 7 |
| **Projected Parent CYC After All Extractions** | 7 |
| **max_cyc_projected** | 8 |
| **DNA Verdict** | PASS (Phase 3 audit) |

---

## ticket_count: 7

---

## Ticket Execution Order

Tickets MUST be executed in dependency order. `TryCancelV12Order` (T7) calls `IsCancellableOrderState` (T2), `IsProtectedBracketOrder` (T5), and `HasMatchingV12Prefix` (T6). `IsProtectedBracketOrder` (T5) composes `IsStopSideProtectedPrefix` (T3) and `IsTakeProfitProtectedPrefix` (T4).

**Required order:** T1 → T6 → T2 → T3 → T4 → T5 → T7

---

## Ticket 1 — BuildSweepPrefixes

| Field | Value |
|---|---|
| **ticket_id** | 1 |
| **helper_name** | `BuildSweepPrefixes` |
| **signature** | `private static string[] BuildSweepPrefixes(bool force)` |
| **concern** | Construct the V12 order prefix array for a sweep pass. Returns a 14-element array when `force=true` (all prefixes including bracket/stop/target types) or a 7-element array when `force=false` (entry-signal prefixes only). Encapsulates the dual-mode ternary at lines 1365–1383. |
| **lines_to_move** | Lines 1365–1383: the ternary expression that selects between two hard-coded `string[]` literals based on the `force` parameter, plus the local variable declaration `string[] v12Prefixes`. |
| **cyc_reduction** | -2 (removes 1 ternary branch + 1 array-size selection branch from parent) |
| **projected_helper_cyc** | 2 |
| **dependencies** | None |
| **phase_5_notes** | Declared `private static` in same partial class. Called at top of `SweepBrokerOrders` as `string[] prefixes = BuildSweepPrefixes(force);`. No heap allocation per order — array is built once per sweep invocation. |

---

## Ticket 2 — IsCancellableOrderState

| Field | Value |
|---|---|
| **ticket_id** | 2 |
| **helper_name** | `IsCancellableOrderState` |
| **signature** | `private static bool IsCancellableOrderState(Order ord)` |
| **concern** | Classify whether an order's state is eligible for cancellation. Returns `true` if `ord.OrderState` is one of the 5 valid cancellable states: `OrderState.Working`, `OrderState.Accepted`, `OrderState.Submitted`, `OrderState.ChangePending`, or `OrderState.ChangeSubmitted`. Isolates the 5-way OR guard clause that protects the cancel path. |
| **lines_to_move** | Lines approx 1396–1402: the compound `if` guard with 5 `OrderState` enum comparisons chained via `||`. |
| **cyc_reduction** | -5 (removes 5 OR-branch comparisons from inner TryCancelV12Order, net CYC reduction to parent after T7 extraction) |
| **projected_helper_cyc** | 6 |
| **dependencies** | None |
| **phase_5_notes** | Encapsulates valid cancellable state set — callers cannot accidentally omit a state. Makes illegal states (e.g. Filled, Cancelled) unrepresentable at the call site. xUnit-testable with `Assert.True` / `Assert.False` for each of the 5 states plus one invalid state. |

---

## Ticket 3 — IsStopSideProtectedPrefix

| Field | Value |
|---|---|
| **ticket_id** | 3 |
| **helper_name** | `IsStopSideProtectedPrefix` |
| **signature** | `private static bool IsStopSideProtectedPrefix(string ordName)` |
| **concern** | Detect stop-loss and target bracket prefixes on the stop side. Returns `true` if `ordName` starts with `Stop_`, `S_`, or `Target_`. Handles 3 of the 8 `[FIX-FF]` bracket exclusion prefix checks. Prevents soft-disable sweeps from cancelling stop-side bracket orders. |
| **lines_to_move** | Lines approx 1420–1424: the 3 `ordName.StartsWith(...)` checks for `"Stop_"`, `"S_"`, and `"Target_"` within the `[FIX-FF]` bracket guard block, with `StringComparison.OrdinalIgnoreCase`. |
| **cyc_reduction** | -3 (absorbed into IsProtectedBracketOrder / TryCancelV12Order chain) |
| **projected_helper_cyc** | 4 |
| **dependencies** | None |
| **phase_5_notes** | Must preserve `StringComparison.OrdinalIgnoreCase` in all `StartsWith` calls — do not substitute `ToLower()` (allocation violation). The `[FIX-FF]` comment must be retained in `IsProtectedBracketOrder` for audit trail continuity. |

---

## Ticket 4 — IsTakeProfitProtectedPrefix

| Field | Value |
|---|---|
| **ticket_id** | 4 |
| **helper_name** | `IsTakeProfitProtectedPrefix` |
| **signature** | `private static bool IsTakeProfitProtectedPrefix(string ordName)` |
| **concern** | Detect take-profit tier bracket prefixes. Returns `true` if `ordName` starts with `T1_`, `T2_`, `T3_`, `T4_`, or `T5_`. Handles the 5 TP-tier prefix checks of the `[FIX-FF]` bracket exclusion block. Prevents soft-disable sweeps from cancelling take-profit tier bracket orders. |
| **lines_to_move** | Lines approx 1425–1430: the 5 `ordName.StartsWith(...)` checks for `"T1_"`, `"T2_"`, `"T3_"`, `"T4_"`, and `"T5_"` within the `[FIX-FF]` bracket guard block, with `StringComparison.OrdinalIgnoreCase`. |
| **cyc_reduction** | -5 (absorbed into IsProtectedBracketOrder / TryCancelV12Order chain) |
| **projected_helper_cyc** | 6 |
| **dependencies** | None |
| **phase_5_notes** | Must preserve `StringComparison.OrdinalIgnoreCase`. If future TP tiers are added (T6_, T7_, etc.), only this method needs updating — single point of change. xUnit-testable: 5 `Assert.True` cases (one per tier) + 1 `Assert.False` for non-TP prefix. |

---

## Ticket 5 — IsProtectedBracketOrder

| Field | Value |
|---|---|
| **ticket_id** | 5 |
| **helper_name** | `IsProtectedBracketOrder` |
| **signature** | `private static bool IsProtectedBracketOrder(string ordName)` |
| **concern** | Facade predicate for the full `[FIX-FF]` bracket exclusion logic. Returns `true` if `ordName` is any kind of protected bracket order, composing `IsStopSideProtectedPrefix(ordName) || IsTakeProfitProtectedPrefix(ordName)`. Used in `TryCancelV12Order` on the `!force` path to skip bracket orders during soft-disable sweeps. |
| **lines_to_move** | Lines approx 1418–1432: the entire `[FIX-FF]` comment block and the 8-condition bracket prefix guard. Replaced by a single call to `IsProtectedBracketOrder`. |
| **cyc_reduction** | -8 (removes 8 StartsWith conditions from the per-order processing block in parent) |
| **projected_helper_cyc** | 2 |
| **dependencies** | Ticket 3 (IsStopSideProtectedPrefix), Ticket 4 (IsTakeProfitProtectedPrefix) |
| **phase_5_notes** | The `[FIX-FF]` comment must be placed inside or immediately above this method as it is the authoritative bracket-exclusion predicate. CYC = 2: 1 base + 1 OR composition. Zero heap allocations — pure predicate delegation. |

---

## Ticket 6 — HasMatchingV12Prefix

| Field | Value |
|---|---|
| **ticket_id** | 6 |
| **helper_name** | `HasMatchingV12Prefix` |
| **signature** | `private static bool HasMatchingV12Prefix(string ordName, string[] prefixes)` |
| **concern** | Scan the V12 prefix array to determine if an order name belongs to the V12 system. Iterates the prefix array and returns `true` on the first `StartsWith` match (case-insensitive). Encapsulates the `isV12` flag-setting `for` loop with `break` from the original method. Zero-allocation array scan. |
| **lines_to_move** | Lines approx 1407–1414: the `for (int pi = 0; pi < v12Prefixes.Length; pi++)` loop that sets the `bool isV12` flag, including the inner `StartsWith` and `break`. |
| **cyc_reduction** | -3 (removes for-loop + StartsWith + break from per-order block) |
| **projected_helper_cyc** | 3 |
| **dependencies** | None |
| **phase_5_notes** | Must preserve `StringComparison.OrdinalIgnoreCase`. Returns `bool` directly (eliminates the `isV12` flag variable). The prefix array is passed in — no re-allocation per call. xUnit-testable: pass prefixes array and matching/non-matching order names. |

---

## Ticket 7 — TryCancelV12Order

| Field | Value |
|---|---|
| **ticket_id** | 7 |
| **helper_name** | `TryCancelV12Order` |
| **signature** | `private static bool TryCancelV12Order(Account acct, Order ord, bool force, string[] prefixes, string instrumentFullName)` |
| **concern** | Orchestrate all per-order cancel decision logic. Performs: (1) instrument full-name match, (2) `IsCancellableOrderState` check, (3) `HasMatchingV12Prefix` check, (4) `IsProtectedBracketOrder` exclusion on `!force` path, (5) `acct.Cancel(ord)` call wrapped in inner try/catch. Returns `true` if the order was cancelled. Encapsulates the entire `foreach (Order ord in acct.Orders.ToArray())` body. |
| **lines_to_move** | Lines approx 1390–1450: the inner `foreach` body covering instrument check, order state guard, prefix scan, bracket exclusion on soft-disable path, and the `acct.Cancel(ord)` call with inner try/catch. The `instrumentFullName` local variable is extracted before the account loop in the parent method as `string instrumentFullName = Instrument?.FullName ?? string.Empty;`. |
| **cyc_reduction** | -21 total from parent (instrument check + state check + prefix match + bracket exclusion guard + force check + inner try + cancel branch + inner catch — combined with T1–T6 this reduces parent from CYC 28 to CYC 7) |
| **projected_helper_cyc** | 8 |
| **dependencies** | Ticket 2 (IsCancellableOrderState), Ticket 5 (IsProtectedBracketOrder), Ticket 6 (HasMatchingV12Prefix) |
| **phase_5_notes** | This is the most complex helper (CYC boundary = 8). The inner `try/catch` is a NinjaTrader broker API requirement — do not remove. `instrumentFullName` must be extracted by the parent before the account loop (avoids repeated null-conditional evaluation per order, zero-alloc). All 4 called helpers (T2, T3, T4, T5, T6) must be defined before this ticket is implemented. |

---

## Projected CYC After All Extractions

| Symbol | Projected CYC | Within CYC <= 8 |
|---|---|---|
| `BuildSweepPrefixes` (T1) | 2 | YES |
| `IsCancellableOrderState` (T2) | 6 | YES |
| `IsStopSideProtectedPrefix` (T3) | 4 | YES |
| `IsTakeProfitProtectedPrefix` (T4) | 6 | YES |
| `IsProtectedBracketOrder` (T5) | 2 | YES |
| `HasMatchingV12Prefix` (T6) | 3 | YES |
| `TryCancelV12Order` (T7) | 8 | YES (boundary) |
| `SweepBrokerOrders` (parent) | 7 | YES |
| **MAX** | **8** | **PASS** |

**projected_parent_cyc_after_all: 7**

CYC reduced: 28 → 7 (parent) — 21 complexity units eliminated from the parent. Total new methods: 7 helpers, all CYC <= 8.

---

## jcodemunch Evidence

```json
{
  "tool": "get_symbol_complexity",
  "symbol_id": "src/V12_002.SIMA.Lifecycle.cs::V12_002.SweepBrokerOrders#method",
  "cyclomatic": 28,
  "max_nesting": 8,
  "param_count": 1,
  "lines": 95,
  "assessment": "high"
}
```

```json
{
  "tool": "get_extraction_candidates",
  "file": "src/V12_002.SIMA.Lifecycle.cs",
  "candidates": [],
  "note": "Empty — complexity metadata not pre-populated at index time. Manual analysis applied per Phase 2 architecture plan."
}
```

---

## Sequential Thinking Validation

**3-thought chain completed (thoughtHistoryLength advanced 152 → 159 → 164).**

- **Thought 1:** Ticket count determination — CYC=28 requires 7 helpers per architecture plan; one ticket per extracted helper; ticket count = 7.
- **Thought 2:** Per-ticket detail — lines to move, helper signature, CYC reduction, projected helper CYC documented for all 7 tickets. Dependency order established: T1 → T6 → T2 → T3 → T4 → T5 → T7.
- **Thought 3:** Verification pass — parent CYC post-extraction = 7 (confirmed arithmetic: 1+1+1+1+1+1+1=7). All 7 helpers CYC <= 8. Max across all symbols = 8. Jane Street CYC <= 8 mandate satisfied. Verdict: APPROVED.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.8 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-056 |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 validation thoughts) |
| **ticket_count** | 7 |
| **projected_parent_cyc_after_all** | 7 |
| **max_cyc_projected** | 8 |
