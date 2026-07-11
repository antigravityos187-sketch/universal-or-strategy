# Phase 2: Architecture Plan — EPIC-W7-142

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-142/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `HandleChartClick_ConvertPrice`
- **Source File:** `src/V12_002.UI.Callbacks.cs`
- **Lines:** 272 – 353
- **Original CYC:** 8 (Lizard/Codacy measurement; ternary not counted)
- **Visibility:** `private`
- **Return type:** `bool` (false = abort click, true = price valid)
- **Signature:**
  ```csharp
  private bool HandleChartClick_ConvertPrice(
      MouseButtonEventArgs e,
      bool momoActive,
      double currentPrice,
      out double clickPrice
  )
  ```

### jcodemunch get_context_bundle result
- Symbol resolved: `src/V12_002.UI.Callbacks.cs::V12_002.HandleChartClick_ConvertPrice#method`
- Full source retrieved (lines 272–353, 82 lines)
- Three structural complexity drivers confirmed:
  1. Compound boundary guard (`if` with 3 `||` operators) — 4 decision points
  2. Dual Y-clamp sequential `if` statements — 2 decision points
  3. Range-validation guard with one `||` — 2 decision points
- Total decisions = 8; CYC = 8 (Lizard/Codacy; ternary excluded per tool convention)

### jcodemunch get_call_hierarchy result
- **Callers (depth 1):** `OnChartClick` at `src/V12_002.UI.Callbacks.cs:231` — 1 direct caller (same file)
- **Callees (depth 1):** `LogBuffer.Format` (inferred via `Print()` delegation)
- **Callees (depth 2):** `LogBuffer.ValidateThreadAffinity`, `LogBuffer.FormatInternal`
- **Cross-file callers:** 0 — method is entirely private with single call site
- External contract (name, params, return type) must not change

### jcodemunch get_dependency_graph result
- `src/V12_002.UI.Callbacks.cs` has **0 edges** (no file-level imports or importers detected in graph)
- File is a self-contained partial class; all extractions remain in the same file
- Cross-file blast radius: **zero**

### jcodemunch get_extraction_candidates result
- No candidates surfaced by automated analysis (min_complexity=3, min_callers=1)
- This is expected: the file has no multi-caller helpers yet — all extracted methods will be new private statics
- Manual analysis from source + hotspot doc is the authoritative extraction guide

---

## Sequential Thinking Summary

**5-thought chain conclusion:**

CYC=8 is confirmed at the boundary per Lizard/Codacy tooling (ternary not counted). Despite being technically compliant, extraction is warranted because:

1. The compound boundary guard packs 4 CYC-points into a single `if` expression — cognitively dense and ideal for named extraction
2. The Phase 1.5 scope boundary doc explicitly anticipates helpers to be named in Phase 2
3. The hotspot analysis pre-computed the same extraction plan independently

**Final verdict:** Extract 2 private static helpers + apply 1 inline `Math.Clamp` replacement.

- `IsClickInsideChartPanel` — replaces 4-decision compound boundary guard → helper CYC = 4
- `IsPriceWithinExtendedRange` — replaces 2-decision range check → helper CYC = 2
- `Math.Clamp` inline — replaces 2-decision Y-clamp pattern → no new method, CYC reduction = -2

Parent CYC after: **3** (base 1 + 2 call-site if-guards)
Max CYC across all methods: **4**

---

## Extraction Plan

| Helper Method Name | Responsibility | Signature | Projected CYC |
|---|---|---|---|
| `IsClickInsideChartPanel` | Pure bounds predicate — returns true if `mousePos` is within the chart panel dimensions | `private static bool IsClickInsideChartPanel(Point mousePos, double panelWidth, double panelHeight)` | 4 |
| `IsPriceWithinExtendedRange` | Pure range predicate — returns true if `price` is within `[min - range, max + range]` | `private static bool IsPriceWithinExtendedRange(double price, double minPrice, double maxPrice, double priceRange)` | 2 |
| *(inline)* `Math.Clamp` replacement | Replace dual `if` Y-clamp with `double yInPanel = Math.Clamp(mouseInPanel.Y, 0.0, effectivePriceHeight);` | N/A — inline expression, no new method | 0 |

### Helper Method Bodies

**`IsClickInsideChartPanel`:**
```csharp
private static bool IsClickInsideChartPanel(Point mousePos, double panelWidth, double panelHeight) =>
    mousePos.X >= 0 && mousePos.X <= panelWidth &&
    mousePos.Y >= 0 && mousePos.Y <= panelHeight;
```
- Zero allocation (value-type parameters only)
- No state mutation, no side effects
- Single responsibility: UI bounds check

**`IsPriceWithinExtendedRange`:**
```csharp
private static bool IsPriceWithinExtendedRange(
    double price,
    double minPrice,
    double maxPrice,
    double priceRange) =>
    price >= minPrice - priceRange && price <= maxPrice + priceRange;
```
- Zero allocation (double arithmetic only)
- No side effects (Print stays in caller — single-responsibility separation)
- Single responsibility: price range validation

---

## Parent Method After Extraction

**Remaining logic in `HandleChartClick_ConvertPrice`:**
1. `clickPrice = 0` (out param init)
2. `Point mouseInPanel = e.GetPosition(...)` — coordinate acquisition
3. `if (!IsClickInsideChartPanel(mouseInPanel, ChartPanel.W, ChartPanel.H)) return false;` — guard call
4. Local variable declarations (`panelHeight`, `maxPrice`, `minPrice`, `priceRange`, `effectivePriceHeight`)
5. `double yInPanel = Math.Clamp(mouseInPanel.Y, 0.0, effectivePriceHeight);` — inline clamp
6. Y-ratio and price conversion arithmetic (`yRatio`, `clickPrice` assignment)
7. `string modeLabel = momoActive ? "MOMO" : "RMA";` — label (ternary, not counted by Lizard)
8. `Print(string.Format(...))` — diagnostic output
9. `clickPrice = Instrument.MasterInstrument.RoundToTickSize(clickPrice);`
10. `if (!IsPriceWithinExtendedRange(clickPrice, minPrice, maxPrice, priceRange)) { Print(...); return false; }` — range guard call
11. `return true;`

**Projected CYC of parent after extraction:**
- Decision points: 2 (outer `if` for panel guard + outer `if` for range guard)
- Base: 1
- **Projected CYC = 3**

---

## max_cyc_projected: 4
## extraction_count: 2

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved: all methods <=8 | **YES** — parent=3, IsClickInsideChartPanel=4, IsPriceWithinExtendedRange=2 |
| Single-responsibility per helper | **YES** — each helper does exactly one thing (bounds check / range check) |
| Lock-free / Actor pattern preserved | **YES** — method is UI callback, no state mutations, no lock() blocks introduced or present |
| Illegal states unrepresentable | **YES** — early-return guard pattern makes out-of-bounds state structurally impossible to pass through |
| Zero-allocation hot paths | **YES** — all helpers use value-type parameters only (Point, double); no heap allocs |
| Extract Guard Clauses | **YES** — boundary guard and range check are extracted as named early-return guards |
| Extract to Named Helper Methods | **YES** — two private static helpers with descriptive names reflecting single concern |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-142 |
| **Bobcoins Used** | 4 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 2 |
| **max_cyc_projected** | 4 |
| **boundary_verdict_input** | PASS (from 01-scope-boundary.md) |
