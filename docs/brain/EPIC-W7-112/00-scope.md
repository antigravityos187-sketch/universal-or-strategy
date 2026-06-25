# Phase 1: Scope Definition - EPIC-W7-112

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:39:11Z

## Epic Overview
- **Target Method**: ClassifyOrderByPrefix
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 1262
- **Current CYC**: 20
- **Target CYC**: ≤8 (Jane Street strict standard)
- **Reduction Required**: 12 points (60% reduction)

## Scope Boundary Analysis

### IN SCOPE: Core Classification Logic
1. **Order Prefix Classification** (PRIMARY TARGET)
   - Master order detection (prefix "M")
   - Fleet order detection (prefix "F")
   - Target order detection (prefix "T")
   - Unclassified order handling
   - Decision tree with 20 cyclomatic paths

2. **Method Signature Preservation**
   - Input: Single Order parameter
   - Output: OrderClassification enum
   - No side effects (pure classification function)

3. **Caller Integration Points** (4 callers, all in same file)
   - AdoptOrdersFromAccount (line 930)
   - AdoptMasterOrders (line 1195)
   - AdoptFleetOrders (line 903)
   - HydrateWorkingOrdersFromBroker (line 309)

### OUT OF SCOPE: External Dependencies
1. **Order Adoption Logic** (separate concern)
   - AdoptOrdersFromAccount implementation
   - AdoptMasterOrders implementation
   - AdoptFleetOrders implementation
   - HydrateWorkingOrdersFromBroker implementation

2. **FSM State Management** (separate concern)
   - SIMA_FSM lifecycle operations
   - State transition logic
   - Order linking to FSM instances

3. **Broker Integration** (separate concern)
   - Order hydration from broker
   - Account-level order queries
   - Order submission/cancellation

4. **Logging and Diagnostics** (preserve as-is)
   - Existing log statements
   - Debug output
   - Error reporting

## Extraction Strategy

### Phase 1: Identify Decision Branches
- Analyze all 20 cyclomatic paths
- Map prefix patterns to classification outcomes
- Identify nested conditionals contributing to complexity

### Phase 2: Extract Sub-Classifiers (Target CYC ≤8 each)
- **ExtractMasterOrderClassification()** - Handle "M" prefix logic
- **ExtractFleetOrderClassification()** - Handle "F" prefix logic
- **ExtractTargetOrderClassification()** - Handle "T" prefix logic
- **ExtractUnclassifiedOrderHandling()** - Handle default/error cases

### Phase 3: Refactor Main Method
- Reduce ClassifyOrderByPrefix to orchestrator (CYC ≤8)
- Delegate to extracted sub-classifiers
- Maintain identical external behavior

## Risk Mitigation

### Zero Blast Radius Advantage
- **No external dependencies** - All callers in same file
- **No downstream calls** - Pure classification function
- **Low refactoring risk** - Changes isolated to SIMA.Lifecycle.cs

### Testing Strategy
1. **Unit Tests** (NEW)
   - Test each extracted sub-classifier independently
   - Cover all 20 decision paths
   - Validate classification outcomes

2. **Integration Tests** (EXISTING)
   - Verify 4 caller methods still work correctly
   - Test order adoption workflows end-to-end
   - Validate FSM lifecycle integration

3. **Regression Prevention**
   - Compare classification results before/after refactoring
   - Test with production order data samples
   - Verify no behavioral changes

## Success Criteria
- ClassifyOrderByPrefix reduced to CYC ≤8
- All extracted methods have CYC ≤8
- All 4 callers continue to work correctly
- Unit tests cover all decision paths
- Build passes with zero errors
- F5 in NinjaTrader successful

## Scope Boundary Validation

### Mandatory Gate (Phase 1.5)
Before proceeding to Phase 2 (Architecture Planning):
1. Verify scope does NOT include order adoption logic
2. Verify scope does NOT include FSM state management
3. Verify scope does NOT include broker integration
4. Confirm focus is ONLY on classification decision tree

### Scope Creep Prevention
- **ONE EPIC = ONE CONCERN**: Classification logic only
- **No "while we're here" improvements**: Resist temptation to refactor callers
- **Surgical extraction**: Touch only ClassifyOrderByPrefix and its extracted methods

## Jane Street Alignment
- **Cognitive Simplicity**: Break 20-path decision tree into ≤8-path sub-trees
- **Testability**: Each sub-classifier independently testable
- **Correctness by Construction**: Enum-based classification prevents invalid states
- **Lock-Free**: Pure function, no state mutations, no locks required

## Next Phase: Architecture Planning (Phase 2)
Input: This scope definition (00-scope.md)
Output: 02-architecture-plan.md with detailed extraction design
