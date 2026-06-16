# Phase 1.0: Scope Definition - EPIC-CCN-058

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: HydrateFSM_MapOrderStateToFsmState
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line Range**: 948-965 (18 lines)
- **Current Complexity**: 9
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Convert if-chain to switch expression (C# 8.0+)

### Complexity Analysis
Current structure (CYC=9):
- Base: 1
- If statement 1 (Filled/PartFilled): +1
- If statement 2 (Accepted): +1
- If statement 3 (Working/Submitted/Initialized/ChangePending/ChangeSubmitted): +5 (1 + 4 OR conditions)
- Default return: included in base
- **Total**: 9

Target structure (CYC≤8):
- Switch expression with pattern matching reduces branching complexity
- Each case arm counts as +1 instead of compound OR conditions
- Expected CYC: 6-7 (base + 5-6 cases)

### Boundary Definition

#### IN SCOPE (SINGLE METHOD ONLY)
- Method body of HydrateFSM_MapOrderStateToFsmState (lines 948-965)
- Refactor if-chain to switch expression
- Maintain exact same mapping logic
- Preserve return type: FollowerBracketState

#### OUT OF SCOPE (NO CHANGES)
- Caller at line 1249 (hydration logic)
- Other methods in V12_002.SIMA.Lifecycle.cs
- OrderState enum definition
- FollowerBracketState enum definition
- Any other files in src/

#### NO SCOPE CREEP
- No "while we are here" improvements to adjacent code
- No fixing pre-existing compilation errors elsewhere
- No bundling multiple concerns
- ONE EPIC = ONE CONCERN (single method extraction)

### Success Criteria

#### Functional Requirements
1. Complexity reduced from 9 to ≤8
2. All existing tests pass (no behavior changes)
3. Exact same mapping logic preserved
4. Return type unchanged: FollowerBracketState

#### Architectural Requirements (V12 DNA)
1. Lock-free Actor/FSM pattern maintained (pure function, no state mutation)
2. ASCII-only compliance (no Unicode in string literals)
3. Correctness by construction (enum mapping with exhaustive cases)
4. No new dependencies introduced

#### Quality Gates
1. Build passes: dotnet build src/V12_002.csproj
2. Tests pass: dotnet test tests/V12_Performance.Tests/
3. Complexity audit: python3 scripts/complexity_audit.py shows CYC ≤8
4. Hard-link sync: powershell -File deploy-sync.ps1 succeeds

### Risk Assessment
- **Blast Radius**: MINIMAL (1 caller, internal scope)
- **Behavioral Risk**: NONE (pure refactor, no logic changes)
- **Jane Street Risk**: NONE (0 P0 violations in original)
- **Overall Risk**: LOW

### Extraction Strategy Details

Switch expression refactor will reduce complexity by treating OR patterns as single case arms instead of multiple branching conditions.

### Jane Street Alignment
- **Cognitive Simplicity**: Switch expression is more readable than if-chain
- **Exhaustive Matching**: Default case ensures all paths covered
- **No Side Effects**: Pure function, no state mutation
- **Microsecond Latency**: No performance impact (compiler optimizes both equally)

### Verification Plan
1. Run complexity audit before extraction (baseline CYC=9)
2. Apply switch expression refactor
3. Run complexity audit after extraction (target CYC≤8)
4. Run full test suite
5. Verify hard-link sync
6. Compare behavior: no changes to caller or runtime behavior

---

**Phase 1.0 Status**: COMPLETE
**Next Phase**: 1.5 (Boundary Validation)
