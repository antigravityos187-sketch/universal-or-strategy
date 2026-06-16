#!/usr/bin/env python3
"""
Phase 0 MCP Server (FastMCP Implementation)
Coordinates hotspot analysis for V12 epic workflow.
WITH Jane Street validation integration.
"""

from fastmcp import FastMCP
import json
from pathlib import Path
from typing import Dict, Any
import sys

# Add scripts directory to path for jane_street_utils
sys.path.insert(0, str(Path(__file__).parent))
from jane_street_utils import load_violations_in_range, format_violation_report

# Initialize FastMCP server
mcp = FastMCP("Phase 0 Hotspot Coordinator")

@mcp.tool()
def execute_phase_0(
    epic_id: str,
    method: str,
    file: str,
    cyc: int,
    jcodemunch_data: Dict[str, Any]
) -> Dict[str, Any]:
    """
    Execute Phase 0 (Hotspot Analysis) for an epic.
    
    Args:
        epic_id: Epic ID (e.g., EPIC-CCN-26)
        method: Method name from jCodemunch hotspot
        file: File path containing the method
        cyc: Cyclomatic complexity score
        jcodemunch_data: Pre-fetched jCodemunch context (hotspots, blast_radius, etc.)
    
    Returns:
        Dict with context for Bob IDE to execute Phase 0
    """
    
    # Load Jane Street violations for the target file
    violations = []
    violation_report = ""
    try:
        # Get method line range from jcodemunch_data if available
        start_line = jcodemunch_data.get('start_line', 0)
        end_line = jcodemunch_data.get('end_line', 999999)
        
        if start_line > 0:
            violations = load_violations_in_range(file, start_line, end_line)
        else:
            # Fallback: load all violations for file
            from jane_street_utils import load_violations_for_file
            violations = load_violations_for_file(file)
        
        violation_report = format_violation_report(violations, f"Jane Street Violations in {method}")
    except Exception as e:
        violation_report = f"⚠️ Could not load Jane Street violations: {e}"
    
    # Prepare context bundle for Bob IDE
    context = {
        "phase": "Phase 0: Hotspot Analysis",
        "epic_id": epic_id,
        "method": method,
        "file": file,
        "complexity": cyc,
        "jcodemunch_context": jcodemunch_data,
        "jane_street_violations": len(violations),
        "instructions": f"""
# Phase 0: Hotspot Analysis for {epic_id}

## Target Method
- **Method**: `{method}`
- **File**: `{file}`
- **Complexity**: {cyc}
- **Jane Street Violations**: {len(violations)}

## jCodemunch Context
{json.dumps(jcodemunch_data, indent=2)}

{violation_report}

## Your Task
1. Analyze the hotspot data above
2. **IMPORTANT**: Note the {len(violations)} Jane Street violations that must be fixed during refactoring
3. Create `docs/brain/{epic_id}/00-hotspots.md` with:
   - Method signature and location
   - Complexity metrics
   - Blast radius analysis
   - **Jane Street violations count and summary**
   - Risk assessment (LOW/MEDIUM/HIGH)
   - **Risk level should be HIGH if violations >5**
4. Create `docs/brain/{epic_id}/manifest.json` with:
   ```json
   {{
     "epic_id": "{epic_id}",
     "method": "{method}",
     "file": "{file}",
     "complexity": {cyc},
     "jane_street_violations": {len(violations)},
     "phases": {{
       "phase_0": {{
         "status": "completed",
         "output": "00-hotspots.md"
       }}
     }}
   }}
   ```

## Success Criteria
- ✅ Hotspots document created
- ✅ Manifest initialized
- ✅ Risk level assigned
- ✅ **Jane Street violations documented**
""",
        "output_files": [
            f"docs/brain/{epic_id}/00-hotspots.md",
            f"docs/brain/{epic_id}/manifest.json"
        ]
    }
    
    return {
        "status": "success",
        "message": f"Phase 0 context prepared for {epic_id}",
        "context": context
    }

if __name__ == "__main__":
    # Run the FastMCP server
    mcp.run()
