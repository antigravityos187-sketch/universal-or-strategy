# MCP Orchestration Lessons - EPIC-CCN-027

**Date**: 2026-06-16
**Context**: Attempting to complete EPIC-CCN-027 locally using MCP phase tools
**Goal**: Document MCP orchestration workflow for building blocks

## What We Tried

### Attempt 1: Direct MCP Tool Call
```xml
<use_mcp_tool>
<server_name>phase-5-execute</server_name>
<tool_name>execute_phase_5</tool_name>
<arguments>
{
  "epic_id": "EPIC-CCN-027",
  "ticket_id": "TICKET-2"
}
</arguments>
</use_mcp_tool>
```

**Result**: ✅ SUCCESS - Tool returned context bundle with instructions
**Issue**: MCP tool is a **coordinator**, not an **executor**
- Returns instructions for Bob CLI
- Does NOT spawn independent agent
- Does NOT execute work directly

### Attempt 2: Spawn Independent Agent with new_task
```xml
<new_task>
<mode>v12-engineer</mode>
<message>[Detailed TICKET-2 instructions]</message>
<todos>[10 checklist items]</todos>
</new_task>
```

**Result**: ⚠️ CANCELED - User stopped the spawned task
**Reason**: User wanted to document the workflow first

## Key Discoveries

### 1. MCP Phase Tools Are Coordinators
**What they do**:
- Prepare context bundles
- Return structured instructions
- Specify input/output files
- Define success criteria

**What they DON'T do**:
- Execute code changes
- Run tests
- Spawn independent agents
- Persist files

### 2. Execution Requires Additional Step
**Options**:
1. **Bob CLI** (preferred for VM): `bob --yolo "$(cat instructions.txt)"`
2. **new_task** (preferred for local): Spawn independent Claude session
3. **Direct execution** (fallback): Use Advanced mode tools directly

### 3. Local vs VM Execution
**VM Workflow** (Wave 4 Phases 0-6):
```bash
# 1. Generate script using building-blocks
cat > script.sh << 'EOF'
#!/bin/bash
export BOBSHELL_API_KEY="..."
cat > /tmp/msg.txt << 'EOFMSG'
Use phase-5-execute MCP to execute Phase 5 for EPIC-CCN-X
EOFMSG
bob --yolo "$(cat /tmp/msg.txt)"
EOF

# 2. Upload to VM
gcloud compute scp script.sh vm:~/

# 3. Execute in screen session
screen -dmS epic bash -l -c './script.sh'
```

**Local Workflow** (EPIC-CCN-027):
```xml
<!-- 1. Call MCP tool to get instructions -->
<use_mcp_tool>
<server_name>phase-5-execute</server_name>
<tool_name>execute_phase_5</tool_name>
<arguments>{"epic_id": "EPIC-CCN-027", "ticket_id": "TICKET-2"}</arguments>
</use_mcp_tool>

<!-- 2. Spawn independent agent with instructions -->
<new_task>
<mode>v12-engineer</mode>
<message>[MCP instructions + context]</message>
<todos>[Checklist from MCP]</todos>
</new_task>

<!-- 3. Monitor spawned task completion -->
<!-- 4. Verify output files created -->
```

## Recommended Workflow for Local Execution

### Phase 5 (Ticket Execution)
1. **Call MCP**: `use_mcp_tool(phase-5-execute, epic_id, ticket_id)`
2. **Extract Instructions**: Parse returned context bundle
3. **Spawn Agent**: `new_task(v12-engineer, instructions, todos)`
4. **Monitor**: Wait for spawned task completion
5. **Verify**: Check output file exists (`ticket-X-completion.md`)

### Phase 5.V (Verification)
1. **Call MCP**: `use_mcp_tool(phase-5-verify, epic_id)`
2. **Extract Instructions**: Parse returned context bundle
3. **Spawn Agent**: `new_task(advanced, instructions, todos)`
4. **Monitor**: Wait for verification completion
5. **Verify**: Check output file exists (`05-verification-report.md`)

### Phase 6 (Final Review)
1. **Call MCP**: `use_mcp_tool(phase-6-review, epic_id)`
2. **Extract Instructions**: Parse returned context bundle
3. **Spawn Agent**: `new_task(advanced, instructions, todos)`
4. **Monitor**: Wait for review completion
5. **Verify**: Check output file exists (`06-completion-report.md`)

## Building Block Template

### Script: `execute_epic_local.sh`
```bash
#!/bin/bash
# Local Epic Execution using MCP Orchestration
# Usage: ./execute_epic_local.sh EPIC-CCN-027

EPIC_ID="$1"

# Phase 5: Execute all tickets
for TICKET in TICKET-1 TICKET-2 TICKET-3; do
    echo "Executing $TICKET for $EPIC_ID..."
    # Call MCP tool via Claude API
    # Spawn new_task with instructions
    # Wait for completion
    # Verify output file
done

# Phase 5.V: Verification
echo "Running verification for $EPIC_ID..."
# Call phase-5-verify MCP
# Spawn new_task
# Verify output

# Phase 6: Final Review
echo "Running final review for $EPIC_ID..."
# Call phase-6-review MCP
# Spawn new_task
# Verify output

echo "Epic $EPIC_ID complete!"
```

## Gaps Identified

### 1. MCP Tools Need Executor Integration
**Current**: MCP tools return instructions only
**Needed**: Option to auto-spawn agent with instructions
**Proposal**: Add `execute=true` parameter to MCP tools

### 2. new_task Monitoring
**Current**: Manual monitoring of spawned task
**Needed**: Callback or polling mechanism
**Proposal**: Add `wait_for_completion` option to new_task

### 3. Cross-Session State
**Current**: Spawned task is independent (no shared state)
**Needed**: Way to pass context between sessions
**Proposal**: Use manifest.json as state bridge

## Next Steps

1. ✅ Document MCP orchestration workflow (THIS DOCUMENT)
2. ⏳ Complete EPIC-CCN-027 using documented workflow
3. ⏳ Create building-block script for local execution
4. ⏳ Test workflow with TICKET-2 and TICKET-3
5. ⏳ Update MCP tools with executor integration (Wave 5)

## Success Criteria for Building Block

- [ ] MCP orchestration workflow documented
- [ ] EPIC-CCN-027 completed using MCP tools
- [ ] Building-block script created and tested
- [ ] Lessons learned documented
- [ ] Gaps identified and proposals made
- [ ] Ready for Wave 5 MCP tool enhancements

---

**Status**: 🟡 IN PROGRESS - Workflow documented, execution pending
**Next**: Complete EPIC-CCN-027 TICKET-2 using new_task orchestration