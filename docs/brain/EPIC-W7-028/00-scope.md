# EPIC-W7-028 — Phase 1: Scope Definition

## Single Method in Scope

**`ProcessFlattenWorkItem_CancelOrders`**

This epic targets exactly one single method. The scope boundary is drawn tightly around `ProcessFlattenWorkItem_CancelOrders` and does not extend to any sibling, caller, or downstream method in the flatten pipeline.

---

## Source Location

| Field | Value |
|-------|-------|
| **Source file** | `src/V12_002.SIMA.Flatten.cs` |
| **Definition line** | 191 |
| **Method body lines** | 191–238 |
| **Visibility** | `private void` |
| **Class** | `partial class V12_002 : Strategy` |

---

## Complexity Targets

| Metric | Value |
|--------|-------|
| **Current CYC** | **9** (manual McCabe count per `00-hotspots.md`; index CYC is 0 due to parse gap in `precomputed.json`) |
| **Target CYC** | **≤ 8** (project threshold) |
| **Post-extraction estimate** | **3** (base 1 + outer foreach 1 + ZombieSweepOnly dispatch 1) |

The index-reported CYC of 0 is a known parse gap. The confirmed CYC of 9 was established by manual McCabe count across lines 191–238: 1 (base) + 1 (foreach) + 1 (null/instrument guard) + 1 (isTerminal compound) + 1 (ZombieSweepOnly branch) + 1 (isZombieTarget compound) + 1 (!isZombieTarget guard) + 1 (ordersToCancel.Count > 0) + 1 (OR term in null guard) = **9**.

---

## Callers

Direct callers found by `grep src/` for `ProcessFlattenWorkItem_CancelOrders`:

| # | Call Site | Caller Method | File | Line |
|---|-----------|---------------|------|------|
| 1 | Primary async path | `PumpFlattenOps` | `src/V12_002.SIMA.Flatten.cs` | 143 |
| 2 | Fallback drain path | `PerformFallbackFlatten` | `src/V12_002.SIMA.Flatten.cs` | 354 |

**Callers count: 2** (both within the same source file; no cross-file callers).

---

## Scope Boundary

The scope boundary for this epic is `ProcessFlattenWorkItem_CancelOrders` in its entirety (lines 191–238 of `src/V12_002.SIMA.Flatten.cs`). Everything outside that boundary — including its callers, the `FlattenWorkItem` struct definition, the flatten queue, and any shared state — is read-only context. No changes will be made to caller signatures, caller logic, shared fields, or any other method in the file.

This is a single method refactor. The two proposed helper extractions (`IsOrderEligibleForCancel` and `BuildZombieCancelList`) will be defined as new `private static` methods within the same `partial class`; they are outputs of the refactor, not additional items in scope for complexity analysis.

---

## Why Other Methods Are NOT in Scope

Version constraint **V12.23** governs this codebase. Under V12.23 refactor rules:

1. **`PumpFlattenOps`** — A caller, not the target. Its CYC (~8) is at the project threshold but is not the hotspot selected by EPIC-W7-028. Touching it would widen the scope boundary beyond the single method defined in this epic.

2. **`PerformFallbackFlatten`** — A caller, not the target. Its CYC (~6) is below the project threshold. No refactor justification under V12.23.

3. **`ChainNextFlattenOp`** — CYC ~5; below threshold. Not referenced by the hotspot analysis as a target.

4. **`FlattenAllApexAccounts`** — CYC ~7; below threshold. Not selected as a target.

5. **All other flatten pipeline methods** (`ClosePositionsOnlyApexAccounts`, `EmergencyFlattenSingleFleetAccount`, etc.) — None exceed CYC 9 and none are referenced in the hotspot selection for this wave.

6. **Cross-file consumers** (19 files in indirect blast radius) — These files read shared state (`isFlattenRunning`, `_pendingFlattenOps`) that `ProcessFlattenWorkItem_CancelOrders` does not write. They are impacted by the flatten pipeline's correctness but not by the internal branching structure of the single method being refactored.

V12.23 mandates that each epic addresses one hotspot method per phase cycle. Broadening scope to additional methods requires a separate epic allocation and wave planning entry.

---

## Extraction Plan (Preview for Phase 2)

| # | Helper | Absorbs | Estimated CYC |
|---|--------|---------|---------------|
| 1 | `IsOrderEligibleForCancel(Order order, string instrumentFullName)` | Null guards (lines 196–199) + five-term terminal-state filter (lines 201–208) | ≤ 5 |
| 2 | `BuildZombieCancelList(IEnumerable<Order> orders, string instrumentFullName)` | `ZombieSweepOnly` branch + 6-prefix `StartsWith` matching (lines 210–221) | ≤ 5 |

Both helpers are `private static` (no `this` access required), zero new heap allocations beyond the existing `List<Order>`.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase1-scope |
| **Wave** | 7 |
| **Epic** | EPIC-W7-028 |
| **Phase** | 1 — Scope Definition |
| **Output** | `docs/brain/EPIC-W7-028/00-scope.md` |
| **Source** | `src/V12_002.SIMA.Flatten.cs` |
| **Single Method** | `ProcessFlattenWorkItem_CancelOrders` |
| **Current CYC** | 9 (confirmed manual McCabe; index CYC 0 is a parse gap) |
| **Target CYC** | ≤ 8 |
| **Callers Count** | 2 |
| **Scope Boundary** | Lines 191–238, `src/V12_002.SIMA.Flatten.cs` only |
| **V12 Constraint** | V12.23 — single method per epic, no cross-method scope creep |
