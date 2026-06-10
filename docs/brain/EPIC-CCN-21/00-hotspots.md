# EPIC-CCN-21: OnBarUpdate Complexity Analysis

**Epic ID**: EPIC-CCN-21  
**Method**: `OnBarUpdate`  
**File**: `src/V12_002.BarUpdate.cs`  
**Current CYC**: 10  
**Target CYC**: ≤8 (Jane Street GODMODE)  
**Status**: Phase 0 - Hotspot Analysis  
**Date**: 2026-06-09

---

## Phase 0: Hotspot Analysis

### Method Signature
```csharp
protected override void OnBarUpdate()
```

### Current Complexity Breakdown

**Total CYC**: 10

**Complexity Sources**:
1. **BarsInProgress guard** (line ~290): +1 (early return)
2. **CurrentBar guard** (line ~293): +1 (early return)
3. **Try-catch-finally block** (line ~296): +1 (exception handling)
4. **BarsArray null check** (line ~330): +1 (conditional)
5. **Session crosses midnight check** (line ~345): +1 (boolean logic)
6. **Active positions check** (line ~370): +1 (conditional)
7. **FFMA armed check** (line ~378): +1 (conditional with AND)
8. **Pending TREND entry check** (line ~323): +1 (conditional)
9. **OR complete check** (line ~367): Embedded in helper calls
10. **Compliance hub check** (line ~75): Embedded in ProcessSessionReset

### Already Extracted Helpers (Good!)
- ✅ `DrawMNLAnchorIfActive()` - MNL anchor drawing
- ✅ `ProcessSessionReset()` - Session reset with compliance
- ✅ `ProcessORWindowBuilding()` - OR window tracking
- ✅ `ProcessORCompletion()` - OR completion marking
- ✅ `UpdateORBoxDisplay()` - OR box display updates

### Remaining Complexity Hotspots

**Hotspot 1: Pending TREND Entry Logic** (lines 323-327)
```csharp
if (pendingTRENDEntry)
{
    double trendDist = CalculateTRENDStopDistance();
    int trendContracts = CalculatePositionSize(trendDist);
    ExecuteTRENDEntry(trendContracts);
}
```
- **CYC Contribution**: +1
- **Extraction Difficulty**: LOW
- **Reason**: Self-contained, clear responsibility

**Hotspot 2: ATR Update Logic** (lines 330-333)
```csharp
if (BarsArray[1] != null && BarsArray[1].Count > RMAATRPeriod)
{
    currentATR = atrIndicator[0];
}
```
- **CYC Contribution**: +1
- **Extraction Difficulty**: LOW
- **Reason**: Simple conditional, single responsibility

**Hotspot 3: Trailing Stops Management** (lines 370-375)
```csharp
if (activePositions.Count > 0)
{
    Enqueue(ctx => ctx.ManageTrailingStops());
    Enqueue(ctx => ctx.ManageCIT());
}
```
- **CYC Contribution**: +1
- **Extraction Difficulty**: LOW
- **Reason**: Clear guard condition, simple enqueue calls

**Hotspot 4: FFMA Condition Check** (lines 378-381)
```csharp
if (isFFMAModeArmed && FFMAEnabled)
{
    CheckFFMAConditions();
}
```
- **CYC Contribution**: +1
- **Extraction Difficulty**: LOW
- **Reason**: Simple guard with AND condition

### Complexity Reduction Strategy

**Target**: Reduce CYC from 10 to ≤8 (reduction of 2-3 points)

**Approach**: Extract 2-3 small helper methods for remaining hotspots

**Estimated Ticket Count**: 2 tickets
- Ticket 1: Extract pending entry processing (TREND + FFMA)
- Ticket 2: Extract position management logic (ATR + trailing stops)

### Extraction Difficulty Assessment

**Overall Difficulty**: **LOW**

**Reasons**:
1. Method already well-structured with 5 helpers
2. Remaining hotspots are small, self-contained
3. No complex state mutations
4. Clear separation of concerns
5. All extractions are pure orchestration moves

**Risk Level**: **MINIMAL**
- No lock-free concerns (already using Enqueue)
- No FSM state transitions
- No order lifecycle changes
- Pure structural refactoring

### Jane Street Alignment

**Current State**:
- ✅ Correctness by construction (guards prevent invalid states)
- ✅ Lock-free (uses Enqueue for state mutations)
- ✅ ASCII-only (no Unicode in strings)
- ⚠️ CYC=10 (exceeds GODMODE threshold of 8)

**Post-Extraction State**:
- ✅ CYC≤8 (Jane Street GODMODE achieved)
- ✅ Cognitive simplicity (all logic in named helpers)
- ✅ Testable (each helper can be unit tested)

### Success Criteria

- [ ] CYC reduced from 10 to ≤8
- [ ] All extracted methods have CYC≤8
- [ ] Zero logic drift (pure structural movement)
- [ ] All tests passing
- [ ] F5 verification successful
- [ ] BUILD_TAG updated

---

## Complexity Assessment Summary

| Metric | Value |
|--------|-------|
| Current CYC | 10 |
| Target CYC | ≤8 |
| CYC Reduction Needed | 2-3 points |
| Estimated Tickets | 2 |
| Extraction Difficulty | LOW |
| Risk Level | MINIMAL |
| Jane Street Alignment | ⚠️ (CYC exceeds threshold) |

---

## Next Phase

**Phase 1**: Scope Definition
- Define exact extraction boundaries
- Prevent scope creep (no pre-existing fixes)
- List dependencies and risks

**Orchestrator**: Ready to proceed to Phase 1.
