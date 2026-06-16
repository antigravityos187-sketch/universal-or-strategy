# Deploy Fixed custom_modes.yaml and Test
# ONLY CHANGE: Fixed write_to_file tool configuration in v12-phase0-hotspot mode
# Everything else (API keys, epics, budget) is IDENTICAL to previous Wave 2 runs

$VM_NAME = "v12-test-golden-v2"
$ZONE = "us-central1-a"
$PROJECT = "project-14c86305-3cba-493f-a73"
$GCLOUD = "C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd"

Write-Host ""
Write-Host "=== Wave 2 Phase 0 - Tool Fix Deployment ===" -ForegroundColor Cyan
Write-Host "ONLY CHANGE: Fixed .bob/custom_modes.yaml (write_to_file tool)" -ForegroundColor Yellow
Write-Host "Everything else IDENTICAL to previous runs" -ForegroundColor Yellow
Write-Host ""

# Step 1: Push fixed custom_modes.yaml
Write-Host "[1/4] Pushing fixed .bob/custom_modes.yaml to VM..." -ForegroundColor White
& $GCLOUD compute scp .bob/custom_modes.yaml "${VM_NAME}:/home/malhitticrypto/universal-or-strategy/.bob/" --zone=$ZONE --project=$PROJECT

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Failed to push custom_modes.yaml" -ForegroundColor Red
    exit 1
}
Write-Host "      OK: custom_modes.yaml deployed" -ForegroundColor Green

# Step 2: Generate Phase 0 scripts (IDENTICAL to previous runs)
Write-Host ""
Write-Host "[2/4] Generating Phase 0 scripts (same API allocation)..." -ForegroundColor White
python scripts/wave2/launch_phase0_v3_custom_mode.py

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Failed to generate scripts" -ForegroundColor Red
    exit 1
}
Write-Host "      OK: Scripts generated (_p0_107.sh through _p0_115.sh)" -ForegroundColor Green

# Step 3: Push scripts to VM
Write-Host ""
Write-Host "[3/4] Pushing scripts to VM..." -ForegroundColor White
& $GCLOUD compute scp _p0_*.sh "${VM_NAME}:/home/malhitticrypto/universal-or-strategy/" --zone=$ZONE --project=$PROJECT

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Failed to push scripts" -ForegroundColor Red
    exit 1
}
Write-Host "      OK: All 9 scripts pushed" -ForegroundColor Green

# Step 4: Instructions
Write-Host ""
Write-Host "[4/4] Ready to test!" -ForegroundColor Green
Write-Host ""
Write-Host "=== Testing Instructions ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "OPTION A: Test single epic first (recommended)" -ForegroundColor Yellow
Write-Host "  1. SSH: gcloud compute ssh $VM_NAME --zone=$ZONE --project=$PROJECT" -ForegroundColor Gray
Write-Host "  2. cd /home/malhitticrypto/universal-or-strategy" -ForegroundColor Gray
Write-Host "  3. bash _p0_107.sh" -ForegroundColor Gray
Write-Host "  4. Verify: ls -la docs/brain/EPIC-CCN-107/" -ForegroundColor Gray
Write-Host "  5. If successful, proceed to Option B" -ForegroundColor Gray
Write-Host ""
Write-Host "OPTION B: Launch all 9 epics in parallel" -ForegroundColor Yellow
Write-Host "  1. SSH: gcloud compute ssh $VM_NAME --zone=$ZONE --project=$PROJECT" -ForegroundColor Gray
Write-Host "  2. cd /home/malhitticrypto/universal-or-strategy" -ForegroundColor Gray
Write-Host "  3. bash launch_phase0_all.sh" -ForegroundColor Gray
Write-Host "  4. Monitor: screen -r p0-107" -ForegroundColor Gray
Write-Host "  5. Check logs: tail -f logs/phase0/EPIC-CCN-107.log" -ForegroundColor Gray
Write-Host ""
Write-Host "=== What Changed ===" -ForegroundColor Cyan
Write-Host "BEFORE: groups: [read, edit, command, mcp]" -ForegroundColor Red
Write-Host "AFTER:  groups: [read, [edit, fileRegex], command, mcp]" -ForegroundColor Green
Write-Host ""
Write-Host "=== What Stayed the Same ===" -ForegroundColor Cyan
Write-Host "  API key allocation: Same as Wave 2 v4" -ForegroundColor Gray
Write-Host "  Budget: 150 bobcoins per epic" -ForegroundColor Gray
Write-Host "  Epic list: 107-115, same methods, same CYC" -ForegroundColor Gray
Write-Host "  Launch scripts: Unchanged" -ForegroundColor Gray
Write-Host ""
Write-Host "READY: Deployment complete. Follow testing instructions above." -ForegroundColor Green

# Made with Bob
