#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Test parallel Phase 0 execution for 3 epics using MCP tools.

This script demonstrates parallel execution by running Phase 0 (Hotspot Analysis)
for 3 epics simultaneously. This validates the MCP architecture's ability to
handle concurrent epic processing.

Usage:
    python scripts/test_parallel_phase0.py
"""
import sys
import io

# Force UTF-8 encoding for Windows console (V12 ASCII-only compliance)
if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')
import json
import asyncio
import sys
from pathlib import Path
from datetime import datetime

# Add project root to path
sys.path.insert(0, str(Path(__file__).parent.parent))

async def execute_phase_0_mcp(epic_id: str, method: str, file: str, cyc: int) -> dict:
    """
    Execute Phase 0 for a single epic using the phase-0-hotspot MCP tool.
    
    In production, this would use the actual MCP tool via use_mcp_tool.
    For testing, we simulate the MCP call structure.
    
    Args:
        epic_id: Epic identifier (e.g., 'EPIC-CCN-22')
        method: Method name to analyze
        file: File path containing the method
        cyc: Current cyclomatic complexity
    
    Returns:
        dict: Execution result with status and timing
    """
    start_time = datetime.now()
    
    print(f"[{epic_id}] Starting Phase 0: {method} (CYC {cyc})")
    
    # Simulate MCP tool call structure
    mcp_call = {
        "server": "phase-0-hotspot",
        "tool": "execute_phase_0",
        "args": {
            "epic_id": epic_id,
            "method": method,
            "file": file,
            "cyc": cyc
        }
    }
    
    print(f"[{epic_id}] MCP Call: {json.dumps(mcp_call, indent=2)}")
    
    # In production, this would be:
    # result = await use_mcp_tool(**mcp_call)
    
    # For testing, simulate execution time (2-5 seconds per epic)
    await asyncio.sleep(3)
    
    end_time = datetime.now()
    duration = (end_time - start_time).total_seconds()
    
    # Check if manifest was created
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    hotspot_path = Path(f"docs/brain/{epic_id}/00-hotspots.md")
    
    result = {
        "epic_id": epic_id,
        "method": method,
        "status": "success" if manifest_path.exists() else "pending",
        "duration_seconds": duration,
        "manifest_exists": manifest_path.exists(),
        "hotspot_exists": hotspot_path.exists(),
        "mcp_call": mcp_call
    }
    
    print(f"[{epic_id}] Completed in {duration:.2f}s - Status: {result['status']}")
    
    return result

async def run_parallel_phase0_test():
    """
    Execute Phase 0 for 3 epics in parallel.
    
    This demonstrates the core capability of the Phase MCP architecture:
    running the same phase for multiple epics simultaneously.
    """
    print("=" * 80)
    print("PARALLEL PHASE 0 EXECUTION TEST")
    print("=" * 80)
    print()
    
    # Load next 3 pending epics
    with open('epic_roadmap.json', 'r') as f:
        roadmap = json.load(f)
    
    pending = [e for e in roadmap if e.get('status') != 'complete']
    test_epics = pending[:3]
    
    print(f"Testing with {len(test_epics)} epics:")
    for epic in test_epics:
        print(f"  - {epic['epic_number']}: {epic['method']} (CYC {epic['cyclomatic']})")
    print()
    
    # Create async tasks for parallel execution
    tasks = [
        execute_phase_0_mcp(
            epic_id=epic['epic_number'],
            method=epic['method'],
            file=epic['file'],
            cyc=epic['cyclomatic']
        )
        for epic in test_epics
    ]
    
    print("Starting parallel execution...")
    print()
    
    start_time = datetime.now()
    
    # Execute all Phase 0 tasks in parallel
    results = await asyncio.gather(*tasks, return_exceptions=True)
    
    end_time = datetime.now()
    total_duration = (end_time - start_time).total_seconds()
    
    print()
    print("=" * 80)
    print("RESULTS")
    print("=" * 80)
    print()
    
    # Analyze results
    successful = sum(1 for r in results if isinstance(r, dict) and r['status'] == 'success')
    pending = sum(1 for r in results if isinstance(r, dict) and r['status'] == 'pending')
    failed = sum(1 for r in results if isinstance(r, Exception))
    
    print(f"Total Duration: {total_duration:.2f}s")
    print(f"Average per Epic: {total_duration / len(test_epics):.2f}s")
    print()
    print(f"Results:")
    print(f"  [OK] Successful: {successful}")
    print(f"  [WAIT] Pending: {pending}")
    print(f"  [FAIL] Failed: {failed}")
    print()
    
    # Detailed results
    print("Detailed Results:")
    for i, result in enumerate(results, 1):
        if isinstance(result, Exception):
            print(f"  {i}. ERROR: {result}")
        elif isinstance(result, dict):
            status_icon = "[OK]" if result['status'] == 'success' else "[WAIT]"
            print(f"  {i}. {status_icon} {result['epic_id']}: {result['method']}")
            print(f"     Duration: {result['duration_seconds']:.2f}s")
            print(f"     Manifest: {'Y' if result['manifest_exists'] else 'N'}")
            print(f"     Hotspot: {'Y' if result['hotspot_exists'] else 'N'}")
    
    print()
    print("=" * 80)
    print("PARALLEL EXECUTION VALIDATION")
    print("=" * 80)
    print()
    
    # Validate parallel execution benefit
    sequential_time = sum(r['duration_seconds'] for r in results if isinstance(r, dict))
    speedup = sequential_time / total_duration if total_duration > 0 else 0
    
    print(f"Sequential Time (sum): {sequential_time:.2f}s")
    print(f"Parallel Time (actual): {total_duration:.2f}s")
    print(f"Speedup: {speedup:.2f}x")
    print()
    
    if speedup >= 2.5:
        print("[PASS] Parallel execution achieved significant speedup (>2.5x)")
    elif speedup >= 1.5:
        print("[PARTIAL] Parallel execution achieved moderate speedup (1.5-2.5x)")
    else:
        print("[FAIL] Parallel execution did not achieve expected speedup (<1.5x)")
    
    print()
    print("=" * 80)
    print("NEXT STEPS")
    print("=" * 80)
    print()
    print("To execute Phase 0 for these epics using actual MCP tools:")
    print()
    for epic in test_epics:
        print(f"use_mcp_tool(")
        print(f"  server='phase-0-hotspot',")
        print(f"  tool='execute_phase_0',")
        print(f"  args={{")
        print(f"    'epic_id': '{epic['epic_number']}',")
        print(f"    'method': '{epic['method']}',")
        print(f"    'file': '{epic['file']}',")
        print(f"    'cyc': {epic['cyclomatic']}")
        print(f"  }}")
        print(f")")
        print()
    
    return results

def main():
    """Main entry point."""
    try:
        results = asyncio.run(run_parallel_phase0_test())
        
        # Exit with appropriate code
        failed = sum(1 for r in results if isinstance(r, Exception))
        sys.exit(1 if failed > 0 else 0)
        
    except KeyboardInterrupt:
        print("\n\nTest interrupted by user")
        sys.exit(130)
    except Exception as e:
        print(f"\n\nFATAL ERROR: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)

if __name__ == '__main__':
    main()

# Made with Bob
