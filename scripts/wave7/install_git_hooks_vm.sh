#!/bin/bash
# Install Git Hooks on VM for Wave 7 Protection
# This script creates all protective git hooks directly on the VM

set -e

echo "=== Wave 7 Git Hooks Installation ==="
echo ""

# Verify we're in the repo root
if [ ! -d ".git" ]; then
    echo "ERROR: Must run from repository root"
    exit 1
fi

# Create hooks directory
mkdir -p .git/hooks

echo "Installing git hooks..."
echo ""

# ============================================================================
# POST-COMMIT HOOK: Auto-push to GitHub
# ============================================================================
cat > .git/hooks/post-commit << 'HOOK_EOF'
#!/bin/bash
echo "=== Post-Commit Hook: Auto-Push ==="
BRANCH=$(git rev-parse --abbrev-ref HEAD)
if [ "$BRANCH" = "main" ]; then
    echo "Auto-pushing to origin/main..."
    git push origin main
    if [ $? -eq 0 ]; then
        echo "✓ Successfully pushed to GitHub"
    else
        echo "⚠ Push failed - manual intervention required"
    fi
else
    echo "Not on main branch - skipping auto-push"
fi
echo ""
HOOK_EOF

chmod +x .git/hooks/post-commit
echo "✓ post-commit hook installed"

# ============================================================================
# PRE-PUSH HOOK: Verify Epic Count
# ============================================================================
cat > .git/hooks/pre-push << 'HOOK_EOF'
#!/bin/bash
echo "=== Pre-Push Hook: Epic Count Verification ==="
EPIC_COUNT=$(find docs/brain -maxdepth 1 -type d -name "EPIC-W7-*" 2>/dev/null | wc -l)
echo "Current epic count: $EPIC_COUNT"
if [ "$EPIC_COUNT" -lt 10 ]; then
    echo ""
    echo "⚠ WARNING: Only $EPIC_COUNT epic directories found!"
    echo "Expected: 161 epics (EPIC-W7-001 through EPIC-W7-161)"
    echo ""
    echo "Press Ctrl+C to abort push, or Enter to continue"
    read -r
fi
echo "✓ Epic count verification passed"
echo ""
HOOK_EOF

chmod +x .git/hooks/pre-push
echo "✓ pre-push hook installed"

# ============================================================================
# POST-MERGE HOOK: Backup After Pull
# ============================================================================
cat > .git/hooks/post-merge << 'HOOK_EOF'
#!/bin/bash
echo "=== Post-Merge Hook: Creating Backup ==="
BACKUP_DIR="$HOME/wave7-backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_PATH="$BACKUP_DIR/backup_$TIMESTAMP"
mkdir -p "$BACKUP_DIR"
EPIC_COUNT=$(find docs/brain -maxdepth 1 -type d -name "EPIC-W7-*" 2>/dev/null | wc -l)
echo "Creating backup of $EPIC_COUNT epic directories..."
if [ "$EPIC_COUNT" -gt 0 ]; then
    mkdir -p "$BACKUP_PATH"
    find docs/brain -maxdepth 1 -type d -name "EPIC-W7-*" -exec cp -r {} "$BACKUP_PATH/" \;
    echo "✓ Backup created: $BACKUP_PATH"
    cd "$BACKUP_DIR"
    ls -t | tail -n +11 | xargs -r rm -rf
    echo "✓ Old backups cleaned (keeping last 10)"
else
    echo "⚠ No epic directories found - skipping backup"
fi
echo ""
HOOK_EOF

chmod +x .git/hooks/post-merge
echo "✓ post-merge hook installed"

# ============================================================================
# SUMMARY
# ============================================================================
echo ""
echo "=== Installation Complete ==="
echo ""
echo "Installed hooks:"
echo "1. post-commit: Auto-push to GitHub after every commit"
echo "2. pre-push: Verify epic count before push (warns if <10)"
echo "3. post-merge: Create backup after git pull"
echo ""
echo "Backups location: $HOME/wave7-backups/"
echo ""
echo "To test:"
echo "  git commit --allow-empty -m 'Test commit' (should auto-push)"
echo "  git pull (should create backup)"
echo ""
echo "To disable a hook:"
echo "  rm .git/hooks/<hook-name>"
echo ""

# Made with Bob
