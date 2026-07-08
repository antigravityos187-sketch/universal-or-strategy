# Phase 4: Ticket Generation — EPIC-W7-142

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Inputs:**
- `docs/brain/EPIC-W7-142/02-architecture-plan.md`
- `docs/brain/EPIC-W7-142/03-audit-report.md`

---

## Method Under Extraction

- **Method:** `HandleChartClick_ConvertPrice`
- **Source File:** `src/V12_002.UI.Callbacks.cs`
- **Lines:** 272 – 353 (82 lines)
- **Original CYC:** 8 (Lizard/Codacy) | 12 (jcodemunch, ternary counted)
- **DNA Verdict:** PASS (Phase 3)

---

## ticket_count: 3

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **helper_name** | `IsClickInsideChartPanel` |
| **concern** | Pure UI bounds predicate — returns `true` if the mouse position is within the chart panel dimensions (width and height). Owns the full boundary guard: replaces the 4-decision compound `if` expression in the parent. |
| **lines_to_move** | The compound boundary guard block inside `HandleChartClick_ConvertPrice` (approx. lines 280–285): the multi-condition check `mousePos.X >= 0 && mousePos.X <= panelWidth && mousePos.Y >= 0 && mousePos.Y <= panelHeight`. Extract these 4 boolean comparisons into a new expression-body private static method. Parent call site becomes `if (!IsClickInsideChartPanel(mouseInPanel, ChartPanel.ActualWidth, ChartPanel.ActualHeight)) return false;` |
| **cyc_reduction** | 4 (removes 4 decision points from parent: 4 comparison branches in && chain) |
| **projected_helper_cyc** | 4 |

**New Method Signature:**
```csharp
private static bool IsClickInsideChartPanel(Point mousePos, double panelWidth, double panelHeight) =>
    mousePos.X >= 0 && mousePos.X <= panelWidth &&
    mousePos.Y >= 0 && mousePos.Y <= panelHeight;
```

**Jane Street Alignment:**
- Zero allocation (value-type `Point` + `double` parameters only)
- No state mutation, no side effects
- Single responsibility: UI bounds check
- Testable with xUnit `[Theory]` — pure static predicate

---

## Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | T2 |
| **helper_name** | `IsPriceWithinExtendedRange` |
| **concern** | Pure price range predicate — returns `true` if a price value falls within the extended trading range `[minPrice - priceRange, maxPrice + priceRange]`. Owns the full range validation guard: replaces the 2-decision compound `if` expression in the parent. |
| **lines_to_move** | The range validation guard block inside `HandleChartClick_ConvertPrice` (approx. lines 340–350): the compound check `price >= minPrice - priceRange && price <= maxPrice + priceRange`. Extract these 2 boolean comparisons into a new expression-body private static method. Parent call site becomes `if (!IsPriceWithinExtendedRange(clickPrice, minPrice, maxPrice, priceRange)) { Print(...); return false; }` |
| **cyc_reduction** | 2 (removes 2 decision points from parent: 2 comparison branches in && chain) |
| **projected_helper_cyc** | 2 |

**New Method Signature:**
```csharp
private static bool IsPriceWithinExtendedRange(
    double price,
    double minPrice,
    double maxPrice,
    double priceRange) =>
    price >= minPrice - priceRange && price <= maxPrice + priceRange;
```

**Jane Street Alignment:**
- Zero allocation (double arithmetic only, no heap objects)
- No side effects (`Print` stays in the caller — single-responsibility separation)
- Single responsibility: price range validation
- Testable with xUnit `[Theory]` — pure static predicate

---

## Ticket 3

| Field | Value |
|---|---|
| **ticket_id** | T3 |
| **helper_name** | *(parent cleanup — no new extracted helper)* |
| **concern** | Parent method structural simplification — replace the dual sequential `if` Y-clamp pattern with a single `Math.Clamp` inline call; wire in calls to `IsClickInsideChartPanel` (T1) and `IsPriceWithinExtendedRange` (T2) at the correct guard positions; remove all inlined condition blocks. Leaves `HandleChartClick_ConvertPrice` with only 2 remaining `if`-guard call sites and 1 ternary (not Lizard-counted). |
| **lines_to_move** | Dual sequential `if` Y-clamp block (approx. lines 305–315): `if (mouseInPanel.Y < 0) mouseInPanel.Y = 0; if (mouseInPanel.Y > effectivePriceHeight) mouseInPanel.Y = effectivePriceHeight;` — replace entirely with `double yInPanel = Math.Clamp(mouseInPanel.Y, 0.0, effectivePriceHeight);`. Also update the two guard sites to use helper call results from T1 and T2. |
| **cyc_reduction** | 2 (removes 2 decision points from parent: the dual if-clamp branches) |
| **projected_helper_cyc** | N/A — no new helper method; this ticket modifies the parent body only |

**Parent After T3 — Remaining Logic:**
1. `clickPrice = 0` — out param init
2. `Point mouseInPanel = e.GetPosition(...)` — coordinate acquisition
3. `if (!IsClickInsideChartPanel(...)) return false;` — guard call (T1)
4. Local variable declarations (`panelHeight`, `maxPrice`, `minPrice`, `priceRange`, `effectivePriceHeight`)
5. `double yInPanel = Math.Clamp(mouseInPanel.Y, 0.0, effectivePriceHeight);` — inline clamp
6. Y-ratio and price conversion arithmetic (`yRatio`, `clickPrice` assignment)
7. `string modeLabel = momoActive ? "MOMO" : "RMA";` — ternary label (not counted by Lizard)
8. `Print(string.Format(...))` — diagnostic output
9. `clickPrice = Instrument.MasterInstrument.RoundToTickSize(clickPrice);`
10. `if (!IsPriceWithinExtendedRange(...)) { Print(...); return false; }` — guard call (T2)
11. `return true;`

---

## CYC Verification Summary

| Method | Before | After | Compliant (<=8) |
|---|---|---|---|
| `HandleChartClick_ConvertPrice` (parent) | 8 (Lizard) | 3 (Lizard) | YES |
| `IsClickInsideChartPanel` (T1, new) | — | 4 | YES |
| `IsPriceWithinExtendedRange` (T2, new) | — | 2 | YES |
| **max_cyc_projected** | — | **4** | YES |

**projected_parent_cyc_after_all: 3**

---

## Execution Order

Tickets must be applied in dependency order:

1. **T1** — Add `IsClickInsideChartPanel` method (no dependencies)
2. **T2** — Add `IsPriceWithinExtendedRange` method (no dependencies)
3. **T3** — Rewrite parent body (depends on T1 + T2 being present)

T1 and T2 may be written simultaneously; T3 requires both.

---

## xUnit Test Stubs

```csharp
// T1 tests
[Theory]
[InlineData(0, 0, 100, 100, true)]       // corner (valid)
[InlineData(100, 100, 100, 100, true)]   // corner (valid boundary)
[InlineData(-1, 50, 100, 100, false)]    // X out of bounds left
[InlineData(50, -1, 100, 100, false)]    // Y out of bounds top
[InlineData(101, 50, 100, 100, false)]   // X out of bounds right
public void IsClickInsideChartPanel_ReturnsExpected(
    double x, double y, double w, double h, bool expected)
{
    Assert.Equal(expected, V12_002.IsClickInsideChartPanel(new Point(x, y), w, h));
}

// T2 tests
[Theory]
[InlineData(100, 90, 110, 5, true)]    // inside extended range
[InlineData(84, 90, 110, 5, false)]    // below min - range
[InlineData(116, 90, 110, 5, false)]   // above max + range
[InlineData(85, 90, 110, 5, true)]     // exactly at lower bound
[InlineData(115, 90, 110, 5, true)]    // exactly at upper bound
public void IsPriceWithinExtendedRange_ReturnsExpected(
    double price, double min, double max, double range, bool expected)
{
    Assert.Equal(expected, V12_002.IsPriceWithinExtendedRange(price, min, max, range));
}
```

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-142 |
| **Bobcoins Used** | 5 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **jcodemunch tools called** | resolve_repo, search_symbols, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 analysis thoughts) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 3 |
| **max_cyc_projected** | 4 |
| **dna_verdict_input** | PASS (from 03-audit-report.md) |
