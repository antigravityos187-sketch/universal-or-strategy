# Phase 1.0: Scope Definition - EPIC-CCN-045

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: OnKeyDown
- File: src/V12_002.UI.Callbacks.cs
- Current Complexity: 9 (Cyclomatic Complexity)
- Target Complexity: <=8 (Jane Street strict standard)
- Extraction Strategy: Break into 2-3 helper methods

### Complexity Reduction Plan

Current State:
- OnKeyDown handles all keyboard input with 8 decision points
- Branches include: key type detection, state validation, mode routing, error handling
- Estimated 50-80 lines of code with 2-3 nesting levels

Target State:
- OnKeyDown becomes orchestrator (CYC <=3)
- Extract 2-3 focused helper methods:
  1. ValidateKeyInputState() - State validation logic (CYC <=2)
  2. RouteKeyByType(KeyEventArgs e) - Key type routing (CYC <=3)
  3. HandleKeyAction(KeyType type) - Action execution (CYC <=2)

Extraction Approach:
- Use Extract Method refactoring pattern
- Maintain identical behavior (zero regression)
- Preserve lock-free Actor/FSM pattern
- Keep all state access patterns unchanged

## Boundary Definition

### IN SCOPE
- OnKeyDown method body only
- Internal branching logic extraction
- Helper method creation within same class
- Complexity reduction from 9 to <=8

### OUT OF SCOPE
- Callers: NinjaTrader event system (framework callback)
- Callees: Existing downstream methods (ValidateState, ProcessKeyInput, etc.)
- Other methods: No changes to other methods in V12_002.UI.Callbacks.cs
- State objects: No modifications to shared UI state structures
- Event pipeline: No changes to event handling infrastructure

### No Scope Creep
- ONE EPIC = ONE CONCERN: Single-method complexity reduction only
- No "while we are here" improvements
- No fixing pre-existing compilation errors
- No bundling multiple refactoring concerns
- No architectural changes beyond method extraction

## Success Criteria

### Functional Requirements
1. Complexity Reduced: OnKeyDown CYC drops from 9 to <=8
2. Behavior Preserved: Zero functional changes (bit-identical output)
3. Tests Pass: All existing tests pass without modification
4. Lock-Free Pattern: Actor/FSM Enqueue model maintained

### Non-Functional Requirements
1. Performance: No measurable latency increase (<1us tolerance)
2. Readability: Improved code clarity through focused methods
3. Maintainability: Easier to add new key handlers in future
4. Testability: Extracted methods are unit-testable in isolation

### Quality Gates
1. Build: Zero compilation errors
2. Lint: Zero new Roslyn warnings
3. Format: CSharpier compliance (braces, line endings)
4. Complexity: Codacy confirms CYC <=8 for OnKeyDown

## Risk Assessment

### Risk Level: LOW
- Scope: Single method, well-contained
- Blast Radius: Limited to UI callback layer
- Reversibility: Easy to revert via git
- Test Coverage: Framework callback (integration tested)

### Mitigation Strategy
1. Checkpointing: Enable Bob CLI auto-checkpoint before extraction
2. Incremental: Extract one helper at a time, verify after each
3. Verification: Run build + tests after each extraction step
4. Rollback Plan: Git restore if any test fails

## Jane Street Alignment

### Cognitive Simplicity Principle
- Functions with CYC >8 are harder to reason about under latency constraints
- Single-responsibility methods reduce cognitive load
- Testable units enable exhaustive path coverage

### HFT Best Practices
- Keep hot-path methods simple and verifiable
- Avoid clever abstractions in event handlers
- Make illegal states unrepresentable through structure

## Phase 1.0 Completion Status
- Extraction scope defined (single method)
- Boundary clearly established (no scope creep)
- Success criteria documented (measurable)
- Risk assessment completed (LOW risk)

Next Phase: Phase 1.5 (Boundary Validation - V12.23 Protocol)
