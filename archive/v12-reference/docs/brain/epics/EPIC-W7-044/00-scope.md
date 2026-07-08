# EPIC-W7-044 — Phase 1: Scope Definition

## Method in Scope

This phase targets a **single method** only:

| Field            | Value                                          |
|------------------|------------------------------------------------|
| **Method**       | `SymmetryGuardCascadeFollowerCleanup`          |
| **File**         | `src/V12_002.Symmetry.Replace.cs` (lines 198–243) |
| **Class**        | `V12_002` (partial)                            |
| **Current CYC**  | 11                                             |
| **Target CYC**   | ≤ 8                                            |
| **Wave / Phase** | Wave 7 / Phase 1                               |

---

## Scope Boundary

The **scope boundary** is drawn tightly around `SymmetryGuardCascadeFollowerCleanup` in
`src/V12_002.Symmetry.Replace.cs`. No other method, class, or file falls inside this phase's
change surface. All work that leaves this boundary — including the direct caller, downstream
helpers, or concurrent-map infrastructure — is treated as read-only context for the purposes
of Phase 1.

---

## Callers

A `grep` of `src/` for the symbol `SymmetryGuardCascadeFollowerCleanup` produced **3 hits**:

| # | File | Line | Nature |
|---|------|------|--------|
| 1 | `src/V12_002.Symmetry.Replace.cs` | 198 | **Definition** |
| 2 | `src/V12_002.Orders.Callbacks.cs` | 771 | **Direct call-site** (sole caller) |
| 3 | `src/V12_002.Orders.Callbacks.AccountOrders.cs` | 693 | Comment reference only — not a call |

**Callers count (runtime call-sites): 1**

The single caller is
[`HandleOrderCancelled_RollbackUnfilledEntry`](src/V12_002.Orders.Callbacks.cs:771).
It fires under the guard `EnableSIMA && !kvp.Value.IsFollower` when a master entry order
receives a confirmed-cancelled event.

---

## Why Other Methods Are NOT in Scope

Per rule **V12.23**, every phase must operate on the minimum necessary surface.
The following methods are explicitly **out of scope** for Phase 1:

- **`HandleOrderCancelled_RollbackUnfilledEntry`** (`src/V12_002.Orders.Callbacks.cs:756`) —
  the sole caller. Its logic is read as context only; reshaping it is deferred to a later phase
  if CYC remains elevated after the extraction plan is applied.

- **`CancelOrderSafe`** (`src/V12_002.Orders.CancelGateway.cs:18`) — downstream gateway
  with 23 call-sites across 9 files. Touching it would breach V12.23's minimum-surface rule
  and risk regressions across an unrelated blast radius.

- **`HandleMatchedFollower_DeltaRollback`** (`src/V12_002.Orders.Callbacks.AccountOrders.cs:691`) —
  deferred rollback helper. It participates in the two-phase cancel/rollback FSM ordering
  guarantee introduced in Build 960 A2-3. Modifying it here could violate that ordering
  guarantee without a dedicated audit phase.

- **`SymmetryGuardReplaceExistingFollowerTarget`** and all other `SymmetryGuard*` siblings** —
  in-file but not the hotspot. Refactoring them concurrently would produce an indeterminate
  combined CYC delta and obscure whether the target method's reduction was actually achieved.

- **`RollbackExpectedPosition` / `CleanupPosition`** — called by the caller *after* this
  method returns. Their execution order relative to this method is a two-phase FSM guarantee;
  they must not be moved or duplicated as a side-effect of any extraction applied here.

All exclusions satisfy V12.23: changes are limited to the **single method** identified in the
hotspot analysis, and every out-of-scope symbol is preserved unmodified.

---

## Complexity Reduction Plan (summary)

Three extractions identified in Phase 0 bring CYC 11 → estimated CYC **4**:

1. `TryResolveCascadeContext` — encapsulates the two-dictionary lookup (lines 200–206).
2. `IsFollowerEntryLive` — encapsulates the three-state `OrderState` predicate (lines 225–229).
3. `TryCancelFollowerEntry` — encapsulates the null-guard cascade + conditional cancel body
   (lines 218–241), reducing the `foreach` body to a single call.

The target threshold of **CYC ≤ 8** is satisfied by any subset of these extractions that
removes at least 3 branches. All three together exceed the target comfortably.

---

## Constraints Carried Forward to Phase 2

- The two-phase cancel/rollback ordering guarantee (cancel in `SymmetryGuardCascadeFollowerCleanup`,
  rollback deferred to `HandleMatchedFollower_DeltaRollback`) **must be preserved exactly**.
- The ADR-019 immutable-snapshot read pattern on `symmetryMasterEntryToDispatch`,
  `symmetryDispatchById`, and `ctx.Followers` must not be altered.
- Extracted helpers must be `private` and placed in the same partial class (`V12_002`)
  to avoid changing visibility or assembly surface.

---

## Agent Tracking

```
EPIC:          EPIC-W7-044
WAVE:          7
PHASE:         1
STATUS:        completed
OUTPUT:        docs/brain/EPIC-W7-044/00-scope.md
AGENT NAME:    v12-phase1-scope
CYC_CURRENT:   11
CYC_TARGET:    <=8
CALLERS_COUNT: 1
SCOPE:         single method — SymmetryGuardCascadeFollowerCleanup
SCOPE_RULE:    V12.23 (minimum surface)
```
