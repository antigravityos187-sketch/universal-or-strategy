# Phase 1: Scope Definition - EPIC-W7-139

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.0
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T01:37:01Z

---

## 1. Method Under Refactoring

| Attribute         | Value                                                    |
|-------------------|----------------------------------------------------------|
| **Method**        | `UpdateStopOrder`                                        |
| **File**          | `src/V12_002.Trailing.StopUpdate.cs`                     |
| **Line**          | 84                                                       |
| **Signature**     | `private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)` |
| **CYC (current)** | 13                                                       |
| **CYC (target)**  | ≤ 8                                                      |
| **LOC**           | 56 (lines 84–139)                                        |
| **Nesting depth** | 4                                                        |

### Verbatim method body (lines 84–139)

```csharp
private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)
{
    // V8.30: Thread-safe check using TryGetValue
    if (!stopOrders.TryGetValue(entryName, out var currentStop))
        return;

    try
    {
        double validatedStopPrice = ValidateStopPrice(
            pos.Direction, newStopPrice, newTrailLevel, pos.EntryPrice
        );

        // Check for stale pending replacement
        if (pendingStopReplacements.TryGetValue(entryName, out var existingPending))
        {
            double pendingAgeSeconds = (DateTime.Now - existingPending.CreatedTime).TotalSeconds;
            if (pendingAgeSeconds > STALE_PENDING_FAST_PATH_SEC)
            {
                HandleStalePendingReplacement(entryName, pos, validatedStopPrice, newTrailLevel);
                return;
            }
        }

        // Route to appropriate handler based on order state
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

        // No existing stop or not in a cancellable state - create directly
        CreateDirectStopOrder(entryName, pos, validatedStopPrice, newTrailLevel);
    }
    catch (Exception ex)
    {
        HandleUpdateException(entryName, pos, ex);
    }
}
```

---

## 2. CYC Accounting

The 13 decision points in `UpdateStopOrder` break down as follows:

| # | Decision point (line)                                          | CYC contribution |
|---|----------------------------------------------------------------|-----------------|
| 1 | Base path (method entry)                                       | +1              |
| 2 | `!stopOrders.TryGetValue(…)` early return (line 87)           | +1              |
| 3 | `pendingStopReplacements.TryGetValue(…)` (line 100)           | +1              |
| 4 | `pendingAgeSeconds > STALE_PENDING_FAST_PATH_SEC` (line 103)  | +1              |
| 5 | `currentStop != null` (first routing guard, line 112)         | +1              |
| 6 | `OrderState == CancelPending` (line 114)                      | +1              |
| 7 | `OrderState == Submitted` (line 115)                          | +1              |
| 8 | `currentStop != null` (second routing guard, line 123)        | +1              |
| 9 | `OrderState == Working` (line 125)                            | +1              |
|10 | `OrderState == Accepted` (line 125)                           | +1              |
|11 | fall-through to `CreateDirectStopOrder` (line 133)            | +1              |
|12 | `catch (Exception ex)` branch (line 135)                      | +1              |
|13 | implicit success path (no exception)                          | +1              |

**Target reduction**: extract the stale-pending guard (decisions 3–4) and the order-state routing block (decisions 5–10) into two private helpers, reducing the coordinator method to CYC ≤ 8.

---

## 3. IN SCOPE — Extractions

### 3.1  `CheckAndHandleStalePending` (new helper)

**Purpose**: Encapsulate the "is there a still-live pending replacement that is stale?" probe-and-dispatch.

**Logic absorbed**:
```
if (pendingStopReplacements.TryGetValue(entryName, out var existingPending))
{
    double pendingAgeSeconds = (DateTime.Now - existingPending.CreatedTime).TotalSeconds;
    if (pendingAgeSeconds > STALE_PENDING_FAST_PATH_SEC)
    {
        HandleStalePendingReplacement(entryName, pos, validatedStopPrice, newTrailLevel);
        return true;   // caller should also return
    }
}
return false;
```

**Proposed signature**:
```csharp
private bool CheckAndHandleStalePending(
    string entryName, PositionInfo pos,
    double validatedStopPrice, int newTrailLevel)
```

**CYC removed from coordinator**: 2 (decisions 3 and 4 above)

---

### 3.2  `RouteStopOrderByState` (new helper)

**Purpose**: Encapsulate the three-way dispatch on `currentStop.OrderState` (CancelPending/Submitted → update existing pending; Working/Accepted → initiate replacement; otherwise → create direct).

**Logic absorbed**:
```
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
CreateDirectStopOrder(entryName, pos, validatedStopPrice, newTrailLevel);
```

**Proposed signature**:
```csharp
private void RouteStopOrderByState(
    string entryName, PositionInfo pos,
    Order currentStop,
    double validatedStopPrice, int newTrailLevel)
```

**CYC removed from coordinator**: 5 (decisions 5–10 above, noting that `null` guards fuse with state checks)

---

### 3.3  Resulting coordinator after extraction (CYC = 5)

```csharp
private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)
{
    if (!stopOrders.TryGetValue(entryName, out var currentStop))   // +1
        return;

    try
    {
        double validatedStopPrice = ValidateStopPrice(
            pos.Direction, newStopPrice, newTrailLevel, pos.EntryPrice);

        if (CheckAndHandleStalePending(entryName, pos, validatedStopPrice, newTrailLevel))  // +1
            return;

        RouteStopOrderByState(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
    }
    catch (Exception ex)   // +1
    {
        HandleUpdateException(entryName, pos, ex);
    }
}
// CYC = 1 (base) + 1 (TryGetValue guard) + 1 (stale-pending bool) + 1 (catch) = 4
// RouteStopOrderByState CYC ≤ 8 (5 decisions internally)
// CheckAndHandleStalePending CYC = 3 (base + TryGetValue + age guard)
```

> All three resulting methods satisfy CYC ≤ 8.

---

## 4. OUT OF SCOPE

| Item                                                                         | Reason                                                                         |
|------------------------------------------------------------------------------|--------------------------------------------------------------------------------|
| **Signature of `UpdateStopOrder`**                                           | Must remain `private void UpdateStopOrder(string, PositionInfo, double, int)` — no callers to update, but polymorphism/reflection cannot be ruled out |
| **Observable behavior change**                                               | Zero — extraction is pure structural; all execution paths, side-effects, and call sequences remain identical |
| **`HandleStalePendingReplacement`** (line 141)                               | Already a separate private method; not touched                                 |
| **`UpdateExistingPendingReplacement`** (line 167)                            | Already a separate private method; not touched                                 |
| **`InitiateStopReplacement`** (line 307)                                     | Already a separate private method; not touched                                 |
| **`CreateDirectStopOrder`** (line 371)                                       | Already a separate private method; not touched                                 |
| **`HandleUpdateException`** (line 496)                                       | Already a separate private method; not touched                                 |
| **`CleanupStalePendingReplacements`** (line 37)                              | Sibling method; unrelated, not touched                                         |
| **All other methods in `V12_002.Trailing.StopUpdate.cs`**                    | Untouched; this epic targets only `UpdateStopOrder`                            |
| **`ValidateStopPrice`** (in `V12_002.Orders.Management.StopSync.cs:1200`)    | Cross-file callee; behavior unchanged, not touched                             |
| **Shared state fields** (`stopOrders`, `pendingStopReplacements`)            | Access patterns unchanged; no locking changes                                  |
| **Any build, test, or CI pipeline files**                                    | Not modified in any phase                                                      |

---

## 5. Extraction Plan

```
Phase 2 (Read)  ─► Confirm exact line ranges for the two extraction blocks
Phase 3 (Tests) ─► Characterisation tests covering all 4 routing paths + stale-pending branch
Phase 4 (Impl)  ─► Extract CheckAndHandleStalePending; extract RouteStopOrderByState; rewrite coordinator
Phase 5 (Verify)─► Re-measure CYC; confirm all tests green; confirm no signature change
```

### Step-by-step extraction sequence (Phase 4)

1. **Create `CheckAndHandleStalePending`** immediately after `UpdateStopOrder` (insert before line 141).
   - Move lines 100–108 (stale-pending guard) verbatim into body.
   - Change the inner `return` to `return true`; add `return false` at end.
   - Replace the original lines 100–108 in `UpdateStopOrder` with the `if (Check…) return;` call.

2. **Create `RouteStopOrderByState`** after `CheckAndHandleStalePending`.
   - Move lines 111–133 (full routing block) verbatim into body.
   - Replace the original lines 111–133 in `UpdateStopOrder` with a single `RouteStopOrderByState(…)` call.

3. **No other lines change.** The `try/catch` frame, `ValidateStopPrice` call, and `TryGetValue` guard all remain in `UpdateStopOrder`.

---

## 6. Risk Assessment

| Risk                                     | Likelihood | Severity | Mitigation                                                         |
|------------------------------------------|------------|----------|--------------------------------------------------------------------|
| Hidden callers via reflection/events     | Medium     | High     | Signature is unchanged; no runtime impact even if callers exist    |
| Race condition on `pendingStopReplacements` | Low     | High     | No access-pattern change; `TryGetValue` call moves as a unit       |
| Incorrect bool return from helper        | Low        | Medium   | The `return true` path maps 1-to-1 with the original `return`      |
| `currentStop` null reference in router   | Low        | Medium   | Both `null` guards are moved intact into `RouteStopOrderByState`   |
| CYC target not met after extraction      | Very Low   | Low      | Counted above: coordinator=4, helper1=3, helper2=5 — all ≤ 8      |

**Overall refactoring risk: LOW** (zero callers detected; well-contained in single file; pure structural extraction).

---

## 7. Success Criteria

| Criterion                                                                 | Measurement                                                  |
|---------------------------------------------------------------------------|--------------------------------------------------------------|
| `UpdateStopOrder` CYC ≤ 8                                                 | Static analysis reports CYC ≤ 8 on coordinator              |
| All newly extracted helpers individually CYC ≤ 8                         | Static analysis reports CYC ≤ 8 for each new method         |
| `UpdateStopOrder` signature unchanged                                     | Diff shows no change to method declaration line              |
| No observable behavior change                                             | All existing + new characterisation tests pass               |
| No other methods in file modified                                         | `git diff` shows only `UpdateStopOrder` + 2 new methods      |
| No `src/` files touched in Phase 1                                       | This phase writes documentation only                         |

---

## Metadata

- **Epic ID**: EPIC-W7-139
- **Phase**: 1 (Scope Definition)
- **Status**: Completed
- **Timestamp**: 2026-06-24T01:37:01Z
- **Agent**: v12-phase1-scope
- **Cost**: 0.0 bobcoins
- **Input**: `00-hotspots.md`, `src/V12_002.Trailing.StopUpdate.cs` (lines 1–140 read)
