# Phase Consolidation Plan for Wave 3

## Current Phase Structure (9 Phases)

**Wave 2 Used**:
1. **Phase -1**: Pre-flight checks (branch strategy, build readiness)
2. **Phase 0**: Hotspot Analysis (jCodemunch complexity scan)
3. **Phase 1**: Scope Definition (what to extract)
4. **Phase 1.5**: Scope Boundary Validation (MANDATORY gate - V12.23)
5. **Phase 2**: Architecture Planning (how to extract)
6. **Phase 3**: DNA & PR Audit (compliance checks)
7. **Phase 4**: Ticket Generation (break into surgical tasks)
8. **Phase 5**: Ticket Execution (Bob CLI does the work)
9. **Phase 5.V**: Per-Ticket Verification (validate each ticket)
10. **Phase 6**: Final Review (epic completion report)

**Total**: 10 phases (including Phase -1)

---

## Recommended Consolidation: 7-Phase Model

### Merges Proposed

**Merge 1: Phase 1 + Phase 1.5** → **New Phase 1** (Scope Definition + Boundary)
- **Rationale**: Both are planning phases, no code changes
- **Time Saved**: ~10 minutes per epic (eliminate handoff overhead)
- **Risk**: LOW - both are read-only analysis

**Merge 2: Phase 3 + Phase 4** → **New Phase 3** (Audit + Tickets)
- **Rationale**: Ticket generation depends on audit results
- **Time Saved**: ~10 minutes per epic (eliminate handoff overhead)
- **Risk**: LOW - both are planning phases

**Keep Separate**:
- ✅ Phase -1 (Pre-flight) - critical gate
- ✅ Phase 0 (Hotspot) - data gathering
- ✅ Phase 2 (Architecture) - complex planning
- ✅ Phase 5 (Execution) - code changes
- ✅ Phase 5.V (Verification) - quality gate
- ✅ Phase 6 (Review) - completion gate

---

## New 7-Phase Structure

| Phase | Name | Activities | Time | Agent |
|-------|------|------------|------|-------|
| **-1** | Pre-flight | Branch check, build verify | 5 min | Script |
| **0** | Hotspot Analysis | jCodemunch scan, complexity audit | 10 min | Ask mode |
| **1** | Scope + Boundary | Define scope, validate single-method boundary | 20 min | Plan mode |
| **2** | Architecture | Design extraction, call graphs, Jane Street checks | 25 min | Plan mode |
| **3** | Audit + Tickets | DNA compliance, PR hygiene, ticket generation | 20 min | Advanced mode |
| **5** | Execution | Bob CLI surgical extraction (per ticket) | 10 min/ticket | Bob CLI |
| **5.V** | Verification | Validate ticket completion | 5 min/ticket | Advanced mode |
| **6** | Final Review | Epic completion report, roadmap update | 10 min | Advanced mode |

**Total Time Per Epic** (with 6 tickets average):
- Planning (Phases -1 to 3): 80 minutes
- Execution (Phase 5): 60 minutes (6 × 10)
- Verification (Phase 5.V): 30 minutes (6 × 5)
- Review (Phase 6): 10 minutes
- **Total**: 180 minutes (~3 hours per epic)

**Time Savings**: 20 minutes per epic × 53 epics = **17.7 hours saved**

---

## Alternative: 6-Phase Model (More Aggressive)

**Additional Merge: Phase 5 + Phase 5.V** → **New Phase 5** (Execution + Verification)
- **Rationale**: Verify immediately after each ticket execution
- **Time Saved**: Additional ~10 minutes per epic (eliminate handoff)
- **Risk**: MEDIUM - verification might miss issues if done too quickly

**Not Recommended**: Higher risk of missing verification issues.

---

## Phase Numbering After Consolidation

### Option A: Renumber Sequentially (Clean)
- Phase -1 → Phase -1 (Pre-flight)
- Phase 0 → Phase 0 (Hotspot)
- Phase 1 + 1.5 → Phase 1 (Scope + Boundary)
- Phase 2 → Phase 2 (Architecture)
- Phase 3 + 4 → Phase 3 (Audit + Tickets)
- Phase 5 → Phase 4 (Execution)
- Phase 5.V → Phase 5 (Verification)
- Phase 6 → Phase 6 (Final Review)

**Total**: 8 phases (including Phase -1)

### Option B: Keep Original Numbers (Backward Compatible) ✅ **RECOMMENDED**
- Phase -1 → Phase -1 (Pre-flight)
- Phase 0 → Phase 0 (Hotspot)
- Phase 1 + 1.5 → Phase 1 (Scope + Boundary) ⚠️ **MERGED**
- Phase 2 → Phase 2 (Architecture)
- Phase 3 + 4 → Phase 3 (Audit + Tickets) ⚠️ **MERGED**
- Phase 5 → Phase 5 (Execution)
- Phase 5.V → Phase 5.V (Verification)
- Phase 6 → Phase 6 (Final Review)

**Total**: 8 phases (including Phase -1)

**Rationale**: Easier to track against Wave 2 documentation, scripts already use these numbers.

---

## Implementation Changes Required

### Scripts to Update

1. **`scripts/epic_manifest.py`**
   - Update phase dependencies
   - Phase 1 now produces both scope + boundary artifacts
   - Phase 3 now produces both audit + tickets artifacts

2. **MCP Servers**
   - `phase-1-scope` → merge with `phase-1-5-boundary`
   - `phase-3-audit` → merge with `phase-4-tickets`

3. **Launch Scripts**
   - Remove `launch_phase1_5_all_screen.sh`
   - Remove `launch_phase4_all_screen.sh`
   - Update `launch_phase1_all_screen.sh` to do both scope + boundary
   - Update `launch_phase3_all_screen.sh` to do both audit + tickets

### Artifact Changes

**Phase 1 Output** (merged):
- `00-scope.md` (scope definition)
- `01-scope-boundary.md` (boundary validation)

**Phase 3 Output** (merged):
- `03-audit-report.md` (DNA & PR audit)
- `04-tickets.md` (ticket generation)

---

## Migration Strategy

### For Wave 3

**Option 1: Full Consolidation** (7-phase model)
- Implement all merges before Wave 3
- Test with 2 epics first
- Rollback if issues found

**Option 2: Incremental** (test Phase 1 merge only)
- Merge Phase 1 + 1.5 for Wave 3
- Keep Phase 3 + 4 separate
- Merge Phase 3 + 4 for Wave 4 if Phase 1 merge succeeds

**Option 3: Conservative** (no changes)
- Keep 9-phase model for Wave 3
- Implement consolidation for Wave 4 after lessons learned

---

## Recommendation

**Use Option B (Keep Original Numbers) + Option 1 (Full Consolidation)**

**Why**:
1. **Time Savings**: 17.7 hours saved across 53 epics
2. **Low Risk**: Both merges are planning-only phases
3. **Backward Compatible**: Original phase numbers preserved
4. **Proven**: Wave 2 showed these phases have natural coupling

**Implementation**:
1. Update MCP servers to merge logic
2. Update scripts to skip removed phases
3. Update manifest dependencies
4. Test with 2 epics before full Wave 3 launch

---

## Final Answer

**Which phases will be merged?**
- Phase 1 + Phase 1.5 → Phase 1 (Scope + Boundary)
- Phase 3 + Phase 4 → Phase 3 (Audit + Tickets)

**What is the total new number of phases?**
- **8 phases** (including Phase -1 pre-flight)
- **7 phases** (excluding Phase -1)

**Phase List**:
1. Phase -1: Pre-flight
2. Phase 0: Hotspot Analysis
3. Phase 1: Scope + Boundary ⚠️ **MERGED**
4. Phase 2: Architecture Planning
5. Phase 3: Audit + Tickets ⚠️ **MERGED**
6. Phase 5: Ticket Execution
7. Phase 5.V: Per-Ticket Verification
8. Phase 6: Final Review

**Time Saved**: 20 minutes per epic × 53 epics = **17.7 hours**