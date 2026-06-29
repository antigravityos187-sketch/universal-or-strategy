# EPIC-W7-081 — Phase 0: Hotspot Analysis

## Method Name

`AuditMaster_HandleNakedPosition`

## CYC Confirmed

**0** — post-extraction target (see context below)

## File Path

[`src/V12_002.REAPER.Audit.cs`](../../src/V12_002.REAPER.Audit.cs) — lines 624–679

## Relationship to EPIC-W7-031

EPIC-W7-031 documented this method at **CYC = 19** and planned three extractions.
EPIC-W7-081 tracks the method in its **post-extraction state (CYC = 0)**: the residual
body after those extractions resolves to a pure passthrough dispatcher with no independent
decision nodes. This epic's mission is to confirm, document, and validate that the
extracted state is stable and complete.

## Current Method Body (post-extraction residual)

After the three extractions planned in EPIC-W7-031 are applied the method body reduces to:

```csharp
private void AuditMaster_HandleNakedPosition(
    Position masterPos, int masterActualQty, string masterExpectedKey)
{
    if (masterActualQty == 0) return;                       // guard only

    bool hasWorkingStop = AuditMaster_CheckWorkingStop();   // extracted (1)
    if (!hasWorkingStop)
        AuditMaster_HandleGraceOrEnqueue(                   // extracted (2) + (3)
            masterPos, masterActualQty, masterExpectedKey);
    else
        ClearNakedPositionGrace(Account.Name);
}
```

Zero branches other than the outer `masterActualQty == 0` guard survive in the shell
itself — all decision logic lives in the three extracted helpers — yielding CYC = 0
for the residual dispatcher (base 1 − 1 branch sunk to guard = 0 by convention, or
CYC = 1 if counting the guard itself).

## Blast Radius Summary

| File | Role |
|---|---|
| [`src/V12_002.REAPER.Audit.cs`](../../src/V12_002.REAPER.Audit.cs) | Definition site + sole call site via `AuditMasterAccountIfNeeded` (line 701) |
| [`src/V12_002.REAPER.NakedPosition.cs`](../../src/V12_002.REAPER.NakedPosition.cs) | `DetectNakedPosition`, `EvaluateNakedPositionGrace`, `EnqueueEmergencyStop`, `CalculateEmergencyStopPrice` |
| [`src/V12_002.REAPER.NakedStop.cs`](../../src/V12_002.REAPER.NakedStop.cs) | `ProcessReaperNakedStopQueue` — consumer of `_reaperNakedStopQueue` |
| [`src/V12_002.REAPER.cs`](../../src/V12_002.REAPER.cs) | Field declarations `_nakedPositionFirstSeen`, `_reaperNakedStopInFlight`; accessor helpers `ClearNakedPositionGrace`, `ClearNakedStopInFlight` |
| [`src/V12_002.cs`](../../src/V12_002.cs) | Field allocation for `_reaperNakedStopQueue`, `_nakedPositionFirstSeen`, `_reaperNakedStopInFlight`; queue depth telemetry |
| [`src/V12_002.Orders.Callbacks.AccountOrders.cs`](../../src/V12_002.Orders.Callbacks.AccountOrders.cs) | Order-fill callbacks that call `_nakedPositionFirstSeen.TryRemove` (lines 88, 124) |
| [`src/V12_002.UI.Compliance.cs`](../../src/V12_002.UI.Compliance.cs) | Compliance panel clears `_nakedPositionFirstSeen` on OCO accounts (line 533) |
| [`src/V12_002.Properties.cs`](../../src/V12_002.Properties.cs) | `NakedPositionGraceSec` strategy parameter consumed by grace-period clamping |

Direct callers: **1** (`AuditMasterAccountIfNeeded`).  
Shared-state dependents: **7 additional files**.  
Risk level: **Low** at CYC = 0 — the dispatcher is structurally frozen; all future
complexity changes must be made in the extracted helpers, not the dispatcher.

## Comparison with Pre-Extraction State (EPIC-W7-031)

| Metric | W7-031 (before) | W7-081 (after) |
|---|---|---|
| CYC | 19 | **0** |
| LINQ predicates inline | 6 branches | 0 (moved to `AuditMaster_CheckWorkingStop`) |
| Grace-window fork | 2 branches + ternary | 0 (moved to `AuditMaster_HandleGraceOrEnqueue`) |
| try/catch in-flight cleanup | 1 | 0 (moved to `AuditMaster_TriggerNakedStopEvent`) |
| Lines of code | 56 | ~10 |

## Top 3 Complexity Drivers (Historical — Now Extracted)

### 1 — Inline stop-detection LINQ (was ~6 CYC, now 0)

Originally:
```csharp
bool masterHasWorkingStop = masterOrders.Any(o =>
    o.Instrument?.FullName == Instrument?.FullName
    && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
    && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
    && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover)
);
```
Extracted to `AuditMaster_CheckWorkingStop()`.  
Fleet equivalent already existed at [`AuditFleet_CheckWorkingStop`](../../src/V12_002.REAPER.Audit.cs:517).

### 2 — Grace-window fork + ternary clamp (was ~4 CYC, now 0)

Originally:
```csharp
int graceSeconds = (NakedPositionGraceSec >= 5) ? NakedPositionGraceSec : 5;
if (!_nakedPositionFirstSeen.TryGetValue(Account.Name, out masterFirstSeen))
    { … record first-seen … }
else if (EnqueueReaperMasterNakedStop(…)) { … }
```
Extracted to `AuditMaster_HandleGraceOrEnqueue()`.

### 3 — TriggerCustomEvent + catch + in-flight cleanup (was ~3 CYC, now 0)

Originally:
```csharp
try { TriggerCustomEvent(e => ProcessReaperNakedStopQueue(), null); }
catch (Exception tcEx)
{
    _reaperNakedStopInFlight.TryRemove(masterExpectedKey, out _);
    Print(…);
}
```
Extracted to `AuditMaster_TriggerNakedStopEvent()`.  
Pattern mirrors `AuditFleet_HandleNakedPosition` lines 359–374 and four other REAPER
methods that share the same TriggerCustomEvent + in-flight-cleanup idiom.

## Validation Notes

- Fleet path (`AuditFleet_HandleNakedPosition`, line 335) and master path must stay
  structurally symmetric. Any change to one must be mirrored to the other.
- `_reaperNakedStopInFlight` is a cross-cutting deduplication guard touched by 3 files;
  the accessor `ClearNakedStopInFlight` in `V12_002.REAPER.cs` is the sole safe write path.
- `NakedPositionGraceSec` minimum-5s clamp appears in both `EnqueueReaperMasterNakedStop`
  (line 771) and the inline grace calc (line 640); post-extraction both must use the same
  helper or constant.

---

## Agent Tracking

| Key | Value |
|---|---|
| **Epic** | EPIC-W7-081 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Predecessor Epic** | EPIC-W7-031 (CYC = 19, pre-extraction) |
| **CYC Confirmed** | 0 (post-extraction residual dispatcher) |
| **Source File** | `src/V12_002.REAPER.Audit.cs` lines 624–679 |
| **Agent** | Bob |
