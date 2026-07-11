#!/usr/bin/env python3
"""
Wave 2 Parallel Executor - Staggered Worktree-Based Parallel Execution
Each epic runs in its own worktree with 30-second staggered startup to avoid git notes race condition
"""

import json
import subprocess
import concurrent.futures
import time
from pathlib import Path
from typing import Dict, List, Tuple, Callable

# Worktree mapping for parallel execution
WORKTREE_MAP = {
    "EPIC-CCN-164": "C:/WSGTA/universal-or-epic-cluster-1",
    "EPIC-CCN-107": "C:/WSGTA/universal-or-epic-cluster-2",
    "EPIC-CCN-108": "C:/WSGTA/universal-or-epic-cluster-3",
    "EPIC-CCN-109": "C:/WSGTA/universal-or-epic-cluster-4",
    "EPIC-CCN-110": "C:/WSGTA/universal-or-epic-cluster-5",
    "EPIC-CCN-155": "C:/WSGTA/universal-or-epic-cluster-1",  # Reuse for Batch 2
    "EPIC-CCN-98": "C:/WSGTA/universal-or-epic-cluster-2",
    "EPIC-CCN-128": "C:/WSGTA/universal-or-epic-cluster-3",
    "EPIC-CCN-129": "C:/WSGTA/universal-or-epic-cluster-4",
}

# Wave 2 Epics - Split into batches for parallel execution
# Batch 1: First 5 epics (test batch)
WAVE_2_BATCH_1 = [
    {"epic_id": "EPIC-CCN-164", "method": "IsCommandForThisInstrument", "file": "src/V12_002.UI.IPC.cs", "cyc": 36},
    {"epic_id": "EPIC-CCN-107", "method": "HydrateFromOpenPositions", "file": "src/V12_002.SIMA.Lifecycle.cs", "cyc": 31},
    {"epic_id": "EPIC-CCN-108", "method": "SweepBrokerOrders", "file": "src/V12_002.SIMA.Lifecycle.cs", "cyc": 24},
    {"epic_id": "EPIC-CCN-109", "method": "HydrateWorkingOrdersFromBroker", "file": "src/V12_002.SIMA.Lifecycle.cs", "cyc": 19},
    {"epic_id": "EPIC-CCN-110", "method": "AdoptMasterOrders", "file": "src/V12_002.SIMA.Lifecycle.cs", "cyc": 19},
]

# Batch 2: Remaining 4 epics
WAVE_2_BATCH_2 = [
    {"epic_id": "EPIC-CCN-155", "method": "TryHandleFleetCommand", "file": "src/V12_002.UI.IPC.Commands.Fleet.cs", "cyc": 19},
    {"epic_id": "EPIC-CCN-98", "method": "ProcessFlattenWorkItem_CancelOrders", "file": "src/V12_002.SIMA.Flatten.cs", "cyc": 18},
    {"epic_id": "EPIC-CCN-128", "method": "SymmetryGuardReplaceExistingFollowerTarget", "file": "src/V12_002.Symmetry.Replace.cs", "cyc": 18},
    {"epic_id": "EPIC-CCN-129", "method": "SymmetryGuardTryResolveFollowersForDispatch", "file": "src/V12_002.Symmetry.Replace.cs", "cyc": 18},
]

# All epics combined
WAVE_2_EPICS = WAVE_2_BATCH_1 + WAVE_2_BATCH_2

def execute_bob_for_epic(epic: Dict, phase_num: str, phase_name: str, mode: str, prompt: str, timeout: int, worktree_path: str, startup_delay: int = 0) -> Tuple[str, bool, str]:
    """Execute Bob CLI for a single epic in its own worktree with staggered startup"""
    epic_id = epic["epic_id"]
    
    if startup_delay > 0:
        print(f"[Phase {phase_num}] {epic_id}: Waiting {startup_delay}s before start...")
        time.sleep(startup_delay)
    
    print(f"[Phase {phase_num}] {epic_id}: Starting in {worktree_path}...")
    
    # Bob is a PowerShell script on Windows
    cmd = [
        "powershell",
        "-Command",
        f"bob --chat-mode {mode} --yolo '{prompt}'"
    ]
    
    try:
        result = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            timeout=timeout,
            cwd=worktree_path  # Run in dedicated worktree
        )
        
        if result.returncode == 0:
            print(f"[OK] {epic_id}: Phase {phase_num} complete")
            return (epic_id, True, "Success")
        else:
            error_msg = result.stderr[:200] if result.stderr else "Unknown error"
            print(f"[FAIL] {epic_id}: Phase {phase_num} failed - {error_msg}")
            return (epic_id, False, error_msg)
    except subprocess.TimeoutExpired:
        print(f"[TIMEOUT] {epic_id}: Phase {phase_num} timeout (>{timeout}s)")
        return (epic_id, False, f"Timeout after {timeout}s")
    except Exception as e:
        print(f"[ERROR] {epic_id}: Phase {phase_num} error - {e}")
        return (epic_id, False, str(e))

def execute_phase_parallel(phase_num: str, phase_name: str, mode: str, prompt_template: Callable[[Dict], str], timeout: int, epics: List[Dict], max_workers: int = 5, stagger_delay: int = 60) -> Dict[str, bool]:
    """Execute a phase for epics in parallel using separate worktrees with staggered startup"""
    print(f"\n[Wave 2 Phase {phase_num}: {phase_name} ({len(epics)} parallel sessions)]")
    print("=" * 60)
    print(f"Worktree-based execution: Each Bob session runs in isolated worktree")
    print(f"Staggered startup: {stagger_delay}s delay between launches")
    print(f"Max parallel workers: {max_workers}")
    
    # Verify worktrees exist
    for epic in epics:
        epic_id = epic["epic_id"]
        worktree = WORKTREE_MAP.get(epic_id)
        if not worktree:
            print(f"[ERROR] No worktree mapped for {epic_id}")
            return {}
        if not Path(worktree).exists():
            print(f"[ERROR] Worktree does not exist: {worktree}")
            return {}
        print(f"  {epic_id} -> {worktree}")
    
    results = {}
    
    with concurrent.futures.ThreadPoolExecutor(max_workers=max_workers) as executor:
        # Submit all epic tasks with their worktrees and staggered delays
        futures = {}
        for i, epic in enumerate(epics):
            startup_delay = i * stagger_delay  # 0s, 30s, 60s, 90s, 120s
            future = executor.submit(
                execute_bob_for_epic,
                epic,
                phase_num,
                phase_name,
                mode,
                prompt_template(epic),
                timeout,
                WORKTREE_MAP[epic["epic_id"]],
                startup_delay
            )
            futures[future] = epic["epic_id"]
        
        # Wait for all to complete
        for future in concurrent.futures.as_completed(futures):
            epic_id, success, message = future.result()
            results[epic_id] = success
    
    # Summary
    success_count = sum(1 for s in results.values() if s)
    total = len(epics)
    print(f"\n[Phase {phase_num} Summary] {success_count}/{total} epics completed successfully")
    
    if success_count < total:
        print("Failed epics:")
        for epic_id, success in results.items():
            if not success:
                print(f"  - {epic_id}")
    
    return results

def phase_1_prompt(epic: Dict) -> str:
    """Generate Phase 1 prompt for an epic"""
    return f"""Execute Phase 1 (Scope Definition) for {epic["epic_id"]}.

Read docs/brain/{epic["epic_id"]}/00-hotspots.md and create docs/brain/{epic["epic_id"]}/00-scope.md with:

1. Single-method boundary validation (MANDATORY - V12.23 Protocol)
2. Extraction scope: what to extract from {epic["method"]}, what stays
3. Dependencies and imports analysis
4. Test coverage requirements

Create the scope document with these sections:
- Method Signature
- Current Complexity: {epic["cyc"]}
- Target Complexity: 8
- Extraction Candidates (sub-functions to extract)
- Boundary Validation (confirm single-method scope)
- Dependencies
- Test Strategy

Update docs/brain/{epic["epic_id"]}/manifest.json phase_1 status to "completed".

File: {epic["file"]}
Method: {epic["method"]}
Target: CYC <= 8 (Jane Street alignment)
"""

def phase_1_5_prompt(epic: Dict) -> str:
    """Generate Phase 1.5 prompt for an epic"""
    return f"""Execute Phase 1.5 (Scope Boundary Validation) for {epic["epic_id"]}.

Read docs/brain/{epic["epic_id"]}/00-scope.md and create docs/brain/{epic["epic_id"]}/01-scope-boundary.md.

MANDATORY VALIDATION (V12.23 Protocol):
- Confirm extraction stays within single-method boundary
- No cross-method dependencies
- No scope creep beyond {epic["method"]}

Create boundary validation document with:
- Boundary Confirmation (PASS/FAIL)
- Scope Violations (if any)
- Mitigation Plan (if violations found)

Update docs/brain/{epic["epic_id"]}/manifest.json phase_1_5 status to "completed".

This is a MANDATORY gate - epic cannot proceed if boundary validation fails.
"""

def phase_2_prompt(epic: Dict) -> str:
    """Generate Phase 2 prompt for an epic"""
    return f"""Execute Phase 2 (Architecture Planning) for {epic["epic_id"]}.

Read docs/brain/{epic["epic_id"]}/01-scope-boundary.md and create docs/brain/{epic["epic_id"]}/02-architecture-plan.md.

Create detailed extraction plan with:
1. Method signatures for extracted functions
2. Call graph (what calls what)
3. Jane Street compliance checks
4. Lock-free Actor pattern validation
5. ASCII-only compliance

Also create docs/brain/{epic["epic_id"]}/02-diagrams.mmd with Mermaid diagrams.

Update docs/brain/{epic["epic_id"]}/manifest.json phase_2 status to "completed".

Target: CYC <= 8 per function
"""

def phase_3_prompt(epic: Dict) -> str:
    """Generate Phase 3 prompt for an epic"""
    return f"""Execute Phase 3 (DNA & PR Audit) for {epic["epic_id"]}.

Read docs/brain/{epic["epic_id"]}/02-architecture-plan.md and create docs/brain/{epic["epic_id"]}/03-audit-report.md.

Run V12 DNA compliance checks:
1. Correctness by Construction validation
2. Lock-free Actor pattern compliance
3. ASCII-only compliance
4. PR hygiene validation (diff size, commit structure)
5. Pre-flight safety checks

Use jCodemunch MCP tools for blast radius analysis.

Update docs/brain/{epic["epic_id"]}/manifest.json phase_3 status to "completed".
"""

def phase_4_prompt(epic: Dict) -> str:
    """Generate Phase 4 prompt for an epic"""
    return f"""Execute Phase 4 (Ticket Generation) for {epic["epic_id"]}.

Read docs/brain/{epic["epic_id"]}/02-architecture-plan.md and create docs/brain/{epic["epic_id"]}/04-tickets.md.

Use jCodemunch to analyze {epic["method"]} complexity and generate surgical extraction tickets.

Each ticket should:
1. Extract one sub-function
2. Target CYC <= 8
3. Include before/after signatures
4. Specify test requirements

Update docs/brain/{epic["epic_id"]}/manifest.json phase_4 status to "completed".
"""

def phase_5_prompt(epic: Dict) -> str:
    """Generate Phase 5 prompt for an epic"""
    return f"""Execute Phase 5 (Ticket Execution) for {epic["epic_id"]}.

Read docs/brain/{epic["epic_id"]}/04-tickets.md and execute all tickets.

Delegate to Bob CLI (v12-engineer mode) for surgical extraction.

This orchestrates execution but Bob does the actual code work.

Create docs/brain/{epic["epic_id"]}/ticket-X-completion.md for each ticket.

Update docs/brain/{epic["epic_id"]}/manifest.json phase_5 status to "completed".
"""

def phase_5_5_prompt(epic: Dict) -> str:
    """Generate Phase 5.5 prompt for an epic"""
    return f"""Execute Phase 5.5 (Verification) for {epic["epic_id"]}.

Verify that ticket execution succeeded:
1. Complexity targets met (CYC <= 8)
2. All quality gates passed
3. Build passes
4. Tests pass

Create docs/brain/{epic["epic_id"]}/05-verification-report.md.

Update docs/brain/{epic["epic_id"]}/manifest.json phase_5_5 status to "completed".
"""

def phase_6_prompt(epic: Dict) -> str:
    """Generate Phase 6 prompt for an epic"""
    return f"""Execute Phase 6 (Final Review) for {epic["epic_id"]}.

Perform final review:
1. Generate completion report
2. Update roadmap with final status
3. Run deploy-sync.ps1
4. Verify F5 in NinjaTrader

Create docs/brain/{epic["epic_id"]}/06-completion-report.md.

Update docs/brain/{epic["epic_id"]}/manifest.json phase_6 status to "completed".
"""

def main():
    """Main orchestrator"""
    import sys
    
    if len(sys.argv) < 2:
        print("Usage: python wave2_parallel_executor.py <phase>")
        print("Phases: 1, 1.5, 2, 3, 4, 5, 5.5, 6, all")
        print("\nNote: Phase 0 is handled by wave2_direct_executor.py (no parallelization needed)")
        sys.exit(1)
    
    phase = sys.argv[1]
    
    phase_config = {
        "1": ("1", "Scope Definition", "plan", phase_1_prompt, 180),
        "1.5": ("1.5", "Scope Boundary", "plan", phase_1_5_prompt, 120),
        "2": ("2", "Architecture Planning", "plan", phase_2_prompt, 240),
        "3": ("3", "DNA & PR Audit", "advanced", phase_3_prompt, 240),
        "4": ("4", "Ticket Generation", "plan", phase_4_prompt, 180),
        "5": ("5", "Ticket Execution", "v12-engineer", phase_5_prompt, 600),
        "5.5": ("5.5", "Verification", "advanced", phase_5_5_prompt, 240),
        "6": ("6", "Final Review", "advanced", phase_6_prompt, 240),
    }
    
    # Determine which batch to run
    batch = sys.argv[2] if len(sys.argv) > 2 else "1"
    
    if batch == "1":
        epics = WAVE_2_BATCH_1
        print(f"\n[Running Batch 1: 5 epics]")
    elif batch == "2":
        epics = WAVE_2_BATCH_2
        print(f"\n[Running Batch 2: 4 epics]")
    elif batch == "all":
        epics = WAVE_2_EPICS
        print(f"\n[Running All: 9 epics]")
    else:
        print(f"Unknown batch: {batch}")
        print("Available batches: 1 (5 epics), 2 (4 epics), all (9 epics)")
        sys.exit(1)
    
    # Stagger delay from command line (default 60s)
    stagger = int(sys.argv[3]) if len(sys.argv) > 3 else 60
    
    if phase in phase_config:
        num, name, mode, prompt_fn, timeout = phase_config[phase]
        execute_phase_parallel(num, name, mode, prompt_fn, timeout, epics, max_workers=5, stagger_delay=stagger)
    elif phase == "all":
        print("\n" + "=" * 60)
        print(f"WAVE 2 FULL EXECUTION - ALL PHASES (PARALLEL, BATCH {batch})")
        print("=" * 60)
        for p in ["1", "1.5", "2", "3", "4", "5", "5.5", "6"]:
            num, name, mode, prompt_fn, timeout = phase_config[p]
            execute_phase_parallel(num, name, mode, prompt_fn, timeout, epics, max_workers=5, stagger_delay=stagger)
            print("\n" + "-" * 60)
        print("\n" + "=" * 60)
        print(f"WAVE 2 COMPLETE - BATCH {batch}")
        print("=" * 60)
    else:
        print(f"Phase {phase} not recognized")
        print("Available: 1, 1.5, 2, 3, 4, 5, 5.5, 6, all")

if __name__ == "__main__":
    main()

# Made with Bob
