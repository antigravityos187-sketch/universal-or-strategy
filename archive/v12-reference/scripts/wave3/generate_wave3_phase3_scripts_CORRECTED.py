#!/usr/bin/env python3
"""
Generate Phase 3 (DNA & PR Audit) scripts for Wave 3
CORRECTED: Uses Claude advanced mode (copied from Wave 2 working pattern)
"""

import json

# Load API keys from JSON files
def load_api_key(filename):
    with open(f'docs/API/{filename}', 'r') as f:
        data = json.load(f)
        return data['apikey']

# Epic to API key mapping
API_ALLOCATION = {
    "116": "b (2).json",
    "117": "b.json",
    "118": "bob (1).json",
    "119": "bob (2).json",
    "120": "bob (3).json",
    "121": "bob (4).json",
    "122": "bob (5).json",
    "123": "bob (6).json",
    "124": "bob.json",
    "125": "sean.carter.jr@atomicmail.io.json",
}

# Load API keys
API_KEYS = {}
for epic_num, filename in API_ALLOCATION.items():
    API_KEYS[filename] = load_api_key(filename)

def generate_phase3_script(epic_num):
    """Generate Phase 3 script for given epic number"""
    api_file = API_ALLOCATION[epic_num]
    api_key = API_KEYS[api_file]
    
    # CRITICAL: Use --chat-mode advanced (Claude), NOT /epic-scan (Bob Shell)
    # This is the CORRECT pattern from Wave 2
    script = f"""#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/EPIC-CCN-{epic_num}
mkdir -p logs/phase3

cat > /tmp/phase3_msg_{epic_num}.txt << 'EOFMSG'
You are executing Phase 3 (DNA & PR Audit) for EPIC-CCN-{epic_num}.

**Input Artifact**: Read `docs/brain/EPIC-CCN-{epic_num}/02-implementation-plan.md` for architecture plan.

**Your Task**: Perform V12 DNA compliance checks and PR hygiene validation.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-{epic_num}/03-audit-report.md` with:
   - V12 DNA compliance checks (lock-free, ASCII-only, Jane Street alignment)
   - PR hygiene validation (diff size, whitespace, scope creep)
   - Pre-flight safety checks
   - Risk assessment
   - Go/No-Go recommendation

2. Update `docs/brain/EPIC-CCN-{epic_num}/manifest.json`:
   - Set phase "3" status to "completed"
   - Add "03-audit-report.md" to outputs

**MANDATORY REPORTING**:
After completing all tasks, you MUST report:
1. Bobcoins used this session: [X.XX]
2. Remaining balance in API key: [Y.YY]
Format: "Cost: X.XX | Balance: Y.YY"

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Target complexity <= 8 (Jane Street alignment)
- Check for lock-free compliance (no lock() statements)
- Verify ASCII-only (no Unicode/emoji)
- Validate PR diff < 10k characters

**Phase**: 3 (DNA & PR Audit)
EOFMSG

bob --yolo --chat-mode advanced "$(cat /tmp/phase3_msg_{epic_num}.txt)" 2>&1 | tee logs/phase3/EPIC-CCN-{epic_num}.log
echo "DONE_EXIT=$?"

# Made with Bob
"""
    
    filename = f"scripts/wave3/_p3_{epic_num}.sh"
    with open(filename, 'w', newline='\n') as f:
        f.write(script)
    
    print(f"[OK] Created {filename} (API: {api_file})")
    return filename

def generate_launcher():
    """Generate launcher script for all Phase 3 scripts"""
    active_epics = ["116", "117", "118", "119", "120", "121", "122", "123", "124", "125"]
    
    launcher = f"""#!/bin/bash
# Launch all Phase 3 (DNA & PR Audit) scripts in screen sessions
# CORRECTED: Uses Claude advanced mode (copied from Wave 2 working pattern)

cd /home/malhitticrypto/universal-or-strategy

echo "Starting Phase 3 (DNA & PR Audit) for {len(active_epics)} epics..."

# Make scripts executable
chmod +x _p3_*.sh

# Launch each epic in its own screen session
for epic in {' '.join(active_epics)}; do
    screen_name="phase3_epic_${{epic}}"
    echo "Launching EPIC-CCN-${{epic}} in screen: ${{screen_name}}"
    screen -dmS "${{screen_name}}" bash -l -c "cd /home/malhitticrypto/universal-or-strategy && ./_p3_${{epic}}.sh"
    sleep 2
done

echo ""
echo "All Phase 3 scripts launched!"
echo ""
echo "Monitor with:"
echo "  screen -ls                    # List all sessions"
echo "  screen -r phase3_epic_116     # Attach to specific epic"
echo "  screen -S phase3_epic_116 -X stuff '^C'  # Kill specific session"
echo ""
echo "Check logs:"
echo "  tail -f logs/phase3/EPIC-CCN-*.log"
echo ""
"""
    
    with open("scripts/wave3/launch_phase3_all_screen.sh", 'w', newline='\n') as f:
        f.write(launcher)
    
    print("[OK] Created scripts/wave3/launch_phase3_all_screen.sh")

def main():
    print("=" * 60)
    print("Phase 3 Script Generator (DNA & PR Audit) - CORRECTED")
    print("Uses Claude advanced mode (Wave 2 working pattern)")
    print("=" * 60)
    print()
    
    # Validate API allocation (no duplicates)
    api_values = list(API_ALLOCATION.values())
    if len(api_values) != len(set(api_values)):
        duplicates = [x for x in api_values if api_values.count(x) > 1]
        raise ValueError(f"DUPLICATE API KEYS DETECTED: {duplicates}")
    print(f"[OK] Validated {len(api_values)} unique API keys")
    print()
    
    # Generate individual scripts
    print("Generating Phase 3 scripts...")
    active_count = 0
    for epic_num in ["116", "117", "118", "119", "120", "121", "122", "123", "124", "125"]:
        result = generate_phase3_script(epic_num)
        if result:
            active_count += 1
    print()
    
    # Generate launcher
    print("Generating launcher...")
    generate_launcher()
    print()
    
    print("=" * 60)
    print(f"Phase 3 Generation Complete!")
    print(f"  Active epics: {active_count}")
    print("=" * 60)
    print()
    print("Next steps:")
    print("1. Deploy to VM:")
    print("   gcloud compute scp scripts/wave3/_p3_*.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a")
    print("   gcloud compute scp scripts/wave3/launch_phase3_all_screen.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a")
    print()
    print("2. Launch Phase 3:")
    print("   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command=\"cd universal-or-strategy && bash launch_phase3_all_screen.sh\"")
    print()
    print("CRITICAL DIFFERENCE FROM PREVIOUS VERSION:")
    print("  OLD (WRONG): bob --yolo /epic-scan EPIC-CCN-X")
    print("  NEW (CORRECT): bob --yolo --chat-mode advanced \"$(cat /tmp/phase3_msg_X.txt)\"")
    print()

if __name__ == "__main__":
    main()