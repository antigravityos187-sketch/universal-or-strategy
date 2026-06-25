# Phase 1: Scope Definition - EPIC-W7-015

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:04:40Z

## Epic Summary
**Target Method**: CancelAll_ProcessSingleFleetAccount
**File**: src/V12_002.UI.IPC.Commands.Fleet.cs
**Current CYC**: 18
**Target CYC**: ≤8 per extracted method
**Blast Radius**: LOW (0 external dependencies)

## Scope Boundary Analysis

### IN SCOPE

#### Primary Extraction Target
- **Method**: CancelAll_ProcessSingleFleetAccount (CYC 18)
  - **Location**: src/V12_002.UI.IPC.Commands.Fleet.cs:300
  - **Rationale**: Exceeds Jane Street threshold by 125% (18 vs ≤8)
  - **Risk**: LOW blast radius, internal-only usage

#### Extraction Candidates (Based on Complexity Analysis)
1. **Order Cancellation Loop Logic**
   - Extract iteration over orders for cancellation
   - Reduce nesting depth from 4 to ≤2
   - Target CYC: ≤5

2. **Terminal State Validation**
   - Extract terminal state checking logic
   - Calls IsOrderTerminal method
   - Target CYC: ≤3

3. **Fleet Account Iteration**
   - Extract account-level iteration logic
   - Coordinate with order-level processing
   - Target CYC: ≤4

#### Direct Dependencies (IN SCOPE for verification)
- **CancelOrderOnAccount** (src/V12_002.Orders.CancelGateway.cs:46)
  - Verify signature compatibility
  - Ensure no breaking changes to call sites

- **IsOrderTerminal** (src/V12_002.Orders.Management.Flatten.cs:698)
  - Verify signature compatibility
  - Ensure no breaking changes to call sites

#### Callers (IN SCOPE for verification)
- **CancelAll_ProcessFleetOrders** (src/V12_002.UI.IPC.Commands.Fleet.cs:275)
  - Verify no signature changes to CancelAll_ProcessSingleFleetAccount
  - Ensure behavioral equivalence after extraction

- **CancelAll_ProcessFleetAccounts** (src/V12_002.UI.IPC.Commands.Fleet.cs:268)
  - Verify no signature changes to CancelAll_ProcessSingleFleetAccount
  - Ensure behavioral equivalence after extraction

### OUT OF SCOPE

#### Excluded from Refactoring
1. **CancelOrderOnAccount method**
   - External method in different file
   - No complexity issues reported
   - Only verify signature compatibility

2. **IsOrderTerminal method**
   - External method in different file
   - No complexity issues reported
   - Only verify signature compatibility

3. **Caller methods (CancelAll_ProcessFleetOrders, CancelAll_ProcessFleetAccounts)**
   - Not targeted for extraction in this epic
   - Only verify behavioral equivalence after refactoring

4. **Other methods in V12_002.UI.IPC.Commands.Fleet.cs**
   - Not part of this epic scope
   - No changes unless directly related to extraction

#### Architectural Changes (OUT OF SCOPE)
- No changes to FSM/Actor pattern
- No changes to IPC command structure
- No changes to fleet management architecture
- No changes to order cancellation gateway

#### Cross-File Refactoring (OUT OF SCOPE)
- No changes to src/V12_002.Orders.CancelGateway.cs
- No changes to src/V12_002.Orders.Management.Flatten.cs
- No changes to other partial class files

## Extraction Strategy

### Target Complexity Reduction
- **Current**: CYC 18 (single method)
- **Target**: 3-4 methods, each CYC ≤8
- **Expected Distribution**:
  - Main orchestration method: CYC ≤4
  - Order cancellation logic: CYC ≤5
  - Terminal state validation: CYC ≤3
  - Fleet account iteration: CYC ≤4

### Scope Validation Criteria
✅ **Complexity**: CYC 18 → target ≤8 per method
✅ **Blast Radius**: LOW (0 external dependencies)
✅ **Call Hierarchy**: 2 callers, both in same file
✅ **Risk**: MEDIUM overall (HIGH complexity, LOW blast radius)

### Boundary Enforcement
- **File Boundary**: Changes limited to src/V12_002.UI.IPC.Commands.Fleet.cs
- **Method Boundary**: Only CancelAll_ProcessSingleFleetAccount and extracted helpers
- **Signature Boundary**: No changes to public/internal method signatures
- **Behavioral Boundary**: Maintain exact behavioral equivalence

## Phase 1 Completion
- ✅ Scope boundaries defined (IN SCOPE vs OUT OF SCOPE)
- ✅ Extraction candidates identified
- ✅ Complexity targets set (≤8 per method)
- ✅ Risk assessment validated (MEDIUM overall)
- ✅ Boundary enforcement rules established

**Next Phase**: Phase 1.5 (Scope Boundary Validation)
