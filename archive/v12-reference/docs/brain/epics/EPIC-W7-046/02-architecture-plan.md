# Phase 2: Architecture Plan — EPIC-W7-046

## Method Under Extraction

- **Method:** `HandleChartClick_ConvertPrice`
- **Source File:** `src/V12_002.UI.Callbacks.cs`
- **Line Range:** 272–353
- **Visibility:** `private bool` (partial class `V12_002 : Strategy`)
- **Original CYC:** 12
- **Target CYC:** <= 8

### jcodemunch get_context_bundle result

Symbol resolved via `search_symbols` fallback (ambiguous due to src-vm-backup copy). Canonical ID: `src/V12_002.UI.Callbacks.cs::V12_002.HandleChartClick_ConvertPrice#method`. Full source body retrieved. Signature:

```csharp
private bool HandleChartClick_ConvertPrice(
    MouseButtonEventArgs e,
    bool momoActive,
    double currentPrice,
    out double clickPrice
)
```

Key findings from body:
- 4-predicate UI safety fence (lines 289–297): `mouseInPanel.X < 0 || X > W || Y < 0 || Y > H` — CYC +4
- Dual Y-clamp (lines 310–313): two sequential `if` guards clamping Y to `[0, effectivePriceHeight]` — CYC +2
- Ternary mode label (line ~319): `momoActive ? "MOMO" : "RMA"` — CYC +1
- Post-round range validation (lines 338–350): compound-OR `if` + Print + return false — CYC +2
- Magic constant `0.667` applied inline for `effectivePriceHeight` (duplicated from `IsPointerInPriceArea`)
- Total base + branches = 1 + 11 ≈ CYC 12 (confirmed)

### jcodemunch get_call_hierarchy result

- **Callers (depth 1):** `OnChartClick` (line 231, `src/V12_002.UI.Callbacks.cs`) — 1 direct caller, AST-resolved
- **Callees (depth 1–2):** `LogBuffer.Format` (ast_inferred), `LogBuffer.ValidateThreadAffinity`, `LogBuffer.FormatInternal` — all via the `Print()` diagnostic calls inside the method body
- No cross-file callers. Method is private; zero external consumers.

### jcodemunch get_dependency_graph result

- File `src/V12_002.UI.Callbacks.cs` has **0 import edges** and **0 importer edges** in the dependency graph.
- This is a partial class file — all dependencies are resolved at compile time via the NinjaTrader SDK namespace imports.
- Blast radius is fully contained within the single file.

### jcodemunch get_extraction_candidates result

- No candidates returned (`min_complexity=3, min_callers=1`).
- Expected: private helpers do not yet exist, so the index has no cross-file caller evidence. Extraction plan is driven by Phase 0 hotspot analysis and context bundle source body inspection.

---

## Sequential Thinking Summary

**Thought 5 — Final verdict:**

Three private helper methods are extracted from `HandleChartClick_ConvertPrice`:

1. **`IsClickWithinChartBounds`** — encapsulates the 4-predicate UI safety fence (returns `false` if click is outside `[0,W]x[0,H]`). CYC = 5. Single responsibility: bounds guard only.

2. **`ConvertYCoordToPrice`** — encapsulates dual Y-clamp + linear coordinate-to-price projection. CYC = 3. Single responsibility: coordinate conversion only.

3. **`ValidatePriceInRange`** — encapsulates post-round range validation with diagnostic `Print`. CYC = 3. Single responsibility: price range guard only.

Parent after extraction retains: mouse position extraction, panel dimension locals, `effectivePriceHeight` constant, 3 delegating guard calls, debug `Print`, `RoundToTickSize`, and `return true`. **Parent CYC = 4.**

All 5 Jane Street rules satisfied. Max projected CYC = 5. Extraction count = 3.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Lines Extracted | Projected CYC |
|---|---|---|---|---|
| `IsClickWithinChartBounds` | `private bool IsClickWithinChartBounds(Point mouseInPanel, double panelW, double panelH)` | Returns `false` if mouse X or Y is outside `[0,W]` / `[0,H]` — UI safety fence | 289–297 | 5 |
| `ConvertYCoordToPrice` | `private double ConvertYCoordToPrice(double yInPanel, double effectivePriceHeight, double maxPrice, double priceRange)` | Clamps `yInPanel` to `[0, effectivePriceHeight]`, then converts Y coordinate to price via linear interpolation | 310–317 | 3 |
| `ValidatePriceInRange` | `private bool ValidatePriceInRange(double clickPrice, double minPrice, double maxPrice, double priceRange, string modeLabel)` | Returns `false` (with diagnostic `Print`) if `clickPrice` falls outside `[minPrice - priceRange, maxPrice + priceRange]` | 338–350 | 3 |

---

## Parent Method After Extraction

**Remaining logic in `HandleChartClick_ConvertPrice` after extraction:**

```
1. clickPrice = 0  (initialization)
2. Point mouseInPanel = e.GetPosition(ChartPanel)
3. if (!IsClickWithinChartBounds(mouseInPanel, ChartPanel.W, ChartPanel.H)) return false;
4. Locals: panelHeight, maxPrice, minPrice, priceRange, effectivePriceHeight
5. clickPrice = ConvertYCoordToPrice(mouseInPanel.Y, effectivePriceHeight, maxPrice, priceRange)
6. string modeLabel = momoActive ? "MOMO" : "RMA";
7. Print(string.Format(...))   [debug diagnostic — existing, not new]
8. clickPrice = Instrument.MasterInstrument.RoundToTickSize(clickPrice)
9. if (!ValidatePriceInRange(clickPrice, minPrice, maxPrice, priceRange, modeLabel)) return false;
10. return true;
```

- **Projected CYC: 4** (1 base + 1 bounds guard + 1 ternary + 1 range guard)
- Method now acts as pure orchestrator: no inline computation, no interleaved branching

---

## max_cyc_projected: 5
## extraction_count: 3

---

## Jane Street Alignment

| Rule | Status | Evidence |
|---|---|---|
| CYC<=8 achieved | **YES** | Parent=4, IsClickWithinChartBounds=5, ConvertYCoordToPrice=3, ValidatePriceInRange=3. Max=5. |
| Single-responsibility per helper | **YES** | Each helper does exactly one thing: bounds check, coordinate conversion, range validation. |
| Lock-free / Actor pattern preserved | **YES** | No `lock()` blocks in method. Downstream `Enqueue` calls (ExecuteMomo/ExecuteRma) are unchanged. No new state mutations introduced. |
| Illegal states unrepresentable | **YES** | Out-of-bounds click state made explicit via `IsClickWithinChartBounds`. Out-of-range price made explicit via `ValidatePriceInRange`. Both return `false` at named guard sites — no ambiguous fall-through. |
| Zero-allocation hot paths | **YES** | All helpers receive value types (`double`, `bool`) or `Point` struct (stack-allocated). `string modeLabel` and `Print` calls are pre-existing diagnostic paths, not added by extraction. No new heap allocations on the hot path. |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-046 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Method** | `HandleChartClick_ConvertPrice` |
| **Source File** | `src/V12_002.UI.Callbacks.cs` |
| **Original CYC** | 12 |
| **Max CYC Projected** | 5 |
| **Extraction Count** | 3 |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | resolve_repo, search_symbols, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Input Artifacts** | `01-scope-boundary.md`, `00-scope.md`, `00-hotspots.md` |
| **Output Artifact** | `docs/brain/EPIC-W7-046/02-architecture-plan.md` |
