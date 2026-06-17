#!/bin/bash
# Wave 6 Clean Slate - Remove Wave 4 Phase 0-4 files
# V12.52 Protocol

set -e

cd ~/universal-or-strategy

echo "=== Wave 6 Clean Slate Cleanup ==="
echo "Removing Wave 4 Phase 0-4 files from all 79 epics..."

# Remove Phase 0-4 files
find docs/brain/EPIC-CCN-* -type f \( -name '0*.md' -o -name '01-*.md' -o -name '02-*.md' -o -name '03-*.md' -o -name '04-*.md' \) -exec rm -f {} \;

echo ""
echo "Cleanup complete. Verifying remaining files..."
echo ""

# Count remaining files per epic (sample first 10)
count=0
for epic in docs/brain/EPIC-CCN-{001..026} docs/brain/EPIC-CCN-{028..080}; do
    if [ -d "$epic" ]; then
        file_count=$(ls -1 "$epic" 2>/dev/null | wc -l)
        echo "$epic: $file_count files"
        count=$((count + 1))
        if [ $count -ge 10 ]; then
            echo "... (showing first 10 epics)"
            break
        fi
    fi
done

echo ""
echo "Expected: Each epic should have only manifest.json (1 file)"
echo "Cleanup complete!"

# Made with Bob
