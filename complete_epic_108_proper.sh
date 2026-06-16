#!/bin/bash
# Complete EPIC-108 using the proven pattern from launch_remaining_epics.sh
set -e

echo "=== EPIC-CCN-108 Completion ==="
echo "Started: $(date)"
echo ""

cd /home/malhitticrypto/universal-or-strategy

# Function to wait for screen session completion
wait_for_completion() {
    local session_name=$1
    while screen -list | grep -q "$session_name"; do
        sleep 10
    done
}

# Function to check validation result
check_validation() {
    local epic=$1
    local ticket=$2
    local verification_file="docs/brain/EPIC-CCN-${epic}/ticket-${ticket}-verification.md"
    
    if [ ! -f "$verification_file" ]; then
        echo "❌ Verification file not found: $verification_file"
        return 1
    fi
    
    if grep -q "Verdict.*FAIL" "$verification_file"; then
        echo "❌ TICKET-${ticket} FAILED validation"
        return 1
    fi
    
    echo "✅ TICKET-${ticket} passed validation"
    return 0
}

# EPIC-CCN-108 (5 tickets)
echo "=== EPIC-CCN-108 (5 tickets) ==="

# First revalidate T1 (already executed, just needs fresh validation)
echo "Revalidating TICKET-1..."
screen -dmS p5v_108_t1 bash -l _p5v_108_t1.sh
wait_for_completion p5v_108_t1
echo "  Revalidation complete"

if ! check_validation 108 1; then
    echo "⚠️ EPIC-108 stopped at TICKET-1 revalidation"
    echo "EPIC-108: BLOCKED" > /tmp/epic_108_status.txt
    exit 1
fi

# Now execute T2-T5
for ticket in 2 3 4 5; do
    echo "Processing TICKET-${ticket}..."
    
    # Execute ticket
    screen -dmS p5_108_t${ticket} bash -l _p5_108_t${ticket}.sh
    wait_for_completion p5_108_t${ticket}
    echo "  Execution complete"
    
    # Validate ticket
    screen -dmS p5v_108_t${ticket} bash -l _p5v_108_t${ticket}.sh
    wait_for_completion p5v_108_t${ticket}
    echo "  Validation complete"
    
    # Check result
    if ! check_validation 108 ${ticket}; then
        echo "⚠️ EPIC-108 stopped at TICKET-${ticket}"
        echo "EPIC-108: BLOCKED" > /tmp/epic_108_status.txt
        exit 1
    fi
done

echo "✅ EPIC-CCN-108 Phase 5 COMPLETE (all tickets validated)"
echo "Completed: $(date)"

# Made with Bob
