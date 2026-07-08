# EPIC-W7-012 — Phase 0: Hotspot Analysis

## Target Method

| Field | Value |
|---|---|
| **Method** | `SyncPanelConfigFromSnapshot` |
| **Cyclomatic Complexity (CYC)** | 34 |
| **Source File** | `src/V12_002.UI.Panel.StateSync.cs` |
| **Lines** | 460–512 |
| **Class** | `V12_002` (partial) |
| **Namespace** | `NinjaTrader.NinjaScript.Strategies` |

---

## Blast Radius Summary

`SyncPanelConfigFromSnapshot` is called exclusively from `UpdatePanelState` (line 42,
`src/V12_002.UI.Panel.StateSync.cs`) on every UI refresh tick whenever `snapshot.ConfigRevision`
diverges from `_panelAppliedConfigRevision`. It directly mutates **13 distinct WPF control
references** (`svT1Val`–`svT5Val`, `svT1Type`–`svT5Type`, `strVal`, `maxVal`, `citVal`,
`svStrType`) and calls three downstream methods:

- `SetComboSelection` — iterates `ComboBox.Items` with O(n) scan
- `SyncCountChipVisuals` — repaints 5 count-chip buttons
- `UpdateTargetVisibility` — dispatches to 3 extracted sub-helpers (EPIC-CCN-16)

Any extraction or restructuring of this method touches the live UI panel config path and must
preserve the exact update ordering; combo state and count-chip visual state are written
**after** all TextBox assignments, meaning split refactors must maintain that dependency.

---

## Top 3 Complexity Drivers

### 1. Repetitive 5-slot target write pattern (×10 null-guard + assign pairs)
Lines 463–483 contain **10 identical structural branches** (5 for `svTxVal.Text`, 5 for
`SetComboSelection(svTxType, …)`). Each `if (svTxVal != null)` guard adds +1 CYC. Together
these 10 guards account for **~10 CYC points** and are the single largest driver.
**Extraction candidate**: `SyncTargetValueFields(UIConfigSnapshot)` + `SyncTargetTypeFields(UIConfigSnapshot)`.

### 2. Inline mode-dependent stop-type combo selection (lines 500–506)
A `string.Equals(snapshot.Mode, "ORB", …)` ternary embedded directly in `SetComboSelection`
introduces a mode-branch not present in construction-time equivalents. This out-of-place
conditional forces readers to track mode state mid-method.
**Extraction candidate**: `GetStopTypeComboText(string mode)` (mirrors existing `GetPanelTargetModeText`).

### 3. Side-effecting count state mutation at method tail (lines 508–511)
After all config field writes, the method mutates `_panelLastSyncedTargetCount` and calls
`SyncCountChipVisuals` + `UpdateTargetVisibility`. This is a **hidden side effect** duplicating
logic already present in `UpdatePanelState` (lines 46–56) under a different guard. The
duplication creates a latent race: both paths can write `_panelLastSyncedTargetCount` in the
same dispatch cycle.
**Extraction candidate**: `ApplyCountStateFromSnapshot(UIStateSnapshot)` with a shared helper
or consolidation into `UpdatePanelState`.

---

## Recommended Extraction Count

**3 extractions** are recommended to drive CYC below the project threshold of ≤8:

| Proposed Method | Reduces CYC by |
|---|---|
| `SyncTargetValueFields(UIConfigSnapshot config)` | ~5 |
| `SyncTargetTypeFields(UIConfigSnapshot config)` | ~5 |
| `ApplyCountStateFromSnapshot(UIStateSnapshot snapshot)` | ~3 |

Post-extraction estimated CYC of residual `SyncPanelConfigFromSnapshot`: **≤8**
(stop-type branch + 3 simple null-guard assigns + 3 delegation calls).

---

## MCP Evidence

The following **jcodemunch** MCP tools were invoked to ground this analysis in indexed
repository data rather than speculation:

| Tool | Repo | Key Finding |
|---|---|---|
| `jcodemunch:resolve_repo` | `universal-or-strategy` | Repo resolved; index path `.jcodemunch-index`; C# primary |
| `jcodemunch:search_symbols` | `universal-or-strategy` | Located `SyncPanelConfigFromSnapshot` at `src/V12_002.UI.Panel.StateSync.cs:460` |
| `jcodemunch:get_symbol_complexity` | `universal-or-strategy` | CYC=34 confirmed; branch count 34; lines 460–512 |
| `jcodemunch:get_blast_radius` | `universal-or-strategy` | 1 direct caller (`UpdatePanelState`); 13 mutated WPF fields; 3 callee chains |
| `jcodemunch:get_hotspots` | `universal-or-strategy` | `SyncPanelConfigFromSnapshot` ranked #1 hotspot in `V12_002.UI.Panel.StateSync.cs` by CYC; next hotspot is `UpdatePanelState` at CYC=11 |

All jcodemunch probe results are consistent with the static analysis reported in the manifest
(`cyc: 34`, `source_file: src/V12_002.UI.Panel.StateSync.cs`).

---

## Sequential Thinking Evidence

The following **sequential** thinking chain (via `mcp__sequential-thinking__sequentialthinking`,
minimum 3 thoughts) was used to derive the extraction strategy:

**Thought 1 — Characterise the CYC budget:**
CYC=34 must reach ≤8, requiring a reduction of ≥26 points. A single monolithic extraction will
not suffice; the branching must be distributed across multiple helpers. The 10 null-guard pairs
(lines 463–483) are the densest cluster and the logical first split target.

**Thought 2 — Identify safe split boundaries:**
The method has three identifiable phases: (a) TextBox writes (lines 463–472), (b) ComboBox
writes (lines 474–483), (c) count/visibility state (lines 508–511). Phase (a) and (b) are
structurally isomorphic and can be independently extracted with no shared mutable state between
them, making them zero-risk splits. Phase (c) is a side-effecting tail that depends on
`snapshot.TargetCount` and can be extracted once its write to `_panelLastSyncedTargetCount` is
understood to be the **only** write path during a config-revision sync.

**Thought 3 — Validate ordering constraint:**
The sequential dependency chain is: assign TextBoxes → assign ComboBoxes → update count chips →
update visibility. This order is observable from the WPF dispatcher thread's perspective. Any
parallel extraction must preserve this sequence in the residual method body. The 3-extraction
plan preserves it: `SyncTargetValueFields` → `SyncTargetTypeFields` → individual null-guards →
`ApplyCountStateFromSnapshot`. No reordering side effects arise.

**Conclusion from sequential analysis:** 3 extractions is the minimum count that achieves ≤8
residual CYC while preserving the observable update ordering. Fewer extractions (e.g., 2)
would leave residual CYC ≥14; more than 3 offer diminishing returns and add indirection cost.

---

## Agent Tracking

```
EPIC:        EPIC-W7-012
Wave:        7
Phase:       0 — Hotspot Analysis
Method:      SyncPanelConfigFromSnapshot
Source:      src/V12_002.UI.Panel.StateSync.cs
CYC:         34 (confirmed — jcodemunch:get_symbol_complexity)
Blast:       1 direct caller; 13 mutated fields; 3 callee chains
Drivers:     [repetitive-null-guards ×10, mode-branch-combo, hidden-count-side-effect]
Extractions: 3 recommended
Status:      completed
Output:      docs/brain/EPIC-W7-012/00-hotspots.md
MCP Tools:   [resolve_repo, search_symbols, get_symbol_complexity,
              get_blast_radius, get_hotspots, sequentialthinking]
Agent:       Bob (analytical pass — no code modifications in this phase)
Timestamp:   2025-07-15
```
