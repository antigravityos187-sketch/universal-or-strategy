#!/usr/bin/env python3
"""
Phase 0: Hotspot Analysis MCP Server (Coordinator Pattern)
Fast, non-blocking coordinator that returns context immediately.
"""
import asyncio
import json
from pathlib import Path
from datetime import datetime
from typing import Dict, Any, List
from mcp.server import Server
from mcp.server.stdio import stdio_server
from mcp.types import Tool, TextContent

# Paths
REPO_PATH = Path("C:/WSGTA/universal-or-strategy")
BRAIN_DIR = REPO_PATH / "docs" / "brain"

# Initialize MCP server
app = Server("phase-0-hotspot")


@app.list_tools()
async def list_tools() -> List[Tool]:
    """List available MCP tools"""
    return [
        Tool(
            name="execute_phase_0",
            description="Coordinate Phase 0 (Hotspot Analysis). Returns context instantly.",
            inputSchema={
                "type": "object",
                "properties": {
                    "epic_id": {"type": "string"},
                    "method": {"type": "string"},
                    "file": {"type": "string"},
                    "cyc": {"type": "integer"},
                    "jcodemunch_data": {"type": "object"}
                },
                "required": ["epic_id", "method", "file", "cyc", "jcodemunch_data"]
            }
        )
    ]


@app.call_tool()
async def call_tool(name: str, arguments: Dict[str, Any]) -> List[TextContent]:
    """Handle MCP tool calls"""
    if name == "execute_phase_0":
        return await execute_phase_0_tool(arguments)
    return [TextContent(type="text", text=json.dumps({"error": f"Unknown tool: {name}"}))]


async def execute_phase_0_tool(args: Dict[str, Any]) -> List[TextContent]:
    """Return context immediately - no blocking operations"""
    try:
        jc = args["jcodemunch_data"]
        result = {
            "success": True,
            "phase": 0,
            "context": {
                "epic_id": args["epic_id"],
                "method": args["method"],
                "file": args["file"],
                "line": jc.get("line", 0),
                "complexity": {
                    "cyclomatic": jc.get("cyclomatic", args["cyc"]),
                    "max_nesting": jc.get("max_nesting", 0),
                    "param_count": jc.get("param_count", 0),
                    "lines": jc.get("lines", 0),
                    "assessment": jc.get("assessment", "unknown")
                },
                "needs_extraction": jc.get("cyclomatic", args["cyc"]) > 10
            }
        }
        return [TextContent(type="text", text=json.dumps(result))]
    except Exception as e:
        return [TextContent(type="text", text=json.dumps({"success": False, "error": str(e)}))]


async def main():
    """Run MCP server"""
    async with stdio_server() as (read_stream, write_stream):
        await app.run(read_stream, write_stream, app.create_initialization_options())


if __name__ == "__main__":
    asyncio.run(main())

# Made with Bob
