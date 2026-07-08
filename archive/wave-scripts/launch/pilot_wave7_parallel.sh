#!/bin/bash
# Wave 7 Phase 0 - Parallel Pilot Test (12-second stagger)
# Tests parallel execution pattern with 3 DIFFERENT incomplete epics
set -e

echo "================================================================================"
echo "WAVE 7 PHASE 0 - PARALLEL PILOT TEST (12-SECOND STAGGER)"
echo "================================================================================"
echo ""
echo "Testing parallel execution with 3 NEW epics:"
echo "  1. EPIC-W7-003 (Different from sequential pilot)"
echo "  2. EPIC-W7-051 (Different from sequential pilot)"
echo "  3. EPIC-W7-101 (Different from sequential pilot)"
echo ""
echo "Launch pattern: 12-second stagger, background execution"
echo ""

# Create logs directory
mkdir -p logs/phase0_pilot

# Test epics (DIFFERENT from sequential pilot 002, 050, 100)
EPICS=("003" "051" "101")
TOTAL=${#EPICS[@]}

# Launch counter
LAUNCHED=0

for epic_num in "${EPICS[@]}"; do
    EPIC_ID="EPIC-W7-${epic_num}"
    SCRIPT="_p0_${epic_num}.sh"
    
    if [ ! -f "$SCRIPT" ]; then
        echo "⚠️  Script $SCRIPT not found - skipping"
        continue
    fi
    
    LAUNCHED=$((LAUNCHED + 1))
    echo "[$LAUNCHED/$TOTAL] Launching $EPIC_ID (background)"
    
    # Launch in background with log redirection
    /usr/bin/bash "$SCRIPT" > "logs/phase0_pilot/${EPIC_ID}.log" 2>&1 &
    PID=$!
    echo "   PID: $PID"
    echo $PID >> logs/phase0_pilot/pids.txt
    
    # 12-second stagger (protocol requirement)
    if [ $LAUNCHED -lt $TOTAL ]; then
        echo "   Waiting 12 seconds before next launch..."
        sleep 12
    fi
done

echo ""
echo "================================================================================"
echo "✅ LAUNCHED $LAUNCHED PILOT EPICS IN PARALLEL"
echo "================================================================================"
echo ""
echo "Monitor progress:"
echo "  - Check logs: tail -f logs/phase0_pilot/EPIC-W7-*.log"
echo "  - Count complete: /usr/bin/python3 -c \"import os; print(len([f for f in os.listdir('docs/brain') if f.startswith('EPIC-W7-') and os.path.exists(f'docs/brain/{f}/00-hotspots.md')]))\""
echo "  - Check PIDs: cat logs/phase0_pilot/pids.txt"
echo ""
echo "Expected completion: ~6 minutes (3 epics × ~2 min each, parallel)"
echo ""

# Wait for all background jobs to complete
echo "Waiting for all pilot epics to complete..."
wait

echo ""
echo "================================================================================"
echo "PILOT TEST COMPLETE"
echo "================================================================================"
echo ""

# Verify completion
COMPLETE=0
for epic_num in "${EPICS[@]}"; do
    EPIC_ID="EPIC-W7-${epic_num}"
    if [ -f "docs/brain/${EPIC_ID}/00-hotspots.md" ]; then
        echo "✅ $EPIC_ID complete"
        COMPLETE=$((COMPLETE + 1))
    else
        echo "❌ $EPIC_ID failed"
    fi
done

echo ""
if [ $COMPLETE -eq $TOTAL ]; then
    echo "🎉 SUCCESS! All $TOTAL pilot epics passed!"
    echo ""
    echo "Ready to launch full Wave 7 execution (148 remaining epics)"
    echo "Run: /usr/bin/bash launch_wave7_parallel.sh"
else
    echo "⚠️  Only $COMPLETE/$TOTAL pilot epics completed"
    echo "Check logs in logs/phase0_pilot/ for errors"
    exit 1
fi
echo ""

# Made with Bob