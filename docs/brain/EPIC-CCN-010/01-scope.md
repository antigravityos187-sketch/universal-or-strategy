# Phase 1.0: Scope Definition - EPIC-CCN-010

## Epic Metadata
- **Epic ID**: EPIC-CCN-010
- **Phase**: 1.0 (Scope Definition)
- **Date**: 2026-06-15
- **Analyst**: Bob Shell (v12-engineer)

## Target Method

### Method Identification
- **Method Name**: ShowModeSpecificControls
- **File**: src/V12_002.UI.Panel.Handlers.cs
- **Current Complexity**: 20 (CYC)
- **V12 Threshold**: 15 (Jane Street aligned)
- **Violation Severity**: 33% over threshold

### Complexity Analysis
- **Decision Points**: ~20 conditional branches
- **Cognitive Load**: HIGH (20 decision points = difficult to reason about)
- **Test Path Explosion**: 2^20 = 1,048,576 possible execution paths
- **Maintainability Risk**: HIGH (bug introduction during modifications)

## Extraction Scope (SINGLE METHOD ONLY)

### What's IN Scope
1. **Method Body**: ShowModeSpecificControls implementation only
2. **Decision Logic**: Extract conditional chains into helper methods
3. **Control Visibility**: Isolate mode-specific UI control logic
4. **Target Complexity**: Reduce from CYC=20 to CYC≤8 (Jane Street strict standard)

### Extraction Strategy
**Approach**: Break into 2-3 focused helper methods

1. **Helper Method 1**: ShowAutomationControls()
   - Purpose: Handle automation mode UI controls
   - Estimated CYC: 5-7

2. **Helper Method 2**: ShowManualControls()
   - Purpose: Handle manual mode UI controls
   - Estimated CYC: 5-7

3. **Helper Method 3**: ShowCommonControls()
   - Purpose: Handle shared/common UI controls
   - Estimated CYC: 3-5

4. **Orchestrator**: ShowModeSpecificControls (refactored)
   - Purpose: Delegate to helper methods based on mode
   - Target CYC: ≤8

### What's OUT of Scope
1. **Callers**: No changes to methods that call ShowModeSpecificControls
2. **Callees**: No changes to control visibility setters or state validators
3. **Other Methods**: No modifications to other methods in V12_002.UI.Panel.Handlers.cs
4. **Pre-existing Issues**: No fixing unrelated compilation errors or warnings
5. **Scope Creep**: No "while we're here" improvements

## Boundary Constraints

### V12 DNA Compliance
- Lock-Free: Maintain Actor/FSM pattern (no lock() blocks)
- ASCII-Only: No Unicode, emoji, or curly quotes
- Atomic State: Preserve atomic state transitions
- Correctness by Construction: Make illegal states unrepresentable

### Jane Street Alignment
- Cognitive Simplicity: Target CYC≤8 per method
- Single Responsibility: Each helper method has one clear purpose
- Testability: Extracted methods are independently testable
- Microsecond Latency: No performance degradation from extraction

## Success Criteria

### Functional Requirements
1. Complexity Reduction: CYC reduced from 20 to ≤8
2. Behavior Preservation: Zero functional changes (pure refactoring)
3. Test Coverage: All existing tests pass
4. UI Stability: No visual regressions in panel rendering

### Technical Requirements
1. Build Success: dotnet build passes with zero errors
2. Lint Clean: powershell -File .\scripts\lint.ps1 passes
3. Format Clean: dotnet csharpier check src/ passes
4. Complexity Audit: python scripts/complexity_audit.py shows CYC≤8

### Process Requirements
1. Hard-Link Sync: powershell -File .\deploy-sync.ps1 after changes
2. Pre-Push Validation: All 13 checks pass
3. PR Hygiene: Diff <10k characters (source code only)
4. Boundary Validation: Phase 1.5 approval obtained

## Risk Assessment

### Extraction Risk: MEDIUM
- **Rationale**: UI control logic with 20 decision points
- **Mitigation**: Incremental extraction with test verification after each step
- **Rollback**: Git checkpointing enabled via Bob CLI

### Impact Analysis
- **Affected Components**: Panel rendering, mode switching, control visibility
- **Blast Radius**: CONTAINED (single method, no caller/callee changes)
- **Regression Risk**: LOW (pure refactoring, no behavior changes)

## Next Steps (Phase 2)

1. **Phase 1.5**: Boundary validation (MANDATORY per V12.23 Protocol)
2. **Phase 2**: Architecture planning (implementation plan + Mermaid diagrams)
3. **Phase 3**: DNA & PR audit (Arena AI red team review)
4. **Phase 4**: Recursive execution (Bob CLI surgical extraction)

---
**Phase 1.0 Status**: COMPLETED
**Approval Required**: Phase 1.5 (Boundary Validation)
**Generated**: 2026-06-15T03:28:16Z
**Analyst**: Bob Shell (v12-engineer)
