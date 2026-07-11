# Wave 4 Phase 4 - Recovery Plan for Failed Epics

**Date**: 2026-06-15
**Status**: 3 epics require recovery before Phase 5

## Executive Summary

3 epics failed during Phase 4 execution. Analysis shows:
- **EPIC-CCN-044**: Missing Phase 2/3 prerequisites (clear path forward)
- **EPIC-CCN-065**: Critical error after MCP tool execution (needs investigation)
- **EPIC-CCN-074**: MCP connection error (phase-0-hotspot server issue)

**Decision**: YES, we must resolve these before Phase 5 to maintain 100% completion target.

## Failure Analysis

### EPIC-CCN-044: Missing Prerequisites ✅ CLEAR PATH

**Root Cause**: Phase 2 and Phase 3 were never executed for this epic.

**Evidence**:
```
Phase 0: Hotspot Detection - COMPLETED
Phase 1.0: Scope Definition - COMPLETED  
Phase 1.5: Scope Boundary Validation - APPROVED
Phase 2.0: Architecture Planning - NOT STARTED ❌
Phase 3.0: DNA & PR Audit - NOT STARTED ❌
Phase 4.0: Ticket Generation - BLOCKED ❌
```

**Missing Files**:
- `docs/brain/EPIC-CCN-044/02-architecture-plan.md`
- `docs/brain/EPIC-CCN-044/03-audit-report.md`

**Recovery Steps**:
1. Execute Phase 2: `use_mcp_tool` with server `phase-2-architecture`, tool `execute_phase_2`, epic_id `EPIC-CCN-044`
2. Execute Phase 3: `use_mcp_tool` with server `phase-3-audit`, tool `execute_phase_3`, epic_id `EPIC-CCN-044`
3. Execute Phase 4: `use_mcp_tool` with server `phase-4-tickets`, tool `execute_phase_4`, epic_id `EPIC-CCN-044`

**Estimated Time**: 30 minutes (10 min per phase)
**Estimated Bobcoins**: ~15 (5 per phase)
**Confidence**: VERY HIGH

---

### EPIC-CCN-065: Critical Error ⚠️ NEEDS INVESTIGATION

**Root Cause**: Unexpected critical error after MCP tool executed successfully.

**Evidence**:
```
[using tool execute_phase_4: {"epic_id":"EPIC-CCN-065"}]
---output---
{"status":"success","message":"Phase 4 context prepared for EPIC-CCN-065",...}
---output---
An unexpected critical error occurred:
[object Object]
```

**Observations**:
- MCP tool returned success
- Context was prepared correctly
- Error occurred AFTER tool execution (likely in Bob Shell processing)
- Error message is generic: `[object Object]` (unhelpful)

**Possible Causes**:
1. Timeout during file writing
2. Memory issue during large file processing
3. Bob Shell internal error
4. File system permission issue

**Recovery Steps**:
1. **Verify Prerequisites**: Check Phase 2/3 files exist
2. **Manual Retry**: Execute Phase 4 manually in Claude session (not VM)
3. **Increase Timeout**: If timeout suspected, use longer timeout
4. **Check Logs**: Look for more detailed error in Bob Shell logs

**Estimated Time**: 20-30 minutes (includes investigation)
**Estimated Bobcoins**: ~5-10
**Confidence**: MEDIUM (needs investigation)

---

### EPIC-CCN-074: MCP Connection Error ⚠️ INFRASTRUCTURE ISSUE

**Root Cause**: `phase-0-hotspot` MCP server connection failed.

**Evidence**:
```
[ERROR] Error during discovery for server 'phase-0-hotspot': 
Connection failed for 'phase-0-hotspot': MCP error -32000: Connection closed
An unexpected critical error occurred:
[object Object]
```

**Observations**:
- Error occurred during MCP server discovery (before tool execution)
- `phase-0-hotspot` server failed to connect
- This is an infrastructure issue, not epic-specific

**Possible Causes**:
1. `phase-0-hotspot` server not running on VM
2. MCP configuration issue in `.bob/mcp.json`
3. Server crashed during execution
4. Port conflict or resource exhaustion

**Recovery Steps**:
1. **Check MCP Config**: Verify `.bob/mcp.json` has correct `phase-0-hotspot` entry
2. **Verify Server**: SSH to VM and check if server is running
3. **Restart Server**: If crashed, restart the MCP server
4. **Manual Retry**: Execute Phase 4 after server is healthy

**Estimated Time**: 15-20 minutes (includes server check)
**Estimated Bobcoins**: ~5
**Confidence**: MEDIUM (infrastructure dependent)

---

## Recovery Strategy

### Option 1: Sequential Recovery (RECOMMENDED)

Execute recoveries one at a time in order of confidence:

**Step 1: EPIC-CCN-044** (Highest confidence)
- Execute Phase 2, 3, 4 sequentially
- Verify each phase before proceeding
- Expected success: 95%

**Step 2: EPIC-CCN-074** (Infrastructure fix)
- Check/fix MCP server issue
- Retry Phase 4
- Expected success: 80%

**Step 3: EPIC-CCN-065** (Needs investigation)
- Investigate error cause
- Manual retry with increased timeout
- Expected success: 70%

**Total Time**: 1-1.5 hours
**Total Bobcoins**: ~25-30

---

### Option 2: Parallel Recovery (FASTER, RISKIER)

Execute all 3 recoveries in parallel on VM:
- Generate recovery scripts for all 3 epics
- Upload to VM
- Launch simultaneously
- Monitor for completion

**Pros**: Faster (30-40 minutes)
**Cons**: Harder to debug if multiple fail

**Recommendation**: Use Option 1 (sequential) for better control and debugging.

---

## Recovery Execution Plan

### Prerequisites
1. ✅ VM accessible
2. ✅ MCP servers configured
3. ✅ Bob Shell API keys valid
4. ⏳ Verify `phase-0-hotspot` server status

### Step-by-Step Recovery

#### Recovery 1: EPIC-CCN-044

```bash
# Phase 2
use_mcp_tool:
  server: phase-2-architecture
  tool: execute_phase_2
  args: {"epic_id": "EPIC-CCN-044"}

# Verify: docs/brain/EPIC-CCN-044/02-architecture-plan.md exists

# Phase 3
use_mcp_tool:
  server: phase-3-audit
  tool: execute_phase_3
  args: {"epic_id": "EPIC-CCN-044"}

# Verify: docs/brain/EPIC-CCN-044/03-audit-report.md exists

# Phase 4
use_mcp_tool:
  server: phase-4-tickets
  tool: execute_phase_4
  args: {"epic_id": "EPIC-CCN-044"}

# Verify: docs/brain/EPIC-CCN-044/04-tickets.md exists
```

#### Recovery 2: EPIC-CCN-074

```bash
# Check MCP server status
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && cat .bob/mcp.json | grep phase-0-hotspot"

# If server missing, add to .bob/mcp.json
# If server present, retry Phase 4

use_mcp_tool:
  server: phase-4-tickets
  tool: execute_phase_4
  args: {"epic_id": "EPIC-CCN-074"}

# Verify: docs/brain/EPIC-CCN-074/04-tickets.md exists
```

#### Recovery 3: EPIC-CCN-065

```bash
# Verify prerequisites exist
ls docs/brain/EPIC-CCN-065/02-architecture-plan.md
ls docs/brain/EPIC-CCN-065/03-audit-report.md

# Manual retry with increased timeout
use_mcp_tool:
  server: phase-4-tickets
  tool: execute_phase_4
  args: {"epic_id": "EPIC-CCN-065"}

# If fails again, execute manually in Claude session (not VM)
```

---

## Success Criteria

### Per Epic
- ✅ File exists: `docs/brain/EPIC-CCN-{ID}/04-tickets.md`
- ✅ File size >1K
- ✅ Manifest updated with phase4_complete
- ✅ No errors in logs

### Overall
- ✅ 80/80 epics complete (100% success)
- ✅ All files synced to local
- ✅ Epic roadmap updated
- ✅ Ready for Phase 5

---

## Risk Assessment

### EPIC-CCN-044
- **Risk**: LOW
- **Confidence**: 95%
- **Blocker**: None

### EPIC-CCN-074
- **Risk**: MEDIUM
- **Confidence**: 80%
- **Blocker**: MCP server health

### EPIC-CCN-065
- **Risk**: MEDIUM-HIGH
- **Confidence**: 70%
- **Blocker**: Unknown error cause

### Overall Wave 4 Risk
- **Current**: 77/80 (96.25%)
- **Target**: 80/80 (100%)
- **Gap**: 3 epics
- **Estimated Recovery Success**: 85-90%

---

## Decision Point

**Question**: Should we proceed with Phase 5 with 77/80 complete, or recover the 3 failed epics first?

**Recommendation**: **RECOVER FIRST**

**Rationale**:
1. **Completeness**: 100% target is achievable with 1-1.5 hours effort
2. **Dependencies**: Phase 5 may depend on Phase 4 tickets
3. **Quality**: Better to fix now than accumulate technical debt
4. **Confidence**: High success probability (85-90%)
5. **Budget**: Only ~25-30 bobcoins needed (1% of budget)

**Alternative**: If time-constrained, proceed with Phase 5 for 77 epics and recover the 3 separately.

---

## Next Steps

### Immediate (This Session)
1. ✅ Recovery plan created
2. ⏳ User decision: Recover now or proceed to Phase 5?

### If Recover Now
1. Execute Recovery 1 (EPIC-CCN-044)
2. Execute Recovery 2 (EPIC-CCN-074)
3. Execute Recovery 3 (EPIC-CCN-065)
4. Verify all 80/80 complete
5. Update roadmap
6. Proceed to Phase 5

### If Proceed to Phase 5
1. Generate Phase 5 scripts for 77 successful epics
2. Execute Phase 5 wave
3. Recover 3 failed epics separately
4. Complete Phase 5 for recovered epics

---

**Status**: 🟡 AWAITING USER DECISION
**Recommendation**: RECOVER FIRST (1-1.5 hours, 85-90% success)