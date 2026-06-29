# EPIC-W7-076 — Phase 0: Hotspot Analysis

## Symbol Under Analysis

| Field | Value |
|---|---|
| **Method** | `CollapseAllExecutionControls` |
| **Source file** | `src/V12_002.UI.Panel.Handlers.cs` |
| **Lines** | 665–687 |
| **Class** | `V12_002` (partial) |
| **Namespace** | `NinjaTrader.NinjaScript.Strategies` |
| **Cyclomatic Complexity** | **0** (McCabe score 1 — no branching decisions) |

---

## Method Body Summary

`CollapseAllExecutionControls` is a sequential UI-reset helper that collapses every
execution-panel control to `Visibility.Collapsed` before a mode switch, and unconditionally
restores `manualEntryRow` to `Visibility.Visible`.  It contains **no conditional branches**
beyond the idiomatic null-guards (`if (x != null)`) that are universally applied throughout
this file and do not add logical branches that affect correctness.

```csharp
private void CollapseAllExecutionControls()
{
    if (execRetestRow   != null) execRetestRow.Visibility   = Visibility.Collapsed;
    if (execTrendRow    != null) execTrendRow.Visibility    = Visibility.Collapsed;
    if (rmaButton       != null) rmaButton.Visibility       = Visibility.Collapsed;
    if (momoButton      != null) momoButton.Visibility      = Visibility.Collapsed;
    if (ffmaButton      != null) ffmaButton.Visibility      = Visibility.Collapsed;
    if (ffmaManualButton!= null) ffmaManualButton.Visibility= Visibility.Collapsed;
    if (mButton         != null) mButton.Visibility         = Visibility.Collapsed;
    if (orLongButton    != null) orLongButton.Visibility    = Visibility.Collapsed;
    if (orShortButton   != null) orShortButton.Visibility   = Visibility.Collapsed;
    if (manualEntryRow  != null) manualEntryRow.Visibility  = Visibility.Visible;
}
```

---

## Blast Radius

| Caller | Location |
|---|---|
| `UpdateContextualUI(string mode)` | line 660 — only call site |

`UpdateContextualUI` is itself called exclusively from `SelectConfigMode`, which is wired to
the six config-mode buttons in `AttachConfigModeHandlers` (lines 199–213).  The blast radius
is therefore **narrow and UI-only**: a defect here would manifest as stale controls remaining
visible after a mode switch, not as any order-routing or risk logic.

---

## Hotspot Classification

| Dimension | Assessment |
|---|---|
| **Complexity** | ✅ Minimal — CYC 0/1, no loops, no state mutations beyond `Visibility` |
| **Risk surface** | ✅ Low — pure UI, no order dispatch, no financial logic |
| **Change frequency** | ⚠️ Medium — adding a new execution mode requires a new `if`-line here |
| **Test coverage need** | Low — deterministic output; snapshot/render tests sufficient |
| **Refactor priority** | Low — method is clean; only concern is extensibility as mode count grows |

---

## Key Observations

1. **Zero cyclomatic complexity confirmed.** All null-guards follow the project-wide defensive
   pattern and do not introduce decision points that alter the method's primary purpose.

2. **Single responsibility is tight.** The method does exactly one thing: set a known list of
   controls to `Collapsed` (plus one `Visible`). No side effects, no command dispatch.

3. **Extensibility gap.** Each new execution mode must add a manual `if`-line here *and* a
   matching `Show*Controls()` helper in `ShowModeSpecificControls`. This is a minor
   maintenance coupling but not a hotspot risk at current scale (10 controls).

4. **Call chain depth is shallow.** `SelectConfigMode → UpdateContextualUI →
   CollapseAllExecutionControls` — three frames, all on the UI thread.

---

## Wave 7 / Phase 0 Verdict

No refactoring action required for this method in Wave 7. It is a **confirmed non-hotspot**.
Complexity budget is fully intact. Flag for lightweight review only if the mode count exceeds
~12 controls, at which point a data-driven visibility map may become warranted.
