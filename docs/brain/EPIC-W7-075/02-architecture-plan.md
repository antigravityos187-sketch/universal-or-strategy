# Phase 2: Architecture Plan -- EPIC-W7-075

## Method Under Extraction

- **Method:** `OnSubmitClick`
- **Source File:** `src/V12_002.UI.Panel.Handlers.cs`
- **Class:** `V12_002` (partial)
- **Lines:** 261-303
- **Original CYC:** 34

---

- **jcodemunch get_context_bundle result:** Symbol resolved as `src/V12_002.UI.Panel.Handlers.cs::V12_002.OnSubmitClick#method`, signature `private void OnSubmitClick(object sender, RoutedEventArgs e)`, lines 261-303. Full source retrieved: 42-line body fusing UI reads, mode resolution, symbol resolution, direction classification, 4-way command dispatch, and PanelCommand/TriggerGlow dispatch. Imports include System, System.Windows, System.Windows.Controls. Initial lookup by bare name was ambiguous (2 definitions: src/ and src-vm-backup/); resolved using full qualified symbol ID for src/.

- **jcodemunch get_call_hierarchy result:** Callers = 0 (wired via event subscription `submitButton.Click += OnSubmitClick` in AttachMiscellaneousHandlers -- not a direct AST call). Callees at depth-1: `GetCurrentConfigMode` (src/V12_002.UI.IPC.Server.cs:37, ast_inferred), `PanelCommand` (src/V12_002.UI.Panel.Handlers.cs:935, ast_resolved), `TriggerGlow` (src/V12_002.UI.Panel.Lifecycle.cs:114, ast_inferred). Callees at depth-2: `Enqueue` (src/V12_002.cs:428, ast_inferred via PanelCommand), `_glowTimer` constant (via TriggerGlow). Confirms Actor/Enqueue pattern is active; no lock() in the call chain.

- **jcodemunch get_dependency_graph result:** File `src/V12_002.UI.Panel.Handlers.cs` shows 0 import edges and 0 importer edges in the import graph (node_count=1, edge_count=0). This is expected for a C# partial class -- the file contributes to the single `V12_002` partial class assembly; cross-file dependencies are resolved at compile time rather than via import statements. Blast radius is confirmed to be contained within the single partial class.

- **jcodemunch get_extraction_candidates result:** No extraction candidates returned (candidates=[]). This is expected: `get_extraction_candidates` requires min_callers=1 via the import graph, and the partial-class pattern means no file-level import edges exist. The CYC data from Phase 0 hotspot analysis (CYC=34) drives the extraction decision independently.

---

## Sequential Thinking Summary

Seven-thought sequential chain completed for EPIC-W7-075 architecture planning:

**Thought 1** identified the 6 fused concerns in the 42-line body: UI direction read, UI price read, mode resolution + normalization, instrument/symbol resolution, direction flag classification, 4-way command dispatch.

**Thought 2** mapped each concern to a named private helper chunk, projecting preliminary CYC estimates per chunk.

**Thought 3** assigned single-responsibility names: `ReadSubmitDirection`, `ReadSubmitPrice`, `ResolveSubmitMode`, `ResolveSubmitSymbol`, `ClassifyDirectionFlag`, `BuildSubmitCommand`. All stay private in the same partial class, satisfying V12.23 no-scope-creep.

**Thought 4** refined CYC projections per helper: ReadSubmitDirection=3, ReadSubmitPrice=2, ResolveSubmitMode=3, ResolveSubmitSymbol=3, ClassifyDirectionFlag=2, BuildSubmitCommand=7. All pass CYC<=8. Parent residual = CYC 1.

**Thought 5** confirmed parent OnSubmitClick post-extraction is pure sequential orchestration (8 sequential statements, zero predicates) -- CYC=1.

**Thought 6** verified lock-free/Actor compliance: PanelCommand(Enqueue) pattern preserved, all 6 helpers are pure reads or pure string transformations with no shared state. ASCII-only string literals confirmed throughout.

**Thought 7 (final verdict):** CYC<=8 achieved across all 7 symbols. Max projected CYC=7 (BuildSubmitCommand). Single-responsibility per helper: YES. Lock-free preserved: YES. Illegal-states partially: direction binary SHORT/LONG and mode normalized before use; command string remains stringly-typed (scope constraint, document for Phase 3). extraction_count=6, max_cyc_projected=7. PLAN COMPLETE AND COMPLIANT.

---

## Extraction Plan

| Helper Method Name | Responsibility | Signature | Projected CYC |
|---|---|---|---|
| `ReadSubmitDirection` | Read directionCombo UI control; return content string with "OR LONG" default | `private string ReadSubmitDirection()` | 3 |
| `ReadSubmitPrice` | Read priceInput.Text with null guard; return trimmed string or empty | `private string ReadSubmitPrice()` | 2 |
| `ResolveSubmitMode` | Resolve mode from _panelLastSyncedMode fallback to GetCurrentConfigMode(); normalize OR->ORB | `private string ResolveSubmitMode()` | 3 |
| `ResolveSubmitSymbol` | Extract symbol name from Instrument.MasterInstrument chain; return empty on null | `private string ResolveSubmitSymbol()` | 3 |
| `ClassifyDirectionFlag` | Convert human-readable direction string to binary SHORT/LONG flag | `private string ClassifyDirectionFlag(string direction)` | 2 |
| `BuildSubmitCommand` | Pure command-string factory: 4-way mode dispatch + price optional suffix; no I/O | `private string BuildSubmitCommand(string mode, string dir, string symbol, string price)` | 7 |

---

## Parent Method After Extraction

**Remaining logic in `OnSubmitClick`:**
```
string direction = ReadSubmitDirection();
string price     = ReadSubmitPrice();
string mode      = ResolveSubmitMode();
string symbol    = ResolveSubmitSymbol();
string dir       = ClassifyDirectionFlag(direction);
string cmd       = BuildSubmitCommand(mode, dir, symbol, price);
PanelCommand(cmd);
TriggerGlow(GreenFg);
```

- **Remaining logic description:** Pure sequential orchestration -- delegates all input reading, resolution, classification, and command building to helpers. No conditionals, no loops, no null checks remain in the parent.
- **Projected CYC: 1**

---

## max_cyc_projected: 7
## extraction_count: 6

---

## Jane Street Alignment

| Rule | Status | Notes |
|---|---|---|
| CYC<=8 achieved | YES | Max=7 (BuildSubmitCommand); parent=1 |
| Single-responsibility per helper | YES | Each helper does exactly one thing |
| Lock-free/Actor pattern preserved | YES | PanelCommand -> Enqueue path unchanged; no lock() introduced |
| Illegal states unrepresentable | PARTIAL | Direction normalized to binary SHORT/LONG flag; mode normalized exactly once in ResolveSubmitMode; command string remains stringly-typed pipe-delimited (V12.23 scope constraint -- typed command record deferred to separate epic) |
| ASCII-only string literals | YES | All literals (OR LONG, OR_SHORT, OR_LONG, TREND_MANUAL_LIMIT, etc.) are ASCII |
| xUnit [Fact] tests per helper | REQUIRED | 6 tests: ReadSubmitDirection, ReadSubmitPrice, ResolveSubmitMode, ResolveSubmitSymbol, ClassifyDirectionFlag, BuildSubmitCommand |
| One method per epic | YES | Only OnSubmitClick + extracted helpers in scope |
| No Unicode/curly quotes | YES | No Unicode in string literals |

---

## xUnit Test Requirements (Phase 5)

| Test Method | What to Assert |
|---|---|
| `ReadSubmitDirection_NullCombo_ReturnsDefault` | Returns "OR LONG" when directionCombo is null |
| `ReadSubmitDirection_ValidItem_ReturnsContent` | Returns ComboBoxItem.Content when set |
| `ReadSubmitPrice_NullInput_ReturnsEmpty` | Returns string.Empty when priceInput is null |
| `ResolveSubmitMode_EmptyLastSynced_CallsGetCurrent` | Falls back to GetCurrentConfigMode() |
| `ResolveSubmitMode_ORMode_RemapsToORB` | "OR" input -> "ORB" output |
| `ResolveSubmitSymbol_NullInstrument_ReturnsEmpty` | Returns empty string when Instrument is null |
| `ClassifyDirectionFlag_SHORT_ReturnsShort` | "OR SHORT" -> "SHORT" |
| `ClassifyDirectionFlag_LONG_ReturnsLong` | "OR LONG" -> "LONG" |
| `BuildSubmitCommand_TrendMode_FormatsCorrectly` | TREND_MANUAL_LIMIT pipe format |
| `BuildSubmitCommand_ORLong_NoPrice_OmitsPrice` | OR_LONG no price suffix when price empty |
| `BuildSubmitCommand_ORLong_WithPrice_AppendPrice` | OR_LONG appends price when non-zero |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic ID** | EPIC-W7-075 |
| **Wave** | 7 |
| **Phase** | 2 |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T02:15:00Z |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 7 |
| **Output** | docs/brain/EPIC-W7-075/02-architecture-plan.md |
| **extraction_count** | 6 |
| **max_cyc_projected** | 7 |
