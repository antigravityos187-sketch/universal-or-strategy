# Phase 1: Scope Definition + Boundary Validation - EPIC-CCN-125

## Epic Metadata
- **Epic ID**: EPIC-CCN-125
- **Target Method**: `EnterORPosition`
- **File**: `src/V12_002.Entries.OR.cs`
- **Current Complexity**: 11 (CYC)
- **Current LOC**: 166
- **Target Complexity**: ≤ 8 (Jane Street HFT standard)
- **Phase**: 1 (Scope + Boundary)

## Method Selection Rationale

Selected `EnterORPosition` from two CYC=11 candidates:

1. **`ProcessSessionReset`** (V12_002.BarUpdate.cs): CYC=11, LOC=32
2. **`EnterORPosition`** (V12_002.Entries.OR.cs): CYC=11, LOC=166 ✅ SELECTED

**Selection Criteria**:
- Higher LOC (166 vs 32) indicates more extraction potential
- Critical OR entry logic - high-value refactoring target
- Complexity + size combination suggests God-function pattern
- OR strategy is core V12 functionality (high impact)

## Target Method Details

### Current State
- **Method**: `EnterORPosition`
- **File**: `src/V12_002.Entries.OR.cs`
- **Cyclomatic Complexity**: 11
- **Lines of Code**: 166
- **Status**: Below V12 threshold (15) but exceeds Jane Street standard (8)

### Method Signature
```csharp
private void EnterORPosition(...)
```

### Responsibilities (Inferred)
Based on method name and file context:
1. Validate OR entry conditions
2. Calculate position sizing
3. Determine entry direction (long/short)
4. Submit entry orders
5. Set up bracket orders (stop/target)
6. Update position state
7. Broadcast entry signals

## Extraction Strategy

### What to Extract

**Target: 3-4 extracted methods to reduce CYC from 11 to ≤ 8**

1. **Entry Validation Logic** (estimated CYC reduction: -2)
   - Pre-flight checks
   - OR window validation
   - Risk management checks
   - Extract to: `ValidateOREntryConditions()`

2. **Position Sizing Calculation** (estimated CYC reduction: -2)
   - Account size checks
   - Risk percentage calculations
   - Quantity clamping
   - Extract to: `CalculateORPositionSize()`

3. **Order Submission Logic** (estimated CYC reduction: -2)
   - Order creation
   - Bracket setup
   - Error handling
   - Extract to: `SubmitOREntryOrders()`

4. **Post-Entry State Management** (estimated CYC reduction: -1)
   - Position tracking updates
   - Signal broadcasting
   - UI updates
   - Extract to: `FinalizeOREntry()`

### What to Keep in Original Method

**Core orchestration logic only**:
- High-level entry flow coordination
- Method call sequencing
- Top-level error handling
- Return value aggregation

**Target remaining CYC**: 6-8 (well below Jane Street threshold)

## Boundary Definition

### Single Method Scope (V12.23 No Scope Creep Protocol)

**STRICT BOUNDARY**: This epic refactors ONLY `EnterORPosition` in `V12_002.Entries.OR.cs`.

**Explicitly OUT OF SCOPE**:
- ❌ Other methods in `V12_002.Entries.OR.cs` (ExecuteLong, ExecuteShort, etc.)
- ❌ Related entry files (FFMA, MOMO, Retest, Trend)
- ❌ Caller methods (wherever EnterORPosition is invoked)
- ❌ Helper methods already called by EnterORPosition
- ❌ Shared state or data structures
- ❌ UI/drawing logic
- ❌ Signal broadcasting infrastructure

**Boundary Enforcement**:
- Extraction creates NEW private methods in SAME file
- No changes to method signature of `EnterORPosition`
- No changes to public API or external contracts
- No modifications to callers or callees
- Extracted methods are implementation details only

## Success Criteria

### Primary Goals
1. ✅ **Complexity Reduction**: CYC reduced from 11 to ≤ 8
2. ✅ **Jane Street Alignment**: Target CYC ≤ 8 (NOT 15)
3. ✅ **Cognitive Simplicity**: Each extracted method has single, clear purpose
4. ✅ **Correctness Preservation**: Zero behavioral changes
5. ✅ **Lock-Free Semantics**: No introduction of locks or shared mutable state

### Verification Criteria
1. **Build Success**: `dotnet build` passes with zero errors
2. **Complexity Audit**: `complexity_audit.py` confirms CYC ≤ 8 for all methods
3. **ASCII Compliance**: No Unicode characters introduced
4. **Test Coverage**: Existing tests pass (if any)
5. **Hard-Link Sync**: `deploy-sync.ps1` succeeds

### Quality Gates
- **Pre-Push Validation**: All 13 checks pass (fast mode minimum)
- **CSharpier**: Zero formatting issues
- **Codacy**: No new complexity violations
- **Manual Review**: Code reads clearly, intent is obvious

## Risk Assessment

### Overall Risk: MEDIUM-LOW

**Risk Factors**:
1. **Complexity**: CYC=11 is manageable (not extreme like 20+)
2. **Size**: 166 LOC requires careful extraction planning
3. **Criticality**: OR entry is core functionality (high test coverage needed)
4. **Dependencies**: Unknown until Phase 2 analysis

**Mitigation Strategies**:
1. **Incremental Extraction**: Extract one method at a time, verify after each
2. **Preserve Signatures**: Keep all existing method signatures unchanged
3. **Test-Driven**: Run tests after each extraction
4. **Checkpointing**: Use Bob CLI checkpointing for rollback safety
5. **Boundary Discipline**: Strict adherence to single-method scope

### Blast Radius
**Estimated Impact**: LOW
- Changes are internal to single method
- No public API modifications
- No cross-file dependencies introduced
- Callers remain unchanged

## Boundary Validation Section

### Scope Boundary Confirmation

**Question**: Does this extraction stay within a single method?
**Answer**: ✅ **YES**

**Validation**:
- ✅ Target: Single method (`EnterORPosition`)
- ✅ File: Single file (`V12_002.Entries.OR.cs`)
- ✅ Extraction: Creates new private methods in SAME file
- ✅ No cross-file changes
- ✅ No caller modifications
- ✅ No shared state mutations outside method scope

### Dependency Boundary Check

**Dependencies that would violate boundary**:
- ❌ Modifying other methods in the file
- ❌ Changing method signatures of existing methods
- ❌ Introducing cross-file dependencies
- ❌ Modifying shared state structures
- ❌ Changing public API contracts

**Confirmed**: NONE of the above violations are planned.

### Explicit Boundary Statement

**Boundary Validated**: ✅ **YES**

**Certification**:
This epic is scoped to extract logic from the single method `EnterORPosition` in file `V12_002.Entries.OR.cs`. All extracted methods will be private implementation details within the same file. No changes will be made to:
- Method signatures (public or private)
- Caller code
- Other files
- Shared state structures
- Public APIs

This extraction adheres to the V12.23 No Scope Creep Protocol.

## V12 DNA Compliance

### Architectural Mandates
- ✅ **Correctness by Construction**: Extracted methods will have clear, single purposes
- ✅ **Lock-Free Actor Pattern**: No locks will be introduced
- ✅ **ASCII-Only Compliance**: All string literals will be ASCII-only
- ✅ **Jane Street Alignment**: Target CYC ≤ 8 (cognitive simplicity)
- ✅ **Hard-Link Integrity**: `deploy-sync.ps1` will be run after changes

## Phase 1 Completion Checklist

- [x] Target method identified: `EnterORPosition`
- [x] Complexity metrics documented: CYC=11, LOC=166
- [x] Extraction strategy defined: 3-4 methods
- [x] Boundary validated: Single method scope confirmed
- [x] Success criteria established: CYC ≤ 8
- [x] Risk assessment completed: MEDIUM-LOW
- [x] V12 DNA compliance verified
- [x] Scope document created
- [ ] Manifest updated (next step)

## Next Phase

**Phase 2: Architecture Planning**
- Detailed method analysis using jCodemunch
- Exact extraction points identified
- Method signatures designed
- Data flow mapping
- Implementation plan with Mermaid diagrams

---

*Generated: 2026-06-13*
*Epic: EPIC-CCN-125*
*Phase: 1 (Scope + Boundary)*
*Target: EnterORPosition (CYC 11 → ≤ 8)*
