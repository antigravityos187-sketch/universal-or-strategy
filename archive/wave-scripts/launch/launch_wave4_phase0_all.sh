#!/bin/bash
# Wave 4 Phase 0 Master Launch Script
# Launches all 80 epics with staggered delays (12-54 seconds)
# Foreground-visible execution via log files

set -e

REPO_DIR="/home/malhitticrypto/universal-or-strategy"
cd "$REPO_DIR"

echo "=== Wave 4 Phase 0: Launching 80 Epics with Staggered Delays ==="
echo "Start time: $(date)"
echo ""

# Array of epic numbers (001-080)
EPICS=($(seq -f "%03g" 1 80))

# Constant delay configuration (12 seconds between all epics)
DELAY=12

echo "Configuration:"
echo "  - Epics: 80 (EPIC-CCN-001 through EPIC-CCN-080)"
echo "  - Delay: ${DELAY} seconds (constant)"
echo "  - Estimated launch time: ~16 minutes"
echo ""

# Launch each epic with constant delay
for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    SCRIPT_PATH="${REPO_DIR}/_p0_${EPIC}.sh"
    
    echo "[$(date '+%H:%M:%S')] Launching EPIC-CCN-${EPIC} (delay: ${DELAY}s, progress: $((i+1))/80)"
    
    # Launch in background, log to file (foreground-visible via tail -f)
    bash "${SCRIPT_PATH}" > "logs/phase0/EPIC-CCN-${EPIC}.log" 2>&1 &
    
    # Store PID for monitoring
    echo $! > "logs/phase0/EPIC-CCN-${EPIC}.pid"
    
    # Wait before next launch (staggered delay)
    sleep ${DELAY}
done

echo ""
echo "=== All 80 Epics Launched ==="
echo "End time: $(date)"
echo ""
echo "Monitor progress:"
echo "  tail -f logs/phase0/EPIC-CCN-001.log          # Watch first epic"
echo "  watch 'ls docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l'  # Count completed"
echo "  ps aux | grep bob | wc -l                     # Count running agents"
echo ""
echo "Check completion:"
echo "  ls -lh docs/brain/EPIC-CCN-*/00-hotspots.md   # View all outputs"
echo "  grep -r 'DONE_EXIT' logs/phase0/*.log         # Check exit codes"
echo ""
echo "Expected output: 160 files total (80 x 00-hotspots.md + 80 x manifest.json)"
echo ""
echo "Estimated completion: ~20 minutes (parallel execution)"

# Made with Bob
