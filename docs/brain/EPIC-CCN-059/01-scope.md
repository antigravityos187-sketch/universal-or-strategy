# Phase 1.0: Scope Definition - EPIC-CCN-059

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**: `AdoptMasterWorkingOrders`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Current Complexity**: 9 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

## Boundary Definition

### ✅ IN SCOPE
- **ONLY** the method body of `AdoptMasterWorkingOrders`
- Internal logic extraction into helper methods
- Complexity reduction from CYC=9 to CYC≤8
- Maintaining lock-free Actor/FSM pattern

### ❌ OUT OF SCOPE
- Callers of `AdoptMasterWorkingOrders` (no changes)
- Callees invoked by `AdoptMasterWorkingOrders` (no changes)
- Other methods in `V12_002.SIMA.Lifecycle.cs` (no changes)
- Pre-existing compilation errors (not our concern)
- "While we're here" improvements (scope creep)
- Bundling multiple concerns (ONE EPIC = ONE CONCERN)

## Success Criteria

1. **Complexity Reduction**: CYC reduced from 9 to ≤8
2. **Test Pass Rate**: 100% (all existing tests pass)
3. **Behavior Preservation**: Zero behavior changes (pure refactoring)
4. **Lock-Free Compliance**: Actor/FSM pattern maintained (no `lock()` blocks)
5. **ASCII-Only**: No Unicode/emoji in string literals
6. **Build Success**: Zero compilation errors introduced
7. **Diff Hygiene**: Changes isolated to target method only

## Extraction Strategy

### Approach: Conditional Logic Extraction
Given CYC=9, likely candidates for extraction:
1. **Helper Method 1**: Extract complex conditional branches
2. **Helper Method 2**: Extract validation logic
3. **Helper Method 3**: Extract state transition logic (if applicable)

### Verification Steps
1. Run `dotnet build` (zero errors)
2. Run `dotnet test` (100% pass)
3. Run `scripts/complexity_audit.py` (verify CYC≤8)
4. Run `grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs` (zero matches)
5. Run `powershell -File .\deploy-sync.ps1` (hard-link sync)

## Risk Assessment
- **Complexity Risk**: LOW (CYC=9, minimal reduction needed)
- **Blast Radius**: MINIMAL (single method, no caller changes)
- **Rollback Risk**: LOW (checkpointing enabled)

## Notes
- Method is already below V12 threshold (15) but above Jane Street strict standard (8)
- This is a preventive refactoring to align with Jane Street cognitive simplicity principles
- No jCodemunch blast radius analysis available (tools unavailable during Phase 0)
