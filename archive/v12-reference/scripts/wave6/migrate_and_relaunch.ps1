# Migrate 24 manifests and relaunch Phase 1
# Root Cause: Pre-V12.52 manifests missing lamport_events

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Manifest Migration + Phase 1 Relaunch" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Step 1: Run migration locally
Write-Host ""
Write-Host "Step 1: Migrating manifests locally..." -ForegroundColor Yellow
python scripts/migrate_manifests_v12_52.py --list

Write-Host ""
Write-Host "Running migration (dry-run first)..." -ForegroundColor Yellow
python scripts/migrate_manifests_v12_52.py --all --dry-run

Write-Host ""
$confirm = Read-Host "Proceed with migration? (y/n)"
if ($confirm -ne 'y') {
    Write-Host "Aborted" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Migrating..." -ForegroundColor Yellow
python scripts/migrate_manifests_v12_52.py --all

# Step 2: Sync manifests to VM
Write-Host ""
Write-Host "Step 2: Syncing manifests to VM..." -ForegroundColor Yellow

$epics = @("001", "004", "016", "020", "021", "028", "050", "051", "052", "053", "054", "055", "056", "057", "058", "059", "060", "061", "070", "073", "076", "077", "078", "079")

foreach ($epic in $epics) {
    $manifestPath = "docs/brain/EPIC-CCN-$epic/manifest.json"
    if (Test-Path $manifestPath) {
        Write-Host "  Uploading EPIC-CCN-$epic manifest..." -ForegroundColor Gray
        gcloud compute scp $manifestPath "v12-test-golden-v2:~/universal-or-strategy/docs/brain/EPIC-CCN-$epic/manifest.json" --zone=us-central1-a 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "    ✅ Synced" -ForegroundColor Green
        } else {
            Write-Host "    ❌ Failed" -ForegroundColor Red
        }
    }
}

# Step 3: Relaunch Phase 1 on VM
Write-Host ""
Write-Host "Step 3: Relaunching Phase 1 for 24 epics..." -ForegroundColor Yellow

$launchCommand = @"
cd ~/universal-or-strategy && \
for epic in 001 004 016 020 021 028 050 051 052 053 054 055 056 057 058 059 060 061 070 073 076 077 078 079; do \
  nohup bash scripts/wave6/_p1_epic_ccn_\$epic.sh > logs/wave6/phase1/EPIC-CCN-\$epic-migrated.log 2>&1 & \
done && \
sleep 2 && \
echo "Launched 24 scripts. Running processes:" && \
ps aux | grep '_p1_epic' | grep -v grep | wc -l
"@

gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command=$launchCommand

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "✅ Migration and Relaunch Complete!" -ForegroundColor Green
Write-Host "Monitor with 4-minute polling" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan

# Made with Bob
