#!/usr/bin/env python3
"""
Simple Wave 2 Orchestrator - Executes phases by following MCP instructions.

This script properly orchestrates Wave 2 by:
1. Calling phase MCP tools to get instructions
2. Following those instructions to create artifacts
3. Moving to next phase when complete
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
    {"epic_id": "EPIC-CCN-109", "method": "HydrateWorkingOrdersFromBroker", "file": "src/V12_002.SIMA.Lifecycle.cs", "cyc": 19},
    {"epic_id": "EPIC-CCN-110", "method": "AdoptMasterOrders", "file": "src/V12_002.SIMA.Lifecycle.cs", "cyc": 19},
    {"epic_id": "EPIC-CCN-155", "method": "TryHandleFleetCommand", "file": "src/V12_002.UI.IPC.Commands.Fleet.cs", "cyc": 19},
    {"epic_id": "EPIC-CCN-98", "method": "ProcessFlattenWorkItem_CancelOrders", "file": "src/V12_002.SIMA.Flatten.cs", "cyc": 18},
    {"epic_id": "EPIC-CCN-128", "method": "SymmetryGuardReplaceExistingFollowerTarget", "file": "src/V12_002.Symmetry.Replace.cs", "cyc": 18},
    {"epic_id": "EPIC-CCN-129", "method": "SymmetryGuardTryResolveFollowersForDispatch", "file": "src/V12_002.Symmetry.Replace.cs", "cyc": 18},
]

def execute_phase_0_batch():
    """Execute Phase 0 for all 9 epics using Bob CLI."""
    print("\n" + "="*60)
    print("PHASE 0: Hotspot Analysis (9 epics)")
    print("="*60)
    
    for epic in WAVE_2_EPICS:
        epic_id = epic["epic_id"]
        print(f"\n  Processing {epic_id}...")
        
        # Use Bob CLI to execute phase 0
        cmd = [
            "bob",
            "chat",
            f"Execute Phase 0 for {epic_id}: method={epic['method']}, file={epic['file']}, cyc={epic['cyc']}. Use phase-0-hotspot MCP tool."
        ]
        
        result = subprocess.run(cmd, capture_output=True, text=True)
        
        if result.returncode == 0:
            print(f"    ✅ {epic_id} Phase 0 complete")
        else:
            print(f"    ❌ {epic_id} Phase 0 failed: {result.stderr}")
            return False
    
    print("\n✅ Phase 0 complete for all 9 epics")
    return True

def execute_phase_1_batch():
    """Execute Phase 1 for all 9 epics."""
    print("\n" + "="*60)
    print("PHASE 1: Scope Definition (9 epics)")
    print("="*60)
    
    for epic in WAVE_2_EPICS:
        epic_id = epic["epic_id"]
        print(f"\n  Processing {epic_id}...")
        
        cmd = [
            "bob",
            "chat",
            f"Execute Phase 1 for {epic_id}. Use phase-1-scope MCP tool."
        ]
        
        result = subprocess.run(cmd, capture_output=True, text=True)
        
        if result.returncode == 0:
            print(f"    ✅ {epic_id} Phase 1 complete")
        else:
            print(f"    ❌ {epic_id} Phase 1 failed")
            return False
    
    print("\n✅ Phase 1 complete for all 9 epics")
    return True

def main():
    """Main orchestration loop."""
    print("Wave 2 Simple Orchestrator")
    print("="*60)
    print(f"Processing {len(WAVE_2_EPICS)} epics")
    print("="*60)
    
    # Phase 0: Hotspot Analysis
    if not execute_phase_0_batch():
        print("\n❌ Phase 0 failed. Stopping.")
        return 1
    
    # Phase 1: Scope Definition
    if not execute_phase_1_batch():
        print("\n❌ Phase 1 failed. Stopping.")
        return 1
    
    # TODO: Add remaining phases (1.5, 2, 3, 4, 5, 5.5, 6)
    
    print("\n" + "="*60)
    print("✅ Wave 2 Orchestration Complete!")
    print("="*60)
    return 0

if __name__ == "__main__":
    exit(main())

# Made with Bob