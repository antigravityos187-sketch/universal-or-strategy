# Phase 1.0: Scope Definition - EPIC-CCN-033

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: FlattenSinglePosition
- **File**: src/V12_002.Orders.Management.Flatten.cs
- **Current Complexity**: 16 (CCN)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Violation Severity**: +1 above V12 DNA threshold (15)

### Extraction Strategy

**Approach**: Break into 2-3 focused helper methods

1. **Extract Validation Logic** (Estimated CCN reduction: 3-4 points)
   - Pre-flatten position state checks
   - Input parameter validation
   - Position eligibility verification
   - Extract to: ValidatePositionForFlattening()

2. **Extract Order Creation Logic** (Estimated CCN reduction: 3-4 points)
   - Market order construction
   - Order parameter setup
   - Quantity calculation
   - Extract to: CreateFlattenMarketOrder()

3. **Simplify Error Handling** (Estimated CCN reduction: 1-2 points)
   - Consolidate exception handling branches
   - Standardize error notification
   - Extract to: HandleFlattenError() (if needed)

**Expected Outcome**:
- Main method CCN: 6-8 (well below threshold)
- Total extracted methods: 2-3
- Cognitive load: SIGNIFICANTLY REDUCED

## Boundary Definition

### ✅ IN SCOPE (SINGLE METHOD ONLY)
- **FlattenSinglePosition method body**: All logic within this method
- **Internal complexity**: Conditional branches, loops, error handling
- **Method signature**: May be preserved or simplified
- **Local variables**: Refactoring within method scope

### ❌ OUT OF SCOPE (STRICT BOUNDARY)
- **Callers**: Position management orchestration, flatten command handlers
- **Callees**: Market order submission primitives, position state queries
- **Other methods**: All other methods in V12_002.Orders.Management.Flatten.cs
- **Cross-file changes**: No modifications outside target file
- **Pre-existing issues**: No fixing unrelated compilation errors
- **Scope creep**: No "while we're here" improvements

### No Scope Creep Enforcement
- **ONE EPIC = ONE CONCERN**: FlattenSinglePosition complexity reduction ONLY
- **No bundling**: Do not combine with other refactoring tasks
- **No opportunistic fixes**: Ignore unrelated code smells in same file
- **Surgical precision**: Touch only what's necessary for extraction

## Success Criteria

### Functional Requirements
1. ✅ **Complexity Reduced**: CCN drops from 16 to ≤8
2. ✅ **All Tests Pass**: Zero test failures after extraction
3. ✅ **No Behavior Changes**: Identical runtime behavior
4. ✅ **Lock-Free Pattern**: FSM/Actor Enqueue model maintained

### V12 DNA Compliance
1. ✅ **Zero lock() blocks**: Must use atomic primitives or Actor pattern
2. ✅ **ASCII-only strings**: No Unicode, emoji, or curly quotes
3. ✅ **Atomic state transitions**: No race conditions introduced
4. ✅ **Make illegal states unrepresentable**: Type-safe error handling

### Code Quality
1. ✅ **Single Responsibility**: Each extracted method has one clear purpose
2. ✅ **Testability**: Extracted methods are unit-testable
3. ✅ **Readability**: Cognitive simplicity improved
4. ✅ **Maintainability**: Future changes easier to reason about

### Verification Gates
1. ✅ **Build passes**: dotnet build succeeds
2. ✅ **Tests pass**: dotnet test 100% pass rate
3. ✅ **Complexity audit**: complexity_audit.py confirms CCN ≤8
4. ✅ **Hard-link sync**: deploy-sync.ps1 succeeds

## Risk Assessment

### Risk Level: LOW
- **Rationale**:
  - Single method extraction (minimal blast radius)
  - Complexity only +1 above threshold (manageable)
  - Well-contained within Orders.Management domain
  - No cross-subgraph dependencies

### Mitigation Strategy
- **Checkpointing**: Enabled via Bob CLI .bob/settings.json
- **Incremental extraction**: One helper method at a time
- **Test-driven**: Verify tests pass after each extraction
- **Rollback ready**: Git commits after each successful extraction

## Phase 1.0 Completion Criteria

- [x] Extraction scope defined (single method: FlattenSinglePosition)
- [x] Boundary clearly marked (no scope creep)
- [x] Success criteria enumerated (complexity, tests, DNA compliance)
- [x] Risk assessment completed (LOW risk)
- [ ] Boundary validation (Phase 1.5 - next step)

---
**Phase**: 1.0 (Scope Definition)  
**Status**: COMPLETE  
**Next Phase**: 1.5 (Boundary Validation)  
**Date**: 2026-06-15
