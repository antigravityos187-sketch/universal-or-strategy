# Phase 1: Scope Boundary - EPIC-W7-132

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: TBD
- **API Key**: Plan Mode
- **Execution Time**: 2026-06-24T01:36:38Z

## Epic Objective
Reduce cyclomatic complexity of `SymmetryNormalizeTradeType` from CYC=10 to CYC≤8 (Jane Street strict standard).

## Target Method
- **Method**: SymmetryNormalizeTradeType
- **File**: src/V12_002.Symmetry.Replace.cs
- **Line**: 322
- **Current CYC**: 10
- **Target CYC**: ≤8

## IN SCOPE

### Primary Extraction Target
1. **SymmetryNormalizeTradeType method** (src/V12_002.Symmetry.Replace.cs:322)
   - Extract conditional branching logic into helper methods
   - Reduce from CYC=10 to CYC≤8
   - Maintain single caller relationship with SymmetryInferTradeType

### Allowed Modifications
1. **Create new private helper methods** in same file
   - Extract trade type normalization logic
   - Extract conditional branches
   - Keep methods private (no API surface changes)

2. **Refactor internal logic**
   - Simplify decision tree
   - Extract switch/if-else chains
   - Improve readability while preserving behavior

3. **Add unit tests** (tests/V12_Performance.Tests/)
   - Test extracted helper methods
   - Verify behavior preservation
   - Cover all decision branches

### Risk Mitigation
- **Zero blast radius**: No external dependencies to break
- **Single caller**: Only SymmetryInferTradeType affected
- **Leaf node**: No downstream calls to coordinate

## OUT OF SCOPE

### Explicitly Excluded
1. **SymmetryInferTradeType method** (caller)
   - Do NOT modify the caller method
   - Do NOT change its signature or behavior
   - Only the target method should be refactored

2. **Public API changes**
   - Do NOT change method signatures
   - Do NOT expose new public methods
   - Keep all extractions private

3. **Behavioral changes**
   - Do NOT alter trade type normalization logic
   - Do NOT change return values
   - Do NOT modify error handling

4. **Other Symmetry methods**
   - Do NOT refactor other methods in V12_002.Symmetry.Replace.cs
   - Do NOT touch SymmetryInferTradeType
   - Do NOT modify unrelated symmetry logic

5. **Cross-file changes**
   - Do NOT modify other .cs files
   - Do NOT change imports/dependencies
   - Keep changes isolated to target file

### Scope Creep Prevention
- **One method, one epic**: Only SymmetryNormalizeTradeType
- **No "while we're here" fixes**: Resist temptation to fix nearby code
- **No pre-existing issues**: Do NOT fix unrelated compilation errors
- **Separate PRs**: Any other issues require separate epics

## Scope Validation

### Boundary Checks
✅ **Target method identified**: SymmetryNormalizeTradeType (CYC=10)
✅ **Risk assessed**: LOW (zero blast radius, single caller, leaf node)
✅ **Threshold violation confirmed**: CYC=10 > 8
✅ **Extraction feasible**: 10 decision points to simplify
✅ **No external dependencies**: Safe to refactor in isolation

### Jane Street Alignment
- **Cognitive simplicity**: Reduce decision branches for microsecond-latency reasoning
- **Exhaustive testing**: Simpler methods = easier to test all paths
- **Race condition auditing**: Lower complexity = easier to verify lock-free correctness

## Success Criteria
1. ✅ SymmetryNormalizeTradeType reduced to CYC≤8
2. ✅ All unit tests pass (existing + new)
3. ✅ Build succeeds (dotnet build)
4. ✅ deploy-sync.ps1 executes successfully
5. ✅ F5 in NinjaTrader loads strategy
6. ✅ No behavioral changes (output identical)
7. ✅ Zero blast radius maintained (no caller changes)

## Phase 1 Completion
- **Scope defined**: ✅
- **Boundaries validated**: ✅
- **Risk assessed**: LOW
- **Ready for Phase 2**: ✅

---

**Next Phase**: Phase 2 (Architecture Planning) - Design extraction strategy
