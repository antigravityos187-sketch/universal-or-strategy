#!/bin/bash
# Fix Phase 6 prerequisite check to accept any Phase 5 completion file
# Issue: Scripts check for exact filename "05-completion.md" but Phase 5 created various patterns

set -e
cd /home/malhitticrypto/universal-or-strategy

echo "[$(date)] Fixing Phase 6 prerequisite checks..."

# For each Phase 6 script, update the prerequisite check to be flexible
for script in scripts/wave4/_p6_*.sh; do
    epic_num=$(basename "$script" | sed 's/_p6_\([0-9]*\)\.sh/\1/')
    epic_id="EPIC-CCN-${epic_num}"
    
    # Create backup
    cp "$script" "${script}.bak"
    
    # Replace strict check with flexible check
    sed -i 's|if \[ ! -f "docs/brain/'"$epic_id"'/05-completion.md" \]; then|# Check for any Phase 5 completion file (flexible pattern)\nif ! ls docs/brain/'"$epic_id"'/05-*.md docs/brain/'"$epic_id"'/ticket-*-completion.md 1>/dev/null 2>\&1; then|' "$script"
    
    echo "Fixed: $script"
done

echo "[$(date)] All Phase 6 scripts updated with flexible prerequisite check"
echo "Backup files created with .bak extension"

# Made with Bob
