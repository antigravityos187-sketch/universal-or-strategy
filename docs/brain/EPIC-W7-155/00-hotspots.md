# EPIC-W7-155 · Phase 0 — Hotspot Analysis

> Wave 7 | Phase 0 | Agent: v12-phase0-hotspot

---

## 1. Method Identity

| Field        | Value                                                                       |
|--------------|-----------------------------------------------------------------------------|
| Method Name  | `TryHandleFleetCommand`                                                     |
| File         | `src/V12_002.UI.IPC.Commands.Fleet.cs`                                      |
| Lines        | 37 – 81 (45 lines)                                                          |
| Visibility   | `private`                                                                   |
| Return Type  | `bool`                                                                      |
| Signature    | `private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)` |

### CYC Score

| Source          | CYC |
|-----------------|-----|
| MCP tool result | **0** (method not resolved by `mcp__jcodemunch-mcp` — requires manual review) |
| Manual analysis | **20** |

> **⚠ Manual Review Required.**
> The `mcp__jcodemunch-mcp__get_symbol_complexity` tool returned CYC = 0 because the symbol
> could not be resolved from the index (partial-class file pattern `V12_002.UI.IPC.Commands.Fleet.cs`
> is not indexed by the MCP tool). Manual branch-count over the method body yields **CYC = 20**:
> 1 base + 1 ternary expression (`senderTicks > 0`) + 18 sequential `if` guard clauses, one per
> sub-handler call (lines 44–78).

---

## 2. Blast Radius

### Direct Call Sites (2 files)

| Caller Method                | File                                    | Line | Role                                      |
|------------------------------|-----------------------------------------|------|-------------------------------------------|
| `ProcessIpcCommandCore`      | `src/V12_002.UI.IPC.cs`                 | 466  | Main IPC message pump (network path)      |
| `<panel dispatch lambda>`    | `src/V12_002.UI.Panel.Handlers.cs`      | 952  | Panel button / context-menu dispatch path |

### Sub-handlers Owned (18 methods, same file)

`TryHandleFleetCommand` is the top-level router for the entire fleet command surface. Each
`if`-branch delegates to a `TryHandleFleet_*` sub-handler in the same file:

| Sub-handler                          | Commands Handled                                |
|--------------------------------------|-------------------------------------------------|
| `TryHandleFleet_Trim`                | `TRIM_25`, `TRIM_50`                            |
| `TryHandleFleet_Lock50`              | `LOCK_50`                                       |
| `TryHandleFleet_FlattenOnly`         | `FLATTEN_ONLY`                                  |
| `TryHandleFleet_Flatten`             | `FLATTEN`                                       |
| `TryHandleFleet_CancelAll`           | `CANCEL_ALL`                                    |
| `TryHandleFleet_ResetMemory`         | `RESET_MEMORY`                                  |
| `TryHandleFleet_LongShort`           | `LONG`, `SHORT`                                 |
| `TryHandleFleet_OrLong`              | `OR_LONG`                                       |
| `TryHandleFleet_OrShort`             | `OR_SHORT`                                      |
| `TryHandleFleet_TrendManualLimit`    | `TREND_MANUAL_LIMIT`                            |
| `TryHandleFleet_RetestManualLimit`   | `RETEST_MANUAL_LIMIT`                           |
| `TryHandleFleet_FfmaManualLimit`     | `FFMA_MANUAL_LIMIT`                             |
| `TryHandleFleet_FfmaManualMarket`    | `FFMA_MANUAL_MARKET`                            |
| `TryHandleFleet_CloseTarget`         | `CLOSE_T*`                                      |
| `TryHandleFleet_MoveTarget`          | `MOVE_TARGET*`, `SET_TARGET_PRICE`              |
| `TryHandleFleet_FleetState`          | `GET_FLEET*`, `SET_SIMA`, `SET_LEADER_ACCOUNT`, `REQUEST_FLEET_STATE` |
| `TryHandleFleet_ToggleAccount`       | `TOGGLE_ACCOUNT*`                               |
| `TryHandleFleet_SetShadow`           | `SET_SHADOW`                                    |

### Indirect Blast Radius

```
TryHandleFleetCommand
 ├── called from ProcessIpcCommandCore    (V12_002.UI.IPC.cs:466)
 │    └── IPC socket receive loop — network-originated commands
 └── called from panel dispatch lambda   (V12_002.UI.Panel.Handlers.cs:952)
      └── WPF UI button/context-menu actions
```

**Scope summary:** `TryHandleFleetCommand` is the single routing gateway for all 18+ fleet
action commands. Both the live IPC network path and the WPF panel path converge here. Any
structural change to the routing logic (order of sub-handler calls, `cmdId` generation) affects
every fleet command. The sub-handlers themselves carry the real business logic and are already
isolated; the router is thin.

**Risk level: MEDIUM** — the method body is structurally simple (linear dispatch chain, no
nesting), but its blast radius is broad: any regression here silently drops all fleet commands.
The `cmdId` ternary at the top affects deduplication for every command that uses
`MetadataGuardDuplicate`.

---

## 3. Top 3 Complexity Drivers

### Driver 1 — 18-branch linear if-chain (lines 44–80)

```csharp
if (TryHandleFleet_Trim(action, parts))            return true;
if (TryHandleFleet_Lock50(action))                 return true;
if (TryHandleFleet_FlattenOnly(action))            return true;
// ... 15 more ...
if (TryHandleFleet_SetShadow(action, parts))       return true;
return false;
```

Every new fleet command requires appending another `if` here. With 18 branches the chain
already constitutes the main CYC source (18 decision points). The evaluation is sequential
O(n) — the first matching sub-handler short-circuits, but worst-case all 18 are evaluated for
any unrecognized action. A dictionary or switch-expression dispatch would reduce CYC to 1 and
make the command registry explicit.

### Driver 2 — Inline `cmdId` construction via ternary (lines 39–43)

```csharp
string cmdId =
    senderTicks > 0
        ? action + "|" + senderTicks.ToString()
        : action + "|" + (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute).ToString();
```

The `cmdId` is forwarded to every sub-handler that calls `MetadataGuardDuplicate`. The
minute-granularity fallback (`/ TimeSpan.TicksPerMinute`) means commands without a
`senderTicks` value are deduplicated only once per minute — an implicit time-window assumption
embedded in the router rather than in the guard itself. This is a latent correctness coupling.

### Driver 3 — Scalability pressure from per-command sub-handler proliferation

Every fleet command is a distinct `TryHandleFleet_*` method (18 total), each beginning with an
`action != "..."` guard that mirrors the `if` in the router. The pattern is sound for isolation
but creates a **dual-maintenance surface**: the router `if`-chain and the sub-handler guard must
stay in sync. A new command added to a sub-handler but not wired in the router would be silently
unreachable, and vice versa. No test currently enforces this invariant.

---

## 4. Recommended Extraction Count

| Extraction | Description                                                                                             | Priority |
|------------|---------------------------------------------------------------------------------------------------------|----------|
| **1**      | Replace the 18-branch `if`-chain with a `Dictionary<string, Func<string[], string, bool>>` command registry, or a C# 8+ `switch` expression. Reduces method CYC from 20 to ~2 and makes the command registry a single visible list. | High     |
| **2**      | Extract `cmdId` construction into a private helper `BuildCmdId(string action, long senderTicks)`. Decouples the deduplication time-window policy from the router and makes the minute-granularity fallback testable in isolation. | Medium   |
| **3**      | Add a compile-time or unit-test assertion that every `TryHandleFleet_*` method is reachable from `TryHandleFleetCommand`. This eliminates the silent orphaning risk without changing runtime behavior. | Low      |

**Total recommended extractions: 3** *(structural — the sub-handlers themselves are already well-extracted)*

---

## 5. Agent Tracking

| Field            | Value                                                                                           |
|------------------|-------------------------------------------------------------------------------------------------|
| Agent Name       | v12-phase0-hotspot                                                                              |
| Epic             | EPIC-W7-155                                                                                     |
| Wave             | 7                                                                                               |
| Phase            | 0 — Hotspot Analysis                                                                            |
| Bobcoins Used    | 6                                                                                               |
| Execution Time   | ~75 seconds                                                                                     |
| MCP Tools Called | `search_symbols` (not resolved — partial-class file), `get_symbol_complexity` (CYC=0, fallback to manual), `get_blast_radius`, `get_hotspots`, `sequentialthinking` |
| CYC Override     | MCP=0 → Manual=20 (requires manual review flag set)                                            |
| Status           | ✅ Completed with manual-review annotation                                                      |
