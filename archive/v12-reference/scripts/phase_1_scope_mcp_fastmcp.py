#!/usr/bin/env python3
"""
Phase 1 MCP Server (FastMCP) - Scope Definition
Coordinator pattern: Prepares context for Bob IDE, returns instructions
"""

from fastmcp import FastMCP
import json

mcp = FastMCP("Phase 1 Scope Coordinator")


@mcp.tool()
def execute_phase_1(epic_id: str) -> dict:
    """
    Execute Phase 1 (Scope Definition) for an epic.
    Reads Phase 0 hotspot analysis and creates scope definition.
    For no-action epics, documents closure rationale.
    
    Args:
        epic_id: Epic ID (e.g., EPIC-CCN-21)
    
    Returns:
        Context bundle with instructions for Bob IDE
    """
    
    # Prepare context bundle
    context = {
        "phase": "Phase 1: Scope Definition",
        "epic_id": epic_id,
        "input_files": [
            f"docs/brain/{epic_id}/00-hotspots.md",
            f"docs/brain/{epic_id}/manifest.json"
        ],
        "instructions": f"""
# Phase 1: Scope Definition for {epic_id}

## Input Files
1. Read `docs/brain/{epic_id}/00-hotspots.md` for hotspot analysis
2. Read `docs/brain/{epic_id}/manifest.json` for epic metadata

## Your Task
Based on the hotspot analysis, create a scope definition document.

### Decision Tree
1. **If complexity ≤ 15**: Mark as NO-ACTION epic
   - Document: "Method already meets Jane Street threshold (CYC ≤ 15)"
   - Update manifest: `"action_required": false`
   - Create closure rationale in `00-scope.md`

2. **If complexity > 15**: Define extraction scope
   - Identify extraction boundaries (single method only)
   - List dependencies (imports, state access)
   - Define success criteria (target CYC ≤ 15)
   - Estimate risk level (LOW/MEDIUM/HIGH)

### Output File
Create `docs/brain/{epic_id}/00-scope.md` with:

```markdown
# Scope Definition: {epic_id}

## Method Target
- **Method**: [method name]
- **File**: [file path]
- **Current Complexity**: [CYC score]
- **Target Complexity**: ≤ 15

## Action Required
[YES/NO]

## Scope Boundary
[If YES: Define extraction scope]
[If NO: Document closure rationale]

## Dependencies
- [List imports, state access, etc.]

## Success Criteria
- [ ] Method complexity reduced to ≤ 15
- [ ] All tests pass
- [ ] No behavioral changes

## Risk Assessment
[LOW/MEDIUM/HIGH]
```

### Update Manifest
Update `docs/brain/{epic_id}/manifest.json`:
```json
{{
  "phases": {{
    "phase_1": {{
      "status": "completed",
      "output": "00-scope.md",
      "action_required": true/false
    }}
  }}
}}
```

## Success Criteria
- ✅ Scope document created
- ✅ Manifest updated with Phase 1 status
- ✅ Action decision documented (YES/NO)
""",
        "output_files": [
            f"docs/brain/{epic_id}/00-scope.md"
        ]
    }
    
    return {
        "status": "success",
        "message": f"Phase 1 context prepared for {epic_id}",
        "context": context
    }


if __name__ == "__main__":
    mcp.run()
