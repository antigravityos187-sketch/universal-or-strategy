#!/bin/bash
# Relaunch Wave 7 - All 161 epics with proper background execution

echo '=== Wave 7 Relaunch ==='
echo 'Target: 161 epics'
echo 'Start time:' $(date)
echo ''

COMPLETED=$(find docs/brain/EPIC-W7-* -name '00-hotspots.md' 2>/dev/null | wc -l)
echo "Currently completed: $COMPLETED/161"
echo ''

# Launch all epics in background
for i in $(seq -f '%03g' 1 161); do
    EPIC="EPIC-W7-$i"
    if [ ! -f "docs/brain/$EPIC/00-hotspots.md" ]; then
        echo "[LAUNCH] $EPIC"
        nohup bash _p0_$i.sh > /dev/null 2>&1 &
        sleep 12
    else
        echo "[SKIP] $EPIC (already complete)"
    fi
done

echo ''
echo 'All epics launched!'
echo 'Monitor with: find docs/brain/EPIC-W7-* -name 00-hotspots.md | wc -l'

# Made with Bob
