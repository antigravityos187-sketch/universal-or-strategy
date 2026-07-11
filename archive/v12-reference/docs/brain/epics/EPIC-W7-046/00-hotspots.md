# EPIC-W7-046 · Phase 0 — Hotspot Analysis

## Method Under Analysis

| Field            | Value                                                   |
|------------------|---------------------------------------------------------|
| **Method Name**  | `HandleChartClick_ConvertPrice`                         |
| **CYC Score**    | 12 (confirmed)                                          |
| **Source File**  | `src/V12_002.UI.Callbacks.cs`                           |
| **Line Range**   | 272 – 353                                               |
| **Visibility**   | `private bool` (partial class `V12_002 : Strategy`)     |
| **Wave / Phase** | Wave 7 / Phase 0                                        |

---

## Blast Radius Summary

`HandleChartClick_ConvertPrice` sits on the **click-to-trade hot path**.

```
PreviewMouseLeftButtonDown (WPF event)
  └─► OnChartClick()                         [line 231]
        └─► HandleChartClick_ConvertPrice()  [line 242]  ← TARGET
              ├─► HandleChartClick_ExecuteMomo()          [line 247]
              │     └─► Enqueue(ctx.ExecuteMOMOEntry)
              └─► HandleChartClick_ExecuteRma()           [line 251]
                    └─► Enqueue(ctx.ExecuteRMAEntryV2)
```

- **Direct callers:** 1 (`OnChartClick`)
- **Downstream order-execution methods:** 2 (`ExecuteMomo`, `ExecuteRma`)
- **Shared state:** `ChartPanel.H * 0.667` magic constant duplicated in
  `IsPointerInPriceArea` (line 153) — single-source-of-truth hazard
- **Risk surface:** Any regression here silently misroutes a live trade order;
  there is no unit-test harness isolating price conversion logic

---

## Top 3 Complexity Drivers

### 1 · Four-predicate UI Safety Fence (lines 289–297) — CYC +4
```csharp
if (mouseInPanel.X < 0
    || mouseInPanel.X > ChartPanel.W
    || mouseInPanel.Y < 0
    || mouseInPanel.Y > ChartPanel.H)
    return false;
```
Each predicate is a distinct branch. The same four checks already exist in
`IsPointerInPriceArea` (lines 145–151), meaning the guard logic is duplicated
rather than reused. Extraction target: `IsClickWithinChartBounds(Point)`.

### 2 · Dual Y-clamp + coordinate projection (lines 310–317) — CYC +3
```csharp
if (yInPanel < 0)  yInPanel = 0;
if (yInPanel > effectivePriceHeight)  yInPanel = effectivePriceHeight;
double yRatio   = yInPanel / effectivePriceHeight;
clickPrice      = maxPrice - (yRatio * priceRange);
```
Two defensive clamps plus a coordinate-space transformation are interleaved in
one block. The `0.667` coefficient is a hard-coded approximation that appears
in two places. Extraction target: `ConvertYCoordToPrice(double y, double panelH,
double maxPrice, double minPrice) → double`.

### 3 · Post-round range validation with dual-branch guard (lines 338–350) — CYC +3
```csharp
if (clickPrice < minPrice - priceRange || clickPrice > maxPrice + priceRange)
{
    Print(string.Format(...));
    return false;
}
```
The asymmetric tolerance (`±priceRange` margin) and the embedded `Print` call
mix validation logic with diagnostic output. The ternary mode label on line 319
(`momoActive ? "MOMO" : "RMA"`) adds one more branch. Extraction target:
`ValidatePriceInRange(double price, double min, double max, string label) → bool`.

---

## Recommended Extraction Count

**3 targeted extractions:**

| # | Extracted Method              | Lines Affected | CYC Reduction |
|---|-------------------------------|----------------|---------------|
| 1 | `IsClickWithinChartBounds`    | 289–297        | −4            |
| 2 | `ConvertYCoordToPrice`        | 299–317        | −3            |
| 3 | `ValidatePriceInRange`        | 338–350        | −3            |

Post-refactor residual CYC of `HandleChartClick_ConvertPrice`: **≤ 3**
(ternary mode label + single `return true` path + `Print` call).

---

## Agent Tracking

```json
{
  "epic":        "EPIC-W7-046",
  "wave":        7,
  "phase":       0,
  "status":      "completed",
  "output":      "docs/brain/EPIC-W7-046/00-hotspots.md",
  "method":      "HandleChartClick_ConvertPrice",
  "cyc":         12,
  "cyc_confirmed": true,
  "source_file": "src/V12_002.UI.Callbacks.cs",
  "lines":       "272-353",
  "blast_radius": {
    "direct_callers": 1,
    "downstream_exec_methods": 2,
    "duplicated_magic_constant_sites": 2,
    "order_execution_risk": "HIGH"
  },
  "extractions_recommended": 3,
  "extracted_methods": [
    "IsClickWithinChartBounds",
    "ConvertYCoordToPrice",
    "ValidatePriceInRange"
  ],
  "generated_by": "Bob (jcodemunch wave-7 hotspot pipeline)",
  "timestamp":   "2025-07-14T00:00:00Z"
}
```
