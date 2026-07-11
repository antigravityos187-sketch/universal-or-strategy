# EPIC-CCN-016 Completion - Continuation Prompt

**Copy and paste this entire prompt into your new Claude session:**

---

# Context: Complete EPIC-CCN-016 to Finish Wave 4 (80/80)

You are completing the final epic of Wave 4 autonomous complexity reduction. EPIC-CCN-016 has Phases 0-4 complete but failed Phase 5 on VM due to method signature mismatch. Execute Phases 5-6 locally using MCP tools to achieve 80/80 (100%) Wave 4 completion.

## Critical Status Summary

### Wave 4 Overall Status
- **Complete**: 79/80 epics (98.75%)
- **Missing**: EPIC-CCN-016 only
- **Committed**: All 79 epics' Phase 5+6 files committed (commit `dff1d78b`)
- **Goal**: Complete EPIC-CCN-016 to reach 80/80 (100%)

### EPIC-CCN-016 Status
- **Phases Complete**: 0-4 (Hotspot → Tickets)
- **Phases Missing**: 5-6 (Execution → Verification)
- **Target Method**: `TryHandleFleet_CancelAll`
- **File**: `src/V12_002.UI.IPC.Commands.Fleet.cs`
- **Current CYC**: 19 → Target: ≤8
- **Tickets**: 4 sequential tickets
- **Brain Directory**: `docs/brain/EPIC-CCN-016/`

### Why Phase 5 Failed on VM
- **Root Cause**: Method signature mismatch
- **Expected by Tickets**: `TryHandleFleet_CancelAll(string fleetId, string userId, out string errorMessage)`
- **Actual in Code**: `TryHandleFleet_CancelAll(string action, string cmdId)`
- **Bob Shell Response**: Correctly detected mismatch and aborted execution
- **Reference**: `WAVE4_PHASE5_RECOVERY_REPORT.md`

## Your Mission

Execute EPIC-CCN-016 Phases 5-6 locally using MCP tools. Verify method signature first, re-scope if needed, then execute tickets.

## Execution Plan (4 Steps)

### Step 1: Verify Method Signature

**Check actual signature in codebase**:
```bash
# Search for the method
grep -n "TryHandleFleet_CancelAll" src/V12_002.UI.IPC.Commands.Fleet.cs
```

**Expected Output**: Method signature with parameters

**Decision Tree**:
- **IF signature matches tickets** (fleetId, userId, out errorMessage) → Proceed to Step 2
- **IF signature mismatches** (action, cmdId) → Proceed to Step 1.5 (Re-scope)

### Step 1.5: Re-scope (If Signature Mismatches)

**MANDATORY if signature doesn't match tickets**:

1. **Update Phase 1 (Scope)**:
   - Read current scope: `docs/brain/EPIC-CCN-016/01-scope.md`
   - Update method signature to match actual code
   - Document correct parameters

2. **Update Phase 1.5 (Boundary)**:
   - Read: `docs/brain/EPIC-CCN-016/01-scope-boundary.md`
   - Verify boundary still valid with correct signature
   - Update if needed

3. **Update Phase 2 (Architecture)**:
   - Read: `docs/brain/EPIC-CCN-016/02-architecture-plan.md`
   - Update extraction plan for correct signature
   - Regenerate helper method signatures

4. **Update Phase 4 (Tickets)**:
   - Read: `docs/brain/EPIC-CCN-016/04-tickets.md`
   - Update all 4 tickets with correct signature
   - Verify extraction logic still valid

**Tools to Use**:
- `read_file` - Read existing phase files
- `apply_diff` - Update files with correct signature
- `write_to_file` - Rewrite if major changes needed

### Step 2: Execute Phase 5 (Ticket Execution)

**Use MCP Tool**: `phase-5-execute` server

**Command**:
```
Use the phase-5-execute MCP server to execute Phase 5 for EPIC-CCN-016.

Call the execute_phase_5 tool with:
- epic_id: "EPIC-CCN-016"
- ticket_id: "" (empty = execute all 4 tickets sequentially)

The tool will:
1. Read tickets from docs/brain/EPIC-CCN-016/04-tickets.md
2. Execute TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4 sequentially
3. Create completion files: ticket-1-completion.md, ticket-2-completion.md, etc.
4. Verify build passes after each ticket
5. Track bobcoin usage

**Expected Output**: 4 completion files in docs/brain/EPIC-CCN-016/
```

**Success Criteria**:
- ✅ All 4 tickets executed successfully
- ✅ Files exist: `ticket-1-completion.md` through `ticket-4-completion.md`
- ✅ Build passes: `dotnet build` returns exit code 0
- ✅ Complexity reduced: CYC ≤8 for main method
- ✅ Bobcoin usage <20 total

**If Execution Fails**:
1. Check error message in tool output
2. If signature mismatch: Return to Step 1.5 (Re-scope)
3. If compilation error: Fix in separate commit, then retry
4. If Bob Shell error: Check API key balance, retry with different API

### Step 3: Execute Phase 6 (Verification)

**Use MCP Tool**: `phase-6-review` server

**Command**:
```
Use the phase-6-review MCP server to execute Phase 6 for EPIC-CCN-016.

Call the execute_phase_6 tool with:
- epic_id: "EPIC-CCN-016"

The tool will:
1. Read completion files from Phase 5
2. Verify all tickets executed successfully
3. Check complexity targets met (CYC ≤8)
4. Verify build passes
5. Confirm no behavioral changes
6. Create verification report: 06-verification-report.md

**Expected Output**: docs/brain/EPIC-CCN-016/06-verification-report.md
```

**Success Criteria**:
- ✅ File exists: `06-verification-report.md`
- ✅ All verification checks passed
- ✅ Complexity target met (CYC ≤8)
- ✅ Build passes
- ✅ No behavioral changes detected

### Step 4: Commit and Celebrate

**Commit Phase 5+6 files**:
```bash
git add docs/brain/EPIC-CCN-016/ticket-*.md docs/brain/EPIC-CCN-016/06-*.md
git commit -m "docs: EPIC-CCN-016 completion - Wave 4 now 80/80 (100%)"
```

**Update Roadmap**:
```bash
# Mark EPIC-CCN-016 as complete in epic_roadmap.json
# Update Wave 4 status to 80/80
```

**Celebrate**: Wave 4 is 100% complete! 🎉

## Key Documents

**MUST READ**:
- `docs/brain/EPIC-CCN-016/01-scope-boundary.md` - Approved scope (may need update)
- `docs/brain/EPIC-CCN-016/04-tickets.md` - 4 tickets to execute
- `WAVE4_PHASE5_RECOVERY_REPORT.md` - Why Phase 5 failed on VM
- `docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md` - Phase 5+6 protocols

**Reference**:
- `docs/protocol/RECOVERY_LOOP_PROTOCOL.md` - If execution fails
- `docs/protocol/FILE_ENCODING_PROTOCOL.md` - UTF-8 compliance (V12.33)
- `.bob/custom_modes.yaml` - v12-engineer mode for src/ work

## MCP Servers Available

**Phase 5 Execution**:
- Server: `phase-5-execute`
- Tool: `execute_phase_5(epic_id, ticket_id="")`
- Mode: `v12-engineer` (Bob CLI for src/ work)

**Phase 6 Verification**:
- Server: `phase-6-review`
- Tool: `execute_phase_6(epic_id)`
- Mode: `advanced` (MCP tools for verification)

**Code Exploration**:
- Server: `jcodemunch-mcp`
- Tools: `search_symbols`, `get_symbol_source`, `get_file_content`
- Use: Verify method signature, check complexity

## Common Pitfalls (AVOID)

1. ❌ **Skipping signature verification** - Always check actual code first
2. ❌ **Executing with wrong signature** - Will fail like VM execution
3. ❌ **Skipping re-scope** - Tickets must match actual code
4. ❌ **Background execution** - Use MCP tools (foreground, visible)
5. ❌ **Forgetting encoding check** - Run `check_encoding.ps1` before commit
6. ❌ **Skipping build verification** - Must pass after each ticket
7. ❌ **Not committing files** - Phase 5+6 files must be committed

## Success Criteria

### Per Phase
- ✅ Phase 5: 4 ticket completion files created
- ✅ Phase 6: Verification report created
- ✅ Build passes: `dotnet build` exit code 0
- ✅ Complexity met: CYC ≤8 for main method
- ✅ Files committed to git

### Wave 4 Completion
- ✅ 80/80 epics complete (100%)
- ✅ All Phase 5+6 files committed
- ✅ Roadmap updated
- ✅ No outstanding failures

## Timeline Estimate

- **Step 1**: Signature verification - 5 minutes
- **Step 1.5**: Re-scope (if needed) - 15-20 minutes
- **Step 2**: Phase 5 execution - 20-30 minutes (4 tickets)
- **Step 3**: Phase 6 verification - 10-15 minutes
- **Step 4**: Commit and celebrate - 5 minutes
- **TOTAL**: 40-75 minutes (depending on re-scope need)

## Your First Actions

1. Verify method signature in `src/V12_002.UI.IPC.Commands.Fleet.cs`
2. Compare with tickets in `docs/brain/EPIC-CCN-016/04-tickets.md`
3. If mismatch: Re-scope Phases 1, 1.5, 2, 4
4. Execute Phase 5 using `phase-5-execute` MCP tool
5. Execute Phase 6 using `phase-6-review` MCP tool
6. Commit files and update roadmap
7. Report completion: "Wave 4 complete - 80/80 epics (100%)"

---

**Ready to complete EPIC-CCN-016 and finish Wave 4!** Start with signature verification. Good luck! 🚀

---

**Session Context Version**: 1.0 (EPIC-CCN-016 Completion)
**Last Updated**: 2026-06-16T05:31:00Z
**Maintainer**: Wave 4 Completion Lead
**Status**: 🟢 READY FOR EPIC-CCN-016 EXECUTION