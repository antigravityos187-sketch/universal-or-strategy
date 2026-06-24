# Phase 1: Scope Definition - EPIC-W7-119

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: 0.18
- API Key: N/A
- Execution Time: 2026-06-24T01:35:54Z

## Epic Metadata
- Epic ID: EPIC-W7-119
- Target Method: GetFsmExpectedPosition
- File: src/V12_002.Symmetry.BracketFSM.cs
- Line: 422
- Current CYC: 14
- Target CYC: ≤8

## Scope Boundary Definition

### IN SCOPE
1. **Primary Target**: GetFsmExpectedPosition method (CYC 14 → ≤8)
   - Extract conditional logic into helper methods
   - Reduce nesting depth from 4 to ≤2
   - Maintain single responsibility principle

2. **Extraction Candidates**:
   - FSM state validation logic
   - Position calculation logic
   - Bracket symmetry checks
   - Error handling paths

3. **Quality Gates**:
   - All extracted methods must have CYC ≤8
   - Maintain method isolation (zero external dependencies)
   - Preserve existing behavior (no logic changes)
   - Add XML documentation to extracted methods

### OUT OF SCOPE
1. **Explicitly Excluded**:
   - Other methods in V12_002.Symmetry.BracketFSM.cs
   - Caller/callee modifications (method is isolated)
   - FSM state machine architecture changes
   - Performance optimizations beyond complexity reduction
   - Test file modifications (no existing tests for this method)

2. **Deferred to Future Epics**:
   - Adding unit tests (requires separate test infrastructure epic)
   - Refactoring other high-CYC methods in same file
   - FSM pattern consolidation across codebase

### Scope Validation
- **Blast Radius**: ZERO (isolated method, no external callers)
- **Risk Level**: LOW-MEDIUM (high complexity, zero dependencies)
- **Refactoring Safety**: VERY HIGH
- **Estimated Tickets**: 1-2 (single method extraction)

### Success Criteria
1. GetFsmExpectedPosition CYC reduced from 14 to ≤8
2. All extracted methods have CYC ≤8
3. Zero compilation errors
4. Zero behavior changes (logic preservation)
5. Build passes after deploy-sync.ps1
6. F5 in NinjaTrader successful

## Phase 1 Completion
Status: SCOPE DEFINED
Scope Validated: 2026-06-24T01:35:54Z
Ready for Phase 2: YES
