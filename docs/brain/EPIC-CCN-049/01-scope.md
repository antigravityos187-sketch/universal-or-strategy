# Phase 1.0: Scope Definition - EPIC-CCN-049

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: ManageTrail_RunPerTradeBranches
- File: src/V12_002.Trailing.cs
- Current Complexity: 9 (CYC)
- Target Complexity: 8 or less (Jane Street strict standard)
- Extraction Strategy: Minimal extraction - extract 1-2 decision points into helper methods

### Complexity Reduction Plan

Current State:
- 8 decision points (CYC = decisions + 1 = 9)
- Cognitive load: MODERATE
- Status: Below V12 threshold (15) but can be optimized

Target State:
- 7 decision points (CYC = 8)
- Cognitive load: LOW
- Improvement: 11 percent complexity reduction

Extraction Approach:
1. Identify 1-2 tightly coupled decision branches
2. Extract into single-purpose helper method(s)
3. Maintain lock-free Actor/FSM pattern
4. Preserve all existing behavior

### Boundary Definition

IN SCOPE (ONLY):
- ManageTrail_RunPerTradeBranches method body
- Internal decision logic within this method
- Local variable extraction if needed
- Helper method creation (private, same class)

OUT OF SCOPE (STRICTLY FORBIDDEN):
- Callers of ManageTrail_RunPerTradeBranches
- Callees invoked by ManageTrail_RunPerTradeBranches
- Other methods in V12_002.Trailing.cs
- Class-level state or fields
- Method signature changes
- Public API modifications

No Scope Creep:
- No while we are here improvements
- No fixing pre-existing compilation errors
- No bundling multiple concerns
- No refactoring adjacent methods
- ONE EPIC = ONE CONCERN = ONE METHOD

### Success Criteria

Functional Requirements:
1. Complexity reduced from 9 to 8 or less
2. All existing tests pass (100 percent pass rate)
3. No behavior changes (bit-for-bit identical output)
4. Lock-free Actor/FSM pattern maintained
5. ASCII-only compliance preserved

Quality Gates:
1. CSharpier formatting check passes
2. Build succeeds (zero errors)
3. Lint audit passes (zero violations)
4. Pre-push validation passes (all 13 checks)
5. Complexity audit confirms CYC 8 or less

V12 DNA Compliance:
1. No lock(stateLock) blocks introduced
2. No Unicode/emoji in string literals
3. Atomic operations via FSM/Actor Enqueue
4. Make illegal states unrepresentable principle maintained

### Extraction Strategy Details

Minimal Extraction Pattern:
- Extract smallest logical unit that reduces CYC by 1
- Prefer extracting conditional branches over loops
- Keep hot-path code inline (microsecond-latency requirement)
- Use descriptive helper method names (e.g., ShouldAdjustTrail, ValidateTrailState)

Jane Street Alignment:
- Cognitive simplicity over clever abstractions
- Single-purpose helper methods
- Exhaustive testability (2^8 = 256 paths vs 2^9 = 512)
- Race condition audit readiness

### Risk Assessment

Overall Risk: LOW
- Method already below threshold (9 < 15)
- Minimal extraction (1 complexity point)
- Well-defined scope (single method)
- No API changes

Mitigation:
- Mandatory unit test coverage for extracted logic
- Pre/post complexity verification
- Behavior preservation testing
- Lock-free pattern validation

## Phase 1.0 Approval

Status: READY FOR PHASE 1.5 (Boundary Validation)

Rationale:
- Scope clearly defined (single method)
- Extraction strategy minimal and focused
- Success criteria measurable and testable
- V12 DNA compliance maintained

Next Step: Proceed to Phase 1.5 (Boundary Validation)
