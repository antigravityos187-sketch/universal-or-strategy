# Phase 5 Execution Protocol (V12.34)

**Version**: 1.0  
**Effective**: 2026-06-16  
**Status**: MANDATORY for all Phase 5 epic executions  
**Supersedes**: Implicit Phase 5 execution guidelines

## Purpose

This protocol defines the **SURGICAL ONLY** mandate for Phase 5 (Ticket Execution) to prevent over-optimization and scope creep that caused Wave 4 failures.

## Core Principle: SURGICAL ONLY

**ONE TICKET = ONE METHOD = ONE CONCERN**

Phase 5 execution MUST be surgical: touch ONLY the target method specified in the ticket, nothing else.

## Mandatory Rules

### 1. Target Method Only
- ✅ **ALLOWED**: Modify ONLY the target method specified in the ticket
- ❌ **FORBIDDEN**: Touch any other method, even if it "needs fixing"
- ❌ **FORBIDDEN**: Fix pre-existing compilation errors
- ❌ **FORBIDDEN**: Add "while we're here" improvements
- ❌ **FORBIDDEN**: Refactor adjacent code

### 2. Pre-Existing Error Handling
If compilation errors exist BEFORE starting the ticket:
1. **STOP immediately**
2. Report to Director: "Pre-existing compilation errors detected"
3. **DO NOT** attempt to fix them
4. **DO NOT** proceed with ticket execution
5. Wait for Director approval to continue

### 3. Scope Verification
Before making ANY changes:
1. Read the ticket scope definition
2. Identify the EXACT target method (name + file + line range)
3. Verify no other methods are mentioned
4. If scope is ambiguous, ask for clarification

### 4. Change Verification
After making changes:
1. Run `git diff` to verify ONLY target method was modified
2. If other methods appear in diff, **REVERT immediately**
3. Document the scope violation
4. Re-execute with correct scope

### 5. No Adjacent Improvements
Common violations to AVOID:
- ❌ Fixing nearby typos
- ❌ Adding missing braces to other methods
- ❌ Reformatting adjacent code
- ❌ Extracting shared logic from other methods
- ❌ Updating comments in other methods
- ❌ Renaming variables in other methods

## Execution Checklist

### Pre-Execution
- [ ] Read ticket scope definition
- [ ] Identify target method (name, file, line range)
- [ ] Verify codebase compiles cleanly
- [ ] Confirm no pre-existing errors in target file
- [ ] Review Jane Street KB for extraction patterns

### During Execution
- [ ] Modify ONLY target method
- [ ] Use building-blocks templates for extraction
- [ ] Follow Jane Street patterns (CYC ≤8)
- [ ] Generate xUnit tests (NEVER NUnit/MSTest)
- [ ] Verify encoding (UTF-8 without BOM)

### Post-Execution
- [ ] Run `git diff` - verify ONLY target method changed
- [ ] Run `dotnet build` - verify compilation passes
- [ ] Run `dotnet csharpier format src/` - verify formatting
- [ ] Run `python scripts/complexity_audit.py` - verify CYC ≤8
- [ ] Document completion in ticket file

## Violation Protocol

If scope violation detected:
1. **STOP immediately**
2. Run `git checkout -- .` to revert all changes
3. Document violation in `docs/brain/EPIC-X/scope-violation.md`
4. Report to Director with root cause analysis
5. Re-execute ticket with correct scope

## Success Criteria

A Phase 5 execution is successful ONLY if:
1. ✅ Target method complexity reduced to ≤8
2. ✅ ONLY target method appears in `git diff`
3. ✅ Build passes (`dotnet build`)
4. ✅ xUnit tests generated and passing
5. ✅ Encoding verified (UTF-8 without BOM)

## Examples

### ✅ CORRECT: Surgical Extraction
**Ticket**: Extract validation logic from `OnSubmitClick` (lines 250-275)

**Changes**:
- Modified `OnSubmitClick` (lines 250-275) → reduced to orchestrator
- Added `ValidateInputs` helper method
- Added xUnit tests for `ValidateInputs`

**Git Diff**: Shows ONLY `OnSubmitClick` and new `ValidateInputs` method

### ❌ WRONG: Over-Optimization
**Ticket**: Extract validation logic from `OnSubmitClick` (lines 250-275)

**Changes**:
- Modified `OnSubmitClick` (lines 250-275) → reduced to orchestrator
- Added `ValidateInputs` helper method
- **VIOLATION**: Fixed typo in `OnCancelClick` (line 300)
- **VIOLATION**: Added missing braces to `OnResetClick` (line 350)
- **VIOLATION**: Reformatted `BuildCommandString` (lines 400-450)

**Git Diff**: Shows 4 methods changed (scope violation)

## Integration with Other Protocols

- **Phase 1.5 (Scope Boundary)**: Validates single-method scope BEFORE Phase 5
- **Phase 5.V (Verification)**: Validates surgical execution AFTER Phase 5
- **Recovery Loop Protocol**: Handles failed executions with scope violations
- **Building-Blocks Method**: Provides templates for surgical extraction

## References

- **Root Cause**: Wave 4 PR #10-16 failures (28 issues, 9 P0)
- **Analysis**: `WAVE4_FULL_PR_AUDIT.md`
- **Hardening Plan**: `WAVE4_PROTOCOL_HARDENING_PLAN.md`
- **Jane Street KB**: `docs/intel/jane-street/RULES_CATALOG.md`

## Version History

- **V1.0** (2026-06-16): Initial protocol based on Wave 4 root cause analysis