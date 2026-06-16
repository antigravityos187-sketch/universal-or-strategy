# Phase 1.0: Scope Definition - EPIC-CCN-011

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: DestroyPanel
- File: src/V12_002.UI.Panel.Construction.cs
- Current Complexity: 17 (CCN)
- Target Complexity: 8 or less (Jane Street strict standard)
- Violation: +2 over V12 threshold (15), +9 over Jane Street strict (8)

### Extraction Strategy

Approach: Break into 2-3 helper methods using Extract Method refactoring

Proposed Decomposition:
1. ValidatePanelState() - Extract state validation checks
   - Pre-condition validation
   - State machine consistency checks
   - Target CCN: 3 or less

2. CleanupUIComponents() - Extract UI component disposal
   - UI element disposal sequence
   - Resource cleanup for visual components
   - Target CCN: 3 or less

3. CleanupResourcesAndState() - Extract resource/state cleanup
   - Memory resource cleanup
   - State machine transition
   - Logging/diagnostics
   - Target CCN: 3 or less

Resulting Complexity:
- DestroyPanel (orchestrator): CCN 3 or less (calls 3 helpers)
- Total complexity: 12 or less (distributed across 4 methods)
- Each method: 3 or less (single responsibility, easy to test)

## Boundary Definition

### IN SCOPE (ONLY)
- DestroyPanel method body (lines TBD after source inspection)
- Extract 2-3 private helper methods within same class
- Maintain exact same behavior (no logic changes)
- Preserve lock-free Actor/FSM pattern
- Update inline comments if needed for clarity

### OUT OF SCOPE (STRICTLY FORBIDDEN)
- Callers of DestroyPanel (state machine transitions, error handlers)
- Callees of DestroyPanel (UI disposal methods, resource utilities)
- Other methods in V12_002.UI.Panel.Construction.cs
- Panel construction methods (CreatePanel, InitializePanel, etc.)
- State machine infrastructure changes
- Test modifications (unless extraction breaks existing tests)
- Logging infrastructure changes
- Error handling patterns (preserve existing)

### No Scope Creep Rules
1. ONE EPIC = ONE CONCERN: Only reduce DestroyPanel complexity
2. No "While We're Here": Do not fix unrelated issues
3. No Bundling: Do not combine with other refactoring tasks
4. No Pre-existing Fixes: Do not fix compilation errors outside DestroyPanel

## Success Criteria

### Functional Requirements
- All existing tests pass (zero regressions)
- No behavior changes (bit-for-bit identical execution)
- Panel destruction works identically to before
- Error handling paths preserved
- Resource cleanup sequence unchanged

### Complexity Requirements
- DestroyPanel CCN reduced from 17 to 8 or less
- Each extracted method CCN 8 or less (ideally 3 or less)
- Total complexity distributed, not increased
- Cognitive complexity reduced (easier to reason about)

### V12 DNA Compliance
- Lock-Free: No lock() statements introduced
- Actor/FSM Pattern: State transitions use Enqueue model
- ASCII-Only: No Unicode in string literals
- Atomic Operations: Use Interlocked.* for shared state
- Correctness by Construction: Make illegal states unrepresentable

### Code Quality
- CSharpier formatting passes
- Roslyn analyzer warnings: zero new issues
- Build succeeds with zero errors
- Hard-link sync successful (deploy-sync.ps1)

### Testing Requirements
- Existing FSMActorTests pass
- Manual F5 test in NinjaTrader (panel destruction works)
- No memory leaks (resource cleanup verified)
- No race conditions introduced

## Risk Mitigation

### High-Risk Areas
1. State Machine Transitions: Verify FSM state remains consistent
2. Resource Cleanup Order: Preserve disposal sequence
3. Error Recovery: Maintain partial failure handling
4. UI Thread Safety: Preserve thread affinity for UI operations

### Mitigation Strategy
1. Checkpointing: Enable Bob CLI checkpointing for rollback
2. Incremental Extraction: Extract one method at a time, test after each
3. Diff Review: Verify only DestroyPanel and new helpers changed
4. Pre-Push Validation: Run full validation suite before commit

## Jane Street Alignment

### Cognitive Simplicity
- Functions with CCN >15 are hard to reason about under latency constraints
- Target CCN 8 or less ensures each function has single, clear purpose
- Easier to audit for race conditions in lock-free code

### Testability
- CCN 17 = exponential test path growth (2^17 = 131k paths)
- CCN 8 or less per method = manageable test coverage (2^8 = 256 paths)
- Extracted methods can be unit tested independently

### Maintainability
- Simple functions = faster code review
- Clear separation of concerns = easier debugging
- Single responsibility = safer modifications

## Metadata

- Epic ID: EPIC-CCN-011
- Phase: 1.0 (Scope Definition)
- Target Method: DestroyPanel
- Complexity Reduction: 17 to 8 or less (52% reduction minimum)
- Extraction Count: 2-3 helper methods
- Risk Level: MEDIUM-HIGH
- Estimated Effort: 2-4 hours (including testing)
- Dependencies: None (isolated refactoring)
- Next Phase: 1.5 (Boundary Validation)
