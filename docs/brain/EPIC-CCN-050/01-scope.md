# Phase 1.0: Scope Definition - EPIC-CCN-050

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: FleetSync_SyncFollowersToLevel
- File: src/V12_002.Trailing.cs
- Line: 142
- Current Complexity: 9
- Target Complexity: 8 or less (Jane Street strict standard)

### Extraction Strategy
Break FleetSync_SyncFollowersToLevel into 2-3 helper methods to reduce cyclomatic complexity from 9 to 8 or less.

Approach:
1. Identify conditional branches and loops within the method
2. Extract logical units into private helper methods
3. Maintain single responsibility per extracted method
4. Preserve lock-free Actor/FSM pattern

Expected Decomposition:
- Main method: Orchestration logic (CYC 3 or less)
- Helper 1: Follower validation/filtering (CYC 3 or less)
- Helper 2: Level synchronization logic (CYC 3 or less)

## Boundary Definition

### IN SCOPE
- ONLY the method body of FleetSync_SyncFollowersToLevel
- Internal logic extraction into private helper methods
- Complexity reduction from 9 to 8 or less
- Maintaining existing behavior (zero functional changes)

### OUT OF SCOPE
- Callers of FleetSync_SyncFollowersToLevel (line 115)
- Callees invoked by FleetSync_SyncFollowersToLevel
- Other methods in V12_002.Trailing.cs
- Pre-existing compilation errors or warnings
- Performance optimizations beyond complexity reduction
- Refactoring of related fleet sync methods

### No Scope Creep Rule
ONE EPIC = ONE CONCERN: This epic addresses ONLY the complexity of FleetSync_SyncFollowersToLevel. No "while we're here" improvements.

## Success Criteria

### Functional Requirements
- Complexity reduced from 9 to 8 or less
- All existing tests pass (zero test failures)
- No behavior changes (bit-for-bit identical output)
- Lock-free Actor/FSM pattern maintained

### Architectural Requirements
- Extracted helpers follow V12 DNA principles
- ASCII-only compliance (no Unicode/emoji)
- Correctness by construction (illegal states unrepresentable)
- Single responsibility per extracted method

### Quality Gates
- CSharpier formatting passes
- Roslyn analyzer passes (zero violations)
- Pre-push validation passes (all 13 checks)
- Codacy shows "Up to quality standards"

### Verification Protocol
1. Run dotnet build - zero errors
2. Run dotnet test - 100% pass rate
3. Run dotnet csharpier check src/ - zero issues
4. Run powershell -File .\scripts\complexity_audit.py - verify CYC 8 or less
5. Run powershell -File .\scripts\pre_push_validation.ps1 -Fast

## Risk Assessment

### Complexity Risk: LOW
- Current CYC=9 is below threshold (15)
- Preventive refactoring (not emergency fix)
- Small delta (9 to 8) reduces extraction risk

### Blast Radius Risk: LOW
- Single caller identified (line 115)
- Localized to trailing position management
- No cross-file dependencies detected

### Jane Street Alignment: HIGH
- Cognitive simplicity prioritized
- Single-method extraction (no bundling)
- Maintains microsecond-latency constraints

## Rationale

Why refactor at CYC=9?
- Jane Street strict standard: 8 or less for cognitive simplicity
- Preventive maintenance (avoid future complexity creep)
- Easier to test exhaustively (fewer code paths)
- Simpler to audit for race conditions in lock-free code

Why single-method scope?
- V12.23 Protocol: Mandatory boundary validation
- Reduces PR diff size (less than 10k characters)
- Enables focused code review
- Prevents scope creep and bundled concerns
