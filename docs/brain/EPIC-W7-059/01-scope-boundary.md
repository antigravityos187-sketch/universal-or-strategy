# Phase 1: Scope Boundary Definition - EPIC-W7-059

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:32:01Z
- **Input**: 00-hotspots.md
- **Output**: 01-scope-boundary.md

## Epic Objective
Reduce cyclomatic complexity of `AdoptMasterWorkingOrders` from 11 to ≤8 (3-point reduction) while maintaining order adoption semantics.

## Target Method
- **Method**: AdoptMasterWorkingOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 711
- **Current CYC**: 11
- **Target CYC**: ≤8
- **Lines**: 48
- **Max Nesting**: 6 levels

## IN SCOPE

### Primary Extraction Target
**AdoptMasterWorkingOrders method body** (lines 711-759)

### Specific Refactoring Actions
1. **Extract nested conditional logic** (nesting depth 6 → ≤3)
   - Order state validation chains
   - Classification result handling
   - Dictionary lookup and assignment logic

2. **Reduce cyclomatic complexity** (CYC 11 → 8)
   - Apply early return pattern for guard clauses
   - Extract complex conditional expressions to helper methods
   - Simplify nested if/else structures

3. **Leverage existing helpers**
   - `IsOrderStateAdoptable` (already exists at line 690)
   - `ClassifyMasterOrderByPrefix` (already exists at line 768)
   - `GetOrderDictionaryByName` (already exists at line 795)

### Allowed Modifications
- ✅ Extract new private helper methods within V12_002.SIMA.Lifecycle.cs
- ✅ Refactor conditional logic to reduce nesting
- ✅ Add early returns for guard clauses
- ✅ Simplify boolean expressions
- ✅ Reorder statements for clarity (preserving semantics)

### Success Criteria
- CYC reduced from 11 to ≤8 (minimum 3-point reduction)
- Max nesting depth reduced from 6 to ≤4
- All existing tests pass (if any)
- No change to public API or method signature
- Preserve exact order adoption behavior

## OUT OF SCOPE

### Existing Helper Methods (DO NOT MODIFY)
- ❌ `IsOrderStateAdoptable` (line 690) - already extracted, working correctly
- ❌ `ClassifyMasterOrderByPrefix` (line 768) - already extracted, working correctly
- ❌ `GetOrderDictionaryByName` (line 795) - already extracted, working correctly
- ❌ `LogBuffer.Format` and related logging methods

### Caller Methods (DO NOT MODIFY)
- ❌ `HydrateWorkingOrdersFromBroker` (line 415)
- ❌ `EnumerateApexAccounts` (line 203)

### Other Lifecycle Methods (DO NOT MODIFY)
- ❌ Any other methods in V12_002.SIMA.Lifecycle.cs not directly part of AdoptMasterWorkingOrders
- ❌ FSM state management logic outside this method
- ❌ Order dictionary initialization logic

### Infrastructure (DO NOT MODIFY)
- ❌ Logging infrastructure (LogBuffer)
- ❌ Order state enums or constants
- ❌ SIMA_FSM class definition
- ❌ Order class definition

### Testing (OUT OF SCOPE FOR THIS EPIC)
- ❌ Adding new unit tests (separate epic if needed)
- ❌ Modifying existing test files
- ❌ Integration test changes

## Scope Boundary Validation

### Blast Radius Check
- **External Dependents**: 0 ✅
- **Direct Callers**: 2 (both internal to SIMA.Lifecycle) ✅
- **Risk Level**: LOW ✅

### Dependency Analysis
- **Callees**: 9 methods (all existing helpers) ✅
- **No new external dependencies required** ✅
- **All helpers already exist** ✅

### Complexity Reduction Feasibility
- **Current CYC**: 11
- **Target CYC**: 8
- **Required Reduction**: 3 points
- **Feasibility**: HIGH (nesting depth 6 suggests multiple extraction opportunities)

## Extraction Strategy

### Phase 2 Planning Guidance
1. **Identify decision points** contributing to CYC 11
2. **Map nesting structure** to find extraction boundaries
3. **Preserve order adoption semantics** exactly
4. **Extract 2-3 helper methods** to achieve CYC ≤8

### Expected Outcome
- Main method: CYC ≤8, nesting ≤4
- 2-3 new private helper methods: each CYC ≤5
- Total lines may increase slightly (acceptable trade-off for clarity)
- Zero behavioral changes

## Risk Mitigation

### Low Risk Factors
- ✅ No external dependents (blast radius = 0)
- ✅ Only 2 internal callers
- ✅ Helper methods already exist
- ✅ Not in top 50 hotspots (lower churn)

### Medium Risk Factors
- ⚠️ Deep nesting (6 levels) requires careful extraction
- ⚠️ Order state semantics must be preserved exactly
- ⚠️ 9 callees indicate moderate internal coupling

### Mitigation Strategy
- Use existing helpers (already tested)
- Extract pure logic first (no side effects)
- Verify with complexity_audit.py after each extraction
- Manual verification in NinjaTrader IDE (F5 test)

## Approval Status
**SCOPE APPROVED** - Proceed to Phase 2 (Architecture Planning)

**Rationale**:
- Clear, focused scope (single method)
- Low blast radius (safe to refactor)
- Existing helpers reduce risk
- Achievable CYC reduction target (11 → 8)
- No scope creep (OUT OF SCOPE clearly defined)
