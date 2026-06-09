# EPIC-CCN-16 Scope Definition

## Epic Identifier
**EPIC-CCN-16**: Extract HydrateFSMsFromWorkingOrders (CYC 45 → ≤8)

## Target Method
- **Name**: `HydrateFSMsFromWorkingOrders`
- **File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:464)
- **Lines**: 464-759 (295 lines)
- **Current Complexity**: CYC 45 (3x Jane Street threshold)
- **Target Complexity**: CYC ≤8 (82.2% reduction)

## What IS In Scope

### 1. Primary Extraction Target
Extract the `HydrateFSMsFromWorkingOrders` method into smaller, focused methods:
- **State Mapping Logic**: Map broker OrderState to FollowerBracketState (lines 488-503)
- **Position Quantity Resolution**: Resolve remaining contracts from live positions (lines 505-518)
- **FSM Construction**: Build FollowerBracketFSM instances (lines 520-528)
- **Order Linking**: Link stop and target orders to FSM (lines 530-588)
- **FSM Registration**: Add FSM to tracking dictionaries (lines 590-598)
- **Position Pass Logic**: Handle accounts with open positions but terminal entry orders (lines 602-743)

### 2. Complexity Reduction Strategy
- Break down the 295-line method into 6-8 focused methods
- Each extracted method: CYC ≤8, single responsibility
- Preserve exact behavior (zero functional changes)
- Maintain idempotency guarantees
- Keep actor-serialization safety

### 3. V12 DNA Compliance
- **Zero Locks**: No `lock()` statements (already compliant)
- **ASCII-Only**: All string literals ASCII (already compliant)
- **Correctness by Construction**: Preserve FSM state machine invariants
- **Jane Street Alignment**: Target CYC ≤8 per method

### 4. Testing Requirements
- **TDD Mandate**: Write tests BEFORE extraction
- **Test Coverage**: 100% of extracted methods
- **Invariant Validation**: FSM state transitions, order linking, idempotency
- **Regression Prevention**: Verify no behavior changes

## What is OUT of Scope

### 1. Pre-Existing Issues (V12.23 "ONE EPIC = ONE CONCERN")
- ❌ Other SIMA methods (AdoptFleetOrders, HydrateWorkingOrdersFromBroker, etc.)
- ❌ Pre-existing compilation errors in other files
- ❌ Unrelated code quality issues
- ❌ Performance optimizations beyond complexity reduction
- ❌ Architectural changes to FSM design

### 2. Adjacent Code
- ❌ Caller method `HydrateWorkingOrdersFromBroker` (line 445)
- ❌ Helper methods (IsFleetAccount, GetStableHash, etc.)
- ❌ Data structures (FollowerBracketFSM, PositionInfo, etc.)
- ❌ Dictionary fields (_followerBrackets, _orderIdToFsmKey, etc.)

### 3. Infrastructure
- ❌ Build system changes
- ❌ CI/CD pipeline modifications
- ❌ Documentation updates (except this epic's artifacts)
- ❌ Tooling configuration

### 4. Scope Creep Violations
If ANY of the following are discovered during execution:
1. **STOP immediately**
2. **Document in failure-analysis.md**
3. **Report to Director**
4. **Create separate PR for unrelated fixes**

Examples of scope creep:
- "While we're here, let's fix this other method"
- "This pre-existing error needs fixing first"
- "Let's improve the FSM design"
- "We should refactor the caller too"

## Success Criteria

### Quantitative Metrics
- ✅ HydrateFSMsFromWorkingOrders: CYC ≤8 (currently 45)
- ✅ All extracted methods: CYC ≤8
- ✅ Zero new compilation errors
- ✅ Zero new runtime errors
- ✅ Test coverage: 100% of extracted methods
- ✅ Build passes: `powershell -File .\scripts\build_readiness.ps1`

### Qualitative Metrics
- ✅ Code is more readable (single-responsibility methods)
- ✅ Easier to test (focused unit tests)
- ✅ Easier to maintain (clear separation of concerns)
- ✅ Preserves exact behavior (zero functional changes)
- ✅ Maintains idempotency guarantees
- ✅ Preserves actor-serialization safety

### V12 DNA Compliance
- ✅ Zero `lock()` statements
- ✅ ASCII-only string literals
- ✅ Correctness by construction (FSM invariants preserved)
- ✅ Jane Street alignment (CYC ≤8)

### Deployment Verification
- ✅ `deploy-sync.ps1` executes successfully
- ✅ F5 in NinjaTrader successful
- ✅ No runtime errors in production

## Risk Assessment

### High Risk Areas
1. **FSM State Mapping**: Lines 488-503 (complex conditional logic)
2. **Position Pass Logic**: Lines 602-743 (nested loops, multiple conditions)
3. **Order Linking**: Lines 530-588 (repetitive code, 5 target orders)

### Mitigation Strategies
1. **TDD First**: Write tests before extraction
2. **Incremental Extraction**: One method at a time
3. **Verification Gates**: Build + test after each extraction
4. **Rollback Plan**: Git worktree isolation for easy rollback

## Dependencies

### Input Artifacts
- [`docs/brain/EPIC-CCN-16/00-hotspots.md`](docs/brain/EPIC-CCN-16/00-hotspots.md)
- [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:464)

### Output Artifacts (This Phase)
- This document: `00-scope.md`

### Next Phase Dependencies
- Phase 1.5 requires: `00-scope.md`
- Phase 2 requires: `00-scope.md`, `01-pattern-analysis.md`

## Precedent

### EPIC-CCN-15 Success
- **Before**: CYC 67
- **After**: CYC 4
- **Reduction**: 94%
- **Outcome**: Zero regressions, clean deployment

### EPIC-CCN-16 Target
- **Before**: CYC 45
- **After**: CYC ≤8
- **Reduction**: 82.2%
- **Confidence**: HIGH (proven pattern)

## Approval Status
- **Phase 0**: ✅ Complete (Hotspot Analysis)
- **Phase 1**: ✅ Complete (This Document)
- **Phase 1.5**: ⏳ Pending (Scope Boundary Validation)

## Notes
- YOLO MODE: Full autonomous execution approved
- No permission requests needed except F5 verification gates
- Work on `gitbutler/workspace` branch only
- Manifest-based workflow (update after each phase)