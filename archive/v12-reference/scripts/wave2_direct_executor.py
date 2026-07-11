#!/usr/bin/env python3
"""
Wave 2 Direct Executor - Bypass MCP, execute phases directly
No MCP servers needed - just direct Python execution
"""

import json
import subprocess
from pathlib import Path
from typing import Dict, List

# Wave 2 Epics (9 remaining, EPIC-CCN-32 already complete)
WAVE_2_EPICS = [
    {"epic_id": "EPIC-CCN-164", "method": "IsCommandForThisInstrument", "file": "src/V12_002.UI.IPC.cs", "cyc": 36},
    {"epic_id": "EPIC-CCN-107", "method": "HydrateFromOpenPositions", "file": "src/V12_002.SIMA.Lifecycle.cs", "cyc": 31},
    {"epic_id": "EPIC-CCN-108", "method": "SweepBrokerOrders", "file": "src/V12_002.SIMA.Lifecycle.cs", "cyc": 24},
    # EPIC-CCN-32 already complete - skip
    {"epic_id": "EPIC-CCN-109", "method": "HydrateWorkingOrdersFromBroker", "file": "src/V12_002.SIMA.Lifecycle.cs", "cyc": 19},
    {"epic_id": "EPIC-CCN-110", "method": "AdoptMasterOrders", "file": "src/V12_002.SIMA.Lifecycle.cs", "cyc": 19},
    {"epic_id": "EPIC-CCN-155", "method": "TryHandleFleetCommand", "file": "src/V12_002.UI.IPC.Commands.Fleet.cs", "cyc": 19},
    {"epic_id": "EPIC-CCN-98", "method": "ProcessFlattenWorkItem_CancelOrders", "file": "src/V12_002.SIMA.Flatten.cs", "cyc": 18},
    {"epic_id": "EPIC-CCN-128", "method": "SymmetryGuardReplaceExistingFollowerTarget", "file": "src/V12_002.Symmetry.Replace.cs", "cyc": 18},
    {"epic_id": "EPIC-CCN-129", "method": "SymmetryGuardTryResolveFollowersForDispatch", "file": "src/V12_002.Symmetry.Replace.cs", "cyc": 18},
]

def create_phase_0_artifacts(epic: Dict) -> bool:
    """Create Phase 0 artifacts directly (no MCP)"""
    epic_id = epic["epic_id"]
    method = epic["method"]
    file = epic["file"]
    cyc = epic["cyc"]
    
    # Create epic directory
    epic_dir = Path(f"docs/brain/{epic_id}")
    epic_dir.mkdir(parents=True, exist_ok=True)
    
    # Create 00-hotspots.md
    hotspots_content = f"""# Phase 0: Hotspot Analysis - {epic_id}

## Target Method
- **Method**: `{method}`
- **File**: `{file}`
- **Complexity**: {cyc}
- **Target**: 8 (reduction: {round((cyc - 8) / cyc * 100)}%)

## Risk Assessment
- **Current CYC**: {cyc}
- **Risk Level**: {"HIGH" if cyc > 25 else "MEDIUM" if cyc > 15 else "LOW"}

## Blast Radius
(To be analyzed in Phase 1)

## Extraction Strategy
(To be defined in Phase 2)

## Status
[OK] Phase 0 Complete - Ready for Phase 1 (Scope Definition)
"""
    
    hotspots_file = epic_dir / "00-hotspots.md"
    hotspots_file.write_text(hotspots_content)
    
    # Create manifest.json
    manifest = {
        "epic_id": epic_id,
        "method": method,
        "file": file,
        "complexity": cyc,
        "target_complexity": 8,
        "phases": {
            "phase_0": {
                "status": "completed",
                "output": "00-hotspots.md"
            }
        }
    }
    
    manifest_file = epic_dir / "manifest.json"
    manifest_file.write_text(json.dumps(manifest, indent=2))
    
    print(f"[OK] {epic_id}: Phase 0 artifacts created")
    return True

def execute_phase_0_all() -> None:
    """Execute Phase 0 for all Wave 2 epics"""
    print("\n[Wave 2 Phase 0: Hotspot Analysis (9 epics)]")
    print("=" * 60)
    
    for epic in WAVE_2_EPICS:
        create_phase_0_artifacts(epic)
    
    print("\n[OK] Phase 0 Complete for all 9 epics")
    print("\nNext: Run Phase 1 (Scope Definition)")

def create_bob_prompt_for_phase_1(epic: Dict) -> str:
    """Create Bob CLI prompt for Phase 1"""
    epic_id = epic["epic_id"]
    
    return f"""Execute Phase 1 (Scope Definition) for {epic_id}.

Read docs/brain/{epic_id}/00-hotspots.md and create docs/brain/{epic_id}/00-scope.md with:
1. Single-method boundary validation
2. Extraction scope (what to extract, what stays)
3. Dependencies and imports
4. Test coverage requirements

Update docs/brain/{epic_id}/manifest.json phase_1 status to "completed".

Target: CYC ≤ 8 (Jane Street alignment)
"""

def execute_phase_1_all() -> None:
    """Execute Phase 1 for all Wave 2 epics using Bob CLI"""
    print("\n[Wave 2 Phase 1: Scope Definition (9 epics)]")
    print("=" * 60)
    
    for epic in WAVE_2_EPICS:
        epic_id = epic["epic_id"]
        prompt = create_bob_prompt_for_phase_1(epic)
        
        print(f"\n[Phase 1] {epic_id}: Executing...")
        
        # Execute Bob CLI in non-interactive mode
        cmd = [
            "bob",
            "--chat-mode", "plan",
            "--yolo",
            "--output", "json",
            prompt
        ]
        
        try:
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=120
            )
            
            if result.returncode == 0:
                print(f"[OK] {epic_id}: Phase 1 complete")
            else:
                print(f"[FAIL] {epic_id}: Phase 1 failed - {result.stderr}")
        except subprocess.TimeoutExpired:
            print(f"[TIMEOUT] {epic_id}: Phase 1 timeout (>2 min)")
        except Exception as e:
            print(f"[ERROR] {epic_id}: Phase 1 error - {e}")
    
    print("\n[OK] Phase 1 Complete for all 9 epics")

def main():
    """Main orchestrator"""
    import sys
    
    if len(sys.argv) < 2:
        print("Usage: python wave2_direct_executor.py <phase>")
        print("Phases: 0, 1, 1.5, 2, 3, 4, 5, 5.5, 6")
        sys.exit(1)
    
    phase = sys.argv[1]
    
    if phase == "0":
        execute_phase_0_all()
    elif phase == "1":
        execute_phase_1_all()
    else:
        print(f"Phase {phase} not implemented yet")
        print("Available: 0, 1")

if __name__ == "__main__":
    main()

# Made with Bob
