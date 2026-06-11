#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Phase 0 Orchestrator with jCodemunch Pre-fetch

This orchestrator handles jCodemunch queries BEFORE calling the Phase 0 MCP server,
eliminating nested MCP calls and timeout issues.

Architecture:
1. Orchestrator (this script) calls jCodemunch MCP to get method data
2. Orchestrator enriches epic data with jCodemunch results
3. Orchestrator calls Phase 0 MCP with enriched data
4. Phase 0 MCP just formats and writes (no nested calls)

Usage:
    python scripts/orchestrate_phase0_with_prep.py EPIC-CCN-22
"""
import sys
import json
from pathlib import Path
from datetime import datetime
from typing import Dict, Optional

def prepare_jcodemunch_data(epic_id: str, method: str, file: str, cyc: int) -> Dict:
    """
    Pre-fetch jCodemunch data for an epic method.
    
    This runs BEFORE calling the Phase 0 MCP server, so the Phase 0 server
    doesn't need to make nested MCP calls.
    
    Args:
        epic_id: Epic identifier
        method: Method name
        file: File path
        cyc: Current cyclomatic complexity
    
    Returns:
        dict: Enriched data ready for Phase 0 MCP server
    """
    print(f"[{epic_id}] Pre-fetching jCodemunch data for {method}...")
    
    # In production, this would use jCodemunch MCP tool
    # For now, simulate the data structure
    
    # TODO: Replace with actual jCodemunch MCP call:
    # result = use_mcp_tool(
    #     server='jcodemunch-mcp',
    #     tool='search_symbols',
    #     args={
    #         'repo': 'universal-or-strategy',
    #         'query': method,
    #         'kind': 'method',
    #         'file_pattern': file
    #     }
    # )
    
    # Simulated jCodemunch response
    jcodemunch_data = {
        "symbol_id": f"{file}::{method}#method",
        "name": method,
        "kind": "method",
        "file": file,
        "line": 0,  # Would come from jCodemunch
        "end_line": 0,  # Would come from jCodemunch
        "signature": f"public void {method}()",  # Would come from jCodemunch
        "summary": f"Method {method} in {file}",  # Would come from jCodemunch
        "complexity": cyc,
        "found": True,
        "verified": False  # Set to True when actual jCodemunch data available
    }
    
    return {
        "epic_id": epic_id,
        "method": method,
        "file": file,
        "cyc": cyc,
        "jcodemunch": jcodemunch_data,
        "timestamp": datetime.now().isoformat()
    }

def call_phase0_mcp(enriched_data: Dict) -> Dict:
    """
    Call Phase 0 MCP server with pre-fetched data.
    
    The Phase 0 server receives all data it needs, so it doesn't
    need to make any nested MCP calls.
    
    Args:
        enriched_data: Data with jCodemunch results already included
    
    Returns:
        dict: Phase 0 execution result
    """
    epic_id = enriched_data['epic_id']
    print(f"[{epic_id}] Calling Phase 0 MCP server...")
    
    # TODO: Replace with actual MCP call:
    # result = use_mcp_tool(
    #     server='phase-0-hotspot',
    #     tool='execute_phase_0',
    #     args=enriched_data
    # )
    
    # Simulated Phase 0 result
    result = {
        "status": "success",
        "epic_id": epic_id,
        "phase": 0,
        "manifest_created": True,
        "hotspot_created": True,
        "artifacts": [
            f"docs/brain/{epic_id}/manifest.json",
            f"docs/brain/{epic_id}/00-hotspots.md"
        ]
    }
    
    print(f"[{epic_id}] Phase 0 complete: {result['status']}")
    return result

def execute_phase0_with_prep(epic_id: str, method: str, file: str, cyc: int) -> Dict:
    """
    Execute Phase 0 with orchestrator-level preparation.
    
    This is the main entry point that coordinates:
    1. jCodemunch data pre-fetch
    2. Data enrichment
    3. Phase 0 MCP call with enriched data
    
    Args:
        epic_id: Epic identifier
        method: Method name
        file: File path
        cyc: Current cyclomatic complexity
    
    Returns:
        dict: Complete execution result
    """
    print(f"\n{'=' * 80}")
    print(f"PHASE 0 EXECUTION WITH PREP: {epic_id}")
    print(f"{'=' * 80}")
    print(f"Method: {method}")
    print(f"File: {file}")
    print(f"Complexity: {cyc}")
    print()
    
    # Step 1: Pre-fetch jCodemunch data (orchestrator handles this)
    enriched_data = prepare_jcodemunch_data(epic_id, method, file, cyc)
    
    # Step 2: Call Phase 0 MCP with enriched data (no nested MCP calls)
    result = call_phase0_mcp(enriched_data)
    
    print()
    print(f"{'=' * 80}")
    print(f"PHASE 0 COMPLETE: {epic_id}")
    print(f"{'=' * 80}")
    print(f"Status: {result['status']}")
    print(f"Artifacts: {len(result.get('artifacts', []))}")
    print()
    
    return result

def main():
    """Main entry point for CLI usage."""
    if len(sys.argv) < 2:
        print("Usage: python scripts/orchestrate_phase0_with_prep.py <epic_id>")
        print("\nExample:")
        print("  python scripts/orchestrate_phase0_with_prep.py EPIC-CCN-22")
        sys.exit(1)
    
    epic_id = sys.argv[1]
    
    # Load epic data from roadmap
    with open('epic_roadmap.json', 'r') as f:
        roadmap = json.load(f)
    
    # Find epic
    epic = next((e for e in roadmap if e['epic_number'] == epic_id), None)
    if not epic:
        print(f"ERROR: Epic {epic_id} not found in roadmap")
        sys.exit(1)
    
    # Execute Phase 0 with prep
    result = execute_phase0_with_prep(
        epic_id=epic['epic_number'],
        method=epic['method'],
        file=epic['file'],
        cyc=epic['cyclomatic']
    )
    
    # Print result
    print(json.dumps(result, indent=2))

if __name__ == '__main__':
    main()

# Made with Bob
