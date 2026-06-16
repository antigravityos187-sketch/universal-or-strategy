# Phase 1.0: Scope Definition - EPIC-CCN-053

## Epic Metadata
- **Epic ID**: EPIC-CCN-053
- **Target Method**: InitiateStopReplacement
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Current Complexity**: 10
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Phase**: 1.0 - Scope Definition
- **Date**: 2026-06-15

---

## 1. Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: InitiateStopReplacement
- **Signature**: 
  ```csharp
  private void InitiateStopReplacement(
      string entryName,
      PositionInfo pos,
      Order currentStop,
      double validatedStopPrice,
      int newTrailLevel
  )
  ```

### Current Complexity Analysis
- **Cyclomatic Complexity**: 10
- **Jane Street Threshold**: 15 (aligned)
- **Target Threshold**: ≤8 (strict standard for cognitive simplicity)
- **Status**: Below threshold but improvable

### Complexity Contributors
1. **Target Snapshot Loop** (lines ~11-30):
   - For loop iterating 1-5 targets
   - Nested conditionals checking order state
   - List building logic
   - **Estimated CYC contribution**: 4-5

2. **Circuit Breaker Logic** (lines ~46-55):
   - Conditional check for threshold
   - State mutation logic
   - **Estimated CYC contribution**: 2-3

3. **Base Logic** (remaining):
   - Object creation, state updates, logging
   - **Estimated CYC contribution**: 3-4

### Extraction Strategy
Break InitiateStopReplacement into 2-3 helper methods:

1. **Extract Target Snapshot Logic** → CaptureActiveTargets(string entryName)
   - Isolates the for-loop and conditional logic
   - Returns List<TargetSnapshot> or array
   - Reduces parent method CYC by ~4-5

2. **Extract Circuit Breaker Check** → CheckAndActivateCircuitBreaker(int currentCount)
   - Isolates circuit breaker activation logic
   - Returns void, handles state mutation internally
   - Reduces parent method CYC by ~2-3

3. **Simplified Parent Method**:
   - Calls helper methods
   - Focuses on orchestration: create pending, add to dictionary, cancel order, update state
   - **Target CYC**: ≤8

---

## 2. Boundary Definition

### What's IN Scope
✅ **InitiateStopReplacement method body ONLY**
- Extract target snapshot loop into helper method
- Extract circuit breaker logic into helper method
- Refactor parent method to call helpers
- Maintain exact same behavior and side effects

### What's OUT of Scope
❌ **Callers** (methods that call InitiateStopReplacement):
- No changes to calling code
- No signature changes to InitiateStopReplacement

❌ **Callees** (methods called by InitiateStopReplacement):
- GetTargetOrdersDictionary() - unchanged
- CancelOrderForReplace() - unchanged
- MarkStickyDirty() - unchanged
- Print() - unchanged

❌ **Other Methods in V12_002.Trailing.StopUpdate.cs**:
- No changes to sibling methods
- No changes to class-level state or fields
- No changes to other complexity hotspots

❌ **Pre-existing Issues**:
- No fixing compilation errors outside this method
- No "while we're here" improvements
- No bundling multiple concerns

### Scope Creep Prevention
- **ONE EPIC = ONE CONCERN**: Reduce InitiateStopReplacement complexity only
- **No Bundling**: Do not combine with other refactoring tasks
- **No Drift**: Stay focused on the single method extraction

---

## 3. Success Criteria

### Functional Requirements
✅ **Behavior Preservation**:
- All existing tests pass (if any)
- No changes to observable behavior
- Same side effects (state mutations, logging, order operations)

✅ **Complexity Reduction**:
- InitiateStopReplacement CYC reduced from 10 to ≤8
- Helper methods each have CYC ≤5 (simple, single-purpose)

✅ **Code Quality**:
- Lock-free Actor/FSM pattern maintained
- No new lock() statements introduced
- ASCII-only compliance maintained
- V12 DNA principles upheld

### Non-Functional Requirements
✅ **Testing**:
- All unit tests pass (FSMActorTests.cs)
- No new test failures introduced
- Consider adding tests for extracted helpers (optional)

✅ **Build Health**:
- Zero compilation errors
- Zero Roslyn analyzer warnings
- CSharpier formatting passes
- Pre-push validation passes

✅ **Performance**:
- No performance degradation
- Method inlining candidates identified (if applicable)
- Hot-path optimization maintained

---

## 4. Extraction Plan

### Step 1: Extract Target Snapshot Logic
**New Method**: CaptureActiveTargets(string entryName)
- Expected CYC: 4-5
- Returns: List<TargetSnapshot>
- Purpose: Isolate target snapshot loop and conditionals

### Step 2: Extract Circuit Breaker Logic
**New Method**: CheckAndActivateCircuitBreaker(int currentCount)
- Expected CYC: 2
- Returns: void
- Purpose: Isolate circuit breaker activation logic

### Step 3: Refactor Parent Method
**Simplified InitiateStopReplacement**:
- Expected CYC: 3-4 (well below target of 8)
- Calls: CaptureActiveTargets(), CheckAndActivateCircuitBreaker()
- Focus: Orchestration and state management

---

## 5. Risk Assessment

### Low Risk Factors
✅ Method is well-isolated (private, single responsibility)
✅ Complexity is moderate (CYC=10, not a God-function)
✅ No Jane Street P0 violations detected
✅ Clear extraction boundaries identified

### Medium Risk Factors
⚠️ Blast radius unknown (jCodemunch unavailable during Phase 0)
⚠️ No existing unit tests for this specific method
⚠️ Circuit breaker state mutation requires careful handling

### Mitigation Strategies
1. **Manual Code Review**: Identify all callers before extraction
2. **Incremental Testing**: Test after each helper extraction
3. **Atomic Commits**: Commit each extraction separately for easy rollback
4. **Pre-Push Validation**: Run full validation suite before push

---

## 6. Jane Street Alignment

### Cognitive Simplicity Principles
- **"Make illegal states unrepresentable"**: Maintained through type safety
- **Single Responsibility**: Each helper method has one clear purpose
- **Predictable Behavior**: No hidden state mutations in helpers
- **Testability**: Extracted helpers are easier to unit test

### HFT Performance Considerations
- **Hot-Path Optimization**: Method is in trailing stop update path (latency-sensitive)
- **Inlining Candidates**: Small helpers may be inlined by JIT compiler
- **Lock-Free Pattern**: No locks introduced, Actor/FSM pattern preserved
- **Memory Allocation**: List allocation in CaptureActiveTargets is acceptable (infrequent operation)

---

## 7. Next Steps

### Phase 1.5: Boundary Validation (MANDATORY)
- Create 01-scope-boundary.md
- Validate no scope creep
- Get Director approval before Phase 2

### Phase 2: Architectural Planning
- Create detailed implementation plan
- Generate Mermaid diagrams for extraction flow
- Document helper method contracts

### Phase 3: DNA & PR Audit
- Arena AI red team review
- Verify V12 DNA compliance
- PR health check

---

## Approval Status
- **Status**: PENDING (awaiting Phase 1.5 boundary validation)
- **Reviewer**: Director
- **Next Gate**: Phase 1.5 boundary check
