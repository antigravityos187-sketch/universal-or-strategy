#!/bin/bash
# Wave 6 Brain Directory Cleanup
# Removes Phase 0-6 files from all epic brain directories to prepare for fresh Wave 6 execution

set -e

echo "=== Wave 6 Brain Directory Cleanup ==="
echo "Removing Phase 0-6 files from all epic brain directories..."
echo ""

CLEANED=0
SKIPPED=0

for epic_dir in docs/brain/EPIC-CCN-*/; do
    if [ -d "$epic_dir" ]; then
        epic_id=$(basename "$epic_dir")
        
        # Check if any Phase 0-6 files exist
        phase_files=$(ls "$epic_dir"00-*.md "$epic_dir"01-*.md "$epic_dir"02-*.md "$epic_dir"03-*.md "$epic_dir"04-*.md "$epic_dir"05-*.md "$epic_dir"ticket-*.md 2>/dev/null | wc -l)
        
        if [ "$phase_files" -gt 0 ]; then
            echo "Cleaning $epic_id ($phase_files files)..."
            
            # Remove Phase 0-6 files
            rm -f "$epic_dir"00-*.md
            rm -f "$epic_dir"01-*.md
            rm -f "$epic_dir"02-*.md
            rm -f "$epic_dir"03-*.md
            rm -f "$epic_dir"04-*.md
            rm -f "$epic_dir"05-*.md
            rm -f "$epic_dir"ticket-*.md
            
            CLEANED=$((CLEANED + 1))
        else
            SKIPPED=$((SKIPPED + 1))
        fi
    fi
done

echo ""
echo "=== Cleanup Complete ==="
echo "Cleaned: $CLEANED epics"
echo "Skipped: $SKIPPED epics (already clean)"
echo ""
echo "Brain directories ready for Wave 6 execution."

# Made with Bob
