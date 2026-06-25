#!/bin/bash

echo "=== Wave 7 Phase 1 Full Launch ==="
echo "Starting: $(date)"
echo ""

# Create logs directory
mkdir -p logs/wave7/phase1

# Launch all epics except pilots (003, 051, 101)
launched=0
for i in {001..161}; do
    # Skip pilot epics (already complete)
    if [ "$i" = "003" ] || [ "$i" = "051" ] || [ "$i" = "101" ]; then
        echo "Skipping EPIC-W7-$i (pilot - already complete)"
        continue
    fi
    
    # Launch epic in background
    echo "Launching EPIC-W7-$i..."
    ./_p1_${i}.sh > logs/wave7/phase1/EPIC-W7-${i}.log 2>&1 &
    
    launched=$((launched + 1))
    
    # Batch control: pause every 50 launches
    if [ $((launched % 50)) -eq 0 ]; then
        echo "Launched $launched epics, pausing 10 seconds..."
        sleep 10
    fi
done

echo ""
echo "✅ Launched $launched epics in parallel"
echo "Completed: $(date)"
echo ""
echo "Monitor progress with:"
echo "  watch -n 60 'find docs/brain/EPIC-W7-*/00-scope.md 2>/dev/null | wc -l'"
