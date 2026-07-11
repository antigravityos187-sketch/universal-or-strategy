# Phase 2: Architecture Plan — EPIC-W7-158

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-158/01-scope-boundary.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `SyncModeChipVisuals` |
| **Source File** | `src/V12_002.UI.Panel.StateSync.cs` |
| **Lines** | 358–408 (50 lines) |
| **Visibility** | `private` |
| **Class** | `V12_002` (partial) |
| **Original CYC** | 9 |
| **Target CYC** | <= 8 |
| **Caller** | `UpdatePanelState` (same file, line 36) |

---

## jCodemunch MCP Results

### get_context_bundle

Symbol ID: `src/V12_002.UI.Panel.StateSync.cs::V12_002.SyncModeChipVisuals#method`

Full source retrieved (lines 358-408). The method signature is `private void SyncModeChipVisuals(string mode)`.
No docstring. Imports: `System`, `System.Windows.Controls`, `System.Windows.Media`, `NinjaTrader.Cbi`.

Source body confirmed:
```csharp
private void SyncModeChipVisuals(string mode)
{
    // Segment 1: Reset loop — foreach over 6 buttons with null guard
    foreach (Button btn in new[] { modeOrbButton, modeRmaButton, modeRetestButton,
                                    modeMomoButton, modeFfmaButton, modeTrendButton })
    {
        if (btn == null) continue;
        btn.Background = BtnBg;
        btn.Foreground = TextMuted;
        btn.BorderBrush = BtnBorder;
    }

    // Segment 2: Active button resolution — switch on mode string (5 cases + default)
    Button activeButton = null;
    switch ((mode ?? "ORB").ToUpperInvariant())
    {
        case "RMA":    activeButton = modeRmaButton;    break;
        case "RETEST": activeButton = modeRetestButton; break;
        case "MOMO":   activeButton = modeMomoButton;   break;
        case "FFMA":   activeButton = modeFfmaButton;   break;
        case "TREND":  activeButton = modeTrendButton;  break;
        default:       activeButton = modeOrbButton;    break;
    }

    // Segment 3: Highlight guard — null-checked highlight application
    if (activeButton != null)
    {
        activeButton.Background = CyanBg;
        activeButton.Foreground = CyanFg;
        activeButton.BorderBrush = CyanBorder;
    }
}
```

### get_call_hierarchy

- **Callers (depth 1):** `UpdatePanelState` — `src/V12_002.UI.Panel.StateSync.cs:13` (ast_resolved, depth=1)
- **Callees:** none
- **Dispatches:** none
- **caller_count:** 1 | **callee_count:** 0

Single caller in same file. Method signature must remain unchanged.

### get_dependency_graph

- **Direction:** both | **Depth:** 1
- **node_count:** 1 | **edge_count:** 0
- **imports:** [] | **importers:** []

No cross-file import edges. The file is a self-contained partial class with no indexed dependencies.
All WPF control references are field members of the same partial class.

### get_extraction_candidates

- **Result:** No candidates returned (empty list)
- **min_complexity:** 3 | **min_callers:** 1
- **Note:** Index does not track private method callers from within the same file; analysis proceeded
  from get_context_bundle source + CYC decomposition in the hotspot analysis.

---

## CYC Decomposition (Original)

| Decision Point | Source | +CYC |
|---|---|---|
| Base | method entry | +1 |
| `foreach` loop | reset pass iteration | +1 |
| `if (btn == null) continue` | null guard in loop | +1 |
| `case "RMA"` | switch arm | +1 |
| `case "RETEST"` | switch arm | +1 |
| `case "MOMO"` | switch arm | +1 |
| `case "FFMA"` | switch arm | +1 |
| `case "TREND"` | switch arm | +1 |
| `if (activeButton != null)` | post-switch guard | +1 |
| **Total** | | **9** |

---

## Sequential Thinking Summary

sequentialthinking chain (5 thoughts):

1. **Thought 1 — Source Analysis:** Full method source confirmed from get_context_bundle. Three logical segments identified: (a) foreach reset loop with null guard (+2), (b) switch on mode string with 5 cases (+5), (c) post-switch null guard (+1). CYC = 9 confirmed. One caller: `UpdatePanelState`.

2. **Thought 2 — Minimal Extraction:** Extracting the switch block into `ResolveActiveModeButton(string mode)` removes 5 branch decisions from the parent. Parent residual CYC = 4 (base + foreach + null-in-loop + post-switch guard). Helper CYC = 6 (base + 5 switch arms). Both under threshold.

3. **Thought 3 — Jane Street Alignment:** `ResolveActiveModeButton` is a pure mapping function (no side effects, no state mutations) — single responsibility satisfied. Lock-free: method runs on WPF dispatcher thread, no locks introduced. Illegal states: `?? "ORB"` default makes nominal state always representable.

4. **Thought 4 — Optional Second Extraction:** `ResetModeChipStyles()` can extract the foreach reset loop (−2 CYC from parent). Parent becomes CYC 2, helper becomes CYC 3. Both well within threshold. Included as second extraction for clean single-responsibility design.

5. **Thought 5 — Final Verification:** 2 helpers confirmed: `ResolveActiveModeButton` (CYC 6) + `ResetModeChipStyles` (CYC 3). Parent after both extractions: CYC 2. Max projected CYC = 6. All ≤ 8. Signature unchanged. No cross-file changes. Architecture plan ready.

---

## Extraction Plan

| # | Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|---|
| 1 | `ResolveActiveModeButton` | `private Button ResolveActiveModeButton(string mode)` | Maps mode string to its corresponding WPF Button reference via switch. Returns `modeOrbButton` as default. | **6** |
| 2 | `ResetModeChipStyles` | `private void ResetModeChipStyles()` | Iterates all 6 mode buttons, skipping nulls, and resets `Background`, `Foreground`, `BorderBrush` to default brush values. | **3** |

---

## Parent Method After Extraction

**Remaining logic in `SyncModeChipVisuals(string mode)` after extraction:**

```csharp
private void SyncModeChipVisuals(string mode)
{
    ResetModeChipStyles();                              // delegate reset pass
    Button activeButton = ResolveActiveModeButton(mode); // delegate resolution
    if (activeButton != null)
    {
        activeButton.Background = CyanBg;
        activeButton.Foreground = CyanFg;
        activeButton.BorderBrush = CyanBorder;
    }
}
```

- **Remaining decisions:** base(1) + null-guard(1) = **CYC 2**
- **Projected CYC:** 2

---

## max_cyc_projected: 6
## extraction_count: 2

---

## Jane Street Alignment

| Principle | Status |
|---|---|
| CYC<=8 achieved | YES — max projected CYC is 6 (ResolveActiveModeButton) |
| Single-responsibility per helper | YES — mapping vs reset are distinct concerns |
| Lock-free/Actor pattern preserved | YES — no locks introduced; WPF dispatcher thread unchanged |
| Illegal states unrepresentable | YES — `?? "ORB"` default in ResolveActiveModeButton ensures a valid button is always resolved |
| Zero-allocation hot path | YES — Button return is a reference; no heap allocations added |
| No cross-file changes | YES — all helpers private in same partial class |
| Caller signature unchanged | YES — `private void SyncModeChipVisuals(string mode)` preserved |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 3 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-158 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output** | docs/brain/EPIC-W7-158/02-architecture-plan.md |
