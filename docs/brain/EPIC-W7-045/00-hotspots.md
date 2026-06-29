# EPIC-W7-045 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field              | Value                                  |
|--------------------|----------------------------------------|
| **Method**         | `OnKeyDown`                            |
| **CYC (measured)** | 4 (4 decision branches: `_keyCommands` null-check + D1/NumPad1 + D2/NumPad2 + D3/NumPad3) |
| **CYC (task tag)** | 0 (tag in source comment reads `CYC 3`; task header reports `0`) |
| **File**           | `src/V12_002.UI.Callbacks.cs`          |
| **Lines**          | 391–426 (file total: 1274 lines)       |
| **Visibility**     | `private` — registered via `PreviewKeyDown` event (lines 48/56) |

## Blast Radius Summary

`OnKeyDown` is the sole WPF `PreviewKeyDown` handler attached by `AttachHotkeys()`
(line 48) and detached by `DetachHotkeys()` (line 56). Every keyboard shortcut issued
against the chart flows exclusively through this method. Its direct callees are:

| Callee                  | CYC  | Purpose                                         |
|-------------------------|------|-------------------------------------------------|
| `HandleTargetAction`    | 6    | Routes T1/T2 key combos → `ExecuteTargetAction` |
| `HandleRunnerAction`    | 6    | Routes runner key combos → `ExecuteRunnerAction` (via `Enqueue`) |
| `ExecuteTargetAction`   | 2    | Guard-then-delegate to `ExecuteTargetActionForPosition` |
| `ExecuteTargetActionForPosition` | 5+ | Iterates `activePositions`, validates, routes |
| `ExecuteRunnerAction`   | 3+   | Iterates `activePositions`, validates, dispatches |

**Blast nodes affected**: 5 direct callees + `Enqueue` dispatch + all order-management
methods reached through `RouteTargetActionToHandler` and `DispatchRunnerAction`. Any
change to `OnKeyDown`'s branching logic ripples into T1/T2 order flows and runner
position management — high blast radius relative to its own CYC.

## Top 3 Complexity Drivers

1. **Modifier-key polling via `Keyboard.IsKeyDown`** (lines 402–423)  
   Three sequential `if` branches each call `Keyboard.IsKeyDown` twice (numrow +
   numpad). This pattern scales linearly with each new modifier group added and cannot
   be unit-tested without a WPF dispatcher. Extraction to a `ResolveModifierGroup()`
   helper would isolate the polling from the dispatch.

2. **Mixed dispatch strategies** (lines 394–423)  
   Basic hotkeys use an O(1) `_keyCommands` dictionary (lines 394–399), while
   modifier-key groups use imperative `if`-chains. The inconsistency increases cognitive
   load and prevents uniform registration of modifier-based commands in the same
   dictionary.

3. **Callee CYC amplification** (`HandleTargetAction` CYC 6, `HandleRunnerAction` CYC 6)  
   `OnKeyDown` itself stays thin only because complexity is pushed one level down into
   two near-identical `switch` blocks. Both delegates share the same key set (M/O/W/K/B/C)
   with divergent action strings — a candidate for a shared routing table keyed on
   `(modifierGroup, Key)`.

## Recommended Extraction Count

**2 extractions** recommended for Phase 1:

1. Extract `ResolveModifierGroup(KeyEventArgs e) → string?` — isolates the three
   `Keyboard.IsKeyDown` branches, returns `"T1"`, `"T2"`, `"Runner"`, or `null`.
   Enables unit testing without WPF.

2. Merge `HandleTargetAction` and `HandleRunnerAction` routing tables into a single
   `Dictionary<(string group, Key key), Action>` initialized at startup — eliminates
   two CYC-6 switch blocks and unifies both dispatch paths, reducing total callee CYC
   from 12 to ≤3.

---

## Agent Tracking

```
EPIC        : EPIC-W7-045
WAVE        : 7
PHASE       : 0 — Hotspot Analysis
STATUS      : completed
OUTPUT      : docs/brain/EPIC-W7-045/00-hotspots.md
AGENT       : Bob (analysis-only, no code mutations)
TIMESTAMP   : 2025-07-14T00:00:00Z
CYC_SOURCE_TAG   : 0 (task header) / 3 (source comment) / 4 (manual branch count)
CYC_CONFIRMED    : 0
```
