#!/bin/bash
# Launch remaining 6 epics (skip EPIC-107 - needs manual fix)
set -e

echo "=== Launching Remaining 6 Epics ==="
echo "Started: $(date)"
echo ""
echo "Skipping EPIC-CCN-107 (TICKET-3 needs manual fix)"
echo "Processing: 108, 109, 111, 112, 113, 114"
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
for ticket in 1 2 3 4 5; do
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
        break
    fi
done

# Skip Phase 6 review - stop at Phase 5 completion
if [ ! -f /tmp/epic_108_status.txt ]; then
    echo "✅ EPIC-CCN-108 Phase 5 COMPLETE (all tickets validated)"
fi

echo ""

# EPIC-CCN-109 (4 tickets)
echo "=== EPIC-CCN-109 (4 tickets) ==="
for ticket in 1 2 3 4; do
    echo "Processing TICKET-${ticket}..."
    
    screen -dmS p5_109_t${ticket} bash -l _p5_109_t${ticket}.sh
    wait_for_completion p5_109_t${ticket}
    echo "  Execution complete"
    
    screen -dmS p5v_109_t${ticket} bash -l _p5v_109_t${ticket}.sh
    wait_for_completion p5v_109_t${ticket}
    echo "  Validation complete"
    
    if ! check_validation 109 ${ticket}; then
        echo "⚠️ EPIC-109 stopped at TICKET-${ticket}"
        echo "EPIC-109: BLOCKED" > /tmp/epic_109_status.txt
        break
    fi
done

if [ ! -f /tmp/epic_109_status.txt ]; then
    echo "✅ EPIC-CCN-109 Phase 5 COMPLETE (all tickets validated)"
fi

echo ""

# EPIC-CCN-111 (3 tickets)
echo "=== EPIC-CCN-111 (3 tickets) ==="
for ticket in 1 2 3; do
    echo "Processing TICKET-${ticket}..."
    
    screen -dmS p5_111_t${ticket} bash -l _p5_111_t${ticket}.sh
    wait_for_completion p5_111_t${ticket}
    echo "  Execution complete"
    
    screen -dmS p5v_111_t${ticket} bash -l _p5v_111_t${ticket}.sh
    wait_for_completion p5v_111_t${ticket}
    echo "  Validation complete"
    
    if ! check_validation 111 ${ticket}; then
        echo "⚠️ EPIC-111 stopped at TICKET-${ticket}"
        echo "EPIC-111: BLOCKED" > /tmp/epic_111_status.txt
        break
    fi
done

if [ ! -f /tmp/epic_111_status.txt ]; then
    echo "✅ EPIC-CCN-111 Phase 5 COMPLETE (all tickets validated)"
fi

echo ""

# EPIC-CCN-112 (6 tickets)
echo "=== EPIC-CCN-112 (6 tickets) ==="
for ticket in 1 2 3 4 5 6; do
    echo "Processing TICKET-${ticket}..."
    
    screen -dmS p5_112_t${ticket} bash -l _p5_112_t${ticket}.sh
    wait_for_completion p5_112_t${ticket}
    echo "  Execution complete"
    
    screen -dmS p5v_112_t${ticket} bash -l _p5v_112_t${ticket}.sh
    wait_for_completion p5v_112_t${ticket}
    echo "  Validation complete"
    
    if ! check_validation 112 ${ticket}; then
        echo "⚠️ EPIC-112 stopped at TICKET-${ticket}"
        echo "EPIC-112: BLOCKED" > /tmp/epic_112_status.txt
        break
    fi
done

if [ ! -f /tmp/epic_112_status.txt ]; then
    echo "✅ EPIC-CCN-112 Phase 5 COMPLETE (all tickets validated)"
fi

echo ""

# EPIC-CCN-113 (5 tickets)
echo "=== EPIC-CCN-113 (5 tickets) ==="
for ticket in 1 2 3 4 5; do
    echo "Processing TICKET-${ticket}..."
    
    screen -dmS p5_113_t${ticket} bash -l _p5_113_t${ticket}.sh
    wait_for_completion p5_113_t${ticket}
    echo "  Execution complete"
    
    screen -dmS p5v_113_t${ticket} bash -l _p5v_113_t${ticket}.sh
    wait_for_completion p5v_113_t${ticket}
    echo "  Validation complete"
    
    if ! check_validation 113 ${ticket}; then
        echo "⚠️ EPIC-113 stopped at TICKET-${ticket}"
        echo "EPIC-113: BLOCKED" > /tmp/epic_113_status.txt
        break
    fi
done

if [ ! -f /tmp/epic_113_status.txt ]; then
    echo "✅ EPIC-CCN-113 Phase 5 COMPLETE (all tickets validated)"
fi

echo ""

# EPIC-CCN-114 (1 ticket)
echo "=== EPIC-CCN-114 (1 ticket) ==="
echo "Processing TICKET-1..."

screen -dmS p5_114_t1 bash -l _p5_114_t1.sh
wait_for_completion p5_114_t1
echo "  Execution complete"

screen -dmS p5v_114_t1 bash -l _p5v_114_t1.sh
wait_for_completion p5v_114_t1
echo "  Validation complete"

if check_validation 114 1; then
    echo "✅ EPIC-CCN-114 Phase 5 COMPLETE (all tickets validated)"
else
    echo "⚠️ EPIC-114 stopped at TICKET-1"
    echo "EPIC-114: BLOCKED" > /tmp/epic_114_status.txt
fi

echo ""
echo "=== Phase 5 Execution Summary ==="
echo "Completed: $(date)"
echo ""
echo "Phase 5 Status (Ticket Execution + Validation):"
[ ! -f /tmp/epic_108_status.txt ] && echo "  ✅ EPIC-CCN-108: Phase 5 COMPLETE" || echo "  ⚠️ EPIC-CCN-108: BLOCKED"
[ ! -f /tmp/epic_109_status.txt ] && echo "  ✅ EPIC-CCN-109: Phase 5 COMPLETE" || echo "  ⚠️ EPIC-CCN-109: BLOCKED"
[ ! -f /tmp/epic_111_status.txt ] && echo "  ✅ EPIC-CCN-111: Phase 5 COMPLETE" || echo "  ⚠️ EPIC-CCN-111: BLOCKED"
[ ! -f /tmp/epic_112_status.txt ] && echo "  ✅ EPIC-CCN-112: Phase 5 COMPLETE" || echo "  ⚠️ EPIC-CCN-112: BLOCKED"
[ ! -f /tmp/epic_113_status.txt ] && echo "  ✅ EPIC-CCN-113: Phase 5 COMPLETE" || echo "  ⚠️ EPIC-CCN-113: BLOCKED"
[ ! -f /tmp/epic_114_status.txt ] && echo "  ✅ EPIC-CCN-114: Phase 5 COMPLETE" || echo "  ⚠️ EPIC-CCN-114: BLOCKED"
echo "  ⏸️ EPIC-CCN-107: PENDING (TICKET-3 needs manual fix)"
echo ""
echo "Phase 6 (Epic Reviews): NOT STARTED - waiting for user approval"
echo ""
echo "Check logs in: logs/phase5/ and logs/phase5v/"
echo "Check verification files in: docs/brain/EPIC-CCN-*/ticket-*-verification.md"

# Made with Bob
