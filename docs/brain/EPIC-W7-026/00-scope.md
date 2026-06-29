# EPIC-W7-026 — Phase 1: Scope Definition

## Epic Metadata

| Field | Value |
|---|---|
| **Epic ID** | EPIC-W7-026 |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Status** | Completed |

---

## Single Method in Scope

This epic targets a **single method** only:

| Attribute | Value |
|---|---|
| **Method** | `ProcessQueuedAccountOrder` |
| **File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **Lines** | 1054–1101 |
| **Current CYC** | **17** |
| **Target CYC** | **≤ 8** |
| **CYC Reduction Required** | ≥ 9 decision nodes removed from the parent body |

The **scope boundary** for this epic is the body of `ProcessQueuedAccountOrder` (lines 1054–1101)
plus the three extraction-target helper methods that will be carved out of it during Phase 2. No
other method definitions are modified, added, or deleted within this epic.

---

## Caller Analysis

| Attribute | Value |
|---|---|
| **Callers count** | **1** |
| **Caller name** | `ProcessAccountOrderQueue` |
| **Caller location** | `src/V12_002.Orders.Callbacks.AccountOrders.cs`, line 222 |
| **Call context** | Drain loop — single call per dequeued `QueuedAccountOrderUpdate` item |

The method is the sole consumer of every queued `OnAccountOrderUpdate` event. The single call site
limits the blast radius at the interface level: no external assembly, no secondary consumer.

---

## Why Other Methods Are NOT in Scope

The V12.23 release policy restricts each refactor epic to a **single method** per wave phase.
Expanding scope beyond `ProcessQueuedAccountOrder` would violate this policy for the following
reasons:

1. **`ProcessFollowerCancellationUnconditional` (CYC = 12)** — This callee contributes 7
   transitive decision nodes to the parent's reported CYC. However, the V12.23 single-method
   constraint prohibits refactoring it within the same epic. Its transitive cost is addressed
   indirectly: Phase 2 extractions reduce the *parent's* dependency on the raw call by wrapping
   the gate behind a named, documented helper contract, making the transitive complexity auditable
   without modifying the callee body.

2. **`HandleMatchedFollowerOrder` (CYC = 10)** — Touches the FollowerBracket FSM and is used
   by other call paths outside this epic's source method. Modifying it risks regressions in
   sibling code paths not covered by this epic's validation scope.

3. **`ExecuteFollowerCascadeCleanup`** — Contains production-safety logic
   (`EmergencyFlattenSingleFleetAccount` via `TriggerCustomEvent`). The V12.23 policy explicitly
   prohibits touching cascade-flatten paths within a complexity-reduction epic to avoid inadvertent
   broadening of the emergency-flatten condition.

4. **`TryFindOrderInPosition`** — Utility method shared across multiple consumers. Out of scope
   per the single-method constraint and because no complexity reduction in the parent depends on
   changing its implementation.

5. **All other methods in the file** — `src/V12_002.Orders.Callbacks.AccountOrders.cs` contains
   additional methods ranked lower than `ProcessQueuedAccountOrder` in the Phase 0 hotspot
   analysis. None of these are in scope; they are candidates for future waves under V12.23.

The **scope boundary** is therefore hard: Phase 2 implementation work touches only
`ProcessQueuedAccountOrder` and the three new extraction helper stubs created from its body.
No existing callee method signatures or bodies are modified.

---

## Complexity Reduction Plan (Summary)

| Extraction | Lines | CYC Reduction from Parent |
|---|---|---|
| `TryMatchFollowerPositionInSnapshot` (scan loop + compound predicate) | 1081–1095 | −4 |
| `DispatchMatchedFollowerResult` (matched/unmatched dispatch branch) | 1097–1100 | −2 |
| `IsValidQueuedOrderForThisInstrument` (merged null-guards) | 1056–1060 | −1 |

**Projected parent CYC post-extraction: 5–6** (well within the ≤ 8 target).

The three new helper methods are themselves simple (CYC ≤ 3 each) and do not introduce new
hotspots. Their bodies are extracted verbatim from the parent — no logic changes, preserving
behavioral equivalence.

---

## Scope Confirmation

- **Single method** targeted: `ProcessQueuedAccountOrder` ✓
- **Scope boundary** defined: lines 1054–1101 plus three new extraction helpers only ✓
- Current CYC: **17** / Target CYC: **≤ 8** ✓
- Callers count: **1** (`ProcessAccountOrderQueue`, line 222) ✓
- V12.23 single-method policy respected: all other methods excluded from scope ✓

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase1-scope |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Epic** | EPIC-W7-026 |
| **Source File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **Method in Scope** | `ProcessQueuedAccountOrder` |
| **Current CYC** | 17 |
| **Target CYC** | ≤ 8 |
| **Callers Count** | 1 |
| **Scope Policy** | V12.23 single-method constraint |
| **Completed At** | 2026-07-01T00:00:00Z |
