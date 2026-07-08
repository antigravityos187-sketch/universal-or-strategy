# Phase 2: Architecture Plan — EPIC-W7-011

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-011/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `DestroyPanel`
- **Source File:** `src/V12_002.UI.Panel.Construction.cs`
- **Signature:** `private void DestroyPanel()`
- **Lines:** 320–509 (189 lines)
- **Original CYC:** 8 (fallback applied; raw tool-reported value was 0; structural branch count resolves to >= 8 per hotspot analysis — confirmed by manual McCabe count in 00-hotspots.md)

### jcodemunch get_context_bundle result

Full source body retrieved via `get_context_bundle` using disambiguated symbol ID `src/V12_002.UI.Panel.Construction.cs::V12_002.DestroyPanel#method`. Key findings:
- Method confirms 3 structural regions: (1) null guard + `DetachPanelHandlers()` call; (2) outer `try/catch` wrapping a `switch (_placementMode)` over 4 arms (Fallback, Injected, Hijack, default), with an inner `try/catch` in the Fallback arm; (3) bulk sequential `= null` assignments across 45+ WPF widget fields plus scalar/enum resets and `_placementRetryTimer` lifecycle.
- Imports: `System`, `System.Collections.Generic`, `System.Windows`, `System.Windows.Controls`, `System.Windows.Media` — all standard WPF namespaces, no external packages.
- Initial `get_context_bundle` call returned "Symbol not found" due to ambiguity with `src-vm-backup/` mirror; resolved by using the full qualified symbol ID.

### jcodemunch get_call_hierarchy result

- **Callers (depth 2):** 0 direct AST callers found in the index. This is consistent with the Phase 0 finding: the single call site is in `HandleTerminated()` at `src/V12_002.Lifecycle.cs:209` dispatched via `ChartControl.Dispatcher.InvokeAsync` — the lambda boundary prevents AST-level caller resolution.
- **Callees (depth 1):**
  | Callee | File | Resolution |
  |---|---|---|
  | `DetachPanelHandlers` | `src/V12_002.UI.Panel.Handlers.cs:229` | ast_inferred |
  | `_placementRetryTimer` | `src/V12_002.UI.Panel.Construction.cs:157` | ast_resolved |
- No deep callee chain — both callees are leaf-level. `DetachPanelHandlers` is already an extracted helper (pre-existing, not in scope).

### jcodemunch get_dependency_graph result

- **File:** `src/V12_002.UI.Panel.Construction.cs`
- **Direction:** both (imports + importers)
- **Edges:** 0 — the file has no indexed import edges and no indexed importers at the file level. This is expected for a C# partial-class file: the `.cs` partial-class pattern means dependencies are resolved at compile time across `using` directives rather than file-level imports captured by the graph. The file is self-contained at the jcodemunch file-graph level.
- **Implication:** All new helper methods stay in the same partial class — no new file dependencies are introduced by this extraction.

### jcodemunch get_extraction_candidates result

- Returned 0 candidates with `min_complexity=3, min_callers=1`.
- Expected: the tool requires `min_callers >= 1` for external file calls; `DestroyPanel` is called from `Lifecycle.cs` via a Dispatcher lambda that AST resolution does not traverse. The zero result is a tool-limitation artifact, not evidence that extraction is unwarranted. Phase 0 hotspot analysis, Phase 1 scope analysis, and the confirmed CYC=8 from manual branch count all independently validate the extraction need.

---

## Sequential Thinking Summary

**Thought 1** — Scope and entry point established: `DestroyPanel` has a single Dispatcher-invoked caller (`HandleTerminated`), three separable concerns (handler detach, placement teardown, field nullification), and CYC=8 from confirmed structural branch count.

**Thought 2** — Complexity driver mapping: The `switch (_placementMode)` plus nested null checks is the primary CYC driver (~8-9 branches). The 45-field bulk nullification is a length driver (zero CYC). Each concern maps directly to an extraction target. The initial plan of 2 helpers (`TeardownPlacedPanel` + `ClearPanelWidgetRefs`) was evaluated; `TeardownPlacedPanel` alone would carry CYC 13+ if it absorbs all per-arm logic inline.

**Thought 3** — Per-arm decomposition required: To keep `TeardownPlacedPanel` at CYC<=8, the three placement arms are each extracted to dedicated private helpers (`TeardownFallbackPlacement`, `TeardownInjectedPlacement`, `TeardownHijackPlacement`). `TeardownPlacedPanel` becomes a switch-dispatch orchestrator (CYC=5). Each arm helper is CYC<=5.

**Thought 4** — Final CYC projections confirmed: Parent=3, TeardownPlacedPanel=5, TeardownFallbackPlacement=2, TeardownInjectedPlacement=5, TeardownHijackPlacement=2, ClearPanelWidgetRefs=1. max_cyc_projected=5. All <=8.

**Thought 5** — Jane Street alignment verdict: APPROVED. All CYC<=8 achieved. Single-responsibility per helper confirmed. Lock-free (WPF Dispatcher thread, no lock blocks). ASCII-only identifiers. xUnit [Fact] tests planned per helper. V12.23 scope boundary holds (all helpers private, same partial class).

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `TeardownPlacedPanel` | `private void TeardownPlacedPanel()` | Restores `_chartTraderElement` visibility; dispatches to per-arm helpers via `switch (_placementMode)`; owns outer `try/catch` | 5 |
| `TeardownFallbackPlacement` | `private void TeardownFallbackPlacement()` | Removes `rootContainer` from `UserControlCollection` inside inner `try/catch`; logs non-fatal removal failure | 2 |
| `TeardownInjectedPlacement` | `private void TeardownInjectedPlacement()` | Removes `rootContainer` from `_placementGrid.Children`; removes last injected `ColumnDefinition` if width matches 210px heuristic | 5 |
| `TeardownHijackPlacement` | `private void TeardownHijackPlacement()` | Removes `rootContainer` from `_placementGrid.Children` when grid is non-null and contains root | 2 |
| `ClearPanelWidgetRefs` | `private void ClearPanelWidgetRefs()` | Nullifies all ~45 WPF widget field references across identity, execution, target, compliance, market-data, and mode/SV sections; resets `_panelLastSyncedMode`, `_panelLastSyncedTargetCount`, `_panelAppliedConfigRevision` | 1 |

---

## Parent Method After Extraction

**Remaining logic in `DestroyPanel` after extraction:**

```
private void DestroyPanel()
{
    if (rootContainer == null)
        return;

    DetachPanelHandlers();

    TeardownPlacedPanel();

    rootContainer = null;
    contentBody = null;
    floatingAnchor = null;
    panelScrollViewer = null;
    mainStack = null;
    _chartTraderElement = null;
    _placementGrid = null;
    _placementMode = PanelPlacement.None;
    if (_placementRetryTimer != null)
    {
        _placementRetryTimer.Stop();
        _placementRetryTimer = null;
    }
    _placementRetryCount = 0;

    ClearPanelWidgetRefs();
}
```

- **Remaining logic description:** null guard early return; `DetachPanelHandlers()` call; `TeardownPlacedPanel()` call; core scalar field resets (6 fields + enum + timer null-check block); `ClearPanelWidgetRefs()` call.
- **Projected CYC:** 3 (guard `if` + timer `if` + base = 3)

---

## max_cyc_projected: 5
## extraction_count: 5

---

## Jane Street Alignment

| Principle | Status | Notes |
|---|---|---|
| CYC<=8 achieved | YES | max_cyc_projected=5; all helpers <=5; parent=3 |
| Single-responsibility per helper | YES | Each helper owns exactly one placement mode or one nullification concern |
| Lock-free/Actor pattern preserved | YES | All execution is WPF Dispatcher-thread-only; no `lock` blocks introduced or present |
| Illegal states unrepresentable | YES | `_placementMode` reset to `PanelPlacement.None` after teardown; all widget refs nulled so any late access throws NullReferenceException rather than operating on stale state |
| Guard clauses (early return) | YES | `if (rootContainer == null) return` preserved as entry guard in parent |
| Extract Loop Body pattern | N/A | No loops in this method |
| String literals ASCII-only | YES | All method names, string literals in error messages use ASCII only; no Unicode or curly quotes |
| xUnit [Fact] tests per helper | PLANNED | Phase 4 tickets will specify one [Fact] per extracted helper |
| ONE method per epic | YES | Only `DestroyPanel` is the target; all helpers are new extractions from it |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-011 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Bobcoins Used** | 3.5 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **jcodemunch tools called** | resolve_repo, get_context_bundle (x2 — initial + disambiguated), get_call_hierarchy (x2 — initial + disambiguated), get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 5 |
| **max_cyc_projected** | 5 |
| **Output File** | docs/brain/EPIC-W7-011/02-architecture-plan.md |
