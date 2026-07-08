# EPIC-W7-142 · Phase 0 — Hotspot Analysis

## Target Method

| Field        | Value                                    |
|--------------|------------------------------------------|
| **Method**   | `HandleChartClick_ConvertPrice`          |
| **CYC**      | 8                                        |
| **File**     | `src/V12_002.UI.Callbacks.cs`            |
| **Lines**    | 272 – 353                                |
| **Visibility** | `private`                              |
| **Return**   | `bool` (false = abort click, true = price valid) |

---

## Blast Radius Summary

| Dimension          | Detail                                                                                       |
|--------------------|----------------------------------------------------------------------------------------------|
| **Direct callers** | 1 — [`OnChartClick`](../../src/V12_002.UI.Callbacks.cs) (line 242, same file)               |
| **Cross-file refs**| 0 — no references outside `V12_002.UI.Callbacks.cs`                                         |
| **Scope**          | **Narrow / contained.** The method is a private helper with a single call site.              |
| **UI thread**      | Yes — executes synchronously on the WPF dispatcher via `MouseButtonEventArgs`               |
| **Side-effects**   | `Print()` diagnostic output (not trade-critical); mutates `out double clickPrice`           |
| **Refactor risk**  | Low — any extraction stays within the same partial class; no public API surface to preserve  |

---

## Top 3 Complexity Drivers

### Driver 1 — Compound boundary guard (CYC +4, lines 289–297)

```csharp
if (
    mouseInPanel.X < 0
    || mouseInPanel.X > ChartPanel.W   // short-circuit OR → +1
    || mouseInPanel.Y < 0              // short-circuit OR → +1
    || mouseInPanel.Y > ChartPanel.H   // short-circuit OR → +1
)
{
    return false;
}
```

A single `if` with four logical sub-conditions (three `||` operators) contributes **4 CYC points** (base `if` + 3 short-circuit branches). This is the dominant hotspot. It encodes the "click is inside the chart panel" invariant and is a natural candidate for extraction into a named guard:

```csharp
private bool IsClickInsidePanel(Point p) =>
    p.X >= 0 && p.X <= ChartPanel.W &&
    p.Y >= 0 && p.Y <= ChartPanel.H;
```

---

### Driver 2 — Dual Y-clamp guards (CYC +2, lines 310–313)

```csharp
if (yInPanel < 0)
    yInPanel = 0;
if (yInPanel > effectivePriceHeight)
    yInPanel = effectivePriceHeight;
```

Two sequential `if` statements clamp `yInPanel` to `[0, effectivePriceHeight]`. Each adds 1 CYC. This logic duplicates a standard `Math.Clamp` pattern and can be collapsed to:

```csharp
double yInPanel = Math.Clamp(mouseInPanel.Y, 0, effectivePriceHeight);
```

---

### Driver 3 — Range-validation guard with short-circuit OR (CYC +2, lines 338–350)

```csharp
if (clickPrice < minPrice - priceRange || clickPrice > maxPrice + priceRange)
{
    Print(…);
    return false;
}
```

The outer `if` (+1) plus one `||` (+1) totals **2 CYC points**. It also embeds a `Print()` side-effect that couples logging to the predicate. Extraction:

```csharp
private bool IsPriceWithinExtendedRange(double price, double min, double max, double range) =>
    price >= min - range && price <= max + range;
```

---

## Recommended Extraction Count

| Extraction | Target name | CYC reduction |
|-----------|-------------|---------------|
| 1 | `IsClickInsidePanel(Point p)` | −4 |
| 2 | Y-clamp via `Math.Clamp` (inline, not extracted) | −2 |
| 3 | `IsPriceWithinExtendedRange(…)` | −1 |

**Total extractions: 2 new private methods + 1 inline replacement.**  
Projected post-refactor CYC of `HandleChartClick_ConvertPrice`: **≤ 3** (base + ternary + range-result bool).

The ternary on line 319 (`momoActive ? "MOMO" : "RMA"`) contributes 1 CYC and is acceptable as-is — it is a single-expression label with no nested logic.

---

## Agent Tracking

| Field             | Value                          |
|-------------------|--------------------------------|
| **Agent Name**    | v12-phase0-hotspot             |
| **Bobcoins Used** | 6                              |
| **Execution Time**| ~90s                           |
| **Wave**          | 7                              |
| **Phase**         | 0 — Hotspot Analysis           |
| **Status**        | completed                      |
