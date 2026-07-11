#!/bin/bash
# Wave 7 Phase 0 - Continuous Execution (No Exit on Error)
# Processes all incomplete epics one by one

echo "================================================================================"
echo "WAVE 7 PHASE 0 - CONTINUOUS EXECUTION"
echo "================================================================================"

# Get list of incomplete epics
INCOMPLETE=()
for i in $(seq -f '%03g' 1 161); do
    if [ ! -f "docs/brain/EPIC-CCN-$i/00-hotspots.md" ]; then
        INCOMPLETE+=($i)
    fi
done

TOTAL=${#INCOMPLETE[@]}
echo "Found $TOTAL incomplete epics"
echo ""

COMPLETED=0
FAILED=0

for epic_num in "${INCOMPLETE[@]}"; do
    CURRENT=$((COMPLETED + FAILED + 1))
    echo "--------------------------------------------------------------------------------"
    echo "[$CURRENT/$TOTAL] Processing EPIC-CCN-$epic_num"
    echo "--------------------------------------------------------------------------------"
    
    script="_p0_$(printf '%03d' $epic_num).sh"
    
    if [ ! -f "$script" ]; then
        echo "⚠️  Script $script not found - skipping"
        ((FAILED++))
        continue
    fi
    
    # Execute script (don't exit on error)
    if /usr/bin/bash "$script" 2>&1; then
        if [ -f "docs/brain/EPIC-CCN-$(printf '%03d' $epic_num)/00-hotspots.md" ]; then
            echo "✅ EPIC-CCN-$epic_num complete"
            ((COMPLETED++))
        else
            echo "⚠️  EPIC-CCN-$epic_num script ran but no output file"
            ((FAILED++))
        fi
    else
        echo "❌ EPIC-CCN-$epic_num failed"
        ((FAILED++))
    fi
    
    echo ""
done

echo "================================================================================"
echo "EXECUTION COMPLETE"
echo "================================================================================"
echo "Completed: $COMPLETED/$TOTAL"
echo "Failed: $FAILED/$TOTAL"
echo ""

FINAL_COUNT=$(/usr/bin/find docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | /usr/bin/wc -l)
echo "Final total: $FINAL_COUNT/161 epics complete"

if [ $FINAL_COUNT -eq 161 ]; then
    echo "🎉 SUCCESS! All 161 epics complete!"
else
    echo "⚠️  $((161 - FINAL_COUNT)) epics still incomplete"
fi

# Made with Bob
