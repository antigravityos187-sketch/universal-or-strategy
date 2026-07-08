#!/bin/bash
# Regenerate 24 Broken Scripts from Working Template (EPIC-002)
# Created: 2026-06-18T04:41:00Z
# Building-Blocks Method: Copy working script, replace epic-specific values

set -e

TEMPLATE="/home/malhitticrypto/universal-or-strategy/scripts/wave6/_p1_epic_ccn_002.sh"
SCRIPTS_DIR="/home/malhitticrypto/universal-or-strategy/scripts/wave6"

EPICS=(
    "001" "004" "016" "020" "021" "028"
    "050" "051" "052" "053" "054" "055" "056" "057" "058" "059"
    "060" "061" "070" "073" "076" "077" "078" "079"
)

echo "=========================================="
echo "Regenerating 24 Scripts from Working Template"
echo "Template: $TEMPLATE"
echo "=========================================="
echo ""

REGENERATED=0
FAILED=0

for EPIC_NUM in "${EPICS[@]}"; do
    EPIC_ID="EPIC-CCN-${EPIC_NUM}"
    AGENT_ID="wave6-p1-${EPIC_NUM}"
    OUTPUT="$SCRIPTS_DIR/_p1_epic_ccn_${EPIC_NUM}.sh"
    
    echo "Regenerating $EPIC_ID..."
    
    # Copy template and replace values
    sed "s/EPIC-CCN-002/$EPIC_ID/g; s/wave6-p1-002/$AGENT_ID/g" "$TEMPLATE" > "$OUTPUT"
    
    # Make executable
    chmod +x "$OUTPUT"
    
    # Verify file created
    if [ -f "$OUTPUT" ] && [ -s "$OUTPUT" ]; then
        LINE_COUNT=$(wc -l < "$OUTPUT")
        echo "  ✅ Created ($LINE_COUNT lines)"
        ((REGENERATED++))
    else
        echo "  ❌ Failed to create"
        ((FAILED++))
    fi
done

echo ""
echo "=========================================="
echo "Regeneration Summary"
echo "=========================================="
echo "Regenerated: $REGENERATED/24"
echo "Failed: $FAILED/24"
echo ""

if [ $REGENERATED -eq 24 ]; then
    echo "✅ All 24 scripts regenerated successfully"
    exit 0
else
    echo "⚠️  Some scripts failed to regenerate"
    exit 1
fi

# Made with Bob
