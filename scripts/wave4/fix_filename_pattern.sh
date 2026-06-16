#!/bin/bash
# Fix filename pattern mismatch in Phase 6 prerequisite checks
# Issue: Scripts look for ticket-*-completion.md but files are named ticket-completion.md

cd /home/malhitticrypto/universal-or-strategy

echo "=== Fixing Filename Pattern in Phase 6 Scripts ==="

# Fix _p6_003.sh
echo "Fixing _p6_003.sh..."
sed -i 's|if ! ls docs/brain/EPIC-CCN-003/ticket-\*-completion.md 1> /dev/null 2>\&1; then|if ! find docs/brain/EPIC-CCN-003 -maxdepth 1 \\( -name "05-*.md" -o -name "ticket-*-completion.md" -o -name "ticket-completion.md" \\) -print -quit | grep -q .; then|' scripts/wave4/_p6_003.sh

# Fix _p6_015.sh
echo "Fixing _p6_015.sh..."
sed -i 's|if ! ls docs/brain/EPIC-CCN-015/ticket-\*-completion.md 1> /dev/null 2>\&1; then|if ! find docs/brain/EPIC-CCN-015 -maxdepth 1 \\( -name "05-*.md" -o -name "ticket-*-completion.md" -o -name "ticket-completion.md" \\) -print -quit | grep -q .; then|' scripts/wave4/_p6_015.sh

# Fix _p6_030.sh
echo "Fixing _p6_030.sh..."
sed -i 's|if ! ls docs/brain/EPIC-CCN-030/ticket-\*-completion.md 1> /dev/null 2>\&1; then|if ! find docs/brain/EPIC-CCN-030 -maxdepth 1 \\( -name "05-*.md" -o -name "ticket-*-completion.md" -o -name "ticket-completion.md" \\) -print -quit | grep -q .; then|' scripts/wave4/_p6_030.sh

echo ""
echo "=== Verification ==="
echo "Checking EPIC-CCN-003 prerequisite:"
grep -A1 "Prerequisite check" scripts/wave4/_p6_003.sh | tail -1

echo ""
echo "Checking EPIC-CCN-015 prerequisite:"
grep -A1 "Prerequisite check" scripts/wave4/_p6_015.sh | tail -1

echo ""
echo "Checking EPIC-CCN-030 prerequisite:"
grep -A1 "Prerequisite check" scripts/wave4/_p6_030.sh | tail -1

echo ""
echo "=== Fix Complete ==="
echo "Scripts now accept both filename patterns:"
echo "  - ticket-*-completion.md (manifest-based)"
echo "  - ticket-completion.md (consolidated)"
echo "  - 05-*.md (alternative Phase 5 output)"

# Made with Bob
