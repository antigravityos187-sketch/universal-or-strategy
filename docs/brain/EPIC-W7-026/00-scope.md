# Phase 1: Scope Definition - EPIC-W7-026

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-23T02:39:29Z (Phase 0 baseline), Phase 1 derived from Phase 0 hotspot
- **Epic ID**: EPIC-W7-026

---

## Method Under Refactoring

| Attribute             | Value                                                              |
|-----------------------|--------------------------------------------------------------------|
| **Method**            | `ProcessQueuedAccountOrder`                                        |
| **File**              | `src/V12_002.Orders.Callbacks.AccountOrders.cs`                    |
| **Line**              | 1054                                                               |
| **Signature**         | `private void ProcessQueuedAccountOrder(QueuedAccountOrderUpdate item)` |
| **Current CYC**       | 17                                                                 |
| **Target CYC**        | ≤ 8                                                                |
| **LOC**               | 48                                                                 |
| **Max Nesting Depth** | 3                                                                  |

### Complexity Breakdown (CYC 17 source)

The cyclomatic complexity of 17 is produced entirely within the method body (lines 1054–1101) by the following decision points:

| Decision Construct                                                                               | Points |
|--------------------------------------------------------------------------------------------------|--------|
| `item.EventArgs == null \|\| item.EventArgs.Order == null`                                       | +2     |
| `order.Instrument != null && order.Instrument.FullName != Instrument.FullName`                   | +2     |
| `item.Account != null ? ...` (ternary in `acctName`)                                             | +1     |
| `if (ProcessFollowerCancellationUnconditional(...))`                                             | +1     |
| `foreach (var kvp in snapshot)`                                                                  | +1     |
| `if (!activePositions.ContainsKey(kvp.Key))`                                                    | +1     |
| `!pos.IsFollower \|\| pos.ExecutingAccount == null \|\| pos.ExecutingAccount != item.Account`    | +3     |
| `if (TryFindOrderInPosition(...))`                                                               | +1     |
| `!string.IsNullOrEmpty(matchedEntry) && matchedPos != null && activePositions.ContainsKey(...)` | +3     |
| Base                                                                                             | +1     |
| **Total**                                                                                        | **17** |

---

## IN SCOPE — Extractions

The following three logical segments within `ProcessQueuedAccountOrder` are candidates for extraction into private helper methods. Each extraction reduces the decision-point count in the orchestrating method and creates a testable, named sub-responsibility.

### Extraction 1 — `IsQueuedOrderUpdateValid`

**Lines affected**: 1056–1060 (early-return guard block)

**Logic to extract**:
```csharp
// Guard: null event args / order
if (item.EventArgs == null || item.EventArgs.Order == null)
    return;
// Guard: instrument mismatch
if (order.Instrument != null && order.Instrument.FullName != Instrument.FullName)
    return;
```

**Proposed signature**:
```csharp
private bool IsQueuedOrderUpdateValid(QueuedAccountOrderUpdate item, Order order)
```

**CYC contribution removed from caller**: 4 (two `||`/`&&` compound conditions)

**Rationale**: The two guard clauses (null-event-arg check and instrument mismatch check) form a cohesive validation responsibility. Naming them makes the preconditions explicit and removes 4 decision points from the orchestrator.

---

### Extraction 2 — `TryMatchFollowerOrderInSnapshot`

**Lines affected**: 1079–1095 (snapshot scan / follower identity resolution loop)

**Logic to extract**:
```csharp
var snapshot = activePositions.ToArray();
string matchedEntry = null;
PositionInfo matchedPos = null;
foreach (var kvp in snapshot)
{
    if (!activePositions.ContainsKey(kvp.Key))
        continue;
    PositionInfo pos = kvp.Value;
    if (!pos.IsFollower || pos.ExecutingAccount == null || pos.ExecutingAccount != item.Account)
        continue;
    if (TryFindOrderInPosition(order, kvp.Key, out matchedEntry))
    {
        matchedPos = pos;
        break;
    }
}
```

**Proposed signature**:
```csharp
private bool TryMatchFollowerOrderInSnapshot(
    Order order,
    NinjaTrader.Cbi.Account account,
    out string matchedEntry,
    out PositionInfo matchedPos,
    out KeyValuePair<string, PositionInfo>[] snapshot)
```

**CYC contribution removed from caller**: 6 (`foreach` +1, `ContainsKey` guard +1, three `||`/`&&` in follower filter +3, `TryFindOrderInPosition` gate +1)

**Rationale**: The snapshot allocation, follower-account filter, and order-identity search are a single coherent lookup responsibility. Extracting them collapses the loop entirely out of the orchestrator and brings the sub-method CYC to ≤ 6 on its own.

---

### Extraction 3 — `DispatchMatchedOrUnmatchedFollower`

**Lines affected**: 1097–1100 (terminal dispatch branch)

**Logic to extract**:
```csharp
if (!string.IsNullOrEmpty(matchedEntry) && matchedPos != null && activePositions.ContainsKey(matchedEntry))
    HandleMatchedFollowerOrder(matchedEntry, matchedPos, order, acctName, reason);
else
    ExecuteFollowerCascadeCleanup(EnableSIMA, order, reason, snapshot);
```

**Proposed signature**:
```csharp
private void DispatchMatchedOrUnmatchedFollower(
    string matchedEntry,
    PositionInfo matchedPos,
    Order order,
    string acctName,
    string reason,
    KeyValuePair<string, PositionInfo>[] snapshot)
```

**CYC contribution removed from caller**: 3 (three `&&` compound conditions in the dispatch guard)

**Rationale**: The dispatch decision—whether an order matched a live position or should cascade-clean—is a named routing responsibility. Extracting it eliminates 3 decision points from the orchestrator and makes the routing rule readable.

---

## Projected Post-Refactor CYC for `ProcessQueuedAccountOrder`

| Before | Extractions Remove | After |
|--------|--------------------|-------|
| 17     | −4 (Extraction 1)  |       |
|        | −6 (Extraction 2)  |       |
|        | −3 (Extraction 3)  |       |
| **17** | **−13**            | **4** |

Residual CYC 4 in the orchestrator = base(1) + early-return call(1) + cancellation gate(1) + dispatch call(1). **This satisfies CYC ≤ 8.**

Each extracted helper stays at CYC ≤ 6, well within threshold.

---

## OUT OF SCOPE

The following are explicitly **not** changed by this refactoring:

1. **Signature of `ProcessQueuedAccountOrder`**: The method remains `private void ProcessQueuedAccountOrder(QueuedAccountOrderUpdate item)`. No parameter additions, removals, or visibility changes.
2. **Observable behavior**: All execution paths produce identical side-effects. No logic re-ordering, early-return reordering, or condition semantics changes.
3. **Callees remain untouched**: `ProcessFollowerCancellationUnconditional`, `HandleMatchedFollowerOrder`, `ExecuteFollowerCascadeCleanup`, `TryFindOrderInPosition`, `RemoveGhostOrderRef`, and all `HandleMatchedFollower_*` / `ExecuteFollowerCascade_*` variants are read-only from this epic's perspective.
4. **Other methods in the file**: No other method in `V12_002.Orders.Callbacks.AccountOrders.cs` is modified.
5. **Callers**: `ProcessAccountOrderQueue` (line 182) and `ProcessAccountOrder_EnqueueTerminalUpdate` (line 154) are untouched — the public/internal contract of `ProcessQueuedAccountOrder` is unchanged.
6. **Concurrency model**: The thread-serialization comment (Build 960 audit, line 573) is preserved verbatim — no synchronization primitives added or removed.
7. **Logging / Print statements**: All `Print(...)` / `LogBuffer.Format` calls remain in-place; none are moved, combined, or elided.

---

## Extraction Plan (Ordered)

Execute extractions in the following order to keep the file in a compilable state at each step:

| Step | Extract                           | Moves Lines     | Caller CYC After Step |
|------|-----------------------------------|-----------------|-----------------------|
| 1    | `TryMatchFollowerOrderInSnapshot` | 1079–1095       | 11                    |
| 2    | `DispatchMatchedOrUnmatchedFollower` | 1097–1100    | 8                     |
| 3    | `IsQueuedOrderUpdateValid`        | 1056–1060       | 4                     |

> Step 1 first because it is the largest complexity reduction and its output (`snapshot`) is required by Step 2's new helper signature.

---

## Risk Assessment

| Risk                            | Severity | Likelihood | Mitigation                                                                                   |
|---------------------------------|----------|------------|----------------------------------------------------------------------------------------------|
| Snapshot `out` param semantics  | Medium   | Low        | Extraction 2 must pass snapshot out so Extraction 3's helper can forward it to `ExecuteFollowerCascadeCleanup`. Verify at each step. |
| `activePositions` concurrent read | Low    | Very Low   | Phase 0 confirmed all callers are on the NT strategy thread; no locking needed.              |
| Null-ref in `matchedPos` out param | Low   | Very Low   | Extraction 2 initialises both `matchedEntry = null` and `matchedPos = null` before any branch. |
| Blast radius                    | None     | None       | Phase 0 confirmed zero external importers; only 2 callers, both in same file.               |
| CYC overshoot in extracted helpers | Low   | Low        | All three helpers top out at CYC 6; each has a single responsibility with no nested dispatch. |

**Overall Risk: LOW** — zero external blast radius, isolated file, no behavior change.

---

## Success Criteria

1. `ProcessQueuedAccountOrder` CYC **≤ 8** (target: 4) as measured by the same static analysis tool used in Phase 0.
2. Three new private helper methods exist:
   - `IsQueuedOrderUpdateValid`
   - `TryMatchFollowerOrderInSnapshot`
   - `DispatchMatchedOrUnmatchedFollower`
3. Each extracted helper has CYC **≤ 8** independently.
4. Method signature `private void ProcessQueuedAccountOrder(QueuedAccountOrderUpdate item)` is **unchanged**.
5. No other method in `src/V12_002.Orders.Callbacks.AccountOrders.cs` is modified.
6. All existing callers (`ProcessAccountOrderQueue`, `ProcessAccountOrder_EnqueueTerminalUpdate`) compile and behave identically.
7. Zero new compiler warnings introduced.
