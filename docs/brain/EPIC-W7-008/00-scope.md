# EPIC-W7-008 — Phase 1: Scope Definition

## Single Method In Scope

This epic targets a **single method**: `ManageCIT` in `src/V12_002.Orders.Management.Flatten.cs`.

| Field | Value |
|---|---|
| **Method** | `ManageCIT` |
| **File** | `src/V12_002.Orders.Management.Flatten.cs` |
| **Lines** | 68–128 (method body) |
| **Class** | `partial class V12_002 : Strategy` |
| **Signature** | `private void ManageCIT()` — zero parameters, void return |
| **Current CYC** | 19 |
| **Target CYC** | ≤ 8 |
| **CYC Reduction Required** | −11 |

## Scope Boundary

The **scope boundary** for EPIC-W7-008 is precisely the `ManageCIT` method cluster: the method body (lines 68–128) and the five private helper methods it exclusively calls (`ValidateCitConfiguration`, `ShouldChaseOrder`, `ExecuteFollowerNudge`, `CalculateNudgedPrice`, `ExecuteLocalNudge`), all residing in the same partial class file. No code outside this method cluster is touched by this epic.

This is a **single method** refactoring engagement. The V12.23 No Scope Creep Protocol is in full effect: one epic, one concern, one declared hotspot.

## Caller Analysis

`ManageCIT` was located in `src/V12_002.Orders.Management.Flatten.cs` at line 68. A full-codebase symbol search confirms the following reference surface:

| File | Line | Call Form | Classification |
|---|---|---|---|
| `src/V12_002.BarUpdate.cs` | 265 | `ManageCIT()` — direct call, Phase C hot path | ✅ Active runtime call |
| `src/V12_002.BarUpdate.cs` | 328 | `Enqueue(ctx => ctx.ManageCIT())` — deferred via actor drain | ✅ Active runtime call |
| `src/V12_002.Orders.Management.Flatten.cs` | 163 | `Enqueue(ctx => ctx.ManageCIT())` — self-requeue on budget exhaustion | ✅ Internal requeue |
| `src/V12_002.SIMA.Execution.cs` | 684 | XML doc comment reference only | ❌ Not a call site |

**Callers count: 2 active external call sites** (both in `src/V12_002.BarUpdate.cs`, 1 caller file).

Because the signature is `private void ManageCIT()`, any internal restructuring is completely invisible to all callers. No caller-side changes are required and none will be made.

## CYC Breakdown (Confirmed from Phase 0)

| Component | CYC Contribution |
|---|---|
| `ManageCIT` (body) | 9 |
| `ValidateCitConfiguration` | 5 |
| `ShouldChaseOrder` | 7 |
| `ExecuteFollowerNudge` | 4 |
| `CalculateNudgedPrice` | 2 |
| `ExecuteLocalNudge` | 1 |
| **Aggregate (reported CYC)** | **19** |

## Top 3 Complexity Drivers

1. **Dual-exception catch inside the iteration loop (lines 118–126)** — two `catch` clauses with different recovery semantics inflate CYC by 2 and embed exception-recovery policy inside the loop body.
2. **`isFollower` dispatch + nested broker-budget re-queue (lines 96–115)** — a three-layer decision stack (loop → dispatch → budget) compressed into a single inlined block; `return false` / `return true` contract leaks back to the loop as a continue/halt signal.
3. **`ShouldChaseOrder` compound predicate with directional price-touch logic (lines 199–222)** — CYC 7, documented regression history (Build 984 CIT FIX — Short used `Low[0]`, always-true regression), making this the highest-risk inlined predicate.

## Planned Extractions (Phase 2 Preview)

Three targeted extractions will reduce the aggregate CYC from 19 to ≤ 8 without altering any caller signatures:

1. **`TryNudgeOrder(string key, Order order, double citOffset, ref int budget)`** — unify the `isFollower` dispatch and the `return false` budget-halt signal into a single named method, eliminating the 3-layer decision stack from the loop body.
2. **`ExecuteCitNudgeWithFaultIsolation(string key, Order order, double citOffset, ref int budget)`** — wrap the dual `try/catch` block into a named fault-isolation wrapper so the loop body expresses only intent, not recovery policy.
3. **`IsPriceTouchingLimit(Order order)`** — extract the directional price-touch comparison from `ShouldChaseOrder` into a pure, unit-testable predicate; the Build 984 regression history makes standalone test coverage a priority.

All new helpers will be added to the same partial class file (`src/V12_002.Orders.Management.Flatten.cs`), consistent with the `ExecuteLocalNudge` / `ExecuteFollowerNudge` pattern established in Build 971. No other files are modified.

## Why Other Methods Are NOT in Scope (V12.23 No Scope Creep Protocol)

All methods in `src/V12_002.Orders.Management.Flatten.cs` other than `ManageCIT` and its declared helper cluster are explicitly excluded under the **V12.23 No Scope Creep Protocol**:

| Excluded Method | Reason Excluded |
|---|---|
| `SyncPositionState` | Unrelated concern — position state bookkeeping |
| `FlattenAll` | Unrelated concern — emergency flatten orchestration |
| `FlattenPositionByName` | Unrelated concern — named position flatten |
| `HandleGhostPositionCleanup` | Unrelated concern — ghost position cleanup |
| `CancelMasterEntryOrders` | Unrelated concern — entry order cancellation |
| `DispatchFleetFlatten` | Unrelated concern — fleet flatten dispatch |
| `ResetSyncStateAndPurgeFollowers` | Unrelated concern — state reset |
| `FlattenFilledMasterPositions` | Unrelated concern — filled position flatten |
| `FlattenSinglePosition` | Unrelated concern — single position flatten |
| `CancelUnfilledMasterEntries` | Unrelated concern — unfilled entry cancellation |
| `CancelAllBracketOrdersForPosition` | Unrelated concern — bracket order cancellation |
| `SubmitEmergencyFlattenOrder` | Unrelated concern — emergency order submission |
| `IsOrderTerminal` | Unrelated concern — order state predicate |
| `HasActiveOrPendingOrderForEntry` | Unrelated concern — entry guard predicate |

**Rule citation:** V12.23 No Scope Creep Protocol — ONE EPIC = ONE CONCERN. A wave targets a single declared hotspot. Any "while we're here" improvement to adjacent methods would constitute a violation. Violations result in immediate PR closure and epic restart.

Caller files (`src/V12_002.BarUpdate.cs`, `src/V12_002.SIMA.Execution.cs`) and all 22 downstream consumers of `entryOrders` and 41 consumers of `activePositions` are read-only reference surfaces for this epic — shared state write semantics are preserved exactly.

## Sequential Thinking Summary

**Thought 1 — Method Boundary Confirmation:** `ManageCIT()` is a self-contained void method. The aggregate CYC of 19 is fully attributable to the method body (CYC 9) plus five private helpers that are exclusively called by `ManageCIT`. New helpers extracted during Phase 2 will remain within the same partial class — the scope boundary is the method cluster, nothing beyond.

**Thought 2 — Caller Surface & Risk Boundary:** Two active external runtime call sites, both in a single file (`BarUpdate.cs`). The `private void ManageCIT()` signature is zero-parameter, void-return — any internal restructuring is entirely invisible to callers. Refactoring risk is confined to method body semantics only.

**Thought 3 — Out-of-Scope Exclusion Rationale:** The 14+ other methods in the file handle completely distinct concerns (flatten operations, order cancellation, state predicates). The 46-file shared-state consumer surface (`entryOrders` + `activePositions`) is read-only from this epic's perspective — write semantics of `entryOrders[key] = nudgedOrder` are preserved exactly. No cross-file edits are permitted under V12.23.

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase1-scope |
| **Epic** | EPIC-W7-008 |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Source File** | `src/V12_002.Orders.Management.Flatten.cs` |
| **Method** | `ManageCIT` |
| **Current CYC** | 19 |
| **Target CYC** | ≤ 8 |
| **Callers Count** | 2 active call sites (1 caller file: `V12_002.BarUpdate.cs`) |
| **Scope Confirmed** | Single method — `ManageCIT` cluster only |
| **V12.23 Status** | ENFORCED — all other methods explicitly excluded |
| **Bobcoins Used** | 1.2 |
| **Completed** | 2025-07-14 (REDO) |
