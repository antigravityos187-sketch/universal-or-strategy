#!/bin/bash
# Launch Phase 0 for EPIC-006 through EPIC-015 in screen sessions
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "Launching Phase 0 for EPIC-006 through EPIC-015..."
echo "Each epic will run in a detached screen session"
echo ""

# Launch each epic in a screen session
for epic in 006 007 008 009 010 011 012 013 014 015; do
    echo "Starting EPIC-$epic in screen session p0-$epic..."
    screen -dmS "p0-$epic" bash -l -c "cd /home/malhitticrypto/universal-or-strategy && ./_p0_${epic}_corrected.sh 2>&1 | tee logs/phase0/EPIC-${epic}.log"
    sleep 1
done

echo ""
echo "✅ All 10 epics launched in screen sessions"
echo ""
echo "Monitor with:"
echo "  screen -ls                    # List all sessions"
echo "  screen -r p0-006              # Attach to specific session"
echo "  tail -f logs/phase0/EPIC-006.log  # Watch log file"
echo ""
echo "Check completion:"
echo "  screen -ls | grep -c 'p0-'    # Count running sessions (0 = all done)"
echo "  ls docs/brain/EPIC-*/00-hotspots.md | wc -l  # Count output files"

# Made with Bob
