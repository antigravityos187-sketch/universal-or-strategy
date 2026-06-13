# Wave 2 Launch - Golden Image v3 with Metadata-Driven Orchestration
# VMs self-orchestrate parallel execution using built-in script

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Wave 2 - Metadata-Driven Parallel Execution" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Architecture: Golden Image v3 with built-in orchestrator"
Write-Host "Start Time: $(Get-Date)"
Write-Host ""

# Configuration
$PROJECT = "project-14c86305-3cba-493f-a73"
$ZONE = "us-central1-a"
$IMAGE = "v12-bob-shell-golden-v3"
$MACHINE_TYPE = "n2-standard-8"
$DISK_SIZE = "100GB"
$VM_NAME = "v12-wave2-v3"

# Wave 2 epics (10 epics) - JSON format for metadata
$EPICS_JSON = @'
[
  {"id":"EPIC-CCN-164","method":"IsCommandForThisInstrument","cyc":36},
  {"id":"EPIC-CCN-107","method":"OnBarUpdate","cyc":28},
  {"id":"EPIC-CCN-108","method":"OnOrderUpdate","cyc":26},
  {"id":"EPIC-CCN-109","method":"OnExecutionUpdate","cyc":24},
  {"id":"EPIC-CCN-110","method":"OnPositionUpdate","cyc":22},
  {"id":"EPIC-CCN-111","method":"OnAccountItemUpdate","cyc":21},
  {"id":"EPIC-CCN-112","method":"ProcessMarketData","cyc":20},
  {"id":"EPIC-CCN-113","method":"ValidateOrderParameters","cyc":20},
  {"id":"EPIC-CCN-114","method":"CalculatePositionSize","cyc":19},
  {"id":"EPIC-CCN-115","method":"UpdateRiskMetrics","cyc":19}
]
'@

$EPIC_COUNT = ($EPICS_JSON | ConvertFrom-Json).Count

Write-Host "Configuration:"
Write-Host "- Total epics: $EPIC_COUNT"
Write-Host "- Execution: Self-orchestrated parallel (metadata-driven)"
Write-Host "- Machine: $MACHINE_TYPE (8 vCPUs, 32 GB RAM)"
Write-Host "- Image: $IMAGE (with built-in orchestrator)"
Write-Host "- VM: $VM_NAME"
Write-Host ""

# Step 1: Launch VM with epic metadata
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 1: Launching VM with Epic Metadata" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Escape JSON for gcloud metadata
$EPICS_ESCAPED = $EPICS_JSON -replace '"','\"'

gcloud compute instances create $VM_NAME `
    --project=$PROJECT `
    --zone=$ZONE `
    --machine-type=$MACHINE_TYPE `
    --image=$IMAGE `
    --boot-disk-size=$DISK_SIZE `
    --maintenance-policy=TERMINATE `
    --provisioning-model=SPOT `
    --scopes=cloud-platform `
    --metadata="epics=$EPICS_JSON"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ VM launch failed" -ForegroundColor Red
    exit 1
}

Write-Host "✅ VM launched with epic metadata" -ForegroundColor Green
Write-Host ""

# Step 2: Wait for orchestrator to start
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 2: Waiting for Orchestrator to Start" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Waiting 60 seconds for VM boot and orchestrator startup..."
Start-Sleep -Seconds 60
Write-Host "✅ Orchestrator should be running" -ForegroundColor Green
Write-Host ""

# Step 3: Monitor progress
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 3: Monitoring Progress" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Checking every 5 minutes for up to 60 minutes..."
Write-Host ""

$maxChecks = 12  # 60 minutes / 5 minutes
$checkInterval = 300  # 5 minutes in seconds

for ($i = 1; $i -le $maxChecks; $i++) {
    Write-Host "--- Check $i of $maxChecks ($(Get-Date)) ---" -ForegroundColor Yellow
    
    # Check screen sessions
    Write-Host "Checking active screen sessions..."
    $screenOutput = gcloud compute ssh $VM_NAME `
        --project=$PROJECT `
        --zone=$ZONE `
        --command="screen -ls 2>&1 || echo 'No sessions'" 2>&1 | Out-String
    
    Write-Host $screenOutput
    
    # Count active sessions
    $activeSessions = ($screenOutput | Select-String "EPIC-CCN" | Measure-Object).Count
    Write-Host "Active sessions: $activeSessions / $EPIC_COUNT" -ForegroundColor Cyan
    
    # Check log files
    Write-Host "`nLog file status:"
    $logStatus = gcloud compute ssh $VM_NAME `
        --project=$PROJECT `
        --zone=$ZONE `
        --command="cd ~/universal-or-strategy/logs && ls -lh *.log 2>/dev/null | awk '{print \`$9, \`$5}' || echo 'No logs yet'" 2>&1 | Out-String
    
    Write-Host $logStatus
    
    if ($activeSessions -eq 0 -and $i -gt 2) {
        Write-Host "`n✅ All agents completed!" -ForegroundColor Green
        break
    }
    
    if ($i -lt $maxChecks) {
        Write-Host "`nWaiting 5 minutes before next check..." -ForegroundColor Gray
        Start-Sleep -Seconds $checkInterval
    }
}

Write-Host ""

# Step 4: Retrieve logs
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 4: Retrieving Logs" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path "logs/wave2-v3" | Out-Null

$epics = $EPICS_JSON | ConvertFrom-Json
foreach ($epic in $epics) {
    Write-Host "Downloading $($epic.id) log..."
    
    gcloud compute scp "$VM_NAME`:~/universal-or-strategy/logs/$($epic.id).log" `
        "logs/wave2-v3/$($epic.id).log" `
        --project=$PROJECT `
        --zone=$ZONE 2>&1 | Out-Null
    
    if ($LASTEXITCODE -eq 0 -and (Test-Path "logs/wave2-v3/$($epic.id).log")) {
        $lines = (Get-Content "logs/wave2-v3/$($epic.id).log" | Measure-Object -Line).Lines
        Write-Host "  ✅ $lines lines" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️ Failed to download" -ForegroundColor Yellow
    }
}

Write-Host ""

# Step 5: Display summaries
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 5: Log Summaries (last 15 lines each)" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

foreach ($epic in $epics) {
    $logPath = "logs/wave2-v3/$($epic.id).log"
    
    if (Test-Path $logPath) {
        Write-Host "`n--- $($epic.id) ($($epic.method), CYC $($epic.cyc)) ---" -ForegroundColor Yellow
        Get-Content $logPath -Tail 15
    }
}

Write-Host ""

# Step 6: Stop VM
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 6: Stopping VM" -ForegroundColor Cyan
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
Write-Host "- Epics processed: $EPIC_COUNT"
Write-Host "- Logs: logs/wave2-v3/"
Write-Host "- Architecture: Metadata-driven self-orchestration"
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Review logs in logs/wave2-v3/"
Write-Host "2. Retrieve epic artifacts if needed"
Write-Host "3. Delete VM: gcloud compute instances delete $VM_NAME --zone=$ZONE"
Write-Host "4. Use this approach for all future waves"

# Made with Bob
