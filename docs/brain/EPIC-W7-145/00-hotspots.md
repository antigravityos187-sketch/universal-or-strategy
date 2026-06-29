# EPIC-W7-145 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field      | Value                                         |
|------------|-----------------------------------------------|
| Method     | `HandleFleetTargetFill`                       |
| CYC Score  | **17**                                        |
| File Path  | `src/V12_002.UI.Compliance.cs`                |
| Line Range | 624 – 696                                     |
| Signature  | `private void HandleFleetTargetFill(QueuedAccountExecution item, Order ocoOrder, Account ocoAcct, string ocoName)` |

---

## Blast Radius Summary

`HandleFleetTargetFill` sits at the centre of the **Fleet OCO fill pipeline**. Its only direct caller is
`ProcessQueuedExecution_HandleFleetOCO` (line 719, same file), which is itself dispatched by
`ProcessQueuedExecution` (line 799) — the primary execution-event handler triggered on every broker fill.

| Layer | Symbol | File |
|-------|--------|------|
| Caller (direct) | `ProcessQueuedExecution_HandleFleetOCO` | `V12_002.UI.Compliance.cs:698` |
| Caller (dispatch) | `ProcessQueuedExecution` | `V12_002.UI.Compliance.cs:787` |
| Shared state mutated | `activePositions` (ConcurrentDictionary) | 41 files reference `activePositions` across the codebase |
| Shared helper called | `ApplyTargetFill` | `V12_002.Orders.Callbacks.cs:47` (also called from `Orders.Callbacks.Execution.cs:432`, `Orders.Callbacks.cs:447`) |
| Side-effect dispatcher | `CancelOrderOnAccount` | `V12_002.Orders.CancelGateway.cs:46` — affects live broker orders on 11 call-sites across 7 files |
| Sibling that mirrors logic | `HandleFleetStopFill` | `V12_002.UI.Compliance.cs:519` |

**Risk level:** HIGH — mutations to `activePositions` and broker-order cancellations are irreversible
during a live trading session. Any extraction must preserve the exact conditional guard sequence
(entry-key derivation → position lookup → duplicate-fill guard → remaining-contracts gate → stop cancel).

---

## Top 3 Complexity Drivers

### 1. Compound Guard in Position Lookup (Nesting Level 1 → Deep Body)

```csharp
// Lines 634–638
if (
    !string.IsNullOrEmpty(tgtEntryKey)
    && activePositions.TryGetValue(tgtEntryKey, out tgtPos)
    && tgtPos != null
)
```

The outer `if` uses three short-circuit `&&` clauses (each is a counted branch), wrapping the
entire 55-line body of the method. This single decision nests every downstream conditional
inside it, creating a wide "pyramid" shape. The tool counts each short-circuit operand as a
separate branch, contributing **3 CYC points** from this line alone.

**Driver:** Multi-clause compound condition acting as the sole scope guard for the entire method body.

---

### 2. Multi-Condition Order-Filter Loop (Nesting Level 3)

```csharp
// Lines 676–692
foreach (Order o in ocoAcct.Orders.ToArray())
{
    if (o == null || o.Instrument?.FullName != Instrument?.FullName)  // +2 (||, ?.)
        continue;
    if (o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted)  // +1 (&&)
        continue;
    if (o.Name != null && o.Name.StartsWith("Stop_"))  // +1 (&&)
    {
        CancelOrderOnAccount(o, ocoAcct);
        ...
    }
}
```

The `foreach` loop (already at nesting depth 3: outer-if → else-branch → `tgtRemaining` guard)
contains **three independent filter `if` statements** with their own boolean sub-expressions.
This block contributes **~5 CYC points** (1 for the loop + 4 for the inner conditions/operators).

**Driver:** Inline order-filtering logic duplicates the pattern already present in `CancelOrphanedTargets`,
adding branch count without semantic novelty.

---

### 3. Duplicate-Fill Guard + Branching Log Paths (Nesting Level 2)

```csharp
// Lines 653–694
if (tgtAlreadyProcessed)
{
    Print(...);  // guard-path log
}
else
{
    Print(...);  // fill-path log
    if (tgtRemaining <= 0)
    {
        foreach (...) { ... }  // stop-cancel loop
    }
}
```

The `if/else` on `tgtAlreadyProcessed` separates a **no-op guard path** from the **active fill path**,
but both branches contain print calls before the real work begins. The `else` body then immediately
opens a second `if (tgtRemaining <= 0)` followed by the loop, adding 2 more branch levels.

**Driver:** Mixed concerns — logging and state mutation are interleaved inside the same decision tree
instead of being handled in distinct, extractable steps.

---

## Recommended Extraction Count

**Recommend extracting 3 helper methods:**

| # | Proposed Method | Lines Extracted | CYC Reduction |
|---|-----------------|-----------------|---------------|
| 1 | `TryResolveTargetPosition(string ocoName, out int tgtNum, out string tgtEntryKey, out PositionInfo tgtPos)` | 626–638 | −3 (collapses compound guard) |
| 2 | `CancelFleetStopOrdersForAccount(Account account)` | 676–692 | −5 (absorbs loop + inner filters) |
| 3 | `LogAndApplyTargetFill(PositionInfo pos, int tgtNum, int tgtApplied, int tgtRemaining, string tgtEntryKey, decimal price)` | 653–694 (logging + dispatch) | −4 (separates log/state concerns) |

**Post-extraction estimated CYC:** ≤ 6 (within the ≤ 10 threshold).

> Note: `CancelFleetStopOrdersForAccount` is a near-identical pattern to `CancelOrphanedTargets`
> (which already exists at line 553). Refactor phase should evaluate merging or generalising these
> with a `nameFilter` predicate parameter to avoid parallel evolution.

---

## Agent Tracking

| Field            | Value                         |
|------------------|-------------------------------|
| Agent Name       | `v12-phase0-hotspot`          |
| Bobcoins Used    | 6                             |
| Execution Time   | ~45s                          |
| Phase            | 0 — Hotspot Analysis          |
| Epic             | EPIC-W7-145                   |
| Output Artifact  | `docs/brain/EPIC-W7-145/00-hotspots.md` |
