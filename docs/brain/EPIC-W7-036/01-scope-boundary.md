# Phase 1: Scope Boundary - EPIC-W7-036

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: N/A
- **Execution Time**: 2026-06-24T01:30:50Z

## Epic Metadata
- **Epic ID**: EPIC-W7-036
- **Target Method**: MoveStop_SinglePosition
- **File**: src/V12_002.Trailing.Breakeven.cs
- **Current CYC**: 21
- **Target CYC**: <=8 per extracted method
- **Lines of Code**: 91

## Scope Definition

### IN SCOPE

#### Primary Extraction Target
**Method**: MoveStop_SinglePosition (CYC 21, 91 LOC)
- **Location**: src/V12_002.Trailing.Breakeven.cs:73
- **Justification**: Exceeds Jane Street threshold (CYC 8) by 13 points
- **Blast Radius**: MINIMAL (0 external importers, 1 internal caller)
- **Risk Level**: MEDIUM-HIGH (high complexity, low churn)

#### Extraction Strategy
Based on the 26 callees and 5-level nesting depth, extract the following logical units:

1. **Stop Order Validation Logic**
   - Validate stop price preconditions
   - Check for stale pending replacements
   - Estimated CYC reduction: 4-6 points

2. **Pending Replacement Management**
   - Handle existing pending replacements
   - Update or create new replacements
   - Estimated CYC reduction: 5-7 points

3. **Stop Order Creation/Update**
   - Initiate stop replacement
   - Create direct stop orders
   - Update existing stop orders
   - Estimated CYC reduction: 4-6 points

4. **Error Handling & Logging**
   - Exception handling logic
   - Log buffer formatting
   - Estimated CYC reduction: 2-3 points

#### Expected Outcome
- **Target**: 3-5 extracted methods, each with CYC <=8
- **Total CYC Reduction**: From 21 to <=8 in orchestrator method
- **Behavior Preservation**: Pure extraction, zero logic changes
- **Test Coverage**: Unit tests for each extracted method

### OUT OF SCOPE

#### Methods NOT Targeted for Extraction
1. **MoveStopsToBreakevenWithOffset** (caller method)
   - **Reason**: Separate epic if needed
   - **Current CYC**: Unknown (requires separate analysis)

2. **Callee Methods** (26 methods called by target)
   - **Reason**: Already extracted/modular
   - **Examples**: UpdateStopOrder, ValidateStopPrice, InitiateStopReplacement
   - **Note**: These are dependencies, not extraction targets

3. **Other Methods in V12_002.Trailing.Breakeven.cs**
   - **Reason**: Not part of this epic scope
   - **Note**: May be addressed in future epics

#### Architectural Changes NOT Included
1. **FSM/Actor Pattern Migration**
   - **Reason**: Beyond scope of complexity reduction
   - **Note**: Preserve existing architecture

2. **Lock-Free Refactoring**
   - **Reason**: No locks detected in target method
   - **Note**: Already compliant with V12 DNA

3. **API Signature Changes**
   - **Reason**: Preserve existing method signature
   - **Note**: Internal extractions only

4. **Cross-File Refactoring**
   - **Reason**: Single-file scope (V12_002.Trailing.Breakeven.cs)
   - **Note**: No changes to callee methods in other files

### Scope Boundary Validation

#### Jane Street Alignment
- Target CYC <=8 (Jane Street strict standard)
- Cognitive Simplicity: Extract to single-responsibility methods
- Test Coverage: TDD approach for each extraction
- Behavior Preservation: Pure refactoring, no logic changes

#### V12 DNA Compliance
- ASCII-Only: No Unicode/emoji in extracted methods
- Lock-Free: No new locks introduced
- Correctness by Construction: Preserve existing type safety
- Hard-Link Integrity: deploy-sync.ps1 after changes

#### Risk Mitigation
- Low Blast Radius: 0 external importers, 1 internal caller
- Stable Method: Low churn (not in top 50 hotspots)
- Contained Impact: Single-file refactoring
- Reversible: Git history preserves original implementation

### Success Criteria

#### Phase 2 (Architecture Planning) Prerequisites
1. Scope boundary defined (IN SCOPE vs OUT OF SCOPE)
2. Extraction targets identified (3-5 sub-methods)
3. CYC reduction estimates provided
4. Risk assessment completed
5. Jane Street alignment verified

#### Phase 5 (Ticket Execution) Exit Criteria
1. All extracted methods have CYC <=8
2. Original method (orchestrator) has CYC <=8
3. Unit tests pass for all extracted methods
4. F5 in NinjaTrader succeeds (BUILD_TAG verification)
5. deploy-sync.ps1 completes successfully
6. No new compilation errors introduced

### Exclusions & Constraints

#### Explicit Exclusions
- No changes to method signature (preserve 4 parameters)
- No changes to callee methods (UpdateStopOrder, ValidateStopPrice, etc.)
- No changes to caller method (MoveStopsToBreakevenWithOffset)
- No cross-file refactoring
- No architectural pattern changes (FSM/Actor migration)

#### Constraints
- Must maintain backward compatibility
- Must preserve existing behavior (pure extraction)
- Must pass all existing tests (if any)
- Must not introduce new dependencies
- Must follow V12 DNA mandates (ASCII, lock-free, CYC <=8)

## Next Steps (Phase 2: Architecture Planning)
1. Read full source of MoveStop_SinglePosition (91 lines)
2. Map decision points to CYC contributors (identify if/else/switch/loops)
3. Design extraction boundaries (3-5 sub-methods)
4. Create method signatures for extracted methods
5. Estimate CYC per extracted method
6. Generate Mermaid diagrams (before/after call hierarchy)
7. Define test cases for each extracted method
