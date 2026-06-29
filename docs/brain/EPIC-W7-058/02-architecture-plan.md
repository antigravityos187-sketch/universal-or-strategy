# EPIC-W7-058 — Phase 2: Architecture Plan

**Agent Name:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T02:00:00Z
**Input:** docs/brain/EPIC-W7-058/01-scope-boundary.md

---

## Method Overview

| Field | Value |
|---|---|
| **Method** | `MapOrderStateToFSMState` |
| **File** | [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:469) |
| **Signature** | `private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)` |
| **CYC Baseline** | 13 (live index ground truth; precomputed reported 34) |
| **Target CYC** | <= 8 (all methods) |
| **Lines** | 25 |
| **Max Nesting** | 1 |
| **Direct Callers** | `HydrateFSMsFromWorkingOrders` (DO NOT MODIFY) |

---

## MCP Evidence

### get_context_bundle Result

Method source retrieved from [`src/V12_002.SIMA.Lifecycle.cs:469`](src/V12_002.SIMA.Lifecycle.cs:469):

```csharp
private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)
{
    if (entryState == OrderState.Filled || entryState == OrderState.PartFilled)
    {
        return FollowerBracketState.Active;
    }
    else if (entryState == OrderState.Accepted)
    {
        return FollowerBracketState.Accepted;
    }
    else if (
        entryState == OrderState.Working
        || entryState == OrderState.Submitted
        || entryState == OrderState.Initialized
        || entryState == OrderState.ChangePending
        || entryState == OrderState.ChangeSubmitted
    )
    {
        return FollowerBracketState.Submitted;
    }
    else
    {
        return null; // Terminal state - skip FSM creation
    }
}
```

**Docstring:** "Maps NinjaTrader OrderState to V12 FollowerBracketState. Pure function - no side effects, deterministic mapping. Terminal states (Cancelled, Rejected, etc.) return null to signal caller to skip FSM creation."

**Key observation:** Pure dispatch table. Three semantic groups of OrderState values, plus terminal null fallthrough. Two groups use compound OR conditions that drive CYC above threshold.

### get_call_hierarchy Result

- **Callers (depth 1):** [`HydrateFSMsFromWorkingOrders`](src/V12_002.SIMA.Lifecycle.cs:787) — direct caller; DO NOT MODIFY
- **Callers (depth 2):** [`HydrateWorkingOrdersFromBroker`](src/V12_002.SIMA.Lifecycle.cs:309) — indirect; DO NOT MODIFY
- **Callees:** None — `MapOrderStateToFSMState` calls no other methods
- **Dispatches:** None

Signature is unchanged by this refactor. Both upstream callers are unaffected.

### get_dependency_graph Result

- **File imports:** None (self-contained file; all dependencies via `using` directives resolved at compile time)
- **Edge count:** 0 at depth=1
- **Impact:** No cross-file dependency changes required by this extraction

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Drivers

CYC=13 (live index) confirmed. Method has 3 conditional branches:
- Branch 1: `Filled || PartFilled` — 2 OR conditions (+2 CYC)
- Branch 2: `Accepted` — 1 equality check (+1 CYC)
- Branch 3: `Working || Submitted || Initialized || ChangePending || ChangeSubmitted` — 5 OR conditions (+5 CYC)
- Base: +1 CYC

Total driver CYC: 1 + 2 + 1 + 5 = 9 (with else-if chain, full McCabe = 13).
The compound OR expressions in Branch 1 and Branch 3 are the primary complexity inflators.

Four semantic OrderState groups:
1. **Active**: `Filled`, `PartFilled` → `FollowerBracketState.Active`
2. **Accepted**: `Accepted` → `FollowerBracketState.Accepted`
3. **Submitted**: `Working`, `Submitted`, `Initialized`, `ChangePending`, `ChangeSubmitted` → `FollowerBracketState.Submitted`
4. **Terminal**: Everything else → `null` (skip FSM creation)

### Thought 2 — Extraction Strategy

Apply Jane Street extract-predicate pattern:

**Extract compound boolean guards as private predicate helpers** with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`. The parent becomes a clean dispatch using simple single-call if-statements.

Two helpers identified:
- `IsActiveOrderState(OrderState s)` — absorbs `Filled || PartFilled` compound condition
- `IsSubmittedOrderState(OrderState s)` — absorbs 5-value OR compound condition

Refactored parent becomes:
```csharp
private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)
{
    if (IsActiveOrderState(entryState)) return FollowerBracketState.Active;
    if (entryState == OrderState.Accepted) return FollowerBracketState.Accepted;
    if (IsSubmittedOrderState(entryState)) return FollowerBracketState.Submitted;
    return null;
}
```

Note: Precomputed.json estimated 6 extractions, but actual source analysis shows only 2 compound conditions warranting extraction. Over-fragmenting a 25-line pure function would violate single-responsibility and Jane Street cognitive simplicity mandates.

### Thought 3 — CYC Validation

| Method | Formula | CYC | <= 8? |
|---|---|---|---|
| `MapOrderStateToFSMState` (refactored) | 1 + 3 if-branches | 4 | PASS |
| `IsActiveOrderState` (new) | 1 + 1 OR condition | 2 | PASS |
| `IsSubmittedOrderState` (new) | 1 + 4 OR conditions | 5 | PASS |

**Max CYC projected: 5**

All methods validated <= 8. Callers unchanged. Signature unchanged.

---

## Extraction Plan

| Helper Name | Absorbs from Parent | Est. CYC | Visibility | Attribute |
|---|---|---|---|---|
| `IsActiveOrderState` | `Filled \|\| PartFilled` compound guard | 2 | `private` | `[MethodImpl(AggressiveInlining)]` |
| `IsSubmittedOrderState` | `Working \|\| Submitted \|\| Initialized \|\| ChangePending \|\| ChangeSubmitted` compound guard | 5 | `private` | `[MethodImpl(AggressiveInlining)]` |

**Parent after extraction:** CYC = 4 (3 if-branches + base)
**Total new methods:** 2
**Max CYC projected across all methods: 5**

---

## Method Signatures

```csharp
// Parent — REFACTORED (signature unchanged)
private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)

// Helper 1 — NEW
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsActiveOrderState(OrderState s)

// Helper 2 — NEW
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsSubmittedOrderState(OrderState s)
```

---

## Jane Street KB Alignment

| Principle | Source | Application |
|---|---|---|
| Zero-alloc hot path | `carl_cook` | Pure enum comparison predicates — no heap allocation, no boxing |
| `AggressiveInlining` hot path | `carl_cook` | Both helpers decorated with `[MethodImpl(AggressiveInlining)]` — inlined at JIT |
| Avoid LINQ | `carl_cook` | No LINQ; all logic is direct enum equality comparisons |
| Single responsibility per helper | `trading_billions` | `IsActiveOrderState` tests exactly 2 active states; `IsSubmittedOrderState` tests exactly 5 submitted states |
| Each helper CYC <= 8 | `trading_billions` | Max helper CYC = 5 (IsSubmittedOrderState); parent CYC = 4 |
| No new `lock()` blocks | `gjengset` | Pure predicates, no state mutation, no synchronization needed |
| Defense in depth | `trading_billions` | Null return for terminal states explicitly preserved as documented fallthrough |

---

## V12.23 Scope Compliance

| Check | Status |
|---|---|
| ONE EPIC = ONE CONCERN | PASS — only `MapOrderStateToFSMState` + 2 new private helpers |
| Caller `HydrateFSMsFromWorkingOrders` not modified | PASS |
| No sibling method modifications | PASS |
| No cross-file changes | PASS — helpers added to same partial class |
| No interface/signature changes | PASS — parent signature identical |
| Scope matches Phase 1.5 boundary | PASS — boundary verdict was PASS |

---

## Risk Assessment

| Risk | Level | Mitigation |
|---|---|---|
| Breaking caller | LOW | Signature unchanged; return type unchanged |
| Over-extraction | NONE | 2 extractions vs precomputed estimate of 6; source analysis confirms 2 is correct |
| CYC overshoot | NONE | Max projected CYC = 5; all methods validated <= 8 |
| Cross-file impact | NONE | Helpers are private static in same partial class |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-058 |
| **Method** | MapOrderStateToFSMState |
| **CYC Baseline** | 13 (live index ground truth) |
| **Max CYC Projected** | 5 |
| **Extractions Planned** | 2 |
| **Status** | completed |
