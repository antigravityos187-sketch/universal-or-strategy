# Phase 1: Scope Definition - EPIC-W7-051

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T00:00:00Z
- **Input**: 00-hotspots.md, manifest.json

---

## Method Under Refactoring

| Attribute         | Value                                           |
|-------------------|-------------------------------------------------|
| **Method**        | `UpdateStopOrder`                               |
| **File**          | `src/V12_002.Trailing.StopUpdate.cs`            |
| **Line**          | 84                                              |
| **Signature**     | `private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)` |
| **Current CYC**   | 13                                              |
| **Target CYC**    | ≤ 8                                             |
| **LOC**           | 56 (lines 84–139)                               |

### Current Body (annotated decision points)

```
D1  if (!stopOrders.TryGetValue(...))  → early return               [guard]
    try {
      ValidateStopPrice(...)
D2    if (pendingStopReplacements.TryGetValue(...))                  [pending check]
D3      if (pendingAgeSeconds > STALE_PENDING_FAST_PATH_SEC)         [stale check]
            HandleStalePendingReplacement(...)  → return
D4    if (currentStop != null                                        [cancel/submit state]
D5         && (OrderState.CancelPending || OrderState.Submitted))
            UpdateExistingPendingReplacement(...)  → return
D6    if (currentStop != null                                        [working/accepted state]
D7         && (OrderState.Working || OrderState.Accepted))
            InitiateStopReplacement(...)  → return
      CreateDirectStopOrder(...)
    }
D8  catch (Exception ex)                                            [exception handler]
        HandleUpdateException(...)
```

> **Note**: Compound boolean expressions within a single `if` (e.g. `D4/D5`, `D6/D7`) are each
> counted as separate decision points by standard McCabe analysis, which yields CYC = 13 when
> accounting for all branches inside `ValidateStopPrice` (called inline).  The visible structural
> branches in the body alone account for 8 decision paths; the remaining points arise from the
> compound `||` conditions and the `try/catch`.

---

## IN SCOPE — Extractions Required

The goal is to reduce `UpdateStopOrder`'s own CYC to ≤ 8 by extracting the compound routing
logic into two focused private helpers.

### Extraction 1 — `TryHandleStalePending`

| Item             | Detail                                                                                     |
|------------------|--------------------------------------------------------------------------------------------|
| **Proposed name**| `TryHandleStalePending`                                                                    |
| **Return type**  | `bool`                                                                                     |
| **Parameters**   | `string entryName, PositionInfo pos, double validatedStopPrice, int newTrailLevel`         |
| **Absorbs**      | Lines 100–108: `pendingStopReplacements.TryGetValue` + age guard + `HandleStalePendingReplacement` call |
| **Decision points removed from caller** | 2 (`D2`, `D3`)                                              |
| **Returns**      | `true` if the stale-pending path was taken (caller must `return`); `false` otherwise       |

**Rationale**: The pending-age check is a self-contained guard with its own state (`existingPending`,
`pendingAgeSeconds`). Extracting it removes two decision points from the orchestrator and gives the
staleness logic a named, testable home.

---

### Extraction 2 — `TryRouteByOrderState`

| Item             | Detail                                                                                     |
|------------------|--------------------------------------------------------------------------------------------|
| **Proposed name**| `TryRouteByOrderState`                                                                     |
| **Return type**  | `bool`                                                                                     |
| **Parameters**   | `string entryName, PositionInfo pos, OrderInfo currentStop, double validatedStopPrice, int newTrailLevel` |
| **Absorbs**      | Lines 111–130: both `if (currentStop != null && …)` blocks routing to `UpdateExistingPendingReplacement` and `InitiateStopReplacement` |
| **Decision points removed from caller** | 4 (`D4`, `D5`, `D6`, `D7`)                                 |
| **Returns**      | `true` if a routed handler was invoked (caller must `return`); `false` → fall through to `CreateDirectStopOrder` |

**Rationale**: The two order-state routing branches are logically a single "which handler owns this
state?" decision. Grouping them isolates the `OrderState` dispatch table, keeps `UpdateStopOrder`
unaware of individual state values, and reduces caller CYC by 4 points.

---

### Projected CYC After Extractions

| Method                        | Before | After |
|-------------------------------|--------|-------|
| `UpdateStopOrder`             | 13     | ≤ 7   |
| `TryHandleStalePending`       | —      | 3     |
| `TryRouteByOrderState`        | —      | 5     |

`UpdateStopOrder` retains: D1 (guard), `try/catch` branch (D8), and the call-site `if (TryHandleStalePending)` + `if (TryRouteByOrderState)` = 4 remaining decision points → **CYC ≈ 5**, well within the ≤ 8 mandate.

---

## OUT OF SCOPE

The following are explicitly excluded from this refactoring:

1. **Signature of `UpdateStopOrder` is unchanged.**  
   `private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)` — no parameter additions, removals, or visibility changes.

2. **No behavior change.**  
   All execution paths must produce identical side-effects, call ordering, and observable state transitions as the original. The refactoring is purely structural.

3. **Existing helper methods are untouched.**  
   `HandleStalePendingReplacement`, `UpdateExistingPendingReplacement`, `InitiateStopReplacement`, `CreateDirectStopOrder`, `HandleUpdateException`, `ValidateStopPrice`, and all other callees remain byte-for-byte identical.

4. **No changes to any other method in `V12_002.Trailing.StopUpdate.cs`.**

5. **No changes to any file outside `src/V12_002.Trailing.StopUpdate.cs`.**

6. **No new fields, properties, constants, or state.**  
   Both extracted helpers operate solely on parameters passed in; no instance-level state is introduced.

7. **No logging, metrics, or tracing additions.**

8. **No test files are authored in this phase** (test coverage is a Phase 3 concern).

---

## Extraction Plan

### Step 1 — Extract `TryHandleStalePending`

Before:
```csharp
if (pendingStopReplacements.TryGetValue(entryName, out var existingPending))
{
    double pendingAgeSeconds = (DateTime.Now - existingPending.CreatedTime).TotalSeconds;
    if (pendingAgeSeconds > STALE_PENDING_FAST_PATH_SEC)
    {
        HandleStalePendingReplacement(entryName, pos, validatedStopPrice, newTrailLevel);
        return;
    }
}
```

After (call-site in `UpdateStopOrder`):
```csharp
if (TryHandleStalePending(entryName, pos, validatedStopPrice, newTrailLevel))
    return;
```

New helper (inserted in same file, after `UpdateStopOrder`):
```csharp
private bool TryHandleStalePending(
    string entryName, PositionInfo pos, double validatedStopPrice, int newTrailLevel)
{
    if (!pendingStopReplacements.TryGetValue(entryName, out var existingPending))
        return false;
    double pendingAgeSeconds = (DateTime.Now - existingPending.CreatedTime).TotalSeconds;
    if (pendingAgeSeconds <= STALE_PENDING_FAST_PATH_SEC)
        return false;
    HandleStalePendingReplacement(entryName, pos, validatedStopPrice, newTrailLevel);
    return true;
}
```

---

### Step 2 — Extract `TryRouteByOrderState`

Before:
```csharp
if (currentStop != null
    && (currentStop.OrderState == OrderState.CancelPending
        || currentStop.OrderState == OrderState.Submitted))
{
    UpdateExistingPendingReplacement(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
    return;
}

if (currentStop != null
    && (currentStop.OrderState == OrderState.Working
        || currentStop.OrderState == OrderState.Accepted))
{
    InitiateStopReplacement(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
    return;
}
```

After (call-site in `UpdateStopOrder`):
```csharp
if (TryRouteByOrderState(entryName, pos, currentStop, validatedStopPrice, newTrailLevel))
    return;
```

New helper (inserted in same file, after `TryHandleStalePending`):
```csharp
private bool TryRouteByOrderState(
    string entryName, PositionInfo pos, OrderInfo currentStop,
    double validatedStopPrice, int newTrailLevel)
{
    if (currentStop == null)
        return false;
    if (currentStop.OrderState == OrderState.CancelPending
        || currentStop.OrderState == OrderState.Submitted)
    {
        UpdateExistingPendingReplacement(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
        return true;
    }
    if (currentStop.OrderState == OrderState.Working
        || currentStop.OrderState == OrderState.Accepted)
    {
        InitiateStopReplacement(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
        return true;
    }
    return false;
}
```

---

## Risk Assessment

| Risk                              | Severity | Mitigation                                                                 |
|-----------------------------------|----------|----------------------------------------------------------------------------|
| Inverted condition introduces new path | LOW  | Both helpers use early-return patterns; unit tests cover all 4 `OrderState` values |
| `currentStop` null-check duplicated | LOW    | Extraction consolidates the null guard into helper — reduces surface area  |
| Blast radius beyond this file     | **NONE** | Phase 0 confirmed 0 external importers and 0 callers at index depth 2      |
| Behavioral drift from `return` placement | LOW | Each `return true` maps 1-to-1 to an original `return` statement          |
| Stale variable capture (closures) | NONE     | No lambdas; all state passed as explicit parameters                        |

**Overall Risk: LOW** (downgraded from Phase 0 MEDIUM — the method body is now read and the
extraction boundaries are clean, with no shared mutable state across branches).

---

## Success Criteria

1. `UpdateStopOrder` CYC ≤ 8 as measured by the project's complexity tool.
2. All original execution paths preserved: 8 distinct input scenarios (null stop, 4 OrderState values, stale pending, fresh pending no-op, exception) produce identical outcomes before and after.
3. Both new helpers (`TryHandleStalePending`, `TryRouteByOrderState`) exist as `private` methods in `src/V12_002.Trailing.StopUpdate.cs`.
4. No other method in any file is modified.
5. `UpdateStopOrder` public (class-level) signature is byte-for-byte identical to the original.
6. No new compiler warnings introduced.
