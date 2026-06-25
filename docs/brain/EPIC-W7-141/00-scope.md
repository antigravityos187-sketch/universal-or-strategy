# Phase 1: Scope Definition - EPIC-W7-141

**Agent**: v12-phase1-scope
**Date**: 2026-06-24
**Epic**: EPIC-W7-141
**Target Method**: AuditFleet_CheckWorkingStop
**File**: V12_002.REAPER.Audit.cs
**Current Complexity**: 9
**Target Complexity**: ≤8

## Scope Overview

This epic targets the `AuditFleet_CheckWorkingStop` method which has a cyclomatic complexity of 9, exceeding the Jane Street threshold of 8 by 1 point. The complexity stems from a compound boolean predicate in a LINQ query that checks multiple order conditions.

## Current Method Analysis

**Location**: `V12_002.REAPER.Audit.cs`, lines 441-451

**Current Implementation**:
```csharp
private bool AuditFleet_CheckWorkingStop(Account acct)
{
    // Build 1108.003 [D3]: Snapshot broker orders before iteration
    var orders = acct.Orders.ToArray();
    return orders.Any(o =>
        o.Instrument?.FullName == Instrument?.FullName
        && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
        && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
        && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover)
    );
}
```

**Complexity Breakdown**:
- Base: 1
- Null-conditional operator (`?.`): +1
- First OR condition (OrderState): +2
- Second OR condition (OrderType): +2
- Third OR condition (OrderAction): +2
- **Total**: 9

## Extraction Strategy

### What Will Be Extracted

**New Helper Method**: `IsWorkingStopOrder`
- **Purpose**: Encapsulate the order validation predicate
- **Signature**: `private bool IsWorkingStopOrder(Order order)`
- **Complexity**: ≤8 (target: 5-6)
- **Responsibility**: Validate if an order is a working stop order for the current instrument

### What Will Remain

**Original Method**: `AuditFleet_CheckWorkingStop`
- **Complexity After**: ≤3
- **Responsibility**: Snapshot orders and delegate validation to helper
- **Implementation**:
  ```csharp
  private bool AuditFleet_CheckWorkingStop(Account acct)
  {
      var orders = acct.Orders.ToArray();
      return orders.Any(o => IsWorkingStopOrder(o));
  }
  ```

## Extraction Boundaries

### In Scope
1. ✅ Extract order validation predicate to `IsWorkingStopOrder`
2. ✅ Preserve order snapshot logic (Build 1108.003 [D3] fix)
3. ✅ Maintain exact validation logic (no behavior changes)
4. ✅ Keep method private (internal helper)

### Out of Scope
1. ❌ Modifying caller methods (`AuditFleet_HandleNakedPosition`, `AuditMaster_HandleNakedPosition`)
2. ❌ Changing order snapshot strategy
3. ❌ Refactoring other REAPER audit methods
4. ❌ Adding new validation logic

## Dependencies and Risks

### Dependencies
- **NinjaTrader API**: `Account.Orders`, `Order` properties
- **Enums**: `OrderState`, `OrderType`, `OrderAction`, `MarketPosition`
- **Instrument**: `Instrument.FullName` for instrument matching
- **Callers**: 
  - `AuditFleet_HandleNakedPosition` (line 237)
  - `AuditMaster_HandleNakedPosition` (line 577)

### Risk Assessment

**Risk Level**: LOW

**Rationale**:
1. **Simple extraction**: Single predicate, no state mutations
2. **No side effects**: Pure validation logic
3. **Well-tested path**: REAPER audit runs every 500ms in production
4. **Isolated change**: No impact on FSM state or order submission

**Mitigation**:
- Preserve exact boolean logic (no refactoring of conditions)
- Maintain order snapshot pattern (Build 1108.003 fix)
- Verify both caller paths in testing

## Success Criteria

### Functional Requirements
1. ✅ `AuditFleet_CheckWorkingStop` complexity ≤8 (target: ≤3)
2. ✅ `IsWorkingStopOrder` complexity ≤8 (target: 5-6)
3. ✅ Exact behavior preservation (no logic changes)
4. ✅ Both callers continue to function correctly

### Technical Requirements
1. ✅ Build passes: `dotnet build` returns 0 errors
2. ✅ Hard links synced: `deploy-sync.ps1` executes successfully
3. ✅ NinjaTrader loads: F5 in IDE shows BUILD_TAG
4. ✅ No regression: REAPER audit continues to detect naked positions

### Quality Gates
1. ✅ ASCII-only compliance maintained
2. ✅ No lock-free violations introduced
3. ✅ Method naming follows V12 conventions
4. ✅ Comments preserved (Build 1108.003 reference)

## Implementation Notes

### Naming Convention
- **Helper Method**: `IsWorkingStopOrder` (follows V12 `Is*` predicate pattern)
- **Location**: Same file, immediately after `AuditFleet_CheckWorkingStop`

### Code Style
- Preserve null-conditional operator (`?.`) for safety
- Maintain compound boolean structure (no premature optimization)
- Keep Build 1108.003 comment in original method

### Testing Strategy
1. **Unit Test**: Verify `IsWorkingStopOrder` with various order states
2. **Integration Test**: F5 in NinjaTrader, verify REAPER heartbeat
3. **Regression Test**: Confirm naked position detection still fires

## Blast Radius

**Affected Files**: 1
- `src/V12_002.REAPER.Audit.cs`

**Affected Methods**: 3
- `AuditFleet_CheckWorkingStop` (modified)
- `IsWorkingStopOrder` (new)
- Callers: `AuditFleet_HandleNakedPosition`, `AuditMaster_HandleNakedPosition` (unchanged)

**Affected Subsystems**: 1
- REAPER audit subsystem (naked position detection)

**Impact**: MINIMAL
- No FSM state changes
- No order submission changes
- No IPC protocol changes

## Next Steps

Proceed to Phase 1.5 (Scope Boundary Validation) to verify:
1. Extraction boundaries are correct
2. No hidden dependencies exist
3. Complexity reduction is achievable
4. Risk assessment is accurate

---

**Agent Tracking**:
- **Agent Name**: v12-phase1-scope
- **Mode**: plan
- **Phase**: 1 (Scope Definition)
- **Input**: docs/brain/EPIC-W7-141/00-hotspots.md
- **Output**: docs/brain/EPIC-W7-141/00-scope.md
- **Status**: ✅ Complete
