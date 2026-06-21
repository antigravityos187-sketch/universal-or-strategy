#!/bin/bash
# Regenerate 24 Scripts - Simple Version (No set -e)
# Created: 2026-06-18T04:44:00Z

TEMPLATE="/home/malhitticrypto/universal-or-strategy/scripts/wave6/_p1_epic_ccn_002.sh"
SCRIPTS_DIR="/home/malhitticrypto/universal-or-strategy/scripts/wave6"

EPICS="001 004 016 020 021 028 050 051 052 053 054 055 056 057 058 059 060 061 070 073 076 077 078 079"

echo "=========================================="
echo "Regenerating 24 Scripts (Simple)"
echo "=========================================="

for EPIC_NUM in $EPICS; do
    EPIC_ID="EPIC-CCN-${EPIC_NUM}"
    AGENT_ID="wave6-p1-${EPIC_NUM}"
    OUTPUT="$SCRIPTS_DIR/_p1_epic_ccn_${EPIC_NUM}.sh"
    
    echo "Processing $EPIC_ID..."
    sed "s/EPIC-CCN-002/$EPIC_ID/g; s/wave6-p1-002/$AGENT_ID/g" "$TEMPLATE" > "$OUTPUT"
    chmod +x "$OUTPUT"
    echo "  Done: $(wc -l < "$OUTPUT") lines"
done

echo ""
echo "=========================================="
echo "Complete"
echo "=========================================="

# Made with Bob
