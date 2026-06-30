# Phase 4 Tickets — EPIC-W7-028

**Epic**: EPIC-W7-028
**Method**: ProcessFlattenWorkItem_CancelOrders
**Source File**: V12_002.SIMA.Flatten.cs
**Original CYC**: 9 (manifest listed 0 due to indexing gap; Phase 2 source analysis confirmed CYC=9)
**Wave**: 7 | **Phase**: 4

## Ticket Summary

ticket_count: 2

## Tickets

### Ticket 1

ticket_id: T1
helper_name: IsTerminalOrderState
concern: Classify whether an OrderState value represents a terminal (done) state — single-responsibility predicate extracted from inline 5-way OR chain in parent loop
lines_to_move: Lines within foreach loop: `state == OrderState.Cancelled || state == OrderState.CancelPending || state == OrderState.CancelSubmitted || state == OrderState.Filled || state == OrderState.Rejected`
cyc_reduction: 4 (removes 4 branch points from parent; 5-OR becomes 1 delegated call)
projected_helper_cyc: 6
inlining_hint: [MethodImpl(MethodImplOptions.AggressiveInlining)] — hot path, called every loop iteration
test_requirement: xUnit [Fact] tests covering all 5 terminal states (Cancelled, CancelPending, CancelSubmitted, Filled, Rejected) + at least 1 non-terminal state (e.g., Working)

**Implementation Contract**:
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsTerminalOrderState(OrderState state)
{
    return state == OrderState.Cancelled
        || state == OrderState.CancelPending
        || state == OrderState.CancelSubmitted
        || state == OrderState.Filled
        || state == OrderState.Rejected;
}
```

---

### Ticket 2

ticket_id: T2
helper_name: IsZombieTargetOrder
concern: Classify whether an Order's name matches a zombie-sweep target pattern — single-responsibility predicate extracted from inline 6-way StartsWith OR chain inside ZombieSweepOnly block
lines_to_move: Lines inside `if (item.ZombieSweepOnly)` block: 6-way `order.Name.StartsWith(...)` OR chain for EMERGENCY_STOP_, T1_, T2_, T3_, T4_, T5_ prefixes
cyc_reduction: 5 (removes 5 branch points from parent; 6-OR becomes 1 delegated call inside cold block)
projected_helper_cyc: 7
inlining_hint: [MethodImpl(MethodImplOptions.NoInlining)] — cold path (ZombieSweepOnly is a rarely-true flag)
test_requirement: xUnit [Fact] tests covering all 6 matching prefixes (EMERGENCY_STOP_, T1_–T5_) + at least 1 non-matching name; OrdinalIgnoreCase case-insensitivity verified

**Implementation Contract**:
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private static bool IsZombieTargetOrder(Order order)
{
    return order.Name.StartsWith("EMERGENCY_STOP_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T5_", StringComparison.OrdinalIgnoreCase);
}
```

---

## Extraction Summary

projected_parent_cyc_after_all: 6

| Symbol | Role | Projected CYC | Inlining Hint | Threshold |
|--------|------|--------------|---------------|-----------|
| `ProcessFlattenWorkItem_CancelOrders` | Parent (reduced) | 6 | N/A | <= 8 PASS |
| `IsTerminalOrderState(OrderState) -> bool` | Hot-path state predicate | 6 | AggressiveInlining | <= 8 PASS |
| `IsZombieTargetOrder(Order) -> bool` | Cold-path name predicate | 7 | NoInlining | <= 8 PASS |

**max_cyc_projected**: 7 (IsZombieTargetOrder)
**CYC baseline → target**: 9 → 6 (parent). Net parent reduction: 3 points.
**All projections within Jane Street <= 8 threshold**: YES

### DNA Compliance (from Phase 3 audit)

- dna_verdict: PASS
- lock_blocks: 0
- ascii_only: PASS
- scope_creep: NONE (zero cross-file edges)
- cyc_compliance: ALL <= 8
- test_framework: xUnit [Fact] (no NUnit/MSTest)

### Sequential Thinking Evidence

- **Thought 1**: CYC=0 in manifest is an indexing artifact. Phase 2 source analysis at lines 191–238 confirms CYC=9. Two extractions required.
- **Thought 2**: T1 = IsTerminalOrderState (5-OR, hot, AggressiveInlining). T2 = IsZombieTargetOrder (6-OR, cold, NoInlining). Each is a single-concern pure predicate.
- **Thought 3**: All CYC projections ≤ 8 (parent=6, T1=6, T2=7). Max projected = 7. Compliant.

---

## Agent Tracking

- Agent Name: v12-phase4-tickets
- Wave: 7
- Phase: 4
- Epic: EPIC-W7-028
- Method: ProcessFlattenWorkItem_CancelOrders
- Original CYC: 9 (indexing gap showed 0; confirmed 9 from Phase 2)
- ticket_count: 2
