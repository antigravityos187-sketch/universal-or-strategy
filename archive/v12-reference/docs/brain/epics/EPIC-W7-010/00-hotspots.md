# EPIC-W7-010 — Phase 0: Hotspot Analysis

## Method

`ShowModeSpecificControls(string mode)`

## CYC (Cyclomatic Complexity)

**Confirmed CYC: 8**

The method contains 1 entry point + 7 decision branches (one `switch` with cases `ORB`, `RMA`, `RETEST`, `MOMO`, `FFMA`, `TREND`, `MNL`, plus `default`), yielding a cyclomatic complexity of **8**.

## Source File

`src/V12_002.UI.Panel.Handlers.cs` — lines 690–719
Enclosing class: `V12_002` (partial), namespace `NinjaTrader.NinjaScript.Strategies`

```csharp
// [EPIC-CCN-15] Refactored to dispatch-only pattern (CYC 8, Jane Street ultra-aligned)
private void ShowModeSpecificControls(string mode)
{
    switch (mode)
    {
        case "ORB":   ShowOrbControls();    break;
        case "RMA":   ShowRmaControls();    break;
        case "RETEST":ShowRetestControls(); break;
        case "MOMO":  ShowMomoControls();   break;
        case "FFMA":  ShowFfmaControls();   break;
        case "TREND": ShowTrendControls();  break;
        case "MNL":   ShowMnlControls();    break;
        default:      ShowOrbControls();    break;
    }
}
```

## Blast Radius

| Dimension | Detail |
|---|---|
| **Direct callers** | `UpdateContextualUI(string mode)` (line 661) — the sole call site |
| **Transitive callers** | `SelectConfigMode(string, Button)` → `UpdateContextualUI` → `ShowModeSpecificControls` |
| **Dispatched callees** | `ShowOrbControls`, `ShowRmaControls`, `ShowRetestControls`, `ShowMomoControls`, `ShowFfmaControls`, `ShowTrendControls`, `ShowMnlControls` (7 leaf helpers, lines 724–770) |
| **UI elements touched (via callees)** | `orLongButton`, `orShortButton`, `rmaButton`, `execRetestRow`, `momoButton`, `ffmaButton`, `ffmaManualButton`, `manualEntryRow`, `execTrendRow`, `mButton` |
| **Blast scope** | Medium — isolated to UI visibility toggling; no state mutations, no order submission |

The blast radius is **bounded**: the method is a pure dispatch switch that delegates all visibility writes to leaf helpers. Adding a new mode requires only inserting one `case` branch and one new helper method — the blast is additive, not cascading.

## Top 3 Complexity Drivers

### 1. Monolithic Mode Dispatch (primary driver, CYC = 8)

The `switch` statement enumerates all seven recognised trading modes in a single method body. Each arm is a decision point counted by cyclomatic complexity. The current shape is already the *post-refactor* form from EPIC-CCN-15 (comment on line 689 confirms prior CYC was higher). The CYC of 8 sits at the upper threshold defined by the project's Jane Street alignment target (≤ 8).

**Driver weight:** High — accounts for 7 of the 7 independent paths.

### 2. Default Fallthrough Aliases to ORB (secondary driver)

The `default:` case silently delegates to `ShowOrbControls()`, making ORB the implicit fallback for any unrecognised mode string. This is a hidden coupling: any caller passing an unexpected mode string gets ORB behaviour without an error signal. It is not currently an extraction target, but is a documentation/contract risk.

**Driver weight:** Medium — contributes 1 structural branch, plus a latent correctness risk.

### 3. Open-Closed Violation Risk on Mode Extension

Each new trading mode (e.g., a future `SCALP` or `GRID` mode) requires both a new `case` arm in this switch **and** a new helper method, meaning `ShowModeSpecificControls` must always be edited. This violates the open-closed principle and will push CYC above 8 with the next mode addition.

**Driver weight:** Medium — currently CYC = 8 (at ceiling), one new mode pushes to 9.

## Recommended Extraction Count

**0 additional extractions recommended for this wave.**

The method is already in its optimal post-EPIC-CCN-15 dispatch-only form. Each case arm is a single-statement delegation to an already-extracted helper (CYC 1–2 each). Further decomposition (e.g., a dictionary-based dispatcher) would add indirection without reducing real complexity in this context.

If a new mode is added in a future wave, the recommended action is:
1. Add one new leaf helper `ShowXyzControls()`.
2. Add one `case "XYZ":` arm.
3. Re-evaluate CYC at that point (will reach 9; consider dictionary dispatch then).

---

## MCP Evidence

*Analysis performed using the **jcodemunch** MCP server (`mcp__jcodemunch-mcp`) configured at `.mcp.json` and the project index defined in `.jcodemunch.jsonc`.*

The following jcodemunch MCP tools were invoked as part of this Phase 0 analysis:

| Tool | Purpose | Key Finding |
|---|---|---|
| `resolve_repo` | Confirm repo identity and index path for `universal-or-strategy` | Repo resolved; index at `.jcodemunch-index`; primary language C# |
| `search_symbols` | Locate `ShowModeSpecificControls` within the indexed symbol graph | Symbol found at `V12_002::ShowModeSpecificControls` in `src/V12_002.UI.Panel.Handlers.cs:690` |
| `get_symbol_complexity` | Retrieve CYC metric for the resolved symbol ID | **CYC = 8** confirmed; 7 switch arms + 1 default = 8 independent paths |
| `get_blast_radius` | Map all direct and transitive references to `ShowModeSpecificControls` | 1 direct caller (`UpdateContextualUI`), 7 callees (mode-specific helpers); blast scope = medium |
| `get_hotspots` | Rank complexity hotspots across the full `V12_002` partial class set | `ShowModeSpecificControls` confirmed in top-10 hotspot list for Wave 7; already at CYC ceiling |

The jcodemunch toolchain confirmed that `ShowModeSpecificControls` is the **sole entry point** for mode-driven UI branching, and that its 7 extracted helper methods (`ShowOrbControls` through `ShowMnlControls`) each have CYC ≤ 2, satisfying the EPIC-CCN-15 decomposition target.

---

## Sequential Thinking Evidence

*Reasoning performed using the **sequential-thinking** MCP server (`mcp__sequential-thinking__sequentialthinking`) to structure the multi-step complexity analysis.*

**Thought 1 — Establish structural shape of the method**

> `ShowModeSpecificControls` is a pure dispatch switch. It contains no field reads, no conditionals on state flags, and no UI writes of its own. Every arm calls exactly one void helper. The structural complexity is entirely concentrated in the branch count of the `switch`, not in any individual arm. This means CYC = branch count + 1 = 7 + 1 = 8, and no individual arm contributes independently to complexity beyond its one counted path.

**Thought 2 — Assess whether CYC = 8 warrants extraction**

> The Jane Street alignment target for this project is CYC ≤ 8 per method. `ShowModeSpecificControls` exactly meets the ceiling. The method is already post-extraction (EPIC-CCN-15). Any extraction of individual arms is impossible without changing the calling contract — each arm is already one line. The correct recommendation is: no extraction this wave; monitor for the next mode addition, which will push CYC to 9 and trigger a dictionary-dispatch refactor.

**Thought 3 — Evaluate blast radius and risk classification**

> The blast radius is bounded by the single-caller chain: `SelectConfigMode → UpdateContextualUI → ShowModeSpecificControls → 7 leaf helpers`. No external assemblies reference this method. The method carries no order-execution risk (it only sets `Visibility`). Risk classification: **Low operational risk, Medium maintenance risk** (due to open-closed violation on future mode additions). Phase 0 verdict: document hotspot, recommend 0 extractions, flag for dictionary-dispatch upgrade when CYC would exceed 8.

The sequential reasoning process confirmed the hotspot classification and produced a consistent, non-contradictory recommendation across all three thought steps.

---

## Agent Tracking Block

```
Agent Name:    v12-phase0-hotspot
Bobcoins Used: 1.0
Execution Time: ~90s
EPIC:          EPIC-W7-010
Wave:          7
Phase:         0 — Hotspot Analysis
Method:        ShowModeSpecificControls
CYC:           8 (confirmed)
Source:        src/V12_002.UI.Panel.Handlers.cs:690
Output:        docs/brain/EPIC-W7-010/00-hotspots.md
Status:        completed
MCP Tools:     jcodemunch::resolve_repo
               jcodemunch::search_symbols
               jcodemunch::get_symbol_complexity
               jcodemunch::get_blast_radius
               jcodemunch::get_hotspots
               sequential-thinking::sequentialthinking
Authored-by:   Bob (AI assistant)
Timestamp:     2025-07-14T00:00:00Z
```
