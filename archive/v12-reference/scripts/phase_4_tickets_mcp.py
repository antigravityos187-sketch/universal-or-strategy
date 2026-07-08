#!/usr/bin/env python3
"""
Phase 4 MCP Server (FastMCP) - Ticket Generation
Coordinator pattern: Prepares context for Bob IDE, returns instructions
"""

from fastmcp import FastMCP

mcp = FastMCP("Phase 4 Tickets Coordinator")


@mcp.tool()
def execute_phase_4(epic_id: str) -> dict:
    """
    Execute Phase 4 (Ticket Generation) for an epic.
    Uses jCodemunch to analyze method complexity and generate
    surgical extraction tickets.
    
    Args:
        epic_id: Epic ID (e.g., EPIC-CCN-22)
    
    Returns:
        Context bundle with instructions for Bob IDE
    """
    
    # Prepare context bundle
    context = {
        "phase": "Phase 4: Ticket Generation",
        "epic_id": epic_id,
        "input_files": [
            f"docs/brain/{epic_id}/03-audit-report.md",
            f"docs/brain/{epic_id}/02-architecture-plan.md",
            f"docs/brain/{epic_id}/manifest.json"
        ],
        "instructions": f"""
# Phase 4: Ticket Generation for {epic_id}

## Input Files
1. Read `docs/brain/{epic_id}/03-audit-report.md` for audit results
2. Read `docs/brain/{epic_id}/02-architecture-plan.md` for architecture plan
3. Read `docs/brain/{epic_id}/manifest.json` for epic metadata

## Your Task
Generate surgical extraction tickets based on complexity analysis.

### Ticket Generation Strategy
1. **Analyze Method Complexity**
   - Use jCodemunch to get detailed complexity metrics
   - Identify logical sub-units within method
   - Determine extraction boundaries

2. **Create Extraction Tickets**
   - One ticket per logical sub-unit
   - Each ticket targets CYC ≤ 15
   - Clear acceptance criteria
   - Dependency ordering

3. **Ticket Structure**
   - **TICKET-1**: [Sub-unit name]
     - Current CYC: [Number]
     - Target CYC: ≤ 15
     - Extraction scope: [Description]
     - Dependencies: [List]
     - Acceptance criteria: [Checklist]

### Output File
Create `docs/brain/{epic_id}/04-tickets.md` with:

```markdown
# Extraction Tickets: {epic_id}

## Overview
- **Total Tickets**: [Number]
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → ...)
- **Estimated Effort**: [Hours]

## TICKET-1: [Sub-unit Name]

### Scope
- **Current Method**: `[MethodName]`
- **Current CYC**: [Number]
- **Target CYC**: ≤ 15
- **Extraction**: [Description]

### Implementation
1. [Step 1]
2. [Step 2]
3. [Step 3]

### Acceptance Criteria
- [ ] Method complexity reduced to ≤ 15
- [ ] All tests pass
- [ ] No behavioral changes
- [ ] Build succeeds

### Dependencies
- None (first ticket)

---

## TICKET-2: [Sub-unit Name]

[Same structure as TICKET-1]

### Dependencies
- TICKET-1 must be completed first
```

### Update Manifest
Update `docs/brain/{epic_id}/manifest.json`:
```json
{{
  "phases": {{
    "phase_4": {{
      "status": "completed",
      "output": "04-tickets.md",
      "ticket_count": [Number]
    }}
  }}
}}
```

## Success Criteria
- ✅ Tickets document created
- ✅ Each ticket has clear scope
- ✅ Acceptance criteria defined
- ✅ Dependencies documented
- ✅ Manifest updated
""",
        "output_files": [
            f"docs/brain/{epic_id}/04-tickets.md"
        ]
    }
    
    return {
        "status": "success",
        "message": f"Phase 4 context prepared for {epic_id}",
        "context": context
    }


if __name__ == "__main__":
    mcp.run()

# Made with Bob
