#!/usr/bin/env python3
"""
Phase 0 MCP Server (FastMCP Implementation)
Coordinates hotspot analysis for V12 epic workflow.
"""

from fastmcp import FastMCP
import json
from pathlib import Path
from typing import Dict, Any

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
    
    # Prepare context bundle for Bob IDE
    context = {
        "phase": "Phase 0: Hotspot Analysis",
        "epic_id": epic_id,
        "method": method,
        "file": file,
        "complexity": cyc,
        "jcodemunch_context": jcodemunch_data,
        "instructions": f"""
# Phase 0: Hotspot Analysis for {epic_id}

## Target Method
- **Method**: `{method}`
- **File**: `{file}`
- **Complexity**: {cyc}

## jCodemunch Context
{json.dumps(jcodemunch_data, indent=2)}

## Your Task
1. Analyze the hotspot data above
2. Create `docs/brain/{epic_id}/00-hotspots.md` with:
   - Method signature and location
   - Complexity metrics
   - Blast radius analysis
   - Risk assessment (LOW/MEDIUM/HIGH)
3. Create `docs/brain/{epic_id}/manifest.json` with:
   ```json
   {{
     "epic_id": "{epic_id}",
     "method": "{method}",
     "file": "{file}",
     "complexity": {cyc},
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
