#!/usr/bin/env python3
"""
Phase 5 MCP Server (FastMCP) - Ticket Execution
Coordinator pattern: Prepares context for Bob IDE, returns instructions
"""

from fastmcp import FastMCP

mcp = FastMCP("Phase 5 Execute Coordinator")


@mcp.tool()
def execute_phase_5(epic_id: str, ticket_id: str = "") -> dict:
    """
    Execute Phase 5 (Ticket Execution) for an epic.
    Delegates to Bob CLI (v12-engineer) for surgical extraction.
    This tool orchestrates execution but Bob does the actual code work.
    
    Args:
        epic_id: Epic ID (e.g., EPIC-CCN-22)
        ticket_id: Specific ticket to execute (e.g., TICKET-1).
                   If omitted, executes all tickets sequentially.
    
    Returns:
        Context bundle with instructions for Bob IDE
    """
    
    # Prepare context bundle
    context = {
        "phase": "Phase 5: Ticket Execution",
        "epic_id": epic_id,
        "ticket_id": ticket_id or "ALL",
        "input_files": [
            f"docs/brain/{epic_id}/04-tickets.md",
            f"docs/brain/{epic_id}/manifest.json"
        ],
        "instructions": f"""
# Phase 5: Ticket Execution for {epic_id}

## Input Files
1. Read `docs/brain/{epic_id}/04-tickets.md` for ticket details
2. Read `docs/brain/{epic_id}/manifest.json` for epic metadata

## Your Task
Execute ticket(s) using Bob CLI (v12-engineer mode).

### Execution Strategy
{"**Single Ticket Mode**: Execute " + ticket_id if ticket_id else "**Sequential Mode**: Execute all tickets in order"}

### Bob CLI Invocation
For each ticket:
1. Switch to Bob CLI (`v12-engineer` mode)
2. Provide ticket context from `04-tickets.md`
3. Let Bob perform surgical extraction
4. Verify build passes
5. Document completion

### Ticket Execution Checklist
- [ ] Read ticket scope and acceptance criteria
- [ ] Invoke Bob CLI with ticket context
- [ ] Monitor Bob's extraction work
- [ ] Verify all acceptance criteria met
- [ ] Run `deploy-sync.ps1` to sync NinjaTrader
- [ ] Confirm build passes
- [ ] Document any issues or deviations

### Output File
Create `docs/brain/{epic_id}/ticket-{ticket_id or "X"}-completion.md` with:

```markdown
# Ticket Completion: {epic_id} - {ticket_id or "TICKET-X"}

## Execution Summary
- **Ticket**: {ticket_id or "TICKET-X"}
- **Status**: COMPLETED/FAILED
- **Duration**: [Time]
- **Bob CLI Session**: [Session ID]

## Changes Made
- [File 1]: [Description]
- [File 2]: [Description]

## Acceptance Criteria
- [x] Method complexity reduced to ≤ 15
- [x] All tests pass
- [x] No behavioral changes
- [x] Build succeeds

## Verification
- **Build Status**: PASS/FAIL
- **Test Status**: PASS/FAIL
- **Complexity**: [Final CYC]

## Issues Encountered
[None or list issues]

## Next Steps
{"Proceed to TICKET-" + str(int(ticket_id.split("-")[1]) + 1) if ticket_id and ticket_id != "ALL" else "Proceed to Phase 5.V (Verification)"}
```

### Update Manifest
Update `docs/brain/{epic_id}/manifest.json`:
```json
{{
  "phases": {{
    "phase_5": {{
      "status": "in_progress",
      "tickets_completed": ["{ticket_id or "TICKET-X"}"],
      "tickets_remaining": [...]
    }}
  }}
}}
```

## Success Criteria
- ✅ Ticket(s) executed via Bob CLI
- ✅ All acceptance criteria met
- ✅ Build passes
- ✅ Completion documented
- ✅ Manifest updated
""",
        "output_files": [
            f"docs/brain/{epic_id}/ticket-{ticket_id or 'X'}-completion.md"
        ]
    }
    
    return {
        "status": "success",
        "message": f"Phase 5 context prepared for {epic_id}" + (f" - {ticket_id}" if ticket_id else " - ALL tickets"),
        "context": context
    }


if __name__ == "__main__":
    mcp.run()

# Made with Bob
