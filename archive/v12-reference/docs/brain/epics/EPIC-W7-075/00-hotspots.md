# EPIC-W7-075 — Phase 0: Hotspot Analysis
## Method: `OnSubmitClick` | CYC: 34
**Source:** `src/V12_002.UI.Panel.Handlers.cs` lines 261–303  
**Wave:** 7 | **Phase:** 0

---

## 1. Symbol Location

| Field | Value |
|---|---|
| Class | `V12_002` (partial) |
| Method | `OnSubmitClick(object sender, RoutedEventArgs e)` |
| File | `src/V12_002.UI.Panel.Handlers.cs` |
| Lines | 261–303 |
| Wired at | `AttachMiscellaneousHandlers` → `submitButton.Click += OnSubmitClick` (line 93) |

---

## 2. Cyclomatic Complexity Breakdown (CYC = 34)

The method fuses **four distinct concerns** in a single 42-line body, generating a dense predicate graph:

| # | Predicate / Branch | Lines |
|---|---|---|
| 1 | `directionCombo != null && directionCombo.SelectedItem is ComboBoxItem` | 264 |
| 2 | `directionItem.Content as string ?? "OR LONG"` (null-coalesce) | 265 |
| 3 | `priceInput != null` (null guard) | 267 |
| 4 | `string.IsNullOrEmpty(mode)` — fallback to `GetCurrentConfigMode()` | 269–270 |
| 5 | `string.Equals(mode, "OR", …)` → remap to `"ORB"` | 271–272 |
| 6 | `Instrument != null && Instrument.MasterInstrument != null` | 275–276 |
| 7 | `direction.IndexOf("SHORT", …) >= 0` — direction classifier | 278 |
| 8 | `string.Equals(mode, "TREND", …)` — branch 1 of 4-way dispatch | 281 |
| 9 | `string.Equals(mode, "RETEST", …)` — branch 2 | 285 |
| 10 | `string.Equals(mode, "FFMA", …)` — branch 3 | 289 |
| 11–12 | implicit `else` + `dir == "LONG"` ternary — branch 4 + inner split | 295–296 |
| 13 | `!string.IsNullOrEmpty(price) && price != "0.00"` — two predicates | 297 |

Each `string.Equals` with `OrdinalIgnoreCase` is an independent edge in the path graph. The `PanelCommand` dispatch continuation (4 handler try-chains inside the enqueued closure at lines 935–956) further amplifies the reachable paths traced back to this method's output, explaining the full CYC=34 attribution.

---

## 3. Blast Radius

```
OnSubmitClick
  └─► PanelCommand(cmd)          [line 301]   ← enqueues closure
        ├─► TryHandleModeCommand              [src/V12_002.UI.IPC.Commands.Mode.cs:37]
        ├─► TryHandleRiskCommand              [src/V12_002.UI.IPC.Commands.Mode.cs:221]
        ├─► TryHandleFleetCommand             [src/V12_002.UI.IPC.Commands.Fleet.cs:37]
        └─► TryHandleConfigCommand            [src/V12_002.UI.IPC.Commands.Misc.cs:37]
  └─► TriggerGlow(GreenFg)       [line 302]   ← UI-only, no risk
```

**Commands emitted by this method (trading-critical):**
- `TREND_MANUAL_LIMIT|{symbol}|{dir}|{price}`
- `RETEST_MANUAL_LIMIT|{symbol}|{dir}|{price}`
- `FFMA_MANUAL_LIMIT|{symbol}|{dir}|{price}`
- `OR_LONG|{symbol}[|{price}]`
- `OR_SHORT|{symbol}[|{price}]`

**Severity: HIGH** — every code path terminates in live order submission. A mis-routed `cmd` string silently submits the wrong order type with no validation layer between this handler and execution.

---

## 4. Structural Issues

1. **Duplicated mode-resolution logic** — The `_panelLastSyncedMode → GetCurrentConfigMode() → OR→ORB` normalisation sequence at lines 268–272 is identical to [`ResolveEffectiveSyncMode`](src/V12_002.UI.Panel.Handlers.cs:406) used by `OnSyncAllClick`. This is an existing DRY violation.

2. **Mixed concerns in one method** — UI input reading (lines 263–278), mode resolution (268–272), instrument resolution (274–277), command building (281–299), and dispatch (301–302) are all inlined. Each concern should be a discrete, testable unit.

3. **Command string is built imperatively** — No intermediate typed representation exists; the raw pipe-delimited string `cmd` is assembled inline, making the shape of valid commands impossible to enforce statically.

4. **No input validation for `price`** — The price string from `priceInput.Text` is passed directly into the command without numeric parsing (contrast with [`CommitLiveTargetPrice`](src/V12_002.UI.Panel.Handlers.cs:919) which does `double.TryParse` before issuing its command).

---

## 5. Recommended Decomposition (Phase 1 preview)

| Extracted Unit | Responsibility | Est. CYC |
|---|---|---|
| `ResolveEffectiveSyncMode()` | Already exists — reuse it | 3 |
| `BuildSubmitCommand(mode, dir, symbol, price)` | Pure command-string factory; 4-way switch, no I/O | ≤8 |
| `ReadSubmitInputs()` → `(direction, price)` | Reads UI controls, applies defaults | ≤4 |
| `OnSubmitClick` (residual) | Orchestrates the three above + dispatch | ≤3 |

Target post-refactor CYC for `OnSubmitClick`: **≤3**.

---

## 6. References

- `AttachMiscellaneousHandlers` → [`src/V12_002.UI.Panel.Handlers.cs:80`](src/V12_002.UI.Panel.Handlers.cs:80)
- `PanelCommand` dispatch → [`src/V12_002.UI.Panel.Handlers.cs:935`](src/V12_002.UI.Panel.Handlers.cs:935)
- `GetCurrentConfigMode` → [`src/V12_002.UI.IPC.Server.cs:37`](src/V12_002.UI.IPC.Server.cs:37)
- `ResolveEffectiveSyncMode` → [`src/V12_002.UI.Panel.Handlers.cs:406`](src/V12_002.UI.Panel.Handlers.cs:406)
- `CommitLiveTargetPrice` (price validation pattern) → [`src/V12_002.UI.Panel.Handlers.cs:919`](src/V12_002.UI.Panel.Handlers.cs:919)
- Prior art: EPIC-CCN-15 (`ShowModeSpecificControls` CYC dispatch-only refactor), EPIC-CCN-16 (`UpdateTargetVisibility` CYC 19→1)
