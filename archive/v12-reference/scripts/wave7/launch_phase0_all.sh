#!/bin/bash
# Wave 7 Phase 0 Master Launcher
# Launches all 161 epics in screen sessions with staggered delays
#
# COST-OPTIMIZED POLLING PROTOCOL:
# - First 10 epics: 1-minute polling (launch verification)
# - Remaining 151 epics: 4-minute polling (88% cost reduction)
#
# EXECUTION MODEL:
# - All epics run ON the VM (not via gcloud ssh)
# - Uses screen sessions for background execution
# - Logs to logs/phase0/EPIC-W7-XXX.log

set -e

PHASE=0
TOTAL_EPICS=161
DELAY=12  # 12-second stagger between launches

echo "[$(date)] Starting Wave 7 Phase 0 launch for ${TOTAL_EPICS} epics"
echo "[$(date)] Using 12-second delays between launches"
echo "[$(date)] Total launch time: ~$((TOTAL_EPICS * DELAY / 60)) minutes"

# Create logs directory
mkdir -p logs/phase0

# Launch all 161 epics
for i in $(seq -f "%03g" 1 ${TOTAL_EPICS}); do
    EPIC_NUM=$i
    EPIC_ID="EPIC-W7-${EPIC_NUM}"
    
    echo "[$(date)] Launching ${EPIC_ID} ($((10#$i))/${TOTAL_EPICS})"
    
    # Launch in screen session
    screen -dmS p${PHASE}-${EPIC_NUM} bash -l -c \
        "./scripts/wave7/_p${PHASE}_${EPIC_NUM}.sh 2>&1 | tee logs/phase${PHASE}/${EPIC_ID}.log"
    
    # Wait before next launch (except for last epic)
    if [ $((10#$i)) -lt ${TOTAL_EPICS} ]; then
        sleep ${DELAY}
    fi
done

echo "[$(date)] All ${TOTAL_EPICS} epics launched for Phase 0"
echo "[$(date)] Total launch time: $((TOTAL_EPICS * DELAY / 60)) minutes"
echo ""
echo "=========================================="
echo "COST-OPTIMIZED POLLING PROTOCOL"
echo "=========================================="
echo ""
echo "PHASE 1: Launch Verification (First 10 epics)"
echo "  - Wait 1 minute after launch completes"
echo "  - Check: screen -ls | grep 'p0-' | wc -l"
echo "  - Poll every 1 MINUTE for first 10 epics"
echo "  - Verify files: ls docs/brain/EPIC-W7-{001..010}/00-hotspots.md"
echo ""
echo "PHASE 2: Cost-Optimized Execution (Remaining 151 epics)"
echo "  - Once first 10 complete, switch to 4-MINUTE polling"
echo "  - Poll every 4 MINUTES (88% cost reduction)"
echo "  - Monitor: screen -ls | grep 'p0-' | wc -l"
echo "  - Check progress: ls docs/brain/EPIC-W7-*/00-hotspots.md | wc -l"
echo ""
echo "=========================================="
echo "MONITORING COMMANDS"
echo "=========================================="
echo ""
echo "Active sessions:  screen -ls | grep 'p0-'"
echo "Session count:    screen -ls | grep 'p0-' | wc -l"
echo "Completed epics:  ls docs/brain/EPIC-W7-*/00-hotspots.md | wc -l"
echo "View log:         tail -f logs/phase0/EPIC-W7-001.log"
echo "Attach session:   screen -r p0-001"
echo "Check bobcoins:   grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase0/EPIC-W7-*.log | tail -20"
echo ""
echo "=========================================="
echo "LAMPORT EVENT TRACKING"
echo "=========================================="
echo ""
echo "Event log:        .lamport/wave7/event_log.jsonl"
echo "Monitor events:   tail -f .lamport/wave7/event_log.jsonl"
echo "Phase 0 events:   grep '\"phase\":\"0\"' .lamport/wave7/event_log.jsonl | wc -l"
echo ""
echo "=========================================="
echo "SUCCESS CRITERIA"
echo "=========================================="
echo ""
echo "1. All 161 screen sessions complete (screen -ls shows no p0-* sessions)"
echo "2. All 161 00-hotspots.md files exist"
echo "3. All 161 manifest.json files exist"
echo "4. No errors in logs"
echo "5. Bobcoin usage reasonable (<50 per epic)"
echo "6. Lamport events logged for all phase transitions"
echo ""
echo "=========================================="
echo "NEXT STEPS"
echo "=========================================="
echo ""
echo "After Phase 0 completes (161/161):"
echo "  1. Verify all files: ls docs/brain/EPIC-W7-*/00-hotspots.md | wc -l"
echo "  2. Check for failures: grep -l 'ERROR\|FAILED' logs/phase0/*.log"
echo "  3. Run Phase 1 pilot: ./scripts/wave7/launch_phase1_pilot.sh"
echo "  4. After pilot success: ./scripts/wave7/launch_phase1_all.sh"

# Made with Bob - Building-Blocks Method (copied from Wave 4)