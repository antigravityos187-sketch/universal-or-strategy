# Phase 1: Scope Definition - EPIC-W7-110

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:38:58Z

## Epic Overview
**Target Method**: `AdoptMasterOrders`
**File**: `src/V12_002.SIMA.Lifecycle.cs`
**Current CYC**: 22 (HIGH - exceeds threshold of 8)
**Target CYC**: ≤8 per extracted method

## Scope Boundary Definition

### IN SCOPE ✅

#### Primary Target
- **Method**: `AdoptMasterOrders` (lines 1195-1255, CYC 22)
  - Extract decision branches into helper methods
  - Reduce main method to orchestration logic (target CYC ≤5)
  - All extracted methods must achieve CYC ≤8

#### Extraction Candidates
Based on the 22 cyclomatic paths, the following logical units are IN SCOPE for extraction:

1. **Order Classification Logic**
   - Prefix-based order classification
   - Master order identification
   - Order type validation

2. **FSM Adoption Logic**
   - FSM state validation
   - Order-to-FSM linking
   - Adoption eligibility checks

3. **Error Handling & Logging**
   - Validation error paths
   - Debug logging statements
   - Edge case handling

#### Testing Requirements
- xUnit tests for all extracted methods
- Preserve existing behavior (no semantic changes)
- Verify callers still function correctly:
  - `HydrateWorkingOrdersFromBroker`
  - `EnumerateApexAccounts`

### OUT OF SCOPE ❌

#### Caller Methods (Separate Epics)
- **HydrateWorkingOrdersFromBroker** (line 309)
  - Has its own complexity profile
  - Will be addressed in separate epic if needed
  
- **EnumerateApexAccounts** (line 140)
  - Has its own complexity profile
  - Will be addressed in separate epic if needed

#### Callee Methods (Already Simple)
- **ClassifyOrderByPrefix** (line 1262)
  - Already a helper method
  - No refactoring needed in this epic

#### Infrastructure Changes
- No changes to FSM/Actor pattern
- No changes to order lifecycle architecture
- No changes to broker integration layer
- No changes to logging infrastructure

#### Cross-File Dependencies
- No changes to other partial classes
- No changes to V12_002.cs main file
- No changes to V12_002.Atm.cs
- No changes to V12_002.DrawingHelpers.cs

### Scope Rationale

**Why This Scope?**
1. **Focused Complexity Reduction**: Target single method with CYC 22
2. **Low Blast Radius**: 0 external dependencies, 2 internal callers
3. **Clear Boundaries**: Method has well-defined input/output contract
4. **Testable**: Can verify behavior preservation with unit tests
5. **Jane Street Aligned**: Achieves CYC ≤8 mandate

**Why Exclude Callers?**
- Each caller has its own complexity profile
- Mixing concerns violates "ONE EPIC = ONE CONCERN" protocol
- Separate epics allow focused testing and verification

**Why Exclude Infrastructure?**
- No architectural changes needed
- Extraction is pure refactoring (behavior preservation)
- Minimizes risk and scope creep

## Extraction Strategy

### Phase 2 Planning Inputs
The following will guide Phase 2 (Architecture Planning):

1. **Method Body Analysis**
   - Identify all 22 decision branches
   - Map branches to logical extraction units
   - Determine optimal helper method signatures

2. **Dependency Analysis**
   - Verify no hidden dependencies
   - Confirm all calls are AST-resolved
   - Validate FSM state access patterns

3. **Test Coverage Planning**
   - Design xUnit test cases for extracted methods
   - Plan integration tests for caller verification
   - Define behavior preservation criteria

## Success Criteria

### Scope Definition Complete ✅
- [x] Primary target identified: `AdoptMasterOrders`
- [x] IN SCOPE items clearly defined
- [x] OUT OF SCOPE items clearly defined with rationale
- [x] Extraction strategy outlined
- [x] Testing requirements specified

### Phase 1.5 Validation Ready
This scope definition is ready for Phase 1.5 (Scope Boundary Validation) to verify:
- No scope creep into caller methods
- No infrastructure changes
- Clear extraction boundaries
- Testable units

## Risk Mitigation

### Identified Risks
1. **Caller Impact**: Changes to `AdoptMasterOrders` affect 2 callers
   - **Mitigation**: Preserve exact method signature and behavior
   
2. **FSM State Access**: Extracted methods may need FSM context
   - **Mitigation**: Pass FSM references as parameters
   
3. **Test Coverage Gap**: No existing tests for this method
   - **Mitigation**: Add comprehensive xUnit tests during extraction

### Scope Creep Prevention
- **Gate**: Phase 1.5 validation MUST approve scope before Phase 2
- **Protocol**: Any scope expansion requires Director approval
- **Enforcement**: Reject any tickets that touch OUT OF SCOPE items

## Next Steps
Proceed to Phase 1.5 (Scope Boundary Validation) to verify:
1. No scope creep detected
2. Extraction boundaries are clear
3. Testing strategy is sufficient
4. Risk mitigation is adequate
