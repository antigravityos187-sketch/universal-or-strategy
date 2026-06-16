# Extraction Tickets: EPIC-CCN-058

## Overview
- **Total Tickets**: 1
- **Execution Order**: Single atomic refactor (TICKET-1)
- **Estimated Effort**: 15 minutes
- **Strategy**: Switch expression with OR patterns (no helper methods)

---

## TICKET-1: Convert If-Chain to Switch Expression with OR Patterns

### Scope
- **Current Method**: `HydrateFSM_MapOrderStateToFsmState`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Lines**: 948-965 (18 lines)
- **Current CYC**: 9
- **Target CYC**: 5
- **Extraction**: In-place refactor (no helper methods needed)

### Current Implementation Pattern
```csharp
// If-chain with compound OR conditions (CYC=9)
if (orderState == OrderState.Filled || orderState == OrderState.PartFilled)
    return FollowerBracketState.Filled;
if (orderState == OrderState.Accepted || orderState == OrderState.Working)
    return FollowerBracketState.Working;
// ... etc
```

### Target Implementation Pattern
```csharp
// Switch expression with OR patterns (CYC=5)
return orderState switch
{
    OrderState.Filled or OrderState.PartFilled => FollowerBracketState.Filled,
    OrderState.Accepted or OrderState.Working => FollowerBracketState.Working,
    OrderState.PendingSubmit or OrderState.PendingChange => FollowerBracketState.Pending,
    OrderState.Cancelled or OrderState.Rejected => FollowerBracketState.Cancelled,
    _ => FollowerBracketState.Unknown
};
```

### Implementation Steps
1. **Git Checkpoint**: Commit current state before refactor
   ```bash
   git add src/V12_002.SIMA.Lifecycle.cs
   git commit -m "checkpoint: before EPIC-CCN-058 refactor"
   ```

2. **Refactor Method**: Replace if-chain with switch expression
   - Locate method at lines 948-965
   - Replace entire method body with switch expression
   - Use C# 9.0 `or` patterns to group related states
   - Maintain default case `_` returning `FollowerBracketState.Unknown`

3. **Verify Complexity**: Run complexity audit
   ```bash
   python scripts/complexity_audit.py
   ```
   - Confirm CYC=5 for `HydrateFSM_MapOrderStateToFsmState`

4. **Build Verification**: Ensure compilation succeeds
   ```bash
   dotnet build
   ```
   - Zero errors expected (pure refactor, no API changes)

5. **Test Validation**: Run test suite
   ```bash
   dotnet test
   ```
   - All tests must pass (behavior unchanged)

6. **Hard-link Sync**: Synchronize NinjaTrader deployment
   ```bash
   powershell -File .\deploy-sync.ps1
   ```
   - Verify sync succeeds without errors

7. **Manual Verification**: F5 in NinjaTrader
   - Visual inspection of strategy behavior
   - Confirm no runtime errors

### Acceptance Criteria
- [ ] Method complexity reduced from CYC=9 to CYC=5
- [ ] Complexity audit confirms CYC=5
- [ ] All unit tests pass (`dotnet test`)
- [ ] Build succeeds with zero errors (`dotnet build`)
- [ ] Hard-link sync succeeds (`deploy-sync.ps1`)
- [ ] No behavioral changes (caller at line 1249 unaffected)
- [ ] No scope creep (only target method modified)
- [ ] ASCII-only compliance maintained
- [ ] No lock() statements introduced
- [ ] Manual test in NinjaTrader passes

### Dependencies
- None (single atomic refactor)

### Risk Assessment
- **Blast Radius**: Single method (18 lines)
- **Caller Impact**: Zero (line 1249 caller unaffected)
- **Risk Level**: LOW
- **Rollback**: Git checkpoint + Bob CLI restore available

### V12 DNA Compliance
- ✅ **Lock-Free**: Pure function, no state mutation
- ✅ **ASCII-Only**: Switch expression uses ASCII-only syntax
- ✅ **Correctness by Construction**: Exhaustive pattern matching
- ✅ **Jane Street Alignment**: CYC=5 (below threshold of 8)

### Language Requirements
- **C# Version**: 9.0+ (for `or` patterns)
- **Project Target**: .NET 8.0 (supports C# 12.0) ✅
- **Compiler Support**: Verified compatible

### Performance Notes
- **Optimization**: Switch expressions compile to jump tables (O(1) lookup)
- **Hot Path**: Not on critical path (initialization, not tick-by-tick)
- **Impact**: NEUTRAL to POSITIVE (more predictable branching)

---

## Execution Summary

**Total Tickets**: 1  
**Total Effort**: 15 minutes  
**Complexity Reduction**: -44% (CYC 9→5)  
**Risk Level**: LOW  
**Jane Street Compliance**: ✅ PASS (CYC ≤8)

**Next Phase**: 5.0 (Ticket Execution via Bob CLI)
