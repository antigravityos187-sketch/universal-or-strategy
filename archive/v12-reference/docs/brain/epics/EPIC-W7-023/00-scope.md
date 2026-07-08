# EPIC-W7-023 — Phase 1: Scope Definition

## Method in Scope

This epic targets a **single method** for cyclomatic complexity reduction. The scope boundary is hard and immutable: no other pre-existing method may be modified during Phase 2 implementation.

| Field               | Value                                                             |
|---------------------|-------------------------------------------------------------------|
| **Method**          | `HandleFlatPositionUpdate`                                        |
| **Current CYC**     | 19                                                                |
| **Target CYC**      | ≤ 8                                                               |
| **File**            | `src/V12_002.Orders.Callbacks.Execution.cs`                       |
| **Lines (current)** | 69 – 128                                                          |
| **Access**          | `private`                                                         |
| **Callers count**   | 1 — `ProcessOnPositionUpdate` (same file, line 55)                |

---

## Scope Boundary

The **scope boundary** is drawn at the entry point of `HandleFlatPositionUpdate`. Anything inside that method body is in scope for structural refactoring. Anything outside — callers, downstream callees, guard helpers, SIMA utilities — is strictly out of scope and must not be touched.

This is a **single method** refactor. The contract: one method enters Phase 2 with CYC 19; that same method (plus new private helpers it exclusively owns) exits Phase 2 with CYC ≤ 8.

> All extracted helpers created during Phase 2 will be `private`, scoped to the same `partial class V12_002`, and owned solely by `HandleFlatPositionUpdate`. They are not in scope as independent modification targets — they exist only to absorb the branch complexity extracted from the parent method.

---

## Caller Map

Confirmed via source inspection of `src/V12_002.Orders.Callbacks.Execution.cs` and full-codebase grep. The method is `private` and has exactly **1 caller** in the entire repository.

```
OnPositionUpdate  (override, line 37)
  └─ Enqueue(ctx => ctx.ProcessOnPositionUpdate(...))
        └─ ProcessOnPositionUpdate  (line 50)
              └─ HandleFlatPositionUpdate  ← SCOPE BOUNDARY  (line 55)
                    [new private helpers will be called from here only]
```

- **Total external callers:** 1 (`ProcessOnPositionUpdate`, line 55, same file)
- **Cross-file callers:** 0 — grep across all `.cs` files confirms no other file invokes `HandleFlatPositionUpdate`
- **Full entry chain:** `OnPositionUpdate` → `Enqueue` → `ProcessOnPositionUpdate` → `HandleFlatPositionUpdate`
- **Trigger condition:** only fires when `marketPosition == MarketPosition.Flat`

---

## Why Other Methods Are NOT in Scope (V12.23 Scope Rule)

The V12.23 single-method scope rule applies to all Wave 7 epics. Its purpose is to constrain the blast radius of each refactor to the minimum possible surface area, preventing cascading changes that could corrupt broker-facing state in a live-trading system.

`HandleFlatPositionUpdate` writes to `expectedPositions` (read by 20+ sites), triggers `ReconcileOrphanedOrders`, cancels live bracket orders via `CancelOrphanedOrdersForPosition`, and calls `CleanupPosition` which tears down the full position state. Any change to a method *other* than the target could silently alter flat-position handling behaviour across all those downstream sites — introducing reverse-position risk, orphaned orders, or false skip-logic suppression.

The V12.23 rule therefore prohibits touching:

| Symbol | Category | Reason not in scope |
|--------|----------|---------------------|
| `ProcessOnPositionUpdate` | Direct caller | Caller of the target — not modified under the single-method rule |
| `OnPositionUpdate` | Indirect caller | NinjaTrader override two levels up — not modified |
| `ReconcileOrphanedOrders` | Downstream callee | Called by target; logic unchanged |
| `CancelOrphanedOrdersForPosition` | Downstream callee | Broker-side-effecting; not touched |
| `CleanupPosition` | Downstream callee | Full position teardown; not touched |
| `HasPendingEntryOrderForAccount` | Guard helper | Read-only predicate; not touched |
| `HasUnfilledPositionForAccount` | Guard helper | Read-only predicate; not touched |
| `IsDispatchSyncPending` | SIMA sync helper | Read-only predicate; not touched |
| `SetExpectedPositionLocked` | SIMA write helper | State-mutation utility; not touched |
| `ExpKey` | Key builder | Pure utility; not touched |

No existing method other than `HandleFlatPositionUpdate` will have a single line changed during Phase 2.

---

## CYC Budget Projection

| State | CYC |
|-------|-----|
| Current (`HandleFlatPositionUpdate`, lines 69–128) | **19** |
| After extracting H-14 skip guard (`ShouldSkipFlatReset`) | −4 → 15 |
| After extracting orphan scan loop (`BuildOrphanedPositionList`) | −3 → 12 (residual in parent ≈ 8) |
| After hoisting `Count == 0` restart guard (`HandleExternalRestartIfFlat`) | −1 → ≈ 7 |
| **Projected post-refactor (parent method only)** | **≤ 8** ✅ |

The three extraction targets are each simple enough (2–4 branches each) that they will not themselves become new hotspots. The scope boundary prevents any CYC introduced into new helpers from propagating back to methods already in production.

---

## Sequential Scope Reasoning

**Thought 1 — Blast-radius containment:**
`HandleFlatPositionUpdate` has exactly one caller (`ProcessOnPositionUpdate`) and zero cross-file references. All extracted helpers will be `private`, making the extraction invisible to all 29+ files that share `expectedPositions` and all 41 files that touch `activePositions`. The blast radius of the extraction is zero from the caller's perspective.

**Thought 2 — Single-method rule enforcement:**
The scope boundary is the entry point of `HandleFlatPositionUpdate`. Callers above it, callees below it, and guard helpers beside it are all frozen. The refactor moves branch logic *out of* the method body into new `private` helpers — it does not move code across method boundaries that already exist.

**Thought 3 — CYC budget validation:**
Starting CYC 19, three extractions remove ≈ 8 branch points from the parent. Residual: 1 baseline + 1 null-guard + ~3 call sites + 1 cleanup print guard = ≈ 6–7. Target ≤ 8 is met with margin. No single extracted helper exceeds CYC 4.

---

## Agent Tracking

| Field            | Value                                             |
|------------------|---------------------------------------------------|
| **Agent Name**   | v12-phase1-scope                                  |
| **Epic**         | EPIC-W7-023                                       |
| **Wave**         | 7                                                 |
| **Phase**        | 1 — Scope Definition                              |
| **Status**       | completed                                         |
| **Output**       | `docs/brain/EPIC-W7-023/00-scope.md`              |
| **Method**       | `HandleFlatPositionUpdate`                        |
| **File**         | `src/V12_002.Orders.Callbacks.Execution.cs:69`    |
| **CYC Current**  | 19                                                |
| **CYC Target**   | ≤ 8                                               |
| **Callers Count**| 1                                                 |
| **Scope Rule**   | V12.23 — single method, scope boundary enforced   |
