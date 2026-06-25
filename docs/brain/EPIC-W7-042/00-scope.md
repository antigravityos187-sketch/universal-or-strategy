# Phase 1: Scope Definition - EPIC-W7-042

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:28:57Z

## Epic Metadata
- **Epic ID**: EPIC-W7-042
- **Target Method**: SymmetryGuardOnFollowerFill
- **File**: src/V12_002.Symmetry.Follower.cs
- **Current CYC**: 16
- **Target CYC**: <=8 per extracted method

## Scope Boundary Validation

### IN SCOPE

#### Primary Target
- **Method**: SymmetryGuardOnFollowerFill (lines 17-89)
  - Current CYC: 16
  - Current Nesting: 6
  - Current Lines: 72
  - Callees: 60 methods

#### Extraction Candidates (Based on Hotspot Analysis)

1. **Guard Validation Logic** (Priority: HIGH)
   - Precondition checks (nesting levels 1-2)
   - Early return conditions
   - Target CYC: <=3

2. **Follower Resolution Logic** (Priority: HIGH)
   - Calls to SymmetryGuardTryResolveFollower
   - Follower lookup and validation
   - Target CYC: <=5

3. **Order Submission Logic** (Priority: MEDIUM)
   - Calls to SymmetryGuardSubmitFollowerBracket
   - Bracket order creation and submission
   - Target CYC: <=6

4. **State Management Logic** (Priority: MEDIUM)
   - Updates to symmetryPendingFollowerFills
   - State mutation tracking
   - Target CYC: <=4

#### Refactoring Constraints
- Maintain exact behavioral equivalence
- Preserve all logging statements
- Keep error handling intact
- No changes to method signature
- No changes to external contracts (zero external callers)

### OUT OF SCOPE

#### Explicitly Excluded

1. **External Methods** (Not Modifying)
   - SymmetryGuardApplyMasterAnchor (called by target)
   - SymmetryGuardSubmitFollowerBracket (called by target)
   - SymmetryGuardTryResolveFollower (called by target)
   - All 60 callee methods remain unchanged

2. **Data Structures** (Not Modifying)
   - symmetryFleetEntryToDispatch (constant)
   - symmetryDispatchById (constant)
   - symmetryPendingFollowerFills (state tracking)
   - No changes to field declarations

3. **Other Methods in File** (Not Touching)
   - Other methods in V12_002.Symmetry.Follower.cs
   - No scope creep to adjacent methods

4. **Test Files** (Separate Epic)
   - Unit test creation is Phase 5 responsibility
   - Not part of extraction scope

5. **Documentation** (Minimal Updates Only)
   - No comprehensive documentation rewrite
   - Only update XML comments for extracted methods

## Blast Radius Analysis

### Impact Assessment: MINIMAL
- **External Callers**: 0 (isolated method)
- **Import Propagation**: None required
- **Breaking Changes**: Zero risk
- **Affected Files**: 1 (src/V12_002.Symmetry.Follower.cs only)

### Safety Guarantees
- No external dependencies to update
- No import graph changes needed
- No cross-file refactoring required
- Safe to refactor without breaking contracts

## Extraction Strategy

### Approach: Vertical Slice Extraction
Extract methods in order of independence:

1. **Extract Guard Validation** (Independent)
   - No dependencies on other extractions
   - Reduces nesting immediately
   - Target: ValidateFollowerFillGuards()

2. **Extract Follower Resolution** (Independent)
   - No dependencies on guard extraction
   - Simplifies control flow
   - Target: ResolveFollowerForFill()

3. **Extract Order Submission** (Depends on #2)
   - Uses resolved follower from #2
   - Isolates side effects
   - Target: SubmitFollowerBracketOrder()

4. **Extract State Management** (Depends on #3)
   - Updates state after submission
   - Makes mutations explicit
   - Target: TrackPendingFollowerFill()

### Success Criteria Per Extraction
- CYC <=8 for each extracted method
- Nesting <=3 for each extracted method
- Lines <=30 for each extracted method
- Callees <=10 for each extracted method
- Zero compilation errors
- Zero behavioral changes

## Risk Mitigation

### Complexity Risk: HIGH to MEDIUM
- **Before**: CYC 16, Nesting 6, 72 lines
- **After**: 4-5 methods, each CYC <=8, Nesting <=3
- **Mitigation**: Sequential extraction with verification

### Blast Radius Risk: LOW (Unchanged)
- Zero external callers = isolated refactoring
- No propagation risk

### Maintenance Risk: HIGH to LOW
- **Before**: 2^16 = 65,536 code paths
- **After**: ~4 methods x 2^8 = ~1,024 paths per method
- **Mitigation**: Smaller, testable units

## Scope Boundary Enforcement

### Phase 1.5 Gate (Mandatory)
Before proceeding to Phase 2, verify:
- No scope creep to external methods
- No data structure modifications
- No changes to other file methods
- Extraction count <=5 methods
- All extractions target CYC <=8

### Rejection Criteria
Phase 2 will be REJECTED if:
- Scope expands beyond SymmetryGuardOnFollowerFill
- External method signatures change
- Data structures are modified
- More than 5 extractions proposed

## Next Phase
Proceed to Phase 1.5 (Scope Boundary Validation) to verify:
1. No scope creep detected
2. Extraction boundaries are clear
3. Dependencies are mapped correctly
4. Risk assessment is accurate
