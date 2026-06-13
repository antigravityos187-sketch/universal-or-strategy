#!/usr/bin/env python3
"""
Generate Phase 2 (Architecture Planning) scripts for Wave 2
Copies Phase 1.5 pattern, only changes phase-specific details
"""

# Epic to API key mapping (from Phase 1.5 success)
API_ALLOCATION = {
    "107": "b (2).json",
    "108": "b.json",
    "109": "bob (1).json",
    "110": "bob (2).json",
    "111": "bob (3).json",
    "112": "bob (4).json",
    "113": "bob (5).json",
    "114": "bob (6).json",
    "115": "bob.json",
}

# API key contents (from docs/API/*.json)
API_KEYS = {
    "b (2).json": "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",
    "b.json": "bob_prod_bob-admin_V8sa2xf9tLezoczf9f7WZADcMhiUphzZPhDfRiMwx82Wxo1VtH3KMprtBvQFAmRYgECy254WHMSeWFxAuzBGzLj_2SQz2BrZKRs3WsotGTN56eL2Gthg4voAhcMZeefDi7wp",
    "bob (1).json": "bob_prod_bob-admin_t9tV9fuaYCkKYJNm5xCaHWAAR5yJT59mUXoLRHLyb3G4uVHazEQaFacXSz2Nd9Pij2WYNHkvn7THr5amYPqQeDa_ASoyvBNoW8FE2m47D2fhv67cbYGy7TXVeWYswv5N1MNF",
    "bob (2).json": "bob_prod_bob-admin_2am9d3VjQYnC4mSub1z5SzdSZJeyptWhfMrxGeEBSorZRPj8WmQvBPtTf8qTpjWHWdRuf7toP2WTDtPEfS6aoTYF_7ufADbTYhnLEY42csrSet3f3ssJuNddPhXD65YewpCWX",
    "bob (3).json": "bob_prod_bob-admin_5eZYFvHuinQHMnDWNZDZ7ciMX4oiUBsfkVyscGyoEahtNto1a7KNWHo5BFmoN4uPy8rbBYJrUsBtnshvB12nrYQJ_7tiXqEriChoWjAwta66uaZ76JKhxrqiQb6mR5C7AZQyo",
    "bob (4).json": "bob_prod_bob-admin_3abxQUhB6oz3484pgXxkjkeZEXxTEJfFGwg4D5cY6GWrCXFjT6uUQhvtLz5n8dB5g9Pue31DVuLwR9wa34zrBNmT_DdGCwiky7h1JVUEzJZVTrDxZNUigAnSRPPdUEJNzeLZT",
    "bob (5).json": "bob_prod_bob-admin_3vzs4jptuwZ7Z63gqpyn3aNy89ozwWyanh2aNB7TQDa22rfmiRJXWCUivJphxYNLAoT8nJMEYmUxaTgWA5Z8URUd_F6U16mpCReKejNsSHgrd7VxPEHuX8sedjJm4hrV7srcQ",
    "bob (6).json": "bob_prod_bob-admin_65hPWuoJAPhLQKgnKSePPDiqS5YRKW1XDF1LM8kRporvu9XTpgAaY4WYvJgAe72VzRDARKEQzqzMei9UqCj28buk_2Astcnxpem897Pn91xpJXnKY6N7dMhDXAriwNtncfzsB",
    "bob.json": "bob_prod_bob-admin_5A6hXsy7FL4vf9T2jqr11gdYTmAZcFgxVm1dGD9qGPmpD5fV6emRy6XYzZPsqw56mjCtoiEbJmLU8B2VL4ZtgXeS_ALp1DF9sj3R3cU3dzddRRAVu44Y52VHhkt1BNkSdC2Nq",
}

def generate_phase2_script(epic_num):
    """Generate Phase 2 script for given epic number"""
    api_file = API_ALLOCATION[epic_num]
    api_key = API_KEYS[api_file]
    
    script = f"""#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/EPIC-CCN-{epic_num}
mkdir -p logs/phase2

cat > /tmp/phase2_msg_{epic_num}.txt << 'EOFMSG'
You are executing Phase 2 (Architecture Planning) for EPIC-CCN-{epic_num}.

**Input Artifact**: Read `docs/brain/EPIC-CCN-{epic_num}/01-scope-boundary.md` for scope definition.

**Your Task**: Create detailed architecture plan for the extraction.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-{epic_num}/02-architecture-plan.md` with:
   - Method signatures (before/after)
   - Call graph analysis
   - Dependency mapping
   - Extraction sequence
   - Jane Street compliance checks
   - Risk mitigation strategies

2. Update `docs/brain/EPIC-CCN-{epic_num}/manifest.json`:
   - Set phase "2" status to "completed"
   - Add "02-architecture-plan.md" to outputs

**MANDATORY REPORTING**:
After completing all tasks, you MUST report:
1. Bobcoins used this session: [X.XX]
2. Remaining balance in API key: [Y.YY]
Format: "Cost: X.XX | Balance: Y.YY"

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Target complexity <= 8 (Jane Street alignment)
- Single method extraction only (V12.23 Protocol)

**Phase**: 2 (Architecture Planning)
EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase2_msg_{epic_num}.txt)" 2>&1 | tee logs/phase2/EPIC-CCN-{epic_num}.log
echo "DONE_EXIT=$?"

# Made with Bob
"""
    
    filename = f"_p2_{epic_num}.sh"
    with open(filename, 'w', newline='\n') as f:
        f.write(script)
    
    print(f"[OK] Created {filename} (API: {api_file})")
    return filename

def generate_launcher():
    """Generate launcher script for all Phase 2 scripts"""
    launcher = """#!/bin/bash
# Launch all Phase 2 (Architecture Planning) scripts in screen sessions
# Generated from Phase 1.5 success pattern

cd /home/malhitticrypto/universal-or-strategy

echo "Starting Phase 2 (Architecture Planning) for 9 epics..."

# Make scripts executable
chmod +x _p2_*.sh

# Launch each epic in its own screen session
for epic in 107 108 109 110 111 112 113 114 115; do
    screen_name="phase2_epic_${epic}"
    echo "Launching EPIC-CCN-${epic} in screen: ${screen_name}"
    screen -dmS "${screen_name}" bash -l -c "cd /home/malhitticrypto/universal-or-strategy && ./_p2_${epic}.sh"
    sleep 2
done

echo ""
echo "All Phase 2 scripts launched!"
echo ""
echo "Monitor with:"
echo "  screen -ls                    # List all sessions"
echo "  screen -r phase2_epic_107     # Attach to specific epic"
echo "  screen -S phase2_epic_107 -X stuff '^C'  # Kill specific session"
echo ""
echo "Check logs:"
echo "  tail -f logs/phase2/EPIC-CCN-*.log"
echo ""
"""
    
    with open("launch_phase2_all_screen.sh", 'w', newline='\n') as f:
        f.write(launcher)
    
    print("[OK] Created launch_phase2_all_screen.sh")

def main():
    print("=" * 60)
    print("Phase 2 Script Generator (Building Blocks Method)")
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
    print("Generating Phase 2 scripts...")
    for epic_num in ["107", "108", "109", "110", "111", "112", "113", "114", "115"]:
        generate_phase2_script(epic_num)
    print()
    
    # Generate launcher
    print("Generating launcher...")
    generate_launcher()
    print()
    
    print("=" * 60)
    print("[SUCCESS] Phase 2 scripts ready!")
    print("=" * 60)
    print()
    print("Next steps:")
    print("1. Deploy to VM: gcloud compute scp _p2_*.sh launch_phase2_all_screen.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a")
    print("2. Make executable: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='cd universal-or-strategy && chmod +x _p2_*.sh launch_phase2_all_screen.sh'")
    print("3. Launch: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='cd universal-or-strategy && bash launch_phase2_all_screen.sh'")
    print()

if __name__ == "__main__":
    main()