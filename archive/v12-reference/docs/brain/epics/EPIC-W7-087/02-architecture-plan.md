# Phase 2: Architecture Plan — EPIC-W7-087

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T02:10:00Z
**Input:** docs/brain/EPIC-W7-087/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `AuditFleet_CheckWorkingStop`
- **Source File:** `src/V12_002.REAPER.Audit.cs`
- **Original CYC:** 0 (confirmed from 00-hotspots.md — branchless LINQ predicate, no control flow decisions)
- **Line Range:** 517–527
- **Signature:** `private bool AuditFleet_CheckWorkingStop(Account acct)`

### jcodemunch get_context_bundle result
- Symbol resolved at `src/V12_002.REAPER.Audit.cs:517–527`
- 11-line method: snapshots `acct.Orders.ToArray()`, returns `orders.Any(o => ...)` with a 4-condition compound predicate
- Conditions: instrument name match, `OrderState.Working || OrderState.Accepted`, `OrderType.StopMarket || OrderType.StopLimit`, `OrderAction.Sell || OrderAction.BuyToCover`
- Build annotation `[D3]` confirms the `ToArray()` snapshot was deliberately placed for thread safety (Build 1108.003)

### jcodemunch get_call_hierarchy result
- **Callers (depth 1):** `AuditFleet_HandleNakedPosition` (line 335, ast_resolved)
- **Callers (depth 2):** `AuditSingleFleetAccount` (line 121, ast_resolved)
- **Callees:** None — pure read predicate with no downstream calls
- **Total caller count:** 2 (1 direct, 1 indirect)

### jcodemunch get_dependency_graph result
- `src/V12_002.REAPER.Audit.cs` has **0 indexed import edges and 0 importer edges** at depth 1
- The file is self-contained in the dependency graph; cross-file blast radius is minimal
- Internal callers only; no external file depends on this file by import graph

### jcodemunch get_extraction_candidates result
- No candidates returned at `min_complexity=3, min_callers=1`
- Confirms `AuditFleet_CheckWorkingStop` has CYC=0 — below extraction threshold for complexity-driven tools
- This plan proceeds with a **structural extraction** (single-responsibility decomposition) rather than complexity-driven extraction

---

## Sequential Thinking Summary

**Thought 1 — Context Assessment:** Method has CYC=0, confirmed branchless. Primary hotspot is H1/H2 from Phase 0: identical 4-condition predicate is copy-pasted inside `AuditMaster_HandleNakedPosition`. Scope is strictly AuditFleet_CheckWorkingStop per V12.23.

**Thought 2 — Extraction Design:** No complexity reduction needed within the method. Structural extraction: pull the 4-condition predicate lambda body into a named private helper `IsWorkingStopOrderForInstrument(Order o)`. Parent becomes a thin 2-step orchestrator (snapshot + Any call).

**Thought 3 — CYC Projection:** Helper `IsWorkingStopOrderForInstrument` has 6 boolean connectives (3 `&&`, 3 `||` binary splits) → projected CYC=5. Parent after extraction: CYC=1 (sequential snapshot + return, no branches). Both ≤8.

**Thought 4 — Jane Street Alignment:** CYC<=8 trivially satisfied. Single-responsibility confirmed. Lock-free preserved (pure read predicate). ToArray() retained intentionally (Build 1108.003 [D3] thread-safety). No state mutations. Illegal states unrepresentable by pure boolean composition.

**Thought 5 — Final Verdict:** Extraction count = 1. Named helper creates a reusable, testable, self-documenting building block for the stop-order predicate. Future epic can consume `IsWorkingStopOrderForInstrument` to eliminate the H1 duplication in `AuditMaster_HandleNakedPosition` (outside this epic's scope). max_cyc_projected = 5. All Jane Street constraints satisfied.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `IsWorkingStopOrderForInstrument(Order o)` | Evaluates whether a single `Order` satisfies all four stop-detection conditions: instrument match, working/accepted state, stop order type (StopMarket or StopLimit), and sell-side action (Sell or BuyToCover). Returns `bool`. Pure predicate — no side effects, no state access. | 5 |

### Extracted Helper Signature
```csharp
private bool IsWorkingStopOrderForInstrument(Order o)
{
    return o.Instrument?.FullName == Instrument?.FullName
        && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
        && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
        && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover);
}
```

---

## Parent Method After Extraction

```csharp
private bool AuditFleet_CheckWorkingStop(Account acct)
{
    // Build 1108.003 [D3]: Snapshot broker orders before iteration.
    var orders = acct.Orders.ToArray();
    return orders.Any(IsWorkingStopOrderForInstrument);
}
```

- **Remaining logic:** Snapshot `acct.Orders` to array (thread-safe per [D3]), then delegate to `IsWorkingStopOrderForInstrument` via `Any()`. No branching.
- **Projected CYC:** 1

---

## max_cyc_projected: 5
## extraction_count: 1

---

## Jane Street Alignment

| Principle | Status | Notes |
|---|---|---|
| CYC<=8 achieved | YES | Parent=1, Helper=5 — both well within mandate |
| Single-responsibility per helper | YES | `IsWorkingStopOrderForInstrument` does exactly one thing: evaluate the 4-condition stop predicate for one order |
| Lock-free / Actor pattern preserved | YES | Pure read predicate — no state mutations, no locks, no enqueue needed |
| Illegal states unrepresentable | YES | Boolean composition — only orders satisfying all 4 conditions can return true; no invalid interim state possible |
| Zero-allocation hot path | NOTED | `ToArray()` retained intentionally per Build 1108.003 [D3] thread-safety annotation; H3 follow-up deferred per V12.23 scope constraint |
| Extract Guard Clauses | N/A | No nested if-chains present — method is already a LINQ expression |
| Replace Switch/If with Lookup | N/A | No switch/if-chain present |
| FSM Decomposition | N/A | No FSM state-handling in this method |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T02:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **MCP resolve_repo** | antigravityos187-sketch/universal-or-strategy (5147 symbols, 2000 files) |
