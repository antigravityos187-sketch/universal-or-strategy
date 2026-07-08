#!/bin/bash
# Verify Phase 0 completion for all 161 Wave 7 epics
# Checks for 00-hotspots.md files and manifest.json status

set -e
cd /home/malhitticrypto/universal-or-strategy

echo "=========================================="
echo "Wave 7 Phase 0 Completion Verification"
echo "=========================================="
echo ""

# Count completed epics (have 00-hotspots.md)
COMPLETED=$(ls docs/brain/EPIC-W7-*/00-hotspots.md 2>/dev/null | wc -l)
echo "[*] Completed epics: $COMPLETED/161"

# List incomplete epics
echo ""
echo "[*] Incomplete epics:"
for i in $(seq -f "%03g" 1 161); do
    EPIC_ID="EPIC-W7-$i"
    if [ ! -f "docs/brain/$EPIC_ID/00-hotspots.md" ]; then
        echo "    - $EPIC_ID"
    fi
done

# Check active screen sessions
echo ""
echo "[*] Active screen sessions:"
screen -ls | grep "phase0_w7" || echo "    (none)"

# Check recent log activity (last 5 minutes)
echo ""
echo "[*] Recent log activity (last 5 minutes):"
find logs/phase0 -name "EPIC-W7-*.log" -mmin -5 2>/dev/null | wc -l | xargs echo "    Active logs:"

# Show completion percentage
PERCENT=$((COMPLETED * 100 / 161))
echo ""
echo "=========================================="
echo "Completion: $COMPLETED/161 ($PERCENT%)"
echo "=========================================="

# Exit with status
if [ $COMPLETED -eq 161 ]; then
    echo ""
    echo "✅ SUCCESS: All 161 epics completed Phase 0!"
    exit 0
else
    REMAINING=$((161 - COMPLETED))
    echo ""
    echo "⚠️  INCOMPLETE: $REMAINING epics remaining"
    exit 1
fi

# Made with Bob
