#!/bin/bash
# Wave 1 Phase 1 Launcher - Batch 2 (EPIC-006-015)
# Launches 10 epics in screen sessions on VM1

set -e

echo "=== Wave 1 Phase 1 Batch 2 Launcher ==="
echo "Target: EPIC-006-015 (10 epics)"
echo "VM: v12-test-golden-v2"
echo ""

# Create log directory
mkdir -p logs/phase1

# Fix message file numbers
echo "Fixing message file numbers..."
chmod +x fix_phase1_message_files.sh
./fix_phase1_message_files.sh

# Make scripts executable
echo "Making scripts executable..."
chmod +x _p1_06.sh _p1_07.sh _p1_08.sh _p1_09.sh _p1_10.sh
chmod +x _p1_11.sh _p1_12.sh _p1_13.sh _p1_14.sh _p1_15.sh

# Launch screen sessions
echo ""
echo "Launching screen sessions..."

screen -dmS p1-006 bash -l -c './_p1_06.sh 2>&1 | tee logs/phase1/EPIC-001-006.log'
echo "✓ Launched p1-006 (EPIC-001-006)"

screen -dmS p1-007 bash -l -c './_p1_07.sh 2>&1 | tee logs/phase1/EPIC-001-007.log'
echo "✓ Launched p1-007 (EPIC-001-007)"

screen -dmS p1-008 bash -l -c './_p1_08.sh 2>&1 | tee logs/phase1/EPIC-001-008.log'
echo "✓ Launched p1-008 (EPIC-001-008)"

screen -dmS p1-009 bash -l -c './_p1_09.sh 2>&1 | tee logs/phase1/EPIC-001-009.log'
echo "✓ Launched p1-009 (EPIC-001-009)"

screen -dmS p1-010 bash -l -c './_p1_10.sh 2>&1 | tee logs/phase1/EPIC-001-010.log'
echo "✓ Launched p1-010 (EPIC-001-010)"

screen -dmS p1-011 bash -l -c './_p1_11.sh 2>&1 | tee logs/phase1/EPIC-001-011.log'
echo "✓ Launched p1-011 (EPIC-001-011)"

screen -dmS p1-012 bash -l -c './_p1_12.sh 2>&1 | tee logs/phase1/EPIC-001-012.log'
echo "✓ Launched p1-012 (EPIC-001-012)"

screen -dmS p1-013 bash -l -c './_p1_13.sh 2>&1 | tee logs/phase1/EPIC-001-013.log'
echo "✓ Launched p1-013 (EPIC-001-013)"

screen -dmS p1-014 bash -l -c './_p1_14.sh 2>&1 | tee logs/phase1/EPIC-001-014.log'
echo "✓ Launched p1-014 (EPIC-001-014)"

screen -dmS p1-015 bash -l -c './_p1_15.sh 2>&1 | tee logs/phase1/EPIC-001-015.log'
echo "✓ Launched p1-015 (EPIC-001-015)"

echo ""
echo "=== All 10 sessions launched ==="
echo ""
echo "Monitor with:"
echo "  screen -ls                    # List all sessions"
echo "  screen -r p1-006              # Attach to specific session"
echo "  tail -f logs/phase1/EPIC-001-006.log  # View log"
echo ""
echo "Check completion:"
echo "  ls docs/brain/EPIC-001-*/00-scope.md | wc -l  # Should be 15 when all done"
echo ""

# Made with Bob
