#!/bin/bash
# Check ticket counts for all epics

for epic in 107 108 109 111 112 113 114 115; do
    count=$(grep -c "^## TICKET-" /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-$epic/04-tickets.md 2>/dev/null || echo 0)
    echo "EPIC-CCN-$epic: $count tickets"
done

echo ""
echo "Total tickets: $(grep -c "^## TICKET-" /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/04-tickets.md 2>/dev/null)"

# Made with Bob
