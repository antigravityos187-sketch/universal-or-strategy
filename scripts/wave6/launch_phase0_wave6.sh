#!/bin/bash
# Wave 6 Phase 0 Master Launch Script
# V12.52 Protocol - Clean Slate Execution
# 78 epics (001-026, 028-080, excluding 024, 027)

set -e

echo "=== Wave 6 Phase 0 Master Launch ==="
echo "Date: $(date)"
echo "Epics: 78 (excluding 024, 027)"
echo "Staggered delay: 9 seconds"
echo "Peak concurrency: ~50 agents"
echo ""

cd ~/universal-or-strategy

# Epic list (excluding 024, 027)
EPICS=(
    001 002 003 004 005 006 007 008 009 010
    011 012 013 014 015 016 017 018 019 020
    021 022 023 025 026 028 029 030
    031 032 033 034 035 036 037 038 039 040
    041 042 043 044 045 046 047 048 049 050
    051 052 053 054 055 056 057 058 059 060
    061 062 063 064 065 066 067 068 069 070
    071 072 073 074 075 076 077 078 079 080
)

TOTAL=${#EPICS[@]}
echo "Total epics to launch: $TOTAL"
echo ""

# Launch counter
LAUNCHED=0
START_TIME=$(date +%s)

# Launch all epics with staggered delays
for EPIC_NUM in "${EPICS[@]}"; do
    EPIC_ID="EPIC-CCN-${EPIC_NUM}"
    SCRIPT="scripts/wave6/_p0_epic_ccn_${EPIC_NUM}.sh"
    LOG="logs/wave6/phase0/${EPIC_ID}.log"
    
    # Create log directory
    mkdir -p logs/wave6/phase0
    
    # Launch in screen session
    SCREEN_NAME="w6p0-${EPIC_NUM}"
    screen -dmS "$SCREEN_NAME" bash -l -c "cd ~/universal-or-strategy && bash $SCRIPT 2>&1 | tee $LOG"
    
    LAUNCHED=$((LAUNCHED + 1))
    echo "[$LAUNCHED/$TOTAL] Launched $EPIC_ID (screen: $SCREEN_NAME)"
    
    # Staggered delay (9 seconds)
    if [ $LAUNCHED -lt $TOTAL ]; then
        sleep 9
    fi
done

END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

echo ""
echo "=== Launch Complete ==="
echo "Total launched: $LAUNCHED epics"
echo "Launch duration: ${DURATION}s"
echo "First script launched at: $(date -d @$START_TIME)"
echo "Last script launched at: $(date -d @$END_TIME)"
echo ""
echo "Monitoring:"
echo "  - Screen sessions: screen -ls | grep w6p0"
echo "  - File count: ls docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l"
echo "  - Logs: tail -f logs/wave6/phase0/EPIC-CCN-001.log"
echo ""
echo "Next: Monitor every 4 minutes starting 1 minute after first launch"
echo "First check at: $(date -d @$((START_TIME + 60)))"

# Made with Bob
