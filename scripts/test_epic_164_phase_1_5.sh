#!/bin/bash
set -e

echo "=========================================="
echo "EPIC-CCN-164 Full Workflow Test"
echo "Testing: Phase 1.5 (Scope Boundary)"
echo "=========================================="
echo "Start Time: $(date)"
echo ""

# Change to repository directory
cd ~/universal-or-strategy

echo "Step 1: Verify Bob Shell installation..."
bob --version
echo ""

echo "Step 2: Verify repository status..."
git status --short
git log --oneline -1
echo ""

echo "Step 3: Check EPIC-CCN-164 directory..."
if [ ! -d "docs/brain/EPIC-CCN-164" ]; then
    echo "❌ ERROR: EPIC-CCN-164 directory not found"
    echo "This epic should already exist with Phase 1 complete"
    exit 1
fi

echo "✅ EPIC-CCN-164 directory exists"
ls -la docs/brain/EPIC-CCN-164/
echo ""

echo "Step 4: Verify Phase 1 artifacts..."
if [ ! -f "docs/brain/EPIC-CCN-164/00-scope.md" ]; then
    echo "❌ ERROR: Phase 1 artifact (00-scope.md) not found"
    exit 1
fi
echo "✅ Phase 1 complete (00-scope.md exists)"
echo ""

echo "Step 5: Run Phase 1.5 (Scope Boundary Validation)..."
echo "Command: bob --accept-license --auth-method api-key -p 'Run epic-scope-boundary for EPIC-CCN-164 phase 1.5' --max-coins 30"
echo ""

bob --accept-license --auth-method api-key -p "Run epic-scope-boundary for EPIC-CCN-164 phase 1.5. This phase validates that the planned extraction stays within single-method boundary and prevents scope creep per V12.23 Protocol. Read docs/brain/EPIC-CCN-164/00-scope.md and validate the scope boundaries." --max-coins 30

echo ""
echo "Step 6: Verify Phase 1.5 output..."
if [ -f "docs/brain/EPIC-CCN-164/01-scope-boundary.md" ]; then
    echo "✅ Phase 1.5 complete (01-scope-boundary.md created)"
    echo ""
    echo "Preview of 01-scope-boundary.md:"
    head -30 docs/brain/EPIC-CCN-164/01-scope-boundary.md
else
    echo "⚠️  Phase 1.5 artifact not found (may have been created with different name)"
    echo "Checking for any new files..."
    ls -lt docs/brain/EPIC-CCN-164/ | head -10
fi

echo ""
echo "=========================================="
echo "Test Complete"
echo "End Time: $(date)"
echo "=========================================="

# Made with Bob
