# Extraction Tickets: EPIC-CCN-002

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 6 hours
- **Target Method**: SymmetryGuardTryResolveFollowersForDispatch
- **Current CYC**: 18
- **Target CYC**: 3 (main) + 6 + 5 + 7 (helpers) = All ≤ 8

## TICKET-1: Extract BuildFollowerWorklistFromSnapshot

### Scope
- **Current Method**: `SymmetryGuardTryResolveFollowersForDispatch`
- **Current CYC**: 18
- **Target CYC**: Helper CYC ~6
- **Extraction**: Phase 1 logic - Snapshot worklist building

### Implementation
1. Create new private method `BuildFollowerWorklistFromSnapshot(string dispatchId, SymmetryDispatchContext ctx)`
2. Extract logic that:
   - Retrieves immutable follower snapshot from dispatch context
   - Validates each follower against multiple dictionaries
   - Builds initial worklist of eligible followers
3. Return `List<string>` of eligible follower names
4. Update main method to call this helper and capture returned list

### Acceptance Criteria
- [ ] Helper method created with signature: `private List<string> BuildFollowerWorklistFromSnapshot(string dispatchId, SymmetryDispatchContext ctx)`
- [ ] Helper complexity CYC ≤ 8 (target ~6)
- [ ] Uses immutable snapshot (ADR-019 compliant)
- [ ] Uses atomic dictionary operations only (TryGetValue, ContainsKey)
- [ ] Zero lock() statements (forensic scan passes)
- [ ] All tests pass
- [ ] Build succeeds
- [ ] No behavioral changes to dispatch logic

### Dependencies
- None (first ticket)

### Verification Commands
```bash
# Complexity check
python scripts/complexity_audit.py

# Lock-free check
grep -r "lock(" src/V12_002.Symmetry.Replace.cs

# Build check
powershell -File .\scripts\build_readiness.ps1
```

---

## TICKET-2: Extract ScanLegacyDispatchMapForMissingFollowers

### Scope
- **Current Method**: `SymmetryGuardTryResolveFollowersForDispatch`
- **Current CYC**: Reduced from TICKET-1
- **Target CYC**: Helper CYC ~5
- **Extraction**: Phase 2 logic - Legacy dispatch map scanning

### Implementation
1. Create new private method `ScanLegacyDispatchMapForMissingFollowers(string dispatchId, List<string> followersToResolve)`
2. Extract logic that:
   - Scans symmetryPendingFollowerFills for missing followers
   - Validates against dispatch linkage
   - Augments worklist with discovered followers
3. Mutate list in-place (pass-by-reference for efficiency)
4. Update main method to call this helper after TICKET-1 helper

### Acceptance Criteria
- [ ] Helper method created with signature: `private void ScanLegacyDispatchMapForMissingFollowers(string dispatchId, List<string> followersToResolve)`
- [ ] Helper complexity CYC ≤ 8 (target ~5)
- [ ] Uses ToArray() for safe iteration over concurrent collections
- [ ] Uses atomic dictionary operations only
- [ ] Zero lock() statements (forensic scan passes)
- [ ] All tests pass
- [ ] Build succeeds
- [ ] No behavioral changes to dispatch logic

### Dependencies
- TICKET-1 must be completed first (builds on worklist from Helper 1)

### Verification Commands
```bash
# Complexity check
python scripts/complexity_audit.py

# Lock-free check
grep -r "lock(" src/V12_002.Symmetry.Replace.cs

# Build check
powershell -File .\scripts\build_readiness.ps1
```

---

## TICKET-3: Extract ResolveFollowerDispatches

### Scope
- **Current Method**: `SymmetryGuardTryResolveFollowersForDispatch`
- **Current CYC**: Reduced from TICKET-2
- **Target CYC**: Helper CYC ~7
- **Extraction**: Phase 3 logic - Follower resolution

### Implementation
1. Create new private method `ResolveFollowerDispatches(List<string> followersToResolve, DateTime nowUtc)`
2. Extract logic that:
   - Iterates through final worklist
   - Retrieves position information
   - Processes each follower's dispatch logic
3. Side effects: updates dictionaries, dispatches orders
4. Update main method to call this helper after TICKET-2 helper

### Acceptance Criteria
- [ ] Helper method created with signature: `private void ResolveFollowerDispatches(List<string> followersToResolve, DateTime nowUtc)`
- [ ] Helper complexity CYC ≤ 8 (target ~7)
- [ ] Uses atomic dictionary operations (TryGetValue)
- [ ] Safe iteration over pre-built list
- [ ] Zero lock() statements (forensic scan passes)
- [ ] All tests pass
- [ ] Build succeeds
- [ ] No behavioral changes to dispatch logic

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first (processes final worklist from Helper 2)

### Verification Commands
```bash
# Complexity check
python scripts/complexity_audit.py

# Lock-free check
grep -r "lock(" src/V12_002.Symmetry.Replace.cs

# Build check
powershell -File .\scripts\build_readiness.ps1
```

---

## TICKET-4: Refactor Main Method to Sequential Calls

### Scope
- **Current Method**: `SymmetryGuardTryResolveFollowersForDispatch`
- **Current CYC**: Reduced from TICKET-3
- **Target CYC**: 3 (sequential calls only)
- **Extraction**: Final refactor - main method becomes orchestrator

### Implementation
1. Refactor main method to contain only:
   - Retrieve ctx from symmetryDispatchById
   - Call Helper 1: `var followersToResolve = BuildFollowerWorklistFromSnapshot(dispatchId, ctx);`
   - Call Helper 2: `ScanLegacyDispatchMapForMissingFollowers(dispatchId, followersToResolve);`
   - Call Helper 3: `ResolveFollowerDispatches(followersToResolve, nowUtc);`
2. Preserve method signature (no caller modifications)
3. Preserve all ADR-019 comments
4. Remove all extracted logic (now in helpers)

### Acceptance Criteria
- [ ] Main method complexity CYC ≤ 8 (target ~3)
- [ ] Method signature unchanged: `private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)`
- [ ] Sequential call structure (Helper 1 → Helper 2 → Helper 3)
- [ ] All ADR-019 comments preserved
- [ ] Zero lock() statements (forensic scan passes)
- [ ] All tests pass (behavioral equivalence)
- [ ] Build succeeds
- [ ] No caller modifications required
- [ ] Performance benchmark passes (microsecond-latency maintained)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Verification Commands
```bash
# Complexity check (all methods should be CYC ≤ 8)
python scripts/complexity_audit.py

# Lock-free check
grep -r "lock(" src/V12_002.Symmetry.Replace.cs

# Full pre-push validation
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# Deploy sync (hard-link integrity)
powershell -File .\deploy-sync.ps1
```

---

## Final Verification Checklist

### Complexity Targets (Jane Street Alignment)
- [ ] Main method: CYC ≤ 8 (target 3)
- [ ] Helper 1 (BuildFollowerWorklistFromSnapshot): CYC ≤ 8 (target 6)
- [ ] Helper 2 (ScanLegacyDispatchMapForMissingFollowers): CYC ≤ 8 (target 5)
- [ ] Helper 3 (ResolveFollowerDispatches): CYC ≤ 8 (target 7)

### DNA Compliance
- [ ] Zero lock() statements (forensic scan: `grep -r "lock(" src/V12_002.Symmetry.Replace.cs`)
- [ ] Immutable snapshots used (ADR-019)
- [ ] Atomic dictionary operations only
- [ ] ASCII-only compliance (no Unicode)

### PR Hygiene
- [ ] Single file modified (V12_002.Symmetry.Replace.cs)
- [ ] Diff size < 10k characters
- [ ] No caller modifications
- [ ] No callee modifications
- [ ] No scope creep

### Testing
- [ ] All unit tests pass
- [ ] Integration tests pass
- [ ] Performance benchmark passes (microsecond-latency)
- [ ] Behavioral equivalence verified

### Build & Deploy
- [ ] Build succeeds (zero errors)
- [ ] CSharpier format passes
- [ ] Pre-push validation passes
- [ ] Deploy sync completes (hard-link integrity)
- [ ] F5 in NinjaTrader succeeds

---

## Execution Strategy

### Incremental Approach
1. **TICKET-1**: Extract first helper, verify build
2. **TICKET-2**: Extract second helper, verify build
3. **TICKET-3**: Extract third helper, verify build
4. **TICKET-4**: Refactor main method, full validation

### Checkpointing
- Enable mandatory checkpointing in Bob CLI session
- Restore point after each ticket completion
- Rollback capability if any ticket fails

### Testing Strategy
- **Original Method**: CYC 18 → 2^18 = 262,144 paths (intractable)
- **Refactored Main**: CYC 3 → 2^3 = 8 paths (trivial)
- **Helper 1**: CYC 6 → 2^6 = 64 paths (manageable)
- **Helper 2**: CYC 5 → 2^5 = 32 paths (manageable)
- **Helper 3**: CYC 7 → 2^7 = 128 paths (manageable)
- **Total Test Paths**: 232 (vs 262,144 original = 99.91% reduction)

### Risk Mitigation
- **Performance**: JIT inlining eliminates helper overhead
- **Behavioral Changes**: Preserve exact logic, comprehensive tests
- **Lock-Free Pattern**: Forensic scan after each ticket
- **Scope Creep**: Phase 5 verification against architecture plan

---

**Document Status**: READY FOR EXECUTION
**Phase**: 4 (Ticket Generation)
**Total Tickets**: 4
**Execution Order**: Sequential
**Estimated Effort**: 6 hours
**Next Phase**: Phase 5 (Ticket Execution)
**Protocol**: V12.23 Sovereign Agent Protocol
**Date**: 2026-06-15
