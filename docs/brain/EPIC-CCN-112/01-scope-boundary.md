# Phase 1.5: Scope Boundary Validation - EPIC-CCN-112

## Epic Overview
- **Epic ID**: EPIC-CCN-112
- **Target Method**: ClassifyOrderByPrefix
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Current Complexity**: 17
- **Target Complexity**: ≤ 15 (Jane Street alignment)
- **Risk Level**: MEDIUM

## Scope Boundary Definition

### IN SCOPE (Single Method Extraction)

#### Primary Target
- **Method**: `ClassifyOrderByPrefix`
- **Location**: src/V12_002.SIMA.Lifecycle.cs
- **Lines**: TBD (to be determined during implementation)
- **Complexity Reduction**: 17 → ≤ 10 (target)

#### Extraction Strategy
1. **Prefix Mapping Extraction**
   - Extract all prefix-to-classification mappings
   - Create immutable Dictionary<string, OrderClassification>
   - Initialize at class construction or static initialization
   - Preserve exact classification logic

2. **Method Simplification**
   - Replace conditional branching with dictionary lookup
   - Maintain identical input/output contract
   - Preserve lock-free semantics
   - Keep method signature unchanged

3. **Test Coverage**
   - Unit tests for all prefix mappings
   - Validation of classification correctness
   - Performance regression tests
   - Concurrency safety tests

### OUT OF SCOPE (V12.23 No Scope Creep Protocol)

#### Explicitly Excluded
- ❌ Adjacent method refactoring
- ❌ Caller modifications
- ❌ State machine redesign
- ❌ Performance optimization beyond complexity reduction
- ❌ API contract changes
- ❌ Additional feature additions
- ❌ Logging/monitoring enhancements
- ❌ Error handling improvements (unless required for correctness)

#### Boundary Enforcement
- **File Limit**: 1 file (src/V12_002.SIMA.Lifecycle.cs)
- **Method Limit**: 1 method (ClassifyOrderByPrefix)
- **Line Impact**: Estimated 20-30 lines
- **Test Files**: New test file allowed for extracted logic

## Extraction Details

### Current Method Structure (Inferred)
```csharp
// Pseudo-code based on complexity analysis
OrderClassification ClassifyOrderByPrefix(string orderPrefix)
{
    if (orderPrefix.StartsWith("A")) return ClassificationA;
    if (orderPrefix.StartsWith("B")) return ClassificationB;
    // ... 15+ more conditional branches
    return DefaultClassification;
}
```

### Target Structure
```csharp
// Static lookup table (class-level)
private static readonly Dictionary<string, OrderClassification> PrefixClassifications = 
    new Dictionary<string, OrderClassification>
    {
        { "A", ClassificationA },
        { "B", ClassificationB },
        // ... all mappings
    };

// Simplified method (CYC ≤ 3)
OrderClassification ClassifyOrderByPrefix(string orderPrefix)
{
    return PrefixClassifications.TryGetValue(orderPrefix, out var classification)
        ? classification
        : DefaultClassification;
}
```

### Complexity Reduction
- **Before**: 17 branches (if/else chain)
- **After**: 2-3 branches (TryGetValue + ternary)
- **Reduction**: ~85% complexity decrease
- **Lookup**: O(1) vs O(n) average case

## Success Criteria

### Mandatory Requirements
1. ✅ **Complexity Target**: Method complexity ≤ 15 (target ≤ 10)
2. ✅ **Behavioral Equivalence**: All existing tests pass
3. ✅ **Lock-Free Correctness**: No new synchronization primitives
4. ✅ **Single Method Scope**: Only ClassifyOrderByPrefix modified
5. ✅ **No API Changes**: Method signature unchanged

### Validation Gates
- [ ] Cyclomatic complexity measured ≤ 15
- [ ] All unit tests pass (existing + new)
- [ ] No performance regression (< 5% overhead)
- [ ] Code review approval
- [ ] Static analysis clean (no new warnings)

## Risk Assessment

### Technical Risks
| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Dictionary initialization overhead | LOW | LOW | Static initialization, one-time cost |
| Prefix collision | LOW | MEDIUM | Validate unique prefixes in tests |
| Memory footprint increase | LOW | LOW | Dictionary is small (< 20 entries) |
| Thread safety regression | LOW | HIGH | Immutable dictionary, no locks needed |

### Scope Creep Risks
| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Adjacent method refactoring | MEDIUM | HIGH | Strict boundary enforcement |
| Caller modifications | MEDIUM | HIGH | Document as out-of-scope |
| Performance optimization | LOW | MEDIUM | Defer to separate epic |

### Mitigation Strategy
- **Boundary Enforcement**: Code review checklist
- **Test Coverage**: 100% branch coverage for extracted logic
- **Rollback Plan**: Git revert if complexity target not met

## Implementation Constraints

### V12 DNA Alignment
- **Cognitive Simplicity**: Dictionary lookup is self-documenting
- **Type Safety**: Enum-based classification (no magic strings)
- **Immutability**: Static readonly dictionary
- **Testability**: Isolated prefix mapping logic

### Jane Street Principles
- **Complexity ≤ 15**: Primary goal
- **Make Illegal States Unrepresentable**: Type-safe classification
- **Prefer Composition**: Dictionary composition over branching
- **Lock-Free Correctness**: Immutable data structure

## Blast Radius

### Direct Impact
- **Files Modified**: 1 (src/V12_002.SIMA.Lifecycle.cs)
- **Methods Modified**: 1 (ClassifyOrderByPrefix)
- **Lines Changed**: ~20-30 (estimated)
- **Test Files Added**: 1 (ClassifyOrderByPrefixTests.cs)

### Indirect Impact
- **Callers**: 0 (no signature changes)
- **Dependencies**: 0 (no new dependencies)
- **State Machine**: 0 (no state changes)

### Containment Strategy
- Single method extraction
- No API surface changes
- Backward compatible
- Isolated test coverage

## Phase 1.5 Deliverables

### Required Outputs
1. ✅ This document (01-scope-boundary.md)
2. ✅ Updated manifest.json (phase 1.5 completed)

### Next Phase Inputs
- Scope boundary definition → Phase 2 (Vision/Spec)
- Extraction strategy → Phase 3 (Implementation Plan)
- Success criteria → Phase 4 (Validation)

## Approval Checklist

### Scope Validation
- [x] Single method target identified
- [x] Extraction strategy defined
- [x] Boundary explicitly stated
- [x] Success criteria measurable
- [x] Risk assessment complete

### V12.23 Protocol Compliance
- [x] No scope creep (single method only)
- [x] Complexity target ≤ 15
- [x] Jane Street alignment
- [x] Lock-free correctness preserved
- [x] Blast radius contained

---

**Document Status**: APPROVED
**Phase**: 1.5 (Scope Boundary Validation)
**Date**: 2026-06-13
**Next Phase**: 2 (Vision/Spec)
**Epic**: EPIC-CCN-112
