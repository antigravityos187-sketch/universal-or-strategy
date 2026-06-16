# Phase 2: Architecture Planning - EPIC-CCN-054

## Method Analysis

### Target Method
- **Method**: SymmetryGuardTryResolveFollower
- **File**: src/V12_002.Symmetry.Follower.cs
- **Lines**: 129-212 (83 LOC)
- **Current Complexity**: 12 (CYC)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Tier**: 2 (Medium complexity)

### Current Method Signature
private bool SymmetryGuardTryResolveFollower(
    string fleetEntryName,
    PositionInfo pos,
    PendingFollowerFill pending,
    DateTime nowUtc
)

## Extraction Strategy

### Logical Sections Identified

The method has 4 distinct logical sections:

1. **Dispatch Context Lookup** (Lines 137-157)
   - Retrieves dispatch context from fleet entry name
   - Validates context exists
   - Handles timeout if context missing
   - **Complexity**: 3 branches

2. **Anchor Resolution Check** (Lines 159-177)
   - Reads atomic anchor snapshot
   - Checks if master anchor is resolved
   - Handles timeout if not resolved
   - **Complexity**: 2 branches

3. **Slippage Validation** (Lines 179-197)
   - Calculates slippage in ticks and USD
   - Validates against thresholds
   - Skips follower if breach detected
   - **Complexity**: 2 branches

4. **Anchor Application** (Lines 199-212)
   - Captures prior entry price
   - Applies master anchor to position
   - Checks if already anchored
   - **Complexity**: 2 branches

### Extraction Plan

**Strategy**: Extract sections 1, 2, and 3 into helper methods. Section 4 remains in main method.

**Rationale**:
- Sections 1-3 are self-contained validation steps
- Section 4 requires direct access to position state
- This reduces main method CYC from 12 to ~5

## Proposed Helper Methods

### Helper 1: TryGetDispatchContext

**Signature**:
private bool TryGetDispatchContext(
    string fleetEntryName,
    PositionInfo pos,
    PendingFollowerFill pending,
    DateTime nowUtc,
    out SymmetryDispatchContext ctx
)

**Responsibilities**:
- Lookup dispatch ID from fleet entry name
- Retrieve dispatch context by ID
- Validate context is not null
- Skip follower if timeout exceeded
- Return context via out parameter

**Complexity**: CYC = 3

### Helper 2: TryGetResolvedAnchor

**Signature**:
private bool TryGetResolvedAnchor(
    SymmetryDispatchContext ctx,
    string fleetEntryName,
    PositionInfo pos,
    PendingFollowerFill pending,
    DateTime nowUtc,
    out double masterAnchor
)

**Responsibilities**:
- Read atomic anchor snapshot (lock-free)
- Check if anchor is resolved
- Skip follower if timeout exceeded
- Return master anchor price via out parameter

**Complexity**: CYC = 2

### Helper 3: ValidateSlippage

**Signature**:
private bool ValidateSlippage(
    string fleetEntryName,
    PositionInfo pos,
    PendingFollowerFill pending,
    double masterAnchor
)

**Responsibilities**:
- Calculate slippage in points, ticks, and USD
- Compare against SymmetryMaxSlippageTicks threshold
- Compare against SymmetryMaxSlippageUsdPerContract threshold
- Skip follower if either threshold breached

**Complexity**: CYC = 2

## Refactored Method Structure

### Main Method (After Extraction)

The refactored method will have CYC = 5:
- 3 helper method calls (each with early return check)
- 2 remaining branches in anchor application section

**Total Complexity**: 5 + 3 + 2 + 2 = 12 (distributed across 4 methods)

## Call Graph

SymmetryGuardTryResolveFollower (CYC=5)
├── TryGetDispatchContext (CYC=3)
│   └── SymmetryGuardSkipFollower (existing)
├── TryGetResolvedAnchor (CYC=2)
│   └── SymmetryGuardSkipFollower (existing)
├── ValidateSlippage (CYC=2)
│   └── SymmetryGuardSkipFollower (existing)
└── SymmetryGuardApplyMasterAnchor (existing)

## Data Flow

### Shared State
- **Read-Only Fields**: tickSize, pointValue, SymmetryAnchorWait
- **Read-Only Dictionaries**: symmetryFleetEntryToDispatch, symmetryDispatchById
- **No Locks**: All state access is lock-free

### Parameter Flow
1. TryGetDispatchContext: Inputs → Output (ctx)
2. TryGetResolvedAnchor: Inputs → Output (masterAnchor)
3. ValidateSlippage: Inputs → Output (bool)
4. Main Method: Applies anchor and checks optimization

### Return Value Semantics
- **False**: Follower is waiting
- **True**: Follower was processed

## Lock-Free Validation

### Compliance Checklist
- [x] No lock() statements
- [x] Atomic snapshots (ADR-019)
- [x] Immutable reads
- [x] FSM/Actor pattern

### Lock-Free Guarantees
1. TryGetDispatchContext: Thread-safe dictionary reads
2. TryGetResolvedAnchor: Atomic snapshot reads
3. ValidateSlippage: Pure calculation
4. Main Method: Calls existing lock-free helpers

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
- **Before**: CYC = 12
- **After**: CYC = 5 (main) + 3 + 2 + 2
- **Result**: All methods ≤8

### Microsecond-Latency Alignment
- No additional allocations (out parameters)
- Inline candidates (small private methods)
- Hot-path optimization preserved

### Testing Principles
- Testability: Independent unit tests
- Exhaustive coverage: Reduced CYC
- Isolation: Clear contracts

### Jane Street KB Insights
- Document: "Why Testing Is Hard and How to Fix It"
- Principle: "Make illegal states unrepresentable"
- Application: Single-responsibility helpers

## Risk Assessment

### Blast Radius
- **Scope**: Single method extraction
- **Impact**: LOW
- **Rollback**: Simple

### Behavioral Preservation
- **Logic**: Zero functional changes
- **Tests**: Existing tests must pass
- **Validation**: Pre-push validation

### Performance Impact
- **Allocation**: Zero
- **Inlining**: High probability
- **Latency**: Negligible

## Implementation Checklist

### Phase 3: DNA & PR Audit
- [ ] Verify no lock() statements
- [ ] Verify ASCII-only compliance
- [ ] Verify CYC ≤8
- [ ] Verify zero functional changes

### Phase 4: Surgical Execution
- [ ] Extract TryGetDispatchContext
- [ ] Extract TryGetResolvedAnchor
- [ ] Extract ValidateSlippage
- [ ] Refactor main method
- [ ] Add XML documentation
- [ ] Run CSharpier formatting

### Phase 5: Verification
- [ ] Run complexity_audit.py
- [ ] Run pre_push_validation.ps1
- [ ] Run FSMActorTests.cs
- [ ] Compare implementation vs plan

### Phase 6: Sign-off
- [ ] Run deploy-sync.ps1
- [ ] F5 in NinjaTrader
- [ ] Verify BUILD_TAG

## Success Criteria

### Mandatory Requirements
1. CYC reduced from 12 to ≤8
2. Zero functional changes
3. No lock() statements
4. ASCII-only compliance
5. All tests pass
6. Pre-push validation passes

### Jane Street Alignment
1. Cognitive simplicity (CYC ≤8)
2. Microsecond-latency optimization
3. Testability (isolated helpers)
4. Single-responsibility principle

## Next Phase

**Phase 3**: DNA & PR Audit (Arena AI)
- **Agent**: Arena AI (Red Team)
- **Deliverable**: Adversarial review
- **Gate**: PASS/FAIL

---

**Phase 2 Status**: COMPLETE
**Architecture Plan**: APPROVED (pending Phase 3 audit)
**Ready for Phase 3**: YES
