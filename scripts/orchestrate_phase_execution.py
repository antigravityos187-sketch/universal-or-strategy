#!/usr/bin/env python3
"""
Phase MCP Orchestration Script
Coordinates parallel execution of epic phases using MCP servers

Usage:
    python scripts/orchestrate_phase_execution.py --epic EPIC-CCN-22
    python scripts/orchestrate_phase_execution.py --epic EPIC-CCN-22 --phase 0
    python scripts/orchestrate_phase_execution.py --wave 1  # Execute all Phase 0s in parallel
"""

import argparse
import asyncio
import json
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Dict, List, Any, Optional

# Add project root to path
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

REPO_PATH = Path("C:/WSGTA/universal-or-strategy")
BRAIN_DIR = REPO_PATH / "docs/brain"
ROADMAP_FILE = REPO_PATH / "epic_roadmap.json"


class PhaseOrchestrator:
    """Orchestrates parallel phase execution across multiple epics"""
    
    def __init__(self):
        self.roadmap = self._load_roadmap()
        
    def _load_roadmap(self) -> Dict[str, Any]:
        """Load epic roadmap"""
        if not ROADMAP_FILE.exists():
            return {"epics": []}
        with open(ROADMAP_FILE, 'r', encoding='utf-8') as f:
            return json.load(f)
    
    def _save_roadmap(self):
        """Save updated roadmap"""
        with open(ROADMAP_FILE, 'w', encoding='utf-8') as f:
            json.dump(self.roadmap, f, indent=2)
    
    def get_epic(self, epic_id: str) -> Optional[Dict[str, Any]]:
        """Get epic by ID"""
        for epic in self.roadmap.get("epics", []):
            if epic.get("id") == epic_id:
                return epic
        return None
    
    def get_epics_by_phase(self, phase: int) -> List[Dict[str, Any]]:
        """Get all epics ready for a specific phase"""
        ready_epics = []
        for epic in self.roadmap.get("epics", []):
            if epic.get("status") == "complete":
                continue
            
            manifest_path = BRAIN_DIR / epic["id"] / "manifest.json"
            if not manifest_path.exists():
                if phase == 0:
                    ready_epics.append(epic)
                continue
            
            with open(manifest_path, 'r', encoding='utf-8') as f:
                manifest = json.load(f)
            
            # Check if ready for this phase
            if self._is_ready_for_phase(manifest, phase):
                ready_epics.append(epic)
        
        return ready_epics
    
    def _is_ready_for_phase(self, manifest: Dict[str, Any], phase: int) -> bool:
        """Check if epic is ready for a specific phase"""
        phases = manifest.get("phases", {})
        
        # Phase 0 can always start
        if phase == 0:
            return phases.get("phase_0", {}).get("status") != "completed"
        
        # Check dependencies
        phase_deps = {
            1: ["phase_0"],
            1.5: ["phase_1"],
            2: ["phase_1_5"],
            3: ["phase_2"],
            4: ["phase_2"],
            5: ["phase_4"],
            6: ["phase_5"]
        }
        
        deps = phase_deps.get(phase, [])
        for dep in deps:
            if phases.get(dep, {}).get("status") != "completed":
                return False
        
        # Check current phase status
        phase_key = f"phase_{phase}".replace(".", "_")
        return phases.get(phase_key, {}).get("status") != "completed"
    
    async def execute_phase(self, epic_id: str, phase: int) -> Dict[str, Any]:
        """Execute a single phase for an epic"""
        print(f"\n{'='*60}")
        print(f"Executing Phase {phase} for {epic_id}")
        print(f"{'='*60}\n")
        
        epic = self.get_epic(epic_id)
        if not epic:
            return {
                "success": False,
                "error": f"Epic {epic_id} not found in roadmap"
            }
        
        # Map phase to MCP tool
        phase_tools = {
            0: ("phase-0-hotspot", "execute_phase_0", {
                "epic_id": epic_id,
                "method": epic.get("method", ""),
                "file": epic.get("file", ""),
                "cyc": epic.get("cyc", 0)
            }),
            1: ("phase-1-scope", "execute_phase_1", {"epic_id": epic_id}),
            1.5: ("phase-1-5-boundary", "execute_phase_1_5", {"epic_id": epic_id}),
            2: ("phase-2-architecture", "execute_phase_2", {"epic_id": epic_id}),
            3: ("phase-3-audit", "execute_phase_3", {"epic_id": epic_id}),
            4: ("phase-4-tickets", "execute_phase_4", {"epic_id": epic_id}),
            5: ("phase-5-execute", "execute_phase_5", {"epic_id": epic_id}),
            6: ("phase-6-review", "execute_phase_6", {"epic_id": epic_id})
        }
        
        if phase not in phase_tools:
            return {
                "success": False,
                "error": f"Invalid phase: {phase}"
            }
        
        server_name, tool_name, params = phase_tools[phase]
        
        # Note: Actual MCP tool invocation would happen here via Bob CLI
        # For now, we document the command that should be run
        print(f"MCP Server: {server_name}")
        print(f"Tool: {tool_name}")
        print(f"Parameters: {json.dumps(params, indent=2)}")
        print(f"\nTo execute via Bob CLI:")
        print(f"  use_mcp_tool(server='{server_name}', tool='{tool_name}', args={json.dumps(params)})")
        
        return {
            "success": True,
            "epic_id": epic_id,
            "phase": phase,
            "server": server_name,
            "tool": tool_name,
            "params": params,
            "timestamp": datetime.now(timezone.utc).isoformat()
        }
    
    async def execute_wave(self, phase: int) -> List[Dict[str, Any]]:
        """Execute a phase for all ready epics in parallel"""
        ready_epics = self.get_epics_by_phase(phase)
        
        if not ready_epics:
            print(f"No epics ready for Phase {phase}")
            return []
        
        print(f"\n{'='*60}")
        print(f"Wave Execution: Phase {phase}")
        print(f"Ready Epics: {len(ready_epics)}")
        print(f"{'='*60}\n")
        
        # Execute all in parallel
        tasks = [
            self.execute_phase(epic["id"], phase)
            for epic in ready_epics
        ]
        
        results = await asyncio.gather(*tasks, return_exceptions=True)
        
        # Process results
        successful = []
        failed = []
        processed_results: List[Dict[str, Any]] = []
        
        for result in results:
            if isinstance(result, Exception):
                error_dict = {"success": False, "error": str(result)}
                failed.append(error_dict)
                processed_results.append(error_dict)
            elif isinstance(result, dict):
                if result.get("success"):
                    successful.append(result)
                else:
                    failed.append(result)
                processed_results.append(result)
        
        print(f"\n{'='*60}")
        print(f"Wave Complete: Phase {phase}")
        print(f"Successful: {len(successful)}")
        print(f"Failed: {len(failed)}")
        print(f"{'='*60}\n")
        
        return processed_results
    
    def generate_execution_plan(self, epic_id: str) -> Dict[str, Any]:
        """Generate complete execution plan for an epic"""
        epic = self.get_epic(epic_id)
        if not epic:
            return {"error": f"Epic {epic_id} not found"}
        
        manifest_path = BRAIN_DIR / epic_id / "manifest.json"
        if manifest_path.exists():
            with open(manifest_path, 'r', encoding='utf-8') as f:
                manifest = json.load(f)
        else:
            manifest = {"phases": {}}
        
        plan = {
            "epic_id": epic_id,
            "current_status": epic.get("status", "pending"),
            "phases": []
        }
        
        # Determine which phases are complete, in progress, or pending
        phase_sequence = [0, 1, 1.5, 2, 3, 4, 5, 6]
        for phase in phase_sequence:
            phase_key = f"phase_{phase}".replace(".", "_")
            phase_data = manifest.get("phases", {}).get(phase_key, {})
            
            plan["phases"].append({
                "phase": phase,
                "status": phase_data.get("status", "pending"),
                "ready": self._is_ready_for_phase(manifest, phase),
                "completed_at": phase_data.get("completed_at")
            })
        
        return plan


async def main():
    parser = argparse.ArgumentParser(description="Orchestrate phase execution")
    parser.add_argument("--epic", help="Epic ID to execute")
    parser.add_argument("--phase", type=float, help="Specific phase to execute (0, 1, 1.5, 2, 3, 4, 5, 6)")
    parser.add_argument("--wave", type=int, help="Execute phase for all ready epics")
    parser.add_argument("--plan", action="store_true", help="Show execution plan for epic")
    
    args = parser.parse_args()
    
    orchestrator = PhaseOrchestrator()
    
    if args.plan and args.epic:
        plan = orchestrator.generate_execution_plan(args.epic)
        print(json.dumps(plan, indent=2))
        return
    
    if args.wave is not None:
        results = await orchestrator.execute_wave(args.wave)
        print(json.dumps(results, indent=2))
        return
    
    if args.epic and args.phase is not None:
        result = await orchestrator.execute_phase(args.epic, args.phase)
        print(json.dumps(result, indent=2))
        return
    
    parser.print_help()


if __name__ == "__main__":
    asyncio.run(main())

# Made with Bob
