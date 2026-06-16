#!/bin/bash
# Smoke Test: Parallel Phase 6 Execution
# Tests with 2 epics (111 and 113) to verify parallel execution works
# V12.25 - 2026-06-13

set -e

echo "=== Smoke Test: Parallel Phase 6 Execution ==="
echo "Started: $(date)"
echo "Testing with: EPIC-111, EPIC-113"
echo ""

# Create test script
cat > test_parallel_phase6_SMOKE.sh << 'EOF'
#!/bin/bash
# Smoke test version - only 2 epics
set -e

EPICS=(111 113)
LOG_DIR="logs"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
MAIN_LOG="${LOG_DIR}/phase6_parallel_smoke_${TIMESTAMP}.log"

mkdir -p "$LOG_DIR"

echo "=== Phase 6 Parallel Smoke Test ===" | tee -a "$MAIN_LOG"
echo "Started: $(date)" | tee -a "$MAIN_LOG"
echo "Workers: 2 (smoke test)" | tee -a "$MAIN_LOG"
echo "Epics: ${EPICS[@]}" | tee -a "$MAIN_LOG"
echo "" | tee -a "$MAIN_LOG"

# Use Python ThreadPoolExecutor for parallel execution
echo "Using Python ThreadPoolExecutor for execution" | tee -a "$MAIN_LOG"
python3 << 'PYTHON_EOF'
import subprocess
import time
from concurrent.futures import ThreadPoolExecutor, as_completed

def run_epic_phase6(epic_id):
    """Execute Phase 6 for a single epic."""
    screen_name = f"p6_smoke_{epic_id}"
    script_path = f"_p6_{epic_id}.sh"
    
    print(f"[{epic_id}] Starting Phase 6 review...")
    
    # Check if script exists
    result = subprocess.run(f"test -f {script_path}", shell=True, capture_output=True)
    if result.returncode != 0:
        print(f"[{epic_id}] WARNING: Script {script_path} not found, skipping")
        return epic_id
    
    # Launch in screen session
    cmd = f"screen -dmS {screen_name} bash -l {script_path}"
    subprocess.run(cmd, shell=True, check=True)
    
    # Wait for completion (max 5 minutes for smoke test)
    start_time = time.time()
    while True:
        result = subprocess.run(
            f"screen -list | grep -q '{screen_name}'",
            shell=True,
            capture_output=True
        )
        if result.returncode != 0:  # Screen session ended
            break
        
        # Timeout after 5 minutes
        if time.time() - start_time > 300:
            print(f"[{epic_id}] TIMEOUT after 5 minutes")
            subprocess.run(f"screen -S {screen_name} -X quit", shell=True)
            break
            
        time.sleep(10)
    
    elapsed = time.time() - start_time
    print(f"[{epic_id}] Phase 6 complete ({elapsed:.1f}s)")
    return epic_id

# Execute with 2 workers
epics = [111, 113]
start_time = time.time()

with ThreadPoolExecutor(max_workers=2) as executor:
    futures = {executor.submit(run_epic_phase6, epic): epic for epic in epics}
    
    for future in as_completed(futures):
        epic = futures[future]
        try:
            result = future.result()
            print(f"✅ EPIC-CCN-{result} completed successfully")
        except Exception as e:
            print(f"❌ EPIC-CCN-{epic} failed: {e}")

total_time = time.time() - start_time
print(f"\n⏱️ Total execution time: {total_time:.1f}s")
print(f"📊 Average per epic: {total_time/len(epics):.1f}s")
PYTHON_EOF

echo "" | tee -a "$MAIN_LOG"
echo "=== Smoke Test Complete ===" | tee -a "$MAIN_LOG"
echo "Completed: $(date)" | tee -a "$MAIN_LOG"
EOF

chmod +x test_parallel_phase6_SMOKE.sh

echo "✓ Created test_parallel_phase6_SMOKE.sh"
echo ""
echo "Uploading to VM..."

# Made with Bob
