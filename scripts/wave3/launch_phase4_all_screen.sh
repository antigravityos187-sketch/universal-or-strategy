#!/bin/bash
# Launch all Phase 4 (Ticket Generation) scripts in screen sessions
# Wave 3: All 10 epics active (no skips)

cd /home/malhitticrypto/universal-or-strategy

echo "Starting Phase 4 (Ticket Generation) for 10 epics..."

# Make scripts executable
chmod +x _p4_*.sh

# Launch each epic in its own screen session
for epic in 116 117 118 119 120 121 122 123 124 125; do
    screen_name="phase4_epic_${epic}"
    echo "Launching EPIC-CCN-${epic} in screen: ${screen_name}"
    screen -dmS "${screen_name}" bash -l -c "cd /home/malhitticrypto/universal-or-strategy && ./_p4_${epic}.sh"
    sleep 2
done

echo ""
echo "All Phase 4 scripts launched!"
echo ""
echo "Monitor with:"
echo "  screen -ls                    # List all sessions"
echo "  screen -r phase4_epic_116     # Attach to specific epic"
echo "  screen -S phase4_epic_116 -X stuff '^C'  # Kill specific session"
echo ""
echo "Check logs:"
echo "  tail -f logs/phase4/EPIC-CCN-*.log"
echo ""
