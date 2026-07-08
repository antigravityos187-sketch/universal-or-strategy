#!/bin/bash
set -e

echo "=========================================="
echo "Simple Epic Workflow Test"
echo "Testing: Phase 0 (Intake) for new epic"
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

echo "Step 3: Run Phase 0 (epic-intake) for test epic..."
echo "Creating EPIC-TEST-VM-001 for validation"
echo ""

bob --accept-license --auth-method api-key -p "Run epic-intake for EPIC-TEST-VM-001. Target: Reduce complexity in V12_002.UI.IPC.cs IsCommandForThisInstrument method (CYC 36 to 8) by extracting global command detection and symbol matching logic into separate methods. This is a test epic to validate the VM golden image and Bob Shell workflow." --max-coins 30

echo ""
echo "Step 4: Verify Phase 0 output..."
if [ -d "docs/brain/EPIC-TEST-VM-001" ]; then
    echo "✅ Epic directory created"
    ls -la docs/brain/EPIC-TEST-VM-001/
    echo ""
    
    if [ -f "docs/brain/EPIC-TEST-VM-001/00-hotspots.md" ]; then
        echo "✅ Phase 0 complete (00-hotspots.md created)"
        echo ""
        echo "Preview of 00-hotspots.md:"
        head -30 docs/brain/EPIC-TEST-VM-001/00-hotspots.md
    else
        echo "⚠️  Hotspots file not found, checking for alternatives..."
        ls -lt docs/brain/EPIC-TEST-VM-001/ | head -10
    fi
else
    echo "⚠️  Epic directory not created, checking docs/brain..."
    ls -lt docs/brain/ | head -10
fi

echo ""
echo "=========================================="
echo "Test Complete"
echo "End Time: $(date)"
echo "=========================================="
echo ""
echo "Summary:"
echo "- Bob Shell: Working ✅"
echo "- API Key Auth: Working ✅"
echo "- Repository Access: Working ✅"
echo "- Epic Workflow: $([ -d 'docs/brain/EPIC-TEST-VM-001' ] && echo '✅ Working' || echo '⚠️  Check output')"

# Made with Bob
