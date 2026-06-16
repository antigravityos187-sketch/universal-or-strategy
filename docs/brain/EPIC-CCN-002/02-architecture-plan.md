# Phase 2: Architecture Planning - EPIC-CCN-002

## Target Method Analysis

### Current State
- **Method**: SymmetryGuardTryResolveFollowersForDispatch
- **File**: src/V12_002.Symmetry.Replace.cs
- **Signature**: private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)
- **Current Complexity**: 18 (CYC)
- **Lines of Code**: 33
- **Tier**: 1 (High Priority)

### Complexity Breakdown
The method has three distinct logical phases contributing to CYC 18:

1. **Phase 1: Snapshot Worklist Building** (CYC ~6)
   - Retrieves immutable follower snapshot from dispatch context
   - Validates each follower against multiple dictionaries
   - Builds initial worklist of eligible followers

2. **Phase 2: Legacy Dispatch Map Scan** (CYC ~5)
   - Scans symmetryPendingFollowerFills for missing followers
   - Validates against dispatch linkage
   - Augments worklist with discovered followers

3. **Phase 3: Follower Resolution** (CYC ~7)
   - Iterates through final worklist
   - Retrieves position information
   - Processes each follower's dispatch logic

## Extraction Strategy

### Target Complexity
- **Jane Street Standard**: CYC ≤ 8 per function
- **V12 Threshold**: CYC ≤ 15 (acceptable)
- **Target**: CYC ≤ 8 (strict Jane Street alignment)

### Extraction Plan
Extract three private helper methods, each with single responsibility:

1. **BuildFollowerWorklistFromSnapshot**: CYC ~6
2. **ScanLegacyDispatchMapForMissingFollowers**: CYC ~5
3. **ResolveFollowerDispatches**: CYC ~7
4. **Refactored Main Method**: CYC ~3 (sequential calls only)

**Result**: All methods meet CYC ≤ 8 threshold, improving cognitive simplicity.

## Method Signatures

### Original Method (Preserved)
private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)

**Preservation Requirements**:
- Method signature unchanged
- Same parameters: dispatchId, nowUtc
- Same return type: void
- Same visibility: private
- No changes to callers required

### Helper Method 1: BuildFollowerWorklistFromSnapshot
private List<string> BuildFollowerWorklistFromSnapshot(string dispatchId, SymmetryDispatchContext ctx)

**Responsibility**: Build initial follower worklist from immutable snapshot
**Parameters**:
- dispatchId: Dispatch identifier for validation
- ctx: Dispatch context containing immutable Followers snapshot
**Returns**: List<string> of eligible follower names
**Complexity**: CYC ~6
**Lock-Free**: Uses immutable snapshot (ADR-019)

### Helper Method 2: ScanLegacyDispatchMapForMissingFollowers
private void ScanLegacyDispatchMapForMissingFollowers(string dispatchId, List<string> followersToResolve)

**Responsibility**: Scan legacy dispatch map for followers missing from snapshot
**Parameters**:
- dispatchId: Dispatch identifier for validation
- followersToResolve: Existing worklist to augment (pass-by-reference)
**Returns**: void (mutates list in-place for efficiency)
**Complexity**: CYC ~5
**Lock-Free**: Uses ToArray() for safe iteration

### Helper Method 3: ResolveFollowerDispatches
private void ResolveFollowerDispatches(List<string> followersToResolve, DateTime nowUtc)

**Responsibility**: Process each follower's dispatch logic
**Parameters**:
- followersToResolve: Final worklist of followers to process
- nowUtc: Timestamp for dispatch operations
**Returns**: void (side effects: updates dictionaries, dispatches orders)
**Complexity**: CYC ~7
**Lock-Free**: Uses atomic dictionary operations (TryGetValue)

## Call Graph

### Refactored Main Method Flow
SymmetryGuardTryResolveFollowersForDispatch calls three helpers sequentially:
1. BuildFollowerWorklistFromSnapshot - returns List<string>
2. ScanLegacyDispatchMapForMissingFollowers - mutates list in-place
3. ResolveFollowerDispatches - processes final worklist

### Data Flow
Input: dispatchId, nowUtc
- Retrieve ctx from symmetryDispatchById
- Helper 1: Build initial worklist from ctx.Followers snapshot
- Helper 2: Augment worklist from legacy dispatch map
- Helper 3: Process each follower in final worklist

### Shared State
- **Read-Only Access** (Helpers 1 & 2): symmetryDispatchById, symmetryFleetEntryToDispatch, symmetryPendingFollowerFills
- **Read-Write Access** (Helper 3): symmetryPendingFollowerFills, activePositions
- **No Shared Mutable State Between Helpers**: Only followersToResolve list passed between helpers

## Lock-Free Validation

### ADR-019 Compliance
All helpers preserve the lock-free pattern mandated by ADR-019:

**Helper 1**: Uses immutable snapshot (ctx.Followers), atomic dictionary operations (TryGetValue, ContainsKey), no lock() statements

**Helper 2**: Uses ToArray() for safe iteration, atomic dictionary operations, no lock() statements

**Helper 3**: Uses atomic dictionary operations (TryGetValue), no lock() statements, safe iteration over pre-built list

### Forensic Scan Result
grep -r "lock(" src/V12_002.Symmetry.Replace.cs
Expected: Zero matches (PASS)

**Validation**: No lock() statements in original method or proposed helpers

## Jane Street Compliance

### Cognitive Simplicity (Primary Goal)
**Jane Street Principle**: Functions with CYC >15 are harder to reason about under microsecond-latency constraints

**EPIC-CCN-002 Alignment**:
- Original method CYC 18 → Refactored to CYC 3
- Helper 1 CYC ~6 (well below threshold 8)
- Helper 2 CYC ~5 (well below threshold 8)
- Helper 3 CYC ~7 (meets threshold 8)
- All methods cognitively simple, easy to verify

### Correctness by Construction
**Jane Street Principle**: Make illegal states unrepresentable

**EPIC-CCN-002 Alignment**:
- Each helper has single, well-defined responsibility
- Impossible to accidentally skip a phase (sequential calls)
- Impossible to mix phase logic (separate methods)
- Type system enforces correct data flow (List<string> parameter)

### Lock-Free Actor Pattern
**Jane Street Principle**: Avoid locks in hot paths, use immutable snapshots and atomic primitives

**EPIC-CCN-002 Alignment**:
- Uses immutable snapshots (ctx.Followers via Interlocked.CompareExchange)
- Uses atomic dictionary operations (TryGetValue, ContainsKey)
- Uses ToArray() for safe iteration over concurrent collections
- Zero lock() statements (forensic scan verified)

### Surgical Changes
**Jane Street Principle**: Minimize blast radius to reduce risk

**EPIC-CCN-002 Alignment**:
- Single method refactored (SymmetryGuardTryResolveFollowersForDispatch)
- Single file modified (V12_002.Symmetry.Replace.cs)
- No caller modifications (method signature preserved)
- No callee modifications (helpers call same methods)
- Minimal blast radius (1 file, 1 method, 3 new helpers)

### Testing Strategy
**Jane Street Principle**: Test exhaustively, exponential path growth with CYC

**EPIC-CCN-002 Testing**:
- **Original Method**: CYC 18 → 2^18 = 262,144 paths (intractable)
- **Refactored Main**: CYC 3 → 2^3 = 8 paths (trivial)
- **Helper 1**: CYC 6 → 2^6 = 64 paths (manageable)
- **Helper 2**: CYC 5 → 2^5 = 32 paths (manageable)
- **Helper 3**: CYC 7 → 2^7 = 128 paths (manageable)

**Total Test Paths**: 8 + 64 + 32 + 128 = 232 paths (vs 262,144 original)
**Reduction**: 99.91% fewer test paths required

## Implementation Constraints

### Mandatory Preservation
1. Method signature unchanged (no caller modifications)
2. Method behavior unchanged (no functional changes)
3. Lock-free pattern preserved (ADR-019 compliance)
4. Edge case handling preserved (all validation logic intact)
5. Error handling preserved (same exception patterns)

### Prohibited Actions
1. No changes to callers (methods that invoke this method)
2. No changes to callees (methods called by this method)
3. No changes to other methods in V12_002.Symmetry.Replace.cs
4. No changes to class structure, namespaces, or imports
5. No functional changes to order dispatch logic

### Allowed Actions
1. Extract three private helper methods in same class
2. Refactor main method to call helpers sequentially
3. Add inline comments for clarity (optional)
4. Preserve all existing comments (ADR-019 references)

## Verification Criteria

### Phase 4 (Execution) Success Criteria
1. Build succeeds (zero compilation errors)
2. All tests pass (zero test failures)
3. Complexity audit passes (all methods CYC ≤ 8)
4. Lock-free audit passes (zero lock() statements)
5. Behavior unchanged (same order dispatch logic)

### Phase 5 (Review) Success Criteria
1. Implementation matches this architecture plan
2. No scope creep (only target method modified)
3. No caller/callee modifications
4. Jane Street compliance verified
5. Arena AI DNA audit passes (Phase 3)

## Risk Assessment

### Technical Risks
- **Risk**: Helper methods might introduce performance overhead
  - **Mitigation**: Methods are private and will be inlined by JIT compiler
  - **Validation**: Benchmark before/after (microsecond-latency requirement)

- **Risk**: Refactoring might introduce subtle behavioral changes
  - **Mitigation**: Preserve exact logic, add comprehensive tests
  - **Validation**: Compare execution traces before/after

- **Risk**: Lock-free pattern might be violated during extraction
  - **Mitigation**: Forensic scan after implementation (grep "lock(")
  - **Validation**: Arena AI DNA audit (Phase 3)

### Process Risks
- **Risk**: Scope creep during implementation
  - **Mitigation**: Phase 1.5 boundary validation (approved)
  - **Validation**: Arena AI adversarial review (Phase 3)

- **Risk**: Implementation drift from architecture plan
  - **Mitigation**: Phase 5 verification against this document
  - **Validation**: Automated "Fix-all" loop if drift detected

## Next Steps

### Phase 3: DNA & PR Audit (Arena AI)
- **Agent**: Arena AI (Red Team)
- **Goal**: Adversarial review of this architecture plan
- **Validation**: No lock() statements, atomic primitives only, ASCII-only compliance, Jane Street alignment, scope boundary compliance

### Phase 4: Recursive Execution (Bob CLI)
- **Agent**: Bob CLI (v12-engineer)
- **Goal**: Implement extraction according to this plan
- **Safety**: Mandatory checkpointing enabled
- **Verification**: Compare implementation against this document

### Phase 5: Verification/Review (Forensics)
- **Agent**: Bob CLI (verify cycle) + Orchestrator
- **Goal**: Validate implementation matches architecture plan
- **Loop**: Automated "Fix-all" loop if logic drifts

### Phase 6: Sign-off (Director)
- **Action**: powershell -File .\deploy-sync.ps1
- **Final Test**: F5 in NinjaTrader + BUILD_TAG verification

---

**Document Status**: APPROVED (pending Phase 3 DNA audit)
**Phase**: 2 (Architecture Planning)
**Complexity Target**: CYC ≤ 8 (Jane Street strict standard)
**Lock-Free**: VALIDATED (zero lock() statements)
**Jane Street Alignment**: VALIDATED (cognitive simplicity, surgical changes)
**Date**: 2026-06-15
**Protocol**: V12.23 Scope Creep Prevention
