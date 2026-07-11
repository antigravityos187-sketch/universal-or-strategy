#!/usr/bin/env python3
"""
Generate Phase 4 (Ticket Generation) scripts for Wave 4
Updated from Wave 3 to use /epic-tickets slash command instead of --chat-mode plan
"""

# Epic to API key mapping (Wave 4: CCN-126 through CCN-135)
API_ALLOCATION = {
    "126": "b (2).json",
    "127": "b.json",
    "128": "bob (1).json",
    "129": "bob (2).json",
    "130": "bob (3).json",
    "131": "bob (4).json",
    "132": "bob (5).json",
    "133": "bob (6).json",
    "134": "bob.json",
    "135": "sean.carter.jr@atomicmail.io.json",
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
    "sean.carter.jr@atomicmail.io.json": "bob_prod_bob-admin_44TtZXuuACpNu133KVpJ7nSGsRr8hhdVUJj3h3jYe5MUk44L1xm6bUAbv5WDab98VadJx53pvp1Kdxmch4E4Qh1H_7J5ULr6U54NC12M2tpGVD6FWjmjk5rgZWcDie42W6mRh",
}

# Wave 4 has no skipped epics
SKIP_EPICS = []

def generate_phase4_script(epic_num):
    """Generate Phase 4 script using /epic-tickets slash command"""
    if epic_num in SKIP_EPICS:
        print(f"[SKIP] EPIC-CCN-{epic_num} (closed as compliant)")
        return None
    
    api_file = API_ALLOCATION[epic_num]
    api_key = API_KEYS[api_file]
    
    script = f"""#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/EPIC-CCN-{epic_num}
mkdir -p logs/phase4

bob --yolo /epic-tickets EPIC-CCN-{epic_num} 2>&1 | tee logs/phase4/EPIC-CCN-{epic_num}.log
echo "DONE_EXIT=$?"

# Made with Bob
"""
    
    filename = f"scripts/wave4/_p4_{epic_num}.sh"
    with open(filename, 'w', newline='\n') as f:
        f.write(script)
    
    print(f"[OK] Created {filename} (API: {api_file})")
    return filename

def generate_launcher():
    """Generate launcher script for all Phase 4 scripts"""
    active_epics = [e for e in ["126", "127", "128", "129", "130", "131", "132", "133", "134", "135"] if e not in SKIP_EPICS]
    
    launcher = f"""#!/bin/bash
# Launch all Phase 4 (Ticket Generation) scripts in screen sessions
# Wave 4: All 10 epics active (no skips)

cd /home/malhitticrypto/universal-or-strategy

echo "Starting Phase 4 (Ticket Generation) for {len(active_epics)} epics..."

# Make scripts executable
chmod +x _p4_*.sh

# Launch each epic in its own screen session
for epic in {' '.join(active_epics)}; do
    screen_name="phase4_epic_${{epic}}"
    echo "Launching EPIC-CCN-${{epic}} in screen: ${{screen_name}}"
    screen -dmS "${{screen_name}}" bash -l -c "cd /home/malhitticrypto/universal-or-strategy && ./_p4_${{epic}}.sh"
    sleep 2
done

echo ""
echo "All Phase 4 scripts launched!"
echo ""
echo "Monitor with:"
echo "  screen -ls                    # List all sessions"
echo "  screen -r phase4_epic_116     # Attach to specific epic"
echo "  screen -S phase4_epic_116 -X stuff '^C'  # Kill specific session"
echo ""
echo "Check logs:"
echo "  tail -f logs/phase4/EPIC-CCN-*.log"
echo ""
"""
    
    with open("scripts/wave4/launch_phase4_all_screen.sh", 'w', newline='\n') as f:
        f.write(launcher)
    
    print("[OK] Created scripts/wave4/launch_phase4_all_screen.sh")

def main():
    print("=" * 60)
    print("Phase 4 Script Generator (Ticket Generation) - Wave 4")
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
    print("Generating Phase 4 scripts...")
    active_count = 0
    for epic_num in ["126", "127", "128", "129", "130", "131", "132", "133", "134", "135"]:
        result = generate_phase4_script(epic_num)
        if result:
            active_count += 1
    print()
    
    # Generate launcher
    print("Generating launcher...")
    generate_launcher()
    print()
    
    print("=" * 60)
    print(f"Phase 4 Generation Complete!")
    print(f"  Active epics: {active_count}")
    print(f"  Skipped epics: {len(SKIP_EPICS)}")
    print("=" * 60)
    print()
    print("Next steps:")
    print("1. Deploy to VM:")
    print("   gcloud compute scp scripts/wave4/_p4_*.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a")
    print("   gcloud compute scp scripts/wave4/launch_phase4_all_screen.sh v12-test-golden-v2:~/universal-or-strategy/ --zone=us-central1-a")
    print()
    print("2. Launch Phase 4:")
    print("   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command=\"cd universal-or-strategy && bash launch_phase4_all_screen.sh\"")
    print()

if __name__ == "__main__":
    main()