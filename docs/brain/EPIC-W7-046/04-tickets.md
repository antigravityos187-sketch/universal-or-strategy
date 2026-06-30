# Phase 4: Implementation Tickets — EPIC-W7-046

**Epic:** EPIC-W7-046
**Method:** HandleChartClick_ConvertPrice
**Source:** src/V12_002.UI.Callbacks.cs
**Original CYC:** 12
**Wave:** 7 | **Phase:** 4 — Ticket Generation

---

## ticket_count: 3

---

## Ticket 1

- **ticket_id:** 1
- **helper_name:** `IsClickWithinChartBounds`
- **signature:** `private bool IsClickWithinChartBounds(Point mouseInPanel, double panelW, double panelH)`
- **concern:** UI safety fence — returns `false` if mouse X or Y is outside `[0, panelW]` / `[0, panelH]`
- **lines_to_move:** Lines 289–297 — the 4-predicate compound-OR bounds check block (`mouseInPanel.X < 0 || mouseInPanel.X > panelW || mouseInPanel.Y < 0 || mouseInPanel.Y > panelH`)
- **cyc_reduction:** 4 (four OR-branch predicates removed from parent)
- **projected_helper_cyc:** 5

---

## Ticket 2

- **ticket_id:** 2
- **helper_name:** `ConvertYCoordToPrice`
- **signature:** `private double ConvertYCoordToPrice(double yInPanel, double effectivePriceHeight, double maxPrice, double priceRange)`
- **concern:** Coordinate conversion — clamps `yInPanel` to `[0, effectivePriceHeight]`, then converts Y pixel coordinate to price via linear interpolation
- **lines_to_move:** Lines 310–317 — two sequential `if`-guards clamping `yInPanel`, followed by the linear interpolation formula: `maxPrice - (yInPanel / effectivePriceHeight) * priceRange`
- **cyc_reduction:** 2 (two clamp guards removed from parent)
- **projected_helper_cyc:** 3

---

## Ticket 3

- **ticket_id:** 3
- **helper_name:** `ValidatePriceInRange`
- **signature:** `private bool ValidatePriceInRange(double clickPrice, double minPrice, double maxPrice, double priceRange, string modeLabel)`
- **concern:** Post-round range guard — returns `false` (with diagnostic `Print`) if `clickPrice` falls outside `[minPrice - priceRange, maxPrice + priceRange]`
- **lines_to_move:** Lines 338–350 — compound-OR `if` block (`clickPrice < minPrice - priceRange || clickPrice > maxPrice + priceRange`) plus the diagnostic `Print(...)` call and `return false` inside
- **cyc_reduction:** 2 (compound-OR if + diagnostic path removed from parent)
- **projected_helper_cyc:** 3

---

## projected_parent_cyc_after_all: 4

**Residual logic in `HandleChartClick_ConvertPrice` after all 3 extractions:**
1. `clickPrice = 0` (initialization)
2. `Point mouseInPanel = e.GetPosition(ChartPanel)` (mouse position)
3. `if (!IsClickWithinChartBounds(...)) return false;` — 1 branch
4. Panel dimension locals + `effectivePriceHeight` constant
5. `clickPrice = ConvertYCoordToPrice(...)` — delegating call
6. `string modeLabel = momoActive ? "MOMO" : "RMA"` — 1 ternary branch
7. Existing diagnostic `Print(...)` call
8. `clickPrice = Instrument.MasterInstrument.RoundToTickSize(clickPrice)`
9. `if (!ValidatePriceInRange(...)) return false;` — 1 branch
10. `return true;`

**CYC breakdown:** 1 (base) + 1 (bounds guard) + 1 (ternary) + 1 (range guard) = **4**

---

## CYC Verification Matrix

| Method | Projected CYC | <= 8 Threshold |
|---|---|---|
| `IsClickWithinChartBounds` (helper 1) | 5 | PASS |
| `ConvertYCoordToPrice` (helper 2) | 3 | PASS |
| `ValidatePriceInRange` (helper 3) | 3 | PASS |
| `HandleChartClick_ConvertPrice` (parent) | 4 | PASS |
| **Maximum** | **5** | **PASS** |

All methods satisfy Jane Street CYC <= 8 mandate.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-046 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | 0.6 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **jcodemunch tools called** | resolve_repo, search_symbols, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket breakdown) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 4 |
| **Original CYC** | 12 |
| **Input Artifacts** | `docs/brain/EPIC-W7-046/02-architecture-plan.md`, `docs/brain/EPIC-W7-046/03-audit-report.md` |
| **Output Artifact** | `docs/brain/EPIC-W7-046/04-tickets.md` |
