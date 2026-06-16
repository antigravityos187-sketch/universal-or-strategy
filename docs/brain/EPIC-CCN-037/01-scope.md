# Phase 1.0: Scope Definition - EPIC-CCN-037

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**:
- **Method Name**: SymmetryNormalizeTradeType
- **File**: src/V12_002.Symmetry.Replace.cs
- **Current Complexity**: 10
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Method Signature**: private static TradeType SymmetryNormalizeTradeType(TradeType tradeType, bool isSymmetryEnabled)

## Extraction Strategy

**Approach**: Break into 2-3 helper methods

**Proposed Decomposition**:
1. **Primary Method** (SymmetryNormalizeTradeType): Orchestration logic only (CYC ≤3)
   - Validate inputs
   - Route to appropriate helper
   - Return normalized result

2. **Helper Method 1** (ValidateTradeTypeForSymmetry): Input validation (CYC ≤2)
   - Check for null/invalid trade types
   - Validate symmetry flag consistency

3. **Helper Method 2** (ApplySymmetryNormalization): Core normalization logic (CYC ≤5)
   - Apply symmetry-specific transformations
   - Handle conditional branches for trade type conversions

**Complexity Reduction**:
- Current: 10 decision points in single method
- Target: 3 (orchestration) + 2 (validation) + 5 (normalization) = 10 total, but distributed
- Each method: ≤8 (Jane Street threshold)

## Boundary Definition

### IN SCOPE
- SymmetryNormalizeTradeType method body only
- Extract conditional logic into helper methods
- Maintain exact same behavior (no logic changes)
- Preserve method signature (private static)
- Keep all existing parameters

### OUT OF SCOPE
- Callers: No changes to methods calling SymmetryNormalizeTradeType
- Callees: No changes to methods called by SymmetryNormalizeTradeType
- Other methods: No changes to other methods in V12_002.Symmetry.Replace.cs
- File structure: No changes to class structure or namespace
- Pre-existing issues: No fixing unrelated compilation errors
- Scope creep: No "while we're here" improvements

### NO SCOPE CREEP
- ONE EPIC = ONE CONCERN: Single-method extraction only
- No bundling: Do not combine with other refactoring tasks
- No opportunistic fixes: Ignore unrelated code smells
- No architectural changes: Maintain existing patterns

## Success Criteria

### Functional Requirements
1. Complexity Reduced: From 10 to ≤8 per method
2. All Tests Pass: Zero test failures after extraction
3. No Behavior Changes: Exact same output for all inputs
4. Lock-Free Pattern: Maintain Actor/FSM pattern (no locks)

### Quality Gates
1. ASCII-Only: No Unicode characters introduced
2. Build Success: Zero compilation errors
3. Lint Clean: Zero new Roslyn warnings
4. CSharpier Formatted: Auto-formatted with braces

### Verification Steps
1. Run complexity audit: python scripts/complexity_audit.py
2. Run build: powershell -File .\scripts\build_readiness.ps1
3. Run tests: dotnet test
4. Verify lock-free: grep -r "lock(" src/V12_002.Symmetry.Replace.cs (expect zero matches)

## Risk Assessment

**Overall Risk**: LOW

**Rationale**:
- Private static method (limited blast radius)
- Clear single responsibility (trade type normalization)
- Complexity below threshold (10 < 15)
- No lock-free violations detected
- ASCII-only compliance verified

**Mitigation**:
- Extract to helpers with clear names
- Add unit tests for each helper
- Verify behavior with existing test suite
- Use checkpointing for safe rollback

## Phase 1.0 Completion

- Scope defined (single method only)
- Boundary validated (no scope creep)
- Success criteria established
- Risk assessed (LOW)
- **Status**: READY FOR PHASE 1.5 (Boundary Validation)
