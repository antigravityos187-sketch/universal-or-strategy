#!/bin/bash
# Create Phase 1 scripts using Building Blocks method
# Copies Phase 0 template and modifies only phase-specific content

set -e
cd "$(dirname "$0")"

echo "Creating Phase 1 scripts from Phase 0 template..."
echo ""

# First, download the Phase 0 template from VM
echo "Step 1: Downloading Phase 0 template from VM..."
gcloud compute scp v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/_p0_003.sh ./_p0_template.sh --zone=us-central1-a

echo "✅ Template downloaded"
echo ""

# Now create Phase 1 scripts for all 15 epics
echo "Step 2: Creating Phase 1 scripts (15 epics)..."

# EPIC-001 through EPIC-015
for i in $(seq -w 1 15); do
    epic_num="EPIC-$(printf '%03d' $i)"
    
    # Copy template
    cp _p0_template.sh _p1_${i}.sh
    
    # Replace phase-specific content using sed
    sed -i "s/phase0/phase1/g" _p1_${i}.sh
    sed -i "s/Phase 0/Phase 1/g" _p1_${i}.sh
    sed -i "s/EPIC-003/${epic_num}/g" _p1_${i}.sh
    sed -i "s/Hotspot Analysis/Scope Definition/g" _p1_${i}.sh
    sed -i "s/00-hotspots.md/00-scope.md/g" _p1_${i}.sh
    sed -i "s/v12-phase0-hotspot/plan/g" _p1_${i}.sh
    
    # Update task description in message
    # This is the key change - Phase 1 reads Phase 0 output and creates scope
    sed -i "s/Execute Phase 0 (Hotspot Analysis)/Execute Phase 1 (Scope Definition)/g" _p1_${i}.sh
    
    # Make executable
    chmod +x _p1_${i}.sh
    
    echo "  ✅ Created _p1_${i}.sh for ${epic_num}"
done

echo ""
echo "✅ Created 15 Phase 1 scripts"
echo ""
echo "Files created:"
ls -lh _p1_*.sh 2>/dev/null | head -5
echo "  ... (10 more files)"
echo ""
echo "Next steps:"
echo "1. Review one script to verify correctness"
echo "2. Distribute scripts across 3 VMs (5 epics each)"
echo "3. Upload and execute"

# Made with Bob