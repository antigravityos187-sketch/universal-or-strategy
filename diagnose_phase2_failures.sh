#!/bin/bash
# Diagnose why Phase 2 epics launched but didn't complete

echo "=== Phase 2 Failure Diagnosis ==="
echo ""

# Count logs vs outputs
LOG_COUNT=$(ls logs/wave7/phase2/*.log 2>/dev/null | wc -l)
OUTPUT_COUNT=$(find docs/brain/EPIC-W7-*/02-architecture-plan.md 2>/dev/null | wc -l)

echo "Logs created: $LOG_COUNT"
echo "Outputs created: $OUTPUT_COUNT"
echo "Failed: $((LOG_COUNT - OUTPUT_COUNT))"
echo ""

# Sample a few failed logs (copy to temp to bypass .bobignore)
echo "=== Sampling Failed Epic Logs ==="
for epic_num in 060 065 070 075; do
    EPIC_ID="EPIC-W7-${epic_num}"
    LOG="logs/wave7/phase2/${EPIC_ID}.log"
    
    if [ -f "$LOG" ]; then
        echo ""
        echo "--- $EPIC_ID ---"
        # Copy to temp location to read
        cp "$LOG" "/tmp/${EPIC_ID}_phase2.log"
        head -20 "/tmp/${EPIC_ID}_phase2.log"
        echo "..."
        tail -10 "/tmp/${EPIC_ID}_phase2.log"
    fi
done

echo ""
echo "=== Checking for Common Error Patterns ==="
# Copy all logs to temp and grep for errors
mkdir -p /tmp/phase2_logs
cp logs/wave7/phase2/*.log /tmp/phase2_logs/ 2>/dev/null

echo "API key errors:"
grep -l "API key" /tmp/phase2_logs/*.log 2>/dev/null | wc -l

echo "Authentication errors:"
grep -l "auth" /tmp/phase2_logs/*.log 2>/dev/null | wc -l

echo "Timeout errors:"
grep -l "timeout\|timed out" /tmp/phase2_logs/*.log 2>/dev/null | wc -l

echo "Bob CLI errors:"
grep -l "bob:" /tmp/phase2_logs/*.log 2>/dev/null | wc -l

echo ""
echo "=== Sample Error Messages ==="
grep -h "error\|Error\|ERROR\|failed\|Failed\|FAILED" /tmp/phase2_logs/*.log 2>/dev/null | head -5

# Made with Bob
