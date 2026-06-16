# Phase 1.0: Scope Definition - EPIC-CCN-008

## Epic Metadata
- Epic ID: EPIC-CCN-008
- Phase: 1.0 (Scope Definition)
- Date: 2026-06-15
- Protocol Version: V12.23

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: UpdateTargetVisibility
- File: src/V12_002.UI.Panel.Handlers.cs
- Current Complexity: 19 (Cyclomatic Complexity)
- V12 Threshold: 15 (Jane Street aligned)
- Violation Severity: MEDIUM (+4 over threshold)

### Target Complexity
- Primary Goal: Reduce from CYC 19 to 8 or less (Jane Street strict standard)
- Per-Method Target: 10 or less for extracted helpers
- Orchestration Target: 7 or less for main method after extraction

### Extraction Strategy
Break UpdateTargetVisibility into 2-3 helper methods plus orchestration:

1. State Validation Helper (CYC approximately 3)
   - Validate panel state before proceeding
   - Check chart object availability
   - Early return on invalid conditions
   - Pure validation logic, no side effects

2. Drawing Operations Helper (CYC approximately 5)
   - Isolate chart object creation/removal
   - Separate line drawing from label drawing
   - Encapsulate drawing helper calls
   - Atomic drawing operations

3. UI Sync Helper (CYC approximately 4)
   - Update button states
   - Sync checkbox states
   - Handle control enable/disable
   - UI consistency enforcement

4. Core Orchestration (CYC approximately 7)
   - Coordinate extracted methods
   - High-level flow control
   - Maintain backward compatibility
   - Single responsibility: orchestration only

## Boundary Definition

### IN SCOPE (What We WILL Change)
- UpdateTargetVisibility method body ONLY
- Extract 2-3 private helper methods within same class
- Refactor conditional logic to reduce complexity
- Maintain exact same external behavior
- Preserve all existing method signatures

### OUT OF SCOPE (What We WILL NOT Change)
- Callers of UpdateTargetVisibility (UI event handlers, initialization)
- Callees (Chart API, drawing helpers, state utilities)
- Other methods in V12_002.UI.Panel.Handlers.cs
- Method signature or access modifiers
- External interfaces or contracts
- Pre-existing compilation errors in other files
- While we are here improvements to adjacent code

### No Scope Creep Mandate
- ONE EPIC = ONE CONCERN: Single-method extraction only
- No Bundling: Do not combine with other refactoring tasks
- No Side Quests: Do not fix unrelated issues discovered during extraction
- Surgical Precision: Touch only what is necessary for complexity reduction

## Success Criteria

### Functional Requirements
- Complexity reduced from 19 to 8 or less in main method
- All extracted helpers have CYC 10 or less
- Zero behavior changes (bit-for-bit identical output)
- All existing tests pass without modification
- No new compilation errors introduced

### V12 DNA Compliance
- Lock-free Actor/FSM pattern maintained (no lock statements)
- ASCII-only compliance verified (no Unicode in string literals)
- Correctness by construction (type-safe state representation)
- Atomic operations preserved (no race conditions)

### Quality Gates
- CSharpier formatting passes
- Roslyn analyzer violations: 0
- Pre-push validation passes (all 13 checks)
- CodeScene Code Health Score improves or maintains
- Codacy complexity check passes (CYC 15 or less)

### Testing Requirements
- Existing unit tests pass (FSMActorTests.cs)
- Manual F5 test in NinjaTrader (UI verification)
- No regression in target visibility toggle behavior
- Chart rendering remains consistent

## Risk Assessment

### Risk Level: MEDIUM
- Rationale: Complexity exceeds threshold but isolated to UI layer
- Mitigation: No impact on trading logic or order execution
- Reversibility: Changes are reversible (UI state only)
- Boundaries: Well-defined within panel handlers

### Blast Radius
- Direct Impact: UI rendering, state consistency, event handling
- Indirect Impact: Drawing layer modifications
- No Impact: Trading engine, order execution, market data processing

## Verification Plan

### Pre-Extraction
1. Run complexity audit to baseline current state
2. Verify no lock statements in target file
3. Check ASCII compliance with pre-push validation

### Post-Extraction
1. Verify complexity reduction with audit script
2. Run full test suite
3. Manual UI test: F5 in NinjaTrader, toggle target visibility
4. Run pre-push validation
5. Sync hard links with deploy-sync script

## Phase 1.0 Approval

Status: READY FOR PHASE 1.5 (Boundary Validation)

Rationale:
- Single-method extraction clearly defined
- Complexity reduction strategy documented
- Success criteria measurable and achievable
- V12 DNA compliance requirements explicit
- Risk level acceptable (MEDIUM, UI-only impact)

Next Step: Proceed to Phase 1.5 (Boundary Validation) per V12.23 Protocol
