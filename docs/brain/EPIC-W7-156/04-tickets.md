# Phase 4: Tickets — EPIC-W7-156

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:25:00Z
**Input:** docs/brain/EPIC-W7-156/02-architecture-plan.md + docs/brain/EPIC-W7-156/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-156 |
| **Method** | `CancelAll_ProcessSingleFleetAccount` |
| **Source File** | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **Original CYC** | 18 (jCodemunch confirmed: cyclomatic=18, assessment=high) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 4 |
| **max_cyc_projected** | 7 |
| **dna_verdict** | PASS (from Phase 3) |

---

## Tickets

---

### Ticket 1 — Extract `IsOrderCancellable`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-156-T1 |
| **helper_name** | `IsOrderCancellable(Order order, string instrumentFullName)` |
| **access** | `private` |
| **return_type** | `bool` |
| **concern** | Order eligibility guard: consolidates null check, instrument name match, and 5-state OrderState OR chain (Working, Accepted, Submitted, ChangePending, ChangeSubmitted) |
| **lines_to_move** | 8 (null guard + instrument check + 5 OrderState OR clauses extracted from parent compound if predicate) |
| **cyc_reduction** | -7 (removes 7 branches from parent: null, instrument, 5 OrderState conditions) |
| **projected_helper_cyc** | **7** ✓ (≤8) |
| **dependency** | None — independent of T2 and T3 |
| **execution_order** | 1 (can run in parallel with T2) |

**Extracted Implementation:**

```csharp
private bool IsOrderCancellable(Order order, string instrumentFullName)
{
    if (order == null) return false;
    if (order.Instrument.FullName != instrumentFullName) return false;
    return order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Accepted
        || order.OrderState == OrderState.Submitted
        || order.OrderState == OrderState.ChangePending
        || order.OrderState == OrderState.ChangeSubmitted;
}
```

**Parent call site after extraction:**

```csharp
if (!IsOrderCancellable(order, Instrument.FullName))
    continue;
```

---

### Ticket 2 — Extract `IsBracketManagementOrder`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-156-T2 |
| **helper_name** | `IsBracketManagementOrder(string orderName)` |
| **access** | `private static` |
| **return_type** | `bool` |
| **concern** | Bracket prefix filter: returns true if order name starts with any of 7 bracket management prefixes (Stop_, S_, T1_, T2_, T3_, T4_, T5_) |
| **lines_to_move** | 7 (7 StartsWith OR clauses extracted from parent compound if predicate) |
| **cyc_reduction** | -6 (removes 6 OR-branch nodes from parent; 7 StartsWith = 6 OR connectors + 1 base) |
| **projected_helper_cyc** | **7** ✓ (≤8) |
| **dependency** | None — independent of T1 and T3; T3 depends on T2 |
| **execution_order** | 1 (can run in parallel with T1; must complete before T3) |

**Extracted Implementation:**

```csharp
private static bool IsBracketManagementOrder(string orderName)
{
    return orderName.StartsWith("Stop_")
        || orderName.StartsWith("S_")
        || orderName.StartsWith("T1_")
        || orderName.StartsWith("T2_")
        || orderName.StartsWith("T3_")
        || orderName.StartsWith("T4_")
        || orderName.StartsWith("T5_");
}
```

**Note:** This helper is also reusable by `CancelAll_ProcessMasterAccount` (DRY improvement per Phase 2).

---

### Ticket 3 — Extract `ShouldPreserveBracketOrder`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-156-T3 |
| **helper_name** | `ShouldPreserveBracketOrder(string orderName, bool acctHasActiveFsm, bool masterHasPosition)` |
| **access** | `private` |
| **return_type** | `bool` |
| **concern** | FSM/position dual-gate (Build 1104.1 invariant): returns true when bracket order should be preserved — order is a bracket management order AND FSM is active AND master has position |
| **lines_to_move** | 4 (bracket order check + acctHasActiveFsm guard + masterHasPosition guard + continue statement) |
| **cyc_reduction** | -2 (removes 2 && branch nodes from parent; bracket prefix check already removed by T2) |
| **projected_helper_cyc** | **3** ✓ (≤8) |
| **dependency** | T2 must complete first (calls `IsBracketManagementOrder` internally) |
| **execution_order** | 2 (must run after T2) |

**Extracted Implementation:**

```csharp
private bool ShouldPreserveBracketOrder(
    string orderName,
    bool acctHasActiveFsm,
    bool masterHasPosition)
{
    return IsBracketManagementOrder(orderName) && acctHasActiveFsm && masterHasPosition;
}
```

**Parent call site after extraction:**

```csharp
if (ShouldPreserveBracketOrder(order.Name, acctHasActiveFsm, masterHasPosition))
    continue;
```

---

## Parent Method After All Extractions

**projected_parent_cyc_after_all: 4** ✓ (≤8)

CYC breakdown: 1(base) + 1(foreach) + 1(IsOrderCancellable guard-continue) + 1(ShouldPreserveBracketOrder guard-continue) = **4**

```csharp
private int CancelAll_ProcessSingleFleetAccount(Account acct, bool masterHasPosition)
{
    int cancelled = 0;
    var acctFsms = _followerBrackets.Values.Where(f => f.AccountName == acct.Name).ToList();
    bool acctHasActiveFsm = acctFsms.Any(f => f.State == FollowerBracketState.Active);

    foreach (Order order in acct.Orders)
    {
        if (!IsOrderCancellable(order, Instrument.FullName))
            continue;
        if (ShouldPreserveBracketOrder(order.Name, acctHasActiveFsm, masterHasPosition))
            continue;
        CancelOrderOnAccount(order, acct);
        cancelled++;
    }
    return cancelled;
}
```

---

## CYC Verification Table

| Method | CYC Before | CYC After | Passes (≤8)? |
|---|---|---|---|
| `CancelAll_ProcessSingleFleetAccount` | 18 | 4 | ✓ YES |
| `IsOrderCancellable` | (new) | 7 | ✓ YES |
| `IsBracketManagementOrder` | (new) | 7 | ✓ YES |
| `ShouldPreserveBracketOrder` | (new) | 3 | ✓ YES |
| **max_cyc_projected** | — | **7** | ✓ YES |

---

## MCP Evidence

### resolve_repo
- **Path:** `/home/malhitticrypto/universal-or-strategy`
- **Result:** `indexed=true`, `repo=antigravityos187-sketch/universal-or-strategy`, `symbol_count=5147`, `file_count=2000`

### get_symbol_complexity
- **Symbol ID:** `src/V12_002.UI.IPC.Commands.Fleet.cs::V12_002.CancelAll_ProcessSingleFleetAccount#method`
- **cyclomatic:** 18
- **max_nesting:** 4
- **param_count:** 2
- **lines:** 44
- **assessment:** high
- **Verdict:** Confirms CYC=18 — extraction required

### get_extraction_candidates
- **File:** `src/V12_002.UI.IPC.Commands.Fleet.cs`
- **Result:** `candidates=[]` (min_complexity=5, min_callers=2)
- **Verdict:** No automated candidates; extractions are intra-method predicate decompositions correctly identified via CYC decomposition in Phase 2 (as noted in 02-architecture-plan.md)

---

## Sequential Thinking Evidence

### Thought 1 — Ticket count determination
- Architecture plan (Phase 2) specifies 3 helper extractions: IsOrderCancellable, IsBracketManagementOrder, ShouldPreserveBracketOrder
- Each helper = 1 atomic ticket (implement helper + replace inline predicate)
- T3 depends on T2 (calls IsBracketManagementOrder); T1 is fully independent
- **Conclusion:** ticket_count = 3

### Thought 2 — Per-ticket detail enumeration
- T1: IsOrderCancellable — 8 lines, removes 7 parent branches, helper CYC 7
- T2: IsBracketManagementOrder — 7 lines, removes 6 parent branches, helper CYC 7
- T3: ShouldPreserveBracketOrder — 4 lines, removes 2 parent branches, helper CYC 3
- T3 execution_order = 2 (after T2); T1 and T2 execution_order = 1 (parallel)

### Thought 3 — CYC verification
- Parent CYC reduction path: 18 → (−7 T1) → 11 → (−6 T2) → 5 → (−2 T3) → 3 structural; with replacement guard calls: parent final = 4
- All helpers: 7, 7, 3 — all ≤ 8 ✓
- max_cyc_projected = 7 ✓
- **Verdict: PASS — all 4 methods achieve CYC ≤ 8**

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.3 |
| **Execution Time** | 2026-06-29T01:25:00Z |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-156 |
| **jCodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates, search_symbols |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket breakdown) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 4 |
| **max_cyc_projected** | 7 |
