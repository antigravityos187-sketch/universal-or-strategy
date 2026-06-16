# Phase 1.0: Scope Definition - EPIC-CCN-065

## Epic Metadata
- Epic ID: EPIC-CCN-065
- Target Method: HandleFsmFilled
- File: src/V12_002.Symmetry.BracketFSM.cs
- Phase: 1.0 - Scope Definition
- Date: 2026-06-15
- Status: APPROVED

## 1. Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: HandleFsmFilled
- Current Complexity: 13 (Cyclomatic Complexity)
- Target Complexity: 8 or less (Jane Street strict standard)
- Extraction Strategy: Break into 2-3 helper methods

### Complexity Analysis
- Current State: 13 CYC (87% of threshold)
- Threshold: 15 (Jane Street aligned)
- Gap to Target: 5 points reduction needed (13 to 8)
- Risk Level: MEDIUM (approaching threshold)

### Extraction Strategy
1. Identify Decision Points: Analyze conditional branches and state transitions
2. Extract Helper Methods: Create 2-3 focused helper methods for state validation logic, transition condition checks, and state update operations
3. Maintain FSM Pattern: Preserve lock-free Actor/FSM enqueue model
4. Preserve Semantics: Zero behavior changes, pure refactoring

## 2. Boundary Definition

### IN SCOPE
- HandleFsmFilled method body only
- Extracting conditional logic into helper methods
- Reducing cyclomatic complexity from 13 to 8 or less
- Maintaining lock-free Actor/FSM pattern
- Preserving all existing behavior
- Adding XML documentation to extracted methods

### OUT OF SCOPE
- Callers of HandleFsmFilled: No changes to upstream code
- Callees of HandleFsmFilled: No changes to downstream dependencies
- Other methods in V12_002.Symmetry.BracketFSM.cs: No modifications
- Other files in src/: No cross-file changes
- Test files: No test modifications (unless new tests required for extracted methods)
- Performance optimizations: Not part of this epic
- Feature additions: Not part of this epic
- Bug fixes: Not part of this epic (unless directly related to extraction)

### Scope Creep Prevention
- ONE EPIC = ONE CONCERN: Complexity reduction only
- No While We are Here Changes: Resist temptation to fix unrelated issues
- No Bundling: Do not combine with other refactoring tasks
- No Pre-existing Errors: Do not fix compilation errors outside HandleFsmFilled

## 3. Success Criteria

### Functional Requirements
- Complexity Reduced: HandleFsmFilled complexity 8 or less (from 13)
- All Tests Pass: 100% test pass rate maintained
- No Behavior Changes: Identical runtime behavior
- Lock-Free Pattern: Actor/FSM enqueue model preserved
- ASCII-Only: No Unicode characters introduced

### Quality Requirements
- Build Success: Zero compilation errors
- Lint Clean: Zero new Roslyn violations
- Format Clean: CSharpier formatting passes
- PR Hygiene: Diff less than 10,000 characters
- Documentation: XML docs for all extracted methods

### Verification Requirements
- Complexity Audit: complexity_audit.py confirms 8 or less
- Pre-Push Validation: All 13 checks pass
- Hard-Link Sync: deploy-sync.ps1 succeeds
- NinjaTrader Test: F5 in NinjaTrader loads without errors

## 4. Risk Assessment

### Technical Risks
- Risk: Breaking FSM state machine logic
  - Mitigation: Preserve exact control flow, add unit tests
- Risk: Introducing race conditions
  - Mitigation: Maintain lock-free Actor pattern, no new locks
- Risk: Performance regression
  - Mitigation: Extract methods are inlined by JIT, no overhead expected

### Process Risks
- Risk: Scope creep during implementation
  - Mitigation: Phase 1.5 boundary validation (mandatory)
- Risk: Diff bloat from whitespace changes
  - Mitigation: CSharpier pre-formatting, surgical changes only

## 5. Jane Street Alignment

### Cognitive Simplicity
- Principle: Make illegal states unrepresentable
- Application: Extract complex conditionals into named, testable methods
- Benefit: Easier to reason about under microsecond latency constraints

### Testing Philosophy
- Principle: Test what you cannot prove correct
- Application: Unit tests for extracted helper methods
- Benefit: Exhaustive path coverage becomes tractable at CYC 8 or less

### HFT Performance
- Principle: Hot-path co-location
- Application: Keep extracted methods in same file (inlining eligible)
- Benefit: Zero performance overhead from extraction

## 6. Implementation Constraints

### V12 DNA Mandates
- Lock-Free: No lock(stateLock) blocks allowed
- ASCII-Only: No Unicode, emoji, or curly quotes
- Atomic Operations: Use FSM/Actor Enqueue or atomic primitives
- Correctness by Construction: Type-safe state transitions

### Code Quality Standards
- Cyclomatic Complexity: 8 or less per method (Jane Street strict)
- Method Length: Prefer less than 50 lines per method
- Single Responsibility: One concern per method
- Testability: All extracted methods must be unit-testable

## 7. Approval

Status: APPROVED

Rationale:
- Single-method extraction (no scope creep)
- Clear complexity reduction goal (13 to 8 or less)
- Well-defined boundaries (IN/OUT scope)
- Jane Street aligned (cognitive simplicity)
- V12 DNA compliant (lock-free, ASCII-only)

Next Phase: Phase 1.5 - Boundary Validation (V12.23 Protocol)

## 8. References

- Hotspot Analysis: docs/brain/EPIC-CCN-065/00-hotspots.md
- V12 DNA: AGENTS.md (Section 2: Architectural Mandates)
- Jane Street Standards: docs/intel/jane-street/ (complexity threshold)
- Complexity Audit: scripts/complexity_audit.py
- Pre-Push Validation: scripts/pre_push_validation.ps1
