# Deploy Fixed custom_modes.yaml and Test Phase 0
# This script:
# 1. Pushes the fixed .bob/custom_modes.yaml to VM
# 2. Creates a test script for EPIC-CCN-107 only
# 3. Provides instructions for testing

$VM_NAME = "v12-test-golden-v2"
$ZONE = "us-central1-a"
$PROJECT = "project-14c86305-3cba-493f-a73"
$GCLOUD = "C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd"

Write-Host "[STEP 1] Pushing fixed custom_modes.yaml to VM..." -ForegroundColor Cyan

# Push the fixed custom_modes.yaml
& $GCLOUD compute scp .bob/custom_modes.yaml "${VM_NAME}:/home/malhitticrypto/universal-or-strategy/.bob/" --zone=$ZONE --project=$PROJECT

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Failed to push custom_modes.yaml" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] custom_modes.yaml pushed successfully" -ForegroundColor Green

Write-Host "`n[STEP 2] Generating single-epic test script..." -ForegroundColor Cyan

# Generate test script for EPIC-CCN-107 only
python scripts/wave2/launch_phase0_v3_custom_mode.py

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Failed to generate test scripts" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] Test scripts generated" -ForegroundColor Green

Write-Host "`n[STEP 3] Pushing test script to VM..." -ForegroundColor Cyan

# Push only the first test script
& $GCLOUD compute scp _p0_107.sh "${VM_NAME}:/home/malhitticrypto/universal-or-strategy/" --zone=$ZONE --project=$PROJECT

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Failed to push test script" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] Test script pushed successfully" -ForegroundColor Green

Write-Host "`n[STEP 4] Test Instructions" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. SSH into VM:" -ForegroundColor White
Write-Host "   gcloud compute ssh $VM_NAME --zone=$ZONE --project=$PROJECT" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Navigate to repo:" -ForegroundColor White
Write-Host "   cd /home/malhitticrypto/universal-or-strategy" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Run test script:" -ForegroundColor White
Write-Host "   bash _p0_107.sh" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Verify files created:" -ForegroundColor White
Write-Host "   ls -la docs/brain/EPIC-CCN-107/" -ForegroundColor Gray
Write-Host "   cat docs/brain/EPIC-CCN-107/00-hotspots.md" -ForegroundColor Gray
Write-Host "   cat docs/brain/EPIC-CCN-107/manifest.json" -ForegroundColor Gray
Write-Host ""
Write-Host "5. If successful, run full Wave 2:" -ForegroundColor White
Write-Host "   python scripts/wave2/launch_phase0_v3_custom_mode.py" -ForegroundColor Gray
Write-Host "   # Then push all _p0_*.sh scripts and run launch_phase0_all.sh" -ForegroundColor Gray
Write-Host ""
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "[READY] Test environment prepared. Follow instructions above." -ForegroundColor Green

# Made with Bob
