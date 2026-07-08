#!/bin/bash
# Wave 7 Phase 0 Recovery Script
# Executes remaining 59 incomplete epics
# Uses absolute paths to work around Bob IDE shell PATH issue

set -e  # Exit on error

echo "================================================================================"
echo "WAVE 7 PHASE 0 RECOVERY - 59 INCOMPLETE EPICS"
echo "================================================================================"
echo ""
echo "Status: 102/161 complete (63.4%)"
echo "Target: 161/161 complete (100%)"
echo "Remaining: 59 epics"
echo ""

# Cost optimization: 4-minute polling interval
POLL_INTERVAL=240

# Track progress
COMPLETED=0
FAILED=0
TOTAL=59

# Function to execute Phase 0 for one epic
execute_phase0() {
    local epic_num=$1
    local epic_id="EPIC-CCN-$(printf '%03d' $epic_num)"
    local script="_p0_$(printf '%03d' $epic_num).sh"
    
    echo "--------------------------------------------------------------------------------"
    echo "[$((COMPLETED + FAILED + 1))/$TOTAL] Executing $epic_id"
    echo "--------------------------------------------------------------------------------"
    
    if [ ! -f "$script" ]; then
        echo "⚠️  WARNING: Script $script not found - skipping"
        ((FAILED++))
        return 1
    fi
    
    # Execute with absolute path to bash
    if /usr/bin/bash "$script"; then
        echo "✅ $epic_id Phase 0 complete"
        ((COMPLETED++))
        
        # Verify output file was created
        if [ -f "docs/brain/$epic_id/00-hotspots.md" ]; then
            echo "✅ Verified: 00-hotspots.md exists"
        else
            echo "⚠️  WARNING: 00-hotspots.md not found after execution"
        fi
    else
        echo "❌ $epic_id Phase 0 FAILED"
        ((FAILED++))
        
        # Log failure for recovery loop
        echo "$epic_id" >> phase0_failures.txt
    fi
    
    echo ""
    
    # Cost optimization: 4-minute delay between epics
    if [ $((COMPLETED + FAILED)) -lt $TOTAL ]; then
        echo "⏱️  Waiting $POLL_INTERVAL seconds (cost optimization)..."
        /usr/bin/sleep $POLL_INTERVAL
    fi
}

# Create failure log
> phase0_failures.txt

echo "Starting Phase 0 execution for 59 incomplete epics..."
echo ""

# Execute EPIC-CCN-081 through 106 (26 epics)
for i in {81..106}; do
    execute_phase0 $i
done

# Execute EPIC-CCN-126 through 161 (excluding 128, 129, 155 which are complete)
for i in 126 127 130 131 132 133 134 135 136 137 138 139 140 141 142 143 144 145 146 147 148 149 150 151 152 153 154 156 157 158 159 160 161; do
    execute_phase0 $i
done

echo "================================================================================"
echo "WAVE 7 PHASE 0 RECOVERY COMPLETE"
echo "================================================================================"
echo ""
echo "Results:"
echo "  ✅ Completed: $COMPLETED/$TOTAL"
echo "  ❌ Failed: $FAILED/$TOTAL"
echo "  📊 Success Rate: $(( COMPLETED * 100 / TOTAL ))%"
echo ""

if [ $FAILED -gt 0 ]; then
    echo "⚠️  $FAILED epics failed. See phase0_failures.txt for list."
    echo "   Apply Recovery Loop Protocol to fix failures."
    echo ""
fi

# Final verification
echo "Running final verification..."
FINAL_COMPLETE=$(/usr/bin/find docs/brain/EPIC-CCN-* -name '00-hotspots.md' 2>/dev/null | /usr/bin/wc -l)
echo "Final count: $FINAL_COMPLETE/161 epics complete"
echo ""

if [ $FINAL_COMPLETE -eq 161 ]; then
    echo "🎉 SUCCESS! All 161 epics Phase 0 complete!"
    echo "   Ready to proceed to Phase 1 (Scope Definition)"
else
    REMAINING=$((161 - FINAL_COMPLETE))
    echo "⚠️  $REMAINING epics still incomplete"
    echo "   Review failures and re-run recovery script"
fi

echo ""
echo "================================================================================"

# Made with Bob
