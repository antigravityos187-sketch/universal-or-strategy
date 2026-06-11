#!/usr/bin/env python3
"""
Phase 3 MCP Server (FastMCP) - DNA & PR Audit
Coordinator pattern: Prepares context for Bob IDE, returns instructions
"""

from fastmcp import FastMCP

mcp = FastMCP("Phase 3 Audit Coordinator")


@mcp.tool()
def execute_phase_3(epic_id: str) -> dict:
    """
    Execute Phase 3 (DNA & PR Audit) for an epic.
    Runs V12 DNA compliance checks, PR hygiene validation,
    and pre-flight safety checks.
    
    Args:
        epic_id: Epic ID (e.g., EPIC-CCN-22)
    
    Returns:
        Context bundle with instructions for Bob IDE
    """
    
    # Prepare context bundle
    context = {
        "phase": "Phase 3: DNA & PR Audit",
        "epic_id": epic_id,
        "input_files": [
            f"docs/brain/{epic_id}/02-architecture-plan.md",
            f"docs/brain/{epic_id}/manifest.json"
        ],
        "instructions": f"""
# Phase 3: DNA & PR Audit for {epic_id}

## Input Files
1. Read `docs/brain/{epic_id}/02-architecture-plan.md` for architecture plan
2. Read `docs/brain/{epic_id}/manifest.json` for epic metadata

## Your Task
Run V12 DNA compliance checks and PR hygiene validation.

### DNA Compliance Checks
1. **Correctness by Construction**
   - Verify illegal states are unrepresentable
   - Check type safety
   - Validate state machine design

2. **Lock-Free Actor Pattern**
   - Confirm no `lock(stateLock)` blocks
   - Verify FSM/Actor Enqueue model
   - Check atomic primitives usage

3. **ASCII-Only Compliance**
   - Scan for Unicode characters
   - Check for emoji or curly quotes
   - Validate string literals

4. **Jane Street Alignment**
   - Review against Jane Street Intel
   - Check cognitive simplicity
   - Validate microsecond-latency patterns

### PR Hygiene Validation
1. **Diff Size Check**
   - Estimate total diff size
   - Target: <10,000 characters
   - Flag if exceeds limit

2. **Scope Creep Check**
   - Verify single-method focus
   - Check for unrelated changes
   - Validate no whitespace mutations

3. **Build Readiness**
   - Confirm compilation will succeed
   - Check for breaking changes
   - Validate test coverage

### Output File
Create `docs/brain/{epic_id}/03-audit-report.md` with:

```markdown
# DNA & PR Audit Report: {epic_id}

## DNA Compliance

### Correctness by Construction
- **Status**: PASS/FAIL
- **Details**: [Explanation]

### Lock-Free Actor Pattern
- **Status**: PASS/FAIL
- **Lock Count**: [Number of lock() blocks found]

### ASCII-Only Compliance
- **Status**: PASS/FAIL
- **Unicode Count**: [Number of non-ASCII characters]

### Jane Street Alignment
- **Status**: PASS/FAIL
- **Cognitive Complexity**: [Assessment]

## PR Hygiene

### Diff Size
- **Estimated Size**: [Characters]
- **Status**: PASS/FAIL (target <10k)

### Scope Creep
- **Status**: PASS/FAIL
- **Single Method**: YES/NO

### Build Readiness
- **Status**: PASS/FAIL
- **Breaking Changes**: [List]

## Overall Assessment
- **PASS**: Ready for Phase 4 (Ticket Generation)
- **FAIL**: Blockers identified - requires fixes

## Blockers (if FAIL)
1. [Blocker 1]
2. [Blocker 2]

## Recommendations
- [Recommendation 1]
- [Recommendation 2]
```

### Update Manifest
Update `docs/brain/{epic_id}/manifest.json`:
```json
{{
  "phases": {{
    "phase_3": {{
      "status": "completed",
      "output": "03-audit-report.md",
      "audit_result": "PASS/FAIL"
    }}
  }}
}}
```

## Success Criteria
- ✅ Audit report created
- ✅ DNA compliance verified
- ✅ PR hygiene validated
- ✅ Manifest updated
- ✅ If FAIL: Blockers documented
""",
        "output_files": [
            f"docs/brain/{epic_id}/03-audit-report.md"
        ]
    }
    
    return {
        "status": "success",
        "message": f"Phase 3 context prepared for {epic_id}",
        "context": context
    }


if __name__ == "__main__":
    mcp.run()

# Made with Bob
