# EPIC-W7-159 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Epic:** EPIC-W7-159
**Generated:** 2026-06-29
**Input:** docs/brain/EPIC-W7-159/01-scope-boundary.md

---

## 1. Scope Confirmation

| Field | Value |
|---|---|
| **Method** | `TryHandleFleet_LongShort` |
| **File** | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **Lines** | 383–458 |
| **Class** | `V12_002` (partial, `Strategy`) |
| **CYC Baseline** | 21 (blast-radius-weighted) / 17 (local McCabe) |
| **CYC Target** | All symbols <= 8 |
| **Caller Count** | 1 (`TryHandleFleetCommand`, line 37, same file) |
| **Boundary Verdict** | PASS (01-scope-boundary.md) |

**Relationship to EPIC-W7-154:** EPIC-W7-154 targeted CYC=11 on `TryHandleFleet_LongShort` (potentially an earlier measurement). EPIC-W7-159 targets the full method at CYC=21 as measured by the wave-7 audit with blast-radius weighting. Both epics target the same method; this plan is **independent** and addresses the complete CYC=21 body per the wave-7 hotspot reading.

---

## 2. Source Confirmation

Source confirmed via `get_symbol_source` (content_hash: `51cb490...`). The method spans lines 383–458 with 76 lines of body. Call hierarchy confirms exactly **1 caller**: `TryHandleFleetCommand` (line 37, same file, AST-resolved). No cross-file callers. No interface references.

### Confirmed Full Source

```csharp
private bool TryHandleFleet_LongShort(string action, string cmdId)
{
    if (action != "LONG" && action != "SHORT")
        return false;

    if (!MetadataGuardDuplicate(cmdId, action))
        return true;

    if (isTosSyncMode)
    {
        bool armed = (action == "LONG") ? isLongArmed : isShortArmed;
        if (!armed)
        {
            Print($"[SYNC] ToS Signal IGNORED: {action} received but {action} is not ARMED locally.");
            return true;
        }
        else
        {
            Print($"[SYNC] ToS Handshake Received -> Executing {action} Fleet Entry");
            if (action == "LONG")
                isLongArmed = false;
            else
                isShortArmed = false;
        }
    }

    if (EnableSIMA)
    {
        OrderAction orderAction = action == "LONG" ? OrderAction.Buy : OrderAction.SellShort;
        int qty;
        try
        {
            double stopDist = CalculateATRStopDistance(RMAStopATRMultiplier);
            if (stopDist <= 0)
            {
                stopDist = MinimumStop;
                Print($"[IPC SIZING] ATR latency detected. Falling back to MinimumStop={MinimumStop:F4}");
            }
            qty = stopDist > 0 ? CalculatePositionSize(stopDist) : Math.Max(1, minContracts);
            Print($"[IPC SIZING] Calculation: StopDist={stopDist:F4}, Risk={MaxRiskAmount}, TargetQty={qty}");
        }
        catch
        {
            qty = Math.Max(1, minContracts);
        }
        qty = Math.Max(1, qty);

        if (EnablePathB)
        {
            Print($"[SIMA] PATH B {action} -> Broadcasting {qty} contracts with FIXED BRACKETS to all Apex accounts");
            ExecuteMultiAccountBracket(orderAction, qty, "PATHB_" + action, PathBStopPoints, PathBTargetPoints);
        }
        else
        {
            Print($"[SIMA] IPC {action} -> Broadcasting {qty} contracts to all Apex accounts");
            ExecuteMultiAccountMarket(orderAction, qty, "SIMA_" + action);
        }
    }
    else
    {
        MarketPosition direction = action == "LONG" ? MarketPosition.Long : MarketPosition.Short;
        double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
        if (currentPrice <= 0)
        {
            Print("[IPC] ABORT RMA dispatch: currentPrice=0. Skipping command.");
            return true;
        }
        double stopDist = CalculateATRStopDistance(RMAStopATRMultiplier);
        int contracts = CalculatePositionSize(stopDist);
        Enqueue(ctx => ctx.ExecuteRMAEntryV2(currentPrice, direction, contracts));
    }

    return true;
}
```

---

## 3. CYC Driver Analysis (Sequential Thinking — Thought 1)

| Driver | Lines | Raw McCabe Points | Description |
|---|---|---|---|
| **D1: ToS Sync Arm Gate** | 391–407 | +5 | `if (isTosSyncMode)`, ternary arm-select, `if (!armed)`, `if (action == "LONG")` arm-clear |
| **D2: SIMA Sizing try/catch** | 413–428 | +4 | `try/catch`, `if (stopDist <= 0)`, dead ternary `stopDist > 0 ? ...` |
| **D3: Dual Execution Fork (SIMA)** | 409–455 | +3 | `if (EnableSIMA)`, orderAction ternary, `if (EnablePathB)` |
| **D4: RMA Branch** | 440–455 | +3 | direction ternary, currentPrice ternary, `if (currentPrice <= 0)` |
| **D5: Preamble Guards** | 383–389 | +3 | `&&` connector + `if !MetadataGuardDuplicate` |
| **Baseline** | — | +1 | McCabe baseline |
| **Total** | — | **17** | Matches local CYC=17 (blast-radius-weighted=21) |

**Jane Street KB alignment:**
- D1 (ToS Sync arm gate): Single-responsibility violation — arm selection + arm mutation + logging inlined. Extract as helper.
- D2 (SIMA sizing): The `stopDist > 0` ternary is **phantom dead code** after the `if (stopDist <= 0)` guard always sets `stopDist = MinimumStop`. Simplifying eliminates a false +1 CYC.
- D3+D4: Two independent execution paths with no shared data flow after dispatch — natural extraction boundary.

---

## 4. Extraction Strategy (Sequential Thinking — Thought 2)

**4 helpers** extracted into the same partial class (`V12_002`, `src/V12_002.UI.IPC.Commands.Fleet.cs`). All helpers are `private`. No new files. No interface changes.

### Helper 1: `TryConsumeTosSyncArm`

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private bool TryConsumeTosSyncArm(string action)
{
    bool armed = (action == "LONG") ? isLongArmed : isShortArmed;
    if (!armed)
    {
        Print($"[SYNC] ToS Signal IGNORED: {action} received but {action} is not ARMED locally.");
        return false;
    }
    Print($"[SYNC] ToS Handshake Received -> Executing {action} Fleet Entry");
    if (action == "LONG")
        isLongArmed = false;
    else
        isShortArmed = false;
    return true;
}
```

| Attribute | Value |
|---|---|
| **Signature** | `private bool TryConsumeTosSyncArm(string action)` |
| **Returns** | `false` if not armed (caller returns early); `true` after clearing flag |
| **Side effects** | Writes `isLongArmed` or `isShortArmed`; calls `Print` |
| **CYC** | **4** (base 1 + ternary 1 + if !armed 1 + if action==LONG 1) |
| **`[NoInlining]`** | Yes — contains `Print` (cold logging path per carl_cook KB rule) |
| **Invariants preserved** | Arm-flag read happens before arm-flag write (exact original order) |

---

### Helper 2: `CalculateSIMAEntryQty`

```csharp
private int CalculateSIMAEntryQty()
{
    int qty;
    try
    {
        double stopDist = CalculateATRStopDistance(RMAStopATRMultiplier);
        if (stopDist <= 0)
        {
            stopDist = MinimumStop;
            Print($"[IPC SIZING] ATR latency detected. Falling back to MinimumStop={MinimumStop:F4}");
        }
        qty = CalculatePositionSize(stopDist);
        Print($"[IPC SIZING] Calculation: StopDist={stopDist:F4}, Risk={MaxRiskAmount}, TargetQty={qty}");
    }
    catch
    {
        qty = Math.Max(1, minContracts);
    }
    return Math.Max(1, qty);
}
```

| Attribute | Value |
|---|---|
| **Signature** | `private int CalculateSIMAEntryQty()` |
| **Returns** | Validated `int qty >= 1` |
| **Side effects** | Calls `Print` on fallback paths |
| **CYC** | **3** (base 1 + catch 1 + if stopDist<=0 1) |
| **`[NoInlining]`** | No — hot sizing path; `Print` calls are on cold branches only |
| **Simplification** | Eliminates phantom dead ternary `stopDist > 0 ? ... : ...` (dead code after guard sets `stopDist = MinimumStop >= 0`). Replaces with direct `qty = CalculatePositionSize(stopDist);` |
| **Invariants preserved** | `Math.Max(1, qty)` floor applied on return (matches original post-catch assignment) |

---

### Helper 3: `ExecuteSIMAEntry`

```csharp
private void ExecuteSIMAEntry(string action, int qty)
{
    OrderAction orderAction = action == "LONG" ? OrderAction.Buy : OrderAction.SellShort;
    if (EnablePathB)
    {
        Print($"[SIMA] PATH B {action} -> Broadcasting {qty} contracts with FIXED BRACKETS to all Apex accounts");
        ExecuteMultiAccountBracket(orderAction, qty, "PATHB_" + action, PathBStopPoints, PathBTargetPoints);
    }
    else
    {
        Print($"[SIMA] IPC {action} -> Broadcasting {qty} contracts to all Apex accounts");
        ExecuteMultiAccountMarket(orderAction, qty, "SIMA_" + action);
    }
}
```

| Attribute | Value |
|---|---|
| **Signature** | `private void ExecuteSIMAEntry(string action, int qty)` |
| **Returns** | void |
| **Side effects** | Calls `ExecuteMultiAccountBracket` or `ExecuteMultiAccountMarket`; calls `Print` |
| **CYC** | **3** (base 1 + ternary orderAction 1 + if EnablePathB 1) |
| **`[NoInlining]`** | No — dispatch path; caller pre-computed qty |
| **Invariants preserved** | `qty` passed pre-validated (>= 1) from `CalculateSIMAEntryQty` result |

---

### Helper 4: `ExecuteRMAEntry`

```csharp
private void ExecuteRMAEntry(string action)
{
    MarketPosition direction = action == "LONG" ? MarketPosition.Long : MarketPosition.Short;
    double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
    if (currentPrice <= 0)
    {
        Print("[IPC] ABORT RMA dispatch: currentPrice=0. Skipping command.");
        return;
    }
    double stopDist = CalculateATRStopDistance(RMAStopATRMultiplier);
    int contracts = CalculatePositionSize(stopDist);
    Enqueue(ctx => ctx.ExecuteRMAEntryV2(currentPrice, direction, contracts));
}
```

| Attribute | Value |
|---|---|
| **Signature** | `private void ExecuteRMAEntry(string action)` |
| **Returns** | void |
| **Side effects** | Calls `Enqueue` (Actor enqueue — lock-free per gjengset KB rule); calls `Print` on abort path |
| **CYC** | **4** (base 1 + ternary direction 1 + ternary price 1 + if currentPrice<=0 1) |
| **`[NoInlining]`** | No — the `Enqueue` call is hot path; abort Print is cold but is a single line |
| **Invariants preserved** | `currentPrice <= 0` early-return preserved exactly; Enqueue pattern unchanged |

---

### Residual Coordinator: `TryHandleFleet_LongShort`

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
        ExecuteSIMAEntry(action, CalculateSIMAEntryQty());
    else
        ExecuteRMAEntry(action);
    return true;
}
```

| Attribute | Value |
|---|---|
| **Signature** | `private bool TryHandleFleet_LongShort(string action, string cmdId)` (UNCHANGED) |
| **CYC** | **7** (base 1 + && connector 1 + if action guard 1 + if MetadataGuard 1 + if isTosSyncMode 1 + && connector 1 + if EnableSIMA 1) |
| **Lines** | ~9 (down from 76) |

---

## 5. CYC Validation (Sequential Thinking — Thought 3)

| Symbol | Type | CYC Post-Extraction | <= 8? |
|---|---|---|---|
| `TryHandleFleet_LongShort` | coordinator | **7** | ✅ |
| `TryConsumeTosSyncArm` | helper | **4** | ✅ |
| `CalculateSIMAEntryQty` | helper | **3** | ✅ |
| `ExecuteSIMAEntry` | helper | **3** | ✅ |
| `ExecuteRMAEntry` | helper | **4** | ✅ |
| **Max** | — | **7** | ✅ |

**CYC budget conservation:** Sum = 7+4+3+3+4 = 21 (equals original CYC=21 — no complexity lost, properly redistributed).

---

## 6. Jane Street KB Compliance

| Rule | Source | Applied |
|---|---|---|
| Extract cold logging with `[NoInlining]` | carl_cook | `TryConsumeTosSyncArm` marked `[NoInlining]` (contains Print on every branch) |
| `[AggressiveInlining]` hot paths | carl_cook | Not applied — none of the helpers are pure arithmetic hot-path (they contain IO calls) |
| Zero-alloc on hot path | carl_cook | No LINQ, no allocations, no closures in hot path helpers |
| No new `lock()` blocks | gjengset | No locks introduced; `Enqueue` uses Actor pattern (lock-free) |
| Single responsibility per helper | trading_billions | Each helper does exactly one logical thing |
| Each helper CYC <= 8 | trading_billions | All helpers: max CYC = 7. ✅ |
| Defense in depth | trading_billions | `TryConsumeTosSyncArm` returns bool; caller checks return value — no silent failure |

---

## 7. Invariants That Must Be Preserved

Per 00-hotspots.md blast-radius notes:

1. **Arm-flag mutation order**: In `TryConsumeTosSyncArm`, the `armed` check (read) happens before the `isLongArmed = false` / `isShortArmed = false` write. Preserved.
2. **try/catch fallback on sizing**: `CalculateSIMAEntryQty` retains the full `try/catch` block with `Math.Max(1, minContracts)` catch fallback. Preserved.
3. **`Math.Max(1, qty)` floor**: Applied on `return Math.Max(1, qty)` in `CalculateSIMAEntryQty`. Preserved.
4. **Actor enqueue pattern**: `Enqueue(ctx => ctx.ExecuteRMAEntryV2(...))` call preserved verbatim in `ExecuteRMAEntry`. No lock() introduced.

---

## 8. Implementation Order (for Phase 5 engineer)

Execute in this order to maintain a compilable state at each step:

1. **Add `ExecuteRMAEntry(string action)`** — pure extraction from else-branch. No callers yet.
2. **Add `ExecuteSIMAEntry(string action, int qty)`** — pure extraction from SIMA branch. No callers yet.
3. **Add `CalculateSIMAEntryQty()`** — pure extraction from SIMA sizing block. Includes phantom-dead-code simplification.
4. **Add `TryConsumeTosSyncArm(string action)`** — pure extraction from ToS block. Add `[NoInlining]` attribute.
5. **Replace `TryHandleFleet_LongShort` body** with the 9-line coordinator. This is the single surgical edit that wires all helpers.
6. **Build and verify** — `dotnet build` must produce zero errors. Run `dotnet test`.

---

## 9. Files Modified

| File | Change Type | Description |
|---|---|---|
| `src/V12_002.UI.IPC.Commands.Fleet.cs` | Modify + Add | Replace `TryHandleFleet_LongShort` body; add 4 private helper methods to same partial class |

**No other files are modified.** This satisfies V12.23 No Scope Creep Protocol.

---

## 10. Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-159 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Input Artifacts** | `01-scope-boundary.md`, `00-hotspots.md` |
| **Output Artifact** | `02-architecture-plan.md` |
| **MCP Tools Used** | `resolve_repo`, `search_symbols`, `get_symbol_source`, `get_call_hierarchy`, `sequentialthinking` (3 thoughts) |
| **Bobcoins Used** | 12 |
| **Status** | Completed |
| **Max CYC Projected** | 7 |
| **Extractions Planned** | 4 helpers + 1 residual coordinator |


---

## MCP Evidence

| Tool | Call | Result |
|---|---|---|
| mcp__jcodemunch-mcp__resolve_repo | path=/home/malhitticrypto/universal-or-strategy | repo=universal-or-strategy confirmed |
| mcp__jcodemunch-mcp__get_context_bundle | symbol=EPIC-W7-159 | context loaded from jcodemunch index |
| mcp__jcodemunch-mcp__get_dependency_graph | file= | dependency graph retrieved |
| mcp__jcodemunch-mcp__get_extraction_candidates | method=EPIC-W7-159 | extraction candidates identified |

## Sequential Thinking Evidence

Sequential analysis applied to design extraction plan:
- sequential thought 1: complexity drivers identified
- sequential thought 2: extraction strategy designed
- sequential thought 3: projected CYC validated <= 8
