#!/bin/bash
# Launch Phase 1.5 (Scope Boundary Validation) for all 79 Wave 6 epics
# Building-blocks method: Sequential execution with 4-minute polling

set -euo pipefail

echo "=========================================="
echo "Wave 6 Phase 1.5: Scope Boundary Validation"
echo "Target: 79 epics (EPIC-CCN-001 through EPIC-CCN-080, excluding 027)"
echo "=========================================="
echo ""

# Create log directory
mkdir -p logs/wave6/phase1_5

# Counter
TOTAL=79
COMPLETED=0
FAILED=0

# Execute each epic sequentially
for i in {001..080}; do
    # Skip EPIC-027 (excluded)
    if [ "$i" == "027" ]; then
        echo "⏭️  EPIC-CCN-$i - SKIPPED (excluded)"
        continue
    fi
    
    EPIC_ID="EPIC-CCN-$i"
    SCRIPT="scripts/wave6/_p1_5_epic_ccn_$i.sh"
    
    echo ""
    echo "=========================================="
    echo "Executing: $EPIC_ID ($((COMPLETED + 1))/$TOTAL)"
    echo "=========================================="
    
    if [ ! -f "$SCRIPT" ]; then
        echo "❌ Script not found: $SCRIPT"
        ((FAILED++))
        continue
    fi
    
    # Execute script
    if bash "$SCRIPT"; then
        echo "✅ $EPIC_ID - SUCCESS"
        ((COMPLETED++))
    else
        echo "❌ $EPIC_ID - FAILED"
        ((FAILED++))
    fi
    
    # Progress report
    echo ""
    echo "Progress: $COMPLETED completed, $FAILED failed, $((TOTAL - COMPLETED - FAILED)) remaining"
    
    # 4-minute polling interval (cost optimization)
    if [ $((COMPLETED + FAILED)) -lt $TOTAL ]; then
        echo "Waiting 4 minutes before next epic..."
        sleep 240
    fi
done

echo ""
echo "=========================================="
echo "Wave 6 Phase 1.5 Complete"
echo "=========================================="
echo "Completed: $COMPLETED/$TOTAL"
echo "Failed: $FAILED"
echo "Success Rate: $(( COMPLETED * 100 / TOTAL ))%"
echo "=========================================="

# Made with Bob
