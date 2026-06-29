# EPIC-W7-005 — Phase 1: Scope Definition

**Epic ID:** EPIC-W7-005
**Wave:** 7
**Phase:** 1 — Scope Definition (REDO)
**Protocol Version:** V12.23

---

## Single Method in Scope

This is a **single method** epic. The one and only method targeted for complexity reduction is:

**Method:** `ClassifyAndRouteFleetOrder`
**Signature:** `private ConcurrentDictionary<string, Order> ClassifyAndRouteFleetOrder(...)`
**Source File:** `src/V12_002.SIMA.Lifecycle.cs`
**Canonical Baseline Line:** 408 (sha 25b55d5)
**Return Type:** `ConcurrentDictionary<string, Order>`
**LOC at Baseline:** 42 (complexity audit) / 60 (Codacy Lizard — includes braces and inline comments)

---

## CYC: Current vs. Target

| Metric | Value | Source |
|--------|-------|--------|
| **Current CYC (epic-list registered)** | 0 | `wave7-epic-list.json` — sparse/phantom entry; data gap at list-build time |
| **Current CYC (confirmed actual)** | 16 | `complexity_audit_full.txt` line 617, `complete_wave_cross_reference.json` line 1587, `autonomous_refactor_baseline_corrected.md` line 50, `TIER2_METHODS_ANALYSIS.md` line 412 |
| **Target CYC** | ≤ 8 | `epic_roadmap_wave7.json` `cyc_target: 8` (Jane Street strict standard) |
| **Required CYC reduction** | ≥ 8 points (≥ 50%) | — |

The CYC=0 in the epic list is a known data-gap pattern from wave7 list generation, **not** an indication of zero complexity. Three independent audit sources converge on **CYC = 16** as the confirmed actual value.

---

## Source File

**Canonical source file:** `src/V12_002.SIMA.Lifecycle.cs`

This file is confirmed as the home of `ClassifyAndRouteFleetOrder` across all audit sources:
- `complete_wave_cross_reference.json` line 1586: `"full_name": "V12_002.SIMA.Lifecycle.cs::ClassifyAndRouteFleetOrder"`
- `docs/brain/complexity_audit_full.txt` line 617
- `docs/brain/autonomous_refactor_baseline_corrected.md` line 50
- `docs/brain/codacy_all_issues.json` line 60 (Lizard: LOC=60, line 408, sha 25b55d5)

**Live HEAD status:** Method body is **no longer present** in the source file at HEAD. Wave 4/6 decomposed the original into three helpers: `ClassifyOrderByPrefix` (line 1262), `AdoptOrdersFromAccount` (line 930), `AdoptSingleOrder` (line 1058). Live grep of the entire `src/` tree yields **zero occurrences** of `ClassifyAndRouteFleetOrder` in any `.cs` file.

---

## Callers Count

**Direct callers of `ClassifyAndRouteFleetOrder` in live source:** **0**

**Direct callers at baseline (pre-Wave 4/6 decomposition):** **2**

| Caller | CYC | Notes |
|--------|-----|-------|
| `AdoptFleetWorkingOrders` | 17 | Primary upstream caller; populated `_workingOrders` and `_fleetStopOrders` before FSM hydration. Now renamed `AdoptFleetOrders` at line 903. |
| `AdoptMasterWorkingOrders` | 9 | Parallel adoption pathway; shared same dictionary mutation pattern. Now renamed `AdoptMasterOrders` at line 1195. |

References to `ClassifyAndRouteFleetOrder` at HEAD exist **only** in documentation and tracking files (`wave7-epic-list.json`, `epic_roadmap_wave7.json`, `complete_wave_cross_reference.json`, `complexity_audit_full.txt`, `codacy_all_issues.json`). None are in `.cs` source files.

---

## Scope Boundary

The **scope boundary** for EPIC-W7-005 is strictly limited to the **single method** `ClassifyAndRouteFleetOrder` and any helper methods that were directly extracted from it during Wave 4/6 decomposition. No other method is in scope.

> **Scope boundary statement:** Only `ClassifyAndRouteFleetOrder` and its direct extracted successors (`ClassifyOrderByPrefix`, `AdoptOrdersFromAccount`, `AdoptSingleOrder`) fall within the scope boundary. Any work on these three helpers is in scope only if Phase 1.5 live CYC measurement confirms a residual helper exceeds CYC 8.

### Why Other Methods Are NOT in Scope (V12.23)

Under **protocol V12.23**, each Wave 7 epic is a **single-method** refactor unit. Scope expansion beyond the registered target method is prohibited unless a formal scope-extension trigger fires (residual CYC > threshold confirmed by live measurement in Phase 1.5). The following method categories are therefore explicitly excluded:

**Downstream consumers — excluded:**
- `HydrateFSMsFromWorkingOrders` (CYC=9) — reads `_workingOrders` populated by the routing logic but is not the complexity target.
- `SweepTrackedOrders` (CYC=12) — depends on dictionary keys written during classification; excluded from scope.
- `SweepBrokerOrders` (CYC=18) — has its own Wave 7 epic (EPIC-W7-007); must not be touched here.
- `ShouldProtectBracketOrder` (CYC=10) — consults order dictionaries for bracket protection; excluded from scope.

**Cross-file consumers — excluded:**
- `EmergencyFlattenSingleFleetAccount` in `V12_002.SIMA.Flatten.cs` (CYC=16) — reads fleet order state seeded by routing; out of scope per V12.23 single-method boundary.
- `AuditMaster_HandleNakedPosition` in `V12_002.REAPER.Audit.cs` (CYC=15) — audits positions whose classification originates in the routing; out of scope.
- State-sync methods in `V12_002.StickyState.cs` (lines 600, 611) — lifecycle coupling; out of scope.

**Caller chain — excluded:**
- `AdoptFleetOrders` (line 903), `AdoptMasterOrders` (line 1195), `HydrateWorkingOrdersFromBroker` (line 309) — these are callers of the decomposed logic, not the target. Modifying them would violate the single-method scope boundary per V12.23.

V12.23 rationale: modifying any method outside the registered target in a Wave 7 phase introduces uncontrolled blast-radius risk and defeats the purpose of the epic decomposition strategy. Each method in the blast radius that requires work is registered as its own epic.

---

## Dependency Graph Summary (live source)

```
HydrateWorkingOrdersFromBroker() [line 309]
  └─ AdoptFleetOrders() [line 903]
       └─ AdoptOrdersFromAccount(acct, ref count) [line 930]
            ├─ ClassifyOrderByPrefix(name) [line 1262]  ← classification (IN SCOPE if CYC > 8)
            └─ AdoptSingleOrder(ord, acct, key, ref count) [line 1058]  ← routing (IN SCOPE if CYC > 8)
```

Cross-file consumers (read-only blast radius — **NOT in scope per V12.23**):
- `V12_002.SIMA.Flatten.cs` — reads fleet order dictionaries seeded by adoption
- `V12_002.REAPER.Audit.cs` — audits positions whose classification originates in adoption
- `V12_002.StickyState.cs` — state-sync coupling at lifecycle transition boundaries

---

## Phase 1.5 Audit Gate

Per `00-hotspots.md` recommendation: before any further code changes, Phase 1.5 must measure live CYC on the three extracted helpers. Expected outcome: CYC target already met by Wave 4/6 decomposition. If confirmed, this epic closes as **decomposition complete**. If any helper exceeds CYC 8, **only that helper** falls within a scope boundary extension, and only after a new scope definition is filed.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase1-scope |
| Epic ID | EPIC-W7-005 |
| Wave | 7 |
| Phase | 1 — Scope Definition (REDO) |
| Protocol | V12.23 |
| CYC (registered) | 0 (epic-list data gap) |
| CYC (confirmed actual) | 16 (multi-source audit) |
| Target CYC | ≤ 8 |
| Scope | Single method: ClassifyAndRouteFleetOrder |
| Source File | src/V12_002.SIMA.Lifecycle.cs |
| Live Callers | 0 (method decomposed at HEAD) |
| Baseline Callers | 2 (AdoptFleetWorkingOrders, AdoptMasterWorkingOrders) |
| Output | docs/brain/EPIC-W7-005/00-scope.md |
| Status | ✅ Phase 1 Complete |

**Sources consulted:**
- [`docs/brain/EPIC-W7-005/00-hotspots.md`](docs/brain/EPIC-W7-005/00-hotspots.md) — primary source of truth for method, CYC, file, blast radius
- [`docs/brain/wave7-epic-list.json`](docs/brain/wave7-epic-list.json) — CYC=0 sparse entry (lines 30–36)
- [`complete_wave_cross_reference.json`](complete_wave_cross_reference.json) — CYC=16, full_name confirmed (lines 1585–1592)
- [`docs/brain/autonomous_refactor_baseline_corrected.md`](docs/brain/autonomous_refactor_baseline_corrected.md) — CYC=16, LOC=42, READY
- [`docs/brain/codacy_all_issues.json`](docs/brain/codacy_all_issues.json) — Lizard LOC=60, line 408, sha 25b55d5
- [`TIER2_METHODS_ANALYSIS.md`](TIER2_METHODS_ANALYSIS.md) — CYC=16, Tier 1 classification (line 412)
- [`epic_roadmap_wave7.json`](epic_roadmap_wave7.json) — cyc_target=8, priority=high
- [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs) — live grep: zero occurrences of method; extracted helpers confirmed at lines 903, 930, 1058, 1195, 1262

---
*Generated by Bob — Wave 7, Phase 1 (REDO)*
*Protocol: EPIC-W7-005 / 00-scope.md / V12.23*
