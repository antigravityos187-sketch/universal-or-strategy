#!/bin/bash
# Final Relaunch of 24 Fixed Epics
# Created: 2026-06-18T04:45:00Z
# Scripts regenerated from working EPIC-002 template

EPICS="001 004 016 020 021 028 050 051 052 053 054 055 056 057 058 059 060 061 070 073 076 077 078 079"

echo "=========================================="
echo "Launching 24 Fixed Epics (Final)"
echo "Time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo "=========================================="

for EPIC_NUM in $EPICS; do
    SCRIPT="/home/malhitticrypto/universal-or-strategy/scripts/wave6/_p1_epic_ccn_${EPIC_NUM}.sh"
    LOG="/home/malhitticrypto/universal-or-strategy/logs/wave6/phase1/EPIC-CCN-${EPIC_NUM}.log"
    
    echo "Launching EPIC-CCN-${EPIC_NUM}..."
    nohup bash "$SCRIPT" > "$LOG" 2>&1 &
    sleep 0.5
done

echo ""
echo "=========================================="
echo "All 24 epics launched"
echo "Monitor with: python scripts/wave6/check_wave6_only_status.py"
echo "=========================================="

# Made with Bob
