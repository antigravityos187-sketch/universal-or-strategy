# Phase 2: Architecture Planning - EPIC-CCN-058

## V12.23 Protocol Compliance

This architecture plan defines the refactoring strategy for `HydrateFSM_MapOrderStateToFsmState` to achieve Jane Street cognitive simplicity standards (CYC ≤8).

## Target Method Analysis

### Current Implementation
- **Method**: `HydrateFSM_MapOrderStateToFsmState`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Lines**: 948-965 (18 lines)
- **Complexity**: CYC=9
- **Pattern**: If-chain with compound OR conditions
- **Purpose**: Maps NinjaTrader OrderState enum to FSM FollowerBracketState enum

### Complexity Breakdown

Current CYC=9 breakdown:
- Base: 1
- If conditions with OR operators: +8
  - if (orderState == OrderState.Filled || orderState == OrderState.PartFilled) → +2
  - if (orderState == OrderState.Accepted || orderState == OrderState.Working) → +2
  - if (orderState == OrderState.PendingSubmit || orderState == OrderState.PendingChange) → +2
  - if (orderState == OrderState.Cancelled || orderState == OrderState.Rejected) → +2

## Extraction Strategy

### Approach: Switch Expression Refactor (No Helper Methods)

**Rationale**:
- Current CYC=9 is caused by compound OR conditions in if-chain
- Switch expression with pattern matching eliminates OR operators
- Each case arm counts as +1 complexity (vs +2 for if-with-OR)
- Target CYC ≤8 achievable with direct switch expression
- **No helper methods needed** - this is a pure mapping function

### Proposed Refactor

**Before (If-Chain, CYC=9)**:

Pure if-chain with compound OR conditions returning enum values based on OrderState input.

**After (Switch Expression with OR Patterns, CYC=5)**:

Switch expression using C# 9.0 OR patterns to group related states, reducing complexity from 9 to 5.

**Complexity Calculation (Revised)**:

Switch expression with OR patterns CYC calculation:
- Base: 1
- Case arms with OR: +4 (4 logical groups)
- Default case (_): +0
- Total: CYC=5 ✅ (below Jane Street threshold of 8)

## Method Signatures

### Original Method

private FollowerBracketState HydrateFSM_MapOrderStateToFsmState(OrderState orderState)

**Parameters**:
- `orderState`: `OrderState` enum (NinjaTrader type)

**Return Type**:
- `FollowerBracketState` enum (V12 FSM type)

**Access Modifier**:
- `private` (internal helper, not exposed)

### No Helper Methods Required
- **Rationale**: Switch expression with OR patterns achieves CYC=5 without extraction
- **Benefit**: Maintains single-method simplicity, no call overhead
- **Jane Street Alignment**: Cognitive simplicity through pattern matching, not decomposition

## Call Graph

### Current Call Graph

HydrateFSM_MapOrderStateToFsmState (line 948)
  ↑
  Called by: Line 1249 (hydration logic in same file)
  ↓
  Calls: NONE (pure mapping function)

### After Refactor (No Change)

HydrateFSM_MapOrderStateToFsmState (line 948)
  ↑
  Called by: Line 1249 (hydration logic in same file)
  ↓
  Calls: NONE (pure mapping function)

**Data Flow**:
- Input: `OrderState` enum value
- Processing: Pattern matching via switch expression
- Output: `FollowerBracketState` enum value
- **Shared State**: NONE (pure function)

## Lock-Free Validation

### ✅ No lock() Statements
- **Current**: Pure function, no locks
- **After Refactor**: Remains pure function, no locks
- **Verification**: No state mutation, no synchronization needed

### ✅ Uses FSM/Actor Enqueue Pattern
- **Current**: Not applicable (pure mapping function)
- **After Refactor**: Not applicable (pure mapping function)
- **Verification**: No state machine transitions in this method

### ✅ Atomic Primitives Only
- **Current**: No shared state, no atomics needed
- **After Refactor**: No shared state, no atomics needed
- **Verification**: Enum-to-enum mapping is inherently atomic

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
- **Current**: CYC=9 (if-chain with compound OR conditions)
- **Target**: CYC=5 (switch expression with OR patterns)
- **Achievement**: ✅ Below Jane Street threshold of 8
- **Method**: Pattern matching eliminates redundant branching

### Microsecond Latency Requirements
- **Performance Impact**: NEUTRAL to POSITIVE
  - Switch expressions compile to jump tables (O(1) lookup)
  - If-chains compile to sequential comparisons (O(n) worst case)
  - Compiler optimizes both, but switch is more predictable
- **Hot Path**: Not on critical path (hydration is initialization, not tick-by-tick)
- **Verification**: No performance regression expected

### Correctness by Construction
- **Current**: Default case returns `Unknown` (safe fallback)
- **After Refactor**: Default case `_` returns `Unknown` (maintains safety)
- **Compiler Enforcement**: Switch expression ensures all paths return a value
- **Exhaustiveness**: Explicit cases cover all expected states, default handles unknowns

### Jane Street KB Insights (Attempted)
- **Query Results**: No specific FSM extraction patterns found in KB
- **Fallback Strategy**: Applied V12 DNA principles + C# 9.0 pattern matching best practices
- **Alignment**: Switch expressions align with Jane Street preference for:
  - Declarative over imperative code
  - Compiler-enforced exhaustiveness
  - Reduced cognitive load through pattern matching

## Risk Assessment

### Blast Radius
- **Single File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Single Method**: `HydrateFSM_MapOrderStateToFsmState` (18 lines)
- **Single Caller**: Line 1249 (internal hydration logic)
- **Zero Callees**: Pure mapping function
- **Risk Level**: LOW (isolated change, no cascading effects)

### Rollback Strategy
- **Git Checkpoint**: Commit before refactor
- **Bob CLI Restore**: Checkpointing enabled (`.bob/settings.json`)
- **Test Coverage**: Existing tests validate behavior
- **Hard-link Sync**: `deploy-sync.ps1` maintains NinjaTrader sync

### Verification Checklist
- [ ] Complexity audit shows CYC=5 after refactor
- [ ] All tests pass (`dotnet test`)
- [ ] Build succeeds (`dotnet build`)
- [ ] Hard-link sync succeeds (`deploy-sync.ps1`)
- [ ] No behavior changes (caller unaffected)
- [ ] No scope creep (only target method modified)

## V12 DNA Compliance Summary

### ✅ Lock-Free Actor Pattern
- Pure function, no state mutation, no locks required

### ✅ ASCII-Only Compliance
- Switch expression uses ASCII-only syntax
- No Unicode, emoji, or curly quotes

### ✅ Correctness by Construction
- Switch expression with exhaustive matching
- Compiler enforces all paths covered
- Default case handles unknown states safely

### ✅ Jane Street Alignment
- CYC=5 (below threshold of 8)
- Pattern matching improves cognitive simplicity
- No performance regression on hot path

## Implementation Notes

### C# Language Version
- **Required**: C# 9.0+ (for `or` patterns in switch expressions)
- **Current**: V12 project targets .NET 8.0 (supports C# 12.0)
- **Verification**: No language version upgrade needed

### Compiler Optimization
- Switch expressions compile to efficient jump tables
- Pattern matching with `or` is optimized by compiler
- No runtime performance difference vs if-chain

### Testing Strategy
- **Existing Tests**: Should pass without modification (behavior unchanged)
- **New Tests**: Not required (pure refactor, no logic changes)
- **Manual Verification**: F5 in NinjaTrader + visual inspection

---

**Phase 2 Status**: COMPLETE  
**Architecture Plan**: APPROVED  
**Next Phase**: 3.0 (DNA & PR Audit via Arena AI)
