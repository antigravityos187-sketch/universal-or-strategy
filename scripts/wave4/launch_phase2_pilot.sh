#!/bin/bash
# Pilot test script for Phase 2 - EPIC-CCN-001 ONLY
# Tests Phase 2 script before launching full wave
#
# SUCCESS CRITERIA:
# 1. File created: docs/brain/EPIC-CCN-001/02-architecture-plan.md
# 2. File verified on disk (ls -lh shows file size)
# 3. Bobcoin usage reported in log
# 4. No errors in log
# 5. Sequential thinking MCP used (check log for "thought" or "sequentialthinking")

set -e

PHASE=2
EPIC="001"

echo "[$(date)] Starting Phase 2 PILOT TEST - EPIC-CCN-${EPIC}"
echo "[$(date)] This is a SINGLE epic test before full wave launch"

# Launch pilot epic in screen session
screen -dmS p${PHASE}-pilot bash -l -c \
    "./_p${PHASE}_${EPIC}.sh 2>&1 | tee logs/phase${PHASE}/EPIC-CCN-${EPIC}.log"

echo "[$(date)] Pilot epic launched"
echo ""
echo "MONITORING COMMANDS:"
echo "  Check status:    screen -ls"
echo "  View log:        tail -f logs/phase${PHASE}/EPIC-CCN-${EPIC}.log"
echo "  Attach session:  screen -r p${PHASE}-pilot"
echo "  Verify file:     ls -lh docs/brain/EPIC-CCN-${EPIC}/02-architecture-plan.md"
echo "  Check bobcoins:  grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase${PHASE}/EPIC-CCN-${EPIC}.log"
echo "  Check seq think: grep -i 'thought\|sequentialthinking' logs/phase${PHASE}/EPIC-CCN-${EPIC}.log"
echo ""
echo "WAIT 25 MINUTES (Phase 2 = 25 min per epic)"
echo "Then verify success before launching full wave"

# Made with Bob
