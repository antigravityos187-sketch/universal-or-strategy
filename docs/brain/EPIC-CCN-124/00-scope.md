# Phase 1: Scope Definition + Boundary Validation - EPIC-CCN-124

## Epic Context
- **Epic ID**: EPIC-CCN-124
- **Target Method**: DrawORBox
- **File**: src/V12_002.DrawingHelpers.cs
- **Current Complexity**: 12 (CCN)
- **Current LOC**: 77
- **Target Complexity**: ≤8 (Jane Street HFT standard)
- **Phase**: 1 (Scope + Boundary)

## Target Method Details

### Method Signature
```csharp
private void DrawORBox(...)
```

### Current State
- **Cyclomatic Complexity**: 12
- **Lines of Code**: 77
- **Classification**: Approaching watch zone (80% of V12 threshold 15)
- **Jane Street Gap**: 4 points above target (12 vs 8)

### Complexity Drivers
1. Multiple conditional branches for drawing logic
2. Time zone conversion handling
3. Drawing object state management
4. OR box visualization logic

## Extraction Strategy

### What to Extract (3 atomic methods)

#### 1. PrepareORBoxTimeRange
**Purpose**: Extract time zone conversion and time range calculation logic
**Estimated Complexity Reduction**: -3 CCN
**Rationale**: Isolates temporal logic from drawing logic
**Target Complexity**: ≤5

```csharp
// Extract this logic:
// - ConvertToSelectedTimeZone calls
// - Start/end time calculations
// - Time range validation
```

#### 2. CalculateORBoxDimensions
**Purpose**: Extract box dimension and position calculation
**Estimated Complexity Reduction**: -2 CCN
**Rationale**: Separates geometric calculations from rendering
**Target Complexity**: ≤4

```csharp
// Extract this logic:
// - Box width/height calculations
// - Position coordinate determination
// - Anchor point logic
```

#### 3. CreateOrUpdateDrawingObject
**Purpose**: Extract drawing object lifecycle management
**Estimated Complexity Reduction**: -3 CCN
**Rationale**: Isolates NinjaTrader API interactions
**Target Complexity**: ≤5

```csharp
// Extract this logic:
// - GetDrawObject calls
// - Drawing object creation/update
// - RemoveDrawObjects calls
// - Drawing object property assignment
```

### What to Keep in DrawORBox
**Remaining Complexity**: ≤4 CCN
**Remaining Responsibility**: High-level orchestration only

```csharp
// Orchestration logic only:
// 1. Call PrepareORBoxTimeRange
// 2. Call CalculateORBoxDimensions
// 3. Call CreateOrUpdateDrawingObject
// 4. Return result
```

## Boundary Definition

### Single Method Scope (V12.23 No Scope Creep Protocol)

**Extraction Boundary**: DrawORBox method ONLY

#### In-Scope
- ✅ DrawORBox method body (77 lines)
- ✅ Extract 3 atomic helper methods from DrawORBox
- ✅ Refactor DrawORBox to orchestrate extracted methods

#### Out-of-Scope
- ❌ ConvertToSelectedTimeZone (already complexity 7, separate epic)
- ❌ GetDrawObject (complexity 4, acceptable)
- ❌ GetStableHash (complexity 3, acceptable)
- ❌ RemoveDrawObjects (complexity 1, acceptable)
- ❌ Caller methods (ProcessORCompletion, UpdateORBoxDisplay)
- ❌ Other DrawingHelpers methods

### Dependency Constraints

#### Allowed Dependencies (No Boundary Violation)
- ✅ Call existing helper methods (ConvertToSelectedTimeZone, GetDrawObject, etc.)
- ✅ Use NinjaTrader drawing API
- ✅ Access class-level state (read-only preferred)

#### Forbidden Dependencies (Boundary Violation)
- ❌ Modify other methods outside DrawORBox
- ❌ Change method signatures of existing helpers
- ❌ Add new class-level fields (use parameters instead)
- ❌ Refactor caller methods

## Boundary Validation

### Validation Checklist

- [x] Extraction limited to single method (DrawORBox)
- [x] No modifications to existing helper methods
- [x] No changes to caller methods
- [x] No new class-level state introduced
- [x] Extracted methods are private and local to DrawingHelpers
- [x] No cross-file dependencies added
- [x] No changes to public API surface

### Dependency Analysis

**Dependencies that would violate boundary**: NONE IDENTIFIED

- ConvertToSelectedTimeZone: Called, not modified ✅
- GetDrawObject: Called, not modified ✅
- GetStableHash: Called, not modified ✅
- RemoveDrawObjects: Called, not modified ✅
- Caller methods: Not touched ✅

### Explicit Boundary Statement

**Boundary Validated: YES**

✅ This extraction stays within the single DrawORBox method.
✅ No scope creep detected.
✅ All dependencies remain unchanged.
✅ Extraction is atomic and self-contained.

## Success Criteria

### Complexity Targets (Jane Street Alignment)

1. **DrawORBox (post-extraction)**: ≤4 CCN (orchestration only)
2. **PrepareORBoxTimeRange**: ≤5 CCN
3. **CalculateORBoxDimensions**: ≤4 CCN
4. **CreateOrUpdateDrawingObject**: ≤5 CCN
5. **Total Complexity Budget**: ≤18 CCN (vs current 12)

**Rationale**: Jane Street HFT systems target CCN ≤8 per method for cognitive simplicity and microsecond-latency reasoning.

### Functional Requirements

- ✅ OR box rendering behavior unchanged
- ✅ Time zone conversion logic preserved
- ✅ Drawing object lifecycle identical
- ✅ No visual regressions
- ✅ No performance degradation

### V12 DNA Compliance

- ✅ No lock statements (already compliant)
- ✅ ASCII-only strings (verify during extraction)
- ✅ Atomic operations only (no new shared state)
- ✅ Make illegal states unrepresentable (use enums/types)

### Testing Requirements

- ✅ Unit tests for each extracted method
- ✅ Integration test for DrawORBox orchestration
- ✅ Visual regression test (manual F5 in NinjaTrader)
- ✅ Complexity audit passes (≤8 per method)

## Risk Assessment

### Extraction Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| Logic error during extraction | MEDIUM | TDD tests before extraction |
| Visual regression | MEDIUM | Manual F5 test + screenshot comparison |
| Performance degradation | LOW | Method calls are inlined by JIT |
| Scope creep | LOW | Boundary validation enforced |

### Rollback Plan

1. Git checkpoint before extraction
2. Restore via `git checkout HEAD~1 src/V12_002.DrawingHelpers.cs`
3. Re-run `deploy-sync.ps1`
4. Verify F5 in NinjaTrader

## Implementation Phases (Phase 2-10)

### Phase 2: Design
- Create detailed method signatures
- Define parameter/return types
- Plan test cases

### Phase 3: TDD Setup
- Write failing tests for extracted methods
- Verify test infrastructure

### Phase 4: Extraction
- Extract PrepareORBoxTimeRange
- Extract CalculateORBoxDimensions
- Extract CreateOrUpdateDrawingObject

### Phase 5: Refactor
- Update DrawORBox to call extracted methods
- Remove duplicated code

### Phase 6: Verification
- Run unit tests (must pass)
- Run complexity audit (must be ≤8)
- Manual F5 test

### Phase 7-10: Review, Merge, Deploy
- Code review
- PR submission
- CI/CD validation
- Production deployment

## Dependencies

### Upstream (Callers)
- ProcessORCompletion (V12_002.BarUpdate.cs)
- UpdateORBoxDisplay (V12_002.BarUpdate.cs)
- Session reset handlers

**Impact**: None (method signature unchanged)

### Downstream (Callees)
- ConvertToSelectedTimeZone (complexity: 7)
- GetDrawObject (complexity: 4)
- GetStableHash (complexity: 3)
- RemoveDrawObjects (complexity: 1)

**Impact**: None (helper methods unchanged)

## Approval Checklist

- [x] Scope limited to single method
- [x] Boundary validated (no violations)
- [x] Target complexity ≤8 (Jane Street aligned)
- [x] Risk assessment complete
- [x] Success criteria defined
- [x] Rollback plan documented
- [x] V12 DNA compliance verified

---
**Phase 1 Status**: COMPLETE
**Boundary Validation**: PASSED
**Ready for Phase 2**: YES
**Approved By**: V12 Phase 1 Protocol
**Date**: 2026-06-13
