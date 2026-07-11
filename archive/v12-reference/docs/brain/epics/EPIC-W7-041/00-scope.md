# EPIC-W7-041 — Phase 1: Scope Definition

## Single Method in Scope

| Field | Value |
|---|---|
| **Method** | `AuditStopQuantityAndPrint` |
| **Source File** | `src/V12_002.Orders.Management.cs` |
| **Lines** | 90–174 |
| **Visibility** | `private` |
| **Signature** | `private void AuditStopQuantityAndPrint(string entryName, PositionInfo pos, Order stopOrder, double validatedStopPrice, int nonRunnerLimitQty, int runnerQty, bool isFollowerSubmit)` |

This epic addresses exactly one **single method**: `AuditStopQuantityAndPrint`. The scope
boundary is drawn at the method declaration in `src/V12_002.Orders.Management.cs` and
does not extend to any caller, callee, or sibling method.

## Cyclomatic Complexity

| Metric | Value |
|---|---|
| **Current CYC** | 8 |
| **Target CYC** | ≤ 5 (post-extraction; hard ceiling ≤ 8 per V12.23 policy) |
| **CYC Source** | `docs/brain/complexity_audit_full.txt` line 334; confirmed via full branch decomposition in `00-hotspots.md` |

The current CYC of **8** sits exactly at the V12.23 policy ceiling. The refactor must
reduce it to **≤ 5** via two planned extractions (see Phase 2), while keeping each new
helper method individually below the ceiling.

## Callers

| # | Caller | File | Line(s) |
|---|---|---|---|
| 1 | `SubmitBracketOrders` | `src/V12_002.Orders.Management.cs` | 74 |

**Callers count: 1 direct caller.**

`SubmitBracketOrders` is itself reached from two call sites in
`src/V12_002.Orders.Callbacks.cs` (lines 332 and 348), both on the order-fill callback
path (`OnOrderUpdate` → fill-callback → `SubmitBracketOrders` → `AuditStopQuantityAndPrint`).
Those upstream sites are outside the scope boundary and require no modification.

## Scope Boundary

The **scope boundary** for this epic is strictly the body of the single method
`AuditStopQuantityAndPrint` (lines 90–174, `src/V12_002.Orders.Management.cs`).
All work — branch decomposition, extraction, and post-refactor validation — must remain
within or directly attached to that boundary. No caller signature changes, no
cross-file edits outside of newly extracted private helpers in the same partial class,
and no changes to shared state definitions are permitted under this scope.

## Why Other Methods Are NOT in Scope (V12.23)

Per **V12.23 policy**, each refactor epic targets exactly one high-complexity method
identified in the Wave 7 hotspot audit. The following methods are explicitly excluded:

| Method | Reason Excluded |
|---|---|
| `SubmitBracketOrders` | Direct caller; CYC within policy ceiling; excluded to prevent cascading scope creep |
| `OnOrderUpdate` | Upstream orchestrator; changes there touch the fill-callback path across multiple files |
| `GetTargetContracts` | Pure helper; CYC = 1; no complexity reduction value |
| `IsRunnerTarget` | Pure helper; CYC = 1; no complexity reduction value |
| `GetTargetPrice` | Pure helper; CYC = 1; no complexity reduction value |
| All other methods in `V12_002.Orders.Management.cs` | Not flagged in Wave 7 hotspot audit; modifying them violates the single-method scope rule |

V12.23 mandates that scope expansion requires a separate epic with its own hotspot
justification. No method outside the scope boundary listed above will be modified,
renamed, or have its signature altered during this epic.

## Planned Extractions (Summary)

Two extractions are planned to reach CYC ≤ 5:

| # | New Method | CYC Removed from Parent | Rationale |
|---|---|---|---|
| 1 | `AppendTargetSlotToMessage(StringBuilder, PositionInfo, int)` | −2 | Moves the 5-slot loop body (zero-fill skip + runner/limit branch) into a dedicated helper |
| 2 | `AssertTargetContractSum(string, int, int)` | −1 | Wraps the distribution-sum guard + diagnostic print into a named assertion |

The compound null-and-quantity stop guard (`if (stopOrder != null && stopOrder.Quantity != pos.TotalContracts)`)
is intentionally left in the parent method; it follows the nil-safe `&&` convention
used consistently across the Orders subsystem and its extraction would not produce a
reusable predicate.

## Risk Assessment

| Dimension | Detail |
|---|---|
| **Side-effects** | `Print()` diagnostic output only; no order mutations |
| **State written** | `pos.CurrentStopPrice` — single field, written before any branch |
| **Threading** | Strategy thread only (NinjaScript order-fill callback chain) |
| **Hot-path membership** | Bracket submission path — once per entry fill, not per tick |
| **Risk level** | LOW-MEDIUM |

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase1-scope |
| **Wave** | 7 |
| **Epic** | EPIC-W7-041 |
| **Phase** | 1 — Scope Definition |
| **Single Method** | `AuditStopQuantityAndPrint` |
| **Source File** | `src/V12_002.Orders.Management.cs` |
| **Current CYC** | 8 |
| **Target CYC** | ≤ 5 |
| **Callers Count** | 1 |
| **Output** | `docs/brain/EPIC-W7-041/00-scope.md` |
