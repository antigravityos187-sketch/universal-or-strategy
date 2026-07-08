#!/bin/bash
# Wave 4 Phase 4 Recovery Launcher
# Generated: 2026-06-15
# Epics: EPIC-CCN-044, 065, 074

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/../.."

echo "=== Wave 4 Phase 4 Recovery ==="
echo "Recovering 3 failed epics..."
echo ""

# Constant delay (12 seconds)
DELAY=12

# Launch recovery scripts
for EPIC_NUM in 044 065 074; do
    SCRIPT="./scripts/wave4/_p4_${EPIC_NUM}_recovery.sh"
    
    if [ ! -f "$SCRIPT" ]; then
        echo "ERROR: Recovery script not found: $SCRIPT"
        exit 1
    fi
    
    echo "Launching EPIC-CCN-${EPIC_NUM} recovery..."
    screen -dmS "p4-recovery-${EPIC_NUM}" bash -l -c "$SCRIPT" | tee "logs/phase4/EPIC-CCN-${EPIC_NUM}_recovery.log"
    
    echo "  Screen session: p4-recovery-${EPIC_NUM}"
    echo "  Waiting ${DELAY}s before next launch..."
    sleep $DELAY
done

echo ""
echo "=== Recovery Launch Complete ==="
echo "3 recovery sessions started"
echo ""
echo "Monitor with:"
echo "  screen -ls | grep p4-recovery"
echo "  ls docs/brain/EPIC-CCN-{044,065,074}/04-tickets.md"
echo ""
