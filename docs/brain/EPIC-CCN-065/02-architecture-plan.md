# Phase 2: Architecture Planning - EPIC-CCN-065

## Epic Metadata
- Epic ID: EPIC-CCN-065
- Target Method: HandleFsmFilled
- File: src/V12_002.Symmetry.BracketFSM.cs
- Phase: 2.0 - Architecture Planning
- Date: 2026-06-15
- Status: DRAFT (Pending Triple-Agent UltraThink Audit)

---

## 1. Extraction Strategy

### Current State
- Method: HandleFsmFilled
- Current Complexity: 13 CYC
- Current LOC: 18
- Tier: 2 (Medium complexity)

### Target State
- Target Complexity: ≤8 CYC per method (Jane Street strict standard)
- Proposed Helper Methods: 3 methods
- Extraction Approach: Single Responsibility Principle - each helper handles one concern

### Complexity Breakdown

Original Method (CYC 13):
- String null checks: +2
- Multiple StartsWith conditions: +6
- Nested if/else branches: +3
- State comparison logic: +2

After Extraction (Target CYC ≤8):
- HandleFsmFilled (orchestrator): CYC 3-4
- IsStopOrTargetOrder (classifier): CYC 6-7
- UpdateBracketStateForFill (state mutator): CYC 2-3
- TransitionToActiveIfEntry (state mutator): CYC 2

---

## 2. Method Signatures

### Original Method
```csharp
private void HandleFsmFilled(AccountEvent evt, FollowerBracketFSM fsm)
```

### Proposed Helper Method 1: Order Classification
```csharp
private bool IsStopOrTargetOrder(string signalName, out bool isStop, out bool isTarget)
```

Rationale: 
- Encapsulates complex string prefix logic
- Uses out parameters to avoid duplicate classification
- Returns combined result for caller convenience
- CYC: 6-7 (null check + 6 prefix checks)

### Proposed Helper Method 2: State Update for Fills
```csharp
private void UpdateBracketStateForFill(FollowerBracketFSM fsm, int filledQty)
```

Rationale:
- Single responsibility: contract decrement + state transition
- Encapsulates the "Filled vs Active" decision logic
- CYC: 2-3 (Math.Max guards + ternary operator)

### Proposed Helper Method 3: Entry Fill Transition
```csharp
private void TransitionToActiveIfEntry(FollowerBracketFSM fsm)
```

Rationale:
- Handles the "else if" branch for entry fills
- Clear single purpose: entry-specific state transition
- CYC: 2 (state comparison + assignment)

---

## 3. Call Graph

### Call Sequence

1. HandleFsmFilled (orchestrator)
   - Calls IsStopOrTargetOrder(evt.SignalName, out isStop, out isTarget)
   - If isStop OR isTarget: calls UpdateBracketStateForFill(fsm, evt.FilledQty)
   - Else if entry state: calls TransitionToActiveIfEntry(fsm)

2. IsStopOrTargetOrder (pure function)
   - No external calls
   - Returns classification result

3. UpdateBracketStateForFill (state mutator)
   - No external calls
   - Mutates fsm.RemainingContracts and fsm.State

4. TransitionToActiveIfEntry (state mutator)
   - No external calls
   - Mutates fsm.State

### Shared State

FSM Object (FollowerBracketFSM fsm):
- Passed by reference to all methods
- Mutated by UpdateBracketStateForFill and TransitionToActiveIfEntry
- Read by HandleFsmFilled for state checks

No Shared Mutable State Between Helpers:
- Each helper operates independently
- No cross-method dependencies
- Clear data flow through parameters

---

## 4. Lock-Free Validation

### Current Implementation Analysis

✅ No lock() statements in HandleFsmFilled
✅ Uses FSM/Actor Enqueue pattern (single-threaded access)
✅ Direct field assignments (safe within Actor boundary)

### Post-Extraction Validation

✅ IsStopOrTargetOrder: Pure function, no state mutation
✅ UpdateBracketStateForFill: Mutates FSM fields within Actor context
✅ TransitionToActiveIfEntry: Mutates FSM fields within Actor context
✅ HandleFsmFilled: Orchestrates calls within same Actor context

### Lock-Free Guarantees

1. Single-Threaded Access: All methods execute within the FSM/Actor's Enqueue queue
2. No Atomic Primitives Needed: Actor model provides synchronization boundary
3. No Race Conditions: Sequential execution guaranteed by Actor pattern
4. No Deadlocks: No locks to deadlock on

### V12 DNA Compliance

- ✅ Lock-Free Actor Pattern: Maintained
- ✅ ASCII-Only: No Unicode in string literals
- ✅ Correctness by Construction: Helper methods have clear contracts
- ✅ Make Illegal States Unrepresentable: State transitions remain explicit

---

## 5. Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)

Before Extraction:
- HandleFsmFilled: CYC 13 (exceeds threshold)

After Extraction:
- HandleFsmFilled: CYC 3-4 ✅
- IsStopOrTargetOrder: CYC 6-7 ✅
- UpdateBracketStateForFill: CYC 2-3 ✅
- TransitionToActiveIfEntry: CYC 2 ✅

### Testability

Before: 
- 13 execution paths in single method
- Difficult to test edge cases in isolation

After:
- Each helper can be unit tested independently
- Clear input/output contracts
- Easier to verify correctness

### Microsecond-Latency Considerations

Performance Impact: NEGLIGIBLE
- Method calls are inlined by JIT compiler
- No additional allocations
- Same execution path, just reorganized
- Actor pattern already provides synchronization overhead

### Incremental Improvement

Principle: Small, focused changes
- ✅ Single method extraction (no wholesale rewrite)
- ✅ Zero behavior changes (pure refactoring)
- ✅ Easy to review and verify
- ✅ Low rollback risk

---

## 6. Implementation Plan

### Step 1: Create Helper Methods
1. Add IsStopOrTargetOrder method below HandleFsmFilled
2. Add UpdateBracketStateForFill method below IsStopOrTargetOrder
3. Add TransitionToActiveIfEntry method below UpdateBracketStateForFill
4. Add XML documentation to each method

### Step 2: Refactor HandleFsmFilled
1. Replace inline string checks with IsStopOrTargetOrder call
2. Replace contract/state update logic with UpdateBracketStateForFill call
3. Replace entry transition logic with TransitionToActiveIfEntry call
4. Verify method complexity reduced to ≤8

### Step 3: Verification
1. Run dotnet build (zero errors required)
2. Run dotnet test (100% pass required)
3. Run python3 scripts/complexity_audit.py (verify CYC ≤8)
4. Run powershell -File .\scripts\pre_push_validation.ps1 -Fast

### Step 4: Testing Strategy
1. Add unit tests for IsStopOrTargetOrder (stop/target/entry cases)
2. Add unit tests for UpdateBracketStateForFill (contract decrement logic)
3. Add unit tests for TransitionToActiveIfEntry (state transition logic)
4. Verify existing integration tests still pass

---

## 7. Success Criteria

### Functional Requirements
- ✅ HandleFsmFilled behavior unchanged (zero logic changes)
- ✅ All existing tests pass (100% pass rate)
- ✅ No new compilation errors
- ✅ No new runtime errors

### Non-Functional Requirements
- ✅ HandleFsmFilled complexity ≤8 CYC
- ✅ All helper methods complexity ≤8 CYC
- ✅ Lock-free Actor pattern maintained
- ✅ ASCII-only compliance maintained
- ✅ XML documentation added to all methods

### Quality Gates
- ✅ Pre-push validation passes (all 13 checks)
- ✅ Codacy shows no new issues
- ✅ CodeRabbit AI review passes (no critical/high findings)
- ✅ PR diff <10,000 characters

---

## 8. Next Steps

### Phase 3: DNA & PR Audit (Adjudicator)
- Submit this plan to Arena AI for adversarial review
- Verify plan against V12 DNA constraints
- Check PR health predictions
- Gate: PASS/FAIL (fail triggers Phase 2 rework)

### Phase 4: Recursive Execution (Engineer)
- Hand off to Bob CLI (v12-engineer) for implementation
- Execute extraction with mandatory checkpointing
- Verify each step against implementation plan

### Phase 5: Verification/Review (Forensics)
- Compare implementation against this plan
- Run automated "Fix-all" loop if logic drifts
- Verify complexity reduction achieved

### Phase 6: Sign-off (Director)
- Run powershell -File .\deploy-sync.ps1
- F5 in NinjaTrader + BUILD_TAG verification
- Merge to main branch

---

## 9. References

- Phase 1.0 Scope Definition: docs/brain/EPIC-CCN-065/01-scope.md
- Phase 1.5 Boundary Validation: docs/brain/EPIC-CCN-065/01-scope-boundary.md
- Hotspot Analysis: docs/brain/EPIC-CCN-065/00-hotspots.md
- V12 DNA Mandates: AGENTS.md (Section 2: Architectural Mandates)
- Jane Street Standards: AGENTS.md (Section 3.5: Complexity Threshold Rationale)
- Lock-Free Actor Pattern: AGENTS.md (Section 2: Lock-Free Actor Pattern)
- Phase 6 Recursive Protocol: AGENTS.md (Section 7: Phase 6 Recursive Protocol)

---

## 10. Approval Status

Status: DRAFT (Pending Triple-Agent UltraThink Audit)

Approval Criteria:
- [ ] Extraction strategy is sound
- [ ] Method signatures are well-designed
- [ ] Call graph is clear and correct
- [ ] Lock-free validation is complete
- [ ] Jane Street compliance verified
- [ ] Implementation plan is detailed
- [ ] Success criteria are measurable

Next Action: Submit to Arena AI for Phase 3 DNA & PR Audit
