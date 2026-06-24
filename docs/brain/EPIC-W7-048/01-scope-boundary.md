# Phase 1.5: Scope Boundary Validation - EPIC-W7-048

## Agent Tracking
- **Agent Name**: v12-phase1-5-boundary
- **Execution Time**: 2026-06-24T00:01:17Z
- **Input**: docs/brain/EPIC-W7-048/00-scope.md

## Boundary Validation Result: APPROVED

### Scope Clarity Assessment

**IN SCOPE Boundaries**: CLEAR
- Single method target: UpdateExistingPendingReplacement
- 4 extraction targets with specific line ranges
- CYC reduction goal: 15 to 8 or less
- Nesting reduction goal: 6 to 3 or less

**OUT OF SCOPE Boundaries**: CLEAR
- Caller method (UpdateStopOrder) explicitly excluded
- Callee methods (16 dependencies) explicitly excluded
- External files (5 files) explicitly excluded
- Test files explicitly excluded (noted as technical debt)

### Scope Creep Risk Analysis

**Risk Level**: LOW

**Potential Creep Vectors**:
1. **Caller Refactoring** - UpdateStopOrder (CYC unknown)
   - Mitigation: Explicitly OUT OF SCOPE
   - Separate epic if needed

2. **Callee Refactoring** - CaptureTargetSnapshot, RefreshTargetSnapshot
   - Mitigation: Only refactor if they independently exceed threshold
   - Not part of this epic

3. **External File Changes** - pendingStopReplacements, MarkStickyDirty
   - Mitigation: Zero blast radius confirmed
   - No changes needed outside target file

4. **Test Coverage** - No existing tests
   - Mitigation: Noted as technical debt, not blocking
   - Manual F5 testing sufficient for this epic

**Verdict**: No scope creep risks identified. Boundaries are well-defined and defensible.

### Extraction Strategy Validation

**Strategy**: Extract 4 helper methods from UpdateExistingPendingReplacement

**Block 1: Snapshot Management** (lines 170-185)
- Target CYC: 3 or less
- Extraction: CaptureTargetSnapshot + RefreshTargetSnapshot logic
- Risk: LOW (isolated conditional block)

**Block 2: Pending Replacement Validation** (lines 186-210)
- Target CYC: 3 or less
- Extraction: Dictionary lookup + validation
- Risk: LOW (clear entry/exit points)

**Block 3: Price Comparison** (lines 211-230)
- Target CYC: 4 or less
- Extraction: Stop price + trail level comparison
- Risk: MEDIUM (complex conditionals, needs careful extraction)

**Block 4: Replacement Update** (lines 231-250)
- Target CYC: 3 or less
- Extraction: Update logic + MarkStickyDirty
- Risk: LOW (straightforward update block)

**Total Extracted CYC**: 13 or less (3+3+4+3)
**Remaining Main Method CYC**: 2 or less (orchestration only)
**Combined CYC**: 8 or less (meets Jane Street threshold)

### Jane Street Alignment Check

**Principle 1: Cognitive Simplicity**
- CYC 8 or less per method (strict threshold)
- Nesting 3 or less levels (reduced from 6)
- Single responsibility per extracted method

**Principle 2: Correctness by Construction**
- No signature changes (illegal states prevented)
- Behavior preservation (no logic changes)
- Type safety maintained

**Principle 3: Lock-Free Pattern**
- No lock() blocks in target method
- FSM/Actor pattern compliance verified

**Principle 4: ASCII-Only**
- No Unicode/emoji in target file
- Compliance verified in scope definition

### Blast Radius Confirmation

**Direct Dependents**: 0
**Importer Count**: 0
**Risk Score**: 0.0 (ISOLATED)
**Single Caller**: UpdateStopOrder

**Verdict**: Zero blast radius confirmed. Extraction is safe and isolated.

### Dependency Preservation Checklist

**16 Callee Dependencies to Preserve**:
1. CaptureTargetSnapshot (src/V12_002.Trailing.StopUpdate.cs:255)
2. RefreshTargetSnapshot (src/V12_002.Trailing.StopUpdate.cs:281)
3. pendingStopReplacements (src/V12_002.cs:210)
4. MarkStickyDirty (src/V12_002.StickyState.cs:619)
5. GetTargetOrdersDictionary (src/V12_002.UI.Callbacks.cs:1039)
6. LogBuffer.Format (src/V12_002.Perf.LogBuffer.cs:28)
7-16. (10 additional dependencies listed in scope)

**Preservation Strategy**: All dependencies remain in extracted methods, no signature changes.

### Success Criteria Validation

**Functional Requirements**: CLEAR
- All 16 callee relationships preserved
- Single caller relationship maintained
- Method signature unchanged
- Behavior identical (no logic changes)

**Quality Requirements**: CLEAR
- CYC 8 or less per method (Jane Street threshold)
- Max nesting 3 or less levels
- ASCII-only compliance
- Lock-free pattern compliance

**Verification Requirements**: CLEAR
- dotnet build succeeds
- deploy-sync.ps1 succeeds
- F5 in NinjaTrader succeeds
- BUILD_TAG appears in output

### Boundary Decision Matrix

| Concern | IN SCOPE | OUT OF SCOPE | Rationale |
|---------|----------|--------------|-----------|
| UpdateExistingPendingReplacement | YES | | Primary target (CYC 15) |
| 4 Helper Extractions | YES | | Complexity reduction strategy |
| UpdateStopOrder (caller) | | YES | Separate epic if needed |
| CaptureTargetSnapshot (callee) | | YES | Only if independently exceeds threshold |
| RefreshTargetSnapshot (callee) | | YES | Only if independently exceeds threshold |
| MarkStickyDirty (callee) | | YES | Only if independently exceeds threshold |
| External files (5 files) | | YES | Zero blast radius |
| Test files | | YES | Technical debt (not blocking) |

### Scope Creep Prevention Measures

**Measure 1: Single Method Focus**
- Only UpdateExistingPendingReplacement is modified
- No changes to caller or callees
- No changes to external files

**Measure 2: Extraction-Only Strategy**
- No logic changes (behavior preservation)
- No signature changes (API stability)
- No dependency changes (relationship preservation)

**Measure 3: Clear Exit Criteria**
- CYC 8 or less achieved
- Nesting 3 or less achieved
- Build + F5 successful
- No additional work beyond these criteria

**Measure 4: Explicit OUT OF SCOPE List**
- Caller method documented as separate epic
- Callee methods documented as conditional refactors
- External files documented as zero-change
- Test coverage documented as technical debt

### Final Boundary Validation

**Question 1**: Is the scope too narrow?
- **Answer**: NO. Single method focus is appropriate for CYC 15 target.

**Question 2**: Is the scope too broad?
- **Answer**: NO. 4 extractions are minimal for CYC 15 to 8 or less reduction.

**Question 3**: Are boundaries defensible?
- **Answer**: YES. Clear IN/OUT SCOPE with explicit rationale.

**Question 4**: Can scope creep occur?
- **Answer**: NO. Prevention measures in place, exit criteria clear.

**Question 5**: Is Jane Street alignment verified?
- **Answer**: YES. All 4 principles validated.

## Phase 1.5 Verdict: BOUNDARIES APPROVED

**Recommendation**: Proceed to Phase 2 (Architecture Planning)

**Rationale**:
- Scope boundaries are clear and defensible
- No scope creep risks identified
- Extraction strategy is sound
- Jane Street alignment confirmed
- Zero blast radius verified
- Success criteria are measurable

**Next Phase**: Phase 2 (Architecture Planning)
- Design 4 helper method signatures
- Plan extraction sequence
- Identify refactoring patterns
- Generate Mermaid diagrams