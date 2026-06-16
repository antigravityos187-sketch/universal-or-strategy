# Wave 2 Launch - Final Version with Orchestrator Upload
# Uploads orchestrator script, then executes with metadata

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Wave 2 - Parallel Execution (Final)" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Start Time: $(Get-Date)"
Write-Host ""

$PROJECT = "project-14c86305-3cba-493f-a73"
$ZONE = "us-central1-a"
$IMAGE = "v12-bob-shell-golden-v3"
$MACHINE_TYPE = "n2-standard-8"
$DISK_SIZE = "100GB"
$VM_NAME = "v12-wave2-final"

# Wave 2 epics (10 epics)
$EPICS = @(
    @{ID="EPIC-CCN-164"; Method="IsCommandForThisInstrument"; CYC=36},
    @{ID="EPIC-CCN-107"; Method="OnBarUpdate"; CYC=28},
    @{ID="EPIC-CCN-108"; Method="OnOrderUpdate"; CYC=26},
    @{ID="EPIC-CCN-109"; Method="OnExecutionUpdate"; CYC=24},
    @{ID="EPIC-CCN-110"; Method="OnPositionUpdate"; CYC=22},
    @{ID="EPIC-CCN-111"; Method="OnAccountItemUpdate"; CYC=21},
    @{ID="EPIC-CCN-112"; Method="ProcessMarketData"; CYC=20},
    @{ID="EPIC-CCN-113"; Method="ValidateOrderParameters"; CYC=20},
    @{ID="EPIC-CCN-114"; Method="CalculatePositionSize"; CYC=19},
    @{ID="EPIC-CCN-115"; Method="UpdateRiskMetrics"; CYC=19}
)

Write-Host "Configuration:"
Write-Host "- Epics: $($EPICS.Count)"
Write-Host "- Machine: $MACHINE_TYPE"
Write-Host "- Image: $IMAGE"
Write-Host "- VM: $VM_NAME"
Write-Host ""

# Step 1: Launch VM
Write-Host "Step 1: Launching VM..." -ForegroundColor Cyan
gcloud compute instances create $VM_NAME `
    --project=$PROJECT `
    --zone=$ZONE `
    --machine-type=$MACHINE_TYPE `
    --image=$IMAGE `
    --boot-disk-size=$DISK_SIZE `
    --maintenance-policy=TERMINATE `
    --provisioning-model=SPOT `
    --scopes=cloud-platform

if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Host "✅ VM launched" -ForegroundColor Green
Write-Host ""

# Step 2: Wait for boot
Write-Host "Step 2: Waiting for boot (45 seconds)..." -ForegroundColor Cyan
Start-Sleep -Seconds 45
Write-Host "✅ Ready" -ForegroundColor Green
Write-Host ""

# Step 3: Create and upload orchestrator
Write-Host "Step 3: Creating orchestrator script..." -ForegroundColor Cyan

$orchestrator = @'
#!/bin/bash
set -e
cd ~/universal-or-strategy
mkdir -p logs

EPICS='@

# Add epic data
foreach ($epic in $EPICS) {
    $orchestrator += "`n$($epic.ID)|$($epic.Method)|$($epic.CYC)"
}

$orchestrator += @'
'

echo "Launching parallel agents..."
while IFS='|' read -r ID METHOD CYC; do
    [ -z "$ID" ] && continue
    echo "Starting $ID ($METHOD, CYC $CYC)"
    screen -dmS "$ID" bash -c "cd ~/universal-or-strategy && source ~/.bashrc && bob --accept-license --auth-method api-key -p 'Run epic-intake for $ID. Target: Reduce complexity in $METHOD (CYC $CYC to 8)' --max-coins 30 > logs/${ID}.log 2>&1"
    sleep 2
done <<< "$EPICS"

echo "All agents launched. Monitoring..."
while true; do
    ACTIVE=$(screen -ls | grep -c "EPIC-CCN" || true)
    [ $ACTIVE -eq 0 ] && break
    echo "$(date): $ACTIVE agents running"
    sleep 60
done
echo "Complete!"
'@

$orchestrator | Out-File -FilePath "scripts/temp_orch.sh" -Encoding ASCII -NoNewline

gcloud compute scp scripts/temp_orch.sh "$VM_NAME`:~/run_wave2.sh" `
    --project=$PROJECT `
    --zone=$ZONE

gcloud compute ssh $VM_NAME `
    --project=$PROJECT `
    --zone=$ZONE `
    --command="chmod +x ~/run_wave2.sh"

Remove-Item "scripts/temp_orch.sh"

Write-Host "✅ Orchestrator uploaded" -ForegroundColor Green
Write-Host ""

# Step 4: Execute orchestrator
Write-Host "Step 4: Executing orchestrator..." -ForegroundColor Cyan
Write-Host "This will run in background. Monitoring every 5 minutes..." -ForegroundColor Yellow
Write-Host ""

# Start orchestrator in background
gcloud compute ssh $VM_NAME `
    --project=$PROJECT `
    --zone=$ZONE `
    --command="nohup ~/run_wave2.sh > ~/orchestrator.log 2>&1 &"

Write-Host "✅ Orchestrator started" -ForegroundColor Green
Write-Host ""

# Step 5: Monitor progress
Write-Host "Step 5: Monitoring (60 minutes max)..." -ForegroundColor Cyan
Write-Host ""

for ($i = 1; $i -le 12; $i++) {
    Write-Host "--- Check $i/12 ($(Get-Date)) ---" -ForegroundColor Yellow
    
    $screens = gcloud compute ssh $VM_NAME --project=$PROJECT --zone=$ZONE --command="screen -ls 2>&1 || echo 'No sessions'" 2>&1 | Out-String
    $active = ($screens | Select-String "EPIC-CCN" | Measure-Object).Count
    
    Write-Host "Active: $active / $($EPICS.Count)"
    
    if ($active -eq 0 -and $i -gt 2) {
        Write-Host "`n✅ All complete!" -ForegroundColor Green
        break
    }
    
    if ($i -lt 12) {
        Write-Host "Waiting 5 minutes...`n"
        Start-Sleep -Seconds 300
    }
}

# Step 6: Retrieve logs
Write-Host "`nStep 6: Retrieving logs..." -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path "logs/wave2-final" | Out-Null

foreach ($epic in $EPICS) {
    gcloud compute scp "$VM_NAME`:~/universal-or-strategy/logs/$($epic.ID).log" "logs/wave2-final/$($epic.ID).log" --project=$PROJECT --zone=$ZONE 2>&1 | Out-Null
    if (Test-Path "logs/wave2-final/$($epic.ID).log") {
        $lines = (Get-Content "logs/wave2-final/$($epic.ID).log" | Measure-Object -Line).Lines
        Write-Host "  $($epic.ID): $lines lines" -ForegroundColor Green
    }
}

Write-Host ""

# Step 7: Stop VM
Write-Host "Step 7: Stopping VM..." -ForegroundColor Cyan
gcloud compute instances stop $VM_NAME --project=$PROJECT --zone=$ZONE
Write-Host "✅ Stopped" -ForegroundColor Green
Write-Host ""

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Wave 2 Complete!" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "End: $(Get-Date)"
Write-Host "Logs: logs/wave2-final/"

# Made with Bob
