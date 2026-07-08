# Phase 4 Tickets — EPIC-W7-024

**Epic**: EPIC-W7-024
**Method**: MonitorRmaProximity
**Source File**: V12_002.Entries.RMA.cs
**Original CYC**: 34 (pre-EPIC-CCN-13 baseline) / 9 (MCP-confirmed current, `src/V12_002.Entries.RMA.cs:383`)
**Wave**: 7 | **Phase**: 4

---

## Ticket Summary

ticket_count: 2

---

## Tickets

### Ticket 1

ticket_id: T1
helper_name: `ProcessProximityOrder`
concern: Process a single RMA entry order's full proximity lifecycle — compute the drawing tag, apply the ShouldMonitorOrder filter guard, calculate tick distance, and delegate threshold routing to DispatchProximityAction.
lines_to_move: Entire foreach body (approx lines 394–417 in current `src/V12_002.Entries.RMA.cs`): `string proximityTag = string.Format(...)`, `if (!ShouldMonitorOrder(...)) continue`, `double distTicks = UpdateProximityAndCalculateDistance(...)`, call to `DispatchProximityAction(...)`.
cyc_reduction: -3 from parent (removes clusters C: ShouldMonitorOrder-guard, D1: proximity-if, D2: cancellation-else-if)
projected_helper_cyc: 3

**Signature**:
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private void ProcessProximityOrder(
    string orderId,
    Order order,
    double currentClose)
```

**CFG breakdown**:
- Base path: +1
- `if (!ShouldMonitorOrder(...)) continue` guard: +1
- Implicit iterator path: +1
- **Subtotal = 3** (threshold dispatch delegated to T2)

---

### Ticket 2

ticket_id: T2
helper_name: `DispatchProximityAction`
concern: 3-way threshold routing — exclusively owns the decision logic between proximity entry, dead-zone hysteresis, and proximity exit based on `distTicks` vs `RmaProximityTicks` / `RmaCancellationTicks`.
lines_to_move: 3-way if/else-if/else block extracted from within `ProcessProximityOrder` (T1): `if (distTicks <= RmaProximityTicks) { HandleProximityEntry(...) }` / `else if (distTicks < RmaCancellationTicks) { /* dead zone */ }` / `else { HandleProximityExit(...) }` (~10 lines).
cyc_reduction: -2 from ProcessProximityOrder (removes D1: proximity-if and D2: cancellation-else-if)
projected_helper_cyc: 3

**Signature**:
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private void DispatchProximityAction(
    string orderId,
    Order order,
    PositionInfo pos,
    double distTicks,
    string proximityTag)
```

**CFG breakdown**:
- Base path: +1
- `if (distTicks <= RmaProximityTicks)`: +1
- `else if (distTicks < RmaCancellationTicks)`: +1
- **Subtotal = 3**

---

## Extraction Order

Tickets must be executed in dependency order:
1. **T2 first** — `DispatchProximityAction` is nested inside T1's body; define it first so T1 can call it.
2. **T1 second** — `ProcessProximityOrder` calls `DispatchProximityAction`; implement after T2 is defined.
3. **Parent update last** — Replace foreach body in `MonitorRmaProximity` with single call to `ProcessProximityOrder(kvp.Key, kvp.Value, currentClose)`.

---

## Extraction Summary

projected_parent_cyc_after_all: 4

**Parent CFG after all extractions**:
- Base path: +1
- `if (!RmaIntelligenceEnabled) return` guard (cluster A): +1
- `foreach (var kvp in entryOrders)` (cluster B): +1
- `try/finally` exception path (cluster E): +1
- **Total = 4** (≤8 ✓)

**Full CYC Validation Table**:

| Symbol | CYC Before | CYC After | Within Budget (≤8)? |
|---|---|---|---|
| `MonitorRmaProximity` (parent) | 9 | 4 | YES ✓ |
| `ProcessProximityOrder` (T1, new) | 0 | 3 | YES ✓ |
| `DispatchProximityAction` (T2, new) | 0 | 3 | YES ✓ |

**max_cyc_projected: 4**

---

## DNA Compliance (Phase 3 Confirmed)

| Check | Status |
|---|---|
| Zero `lock()` blocks | PASS |
| ASCII-only string literals | PASS |
| UTF-8 no BOM | PASS |
| No scope creep (only `src/V12_002.Entries.RMA.cs` changes) | PASS |
| xUnit `[Fact]`/`Assert.Equal()` tests required in Phase 5 | MANDATED |
| `max_cyc_projected` ≤ 8 | PASS (4) |

---

## Agent Tracking

- Agent Name: v12-phase4-tickets
- Wave: 7
- Phase: 4
- Epic: EPIC-W7-024
- Method: MonitorRmaProximity
- Original CYC: 34 (baseline) / 9 (current MCP)
- ticket_count: 2
- MCP Tools Used: resolve_repo, search_symbols, get_symbol_complexity, get_extraction_candidates, sequential-thinking (3 thoughts)
- Sequential Thinking Conclusion: 2 tickets validated; parent CYC=4, T1 CYC=3, T2 CYC=3; all ≤8
- Generated: 2026-06-29T01:20:00Z
