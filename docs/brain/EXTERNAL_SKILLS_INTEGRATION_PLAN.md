# External Skills Integration Plan

**Date**: 2026-06-08
**Last Updated**: 2026-06-08 (V12.23 - Matt Pocock Integration Complete)
**Status**: PARTIAL - 1 of 2 skills integrated
**Priority**: MEDIUM (1 remaining skill)

---

## Executive Summary

This plan addresses the integration of 2 external skills referenced by the Director:

1. **agentic-engineering-workflow** (Micky Podcast) - ⚠️ NOT ANALYZED
2. **improve-codebase-architecture** (Matt Pocock) - ✅ COMPLETE (V12.23)

**Completed**: 1 skill (6 hours)
**Remaining**: 1 skill (3-4 hours estimated)

---

## Skill 1: agentic-engineering-workflow (Micky Podcast)

### Source

**URL**: https://github.com/pawel-cell/micky-podcast-agentic-engineering/blob/main/skills/agentic-engineering-workflow/SKILL.md  
**Author**: Pawel (Micky Podcast)  
**License**: MIT (assumed)

### Analysis Status

**Current**: ⚠️ REFERENCED but not analyzed  
**Evidence**: Mentioned in task brief as external skill to audit

### Integration Approach

**Phase 1: Fetch and Analyze** (1.5 hours)
1. Fetch skill content from GitHub
2. Extract core principles and patterns
3. Compare to V12 Phase 6 Recursive Protocol
4. Identify gaps and overlaps

**Phase 2: Gap Analysis** (1 hour)
1. Map skill concepts to V12 workflows
2. Identify missing patterns in V12
3. Assess integration value (HIGH/MEDIUM/LOW)
4. Document findings

**Phase 3: Integration Decision** (0.5 hours)
- **Option A**: Direct adoption (copy skill as-is)
- **Option B**: Adaptation (modify for V12 DNA)
- **Option C**: Already covered (existing workflow does this)
- **Option D**: Not applicable (doesn't fit V12 architecture)

**Phase 4: Implementation** (if approved) (1 hour)
- Create V12 skill or update existing workflow
- Document integration in AGENTS.md
- Add to workflow commands

### Expected Outcomes

**Likely Patterns** (based on Micky Podcast's other skills):
- Workflow orchestration patterns
- Agent coordination protocols
- Task decomposition strategies
- Quality gates and checkpoints

**Potential V12 Integration Points**:
- `/epic-run` workflow enhancement
- `/autonomous-refactor` coordination
- Agent handoff protocols in `nexus_a2a.json`

### Priority Assessment

**Priority**: HIGH  
**Rationale**: 
- Director explicitly requested analysis
- Micky Podcast's other skills (source-code-context, code-structure-cleanup) were highly valuable
- Likely contains workflow patterns we're missing

---

## Skill 2: improve-codebase-architecture (Matt Pocock)

### Source

**URL**: https://www.skills.sh/mattpocock/skills/improve-codebase-architecture
**Author**: Matt Pocock
**Platform**: skills.sh

### Analysis Status

**Current**: ✅ COMPLETE (V12.23)
**Integration Date**: 2026-06-08
**Integration Report**: `docs/brain/MATTPOCOCK_ARCHITECTURE_INTEGRATION.md`

### Integration Summary

**Phase 1: Fetch and Analyze** (✅ COMPLETE - 1 hour)
1. ✅ Fetched skill content from skills.sh
2. ✅ Extracted core principles (John Ousterhout's "deep module" principle)
3. ✅ Compared to V12 architectural validation skill (complementary, not overlapping)
4. ✅ Identified gaps (no pre-analysis exploration phase)

**Phase 2: Adaptation** (✅ COMPLETE - 2 hours)
1. ✅ Created `plugins/codebase-architecture/SKILL.md` (598 lines)
2. ✅ Applied Anthropic self-improving format
3. ✅ Added V12 DNA constraints (CYC ≤ 15, zero-allocation, lock-free, ASCII-only)
4. ✅ Integrated jCodemunch tools for exploration
5. ✅ Added post-use audit protocol

**Phase 3: Workflow Integration** (✅ COMPLETE - 2 hours)
1. ✅ Added Phase 0 to `/epic-plan` (Architectural Exploration)
2. ✅ Updated `/epic-run` (Phase 2 note)
3. ✅ Updated `/autonomous-refactor` (prerequisite check)
4. ✅ Updated `AGENTS.md` (Section 6 documentation)

**Phase 4: Validation** (✅ COMPLETE - 1 hour)
1. ✅ Validated integration (no conflicts)
2. ✅ Created 3 usage examples (God-file splitting, untested seams, coupling)
3. ✅ Updated skills inventory

### Key Principles Integrated

**Deep Module Principle** (John Ousterhout):
- Small interfaces hiding large implementations (high leverage)
- Deletion test: Concentrate vs scatter complexity
- Seams: Alter behavior without editing in place

**V12 DNA Alignment**:
- Correctness by Construction: Small interfaces make illegal states unrepresentable
- CYC ≤ 15: Shallow modules indicate over-extraction
- Jane Street Cognitive Simplicity: Small interfaces reduce mental load
- Lock-Free Actor Pattern: Deep modules can encapsulate FSM patterns

### Workflow Integration Points

1. **`/epic-plan`**: Phase 0 (Architectural Exploration) - OPTIONAL but RECOMMENDED
2. **`/epic-run`**: Phase 2 note documenting Phase 0 availability
3. **`/autonomous-refactor`**: Prerequisite check for skill existence
4. **`AGENTS.md`**: Section 6 documentation

### Deliverables

- ✅ `plugins/codebase-architecture/SKILL.md` (598 lines)
- ✅ `docs/brain/external-skills/mattpocock-improve-codebase-architecture-raw.md` (95 lines)
- ✅ `docs/brain/MATTPOCOCK_ARCHITECTURE_INTEGRATION.md` (698 lines)
- ✅ Updated 4 workflow commands
- ✅ Updated 3 documentation files

### Status

**Integration**: ✅ COMPLETE
**Blocking Task**: ✅ RESOLVED - Ready for `/autonomous-refactor 15 179`
**Total Time**: 6 hours

**Phase 2: Gap Analysis** (1 hour)
1. Map skill concepts to V12 architecture workflows
2. Compare to `plugins/architecture-validation/SKILL.md`
3. Assess integration value (HIGH/MEDIUM/LOW)
4. Document findings

**Phase 3: Integration Decision** (0.5 hours)
- **Option A**: Enhance existing `architecture-validation` skill
- **Option B**: Create new `architecture-improvement` skill
- **Option C**: Already covered (existing skill does this)
- **Option D**: Not applicable (doesn't fit V12 needs)

**Phase 4: Implementation** (if approved) (1 hour)
- Update `architecture-validation` skill or create new skill
- Document integration in AGENTS.md
- Add to `/epic-plan` workflow

### Expected Outcomes

**Likely Patterns** (based on Matt Pocock's expertise):
- TypeScript/JavaScript architectural patterns
- Module boundary design
- Dependency management
- Refactoring strategies

**Potential V12 Integration Points**:
- `/epic-plan` architectural design phase
- `plugins/architecture-validation/SKILL.md` enhancement
- Phase 6 Recursive Protocol Stage 2 (Arch Planning)

### Priority Assessment

**Priority**: HIGH  
**Rationale**:
- Director explicitly requested analysis
- Matt Pocock is a recognized expert in code architecture
- May contain patterns applicable to C# architecture (V12 is C#)
- Could enhance existing architecture-validation skill

---

## Integration Timeline

### Week 1: Analysis Phase

**Day 1-2**: Skill 1 (agentic-engineering-workflow)
- Fetch and analyze (1.5 hours)
- Gap analysis (1 hour)
- Integration decision (0.5 hours)
- **Deliverable**: Analysis document

**Day 3-4**: Skill 2 (improve-codebase-architecture)
- Fetch and analyze (1.5 hours)
- Gap analysis (1 hour)
- Integration decision (0.5 hours)
- **Deliverable**: Analysis document

**Day 5**: Synthesis and Recommendations
- Compare both skills
- Prioritize integration opportunities
- Create implementation plan
- **Deliverable**: Integration recommendations

### Week 2: Implementation Phase (if approved)

**Day 1-2**: Skill 1 Implementation (if approved)
- Create/update V12 skill (1 hour)
- Update workflows (0.5 hours)
- Test integration (0.5 hours)
- **Deliverable**: Integrated skill

**Day 3-4**: Skill 2 Implementation (if approved)
- Create/update V12 skill (1 hour)
- Update workflows (0.5 hours)
- Test integration (0.5 hours)
- **Deliverable**: Integrated skill

**Day 5**: Documentation and Handoff
- Update AGENTS.md
- Update workflow commands
- Create usage examples
- **Deliverable**: Complete integration documentation

---

## Success Criteria

### Analysis Phase Success

- ✅ Both skills fetched and analyzed
- ✅ Gap analysis completed for each
- ✅ Integration decision documented
- ✅ Recommendations provided to Director

### Implementation Phase Success (if approved)

- ✅ Skills integrated into V12 workflows
- ✅ Documentation updated (AGENTS.md, workflow commands)
- ✅ Usage examples created
- ✅ Integration tested in real workflow

---

## Risk Assessment

### Risk 1: Skill Content Not Accessible

**Probability**: LOW  
**Impact**: HIGH  
**Mitigation**: 
- GitHub URL is public (Micky Podcast)
- skills.sh may require account (Matt Pocock)
- Fallback: Contact authors directly

### Risk 2: Skills Not Applicable to V12

**Probability**: MEDIUM  
**Impact**: MEDIUM  
**Mitigation**:
- Analyze before committing to integration
- Document "not applicable" decision clearly
- Extract any valuable sub-patterns

### Risk 3: Integration Conflicts with V12 DNA

**Probability**: LOW  
**Impact**: HIGH  
**Mitigation**:
- Adapt skills to V12 DNA principles
- Don't force integration if conflicts exist
- Document adaptation rationale

---

## Resource Requirements

### Time

- **Analysis Phase**: 6 hours (3 hours per skill)
- **Implementation Phase**: 4 hours (2 hours per skill, if approved)
- **Total**: 10 hours maximum

### Personnel

- **Primary**: Advanced Mode Agent (analysis + implementation)
- **Review**: Director (approval gates)
- **Testing**: Bob CLI (workflow integration testing)

### Tools

- **Fetch**: Browser tools (skills.sh), GitHub API (Micky Podcast)
- **Analysis**: jcodemunch-mcp (pattern matching)
- **Integration**: File editing tools, workflow command updates

---

## Approval Gates

### Gate 1: Analysis Complete

**Trigger**: Both skills analyzed  
**Deliverable**: Analysis documents for both skills  
**Approver**: Director  
**Decision**: Proceed to implementation OR document as "not applicable"

### Gate 2: Integration Plan Approved

**Trigger**: Integration approach documented  
**Deliverable**: Implementation plan with effort estimates  
**Approver**: Director  
**Decision**: Proceed with implementation OR defer to backlog

### Gate 3: Implementation Complete

**Trigger**: Skills integrated and tested  
**Deliverable**: Updated workflows + documentation  
**Approver**: Director  
**Decision**: Merge to main OR iterate on feedback

---

## Backlog: Additional External Skills

### Future Consideration

**Anthropic Skill-Creator Template**:
- **Status**: Partially applied (6/6 plugins/ skills converted)
- **Gap**: May be missing from other skill locations
- **Action**: Audit all 46 skills for Anthropic format compliance

**Greptile Skills** (if available):
- **Status**: Greptile MCP server integrated, but no skill docs found
- **Action**: Check if Greptile publishes skills on skills.sh or GitHub

**Jane Street Patterns** (from ingested intel):
- **Status**: Patterns documented in `docs/intel/jane-street/`
- **Action**: Consider creating formal skills from Jane Street patterns

---

## Next Steps

### Immediate (This Session)

1. ✅ Complete comprehensive skills audit
2. ✅ Create this integration plan
3. ⏳ Commit both documents to gitbutler/workspace
4. ⏳ Report completion to Director

### After Director Approval

1. ⏳ Fetch agentic-engineering-workflow skill
2. ⏳ Analyze and document findings
3. ⏳ Fetch improve-codebase-architecture skill
4. ⏳ Analyze and document findings
5. ⏳ Present integration recommendations
6. ⏳ Await approval for implementation phase

---

## References

### Existing Integration Examples

- **source-code-context**: [`SOURCE_CODE_CONTEXT_INTEGRATION.md`](SOURCE_CODE_CONTEXT_INTEGRATION.md)
- **code-structure-cleanup**: [`EPIC-POSINFO/skill-analysis.md`](EPIC-POSINFO/skill-analysis.md)
- **Skills Conversion Report**: [`skills-conversion-report.md`](skills-conversion-report.md)

### V12 Workflows

- **Epic Run**: `.bob/commands/epic-run.md`
- **Epic Plan**: `.bob/commands/epic-plan.md`
- **Architecture Validation**: `plugins/architecture-validation/SKILL.md`

### External Sources

- **Micky Podcast GitHub**: https://github.com/pawel-cell/micky-podcast-agentic-engineering
- **Matt Pocock skills.sh**: https://www.skills.sh/mattpocock
- **Anthropic Skills**: https://github.com/anthropics/skills

---

**Plan Status**: ✅ COMPLETE - Awaiting Director Approval  
**Estimated Effort**: 6-8 hours (analysis) + 4 hours (implementation if approved)  
**Priority**: HIGH (Director-requested)  
**Next Action**: Commit and report to Director