# EPIC-W7-040 — Phase 1: Scope Definition

## Summary

This document establishes the precise scope boundary for EPIC-W7-040. A single method has been identified as the sole target of the refactoring work in this epic. All analysis, extraction planning, and implementation work is bounded to that single method unless a subsequent phase explicitly revises this document.

---

## Method in Scope

| Field | Value |
|---|---|
| **Method** | `FindTargetOrderForPosition` |
| **File** | `src/V12_002.Trailing.Breakeven.cs` |
| **Lines** | 186–222 |
| **Visibility** | `private` |
| **Region** | `#region Stop Management Helpers` |
| **Current CYC** | **10** |
| **Target CYC** | **≤ 8** (threshold enforcement; projected post-extraction: **4**) |
| **Caller Count** | **1** — `MoveSpecificTarget`, same file, line 356 |

This is a **single method** scope. Exactly one symbol is under active modification in this epic.

---

## Scope Boundary

The **scope boundary** for EPIC-W7-040 is defined as follows:

- **In scope:** `FindTargetOrderForPosition` (lines 186–222, `src/V12_002.Trailing.Breakeven.cs`) — the method body, its internal logic, and the two helper methods to be extracted from it (`IsMatchingWorkingOrder` and `ResolveSearchAccount`), which are new additions with no pre-existing call-sites.
- **Out of scope:** All other methods in `src/V12_002.Trailing.Breakeven.cs`, all cross-file symbols, all caller call-sites (the `MoveSpecificTarget` invocation at line 356 remains syntactically unchanged), and the structural twin `FindTargetOrderForAbsoluteMove`.

The scope boundary is intentionally narrow. The method is `private` with a single call-site, providing a minimal blast radius. No interface contracts, no cross-file references, no public API surface changes.

---

## Callers

`grep` across `src/` returned **2 matches** for `FindTargetOrderForPosition`:

| Match | File | Line | Role |
|---|---|---|---|
| Definition | `src/V12_002.Trailing.Breakeven.cs` | 186 | Method declaration |
| Caller | `src/V12_002.Trailing.Breakeven.cs` | 356 | `MoveSpecificTarget` — only call-site |

**Callers count: 1.** The method is never called from outside its declaring file. No cross-file caller exists.

---

## Why Other Methods Are NOT in Scope

The `src/V12_002.Trailing.Breakeven.cs` file is a 596-line partial class (V12.23) containing 8 stop/target helpers in the same `#region Stop Management Helpers` block. Each of those helpers is excluded from this epic's scope for the following reasons:

| Method / Region | Reason Excluded |
|---|---|
| `FindTargetOrderForAbsoluteMove` (lines 438–462) | Structural twin with similar complexity; designated for a **future epic** — it shares the account-routing duplication pattern but requires its own isolated analysis, scope, and test pass before modification. Merging it into EPIC-W7-040 would expand the blast radius and violate the single-method discipline. |
| `MoveSpecificTarget` (line 356) | Direct caller of the in-scope method. Its call-site signature is unchanged by the extraction plan; it is affected only as a passive consumer, not as a modification target. |
| `MoveStop_SinglePosition` (CYC 8) | Separate hotspot, separate epic. Its CYC is above threshold but it shares no decision-node overlap with `FindTargetOrderForPosition`. Bundling it here would prevent clean, reviewable commits. |
| `ExecuteTargetAbsoluteMove` (CYC 7) | At threshold; no active breach. Excluded per minimum-change discipline — work is performed only where a breach exists. |
| All remaining helpers in V12.23 | No CYC breach detected by Phase 0 analysis. Out of scope by default — no evidence justifies inclusion. |

The constraint is explicit: **single method** work is the unit of delivery for each wave-7 epic. Expanding scope to additional methods in V12.23, even structurally related ones, would undermine traceability, increase review surface, and risk silent regression in untested helpers.

---

## Complexity Reduction Plan (Reference)

Derived from `00-hotspots.md` Phase 0 findings. Included here for scope traceability only — detailed implementation steps are deferred to Phase 2.

| Extraction | Target New Method | CYC Removed from `FindTargetOrderForPosition` |
|---|---|---|
| Compound order-match predicate (`&&` / `\|\|` chain) | `IsMatchingWorkingOrder(Order order, string targetOrderName)` | −4 |
| Account-routing ternary + inner `&&` | `ResolveSearchAccount(PositionInfo pos)` | −2 |

Post-extraction projected CYC of `FindTargetOrderForPosition`: **4**.
All three resulting methods individually below the CYC ≤ 8 target.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase1-scope |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Epic** | EPIC-W7-040 |
| **Source File** | `src/V12_002.Trailing.Breakeven.cs` |
| **Method in Scope** | `FindTargetOrderForPosition` |
| **Current CYC** | 10 |
| **Target CYC** | ≤ 8 |
| **Callers Count** | 1 |
| **Scope Boundary Confirmed** | Yes — single method, private, one call-site, no cross-file exposure |
| **Input Docs** | `00-hotspots.md` (Phase 0 output) |
| **Output Doc** | `00-scope.md` (this file) |
