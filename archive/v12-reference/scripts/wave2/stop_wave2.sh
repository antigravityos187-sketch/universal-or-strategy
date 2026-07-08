#!/bin/bash
# Stop all Wave 2 v2 agents

echo "[STOP] Killing all v12-EPIC screen sessions..."

# List all screen sessions and kill those matching v12-EPIC
for session in $(screen -ls | grep 'v12-EPIC' | awk '{print $1}'); do
    echo "[STOP] Killing session: $session"
    screen -S "$session" -X quit
done

echo "[STOP] Done. Remaining screens:"
screen -ls || echo "No screens running"

# Made with Bob
