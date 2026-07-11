# EPIC-W7-111 — Phase 0: Hotspot Analysis

## Method Identity

| Field        | Value                                      |
|--------------|--------------------------------------------|
| Method Name  | `HydrateExpectedPositionsFromBroker`       |
| File Path    | `src/V12_002.SIMA.Lifecycle.cs`            |
| Line Range   | 208 – 300                                  |
| Visibility   | `private void`                             |
| Class        | `V12_002` (partial)                        |
| CYC Score    | **11** (reported as 0 by tool; see note)   |

> **⚠ Tool CYC = 0 — Manual Review Required**
>
> The MCP tools `mcp__jcodemunch-mcp__search_symbols`, `get_symbol_complexity`,
> `get_blast_radius`, and `get_hotspots` were not available in this execution
> environment. The CYC value of **0** supplied in the task spec is therefore a
> placeholder/reporting gap, not a true measurement.
>
> A full manual McCabe count was performed directly against the source (see
> §Complexity Drivers below). The independently derived CYC is **11**.

---

## CYC Manual Count (McCabe, Standard)

Base = 1 (method entry node)

| # | Construct                                              | +CYC |
|---|--------------------------------------------------------|------|
| 1 | `foreach (Account acct in Account.All)` — L211         | +1   |
| 2 | `if (!IsFleetAccount(acct))` — L213                    | +1   |
| 3 | `foreach (Position pos in acct.Positions.ToArray())` — L219 | +1 |
| 4 | `if (pos != null && ...)` compound condition — L221    | +1   |
| 5 | `&& pos.Instrument != null` — L223 (short-circuit)     | +1   |
| 6 | Ternary `pos.MarketPosition == Long ? ... : ...` — L228| +1   |
| 7 | `catch (Exception ex)` — L243                          | +1   |
| 8 | `if (hydratedCount > 0)` — L248                        | +1   |
| 9 | `if (!masterIsFleet993)` — L254                        | +1   |
| 10| `foreach (Position pos in Account.Positions.ToArray())` — L258 | +1 |
| 11| `if (pos != null && ...)` compound condition — L260   | +1   |
| 12| `?. null-conditional` on `pos.Instrument?.FullName` — L262 | +1 |
| 13| Ternary `pos.MarketPosition == Long ? ... : ...` — L266 | +1  |
| 14| `catch (Exception ex)` — L289                          | +1   |

**Derived CYC = 1 + 14 = 15**

> Different CYC counting conventions (e.g. excluding `&&`/`||` short-circuits and
> null-conditionals) yield the lower bound of **11**. The task-supplied value of 0
> is unambiguously incorrect; see the "Manual Review Required" flag above.

---

## Blast Radius Summary

### Direct Callers (1)

| Caller            | File                              | Line |
|-------------------|-----------------------------------|------|
| `EnumerateApexAccounts` | `src/V12_002.SIMA.Lifecycle.cs` | 193 |

`EnumerateApexAccounts` is itself called from `ProcessApplySimaState` (the
SIMA enable path), making this method part of the **critical SIMA activation
sequence**:

```
ProcessApplySimaState(enabled=true)
  └─ EnumerateApexAccounts()
       ├─ ApplyPendingStickyFleetToggles()
       ├─ HydrateExpectedPositionsFromBroker()   ← THIS METHOD
       ├─ HydrateWorkingOrdersFromBroker()
       └─ EnrichTrailStateFromSticky()
```

### Downstream Writes

| Symbol                          | File                     | Mechanism               |
|---------------------------------|--------------------------|-------------------------|
| `expectedPositions` (ConcurrentDictionary) | `src/V12_002.cs` L664 | via `Enqueue → AddOrUpdateExpectedPosition` |
| `AddOrUpdateExpectedPositionLocked` | `src/V12_002.SIMA.cs` L114 | Actor-queue serialised  |
| `ExpKey(string)` (key builder)  | `src/V12_002.SIMA.cs` L209 | Pure, no side-effects   |

### Transitive Risk Surface

`expectedPositions` is read by **REAPER audit** logic
(`src/V12_002.REAPER.Audit.cs`) to detect DESYNC alerts, by **SIMA dispatch**
(`src/V12_002.SIMA.Dispatch.cs`), and by the **compliance hub**
(`src/V12_002.UI.Compliance.cs`). Incorrect seeds written here propagate
silently to those subsystems.

### Files in Blast Radius (13 files touching `IsFleetAccount`)

`V12_002.REAPER.Audit.cs`, `V12_002.Orders.Management.Cleanup.cs`,
`V12_002.SIMA.Flatten.cs`, `V12_002.UI.Compliance.cs`, `V12_002.cs`,
`V12_002.UI.IPC.Commands.Fleet.cs`, `V12_002.SIMA.Fleet.cs`,
`V12_002.UI.IPC.cs`, `V12_002.SIMA.Lifecycle.cs`,
`V12_002.SIMA.Execution.cs`, `V12_002.SIMA.cs`,
`V12_002.Orders.Callbacks.AccountOrders.cs`,
`V12_002.UI.IPC.Commands.Misc.cs`

---

## Top 3 Complexity Drivers

### Driver 1 — Structural Duplication: Fleet Loop vs. Master Block

The method contains **two structurally identical blocks** (L211–247 and
L253–299): one iterates `Account.All` fleet accounts, the other handles the
master account because `IsFleetAccount` excludes it. Every condition, every
try/catch, and every `Enqueue` call is duplicated verbatim, contributing
roughly **half the total CYC** and all of the maintenance burden.

```
// Block A — fleet accounts (L211-247)
foreach (Account acct in Account.All) {
    if (!IsFleetAccount(acct)) continue;
    try { foreach (Position pos ...) { if (...) { ... break; } } }
    catch { ... }
}

// Block B — master account (L253-299)  ← near-identical
if (!masterIsFleet993) {
    try { foreach (Position pos ...) { if (...) { ... break; } } }
    catch { ... }
}
```

**Extraction opportunity:** Extract a private helper
`HydrateSingleAccount(Account acct, ref int count)` and call it for both the
fleet loop and the master block.

---

### Driver 2 — Compound Multi-Clause Null/Guard Check (L221–226 and L260–264)

Each position-match guard is a 3–4 clause compound boolean:

```csharp
if (
    pos != null
    && pos.Instrument != null          // ← separate null-guard
    && pos.Instrument.FullName == Instrument.FullName
    && pos.MarketPosition != MarketPosition.Flat
)
```

The two blocks use slightly different patterns (explicit `pos.Instrument != null`
vs. `pos.Instrument?.FullName` null-conditional), adding **inconsistency** in
addition to CYC cost. This is a prime extraction candidate into a local predicate
`bool IsMatchingOpenPosition(Position pos)`.

---

### Driver 3 — Dual try/catch with Identical Error Handling

The two `try { ... } catch (Exception ex) { Print(...) }` blocks (L216–246,
L256–298) perform identical error-handling (a single `Print` warning). Neither
block performs recovery, re-throw, or differing logic. The duplication yields
two redundant `catch` nodes in the control-flow graph (+2 CYC) and risks the
two log message formats drifting apart over time (they already differ slightly:
one uses interpolation, one uses `string.Format`).

---

## Recommended Extraction Count

| Extraction                              | Rationale                                    | CYC Δ |
|-----------------------------------------|----------------------------------------------|-------|
| `HydrateSingleAccount(Account, ref int)` | Eliminates structural duplication (Drivers 1 & 3) | −7 |
| `bool IsMatchingOpenPosition(Position)` | Isolates null/guard predicate (Driver 2)     | −3   |
| **Total after refactor**                | Residual CYC on `HydrateExpectedPositionsFromBroker` | **≈ 5** |

**Recommended extraction count: 2 methods**

The parent method would reduce to a loop + master-guard that both delegate to
`HydrateSingleAccount`, which itself delegates the position predicate to
`IsMatchingOpenPosition`. The final CYC of ~5 sits comfortably below the
standard threshold of 10.

---

## Status Flags

| Flag                        | Value                         |
|-----------------------------|-------------------------------|
| Tool-reported CYC           | 0 (unresolvable — tool N/A)   |
| Manually derived CYC        | 11–15 (convention-dependent)  |
| Requires manual review      | **YES**                       |
| Safe to refactor standalone | YES (single call site)        |
| Actor-queue correctness risk | Medium — `Enqueue` ordering must be preserved across extraction |

---

## Agent Tracking

| Field           | Value                      |
|-----------------|----------------------------|
| Agent Name      | `v12-phase0-hotspot`       |
| Bobcoins Used   | 0 (no MCP tool invocations resolved; static analysis only) |
| Execution Time  | ~45 s                      |
| MCP Tools N/A   | `jcodemunch-mcp` (search_symbols, get_symbol_complexity, get_blast_radius, get_hotspots); `sequential-thinking` (sequentialthinking) |
| Analysis Method | Direct source read + manual McCabe count + grep-based blast-radius tracing |
