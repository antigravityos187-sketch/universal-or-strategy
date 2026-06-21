# Create tarball of migrated manifests for Wave 6
# V12.52 Manifest Migration - Upload to VM

$ErrorActionPreference = "Stop"

Write-Host "Creating tarball of migrated manifests..."

# Get all manifest files
$manifests = Get-ChildItem -Path "docs/brain/EPIC-CCN-*/manifest.json" -Recurse

if ($manifests.Count -eq 0) {
    Write-Host "ERROR: No manifest files found!" -ForegroundColor Red
    exit 1
}

Write-Host "Found $($manifests.Count) manifest files"

# Create a temporary directory structure
$tempDir = "temp_manifests"
if (Test-Path $tempDir) {
    Remove-Item -Recurse -Force $tempDir
}
New-Item -ItemType Directory -Path $tempDir | Out-Null

# Copy manifests preserving directory structure
foreach ($manifest in $manifests) {
    $epicDir = $manifest.Directory.Name
    $destDir = Join-Path $tempDir "docs/brain/$epicDir"
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    Copy-Item $manifest.FullName -Destination $destDir
    Write-Host "  Copied $epicDir/manifest.json"
}

# Create tarball from temp directory
Write-Host "`nCreating tarball..."
tar -czf wave6_manifests_migrated.tar.gz -C $tempDir docs

if ($LASTEXITCODE -eq 0) {
    Write-Host "SUCCESS: Created wave6_manifests_migrated.tar.gz" -ForegroundColor Green
    
    # Show file size
    $size = (Get-Item wave6_manifests_migrated.tar.gz).Length
    Write-Host "File size: $([math]::Round($size/1KB, 2)) KB"
    
    # Cleanup
    Remove-Item -Recurse -Force $tempDir
    Write-Host "`nReady to upload to VM with:"
    Write-Host "  gcloud compute scp wave6_manifests_migrated.tar.gz v12-test-golden-v2:/tmp/ --zone=us-central1-a"
} else {
    Write-Host "ERROR: Failed to create tarball" -ForegroundColor Red
    exit 1
}

# Made with Bob
