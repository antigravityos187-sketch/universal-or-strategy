# Phase 2: Architecture Planning - EPIC-CCN-040

## Epic Metadata
- **Epic ID**: EPIC-CCN-040
- **Phase**: 2 (Architecture Planning)
- **Date**: 2026-06-15
- **Target Method**: FindTargetOrderForPosition
- **Current Complexity**: 9
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **File**: src/V12_002.Trailing.Breakeven.cs

## Method Analysis

### Current Implementation
The method has 23 lines of code with cyclomatic complexity of 9.

**Method Signature**:
```
private Order FindTargetOrderForPosition(
    PositionInfo pos,
    string entryName,
    int targetNum,
    out string notFoundReason
)
```

### Complexity Breakdown
- **Current Cyclomatic Complexity**: 9
- **Decision Points**:
  1. Entry validation check
  2. Account selection (ternary with AND condition)
  3. Foreach loop iteration
  4. Null check on order
  5. Name match check
  6. Instrument match check
  7. OrderState Working check
  8. OrderState Accepted check (OR condition)

## Extraction Strategy

### Goal
Reduce complexity from 9 to ≤8 by extracting 2 helper methods:
1. **GetSearchAccount**: Extract account selection logic
2. **IsMatchingTargetOrder**: Extract order matching predicate

### Rationale
- **Account Selection**: The ternary operator with follower logic is a self-contained decision
- **Order Matching**: The 4-condition AND chain is a complex predicate
- **Cognitive Simplicity**: Each helper has a single, clear responsibility (Jane Street principle)

## Proposed Architecture

### Helper Method 1: GetSearchAccount

**Purpose**: Determine which account to search for orders (Master vs Follower)

**Signature**: private Account GetSearchAccount(PositionInfo pos)

**Parameters**: pos (PositionInfo) - Position information containing follower status and executing account

**Returns**: Account - The account to search (either pos.ExecutingAccount or this.Account)

**Complexity**: 2 (one if statement with AND condition)

### Helper Method 2: IsMatchingTargetOrder

**Purpose**: Check if an order matches the target criteria

**Signature**: private bool IsMatchingTargetOrder(Order order, string targetOrderName)

**Parameters**:
- order (Order): The order to check
- targetOrderName (string): The expected order name

**Returns**: bool - True if order matches all criteria, false otherwise

**Complexity**: 5 (null check + 3 AND conditions + 1 OR condition)

### Refactored Main Method

**New Complexity**: 3 (entry validation if + foreach loop + helper method call if)

## Call Graph

```
FindTargetOrderForPosition (CYC: 3)
├── GetSearchAccount (CYC: 2)
│   └── Returns: Account
└── IsMatchingTargetOrder (CYC: 5)
    └── Returns: bool
```

**Data Flow**:
1. Main method validates entry filled status
2. Main method calls GetSearchAccount(pos) → receives Account
3. Main method iterates through Account.Orders
4. For each order, calls IsMatchingTargetOrder(order, targetOrderName) → receives bool
5. If match found, returns order; otherwise returns null with reason

**Shared State**: None (all methods are pure functions operating on parameters)

## Complexity Validation

### Before Extraction
- **FindTargetOrderForPosition**: CYC = 9

### After Extraction
- **FindTargetOrderForPosition**: CYC = 3 ✅
- **GetSearchAccount**: CYC = 2 ✅
- **IsMatchingTargetOrder**: CYC = 5 ✅

**Total Complexity**: 3 + 2 + 5 = 10 (distributed across 3 methods)
**Max Method Complexity**: 5 ≤ 8 ✅

## Lock-Free Validation

### Analysis
- ✅ **No lock() statements**: Method is read-only, no state mutations
- ✅ **No shared mutable state**: All parameters are read-only
- ✅ **Thread-safe**: Method only reads from Account.Orders (NinjaTrader managed collection)
- ✅ **Atomic primitives**: Not applicable (no state mutations)

### FSM/Actor Pattern Compliance
- **Pattern**: Read-only query method
- **State Access**: Read-only access to Account.Orders and PositionInfo
- **Mutations**: None (method returns Order reference or null)
- **Compliance**: ✅ No violations of lock-free Actor pattern

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
- ✅ **Main method**: CYC = 3 (well below threshold)
- ✅ **Helper 1**: CYC = 2 (simple conditional)
- ✅ **Helper 2**: CYC = 5 (focused predicate)

### Single Responsibility Principle
- ✅ **FindTargetOrderForPosition**: Orchestrates order search workflow
- ✅ **GetSearchAccount**: Determines correct account for search
- ✅ **IsMatchingTargetOrder**: Validates order against criteria

### Testability
- ✅ **Main method**: Can be tested with mock PositionInfo and Account
- ✅ **GetSearchAccount**: Pure function, easily unit testable
- ✅ **IsMatchingTargetOrder**: Pure function, easily unit testable

### Maintainability
- ✅ **Clear intent**: Each method has a single, obvious purpose
- ✅ **Low coupling**: Methods communicate via parameters only
- ✅ **High cohesion**: Related logic grouped in focused methods

## Jane Street Knowledge Base Insights

### Query Results
- **Query**: "testing" (from will_wilson_why_testing_hard_2026)
- **Relevance**: Testing patterns for extracted methods

### Testing Strategy (Jane Street Aligned)
1. **Unit Tests for Helpers**: Test GetSearchAccount with follower/non-follower positions; Test IsMatchingTargetOrder with various order states
2. **Integration Tests**: Verify FindTargetOrderForPosition behavior unchanged; Test edge cases
3. **Property-Based Testing**: Invariant - Extracted methods preserve original behavior

## Risk Assessment

### Blast Radius
- **Scope**: Single method in single file
- **Callers**: 1 caller at line 356 (MoveSpecificTarget context)
- **Dependencies**: No changes to method signature
- **Impact**: Localized to trailing breakeven functionality

### Mitigation Strategy
1. **Git Checkpoint**: Create restore point before extraction
2. **Incremental Extraction**: Extract one helper at a time
3. **Test After Each Step**: Verify compilation and tests pass
4. **Rollback Plan**: Revert to checkpoint if any test fails

### Success Criteria
- ✅ **Complexity**: Main method CYC ≤ 8 (target: 3)
- ✅ **Behavior**: All existing tests pass without modification
- ✅ **Build**: Zero compilation errors
- ✅ **Lint**: Zero new Roslyn warnings
- ✅ **Format**: CSharpier compliant
- ✅ **Lock-Free**: No lock() statements introduced

## Implementation Sequence

### Step 1: Extract GetSearchAccount
1. Create private method GetSearchAccount(PositionInfo pos)
2. Move account selection logic from line 206
3. Replace ternary with method call
4. Verify compilation and tests

### Step 2: Extract IsMatchingTargetOrder
1. Create private method IsMatchingTargetOrder(Order order, string targetOrderName)
2. Move 4-condition AND chain from lines 211-216
3. Replace inline condition with method call
4. Verify compilation and tests

### Step 3: Verification
1. Run complexity audit
2. Verify CYC ≤ 8 for all methods
3. Run full quality gates
4. Verify zero regressions

## V12 DNA Compliance

- ✅ **Type Safety**: All parameters strongly typed
- ✅ **Null Safety**: Explicit null checks and out parameter
- ✅ **ASCII-Only**: All strings use ASCII characters
- ⚠️ **Hard-Link Integrity**: Must run deploy-sync.ps1 after extraction

## Approval Decision

### Status: READY FOR IMPLEMENTATION

### Rationale
1. Clear extraction plan with 2 helper methods
2. Complexity reduction from 9 to 3 (67% reduction)
3. Low risk - pure functions, no state mutations, single caller
4. Jane Street aligned - CYC ≤ 8, cognitive simplicity, testability
5. V12 DNA compliant - lock-free, ASCII-only, type-safe

### Next Phase
- **Phase 3**: DNA & PR Audit (Arena AI adjudication)
- **Phase 4**: Recursive Execution (Bob CLI v12-engineer)
- **Deliverable**: Extracted methods in V12_002.Trailing.Breakeven.cs

## Metadata
- **Architect**: Bob Shell (Plan Mode)
- **Planning Date**: 2026-06-15
- **Protocol Version**: V12.23
- **Jane Street KB**: Queried (testing patterns)
- **Approval Authority**: Automated (low-risk, single-method extraction)
