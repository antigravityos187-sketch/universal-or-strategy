# Wave 2 TEST - 2 Parallel Agents
# PowerShell version

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Wave 2 TEST - 2 Parallel Agents" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Architecture: 1 VM × 2 Bob Shell Agents"
Write-Host "Start Time: $(Get-Date)"
Write-Host ""

# Configuration
$PROJECT = "project-14c86305-3cba-493f-a73"
$ZONE = "us-central1-a"
$IMAGE = "v12-bob-shell-golden-v2"
$MACHINE_TYPE = "n2-standard-8"
$DISK_SIZE = "100GB"
$VM_NAME = "v12-wave2-test-2agents"

# Test with 2 epics
$EPICS = @(
    @{ID="EPIC-CCN-164"; Method="IsCommandForThisInstrument"; CYC=36},
    @{ID="EPIC-CCN-107"; Method="OnBarUpdate"; CYC=28}
)

Write-Host "Test Configuration:"
Write-Host "- Total epics: $($EPICS.Count)"
Write-Host "- Execution mode: Parallel (2 agents on 1 VM)"
Write-Host "- Machine type: $MACHINE_TYPE (8 vCPUs, 32 GB RAM)"
Write-Host "- VM name: $VM_NAME"
Write-Host ""

# Step 1: Launch VM
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 1: Launching VM" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Time: $(Get-Date)"
Write-Host ""

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

# Step 2: Wait for VM boot
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 2: Waiting for VM Boot" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Waiting 30 seconds..."
Start-Sleep -Seconds 30
Write-Host "✅ VM ready" -ForegroundColor Green
Write-Host ""

# Step 3: Create logs directory
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 3: Preparing Environment" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

gcloud compute ssh $VM_NAME `
    --project=$PROJECT `
    --zone=$ZONE `
    --command="mkdir -p ~/universal-or-strategy/logs"

Write-Host "✅ Logs directory created" -ForegroundColor Green
Write-Host ""

# Step 4: Launch 2 parallel agents
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 4: Launching 2 Parallel Agents" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Time: $(Get-Date)"
Write-Host ""

# Build parallel command
$parallelCmd = "cd ~/universal-or-strategy && "

foreach ($epic in $EPICS) {
    Write-Host "Launching agent for $($epic.ID) ($($epic.Method), CYC $($epic.CYC))"
    
    $parallelCmd += "bob --accept-license --auth-method api-key -p 'Run epic-intake for $($epic.ID). Target: Reduce complexity in $($epic.Method) (CYC $($epic.CYC) to 8)' --max-coins 30 > logs/$($epic.ID).log 2>&1 & "
}

$parallelCmd += "wait && echo 'Both agents complete'"

Write-Host ""
Write-Host "Executing parallel agents..."
Write-Host ""

# Execute via SSH (this will block until both agents complete)
gcloud compute ssh $VM_NAME `
    --project=$PROJECT `
    --zone=$ZONE `
    --command=$parallelCmd

Write-Host ""
Write-Host "✅ Both agents completed" -ForegroundColor Green
Write-Host ""

# Step 5: Retrieve logs
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 5: Retrieving Logs" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path "logs/wave2-test" | Out-Null

foreach ($epic in $EPICS) {
    gcloud compute scp "$VM_NAME`:~/universal-or-strategy/logs/$($epic.ID).log" `
        "logs/wave2-test/$($epic.ID).log" `
        --project=$PROJECT `
        --zone=$ZONE
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️ Could not retrieve log for $($epic.ID)" -ForegroundColor Yellow
    }
}

Write-Host "✅ Logs retrieved" -ForegroundColor Green
Write-Host ""

# Step 6: Display log summaries
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 6: Log Summaries" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

foreach ($epic in $EPICS) {
    Write-Host ""
    Write-Host "--- $($epic.ID) Log (last 20 lines) ---" -ForegroundColor Yellow
    
    $logPath = "logs/wave2-test/$($epic.ID).log"
    if (Test-Path $logPath) {
        Get-Content $logPath -Tail 20
    } else {
        Write-Host "⚠️ Log not found" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Step 7: Stop VM
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Step 7: Stopping VM" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

gcloud compute instances stop $VM_NAME `
    --project=$PROJECT `
    --zone=$ZONE

Write-Host "✅ VM stopped" -ForegroundColor Green
Write-Host ""

# Summary
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Test Complete" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "End Time: $(Get-Date)"
Write-Host ""
Write-Host "Results:"
Write-Host "- Epics processed: $($EPICS.Count)"
Write-Host "- Logs: logs/wave2-test/"
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Review logs above"
Write-Host "2. If successful, run full Wave 2 (10 agents)"
Write-Host "3. Delete test VM: gcloud compute instances delete $VM_NAME --zone=$ZONE"

# Made with Bob
