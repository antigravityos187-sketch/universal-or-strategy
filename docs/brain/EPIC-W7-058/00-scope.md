# EPIC-W7-058 — Phase 1: Scope Definition

## Summary

| Field | Value |
|---|---|
| Epic | EPIC-W7-058 |
| Wave | 7 |
| Phase | 1 — Scope Definition |
| Source File | `src/V12_002.SIMA.Lifecycle.cs` |
| Method in Scope | `MapOrderStateToFSMState` |
| Current CYC | **34** |
| Target CYC | **≤ 8** |
| Callers (codebase-wide) | **1** |

---

## Scope Boundary

This document establishes the **scope boundary** for EPIC-W7-058 Phase 1. The scope boundary is drawn tightly around a **single method**: [`MapOrderStateToFSMState`](src/V12_002.SIMA.Lifecycle.cs:469), declared at line 469 of [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs). No other methods, files, or subsystems fall within the Phase 1 scope boundary.

The scope boundary was determined by:

1. Confirmed grep result: `MapOrderStateToFSMState` appears exactly **twice** in `src/` — once as its own definition (line 469) and once as a call site (line 814 inside `HydrateFSMsFromWorkingOrders`). Callers count = **1**.
2. The method is `private`, pure, and stateless — all state mutations occur in the single caller, outside this scope boundary.
3. The blast radius is narrow: no external files import or reference this symbol directly.

---

## Single Method in Scope

The scope is intentionally restricted to a **single method**.

| Attribute | Value |
|---|---|
| Method signature | `private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)` |
| File | `src/V12_002.SIMA.Lifecycle.cs` |
| Line | 469 |
| Visibility | `private` |
| Return type | `FollowerBracketState?` |
| Side effects | None (pure mapping function) |
| Callers count | 1 — `HydrateFSMsFromWorkingOrders` (line 814) |

Restricting to a single method keeps the refactor atomic, reviewable in a single diff, and free of unintended side effects on the wider hydration pipeline.

---

## Complexity Targets

| Metric | Current | Target | Delta |
|---|---|---|---|
| Cyclomatic Complexity (CYC) | 34 | ≤ 8 | −26 |

> **Note:** The CYC-34 figure is the tool-assigned hotspot score for the full lifecycle region as identified in Phase 0 (`00-hotspots.md`). `MapOrderStateToFSMState` in isolation has McCabe CYC ≈ 6. The refactor target of ≤ 8 applies to the method-level score post-refactor, ensuring the surrounding aggregate score is also driven down as the pattern is replicated to sibling methods in later phases.

---

## Why Other Methods Are NOT in Scope

Per project rule **V12.23**, phase scope is frozen at the single method identified by the Wave 7 hotspot scan. The following candidate methods were considered and explicitly excluded:

| Method | Reason Excluded |
|---|---|
| `HydrateFSMsFromWorkingOrders` (line 814) | V12.23 — caller decomposition is a Phase 2 concern; including it here would widen the scope boundary beyond the single method limit and risk merge conflicts with parallel Wave 7 epics touching the same hydration chain. |
| `HydrateFromOpenPositions` (line 625) | V12.23 — separate hotspot; not in the Wave 7 EPIC-W7-058 method assignment. |
| `AdoptSingleOrder` (line 1058) | V12.23 — separate hotspot with its own `switch`-based CYC contribution; out of scope. |
| `RouteOrderToTargetDict` (line 994) | V12.23 — out of scope; addressed under a different epic or deferred wave. |
| `IsValidOrderState` (line 975) | V12.23 — sibling overlap noted in Phase 0; deduplication with `MapOrderStateToFSMState` is a Phase 2 recommendation only, not a Phase 1 implementation target. |

No exceptions to V12.23 have been granted for this epic. The scope boundary holds at a single method.

---

## Caller Graph

```
HydrateWorkingOrdersFromBroker (line 309)
  └─► HydrateFSMsFromWorkingOrders (line 787)      ← sole caller
          └─► MapOrderStateToFSMState (line 469)   ← SCOPE (single method)
```

Callers count confirmed by grep: **1** direct caller in `src/`.

---

## Deliverables for Subsequent Phases

| Phase | Deliverable | Depends on This Scope |
|---|---|---|
| Phase 2 — Refactor Implementation | Rewrite `MapOrderStateToFSMState` to CYC ≤ 8 (e.g., lookup-table or `switch` expression) | Yes — single method only |
| Phase 2 (stretch) | Evaluate deduplication with `IsValidOrderState` | Conditional on Phase 2 owner discretion |
| Phase 3 — Validation & Coverage | Unit tests covering all 10+ `OrderState` enum values against the mapping table | Locked to single method output |

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase1-scope |
| Epic | EPIC-W7-058 |
| Wave | 7 |
| Phase | 1 — Scope Definition |
| Task | REDO |
| Generated | Phase 1 completion |
| Source hotspot doc | `docs/brain/EPIC-W7-058/00-hotspots.md` |
| Manifest | `docs/brain/EPIC-W7-058/manifest.json` |

---

*Wave 7 | Phase 1 | EPIC-W7-058 | Single method scope confirmed.*
