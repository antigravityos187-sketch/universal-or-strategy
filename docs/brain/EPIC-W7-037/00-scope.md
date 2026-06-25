# Phase 1: Scope Definition - EPIC-W7-037

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:28:10Z
- **Input**: docs/brain/EPIC-W7-037/00-hotspots.md

## Target Method
- **Method**: SymmetryNormalizeTradeType
- **File**: src/V12_002.Symmetry.Replace.cs
- **Line**: 322
- **Current CYC**: 10
- **Target CYC**: ≤8 (Jane Street threshold)

## Scope Boundary Definition

### IN SCOPE ✅

**Primary Target**:
- `SymmetryNormalizeTradeType` method (lines 322-342, estimated)
- Conditional branching logic for trade type normalization
- String comparison/mapping logic

**Extraction Strategy**:
1. Extract trade type mapping logic to lookup table/dictionary
2. Create helper methods for trade type category validation
3. Reduce branching complexity from CYC=10 to CYC≤8

**Allowed Changes**:
- Refactor conditional branches to reduce complexity
- Introduce private helper methods within same file
- Add lookup table/dictionary for trade type mappings
- Preserve method signature: `private ? SymmetryNormalizeTradeType(?)`
- Maintain single-parameter interface

**Testing Requirements**:
- Unit tests for each normalized trade type case
- Edge case tests for malformed inputs
- Regression test for caller (SymmetryInferTradeType)

### OUT OF SCOPE ❌

**Caller Method**:
- `SymmetryInferTradeType` (line 304) - DO NOT MODIFY
- Caller behavior must remain unchanged
- No signature changes to target method

**Other Methods**:
- Any other methods in V12_002.Symmetry.Replace.cs
- Methods in other files
- Global state or class-level fields

**Infrastructure**:
- No changes to file structure
- No new files created
- No changes to build configuration

**Cross-Cutting Concerns**:
- No logging changes
- No error handling pattern changes
- No performance optimization beyond complexity reduction

## Scope Validation

**Blast Radius Check** ✅:
- Importer count: 0
- Direct dependents: 0
- Overall risk score: 0.0
- **VERDICT**: Isolated method, safe for refactoring

**Call Hierarchy Check** ✅:
- Single caller: SymmetryInferTradeType
- Zero callees (leaf method)
- **VERDICT**: Shallow coupling, predictable impact

**Complexity Check** ⚠️:
- Current CYC: 10
- Target CYC: ≤8
- **VERDICT**: Requires extraction to meet threshold

## Success Criteria

**Phase 2 (Architecture Planning) Prerequisites**:
1. ✅ Scope boundaries clearly defined (IN/OUT)
2. ✅ Blast radius validated (zero external dependencies)
3. ✅ Call hierarchy mapped (single caller, leaf method)
4. ✅ Extraction strategy identified (lookup table pattern)
5. ✅ Testing requirements specified

**Phase 5 (Ticket Execution) Constraints**:
- Method signature MUST remain unchanged
- Caller behavior MUST remain unchanged
- CYC MUST be reduced to ≤8
- No new files created
- All changes within src/V12_002.Symmetry.Replace.cs

## Risk Mitigation

**Identified Risks**:
1. **String mapping logic**: May have edge cases not visible in static analysis
   - Mitigation: Comprehensive unit tests for all trade type variants
   
2. **Caller dependency**: SymmetryInferTradeType expects specific return behavior
   - Mitigation: Regression test to verify caller behavior unchanged

3. **Complexity reduction**: May require multiple helper methods
   - Mitigation: Keep helpers private, maintain cohesion within file

**Rollback Plan**:
- Single file change (V12_002.Symmetry.Replace.cs)
- Git revert available
- No cross-file dependencies to untangle

## Scope Approval

**Scope Status**: APPROVED ✅

**Rationale**:
- Well-bounded target (20 lines, single method)
- Zero blast radius (isolated method)
- Clear extraction strategy (lookup table pattern)
- Low risk (single caller, leaf method)
- Achievable CYC reduction (10→≤8)

**Next Phase**: Phase 2 (Architecture Planning)
- Design lookup table structure
- Plan helper method signatures
- Define test cases for all trade type mappings
