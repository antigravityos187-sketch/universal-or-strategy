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

## ⚠️ CRITICAL: 5-Check Protocol (V12.34)

**MANDATORY READING**: `docs/protocol/PHASE5V_VERIFICATION_PROTOCOL.md`

**ALL 5 CHECKS MUST PASS** before marking epic as complete.

## Input Files
1. Read `docs/brain/{epic_id}/04-tickets.md` for ticket details
2. Read `docs/brain/{epic_id}/manifest.json` for epic metadata
3. Read all `ticket-X-completion.md` files
4. **MANDATORY**: Read `docs/protocol/PHASE5V_VERIFICATION_PROTOCOL.md` for verification rules

## Your Task
Execute the 5 MANDATORY CHECKS and verify quality gates passed.

### The 5 Mandatory Checks

#### ✅ Check 1: Compilation
**Command**: `dotnet build`
**Success**: Exit code 0, zero errors, zero P0 warnings
**Failure**: Document errors, mark FAILED, trigger recovery

#### ✅ Check 2: Complexity Reduction
**Command**: `python scripts/complexity_audit.py`
**Success**: Target method CYC ≤8 (Jane Street strict)
**Failure**: Document actual CYC, mark FAILED if >8

#### ✅ Check 3: Scope Compliance
**Command**: `git diff HEAD~1 --stat`
**Success**: ONLY target file + target method modified
**Failure**: Document scope violations, mark FAILED (scope creep)

#### ✅ Check 4: Test Coverage
**Command**: `dotnet test`
**Success**: xUnit tests exist and pass (NEVER NUnit/MSTest)
**Failure**: Mark FAILED if tests fail or wrong framework

#### ✅ Check 5: Encoding Compliance
**Command**: `powershell -File .\scripts\check_encoding.ps1`
**Success**: Exit code 0, all files UTF-8 without BOM
**Failure**: Run with `-Fix` flag, re-verify, mark FAILED if still fails

### Output File
Create `docs/brain/{epic_id}/06-verification-report.md` with:

```markdown
# Phase 5.V Verification Report: {epic_id}

## Check Results

### ✅ Check 1: Compilation
- Command: `dotnet build`
- Exit Code: [0 or error code]
- Errors: [count]
- Warnings: [count]
- Status: PASS/FAIL

### ✅ Check 2: Complexity Reduction
- Target Method: [MethodName]
- Before CYC: [number]
- After CYC: [number]
- Target: ≤8
- Status: PASS/FAIL ([percentage] under/over target)

### ✅ Check 3: Scope Compliance
- Files Modified: [count] (target file only?)
- Methods Modified: [count] (target + extracted only?)
- Adjacent Changes: [count] (MUST be 0)
- Status: PASS/FAIL

### ✅ Check 4: Test Coverage
- Framework: xUnit/NUnit/MSTest (MUST be xUnit)
- Tests Generated: [count]
- Tests Passing: [count]
- Status: PASS/FAIL

### ✅ Check 5: Encoding Compliance
- Files Checked: [count]
- UTF-8 without BOM: [count]
- UTF-16 Violations: [count] (MUST be 0)
- Status: PASS/FAIL

## Overall Result
- **Status**: ✅ ALL CHECKS PASS / ❌ FAILED
- **Epic Status**: COMPLETE / FAILED
- **Ready for PR**: YES / NO

## Issues (if any check FAILED)
1. [Issue 1 with check number]
2. [Issue 2 with check number]

## Recovery Actions (if FAILED)
1. [Action 1]
2. [Action 2]
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

## Success Criteria (5-Check Protocol)
- ✅ Check 1: Compilation PASS
- ✅ Check 2: Complexity ≤8 PASS
- ✅ Check 3: Scope compliance PASS (no adjacent changes)
- ✅ Check 4: xUnit tests PASS
- ✅ Check 5: UTF-8 encoding PASS
- ✅ ALL 5 CHECKS PASS = Epic COMPLETE
- ❌ ANY CHECK FAILS = Trigger recovery loop

## Protocol References
- **Verification Rules**: `docs/protocol/PHASE5V_VERIFICATION_PROTOCOL.md` (V12.34)
- **Execution Rules**: `docs/protocol/PHASE5_EXECUTION_PROTOCOL.md` (V12.34)
- **Recovery Protocol**: `docs/protocol/RECOVERY_LOOP_PROTOCOL.md` (V12.26)
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
