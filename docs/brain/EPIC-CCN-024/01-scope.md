# Phase 1.0: Scope Definition - EPIC-CCN-024

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: MonitorRmaProximity
- **File**: src/V12_002.Entries.RMA.cs
- **Current Complexity**: 17 (CCN)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Threshold Violation**: +2 over V12 threshold (15), +9 over Jane Street strict (8)

### Extraction Strategy

**Approach**: Break into 2-3 helper methods using Extract Method refactoring

**Complexity Reduction Plan**:
1. **Extract conditional validation logic** - Helper method 1 (CCN ~3-4)
2. **Extract distance calculation logic** - Helper method 2 (CCN ~2-3)
3. **Extract decision/action logic** - Helper method 3 (CCN ~2-3)
4. **Main method orchestration** - Reduced to CCN ~5-6

**Target CCN Distribution**:
- MonitorRmaProximity (main): ≤8
- Helper method 1: ≤5
- Helper method 2: ≤5
- Helper method 3: ≤5

### Boundary Definition

#### IN SCOPE
- **MonitorRmaProximity method body ONLY**
- Extract 2-3 private helper methods within same class
- Preserve method signature (no parameter changes)
- Maintain return type and semantics
- Keep all logic within V12_002.Entries.RMA.cs

#### OUT OF SCOPE
- **Callers**: No changes to methods that call MonitorRmaProximity
- **Callees**: No changes to methods called by MonitorRmaProximity
- **Other methods**: No changes to other methods in V12_002.Entries.RMA.cs
- **File structure**: No file splits, no new files
- **Public API**: No signature changes visible to callers
- **Pre-existing bugs**: No fixing unrelated compilation errors
- **Performance optimization**: No algorithmic changes
- **Feature additions**: No new functionality

#### NO SCOPE CREEP
- ONE EPIC = ONE CONCERN: Complexity reduction of MonitorRmaProximity ONLY
- No "while we're here" improvements
- No bundling multiple refactoring concerns
- No fixing adjacent code issues

### Success Criteria

#### Functional Requirements
- All existing tests pass (no behavior changes)
- Entry signal validation logic preserved exactly
- RMA proximity monitoring semantics unchanged
- No new compilation errors introduced

#### Complexity Requirements
- MonitorRmaProximity CCN reduced from 17 to ≤8
- All extracted helper methods CCN ≤5
- Total complexity budget maintained (no complexity hiding)

#### V12 DNA Compliance
- Lock-free Actor/FSM pattern maintained (no lock introduction)
- ASCII-only compliance (no Unicode in strings)
- Atomic state access patterns preserved
- Jane Street cognitive simplicity alignment

#### Quality Gates
- CSharpier formatting passes
- Roslyn analyzer passes (zero violations)
- Pre-push validation passes (all 13 checks)
- F5 manual test in NinjaTrader succeeds

### Risk Assessment

**Overall Risk**: MEDIUM

**Risk Factors**:
- Entry signal logic is business-critical
- No existing unit tests for this method (requires creation)
- Manual F5 testing required for validation

**Mitigation**:
- Extract methods incrementally (one at a time)
- Run build + tests after each extraction
- Create unit tests for extracted methods
- Verify behavior preservation with integration tests

### Implementation Constraints

#### Architectural Constraints
- Must maintain lock-free semantics
- Must preserve atomic state access patterns
- Must not introduce new dependencies
- Must keep all code in same file

#### Testing Constraints
- Must create unit tests for extracted methods
- Must verify integration with entry signal pipeline
- Must perform F5 manual test in NinjaTrader

#### Performance Constraints
- Must not degrade latency (microsecond-sensitive)
- Must not introduce heap allocations in hot path
- Must preserve inline-ability of critical paths

---

**Scope Status**: DEFINED
**Boundary Status**: VALIDATED (see 01-scope-boundary.md)
**Ready for Phase 2**: Pending boundary validation approval
