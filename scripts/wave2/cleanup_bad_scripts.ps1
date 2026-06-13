# Cleanup Bad Wave 2 Scripts (Tool Bug Instructions)
# Archives scripts that tell agents to use write_to_file/read_file tools

$ErrorActionPreference = "Stop"

Write-Host "=" -NoNewline -ForegroundColor Cyan
Write-Host ("=" * 59) -ForegroundColor Cyan
Write-Host "Wave 2 Script Cleanup - Archive Bad Tool Instructions" -ForegroundColor Cyan
Write-Host "=" -NoNewline -ForegroundColor Cyan
Write-Host ("=" * 59) -ForegroundColor Cyan
Write-Host ""

# Create archive directory
$archiveDir = "scripts/wave2/_deprecated_tool_bugs"
if (-not (Test-Path $archiveDir)) {
    New-Item -ItemType Directory -Path $archiveDir | Out-Null
    Write-Host "[CREATE] $archiveDir" -ForegroundColor Green
}

# Files to archive (contain bad tool instructions)
$badFiles = @(
    "scripts/wave2/launch_phase0_fixed.py",
    "scripts/wave2/launch_wave2_phase0_with_verification.py",
    "scripts/wave2/launch_phase0_v3_custom_mode.py",
    "scripts/wave2/phase0_message_template.txt"
)

Write-Host ""
Write-Host "Archiving files with bad tool instructions..." -ForegroundColor Yellow
Write-Host ""

foreach ($file in $badFiles) {
    if (Test-Path $file) {
        $filename = Split-Path $file -Leaf
        $destination = Join-Path $archiveDir $filename
        Move-Item -Path $file -Destination $destination -Force
        Write-Host "[MOVED] $filename -> _deprecated_tool_bugs/" -ForegroundColor Yellow
    } else {
        Write-Host "[SKIP] $filename (not found)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "=" -NoNewline -ForegroundColor Green
Write-Host ("=" * 59) -ForegroundColor Green
Write-Host "Cleanup Complete" -ForegroundColor Green
Write-Host "=" -NoNewline -ForegroundColor Green
Write-Host ("=" * 59) -ForegroundColor Green
Write-Host ""
Write-Host "CORRECT SCRIPT TO USE:" -ForegroundColor Cyan
Write-Host "  python scripts/wave2/launch_phase0_v4_shell_commands.py" -ForegroundColor White
Write-Host ""
Write-Host "This script uses:" -ForegroundColor Cyan
Write-Host "  - phase0_message_template_shell.txt (shell commands)" -ForegroundColor White
Write-Host "  - Custom mode: v12-phase0-hotspot" -ForegroundColor White
Write-Host "  - NO write_to_file or read_file tool instructions" -ForegroundColor White
Write-Host ""

# Made with Bob
