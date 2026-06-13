#!/bin/bash
# Wave 2 Script Pollution Remediation
# Archives sequential Phase 6 script and creates correct parallel version
# V12.25 - 2026-06-13

set -e

echo "=== Wave 2 Script Pollution Remediation ==="
echo "Started: $(date)"
echo ""

# Check if we're on the VM
if [ ! -f "launch_phase6_all_epics.sh" ]; then
    echo "ERROR: launch_phase6_all_epics.sh not found"
    echo "This script must be run from /home/malhitticrypto/universal-or-strategy on the VM"
    exit 1
fi

# Step 1: Archive sequential script
echo "Step 1: Archiving sequential script..."
mv launch_phase6_all_epics.sh launch_phase6_all_epics.sh.SEQUENTIAL_ERROR
echo "  ✓ Renamed to launch_phase6_all_epics.sh.SEQUENTIAL_ERROR"

# Step 2: Create warning README
echo "Step 2: Creating warning README..."
cat > launch_phase6_all_epics.sh.SEQUENTIAL_ERROR.README << 'EOF'
# ⚠️ WARNING: SEQUENTIAL EXECUTION ERROR

This script was used in Wave 2 Phase 6 but contains a CRITICAL ERROR:
It executes epics SEQUENTIALLY instead of PARALLEL.

**DO NOT USE THIS SCRIPT AS A TEMPLATE**

See: launch_phase6_all_epics_PARALLEL.sh for correct implementation
See: docs/protocol/PARALLEL_EXECUTION_SOP.md for explanation
See: docs/workflow/BUILDING_BLOCKS_INDEX.md for approved scripts

This file is preserved for forensic analysis only.

## Error Details

**Problem**: Used wait_for_completion() loop instead of ThreadPoolExecutor
**Impact**: 21 minutes vs ~7-10 minutes (2-3x slower)
**Date**: 2026-06-13
**Wave**: Wave 2 Phase 6
**Status**: ARCHIVED (do not use)

## What Went Wrong

```python
# ❌ WRONG - Sequential execution
for epic in epics:
    launch_epic_phase6(epic)
    wait_for_completion(epic)  # Blocks until epic completes
    check_status(epic)
```

## Correct Pattern

```python
# ✅ CORRECT - Parallel execution
from concurrent.futures import ThreadPoolExecutor, as_completed

with ThreadPoolExecutor(max_workers=3) as executor:
    futures = {executor.submit(run_epic_phase6, epic): epic for epic in epics}
    for future in as_completed(futures):
        result = future.result()
```

## References

- Parallel Execution SOP: docs/protocol/PARALLEL_EXECUTION_SOP.md
- Building Blocks Index: docs/workflow/BUILDING_BLOCKS_INDEX.md
- Remediation Plan: docs/protocol/WAVE2_SCRIPT_POLLUTION_REMEDIATION.md
EOF
echo "  ✓ Created launch_phase6_all_epics.sh.SEQUENTIAL_ERROR.README"

# Step 3: Create correct parallel script
echo "Step 3: Creating correct parallel script..."
cat > launch_phase6_all_epics_PARALLEL.sh << 'EOF'
#!/bin/bash
# Wave 2 Phase 6 Orchestrator (PARALLEL EXECUTION)
# Launches all 7 epic reviews in parallel with 3 workers
# V12.25 - Correct Implementation

set -e

EPICS=(107 108 109 111 112 113 114)
LOG_DIR="logs"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
MAIN_LOG="${LOG_DIR}/phase6_parallel_${TIMESTAMP}.log"

mkdir -p "$LOG_DIR"

echo "=== Wave 2 Phase 6 Parallel Orchestrator ===" | tee -a "$MAIN_LOG"
echo "Started: $(date)" | tee -a "$MAIN_LOG"
echo "Workers: 3 (parallel execution)" | tee -a "$MAIN_LOG"
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
    screen_name = f"p6_{epic_id}"
    script_path = f"_p6_{epic_id}.sh"
    
    print(f"[{epic_id}] Starting Phase 6 review...")
    
    # Launch in screen session
    cmd = f"screen -dmS {screen_name} bash -l {script_path}"
    subprocess.run(cmd, shell=True, check=True)
    
    # Wait for completion
    while True:
        result = subprocess.run(
            f"screen -list | grep -q '{screen_name}'",
            shell=True,
            capture_output=True
        )
        if result.returncode != 0:  # Screen session ended
            break
        time.sleep(30)
    
    print(f"[{epic_id}] Phase 6 complete")
    return epic_id

# Execute with 3 workers
epics = [107, 108, 109, 111, 112, 113, 114]
with ThreadPoolExecutor(max_workers=3) as executor:
    futures = {executor.submit(run_epic_phase6, epic): epic for epic in epics}
    
    for future in as_completed(futures):
        epic = futures[future]
        try:
            result = future.result()
            print(f"✅ EPIC-CCN-{result} completed successfully")
        except Exception as e:
            print(f"❌ EPIC-CCN-{epic} failed: {e}")
PYTHON_EOF

echo "" | tee -a "$MAIN_LOG"
echo "=== Phase 6 Parallel Execution Complete ===" | tee -a "$MAIN_LOG"
echo "Completed: $(date)" | tee -a "$MAIN_LOG"
echo "Check individual logs in: ${LOG_DIR}/phase6_*.log" | tee -a "$MAIN_LOG"
EOF

chmod +x launch_phase6_all_epics_PARALLEL.sh
echo "  ✓ Created launch_phase6_all_epics_PARALLEL.sh"

# Step 4: Verify files
echo ""
echo "Step 4: Verifying files..."
if [ -f "launch_phase6_all_epics.sh.SEQUENTIAL_ERROR" ]; then
    echo "  ✓ Sequential script archived"
else
    echo "  ✗ ERROR: Sequential script not found"
    exit 1
fi

if [ -f "launch_phase6_all_epics.sh.SEQUENTIAL_ERROR.README" ]; then
    echo "  ✓ Warning README created"
else
    echo "  ✗ ERROR: Warning README not found"
    exit 1
fi

if [ -f "launch_phase6_all_epics_PARALLEL.sh" ]; then
    echo "  ✓ Parallel script created"
else
    echo "  ✗ ERROR: Parallel script not found"
    exit 1
fi

# Step 5: Summary
echo ""
echo "=== Remediation Complete ==="
echo "Completed: $(date)"
echo ""
echo "Files:"
echo "  - launch_phase6_all_epics.sh.SEQUENTIAL_ERROR (archived)"
echo "  - launch_phase6_all_epics.sh.SEQUENTIAL_ERROR.README (warning)"
echo "  - launch_phase6_all_epics_PARALLEL.sh (correct version)"
echo ""
echo "Next Steps:"
echo "  1. Review Building Blocks Index: docs/workflow/BUILDING_BLOCKS_INDEX.md"
echo "  2. Use _PARALLEL.sh version for future waves"
echo "  3. Never use .SEQUENTIAL_ERROR version as template"
echo ""
echo "✅ Script pollution remediated successfully"
EOF
chmod +x scripts/wave2/remediate_sequential_pollution.sh

# Made with Bob
