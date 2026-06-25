# Phase 1: Scope Definition - EPIC-W7-044

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:06:39Z

## Epic Metadata
- **Target Method**: SymmetryGuardCascadeFollowerCleanup
- **File**: src/V12_002.Symmetry.Replace.cs
- **Current CYC**: 11 (exceeds threshold by 3)
- **Target CYC**: ≤8 per method
- **Blast Radius**: LOW (0 external callers)

## Scope Boundary Analysis

### IN SCOPE

#### Primary Extraction Target
**Method**: SymmetryGuardCascadeFollowerCleanup(string masterEntryName)
- **Lines**: 198-244 (46 lines)
- **Complexity**: CYC 11, Nesting 6
- **Reason**: Exceeds Jane Street threshold, coordinator pattern with multiple responsibilities

#### Extraction Candidates (Helper Methods)
1. **Dispatch Lookup Logic**
   - Extract: Symmetry dispatch dictionary lookup and validation
   - Reduces: Nesting depth from 6 to 4
   - Target CYC: 2-3

2. **Follower Cleanup Loop**
   - Extract: Iteration over follower entries with position/order checks
   - Reduces: Main method CYC by 4-5 points
   - Target CYC: 5-6

3. **Order Cancellation Logic**
   - Extract: Terminal order state checks and CancelOrderSafe calls
   - Reduces: Conditional complexity
   - Target CYC: 2-3

#### Dependencies to Preserve
- symmetryMasterEntryToDispatch (constant) - dispatch lookup
- symmetryDispatchById (constant) - dispatch registry
- activePositions (constant) - position tracking
- entryOrders (constant) - order tracking
- CancelOrderSafe (method) - order cancellation
- LogBuffer.Format (method) - logging
- IsOrderTerminal (method) - order state check

### OUT OF SCOPE

#### Excluded from Refactoring
1. **Caller Investigation**
   - **Reason**: 0 direct callers detected - may be dead code or reflection-based
   - **Action**: Document as potential dead code, do NOT attempt to trace reflection calls
   - **Rationale**: Scope creep risk, requires runtime analysis

2. **Downstream Callees (18 methods)**
   - **Reason**: All callees are stable, tested methods
   - **Action**: Use as-is, do NOT refactor callees
   - **Rationale**: Blast radius containment

3. **Dictionary Implementations**
   - symmetryMasterEntryToDispatch, symmetryDispatchById, activePositions, entryOrders
   - **Reason**: Shared state across multiple methods
   - **Action**: Read-only access, do NOT modify structure
   - **Rationale**: Cross-method dependencies

4. **Logging Infrastructure**
   - LogBuffer.Format, LogBuffer.ValidateThreadAffinity, LogBuffer.FormatInternal
   - **Reason**: Stable logging framework
   - **Action**: Use as-is, do NOT refactor logging calls
   - **Rationale**: Non-functional concern

5. **Order Management Methods**
   - CancelOrderSafe, IsOrderTerminal
   - **Reason**: Core order lifecycle methods
   - **Action**: Use as-is, do NOT modify signatures
   - **Rationale**: High-risk, tested methods

## Extraction Strategy

### Approach: SURGICAL EXTRACTION
Break the 46-line coordinator into 3-4 focused helper methods:

1. **Helper 1**: TryGetSymmetryDispatch(string masterEntryName, out SymmetryDispatch dispatch)
   - **Responsibility**: Lookup and validate dispatch entry
   - **Returns**: bool (success/failure)
   - **CYC Target**: 2-3

2. **Helper 2**: CleanupFollowerEntries(SymmetryDispatch dispatch)
   - **Responsibility**: Iterate followers, cancel orders, remove positions
   - **Returns**: void
   - **CYC Target**: 5-6

3. **Helper 3**: CancelFollowerOrderIfActive(Order order)
   - **Responsibility**: Check terminal state, cancel if active
   - **Returns**: void
   - **CYC Target**: 2-3

### Post-Extraction Structure
Main method will delegate to helpers, reducing CYC from 11 to 2-3.
Expected CYC: 2-3 (main method) + 2-3 + 5-6 + 2-3 = all ≤8

## Risk Mitigation

### Low-Risk Factors
- ✅ 0 external callers (no blast radius)
- ✅ Isolated to single file
- ✅ No cross-method state mutations

### Medium-Risk Factors
- ⚠️ Touches 4 concurrent dictionaries (state coordination)
- ⚠️ 18 downstream callees (testing surface)
- ⚠️ Potential dead code (0 callers detected)

### Mitigation Strategy
1. **Preserve all dictionary access patterns** (no structural changes)
2. **Extract helpers as private methods** (no visibility changes)
3. **Maintain exact call sequence** (no reordering)
4. **Add unit tests for extracted helpers** (verify behavior preservation)

## Success Criteria

### Phase 2 (Architecture Planning)
- [ ] Define helper method signatures
- [ ] Map control flow to helper methods
- [ ] Identify shared state access patterns
- [ ] Plan test coverage strategy

### Phase 5 (Ticket Execution)
- [ ] Extract TryGetSymmetryDispatch (CYC ≤3)
- [ ] Extract CleanupFollowerEntries (CYC ≤6)
- [ ] Extract CancelFollowerOrderIfActive (CYC ≤3)
- [ ] Verify main method CYC ≤8
- [ ] Add unit tests for all extracted methods
- [ ] Verify build passes
- [ ] Verify F5 in NinjaTrader

## Jane Street Alignment

### Principles Applied
1. **Cognitive Simplicity**: Reduce CYC from 11 to ≤8
2. **Single Responsibility**: Each helper does one thing
3. **Testability**: Smaller methods = exhaustive testing
4. **Correctness by Construction**: Preserve exact behavior

### Threshold Compliance
- **Current**: CYC 11 (FAILS Jane Street ≤8)
- **Target**: CYC ≤8 per method (PASSES Jane Street)
- **Approach**: Extract 3 helpers, reduce main to CYC 2-3

## Scope Boundary Validation

### Scope Creep Prevention
- ❌ Do NOT investigate reflection-based callers
- ❌ Do NOT refactor downstream callees
- ❌ Do NOT modify dictionary structures
- ❌ Do NOT change logging infrastructure
- ❌ Do NOT alter order management methods

### Scope Adherence
- ✅ Extract ONLY from SymmetryGuardCascadeFollowerCleanup
- ✅ Create ONLY private helper methods
- ✅ Preserve EXACT behavior
- ✅ Maintain EXACT call sequence
- ✅ Test ONLY extracted methods

## Next Phase: Architecture Planning
Phase 2 will define:
1. Exact helper method signatures
2. Control flow mapping
3. State access patterns
4. Test coverage plan
