# 02-Architecture Plan — EPIC-W7-014

## Epic Metadata

| Field | Value |
|-------|-------|
| Epic ID | EPIC-W7-014 |
| Wave | 7 |
| Phase | 2 — Architecture Planning |
| Agent | v12-phase2-architecture |

---

## Original Method Profile (MCP-Confirmed)

| Field | Value |
|-------|-------|
| Method Name | `TryHandleFleetCommand` |
| File | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| Line Range | 37–81 |
| Lines | 45 |
| CYC (MCP-confirmed) | **20** |
| Max Nesting | 2 |
| Param Count | 3 (`string action`, `string[] parts`, `long senderTicks`) |
| Assessment | high (CYC > 8 — extraction required) |
| Signature | `private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)` |

---

## Complexity Driver Analysis

The method body is a linear chain-of-responsibility dispatcher. Every `if (TryHandleFleet_X(...)) return true;` statement contributes +1 CYC. Full breakdown:

| # | Branch Point | CYC Contribution |
|---|-------------|-----------------|
| — | Base | +1 |
| 1 | `senderTicks > 0` ternary in cmdId | +1 |
| 2 | `if (TryHandleFleet_Trim(...))` | +1 |
| 3 | `if (TryHandleFleet_Lock50(...))` | +1 |
| 4 | `if (TryHandleFleet_FlattenOnly(...))` | +1 |
| 5 | `if (TryHandleFleet_Flatten(...))` | +1 |
| 6 | `if (TryHandleFleet_CancelAll(...))` | +1 |
| 7 | `if (TryHandleFleet_ResetMemory(...))` | +1 |
| 8 | `if (TryHandleFleet_LongShort(...))` | +1 |
| 9 | `if (TryHandleFleet_OrLong(...))` | +1 |
| 10 | `if (TryHandleFleet_OrShort(...))` | +1 |
| 11 | `if (TryHandleFleet_TrendManualLimit(...))` | +1 |
| 12 | `if (TryHandleFleet_RetestManualLimit(...))` | +1 |
| 13 | `if (TryHandleFleet_FfmaManualLimit(...))` | +1 |
| 14 | `if (TryHandleFleet_FfmaManualMarket(...))` | +1 |
| 15 | `if (TryHandleFleet_CloseTarget(...))` | +1 |
| 16 | `if (TryHandleFleet_MoveTarget(...))` | +1 |
| 17 | `if (TryHandleFleet_FleetState(...))` | +1 |
| 18 | `if (TryHandleFleet_ToggleAccount(...))` | +1 |
| 19 | `if (TryHandleFleet_SetShadow(...))` | +1 |
| **Total** | | **20** |

Note: The 19 leaf helpers (`TryHandleFleet_*`) are already extracted. The complexity is solely from the length of the if-chain in the parent dispatcher.

---

## Extraction Plan

Three sub-dispatcher helpers group semantically related fleet operations:

| Helper Name | Responsibility | Lines Moved (approx) | Projected CYC |
|-------------|---------------|---------------------|---------------|
| `TryHandleFleet_BasicOps` | Routes basic single-action flat/reset commands | ~12 lines (6 if-checks) | **7** |
| `TryHandleFleet_DirectionalOps` | Routes directional/entry order commands | ~14 lines (7 if-checks) | **8** |
| `TryHandleFleet_StateOps` | Routes state/target manipulation commands | ~10 lines (5 if-checks) | **6** |

### Helper Method Signatures

```csharp
// Group 1: Basic flat/reset ops — Trim, Lock50, FlattenOnly, Flatten, CancelAll, ResetMemory
private bool TryHandleFleet_BasicOps(string action, string[] parts, string cmdId)

// Group 2: Directional/entry ops — LongShort, OrLong, OrShort, TrendManualLimit,
//          RetestManualLimit, FfmaManualLimit, FfmaManualMarket
private bool TryHandleFleet_DirectionalOps(string action, string[] parts, string cmdId)

// Group 3: State/target ops — CloseTarget, MoveTarget, FleetState, ToggleAccount, SetShadow
private bool TryHandleFleet_StateOps(string action, string[] parts)
```

### Parent Method After Extraction

```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
{
    string cmdId =
        senderTicks > 0
            ? action + "|" + senderTicks.ToString()
            : action + "|" + (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute).ToString();

    if (TryHandleFleet_BasicOps(action, parts, cmdId))
        return true;
    if (TryHandleFleet_DirectionalOps(action, parts, cmdId))
        return true;
    if (TryHandleFleet_StateOps(action, parts))
        return true;
    return false;
}
```

---

## Projected CYC Summary

| Method | Projected CYC | Compliant (<= 8) |
|--------|--------------|-----------------|
| `TryHandleFleetCommand` (parent) | 5 | YES |
| `TryHandleFleet_BasicOps` | 7 | YES |
| `TryHandleFleet_DirectionalOps` | 8 | YES |
| `TryHandleFleet_StateOps` | 6 | YES |

**max_cyc_projected: 8**

---

## Jane Street Alignment Notes

| Principle | Application |
|-----------|-------------|
| `carl_cook` zero-alloc hot path | `cmdId` string is built once in parent and passed as `string cmdId` parameter — no re-allocation in sub-dispatchers. No LINQ usage. No heap allocation in routing logic. |
| `carl_cook` AggressiveInlining hot | Sub-dispatchers are pure routing stubs (3-4 lines each); runtime JIT will inline them naturally. Consider `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on all three new helpers. |
| `gjengset` no new lock() blocks | No locks added. The method is a pure delegation dispatcher. All state mutation remains in the existing `TryHandleFleet_*` leaf helpers. |
| `trading_billions` single responsibility | BasicOps = flat/cancel/reset commands. DirectionalOps = entry order commands. StateOps = state/target management. Each group has a clear, distinct domain. |
| `trading_billions` CYC <= 8 per helper | All projected CYC values are <= 8. DirectionalOps is exactly 8 (at limit, acceptable). |
| `trading_billions` defense in depth | Boolean short-circuit pattern preserved. The first matching sub-dispatcher returns true, preventing further evaluation — maintaining the same early-exit semantics as the original. |

---

## MCP Evidence

| Tool | Inputs | Key Result |
|------|--------|-----------|
| `resolve_repo` | path="/home/malhitticrypto/universal-or-strategy" | Repo confirmed: `antigravityos187-sketch/universal-or-strategy`, 5147 symbols indexed |
| `search_symbols` | query="TryHandleFleetCommand" | Symbol ID: `src/V12_002.UI.IPC.Commands.Fleet.cs::V12_002.TryHandleFleetCommand#method`, line 37 |
| `get_symbol_complexity` | symbol_id above | CYC=20, max_nesting=2, param_count=3, lines=45, assessment="high" |
| `get_symbol_source` | symbol_id above | Full 45-line source confirmed: 19 sequential if-return-true dispatcher, line 37-81 |
| `get_call_hierarchy` | symbol_id above, depth=2 | 0 callers; 19 leaf callee dispatches (all `TryHandleFleet_*` helpers already extracted); secondary callees via helpers confirmed |
| `get_dependency_graph` | file="src/V12_002.UI.IPC.Commands.Fleet.cs" | node_count=1, edge_count=0 (self-contained partial class) |

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Drivers
Enumerated all 19 if-branch points in the dispatcher chain. Each `if (TryHandleFleet_X(...)) return true;` contributes +1 CYC. The ternary in cmdId assignment adds +1. Total confirmed: CYC = 20. The 19 leaf helpers are already extracted — complexity is purely from the if-chain length in the parent.

### Thought 2 — Extraction Strategy
Grouped the 19 existing helper calls into 3 semantically cohesive groups:
- BasicOps (6 if-checks): flat/cancel/reset operations → projected CYC 7
- DirectionalOps (7 if-checks): entry/directional orders → projected CYC 8
- StateOps (5 if-checks): state/target management → projected CYC 6
Parent after extraction has 3 if-calls + ternary = projected CYC 5.

### Thought 3 — CYC Validation
Validated all projected CYC values:
- TryHandleFleetCommand: 5 (PASS)
- TryHandleFleet_BasicOps: 7 (PASS)
- TryHandleFleet_DirectionalOps: 8 (at limit, PASS)
- TryHandleFleet_StateOps: 6 (PASS)
max_cyc_projected = 8. All within Jane Street strict standard. Hypothesis verified.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase2-architecture |
| Bobcoins Used | 6 |
| Execution Time | ~45s |
| MCP Tools Called | resolve_repo, search_symbols, get_symbol_complexity, get_symbol_source, get_call_hierarchy, get_dependency_graph |
| Sequential Thoughts | 3 (probe + 3 architecture thoughts = 4 total) |
| Phase | 2 — Architecture Planning |
| Status | COMPLETE |
| max_cyc_projected | 8 |
