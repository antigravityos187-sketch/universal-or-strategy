# Phase 1: Scope Definition - EPIC-CCN-112

## Target Method Details

### Method Identification
- **Method Name**: `ClassifyOrderByPrefix`
- **File Path**: `src/V12_002.SIMA.Lifecycle.cs`
- **Current Complexity**: 17 (CYC)
- **Target Complexity**: ≤ 15 (V12 threshold)
- **Overage**: +2 (13% over threshold)

### Method Purpose
Order classification by prefix matching - determines order routing logic and affects SIMA state machine transitions.

### Complexity Drivers
1. **Multiple Prefix Comparisons**: 17 conditional branches for prefix matching
2. **Linear Branching Logic**: O(n) complexity for classification
3. **Cognitive Load**: 2^17 = 131,072 theoretical execution paths

## Extraction Strategy

### What to Extract

#### Primary Extraction Target
**Prefix-to-Classification Mapping Logic**

```csharp
// BEFORE (Complexity 17):
// if (prefix == "A") return ClassA;
// else if (prefix == "B") return ClassB;
// ... (15+ more branches)

// AFTER (Complexity ≤ 10):
// return _prefixClassificationMap.GetValueOrDefault(prefix, DefaultClass);
```

#### Extraction Components
1. **Static Lookup Table**: Dictionary<string, OrderClassification>
2. **Initialization Logic**: Populate map in static constructor
3. **Fallback Handler**: Default classification for unknown prefixes

### What to Keep in Original Method

1. **Method Signature**: Preserve public API contract
2. **Validation Logic**: Null/empty prefix checks (if any)
3. **Logging/Telemetry**: Existing instrumentation hooks
4. **State Machine Integration**: SIMA lifecycle coupling points

### Refactoring Pattern

**Strategy**: Dictionary Lookup (O(1) vs O(n))

```csharp
private static readonly Dictionary<string, OrderClassification> PrefixMap = new()
{
    ["A"] = OrderClassification.TypeA,
    ["B"] = OrderClassification.TypeB,
    // ... remaining mappings
};

public OrderClassification ClassifyOrderByPrefix(string prefix)
{
    // Validation (if needed)
    if (string.IsNullOrEmpty(prefix))
        return OrderClassification.Unknown;
    
    // O(1) lookup replaces O(n) branching
    return PrefixMap.GetValueOrDefault(prefix, OrderClassification.Unknown);
}
```

**Expected Complexity Reduction**: 17 → 5-8 (validation + lookup + fallback)

## Boundary Definition

### Scope Constraints (V12.23 No Scope Creep Protocol)

#### IN SCOPE
- ✅ **Single Method**: `ClassifyOrderByPrefix` only
- ✅ **Prefix Mapping Extraction**: Move branching logic to lookup table
- ✅ **Unit Tests**: Add TDD tests for all prefix mappings
- ✅ **Complexity Reduction**: Target CYC ≤ 15 (stretch goal: ≤ 10)

#### OUT OF SCOPE
- ❌ **Adjacent Methods**: No changes to other SIMA lifecycle methods
- ❌ **Caller Refactoring**: No modifications to upstream/downstream code
- ❌ **State Machine Redesign**: SIMA FSM logic remains unchanged
- ❌ **Performance Optimization**: Focus on complexity, not speed
- ❌ **API Changes**: Method signature must remain identical

### Blast Radius Containment

**Affected Files**: 1 (src/V12_002.SIMA.Lifecycle.cs)
**Affected Methods**: 1 (ClassifyOrderByPrefix)
**Affected Lines**: ~20-30 (estimated)

**Isolation Strategy**:
- Extract to private static field (no new classes)
- Preserve method signature (no breaking changes)
- Maintain lock-free correctness (readonly dictionary)

## Success Criteria

### Primary Goals

1. **Complexity Threshold**: CYC ≤ 15 (MANDATORY)
   - Stretch Goal: CYC ≤ 10 (Jane Street best practice)
   - Measurement: `complexity_audit.py` post-refactoring

2. **Correctness Preservation**: 100% behavioral equivalence
   - All existing prefix mappings preserved
   - Fallback logic identical to original
   - No state machine regressions

3. **Test Coverage**: 100% path coverage
   - Unit test for each prefix mapping
   - Edge case tests (null, empty, unknown prefix)
   - Integration test with SIMA lifecycle

4. **Build Success**: Zero compilation errors
   - `dotnet build` passes
   - `deploy-sync.ps1` succeeds (hard-link sync)
   - NinjaTrader F5 test passes

### Quality Gates

| Gate | Tool | Threshold | Status |
|------|------|-----------|--------|
| Complexity | complexity_audit.py | CYC ≤ 15 | ⏳ Pending |
| Build | dotnet build | Zero errors | ⏳ Pending |
| Tests | dotnet test | 100% pass | ⏳ Pending |
| Formatting | CSharpier | Zero issues | ⏳ Pending |
| Lint | Roslyn | Zero violations | ⏳ Pending |

### Verification Protocol

1. **Pre-Refactoring Baseline**:
   - Run `complexity_audit.py` → confirm CYC = 17
   - Run `dotnet test` → capture baseline pass rate

2. **Post-Refactoring Validation**:
   - Run `complexity_audit.py` → verify CYC ≤ 15
   - Run `dotnet test` → confirm 100% pass (no regressions)
   - Run `deploy-sync.ps1` → sync NinjaTrader hard links
   - F5 in NinjaTrader → manual smoke test

3. **Rollback Criteria**:
   - If CYC > 15: ABORT and revert
   - If tests fail: ABORT and revert
   - If build breaks: ABORT and revert

## Risk Assessment

### Risk Level: MEDIUM

#### Risk Factors

1. **Critical Path Impact** (HIGH)
   - Method is in SIMA lifecycle hot path
   - Order routing depends on classification correctness
   - State machine transitions coupled to this logic

2. **Complexity Overage** (MEDIUM)
   - +2 over threshold (13% overage)
   - Moderate refactoring required (not trivial)
   - Dictionary lookup introduces new data structure

3. **Lock-Free Correctness** (MEDIUM)
   - Must preserve atomic guarantees
   - Readonly dictionary ensures thread safety
   - No new locks introduced (V12 DNA mandate)

4. **Test Coverage Gap** (MEDIUM)
   - Current test coverage unknown
   - 131k theoretical paths → need comprehensive tests
   - TDD approach mitigates risk

#### Mitigation Strategies

1. **TDD Approach**: Write tests BEFORE refactoring
   - Capture all existing prefix mappings in tests
   - Add edge case tests (null, empty, unknown)
   - Verify behavioral equivalence

2. **Incremental Extraction**: Small, verifiable steps
   - Step 1: Add static dictionary (no logic change)
   - Step 2: Replace first branch with lookup
   - Step 3: Replace remaining branches
   - Step 4: Remove dead code

3. **Blast Radius Containment**: Single-method scope
   - No changes to callers
   - No changes to state machine
   - No API modifications

4. **Rollback Plan**: Git checkpoint before each step
   - Bob CLI checkpointing enabled
   - Manual git tag before refactoring
   - Revert script ready if needed

### Risk Matrix

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Build Break | LOW | HIGH | TDD + incremental steps |
| Test Regression | MEDIUM | HIGH | Comprehensive test suite |
| Performance Degradation | LOW | MEDIUM | Dictionary lookup is O(1) |
| State Machine Bug | LOW | CRITICAL | Integration tests + manual F5 |
| Scope Creep | LOW | MEDIUM | V12.23 protocol enforcement |

## V12 DNA Alignment

### Current Violations
- ❌ **Complexity**: 17 > 15 (Jane Street threshold)
- ⚠️ **Cognitive Simplicity**: 131k theoretical paths

### Post-Refactoring Compliance
- ✅ **Complexity**: ≤ 15 (target: ≤ 10)
- ✅ **Lock-Free**: Readonly dictionary (no locks)
- ✅ **ASCII-Only**: No Unicode in string literals
- ✅ **Correctness by Construction**: Type-safe lookup
- ✅ **Testability**: Linear test paths (not exponential)

## Implementation Notes

### Key Decisions

1. **Dictionary vs Strategy Pattern**:
   - **Choice**: Dictionary lookup
   - **Rationale**: Simpler, O(1) performance, no new classes
   - **Trade-off**: Less extensible than Strategy, but sufficient for static mappings

2. **Static vs Instance Field**:
   - **Choice**: Static readonly field
   - **Rationale**: Thread-safe, no allocation overhead, immutable
   - **Trade-off**: Cannot be mocked (acceptable for pure lookup)

3. **Fallback Strategy**:
   - **Choice**: `GetValueOrDefault(prefix, OrderClassification.Unknown)`
   - **Rationale**: Explicit default, no exceptions
   - **Trade-off**: Silent failure (acceptable if logged)

### Code Review Checklist

- [ ] Complexity ≤ 15 (verified by complexity_audit.py)
- [ ] All prefix mappings preserved
- [ ] Fallback logic identical to original
- [ ] No new locks introduced
- [ ] ASCII-only strings
- [ ] Unit tests cover all paths
- [ ] Build passes (dotnet build)
- [ ] Tests pass (dotnet test)
- [ ] Hard links synced (deploy-sync.ps1)
- [ ] NinjaTrader F5 test passes

## Next Steps (Phase 2)

1. **Architecture Planning**: Design dictionary structure
2. **TDD Test Suite**: Write tests for all prefix mappings
3. **Incremental Extraction**: Replace branches with lookup
4. **Verification**: Run full validation protocol

---

**Scope Defined**: 2026-06-13
**Analyst**: V12 Phase 1 Scope Planner
**Epic**: EPIC-CCN-112
**Phase**: 1 (Scope Definition)
**Status**: ✅ APPROVED (Single-method scope, no creep)
