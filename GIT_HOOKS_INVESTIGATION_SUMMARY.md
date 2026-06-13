# Git Hooks Investigation Summary

**Date**: 2026-06-13
**Investigator**: Claude (Advanced Mode)
**Purpose**: Investigate current graphify and jCodemunch update patterns to design consolidated git hooks system

---

## 🔍 Investigation Results

### Graphify Update Patterns

**Current State**:
- ❌ **NOT automated** - Manual `graphify update .` command only
- 📍 **51 references** found across documentation
- ⏱️ **Performance**: 30-60 seconds (slow, but AST-only, no API cost)
- 📂 **Output**: `graphify-out/graph.json`, `GRAPH_REPORT.md`, `wiki/`

**Current Triggers** (all manual):
1. After "major structural changes to src/"
2. After epic completion (recommended in MCP_UPDATE_PROTOCOL.md)
3. V12.19 mandate: MUST run after ANY src/ structural change
4. Before architectural exploration (check graph first)

**Key Finding**: 
> **V12.19 Protocol Change** - Graphify updates are now MANDATORY after ANY src/ structural change, with NO skip rules allowed. This was formalized to prevent stale knowledge graphs.

**Problem**:
- Agents forget to run updates manually
- Knowledge graph becomes stale
- Reduces navigation efficiency (71x token savings lost)

### jCodemunch Update Patterns

**Current State**:
- ❌ **NOT automated** - Manual MCP tool calls only
- 📍 **30 references** found across documentation
- ⏱️ **Performance**: Fast (`register_edit`), Medium (`index_file`), Slow (`index_folder`)

**Current Triggers** (all manual):
1. **Session start**: `resolve_repo`, `index_folder` if not indexed
2. **After edits**: `register_edit(repo, file_paths)` - cache invalidation
3. **Single file**: `index_file(path)` - re-index one file
4. **Batch update**: `index_folder(path, incremental=true)` - incremental re-index

**Known Issues**:
- **Stale Index Problem** (documented in `autonomous_refactor_progress.md`):
  - Index dated 2026-05-19, actual code refactored since
  - Example: `MonitorRmaProximity` reported CYC 32, actual is CYC 7
  - Caused **EPIC-CCN-1 failure** (wrong complexity data led to bad planning)
- **Manual Updates**: Agents forget to call `register_edit` or `index_file`
- **No Automation**: No git hooks or automatic triggers

**Recommended Strategy** (from MCP_UPDATE_PROTOCOL.md):
```
Every Edit → register_edit(repo, file_paths)  ✅ RECOMMENDED
Every Commit → index_file(path)               ⚠️ Too infrequent
Every Task → index_folder(incremental=true)   ❌ Too slow
```

### PROJECT_DIRECTORY.md Update Patterns

**Current State**:
- ❌ **NOT automated** - Manual updates only
- ❌ **No update mechanism** - Becomes stale over time
- ✅ **Critical for navigation** - "Mall directory" for entire codebase

**Content**:
- Commands and what they use
- Skills and what they implement
- SOPs and who follows them
- Tools and what they update
- Complete dependency maps

**Problem**:
- No tracking of when it was last updated
- No automated scanning of new files
- Agents must manually update after adding commands/skills/SOPs

---

## 🏗️ Proposed Solution: Consolidated Git Hooks

### Architecture Overview

```
.git/hooks/
├── modules/
│   ├── bob-notes.sh           # Extract from existing hooks
│   ├── v12-protection.sh      # Extract from pre-commit
│   ├── project-directory.sh   # NEW - Auto-update directory
│   ├── graphify-update.sh     # NEW - Knowledge graph
│   └── jcodemunch-index.sh    # NEW - Code index
├── pre-commit                 # Orchestrator (V12 protection + cleanup)
├── post-commit                # Orchestrator (notes + updates)
├── post-checkout              # Orchestrator (cleanup)
└── post-merge                 # Orchestrator (cleanup + updates)
```

### Key Design Decisions

#### 1. Modular Architecture
- **Why**: Separation of concerns, easier testing, reusable functions
- **How**: Each concern in separate `.sh` module, sourced by orchestrators

#### 2. Non-Blocking Updates
- **Why**: Don't slow down commits, failures shouldn't block workflow
- **How**: Run updates in background (`&`), log failures but don't exit 1

#### 3. Smart Triggers
- **PROJECT_DIRECTORY.md**: Only on creates/deletes/moves in tracked dirs
- **Graphify**: Only on ANY src/ change (V12.19 compliance)
- **jCodemunch**: Only on ANY src/ change, use `register_edit` + `index_file`

#### 4. Performance Optimization
- **Fast operations**: Run synchronously (<1s)
- **Slow operations**: Run in background (30-60s)
- **Skip conditions**: Check if update needed before running

### Trigger Matrix

| Hook | Bob Notes | V12 Protection | PROJECT_DIR | Graphify | jCodemunch |
|------|-----------|----------------|-------------|----------|------------|
| **pre-commit** | Cleanup | ✅ Check | - | - | - |
| **post-commit** | Attach + Sync | - | ✅ Update | ✅ Update | ✅ Update |
| **post-checkout** | Cleanup | - | - | - | - |
| **post-merge** | Cleanup | - | ✅ Update | ✅ Update | ✅ Update |

### Change Detection Strategy

**PROJECT_DIRECTORY.md**:
```bash
# Trigger on: A (added), D (deleted), R (renamed)
# In directories: .bob/commands/, .bob/skills/, docs/workflow/, scripts/
git diff-tree --name-status -r HEAD | grep -E '^[ADR].*(bob/commands|bob/skills|docs/workflow|scripts)/'
```

**Graphify**:
```bash
# Trigger on: ANY change in src/
git diff-tree --name-only -r HEAD | grep '^src/'
```

**jCodemunch**:
```bash
# Same as graphify, but use different update strategy:
# - <10 files: register_edit + index_file per file
# - >10 files: register_edit + index_folder(incremental=true)
```

---

## 📊 Impact Analysis

### Benefits

1. **Prevents Stale Data**:
   - Graphify graph always current (V12.19 compliance)
   - jCodemunch index always fresh (prevents EPIC-CCN-1 type failures)
   - PROJECT_DIRECTORY.md always accurate

2. **Reduces Agent Cognitive Load**:
   - No need to remember to run updates
   - Automatic compliance with V12.19 mandate
   - Focus on task, not maintenance

3. **Improves Navigation Efficiency**:
   - Fresh graphify graph = 71x token savings
   - Fresh jCodemunch index = accurate complexity data
   - Fresh PROJECT_DIRECTORY.md = faster file discovery

4. **Prevents Planning Failures**:
   - EPIC-CCN-1 failed due to stale jCodemunch index
   - Automatic updates prevent similar failures
   - Accurate data = better architectural decisions

### Risks

1. **Performance Impact**:
   - Graphify takes 30-60s (mitigated by background execution)
   - jCodemunch batch updates can be slow (mitigated by per-file strategy)
   - Solution: Run in background, don't block commits

2. **Tool Availability**:
   - What if `graphify` or `jcodemunch-mcp` not installed?
   - Solution: Check tool availability, skip gracefully if missing

3. **Windows Compatibility**:
   - Bash hooks on Windows (Git for Windows includes bash)
   - Solution: Test on Windows before deployment

4. **jCodemunch MCP Integration**:
   - How to call MCP tools from bash?
   - Options: Bob CLI wrapper, Python script, direct API
   - Solution: TBD - need to test integration methods

### Open Questions

1. **jCodemunch MCP from Bash**:
   - How to call `register_edit`, `index_file` from bash script?
   - Option A: `bob --yolo --chat-mode advanced "use_mcp_tool jcodemunch register_edit ..."`
   - Option B: Python wrapper script that calls MCP tools
   - Option C: Direct API calls (if jCodemunch has REST API)

2. **Rate Limiting**:
   - Should we skip graphify if last update <5 minutes ago?
   - Prevents excessive updates during rapid commits
   - Trade-off: Might miss some changes

3. **Conflict Resolution**:
   - What if multiple hooks try to update PROJECT_DIRECTORY.md?
   - Need locking mechanism or merge strategy
   - Git's built-in locking might be sufficient

---

## 🎯 Recommendations

### Immediate Actions

1. **Review Analysis**: User reviews `GIT_HOOKS_CONSOLIDATION_ANALYSIS.md` (700 lines)
2. **Brainstorm**: Discuss design decisions, open questions
3. **Decide on jCodemunch Integration**: Choose Option A, B, or C
4. **Prototype**: Create one module (e.g., `project-directory.sh`) as proof of concept

### Implementation Phases

**Phase 1: Extract Existing** (Low Risk)
- Extract `bob-notes.sh` from existing hooks
- Extract `v12-protection.sh` from pre-commit
- Test that existing functionality still works
- **Estimated Time**: 1-2 hours

**Phase 2: Create New Modules** (Medium Risk)
- Implement `project-directory.sh` (simplest, no external tools)
- Implement `graphify-update.sh` (CLI tool, straightforward)
- Implement `jcodemunch-index.sh` (needs MCP integration solution)
- Test each module independently
- **Estimated Time**: 3-4 hours

**Phase 3: Update Orchestrators** (Medium Risk)
- Update all 4 hooks to use modules
- Test full workflow (commit, merge, checkout)
- Verify performance acceptable
- **Estimated Time**: 2-3 hours

**Phase 4: Documentation** (Low Risk)
- Create `docs/workflow/GIT_HOOKS_GUIDE.md`
- Update PROJECT_DIRECTORY.md with hook info
- Add troubleshooting section
- **Estimated Time**: 1-2 hours

**Total Estimated Time**: 7-11 hours

### Success Criteria

- ✅ All existing hook functionality preserved
- ✅ PROJECT_DIRECTORY.md auto-updates on structural changes
- ✅ Graphify auto-updates on src/ changes (V12.19 compliance)
- ✅ jCodemunch auto-updates on src/ changes
- ✅ Commit time <2 seconds (excluding background tasks)
- ✅ No false positives or false negatives
- ✅ Clear error messages for debugging

---

## 📚 Key Documents Created

1. **`docs/workflow/GIT_HOOKS_CONSOLIDATION_ANALYSIS.md`** (700 lines)
   - Complete technical specification
   - Module designs and interfaces
   - Performance considerations
   - Testing strategy
   - Rollback plan

2. **`GIT_HOOKS_INVESTIGATION_SUMMARY.md`** (this document)
   - Executive summary of findings
   - Current state analysis
   - Proposed solution overview
   - Recommendations and next steps

---

## 🤔 Discussion Points for Brainstorming

### 1. jCodemunch MCP Integration
**Question**: How should we call jCodemunch-MCP tools from bash hooks?

**Option A: Bob CLI Wrapper**
```bash
bob --yolo --chat-mode advanced "use_mcp_tool jcodemunch register_edit ..."
```
- ✅ Uses existing Bob infrastructure
- ❌ Slow (Bob startup overhead)
- ❌ Complex command construction

**Option B: Python Wrapper Script**
```bash
python scripts/jcodemunch_hook.py register_edit --files="$changed_files"
```
- ✅ Fast (direct MCP call)
- ✅ Clean interface
- ❌ Requires Python script maintenance

**Option C: Direct API Calls**
```bash
curl -X POST http://localhost:3000/mcp/jcodemunch/register_edit ...
```
- ✅ Fastest
- ❌ Requires jCodemunch MCP server running
- ❌ May not be available

**Recommendation**: Start with Option B (Python wrapper), fall back to Option A if needed.

### 2. Rate Limiting Graphify
**Question**: Should we skip graphify updates if last update was <5 minutes ago?

**Pros**:
- Prevents excessive updates during rapid commits
- Reduces system load
- Still complies with V12.19 (updates happen, just batched)

**Cons**:
- Might miss some changes temporarily
- Adds complexity to skip logic
- Could cause confusion ("why didn't it update?")

**Recommendation**: Start without rate limiting, add if performance issues arise.

### 3. Background vs Synchronous
**Question**: Which operations should run in background vs synchronously?

**Current Proposal**:
- **Synchronous** (blocking): V12 protection, Bob notes attachment
- **Background** (non-blocking): Graphify, jCodemunch, PROJECT_DIRECTORY.md

**Alternative**: Run everything synchronously, but with timeouts
- ✅ Simpler logic
- ✅ Guaranteed completion before next command
- ❌ Slower commits (30-60s for graphify)

**Recommendation**: Stick with background execution for slow operations.

---

## 🚀 Next Steps

1. **User Review** (Now):
   - Read `GIT_HOOKS_CONSOLIDATION_ANALYSIS.md`
   - Discuss design decisions
   - Answer open questions
   - Approve or request changes

2. **Prototype** (After Approval):
   - Create `project-directory.sh` module
   - Test independently
   - Integrate into `post-commit`
   - Verify it works

3. **Full Implementation** (After Prototype Success):
   - Follow 4-phase plan
   - Test thoroughly
   - Deploy gradually
   - Monitor for issues

4. **Documentation** (Parallel with Implementation):
   - Create user guide
   - Add troubleshooting section
   - Update PROJECT_DIRECTORY.md
   - Document lessons learned

---

## 📝 Session Context

**Current Task**: Wave 2 Phase 4 (Ticket Generation) running on VM
- 8 epics executing in parallel
- EPIC-107 actively generating tickets (visible in terminal)
- User watching via VSCode Remote-SSH

**This Investigation**: Triggered by user request to integrate graphify and jCodemunch updates into git hooks system, alongside PROJECT_DIRECTORY.md auto-updates.

**Status**: Investigation complete, awaiting user review and brainstorming session.