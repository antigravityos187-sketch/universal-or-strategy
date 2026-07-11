#!/bin/bash
# Monitor Phase 6 recovery progress
cd /home/malhitticrypto/universal-or-strategy

echo "=== Phase 6 Recovery Status ==="
date
echo ""

echo "Active screen sessions:"
screen -ls | grep 'p6-' | wc -l

echo ""
echo "Phase 6 verification reports:"
total=$(ls docs/brain/EPIC-CCN-*/06-verification-report.md 2>/dev/null | wc -l)
echo "Total: $total/79 (target: 72 baseline + 7 recovered)"

echo ""
echo "Step 1 PATH-fix epics (012, 027, 045):"
for epic in 012 027 045; do
    echo -n "  EPIC-CCN-$epic: "
    if [ -f "docs/brain/EPIC-CCN-$epic/06-verification-report.md" ]; then
        echo "COMPLETE"
    else
        echo "PENDING"
    fi
done

echo ""
echo "Newly recovered Phase 5 epics (003, 015, 030, 031, 033, 042, 055):"
for epic in 003 015 030 031 033 042 055; do
    echo -n "  EPIC-CCN-$epic: "
    if [ -f "docs/brain/EPIC-CCN-$epic/06-verification-report.md" ]; then
        echo "COMPLETE"
    else
        echo "PENDING"
    fi
done

# Made with Bob
