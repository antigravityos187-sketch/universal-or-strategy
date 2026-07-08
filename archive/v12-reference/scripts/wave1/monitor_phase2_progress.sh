#!/bin/bash
# Real-time monitoring script for Phase 2 rolling launch

echo "=========================================="
echo "Wave 1 Phase 2 Progress Monitor"
echo "=========================================="
echo ""

while true; do
    clear
    echo "=========================================="
    echo "Wave 1 Phase 2 Progress Monitor"
    echo "Updated: $(date '+%Y-%m-%d %H:%M:%S')"
    echo "=========================================="
    echo ""
    
    # Count running agents
    running=$(screen -ls | grep -c 'p2-' 2>/dev/null || echo "0")
    echo "Running agents: $running"
    
    # Count completed epics
    completed=$(ls docs/brain/EPIC-*/02-architecture-plan.md 2>/dev/null | wc -l)
    echo "Completed epics: $completed"
    
    # Count total scripts
    total=$(ls scripts/wave1/_p2_*.sh 2>/dev/null | wc -l)
    echo "Total epics: $total"
    
    # Calculate progress
    if [ $total -gt 0 ]; then
        progress=$((completed * 100 / total))
        echo "Progress: ${progress}%"
    fi
    
    echo ""
    echo "VM Status:"
    uptime
    echo ""
    free -h | grep -E 'Mem:|Swap:'
    
    echo ""
    echo "Recent completions (last 5):"
    ls -lt docs/brain/EPIC-*/02-architecture-plan.md 2>/dev/null | head -5 | awk '{print "  " $9}'
    
    echo ""
    echo "=========================================="
    echo "Press Ctrl+C to exit"
    echo "=========================================="
    
    sleep 10
done

# Made with Bob
