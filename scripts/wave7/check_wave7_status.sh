#!/bin/bash
# Wave 7 Status Monitor
# Checks progress across all phases for all 161 epics
#
# Usage: ./scripts/wave7/check_wave7_status.sh [phase]
#   phase: Optional phase number (0-6) to check specific phase
#          If omitted, shows overview of all phases

PHASE=${1:-"all"}
TOTAL_EPICS=161

echo "=========================================="
echo "Wave 7 Status Monitor"
echo "=========================================="
echo "Total Epics: ${TOTAL_EPICS}"
echo "Time: $(date)"
echo ""

# Function to count completed epics for a phase
count_phase_completion() {
    local phase=$1
    local file_pattern=$2
    local count=$(ls docs/brain/EPIC-W7-*/${file_pattern} 2>/dev/null | wc -l)
    echo $count
}

# Function to check active screen sessions for a phase
count_active_sessions() {
    local phase=$1
    local count=$(screen -ls | grep "p${phase}-" | wc -l)
    echo $count
}

if [ "$PHASE" == "all" ]; then
    echo "=========================================="
    echo "PHASE OVERVIEW"
    echo "=========================================="
    echo ""
    
    # Phase 0: Hotspot Analysis
    P0_COMPLETE=$(count_phase_completion 0 "00-hotspots.md")
    P0_ACTIVE=$(count_active_sessions 0)
    echo "Phase 0 (Hotspot):     ${P0_COMPLETE}/${TOTAL_EPICS} complete, ${P0_ACTIVE} active"
    
    # Phase 1: Scope Definition
    P1_COMPLETE=$(count_phase_completion 1 "00-scope.md")
    P1_ACTIVE=$(count_active_sessions 1)
    echo "Phase 1 (Scope):       ${P1_COMPLETE}/${TOTAL_EPICS} complete, ${P1_ACTIVE} active"
    
    # Phase 1.5: Boundary Validation
    P1_5_COMPLETE=$(count_phase_completion 1 "01-scope-boundary.md")
    P1_5_ACTIVE=$(count_active_sessions 1.5)
    echo "Phase 1.5 (Boundary):  ${P1_5_COMPLETE}/${TOTAL_EPICS} complete, ${P1_5_ACTIVE} active"
    
    # Phase 2: Architecture Planning
    P2_COMPLETE=$(count_phase_completion 2 "02-architecture-plan.md")
    P2_ACTIVE=$(count_active_sessions 2)
    echo "Phase 2 (Architecture):${P2_COMPLETE}/${TOTAL_EPICS} complete, ${P2_ACTIVE} active"
    
    # Phase 3: DNA Audit
    P3_COMPLETE=$(count_phase_completion 3 "03-audit-report.md")
    P3_ACTIVE=$(count_active_sessions 3)
    echo "Phase 3 (Audit):       ${P3_COMPLETE}/${TOTAL_EPICS} complete, ${P3_ACTIVE} active"
    
    # Phase 4: Ticket Generation
    P4_COMPLETE=$(count_phase_completion 4 "04-tickets.md")
    P4_ACTIVE=$(count_active_sessions 4)
    echo "Phase 4 (Tickets):     ${P4_COMPLETE}/${TOTAL_EPICS} complete, ${P4_ACTIVE} active"
    
    # Phase 5: Execution (check for any ticket completion files)
    P5_COMPLETE=$(ls docs/brain/EPIC-W7-*/ticket-*-completion.md 2>/dev/null | wc -l)
    P5_ACTIVE=$(count_active_sessions 5)
    echo "Phase 5 (Execute):     ${P5_COMPLETE} tickets complete, ${P5_ACTIVE} active"
    
    # Phase 5.V: Verification
    P5V_COMPLETE=$(ls docs/brain/EPIC-W7-*/ticket-*-verification.md 2>/dev/null | wc -l)
    P5V_ACTIVE=$(count_active_sessions 5v)
    echo "Phase 5.V (Verify):    ${P5V_COMPLETE} tickets verified, ${P5V_ACTIVE} active"
    
    # Phase 6: Final Review
    P6_COMPLETE=$(count_phase_completion 6 "05-completion-report.md")
    P6_ACTIVE=$(count_active_sessions 6)
    echo "Phase 6 (Review):      ${P6_COMPLETE}/${TOTAL_EPICS} complete, ${P6_ACTIVE} active"
    
    echo ""
    echo "=========================================="
    echo "OVERALL PROGRESS"
    echo "=========================================="
    echo ""
    
    # Calculate overall completion (epics with Phase 6 complete)
    OVERALL_COMPLETE=$P6_COMPLETE
    OVERALL_PERCENT=$((OVERALL_COMPLETE * 100 / TOTAL_EPICS))
    echo "Wave 7 Completion: ${OVERALL_COMPLETE}/${TOTAL_EPICS} (${OVERALL_PERCENT}%)"
    
    # Check for failures
    FAILED_LOGS=$(grep -l 'ERROR\|FAILED' logs/phase*/*.log 2>/dev/null | wc -l)
    if [ $FAILED_LOGS -gt 0 ]; then
        echo "⚠️  WARNING: ${FAILED_LOGS} logs contain errors"
        echo "    Check: grep -l 'ERROR\|FAILED' logs/phase*/*.log"
    fi
    
    echo ""
    echo "=========================================="
    echo "LAMPORT EVENT TRACKING"
    echo "=========================================="
    echo ""
    
    if [ -f .lamport/wave7/event_log.jsonl ]; then
        TOTAL_EVENTS=$(wc -l < .lamport/wave7/event_log.jsonl)
        echo "Total Events: ${TOTAL_EVENTS}"
        echo "Latest Events:"
        tail -5 .lamport/wave7/event_log.jsonl | jq -r '"\(.timestamp) | \(.epic_id) | Phase \(.phase) | \(.event_type) | \(.status)"'
    else
        echo "No Lamport event log found"
    fi
    
else
    # Show detailed status for specific phase
    echo "=========================================="
    echo "PHASE ${PHASE} DETAILED STATUS"
    echo "=========================================="
    echo ""
    
    # Determine file pattern based on phase
    case $PHASE in
        0) FILE_PATTERN="00-hotspots.md" ;;
        1) FILE_PATTERN="00-scope.md" ;;
        1.5) FILE_PATTERN="01-scope-boundary.md" ;;
        2) FILE_PATTERN="02-architecture-plan.md" ;;
        3) FILE_PATTERN="03-audit-report.md" ;;
        4) FILE_PATTERN="04-tickets.md" ;;
        5) FILE_PATTERN="ticket-*-completion.md" ;;
        5v) FILE_PATTERN="ticket-*-verification.md" ;;
        6) FILE_PATTERN="05-completion-report.md" ;;
        *) echo "Invalid phase: ${PHASE}"; exit 1 ;;
    esac
    
    # Count completed
    COMPLETED=$(ls docs/brain/EPIC-W7-*/${FILE_PATTERN} 2>/dev/null | wc -l)
    ACTIVE=$(count_active_sessions $PHASE)
    
    echo "Completed: ${COMPLETED}/${TOTAL_EPICS}"
    echo "Active Sessions: ${ACTIVE}"
    echo ""
    
    # Show incomplete epics
    echo "Incomplete Epics:"
    for i in $(seq -f "%03g" 1 ${TOTAL_EPICS}); do
        EPIC_ID="EPIC-W7-${i}"
        if [ ! -f "docs/brain/${EPIC_ID}/${FILE_PATTERN}" ]; then
            echo "  - ${EPIC_ID}"
        fi
    done | head -20
    
    if [ $((TOTAL_EPICS - COMPLETED)) -gt 20 ]; then
        echo "  ... and $((TOTAL_EPICS - COMPLETED - 20)) more"
    fi
    
    echo ""
    echo "Recent Logs:"
    ls -lt logs/phase${PHASE}/*.log 2>/dev/null | head -5
fi

echo ""
echo "=========================================="
echo "QUICK COMMANDS"
echo "=========================================="
echo ""
echo "Monitor active:   screen -ls | grep 'p[0-9]-'"
echo "Check logs:       tail -f logs/phase0/EPIC-W7-001.log"
echo "Count complete:   ls docs/brain/EPIC-W7-*/00-hotspots.md | wc -l"
echo "Find errors:      grep -l 'ERROR\|FAILED' logs/phase*/*.log"
echo "Lamport events:   tail -f .lamport/wave7/event_log.jsonl"

# Made with Bob - Building-Blocks Method (copied from Wave 4)