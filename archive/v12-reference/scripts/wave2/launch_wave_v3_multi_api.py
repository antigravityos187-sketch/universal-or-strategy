#!/usr/bin/env python3
"""
V12 Wave 2 v3 - Multi-API Architecture (1 API per agent)
Fixes the single-API bottleneck from v2

Key Changes:
1. Loads 9 API keys from docs/API/*.json
2. Assigns 1 unique API key per agent
3. Each agent gets isolated bobcoin budget
4. Prevents API exhaustion

Usage:
    python scripts/wave2/launch_wave_v3_multi_api.py
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
MAX_COINS  = "200"
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


def load_api_keys() -> list[str]:
    """Load API keys from docs/API/*.json files."""
    api_keys = []
    for json_file in sorted(API_DIR.glob("*.json")):
        try:
            data = json.loads(json_file.read_text())
            if "apikey" in data:
                api_keys.append(data["apikey"])
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


def build_wave_script(epics: list[tuple[str, str, int]], api_keys: list[str]) -> str:
    """Generate orchestrator script with 1 API key per agent."""
    if len(api_keys) < len(epics):
        raise ValueError(f"Need {len(epics)} API keys, only have {len(api_keys)}")
    
    repo_abs = REPO
    logs_abs = f"{repo_abs}/logs"

    lines = [
        "#!/bin/bash",
        "# V12 Wave 2 v3 Orchestrator - Multi-API Architecture",
        f"# Generated: {datetime.now(timezone.utc).isoformat()}",
        f"# {len(epics)} agents, {len(api_keys)} API keys (1:1 mapping)",
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
        "echo '[WAVE2-V3] Launching parallel Bob Shell agents (MULTI-API)...'",
    ]

    for idx, (epic_id, method, cyc) in enumerate(epics):
        api_key = api_keys[idx]
        session  = f"v12-{epic_id}"
        log_path = f"{logs_abs}/{epic_id}.log"
        
        prompt = (
            f"Execute complete epic-intake workflow for {epic_id}: "
            f"Extract {method} (complexity {cyc} -> 8). "
            f"Run all phases: hotspot analysis, scope definition, scope boundary validation, "
            f"architecture planning, DNA audit, and ticket generation."
        )
        
        # KEY FIX: Export unique API key per agent
        cmd = (
            f"export BOBSHELL_API_KEY='{api_key}' && "
            f"cd {repo_abs} && "
            f"bob --accept-license --mode v12-epic-planner --max-coins {MAX_COINS} "
            f"-p '{prompt}' "
            f"> {log_path} 2>&1; "
            f"echo DONE_EXIT=$? >> {log_path}"
        )
        lines.append(f'screen -dmS {session} bash -l -c "{cmd}"')
        lines.append(f"echo '[WAVE2-V3] Launched: {epic_id} ({method}, CYC {cyc}) with API #{idx+1}'")
        lines.append("sleep 1")

    lines += [
        "",
        "sleep 2",
        f"echo '[WAVE2-V3] All {len(epics)} agents launched (MULTI-API MODE).'",
        f"echo '[WAVE2-V3] Each agent has dedicated API key + {MAX_COINS} bobcoins.'",
        "screen -ls || true",
        "",
        f"echo '[WAVE2-V3] Done. Logs: {logs_abs}'",
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
    print(f"\n[WAVE2-V3] Loaded {len(api_keys)} API keys from {API_DIR}")
    
    # Load epics
    epics = load_epics()
    print(f"[WAVE2-V3] Loaded {len(epics)} epics from {EPICS_FILE}")
    
    if len(api_keys) < len(epics):
        print(f"[ERROR] Need {len(epics)} API keys, only have {len(api_keys)}")
        sys.exit(1)
    
    print(f"\n[WAVE2-V3] Preparing MULTI-API execution:")
    print(f"[WAVE2-V3] - {len(epics)} agents")
    print(f"[WAVE2-V3] - {len(api_keys)} API keys (1:1 mapping)")
    print(f"[WAVE2-V3] - {MAX_COINS} bobcoins per agent")
    print(f"[WAVE2-V3] - Total budget: {int(MAX_COINS) * len(epics)} bobcoins\n")
    
    epic_summary = ', '.join(f"{eid} ({method}, CYC {cyc})" for eid, method, cyc in epics)
    print(f"[WAVE2-V3] Epics: {epic_summary}\n")

    # Build script
    script = build_wave_script(epics, api_keys)
    script_path = Path("scripts/wave2/_wave2_v3_launch_generated.sh")
    with open(script_path, 'w', newline='\n', encoding='utf-8') as f:
        f.write(script)
    print(f"[WAVE2-V3] Generated orchestrator: {script_path}")
    
    # Verify LF-only
    raw = script_path.read_bytes()
    crlf_count = raw.count(b'\r\n')
    if crlf_count > 0:
        print(f"[ERROR] Script has {crlf_count} CRLF sequences! Aborting.")
        raise RuntimeError("CRLF detected")
    print(f"[WAVE2-V3] Line ending check: OK (LF only, {len(script.splitlines())} lines)")

    # Clear SSH key cache
    clear_stale_ssh_key()

    # Upload
    remote_path = f"/tmp/wave2_v3_launch.sh"
    print(f"\n[WAVE2-V3] Uploading script to {VM_NAME}:{remote_path}...")
    gcloud(
        "compute", "scp",
        str(script_path),
        f"{VM_NAME}:{remote_path}",
        f"--zone={ZONE}",
        f"--project={PROJECT}",
    )
    print("[WAVE2-V3] Upload complete.")

    # Execute
    print(f"\n[WAVE2-V3] Executing on VM...")
    gcloud(
        "compute", "ssh", VM_NAME,
        f"--zone={ZONE}",
        f"--project={PROJECT}",
        f"--command=bash {remote_path}",
    )

    print("\n[WAVE2-V3] [OK] Launch complete!")
    print(f"[WAVE2-V3] Expected: 30-60 minutes for all {len(epics)} epics")
    print(f"[WAVE2-V3] Each agent has isolated API key (no shared quota)")
    print(f"[WAVE2-V3] Monitor: gcloud compute ssh {VM_NAME} --zone={ZONE} --command='screen -ls'")


if __name__ == "__main__":
    main()

# Made with Bob
