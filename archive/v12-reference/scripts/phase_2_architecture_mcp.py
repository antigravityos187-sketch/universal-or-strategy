#!/usr/bin/env python3
"""
Phase 2 MCP Server (FastMCP) - Architecture Planning
Coordinator pattern: Prepares context for Bob IDE, returns instructions
"""

from fastmcp import FastMCP

mcp = FastMCP("Phase 2 Architecture Coordinator")


@mcp.tool()
def execute_phase_2(epic_id: str) -> dict:
    """
    Execute Phase 2 (Architecture Planning) for an epic.
    Creates detailed extraction plan with method signatures, call graphs,
    and Jane Street compliance checks.
    
    Args:
        epic_id: Epic ID (e.g., EPIC-CCN-22)
    
    Returns:
        Context bundle with instructions for Bob IDE
    """
    
    # Prepare context bundle
    context = {
        "phase": "Phase 2: Architecture Planning",
        "epic_id": epic_id,
        "input_files": [
            f"docs/brain/{epic_id}/01-scope-boundary.md",
            f"docs/brain/{epic_id}/00-scope.md",
            f"docs/brain/{epic_id}/manifest.json"
        ],
        "instructions": f"""
# Phase 2: Architecture Planning for {epic_id}

## Input Files
1. Read `docs/brain/{epic_id}/01-scope-boundary.md` for validated scope
2. Read `docs/brain/{epic_id}/00-scope.md` for scope definition
3. Read `docs/brain/{epic_id}/manifest.json` for epic metadata

## Your Task
Create a detailed extraction plan with method signatures and call graphs.

### Architecture Design
1. **Method Signature Analysis**
   - Current method signature
   - Proposed extracted method signatures
   - Parameter types and return types
   - State dependencies

2. **Call Graph Mapping**
   - Identify all callers of target method
   - Map internal method calls
   - Document state access patterns
   - Identify side effects

3. **Extraction Strategy**
   - Define extraction boundaries
   - Plan parameter passing
   - Design state management
   - Minimize coupling

4. **Jane Street Compliance**
   - Verify "Make illegal states unrepresentable"
   - Check for lock-free patterns (no `lock()` blocks)
   - Validate Actor/FSM pattern usage
   - Confirm ASCII-only compliance

### Output File
Create `docs/brain/{epic_id}/02-architecture-plan.md` with:

```markdown
# Architecture Plan: {epic_id}

## Current Method Signature
```csharp
[Current method signature with full parameters]
```

## Proposed Extracted Methods
```csharp
// Method 1
[Signature]

// Method 2 (if needed)
[Signature]
```

## Call Graph
```
Callers:
- [File1.cs::Method1()]
- [File2.cs::Method2()]

Internal Calls:
- [Helper1()]
- [Helper2()]

State Access:
- [Field1]
- [Field2]
```

## Extraction Strategy
1. [Step 1]
2. [Step 2]
3. [Step 3]

## Jane Street Compliance
- [ ] No illegal states possible
- [ ] Lock-free (no `lock()` blocks)
- [ ] Actor/FSM pattern used
- [ ] ASCII-only strings

## Risk Mitigation
- [Risk 1]: [Mitigation]
- [Risk 2]: [Mitigation]
```

### Update Manifest
Update `docs/brain/{epic_id}/manifest.json`:
```json
{{
  "phases": {{
    "phase_2": {{
      "status": "completed",
      "output": "02-architecture-plan.md"
    }}
  }}
}}
```

## Success Criteria
- ✅ Architecture plan document created
- ✅ Method signatures defined
- ✅ Call graph mapped
- ✅ Jane Street compliance verified
- ✅ Manifest updated
""",
        "output_files": [
            f"docs/brain/{epic_id}/02-architecture-plan.md"
        ]
    }
    
    return {
        "status": "success",
        "message": f"Phase 2 context prepared for {epic_id}",
        "context": context
    }


if __name__ == "__main__":
    mcp.run()

# Made with Bob
