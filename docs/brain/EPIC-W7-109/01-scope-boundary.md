# Phase 1.5: Scope Boundary Validation - EPIC-W7-109

## Agent Tracking
- **Agent Name**: v12-phase1-scope (boundary validation)
- **Mode**: plan
- **Execution Time**: 2026-06-24T00:13:03Z

## Boundary Validation Summary

**Status**: APPROVED - Clear boundaries, no scope creep detected

**Primary Extraction**: ReconstructMasterPositionFromBroker (lines 335-442)
**Estimated Impact**: CYC 23 to 5-6 (original method), CYC 6-7 (extracted method)

---

## IN SCOPE - Explicit Boundaries

### 1. Primary Extraction Target

**Method**: ReconstructMasterPositionFromBroker
- **Start Line**: 335 (try block for master position reconstruction)
- **End Line**: 442 (closing brace of catch block)
- **Line Count**: 107 lines
- **Current CYC**: 15-18 (estimated)
- **Target CYC**: <=8

**Responsibilities** (ALL IN SCOPE):
1. Scan broker positions for master account
2. Extract position details (MarketPosition, Quantity, AvgPrice)
3. Match stop orders to position
4. Calculate target distribution (T1-T5)
5. Construct PositionInfo object
6. Classify signal type (MOMO, TREND, RMA, FFMA, Retest)
7. Register position in activePositions dictionary

### 2. State Modifications

**Modified State** (IN SCOPE):
- activePositions dictionary (passed as parameter, modified in-place)

**Read-Only Access** (IN SCOPE):
- Account.Positions (broker positions)
- Instrument.FullName (instrument identifier)
- stopOrders (dictionary - read-only in extracted method)

### 3. Dependencies

**Existing Helper Methods** (IN SCOPE - already extracted):
- GetTargetDistribution() - Target calculation
- GetStableHash() - Hash generation
- Print() - Logging

**No New Dependencies Required**

---

## OUT OF SCOPE - Explicit Exclusions

### 1. Already Extracted Methods

**NOT extracting** (already done in previous epics):
- AdoptFleetOrders() - Already extracted
- AdoptMasterOrders() - Already extracted
- HydrateFSMsFromWorkingOrders() - Already extracted

**Rationale**: These methods are already at acceptable CYC levels

### 2. Orchestration Logic

**NOT extracting** (core responsibility):
- Lines 309-314: Initial setup and AdoptFleetOrders call
- Lines 315-332: Master order adoption try-catch (CYC ~2-3, only 16 lines)
- Lines 445-456: FSM hydration and completion logging

**Rationale**: Orchestration is the primary responsibility of HydrateWorkingOrdersFromBroker

### 3. Future Refactoring Candidates

**NOT extracting in this epic** (defer to future):
- Signal type classification logic (within extracted method)
- Stop order matching logic (within extracted method)
- PositionInfo construction logic (within extracted method)

**Rationale**: Focus on single largest complexity driver first. If extracted method CYC >8, create follow-up epic.

---

## Scope Creep Risk Assessment

### NO SCOPE CREEP DETECTED

#### Risk Factor 1: Adjacent Code Temptation
**Risk**: Temptation to "fix" lines 316-332 (master order adoption try-catch)
**Mitigation**: Explicitly marked OUT OF SCOPE (only 16 lines, CYC ~2-3)
**Status**: SAFE - Clear boundary at line 335

#### Risk Factor 2: Helper Method Extraction
**Risk**: Temptation to extract GetTargetDistribution or GetStableHash
**Mitigation**: Already extracted in previous epics
**Status**: SAFE - No new helper extraction needed

#### Risk Factor 3: Over-Extraction
**Risk**: Temptation to extract signal classification, stop matching, or PositionInfo construction
**Mitigation**: Explicitly deferred to future epics if needed
**Status**: SAFE - Single extraction target only

#### Risk Factor 4: Cross-File Dependencies
**Risk**: Temptation to modify other files in V12_002.*.cs
**Mitigation**: All code within V12_002.SIMA.Lifecycle.cs
**Status**: SAFE - No cross-file changes

#### Risk Factor 5: Signature Changes
**Risk**: Temptation to modify HydrateWorkingOrdersFromBroker signature
**Mitigation**: Constraint: "Method signature must remain unchanged"
**Status**: SAFE - No breaking changes

---

## Boundary Clarity Validation

### Entry Point
**Line 335**: Start of try block for master position reconstruction
**Trigger**: After AdoptMasterOrders() completes successfully
**Precondition**: Master account exists and needs processing

### Exit Point
**Line 442**: End of catch block for master position reconstruction
**Next Step**: Call HydrateFSMsFromWorkingOrders() (line 445)
**Postcondition**: activePositions dictionary updated with master position

### Data Flow

**Inputs** (clear):
- Account.Positions (broker positions)
- Instrument.FullName (instrument identifier)
- stopOrders (dictionary)
- activePositions (dictionary - to be modified)

**Outputs** (clear):
- Modified activePositions dictionary
- Log messages via Print()

**Side Effects** (clear):
- None beyond activePositions modification and logging

---

## Jane Street Boundary Compliance

### Current Violations
1. CYC 23 > 8 (Jane Street threshold)
2. Nesting 7 > 4 (cognitive simplicity)
3. Lines 149 > 50 (single-screen visibility)

### Post-Refactor Compliance
1. Original method CYC 5-6 <= 8
2. Extracted method CYC 6-7 <= 8
3. Original method nesting 3-4 <= 4
4. Original method lines ~42 <= 50
5. Extracted method nesting 3-4 <= 4

**Boundary Alignment**: COMPLIANT - Extraction boundaries align with Jane Street principles

---

## Risk Assessment

### Low Risk Factors
1. **Blast Radius**: 0 direct dependents (isolated method)
2. **Clear Boundaries**: Well-defined start (line 335) and end (line 442)
3. **No Cross-File Dependencies**: All code within same file
4. **Existing Helpers**: GetTargetDistribution, GetStableHash already extracted
5. **Simple State**: Only activePositions dictionary modified
6. **No Breaking Changes**: Method signature unchanged

### Medium Risk Factors
1. **Deep Nesting**: 7 levels - requires careful extraction
2. **Multiple Responsibilities**: Position scanning + classification + registration
3. **Error Handling**: Try-catch block must be preserved
4. **Dictionary Mutation**: activePositions modified in-place

### Mitigation Strategies
1. **Preserve Error Handling**: Keep try-catch in extracted method
2. **Pass Dictionary by Reference**: Maintain in-place modification behavior
3. **Preserve Logging**: Keep all Print() statements
4. **Unit Test**: Verify position reconstruction logic

---

## Scope Creep Prevention Checklist

- [x] Primary extraction target clearly defined (lines 335-442)
- [x] OUT OF SCOPE items explicitly listed
- [x] No adjacent code improvements planned
- [x] No helper method extractions planned
- [x] No cross-file changes planned
- [x] No signature changes planned
- [x] No while-we-are-here fixes planned
- [x] Future refactoring candidates deferred
- [x] Single concern: Extract master position reconstruction only

---

## Approval Decision

**Status**: APPROVED

**Rationale**:
1. Clear IN SCOPE boundaries (lines 335-442)
2. Clear OUT OF SCOPE exclusions (orchestration, already-extracted methods)
3. No scope creep risks detected
4. Single extraction target (no over-extraction)
5. Jane Street compliant boundaries
6. Low risk profile with clear mitigations

**Recommendation**: Proceed to Phase 2 (Architecture Planning)

---

## Next Phase Requirements

### Phase 2: Architecture Planning
**Input**: This boundary validation document
**Output**: 02-architecture-plan.md

**Required Decisions**:
1. Method signature for ReconstructMasterPositionFromBroker
2. Parameter passing strategy (ref vs out for activePositions)
3. Error handling approach (preserve try-catch)
4. Unit test structure
5. Extraction sequence diagram

**Boundary Constraints**:
- Must respect lines 335-442 boundary
- Must preserve all logging
- Must maintain dictionary mutation behavior
- Must keep error handling intact
