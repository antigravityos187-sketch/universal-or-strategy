# EPIC-W7-158 — Phase 0: Hotspot Analysis

## Target Method

| Field               | Value                                   |
|---------------------|-----------------------------------------|
| **Method**          | `SyncModeChipVisuals`                   |
| **CYC (Confirmed)** | 9                                       |
| **File**            | `src/V12_002.UI.Panel.StateSync.cs`     |
| **Lines**           | 358–408 (50 lines)                      |
| **Visibility**      | `private`                               |
| **Class**           | `V12_002` (partial)                     |

---

## Blast Radius Summary

`SyncModeChipVisuals` is a **UI chip highlight synchroniser** called exclusively when the active trading mode
changes, resetting all six mode-button visuals and highlighting the selected one.

**Callers:**

| Caller | Site | Context |
|--------|------|---------|
| `UpdatePanelState` | `V12_002.UI.Panel.StateSync.cs:36` | Invoked only when `_panelLastSyncedMode` differs from the current snapshot mode — i.e., change-gated |

**Direct downstream effects (by mutation):**

| Widget Field | Property Written | Count |
|---|---|---|
| `modeOrbButton`, `modeRmaButton`, `modeRetestButton`, `modeMomoButton`, `modeFfmaButton`, `modeTrendButton` | `Background`, `Foreground`, `BorderBrush` | 3 per button (reset pass) |
| One of the above (active) | `Background`, `Foreground`, `BorderBrush` | 3 (highlight pass) |

**Cross-file blast surface:** 1 caller in the same file, 0 callers in other files.
All mutations run on the **WPF dispatcher thread** (inherited from `UpdatePanelState`'s dispatch context).
No state fields written; method is **side-effect–bounded** to WPF visual properties only.

---

## Top 3 Complexity Drivers

### 1 — Flat 6-arm `switch` for active-button resolution (lines 380–400)
The `switch ((mode ?? "ORB").ToUpperInvariant())` contains **5 explicit cases** (`RMA`, `RETEST`, `MOMO`,
`FFMA`, `TREND`) plus a `default` arm that selects `modeOrbButton`. Each case is an independent CFG branch
(+5 decision points). Because the arms each assign one of six distinct widget references, they cannot be
collapsed without a data-structure lookup table. This single construct contributes the majority of the CYC.

### 2 — `foreach` loop with per-iteration null guard (lines 360–377)
The reset pass iterates over a 6-element inline array of nullable button references, applying
`if (btn == null) continue` on every iteration. The loop itself is `+1`, and the null guard is `+1`,
contributing **+2 decision points**. The `continue` creates an early-exit path inside the loop body,
which is the only internal control-flow branch within the reset pass.

### 3 — Post-switch null guard on `activeButton` (lines 402–407)
After the switch resolves `activeButton`, a final `if (activeButton != null)` check gates the highlight
assignment. Although all named button fields are initialised during panel construction, the method must
tolerate the case where the `default` arm leaves `activeButton = modeOrbButton` which may be `null` (e.g.,
panel torn down mid-tick). This guard adds **+1 decision point** and couples the method's safety invariant
to the panel lifecycle — an implicit dependency not expressed in the signature.

---

## Recommended Extraction Count

**1 targeted extraction** is sufficient to bring residual CYC to ≤ 8:

| # | Proposed Extract | Signature | CYC Reduction |
|---|-----------------|-----------|---------------|
| 1 | `ResolveActiveModeButton(string mode)` — extract the entire `switch` block into a private helper that returns the `Button` reference; the `foreach` reset pass and final null-guarded highlight remain in the caller | `private Button ResolveActiveModeButton(string mode)` | −1 (removes one branch level from the outer method's CFG; switch arms move to the helper, leaving caller at CYC ≤ 4) |

> **Note:** The `foreach` + null guard pair (driver 2) could also be extracted as
> `ResetModeChipStyles()` for an additional −2, but that is optional — a single extraction already
> satisfies the CYC ≤ 8 target.

---

## Agent Tracking

```
Agent Name     : v12-phase0-hotspot
EPIC           : EPIC-W7-158
Wave           : 7
Phase          : 0  (Hotspot Analysis)
Status         : completed
Output         : docs/brain/EPIC-W7-158/00-hotspots.md
CYC Confirmed  : 9  (1 base + foreach +1, null-in-loop +1, switch 5 cases +5, post-switch null guard +1)
Bobcoins Used  : 18
Execution Time : ~40s
Timestamp      : 2025-07-14T00:00:00Z
```
