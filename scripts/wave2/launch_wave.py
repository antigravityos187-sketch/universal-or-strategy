#!/usr/bin/env python3
"""
V12 Wave 2 Launcher - Windows-compatible, zero quote escaping issues
Uses GCP startup-script metadata approach to inject and run orchestrator on VM.

Usage:
    python scripts/wave2/launch_wave.py --wave 2 --epics EPIC-CCN-164,EPIC-CCN-107,...
    python scripts/wave2/launch_wave.py --wave 2 --epics-file scripts/wave2/wave2_epics.txt

Requirements:
    pip install google-cloud-compute google-auth
"""

import argparse
import subprocess
import sys
import time
import json
from pathlib import Path
from datetime import datetime, timezone

# ── Configuration ────────────────────────────────────────────────────────────
PROJECT_ID    = "project-14c86305-3cba-493f-a73"
ZONE          = "us-central1-a"
GOLDEN_IMAGE  = "v12-bob-shell-golden-v3"  # v3 has BOBSHELL_API_KEY pre-baked in ~/.profile
MACHINE_TYPE  = "n2-standard-8"
DISK_SIZE_GB  = 50
RUN_USER      = "malhitticrypto"
REPO_URL      = "https://github.com/malhitticrypto-debug/universal-or-strategy.git"
MAX_COINS     = "50"
# ─────────────────────────────────────────────────────────────────────────────

ORCHESTRATOR_SCRIPT = Path(__file__).parent / "orchestrator.sh"


def run(cmd: list[str], check=True, capture=False) -> subprocess.CompletedProcess:
    """Run a subprocess command. Paths with spaces are handled via list args."""
    print(f"[CMD] {' '.join(cmd)}")
    return subprocess.run(
        cmd,
        check=check,
        capture_output=capture,
        text=True,
    )


def gcloud(*args, capture=False) -> subprocess.CompletedProcess:
    return run(["gcloud", *args], capture=capture)


def get_bob_api_key() -> str:
    """Read Bob API key from local .env or environment variable.
    
    NOTE: Golden image v3 already has BOBSHELL_API_KEY baked into ~/.profile.
    This function is kept for backward compatibility / override scenarios only.
    """
    import os
    key = os.environ.get("BOB_API_KEY", "")
    if not key:
        env_file = Path(__file__).parent.parent.parent / ".env"
        if env_file.exists():
            for line in env_file.read_text().splitlines():
                if line.startswith("BOB_API_KEY="):
                    key = line.split("=", 1)[1].strip().strip('"').strip("'")
                    break
    return key


def launch_wave(wave_num: int, epics: list[str], dry_run: bool = False) -> str:
    """Launch a wave VM with orchestrator via startup script metadata."""
    ts = datetime.now(timezone.utc).strftime("%Y%m%d-%H%M%S")
    vm_name = f"v12-wave{wave_num}-{ts}"

    orchestrator_script = ORCHESTRATOR_SCRIPT.read_text()
    epics_csv = ",".join(epics)
    bob_api_key = get_bob_api_key()  # optional override

    # Build metadata dict - no quote escaping needed, gcloud handles encoding
    # Golden image v3 already has BOBSHELL_API_KEY in ~/.profile - no injection needed
    metadata_items = [
        f"v12-repo-url={REPO_URL}",
        f"v12-epics={epics_csv}",
        f"v12-max-coins={MAX_COINS}",
        f"v12-run-user={RUN_USER}",
        f"startup-script={orchestrator_script}",   # KEY: script runs at boot
    ]
    if bob_api_key:
        # Override: inject a different key via metadata (useful for testing)
        metadata_items.append(f"v12-bob-api-key={bob_api_key}")
        print("[INFO] BOB_API_KEY override found - injecting via metadata.")
    else:
        print("[INFO] Using BOBSHELL_API_KEY pre-baked in golden image v3.")

    metadata_str = ",".join(metadata_items)

    print(f"\n[WAVE {wave_num}] Launching VM: {vm_name}")
    print(f"[WAVE {wave_num}] Epics ({len(epics)}): {epics_csv}")
    print(f"[WAVE {wave_num}] Image: {GOLDEN_IMAGE}")

    if dry_run:
        print("[DRY RUN] Would run:")
        print(f"  gcloud compute instances create {vm_name} ...")
        return vm_name

    # Create VM - the startup-script metadata key is the magic
    # GCP automatically runs it as root at first boot
    gcloud(
        "compute", "instances", "create", vm_name,
        f"--project={PROJECT_ID}",
        f"--zone={ZONE}",
        f"--machine-type={MACHINE_TYPE}",
        f"--image={GOLDEN_IMAGE}",
        "--image-project=project-14c86305-3cba-493f-a73",
        f"--boot-disk-size={DISK_SIZE_GB}GB",
        "--provisioning-model=SPOT",
        "--instance-termination-action=DELETE",
        "--scopes=cloud-platform",
        f"--metadata={metadata_str}",
        "--format=json",
    )

    print(f"[WAVE {wave_num}] VM {vm_name} created. Startup script running...")
    print(f"[WAVE {wave_num}] Monitor with: python scripts/wave2/launch_wave.py --monitor {vm_name}")
    return vm_name


def monitor_wave(vm_name: str):
    """Poll VM for orchestrator status and agent progress."""
    print(f"\n[MONITOR] Watching VM: {vm_name}")
    for attempt in range(60):  # 30 minutes max
        time.sleep(30)
        try:
            result = gcloud(
                "compute", "ssh", vm_name,
                f"--zone={ZONE}",
                f"--project={PROJECT_ID}",
                "--command=cat /tmp/v12-orchestrator-status 2>/dev/null && screen -ls 2>/dev/null || echo 'Still starting...'",
                capture=True,
            )
            print(f"\n[{datetime.now().strftime('%H:%M:%S')}] {vm_name} status:")
            print(result.stdout)
            if "LAUNCHED=" in result.stdout:
                print("[MONITOR] Orchestrator launched! Agents running.")
                break
        except subprocess.CalledProcessError:
            print(f"[{datetime.now().strftime('%H:%M:%S')}] VM not ready yet, retrying...")


def collect_results(vm_name: str, output_dir: str = "docs/brain/wave-results"):
    """Pull log files from VM back to local machine."""
    Path(output_dir).mkdir(parents=True, exist_ok=True)
    print(f"\n[COLLECT] Pulling logs from {vm_name}...")
    gcloud(
        "compute", "scp",
        "--recurse",
        f"{vm_name}:/home/{RUN_USER}/universal-or-strategy/logs/",
        f"{output_dir}/",
        f"--zone={ZONE}",
        f"--project={PROJECT_ID}",
    )
    print(f"[COLLECT] Logs saved to {output_dir}/")


def main():
    parser = argparse.ArgumentParser(description="V12 Wave Launcher")
    parser.add_argument("--wave", type=int, default=2, help="Wave number (default: 2)")
    parser.add_argument("--epics", type=str, help="Comma-separated epic IDs")
    parser.add_argument("--epics-file", type=str, help="File with one epic ID per line")
    parser.add_argument("--monitor", type=str, help="VM name to monitor")
    parser.add_argument("--collect", type=str, help="VM name to collect results from")
    parser.add_argument("--dry-run", action="store_true", help="Print commands without executing")
    args = parser.parse_args()

    if args.monitor:
        monitor_wave(args.monitor)
        return

    if args.collect:
        collect_results(args.collect)
        return

    # Parse epic list
    epics = []
    if args.epics:
        epics = [e.strip() for e in args.epics.split(",") if e.strip()]
    elif args.epics_file:
        epics = [l.strip() for l in Path(args.epics_file).read_text().splitlines() if l.strip() and not l.startswith("#")]
    else:
        print("[ERROR] Must provide --epics or --epics-file")
        sys.exit(1)

    vm_name = launch_wave(args.wave, epics, dry_run=args.dry_run)
    if not args.dry_run:
        monitor_wave(vm_name)


if __name__ == "__main__":
    main()
