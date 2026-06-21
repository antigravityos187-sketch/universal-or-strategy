#!/bin/bash
# Pilot test script for Wave 7 Phase 0 - 3 epics (low/medium/high complexity)
# Tests Phase 0 script before launching full wave
#
# SUCCESS CRITERIA:
# 1. Files created: docs/brain/EPIC-W7-{001,050,100}/00-hotspots.md
# 2. Files verified on disk (ls -lh shows file size)
# 3. Bobcoin usage reported in logs
# 4. No errors in logs
# 5. jCodemunch MCP used (check logs for "get_hotspots" or "get_blast_radius")

set -e

PHASE=0
PILOT_EPICS=("001" "050" "100")  # Low, medium, high complexity

echo "[$(date)] Starting Phase 0 PILOT TEST - Wave 7"
echo "[$(date)] Testing 3 epics: EPIC-W7-001, EPIC-W7-050, EPIC-W7-100"
echo "[$(date)] This is a PILOT test before full wave launch (161 epics)"

# Create logs directory
mkdir -p logs/phase0

# Launch pilot epics in screen sessions with 12-second stagger
for i in "${!PILOT_EPICS[@]}"; do
    EPIC="${PILOT_EPICS[$i]}"
    
    echo "[$(date)] Launching EPIC-W7-${EPIC} ($(($i + 1))/3)"
    
    screen -dmS p${PHASE}-${EPIC} bash -l -c \
        "./scripts/wave7/_p${PHASE}_${EPIC}.sh 2>&1 | tee logs/phase${PHASE}/EPIC-W7-${EPIC}.log"
    
    # Wait before next launch (except for last epic)
    if [ $i -lt $((${#PILOT_EPICS[@]} - 1)) ]; then
        sleep 12
    fi
done

echo "[$(date)] Pilot epics launched"
echo ""
echo "MONITORING COMMANDS:"
echo "  Check status:    screen -ls | grep p${PHASE}-"
echo "  View logs:       tail -f logs/phase${PHASE}/EPIC-W7-*.log"
echo "  Attach session:  screen -r p${PHASE}-001"
echo "  Verify files:    ls -lh docs/brain/EPIC-W7-{001,050,100}/00-hotspots.md"
echo "  Check bobcoins:  grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase${PHASE}/EPIC-W7-*.log"
echo "  Check jCodemunch: grep -i 'get_hotspots\|get_blast_radius' logs/phase${PHASE}/EPIC-W7-*.log"
echo ""
echo "WAIT 15 MINUTES (Phase 0 = ~15 min per epic)"
echo "Then verify success before launching full wave (161 epics)"
echo ""
echo "SUCCESS VERIFICATION:"
echo "  1. All 3 screen sessions completed (screen -ls shows no p0-* sessions)"
echo "  2. All 3 00-hotspots.md files exist and >100 lines"
echo "  3. All 3 manifest.json files exist"
echo "  4. No errors in logs"
echo "  5. Bobcoin usage reasonable (<50 per epic)"

# Made with Bob - Building-Blocks Method (copied from Wave 4)