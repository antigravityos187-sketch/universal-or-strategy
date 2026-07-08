# EPIC-W7-045 — Phase 1: Scope Definition

## Scope Summary

This document establishes the exact scope boundary for the refactoring work
carried out under EPIC-W7-045, Wave 7. Only a single method is placed in scope
for this epic. No other methods are included.

---

## Method in Scope

| Field              | Value                                   |
|--------------------|-----------------------------------------|
| **Method**         | `OnKeyDown`                             |
| **File**           | `src/V12_002.UI.Callbacks.cs`           |
| **Lines**          | 391–426                                 |
| **Visibility**     | `private` — WPF `PreviewKeyDown` handler |
| **CYC (current)**  | 4 (per hotspot analysis: `_keyCommands` null-check + D1/NumPad1 + D2/NumPad2 + D3/NumPad3) |
| **CYC (task tag)** | 0 (task header) / 3 (source comment `// CYC 3`)  |
| **CYC (target)**   | ≤ 8                                     |

The current measured CYC of `OnKeyDown` is 4, comfortably below the target
ceiling of ≤ 8. Even so, the method presents structural complexity drivers
(mixed dispatch strategies, modifier-key polling that is untestable without a
WPF dispatcher) that warrant scoped refactoring — details recorded in
`00-hotspots.md`.

---

## Caller Analysis

`OnKeyDown` is a **single method** registered as a WPF `PreviewKeyDown` event
handler. The callers found by static search of `src/` are:

| Location                         | Line | Role                              |
|----------------------------------|------|-----------------------------------|
| `src/V12_002.UI.Callbacks.cs:48` | 48   | Attaches handler in `AttachHotkeys()` via `+=` |
| `src/V12_002.UI.Callbacks.cs:56` | 56   | Detaches handler in `DetachHotkeys()` via `-=` |

**Callers count: 2** (both are event-subscription sites, not direct call-sites).

No external assembly or test file calls `OnKeyDown` directly. The method is
`private` and reachable only through the WPF event infrastructure.

---

## Scope Boundary

The scope boundary for EPIC-W7-045 is limited to the single method
`OnKeyDown` (lines 391–426, `src/V12_002.UI.Callbacks.cs`). This boundary
encompasses:

- The method declaration and its four conditional branches.
- Any helper extracted *from* `OnKeyDown` as part of the refactoring (e.g., a
  prospective `ResolveModifierGroup()` per `00-hotspots.md`).
- Adjustments to the `_keyCommands` initialisation **only** where driven by
  changes to `OnKeyDown`'s branching logic.

Everything else is explicitly out of scope (see section below).

---

## Why Other Methods Are NOT in Scope (V12.23 Constraint)

Per project convention **V12.23**, a Wave-7 epic targets exactly one hotspot
method per phase. The following related methods were identified during hotspot
analysis but are explicitly excluded:

| Method                             | CYC  | Reason Excluded                                      |
|------------------------------------|------|------------------------------------------------------|
| `HandleTargetAction`               | 6    | Callee of `OnKeyDown`; separate hotspot candidate    |
| `HandleRunnerAction`               | 6    | Callee of `OnKeyDown`; separate hotspot candidate    |
| `ExecuteTargetAction`              | 2    | Below threshold; not a complexity driver             |
| `ExecuteTargetActionForPosition`   | 5+   | Out of scope — different responsibility layer        |
| `ExecuteRunnerAction`              | 3+   | Out of scope — different responsibility layer        |
| `AttachHotkeys` / `DetachHotkeys`  | 1    | Event plumbing only; zero branching logic to refactor |

Expanding scope to any callee would violate the V12.23 single-method
constraint and risk cascading blast-radius changes that go beyond what was
approved for Wave 7, Phase 1.

---

## Acceptance Criteria

1. `OnKeyDown` CYC remains ≤ 8 after any refactoring.
2. All existing keyboard shortcuts (basic hotkeys, T1, T2, Runner) continue to
   function identically (behaviour parity).
3. No net-new public API surface is introduced.
4. Any extracted helpers are `private` and reside in the same file.
5. Callers (`AttachHotkeys` / `DetachHotkeys`) require zero modification.

---

## Agent Tracking

```
EPIC         : EPIC-W7-045
WAVE         : 7
PHASE        : 1 — Scope Definition
STATUS       : completed
OUTPUT       : docs/brain/EPIC-W7-045/00-scope.md
Agent Name   : v12-phase1-scope
TIMESTAMP    : 2025-07-14T00:00:00Z
CYC_CURRENT  : 4 (measured) / 0 (task tag) / 3 (source comment)
CYC_TARGET   : <= 8
CALLERS      : 2 (event subscription sites, same file)
SCOPE        : single method — OnKeyDown (src/V12_002.UI.Callbacks.cs:391-426)
```
