# EPIC-W7-159 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Epic:** EPIC-W7-159
**Generated:** 2026-06-29
**Inputs:** `docs/brain/EPIC-W7-159/02-architecture-plan.md`, `docs/brain/EPIC-W7-159/03-audit-report.md`

---

## Summary

| Field | Value |
|---|---|
| **Method** | `TryHandleFleet_LongShort` |
| **File** | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **CYC Baseline** | 21 |
| **ticket_count** | **3** |
| **projected_parent_cyc_after_all** | **7** |
| **Max helper CYC** | 4 |
| **All symbols <= 8** | ✅ |

---

## Ticket 1 — Extract SIMA/RMA Execution Helpers

| Field | Value |
|---|---|
| **ticket_id** | `W7-159-T1` |
| **helper_names** | `ExecuteRMAEntry`, `ExecuteSIMAEntry`, `CalculateSIMAEntryQty` |
| **concern** | CYC drivers D2 (SIMA sizing try/catch), D3 (dual execution fork SIMA), D4 (RMA dispatch branch) |
| **lines_to_move** | ~43 lines (SIMA block lines 413–455 + RMA else-branch lines 440–455 from original body) |
| **cyc_reduction** | −10 McCabe points removed from parent when wired in Ticket 3 |
| **projected_helper_cyc** | `ExecuteRMAEntry`=4, `ExecuteSIMAEntry`=3, `CalculateSIMAEntryQty`=3 — max **4** ✅ |
| **depends_on** | None (add-only; no existing code changed) |

### Helpers to Add

**`CalculateSIMAEntryQty`** — CYC=3

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

**`ExecuteSIMAEntry`** — CYC=3

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

**`ExecuteRMAEntry`** — CYC=4

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

### Invariants

- `CalculateSIMAEntryQty`: phantom dead ternary `stopDist > 0 ? ... : ...` eliminated (dead after guard sets `stopDist = MinimumStop`). Replaced with direct `qty = CalculatePositionSize(stopDist)`.
- `CalculateSIMAEntryQty`: `Math.Max(1, qty)` floor applied on return — matches original post-catch assignment.
- `ExecuteRMAEntry`: `Enqueue(ctx => ctx.ExecuteRMAEntryV2(...))` preserved verbatim (lock-free Actor pattern).
- `ExecuteRMAEntry`: `currentPrice <= 0` early-return preserved exactly.

### Verification

- `dotnet build` must succeed with zero errors after adding these 3 methods (no callers yet).
- No existing method bodies modified.

---

## Ticket 2 — Extract ToS Sync Arm Gate Helper

| Field | Value |
|---|---|
| **ticket_id** | `W7-159-T2` |
| **helper_name** | `TryConsumeTosSyncArm` |
| **concern** | CYC driver D1 — arm selection, arm mutation, Print calls (cold logging path, `[NoInlining]` required) |
| **lines_to_move** | ~17 lines (ToS block lines 391–407 from original body) |
| **cyc_reduction** | −5 McCabe points removed from parent when wired in Ticket 3 |
| **projected_helper_cyc** | **4** ✅ (base 1 + ternary arm-select 1 + `if (!armed)` 1 + `if (action == "LONG")` arm-clear 1) |
| **depends_on** | None (add-only; no existing code changed) |

### Helper to Add

**`TryConsumeTosSyncArm`** — CYC=4, `[NoInlining]`

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

### Invariants

- Arm-flag read (`armed = ...`) happens before arm-flag write (`isLongArmed = false` / `isShortArmed = false`) — exact original order preserved.
- Returns `false` when not armed (caller uses this to early-return `true`); returns `true` after clearing flag.
- `[NoInlining]` applied — cold logging path per Jane Street KB (carl_cook rule: extract cold `Print` calls with `[NoInlining]`).

### Verification

- `dotnet build` must succeed with zero errors after adding this method (no callers yet).
- No existing method bodies modified.

---

## Ticket 3 — Replace Coordinator Body (Wire All Helpers)

| Field | Value |
|---|---|
| **ticket_id** | `W7-159-T3` |
| **helper_name** | `TryHandleFleet_LongShort` (residual coordinator — existing method, body replaced) |
| **concern** | D5 preamble guards + wire all 4 extracted helpers; single surgical edit to the existing method body |
| **lines_to_move** | Replace lines 383–458 (~76 lines) with the 9-line coordinator body |
| **cyc_reduction** | Parent: 21 → 7 = **−14 net** |
| **projected_helper_cyc** | N/A (coordinator, not a helper) |
| **projected_parent_cyc** | **7** ✅ |
| **depends_on** | `W7-159-T1` and `W7-159-T2` both completed |

### Coordinator Replacement Body

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

**CYC breakdown:** base 1 + `&&` connector (action guard) 1 + `if` action guard 1 + `if` MetadataGuard 1 + `if isTosSyncMode` 1 + `&&` connector (ToS short-circuit) 1 + `if EnableSIMA` 1 = **7**

### Verification

- `dotnet build` must succeed with zero errors.
- `dotnet test` must pass.
- `powershell -File .\deploy-sync.ps1` must succeed (NinjaTrader hard-link re-sync).
- Confirm `TryHandleFleet_LongShort` body is exactly 9 lines (excluding braces).

---

## projected_parent_cyc_after_all: 7

All 5 symbols after extraction:

| Symbol | Role | CYC | <= 8? |
|---|---|---|---|
| `TryHandleFleet_LongShort` | Coordinator (Ticket 3) | **7** | ✅ |
| `TryConsumeTosSyncArm` | Helper — ToS arm gate (Ticket 2) | **4** | ✅ |
| `CalculateSIMAEntryQty` | Helper — SIMA sizing (Ticket 1) | **3** | ✅ |
| `ExecuteSIMAEntry` | Helper — SIMA dispatch (Ticket 1) | **3** | ✅ |
| `ExecuteRMAEntry` | Helper — RMA dispatch (Ticket 1) | **4** | ✅ |
| **Max** | — | **7** | ✅ |

**CYC budget conservation:** 7 + 4 + 3 + 3 + 4 = **21** = original CYC=21 (complexity redistributed, none lost).

---

## Execution Order for Phase 5 Engineer

1. Complete **Ticket 1** — add `ExecuteRMAEntry`, `ExecuteSIMAEntry`, `CalculateSIMAEntryQty` to file; build verify.
2. Complete **Ticket 2** — add `TryConsumeTosSyncArm` with `[NoInlining]`; build verify.
3. Complete **Ticket 3** — replace `TryHandleFleet_LongShort` body with 9-line coordinator; build + test + deploy-sync.

---

## MCP Evidence

| Tool | Parameters | Result |
|---|---|---|
| `mcp__jcodemunch-mcp__resolve_repo` | `path=/home/malhitticrypto/universal-or-strategy` | `repo=antigravityos187-sketch/universal-or-strategy`, `indexed=true`, `symbol_count=5147` — ✅ |
| `mcp__jcodemunch-mcp__get_symbol_complexity` | `symbol_id=TryHandleFleet_LongShort` | `{"error":"Symbol not found in index"}` — symbol pre-extraction; CYC=21 sourced from wave-7 audit and 02-architecture-plan.md |
| `mcp__jcodemunch-mcp__get_extraction_candidates` | `file_path=src/V12_002.UI.IPC.Commands.Fleet.cs` | `candidates=[]` — no candidates returned (callers below min_callers=2 threshold; single-caller method per architecture plan). CYC analysis sourced from Phase 2 architecture plan which confirmed CYC=21 via direct McCabe analysis. |

---

## Sequential Thinking Evidence

| Thought | Summary |
|---|---|
| **Probe** | `probe: starting EPIC-W7-159 Phase 4 ticket generation` — MCP confirmed. |
| **Thought 1** | Determined ticket_count=3 by mapping CYC drivers D1/D2+D3/D4/D5 to logical concern groups: (1) SIMA+RMA helpers (D2+D3+D4), (2) ToS sync helper (D1), (3) coordinator wiring (D5+wire). All add-only tickets safe before coordinator replacement. |
| **Thought 2** | Detailed each ticket: T1 adds 3 helpers (max CYC=4), T2 adds 1 helper with `[NoInlining]` (CYC=4), T3 replaces coordinator body (CYC=7). Phantom dead ternary elimination in CalculateSIMAEntryQty confirmed. Dependencies: T1,T2 independent; T3 depends on T1+T2. |
| **Thought 3** | Verified all symbols: max CYC=7 (coordinator). Sum=21 conserved. All 5 symbols ≤ 8. Ticket ordering safe (add-only tickets 1+2 maintain compilable state; ticket 3 is the single wiring edit). VERIFIED. |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-159 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Lane** | P4-L10 |
| **Input Artifacts** | `02-architecture-plan.md`, `03-audit-report.md` |
| **Output Artifact** | `04-tickets.md` |
| **MCP Tools Used** | `resolve_repo`, `sequentialthinking` (1 probe + 3 thoughts), `get_symbol_complexity`, `get_extraction_candidates` |
| **Bobcoins Used** | 6 |
| **Execution Time** | 2026-06-29T01:30:00Z |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 7 |
| **Status** | Completed |
