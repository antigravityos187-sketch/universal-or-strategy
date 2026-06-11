#!/usr/bin/env python3
"""
Wave Coordinator for V12 Epic Parallel Execution.

Architecture: Single Orchestrator with Sequential Phase Waves
- Process N epics through Phase 0, then Phase 1, then Phase 2, etc.
- Each phase calls its dedicated MCP tool
- Configurable wave size (default: 10 epics)
"""

import json
import sys
from pathlib import Path
from typing import List, Dict, Any
from datetime import datetime

class WaveCoordinator:
    """Coordinates wave-based epic execution through all phases."""
    
    # Phase definitions (in execution order)
    PHASES = [
        {"id": 0, "name": "Hotspot Analysis", "mcp_server": "phase-0-hotspot", "tool": "execute_phase_0"},
        {"id": 1, "name": "Scope Definition", "mcp_server": "phase-1-scope", "tool": "execute_phase_1"},
        {"id": 1.5, "name": "Scope Boundary", "mcp_server": "phase-1-5-boundary", "tool": "execute_phase_1_5"},
        {"id": 2, "name": "Architecture Planning", "mcp_server": "phase-2-architecture", "tool": "execute_phase_2"},
        {"id": 3, "name": "DNA & PR Audit", "mcp_server": "phase-3-audit", "tool": "execute_phase_3"},
        {"id": 4, "name": "Ticket Generation", "mcp_server": "phase-4-tickets", "tool": "execute_phase_4"},
        {"id": 5, "name": "Ticket Execution", "mcp_server": "phase-5-execute", "tool": "execute_phase_5"},
        {"id": 5.5, "name": "Verification", "mcp_server": "phase-5-verify", "tool": "execute_phase_5_verify"},
        {"id": 6, "name": "Final Review", "mcp_server": "phase-6-review", "tool": "execute_phase_6"},
    ]
    
    def __init__(self, wave_size: int = 10, start_phase: int = 0, end_phase: int = 6):
        """
        Initialize Wave Coordinator.
        
        Args:
            wave_size: Number of epics to process per wave (default: 10)
            start_phase: Starting phase ID (default: 0)
            end_phase: Ending phase ID (default: 6)
        """
        self.wave_size = wave_size
        self.start_phase = start_phase
        self.end_phase = end_phase
        self.roadmap_path = Path("epic_roadmap.json")
        self.results = []
        
    def load_roadmap(self) -> List[Dict[str, Any]]:
        """Load epic roadmap and filter pending epics."""
        data = json.loads(self.roadmap_path.read_text())
        pending = [e for e in data if e.get("status") != "complete"]
        return pending
    
    def get_phase_config(self, phase_id: float) -> Dict[str, Any]:
        """Get phase configuration by ID."""
        for phase in self.PHASES:
            if phase["id"] == phase_id:
                return phase
        raise ValueError(f"Unknown phase ID: {phase_id}")
    
    def execute_wave(self, epic_ids: List[str], phase_id: float) -> Dict[str, Any]:
        """
        Execute one phase for multiple epics.
        
        This is a COORDINATOR function - it prepares the MCP tool calls
        but YOU (the orchestrator) must execute them in Bob IDE.
        
        Args:
            epic_ids: List of epic IDs to process
            phase_id: Phase ID to execute
            
        Returns:
            Dict with MCP tool call instructions
        """
        phase = self.get_phase_config(phase_id)
        
        print(f"\n{'='*60}")
        print(f"WAVE: Phase {phase_id} - {phase['name']}")
        print(f"Processing {len(epic_ids)} epics")
        print(f"{'='*60}\n")
        
        # Prepare MCP tool calls
        mcp_calls = []
        for epic_id in epic_ids:
            call = {
                "server_name": phase["mcp_server"],
                "tool_name": phase["tool"],
                "arguments": {"epic_id": epic_id},
                "epic_id": epic_id
            }
            mcp_calls.append(call)
            print(f"  - {epic_id}: {phase['mcp_server']}.{phase['tool']}")
        
        return {
            "phase_id": phase_id,
            "phase_name": phase["name"],
            "epic_count": len(epic_ids),
            "mcp_calls": mcp_calls,
            "instructions": self._generate_instructions(phase, mcp_calls)
        }
    
    def _generate_instructions(self, phase: Dict, mcp_calls: List[Dict]) -> str:
        """Generate human-readable instructions for executing MCP calls."""
        instructions = f"""
# Execute Phase {phase['id']}: {phase['name']}

## MCP Tool Calls (Execute in Bob IDE)

You must call the following MCP tools in Bob IDE:

"""
        for i, call in enumerate(mcp_calls, 1):
            instructions += f"""
### Call {i}: {call['epic_id']}
```
use_mcp_tool(
    server_name="{call['server_name']}",
    tool_name="{call['tool_name']}",
    arguments={json.dumps(call['arguments'], indent=2)}
)
```
"""
        
        instructions += f"""

## Expected Results

- {len(mcp_calls)} MCP tool calls should complete
- Each call should return in <1 second (FastMCP)
- Check for any failures and report back

## Next Steps

After all {len(mcp_calls)} calls complete:
1. Review results for any failures
2. Call `wave_coordinator.next_wave()` to proceed to next phase
"""
        return instructions
    
    def run_wave_batch(self, epic_ids: List[str], phases: List[float] | None = None) -> List[Dict]:
        """
        Run a batch of epics through specified phases.
        
        Args:
            epic_ids: List of epic IDs to process
            phases: List of phase IDs to execute (default: all phases)
            
        Returns:
            List of wave execution results
        """
        if phases is None:
            phases = [p["id"] for p in self.PHASES if self.start_phase <= p["id"] <= self.end_phase]
        
        results = []
        for phase_id in phases:
            wave_result = self.execute_wave(epic_ids, phase_id)
            results.append(wave_result)
            
            # Save checkpoint after each phase
            self._save_checkpoint(phase_id, epic_ids, wave_result)
        
        return results
    
    def _save_checkpoint(self, phase_id: float, epic_ids: List[str], result: Dict):
        """Save wave execution checkpoint."""
        checkpoint = {
            "timestamp": datetime.utcnow().isoformat(),
            "phase_id": phase_id,
            "epic_ids": epic_ids,
            "result": result
        }
        
        checkpoint_path = Path("docs/brain/wave_checkpoints.json")
        checkpoint_path.parent.mkdir(parents=True, exist_ok=True)
        
        # Load existing checkpoints
        checkpoints = []
        if checkpoint_path.exists():
            checkpoints = json.loads(checkpoint_path.read_text())
        
        checkpoints.append(checkpoint)
        checkpoint_path.write_text(json.dumps(checkpoints, indent=2))
        print(f"✅ Checkpoint saved: Phase {phase_id} for {len(epic_ids)} epics")
    
    def get_next_wave(self, wave_number: int = 1) -> List[str]:
        """
        Get next batch of pending epics.
        
        Args:
            wave_number: Wave number (1-based)
            
        Returns:
            List of epic IDs for this wave
        """
        pending = self.load_roadmap()
        start_idx = (wave_number - 1) * self.wave_size
        end_idx = start_idx + self.wave_size
        
        wave_epics = pending[start_idx:end_idx]
        epic_ids = [e["epic_number"] for e in wave_epics]
        
        print(f"\n{'='*60}")
        print(f"WAVE {wave_number}: {len(epic_ids)} epics")
        print(f"{'='*60}")
        for epic in wave_epics:
            print(f"  - {epic['epic_number']}: {epic['method']} (CYC={epic['cyclomatic']})")
        print()
        
        return epic_ids
    
    def generate_execution_plan(self, num_waves: int = 1) -> Dict[str, Any]:
        """
        Generate complete execution plan for N waves.
        
        Args:
            num_waves: Number of waves to plan (default: 1)
            
        Returns:
            Complete execution plan with all MCP calls
        """
        pending = self.load_roadmap()
        total_epics = len(pending)
        total_waves = (total_epics + self.wave_size - 1) // self.wave_size
        
        plan = {
            "total_epics": total_epics,
            "total_waves": total_waves,
            "wave_size": self.wave_size,
            "waves_to_execute": min(num_waves, total_waves),
            "waves": []
        }
        
        for wave_num in range(1, min(num_waves, total_waves) + 1):
            epic_ids = self.get_next_wave(wave_num)
            wave_plan = {
                "wave_number": wave_num,
                "epic_ids": epic_ids,
                "phases": []
            }
            
            for phase in self.PHASES:
                if self.start_phase <= phase["id"] <= self.end_phase:
                    wave_result = self.execute_wave(epic_ids, phase["id"])
                    wave_plan["phases"].append(wave_result)
            
            plan["waves"].append(wave_plan)
        
        return plan


def main():
    """CLI entry point for wave coordinator."""
    import argparse
    
    parser = argparse.ArgumentParser(description="V12 Wave Coordinator")
    parser.add_argument("--wave-size", type=int, default=10, help="Epics per wave (default: 10)")
    parser.add_argument("--num-waves", type=int, default=1, help="Number of waves to execute (default: 1)")
    parser.add_argument("--start-phase", type=float, default=0, help="Starting phase ID (default: 0)")
    parser.add_argument("--end-phase", type=float, default=6, help="Ending phase ID (default: 6)")
    parser.add_argument("--plan-only", action="store_true", help="Generate plan without executing")
    
    args = parser.parse_args()
    
    coordinator = WaveCoordinator(
        wave_size=args.wave_size,
        start_phase=args.start_phase,
        end_phase=args.end_phase
    )
    
    if args.plan_only:
        plan = coordinator.generate_execution_plan(args.num_waves)
        print(json.dumps(plan, indent=2))
    else:
        print("Wave Coordinator initialized.")
        print(f"Wave size: {args.wave_size}")
        print(f"Phases: {args.start_phase} to {args.end_phase}")
        print("\nUse coordinator.get_next_wave() to start execution.")


if __name__ == "__main__":
    main()

# Made with Bob
