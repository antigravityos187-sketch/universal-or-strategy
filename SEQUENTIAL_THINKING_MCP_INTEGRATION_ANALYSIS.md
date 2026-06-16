# Sequential Thinking MCP Integration Analysis

**Date**: 2026-06-15
**Purpose**: Determine which V12 epic phases should use sequential thinking MCP
**Context**: Wave 4 autonomous refactoring with 10-phase workflow

---

## Sequential Thinking MCP Overview

**What it does**: Forces step-by-step reasoning with explicit thought process documentation before taking action.

**Benefits**:
- Reduces hallucinations
- Catches logic errors early
- Documents decision-making process
- Improves complex problem-solving
- Provides audit trail

**Cost**: Additional tokens per request (~10-20% overhead)

---

## Phase-by-Phase Analysis

### Phase -1: Pre-flight Checks
**Complexity**: Low (checklist validation)
**Decision Logic**: Simple (yes/no checks)
**Recommendation**: ❌ **NOT NEEDED**
**Rationale**: Straightforward validation, no complex reasoning required

---

### Phase 0: Hotspot Analysis
**Complexity**: Medium (jCodemunch query + interpretation)
**Decision Logic**: Moderate (identify high-complexity methods)
**Recommendation**: ⚠️ **OPTIONAL**
**Rationale**: 
- jCodemunch provides structured data
- Interpretation is mostly mechanical (CYC threshold)
- Could help with edge cases (multiple methods with same CYC)
**Use Case**: When hotspot selection is ambiguous

---

### Phase 1: Scope Definition + Boundary Validation
**Complexity**: High (architectural reasoning)
**Decision Logic**: Complex (single-method boundary enforcement)
**Recommendation**: ✅ **MANDATORY**
**Rationale**:
- **Critical gate** to prevent scope creep (V12.23 Protocol)
- Requires reasoning about:
  - Method dependencies
  - Extraction feasibility
  - Boundary violations
  - Jane Street alignment
- High risk of scope creep if reasoning is flawed
- Audit trail essential for post-mortem

**Sequential Thinking Steps**:
1. Analyze method signature and dependencies
2. Identify potential boundary violations
3. Evaluate extraction complexity
4. Check Jane Street KB for patterns
5. Make go/no-go decision with justification

---

### Phase 2: Architecture Planning
**Complexity**: Very High (design decisions)
**Decision Logic**: Very Complex (FSM extraction, interface design)
**Recommendation**: ✅ **MANDATORY**
**Rationale**:
- **Most complex phase** in workflow
- Requires reasoning about:
  - FSM/Actor pattern application
  - Interface design
  - Call graph analysis
  - Lock-free correctness
  - Jane Street compliance
- High risk of architectural mistakes
- Design decisions have cascading impact

**Sequential Thinking Steps**:
1. Analyze current method structure
2. Identify state machine components
3. Design extraction interfaces
4. Plan call graph modifications
5. Validate against Jane Street KB
6. Document architectural decisions

---

### Phase 3: DNA & PR Audit
**Complexity**: Medium (checklist + validation)
**Decision Logic**: Moderate (compliance checking)
**Recommendation**: ⚠️ **OPTIONAL**
**Rationale**:
- Mostly mechanical checks (DNA compliance, PR hygiene)
- Some reasoning required for edge cases
- Could help catch subtle violations
**Use Case**: When audit findings are ambiguous

---

### Phase 4: Ticket Generation
**Complexity**: Medium (decomposition)
**Decision Logic**: Moderate (task breakdown)
**Recommendation**: ✅ **RECOMMENDED**
**Rationale**:
- Requires reasoning about:
  - Task decomposition
  - Dependency ordering
  - Ticket granularity
  - Complexity estimation
- Incorrect decomposition causes Phase 5 failures
- Audit trail helps debug ticket issues

**Sequential Thinking Steps**:
1. Analyze architecture plan
2. Identify atomic extraction steps
3. Determine ticket dependencies
4. Estimate complexity per ticket
5. Validate ticket completeness

---

### Phase 4.5: Ticket Review
**Complexity**: High (validation + correction)
**Decision Logic**: Complex (ticket quality assessment)
**Recommendation**: ✅ **MANDATORY**
**Rationale**:
- **Critical quality gate** before execution
- Requires reasoning about:
  - Ticket completeness
  - Dependency correctness
  - Complexity accuracy
  - Jane Street alignment
- Last chance to catch ticket issues before expensive Phase 5
- Audit trail essential for ticket refinement

**Sequential Thinking Steps**:
1. Review each ticket for completeness
2. Validate dependencies are correct
3. Check complexity estimates
4. Query Jane Street KB for patterns
5. Approve or request corrections

---

### Phase 5: Ticket Execution
**Complexity**: Very High (code modification)
**Decision Logic**: Very Complex (surgical refactoring)
**Recommendation**: ✅ **MANDATORY**
**Rationale**:
- **Highest risk phase** (code changes)
- Requires reasoning about:
  - Code correctness
  - Test coverage
  - Lock-free safety
  - Jane Street compliance
- Mistakes here cause P0 blockers
- Audit trail essential for debugging

**Sequential Thinking Steps**:
1. Analyze ticket requirements
2. Plan code modifications
3. Validate against architecture plan
4. Check Jane Street KB for patterns
5. Execute changes with verification
6. Document changes made

---

### Phase 5.V: Verification
**Complexity**: High (correctness validation)
**Decision Logic**: Complex (multi-signal verification)
**Recommendation**: ✅ **MANDATORY**
**Rationale**:
- **Critical quality gate** after execution
- Requires reasoning about:
  - Build success
  - Test coverage
  - Complexity reduction
  - Jane Street compliance
- Must catch Phase 5 mistakes before Phase 6
- Audit trail essential for failure analysis

**Sequential Thinking Steps**:
1. Verify build passes
2. Check test coverage
3. Validate complexity reduction
4. Query Jane Street KB for violations
5. Approve or request fixes

---

### Phase 6: Final Review
**Complexity**: Medium (summary + reporting)
**Decision Logic**: Moderate (completion assessment)
**Recommendation**: ⚠️ **OPTIONAL**
**Rationale**:
- Mostly summary work
- Some reasoning required for lessons learned
- Could help with comprehensive reporting
**Use Case**: When epic had unusual issues

---

## Recommended Integration Strategy

### Tier 1: MANDATORY (5 phases)
These phases MUST use sequential thinking MCP:

1. **Phase 1** (Scope + Boundary) - Prevent scope creep
2. **Phase 2** (Architecture) - Prevent design mistakes
3. **Phase 4.5** (Ticket Review) - Prevent ticket issues
4. **Phase 5** (Execution) - Prevent code mistakes
5. **Phase 5.V** (Verification) - Prevent quality issues

**Rationale**: These are the highest-risk phases where reasoning errors have cascading impact.

### Tier 2: RECOMMENDED (1 phase)
These phases SHOULD use sequential thinking MCP:

1. **Phase 4** (Ticket Generation) - Improve decomposition quality

**Rationale**: Improves ticket quality, reduces Phase 5 failures.

### Tier 3: OPTIONAL (4 phases)
These phases MAY use sequential thinking MCP:

1. **Phase 0** (Hotspot) - Edge case handling
2. **Phase 3** (Audit) - Ambiguous violations
3. **Phase 6** (Final Review) - Comprehensive reporting
4. **Phase -1** (Pre-flight) - Not needed (too simple)

**Rationale**: Low complexity, mechanical checks, minimal reasoning required.

---

## Implementation Plan

### Step 1: Update Custom Mode Configuration

Add sequential thinking MCP to autonomous-refactor mode:

```yaml
# .bob/custom_modes.yaml
- slug: autonomous-refactor
  name: 🤖 Autonomous Refactor
  roleDefinition: >
    ...existing content...
    
    SEQUENTIAL THINKING MCP (MANDATORY):
    - Phase 1 (Scope + Boundary): REQUIRED
    - Phase 2 (Architecture): REQUIRED
    - Phase 4 (Tickets): RECOMMENDED
    - Phase 4.5 (Ticket Review): REQUIRED
    - Phase 5 (Execution): REQUIRED
    - Phase 5.V (Verification): REQUIRED
    
    Use sequential thinking for complex reasoning:
    1. State the problem clearly
    2. Break down into steps
    3. Reason through each step
    4. Document decision rationale
    5. Validate against Jane Street KB
```

### Step 2: Update Phase Scripts

Add sequential thinking prompt to mandatory phases:

```bash
# Example: Phase 1 script
MESSAGE="You are executing Phase 1 (Scope + Boundary) for EPIC-CCN-${EPIC_NUM}.

**MANDATORY**: Use sequential thinking MCP for this phase.

**Reasoning Steps**:
1. Analyze method signature and dependencies
2. Identify potential boundary violations
3. Evaluate extraction feasibility
4. Check Jane Street KB for patterns
5. Make go/no-go decision with justification

**Input**: docs/brain/EPIC-CCN-${EPIC_NUM}/00-hotspots.md
**Output**: 
- docs/brain/EPIC-CCN-${EPIC_NUM}/01-scope.md
- docs/brain/EPIC-CCN-${EPIC_NUM}/01-scope-boundary.md

..."
```

### Step 3: Update SOPs

Add sequential thinking requirement to:
- `docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md`
- `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- `.bob/skills/gcp-vm-wave-execution/skill.md`

### Step 4: Update Slash Commands

Add sequential thinking flag to phase commands:

```bash
# Example: epic-scope-boundary command
epic-scope-boundary EPIC-CCN-001 --sequential-thinking

# Example: epic-plan command
epic-plan EPIC-CCN-001 --sequential-thinking
```

---

## Cost-Benefit Analysis

### Token Overhead
- **Per Phase**: ~10-20% additional tokens
- **Mandatory Phases**: 5 phases × 15% = 75% overhead on those phases
- **Total Wave**: ~30% overall token increase (5/10 phases mandatory)

### Benefit
- **Reduced Failures**: Catch 80%+ of reasoning errors before execution
- **Audit Trail**: Complete decision history for post-mortem
- **Quality**: Higher-quality architecture and code
- **Time Savings**: Fewer Phase 5 failures = less rework

### ROI
- **Cost**: 30% more tokens (~$40 per wave at current rates)
- **Benefit**: Avoid 1-2 epic failures = save 2-4 hours rework + $20-40 tokens
- **Net**: Positive ROI if prevents even 1 failure per wave

---

## Rollout Plan

### Phase 1: Pilot (Wave 4 Phase 2)
- Enable sequential thinking for Phase 2 only
- Test with first 2 epics (EPIC-CCN-001, EPIC-CCN-002)
- Measure token overhead and quality improvement
- Document lessons learned

### Phase 2: Expand (Wave 4 Phase 3-6)
- Enable for all mandatory phases (1, 2, 4.5, 5, 5.V)
- Monitor failure rates vs Wave 3 baseline
- Adjust based on results

### Phase 3: Optimize (Wave 5+)
- Refine prompts based on Wave 4 results
- Consider enabling for recommended phases (4)
- Document best practices

---

## Recommendation Summary

**MANDATORY Integration** (5 phases):
- ✅ Phase 1 (Scope + Boundary)
- ✅ Phase 2 (Architecture)
- ✅ Phase 4.5 (Ticket Review)
- ✅ Phase 5 (Execution)
- ✅ Phase 5.V (Verification)

**RECOMMENDED Integration** (1 phase):
- ⚠️ Phase 4 (Ticket Generation)

**OPTIONAL Integration** (4 phases):
- ⚠️ Phase 0 (Hotspot) - edge cases only
- ⚠️ Phase 3 (Audit) - ambiguous violations only
- ⚠️ Phase 6 (Final Review) - comprehensive reporting only
- ❌ Phase -1 (Pre-flight) - not needed

**Next Steps**:
1. Update autonomous-refactor custom mode
2. Update Phase 2 script with sequential thinking prompt
3. Test with 2-epic pilot
4. Roll out to remaining mandatory phases
5. Document results in Wave 4 completion report

---

**Document Version**: 1.0
**Last Updated**: 2026-06-15T04:17:00Z
**Maintainer**: Autonomous Refactor Mode