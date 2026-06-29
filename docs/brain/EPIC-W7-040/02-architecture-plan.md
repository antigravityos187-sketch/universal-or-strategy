# Phase 2: Architecture Plan — EPIC-W7-040

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-040/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `FindTargetOrderForPosition`
- **Source File:** `src/V12_002.Trailing.Breakeven.cs`
- **Lines:** 186–222
- **Visibility:** `private`
- **Region:** `#region Stop Management Helpers`
- **Original CYC:** 10

### jcodemunch get_context_bundle result

`get_context_bundle` with symbol_id `FindTargetOrderForPosition` returned a disambiguation error (2 definitions: `src/` and `src-vm-backup/`). Resolved via `search_symbols` with `file_pattern=src/V12_002.Trailing.Breakeven.cs`. Symbol confirmed:

- **Resolved ID:** `src/V12_002.Trailing.Breakeven.cs::V12_002.FindTargetOrderForPosition#method`
- **Signature:** `private Order FindTargetOrderForPosition(PositionInfo pos, string entryName, int targetNum, out string notFoundReason)`
- **Summary:** `[Phase7-S5-T05] Helper 2: Find target order for position`
- **Source verified at lines 186–222** — body reads `searchAcct.Orders` in a `foreach`, applies a compound 4-clause `&&`/`||` predicate, and returns the first matching `Order` or `null` with a diagnostic `notFoundReason` out-param.

### jcodemunch get_call_hierarchy result

- **Callers (depth=1):** 1 — `MoveSpecificTarget` (`src/V12_002.Trailing.Breakeven.cs:335`), resolution: `ast_resolved`
- **Callees (depth=1):** 0 — no explicit outgoing calls; reads `searchAcct.Orders` (property access, not a method call in the call graph)
- **Depth reached:** 1 (no further callee chain)
- **Implication:** Method is a pure query helper. Single call-site, no downstream dispatch. Extraction is safe with zero blast radius beyond the declaring file.

### jcodemunch get_dependency_graph result

- **File:** `src/V12_002.Trailing.Breakeven.cs`
- **Direction:** both (imports + importers)
- **Result:** `node_count=1`, `edge_count=0` — no file-level import edges detected
- **Implication:** The file is a self-contained partial class with no tracked file-level imports in the index. All dependencies are resolved at the C# partial-class level within the project, not via using-directives tracked as import edges. Cross-file blast radius confirmed zero.

### jcodemunch get_extraction_candidates result

- **Candidates returned:** 0 (no pre-existing high-complexity multi-caller candidates in the index)
- **Implication:** The extraction candidates are net-new private helpers (no pre-existing symbol). This is expected — `IsMatchingWorkingOrder` and `ResolveSearchAccount` do not yet exist in the codebase. The 0-result from this tool is correct and not a failure.

---

## Sequential Thinking Summary

**5 thoughts completed. Final verdict (Thought 5):**

Architecture plan for EPIC-W7-040 is sound and minimal. Two private helper extractions reduce the parent method from CYC 10 to CYC 4. Each helper satisfies the CYC <=8 mandate individually:

- `IsMatchingWorkingOrder(Order order, string targetOrderName) → bool`: Encapsulates the 4-clause compound predicate inside the foreach body. CYC breakdown: base(1) + null guard(1) + name &&(1) + instrument &&(1) + Working state check(1) + Accepted || check(1) = **CYC 6**. Replaces 5 decision nodes in parent with 1.
- `ResolveSearchAccount(PositionInfo pos) → Account`: Encapsulates the follower/master account-routing ternary with its inner `&&` guard. CYC breakdown: base(1) + ternary ?:(1) + && compound guard(1) = **CYC 3**. Replaces 2 decision nodes in parent with 0. Also eliminates duplication at lines 204, 446, 507.
- **Parent after extraction:** base(1) + EntryFilled guard(1) + foreach(1) + IsMatchingWorkingOrder call in if(1) = **CYC 4**.

Jane Street alignment: all five mandated checks pass (CYC, single-responsibility, lock-free, illegal-states, zero-allocation). Extraction count = 2. Max projected CYC = 6.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `IsMatchingWorkingOrder` | `private bool IsMatchingWorkingOrder(Order order, string targetOrderName)` | Returns true if the order is non-null, matches the target name and instrument, and is in a Working or Accepted state. Encapsulates the full 4-clause `&&`/`||` predicate from the foreach body. | **6** |
| `ResolveSearchAccount` | `private Account ResolveSearchAccount(PositionInfo pos)` | Returns the correct account to search for orders: `pos.ExecutingAccount` for follower positions (when non-null), otherwise `this.Account`. Eliminates the inline ternary and its `&&` guard. Also resolves the 3-site duplication at lines 204, 446, 507. | **3** |

### Extracted Helper Signatures (Phase 5 implementation reference)

```csharp
/// <summary>Returns true if order is a working/accepted order matching the target name and instrument.</summary>
private bool IsMatchingWorkingOrder(Order order, string targetOrderName)
{
    return order != null
        && order.Name == targetOrderName
        && order.Instrument.FullName == Instrument.FullName
        && (order.OrderState == OrderState.Working || order.OrderState == OrderState.Accepted);
}

/// <summary>Returns the account to search for orders: follower account if applicable, else master account.</summary>
private Account ResolveSearchAccount(PositionInfo pos)
{
    return (pos.IsFollower && pos.ExecutingAccount != null) ? pos.ExecutingAccount : Account;
}
```

---

## Parent Method After Extraction

```csharp
private Order FindTargetOrderForPosition(
    PositionInfo pos,
    string entryName,
    int targetNum,
    out string notFoundReason
)
{
    notFoundReason = null;

    if (!pos.EntryFilled)
    {
        notFoundReason = $"[V14] MoveSpecificTarget T{targetNum}: Skipping {entryName} - entry not filled";
        return null;
    }

    string targetOrderName = $"T{targetNum}_{entryName}";
    var searchAcct = ResolveSearchAccount(pos);  // extracted

    foreach (Order order in searchAcct.Orders)
    {
        if (IsMatchingWorkingOrder(order, targetOrderName))  // extracted
        {
            return order;
        }
    }

    notFoundReason =
        $"[V14] MoveSpecificTarget T{targetNum}: No working order found for {entryName} (may already be filled)";
    return null;
}
```

- **Remaining logic:** Entry-filled guard (early return), target order name construction, account resolution call, foreach loop with single matching call, not-found diagnostic assignment.
- **Projected CYC:** **4** — base(1) + EntryFilled guard if(1) + foreach(1) + IsMatchingWorkingOrder if(1) = 4

---

## max_cyc_projected: 6
## extraction_count: 2

---

## Jane Street Alignment

| Mandate | Status | Evidence |
|---|---|---|
| CYC<=8 achieved | **YES** | Parent=4, IsMatchingWorkingOrder=6, ResolveSearchAccount=3; max=6 |
| Single-responsibility per helper | **YES** | `IsMatchingWorkingOrder` answers only "is this the right working order?"; `ResolveSearchAccount` answers only "which account to search?" |
| Lock-free/Actor pattern preserved | **YES** | Method is read-only query; no state mutations, no lock() blocks exist or introduced |
| Illegal states unrepresentable | **YES** | `ResolveSearchAccount` always returns a non-null `Account`; `IsMatchingWorkingOrder` fully encapsulates null-safety guard preventing partial-match states |
| Zero-allocation hot path | **YES** | `bool` and `Account` returns; no boxing, no new heap allocations in extracted helpers |
| Extract guard clauses | **YES** | EntryFilled early-return preserved as-is (CYC 1, self-documenting; extraction not warranted) |
| Extract loop body | **YES** | foreach body reduced to single `if(IsMatchingWorkingOrder(...)) return order;` — maximally simple |
| DRY / duplication elimination | **BONUS** | `ResolveSearchAccount` also resolves the 3-site account-routing duplication at lines 204, 446, 507 in the same file |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Epic** | EPIC-W7-040 |
| **Source Method** | `FindTargetOrderForPosition` |
| **Source File** | `src/V12_002.Trailing.Breakeven.cs` |
| **Original CYC** | 10 |
| **Projected Max CYC** | 6 |
| **Extraction Count** | 2 |
| **jcodemunch tools called** | resolve_repo, search_symbols, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Scope Boundary Input** | `docs/brain/EPIC-W7-040/01-scope-boundary.md` |
| **boundary_verdict** | PASS (from Phase 1.5) |
