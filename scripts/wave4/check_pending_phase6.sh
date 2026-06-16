#!/bin/bash
# Check Phase 6 status for 10 pending epics
cd /home/malhitticrypto/universal-or-strategy

echo "=== Phase 6 Status for 10 Pending Epics ==="
echo ""

echo "Step 1 PATH-fix epics (012, 027, 045):"
for epic in 012 027 045; do
    echo -n "  EPIC-CCN-$epic: "
    if [ -f "docs/brain/EPIC-CCN-$epic/06-completion-report.md" ]; then
        size=$(ls -lh "docs/brain/EPIC-CCN-$epic/06-completion-report.md" | awk '{print $5}')
        echo "COMPLETE ($size)"
    else
        echo "PENDING"
    fi
done

echo ""
echo "Newly recovered Phase 5 epics (003, 015, 030, 031, 033, 042, 055):"
for epic in 003 015 030 031 033 042 055; do
    echo -n "  EPIC-CCN-$epic: "
    if [ -f "docs/brain/EPIC-CCN-$epic/06-completion-report.md" ]; then
        size=$(ls -lh "docs/brain/EPIC-CCN-$epic/06-completion-report.md" | awk '{print $5}')
        echo "COMPLETE ($size)"
    else
        echo "PENDING"
    fi
done

echo ""
echo "Summary:"
complete=0
for epic in 012 027 045 003 015 030 031 033 042 055; do
    if [ -f "docs/brain/EPIC-CCN-$epic/06-completion-report.md" ]; then
        complete=$((complete + 1))
    fi
done
echo "Complete: $complete/10"
echo "Total Phase 6: 68 baseline + $complete pending = $((68 + complete))/79"

# Made with Bob
