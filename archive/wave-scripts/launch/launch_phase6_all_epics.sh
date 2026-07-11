#!/bin/bash
# Launch Phase 6 (Epic-Level Reviews) for all 7 epics
# Based on launch_remaining_epics.sh pattern
set -e

echo "=== Launching Phase 6 for All 7 Epics ==="
echo "Started: $(date)"
echo ""
echo "Processing: 107, 108, 109, 111, 112, 113, 114"
echo ""

cd /home/malhitticrypto/universal-or-strategy

# Function to wait for screen session completion
wait_for_completion() {
    local session_name=$1
    while screen -list | grep -q "$session_name"; do
        sleep 10
    done
}

# Function to check epic review result
check_epic_review() {
    local epic=$1
    local completion_file="docs/brain/EPIC-CCN-${epic}/05-completion-report.md"
    
    if [ ! -f "$completion_file" ]; then
        echo "❌ Completion report not found: $completion_file"
        return 1
    fi
    
    if grep -qi "fail\|blocked" "$completion_file"; then
        echo "⚠️ EPIC-${epic} review has issues"
        return 1
    fi
    
    echo "✅ EPIC-${epic} review complete"
    return 0
}

# Launch all 7 epic reviews sequentially
for epic in 107 108 109 111 112 113 114; do
    echo "=== EPIC-CCN-${epic} Phase 6 Review ==="
    
    screen -dmS p6_${epic} bash -l _p6_${epic}.sh
    wait_for_completion p6_${epic}
    
    if check_epic_review ${epic}; then
        echo "✅ EPIC-CCN-${epic} Phase 6 COMPLETE"
    else
        echo "⚠️ EPIC-CCN-${epic} Phase 6 has issues (check completion report)"
    fi
    
    echo ""
done

echo "=== Phase 6 Complete ==="
echo "Completed: $(date)"
echo ""
echo "Summary:"
echo "  All 7 epic reviews executed"
echo "  Check completion reports in: docs/brain/EPIC-CCN-*/05-completion-report.md"
echo "  Check logs in: logs/phase6/"

# Made with Bob
