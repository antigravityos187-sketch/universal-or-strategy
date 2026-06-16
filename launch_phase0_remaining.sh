#!/bin/bash
set -e

# Wave 4 Phase 0 - Remaining Epics (020-080)
# Generated: 2026-06-14
# Strategy: Constant 12s delay + 1 min initial check + 4 min polling

REPO_DIR="/home/malhitticrypto/universal-or-strategy"
cd "$REPO_DIR"

PHASE=0
DELAY=12  # Constant delay (not incrementing)

# Remaining epics (020-080, excluding already completed 001-019)
EPICS=($(seq -f "%03g" 20 80))

echo "=========================================="
echo "Wave 4 Phase 0 - Remaining Epics"
echo "=========================================="
echo "Epics to launch: ${#EPICS[@]} (020-080)"
echo "Delay between launches: ${DELAY}s (constant)"
echo "Estimated launch time: $((${#EPICS[@]} * DELAY / 60)) minutes"
echo "Estimated execution time: ~10 minutes (parallel)"
echo "Polling strategy: 1 min initial + 4 min continuous"
echo "=========================================="
echo ""

# Launch all remaining epics
echo "[$(date '+%Y-%m-%d %H:%M:%S')] Starting launches..."
for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    SCRIPT_PATH="${REPO_DIR}/_p${PHASE}_${EPIC}.sh"
    LOG_PATH="${REPO_DIR}/logs/phase${PHASE}/EPIC-CCN-${EPIC}.log"
    PID_PATH="${REPO_DIR}/logs/phase${PHASE}/EPIC-CCN-${EPIC}.pid"
    
    echo "[$(date '+%H:%M:%S')] Launching EPIC-CCN-${EPIC} ($((i+1))/${#EPICS[@]})"
    
    # Launch in background
    bash "${SCRIPT_PATH}" > "${LOG_PATH}" 2>&1 &
    echo $! > "${PID_PATH}"
    
    # Constant delay - same for ALL epics
    sleep ${DELAY}
done

echo "[$(date '+%Y-%m-%d %H:%M:%S')] All ${#EPICS[@]} epics launched"
echo ""

# Initial status check (1 minute)
echo "[$(date '+%Y-%m-%d %H:%M:%S')] Waiting 1 minute for initial status check..."
sleep 60

echo "[$(date '+%Y-%m-%d %H:%M:%S')] Initial Status Check:"
COMPLETE=$(ls ${REPO_DIR}/docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l)
echo "  Completed: $COMPLETE/80 epics"

RUNNING=$(ps aux | grep '_p0_' | grep -v grep | wc -l)
echo "  Running: $RUNNING agents"

# Continuous polling (4-minute intervals for cache optimization)
echo ""
echo "[$(date '+%Y-%m-%d %H:%M:%S')] Starting 4-minute polling cycle..."
echo "  (Optimized for 90% cache hit rate)"

POLL_COUNT=0
while true; do
    sleep 240  # 4 minutes (cache-optimized)
    POLL_COUNT=$((POLL_COUNT + 1))
    
    echo ""
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] Poll #${POLL_COUNT}:"
    
    # Count completed epics
    COMPLETE=$(ls ${REPO_DIR}/docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l)
    echo "  Completed: $COMPLETE/80 epics"
    
    # Count running agents
    RUNNING=$(ps aux | grep '_p0_' | grep -v grep | wc -l)
    echo "  Running: $RUNNING agents"
    
    # Check for errors
    ERRORS=$(grep -i 'error\|failed\|exception' ${REPO_DIR}/logs/phase0/*.log 2>/dev/null | wc -l)
    if [ "$ERRORS" -gt 0 ]; then
        echo "  ⚠️  Errors detected: $ERRORS"
        echo "  Sample errors:"
        grep -i 'error\|failed\|exception' ${REPO_DIR}/logs/phase0/*.log 2>/dev/null | head -3
    else
        echo "  ✅ No errors detected"
    fi
    
    # Check VM health
    LOAD=$(uptime | awk -F'load average:' '{print $2}' | awk '{print $1}' | tr -d ',')
    echo "  VM Load: $LOAD"
    
    # Exit if all complete
    if [ "$COMPLETE" -eq 80 ]; then
        echo ""
        echo "=========================================="
        echo "✅ Phase 0 Complete!"
        echo "=========================================="
        echo "Total epics: $COMPLETE/80"
        echo "Total polls: $POLL_COUNT"
        echo "Completion time: $(date '+%Y-%m-%d %H:%M:%S')"
        echo "=========================================="
        break
    fi
    
    # Show progress
    PROGRESS=$((COMPLETE * 100 / 80))
    echo "  Progress: ${PROGRESS}%"
done

echo ""
echo "Next step: Extract bobcoin usage and sync results to local"

# Made with Bob
