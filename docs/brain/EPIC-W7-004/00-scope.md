# EPIC-W7-004 — Phase 1: Scope Definition

## Method in Scope

This epic targets a **single method** for cyclomatic-complexity reduction.

| Property         | Value                                                                                       |
|------------------|---------------------------------------------------------------------------------------------|
| **Method**       | `HandleFleetTargetFill`                                                                     |
| **Signature**    | `private void HandleFleetTargetFill(QueuedAccountExecution item, Order ocoOrder, Account ocoAcct, string ocoName)` |
| **Source File**  | `src/V12_002.UI.Compliance.cs`                                                              |
| **Lines**        | 624–696                                                                                     |
| **Current CYC**  | **34**                                                                                      |
| **Target CYC**   | **≤ 8**                                                                                     |
| **Reduction**    | ≥ 26 complexity points via 3 targeted method extractions                                    |

---

## Scope Boundary

The **scope boundary** for EPIC-W7-004 is precisely and exclusively `HandleFleetTargetFill` in
`src/V12_002.UI.Compliance.cs` (lines 624–696). No other method, class, or file falls inside
the refactoring perimeter for this epic. This is a **single method** scope.

---

## Caller Map

Symbol resolution via `jcodemunch search_symbols` (query: `"HandleFleetTargetFill"`,
repo: `universal-or-strategy`) and confirmed by static grep across `src/`:

| # | Caller | File | Line | Relationship |
|---|--------|------|------|--------------|
| 1 | `ProcessQueuedExecution_HandleFleetOCO` | `src/V12_002.UI.Compliance.cs` | 719 | **Direct caller** — 1 call site; dispatches on order-name prefix `T[n]_` |
| 2 | `ProcessQueuedExecution` | `src/V12_002.UI.Compliance.cs` | 799 | Indirect caller — routes all queued executions to the OCO handler |
| 3 | `OnAccountExecutionUpdate` | `src/V12_002.UI.Compliance.cs` | 401 | Root trigger — enqueues `QueuedAccountExecution` items that eventually reach this method |

**Callers count: 3** (1 direct, 2 indirect — all within `src/V12_002.UI.Compliance.cs`).

The narrow, single-file call chain means that the planned method extractions carry
**zero risk of signature-change cascade** outside `src/V12_002.UI.Compliance.cs`.

---

## Complexity Budget

| Metric              | Value |
|---------------------|-------|
| Current CYC         | 34    |
| Project threshold   | ≤ 10  |
| EPIC target CYC     | ≤ 8   |
| Excess branches     | ~26   |
| Planned extractions | 3     |

### Planned Extractions (from Phase 0 Hotspot Analysis)

| ID | Proposed Method | Responsibility |
|----|-----------------|----------------|
| E1 | `ParseTargetFillKey(string ocoName) → (int tgtNum, string tgtEntryKey)` | Isolates naming-convention string parsing; pure, no broker objects; unit-testable in isolation |
| E2 | `TryGetTargetPosition(string tgtEntryKey, out PositionInfo pos) → bool` | Wraps the `activePositions` guard; establishes a single null-check boundary |
| E3 | `CancelRemainingStopsForAccount(Account ocoAcct)` | Extracts the order-sweep `foreach` (lines 676–692) into a named, testable method matching the existing `CancelOrphanedTargets` pattern |

Post-extraction estimated CYC of `HandleFleetTargetFill`: **≤ 8**.

---

## Why Other Methods Are NOT in Scope (V12.23)

The **V12.23 No Scope Creep Protocol** mandates that each wave epic targets exactly one declared
hotspot method. The following candidates were explicitly considered and explicitly excluded:

| Excluded Method | CYC | Reason Excluded |
|-----------------|-----|-----------------|
| `ProcessQueuedExecution_HandleFleetOCO` | 18 | Wave 7 #2 hotspot — ranked below `HandleFleetTargetFill`; owns separate concerns; will be addressed in a dedicated future epic |
| `IsOrderAllowed` | 16 | Wave 7 #3 hotspot — unrelated order-validation logic; separate epic required |
| `ProcessQueuedExecution` | — | Indirect caller only; its complexity is not the declared Wave 7 hotspot |
| `CancelOrderOnAccount` | — | Callee helper; already within acceptable complexity range |
| `ApplyTargetFill` | — | Cross-file callee in `src/V12_002.Orders.Callbacks.cs`; stable `out`-param contract; untouched by this epic |

**Rule citation:** V12.23 No Scope Creep Protocol — a wave epic targets a single declared hotspot.
Expanding scope to multiple methods would invalidate the Phase 1.5 boundary gate and is prohibited.
Extracting helper methods E1–E3 from inside `HandleFleetTargetFill` is categorically different from
adding adjacent methods to scope; those helpers are new private methods that exist solely to reduce
the complexity of the single in-scope method.

---

## Cross-File Dependencies (Read-Only Context)

These symbols are called from within `HandleFleetTargetFill` but are **not modified** by this epic:

| Symbol | File | Note |
|--------|------|------|
| `ApplyTargetFill` | `src/V12_002.Orders.Callbacks.cs:47` | `out`-param contract preserved as-is |
| `CancelOrderOnAccount` | `src/V12_002.UI.Compliance.cs:573` | Called from extracted E3; signature unchanged |
| `activePositions` | `src/V12_002.cs` | `ConcurrentDictionary` read via `TryGetValue`; no mutation |
| `ocoAcct.Orders` | NinjaTrader.Cbi (external) | Defensive `.ToArray()` enumeration pattern preserved |

---

## Acceptance Criteria

- [ ] `HandleFleetTargetFill` post-refactor CYC ≤ 8 (verified by `jcodemunch get_symbol_complexity`)
- [ ] All 3 extracted helpers (`ParseTargetFillKey`, `TryGetTargetPosition`, `CancelRemainingStopsForAccount`) have CYC ≤ 4 individually
- [ ] Zero changes to any caller (`ProcessQueuedExecution_HandleFleetOCO`, `ProcessQueuedExecution`, `OnAccountExecutionUpdate`)
- [ ] Zero changes to cross-file callees (`ApplyTargetFill`, `CancelOrderOnAccount`)
- [ ] Existing test suite passes (no regressions)
- [ ] Scope boundary not exceeded — confirmed by Phase 1.5 boundary gate

---

## Agent Tracking

```
epic:            EPIC-W7-004
wave:            7
phase:           1
agent_name:      v12-phase1-scope
method:          HandleFleetTargetFill
cyc_current:     34
cyc_target:      <=8
source:          src/V12_002.UI.Compliance.cs
callers_count:   3
callers_direct:  1
callers_indirect: 2
scope_boundary:  single method — HandleFleetTargetFill only
output:          docs/brain/EPIC-W7-004/00-scope.md
status:          completed
```
