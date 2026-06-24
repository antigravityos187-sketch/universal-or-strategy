# Phase 1: Scope Boundary Definition - EPIC-W7-156

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:38:41Z
- **Mode**: plan

## Epic Summary
**Target**: CancelAll_ProcessSingleFleetAccount (CYC 18 → ≤8)
**File**: src/V12_002.UI.IPC.Commands.Fleet.cs
**Line**: 300
**Complexity Reduction**: 18 → ≤8 (10-point reduction required)

## IN SCOPE

### Primary Target
✅ **CancelAll_ProcessSingleFleetAccount method** (lines 300-344)
- Extract decision logic into helper methods
- Reduce CYC from 18 to ≤8
- Maintain exact external behavior
- Preserve all error handling

### Extraction Candidates
Based on CYC=18 and 4 nesting levels, likely extractions:

1. **Order Filtering Logic**
   - Terminal order checks
   - Order state validation
   - Account-specific filtering

2. **Cancellation Decision Logic**
   - Conditional cancellation rules
   - Order eligibility checks
   - Fleet-specific business rules

3. **Error Handling Paths**
   - Exception handling
   - Logging logic
   - Failure recovery

### Success Criteria
1. ✅ All extracted methods have CYC ≤8
2. ✅ Original method becomes orchestrator (CYC ≤5)
3. ✅ Zero behavior changes (external contract preserved)
4. ✅ Build passes with deploy-sync.ps1
5. ✅ F5 in NinjaTrader succeeds

## OUT OF SCOPE

### Explicitly Excluded
❌ **Caller Methods** (same file, but separate concerns)
- CancelAll_ProcessFleetOrders (line 275)
- CancelAll_ProcessFleetAccounts (line 268)
- These are separate epics if they exceed CYC threshold

❌ **Callee Methods** (different files)
- CancelOrderOnAccount (src/V12_002.Orders.CancelGateway.cs:46)
- IsOrderTerminal (src/V12_002.Orders.Management.Flatten.cs:698)
- These are stable dependencies, do not modify

❌ **Related Fleet Commands**
- Other IPC command handlers in same file
- Fleet management infrastructure
- IPC communication layer

❌ **Test Files**
- No test modifications unless tests break
- No new test creation (separate epic)

❌ **Documentation Updates**
- No XML doc comment changes
- No README updates
- Focus purely on complexity reduction

### Boundary Enforcement
**ONE EPIC = ONE CONCERN** (V12.23 Protocol)
- If pre-existing compilation errors found → STOP and report
- If unrelated issues discovered → Document but do not fix
- If scope expands → Seek Director approval

## Risk Mitigation

### Low-Risk Factors
✅ **Isolated Blast Radius**: 0 external dependencies
✅ **Same-File Callers**: Both callers in same file
✅ **Clear Purpose**: Fleet order cancellation logic
✅ **No Cross-File Impact**: Changes contained

### Medium-Risk Factors
⚠️ **High Complexity**: CYC=18 requires careful extraction
⚠️ **Moderate Nesting**: 4 levels needs attention
⚠️ **44 Lines**: Substantial logic to decompose

### Mitigation Strategy
1. Extract one helper at a time
2. Verify build after each extraction
3. Run deploy-sync.ps1 after each change
4. Test in NinjaTrader after each ticket

## Scope Validation Checklist

### Pre-Refactoring
- [ ] Verify codebase compiles cleanly
- [ ] Confirm no uncommitted src/ changes
- [ ] Check GitButler virtual branch active
- [ ] Verify jCodemunch index fresh

### During Refactoring
- [ ] Touch only CancelAll_ProcessSingleFleetAccount
- [ ] No modifications to callers or callees
- [ ] No whitespace mutations in other methods
- [ ] No "while we're here" improvements

### Post-Refactoring
- [ ] All extracted methods CYC ≤8
- [ ] Original method CYC ≤5
- [ ] Build passes
- [ ] deploy-sync.ps1 succeeds
- [ ] F5 in NinjaTrader succeeds

## Scope Boundary Approval

**Status**: ✅ APPROVED
**Rationale**: 
- Clear, isolated target method
- Zero external dependencies
- Well-defined extraction scope
- Low blast radius risk
- Aligns with V12.23 No Scope Creep Protocol

**Next Phase**: Phase 2 (Architecture Planning)
