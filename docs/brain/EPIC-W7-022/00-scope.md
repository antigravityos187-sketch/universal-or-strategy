# EPIC-W7-022 — Phase 1: Scope Definition

## Single Method in Scope

| Field                | Value                                                          |
|---|---|
| **Method**           | `PropagateMaster_IdentifyMove`                                 |
| **File**             | `src/V12_002.Orders.Callbacks.Propagation.cs`                  |
| **Class**            | `V12_002 : Strategy` (partial)                                 |
| **Visibility**       | `private bool`                                                 |
| **Lines**            | 82–120                                                         |
| **CYC (current)**    | 9 (fallback; jcodemunch reported 0, fallback per spec = 9)     |
| **CYC (target)**     | ≤ 8                                                            |
| **Wave**             | 7                                                              |
| **Epic**             | EPIC-W7-022                                                    |

---

## Scope Boundary

This epic enforces a strict **scope boundary**: exactly one **single method** is in scope for
Phase 1 analysis and all downstream phases.

> **Only `PropagateMaster_IdentifyMove` (lines 82–120 of
> `src/V12_002.Orders.Callbacks.Propagation.cs`) is in scope.**

No other method in the file — including those in the downstream call chain, the helper methods
it delegates to, or the callers above it — falls within the scope boundary of this epic.

---

## Callers

Symbol search across the repository (`grep` over `src/`) confirms:

| Caller                    | File                                          | Line | Call Site |
|---|---|---|---|
| `PropagateMasterPriceMove` | `src/V12_002.Orders.Callbacks.Propagation.cs` | 52   | `!PropagateMaster_IdentifyMove(...)` — early-return gate |

**Callers count: 1**

`PropagateMaster_IdentifyMove` is called exclusively by `PropagateMasterPriceMove`
(defined at line 37 of the same file). There are no other call sites anywhere in the codebase.
This is consistent with the blast-radius analysis in `00-hotspots.md`, which classified it as
the sole gating condition on the entire propagation pipeline.

---

## Why Other Methods Are NOT in Scope (V12.23)

Per rule **V12.23** of the V12 SOP (single-method epic constraint), each epic in Wave 7 is
scoped to exactly one target method. The following methods, while related, are explicitly
**excluded** from this epic's scope boundary:

| Method | Reason Excluded |
|---|---|
| `PropagateMasterPriceMove` (line 37) | Direct caller — upstream; not the refactor target |
| `PropagateMaster_ResolveFollowers` | Downstream sibling called after the gate — separate concern |
| `PropagateMaster_ApplyFollowerMove` | Downstream dispatcher — separate concern |
| `PropagateMasterEntryMove` | Deep downstream FSM entry — highest CYC in file (rank 1 hotspot per `get_hotspots`), assigned its own epic |
| `PropagateFollowerEntryReplace` | Deep downstream FSM — rank 2 hotspot, assigned its own epic |
| `SubmitFollowerReplacement_RegisterState` | Deep downstream actor-pipeline closure — rank 3 hotspot, assigned its own epic |
| `ScanOrderDictionaryForMaster` | Helper delegate called by subject — pure helper, no independent CYC concern |
| `ScanTargetDictionariesForMaster` | Helper delegate called by subject — pure helper, no independent CYC concern |
| `PropagateMasterStopMove` | Downstream — separate concern |
| `PropagateMasterTargetMove` | Downstream — separate concern |

Rule V12.23 prohibits scope creep into adjacent methods even when they share the same partial
class file. Each method above either has its own epic allocated or is a stateless helper that
does not meet the CYC-threshold criterion for an independent epic. Adding any of these to the
present scope boundary would violate the single-method constraint and invalidate Phase 2
and Phase 3 outputs.

---

## Method Behaviour Summary

`PropagateMaster_IdentifyMove` is a pure classifier with no side effects, no mutations, and no
async paths. It performs three sequential dictionary scans:

1. **Entry scan** — `ScanOrderDictionaryForMaster(entryOrders, ...)` → sets `isEntryMove = true`
2. **Stop scan** — `ScanOrderDictionaryForMaster(stopOrders, ...)` → sets `isStopMove = true`
3. **Target scan** — `ScanTargetDictionariesForMaster(...)` → sets `isTargetMove = true`, populates `masterTargetNum`

Each branch is mutually exclusive (early `return true`). If no scan matches, the method returns
`false` at line 119, which silently suppresses the entire downstream propagation cascade for all
follower accounts — the silent failure mode identified in `00-hotspots.md`.

**Risk classification:** TEST-PRIORITY (low structural complexity, HIGH blast consequence)  
**Refactor recommendation:** 0 extractions warranted in this method per hotspot analysis.
Phase 2 extraction targets are `PropagateMasterEntryMove` and `PropagateFollowerEntryReplace`.

---

## Agent Tracking

| Field              | Value                                              |
|---|---|
| **Agent Name**     | v12-phase1-scope                                   |
| **Epic**           | EPIC-W7-022                                        |
| **Wave**           | 7                                                  |
| **Phase**          | 1 — Scope Definition                               |
| **Output file**    | `docs/brain/EPIC-W7-022/00-scope.md`               |
| **Source file**    | `src/V12_002.Orders.Callbacks.Propagation.cs`      |
| **Method**         | `PropagateMaster_IdentifyMove`                     |
| **CYC current**    | 9 (fallback; tool reported 0)                      |
| **CYC target**     | ≤ 8                                                |
| **Callers found**  | 1 (`PropagateMasterPriceMove`, line 52)            |
| **Scope rule**     | V12.23 — single method per epic                    |
| **Execution Time** | Phase 1 complete                                   |
