# EPIC-W7-155 — Phase 4: Ticket Definitions

**Agent Name:** v12-phase4-tickets
**Generated:** 2026-06-29
**Epic:** EPIC-W7-155
**Wave:** 7
**Method:** `TryHandleFleetCommand`
**Source:** [`src/V12_002.UI.IPC.Commands.Fleet.cs`](../../../src/V12_002.UI.IPC.Commands.Fleet.cs)
**ticket_count:** 5

---

## Summary

| Field | Value |
|---|---|
| Target Method | `TryHandleFleetCommand` |
| Original CYC | 20 |
| Extraction Strategy | Group-Cohort Dispatcher |
| Ticket Count | **5** |
| projected_parent_cyc_after_all | **7** ✅ |
| Max Helper CYC | **6** ✅ |
| All Methods CYC <= 8 | **YES** ✅ |

---

## Ticket Definitions

---

### Ticket 1 — Extract `TryHandleFleetCommand_CoreOps`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-155-T1 |
| **helper_name** | `TryHandleFleetCommand_CoreOps` |
| **concern** | Core fleet operations: Trim, Lock50, FlattenOnly, Flatten, CancelAll, ResetMemory |
| **lines_to_move** | 12 (6 if-blocks, 2 lines each: Trim, Lock50, FlattenOnly, Flatten, CancelAll, ResetMemory) |
| **cyc_reduction** | -6 (removes 6 branch points from parent) |
| **projected_helper_cyc** | **6** ✅ |
| **parent_cyc_after_this_ticket** | Part of atomic refactor; parent finalized after all 5 tickets |

**Extracted Method:**
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

**Handles:** `TryHandleFleet_Trim`, `TryHandleFleet_Lock50`, `TryHandleFleet_FlattenOnly`, `TryHandleFleet_Flatten`, `TryHandleFleet_CancelAll`, `TryHandleFleet_ResetMemory`

---

### Ticket 2 — Extract `TryHandleFleetCommand_DirectionalTrades`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-155-T2 |
| **helper_name** | `TryHandleFleetCommand_DirectionalTrades` |
| **concern** | Directional trade commands: LongShort, OrLong, OrShort |
| **lines_to_move** | 6 (3 if-blocks, 2 lines each: LongShort, OrLong, OrShort) |
| **cyc_reduction** | -3 (removes 3 branch points from parent) |
| **projected_helper_cyc** | **3** ✅ |
| **parent_cyc_after_this_ticket** | Part of atomic refactor; parent finalized after all 5 tickets |

**Extracted Method:**
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

**Handles:** `TryHandleFleet_LongShort`, `TryHandleFleet_OrLong`, `TryHandleFleet_OrShort`

---

### Ticket 3 — Extract `TryHandleFleetCommand_ManualLimits`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-155-T3 |
| **helper_name** | `TryHandleFleetCommand_ManualLimits` |
| **concern** | Manual limit order commands: TrendManualLimit, RetestManualLimit, FfmaManualLimit, FfmaManualMarket |
| **lines_to_move** | 8 (4 if-blocks, 2 lines each) |
| **cyc_reduction** | -4 (removes 4 branch points from parent) |
| **projected_helper_cyc** | **4** ✅ |
| **parent_cyc_after_this_ticket** | Part of atomic refactor; parent finalized after all 5 tickets |

**Extracted Method:**
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

**Handles:** `TryHandleFleet_TrendManualLimit`, `TryHandleFleet_RetestManualLimit`, `TryHandleFleet_FfmaManualLimit`, `TryHandleFleet_FfmaManualMarket`

---

### Ticket 4 — Extract `TryHandleFleetCommand_PositionManagement`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-155-T4 |
| **helper_name** | `TryHandleFleetCommand_PositionManagement` |
| **concern** | Position management commands: CloseTarget, MoveTarget |
| **lines_to_move** | 4 (2 if-blocks, 2 lines each: CloseTarget, MoveTarget) |
| **cyc_reduction** | -2 (removes 2 branch points from parent) |
| **projected_helper_cyc** | **2** ✅ |
| **parent_cyc_after_this_ticket** | Part of atomic refactor; parent finalized after all 5 tickets |

**Extracted Method:**
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

**Handles:** `TryHandleFleet_CloseTarget`, `TryHandleFleet_MoveTarget`

---

### Ticket 5 — Extract `TryHandleFleetCommand_StateManagement`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-155-T5 |
| **helper_name** | `TryHandleFleetCommand_StateManagement` |
| **concern** | Fleet state management commands: FleetState, ToggleAccount, SetShadow |
| **lines_to_move** | 6 (3 if-blocks, 2 lines each: FleetState, ToggleAccount, SetShadow) |
| **cyc_reduction** | -3 (removes 3 branch points from parent) |
| **projected_helper_cyc** | **3** ✅ |
| **parent_cyc_after_this_ticket** | Part of atomic refactor; parent finalized after all 5 tickets |

**Extracted Method:**
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

**Handles:** `TryHandleFleet_FleetState`, `TryHandleFleet_ToggleAccount`, `TryHandleFleet_SetShadow`

---

## Refactored Parent (Post All 5 Tickets)

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

**projected_parent_cyc_after_all: 7** ✅ (1 ternary + 5 dispatcher calls + 1 base path)

---

## CYC Compliance Table

| Method | CYC Before | CYC After | <= 8? |
|---|---|---|---|
| `TryHandleFleetCommand` | 20 | **7** | ✅ YES |
| `TryHandleFleetCommand_CoreOps` | N/A (new) | **6** | ✅ YES |
| `TryHandleFleetCommand_DirectionalTrades` | N/A (new) | **3** | ✅ YES |
| `TryHandleFleetCommand_ManualLimits` | N/A (new) | **4** | ✅ YES |
| `TryHandleFleetCommand_PositionManagement` | N/A (new) | **2** | ✅ YES |
| `TryHandleFleetCommand_StateManagement` | N/A (new) | **3** | ✅ YES |
| **Max projected CYC** | — | **7** | ✅ |

---

## MCP Evidence

### resolve_repo
```json
{
  "found": true,
  "indexed": true,
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "symbol_count": 5147,
  "file_count": 2000,
  "indexed_at": "2026-06-29T01:05:21.006184"
}
```

### get_symbol_complexity (TryHandleFleetCommand)
```json
{
  "error": "Symbol 'TryHandleFleetCommand' not found in index."
}
```
Note: Index gap confirmed (consistent with Phase 2 finding). CYC=20 established and verified in Phase 2 via direct symbol source analysis. Phase 3 audit confirmed this value. Architecture plan uses CYC=20 as authoritative baseline.

### get_extraction_candidates (src/V12_002.UI.IPC.Commands.Fleet.cs)
```json
{
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "file": "src/V12_002.UI.IPC.Commands.Fleet.cs",
  "candidates": [],
  "min_complexity": 5,
  "min_callers": 2
}
```
Note: Empty result expected — `TryHandleFleet_*` sub-dispatchers are called exclusively from within the same file, not from multiple distinct caller files. Extraction design is driven by the complexity reduction mandate, not by multi-file coupling.

---

## Sequential Thinking Evidence

### Thought 1 — Ticket Count Determination
One ticket = one extracted helper = one concern (V12 mandate). The architecture plan defines 5 distinct group-cohort helpers (CoreOps, DirectionalTrades, ManualLimits, PositionManagement, StateManagement). Each helper encapsulates one logical category of fleet commands. All 5 tickets form an atomic refactoring set — the parent's CYC only reaches 7 after all 5 extractions are applied. Result: **5 tickets**.

### Thought 2 — Per-Ticket Detail
T1 CoreOps: moves 6 if-blocks (12 lines), CYC reduction=-6, helper CYC=6.
T2 DirectionalTrades: moves 3 if-blocks (6 lines), CYC reduction=-3, helper CYC=3.
T3 ManualLimits: moves 4 if-blocks (8 lines), CYC reduction=-4, helper CYC=4.
T4 PositionManagement: moves 2 if-blocks (4 lines), CYC reduction=-2, helper CYC=2.
T5 StateManagement: moves 3 if-blocks (6 lines), CYC reduction=-3, helper CYC=3.
Total CYC removed from parent: 18 branch calls. Added: 5 dispatcher calls. Net parent CYC: 20 - 18 + 5 = 7.

### Thought 3 — CYC Compliance Verification
All helpers verified <= 8: CoreOps=6 ✅, DirectionalTrades=3 ✅, ManualLimits=4 ✅, PositionManagement=2 ✅, StateManagement=3 ✅. Parent after all tickets=7 ✅. Maximum projected CYC across all methods = 7. Jane Street threshold of 8 satisfied. All 5 tickets and parent comply. Ticket model is valid.

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase4-tickets |
| Wave | 7 |
| Epic | EPIC-W7-155 |
| Phase | 4 (Ticket Generation) |
| Lane | P4-L10 |
| Input | `docs/brain/EPIC-W7-155/02-architecture-plan.md` + `docs/brain/EPIC-W7-155/03-audit-report.md` |
| Output | `docs/brain/EPIC-W7-155/04-tickets.md` |
| MCP Tools Used | resolve_repo, sequentialthinking (probe+3 thoughts), get_symbol_complexity, get_extraction_candidates |
| Sequential Thinking | 4 thoughts (1 probe + 3 analysis) |
| Bobcoins Used | ~8 |
| Execution Time | 2026-06-29T00:00:00Z |
| ticket_count | 5 |
| projected_parent_cyc_after_all | 7 |
| Max Helper CYC | 6 |
