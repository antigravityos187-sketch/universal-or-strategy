#!/bin/bash
# Wave 7 Phase 1 Batch Launch Script
# Purpose: Launch Phase 1 (Scope Definition) for all epics with completed Phase 0
# Building-Blocks Method: Copied from phase1_template_wave7.sh
# Polling: 4-minute intervals (cost-optimized)

set -euo pipefail

WAVE_ID="wave7"
PHASE="1"
AGENT_ID="autonomous-refactor-$(date +%s)"

echo "=========================================="
echo "Wave 7 Phase 1 Batch Launch"
echo "Agent: $AGENT_ID"
echo "Polling: 4-minute intervals"
echo "=========================================="

# Step 1: Create necessary directories
echo ""
echo "Step 1: Creating directories..."
mkdir -p logs/wave7/phase1
mkdir -p scripts/wave7/phase1_scripts
echo "✅ Directories created"

# Step 2: Identify epics with Phase 0 complete
echo ""
echo "Step 2: Identifying epics with Phase 0 complete..."

# Find all EPIC-W7-* directories with 00-hotspots.md
EPICS_WITH_PHASE0=()
for epic_dir in docs/brain/EPIC-W7-*/; do
    if [ -f "${epic_dir}00-hotspots.md" ]; then
        epic_id=$(basename "$epic_dir")
        
        # Check if Phase 1 already started (00-scope.md exists)
        if [ ! -f "${epic_dir}00-scope.md" ]; then
            EPICS_WITH_PHASE0+=("$epic_id")
        fi
    fi
done

EPIC_COUNT=${#EPICS_WITH_PHASE0[@]}
echo "✅ Found $EPIC_COUNT epics needing Phase 1"

if [ $EPIC_COUNT -eq 0 ]; then
    echo "⚠️  No epics need Phase 1 execution"
    exit 0
fi

# Display epic list
echo ""
echo "EPICs to process:"
for epic in "${EPICS_WITH_PHASE0[@]}"; do
    echo "  - $epic"
done

# Step 3: Generate individual Phase 1 scripts for each epic
echo ""
echo "Step 3: Generating Phase 1 scripts..."

for epic_id in "${EPICS_WITH_PHASE0[@]}"; do
    script_file="scripts/wave7/phase1_scripts/${epic_id}_phase1.sh"
    
    # Copy template and replace placeholders
    cat building-blocks/wave7/phase1_template_wave7.sh | \
        sed "s/{EPIC_ID}/$epic_id/g" | \
        sed "s/{AGENT_ID}/$AGENT_ID/g" > "$script_file"
    
    chmod +x "$script_file"
    echo "  ✅ Generated: $script_file"
done

echo "✅ All Phase 1 scripts generated"

# Step 4: Launch Phase 1 for each epic in screen sessions
echo ""
echo "Step 4: Launching Phase 1 executions..."

LAUNCHED_COUNT=0
FAILED_COUNT=0

for epic_id in "${EPICS_WITH_PHASE0[@]}"; do
    script_file="scripts/wave7/phase1_scripts/${epic_id}_phase1.sh"
    session_name="wave7_phase1_${epic_id}"
    
    echo ""
    echo "Launching $epic_id..."
    
    # Check if session already exists
    if screen -list | grep -q "$session_name"; then
        echo "  ⚠️  Session already exists: $session_name"
        continue
    fi
    
    # Launch in screen session
    screen -dmS "$session_name" bash -c "
        cd ~/universal-or-strategy
        bash $script_file 2>&1 | tee logs/wave7/phase1/${epic_id}.log
        echo 'Exit code: \$?' >> logs/wave7/phase1/${epic_id}.log
    "
    
    if [ $? -eq 0 ]; then
        echo "  ✅ Launched: $session_name"
        LAUNCHED_COUNT=$((LAUNCHED_COUNT + 1))
    else
        echo "  ❌ Failed to launch: $session_name"
        FAILED_COUNT=$((FAILED_COUNT + 1))
    fi
    
    # Cost-optimized polling: 4-minute intervals between launches
    if [ $LAUNCHED_COUNT -lt $EPIC_COUNT ]; then
        echo "  ⏳ Waiting 4 minutes before next launch (cost optimization)..."
        sleep 240
    fi
done

# Step 5: Summary
echo ""
echo "=========================================="
echo "Phase 1 Batch Launch Complete"
echo "=========================================="
echo "Total epics: $EPIC_COUNT"
echo "Launched: $LAUNCHED_COUNT"
echo "Failed: $FAILED_COUNT"
echo ""
echo "Monitor progress:"
echo "  screen -ls | grep wave7_phase1"
echo "  tail -f logs/wave7/phase1/*.log"
echo ""
echo "Check individual epic:"
echo "  screen -r wave7_phase1_EPIC-W7-XXX"
echo "=========================================="

# Step 6: Monitor loop (optional - run in background)
echo ""
echo "Starting monitoring loop (4-minute intervals)..."
echo "Press Ctrl+C to stop monitoring"
echo ""

MONITORING=true
while $MONITORING; do
    sleep 240  # 4-minute polling interval
    
    echo ""
    echo "=== Status Check $(date) ==="
    
    RUNNING=0
    COMPLETED=0
    
    for epic_id in "${EPICS_WITH_PHASE0[@]}"; do
        session_name="wave7_phase1_${epic_id}"
        
        if screen -list | grep -q "$session_name"; then
            RUNNING=$((RUNNING + 1))
        else
            # Check if output file exists
            if [ -f "docs/brain/${epic_id}/00-scope.md" ]; then
                COMPLETED=$((COMPLETED + 1))
            fi
        fi
    done
    
    echo "Running: $RUNNING"
    echo "Completed: $COMPLETED"
    echo "Remaining: $((EPIC_COUNT - COMPLETED))"
    
    # Stop monitoring if all complete
    if [ $COMPLETED -eq $EPIC_COUNT ]; then
        echo ""
        echo "✅ All Phase 1 executions complete!"
        MONITORING=false
    fi
done

echo ""
echo "=========================================="
echo "✅ Wave 7 Phase 1 Batch Complete"
echo "=========================================="

# Made with Bob
