#!/usr/bin/env python3
"""
Phase 6 MCP Server (FastMCP) - Final Review
Coordinator pattern: Prepares context for Bob IDE, returns instructions
"""

from fastmcp import FastMCP

mcp = FastMCP("Phase 6 Review Coordinator")


@mcp.tool()
def execute_phase_6(epic_id: str) -> dict:
    """
    Execute Phase 6 (Final Review) for an epic.
    Performs final review, generates completion report,
    and updates roadmap with final status.
    
    Args:
        epic_id: Epic ID (e.g., EPIC-CCN-22)
    
    Returns:
        Context bundle with instructions for Bob IDE
    """
    
    # Prepare context bundle
    context = {
        "phase": "Phase 6: Final Review",
        "epic_id": epic_id,
        "input_files": [
            f"docs/brain/{epic_id}/05-verification-report.md",
            f"docs/brain/{epic_id}/manifest.json"
        ],
        "instructions": f"""
# Phase 6: Final Review for {epic_id}

## Input Files
1. Read `docs/brain/{epic_id}/05-verification-report.md` for verification results
2. Read `docs/brain/{epic_id}/manifest.json` for epic metadata
3. Review all phase outputs (00-hotspots.md through 05-verification-report.md)

## Your Task
Perform final review and generate completion report.

### Final Review Checklist
1. **Epic Completion**
   - All phases completed successfully
   - All tickets executed and verified
   - No outstanding blockers

2. **Quality Metrics**
   - Complexity reduced to ≤ 15 CYC
   - Build passes
   - Tests pass
   - Lint clean

3. **Documentation**
   - All phase outputs present
   - Manifest complete
   - Lessons learned captured

4. **Roadmap Update**
   - Update `epic_roadmap.json`
   - Mark epic as COMPLETED
   - Record completion date

### Output File
Create `docs/brain/{epic_id}/06-completion-report.md` with:

```markdown
# Epic Completion Report: {epic_id}

## Executive Summary
- **Epic**: {epic_id}
- **Status**: COMPLETED/FAILED
- **Duration**: [Time]
- **Complexity Reduction**: [Before CYC] → [After CYC]

## Phase Summary
- **Phase 0**: Hotspot Analysis - COMPLETED
- **Phase 1**: Scope Definition - COMPLETED
- **Phase 1.5**: Boundary Validation - COMPLETED
- **Phase 2**: Architecture Planning - COMPLETED
- **Phase 3**: DNA & PR Audit - COMPLETED
- **Phase 4**: Ticket Generation - COMPLETED
- **Phase 5**: Ticket Execution - COMPLETED
- **Phase 5.V**: Verification - COMPLETED
- **Phase 6**: Final Review - COMPLETED

## Quality Metrics
- **Complexity**: [Before] → [After] (Target: ≤15)
- **Build**: PASS
- **Tests**: PASS
- **Lint**: PASS

## Files Modified
- [File 1]: [Description]
- [File 2]: [Description]

## Lessons Learned
- [Lesson 1]
- [Lesson 2]

## Recommendations for Future Epics
- [Recommendation 1]
- [Recommendation 2]

## Next Steps
- Epic marked as COMPLETED in roadmap
- Ready for next epic in queue
```

### Update Manifest
Update `docs/brain/{epic_id}/manifest.json`:
```json
{{
  "phases": {{
    "phase_6": {{
      "status": "completed",
      "output": "06-completion-report.md"
    }}
  }},
  "epic_status": "COMPLETED",
  "completion_date": "[ISO 8601 timestamp]"
}}
```

### Update Roadmap
Update `epic_roadmap.json`:
```json
{{
  "epics": [
    {{
      "id": "{epic_id}",
      "status": "COMPLETED",
      "completion_date": "[ISO 8601 timestamp]",
      "complexity_before": [Number],
      "complexity_after": [Number]
    }}
  ]
}}
```

## Success Criteria
- ✅ Completion report created
- ✅ All phases verified
- ✅ Manifest finalized
- ✅ Roadmap updated
- ✅ Epic marked as COMPLETED
""",
        "output_files": [
            f"docs/brain/{epic_id}/06-completion-report.md"
        ]
    }
    
    return {
        "status": "success",
        "message": f"Phase 6 context prepared for {epic_id}",
        "context": context
    }


if __name__ == "__main__":
    mcp.run()

# Made with Bob
