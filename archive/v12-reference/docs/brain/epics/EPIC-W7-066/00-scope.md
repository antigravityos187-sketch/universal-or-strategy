# EPIC-W7-066 — Phase 1: Scope Definition

## Single Method in Scope

This phase targets exactly one **single method**: `RemoveFsmOrderIdMappings`, declared at
[`src/V12_002.Symmetry.BracketFSM.cs:103`](../../src/V12_002.Symmetry.BracketFSM.cs:103).

| Field | Value |
|---|---|
| **Method** | `RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)` |
| **Class** | `V12_002` (partial) |
| **Source File** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **Lines** | 103–125 |
| **Current CYC** | **10** |
| **Target CYC** | **≤ 8** |
| **Wave / Phase** | Wave 7 / Phase 1 |

---

## Scope Boundary

The **scope boundary** for this epic is strictly limited to the single method
`RemoveFsmOrderIdMappings`. No other methods, classes, or files are subject to
modification or refactoring within this phase. Everything outside
`RemoveFsmOrderIdMappings` in `src/V12_002.Symmetry.BracketFSM.cs`, and all other
source files in the `src/` directory, lie outside the scope boundary.

---

## Caller Count

A `grep` scan of `src/` for `RemoveFsmOrderIdMappings` returned **2 matches**:

| # | File | Line | Role |
|---|---|---|---|
| 1 | `src/V12_002.Symmetry.BracketFSM.cs` | 103 | **Definition** |
| 2 | `src/V12_002.Symmetry.BracketFSM.cs` | 135 | **Sole direct caller** (`TryTerminateFollowerBracket`) |

There is exactly **1 direct caller** of `RemoveFsmOrderIdMappings` in the entire
codebase: `TryTerminateFollowerBracket`, which calls it immediately after a successful
`_followerBrackets.TryRemove`. No other method calls `RemoveFsmOrderIdMappings` directly.

---

## Complexity Profile

`RemoveFsmOrderIdMappings` has **CYC = 10**, confirmed by the following 9 decision
points above the base of 1:

1. `if (fsm == null)` — null-guard on the FSM itself
2. `if (fsm.EntryOrder != null …)` — null-check on `EntryOrder` object
3. `&& !string.IsNullOrEmpty(fsm.EntryOrder.OrderId)` — empty-guard on `EntryOrder.OrderId`
4. `if (!string.IsNullOrEmpty(fsm.ReplacingCancelOrderId))` — bare-string empty-guard
5. `if (fsm.StopOrder != null …)` — null-check on `StopOrder` object
6. `&& !string.IsNullOrEmpty(fsm.StopOrder.OrderId)` — empty-guard on `StopOrder.OrderId`
7. `if (fsm.Targets == null)` — early-exit null-guard on the targets collection
8. `foreach (Order target in fsm.Targets)` — loop header decision
9. `if (target != null && !string.IsNullOrEmpty(target.OrderId))` — per-element null+empty guard (counts as 2 branches)

The **target CYC is ≤ 8**, achievable by extracting a `RemoveOrderIdIfPresent(Order)`
helper that collapses the repeated `null + IsNullOrEmpty` guard pairs from 2 branches
per field down to 1 logical call site, reducing net decision points by at least 2.

---

## Why Other Methods Are NOT in Scope

Per **rule V12.23** (single-method epic atomicity), each wave epic is scoped to exactly
one method as identified by the hotspot analysis in Phase 0. This constraint exists to:

- Keep diffs reviewable in a single pass.
- Prevent unintended complexity migration to adjacent methods.
- Ensure that CYC measurement and target verification are unambiguous.

The following methods in `src/V12_002.Symmetry.BracketFSM.cs` and across the broader
`src/` tree are **explicitly excluded** from this epic's scope boundary despite being
structurally related:

| Method | Reason Excluded |
|---|---|
| `TryTerminateFollowerBracket` (line 127) | Sole caller; not a hotspot; altering it risks caller-contract changes beyond V12.23 boundary |
| `SetFsmReplacing` (line 139) | Adjacent method; separate concern (replace-order bookkeeping) |
| `V12_002.REAPER.Audit` (transitive caller) | Transitive caller two hops away; outside V12.23 single-method boundary |
| `V12_002.Orders.Management.Cleanup` (transitive caller) | Same — transitive, not direct |
| Bypass sites in `SIMA.Dispatch`, `SIMA.Fleet`, `SIMA.Execution` | Consistency concern documented in hotspots; remediation is a separate epic (bypass consolidation) |

**V12.23 requires that refactoring effort not bleed into methods not named in the hotspot
report.** The Phase 0 hotspot report names only `RemoveFsmOrderIdMappings`; therefore
all other methods remain out of scope for this phase.

---

## Shared State (Read-Only Reference)

The following shared dictionaries are mutated by the in-scope method. They are listed
here for awareness; their declarations and write sites in other files are **not** part
of this epic's scope boundary:

| Dictionary | Type | Declared |
|---|---|---|
| `_orderIdToFsmKey` | `ConcurrentDictionary<string,string>` | `src/V12_002.cs:836` |

`_followerBrackets` is read by `TryTerminateFollowerBracket` (the caller) but is **not
mutated** by `RemoveFsmOrderIdMappings` itself.

---

## Agent Tracking

```
Agent Name:   v12-phase1-scope
Epic:         EPIC-W7-066
Wave:         7
Phase:        1 — Scope Definition
Status:       completed
Input:        00-hotspots.md
Output:       00-scope.md
CYC Current:  10
CYC Target:   <=8
Method:       RemoveFsmOrderIdMappings
File:         src/V12_002.Symmetry.BracketFSM.cs
Callers:      1 direct (TryTerminateFollowerBracket @ line 135)
Rule:         V12.23 — single method epic atomicity
```
