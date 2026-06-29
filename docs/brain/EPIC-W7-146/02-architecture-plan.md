# EPIC-W7-146 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T00:35:02Z
**Input:** docs/brain/EPIC-W7-146/01-scope-boundary.md

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-146 |
| **Bobcoins Used** | 1.0 |

---

## Target Method Table

| Method | File | Line | CYC Baseline | CYC Target |
|---|---|---|---|---|
| `CancelOrphanedTargets` | `src/V12_002.UI.Compliance.cs` | 553 | 13 | 7 |

---

## Complexity Drivers

The method spans lines 553–578 (26 LOC) and accumulates CYC=13 from these sources:

| Driver | Branch Count | Notes |
|---|---|---|
| Base | +1 | Method entry |
| `foreach (Order o in account.Orders.ToArray())` | +1 | Loop branch |
| `o == null \|\| o.Instrument?.FullName != Instrument?.FullName` | +2 | Two OR conditions in null/instrument guard |
| `o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted` | +2 | Two negated-AND conditions in state guard |
| `o.Name != null` | +1 | Null guard before prefix check |
| `o.Name.StartsWith("T1_") \|\| T2_ \|\| T3_ \|\| T4_ \|\| T5_` | +5 | 5-way OR prefix chain |
| **Total** | **13** | |

**Dominant driver:** The 5-way OR prefix chain (`T1_`–`T5_`) contributes +5 CYC and is the primary extraction target.

---

## Extraction Plan

| Helper Name | Responsibility | CYC Projected | Modifier |
|---|---|---|---|
| `IsTargetOrderName(string name)` | Returns true if name matches any of the T1_–T5_ target prefixes | 6 | `private`, `[MethodImpl(AggressiveInlining)]` |

### Resulting Parent Method

After extracting `IsTargetOrderName`, `CancelOrphanedTargets` becomes:

```csharp
private int CancelOrphanedTargets(Account account)
{
    int cancelledTargets = 0;
    foreach (Order o in account.Orders.ToArray())               // +1
    {
        if (o == null || o.Instrument?.FullName != Instrument?.FullName)  // +2
            continue;
        if (o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted) // +2
            continue;
        if (o.Name != null && IsTargetOrderName(o.Name))        // +1
        {
            CancelOrderOnAccount(o, account);
            cancelledTargets++;
        }
    }
    return cancelledTargets;
}
```

### New Helper Method

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool IsTargetOrderName(string name)
{
    return name.StartsWith("T1_")   // +1
        || name.StartsWith("T2_")   // +1
        || name.StartsWith("T3_")   // +1
        || name.StartsWith("T4_")   // +1
        || name.StartsWith("T5_");  // +1
}
```

---

## Max CYC Projected Table

| Method | CYC Before | CYC After | Status |
|---|---|---|---|
| `CancelOrphanedTargets` | 13 | 7 | PASS (<=8) |
| `IsTargetOrderName` | N/A (new) | 6 | PASS (<=8) |
| **Max CYC Projected** | — | **7** | **PASS** |

All helpers CYC <= 8. Jane Street threshold satisfied.

---

## Jane Street KB Compliance Table

| Rule Source | Rule | Compliance |
|---|---|---|
| carl_cook | Zero-alloc hot path | PASS — no new allocations; `.ToArray()` already present in original |
| carl_cook | Extract cold logging out-of-line | PASS — no logging in this method |
| carl_cook | `AggressiveInlining` on hot path | PASS — `IsTargetOrderName` marked `[MethodImpl(AggressiveInlining)]` |
| carl_cook | Avoid LINQ | PASS — no LINQ used; `.ToArray()` on Orders is existing, not new |
| gjengset | No new `lock()` blocks | PASS — no locking introduced |
| gjengset | `volatile` + `Thread.MemoryBarrier` where needed | PASS — read-only logic, no shared state mutation |
| gjengset | 64-byte cache line alignment | PASS — no new fields introduced |
| trading_billions | Single responsibility per helper | PASS — `IsTargetOrderName` handles prefix classification only |
| trading_billions | Defense in depth | PASS — null guard preserved before `IsTargetOrderName` call |
| trading_billions | Each helper CYC <= 8 | PASS — `IsTargetOrderName` CYC=6, `CancelOrphanedTargets` CYC=7 |
| trading_billions | Rate-limit circuit breaker | N/A — not applicable to this extraction |

---

## MCP Evidence

### Symbol Source (jCodemunch)

- **Symbol ID:** `src/V12_002.UI.Compliance.cs::V12_002.CancelOrphanedTargets#method`
- **File:** `src/V12_002.UI.Compliance.cs`
- **Lines:** 553–578 (26 LOC)
- **Signature:** `private int CancelOrphanedTargets(Account account)`
- **Source confirmed via:** `get_symbol_source`

### Call Hierarchy (jCodemunch — callers, depth=1)

| Caller | File | Line | Resolution |
|---|---|---|---|
| `HandleFleetStopFill` | `src/V12_002.UI.Compliance.cs` | 519 | `ast_resolved` |

**Caller count: 1** — Caller signature unchanged; unaffected by extraction.

### Dependency Graph (jCodemunch — imports, depth=1)

- `src/V12_002.UI.Compliance.cs` has **0 import edges** at depth=1 in the indexed graph.
- Blast radius confined to single file. No cross-file dependency changes required.

### Repo Resolution

- **Repo:** `antigravityos187-sketch/universal-or-strategy`
- **Symbol count:** 5,147 | **File count:** 2,000
- **Index status:** loadable (fresh)

---

## Sequential Thinking Evidence

| Thought | Summary |
|---|---|
| 1 (probe) | Identified CancelOrphanedTargets CYC=13; primary driver is 5-way OR prefix chain; extraction of IsTargetOrderName yields parent CYC=7, helper CYC=6 |
| 2 (complexity drivers) | Decomposed all 6 CYC contributors: base(1)+foreach(1)+null/instrument guard(2)+state guard(2)+name null(1)+5-way OR(5)=13 |
| 3 (extraction strategy) | Confirmed IsTargetOrderName extraction: CYC=6; parent after = CYC=7; AggressiveInlining applied; no LINQ, no locks, no allocs |
| 4 (CYC validation) | Verified both methods <=8; max_cyc_projected=7; all Jane Street KB rules satisfied |

---

## Scope Boundary Compliance

- **boundary_verdict from Phase 1.5:** PASS
- **Scope:** `CancelOrphanedTargets` + 1 new extracted helper (`IsTargetOrderName`)
- **V12.23 No Scope Creep:** ONE EPIC = ONE CONCERN — only this method and its extracted helper are modified
- **Caller contract:** Unchanged — `HandleFleetStopFill` call site requires no modification
- **File scope:** `src/V12_002.UI.Compliance.cs` only (same partial class, private helper)
