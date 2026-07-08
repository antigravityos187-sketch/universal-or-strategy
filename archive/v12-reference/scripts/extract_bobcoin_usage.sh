#!/bin/bash
cd /home/malhitticrypto/universal-or-strategy

echo "=== Wave 4 Phase 0 Bobcoin Usage ==="
echo ""

# Extract all cost values and sum them
TOTAL=$(grep -h 'Cost:' logs/phase0/EPIC-CCN-*.log 2>/dev/null | grep -oE '[0-9]+\.[0-9]+' | awk '{sum += $1} END {print sum}')

echo "Total bobcoins used: $TOTAL"
echo ""

# Count how many epics reported costs
COUNT=$(grep -l 'Cost:' logs/phase0/EPIC-CCN-*.log 2>/dev/null | wc -l)
echo "Epics with cost data: $COUNT/80"
echo ""

# Calculate average
if [ -n "$TOTAL" ] && [ "$COUNT" -gt 0 ]; then
    AVG=$(echo "scale=2; $TOTAL / $COUNT" | bc)
    echo "Average per epic: $AVG bobcoins"
fi

# Made with Bob
