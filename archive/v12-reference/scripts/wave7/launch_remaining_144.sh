#!/bin/bash
# Launch remaining 144 epics (161 total - 17 complete)
# Uses 16 fresh API keys with even distribution

set -e
cd /home/malhitticrypto/universal-or-strategy

echo "=== Wave 7 Remaining Epic Launch ==="
echo "Target: 144 remaining epics (161 total - 17 complete)"
echo "API Keys: 16 fresh keys (~10 epics each)"
echo "Start time: $(date)"
echo ""

# Count completed epics
COMPLETED=$(find docs/brain/EPIC-W7-* -name '00-hotspots.md' 2>/dev/null | wc -l)
echo "[*] Currently completed: $COMPLETED/161"
echo ""

# Launch all 161 epics (script will skip completed ones)
LAUNCHED=0
SKIPPED=0

for i in $(seq -f "%03g" 1 161); do
    EPIC_ID="EPIC-W7-$i"
    SCRIPT="scripts/wave7/_p0_$i.sh"
    
    # Check if already complete
    if [ -f "docs/brain/$EPIC_ID/00-hotspots.md" ]; then
        echo "[SKIP] $EPIC_ID (already complete)"
        ((SKIPPED++))
        continue
    fi
    
    # Launch epic
    echo "[LAUNCH] $EPIC_ID"
    bash "$SCRIPT" &
    ((LAUNCHED++))
    
    # Stagger launches (12 seconds between each)
    sleep 12
done

echo ""
echo "=== Launch Summary ==="
echo "Launched: $LAUNCHED epics"
echo "Skipped: $SKIPPED epics (already complete)"
echo "Total: $((LAUNCHED + SKIPPED))/161"
echo ""
echo "Monitor progress:"
echo "  watch -n 60 'find docs/brain/EPIC-W7-* -name \"00-hotspots.md\" 2>/dev/null | wc -l'"
echo ""
echo "End time: $(date)"

# Made with Bob
