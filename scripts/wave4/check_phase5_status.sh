#!/bin/bash
# Check Phase 5 recovery status
cd /home/malhitticrypto/universal-or-strategy

echo "Completed epics:"
for epic in 003 015 030 031 033 042 055; do
    if ls docs/brain/EPIC-CCN-$epic/ticket-*-completion.md 2>/dev/null | grep -q .; then
        echo "  EPIC-CCN-$epic: COMPLETE"
    else
        echo "  EPIC-CCN-$epic: PENDING"
    fi
done

echo ""
echo "Active screens:"
screen -ls | grep 'p5-' || echo "  None"

echo ""
echo "Recent log errors:"
for epic in 003 015 030 031 033 042 055; do
    if [ -f "logs/phase5/EPIC-CCN-$epic.log" ]; then
        if grep -i "error\|failed" logs/phase5/EPIC-CCN-$epic.log | tail -1 | grep -q .; then
            echo "  EPIC-CCN-$epic: $(grep -i 'error\|failed' logs/phase5/EPIC-CCN-$epic.log | tail -1)"
        fi
    fi
done

# Made with Bob
