#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Wave 2 Bob Shell Executor - Uses Bob Shell API mode for parallel execution
Correct syntax: bob --chat-mode v12-engineer -p "prompt"
"""

import subprocess
import json
import os
import sys
import time
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor, as_completed

# Fix Windows console encoding for emoji
if sys.platform == 'win32':
    sys.stdout.reconfigure(encoding='utf-8')

# Wave 2 epics (6 remaining - Phase 1 complete: EPIC-CCN-32, 164, 107, 108)
WAVE_2_EPICS = [
    {"id": "EPIC-CCN-109", "method": "HydrateWorkingOrdersFromBroker", "file": "src/V12_002.SIMA.Lifecycle.cs", "cyc": 19},
    {"id": "EPIC-CCN-110", "method": "AdoptMasterOrders", "file": "src/V12_002.SIMA.Lifecycle.cs", "cyc": 19},
    {"id": "EPIC-CCN-155", "method": "TryHandleFleetCommand", "file": "src/V12_002.UI.IPC.Commands.Fleet.cs", "cyc": 19},
    {"id": "EPIC-CCN-98", "method": "ProcessFlattenWorkItem_CancelOrders", "file": "src/V12_002.SIMA.Flatten.cs", "cyc": 18},
    {"id": "EPIC-CCN-128", "method": "SymmetryGuardReplaceExistingFollowerTarget", "file": "src/V12_002.Symmetry.Replace.cs", "cyc": 18},
    {"id": "EPIC-CCN-129", "method": "SymmetryGuardTryResolveFollowersForDispatch", "file": "src/V12_002.Symmetry.Replace.cs", "cyc": 18},
]

def execute_phase_with_bob_shell(epic_id: str, phase: int, phase_name: str) -> dict:
    """Execute a phase using Bob Shell API mode (non-interactive)"""
    
    # Read Phase 0 hotspot analysis
    hotspot_file = Path(f"docs/brain/{epic_id}/00-hotspots.md")
    if not hotspot_file.exists():
        return {"success": False, "error": f"Phase 0 not complete for {epic_id}"}
    
    hotspot_content = hotspot_file.read_text()
    
    # Create prompt for Bob Shell
    prompt = f"""Execute Phase {phase} ({phase_name}) for {epic_id}.

Context from Phase 0:
{hotspot_content}

Instructions:
1. Read the Phase 0 analysis above
2. Execute Phase {phase} according to V12 Epic Workflow
3. Create output artifact at docs/brain/{epic_id}/0{phase}-{phase_name.lower().replace(' ', '-')}.md
4. Update manifest.json with phase status

Follow V12 DNA principles and Jane Street alignment.
"""
    
    # Execute Bob Shell in API mode (non-interactive via -p flag)
    try:
        # Use bob.cmd on Windows (npm global install)
        bob_cmd = "bob.cmd" if sys.platform == 'win32' else "bob"
        
        # Set environment with UTF-8 encoding
        env = os.environ.copy()
        env["BOBSHELL_API_KEY"] = os.environ.get("BOBSHELL_API_KEY", "")
        env["PYTHONIOENCODING"] = "utf-8"
        
        # Use Popen with explicit binary mode to avoid encoding issues
        process = subprocess.Popen(
            [
                bob_cmd,
                "--chat-mode", "v12-engineer",
                "--yolo",  # Enable file modifications
                "-p", prompt
            ],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            env=env
        )
        
        # Wait with timeout and decode manually
        try:
            stdout_bytes, stderr_bytes = process.communicate(timeout=600)
            stdout = stdout_bytes.decode('utf-8', errors='replace')
            stderr = stderr_bytes.decode('utf-8', errors='replace')
            returncode = process.returncode
        except subprocess.TimeoutExpired:
            process.kill()
            return {"success": False, "epic_id": epic_id, "phase": phase, "error": "Timeout (10 minutes)"}
        
        return {
            "success": returncode == 0,
            "epic_id": epic_id,
            "phase": phase,
            "stdout": stdout,
            "stderr": stderr,
            "returncode": returncode
        }
    except Exception as e:
        return {"success": False, "epic_id": epic_id, "phase": phase, "error": str(e)}

def execute_phase_parallel(phase: int, phase_name: str, max_workers: int = 3):
    """Execute a phase for all epics in parallel"""
    print(f"\n{'='*80}")
    print(f"Phase {phase}: {phase_name}")
    print(f"{'='*80}\n")
    
    results = []
    with ThreadPoolExecutor(max_workers=max_workers) as executor:
        futures = {
            executor.submit(execute_phase_with_bob_shell, epic["id"], phase, phase_name): epic
            for epic in WAVE_2_EPICS
        }
        
        for future in as_completed(futures):
            epic = futures[future]
            try:
                result = future.result()
                results.append(result)
                
                if result["success"]:
                    print(f"✅ {epic['id']}: Phase {phase} complete")
                else:
                    error_msg = result.get('error', result.get('stderr', 'Unknown error'))
                    print(f"❌ {epic['id']}: Phase {phase} failed - {error_msg}")
            except Exception as e:
                print(f"❌ {epic['id']}: Exception - {str(e)}")
                results.append({"success": False, "epic_id": epic["id"], "phase": phase, "error": str(e)})
    
    return results

def main():
    """Execute all phases for Wave 2 epics"""
    
    # Check for API key
    if not os.environ.get("BOBSHELL_API_KEY"):
        print("❌ Error: BOBSHELL_API_KEY environment variable not set")
        print("\nSet it with:")
        print('  $env:BOBSHELL_API_KEY="your-api-key-here"')
        return
    
    print("🚀 Wave 2 Bob Shell Executor")
    print(f"📊 Processing {len(WAVE_2_EPICS)} epics")
    api_key = os.environ.get('BOBSHELL_API_KEY', '')
    print(f"🔑 API Key: {api_key[:20] if api_key else 'NOT SET'}...")
    
    phases = [
        (1, "Scope Definition"),
        (1.5, "Scope Boundary"),
        (2, "Architecture Planning"),
        (3, "DNA & PR Audit"),
        (4, "Ticket Generation"),
        (5, "Ticket Execution"),
        (5.5, "Verification"),
        (6, "Final Review"),
    ]
    
    all_results = {}
    
    for phase_num, phase_name in phases:
        results = execute_phase_parallel(phase_num, phase_name, max_workers=3)
        all_results[f"phase_{phase_num}"] = results
        
        # Check if all succeeded before moving to next phase
        failures = [r for r in results if not r["success"]]
        successes = [r for r in results if r["success"]]
        
        print(f"\n📈 Phase {phase_num} Summary: {len(successes)}/{len(WAVE_2_EPICS)} succeeded")
        
        if failures:
            print(f"⚠️  {len(failures)} failures:")
            for f in failures:
                print(f"   - {f['epic_id']}: {f.get('error', 'Unknown')}")
            
            # Auto-continue on failures (no user input needed)
            print("\n⚠️  Continuing to next phase despite failures...")
        
        # Brief pause between phases
        time.sleep(5)
    
    # Save results
    results_file = Path("docs/workflow/WAVE_2_BOB_SHELL_RESULTS.json")
    results_file.write_text(json.dumps(all_results, indent=2))
    print(f"\n✅ Results saved to {results_file}")
    
    # Summary
    total_phases = len(all_results)
    total_tasks = sum(len(results) for results in all_results.values())
    total_successes = sum(len([r for r in results if r["success"]]) for results in all_results.values())
    
    print(f"\n{'='*80}")
    print(f"📊 Wave 2 Execution Summary")
    print(f"{'='*80}")
    print(f"Phases executed: {total_phases}")
    print(f"Total tasks: {total_tasks}")
    print(f"Successes: {total_successes}")
    print(f"Failures: {total_tasks - total_successes}")
    print(f"Success rate: {(total_successes/total_tasks*100):.1f}%")

if __name__ == "__main__":
    main()

# Made with Bob
