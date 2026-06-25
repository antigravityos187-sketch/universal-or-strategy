# Phase 1: Scope Definition - EPIC-W7-142

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:43:37Z

## Epic Metadata
- **Epic ID**: EPIC-W7-142
- **Target Method**: HandleChartClick_ConvertPrice
- **File**: src/V12_002.UI.Callbacks.cs
- **Line**: 272
- **Current CYC**: 12
- **Target CYC**: ≤8 (Jane Street strict standard)

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
- **Method**: HandleChartClick_ConvertPrice (line 272)
  - **Current Metrics**: CYC=12, Nesting=5, Lines=82
  - **Refactoring Goal**: Reduce CYC to ≤8, reduce nesting to ≤3
  - **Approach**: Extract nested conditional logic and price conversion logic

#### Extraction Candidates
Based on the 82-line method with CYC=12 and nesting depth 5, the following logic blocks are candidates for extraction:

1. **Price Conversion Logic**
   - Extract price-to-Y-coordinate conversion logic
   - Likely involves chart scale calculations
   - Estimated CYC reduction: 2-3 points

2. **Nested Conditional Branches**
   - Extract deeply nested if/else blocks (nesting depth 5)
   - Separate validation logic from business logic
   - Estimated CYC reduction: 3-4 points

3. **Chart Interaction Logic**
   - Extract chart click handling logic
   - Separate UI interaction from data processing
   - Estimated CYC reduction: 2-3 points

#### Signature Preservation
- **Current Parameters**: 4 parameters (reasonable, no change needed)
- **Return Type**: Preserve existing return type
- **Access Modifier**: Keep private (single caller: OnChartClick)

#### Testing Strategy
- **Unit Tests**: Add tests for extracted methods
- **Integration Test**: Verify OnChartClick still works correctly
- **Regression Test**: Verify chart click behavior unchanged

### OUT OF SCOPE

#### Caller Method
- **OnChartClick** (line 231)
  - Reason: Not the target of this epic
  - Note: Will be tested to ensure integration works

#### Callee Methods
- **LogBuffer.Format** (src/V12_002.Perf.LogBuffer.cs:28)
- **LogBuffer.ValidateThreadAffinity** (src/V12_002.Perf.LogBuffer.cs:119)
- **LogBuffer.FormatInternal** (src/V12_002.Perf.LogBuffer.cs:56)
  - Reason: These are dependencies, not refactoring targets
  - Note: Calls to these methods will be preserved

#### Other UI Callback Methods
- Any other methods in V12_002.UI.Callbacks.cs
  - Reason: Outside the scope of EPIC-W7-142
  - Note: May be addressed in future epics

#### Chart Infrastructure
- Chart rendering logic
- Chart scale calculations (unless directly embedded in target method)
  - Reason: Infrastructure concerns, not complexity hotspots

## Scope Validation

### Complexity Reduction Target
- **Current**: CYC=12, Nesting=5
- **Target**: CYC≤8, Nesting≤3
- **Strategy**: Extract 2-3 helper methods to reduce branching

### Blast Radius Confirmation
- **Risk Score**: 0.0 (LOW)
- **External Dependencies**: 0
- **Callers**: 1 (OnChartClick only)
- **Safety**: HIGH (private method, isolated)

### Jane Street Alignment
- **Threshold**: CYC≤8 (strict standard)
- **Current Gap**: 4 points over threshold
- **Cognitive Load**: HIGH to target MEDIUM
- **Testability**: MEDIUM to target HIGH

## Success Criteria

### Phase 1 Completion
- Scope boundary clearly defined (IN SCOPE vs OUT OF SCOPE)
- Extraction candidates identified
- Complexity reduction strategy documented
- Testing approach defined

### Epic Completion (Future Phases)
- CYC reduced from 12 to ≤8
- Nesting depth reduced from 5 to ≤3
- All extracted methods have unit tests
- OnChartClick integration verified
- Build passes, deploy-sync.ps1 successful

## Risk Mitigation

### Low Risk Factors
- Private method (no external API surface)
- Single caller (easy to verify)
- Zero external dependencies
- Isolated functionality

### Mitigation Strategies
1. **Preserve Behavior**: Extract logic without changing semantics
2. **Test Coverage**: Add unit tests before refactoring
3. **Incremental Approach**: Extract one method at a time
4. **Verification**: Test OnChartClick after each extraction

## Next Steps (Phase 2)
1. Analyze method source code in detail
2. Identify exact extraction points (line ranges)
3. Design extracted method signatures
4. Plan test cases for each extracted method
5. Create architecture plan with before/after diagrams
