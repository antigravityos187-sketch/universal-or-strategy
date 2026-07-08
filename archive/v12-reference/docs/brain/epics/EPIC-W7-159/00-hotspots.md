# EPIC-W7-159 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field | Value |
|---|---|
| **Method** | `TryHandleFleet_LongShort` |
| **CYC Score** | 21 (blast-radius–weighted, jCodeMunch) |
| **Local CYC** | 17 (raw McCabe, method body only) |
| **File** | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **Lines** | 383–458 |
| **Class** | `V12_002` (partial, `Strategy`) |
| **Namespace** | `NinjaTrader.NinjaScript.Strategies` |

---

## Blast Radius Summary

`TryHandleFleet_LongShort` is the **primary fleet entry dispatcher** for `LONG`/`SHORT` IPC commands. It sits on the IPC command-handling chain rooted at `TryHandleFleetCommand` (line 56) and fans out into two distinct execution sub-trees — SIMA multi-account broadcast and the single-account RMA entry path — each with their own sizing logic.

```
TryHandleFleetCommand (Fleet.cs:37)
  └─ TryHandleFleet_LongShort (Fleet.cs:383)
       ├─ MetadataGuardDuplicate()              [MetadataGuard.cs — dedup gate]
       ├─ [isTosSyncMode branch]
       │     └─ reads/writes: isLongArmed, isShortArmed
       ├─ [EnableSIMA == true]
       │     ├─ CalculateATRStopDistance()      [UI.Sizing.cs → PureLogic.cs]
       │     ├─ CalculatePositionSize()         [UI.Sizing.cs → PureLogic.cs]
       │     ├─ ExecuteMultiAccountBracket()    [SIMA.Execution.cs — PathB]
       │     └─ ExecuteMultiAccountMarket()     [SIMA.Execution.cs — default]
       └─ [EnableSIMA == false]
             ├─ CalculateATRStopDistance()      [UI.Sizing.cs → PureLogic.cs]
             ├─ CalculatePositionSize()         [UI.Sizing.cs → PureLogic.cs]
             └─ ExecuteRMAEntryV2()             [SIMA.Execution.cs — enqueued]
```

**Files directly coupled to this method:**

| File | Coupling Type |
|---|---|
| `src/V12_002.UI.IPC.Commands.Fleet.cs` | Owner; called from `TryHandleFleetCommand` |
| `src/V12_002.MetadataGuard.cs` | Dedup gate — `MetadataGuardDuplicate()` |
| `src/V12_002.UI.Sizing.cs` | Position sizing — `CalculateATRStopDistance`, `CalculatePositionSize` |
| `src/V12_002.PureLogic.cs` | Pure-logic sizing backend (transitive via Sizing.cs) |
| `src/V12_002.SIMA.Execution.cs` | Execution — `ExecuteMultiAccountBracket`, `ExecuteMultiAccountMarket`, `ExecuteRMAEntryV2` |
| `src/V12_002.Properties.cs` | Config flags read: `isTosSyncMode`, `isLongArmed`, `isShortArmed`, `EnableSIMA`, `EnablePathB`, `RMAStopATRMultiplier`, `MinimumStop`, `MaxRiskAmount`, `PathBStopPoints`, `PathBTargetPoints`, `minContracts`, `lastKnownPrice` |
| `src/V12_002.UI.Callbacks.cs` | Shares the same `CalculateATRStopDistance → CalculatePositionSize → ExecuteRMAEntryV2` pattern (parallel call-site coupling) |

**Blast radius level: MEDIUM-HIGH** — the method is on the IPC hot path and directly triggers live order placement on multiple accounts. Any extraction must preserve:
1. The exact arm-flag mutation order (arm read before arm write within the `isTosSyncMode` block).
2. The `try/catch` fallback on sizing (protects against ATR-not-ready at strategy startup).
3. The `qty = Math.Max(1, qty)` floor that must apply regardless of which sizing path ran.

---

## Top 3 Complexity Drivers

### Driver 1 — ToS Sync Mode guard with nested ternary arm-selection and bidirectional arm-clear (lines 391–407, +5 CYC)

```
if (isTosSyncMode)                                         // +1
{
    bool armed = (action == "LONG") ? isLongArmed          // +1 (ternary)
                                    : isShortArmed;
    if (!armed)                                            // +1
        return true;
    else
    {
        if (action == "LONG")                              // +1
            isLongArmed = false;
        else
            isShortArmed = false;
    }
}
```

These four decision points implement a single logical concept — **"check if the requested direction is armed, then clear the arm flag"** — but are inlined as four separate branches. The ternary arm-selection and the `if (action == "LONG")` arm-clear are performing symmetric operations that could be collapsed into a `HandleTosSyncArm(action)` helper returning a `bool` (armed/not-armed) and clearing the flag internally.

**Extraction target:** `private bool TryConsumeTosSyncArm(string action)` — returns `false` if not armed (caller returns `true` early), clears the appropriate flag and returns `true` if armed.

---

### Driver 2 — SIMA sizing try/catch with multi-guard ternary chain (lines 413–428, +5 CYC)

```
try
{
    double stopDist = CalculateATRStopDistance(...);
    if (stopDist <= 0)                                     // +1
    {
        stopDist = MinimumStop;
        ...
    }
    qty = stopDist > 0                                     // +1 (ternary)
              ? CalculatePositionSize(stopDist)
              : Math.Max(1, minContracts);
    ...
}
catch                                                      // +1
{
    qty = Math.Max(1, minContracts);
}
qty = Math.Max(1, qty);
```

The `stopDist <= 0` guard and the redundant `stopDist > 0` ternary that follows it are **logically inconsistent**: after the first `if` block, `stopDist` is guaranteed `> 0` (it was set to `MinimumStop`), making the ternary's false-branch dead code. This hidden invariant adds a phantom +1 CYC and obscures the actual fallback contract. The entire block encodes one responsibility: **"calculate a safe, positive qty for SIMA"**.

**Extraction target:** `private int CalculateSIMAEntryQty()` — encapsulates the `try/catch`, the `stopDist` guard, and the `Math.Max(1,...)` floor. Returns a validated `int qty ≥ 1`.

---

### Driver 3 — Dual-branch execution fork (EnableSIMA × EnablePathB) with symmetric ternary direction mapping (lines 409–455, +6 CYC)

```
if (EnableSIMA)                                            // +1
{
    OrderAction orderAction = action == "LONG" ? ...       // +1 (ternary)
    // [sizing block — Driver 2]
    if (EnablePathB)                                       // +1
        ExecuteMultiAccountBracket(...)
    else
        ExecuteMultiAccountMarket(...)
}
else
{
    MarketPosition direction = action == "LONG" ? ...      // +1 (ternary)
    double currentPrice = lastKnownPrice > 0 ? ...        // +1 (ternary)
    if (currentPrice <= 0)                                 // +1
        return true;
    ...
}
```

The outer `if (EnableSIMA)` creates two independent execution paths, each with its own direction-mapping ternary, its own sizing call, and its own dispatch method. These paths share no data flow after the action-to-direction mapping and could each become a dedicated method:
- `ExecuteSIMAEntry(string action)` — covers the `EnableSIMA == true` branch (including Driver 2 sizing).
- `ExecuteRMAEntry(string action)` — covers the `EnableSIMA == false` branch.

After extraction the parent method becomes a 5-line coordinator.

---

## Recommended Extraction Count

**3 extractions** are recommended:

| # | Extraction | Signature | Complexity Reduced |
|---|---|---|---|
| 1 | ToS sync arm gate | `private bool TryConsumeTosSyncArm(string action)` | Eliminates Driver 1 (+5 CYC from parent) |
| 2 | SIMA qty calculator | `private int CalculateSIMAEntryQty()` | Eliminates Driver 2 (+5 CYC from parent) |
| 3 | SIMA execution branch | `private void ExecuteSIMAEntry(string action, int qty)` | Collapses Driver 3's SIMA fork (+3 CYC from parent) |

After extraction, `TryHandleFleet_LongShort` reduces to a ≤7-line coordinator:

```csharp
private bool TryHandleFleet_LongShort(string action, string cmdId)
{
    if (action != "LONG" && action != "SHORT")
        return false;
    if (!MetadataGuardDuplicate(cmdId, action))
        return true;
    if (isTosSyncMode && !TryConsumeTosSyncArm(action))
        return true;
    if (EnableSIMA)
        ExecuteSIMAEntry(action);
    else
        ExecuteRMAEntry(action);
    return true;
}
```

**Projected post-extraction CYC:** Local = 5; blast-radius–weighted ≈ 10–12.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Epic** | EPIC-W7-159 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Bobcoins Used** | 16 |
| **Execution Time** | ~120 seconds |
| **MCP Tools Invoked** | `list_files` ×1, `glob` ×3, `read_file` ×5, `grep` ×5, `write_file` ×2 |
| **Status** | ✅ Completed |
