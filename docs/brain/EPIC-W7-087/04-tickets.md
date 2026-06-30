# Phase 4: Implementation Tickets — EPIC-W7-087

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T02:30:00Z
**Input:** docs/brain/EPIC-W7-087/02-architecture-plan.md + docs/brain/EPIC-W7-087/03-audit-report.md

---

## Method Under Extraction

- **Method:** `AuditFleet_CheckWorkingStop`
- **Source File:** `src/V12_002.REAPER.Audit.cs`
- **Original CYC:** 0 (branchless LINQ predicate — no control flow decisions)
- **Line Range:** 517–527
- **Extraction Count:** 1
- **ticket_count:** 2
- **projected_parent_cyc_after_all:** 1

---

## Sequential Thinking Summary

**Thought 1 — Context Assessment:** CYC=0 method; 1 structural extraction identified by architecture plan. Helper `IsWorkingStopOrderForInstrument` extracts the anonymous 4-condition LINQ predicate into a named, testable method.

**Thought 2 — Ticket Scope:** 2 tickets required — T-01 for the extraction itself, T-02 for xUnit test coverage of the extracted helper per DNA check #5.

**Thought 3 — Detail Precision:** T-01 moves lines 522–526 (predicate body) into a new method. Parent cyc reduction is structural (0→1 by sequential steps). Helper projected cyc=5.

**Thought 4 — Final Plan:** ticket_count=2, projected_parent_cyc_after_all=1. All DNA checks satisfied.

---

## Tickets

---

### Ticket T-01 — Extraction: `IsWorkingStopOrderForInstrument`

| Field | Value |
|---|---|
| **ticket_id** | T-01 |
| **type** | extraction |
| **helper_name** | `IsWorkingStopOrderForInstrument(Order o)` |
| **concern** | Single-responsibility decomposition of the anonymous 4-condition stop-order predicate in `AuditFleet_CheckWorkingStop`. Extracts the compound LINQ lambda into a named private helper for readability, reusability, and testability. |
| **source_file** | `src/V12_002.REAPER.Audit.cs` |
| **parent_method** | `AuditFleet_CheckWorkingStop` |
| **parent_lines** | 517–527 |
| **lines_to_move** | 522–526 (the 4-condition boolean predicate body currently inlined as `o => o.Instrument?.FullName == ...`) |
| **cyc_reduction** | 0→1 on parent (structural: snapshot + delegate, no branches removed; net change is decomposition, not branch elimination) |
| **projected_helper_cyc** | 5 |
| **projected_parent_cyc** | 1 |

#### Implementation

**Before (lines 517–527):**
```csharp
private bool AuditFleet_CheckWorkingStop(Account acct)
{
    // Build 1108.003 [D3]: Snapshot broker orders before iteration.
    var orders = acct.Orders.ToArray();
    return orders.Any(o =>
        o.Instrument?.FullName == Instrument?.FullName
        && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
        && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
        && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover));
}
```

**After — Parent (projected CYC=1):**
```csharp
private bool AuditFleet_CheckWorkingStop(Account acct)
{
    // Build 1108.003 [D3]: Snapshot broker orders before iteration.
    var orders = acct.Orders.ToArray();
    return orders.Any(IsWorkingStopOrderForInstrument);
}
```

**After — Extracted Helper (projected CYC=5):**
```csharp
private bool IsWorkingStopOrderForInstrument(Order o)
{
    return o.Instrument?.FullName == Instrument?.FullName
        && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
        && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
        && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover);
}
```

#### Jane Street Constraints
- Lock-free: YES — pure read predicate, no state mutations
- ASCII-only: YES — all identifiers and string literals ASCII-safe
- Scope: Bounded to `src/V12_002.REAPER.Audit.cs` only (V12.23 respected)
- `ToArray()` retained: YES — Build 1108.003 [D3] thread-safety annotation preserved
- max_cyc_projected: 5 ≤ 8 ✅

#### Acceptance Criteria
1. `IsWorkingStopOrderForInstrument` exists as a `private bool` method in `src/V12_002.REAPER.Audit.cs`
2. `AuditFleet_CheckWorkingStop` body is exactly: ToArray snapshot + `return orders.Any(IsWorkingStopOrderForInstrument);`
3. Build passes (`dotnet build`): zero errors, zero warnings
4. `grep -n "lock(" src/V12_002.REAPER.Audit.cs` returns 0 matches
5. `dotnet csharpier check src/` reports 0 issues

---

### Ticket T-02 — Verification: xUnit Tests for `IsWorkingStopOrderForInstrument`

| Field | Value |
|---|---|
| **ticket_id** | T-02 |
| **type** | verification |
| **helper_name** | `IsWorkingStopOrderForInstrument(Order o)` |
| **concern** | xUnit `[Fact]` test coverage for the extracted predicate helper. Verifies all 4 boolean conditions (instrument match, order state, order type, order action) using `Assert.True`/`Assert.False`. Required by DNA check #5 (xUnit mandate). |
| **source_file** | `tests/V12_Performance.Tests/` (new test class or extension of existing audit tests) |
| **lines_to_move** | N/A — new test file |
| **cyc_reduction** | N/A — verification ticket |
| **projected_helper_cyc** | N/A |
| **projected_parent_cyc** | N/A |

#### Test Cases Required
1. Returns `true` when all 4 conditions satisfied (nominal working stop sell order matching instrument)
2. Returns `false` when instrument name does not match
3. Returns `false` when `OrderState` is neither `Working` nor `Accepted`
4. Returns `false` when `OrderType` is neither `StopMarket` nor `StopLimit`
5. Returns `false` when `OrderAction` is neither `Sell` nor `BuyToCover`

#### Acceptance Criteria
1. All 5 `[Fact]` tests pass via `dotnet test`
2. Only xUnit attributes used — zero NUnit/MSTest references
3. Test class targets `IsWorkingStopOrderForInstrument` directly
4. Build passes with zero errors

---

## projected_parent_cyc_after_all: 1

---

## Ticket Summary

| ticket_id | type | helper_name | lines_to_move | cyc_reduction | projected_helper_cyc |
|---|---|---|---|---|---|
| T-01 | extraction | `IsWorkingStopOrderForInstrument(Order o)` | 522–526 | 0→1 (structural) | 5 |
| T-02 | verification | `IsWorkingStopOrderForInstrument(Order o)` | N/A | N/A | N/A |

**ticket_count:** 2
**projected_parent_cyc_after_all:** 1
**max_cyc_projected:** 5

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T02:30:00Z |
| **Wave** | 7 |
| **Phase** | 4 |
| **jcodemunch tools called** | `resolve_repo` |
| **sequential-thinking calls** | 4 |
| **MCP resolve_repo** | antigravityos187-sketch/universal-or-strategy (5147 symbols, 2000 files) |
| **DNA verdict (Phase 3)** | PASS (0 violations) |
| **Extraction strategy** | Structural decomposition (CYC=0 → named predicate helper) |
