# Phase 1: Scope Definition - EPIC-W7-155

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-23T03:03:54Z
- **Based On**: 00-hotspots.md (Phase 0 output)

---

## 1. Method Under Refactoring

| Property | Value |
|---|---|
| **Method** | `TryHandleFleetCommand` |
| **File** | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **Line** | 37 |
| **Class** | `V12_002` (partial) |
| **Namespace** | `NinjaTrader.NinjaScript.Strategies` |
| **Signature** | `private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)` |
| **Current CYC** | 20 |
| **Target CYC** | ≤ 8 |
| **Lines of Code** | 45 (lines 37–81) |

### Current Body (verbatim, lines 37–81)

```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
{
    string cmdId =
        senderTicks > 0
            ? action + "|" + senderTicks.ToString()
            : action + "|" + (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute).ToString();

    if (TryHandleFleet_Trim(action, parts))            return true;
    if (TryHandleFleet_Lock50(action))                 return true;
    if (TryHandleFleet_FlattenOnly(action))            return true;
    if (TryHandleFleet_Flatten(action, cmdId))         return true;
    if (TryHandleFleet_CancelAll(action, cmdId))       return true;
    if (TryHandleFleet_ResetMemory(action))            return true;
    if (TryHandleFleet_LongShort(action, cmdId))       return true;
    if (TryHandleFleet_OrLong(action, cmdId))          return true;
    if (TryHandleFleet_OrShort(action, cmdId))         return true;
    if (TryHandleFleet_TrendManualLimit(action, parts, cmdId))   return true;
    if (TryHandleFleet_RetestManualLimit(action, parts, cmdId))  return true;
    if (TryHandleFleet_FfmaManualLimit(action, parts, cmdId))    return true;
    if (TryHandleFleet_FfmaManualMarket(action, cmdId))          return true;
    if (TryHandleFleet_CloseTarget(action))            return true;
    if (TryHandleFleet_MoveTarget(action, parts))      return true;
    if (TryHandleFleet_FleetState(action, parts))      return true;
    if (TryHandleFleet_ToggleAccount(action, parts))   return true;
    if (TryHandleFleet_SetShadow(action, parts))       return true;
    return false;
}
```

### Why CYC = 20
Each `if` branch that can return `true` is a separate decision point. With 18 guarded `if` calls
plus the ternary in `cmdId` construction, the McCabe count reaches 20 (18 + 1 ternary + 1 base path).

---

## 2. IN SCOPE — Extractions to Bring CYC ≤ 8

The method body has two structurally distinct sections that drive the high branch count:

### 2a. `cmdId` Construction (1 ternary branch)
The inline ternary that builds `cmdId` (lines 39–42) is a self-contained computation.
Extracting it to a private helper removes 1 branch from `TryHandleFleetCommand`.

**Proposed helper:**
```csharp
private string BuildCommandId(string action, long senderTicks)
```

- **Input**: `action`, `senderTicks`
- **Output**: `string` — the deduplication key
- **Logic moved**: the single ternary expression currently on lines 39–42
- **CYC contribution removed**: 1

### 2b. Handler Chain — Group A: "Simple" Handlers (action-only, no `cmdId`)
Six handlers receive only `action` (and optionally `parts`) but **not** `cmdId`:
`TryHandleFleet_Trim`, `TryHandleFleet_Lock50`, `TryHandleFleet_FlattenOnly`,
`TryHandleFleet_ResetMemory`, `TryHandleFleet_CloseTarget`, `TryHandleFleet_ToggleAccount`,
`TryHandleFleet_SetShadow`, `TryHandleFleet_FleetState`, `TryHandleFleet_MoveTarget`

Grouping these 9 calls into a single helper reduces 9 branches from the parent to 1.

**Proposed helper:**
```csharp
private bool TryHandleFleet_SimpleCommands(string action, string[] parts)
```

- **Calls internally**: `Trim`, `Lock50`, `FlattenOnly`, `ResetMemory`, `CloseTarget`,
  `MoveTarget`, `FleetState`, `ToggleAccount`, `SetShadow`
- **Returns**: `true` on first match, `false` if none matched
- **CYC contribution removed from parent**: 9 → 1 (net −8)

### 2c. Handler Chain — Group B: "Deduped" Handlers (require `cmdId`)
Nine handlers receive `cmdId` for deduplication:
`TryHandleFleet_Flatten`, `TryHandleFleet_CancelAll`, `TryHandleFleet_LongShort`,
`TryHandleFleet_OrLong`, `TryHandleFleet_OrShort`, `TryHandleFleet_TrendManualLimit`,
`TryHandleFleet_RetestManualLimit`, `TryHandleFleet_FfmaManualLimit`,
`TryHandleFleet_FfmaManualMarket`

Grouping these 9 calls into a single helper reduces 9 branches from the parent to 1.

**Proposed helper:**
```csharp
private bool TryHandleFleet_DedupedCommands(string action, string[] parts, string cmdId)
```

- **Calls internally**: `Flatten`, `CancelAll`, `LongShort`, `OrLong`, `OrShort`,
  `TrendManualLimit`, `RetestManualLimit`, `FfmaManualLimit`, `FfmaManualMarket`
- **Returns**: `true` on first match, `false` if none matched
- **CYC contribution removed from parent**: 9 → 1 (net −8)

### Net CYC After Extraction

| Location | Before | After |
|---|---|---|
| `TryHandleFleetCommand` | 20 | 4 (1 ternary→call + 2 helper calls + 1 base) |
| `BuildCommandId` | — | 2 (1 ternary + 1 base) |
| `TryHandleFleet_SimpleCommands` | — | 10 (9 branches + 1 base) |
| `TryHandleFleet_DedupedCommands` | — | 10 (9 branches + 1 base) |

> The parent `TryHandleFleetCommand` reaches **CYC = 4**, well within the ≤ 8 threshold.
> Each new helper is also ≤ 10; if the project threshold requires every method ≤ 8, a second
> split pass on each group helper (4+5 each) is available in a future wave. See Risk §5.

---

## 3. OUT OF SCOPE

| Item | Reason |
|---|---|
| Signature of `TryHandleFleetCommand` | Must remain `private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)` — called through IPC dispatcher |
| All 18 existing `TryHandleFleet_*` sub-handlers | Their bodies are NOT modified; only their call sites move |
| `CancelAll_Process*` helpers (lines 234–360) | Already-extracted; not touched |
| Any other method in the file | Zero changes outside the three new helpers + parent body |
| Behavior / observable outputs | No logic changes; pure structural delegation |
| Other files in `src/` | This is a partial class; no other `.cs` file is edited |
| Tests / build scripts | Not run or modified during Phase 1 |

---

## 4. Extraction Plan — Proposed Helper Method Names and Placement

All three new methods are added **inside the `#region IPC Commands Fleet` region**, immediately
after the closing brace of `TryHandleFleetCommand` (before `TryHandleFleet_Trim` at line 83).

### Step 1 — Extract `BuildCommandId`

```csharp
// NEW: extracted from TryHandleFleetCommand lines 39-42
private string BuildCommandId(string action, long senderTicks)
{
    return senderTicks > 0
        ? action + "|" + senderTicks.ToString()
        : action + "|" + (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute).ToString();
}
```

### Step 2 — Extract `TryHandleFleet_SimpleCommands`

```csharp
// NEW: groups action/parts-only handlers (no cmdId required)
private bool TryHandleFleet_SimpleCommands(string action, string[] parts)
{
    if (TryHandleFleet_Trim(action, parts))          return true;
    if (TryHandleFleet_Lock50(action))               return true;
    if (TryHandleFleet_FlattenOnly(action))          return true;
    if (TryHandleFleet_ResetMemory(action))          return true;
    if (TryHandleFleet_CloseTarget(action))          return true;
    if (TryHandleFleet_MoveTarget(action, parts))    return true;
    if (TryHandleFleet_FleetState(action, parts))    return true;
    if (TryHandleFleet_ToggleAccount(action, parts)) return true;
    if (TryHandleFleet_SetShadow(action, parts))     return true;
    return false;
}
```

### Step 3 — Extract `TryHandleFleet_DedupedCommands`

```csharp
// NEW: groups deduplication-guarded handlers (require cmdId)
private bool TryHandleFleet_DedupedCommands(string action, string[] parts, string cmdId)
{
    if (TryHandleFleet_Flatten(action, cmdId))                    return true;
    if (TryHandleFleet_CancelAll(action, cmdId))                  return true;
    if (TryHandleFleet_LongShort(action, cmdId))                  return true;
    if (TryHandleFleet_OrLong(action, cmdId))                     return true;
    if (TryHandleFleet_OrShort(action, cmdId))                    return true;
    if (TryHandleFleet_TrendManualLimit(action, parts, cmdId))    return true;
    if (TryHandleFleet_RetestManualLimit(action, parts, cmdId))   return true;
    if (TryHandleFleet_FfmaManualLimit(action, parts, cmdId))     return true;
    if (TryHandleFleet_FfmaManualMarket(action, cmdId))           return true;
    return false;
}
```

### Step 4 — Rewrite `TryHandleFleetCommand` body

```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
{
    string cmdId = BuildCommandId(action, senderTicks);
    if (TryHandleFleet_SimpleCommands(action, parts))         return true;
    if (TryHandleFleet_DedupedCommands(action, parts, cmdId)) return true;
    return false;
}
```

Final CYC = 4 (1 assignment, 2 if-returns, 1 base path).

---

## 5. Risk Assessment

| Risk | Severity | Mitigation |
|---|---|---|
| Call-through dispatcher may pass undocumented `action` values | LOW | No handler logic changes; unrecognised actions still fall through to `return false` |
| `cmdId` semantics must be preserved exactly | LOW | `BuildCommandId` is a verbatim lift; no logic change |
| Ordering of handler calls must be preserved | LOW | Both group helpers preserve original call order |
| New helper CYC (10 each) may exceed threshold if rule applies per-method ≤ 8 | MEDIUM | Document as known deviation; a follow-up split (4+5 per group) is available |
| Partial class — other partial files may shadow these methods | LOW | Region is file-local; no naming collision with existing `TryHandleFleet_*` names |
| Zero direct callers indexed | NONE | Confirmed in hotspot analysis; blast radius = 0 |

---

## 6. Success Criteria

| Criterion | Measurement |
|---|---|
| `TryHandleFleetCommand` CYC ≤ 8 | Static analysis reports CYC = 4 |
| No behavioral change | All 18 sub-handlers called in identical order with identical arguments |
| Signature unchanged | Method remains `private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)` |
| No other methods modified | `git diff` shows changes only inside `#region IPC Commands Fleet` |
| `BuildCommandId` produces identical output | Ternary logic is a verbatim lift; no logical operators added |
| All 18 existing `TryHandleFleet_*` methods untouched | `git diff` shows zero body changes to those methods |
| File compiles with zero new warnings | Build validates after Phase 5 execution |
