# EPIC-W7-079 — Phase 0: Hotspot Analysis

## Symbol

| Field          | Value                                           |
|----------------|-------------------------------------------------|
| Method         | `CreateSection0_Identity`                       |
| Source File    | `src/V12_002.UI.Panel.Construction.cs`          |
| Lines          | 511 – 705                                       |
| Class          | `V12_002` (partial)                             |
| Namespace      | `NinjaTrader.NinjaScript.Strategies`            |
| Wave / Phase   | Wave 7 / Phase 0 — Hotspot Analysis             |
| CYC Confirmed  | 0 (no branches at method entry; lambdas account for internal branching) |

---

## CYC Complexity Breakdown

The method itself has **CYC = 0** at its structural entry point — there are no `if/else`, `switch`, or loop constructs directly in the top-level flow. All branching is delegated to:

1. **Lambda closures** (lines 597–618, 650–668): `selectAllCheck.Checked`, `selectAllCheck.Unchecked`, `cb.Checked`, `cb.Unchecked` — each contains 1–2 branch paths (null-guard + conditional add/remove on `selectedFleetAccounts`).
2. **Inline ternary** (line 539, 690): `Account != null ? ... : "--"` and `lastKnownPrice > 0 ? ... : "0.00"`.
3. **`foreach` over `fleetAccounts`** (line 633): Dynamic runtime branching inside the loop, including `TryGetValue` + `Contains` guard.

The method spans **194 lines** — this is a large UI construction method; its zero top-level CYC masks the real complexity sitting inside the event lambdas and the fleet account loop.

---

## Hotspots Identified

### H-1 · Fleet Popup Inline Construction (lines 569–673) — **HIGH**
A full popup widget tree (`Popup` → `Border` → `StackPanel` → `CheckBox` × N + `ScrollViewer`) is constructed inline, including stateful lambda closures that mutate `selectedFleetAccounts` and fire `PanelCommand(...)`. This block:
- Creates cross-cutting state dependency between `fleetCheckboxPanel`, `selectedFleetAccounts`, and `activeFleetAccounts` (a `ConcurrentDictionary` owned by the parent class in `src/V12_002.cs:195`).
- Lambda capture of `cb` and `accountName` inside the foreach loop — a known C# closure-over-loop-variable pattern, though here `accountName = cb.Tag as string` defers access safely.
- Calls `GetFleetAccountsSnapshot()` at construction time (line 632), meaning stale data if the panel is rebuilt without a full teardown.

### H-2 · `selectAllCheck` Guards on Null `fleetCheckboxPanel` (lines 597–618) — **MEDIUM**
The Select All / Deselect All lambdas guard on `if (fleetCheckboxPanel == null) return;`. However, `fleetCheckboxPanel` is assigned **after** `selectAllCheck` is added to `popupStack` (line 629 vs 619). If the popup were ever opened before the second half of the method completes (impossible in synchronous construction, but risky if construction is ever parallelised or split), the guard would silently swallow the user action.

### H-3 · `activeFleetAccounts` Cross-File Dependency (line 636) — **MEDIUM**
`activeFleetAccounts` is a `ConcurrentDictionary<string, bool>` declared in `src/V12_002.cs:195` and written from three other files (`V12_002.UI.IPC.Commands.Config.cs`, `V12_002.SIMA.Lifecycle.cs`, `V12_002.SIMA.Dispatch.cs`). Reading it here in a UI construction path on the dispatcher thread while IPC threads may be writing introduces a thread-safety risk for the **initial checked state** of each fleet checkbox. The `ConcurrentDictionary.TryGetValue` call is itself safe, but the `bool isActive` snapshot is not atomically consistent with the subsequent `selectedFleetAccounts.Add`.

### H-4 · Method Length / SRP Violation (lines 511–705, 194 lines) — **LOW**
The method constructs three distinct logical sub-panels:
1. Hub status LED + leader account row (lines 522–546).
2. Fleet popup with dynamic checkbox list (lines 548–676).
3. Manual entry row: direction combo + price input + submit button (lines 678–701).

Each sub-panel could be a private factory method, reducing this to a three-line compositor.

---

## Blast Radius

| Consumer                              | File                                     | Nature                            |
|---------------------------------------|------------------------------------------|-----------------------------------|
| Panel build sequence (line 190)       | `V12_002.UI.Panel.Construction.cs:190`   | Direct call — panel init          |
| `UpdateFleetButtonText()` (line 1511) | `V12_002.UI.Panel.Construction.cs:1511`  | Reads `fleetSelectButton`, `selectedFleetAccounts`, `fleetCheckboxPanel` |
| `activeFleetAccounts` (line 636)      | `src/V12_002.cs:195`                     | ConcurrentDictionary shared state |
| `GetFleetAccountsSnapshot()`          | Elsewhere in `V12_002` partial class     | Fleet account source-of-truth     |
| `PanelCommand()` lambdas              | `src/V12_002.UI.IPC.Commands.Config.cs`  | Downstream IPC command dispatch   |
| Teardown / nullification (line 400–409) | `V12_002.UI.Panel.Construction.cs`     | Cleanup path must match all fields assigned here |
| Field declarations (lines 37–46)      | `V12_002.UI.Panel.Construction.cs`      | 9 instance fields written by this method |

---

## Recommended Actions for Later Phases

1. **Phase 1 (Decompose)**: Extract `BuildFleetPopup()` and `BuildManualEntryRow()` as private factory methods — reduces method to a compositor, improves testability.
2. **Phase 2 (Thread Safety)**: Snapshot `activeFleetAccounts` entries atomically before the foreach loop using a `ToArray()` or local `Dictionary` copy to avoid race conditions on initial checkbox state.
3. **Phase 3 (Ordering Guard)**: Move `fleetCheckboxPanel = new StackPanel()` assignment (line 629) before `selectAllCheck` lambda registration, or restructure to remove the null-guard entirely.
4. **Phase 4 (Stale Data)**: Document or enforce that `CreateSection0_Identity` is only callable after `GetFleetAccountsSnapshot()` returns a non-stale view — or pass snapshot as a parameter.

---

## Summary

`CreateSection0_Identity` reports **CYC = 0** at the method level because all control flow lives inside lambdas and a single foreach. The real complexity is structural: a 194-line monolithic UI factory that owns 9 instance fields, reads cross-file shared state (`activeFleetAccounts`), fires IPC commands from inline lambdas, and has a subtle ordering dependency between `selectAllCheck` registration and `fleetCheckboxPanel` assignment. The blast radius spans 5 source files and 3 distinct subsystems (UI, IPC, SIMA).
