# Extraction Tickets: EPIC-CCN-008

## Overview
- **Epic ID**: EPIC-CCN-008
- **Method**: UpdateTargetVisibility
- **File**: src/V12_002.UI.Panel.Handlers.cs
- **Current Complexity**: 19 (CYC)
- **Target Complexity**: ≤8 (CYC)
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 4-6 hours

## Execution Strategy
Each ticket extracts one logical sub-unit from UpdateTargetVisibility. Tickets must be executed in order to maintain build stability and enable incremental testing.

---

## TICKET-1: Extract State Validation Logic

### Scope
- **Current Method**: `UpdateTargetVisibility`
- **Current CYC**: 19
- **Target CYC**: ≤3 (for extracted method)
- **Extraction**: State validation and precondition checks

### Implementation
1. Create new private method: `private bool ValidateTargetState(TargetState targetState, bool isVisible)`
2. Extract validation logic:
   - Null checks for targetState
   - isVisible flag consistency validation
   - State transition conflict detection
   - Early return conditions
3. Add XML documentation with purpose and return value
4. Update UpdateTargetVisibility to call ValidateTargetState with early return
5. Run CSharpier formatting: `dotnet csharpier format src/V12_002.UI.Panel.Handlers.cs`

### Acceptance Criteria
- [ ] ValidateTargetState method created with CYC ≤3
- [ ] Method is pure (no side effects)
- [ ] All validation logic extracted from UpdateTargetVisibility
- [ ] UpdateTargetVisibility calls ValidateTargetState with early return
- [ ] XML documentation added
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] Complexity audit passes: `python scripts/complexity_audit.py`
- [ ] No behavioral changes (UI behaves identically)

### Dependencies
- None (first ticket)

### Lock-Free Validation
- ✅ No lock() statements
- ✅ Pure function (read-only operations)
- ✅ No shared mutable state
- ✅ No race conditions possible

---

## TICKET-2: Extract Drawing Operations

### Scope
- **Current Method**: `UpdateTargetVisibility`
- **Current CYC**: ~16 (after TICKET-1)
- **Target CYC**: ≤5 (for extracted method)
- **Extraction**: Chart drawing operations and visual rendering

### Implementation
1. Create new private method: `private void ExecuteDrawingOperations(TargetState targetState, ChartContext chartContext)`
2. Extract drawing logic:
   - Chart API calls
   - Drawing state updates
   - Visual element visibility changes
   - Error handling for drawing failures
3. Add XML documentation with purpose and side effects
4. Update UpdateTargetVisibility to call ExecuteDrawingOperations
5. Run CSharpier formatting: `dotnet csharpier format src/V12_002.UI.Panel.Handlers.cs`

### Acceptance Criteria
- [ ] ExecuteDrawingOperations method created with CYC ≤5
- [ ] All drawing logic extracted from UpdateTargetVisibility
- [ ] Error handling preserved
- [ ] UpdateTargetVisibility calls ExecuteDrawingOperations
- [ ] XML documentation added
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] Complexity audit passes: `python scripts/complexity_audit.py`
- [ ] No behavioral changes (chart renders identically)

### Dependencies
- TICKET-1 must be completed first

### Lock-Free Validation
- ✅ No lock() statements
- ✅ Drawing operations enqueued to chart actor (FSM pattern)
- ✅ Side effects isolated to actor message queue
- ✅ No direct state mutation

---

## TICKET-3: Extract UI Synchronization

### Scope
- **Current Method**: `UpdateTargetVisibility`
- **Current CYC**: ~11 (after TICKET-2)
- **Target CYC**: ≤4 (for extracted method)
- **Extraction**: UI control state synchronization

### Implementation
1. Create new private method: `private void SynchronizeUIControls(TargetState targetState, ControlContext controlContext)`
2. Extract UI synchronization logic:
   - Control visibility updates
   - Event handler synchronization
   - UI consistency checks
   - Atomic flag updates using Interlocked.Exchange
3. Add XML documentation with purpose and side effects
4. Update UpdateTargetVisibility to call SynchronizeUIControls
5. Run CSharpier formatting: `dotnet csharpier format src/V12_002.UI.Panel.Handlers.cs`

### Acceptance Criteria
- [ ] SynchronizeUIControls method created with CYC ≤4
- [ ] All UI synchronization logic extracted from UpdateTargetVisibility
- [ ] Atomic operations used for flag updates
- [ ] UpdateTargetVisibility calls SynchronizeUIControls
- [ ] XML documentation added
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] Complexity audit passes: `python scripts/complexity_audit.py`
- [ ] No behavioral changes (UI controls update identically)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Lock-Free Validation
- ✅ No lock() statements
- ✅ UI updates enqueued to UI actor (FSM pattern)
- ✅ Atomic primitives (Interlocked.Exchange) for flags
- ✅ Side effects isolated to actor message queue

---

## TICKET-4: Refactor Orchestrator Method

### Scope
- **Current Method**: `UpdateTargetVisibility`
- **Current CYC**: ~7 (after TICKET-3)
- **Target CYC**: ≤7 (orchestration only)
- **Extraction**: Final orchestration refactoring

### Implementation
1. Simplify UpdateTargetVisibility to orchestration only:
   - Call ValidateTargetState (early return if invalid)
   - Call ExecuteDrawingOperations
   - Call SynchronizeUIControls
   - Optional logging (if enabled)
2. Ensure method signature unchanged: `private void UpdateTargetVisibility(TargetState, bool, ChartContext, ControlContext)`
3. Add/update XML documentation with orchestration flow
4. Run CSharpier formatting: `dotnet csharpier format src/V12_002.UI.Panel.Handlers.cs`
5. Run full pre-push validation: `powershell -File scripts/pre_push_validation.ps1`

### Acceptance Criteria
- [ ] UpdateTargetVisibility reduced to CYC ≤7
- [ ] Method signature unchanged (no breaking changes)
- [ ] Orchestration flow clear and linear
- [ ] XML documentation updated
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] Complexity audit passes: `python scripts/complexity_audit.py`
- [ ] Pre-push validation passes (all 13 checks)
- [ ] No behavioral changes (end-to-end UI behavior identical)
- [ ] Hard-link sync: `powershell -File deploy-sync.ps1`

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Lock-Free Validation
- ✅ No lock() statements
- ✅ Orchestrator coordinates actors without direct state mutation
- ✅ All state mutations delegated to helper methods
- ✅ FSM/Actor Enqueue pattern maintained

---

## Quality Assurance Checklist

### Per-Ticket Validation
After each ticket completion:
1. Run complexity audit: `python scripts/complexity_audit.py`
2. Verify target CYC achieved
3. Run unit tests: `dotnet test`
4. Verify zero test failures
5. Run build: `dotnet build`
6. Verify zero compilation errors
7. Test UI behavior manually (if applicable)

### Final Validation (After TICKET-4)
1. Run full pre-push validation: `powershell -File scripts/pre_push_validation.ps1`
2. Verify all 13 checks pass
3. Run hard-link sync: `powershell -File deploy-sync.ps1`
4. Verify DIFF GUARD passes (<10k characters)
5. Test in NinjaTrader (F5 load)
6. Verify BUILD_TAG matches

### Rollback Strategy
- Bob CLI checkpointing enabled (automatic)
- Each ticket is a separate commit
- Rollback command: `/restore` in Bob CLI
- Emergency rollback: `git revert <commit-hash>`

---

## V12 DNA Compliance Summary

### Correctness by Construction
- ✅ ValidateTargetState enforces preconditions
- ✅ Type safety maintained (TargetState, ChartContext, ControlContext)
- ✅ Null guards at entry point
- ✅ FSM ensures valid state transitions
- ✅ Early return pattern prevents invalid operations

### Lock-Free Actor Pattern
- ✅ Zero lock() statements across all tickets
- ✅ FSM/Actor Enqueue pattern for state mutations
- ✅ Atomic primitives (Interlocked.Exchange) for flags
- ✅ Side effects isolated to actor message queues
- ✅ Drawing operations enqueued to chart actor
- ✅ UI updates enqueued to UI actor

### ASCII-Only Compliance
- ✅ No Unicode, emoji, or curly quotes
- ✅ ASCII-only identifiers
- ✅ Standard ASCII documentation

### Jane Street Alignment
- ✅ All methods CYC ≤8 (strict standard)
- ✅ Cognitive simplicity achieved
- ✅ Hot path optimization (validation first)
- ✅ No unnecessary branching
- ✅ Microsecond-latency ready
- ✅ Single responsibility per method

---

## Estimated Timeline

- **TICKET-1**: 1 hour (validation extraction)
- **TICKET-2**: 1.5 hours (drawing operations)
- **TICKET-3**: 1.5 hours (UI synchronization)
- **TICKET-4**: 1 hour (orchestrator refactoring)
- **Total**: 5 hours (including testing and validation)

---

## Success Metrics

### Complexity Reduction
- UpdateTargetVisibility: 19 → ≤7 (63% reduction)
- ValidateTargetState: CYC ≤3
- ExecuteDrawingOperations: CYC ≤5
- SynchronizeUIControls: CYC ≤4

### Quality Gates
- ✅ Zero lock() statements
- ✅ Zero compilation errors
- ✅ Zero test failures
- ✅ Zero behavioral changes
- ✅ Pre-push validation passes
- ✅ Codacy shows no new issues

### PR Hygiene
- ✅ Diff size <10,000 characters
- ✅ Single file modified (src/V12_002.UI.Panel.Handlers.cs)
- ✅ Zero scope creep
- ✅ Zero whitespace mutations

---

Document Version: 1.0
Phase: 4 (Ticket Generation)
Status: READY FOR EXECUTION
Generated: 2026-06-15T16:47:37Z
