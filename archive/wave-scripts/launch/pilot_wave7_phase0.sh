#!/bin/bash
# Wave 7 Phase 0 Pilot Test - 3 epics
# Tests low/medium/high complexity before full launch
set -e

echo "================================================================================"
echo "WAVE 7 PHASE 0 - PILOT TEST (3 EPICS)"
echo "================================================================================"
echo ""
echo "Testing 3 epics before full launch:"
echo "  1. EPIC-W7-002 (Low complexity)"
echo "  2. EPIC-W7-050 (Medium complexity)"
echo "  3. EPIC-W7-100 (High complexity)"
echo ""

# Test epic 002
echo "--------------------------------------------------------------------------------"
echo "[1/3] Testing EPIC-W7-002..."
echo "--------------------------------------------------------------------------------"
/usr/bin/bash _p0_002.sh
if [ -f "docs/brain/EPIC-W7-002/00-hotspots.md" ]; then
    echo "✅ EPIC-W7-002 complete"
else
    echo "❌ EPIC-W7-002 failed"
    exit 1
fi
echo ""

# Test epic 050
echo "--------------------------------------------------------------------------------"
echo "[2/3] Testing EPIC-W7-050..."
echo "--------------------------------------------------------------------------------"
/usr/bin/bash _p0_050.sh
if [ -f "docs/brain/EPIC-W7-050/00-hotspots.md" ]; then
    echo "✅ EPIC-W7-050 complete"
else
    echo "❌ EPIC-W7-050 failed"
    exit 1
fi
echo ""

# Test epic 100
echo "--------------------------------------------------------------------------------"
echo "[3/3] Testing EPIC-W7-100..."
echo "--------------------------------------------------------------------------------"
/usr/bin/bash _p0_100.sh
if [ -f "docs/brain/EPIC-W7-100/00-hotspots.md" ]; then
    echo "✅ EPIC-W7-100 complete"
else
    echo "❌ EPIC-W7-100 failed"
    exit 1
fi
echo ""

echo "================================================================================"
echo "✅ PILOT TEST COMPLETE - ALL 3 EPICS PASSED"
echo "================================================================================"
echo ""
echo "Ready to launch full Wave 7 execution (151 remaining epics)"
echo "Run: ./relaunch_wave7_clean.sh"
echo ""

# Made with Bob
