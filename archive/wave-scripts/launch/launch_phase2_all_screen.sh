#!/bin/bash
# Launch all Phase 2 (Architecture Planning) scripts in screen sessions
# Generated from Phase 1.5 success pattern

cd /home/malhitticrypto/universal-or-strategy

echo "Starting Phase 2 (Architecture Planning) for 9 epics..."

# Make scripts executable
chmod +x _p2_*.sh

# Launch each epic in its own screen session
for epic in 107 108 109 110 111 112 113 114 115; do
    screen_name="phase2_epic_${epic}"
    echo "Launching EPIC-CCN-${epic} in screen: ${screen_name}"
    screen -dmS "${screen_name}" bash -l -c "cd /home/malhitticrypto/universal-or-strategy && ./_p2_${epic}.sh"
    sleep 2
done

echo ""
echo "All Phase 2 scripts launched!"
echo ""
echo "Monitor with:"
echo "  screen -ls                    # List all sessions"
echo "  screen -r phase2_epic_107     # Attach to specific epic"
echo "  screen -S phase2_epic_107 -X stuff '^C'  # Kill specific session"
echo ""
echo "Check logs:"
echo "  tail -f logs/phase2/EPIC-CCN-*.log"
echo ""
