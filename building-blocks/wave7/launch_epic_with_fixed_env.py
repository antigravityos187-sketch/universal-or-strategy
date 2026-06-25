#!/usr/bin/env python3
"""
Universal Epic Launcher with Fixed Environment

LONG-TERM SYSTEM FIX for subprocess PATH inheritance issue.
Use this template for all future wave execution scripts.

ROOT CAUSE: subprocess.Popen inherits broken PATH from parent process
SOLUTION: Explicitly set PATH in subprocess environment

Usage:
    from launch_epic_with_fixed_env import launch_epic
    launch_epic(script_path, log_path)
"""

import os
import subprocess
from pathlib import Path


def get_fixed_environment():
    """
    Create a fixed environment with proper PATH.
    
    Returns:
        dict: Environment variables with corrected PATH
    """
    env = os.environ.copy()
    
    # Ensure critical system directories are in PATH
    system_paths = [
        "/usr/bin",
        "/bin",
        "/usr/local/bin",
        "/usr/sbin",
        "/sbin"
    ]
    
    # Get existing PATH
    existing_path = env.get("PATH", "")
    
    # Prepend system paths (ensures they take priority)
    new_path = ":".join(system_paths)
    if existing_path:
        new_path = f"{new_path}:{existing_path}"
    
    env["PATH"] = new_path
    
    return env


def launch_epic(script_path, log_path, working_dir=None):
    """
    Launch an epic script with fixed environment.
    
    Args:
        script_path (str): Path to the bash script to execute
        log_path (str): Path to the log file
        working_dir (str, optional): Working directory for execution
        
    Returns:
        subprocess.Popen: The launched process
    """
    # Ensure log directory exists
    log_dir = Path(log_path).parent
    log_dir.mkdir(parents=True, exist_ok=True)
    
    # Get fixed environment
    env = get_fixed_environment()
    
    # Launch process
    with open(log_path, 'w') as log:
        proc = subprocess.Popen(
            ['/usr/bin/bash', script_path],
            stdout=log,
            stderr=subprocess.STDOUT,
            env=env,
            cwd=working_dir
        )
    
    return proc


def launch_epic_batch(epic_scripts, log_dir, stagger_seconds=5, working_dir=None):
    """
    Launch multiple epics in parallel with staggered start times.
    
    Args:
        epic_scripts (list): List of (epic_id, script_path) tuples
        log_dir (str): Directory for log files
        stagger_seconds (int): Seconds between launches
        working_dir (str, optional): Working directory for execution
        
    Returns:
        list: List of (epic_id, pid) tuples
    """
    import time
    
    pids = []
    
    for i, (epic_id, script_path) in enumerate(epic_scripts):
        log_path = f"{log_dir}/{epic_id}.log"
        proc = launch_epic(script_path, log_path, working_dir)
        pids.append((epic_id, proc.pid))
        
        # Stagger launches
        if i < len(epic_scripts) - 1:
            time.sleep(stagger_seconds)
    
    return pids


if __name__ == "__main__":
    # Example usage
    print("Universal Epic Launcher with Fixed Environment")
    print("=" * 60)
    print()
    print("This module provides:")
    print("  - get_fixed_environment(): Returns env dict with corrected PATH")
    print("  - launch_epic(): Launch single epic with fixed env")
    print("  - launch_epic_batch(): Launch multiple epics in parallel")
    print()
    print("Import this module in your wave execution scripts:")
    print("  from launch_epic_with_fixed_env import launch_epic")
    print()
    print("PATH Fix Details:")
    env = get_fixed_environment()
    print(f"  Fixed PATH: {env['PATH'][:100]}...")

# Made with Bob
