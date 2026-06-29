# EPIC-W7-064 — Phase 1: Scope Definition

---

## Single Method in Scope

This epic targets exactly one single method: **`ResolveFsm_ByScan`**.

| Attribute        | Value                                     |
|------------------|-------------------------------------------|
| Method           | `ResolveFsm_ByScan`                       |
| File             | `src/V12_002.Symmetry.BracketFSM.cs`      |
| Lines            | 209–246                                   |
| Current CYC      | **11**                                    |
| Target CYC       | **≤ 8**                                   |
| Callers (count)  | **1** — `ResolveFsmFromEvent` (line 264)  |

The scope boundary is drawn at the body of `ResolveFsm_ByScan` and any private helper(s)
extracted directly from it. Nothing outside this boundary is modified during this epic.

---

## Callers Analysis

A grep across the entire `src/` tree yielded **2 occurrences** of the symbol `ResolveFsm_ByScan`:

| Occurrence | File                                       | Line | Role               |
|------------|--------------------------------------------|------|--------------------|
| 1          | `src/V12_002.Symmetry.BracketFSM.cs`       | 209  | Method definition  |
| 2          | `src/V12_002.Symmetry.BracketFSM.cs`       | 264  | Sole call site     |

There is exactly **1 caller**: `ResolveFsmFromEvent`, located in the same file. No external assemblies,
tests, or other source files reference this method. This single-caller topology keeps the
scope boundary tight and eliminates any cross-file coordination risk during refactoring.

---

## Why Other Methods Are NOT in Scope

Per constraint **V12.23**, each epic addresses a single method unit. The following peer symbols
were considered and explicitly excluded:

| Symbol                        | Reason excluded                                                                                        |
|-------------------------------|--------------------------------------------------------------------------------------------------------|
| `ResolveFsmFromEvent`         | Orchestrator/caller — its own CYC is within tolerance; modifying it would widen the scope boundary.   |
| `ResolveFsm_ByOrderId`        | Tier 1 O(1) path — CYC is low; no hotspot designation; out of scope per V12.23 single-method rule.    |
| `ResolveFsm_BySignalName`     | Tier 2 O(1) path — CYC is low; no hotspot designation; out of scope per V12.23 single-method rule.    |
| `ProcessBracketEvent`         | Upstream orchestrator; listed in blast-radius chain only; CYC not flagged; excluded per V12.23.       |
| `ValidateFsmEventPreconditions` | Upstream guard; CYC not flagged; excluded per V12.23 single-method rule.                            |
| `MatchOrderInFsm` (proposed)  | A helper to be **extracted from** the in-scope method; it is a product of this epic, not a peer scope.|

**V12.23 mandate:** Only the method bearing the Tier-3 hotspot designation and CYC ≥ threshold
enters scope. All coupled methods remain at read-only blast-radius awareness status.

---

## Complexity Reduction Plan (Summary)

Three actions drive CYC from 11 → ≤ 8 inside the scope boundary:

1. **Extract `MatchOrderInFsm`** — encapsulates the Stop / Targets[0-4] / Entry 3-slot scan with
   backfill writes into a dedicated helper; removes the inner `for` loop and compound guard chain
   from the parent body (−5 CYC from parent, +3 in new helper, net parent reduction = −5).
2. **Remove dead-code `foundT` flag** — lines 225 and 234–235 are provably unreachable (`return f`
   precedes every `foundT = true` assignment); deletion removes 1 spurious branch (−1 CYC).
3. **(Optional) Account-filter extraction** — move the `accountAlias` equality guard into a
   pre-filtered enumerable; reduces outer loop guard count (−1 CYC).

Post-refactor projected CYC for `ResolveFsm_ByScan` parent body: **≤ 5**, well inside target ≤ 8.

---

## Risk & Constraints

| Risk                          | Mitigation                                                                                     |
|-------------------------------|------------------------------------------------------------------------------------------------|
| Backfill side-effect loss     | `_orderIdToFsmKey` writes at lines 221, 230, 240 are load-bearing; extracted helper must preserve them. |
| Threading invariant           | Strategy-thread-only contract must not be broadened; no lock primitives to add or remove.     |
| ConcurrentDictionary enumeration | Live lock-free enumeration of `_followerBrackets.Values` must remain unchanged.           |
| Single caller contract        | `ResolveFsmFromEvent` call signature at line 264 must remain identical post-refactor.         |

---

## Agent Tracking

```
Agent Name:      v12-phase1-scope
Epic:            EPIC-W7-064
Wave:            7
Phase:           1 — Scope Definition
Method in scope: ResolveFsm_ByScan
CYC current:     11
CYC target:      <= 8
Callers found:   1
Scope boundary:  ResolveFsm_ByScan body + extracted helpers only
V12.23 applied:  yes — single method rule enforced
```
