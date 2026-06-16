# Wave 2 Parallel Execution - Robust Solution
# Uses screen sessions for persistent background execution

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Wave 2 - Parallel Execution (Robust)" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Architecture: 1 VM × 10 Bob Shell Agents (screen sessions)"
Write-Host "Start Time: $(Get-Date)"
Write-Host ""

# Configuration
$PROJECT = "project-14c86305-3cba-493f-a73"
$ZONE = "us-central1-a"
$IMAGE = "v12-bob-shell-golden-v2"
$MACHINE_TYPE = "n2-standard-8"
$DISK_SIZE = "100GB"
$VM_NAME = "v12-wave2-parallel"

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
Write-Host "- Total epics: $($EPICS.Count)"
Write-Host "- Execution: Parallel (screen sessions)"
Write-Host "- Machine: $MACHINE_TYPE (8 vCPUs, 32 GB RAM)"
Write-Host "- VM: $VM_NAME"
Write-Host ""

# Step 1: Launch VM
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 1: Launching VM" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

gcloud compute instances create $VM_NAME `
    --project=$PROJECT `
    --zone=$ZONE `
    --machine-type=$MACHINE_TYPE `
    --image=$IMAGE `
    --boot-disk-size=$DISK_SIZE `
    --maintenance-policy=TERMINATE `
    --provisioning-model=SPOT `
    --scopes=cloud-platform

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ VM launch failed" -ForegroundColor Red
    exit 1
}

Write-Host "✅ VM launched" -ForegroundColor Green
Write-Host ""

# Step 2: Wait for boot
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 2: Waiting for VM Boot" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Waiting 30 seconds..."
Start-Sleep -Seconds 30
Write-Host "✅ VM ready" -ForegroundColor Green
Write-Host ""

# Step 3: Install screen and prepare environment
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 3: Installing screen & Preparing Environment" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

gcloud compute ssh $VM_NAME `
    --project=$PROJECT `
    --zone=$ZONE `
    --command="sudo apt-get update && sudo apt-get install -y screen && cd ~/universal-or-strategy && mkdir -p logs"

Write-Host "✅ Environment ready" -ForegroundColor Green
Write-Host ""

# Step 4: Launch all agents in screen sessions
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 4: Launching $($EPICS.Count) Parallel Agents" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Time: $(Get-Date)"
Write-Host ""

foreach ($epic in $EPICS) {
    Write-Host "Launching screen session for $($epic.ID)..." -ForegroundColor Yellow
    
    $screenCmd = "cd ~/universal-or-strategy && bob --accept-license --auth-method api-key -p 'Run epic-intake for $($epic.ID). Target: Reduce complexity in $($epic.Method) (CYC $($epic.CYC) to 8)' --max-coins 30 > logs/$($epic.ID).log 2>&1"
    
    gcloud compute ssh $VM_NAME `
        --project=$PROJECT `
        --zone=$ZONE `
        --command="screen -dmS $($epic.ID) bash -c '$screenCmd'"
    
    Write-Host "  ✅ Screen session '$($epic.ID)' started" -ForegroundColor Green
    Start-Sleep -Seconds 2
}

Write-Host ""
Write-Host "✅ All $($EPICS.Count) agents launched in screen sessions" -ForegroundColor Green
Write-Host ""

# Step 5: Monitor progress
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 5: Monitoring Progress" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Checking every 5 minutes for 30 minutes..."
Write-Host ""

$maxChecks = 6  # 30 minutes / 5 minutes
$checkInterval = 300  # 5 minutes in seconds

for ($i = 1; $i -le $maxChecks; $i++) {
    Write-Host "--- Check $i of $maxChecks ($(Get-Date)) ---" -ForegroundColor Yellow
    
    # Check running screen sessions
    $screenList = gcloud compute ssh $VM_NAME `
        --project=$PROJECT `
        --zone=$ZONE `
        --command="screen -ls" 2>&1 | Out-String
    
    Write-Host "Active screen sessions:"
    Write-Host $screenList
    
    # Check log file sizes
    Write-Host "`nLog file sizes:"
    foreach ($epic in $EPICS) {
        $logSize = gcloud compute ssh $VM_NAME `
            --project=$PROJECT `
            --zone=$ZONE `
            --command="wc -l ~/universal-or-strategy/logs/$($epic.ID).log 2>/dev/null || echo '0'" 2>&1
        
        Write-Host "  $($epic.ID): $logSize lines"
    }
    
    # Count remaining sessions
    $remainingSessions = ($screenList | Select-String "EPIC-CCN" | Measure-Object).Count
    Write-Host "`nRemaining sessions: $remainingSessions / $($EPICS.Count)" -ForegroundColor Cyan
    
    if ($remainingSessions -eq 0) {
        Write-Host "`n✅ All agents completed!" -ForegroundColor Green
        break
    }
    
    if ($i -lt $maxChecks) {
        Write-Host "`nWaiting 5 minutes before next check..." -ForegroundColor Gray
        Start-Sleep -Seconds $checkInterval
    }
}

Write-Host ""

# Step 6: Retrieve logs
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 6: Retrieving Logs" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path "logs/wave2" | Out-Null

foreach ($epic in $EPICS) {
    Write-Host "Downloading $($epic.ID) log..."
    
    gcloud compute scp "$VM_NAME`:~/universal-or-strategy/logs/$($epic.ID).log" `
        "logs/wave2/$($epic.ID).log" `
        --project=$PROJECT `
        --zone=$ZONE 2>&1 | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        $lines = (Get-Content "logs/wave2/$($epic.ID).log" | Measure-Object -Line).Lines
        Write-Host "  ✅ $lines lines" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️ Failed to download" -ForegroundColor Yellow
    }
}

Write-Host ""

# Step 7: Display summaries
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 7: Log Summaries (last 10 lines each)" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

foreach ($epic in $EPICS) {
    $logPath = "logs/wave2/$($epic.ID).log"
    
    if (Test-Path $logPath) {
        Write-Host "`n--- $($epic.ID) ---" -ForegroundColor Yellow
        Get-Content $logPath -Tail 10
    }
}

Write-Host ""

# Step 8: Stop VM
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 8: Stopping VM" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

gcloud compute instances stop $VM_NAME `
    --project=$PROJECT `
    --zone=$ZONE

Write-Host "✅ VM stopped" -ForegroundColor Green
Write-Host ""

# Summary
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Wave 2 Complete" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "End Time: $(Get-Date)"
Write-Host ""
Write-Host "Results:"
Write-Host "- Epics processed: $($EPICS.Count)"
Write-Host "- Logs: logs/wave2/"
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Review logs in logs/wave2/"
Write-Host "2. Check epic artifacts (will need separate retrieval)"
Write-Host "3. Delete VM: gcloud compute instances delete $VM_NAME --zone=$ZONE"

# Made with Bob
