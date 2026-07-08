# EPIC-W7-061 — Phase 1: Scope Definition

**Wave:** 7 | **Phase:** 1  
**Method:** `SubmitAndRegisterFleetOrders`  
**Source:** [`src/V12_002.SIMA.Fleet.cs`](../../src/V12_002.SIMA.Fleet.cs)  
**CYC Current:** 12 | **CYC Target:** ≤ 8  
**Generated:** Phase 1 automated scope definition

---

## 1. Single Method in Scope

This epic operates on a **single method** boundary. The one and only method under analysis and refactoring is:

| Method | File | Definition Line |
|--------|------|----------------|
| [`SubmitAndRegisterFleetOrders`](../../src/V12_002.SIMA.Fleet.cs:174) | `src/V12_002.SIMA.Fleet.cs` | L174 |

This is the **scope boundary** for EPIC-W7-061. No other method, class, or file falls within the change perimeter for Phase 1 through Phase 3 of this epic. All analysis, refactoring, and validation work is constrained entirely to this single method.

---

## 2. Complexity Profile

| Attribute | Value |
|-----------|-------|
| Current CYC | **12** |
| Target CYC | **≤ 8** |
| Required reduction | ≥ 4 complexity points |
| Method LOC (approx.) | ~50 lines |
| Access modifier | `private` |

The current CYC of 12 exceeds the Wave-7 ceiling of 10 and the stricter ≤ 8 target established by the hotspot analysis phase. The excess complexity is concentrated in two areas identified as H-1 and H-4 in [`00-hotspots.md`](./00-hotspots.md):

- **H-1:** Redundant double `TryGetValue` on `_followerBrackets` (same dictionary key, consecutive calls, contributing +3 branches: decision points 4, 5, 6/7).
- **H-4:** Compound `string.IsNullOrEmpty(ord.OrderId)` guard inside a `for` loop (contributing +2 implicit branches: decision points 11, 12).

---

## 3. Callers

A `grep` over `src/` for `SubmitAndRegisterFleetOrders` yielded **2 matches** in `src/V12_002.SIMA.Fleet.cs`:

| Match type | File | Line | Context |
|------------|------|------|---------|
| **Definition** | `src/V12_002.SIMA.Fleet.cs` | L174 | `private void SubmitAndRegisterFleetOrders(…)` |
| **Call site** | `src/V12_002.SIMA.Fleet.cs` | L65 | Called from `ProcessFleetSlot` |

**Caller count: 1** — [`ProcessFleetSlot`](../../src/V12_002.SIMA.Fleet.cs:44) is the sole direct caller. It invokes the method inside a `try/catch` block that uses the `ref syncCleared` parameter for rollback orchestration. No other file or method in `src/` references `SubmitAndRegisterFleetOrders`.

Transitive callers (2 hops, not in scope):
- [`PumpFleetDispatch`](../../src/V12_002.SIMA.Fleet.cs:233) — legacy `ConcurrentQueue` drain path
- [`ProcessValidPhotonSlot`](../../src/V12_002.SIMA.Fleet.cs:395) — Photon ring consumer path

These transitive callers reach the method only through `ProcessFleetSlot` and are outside the scope boundary.

---

## 4. Why Other Methods Are NOT in Scope

### V12.23 Exclusion Rationale

The Wave-7 ticket is scoped under versioning constraint **V12.23**, which restricts each epic to the minimum-blast-radius change unit. The following methods are explicitly excluded:

| Method | Reason for exclusion |
|--------|---------------------|
| [`ProcessFleetSlot`](../../src/V12_002.SIMA.Fleet.cs:44) | Sole caller; its structure is unaffected by internal refactors to `SubmitAndRegisterFleetOrders`. V12.23 prohibits propagating changes upward through the call graph unless the caller's own CYC exceeds threshold. |
| [`PumpFleetDispatch`](../../src/V12_002.SIMA.Fleet.cs:233) | Transitive caller (2 hops); no complexity issue surfaced in Wave-7 scan. V12.23 excludes 2-hop callers from single-method epics. |
| [`ProcessValidPhotonSlot`](../../src/V12_002.SIMA.Fleet.cs:395) | Transitive caller (2 hops); Photon ring consumer path. Same V12.23 exclusion applies. |
| [`ClearDispatchSyncPending`](../../src/V12_002.SIMA.Fleet.cs) | Called by the in-scope method; it is a leaf helper with CYC < 4. Extracting further helpers from `SubmitAndRegisterFleetOrders` may delegate to it but will not modify it. V12.23 prohibits modifying stable helpers under a single-method epic. |
| `InitializeFollowerBracketFSM` | Sibling method in `ProcessFleetSlot`; unrelated to the in-scope complexity hotspots. |

**Summary:** V12.23 enforces a **single method** scope boundary per epic ticket. Expanding scope to any of the above methods would require a separate Wave-7 ticket and its own Phase 0 hotspot analysis.

---

## 5. Planned Refactors (Phase 2 Preview)

The following refactors will be executed in Phase 2, all confined within the scope boundary:

| Priority | Refactor | Expected CYC reduction |
|----------|----------|----------------------|
| P1 | Merge the two `_followerBrackets.TryGetValue` lookups into a single lookup block; handle both the `PendingSubmit` → `Submitted` FSM transition and the order-ID indexing in one `if` body | −3 |
| P2 | Extract order-ID indexing loop into a private helper `RegisterOrderIds(FollowerBracketFSM, Order[], int)` | −2 |
| P3 | Add inline contract comment on `ref syncCleared` ordering requirement (zero CYC delta; safety documentation only) | 0 |

Applying P1 + P2 brings the projected CYC to **7**, satisfying the ≤ 8 target.

---

## 6. Scope Boundary Summary

```
EPIC-W7-061 scope boundary
══════════════════════════════════════════════════════════════════
  IN SCOPE
  └─ SubmitAndRegisterFleetOrders        src/V12_002.SIMA.Fleet.cs:174
       CYC 12 → target ≤ 8
       1 direct caller (ProcessFleetSlot)
       Refactors: P1 (merge TryGetValue) + P2 (extract loop helper)

  OUT OF SCOPE (V12.23)
  ├─ ProcessFleetSlot                    (sole caller — upstream)
  ├─ PumpFleetDispatch                   (2-hop transitive caller)
  ├─ ProcessValidPhotonSlot              (2-hop transitive caller)
  └─ ClearDispatchSyncPending            (leaf helper — downstream)
══════════════════════════════════════════════════════════════════
```

---

## 7. Agent Tracking

```
Agent Name:       v12-phase1-scope
Epic:             EPIC-W7-061
Wave:             7
Phase:            1 — Scope Definition
Status:           completed
Single method:    SubmitAndRegisterFleetOrders
Source file:      src/V12_002.SIMA.Fleet.cs
CYC current:      12
CYC target:       ≤ 8
Callers found:    1 (ProcessFleetSlot @ L65)
Scope boundary:   single method — SubmitAndRegisterFleetOrders only
V12.23 applied:   true
Output:           docs/brain/EPIC-W7-061/00-scope.md
```
