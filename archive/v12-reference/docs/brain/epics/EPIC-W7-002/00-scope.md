# EPIC-W7-002 — Phase 1: Scope Definition

## Single Method in Scope

This phase targets exactly one **single method** for cyclomatic complexity reduction. No
other methods are included. The scope boundary is explicitly defined below.

| Method | File | Lines | Current CYC | Target CYC |
|---|---|---|---|---|
| `SymmetryGuardTryResolveFollowersForDispatch` | `src/V12_002.Symmetry.Replace.cs` | 134–191 | **16** | **≤ 8** |

---

## Method Details

**Method:** `SymmetryGuardTryResolveFollowersForDispatch`  
**Signature:** `private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)`  
**File:** `src/V12_002.Symmetry.Replace.cs`  
**Location:** Lines 134–191 (58 lines of body)  
**Current CYC:** 16  
**Target CYC:** ≤ 8  

---

## Callers Analysis

Codebase grep across all `.cs` files confirms exactly **1 direct caller**:

| Caller File | Caller Line | Call Site |
|---|---|---|
| `src/V12_002.Symmetry.cs` | 322 | `SymmetryGuardTryResolveFollowersForDispatch(ctx.DispatchId, DateTime.UtcNow)` |

The call is made immediately after the master anchor is locked, which is a critical
serialisation point. No other call sites exist anywhere in the 80-file codebase.

---

## Scope Boundary

> **scope boundary**: Only `SymmetryGuardTryResolveFollowersForDispatch` (lines 134–191 of
> `src/V12_002.Symmetry.Replace.cs`) and any new private helper methods extracted from it
> during Phase 2 are within scope. Every other symbol in the repository is **out of scope**
> for this epic.

The scope boundary is enforced by the V12.23 No Scope Creep Protocol (see below). Any
refactoring touch outside the defined scope boundary must be rejected and handled as a
separate work item.

---

## Complexity Profile (from Phase 0 Hotspot Analysis)

The CYC of 16 is produced by three structural drivers inside the single method:

1. **Dual-pass follower collection** — Pass 1 walks `ctx.Followers` immutable snapshot;
   Pass 2 scans all of `symmetryPendingFollowerFills` as a legacy safety net (ADR-019).
   Each pass contains 3–4 independent guard conditions, contributing ~8 branch points.

2. **Asymmetric precondition gates** — Pass 1 is guarded by
   `symmetryDispatchById.TryGetValue(dispatchId, out var ctx) && ctx != null`; Pass 2 is
   **not** guarded by the same condition. The reader must mentally maintain two separate
   precondition sets converging into the same `followersToResolve` list.

3. **Resolution loop with nested side-effects** — the final `foreach` (lines 176–190)
   nests `TryGetValue`, `IsFollower` check, `SymmetryGuardTryResolveFollower` call, and
   `TryRemove` inline, making partial-resolution states difficult to reason about.

---

## Recommended Extraction Plan (Phase 2 Preview)

Three helper methods will be extracted from the **single method** under scope, each with
an estimated CYC ≤ 4, leaving the coordinator shell at CYC ≤ 2:

| # | Proposed Helper | Responsibility | Est. CYC |
|---|---|---|---|
| 1 | `CollectFollowersFromSnapshot` | Pass 1 — snapshot-driven worklist build | 4 |
| 2 | `CollectFollowersFromPendingMap` | Pass 2 — legacy full-map scan + dedup | 4 |
| 3 | `ResolveFollowerWorklist` | Final resolution loop with TryRemove | 4 |

The three helpers are extracted into the **same file and class** as the parent method.
They are not exposed beyond the existing `private` visibility surface.

---

## V12.23 No Scope Creep Protocol — Why Other Methods Are NOT in Scope

The following callee and sibling methods are explicitly **excluded** from this epic's scope:

| Excluded Symbol | Reason |
|---|---|
| `SymmetryGuardTryResolveFollower` (`Symmetry.Follower.cs`) | Callee — behaviour must remain unchanged; modifying it risks ADR-019 contract breakage |
| `SymmetryGuardSkipFollower` | Transitive callee — independent blast radius; separate epic candidate |
| `FlattenPositionByName` | Order-management subsystem — unrelated CYC budget |
| `CleanupPosition` | Position lifecycle — unrelated CYC budget |
| `SymmetryGuardForgetEntry` | Fleet-entry cleanup — unrelated CYC budget |
| All other methods in `src/V12_002.Symmetry.Replace.cs` | Same file, different responsibilities — V12.23 prohibits opportunistic cleanup |
| `src/V12_002.Symmetry.cs:322` call site | Read-only context; call signature is unchanged |

V12.23 mandates that **every changed line must trace directly to the stated CYC reduction
goal of the single method in scope**. Touching any excluded symbol constitutes scope creep
and must be tracked as a separate backlog item.

---

## Shared State Context (Read-Only Reference)

The three `ConcurrentDictionary` stores accessed by the in-scope method span 6 source
files (per Phase 0 blast-radius analysis). They are listed here for awareness only — none
of the stores themselves are modified by this refactoring:

| Store | Writers (out of scope) | Readers |
|---|---|---|
| `symmetryDispatchById` | `Symmetry.cs`, `AccountOrders.cs` | in-scope method (read-only) |
| `symmetryFleetEntryToDispatch` | `Symmetry.cs`, `SIMA.Shadow.cs` | in-scope method (read-only) |
| `symmetryPendingFollowerFills` | `Follower.cs` | in-scope method (TryRemove side-effect preserved) |

---

## Agent Tracking

```
Agent Name:     v12-phase1-scope
Epic:           EPIC-W7-002
Wave:           7
Phase:          1 — Scope Definition (REDO)
Status:         completed
Output:         docs/brain/EPIC-W7-002/00-scope.md
Method:         SymmetryGuardTryResolveFollowersForDispatch
Source File:    src/V12_002.Symmetry.Replace.cs
CYC Current:    16
CYC Target:     <= 8
Callers Count:  1 (V12_002.Symmetry.cs:322)
Bobcoins Used:  8
Execution Time: 2025-07-11T00:00:00Z
```
