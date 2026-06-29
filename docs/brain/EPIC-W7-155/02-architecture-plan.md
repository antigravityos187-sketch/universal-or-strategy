# EPIC-W7-155 — Phase 2: Architecture Plan

**Agent Name:** v12-phase2-architecture
**Generated:** 2026-06-29
**Status:** EXTRACTION REQUIRED (CYC=20 > threshold 8)

---

## 1. Target Method Table

| Field | Value |
|---|---|
| Method | `TryHandleFleetCommand` |
| File | [`src/V12_002.UI.IPC.Commands.Fleet.cs`](../../../src/V12_002.UI.IPC.Commands.Fleet.cs) |
| Line | 37 |
| End Line | 81 |
| Lines | 45 |
| Actual CYC (jcodemunch) | **20** |
| Precomputed CYC | 0 (index gap — actual verified via get_symbol_complexity) |
| Target CYC | <= 8 |
| Status | **EXTRACTION NEEDED** |

---

## 2. Method Source (as retrieved)

```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
{
    string cmdId =
        senderTicks > 0
            ? action + "|" + senderTicks.ToString()
            : action + "|" + (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute).ToString();

    if (TryHandleFleet_Trim(action, parts))
        return true;
    if (TryHandleFleet_Lock50(action))
        return true;
    if (TryHandleFleet_FlattenOnly(action))
        return true;
    if (TryHandleFleet_Flatten(action, cmdId))
        return true;
    if (TryHandleFleet_CancelAll(action, cmdId))
        return true;
    if (TryHandleFleet_ResetMemory(action))
        return true;
    if (TryHandleFleet_LongShort(action, cmdId))
        return true;
    if (TryHandleFleet_OrLong(action, cmdId))
        return true;
    if (TryHandleFleet_OrShort(action, cmdId))
        return true;
    if (TryHandleFleet_TrendManualLimit(action, parts, cmdId))
        return true;
    if (TryHandleFleet_RetestManualLimit(action, parts, cmdId))
        return true;
    if (TryHandleFleet_FfmaManualLimit(action, parts, cmdId))
        return true;
    if (TryHandleFleet_FfmaManualMarket(action, cmdId))
        return true;
    if (TryHandleFleet_CloseTarget(action))
        return true;
    if (TryHandleFleet_MoveTarget(action, parts))
        return true;
    if (TryHandleFleet_FleetState(action, parts))
        return true;
    if (TryHandleFleet_ToggleAccount(action, parts))
        return true;
    if (TryHandleFleet_SetShadow(action, parts))
        return true;
    return false;
}
```

---

## 3. Complexity Analysis

The method is a **chain dispatcher**: it builds `cmdId` (1 ternary branch) then calls 19 `TryHandleFleet_*` helpers in sequence via `if (...) return true` (19 branch points).

| Branch Source | Count |
|---|---|
| cmdId ternary | 1 |
| `if (TryHandleFleet_X) return true` calls | 19 |
| **Total CYC** | **20** |

---

## 4. Extraction Plan

### Strategy: Group-Cohort Dispatcher Extraction

Extract 5 private group-helper methods, each responsible for one logical cohort of fleet commands. The parent `TryHandleFleetCommand` delegates to these 5 groups.

### 4.1 Refactored Parent — `TryHandleFleetCommand` → CYC = 7

```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
{
    string cmdId =
        senderTicks > 0
            ? action + "|" + senderTicks.ToString()
            : action + "|" + (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute).ToString();

    if (TryHandleFleetCommand_CoreOps(action, parts, cmdId))
        return true;
    if (TryHandleFleetCommand_DirectionalTrades(action, cmdId))
        return true;
    if (TryHandleFleetCommand_ManualLimits(action, parts, cmdId))
        return true;
    if (TryHandleFleetCommand_PositionManagement(action, parts))
        return true;
    if (TryHandleFleetCommand_StateManagement(action, parts, cmdId))
        return true;
    return false;
}
```

**CYC:** 1 (ternary) + 5 (if chains) + 1 (base) = **7** ✅

### 4.2 Extracted Helpers

#### `TryHandleFleetCommand_CoreOps` — CYC = 6

```csharp
private bool TryHandleFleetCommand_CoreOps(string action, string[] parts, string cmdId)
{
    if (TryHandleFleet_Trim(action, parts))
        return true;
    if (TryHandleFleet_Lock50(action))
        return true;
    if (TryHandleFleet_FlattenOnly(action))
        return true;
    if (TryHandleFleet_Flatten(action, cmdId))
        return true;
    if (TryHandleFleet_CancelAll(action, cmdId))
        return true;
    if (TryHandleFleet_ResetMemory(action))
        return true;
    return false;
}
```

**Handles:** Trim, Lock50, FlattenOnly, Flatten, CancelAll, ResetMemory
**CYC:** 6 ✅

#### `TryHandleFleetCommand_DirectionalTrades` — CYC = 3

```csharp
private bool TryHandleFleetCommand_DirectionalTrades(string action, string cmdId)
{
    if (TryHandleFleet_LongShort(action, cmdId))
        return true;
    if (TryHandleFleet_OrLong(action, cmdId))
        return true;
    if (TryHandleFleet_OrShort(action, cmdId))
        return true;
    return false;
}
```

**Handles:** LongShort, OrLong, OrShort
**CYC:** 3 ✅

#### `TryHandleFleetCommand_ManualLimits` — CYC = 4

```csharp
private bool TryHandleFleetCommand_ManualLimits(string action, string[] parts, string cmdId)
{
    if (TryHandleFleet_TrendManualLimit(action, parts, cmdId))
        return true;
    if (TryHandleFleet_RetestManualLimit(action, parts, cmdId))
        return true;
    if (TryHandleFleet_FfmaManualLimit(action, parts, cmdId))
        return true;
    if (TryHandleFleet_FfmaManualMarket(action, cmdId))
        return true;
    return false;
}
```

**Handles:** TrendManualLimit, RetestManualLimit, FfmaManualLimit, FfmaManualMarket
**CYC:** 4 ✅

#### `TryHandleFleetCommand_PositionManagement` — CYC = 2

```csharp
private bool TryHandleFleetCommand_PositionManagement(string action, string[] parts)
{
    if (TryHandleFleet_CloseTarget(action))
        return true;
    if (TryHandleFleet_MoveTarget(action, parts))
        return true;
    return false;
}
```

**Handles:** CloseTarget, MoveTarget
**CYC:** 2 ✅

#### `TryHandleFleetCommand_StateManagement` — CYC = 3

```csharp
private bool TryHandleFleetCommand_StateManagement(string action, string[] parts, string cmdId)
{
    if (TryHandleFleet_FleetState(action, parts))
        return true;
    if (TryHandleFleet_ToggleAccount(action, parts))
        return true;
    if (TryHandleFleet_SetShadow(action, parts))
        return true;
    return false;
}
```

**Handles:** FleetState, ToggleAccount, SetShadow
**CYC:** 3 ✅

---

## 5. CYC Summary Table

| Method | CYC Before | CYC After | Compliant |
|---|---|---|---|
| `TryHandleFleetCommand` | 20 | 7 | YES ✅ |
| `TryHandleFleetCommand_CoreOps` | N/A (new) | 6 | YES ✅ |
| `TryHandleFleetCommand_DirectionalTrades` | N/A (new) | 3 | YES ✅ |
| `TryHandleFleetCommand_ManualLimits` | N/A (new) | 4 | YES ✅ |
| `TryHandleFleetCommand_PositionManagement` | N/A (new) | 2 | YES ✅ |
| `TryHandleFleetCommand_StateManagement` | N/A (new) | 3 | YES ✅ |
| **Max projected CYC** | — | **7** | ✅ |

---

## 6. Jane Street KB Compliance

| Rule | Source | Status |
|---|---|---|
| Zero-alloc hot path; extract cold logging `[NoInlining]` | carl_cook | No logging in this method — not applicable. Pure dispatch, no allocations beyond string concat in cmdId (existing, unchanged). ✅ |
| No new `lock()` blocks | gjengset | No lock blocks added or present. ✅ |
| Single responsibility per helper; each helper CYC <= 8 | trading_billions | Each extracted helper handles exactly one command group. All CYC <= 6. ✅ |

---

## 7. MCP Evidence

### resolve_repo
```json
{
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "indexed": true,
  "symbol_count": 5147,
  "file_count": 2000,
  "indexed_at": "2026-06-29T01:05:21.006184"
}
```

### search_symbols
- Symbol found: `src/V12_002.UI.IPC.Commands.Fleet.cs::V12_002.TryHandleFleetCommand#method`
- Line 37, signature: `private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)`

### get_symbol_source
- Lines 37–81 (45 lines)
- 19 `if (TryHandleFleet_X) return true` dispatch calls
- 1 ternary for cmdId construction

### get_symbol_complexity
```json
{
  "cyclomatic": 20,
  "max_nesting": 2,
  "param_count": 3,
  "lines": 45,
  "assessment": "high"
}
```

---

## 8. Sequential Thinking Evidence

**Thought 1:** Identified precomputed CYC=0 as index gap. Retrieved actual CYC=20 via jcodemunch. Extraction required since 20 > 8.

**Thought 2:** Designed group-cohort extraction into 5 helpers (CoreOps, DirectionalTrades, ManualLimits, PositionManagement, StateManagement). Parent reduced to CYC=7. All helpers CYC<=6. Confirmed Jane Street KB alignment: single responsibility per helper, no locks added.

---

## 9. Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase2-architecture |
| Wave | 7 |
| Epic | EPIC-W7-155 |
| Phase | 2 (Architecture Planning) |
| Input | `docs/brain/EPIC-W7-155/01-scope-boundary.md` |
| Output | `docs/brain/EPIC-W7-155/02-architecture-plan.md` |
| MCP Tools Used | resolve_repo, search_symbols, get_symbol_source, get_symbol_complexity |
| Sequential Thinking | 2 thoughts |
| Max Projected CYC | 7 |
