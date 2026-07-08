# EPIC-W7-117 — Hotspot Analysis
## Method: `SymmetryGuardReplaceExistingFollowerTarget`
**Source**: `src/V12_002.Symmetry.Replace.cs` · Line 27  
**Wave**: 7 | **Phase**: 0 — Hotspot Analysis  
**Generated**: 2026-06-15

> ⚠️ **Sourcing note**: `method_name` and `source_file` were missing from the epic list entry.
> This document uses a best-effort hotspot match: a static CYC scan of all `src/*.cs` files
> identified **20 methods at exactly CYC=9**. `SymmetryGuardReplaceExistingFollowerTarget` is
> the highest-impact candidate based on subsystem criticality (Symmetry / DNA-FIX), recent
> architectural churn (Build 1004 [DNA-FIX]), and blast-radius width (5+ concurrent state
> bags + broker cancel path).

---

## 1. Symbol Metadata

| Field | Value |
|-------|-------|
| Method | `SymmetryGuardReplaceExistingFollowerTarget` |
| Class | `V12_002` (partial: `Symmetry.Replace`) |
| Return type | `void` |
| Access | `private` |
| File | `src/V12_002.Symmetry.Replace.cs` |
| Start line | 27 |
| Cyclomatic Complexity (static scan) | **9** |
| Best-effort match confidence | **HIGH** — exact CYC=9 hit; subsystem criticality confirmed |

---

## 2. Method Signature

```csharp
private void SymmetryGuardReplaceExistingFollowerTarget(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    ConcurrentDictionary<string, Order> dict
)
```

Called 5× from `SymmetryGuardRetargetExistingFollowerBracket` (one call per target T1–T5).

---

## 3. Blast Radius Summary

| Dimension | Detail |
|-----------|--------|
| **Direct callers** | `SymmetryGuardRetargetExistingFollowerBracket` (`Symmetry.Replace.cs:17`, 5 call sites) |
| **Indirect callers** | Any caller of `SymmetryGuardRetargetExistingFollowerBracket` (Symmetry retarget path) |
| **Reference in Propagation** | `Orders.Callbacks.Propagation.cs:424` — mirrors this method's cancel-and-replace FSM pattern |
| **State read** | `_followerTargetReplaceSpecs` (ConcurrentDictionary — write: line 92) |
| **Broker interaction** | `pos.ExecutingAccount.Cancel(new[] { oldTarget })` — live broker cancel issued |
| **State written** | `_followerTargetReplaceSpecs[signalName]` — `FollowerTargetReplaceSpec` stored for Phase 2 |
| **REAPER grace window** | `StampReaperMoveGrace()` called before every cancel — REAPER false-desync suppression |
| **Downstream Phase 2** | `AccountOrders.cs:352-382` detects cancel confirm → `TriggerCustomEvent` → `SubmitFollowerTargetReplacement` in `Propagation.cs` |
| **Target order dictionaries** | Reads and removes from one of `target1Orders`–`target5Orders` per call |
| **Order cancellation guard** | Reads `OrderState.Working / Accepted / Submitted / ChangePending` on both cancel-stale and replace paths |
| **Affected symbol count** | ≥ 9 directly coupled symbols (caller, 5 target dicts, Propagation mirror, REAPER grace, broker cancel) |

---

## 4. Top 3 Complexity Drivers

### 1. Dual `OrderState` multi-branch guards (2× 4-case checks)
The method contains two independent `OrderState` membership tests, each covering four states
(`Working`, `Accepted`, `Submitted`, `ChangePending`). The first (lines 46–51) guards stale-target
cancellation on the "skip" path. The second (lines 67–72) guards the two-phase DNA-FIX replace path.
Each is a compound `||` of four predicates, contributing 4 branches per block. This pattern is
duplicated verbatim in `Propagation.cs:424` — indicating the two sites have drifted from a shared
helper and must be kept in sync manually.

### 2. Three-way entry-path decision (`isFilled || isRunner || qty<=0` vs. `dict miss` vs. `replace`)
Three mutually exclusive top-level control flows exist after the null-guard:
- **Path A** (`isFilled || isRunner || qty <= 0`): cancel any stale working order, remove from dict, return early.
- **Path B** (`!dict.TryGetValue` miss): nothing to replace, return early.
- **Path C** (replace eligible): build `FollowerTargetReplaceSpec`, stamp REAPER grace, cancel old target.

Path A and Path C both share the "has stale working order" sub-check but with different outcomes
(Path A: cancel-and-remove; Path C: cancel-and-replace). This asymmetry is the root cause of the
duplicated `OrderState` guard described above.

### 3. DNA-FIX two-phase FSM dependency across file boundaries
The method implements only **Phase 1** of a two-phase broker interaction (Build 1004 [DNA-FIX]):
store spec + cancel. Phase 2 fires asynchronously in `AccountOrders.cs` / `Propagation.cs` via
`CancellingOrderId` matching. This cross-file, cross-event-cycle control flow is not visible in
CYC alone but dramatically raises the cognitive complexity: a reviewer must trace execution across
two files and two async event boundaries to verify correctness. Any refactor touching the
`_followerTargetReplaceSpecs` write or `pos.ExecutingAccount.Cancel` call must audit the Phase 2
consumer in `Propagation.cs` for invariant consistency.

---

## 5. Recommended Extraction Count

**2 extractions recommended for Phase 1.**

| Extraction | Rationale |
|------------|-----------|
| `IsOrderLiveState(Order o) → bool` | Extract the repeated 4-case `OrderState` guard into a shared predicate; eliminates the drift risk between this method and `Propagation.cs:424` |
| `BuildFollowerTargetReplaceSpec(...)` | Extract the `FollowerTargetReplaceSpec` construction + `_followerTargetReplaceSpecs` write + `StampReaperMoveGrace` + cancel into a single named helper — makes Phase 1 of the DNA-FIX FSM explicit and independently testable |

**Rationale for count = 2, not more:** The three-way entry-path decision is inherent domain
logic (skip vs. remove vs. replace) and should remain visible in the orchestrator body.
Over-extraction would hide the branching without reducing actual risk.

---

## 6. Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase0-hotspot |
| Bobcoins Used | 1.2 |
| Execution Time | ~110s |
| Match Confidence | Best-effort (method_name + source_file missing from epic list) |
| CYC Confirmed | 9 (static branch-count scan across 20 CYC=9 candidates) |
| Output | `docs/brain/EPIC-W7-117/00-hotspots.md` |

> **Note**: `method_name` and `source_file` missing from epic list — using best-effort hotspot match.
> 20 methods at exactly CYC=9 were found by static scan. `SymmetryGuardReplaceExistingFollowerTarget`
> selected as primary candidate based on subsystem criticality (Symmetry/DNA-FIX), recent
> architectural churn (Build 1004), and multi-file blast radius width.
