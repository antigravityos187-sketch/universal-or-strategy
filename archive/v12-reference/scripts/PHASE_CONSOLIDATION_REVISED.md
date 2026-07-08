# Phase Consolidation - Revised Analysis

## User Feedback

**Disagreement with Merge 2** (Phase 3 + Phase 4):
- User does NOT want to merge Audit + Tickets
- Reason: These are distinct concerns

**Alternative Merge Proposals**:
1. Merge Phase -1 + Phase 0 + Phase 1 (Pre-flight + Hotspot + Scope)
2. OR merge Phase 0 + Phase 1 only (Hotspot + Scope)

**New Phase Proposal**:
- Add Phase 4.5 (Ticket Review) after Phase 4 (Ticket Generation)
- Purpose: Review tickets against plan + Jane Street rules
- Catch mistakes early before execution

---

## Analysis of Proposed Merges

### Option A: Merge Phase -1 + Phase 0 + Phase 1

**Phase -1 (Pre-flight)**:
- Branch strategy check
- Build readiness verification
- **Location**: LOCAL (runs on Windows)
- **Time**: 5 minutes
- **Agent**: PowerShell script

**Phase 0 (Hotspot)**:
- jCodemunch complexity scan
- **Location**: VM (requires jCodemunch MCP)
- **Time**: 10 minutes
- **Agent**: Ask mode

**Phase 1 (Scope)**:
- Define what to extract
- **Location**: VM
- **Time**: 10 minutes
- **Agent**: Plan mode

**Can we merge?** ❌ **NO**

**Reasons**:
1. **Location Mismatch**: Phase -1 is LOCAL, Phase 0/1 are VM
2. **Dependency**: Phase 0 needs jCodemunch data, Phase 1 needs Phase 0 output
3. **Agent Mismatch**: Script → Ask mode → Plan mode (different contexts)

**Verdict**: Keep Phase -1 separate (local pre-flight gate)

---

### Option B: Merge Phase 0 + Phase 1 (Hotspot + Scope)

**Phase 0 (Hotspot)**:
- jCodemunch complexity scan
- Outputs: `00-hotspots.md` (method list with CYC scores)
- **Agent**: Ask mode
- **Time**: 10 minutes

**Phase 1 (Scope)**:
- Read `00-hotspots.md`
- Define extraction scope
- Outputs: `00-scope.md`
- **Agent**: Plan mode
- **Time**: 10 minutes

**Can we merge?** ✅ **YES** (with caveats)

**Pros**:
- Sequential dependency (Phase 1 reads Phase 0 output)
- Both are read-only analysis (no code changes)
- Both run on VM
- Combined time: 20 minutes (same as separate)

**Cons**:
- Agent mode switch (Ask → Plan)
- Lose checkpoint between data gathering and decision-making
- If scope definition fails, must re-run hotspot analysis

**Recommendation**: ⚠️ **RISKY** - Keep separate for checkpoint granularity

---

## Revised Phase Structure (User Preferences)

### Option 1: Conservative (No Merges)

| Phase | Name | Time | Agent | Location |
|-------|------|------|-------|----------|
| **-1** | Pre-flight | 5 min | Script | LOCAL |
| **0** | Hotspot | 10 min | Ask | VM |
| **1** | Scope | 10 min | Plan | VM |
| **1.5** | Boundary | 10 min | Plan | VM |
| **2** | Architecture | 25 min | Plan | VM |
| **3** | Audit | 10 min | Advanced | VM |
| **4** | Tickets | 10 min | Plan | VM |
| **4.5** | Ticket Review | 10 min | Advanced | VM |
| **5** | Execution | 10 min/ticket | Bob CLI | VM |
| **5.V** | Verification | 5 min/ticket | Advanced | VM |
| **6** | Final Review | 10 min | Advanced | VM |

**Total Phases**: 11
**Planning Time**: 100 minutes per epic

---

### Option 2: Merge Phase 1 + 1.5 Only (User Approved)

| Phase | Name | Time | Agent | Location |
|-------|------|------|-------|----------|
| **-1** | Pre-flight | 5 min | Script | LOCAL |
| **0** | Hotspot | 10 min | Ask | VM |
| **1** | Scope + Boundary | 20 min | Plan | VM |
| **2** | Architecture | 25 min | Plan | VM |
| **3** | Audit | 10 min | Advanced | VM |
| **4** | Tickets | 10 min | Plan | VM |
| **4.5** | Ticket Review | 10 min | Advanced | VM |
| **5** | Execution | 10 min/ticket | Bob CLI | VM |
| **5.V** | Verification | 5 min/ticket | Advanced | VM |
| **6** | Final Review | 10 min | Advanced | VM |

**Total Phases**: 10
**Planning Time**: 100 minutes per epic
**Time Saved**: 10 minutes per epic (vs Option 1)

---

## Phase 4.5 (Ticket Review) - New Gate

### Purpose
Review generated tickets against:
1. Architecture plan (Phase 2)
2. Jane Street rules (ingested Firebase KB)
3. V12 DNA principles

### Activities
1. Load `02-architecture-plan.md` and `04-tickets.md`
2. Verify each ticket:
   - Matches planned extraction
   - Respects single-method boundary
   - Follows Jane Street patterns
   - Includes proper test coverage
3. Output: `04-tickets-review.md` (approval or revision needed)

### Agent
- **Mode**: Advanced (needs MCP tools for Jane Street KB)
- **Time**: 10 minutes
- **Blocking**: YES (must pass before Phase 5)

### Benefits
- Catches scope creep before execution
- Validates Jane Street compliance early
- Prevents wasted execution time on bad tickets

---

## Jane Street Integration Analysis

### Current Jane Street Usage

**Phase 0 (Hotspot)**:
- ❌ **NO** Jane Street integration
- Uses: jCodemunch complexity scan only
- **Should use**: Jane Street threshold (CYC ≤8)

**Phase 1 (Scope)**:
- ❌ **NO** Jane Street integration
- Uses: Manual scope definition
- **Should use**: Jane Street extraction patterns

**Phase 1.5 (Boundary)**:
- ❌ **NO** Jane Street integration
- Uses: Manual boundary validation
- **Should use**: Jane Street single-concern principle

**Phase 2 (Architecture)**:
- ✅ **YES** Jane Street integration
- Uses: `query_kb.py` for HFT patterns
- Custom mode: `v12-epic-planner` (has Firebase hook)

**Phase 3 (Audit)**:
- ⚠️ **PARTIAL** Jane Street integration
- Uses: V12 DNA checks (derived from Jane Street)
- **Should use**: Direct Jane Street KB queries

**Phase 4 (Tickets)**:
- ❌ **NO** Jane Street integration
- Uses: Manual ticket generation
- **Should use**: Jane Street test patterns

**Phase 4.5 (Ticket Review)** - NEW:
- ✅ **YES** Jane Street integration (proposed)
- Uses: Jane Street KB for validation
- Custom mode: Advanced (with Firebase hook)

**Phase 5 (Execution)**:
- ✅ **YES** Jane Street integration
- Uses: Bob CLI with `v12-engineer` mode
- Custom mode: Has Jane Street rules in `.bob/rules-v12-engineer/`

**Phase 5.V (Verification)**:
- ⚠️ **PARTIAL** Jane Street integration
- Uses: Complexity checks (CYC ≤8)
- **Should use**: Jane Street test coverage patterns

**Phase 6 (Final Review)**:
- ❌ **NO** Jane Street integration
- Uses: Manual review
- **Should use**: Jane Street quality gates

---

## Custom Modes with Firebase Hook

### Current Custom Modes

**`.bob/custom_modes.yaml`**:
- `v12-epic-planner` - Phase 2 (Architecture)
- `v12-engineer` - Phase 5 (Execution)

**Firebase Hook**:
- Located in: `scripts/query_kb.py`
- Queries: Firestore knowledge base
- Returns: Jane Street HFT patterns, testing standards

### Which Phases Should Use Firebase Hook?

**SHOULD USE** (High Priority):
1. ✅ Phase 2 (Architecture) - ALREADY USES
2. ✅ Phase 5 (Execution) - ALREADY USES
3. ⚠️ Phase 4.5 (Ticket Review) - NEW PHASE (should use)
4. ⚠️ Phase 3 (Audit) - SHOULD ADD
5. ⚠️ Phase 5.V (Verification) - SHOULD ADD

**COULD USE** (Medium Priority):
6. Phase 1 (Scope) - For extraction pattern guidance
7. Phase 4 (Tickets) - For test pattern templates

**DON'T NEED** (Low Priority):
8. Phase -1 (Pre-flight) - Local script, no AI
9. Phase 0 (Hotspot) - Data gathering only
10. Phase 1.5 (Boundary) - Simple validation
11. Phase 6 (Final Review) - Retrospective only

---

## Recommended Phase Structure (Final)

### User-Approved Configuration

| Phase | Name | Time | Agent | Jane Street | Firebase Hook |
|-------|------|------|-------|-------------|---------------|
| **-1** | Pre-flight | 5 min | Script | ❌ | ❌ |
| **0** | Hotspot | 10 min | Ask | ❌ | ❌ |
| **1** | Scope + Boundary | 20 min | Plan | ⚠️ | ⚠️ |
| **2** | Architecture | 25 min | Plan | ✅ | ✅ |
| **3** | Audit | 10 min | Advanced | ⚠️ | ⚠️ |
| **4** | Tickets | 10 min | Plan | ⚠️ | ⚠️ |
| **4.5** | Ticket Review | 10 min | Advanced | ✅ | ✅ |
| **5** | Execution | 10 min/ticket | Bob CLI | ✅ | ✅ |
| **5.V** | Verification | 5 min/ticket | Advanced | ⚠️ | ⚠️ |
| **6** | Final Review | 10 min | Advanced | ❌ | ❌ |

**Total Phases**: 10
**Planning Time**: 100 minutes per epic
**Jane Street Gates**: 3 (Phase 2, 4.5, 5)

---

## Implementation Changes Required

### 1. Add Phase 4.5 (Ticket Review)

**Create**: `scripts/phase_4_5_ticket_review_mcp.py`

```python
# Phase 4.5: Ticket Review
# Reviews tickets against architecture plan and Jane Street rules

def execute_phase_4_5(epic_id):
    # 1. Load architecture plan
    arch_plan = load_file(f"docs/brain/{epic_id}/02-architecture-plan.md")
    
    # 2. Load tickets
    tickets = load_file(f"docs/brain/{epic_id}/04-tickets.md")
    
    # 3. Query Jane Street KB for validation patterns
    jane_street_patterns = query_kb("extraction patterns, test coverage")
    
    # 4. Validate each ticket
    for ticket in tickets:
        validate_ticket(ticket, arch_plan, jane_street_patterns)
    
    # 5. Output review report
    write_file(f"docs/brain/{epic_id}/04-tickets-review.md", review_report)
```

### 2. Add Firebase Hook to Phase 3 (Audit)

**Update**: `scripts/phase_3_audit_mcp.py`

```python
# Add Jane Street KB query
jane_street_rules = query_kb("code quality, complexity thresholds")
```

### 3. Add Firebase Hook to Phase 5.V (Verification)

**Update**: `scripts/phase_5_verify_mcp.py`

```python
# Add Jane Street test coverage patterns
test_patterns = query_kb("test coverage, verification standards")
```

---

## Final Recommendation

**Phase Structure**: Option 2 (10 phases)
- Keep Phase -1 separate (local pre-flight)
- Keep Phase 0 separate (data gathering checkpoint)
- Merge Phase 1 + 1.5 (Scope + Boundary)
- Keep Phase 3 separate (Audit)
- Keep Phase 4 separate (Tickets)
- **ADD Phase 4.5** (Ticket Review with Jane Street validation)

**Jane Street Integration**:
- Phase 2: ✅ Already integrated
- Phase 4.5: ✅ Add integration (NEW)
- Phase 5: ✅ Already integrated
- Phase 3: ⚠️ Add integration (enhancement)
- Phase 5.V: ⚠️ Add integration (enhancement)

**Time Savings**: 10 minutes per epic (vs 11-phase model)
**Quality Gates**: 3 Jane Street validation points (Phase 2, 4.5, 5)

---

## Summary

**User is RIGHT** to reject Phase 3 + 4 merge:
- Audit and Tickets are distinct concerns
- Losing checkpoint between them is risky

**User's proposed merges**:
- Phase -1 + 0 + 1: ❌ NO (location mismatch)
- Phase 0 + 1: ⚠️ RISKY (lose checkpoint)
- **Recommendation**: Keep Phase 0 separate, merge Phase 1 + 1.5 only

**New Phase 4.5**:
- ✅ EXCELLENT IDEA
- Catches mistakes early
- Validates Jane Street compliance before execution
- Prevents wasted execution time

**Jane Street Integration**:
- Currently: 2 phases (Phase 2, 5)
- Should be: 5 phases (Phase 2, 3, 4.5, 5, 5.V)
- Priority: Add to Phase 4.5 first (new gate)