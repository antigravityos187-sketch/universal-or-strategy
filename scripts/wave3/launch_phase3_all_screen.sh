#!/bin/bash
# Launch all Phase 3 (DNA & PR Audit) scripts in screen sessions
# CORRECTED: Uses Claude advanced mode (copied from Wave 2 working pattern)

cd /home/malhitticrypto/universal-or-strategy

echo "Starting Phase 3 (DNA & PR Audit) for 10 epics..."

# Make scripts executable
chmod +x _p3_*.sh

# Launch each epic in its own screen session
for epic in 116 117 118 119 120 121 122 123 124 125; do
    screen_name="phase3_epic_${epic}"
    echo "Launching EPIC-CCN-${epic} in screen: ${screen_name}"
    screen -dmS "${screen_name}" bash -l -c "cd /home/malhitticrypto/universal-or-strategy && ./_p3_${epic}.sh"
    sleep 2
done

echo ""
echo "All Phase 3 scripts launched!"
echo ""
echo "Monitor with:"
echo "  screen -ls                    # List all sessions"
echo "  screen -r phase3_epic_116     # Attach to specific epic"
echo "  screen -S phase3_epic_116 -X stuff '^C'  # Kill specific session"
echo ""
echo "Check logs:"
echo "  tail -f logs/phase3/EPIC-CCN-*.log"
echo ""
