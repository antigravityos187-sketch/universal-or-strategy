
# Phase 1: Scope Boundary - EPIC-W7-139

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Mode**: plan
- **Execution Time**: 2026-06-24T01:37:01Z
- **Input**: docs/brain/EPIC-W7-139/00-hotspots.md

## Target Method
- **Method**: UpdateStopOrder
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Line**: 84
- **Current CYC**: 13
- **Target CYC**: <=8
- **Reduction Required**: -5 decision points

## Scope Definition

### IN SCOPE

#### Primary Extraction Target
**UpdateStopOrder method body** (lines 84-140)
- Reduce CYC from 13 to <=8
- Extract 2-3 helper methods
- Preserve existing behavior exactly

#### Extraction Candidates (Based on Call Hierarchy)
1. **Stop Price Validation Logic**
   - Calls: ValidateStopPrice
   - Purpose: Validate stop price against current market
   - Complexity: Likely 2-3 decision points
   - Extract to: ValidateAndPrepareStopUpdate()

2. **Pending Replacement Handling**
   - Calls: HandleStalePendingReplacement, UpdateExistingPendingReplacement
   - Purpose: Manage pending stop order replacements
   - Complexity: Likely 2-3 decision points
   - Extract to: ProcessPendingStopReplacement()

3. **Stop Order Creation/Update Logic**
   - Calls: InitiateStopReplacement, CreateDirectStopOrder
   - Purpose: Execute stop order creation or replacement
   - Complexity: Likely 2-3 decision points
   - Extract to: ExecuteStopOrderUpdate()

#### State Access (Must Preserve)
- stopOrders (constant, line 201)
- pendingStopReplacements (constant, line 210)
- All shared state access patterns must remain unchanged

#### Error Handling
- HandleUpdateException (line 496)
- All exception handling must be preserved
- No changes to error propagation

### OUT OF SCOPE

#### Downstream Methods (Already Extracted)
- ValidateStopPrice (src/V12_002.Orders.Management.StopSync.cs:1200)
- HandleStalePendingReplacement (src/V12_002.Trailing.StopUpdate.cs:141)
- UpdateExistingPendingReplacement (src/V12_002.Trailing.StopUpdate.cs:167)
- InitiateStopReplacement (src/V12_002.Trailing.StopUpdate.cs:307)
- CreateDirectStopOrder (src/V12_002.Trailing.StopUpdate.cs:371)
- HandleUpdateException (src/V12_002.Trailing.StopUpdate.cs:496)

**Rationale**: These are already separate methods. Do NOT refactor them.

#### Caller Investigation
- Runtime verification of call sites (event handlers, callbacks)
- Reflection/polymorphism analysis
- **Rationale**: Zero static callers detected - requires runtime analysis, but NOT part of this epic scope

#### Other Files
- src/V12_002.Orders.Management.StopSync.cs
- Any other partial class files
- **Rationale**: Single-file refactoring only

#### Test Creation
- Unit tests for extracted methods
- **Rationale**: Testing is Phase 5.V (Verification), not Phase 2-5

#### Lock-Free Conversion
- Actor pattern migration
- State synchronization changes
- **Rationale**: UpdateStopOrder does not use lock() - no conversion needed

### BOUNDARY RULES

#### What Stays in UpdateStopOrder
- Method signature (4 parameters)
- High-level orchestration logic
- State access patterns (stopOrders, pendingStopReplacements)
- Error handling delegation to HandleUpdateException

#### What Gets Extracted
- Validation logic (stop price checks)
- Pending replacement decision logic
- Stop order creation/update decision logic

#### Complexity Target
- **UpdateStopOrder**: CYC <=8 (currently 13, reduce by 5)
- **Extracted Method 1**: CYC <=8
- **Extracted Method 2**: CYC <=8
- **Extracted Method 3**: CYC <=8

#### Naming Convention
- Use descriptive names: ValidateAndPrepareStopUpdate(), ProcessPendingStopReplacement(), ExecuteStopOrderUpdate()
- Follow existing V12 naming patterns
- Private methods (not public API changes)

## Risk Mitigation

### Zero Blast Radius Concern
**Finding**: 0 direct callers detected in static analysis

**Mitigation Strategy**:
1. Assume method is called via event handler or callback
2. Preserve exact method signature (no parameter changes)
3. Preserve exact return behavior
4. No changes to exception handling
5. Phase 5.V will verify runtime behavior

### 62 Callees Concern
**Finding**: UpdateStopOrder orchestrates 62 downstream methods

**Mitigation Strategy**:
1. Do NOT modify any of the 62 callees
2. Preserve exact call sequence
3. Preserve exact parameter passing
4. Only extract decision logic, not orchestration calls

### Shared State Access
**Finding**: Accesses stopOrders and pendingStopReplacements

**Mitigation Strategy**:
1. Pass state as parameters to extracted methods
2. No changes to state access patterns
3. Preserve thread-safety assumptions

## Jane Street Alignment

### Cognitive Simplicity
- **Before**: CYC=13 (too complex for microsecond-latency reasoning)
- **After**: CYC<=8 (Jane Street strict standard)
- **Method**: Extract 2-3 helper methods with single responsibilities

### Testability
- **Before**: 13 execution paths (exponential test case growth)
- **After**: <=8 paths per method (exhaustive testing feasible)
- **Benefit**: Each extracted method can be tested independently

### Race Condition Auditing
- **Before**: 4 nesting levels + 13 paths = difficult to audit
- **After**: Flat, simple logic per method
- **Note**: No lock-free conversion needed (no lock() usage detected)

## Success Criteria

### Phase 2 (Architecture Planning)
- Identify exact extraction points (line numbers)
- Design extracted method signatures
- Verify no signature changes to UpdateStopOrder
- Create Mermaid diagram of before/after call flow

### Phase 5 (Ticket Execution)
- UpdateStopOrder CYC <=8
- All extracted methods CYC <=8
- Zero compilation errors
- deploy-sync.ps1 successful
- F5 in NinjaTrader successful

### Phase 5.V (Verification)
- Runtime verification of call sites
- Behavior unchanged (integration test)
- No performance regression

## Metadata

- **Epic ID**: EPIC-W7-139
- **Phase**: 1 (Scope Boundary)
- **Status**: Completed
- **Timestamp**: 2026-06-24T01:37:01Z
- **Input**: 00-hotspots.md
- **Output**: 01-scope-boundary.md
- **Next Phase**: Phase 2 (Architecture Planning)
