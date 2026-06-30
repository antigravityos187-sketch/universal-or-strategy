# Phase 4: Tickets — EPIC-W7-139

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:30:00Z
**Inputs:**
- `docs/brain/EPIC-W7-139/02-architecture-plan.md`
- `docs/brain/EPIC-W7-139/03-audit-report.md`

---

## Summary

| Field                          | Value                                     |
|-------------------------------|-------------------------------------------|
| **Epic ID**                    | EPIC-W7-139                               |
| **Method**                     | `UpdateStopOrder`                         |
| **Source File**                | `src/V12_002.Trailing.StopUpdate.cs`      |
| **Original CYC**               | 8                                         |
| **ticket_count**               | 2                                         |
| **projected_parent_cyc_after_all** | 5                                     |
| **max_cyc_projected**          | 5                                         |
| **DNA Audit Verdict**          | PASS (0 violations)                       |

---

## MCP Tool Results

### get_symbol_complexity — UpdateStopOrder
- **Result:** `Symbol 'UpdateStopOrder' not found in index.`
- **Note:** Consistent with partial-class AST resolution artefact documented in Phase 2. Manual static CYC=8 confirmed from hotspot analysis. Tool-reported CYC=0 is an artefact, not the true value.

### get_extraction_candidates — src/V12_002.Trailing.StopUpdate.cs
- **Result:** `candidates=[]` (min_complexity=3, min_callers=1)
- **Note:** Zero candidates due to partial-class indexing limitation — cyclomatic complexity data absent from index. Extraction plan derived from Phase 2 manual static analysis and sequential thinking chain below.

---

## Sequential Thinking Chain

### Thought 1 — Ticket Count Decision
Two extractable clusters identified in Phase 2 with strictly orthogonal responsibilities:
1. **Staleness gate** (decision points 2+3): pendingStopReplacements.TryGetValue + DateTime age arithmetic + STALE_PENDING_FAST_PATH_SEC threshold → single concern: "is this pending replacement stale?"
2. **State-routing cascade** (decision points 4+5): compound-OR if/else if on OrderState → single concern: "which execution path handles this OrderState?"

One ticket per helper, one helper per concern. **ticket_count = 2**

Sequencing: Ticket 1 (IsStalePendingReplacement) precedes Ticket 2 (RouteStopOrderByState) because the staleness guard appears before the routing dispatch in the parent body.

### Thought 2 — Per-Ticket Detail

**Ticket 1 — IsStalePendingReplacement:**
- Lines to move (~95–108): `pendingStopReplacements.TryGetValue(entryName, out var pendingReplacement)` + `(DateTime.Now - pendingReplacement.SubmittedAt).TotalSeconds` + `>= STALE_PENDING_FAST_PATH_SEC` threshold comparison.
- CYC reduction from parent: -2 (TryGetValue success branch + threshold comparison)
- Projected helper CYC: 3 (base=1 + TryGetValue success=1 + threshold comparison=1)

**Ticket 2 — RouteStopOrderByState:**
- Lines to move (~112–135): entire compound if/else if block on currentStop.OrderState (CancelPending, Submitted, Working, Accepted arms + implicit default).
- Refactor: replace if/else if cascade with switch expression; add explicit default arm calling CreateDirectStopOrder.
- CYC reduction from parent: -4 (four state-dispatch branches)
- Projected helper CYC: 4 (base=1 + CancelPending=1 + Submitted=1 + Working/Accepted combined arm=1)

### Thought 3 — CYC Verification
Post-extraction CYC accounting:

| Component                      | CYC  | <= 8? |
|-------------------------------|------|-------|
| `UpdateStopOrder` (final)      | 5    | YES   |
| `IsStalePendingReplacement`    | 3    | YES   |
| `RouteStopOrderByState`        | 4    | YES   |
| **max_cyc_projected**          | **5**| **YES** |

Parent CYC breakdown: base=1 + try/catch=1 + TryGetValue guard=1 + ValidateStopPrice if=1 + IsStalePendingReplacement if=1 = **5**. The RouteStopOrderByState call site contributes 0 branches (pure dispatch). All components satisfy Jane Street CYC<=8 mandate.

---

## Ticket Definitions

---

### Ticket 1

| Field                   | Value |
|------------------------|-------|
| **ticket_id**           | 1 |
| **helper_name**         | `IsStalePendingReplacement` |
| **concern**             | Staleness detection — determines whether the pending stop-order replacement for `entryName` has exceeded the stale-threshold age (`STALE_PENDING_FAST_PATH_SEC`). Encapsulates `pendingStopReplacements.TryGetValue`, `DateTime.Now` age arithmetic, and the threshold comparison. Returns `bool` result and the stale order via `out` parameter. |
| **lines_to_move**       | `UpdateStopOrder` lines ~95–108: the block that performs `pendingStopReplacements.TryGetValue(entryName, out var pendingReplacement)`, computes `(DateTime.Now - pendingReplacement.SubmittedAt).TotalSeconds`, and compares against `STALE_PENDING_FAST_PATH_SEC`. |
| **new_signature**       | `private bool IsStalePendingReplacement(string entryName, out Order stalePendingOrder)` |
| **replacement_in_parent** | `if (IsStalePendingReplacement(entryName, out var stalePending)) { HandleStalePendingReplacement(entryName, pos, stalePending); return; }` |
| **cyc_reduction**       | -2 (removes two decision points from parent: TryGetValue success branch + threshold comparison) |
| **projected_helper_cyc** | 3 |
| **verify_criteria**     | Build passes with zero errors; `IsStalePendingReplacement` exists in `src/V12_002.Trailing.StopUpdate.cs` as a private method; parent `UpdateStopOrder` body calls `IsStalePendingReplacement`; no `lock()` introduced. |

---

### Ticket 2

| Field                   | Value |
|------------------------|-------|
| **ticket_id**           | 2 |
| **helper_name**         | `RouteStopOrderByState` |
| **concern**             | State-dispatch routing — routes the stop update to the correct execution path based on `currentStop.OrderState`. Replaces the compound-OR `if/else if` cascade with a `switch` expression. Cases: `CancelPending` → `HandleStalePendingReplacement`; `Submitted` → `UpdateExistingPendingReplacement`; `Working`/`Accepted` → `InitiateStopReplacement`; explicit `default` arm → `CreateDirectStopOrder`. Makes previously-implicit fall-through explicit and unrepresentable-as-invalid. |
| **lines_to_move**       | `UpdateStopOrder` lines ~112–135: the entire compound `if/else if` block branching on `currentStop.OrderState` including all four arms and the implicit default path. |
| **new_signature**       | `private void RouteStopOrderByState(string entryName, PositionInfo pos, Order currentStop, double validatedStopPrice, int newTrailLevel)` |
| **replacement_in_parent** | `RouteStopOrderByState(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);` (pure dispatch call, 0 branches in parent) |
| **cyc_reduction**       | -4 (removes four decision points from parent: CancelPending, Submitted, Working, Accepted arms of the compound cascade) |
| **projected_helper_cyc** | 4 |
| **verify_criteria**     | Build passes with zero errors; `RouteStopOrderByState` exists in `src/V12_002.Trailing.StopUpdate.cs` as a private method; `switch` expression contains explicit `default` arm; parent `UpdateStopOrder` body calls `RouteStopOrderByState` as single dispatch; no `lock()` introduced; parent CYC <= 8. |

---

## Projected Parent State After All Extractions

```
UpdateStopOrder (final body):
  try {
    1. Guard: if (!stopOrders.TryGetValue(entryName, out var currentStop)) return;
    2. Validate: if (!ValidateStopPrice(entryName, pos, newStopPrice, out var validatedStopPrice)) return;
    3. Stale check: if (IsStalePendingReplacement(entryName, out var stalePending)) {
                       HandleStalePendingReplacement(entryName, pos, stalePending); return; }
    4. Dispatch: RouteStopOrderByState(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
  } catch (Exception ex) { HandleUpdateException(entryName, ex); }
```

**projected_parent_cyc_after_all: 5**
(base=1 + TryGetValue guard=1 + ValidateStopPrice if=1 + IsStalePendingReplacement if=1 + try/catch=1)

---

## Jane Street Alignment Confirmation

| Principle                              | Status |
|---------------------------------------|--------|
| CYC <= 8 (all components)             | YES — parent=5, helper1=3, helper2=4 |
| Single-responsibility per helper       | YES — staleness detection vs. state routing are orthogonal |
| Lock-free / Actor pattern preserved    | YES — `Enqueue` pattern in downstream delegates unchanged; no `lock()` |
| Illegal states unrepresentable         | YES — `switch` with explicit `default` arm replaces implicit fall-through |
| Zero-allocation hot paths              | YES — `out` parameter reuses stack slot; no new heap allocations |
| No scope creep (V12.23)               | YES — all changes confined to `src/V12_002.Trailing.StopUpdate.cs` |
| Caller signature unchanged             | YES — `UpdateStopOrder` public signature unchanged |
| xUnit tests planned                   | YES — `[Fact]` + `Assert.Equal()` for both helpers |

---

## Agent Tracking

| Field                  | Value |
|-----------------------|-------|
| **Agent Name**         | v12-phase4-tickets |
| **Bobcoins Used**      | 3 |
| **Execution Time**     | 2026-06-29T01:30:00Z |
| **Wave**               | 7 |
| **Phase**              | 4 |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket analysis) |
| **ticket_count**       | 2 |
| **projected_parent_cyc_after_all** | 5 |
| **max_cyc_projected**  | 5 |
