#!/bin/bash
# Setup Git Hooks for Wave 7 Protection
# Run this script to install all protective git hooks

HOOKS_DIR=".git/hooks"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=== Installing Wave 7 Protective Git Hooks ==="
echo ""

# Create hooks directory if it doesn't exist
mkdir -p "$HOOKS_DIR"

# Install post-commit hook (auto-push)
echo "Installing post-commit hook (auto-push)..."
cp "$SCRIPT_DIR/post-commit" "$HOOKS_DIR/post-commit"
chmod +x "$HOOKS_DIR/post-commit"
echo "✓ post-commit installed"

# Install pre-push hook (verification)
echo "Installing pre-push hook (verification)..."
cp "$SCRIPT_DIR/pre-push" "$HOOKS_DIR/pre-push"
chmod +x "$HOOKS_DIR/pre-push"
echo "✓ pre-push installed"

# Install post-merge hook (backup after pull)
echo "Installing post-merge hook (backup after pull)..."
cp "$SCRIPT_DIR/post-merge" "$HOOKS_DIR/post-merge"
chmod +x "$HOOKS_DIR/post-merge"
echo "✓ post-merge installed"

echo ""
echo "=== Git Hooks Installed Successfully ==="
echo ""
echo "Active hooks:"
echo "1. post-commit: Auto-push to GitHub after every commit"
echo "2. pre-push: Verify epic count before push"
echo "3. post-merge: Create backup after git pull"
echo ""
echo "To disable a hook, remove it from .git/hooks/"
echo "To re-enable, run this script again"

# Made with Bob
