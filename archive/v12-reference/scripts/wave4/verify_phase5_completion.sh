#!/bin/bash
# Verify Phase 5 completion files (both naming patterns)
cd /home/malhitticrypto/universal-or-strategy

echo "Checking Phase 5 completion files for 7 recovery epics:"
echo ""

for epic in 003 015 031 033 042 055; do
    echo -n "EPIC-CCN-$epic: "
    if [ -f "docs/brain/EPIC-CCN-$epic/ticket-completion.md" ]; then
        size=$(ls -lh "docs/brain/EPIC-CCN-$epic/ticket-completion.md" | awk '{print $5}')
        echo "SUCCESS (ticket-completion.md, $size)"
    elif ls docs/brain/EPIC-CCN-$epic/ticket-*-completion.md 2>/dev/null | grep -q .; then
        count=$(ls docs/brain/EPIC-CCN-$epic/ticket-*-completion.md 2>/dev/null | wc -l)
        echo "SUCCESS (ticket-*-completion.md, $count files)"
    else
        echo "MISSING"
    fi
done

echo ""
echo "Summary:"
success=0
for epic in 003 015 031 033 042 055; do
    if [ -f "docs/brain/EPIC-CCN-$epic/ticket-completion.md" ] || ls docs/brain/EPIC-CCN-$epic/ticket-*-completion.md 2>/dev/null | grep -q .; then
        success=$((success + 1))
    fi
done
echo "Successful: $success/6"

# Made with Bob
