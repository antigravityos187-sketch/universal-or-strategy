# Upload and Launch 24 Phase 1 Relaunch Scripts
# Building Blocks Method - Wave 6 Phase 1 Recovery

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Uploading 24 Phase 1 Relaunch Scripts" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Get all relaunch scripts
$scripts = Get-ChildItem scripts/wave6/_p1_relaunch_*.sh | Sort-Object Name

Write-Host "Found $($scripts.Count) scripts to upload" -ForegroundColor Yellow

# Upload each script individually
foreach ($script in $scripts) {
    Write-Host "Uploading $($script.Name)..." -ForegroundColor Yellow
    gcloud compute scp $script.FullName v12-test-golden-v2:~/universal-or-strategy/scripts/wave6/$($script.Name) --zone=us-central1-a
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ Uploaded $($script.Name)" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Failed to upload $($script.Name)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Setting permissions and fixing line endings..." -ForegroundColor Yellow
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd ~/universal-or-strategy/scripts/wave6 && chmod +x _p1_relaunch_*.sh && sed -i 's/\r$//' _p1_relaunch_*.sh"

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "✅ Upload Complete!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan

Write-Host ""
Write-Host "Launching all 24 scripts in parallel..." -ForegroundColor Yellow

# Launch all scripts in parallel on VM
$launchCommand = @"
cd ~/universal-or-strategy && \
for script in scripts/wave6/_p1_relaunch_*.sh; do \
  nohup bash \$script > logs/wave6/phase1/\$(basename \$script .sh).log 2>&1 & \
done && \
echo "Launched \$(ls scripts/wave6/_p1_relaunch_*.sh | wc -l) scripts" && \
sleep 2 && \
ps aux | grep '_p1_relaunch' | grep -v grep | wc -l
"@

gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command=$launchCommand

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "✅ Launch Complete!" -ForegroundColor Green
Write-Host "Monitor progress with 4-minute polling" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan

# Made with Bob
