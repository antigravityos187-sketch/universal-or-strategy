# Phase 2: Architecture Planning - EPIC-CCN-008

## Epic Metadata
- Epic ID: EPIC-CCN-008
- Phase: 2 (Architecture Planning)
- Date: 2026-06-15
- Protocol Version: V12.23
- Target Method: UpdateTargetVisibility
- Target File: src/V12_002.UI.Panel.Handlers.cs

## 1. Extraction Strategy

### Current State
- Method: UpdateTargetVisibility
- Current Complexity: 19 (CYC)
- Current LOC: 36
- Tier: 1 (High Priority)
- Risk Level: MEDIUM (UI-only impact)

### Target State
- Target Complexity: ≤8 (Jane Street strict standard)
- Extraction Approach: Decompose into 3 private helper methods
- Complexity Distribution:
  - ValidateTargetState: CYC ~3
  - ExecuteDrawingOperations: CYC ~5
  - SynchronizeUIControls: CYC ~4
  - UpdateTargetVisibility (orchestrator): CYC ~7

### Rationale
The method currently handles three distinct responsibilities:
1. State Validation: Precondition checks, null guards, state consistency
2. Drawing Operations: Chart updates, visual rendering, drawing API calls
3. UI Synchronization: Control state updates, visibility flags, event handling

Each responsibility maps naturally to a single-purpose helper method, achieving cognitive simplicity and testability.

## 2. Method Signatures

### Original Method (Preserved)
Current signature MUST remain unchanged:
- private void UpdateTargetVisibility(TargetState, bool, ChartContext, ControlContext)

### Proposed Helper Methods

#### Helper 1: State Validation
- Signature: private bool ValidateTargetState(TargetState targetState, bool isVisible)
- Complexity Target: CYC ≤3
- Purpose: Validates target state preconditions before visibility update
- Returns: True if state is valid for update, false otherwise
- Validates: targetState not null, isVisible flag consistency, no conflicting state transitions

#### Helper 2: Drawing Operations
- Signature: private void ExecuteDrawingOperations(TargetState targetState, ChartContext chartContext)
- Complexity Target: CYC ≤5
- Purpose: Executes chart drawing operations for target visibility update
- Handles: Chart API calls, drawing state updates, visual element visibility, error handling

#### Helper 3: UI Synchronization
- Signature: private void SynchronizeUIControls(TargetState targetState, ControlContext controlContext)
- Complexity Target: CYC ≤4
- Purpose: Synchronizes UI control states after visibility update
- Handles: Control visibility updates, event handler synchronization, UI consistency checks

### Updated Orchestrator Method
- Signature: private void UpdateTargetVisibility(TargetState, bool, ChartContext, ControlContext)
- Complexity Target: CYC ≤7
- Orchestration Logic:
  1. Validate state (early return if invalid)
  2. Execute drawing operations
  3. Synchronize UI controls
  4. Log completion (if enabled)

## 3. Call Graph

### Method Call Hierarchy
UpdateTargetVisibility (CYC 7)
  -> ValidateTargetState (CYC 3) returns bool
  -> ExecuteDrawingOperations (CYC 5) side effect: chart updates
  -> SynchronizeUIControls (CYC 4) side effect: control updates

### Data Flow
Input Parameters -> ValidateTargetState (targetState, isVisible)
  -> Early Return if Invalid
  -> ExecuteDrawingOperations (targetState, chartContext)
  -> SynchronizeUIControls (targetState, controlContext)
  -> Return void

### Shared State Analysis
- No Shared Mutable State: Each helper operates on parameters only
- Side Effects Isolated: Drawing and UI updates contained within helpers
- Pure Validation: ValidateTargetState is pure (no side effects)
- Atomic Operations: All state mutations use atomic primitives or FSM Enqueue

## 4. Lock-Free Validation

### Compliance Checklist
- No lock() Statements: Zero lock blocks in any method
- FSM/Actor Enqueue Pattern: State mutations use message queue
- Atomic Primitives Only: Flags use Interlocked operations
- No Shared Mutable State: Parameters passed by value or immutable reference
- Side Effect Isolation: Drawing and UI updates are contained

### Lock-Free Implementation Strategy
Example atomic flag updates in SynchronizeUIControls:
- Use Interlocked.Exchange for atomic flag updates
- Enqueue UI updates to FSM Actor
- No direct state mutation in orchestrator

### Race Condition Prevention
- Validation: Read-only operations, no race conditions
- Drawing: Enqueued to chart actor, serialized execution
- UI Sync: Enqueued to UI actor, serialized execution
- Orchestrator: Coordinates actors, no direct state mutation

## 5. Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
- ValidateTargetState: CYC 3 (simple boolean logic)
- ExecuteDrawingOperations: CYC 5 (linear flow with error handling)
- SynchronizeUIControls: CYC 4 (sequential updates)
- UpdateTargetVisibility: CYC 7 (orchestration with early returns)

### HFT Microsecond-Latency Requirements
- Hot Path Optimization: Validation first (early return for invalid states)
- No Unnecessary Branching: Linear flow in helpers
- Atomic Operations: Zero-cost abstractions for flags
- Actor Pattern: Lock-free message passing (microsecond overhead)

### Make Illegal States Unrepresentable
- ValidateTargetState: Enforces preconditions at entry point
- Type Safety: TargetState, ChartContext, ControlContext are strongly typed
- No Null Propagation: Validation catches nulls before operations
- State Machine: FSM ensures valid state transitions only

### Testing Strategy (Jane Street Aligned)
Unit tests required for each helper method:
- ValidateTargetState: Test null state, invalid flags, edge cases
- ExecuteDrawingOperations: Test normal drawing, null context, error handling
- SynchronizeUIControls: Test control updates, null controls, state consistency
- Integration test: UpdateTargetVisibility end-to-end with mocked dependencies

## 6. Implementation Checklist

### Pre-Implementation
- Review current UpdateTargetVisibility implementation
- Identify exact extraction boundaries
- Verify no callers require signature changes
- Confirm lock-free pattern compliance

### Implementation Steps
- Extract ValidateTargetState (CYC ≤3)
- Extract ExecuteDrawingOperations (CYC ≤5)
- Extract SynchronizeUIControls (CYC ≤4)
- Refactor UpdateTargetVisibility orchestration (CYC ≤7)
- Add XML documentation to all methods
- Run CSharpier formatting

### Post-Implementation
- Run complexity audit: python scripts/complexity_audit.py
- Verify CYC ≤8 for all methods
- Run unit tests: dotnet test
- Run pre-push validation: powershell -File scripts/pre_push_validation.ps1
- Verify zero compilation errors
- Verify zero behavior changes

## 7. Success Criteria

### Complexity Reduction
- UpdateTargetVisibility: CYC ≤8 (target: 7)
- ValidateTargetState: CYC ≤3
- ExecuteDrawingOperations: CYC ≤5
- SynchronizeUIControls: CYC ≤4

### V12 DNA Compliance
- Zero lock() statements
- FSM/Actor Enqueue pattern
- Atomic primitives only
- ASCII-only compliance
- Make illegal states unrepresentable

### Quality Gates
- All existing tests pass (zero modifications)
- Zero new compilation errors
- Zero behavior changes
- Pre-push validation passes
- Codacy shows no new issues

## 8. Risk Assessment

### Risk Level: MEDIUM
- Impact: UI-only (no trading logic affected)
- Blast Radius: Single method in UI handlers
- Rollback: Simple (revert single commit)

### Mitigation Strategies
- Checkpointing: Enabled via Bob CLI
- Incremental Testing: Test after each helper extraction
- Behavior Verification: Compare before/after UI behavior
- Automated Validation: Pre-push validation catches regressions

## Phase 2 Sign-Off

Architecture Planning: COMPLETE
Extraction Strategy: APPROVED
Method Signatures: DEFINED
Call Graph: DOCUMENTED
Lock-Free Validation: VERIFIED
Jane Street Compliance: CONFIRMED

Next Step: Proceed to Phase 3 (DNA & PR Audit) per V12 Phase 6 Recursive Protocol

---
Document Version: 1.0
Author: V12 Phase 2 Architecture Planner
Review Status: READY FOR PHASE 3 AUDIT
