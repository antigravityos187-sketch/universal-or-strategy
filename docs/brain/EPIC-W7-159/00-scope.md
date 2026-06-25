# Phase 1: Scope Definition - EPIC-W7-159

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-23T03:34:11Z
- **Input**: docs/brain/EPIC-W7-159/00-hotspots.md
- **Input**: src/V12_002.UI.IPC.Commands.Fleet.cs (lines 383–458)

---

## Method Under Refactoring

| Field | Value |
|---|---|
| **Method** | `TryHandleFleet_LongShort` |
| **File** | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **Line** | 383 |
| **Signature** | `private bool TryHandleFleet_LongShort(string action, string cmdId)` |
| **Current CYC** | 21 |
| **Target CYC** | ≤ 8 |
| **LOC** | 76 |

The method handles two fleet entry directions (`LONG` / `SHORT`) with three distinct execution paths:
1. **Guard clause** — early-exit if action is unrecognised or a duplicate command.
2. **ToS-Sync branch** — when `isTosSyncMode` is set, validates armed state before proceeding.
3. **SIMA branch** — when `EnableSIMA` is set, calculates ATR-based position size and dispatches either a bracket order (Path B) or a market order.
4. **RMA branch** — when `EnableSIMA` is not set, calculates ATR stop distance and enqueues an `ExecuteRMAEntryV2` actor call.

---

## IN SCOPE

### Extractions Required to Reach CYC ≤ 8

#### 1. `HandleTosSyncGate(string action) → bool`
- **Source lines**: 391–407 (`if (isTosSyncMode) { … }`)
- **Responsibility**: Checks whether the armed-state flag for the given direction is set. If not armed, prints a SYNC-ignored message and returns `false`. If armed, prints a handshake message, clears the arm flag, and returns `true`.
- **Decision nodes extracted**: 3 (isTosSyncMode check, armed check, action == "LONG" branch)
- **Returns**: `true` if execution should continue, `false` to abort with `return true` at call site.

#### 2. `CalculateSIMAQty(string action) → int`
- **Source lines**: 410–428 (try/catch block inside `if (EnableSIMA)`)
- **Responsibility**: Derives `OrderAction`, calls `CalculateATRStopDistance`, applies the `MinimumStop` fallback when ATR is zero, calls `CalculatePositionSize`, catches any exception by falling back to `minContracts`, and returns the final clamped `qty ≥ 1`.
- **Decision nodes extracted**: 4 (stopDist ≤ 0, stopDist > 0 ternary, catch, Math.Max clamp)
- **Returns**: `int qty` (already clamped to ≥ 1).

#### 3. `DispatchSIMAOrder(string action, int qty) → void`
- **Source lines**: 430–441 (`if (EnablePathB) … else …`)
- **Responsibility**: Selects between Path-B bracket execution and standard SIMA market execution based on `EnablePathB`, printing the appropriate log line and calling either `ExecuteMultiAccountBracket` or `ExecuteMultiAccountMarket`.
- **Decision nodes extracted**: 2 (EnablePathB check, the two Print/Execute pairs)
- **Returns**: void.

#### 4. `DispatchRMAOrder(string action) → bool`
- **Source lines**: 443–455 (`else` branch of `if (EnableSIMA)`)
- **Responsibility**: Determines `MarketPosition` direction, reads `lastKnownPrice` with fallback to `Close[0]`, guards against `currentPrice ≤ 0`, calculates ATR stop distance and contracts, then enqueues the `ExecuteRMAEntryV2` actor call.
- **Decision nodes extracted**: 3 (direction ternary, price fallback ternary, currentPrice ≤ 0 guard)
- **Returns**: `bool` — `false` signals normal completion, `true` signals an early abort (currentPrice == 0 path).

---

## OUT OF SCOPE

| Item | Reason |
|---|---|
| Method signature of `TryHandleFleet_LongShort` | Must remain `private bool TryHandleFleet_LongShort(string action, string cmdId)` — single caller `TryHandleFleetCommand` must not be touched. |
| Observable behaviour / return values | No logic change. All branches produce identical outcomes post-extraction. |
| `TryHandleFleetCommand` (line 37) | Sole caller — untouched. |
| Any other method in the file | `TryHandleFleet_OrLong` (line 460) and all other siblings — untouched. |
| `MetadataGuardDuplicate` guard (lines 385–389) | Already a single-line call; CYC contribution is 2 (one per `if`). No extraction needed. |
| Unit tests / test projects | No test changes in Phase 1; testing is a Phase 3 concern. |
| Build system / project files | No `.csproj` or solution changes. |
| Logging format strings | Exact string content of all `Print(…)` calls preserved verbatim. |

---

## Extraction Plan

### Proposed Helper Method Names and Signatures

```csharp
// 1 — ToS-Sync armed-state gate
//     Returns false  → caller should `return true` (handled, no-op)
//     Returns true   → caller should continue
private bool HandleTosSyncGate(string action)

// 2 — SIMA ATR-based quantity calculation (includes try/catch fallback)
//     Returns clamped qty ≥ 1
private int CalculateSIMAQty(string action)

// 3 — SIMA order dispatch (Path B vs market)
private void DispatchSIMAOrder(string action, int qty)

// 4 — RMA actor-enqueue dispatch
//     Returns true  → caller should `return true` (currentPrice=0 abort)
//     Returns false → caller should fall through to `return true` at end
private bool DispatchRMAOrder(string action)
```

### Reconstructed `TryHandleFleet_LongShort` Skeleton After Extraction

```csharp
private bool TryHandleFleet_LongShort(string action, string cmdId)
{
    if (action != "LONG" && action != "SHORT")            // CYC +1
        return false;

    if (!MetadataGuardDuplicate(cmdId, action))           // CYC +1
        return true;

    if (isTosSyncMode && !HandleTosSyncGate(action))      // CYC +1
        return true;

    if (EnableSIMA)                                       // CYC +1
    {
        int qty = CalculateSIMAQty(action);
        DispatchSIMAOrder(action, qty);
    }
    else
    {
        if (DispatchRMAOrder(action))                     // CYC +1
            return true;
    }

    return true;
}
// Residual CYC in parent: 5  (well within ≤8 target)
```

### CYC Budget After Extraction

| Method | Estimated CYC |
|---|---|
| `TryHandleFleet_LongShort` (residual) | 5 |
| `HandleTosSyncGate` | 4 |
| `CalculateSIMAQty` | 5 |
| `DispatchSIMAOrder` | 2 |
| `DispatchRMAOrder` | 4 |
| **Max across any single method** | **5** ✅ |

All five methods stay at ≤ 8. Combined CYC is 20 (one shared decision reduced from the try/catch flow due to flattening).

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|---|---|---|
| Accidental logic inversion on `HandleTosSyncGate` return | Low | Explicit `bool` return with documented polarity in XML-doc; Phase 2 review checklist. |
| `CalculateSIMAQty` captures wrong field (e.g. `minContracts`) via closure | Low | Helper is an instance method on the same class — field access identical to inline version. |
| `DispatchRMAOrder` bool return polarity misread at call site | Low | Return documented as "true = abort"; name `Dispatch…` makes void seem natural; review guards against this. |
| Actor/Enqueue thread contract broken by extraction | None | `Enqueue` call moved verbatim inside `DispatchRMAOrder`; no lambda capture changes. |
| Blast radius | None | Phase 0 confirmed zero direct dependents. |

**Overall Phase 2 Risk**: LOW.

---

## Success Criteria

1. `TryHandleFleet_LongShort` CYC ≤ 8 (target: 5).
2. All four extracted helpers each have CYC ≤ 8.
3. Method signature of `TryHandleFleet_LongShort` is byte-for-byte unchanged.
4. No other method in `src/V12_002.UI.IPC.Commands.Fleet.cs` is modified.
5. All `Print(…)` log strings are preserved verbatim.
6. Build compiles with zero new errors or warnings.
7. Single caller `TryHandleFleetCommand` is untouched.
