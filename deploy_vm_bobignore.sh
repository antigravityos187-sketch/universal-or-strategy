#!/bin/bash
# Deploy VM-specific .bobignore to allow Wave 7 script access

set -e

echo "=== Deploying VM-Specific .bobignore ==="

cd /home/malhitticrypto/universal-or-strategy

# Backup existing .bobignore
if [ -f .bobignore ]; then
    cp .bobignore .bobignore.local.backup
    echo "✅ Backed up local .bobignore to .bobignore.local.backup"
fi

# Deploy VM-specific version
if [ -f .bobignore.vm ]; then
    cp .bobignore.vm .bobignore
    echo "✅ Deployed .bobignore.vm as .bobignore"
else
    echo "❌ ERROR: .bobignore.vm not found!"
    exit 1
fi

# Verify critical files are now accessible
echo ""
echo "=== Verifying File Access ==="

# Check phase scripts
if ls _p0_001.sh >/dev/null 2>&1; then
    echo "✅ Phase 0 scripts accessible"
else
    echo "⚠️  Phase 0 scripts not found (may not be generated yet)"
fi

# Check epic directories
EPIC_COUNT=$(find docs/brain/EPIC-CCN-* -maxdepth 0 -type d 2>/dev/null | wc -l)
echo "✅ Found $EPIC_COUNT EPIC-CCN-* directories"

# Check building-blocks
if [ -d building-blocks/wave7 ]; then
    echo "✅ building-blocks/wave7/ accessible"
else
    echo "❌ building-blocks/wave7/ not accessible"
fi

echo ""
echo "=== Deployment Complete ==="
echo ""
echo "The VM can now access:"
echo "  - Phase scripts (_p0_*.sh, _p1_*.sh, etc.)"
echo "  - Epic directories (EPIC-CCN-*/)"
echo "  - Building blocks templates (building-blocks/wave7/)"
echo ""
echo "To restore local .bobignore later:"
echo "  cp .bobignore.local.backup .bobignore"

# Made with Bob
