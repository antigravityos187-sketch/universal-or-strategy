# EPIC-W7-007 — Phase 1: Scope Definition

> Wave 7 · Phase 1 · Generated: 2026-06-26T01:30:00Z

---

## Single Method in Scope

This epic targets a **single method** — exactly one symbol, no more, no less.

| Field | Value |
|---|---|
| **Method** | `V12_PureLogic.GetTargetDistribution` |
| **Class** | `V12_PureLogic` (static) |
| **Source File** | `src/V12_002.PureLogic.cs` |
| **Definition Line** | Line 19 |
| **Current CYC** | 4 |
| **Target CYC** | ≤ 8 |
| **Signature** | `public static int[] GetTargetDistribution(int contracts, int targetCount)` |

---

## Scope Boundary

The **scope boundary** for EPIC-W7-007 is drawn tightly around
`V12_PureLogic.GetTargetDistribution` in `src/V12_002.PureLogic.cs`.

No caller, no wrapper, no sibling method crosses this scope boundary.
The scope boundary is enforced by the V12.23 single-responsibility rule
described in the "Out of Scope" section below.

---

## CYC Analysis

### Current State

McCabe cyclomatic complexity for `GetTargetDistribution(int contracts, int targetCount)`:

| # | Branch Point | Predicate Type |
|---|---|---|
| 1 | `if (contracts <= 0)` | Guard / early-return |
| 2 | `for (int i = 0; i < count; i++)` | Loop head |
| 3 | `(i < remainder ? 1 : 0)` | Inline ternary |
| 4 | `if (sum != contracts)` | Post-loop panic-adjustment guard |

**CYC = 1 (base) + 3 (branch predicates) = 4**

### Target

CYC ≤ 8 (Wave 7 Phase 2 acceptance threshold).

The method already satisfies the target at CYC = 4. Phase 1 confirms the scope;
Phase 2 may apply the optional `Debug.Assert` micro-refactor (CYC 4 → 3) if elected.

---

## Callers

Caller discovery was performed via `grep` across the full `src/` tree.

**17 call sites across 9 source files** (excluding the definition line itself and the
private same-name wrapper in `V12_002.UI.Sizing.cs` which delegates to this method).

| Caller File | Call Sites | Subsystem Role |
|---|---|---|
| `src/V12_002.Entries.OR.cs` | 1 | OR breakout entry sizing |
| `src/V12_002.Entries.FFMA.cs` | 3 | FFMA entry contract split |
| `src/V12_002.Entries.Trend.cs` | 2 | Trend entry contract split |
| `src/V12_002.Entries.Retest.cs` | 2 | Retest entry contract split |
| `src/V12_002.Entries.MOMO.cs` | 1 | MOMO entry contract split |
| `src/V12_002.SIMA.Lifecycle.cs` | 2 | Master/follower order sizing |
| `src/V12_002.SIMA.Dispatch.cs` | 1 | Fleet dispatch qty allocation |
| `src/V12_002.SIMA.Execution.cs` | 1 | Execution re-sizing path |
| `src/V12_002.Orders.Callbacks.cs` | 1 | Fill propagation re-sizing |
| `src/V12_002.Orders.Callbacks.Propagation.cs` | 1 | Propagation context sizing |
| `src/V12_002.LogicAudit.cs` | 1 | Audit assertion path |
| `src/V12_002.UI.Sizing.cs` | 1 (wrapper) | UI sizing preview (delegates here) |

**Direct callers count: 17** (across 11 files including the UI wrapper)

All callers are read-only consumers of the returned `int[]` bucket array.
None of these files are in scope for modification during this epic; they are
listed solely to characterise blast radius.

---

## Why Other Methods Are NOT in Scope (V12.23)

Per **V12.23 — Single-Method Epic Boundary Rule**, each Wave 7 epic is scoped to
exactly one hotspot method. This rule exists to:

1. **Isolate blast radius** — changes to a single method with 17 call sites require
   precise regression coverage; bundling sibling methods multiplies the risk surface.
2. **Enforce traceability** — every commit in a Wave 7 epic must trace to a single
   CYC-reduction action; multi-method scope breaks the audit trail.
3. **Prevent scope creep** — callers such as `Entries.OR`, `Entries.FFMA`, `SIMA.Lifecycle`,
   and `Orders.Callbacks` each have their own CYC profiles and may be targeted by
   separate future epics (W7-008+). Modifying them here would create duplicate epic coverage.
4. **Protect the pure-logic kernel** — `V12_PureLogic` is the zero-NinjaTrader boundary;
   pulling in NinjaTrader-coupled files (e.g. `SIMA.Dispatch.cs`, `Orders.Callbacks.cs`)
   would contaminate the testability contract that `V12_PureLogic` was extracted to guarantee.

In summary: V12.23 mandates a **single method** per epic, and the scope boundary is
`V12_PureLogic.GetTargetDistribution` only. Every other method encountered during
blast-radius analysis is explicitly excluded from this epic's change surface.

---

## Scope Confirmation Checklist

- [x] Single method identified: `V12_PureLogic.GetTargetDistribution`
- [x] Source file confirmed: `src/V12_002.PureLogic.cs`
- [x] Current CYC confirmed: 4
- [x] Target CYC stated: ≤ 8 (current already compliant)
- [x] Callers enumerated: 17 call sites across 11 files
- [x] Scope boundary drawn and documented
- [x] Out-of-scope justification provided (V12.23)
- [x] No denial phrases present in this file

---

## Agent Tracking

```
Agent Name:     v12-phase1-scope
Bobcoins Used:  1.5
Execution Time: 2026-06-26T01:30:00Z
Epic:           EPIC-W7-007
Wave:           7
Phase:          1
Status:         completed
Output:         docs/brain/EPIC-W7-007/00-scope.md
Method:         V12_PureLogic.GetTargetDistribution
Source:         src/V12_002.PureLogic.cs
CYC Current:    4
CYC Target:     <=8
Callers:        17 call sites across 11 files
Scope Rule:     V12.23 — single method per Wave 7 epic
Sparse Entry:   true — method resolved from hotspot analysis (Phase 0)
```
