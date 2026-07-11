#!/usr/bin/env python3
"""Wave 2 Phase 0 v4 - Custom Mode with Shell Commands (Tool Bug Workaround)"""
import subprocess, sys, json
from pathlib import Path

# Load epic data from roadmap instead of hardcoding
def load_epics_from_roadmap():
    roadmap = json.loads(Path("epic_roadmap.json").read_text())
    epics = []
    for epic_id in range(107, 116):
        epic_key = f"EPIC-CCN-{epic_id}"
        for entry in roadmap:
            if entry.get("epic_number") == epic_key:
                epics.append({
                    "id": str(epic_id),
                    "method": entry["method"],
                    "file": entry["file"],
                    "cyc": entry["cyclomatic"]
                })
                break
    return epics

EPICS = load_epics_from_roadmap()

API_ALLOCATION = {
    "107": "b (2).json", "108": "b.json", "109": "bob (1).json",
    "110": "bob (2).json", "111": "bob (3).json", "112": "bob (4).json",
    "113": "bob (5).json", "114": "bob (6).json", "115": "bob.json",
}

def load_api_key(filename: str) -> str:
    return json.loads((Path("docs/API") / filename).read_text())["apikey"]

# Load shell command template
SHELL_TEMPLATE = Path("scripts/wave2/phase0_message_template_shell.txt").read_text(encoding='utf-8')

def create_script(epic_id: str, method: str, file: str, cyc: int, api_key: str) -> str:
    # Replace template placeholders with actual epic data
    message = SHELL_TEMPLATE.replace("{EPIC_ID}", epic_id)
    message = message.replace("{METHOD}", method)
    message = message.replace("{FILE}", file)
    message = message.replace("{CYC}", str(cyc))
    
    return f'''#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/EPIC-CCN-{epic_id}
mkdir -p logs/phase0

cat > /tmp/phase0_msg_{epic_id}.txt << 'EOFMSG'
{message}
EOFMSG

bob --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_{epic_id}.txt)" 2>&1 | tee logs/phase0/EPIC-CCN-{epic_id}.log
echo "DONE_EXIT=$?"
'''

def main():
    print("=" * 60)
    print("Wave 2 Phase 0 v4 - Shell Commands (Custom Mode)")
    print("=" * 60)
    print()
    print("Generating scripts for 9 epics...")
    print()
    
    for epic in EPICS:
        api_key = load_api_key(API_ALLOCATION[epic["id"]])
        script = create_script(epic["id"], epic["method"], epic["file"], epic["cyc"], api_key)
        script_path = Path(f"_p0_{epic['id']}.sh")
        script_path.write_text(script, encoding='utf-8', newline="\n")
        print(f"[OK] {script_path}")
    
    # Create launcher script
    launcher = """#!/bin/bash
# Launch all Phase 0 agents in parallel
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "[WAVE2-P0] Launching 9 parallel agents..."
"""
    
    for epic in EPICS:
        launcher += f"""
screen -dmS p0-{epic['id']} bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && bash _p0_{epic['id']}.sh'
echo "[WAVE2-P0] Launched: EPIC-CCN-{epic['id']} ({epic['method']}, CYC {epic['cyc']})"
sleep 1
"""
    
    launcher += """
echo "[WAVE2-P0] All 9 agents launched."
echo "[WAVE2-P0] Monitor: screen -ls"
echo "[WAVE2-P0] Logs: tail -f logs/phase0/EPIC-CCN-107.log"
"""
    
    Path("launch_phase0_all.sh").write_text(launcher, encoding='utf-8', newline="\n")
    print(f"[OK] launch_phase0_all.sh")
    print()
    print("=" * 60)
    print("DEPLOYMENT COMMANDS")
    print("=" * 60)
    print()
    print("1. Upload scripts to VM:")
    print("   gcloud compute scp _p0_*.sh launch_phase0_all.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a")
    print()
    print("2. Deploy custom modes to VM:")
    print("   gcloud compute scp .bob/custom_modes.yaml v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/.bob/ --zone=us-central1-a")
    print()
    print("3. TEST with single epic first:")
    print("   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='cd /home/malhitticrypto/universal-or-strategy && bash _p0_107.sh'")
    print()
    print("4. If test succeeds, launch all 9:")
    print("   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='cd /home/malhitticrypto/universal-or-strategy && bash launch_phase0_all.sh'")
    print()
    print("=" * 60)
    print("MONITORING")
    print("=" * 60)
    print()
    print("Check status:")
    print("  gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='screen -ls'")
    print()
    print("View log:")
    print("  gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='tail -f /home/malhitticrypto/universal-or-strategy/logs/phase0/EPIC-CCN-107.log'")
    print()
    print("Verify files:")
    print("  gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='ls -lh /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/00-hotspots.md'")
    print()

if __name__ == "__main__":
    main()

# Made with Bob
