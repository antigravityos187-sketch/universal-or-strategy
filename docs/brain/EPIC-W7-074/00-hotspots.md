# EPIC-W7-074 | Phase 0 — Hotspot Analysis

**Wave:** 7 | **Phase:** 0  
**Method:** `AttachExecutionPanelHandlers`  
**Source:** [`src/V12_002.UI.Panel.Handlers.cs`](../../src/V12_002.UI.Panel.Handlers.cs:96)  
**Generated:** Phase 0 — Hotspot Analysis

---

## 1. Complexity Summary

| Metric                | Value |
|-----------------------|-------|
| Cyclomatic Complexity | **12** (confirmed) |
| Lines (method body)   | 53 (lines 96–149) |
| Branch count          | 11 null-guards + 1 base path |
| Handler types         | 9 inline lambdas, 2 named-method delegates |
| Call site             | `AttachPanelHandlers` (line 46, same file) |
| Execution context     | UI thread — panel construction only (one-shot) |

### CYC Breakdown

Each `if (button != null)` null-guard constitutes an independent decision point (+1 each).  
Base path = 1.  Total = 1 + 11 guards = **12**.

```
if (orLongButton != null)      → +1  [lambda: OR_LONG  + ResetExecutionMode + TriggerGlow(CyanAccent)]
if (orShortButton != null)     → +1  [lambda: OR_SHORT + ResetExecutionMode + TriggerGlow(PinkFg)]
if (retestButton != null)      → +1  [delegate: OnRetestClick]
if (retestRmaToggle != null)   → +1  [delegate: OnRetestRmaToggleClick]
if (rmaButton != null)         → +1  [delegate: OnRmaClick]
if (momoButton != null)        → +1  [lambda: MODE_MOMO + ResetExecutionMode + TriggerGlow(GreenFg)]
if (ffmaButton != null)        → +1  [lambda: MODE_FFMA + ResetExecutionMode + TriggerGlow(PinkFg)]
if (ffmaManualButton != null)  → +1  [lambda: FFMA_MANUAL_MARKET + ResetExecutionMode + TriggerGlow(PinkFg)]
if (mButton != null)           → +1  [lambda: MODE_M + TriggerGlow(OrangeFg)]
if (trendButton != null)       → +1  [delegate: OnTrendClick]
if (trendRmaToggle != null)    → +1  [delegate: OnTrendRmaToggleClick]
```

---

## 2. Blast Radius

### Direct call chain

```
AttachExecutionPanelHandlers
  └─ each handler → PanelCommand(string)          [line 935, same file]
       └─ Enqueue(ctx => ...)
            ├─ ctx.TryHandleModeCommand()          [V12_002.UI.IPC.Commands.Mode.cs]
            ├─ ctx.TryHandleRiskCommand()          [V12_002.UI.IPC.Commands.Mode.cs]
            ├─ ctx.TryHandleFleetCommand()         [V12_002.UI.IPC.Commands.Fleet.cs]
            └─ ctx.TryHandleConfigCommand()        [V12_002.UI.IPC.Commands.Misc.cs]
```

### Side-effect dependencies (state mutations inside lambdas)

| Symbol            | Mutated by handler(s)        | Also read/written in (N files) |
|-------------------|------------------------------|-------------------------------|
| `isRMAModeActive` | OR_LONG, OR_SHORT, MOMO, FFMA, FFMA_MANUAL (via `ResetExecutionMode`) | 13 files |
| `retestCycleState`| OR_LONG, OR_SHORT, MOMO, FFMA, TREND (via `ResetExecutionMode`) | 1 file |
| `isTrendRmaToggle`| OR_LONG, OR_SHORT, MOMO, FFMA, TREND (via `ResetExecutionMode`) | 1 file |
| `isRetestRmaToggle`| OR_LONG, OR_SHORT, MOMO, FFMA, TREND (via `ResetExecutionMode`) | 1 file |

### Files in blast radius

- [`src/V12_002.UI.Panel.Handlers.cs`](../../src/V12_002.UI.Panel.Handlers.cs) — source
- [`src/V12_002.UI.IPC.Commands.Mode.cs`](../../src/V12_002.UI.IPC.Commands.Mode.cs) — `TryHandleModeCommand`, `TryHandleRiskCommand`
- [`src/V12_002.UI.IPC.Commands.Fleet.cs`](../../src/V12_002.UI.IPC.Commands.Fleet.cs) — `TryHandleFleetCommand`
- [`src/V12_002.UI.IPC.Commands.Misc.cs`](../../src/V12_002.UI.IPC.Commands.Misc.cs) — `TryHandleConfigCommand`
- [`src/V12_002.UI.Callbacks.cs`](../../src/V12_002.UI.Callbacks.cs) — `ClearClickTraderBorderIfInactive`, reads `isRMAModeActive`
- [`src/V12_002.UI.Panel.StateSync.cs`](../../src/V12_002.UI.Panel.StateSync.cs) — `UpdateRmaButtonVisual`
- [`src/V12_002.UI.Panel.Construction.cs`](../../src/V12_002.UI.Panel.Construction.cs) — `UpdateRmaButtonVisual`
- [`src/V12_002.UI.IPC.Server.cs`](../../src/V12_002.UI.IPC.Server.cs) — reads `isRMAModeActive`
- [`src/V12_002.UI.Snapshot.cs`](../../src/V12_002.UI.Snapshot.cs) — reads `isRMAModeActive`
- [`src/V12_002.Entries.RMA.cs`](../../src/V12_002.Entries.RMA.cs) — clears `isRMAModeActive`
- [`src/V12_002.Entries.MOMO.cs`](../../src/V12_002.Entries.MOMO.cs) — reads `isRMAModeActive`, calls `ClearClickTraderBorderIfInactive`
- [`src/V12_002.UI.IPC.cs`](../../src/V12_002.UI.IPC.cs) — parallel command dispatch path
- [`src/V12_002.UI.Panel.Lifecycle.cs`](../../src/V12_002.UI.Panel.Lifecycle.cs) — `TriggerGlow` definition

**Total blast-radius file count: 13**

---

## 3. Complexity Driver Classification

| Driver type         | Count | Notes |
|---------------------|-------|-------|
| Null-guard branches | 11    | Mechanical, structural — no business logic |
| Named-method delegates | 2  | `OnRetestClick` (CYC 5), `OnTrendRmaToggleClick` (CYC 2) |
| Inline lambdas      | 9    | Each calls `PanelCommand` + `ResetExecutionMode` + `TriggerGlow` |
| State mutations     | 4    | Via `ResetExecutionMode` side-effects |

**Classification:** Structural/mechanical complexity — complexity is driven by repetitive null-guards, not conditional business logic. The method itself is a pure registration method called once at panel construction.

---

## 4. Refactoring Opportunities

### [R1] Extract `BindClick` null-guard helper — drops CYC 11 → 1
```csharp
// Helper pattern (not yet implemented):
private static void BindClick(Button btn, RoutedEventHandler handler)
{
    if (btn != null) btn.Click += handler;
}
```
Would reduce method CYC from 12 to 1. Zero behavioural change.

### [R2] Extract remaining 9 inline lambdas to named methods
Consistent with the existing pattern used by `OnRetestClick`, `OnTrendClick`.  
Improves unit-testability; eliminates 9 heap-allocated lambda closures at construction time.

### [R3] Group OR_LONG/OR_SHORT handlers (low priority)
Both share identical structure (`PanelCommand → ResetExecutionMode → TriggerGlow`). Could be parameterised.

---

## 5. Risk Assessment

| Risk area         | Level  | Notes |
|-------------------|--------|-------|
| Runtime safety    | LOW    | One-shot construction; UI thread only; no data-race risk |
| Regression surface| MEDIUM | `isRMAModeActive` is read/written in 13 files |
| Testability       | LOW    | Current lambdas are not individually unit-testable |
| Refactoring cost  | LOW    | Purely structural; no logic changes required |

**Refactoring priority: MEDIUM** — improves readability and testability; no correctness or performance risk.

---

## 6. References

- Method body: [`src/V12_002.UI.Panel.Handlers.cs:96`](../../src/V12_002.UI.Panel.Handlers.cs:96)
- Call site: [`AttachPanelHandlers` line 46](../../src/V12_002.UI.Panel.Handlers.cs:42)
- Command bus: [`PanelCommand` line 935](../../src/V12_002.UI.Panel.Handlers.cs:935)
- `ResetExecutionMode`: [`line 558`](../../src/V12_002.UI.Panel.Handlers.cs:558)
- `TriggerGlow`: [`V12_002.UI.Panel.Lifecycle.cs:114`](../../src/V12_002.UI.Panel.Lifecycle.cs:114)
