#!/usr/bin/env python3
"""
V12 Wave 2 v4 - Safe Budget with Bobcoin Tracking
Fixes budget overflow from v3 by reducing per-epic budget to 150 bobcoins

Key Changes from v3:
1. MAX_COINS reduced from 200 to 150 (safe budget)
2. Total budget: 1,350 bobcoins (9 epics × 150)
3. Available: 1,600 bobcoins (10 fresh APIs)
4. Safety margin: 250 bobcoins (15.6%)
5. Includes bobcoin tracking protocol

Usage:
    python scripts/wave2/launch_wave_v4_safe_budget.py
"""

import subprocess
import sys
import json
from pathlib import Path
from datetime import datetime, timezone

# ── Config ───────────────────────────────────────────────────────────────────
VM_NAME    = "v12-test-golden-v2"
ZONE       = "us-central1-a"
PROJECT    = "project-14c86305-3cba-493f-a73"
RUN_USER   = "malhitticrypto"
REPO       = f"/home/{RUN_USER}/universal-or-strategy"
MAX_COINS  = "150"  # SAFE BUDGET - prevents negative balances
SAFETY_BUFFER = 10  # Keep 10 bobcoins minimum per API
# ─────────────────────────────────────────────────────────────────────────────

EPICS_FILE = Path(__file__).parent / "wave2_epics_v2.txt"
API_DIR = Path("docs/API")


def gcloud(*args, check=True) -> subprocess.CompletedProcess:
    gcloud_exe = r"C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd"
    cmd = [gcloud_exe, *args]
    print(f"[CMD] {' '.join(cmd)}")
    return subprocess.run(cmd, check=check, capture_output=False, text=True, shell=True)


def gcloud_capture(*args) -> str:
    gcloud_exe = r"C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd"
    cmd = [gcloud_exe, *args]
    result = subprocess.run(cmd, check=False, capture_output=True, text=True, shell=True)
    return result.stdout + result.stderr


def load_api_keys() -> list[tuple[str, str]]:
    """Load API keys from docs/API/*.json files. Returns list of (filename, apikey) tuples."""
    api_keys = []
    for json_file in sorted(API_DIR.glob("*.json")):
        try:
            data = json.loads(json_file.read_text())
            if "apikey" in data:
                api_keys.append((json_file.name, data["apikey"]))
                print(f"[API] Loaded key from {json_file.name}")
        except Exception as e:
            print(f"[WARN] Failed to load {json_file}: {e}")
    return api_keys


def load_epics() -> list[tuple[str, str, int]]:
    """Load epics from file. Returns list of (epic_id, method_name, complexity) tuples."""
    epics = []
    for line in EPICS_FILE.read_text().splitlines():
        line = line.strip()
        if not line or line.startswith("#"):
            continue
        parts = line.split("|")
        if len(parts) == 3:
            epic_id, method, cyc = parts
            epics.append((epic_id.strip(), method.strip(), int(cyc.strip())))
    return epics


def build_wave_script(epics: list[tuple[str, str, int]], api_keys: list[tuple[str, str]]) -> str:
    """Generate orchestrator script with 1 API key per agent + budget tracking."""
    if len(api_keys) < len(epics):
        raise ValueError(f"Need {len(epics)} API keys, only have {len(api_keys)}")
    
    # Calculate budget metrics
    total_budget = int(MAX_COINS) * len(epics)
    total_available = 160 * len(api_keys)
    safety_margin = total_available - total_budget
    safety_pct = (safety_margin / total_available) * 100
    
    repo_abs = REPO
    logs_abs = f"{repo_abs}/logs"

    lines = [
        "#!/bin/bash",
        "# V12 Wave 2 v4 Orchestrator - Safe Budget with Tracking",
        f"# Generated: {datetime.now(timezone.utc).isoformat()}",
        f"# {len(epics)} agents, {len(api_keys)} API keys (1:1 mapping)",
        f"# Budget: {MAX_COINS} bobcoins/epic × {len(epics)} = {total_budget} bobcoins",
        f"# Available: {total_available} bobcoins ({len(api_keys)} APIs × 160)",
        f"# Safety Margin: {safety_margin} bobcoins ({safety_pct:.1f}%)",
        "",
        "# Global git identity",
        f'git config --global user.email "malhitticrypto@gmail.com"',
        f'git config --global user.name "malhitticrypto"',
        "",
        f"mkdir -p {logs_abs}",
        "",
        "# Pull latest repo",
        f"cd {repo_abs} && git pull --ff-only origin main || true",
        "",
        "echo '[WAVE2-V4] Launching parallel Bob Shell agents (SAFE BUDGET)...'",
        f"echo '[WAVE2-V4] Budget: {MAX_COINS} bobcoins per epic'",
        f"echo '[WAVE2-V4] Total: {total_budget} / {total_available} bobcoins ({safety_pct:.1f}% safety margin)'",
        "",
    ]

    # API allocation mapping
    api_allocation = []
    for idx, (epic_id, method, cyc) in enumerate(epics):
        api_filename, api_key = api_keys[idx]
        api_allocation.append(f"# {epic_id} → API #{idx+1} ({api_filename})")
        
        session  = f"v12-{epic_id}"
        log_path = f"{logs_abs}/{epic_id}.log"
        
        prompt = (
            f"Execute complete epic-intake workflow for {epic_id}: "
            f"Extract {method} (complexity {cyc} -> 8). "
            f"Run all phases: hotspot analysis, scope definition, scope boundary validation, "
            f"architecture planning, DNA audit, and ticket generation."
        )
        
        # Export unique API key per agent with safe budget
        cmd = (
            f"export BOBSHELL_API_KEY='{api_key}' && "
            f"cd {repo_abs} && "
            f"bob --accept-license --chat-mode plan --max-coins {MAX_COINS} "
            f"-p '{prompt}' "
            f"> {log_path} 2>&1; "
            f"echo DONE_EXIT=$? >> {log_path}"
        )
        lines.append(f'screen -dmS {session} bash -l -c "{cmd}"')
        lines.append(f"echo '[WAVE2-V4] Launched: {epic_id} ({method}, CYC {cyc}) with {api_filename} (150 bobcoins)'")
        lines.append("sleep 1")

    lines += [
        "",
        "sleep 2",
        f"echo '[WAVE2-V4] All {len(epics)} agents launched (SAFE BUDGET MODE).'",
        f"echo '[WAVE2-V4] Each agent: dedicated API + {MAX_COINS} bobcoins.'",
        f"echo '[WAVE2-V4] Reserve: {safety_margin} bobcoins ({safety_pct:.1f}% safety margin).'",
        "screen -ls || true",
        "",
        "# API Allocation Summary",
    ] + api_allocation + [
        "",
        f"echo '[WAVE2-V4] Done. Logs: {logs_abs}'",
        "echo '[WAVE2-V4] Monitor bobcoin usage to prevent negatives!'",
    ]

    return "\n".join(lines)


def clear_stale_ssh_key():
    """Remove stale Plink key cache entry for the VM's IP."""
    result = gcloud_capture(
        "compute", "instances", "describe", VM_NAME,
        f"--zone={ZONE}", f"--project={PROJECT}",
        "--format=get(networkInterfaces[0].accessConfigs[0].natIP)"
    )
    ip = result.strip().split("\n")[0].strip()
    if not ip:
        print("[WARN] Could not get VM IP for key cache cleanup.")
        return

    print(f"[INFO] VM IP: {ip} — clearing Plink SSH host key cache...")
    import winreg
    try:
        key = winreg.OpenKey(
            winreg.HKEY_CURRENT_USER,
            r"Software\SimonTatham\PuTTY\SshHostKeys",
            0, winreg.KEY_ALL_ACCESS
        )
        vals = []
        try:
            i = 0
            while True:
                name, _, _ = winreg.EnumValue(key, i)
                if ip in name:
                    vals.append(name)
                i += 1
        except OSError:
            pass
        for name in vals:
            winreg.DeleteValue(key, name)
            print(f"[INFO] Removed stale key: {name}")
        winreg.CloseKey(key)
    except Exception as e:
        print(f"[WARN] Could not clear key cache: {e}")


def main():
    # Load API keys
    api_keys = load_api_keys()
    print(f"\n[WAVE2-V4] Loaded {len(api_keys)} API keys from {API_DIR}")
    
    # Load epics
    epics = load_epics()
    print(f"[WAVE2-V4] Loaded {len(epics)} epics from {EPICS_FILE}")
    
    if len(api_keys) < len(epics):
        print(f"[ERROR] Need {len(epics)} API keys, only have {len(api_keys)}")
        sys.exit(1)
    
    # Budget validation
    total_budget = int(MAX_COINS) * len(epics)
    total_available = 160 * len(api_keys)
    safety_margin = total_available - total_budget
    safety_pct = (safety_margin / total_available) * 100
    
    print(f"\n[WAVE2-V4] Budget Analysis:")
    print(f"[WAVE2-V4] - Per-Epic Budget: {MAX_COINS} bobcoins")
    print(f"[WAVE2-V4] - Total Budget: {total_budget} bobcoins ({len(epics)} epics)")
    print(f"[WAVE2-V4] - Total Available: {total_available} bobcoins ({len(api_keys)} APIs × 160)")
    print(f"[WAVE2-V4] - Safety Margin: {safety_margin} bobcoins ({safety_pct:.1f}%)")
    
    if safety_pct < 10:
        print(f"[WARN] Safety margin below 10%! Consider reducing epic count or per-epic budget.")
    
    print(f"\n[WAVE2-V4] API Allocation:")
    for idx, (epic_id, method, cyc) in enumerate(epics):
        api_filename, _ = api_keys[idx]
        print(f"[WAVE2-V4] - {epic_id} ({method}, CYC {cyc}) -> {api_filename}")
    
    reserve_api = api_keys[len(epics)][0] if len(api_keys) > len(epics) else "None"
    print(f"[WAVE2-V4] - RESERVE: {reserve_api}\n")

    # Build script
    script = build_wave_script(epics, api_keys)
    script_path = Path("scripts/wave2/_wave2_v4_launch_generated.sh")
    with open(script_path, 'w', newline='\n', encoding='utf-8') as f:
        f.write(script)
    print(f"[WAVE2-V4] Generated orchestrator: {script_path}")
    
    # Verify LF-only
    raw = script_path.read_bytes()
    crlf_count = raw.count(b'\r\n')
    if crlf_count > 0:
        print(f"[ERROR] Script has {crlf_count} CRLF sequences! Aborting.")
        raise RuntimeError("CRLF detected")
    print(f"[WAVE2-V4] Line ending check: OK (LF only, {len(script.splitlines())} lines)")

    # Clear SSH key cache
    clear_stale_ssh_key()

    # Upload
    remote_path = f"/tmp/wave2_v4_launch.sh"
    print(f"\n[WAVE2-V4] Uploading script to {VM_NAME}:{remote_path}...")
    gcloud(
        "compute", "scp",
        str(script_path),
        f"{VM_NAME}:{remote_path}",
        f"--zone={ZONE}",
        f"--project={PROJECT}",
    )
    print("[WAVE2-V4] Upload complete.")

    # Execute
    print(f"\n[WAVE2-V4] Executing on VM...")
    gcloud(
        "compute", "ssh", VM_NAME,
        f"--zone={ZONE}",
        f"--project={PROJECT}",
        f"--command=bash {remote_path}",
    )

    print("\n[WAVE2-V4] [OK] Launch complete!")
    print(f"[WAVE2-V4] Expected: 30-60 minutes for all {len(epics)} epics")
    print(f"[WAVE2-V4] Budget: {total_budget} / {total_available} bobcoins ({safety_pct:.1f}% safety margin)")
    print(f"[WAVE2-V4] Each agent has isolated API key (no shared quota)")
    print(f"[WAVE2-V4] Monitor: gcloud compute ssh {VM_NAME} --zone={ZONE} --command='screen -ls'")
    print(f"\n[WAVE2-V4] WARNING: Monitor bobcoin usage to prevent negative balances!")
    print(f"[WAVE2-V4] Update docs/workflow/API_BUDGET_TRACKING.md after completion.")


if __name__ == "__main__":
    main()

# Made with Bob