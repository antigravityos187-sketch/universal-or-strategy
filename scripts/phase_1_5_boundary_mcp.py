#!/usr/bin/env python3
"""
Phase 1.5 MCP Server (FastMCP) - Scope Boundary Validation
Coordinator pattern: Prepares context for Bob IDE, returns instructions
"""

from fastmcp import FastMCP

mcp = FastMCP("Phase 1.5 Boundary Coordinator")


@mcp.tool()
def execute_phase_1_5(epic_id: str) -> dict:
    """
    Execute Phase 1.5 (Scope Boundary Validation) for an epic.
    Validates that planned extraction stays within single-method boundary.
    MANDATORY gate to prevent scope creep (V12.23 Protocol).
    
    Args:
        epic_id: Epic ID (e.g., EPIC-CCN-22)
    
    Returns:
        Context bundle with instructions for Bob IDE
    """
    
    # Prepare context bundle
    context = {
        "phase": "Phase 1.5: Scope Boundary Validation",
        "epic_id": epic_id,
        "input_files": [
            f"docs/brain/{epic_id}/00-scope.md",
            f"docs/brain/{epic_id}/manifest.json"
        ],
        "instructions": f"""
# Phase 1.5: Scope Boundary Validation for {epic_id}

## Input Files
1. Read `docs/brain/{epic_id}/00-scope.md` for scope definition
2. Read `docs/brain/{epic_id}/manifest.json` for epic metadata

## Your Task
Validate that the planned extraction stays within single-method boundary.

### Validation Checklist
1. **Single Method Rule**: Confirm scope targets exactly ONE method
   - ❌ FAIL: Multiple methods, cross-file changes, architectural refactoring
   - ✅ PASS: Single method extraction only

2. **Dependency Boundary**: Verify dependencies are minimal
   - ❌ FAIL: Requires changes to >3 other files
   - ✅ PASS: Self-contained or minimal dependencies

3. **Complexity Target**: Confirm target CYC ≤ 15
   - ❌ FAIL: Target complexity >15
   - ✅ PASS: Target complexity ≤15

4. **Risk Assessment**: Validate risk level is appropriate
   - ❌ FAIL: HIGH risk without mitigation plan
   - ✅ PASS: LOW/MEDIUM risk or HIGH with mitigation

### Decision Matrix
- **ALL PASS**: Proceed to Phase 2 (Architecture Planning)
- **ANY FAIL**: STOP - Scope creep detected
  - Document failure reason
  - Mark epic as BLOCKED
  - Require Director approval to proceed

### Output File
Create `docs/brain/{epic_id}/01-scope-boundary.md` with:

```markdown
# Scope Boundary Validation: {epic_id}

## Validation Results

### Single Method Rule
- **Status**: PASS/FAIL
- **Details**: [Explanation]

### Dependency Boundary
- **Status**: PASS/FAIL
- **Dependencies**: [List]

### Complexity Target
- **Status**: PASS/FAIL
- **Target CYC**: [Number]

### Risk Assessment
- **Status**: PASS/FAIL
- **Risk Level**: [LOW/MEDIUM/HIGH]

## Overall Decision
- **PASS**: Proceed to Phase 2
- **FAIL**: BLOCKED - Scope creep detected

## Failure Reason (if FAIL)
[Detailed explanation of why scope exceeds single-method boundary]

## Mitigation Plan (if HIGH risk)
[Steps to reduce risk before proceeding]
```

### Update Manifest
Update `docs/brain/{epic_id}/manifest.json`:
```json
{{
  "phases": {{
    "phase_1_5": {{
      "status": "completed",
      "output": "01-scope-boundary.md",
      "validation_result": "PASS/FAIL"
    }}
  }}
}}
```

## Success Criteria
- ✅ Boundary validation document created
- ✅ Manifest updated with Phase 1.5 status
- ✅ PASS/FAIL decision documented
- ✅ If FAIL: Epic marked as BLOCKED
""",
        "output_files": [
            f"docs/brain/{epic_id}/01-scope-boundary.md"
        ]
    }
    
    return {
        "status": "success",
        "message": f"Phase 1.5 context prepared for {epic_id}",
        "context": context
    }


if __name__ == "__main__":
    mcp.run()

# Made with Bob
