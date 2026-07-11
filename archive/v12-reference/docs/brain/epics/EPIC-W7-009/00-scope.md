# EPIC-W7-009 — Phase 1: Scope Definition

## Single Method in Scope

| Field              | Value                                         |
|--------------------|-----------------------------------------------|
| Method             | `FindChartTraderViaChartTab`                  |
| Source file        | `src/V12_002.UI.Panel.Helpers.cs`             |
| Lines              | 529–564                                       |
| Current CYC        | **9**                                         |
| Target CYC         | **≤ 8** (minimum acceptable reduction: −1)    |
| Class              | `V12_002`                                     |
| Namespace          | `NinjaTrader.NinjaScript.Strategies`          |
| Visibility         | `private`                                     |
| Return type        | `FrameworkElement`                            |

This is a **single method** refactor. The scope boundary is drawn around
`FindChartTraderViaChartTab` exclusively and does not extend to any of its
callers, sibling strategies, or extracted helper methods.

---

## Scope Boundary

The **scope boundary** for EPIC-W7-009 is precisely:

```
src/V12_002.UI.Panel.Helpers.cs  lines 529–564
method: FindChartTraderViaChartTab()
```

Everything outside that boundary — callers, downstream helpers, other files —
is read-only reference material. No modifications may be made to symbols
outside the scope boundary during Phase 1 or Phase 2 of this epic.

---

## Callers

Caller discovery was performed via `grep` across the full 53-file `.cs`
corpus in `src/`.

| # | Symbol | File | Line | Role |
|---|--------|------|------|------|
| 1 | `FindChartTrader` | `src/V12_002.UI.Panel.Helpers.cs` | 491 | Direct caller — Strategy-1 arm in 5-strategy chain |
| 2 | `V12_002` constructor / panel init | `src/V12_002.UI.Panel.Construction.cs` | 244 | Indirect caller — `_chartTraderElement = FindChartTrader()` |

**Total callers: 2** (1 direct, 1 indirect via `FindChartTrader`).

No additional call sites for `FindChartTraderViaChartTab` were found anywhere
in the repository. The blast radius is LOW-MEDIUM; the method is invoked
once at panel initialisation and is not on any hot path.

---

## Why Other Methods Are NOT in Scope

The codebase is at version **V12.23**. Under the V12.23 engineering policy the
following constraints apply:

1. **Single-method CYC reduction principle.** Wave-7 epics are scoped to the
   single highest-CYC hotspot per ticket. `FindChartTrader` (CYC 6) is below
   the intervention threshold of CYC 8 and is therefore excluded.

2. **Extracted helpers are frozen post-CCN-17.** The five helpers extracted
   during EPIC-CCN-17 (`TryFindChartTabViaVisualTree`,
   `TryFindChartTabViaLogicalTree`, `TryGetChartTraderViaProperty`,
   `TryGetChartTraderViaFields`, `TryGetChartTraderViaDescendants`) are
   already at CYC ≤ 4. Re-entering them would create unnecessary churn
   against stable, tested code.

3. **Sibling Panel.Helpers methods.** Static analysis confirmed that no other
   method in `src/V12_002.UI.Panel.Helpers.cs` exceeds CYC 5. None qualifies
   for Wave-7 intervention under the V12.23 threshold policy.

4. **Caller chain is read-only.** `FindChartTrader` (line 478) and the
   `V12_002` constructor (Construction.cs:244) are part of the blast radius
   but are not complexity hotspots. Modifying them would broaden the scope
   boundary beyond the single method mandate of this epic.

5. **Cross-file exclusion.** The remaining 52 `.cs` files in `src/` contain
   no references to `FindChartTraderViaChartTab` and are entirely outside the
   scope boundary.

---

## Complexity Drivers (from Phase 0)

| Branch | Description | CYC contribution |
|--------|-------------|-----------------|
| 1 | Method base | +1 |
| 2 | `if (chartTab == null)` → visual-tree miss | +1 |
| 3 | `chartTab = TryFindChartTabViaLogicalTree(...)` separate path | +1 |
| 4 | `if (chartTab == null)` → both trees failed, early return | +1 |
| 5 | First `??` operator in reflection chain | +1 |
| 6 | Second `??` operator in reflection chain | +1 |
| 7 | `if (result == null)` diagnostic guard | +1 |
| 8 | `catch (Exception ex)` handler | +1 |
| 9 | Implicit return-null after catch | +1 |
| **Total** | | **9** |

Recommended reduction (Phase 2): consolidate the dual-tree ChartTab search
(`branches 2–3`) into a single `TryFindChartTab(ChartControl)` helper,
eliminating one predicate node → **CYC 9 → 8**, meeting the ≤ 8 target.

---

## Agent Tracking

| Field           | Value                                      |
|-----------------|--------------------------------------------|
| Epic            | EPIC-W7-009                                |
| Wave            | 7                                          |
| Phase           | 1 — Scope Definition                       |
| Method          | `FindChartTraderViaChartTab`               |
| Source file     | `src/V12_002.UI.Panel.Helpers.cs`          |
| Current CYC     | 9                                          |
| Target CYC      | ≤ 8                                        |
| Callers count   | 2 (1 direct, 1 indirect)                   |
| Scope confirmed | single method                              |
| Output artifact | `docs/brain/EPIC-W7-009/00-scope.md`       |
| Agent Name      | v12-phase1-scope                           |
| Status          | Phase 1 complete                           |
| Timestamp       | 2025-07-10                                 |
