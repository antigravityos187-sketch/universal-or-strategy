#!/bin/bash
# Master orchestration script for Phase 1 execution across 3 VMs
# Distributes 15 epics: VM1 (001-005), VM2 (006-010), VM3 (011-015)

set -e

echo "=========================================="
echo "Wave 1 Phase 1: 3-VM Parallel Execution"
echo "=========================================="
echo ""
echo "Distribution:"
echo "  VM1 (v12-test-golden-v2): EPIC-001 to EPIC-005"
echo "  VM2 (v12-test-golden-v3): EPIC-006 to EPIC-010"
echo "  VM3 (v12-test-golden-v4): EPIC-011 to EPIC-015"
echo ""

# Step 1: Generate Phase 1 scripts
echo "Step 1: Generating Phase 1 scripts from Phase 0 template..."
bash create_phase1_scripts.sh
echo ""

# Step 2: Upload scripts to VM1
echo "Step 2: Uploading scripts to VM1 (EPIC-001-005)..."
gcloud compute scp _p1_0{1..5}.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp launch_phase1_vm1.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/_p1_*.sh /home/malhitticrypto/universal-or-strategy/launch_phase1_vm1.sh"
echo "✅ VM1 ready"
echo ""

# Step 3: Upload scripts to VM2
echo "Step 3: Uploading scripts to VM2 (EPIC-006-010)..."
gcloud compute scp _p1_{06..10}.sh v12-test-golden-v3:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp launch_phase1_vm2.sh v12-test-golden-v3:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute ssh v12-test-golden-v3 --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/_p1_*.sh /home/malhitticrypto/universal-or-strategy/launch_phase1_vm2.sh"
echo "✅ VM2 ready"
echo ""

# Step 4: Upload scripts to VM3
echo "Step 4: Uploading scripts to VM3 (EPIC-011-015)..."
gcloud compute scp _p1_{11..15}.sh v12-test-golden-v4:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp launch_phase1_vm3.sh v12-test-golden-v4:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute ssh v12-test-golden-v4 --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/_p1_*.sh /home/malhitticrypto/universal-or-strategy/launch_phase1_vm3.sh"
echo "✅ VM3 ready"
echo ""

# Step 5: Launch execution on all VMs
echo "Step 5: Launching Phase 1 execution on all 3 VMs..."
echo ""

echo "Launching VM1..."
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && bash launch_phase1_vm1.sh"
echo "✅ VM1 launched (5 epics)"
echo ""

echo "Launching VM2..."
gcloud compute ssh v12-test-golden-v3 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && bash launch_phase1_vm2.sh"
echo "✅ VM2 launched (5 epics)"
echo ""

echo "Launching VM3..."
gcloud compute ssh v12-test-golden-v4 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && bash launch_phase1_vm3.sh"
echo "✅ VM3 launched (5 epics)"
echo ""

echo "=========================================="
echo "✅ Phase 1 Execution Started"
echo "=========================================="
echo ""
echo "All 15 epics are now running in parallel across 3 VMs"
echo ""
echo "Monitor progress:"
echo "  VM1: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='screen -ls'"
echo "  VM2: gcloud compute ssh v12-test-golden-v3 --zone=us-central1-a --command='screen -ls'"
echo "  VM3: gcloud compute ssh v12-test-golden-v4 --zone=us-central1-a --command='screen -ls'"
echo ""
echo "Check completion (expect 15 total):"
echo "  VM1: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-{001..005}/00-scope.md 2>/dev/null | wc -l'"
echo "  VM2: gcloud compute ssh v12-test-golden-v3 --zone=us-central1-a --command='ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-{006..010}/00-scope.md 2>/dev/null | wc -l'"
echo "  VM3: gcloud compute ssh v12-test-golden-v4 --zone=us-central1-a --command='ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-{011..015}/00-scope.md 2>/dev/null | wc -l'"
echo ""
echo "Extract bobcoin usage:"
echo "  VM1: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='grep -E \"Cost:.*Balance:|Cost: [0-9]\" /home/malhitticrypto/universal-or-strategy/logs/phase1/EPIC-{001..005}.log'"
echo "  VM2: gcloud compute ssh v12-test-golden-v3 --zone=us-central1-a --command='grep -E \"Cost:.*Balance:|Cost: [0-9]\" /home/malhitticrypto/universal-or-strategy/logs/phase1/EPIC-{006..010}.log'"
echo "  VM3: gcloud compute ssh v12-test-golden-v4 --zone=us-central1-a --command='grep -E \"Cost:.*Balance:|Cost: [0-9]\" /home/malhitticrypto/universal-or-strategy/logs/phase1/EPIC-{011..015}.log'"
echo ""
echo "Estimated completion: 20-30 minutes"

# Made with Bob