#!/usr/bin/env python3
"""
Phase 5.V MCP Server (FastMCP) - Verification
Coordinator pattern: Prepares context for Bob IDE, returns instructions
"""

from fastmcp import FastMCP

mcp = FastMCP("Phase 5.V Verify Coordinator")


@mcp.tool()
def execute_phase_5_verify(epic_id: str) -> dict:
    """
    Execute Phase 5.V (Verification) for an epic.
    Verifies that ticket execution succeeded, complexity targets met,
    and all quality gates passed.
    
    Args:
        epic_id: Epic ID (e.g., EPIC-CCN-22)
    
    Returns:
        Context bundle with instructions for Bob IDE
    """
    
    # Prepare context bundle
    context = {
        "phase": "Phase 5.V: Verification",
        "epic_id": epic_id,
        "input_files": [
            f"docs/brain/{epic_id}/04-tickets.md",
            f"docs/brain/{epic_id}/manifest.json"
        ],
        "instructions": f"""
# Phase 5.V: Verification for {epic_id}

## Input Files
1. Read `docs/brain/{epic_id}/04-tickets.md` for ticket details
2. Read `docs/brain/{epic_id}/manifest.json` for epic metadata
3. Read all `ticket-X-completion.md` files

## Your Task
Verify that all tickets executed successfully and quality gates passed.

### Verification Checklist
1. **Ticket Completion**
   - All tickets marked as COMPLETED
   - No FAILED tickets
   - All acceptance criteria met

2. **Complexity Targets**
   - Run jCodemunch complexity analysis
   - Verify all methods ≤ 15 CYC
   - Compare before/after metrics

3. **Build Verification**
   - Run `build_readiness.ps1`
   - Confirm zero errors
   - Verify all tests pass

4. **Quality Gates**
   - Run `lint.ps1` (zero violations)
   - Run `pre_push_validation.ps1` (all checks pass)
   - Verify `deploy-sync.ps1` succeeded

5. **Behavioral Verification**
   - No logic changes (only extraction)
   - All tests still pass
   - F5 in NinjaTrader successful

### Output File
Create `docs/brain/{epic_id}/05-verification-report.md` with:

```markdown
# Verification Report: {epic_id}

## Ticket Completion Status
- **Total Tickets**: [Number]
- **Completed**: [Number]
- **Failed**: [Number]

## Complexity Verification
- **Target**: All methods ≤ 15 CYC
- **Achieved**: YES/NO
- **Before**: [CYC scores]
- **After**: [CYC scores]

## Build Verification
- **Build Status**: PASS/FAIL
- **Test Status**: PASS/FAIL
- **Lint Status**: PASS/FAIL

## Quality Gates
- **Pre-Push Validation**: PASS/FAIL
- **Deploy Sync**: PASS/FAIL
- **F5 Verification**: PASS/FAIL

## Behavioral Verification
- **Logic Changes**: NONE/DETECTED
- **Test Regression**: NONE/DETECTED
- **NinjaTrader**: WORKING/BROKEN

## Overall Status
- **PASS**: All verification checks passed
- **FAIL**: Issues detected (see below)

## Issues (if FAIL)
1. [Issue 1]
2. [Issue 2]

## Recommendations
- [Recommendation 1]
- [Recommendation 2]
```

### Update Manifest
Update `docs/brain/{epic_id}/manifest.json`:
```json
{{
  "phases": {{
    "phase_5_verify": {{
      "status": "completed",
      "output": "05-verification-report.md",
      "verification_result": "PASS/FAIL"
    }}
  }}
}}
```

## Success Criteria
- ✅ All tickets verified
- ✅ Complexity targets met
- ✅ Build passes
- ✅ Quality gates passed
- ✅ Behavioral verification passed
- ✅ Manifest updated
""",
        "output_files": [
            f"docs/brain/{epic_id}/05-verification-report.md"
        ]
    }
    
    return {
        "status": "success",
        "message": f"Phase 5.V context prepared for {epic_id}",
        "context": context
    }


if __name__ == "__main__":
    mcp.run()

# Made with Bob
