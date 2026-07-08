#!/bin/bash
# Safe Shutdown Script for Frozen Phase 1.5 Processes
# Run this on the VM: bash kill_frozen_phase1_5.sh

echo "=== Checking for frozen Bob and Phase 1.5 processes ==="
ps aux | grep -E '(bob|_p1_5_epic)' | grep -v grep

echo ""
echo "=== Killing frozen processes ==="
pkill -9 -f bob
pkill -9 -f _p1_5_epic
pkill -9 -f phase

echo ""
echo "=== Verifying clean state ==="
REMAINING=$(ps aux | grep -E '(bob|phase|epic)' | grep -v grep | wc -l)
echo "Remaining processes: $REMAINING"

if [ "$REMAINING" -eq 0 ]; then
    echo "✅ All processes killed successfully"
else
    echo "⚠️ Some processes still running:"
    ps aux | grep -E '(bob|phase|epic)' | grep -v grep
fi

# Made with Bob
