# Phase 1: Scope Definition - EPIC-W7-004

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:24:23Z
- **Input**: docs/brain/EPIC-W7-004/00-hotspots.md

## Target Method
- **Method**: HandleFleetTargetFill
- **File**: src/V12_002.UI.Compliance.cs
- **Line**: 624
- **Current CYC**: 17
- **Target CYC**: <=8 (Jane Street strict standard)
- **Reduction Required**: -9 (53% reduction)

## Scope Boundary Definition

### IN SCOPE

#### Primary Extraction Target
1. **HandleFleetTargetFill method body** (lines 624-697)
   - Nested conditional logic (depth 8 -> target <=3)
   - Order state validation blocks
   - Target fill processing logic
   - OCO order cancellation logic

#### Helper Method Extractions (Estimated 3-4 methods)
1. **ValidateFleetTargetState** (guard clauses)
   - Extract early-return conditions
   - Reduce nesting depth from 8 to <=3
   - Target CYC: <=3

2. **ProcessTargetFillUpdate** (core logic)
   - Extract target fill application logic
   - Group PositionInfo method calls (IsTargetFilled, GetTargetContracts, etc.)
   - Target CYC: <=5

3. **HandleOCOCancellation** (cancellation logic)
   - Extract OCO order cancellation block
   - Group CancelOrderOnAccount calls
   - Target CYC: <=3

4. **LogFleetTargetFillEvent** (logging)
   - Extract LogBuffer.Format calls
   - Consolidate logging logic
   - Target CYC: <=2

#### Refactoring Techniques
- **Guard Clauses**: Convert nested if/else to early returns
- **Extract Method**: Split into single-responsibility helpers
- **Group Related Calls**: Reduce 24 callees to <=10 per method

### OUT OF SCOPE

#### Caller Methods (No Changes)
1. **ProcessQueuedExecution_HandleFleetOCO** (line 698)
   - Caller remains unchanged
   - Only signature compatibility required

2. **ProcessQueuedExecution** (line 787)
   - Indirect caller remains unchanged
   - No modifications needed

#### Callee Methods (No Changes)
- **PositionInfo methods**: IsTargetFilled, GetTargetContracts, GetTargetFilledQuantity, SetTargetFilledQuantity, MarkTargetFilled
- **LogBuffer methods**: Format, ValidateThreadAffinity, FormatInternal
- **Order methods**: CancelOrderOnAccount, ApplyTargetFill, IsOrderTerminal
- **activePositions constant**

#### Other Files
- No cross-file changes
- Private method scope = localized refactoring only

#### Test Files
- Test creation is Phase 5 responsibility
- Out of scope for Phase 1-4

### BOUNDARY VALIDATION CRITERIA

#### Must Preserve
1. **Method Signature**: private void HandleFleetTargetFill(QueuedAccountExecution item, Order ocoOrder, Account ocoAcct, string ocoName)
2. **Caller Compatibility**: ProcessQueuedExecution_HandleFleetOCO must call without changes
3. **Behavioral Equivalence**: All extracted logic must produce identical results
4. **Lock-Free Pattern**: No lock() blocks introduced (V12 DNA mandate)
5. **ASCII-Only**: No Unicode characters (V12 DNA mandate)

#### Success Metrics
- **CYC Reduction**: 17 -> <=8 (target: 8 or below)
- **Nesting Reduction**: 8 -> <=3 (target: 3 or below)
- **Callee Reduction**: 24 -> <=10 per method (distributed across helpers)
- **Build Status**: Zero compilation errors
- **Behavioral Tests**: All existing tests pass (if any)

## Risk Mitigation

### Low Risk Factors
- **Blast Radius**: 0 external dependents
- **Visibility**: Private method (no breaking changes)
- **Callers**: Only 2 within same file
- **File Scope**: Single file modification

### Medium Risk Factors
- **Complexity**: High CYC (17) and nesting (8)
- **Coupling**: 24 callees (must preserve all calls)
- **Churn**: 12 commits in 90 days (active code)

### Mitigation Strategy
1. **Preserve All Callees**: No changes to called methods
2. **Maintain Signature**: No parameter changes
3. **Extract Incrementally**: One helper at a time
4. **Verify After Each**: Build + manual test after each extraction

## Jane Street Alignment

### Queried Patterns
- **Complexity Reduction**: Extract to CYC <=8
- **Fleet Order Processing**: Maintain order state integrity
- **Guard Clauses**: Early returns over nested conditionals

### Applied Principles
1. **Cognitive Simplicity**: Each method does one thing
2. **Testability**: Smaller methods = easier to test
3. **Maintainability**: Reduced nesting = easier to reason about

## Phase 1.5 Preview

### Boundary Validation Questions
1. Are all 4 helper methods truly independent?
2. Does signature preservation maintain caller compatibility?
3. Are there hidden dependencies in the 24 callees?
4. Does extraction preserve order processing semantics?

### Expected Validation Outcome
- **Boundary Clarity**: HIGH (private method, clear extraction points)
- **Scope Creep Risk**: LOW (no cross-file changes)
- **Approval Confidence**: HIGH (localized, low blast radius)

## Next Steps (Phase 2)
1. Generate architecture plan with extraction sequence
2. Define helper method signatures
3. Map callee distribution across helpers
4. Create Mermaid diagrams for before/after structure
5. Validate against V12 DNA mandates

## References
- **Hotspot Analysis**: docs/brain/EPIC-W7-004/00-hotspots.md
- **Jane Street KB**: Complexity reduction, fleet order processing
- **V12 DNA**: CYC <=8, lock-free Actor pattern, ASCII-only
- **CodeScene**: Check Code Health Score for V12_002.UI.Compliance.cs
