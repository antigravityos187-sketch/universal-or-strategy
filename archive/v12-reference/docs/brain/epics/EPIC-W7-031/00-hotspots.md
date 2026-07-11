# EPIC-W7-031 — Phase 0: Hotspot Analysis

## Method Name

`AuditMaster_HandleNakedPosition`

## CYC

**19**

## File Path

`src/V12_002.REAPER.Audit.cs` — lines 624–679

## Blast Radius Summary

The method is a leaf called exclusively by `AuditMasterAccountIfNeeded` (line 701), but
the state it touches fans out across **9 files**:

| File | Role |
|---|---|
| `src/V12_002.REAPER.Audit.cs` | Definition + call site |
| `src/V12_002.REAPER.NakedPosition.cs` | `DetectNakedPosition`, `ClearNakedPositionGrace` |
| `src/V12_002.REAPER.NakedStop.cs` | `ProcessReaperNakedStopQueue`, `_reaperNakedStopQueue` |
| `src/V12_002.REAPER.cs` | `_nakedPositionFirstSeen`, `_reaperNakedStopInFlight` fields |
| `src/V12_002.Orders.Callbacks.AccountOrders.cs` | Order-state callbacks that share `_reaperNakedStopInFlight` |
| `src/V12_002.Lifecycle.cs` | Initialization / teardown of naked-stop state |
| `src/V12_002.UI.Compliance.cs` | Compliance display reads `NakedPositionGraceSec` |
| `src/V12_002.cs` | Strategy entry point, `TriggerCustomEvent` dispatcher |
| `src/V12_002.Properties.cs` | `NakedPositionGraceSec` parameter declaration |

Direct callers: 1 (`AuditMasterAccountIfNeeded`).  
Shared-state dependents: 8 additional files.  
Risk level: **Medium-High** — any refactor of the stop-detection LINQ predicate or
grace-window dict must be mirrored against the fleet-account path in `AuditFleet_HandleNakedPosition`.

## Top 3 Complexity Drivers

### 1 — Inline stop-detection LINQ (≈6 predicate branches, CYC contribution ~6)

```csharp
bool masterHasWorkingStop = masterOrders.Any(o =>
    o.Instrument?.FullName == Instrument?.FullName
    && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
    && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
    && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover)
);
```

This block is **already extracted** for fleet accounts as `AuditFleet_CheckWorkingStop`
(line 517) but is duplicated in-line here for Master. It is the single largest driver.

### 2 — Two-branch grace-window fork + ternary clamp (CYC contribution ~4)

```csharp
int graceSeconds = (NakedPositionGraceSec >= 5) ? NakedPositionGraceSec : 5;
if (!_nakedPositionFirstSeen.TryGetValue(Account.Name, out masterFirstSeen))
{
    _nakedPositionFirstSeen[Account.Name] = DateTime.UtcNow;
    Print(…);
}
else if (EnqueueReaperMasterNakedStop(…)) { … }
```

The ternary clamp, the `TryGetValue` branch, and the `else if` on `Enqueue` result all
add independent paths through a single conceptual "am I still in grace?" decision.

### 3 — Exception-recovery path inside the enqueue branch (CYC contribution ~3)

```csharp
try
{
    TriggerCustomEvent(e => ProcessReaperNakedStopQueue(), null);
}
catch (Exception tcEx)
{
    _reaperNakedStopInFlight.TryRemove(masterExpectedKey, out _);
    Print(…);
}
```

The `try/catch` constitutes an extra execution path. The in-flight cleanup on failure
mirrors code already present in four other methods (`AuditFleet_HandleNakedPosition`,
`AuditFleet_HandleDesyncRepair`, `AuditMaster_HandleDesyncFlatten`,
`AuditFleet_HandleCriticalDesyncFlatten`) and should be a shared helper.

## Recommended Extraction Count

**3 extractions** — targeting a post-refactor CYC of ≤ 6 for the residual dispatcher body:

| # | Proposed Method | Eliminates |
|---|---|---|
| 1 | `AuditMaster_CheckWorkingStop()` | Inline LINQ — adapt/reuse `AuditFleet_CheckWorkingStop` for `Account` | ~6 CYC |
| 2 | `AuditMaster_RecordNakedFirstSeen()` | Grace-start log + dict write branch | ~2 CYC |
| 3 | `AuditMaster_TriggerNakedStopEvent()` | `TriggerCustomEvent` + catch + in-flight cleanup | ~3 CYC |

---

## Agent Tracking

| Key | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~45s |
| **Epic** | EPIC-W7-031 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Source Tool** | Bob (jcodemunch-mcp / sequential-thinking / native file tools) |
| **CYC Confirmed** | 19 |
