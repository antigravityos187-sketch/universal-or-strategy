# Upload Missing Phase 5 Scripts to VM
# Wave 4 Recovery - Step 2

$epics = @("003", "015", "030", "031", "033", "042", "055")
$uploaded = 0
$failed = 0

Write-Host "Uploading 7 missing Phase 5 scripts to VM..."

foreach ($epic in $epics) {
    $script = "scripts/wave4/_p5_$epic.sh"
    $remote = "v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/scripts/wave4/_p5_$epic.sh"
    
    Write-Host "Uploading $script..."
    gcloud compute scp $script $remote --zone=us-central1-a
    
    if ($LASTEXITCODE -eq 0) {
        $uploaded++
        Write-Host "[OK] Uploaded: _p5_$epic.sh"
    } else {
        $failed++
        Write-Host "[ERROR] Failed: _p5_$epic.sh"
    }
}

Write-Host "`n=== Upload Summary ==="
Write-Host "Uploaded: $uploaded/7"
Write-Host "Failed: $failed/7"

if ($uploaded -eq 7) {
    Write-Host "`n[OK] All 7 scripts uploaded successfully"
    
    # Verify upload
    Write-Host "`nVerifying upload..."
    $vmCount = gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/scripts/wave4/_p5_{003,015,030,031,033,042,055}.sh 2>/dev/null | wc -l"
    
    if ($vmCount -match "7") {
        Write-Host "[OK] Upload verified: 7 scripts on VM"
        
        # Set permissions
        Write-Host "`nSetting permissions..."
        gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/scripts/wave4/_p5_{003,015,030,031,033,042,055}.sh"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] Permissions set"
            Write-Host "`n[OK] Step 2 complete - Ready to launch Phase 5"
        }
    } else {
        Write-Host "[ERROR] Upload verification failed. Expected 7, got: $vmCount"
    }
} else {
    Write-Host "`n[ERROR] Upload incomplete. $failed scripts failed."
    exit 1
}

# Made with Bob
