# Download Wave 2 v4 Logs from GCP VM
# Downloads all EPIC-CCN-{107-115}.log files

$epics = @(108, 109, 110, 111, 112, 113, 114, 115)
$zone = "us-central1-a"
$vm = "v12-test-golden-v2"
$remote_path = "/home/malhitticrypto/universal-or-strategy/logs"
$local_path = "logs/wave2"

Write-Host "[INFO] Downloading Wave 2 v4 logs from $vm..." -ForegroundColor Cyan

foreach ($epic in $epics) {
    $filename = "EPIC-CCN-$epic.log"
    $remote_file = "$remote_path/$filename"
    
    Write-Host "[DOWNLOAD] $filename..." -ForegroundColor Yellow
    
    gcloud compute scp "${vm}:${remote_file}" "$local_path/" --zone=$zone
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[SUCCESS] $filename downloaded" -ForegroundColor Green
    } else {
        Write-Host "[ERROR] Failed to download $filename" -ForegroundColor Red
    }
}

Write-Host "`n[COMPLETE] All logs downloaded to $local_path/" -ForegroundColor Cyan
Write-Host "[INFO] EPIC-CCN-107.log was already downloaded" -ForegroundColor Gray

# Made with Bob
