# Phase 1.0: Scope Definition - EPIC-CCN-051

## Epic Metadata
- Epic ID: EPIC-CCN-051
- Target Method: UpdateStopOrder
- File: src/V12_002.Trailing.StopUpdate.cs
- Phase: 1.0 - Scope Definition
- Date: 2026-06-15

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: UpdateStopOrder
- Signature: private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)
- Current Complexity: 11 (Cyclomatic Complexity)
- Target Complexity: <=8 (Jane Street strict standard)
- Method Type: Private instance method
- Return Type: void

### Complexity Reduction Strategy
Extraction Approach: Break into 2-3 helper methods

Proposed Decomposition:
1. ValidateStopOrderParameters (CYC ~2-3)
   - Validate entryName, pos, newStopPrice, newTrailLevel
   - Return early if validation fails
   - Reduces branching in main method

2. CalculateStopOrderUpdate (CYC ~3-4)
   - Calculate new stop order values
   - Handle trail level adjustments
   - Pure calculation logic

3. ApplyStopOrderUpdate (CYC ~2-3)
   - Apply validated and calculated values to order
   - Handle state transitions
   - Maintain Actor/FSM pattern

Expected Result: UpdateStopOrder becomes orchestrator (CYC ~2-3) calling 3 focused helpers

## Boundary Definition

### IN SCOPE
- UpdateStopOrder method body ONLY
- Extract helper methods within same file
- Maintain private access level
- Preserve method signature (no parameter changes)
- Keep error handling pattern (try-catch with Print logging)

### OUT OF SCOPE
- Callers: No changes to UI.IPC.Commands.Mode.cs
- Callers: No changes to Symmetry.Replace.cs
- Callees: No changes to methods called by UpdateStopOrder
- Other Methods: No changes to other methods in V12_002.Trailing.StopUpdate.cs
- State Management: No changes to PositionInfo structure
- Logging: No changes to Print() logging infrastructure

### No Scope Creep
- ONE EPIC = ONE CONCERN: Complexity reduction of UpdateStopOrder only
- No "While We're Here": Do not fix unrelated issues
- No Bundling: Do not combine with other refactoring tasks
- No Pre-existing Errors: Do not fix compilation errors outside scope

## Success Criteria

### Functional Requirements
1. Complexity Reduced: UpdateStopOrder CYC reduced from 11 to <=8
2. All Tests Pass: Existing test suite passes without modification
3. No Behavior Changes: Identical runtime behavior (black-box equivalence)
4. Lock-Free Pattern: Actor/FSM pattern maintained (no lock() blocks)

### Non-Functional Requirements
1. ASCII-Only: No Unicode, emoji, or curly quotes in code
2. Private Scope: Extracted helpers remain private
3. Error Handling: Preserve try-catch with Print logging
4. Atomic Operations: Maintain atomicity of stop order updates

### Quality Gates
1. Build Success: dotnet build passes
2. Lint Clean: powershell -File .\scripts\lint.ps1 passes
3. Format Check: dotnet csharpier check src/ passes
4. Complexity Audit: python scripts/complexity_audit.py shows CYC <=8

## Risk Assessment

### Blast Radius
- Direct Callers: 2 identified (UI.IPC.Commands.Mode, Symmetry.Replace)
- Impact Scope: MEDIUM - affects trailing stop order updates
- Coupling Risk: MEDIUM - 2 distinct call sites across subsystems

### Mitigation Strategy
1. Preserve Signature: No changes to method signature or parameters
2. Black-Box Testing: Verify identical behavior via integration tests
3. Incremental Extraction: Extract one helper at a time, verify after each
4. Rollback Plan: Git checkpoint before each extraction step

## Jane Street Alignment

### Cognitive Simplicity
- Current State: CYC=11 approaching threshold (15)
- Target State: CYC<=8 for microsecond-latency reasoning
- Principle: "Make illegal states unrepresentable" via focused helpers

### Testing Strategy
- Exhaustive Paths: Reduced CYC enables exhaustive path testing
- Race Condition Audit: Simpler logic easier to audit for lock-free correctness
- HFT Constraints: Maintain sub-microsecond latency requirements

## Next Steps
1. Phase 1.5: Boundary Validation (mandatory V12.23 protocol)
2. Phase 2: Implementation Plan with Mermaid diagrams
3. Phase 3: DNA & PR Audit (Arena AI red team)
4. Phase 4: Recursive Execution (Bob CLI v12-engineer)
