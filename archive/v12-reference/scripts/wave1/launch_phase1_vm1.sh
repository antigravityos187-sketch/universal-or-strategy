#!/bin/bash
# Launch Phase 1 for EPIC-001 through EPIC-005 on VM1
# Uses screen sessions for parallel execution

set -e
cd /home/malhitticrypto/universal-or-strategy

echo "Launching Phase 1 for EPIC-001 through EPIC-005..."
echo ""

# Create log directory
mkdir -p logs/phase1

# Launch each epic in a screen session
for i in 01 02 03 04 05; do
    session_name="p1-${i}"
    script_name="_p1_${i}.sh"
    
    echo "Launching ${session_name}..."
    screen -dmS "$session_name" bash -l "$script_name"
    sleep 1
done

echo ""
echo "✅ Launched 5 screen sessions"
echo ""
echo "Monitor with:"
echo "  screen -ls"
echo ""
echo "View logs:"
echo "  tail -f logs/phase1/EPIC-*.log"
echo ""
echo "Check completion:"
echo "  ls docs/brain/EPIC-{001..005}/00-scope.md | wc -l"

# Made with Bob