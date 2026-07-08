# EPIC-W7-021 — Phase 1: Scope Definition

> Wave 7 | Phase 1 — Scope Definition (REDO) | Method: `ProcessOnOrderUpdate` | Source: `src/V12_002.Orders.Callbacks.cs`

---

## 1. Single Method in Scope

This epic targets exactly one **single method**: `ProcessOnOrderUpdate`.

| Field              | Value                                          |
|--------------------|------------------------------------------------|
| **Method**         | `ProcessOnOrderUpdate`                         |
| **File**           | `src/V12_002.Orders.Callbacks.cs`              |
| **Lines**          | 245–294 (50 LOC body)                          |
| **Visibility**     | `private`                                      |
| **Class**          | `V12_002` (partial class, `Strategy`)          |
| **Current CYC**    | **16**                                         |
| **Target CYC**     | **≤ 8**                                        |
| **Callers Count**  | **1** — `OnOrderUpdate` (line 192, same file)  |

---

## 2. Scope Boundary

The **scope boundary** for EPIC-W7-021 is precisely defined as:

> **Only `ProcessOnOrderUpdate` and any new private helper methods extracted from its body.**

No existing sibling methods, downstream callees, upstream callers, or infrastructure files
are within the scope boundary. The scope boundary is intentionally narrow — this is a
**single method** extraction refactor only. All code that currently lives *outside* the body
of `ProcessOnOrderUpdate` (lines 245–294) is explicitly excluded from modification.

Planned extractions (`DispatchOrderStateRouting`, `ApplyPricePropagationIfNeeded`,
`ExecuteOrderUpdateCore`) will be created from code already residing *inside*
`ProcessOnOrderUpdate` and will be placed in the same file as `private` methods.
No cross-file changes, no public API changes, no signature changes to `ProcessOnOrderUpdate`.

---

## 3. Caller Analysis

The grep search across all `src/*.cs` files confirmed:

| Caller          | File                               | Line | Mechanism                                               |
|-----------------|------------------------------------|------|---------------------------------------------------------|
| `OnOrderUpdate` | `src/V12_002.Orders.Callbacks.cs`  | 192  | `Enqueue(ctx => ctx.ProcessOnOrderUpdate(...))` — actor drain lambda |

**Callers count: 1.**

`OnOrderUpdate` (lines 168–193) is the NT8 platform override — a thin shell that captures
order primitives and enqueues a lambda. `ProcessOnOrderUpdate` is the drain-side actor handler;
it is never invoked directly from any other site. The `_histProcessOnOrderUpdate` field at
`src/V12_002.cs:846` is a `LatencyHistogram` metric infrastructure field referenced from
*within* `ProcessOnOrderUpdate`'s own `finally` block — it is not a caller.

No other method anywhere in the codebase calls `ProcessOnOrderUpdate`, making the upstream
coupling surface zero. Refactoring the drain-side body carries no upstream breakage risk.

---

## 4. Why Other Methods Are NOT in Scope (V12.23)

Per the V12.23 single-method refactor rule, only the identified hotspot method is eligible
for modification in any given epic phase. All methods identified as callees in the Phase 0
blast-radius analysis — `HandleOrderState_Filled`, `HandleOrderState_Terminal`,
`HandleOrderState_Working`, `ShouldPropagatePriceMove`, `IsTerminalState`,
`PropagateMasterPriceMove`, `RemoveGhostOrderRef`, `HandleEntryOrderFilled`,
`HandleSecondaryOrderFilled`, `HandleOrderRejected`, `HandleOrderCancelled`, and all
transitive dependents — are **not in scope** for the following reasons:

1. **V12.23 single-method discipline:** Each epic targets the one hotspot method identified
   by the CYC audit. Touching callees simultaneously would conflate two separate refactor
   intents, making verification of functional equivalence unreliable and diff review unwieldy.

2. **Blast-radius containment:** The nine cross-file callee dependencies identified in Phase 0
   carry CRITICAL blast-radius classification. Modifying them under this epic would widen
   the change surface beyond what can be safely verified in a single wave phase.

3. **Extraction-only strategy:** The three planned extractions (E-1, E-2, E-3) move code
   *from inside* `ProcessOnOrderUpdate` *to new private methods*. They do not alter the
   behaviour or signatures of any existing callee. No callee needs modification to achieve
   the CYC 16 → ≤ 8 target.

4. **Separate CYC budgets:** Callees such as `HandleOrderState_Terminal` (CYC ≈ 4) and
   `HandleSecondaryOrderFilled` (CYC ≈ 5) are within acceptable thresholds and are not
   wave-7 hotspots. They belong to separate epics if and when they breach threshold.

---

## 5. Complexity Reduction Plan (from Phase 0)

| #   | Extraction                        | Proposed Name                          | Lines Extracted | CYC Reduction |
|-----|-----------------------------------|----------------------------------------|-----------------|---------------|
| E-1 | State dispatch block              | `DispatchOrderStateRouting(...)`       | 271–282         | −7 from body  |
| E-2 | Price propagation pre-check       | `ApplyPricePropagationIfNeeded(...)`   | 263–266         | −4 from body  |
| E-3 | Latency instrumentation frame     | `ExecuteOrderUpdateCore(...)`          | 260–293 (inner) | −3 structural |

After all three extractions, `ProcessOnOrderUpdate` becomes a ≤5-line orchestrator targeting
**CYC ≤ 4**, well under the ≤ 8 target.

---

## 6. Dependency Map (out-of-scope reference)

```
ProcessOnOrderUpdate  [IN SCOPE — body only]
├── calls (OUT OF SCOPE — must not be modified)
│   ├── ShouldPropagatePriceMove        Callbacks.cs:196
│   ├── PropagateMasterPriceMove        Callbacks.Propagation.cs:37
│   ├── HandleOrderState_Filled         Callbacks.cs:207
│   ├── HandleOrderState_Terminal       Callbacks.cs:222
│   ├── HandleOrderState_Working        Callbacks.cs:234
│   ├── IsTerminalState                 Callbacks.cs:240
│   ├── RemoveGhostOrderRef             Orders.Management.Cleanup.cs:254
│   └── LatencyProbe / _hist*           infrastructure (V12_002.cs)
└── called by (OUT OF SCOPE — must not be modified)
    └── OnOrderUpdate (line 192)        Callbacks.cs
```

---

## 7. Risk Assessment

| Risk                | Assessment                                                                              |
|---------------------|-----------------------------------------------------------------------------------------|
| **Concurrency**     | None. Drain runs single-threaded in actor queue; extracted helpers share same context.  |
| **Signature**       | None. `ProcessOnOrderUpdate` is `private` with exactly 1 call-site; signature unchanged.|
| **Functional**      | Extractions are parameter-passing only; no shared mutable state crosses boundaries.     |
| **Cross-file**      | None. All extractions stay in `src/V12_002.Orders.Callbacks.cs`.                        |

---

## Agent Tracking

| Field                       | Value                                             |
|-----------------------------|---------------------------------------------------|
| **Agent Name**              | v12-phase1-scope                                  |
| **Epic**                    | EPIC-W7-021                                       |
| **Wave**                    | 7                                                 |
| **Phase**                   | 1 — Scope Definition                              |
| **Method**                  | `ProcessOnOrderUpdate`                            |
| **CYC Current**             | 16                                                |
| **CYC Target**              | ≤ 8                                               |
| **Source File**             | `src/V12_002.Orders.Callbacks.cs`                 |
| **Callers Count**           | 1 (`OnOrderUpdate` line 192)                      |
| **Single Method Confirmed** | ✅ `ProcessOnOrderUpdate` only                    |
| **Scope Boundary Locked**   | ✅ No other existing methods in scope             |
| **V12.23 Rule Applied**     | ✅ All callees explicitly excluded                |
| **Output File**             | `docs/brain/EPIC-W7-021/00-scope.md`              |
