#!/usr/bin/env python3
"""
Phase 1 MCP Server: Scope Definition
Reads Phase 0 hotspot analysis and creates scope definition document.
"""

from __future__ import annotations

import asyncio
import json
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from mcp.server import Server
from mcp.types import Tool, TextContent

# Add project root to path
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

app = Server("phase-1-scope")


@app.list_tools()
async def list_tools() -> list[Tool]:
    """List available Phase 1 tools."""
    return [
        Tool(
            name="execute_phase_1",
            description=(
                "Execute Phase 1 (Scope Definition) for an epic. "
                "Reads Phase 0 hotspot analysis and creates scope definition. "
                "For no-action epics, documents closure rationale."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "epic_id": {
                        "type": "string",
                        "description": "Epic ID (e.g., EPIC-CCN-21)",
                    },
                },
                "required": ["epic_id"],
            },
        )
    ]


@app.call_tool()
async def call_tool(name: str, arguments: Any) -> list[TextContent]:
    """Handle tool calls."""
    if name == "execute_phase_1":
        return await execute_phase_1_tool(arguments)
    raise ValueError(f"Unknown tool: {name}")


async def execute_phase_1_tool(arguments: dict) -> list[TextContent]:
    """Execute Phase 1: Scope Definition."""
    epic_id = arguments["epic_id"]
    
    try:
        # Load manifest
        manifest_path = project_root / "docs" / "brain" / epic_id / "manifest.json"
        if not manifest_path.exists():
            return [
                TextContent(
                    type="text",
                    text=json.dumps({
                        "success": False,
                        "error": f"Manifest not found: {manifest_path}",
                        "epic_id": epic_id,
                    }, indent=2)
                )
            ]
        
        with open(manifest_path) as f:
            manifest = json.load(f)
        
        # Verify Phase 0 completed
        if manifest["phases"]["0"]["status"] != "completed":
            return [
                TextContent(
                    type="text",
                    text=json.dumps({
                        "success": False,
                        "error": "Phase 0 not completed",
                        "epic_id": epic_id,
                        "phase_0_status": manifest["phases"]["0"]["status"],
                    }, indent=2)
                )
            ]
        
        # Read Phase 0 hotspot analysis
        hotspot_path = project_root / "docs" / "brain" / epic_id / "00-hotspots.md"
        if not hotspot_path.exists():
            return [
                TextContent(
                    type="text",
                    text=json.dumps({
                        "success": False,
                        "error": f"Hotspot analysis not found: {hotspot_path}",
                        "epic_id": epic_id,
                    }, indent=2)
                )
            ]
        
        with open(hotspot_path) as f:
            hotspot_content = f.read()
        
        # Determine scope based on Phase 0 recommendation
        recommendation = manifest.get("recommendation", "unknown")
        
        if recommendation == "no_action_required":
            scope_content = create_no_action_scope(manifest, hotspot_content)
        else:
            scope_content = create_extraction_scope(manifest, hotspot_content)
        
        # Write scope definition
        scope_path = project_root / "docs" / "brain" / epic_id / "00-scope.md"
        with open(scope_path, "w") as f:
            f.write(scope_content)
        
        # Update manifest
        manifest["phases"]["1"]["status"] = "completed"
        manifest["phases"]["1"]["started"] = datetime.now(timezone.utc).isoformat()
        manifest["phases"]["1"]["completed"] = datetime.now(timezone.utc).isoformat()
        manifest["phases"]["1"]["output_artifacts"] = [str(scope_path.relative_to(project_root))]
        manifest["last_updated"] = datetime.now(timezone.utc).isoformat()
        
        if recommendation == "no_action_required":
            manifest["status"] = "completed"
            manifest["phases"]["1"]["notes"] = "Epic closed - no action required"
        else:
            manifest["status"] = "phase_1_complete"
            manifest["phases"]["1"]["notes"] = "Scope defined - proceeding to Phase 1.5"
        
        with open(manifest_path, "w") as f:
            json.dump(manifest, f, indent=2)
        
        return [
            TextContent(
                type="text",
                text=json.dumps({
                    "success": True,
                    "epic_id": epic_id,
                    "phase": "1",
                    "status": "completed",
                    "recommendation": recommendation,
                    "output_file": str(scope_path.relative_to(project_root)),
                    "next_phase": None if recommendation == "no_action_required" else "1.5",
                }, indent=2)
            )
        ]
    
    except Exception as e:
        return [
            TextContent(
                type="text",
                text=json.dumps({
                    "success": False,
                    "error": str(e),
                    "epic_id": epic_id,
                    "phase": "1",
                }, indent=2)
            )
        ]


def create_no_action_scope(manifest: dict, hotspot_content: str) -> str:
    """Create scope document for no-action epic."""
    epic_id = manifest["epic_id"]
    method = manifest["method"]
    file = manifest["file"]
    current_cyc = manifest["current_cyc"]
    target_cyc = manifest["target_cyc"]
    
    return f"""# {epic_id} - Scope Definition

## Epic Closure: No Action Required

**Method**: `{method}`
**File**: `{file}`
**Current CYC**: {current_cyc}
**Target CYC**: {target_cyc}

## Phase 0 Analysis Summary

The Phase 0 hotspot analysis determined that this method is already at or below the target complexity threshold.

**Key Findings**:
- Current cyclomatic complexity: {current_cyc}
- Target threshold: {target_cyc}
- Jane Street compliant: ✅ YES
- Extraction needed: ❌ NO

## Closure Rationale

This epic is being closed without further action because:

1. **Complexity Target Met**: The method's current CYC ({current_cyc}) is at or below the Jane Street-aligned target ({target_cyc})
2. **No Refactoring Needed**: The method structure is already maintainable and testable
3. **Resource Optimization**: No engineering time should be spent on already-compliant code

## Roadmap Updates Required

The following fields in `epic_roadmap.json` should be updated:

```json
{{
  "epic_number": "{epic_id}",
  "status": "completed",
  "final_cyc": {current_cyc},
  "completion_date": "{datetime.now(timezone.utc).strftime('%Y-%m-%d')}",
  "tickets_completed": 0,
  "notes": "No action required - already at target complexity"
}}
```

## Next Steps

1. ✅ Update roadmap status to "completed"
2. ✅ Mark epic as closed in tracking system
3. ✅ No further phases needed (1.5, 2, 3, 4, 5, 6 are N/A)

---

**Phase 1 Completed**: {datetime.now(timezone.utc).isoformat()}
**Epic Status**: CLOSED
"""


def create_extraction_scope(manifest: dict, hotspot_content: str) -> str:
    """Create scope document for extraction epic."""
    epic_id = manifest["epic_id"]
    method = manifest["method"]
    file = manifest["file"]
    current_cyc = manifest["current_cyc"]
    target_cyc = manifest["target_cyc"]
    
    return f"""# {epic_id} - Scope Definition

## Epic Scope: Complexity Reduction Required

**Method**: `{method}`
**File**: `{file}`
**Current CYC**: {current_cyc}
**Target CYC**: {target_cyc}
**Reduction Needed**: {current_cyc - target_cyc} points

## Phase 0 Analysis Summary

The Phase 0 hotspot analysis determined that this method exceeds the target complexity threshold and requires refactoring.

**Key Findings**:
- Current cyclomatic complexity: {current_cyc}
- Target threshold: {target_cyc}
- Jane Street compliant: ❌ NO
- Extraction needed: ✅ YES

## Scope Definition

### Primary Objective
Reduce `{method}` complexity from CYC {current_cyc} to ≤ {target_cyc} through surgical extraction.

### Constraints
1. **Zero Behavioral Change**: Extracted code must be functionally identical
2. **Lock-Free Mandate**: No `lock()` statements in extracted or remaining code
3. **ASCII-Only**: No Unicode characters in any code or comments
4. **Build Integrity**: Must compile cleanly after each extraction
5. **Test Coverage**: Extracted methods must be testable in isolation

### Success Criteria
- [ ] Final CYC ≤ {target_cyc}
- [ ] All tests pass
- [ ] Build succeeds
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained
- [ ] Hard-link sync successful (`deploy-sync.ps1`)

## Next Steps

1. ✅ Proceed to Phase 1.5 (Scope Boundary Validation)
2. ⏳ Phase 2 (Architecture Planning)
3. ⏳ Phase 3 (DNA & PR Audit)
4. ⏳ Phase 4 (Ticket Generation)
5. ⏳ Phase 5 (Ticket Execution)
6. ⏳ Phase 6 (Final Review)

---

**Phase 1 Completed**: {datetime.now(timezone.utc).isoformat()}
**Epic Status**: ACTIVE - Proceeding to Phase 1.5
"""


async def main():
    """Run the MCP server."""
    from mcp.server.stdio import stdio_server
    
    async with stdio_server() as (read_stream, write_stream):
        await app.run(
            read_stream,
            write_stream,
            app.create_initialization_options()
        )


if __name__ == "__main__":
    asyncio.run(main())

# Made with Bob
