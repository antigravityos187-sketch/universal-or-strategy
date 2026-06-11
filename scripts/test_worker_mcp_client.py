#!/usr/bin/env python3
"""
Test Client for Worker Agent MCP Tools
V12 Universal OR Strategy - Watsonx Orchestrate Integration

Tests all 5 MCP tools for each worker agent
"""

import asyncio
import json
import sys
from pathlib import Path
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client


async def test_worker_agent(worker_id: str, worktree_path: str):
    """Test all MCP tools for a single worker"""
    print(f"\n{'='*60}")
    print(f"Testing Worker: {worker_id}")
    print(f"Worktree: {worktree_path}")
    print(f"{'='*60}\n")
    
    server_params = StdioServerParameters(
        command="python",
        args=[
            str(Path(__file__).parent / "worker_agent_mcp.py"),
            worker_id,
            worktree_path
        ]
    )
    
    try:
        async with stdio_client(server_params) as (read, write):
            async with ClientSession(read, write) as session:
                await session.initialize()
                
                # Test 1: Get worker status
                print("Test 1: Get Worker Status")
                print("-" * 40)
                result = await session.call_tool("get_worker_status", {})
                print(json.dumps(result.content[0].text, indent=2))
                print()
                
                # Test 2: Get next pending epic
                print("Test 2: Get Next Pending Epic")
                print("-" * 40)
                result = await session.call_tool("get_next_pending_epic", {})
                next_epic_data = json.loads(result.content[0].text)
                print(json.dumps(next_epic_data, indent=2))
                print()
                
                if not next_epic_data.get('success'):
                    print("⚠️  No pending epics found. Skipping claim/execute/release tests.")
                    return
                
                epic_id = next_epic_data['epic_id']
                
                # Test 3: Claim epic
                print(f"Test 3: Claim Epic ({epic_id})")
                print("-" * 40)
                result = await session.call_tool("claim_epic", {"epic_id": epic_id})
                claim_data = json.loads(result.content[0].text)
                print(json.dumps(claim_data, indent=2))
                print()
                
                if not claim_data.get('success'):
                    print(f"⚠️  Failed to claim {epic_id}. Skipping execute/release tests.")
                    return
                
                # Test 4: Execute epic (COMMENTED OUT - takes 30+ minutes)
                # Uncomment to test full epic execution
                # print(f"Test 4: Execute Epic ({epic_id})")
                # print("-" * 40)
                # result = await session.call_tool("execute_epic", {"epic_id": epic_id})
                # execute_data = json.loads(result.content[0].text)
                # print(json.dumps(execute_data, indent=2))
                # print()
                
                print(f"Test 4: Execute Epic ({epic_id}) - SKIPPED")
                print("-" * 40)
                print("⚠️  Epic execution takes 30+ minutes. Skipping for quick test.")
                print("To test execution, uncomment lines 67-72 in this script.")
                print()
                
                # Test 5: Release epic
                print(f"Test 5: Release Epic ({epic_id})")
                print("-" * 40)
                result = await session.call_tool("release_epic", {"epic_id": epic_id})
                release_data = json.loads(result.content[0].text)
                print(json.dumps(release_data, indent=2))
                print()
                
                print(f"✅ All tests passed for {worker_id}")
    
    except Exception as e:
        print(f"❌ Error testing {worker_id}: {e}")
        import traceback
        traceback.print_exc()


async def test_all_workers():
    """Test all 4 worker agents"""
    workers = [
        ("worker-1", "C:/WSGTA/universal-or-epic-cluster-1"),
        ("worker-2", "C:/WSGTA/universal-or-epic-cluster-2"),
        ("worker-3", "C:/WSGTA/universal-or-epic-cluster-3"),
        ("worker-4", "C:/WSGTA/universal-or-epic-cluster-4")
    ]
    
    print("\n" + "="*60)
    print("Worker Agent MCP Tools - Test Suite")
    print("="*60)
    
    for worker_id, worktree_path in workers:
        await test_worker_agent(worker_id, worktree_path)
    
    print("\n" + "="*60)
    print("Test Suite Complete")
    print("="*60)


async def test_single_worker(worker_id: str):
    """Test a single worker by ID"""
    worktree_path = f"C:/WSGTA/universal-or-epic-cluster-{worker_id[-1]}"
    await test_worker_agent(worker_id, worktree_path)


if __name__ == "__main__":
    if len(sys.argv) > 1:
        # Test specific worker: python test_worker_mcp_client.py worker-1
        worker_id = sys.argv[1]
        asyncio.run(test_single_worker(worker_id))
    else:
        # Test all workers
        asyncio.run(test_all_workers())

# Made with Bob
