# EPIC-W7-097 Hotspot Analysis

**Method:** ExecuteRMAEntryV2
**CYC:** 0 (orchestrator shell; complexity distributed to extracted helpers)
**File:** src/V12_002.SIMA.Execution.cs

---

## Overview

`ExecuteRMAEntryV2` (line 686, `src/V12_002.SIMA.Execution.cs`) is the master RMA
(Return to Mean / Adaptive) limit-entry dispatcher for the V12 SIMA fleet. It places
a Limit entry order on the local chart account and then iterates `Account.All` to
replicate the same bracket across every active fleet account matching the prefix.

The method was refactored during a prior wave into four private helper methods —
`ValidateRMAEntryGuards`, `CalculateRMABracketPrices`, `SubmitLocalRMAEntry`, and
`ProcessSingleFleetRMAAccount` — reducing the orchestrator body to a straight-line
dispatch shell. The jcodemunch-reported CYC of **0** reflects this: the orchestrator
itself introduces no net-new decision branches beyond delegating to helpers. The
aggregate complexity carried by the four helpers is documented below.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Inbound callers** | `HandleChartClick_ExecuteRma` (`UI.Callbacks.cs:373`); IPC handler (`UI.IPC.Commands.Fleet.cs:454`) |
| **Dispatch gate** | Both callers enqueue via `Enqueue(ctx => ctx.ExecuteRMAEntryV2(...))` — strategy thread only |
| **Extracted helpers** | `ValidateRMAEntryGuards` (guard chain), `CalculateRMABracketPrices` (pricing), `SubmitLocalRMAEntry` (local ATOMIC), `ProcessSingleFleetRMAAccount` (per-fleet ATOMIC) |
| **Shared mutable state written** | `activePositions` (ConcurrentDictionary), `entryOrders` (ConcurrentDictionary), `_followerBrackets` (ConcurrentDictionary), `_orderIdToFsmKey` (ConcurrentDictionary) |
| **Locked state** | `AddExpectedPositionDeltaLocked` / `MarkDispatchSyncPending` protected by `stateLock` |
| **Symmetry subsystem** | `SymmetryGuardBeginDispatch`, `SymmetryGuardRegisterMasterEntry`, `SymmetryGuardRegisterFollower`, `SymmetryGuardRollbackDispatch` (`Symmetry.cs`) |
| **Downstream consumers** | REAPER (`expectedPositions` read on every tick), `OnAccountExecutionUpdate` (reads `_followerBrackets` on fill to trigger deferred bracket submission) |
| **MetadataGuard** | `MetadataGuardDuplicate` called inside `ValidateRMAEntryGuards` — duplicate-dispatch rejection gate |
| **Side-effects** | Writes to 4 ConcurrentDictionaries; updates `expectedPositions`; submits broker orders via `SubmitOrderUnmanaged` + `Account.Submit`; emits forensic log |
| **Threading constraint** | Strategy thread only (enqueued); `_followerBrackets`, `activePositions` enumerated/written lock-free outside `stateLock` per B966 ordering invariant |
| **Risk on change** | **High** — atomic ordering invariant (B966/923B-FIX-B: dicts → expectedPositions → Submit) must not be disturbed; Symmetry dispatch must remain balanced (begin/rollback pairs on every failure path) |

**Affected symbol count (blast radius):** 4 helper methods, 4 concurrent state bags,
3 Symmetry guard methods, 2 inbound callers, REAPER + OnAccountExecutionUpdate
downstream — **≥ 12 symbols directly coupled**.

---

## Top 3 Complexity Drivers

1. **Atomic ordering invariant across 4 helpers (B966 / 923B-FIX-B)**
   The most dangerous complexity is _structural_: `SubmitLocalRMAEntry` and
   `ProcessSingleFleetRMAAccount` both enforce a strict registration order —
   `activePositions`/`entryOrders` dictionaries must be populated **before**
   `AddExpectedPositionDeltaLocked` is called, which must complete **before**
   `acct.Submit` is issued. Violating this order creates a REAPER false-desync
   race window where `hasWorkingEntry=false` triggers a phantom-repair second Limit
   order. This invariant spans two helper boundaries and is not visible in the
   orchestrator's CYC count, making it the highest-impact hidden complexity driver.

2. **Symmetry guard lifecycle balanced across 5 failure paths**
   `SymmetryGuardBeginDispatch` is called once in the orchestrator. It must be
   matched by `SymmetryGuardRollbackDispatch` on: (a) `ValidateRMAEntryGuards`
   returning false (guard fires before `BeginDispatch` — no rollback needed there),
   (b) `SubmitLocalRMAEntry` throwing an exception (line 744), (c) `localSubmitted`
   returning false (line 757), and (d) the outer `catch` block (line 840).
   `SymmetryGuardRegisterMasterEntry` is called inside `SubmitLocalRMAEntry`, and
   `SymmetryGuardRegisterFollower` inside `ProcessSingleFleetRMAAccount`. Any
   refactoring that reorganises these failure paths must re-audit all 4 rollback sites.

3. **Non-deterministic Account.All iteration with per-account ATOMIC try/catch**
   The `foreach (Account acct in Account.All)` loop (line 773) is the remaining
   non-trivial decision node in the orchestrator shell. Each iteration delegates to
   `ProcessSingleFleetRMAAccount` which itself carries ~8 CYC (inactive guard,
   consistency lock check, null guard on `CreateOrder`, FSM duplicate check, delta
   reservation, Submit, OrderId registration, and full rollback catch). Because
   `Account.All` ordering is broker-defined and non-deterministic, partial-failure
   scenarios (some accounts OK, some FAIL) must be handled gracefully — the current
   design correctly allows per-account isolation but the `fleetOk`/`fleetSkip`
   counters are advisory-only (no abort-on-first-failure logic).

---

## Recommended Extraction Count

**0 additional extractions recommended for Phase 0.**

**Rationale:**

The orchestrator CYC=0 confirms the prior refactoring wave fully decomposed the method.
The four helpers cover all substantive logic:

- `ValidateRMAEntryGuards` — guard chain (flatten-guard, contracts≤0, price≤0, MetadataGuard)
- `CalculateRMABracketPrices` — ATR stop + 5-target ladder pricing + distribution sizing
- `SubmitLocalRMAEntry` — local account ATOMIC registration + SubmitOrderUnmanaged
- `ProcessSingleFleetRMAAccount` — per-fleet ATOMIC registration + `acct.Submit`

Phase 1 work should focus on: validating each helper's CYC independently, confirming
the B966 ordering invariant is enforced by a comment-contract or test, and ensuring
the Symmetry rollback sites remain exhaustive as new failure modes are introduced.

---

## Agent Tracking

Agent: Bob (Wave 7 | Phase 0)
Bobcoins Used: 1.0
Execution Time: ~90s
Output: docs/brain/EPIC-W7-097/00-hotspots.md
