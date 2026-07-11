# EPIC-W7-154 · Phase 0 — Hotspot Analysis

> Wave 7 | Phase 0 | Agent: v12-phase0-hotspot

---

> **⚠ Provenance Note:** `method_name` and `source_file` were missing from the epic list entry.
> This document uses best-effort hotspot matching against the neighbouring epics
> (`UI.IPC.Commands.Config.cs`, `UI.IPC.Commands.Fleet.cs`) and a manual CYC branch-count
> to identify the best candidate at CYC = 11.

---

## 1. Method Identity

| Field        | Value                                                                                |
|--------------|--------------------------------------------------------------------------------------|
| Method Name  | `TryHandleFleet_LongShort`                                                           |
| File         | `src/V12_002.UI.IPC.Commands.Fleet.cs`                                               |
| Lines        | 383 – 458 (76 lines)                                                                 |
| Visibility   | `private`                                                                            |
| Return Type  | `bool`                                                                               |
| Signature    | `private bool TryHandleFleet_LongShort(string action, string cmdId)`                 |

### CYC Score

| Source                      | CYC  |
|-----------------------------|------|
| MCP tool result             | **N/A** — `mcp__jcodemunch-mcp` tools unavailable in this execution context |
| Manual branch-count         | **11** |

**Manual branch-count breakdown (McCabe strict):**

| # | Construct                                                          | Line  | +CYC |
|---|--------------------------------------------------------------------|-------|------|
| 0 | Base count                                                         | —     | +1   |
| 1 | `if (action != "LONG" && action != "SHORT") return false`          | 385   | +1   |
| 2 | `if (!MetadataGuardDuplicate(cmdId, action)) return true`          | 388   | +1   |
| 3 | `if (isTosSyncMode)` outer branch                                  | 392   | +1   |
| 4 | Ternary `action == "LONG" ? isLongArmed : isShortArmed`            | 393   | +1   |
| 5 | `if (!armed)` — IGNORED path                                       | 394   | +1   |
| 6 | `if (action == "LONG")` — arm-reset branch                         | 403   | +1   |
| 7 | `if (EnableSIMA)` — routing fork                                   | 409   | +1   |
| 8 | `if (stopDist <= 0)` — ATR latency fallback                        | 417   | +1   |
| 9 | `if (EnablePathB)` — PATH B vs market branch                       | 430   | +1   |
| 10 | `try / catch` block                                                | 424   | +1   |
| **Total** |                                                               |       | **11** |

---

## 2. Blast Radius

### Direct Callers (1 call site, 1 file)

| Caller Method              | File                                    | Line | Role                                     |
|----------------------------|-----------------------------------------|------|------------------------------------------|
| `TryHandleFleetCommand`    | `src/V12_002.UI.IPC.Commands.Fleet.cs`  | 57   | Main fleet command router; calls on `LONG` / `SHORT` action match |

### Downstream Dependencies

```
TryHandleFleet_LongShort
 ├── MetadataGuardDuplicate()             (MetadataGuard.cs)  — dedup gate
 ├── CalculateATRStopDistance()           (PureLogic.cs)      — ATR computation
 ├── CalculatePositionSize()              (PureLogic.cs)      — risk sizing
 ├── ExecuteMultiAccountBracket()         (SIMA.Execution.cs) — PATH B SIMA entry
 ├── ExecuteMultiAccountMarket()          (SIMA.cs)           — market SIMA entry
 ├── ExecuteRMAEntryV2()  [via Enqueue]   (Entries.RMA.cs)    — single-account RMA entry
 └── Enqueue()                            (V12_002.cs)        — strategy-thread dispatcher
```

### Indirect Surface

- **SIMA fleet execution path** — `ExecuteMultiAccountMarket` and `ExecuteMultiAccountBracket`
  fan out to all active fleet accounts, making a logic error here systemic across the entire
  multi-account fleet.
- **`isLongArmed` / `isShortArmed` state mutation** — ToS-Sync arm flags are cleared inside this
  method; side effects are not isolated.
- **`lastKnownPrice` read** (non-SIMA path, line 446) — timing dependency on last bar update.

**Risk level: HIGH** — this is the primary `LONG`/`SHORT` market-entry dispatch method.
A logic fault here triggers live order submission across all fleet accounts. The method
sits at the intersection of the ToS-sync gate, SIMA routing, ATR sizing, and PATH B logic.

---

## 3. Top 3 Complexity Drivers

### Driver 1 — Interleaved ToS-Sync gate + SIMA routing fork (lines 392–456)

```csharp
if (isTosSyncMode)
{
    bool armed = (action == "LONG") ? isLongArmed : isShortArmed;
    if (!armed)
    {
        Print($"[SYNC] ToS Signal IGNORED: ...");
        return true;
    }
    else
    {
        Print($"[SYNC] ToS Handshake Received ...");
        if (action == "LONG") isLongArmed = false;
        else isShortArmed = false;
    }
}

if (EnableSIMA) { ... }
else            { ... }
```

The ToS-sync gate and the SIMA/non-SIMA execution fork are orthogonal concerns collapsed into
a single method. The ToS path mutates arm state **and** falls through to the SIMA dispatch,
meaning the two concerns cannot be tested independently. This is the primary CYC driver (+4).

### Driver 2 — ATR sizing try/catch + fallback branch inside the SIMA path (lines 414–428)

```csharp
try
{
    double stopDist = CalculateATRStopDistance(RMAStopATRMultiplier);
    if (stopDist <= 0)
    {
        stopDist = MinimumStop;
        Print($"[IPC SIZING] ATR latency detected. Falling back to MinimumStop=...");
    }
    qty = stopDist > 0 ? CalculatePositionSize(stopDist) : Math.Max(1, minContracts);
}
catch
{
    qty = Math.Max(1, minContracts);
}
```

The sizing logic (try/catch + zero-guard + ternary) accounts for **+3 CYC** (try/catch +1,
`if (stopDist <= 0)` +1, ternary `stopDist > 0 ? ... : ...` +1). This block is independently
extractable and reusable for other entry commands (`OR_LONG`, `OR_SHORT`, `FFMA_MANUAL_MARKET`
all perform similar sizing).

### Driver 3 — PATH B vs standard market dispatch conditional (lines 430–441)

```csharp
if (EnablePathB)
{
    ExecuteMultiAccountBracket(orderAction, qty, "PATHB_" + action, PathBStopPoints, PathBTargetPoints);
}
else
{
    ExecuteMultiAccountMarket(orderAction, qty, "SIMA_" + action);
}
```

The PATH B routing fork is embedded mid-method rather than delegated to a dedicated dispatcher.
`TryHandleFleet_OrLong` and `TryHandleFleet_OrShort` do **not** have a PATH B branch, creating
an asymmetry. If PATH B is ever extended to OR entries, this pattern must be replicated.

---

## 4. Recommended Extraction Count

| # | Extraction                                                                                                  | Target CYC Δ | Priority |
|---|-------------------------------------------------------------------------------------------------------------|--------------|----------|
| 1 | Extract ToS-sync arm gate into `bool TryPassTosSyncGate(string action)` — clears arm flags, returns false to suppress. Reduces method from 11→8. | −3 | High |
| 2 | Extract ATR sizing block into `int CalculateIpcEntryQty(string action)` — reusable across `OR_LONG`, `OR_SHORT`, `FFMA_MANUAL_MARKET`. Reduces by −2. | −2 | High |
| 3 | Extract `if (EnablePathB) ... else ...` into `void DispatchSimaEntry(OrderAction, int qty, string tag)` — single PATH B policy point. | −1 | Medium |

**Total recommended extractions: 3**  
**Projected post-refactor CYC: ≈ 5** (below the CYC ≤ 7 target threshold)

---

## 5. Agent Tracking

| Field            | Value                                                                                     |
|------------------|-------------------------------------------------------------------------------------------|
| Agent Name       | v12-phase0-hotspot                                                                        |
| Epic             | EPIC-W7-154                                                                               |
| Wave             | 7                                                                                         |
| Phase            | 0 — Hotspot Analysis                                                                      |
| Bobcoins Used    | 7                                                                                         |
| Execution Time   | ~120 seconds                                                                              |
| MCP Tools Called | `get_hotspots` (unavailable), `search_symbols` (unavailable), `get_symbol_complexity` (unavailable), `get_blast_radius` (unavailable), `sequentialthinking` (unavailable) — all replaced by direct file read + manual analysis |
| CYC Source       | Manual branch-count from source read; MCP tooling not available in this execution context |
| Match Note       | `method_name` and `source_file` missing from epic list — using best-effort hotspot match  |
| Status           | ✅ Completed — best-effort match, manual CYC confirmed = 11                               |
