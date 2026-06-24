# Phase 1: Scope Boundary - EPIC-W7-015

## Agent Tracking
- **Agent Name**: plan mode
- **Execution Time**: 2026-06-24T01:49:01Z
- **Phase**: 1 (Scope Definition)

## Epic Summary
**Target**: CancelAll_ProcessSingleFleetAccount
**File**: src/V12_002.UI.IPC.Commands.Fleet.cs
**Current CYC**: 18
**Target CYC**: ≤8 per extracted method
**Risk Level**: MEDIUM (HIGH complexity, LOW blast radius)

## IN SCOPE

### Primary Target
- **Method**: CancelAll_ProcessSingleFleetAccount
  - **Location**: src/V12_002.UI.IPC.Commands.Fleet.cs:300
  - **Current CYC**: 18
  - **Lines**: 44
  - **Action**: Extract to reduce complexity to ≤8

### Extraction Candidates (Based on Hotspot Analysis)
1. **Order Cancellation Loop Logic**
   - Extract iteration over orders for a single fleet account
   - Reduce nesting depth from 4 to ≤2
   
2. **Terminal State Validation**
   - Extract terminal state checking logic
   - Calls IsOrderTerminal (already exists in V12_002.Orders.Management.Flatten.cs:698)
   
3. **Fleet Account Iteration**
   - Extract fleet account processing logic
   - Coordinate with CancelOrderOnAccount (V12_002.Orders.CancelGateway.cs:46)

### Files to Modify
- src/V12_002.UI.IPC.Commands.Fleet.cs (primary target file)

## OUT OF SCOPE

### Callers (No Changes Required)
- **CancelAll_ProcessFleetOrders** (line 275)
  - Reason: Caller interface remains unchanged
  - Action: None
  
- **CancelAll_ProcessFleetAccounts** (line 268)
  - Reason: Caller interface remains unchanged
  - Action: None

### Callees (No Changes Required)
- **CancelOrderOnAccount** (V12_002.Orders.CancelGateway.cs:46)
  - Reason: Well-defined interface, no modifications needed
  - Action: None
  
- **IsOrderTerminal** (V12_002.Orders.Management.Flatten.cs:698)
  - Reason: Existing utility method, no modifications needed
  - Action: None

### Related Files (No Changes)
- src/V12_002.Orders.CancelGateway.cs
- src/V12_002.Orders.Management.Flatten.cs
- Any other files in the codebase

## Scope Validation

### Complexity Reduction Target
- **Before**: CYC 18 (single method)
- **After**: CYC ≤8 per extracted method
- **Strategy**: Extract 2-3 helper methods to distribute complexity

### Blast Radius Confirmation
- **Direct Dependents**: 0 external files
- **Importer Count**: 0
- **Risk**: LOW - All changes contained within single file

### Jane Street Alignment
- ✅ Cognitive simplicity: Target CYC ≤8
- ✅ Single responsibility: Each extracted method has one clear purpose
- ✅ Testability: Smaller methods easier to unit test
- ✅ Correctness by construction: Reduce nesting to prevent illegal states

## Boundary Enforcement

### What Changes
- Method signature: NO (preserve existing interface)
- Method body: YES (extract helper methods)
- File structure: NO (remain in same file)
- External interfaces: NO (callers/callees unchanged)

### What Stays the Same
- Public API surface
- Caller contracts
- Callee contracts
- Cross-file dependencies

## Success Criteria
1. ✅ CancelAll_ProcessSingleFleetAccount reduced to CYC ≤8
2. ✅ All extracted methods have CYC ≤8
3. ✅ No changes to callers (CancelAll_ProcessFleetOrders, CancelAll_ProcessFleetAccounts)
4. ✅ No changes to callees (CancelOrderOnAccount, IsOrderTerminal)
5. ✅ Build passes after extraction
6. ✅ F5 in NinjaTrader successful

## Phase 1 Completion
- ✅ Scope boundary defined (IN SCOPE vs OUT OF SCOPE)
- ✅ Extraction targets identified
- ✅ Risk assessment confirmed
- ✅ Jane Street alignment verified

**Next Phase**: Phase 2 (Architecture Planning)
