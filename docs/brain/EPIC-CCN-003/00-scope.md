# Phase 1: Scope Definition - EPIC-CCN-003

## Epic Metadata
- **Epic ID**: EPIC-CCN-003
- **Target Method**: IsOrderAllowed
- **File**: src/V12_002.UI.Compliance.cs
- **Current CYC**: 16
- **Target CYC**: ≤ 8
- **Reduction Required**: 8 points

## Scope Boundaries

### IN SCOPE
1. **Primary Target**: IsOrderAllowed method refactoring
   - Extract conditional logic into helper methods
   - Reduce cyclomatic complexity from 16 to ≤ 8
   - Maintain existing method signature (preserve caller compatibility)

2. **Helper Method Extraction**:
   - Extract order validation checks into separate methods
   - Extract compliance rule checks into dedicated validators
   - Extract state-dependent logic into focused helpers

3. **Testing**:
   - Add unit tests for extracted helper methods
   - Add characterization tests for IsOrderAllowed behavior
   - Verify integration with existing callers

4. **Documentation**:
   - Update method XML comments
   - Document extracted helper methods
   - Add inline comments for complex logic

### OUT OF SCOPE
1. **Caller Modifications**: Do NOT modify any methods that call IsOrderAllowed
2. **Signature Changes**: Do NOT change IsOrderAllowed method signature
3. **Behavioral Changes**: Do NOT alter the logic or decision flow
4. **Other Methods**: Do NOT refactor other methods in V12_002.UI.Compliance.cs
5. **Unrelated Fixes**: Do NOT fix pre-existing issues outside this method

## Blast Radius Analysis

### Direct Impact
- **File**: src/V12_002.UI.Compliance.cs (single file modification)
- **Method**: IsOrderAllowed (primary target)
- **New Methods**: 3-5 helper methods (to be created)

### Caller Impact
- **Risk Level**: LOW (signature preserved, behavior unchanged)
- **Callers**: Multiple locations across codebase
- **Mitigation**: Characterization tests ensure behavior preservation

### Dependency Impact
- **Dependencies**: None identified (self-contained compliance logic)
- **Side Effects**: None expected (pure validation logic)

## Extraction Strategy

### Phase 1: Analysis (Current)
- ✅ Hotspot analysis complete
- ✅ Code structure analyzed via jCodemunch
- ✅ Blast radius assessed
- ✅ Scope boundaries defined

### Phase 2: Architecture Planning
- Design helper method signatures
- Map conditional branches to extraction targets
- Validate against Jane Street patterns
- Create extraction sequence diagram

### Phase 3: DNA & PR Audit
- Verify lock-free compliance (no lock() blocks)
- Verify ASCII-only compliance
- Check for Jane Street violations
- Validate against V12 DNA mandates

### Phase 4: Ticket Generation
- Ticket 1: Extract order validation checks
- Ticket 2: Extract compliance rule validators
- Ticket 3: Extract state-dependent logic
- Ticket 4: Add unit tests
- Ticket 5: Integration verification

### Phase 5: Surgical Refactoring
- Execute tickets sequentially
- Verify build after each ticket
- Run deploy-sync.ps1 after each change
- F5 verification in NinjaTrader

### Phase 6: Final Review
- Verify CYC ≤ 8 achieved
- Confirm all tests pass
- Validate BUILD_TAG in NinjaTrader
- Document completion

## Success Criteria

### Functional Requirements
- ✅ IsOrderAllowed CYC reduced from 16 to ≤ 8
- ✅ All existing callers continue to work unchanged
- ✅ No behavioral changes (logic preserved exactly)
- ✅ Build passes with zero errors

### Quality Requirements
- ✅ Unit tests added for all extracted methods
- ✅ Characterization tests verify original behavior
- ✅ Code coverage ≥ 80% for new methods
- ✅ No lock() blocks introduced

### V12 DNA Compliance
- ✅ Lock-free Actor/FSM pattern maintained
- ✅ ASCII-only compliance verified
- ✅ Jane Street alignment (CYC ≤ 8)
- ✅ Correctness by construction principles applied

### Deployment Requirements
- ✅ deploy-sync.ps1 executed successfully
- ✅ F5 in NinjaTrader shows BUILD_TAG
- ✅ Strategy loads without errors
- ✅ Pre-push validation passes (all 13 checks)

## Risk Mitigation

### Risk 1: Breaking Existing Callers
- **Mitigation**: Preserve method signature exactly
- **Verification**: Characterization tests before/after
- **Rollback**: Git revert if integration fails

### Risk 2: Introducing Bugs
- **Mitigation**: Extract logic without modification
- **Verification**: Unit tests + integration tests
- **Rollback**: Restore from checkpoint

### Risk 3: Scope Creep
- **Mitigation**: Strict adherence to scope boundaries
- **Verification**: Code review against scope document
- **Enforcement**: Reject any out-of-scope changes

### Risk 4: Build Failures
- **Mitigation**: Build after each ticket
- **Verification**: deploy-sync.ps1 + F5 verification
- **Rollback**: Revert last commit if build fails

## Scope Validation Checklist

- ✅ Target method identified and analyzed
- ✅ Blast radius assessed (LOW risk)
- ✅ Scope boundaries clearly defined
- ✅ IN SCOPE items enumerated
- ✅ OUT OF SCOPE items enumerated
- ✅ Success criteria defined
- ✅ Risk mitigation strategies documented
- ✅ Sequential Thinking validation complete

## Next Phase
Proceed to Phase 1.5 (Scope Boundary Validation) to verify scope boundaries with Sequential Thinking MCP before architecture planning.

---

**Agent Tracking**:
- **Phase**: 1 (Scope Definition)
- **Agent**: v12-phase1-scope (Bob Shell)
- **MCP Tools Used**: jCodemunch (resolve_repo, search_symbols, get_symbol_source, find_references), Sequential Thinking (sequentialthinking)
- **Timestamp**: 2026-06-18T01:51:14Z
- **Status**: COMPLETE

---

## Agent Tracking

- **Agent Name**: wave6-p1-003
- **Mode**: v12-phase1-scope
- **Bobcoins Used**: 
- **API Key**: 
- **Model**: 
- **Execution Time**: 69s
- **Timestamp**: 2026-06-18T01:52:12Z

### MCP Tools Used

- jcodemunch-mcp: get_file_outline, find_references, get_dependency_graph
- sequential-thinking: sequentialthinking (scope boundary validation)
- graphify: Codebase structure visualization
