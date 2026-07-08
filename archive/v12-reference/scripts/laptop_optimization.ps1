# Laptop Performance Optimization Script
# Run as Administrator for full effect

Write-Host "=== LAPTOP OPTIMIZATION SCRIPT ===" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "WARNING: Not running as Administrator. Some operations will be skipped." -ForegroundColor Yellow
    Write-Host "For full cleanup, right-click PowerShell and 'Run as Administrator'" -ForegroundColor Yellow
    Write-Host ""
}

# 1. Clean Temp Files
Write-Host "1. Cleaning Temporary Files..." -ForegroundColor Yellow
$tempPaths = @(
    "$env:TEMP",
    "$env:LOCALAPPDATA\Temp",
    "C:\Windows\Temp"
)
$totalCleaned = 0
foreach ($path in $tempPaths) {
    if (Test-Path $path) {
        try {
            $beforeSize = (Get-ChildItem -Path $path -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
            Get-ChildItem -Path $path -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
            $afterSize = (Get-ChildItem -Path $path -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
            $cleaned = $beforeSize - $afterSize
            $totalCleaned += $cleaned
            Write-Host "  Cleaned $path : $([math]::Round($cleaned/1MB, 2)) MB" -ForegroundColor Green
        } catch {
            Write-Host "  Error cleaning $path : $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}
Write-Host "  Total temp files cleaned: $([math]::Round($totalCleaned/1GB, 2)) GB" -ForegroundColor Green

# 2. Empty Recycle Bin
Write-Host "`n2. Emptying Recycle Bin..." -ForegroundColor Yellow
try {
    Clear-RecycleBin -Force -ErrorAction Stop
    Write-Host "  Recycle Bin emptied successfully" -ForegroundColor Green
} catch {
    Write-Host "  Could not empty Recycle Bin: $($_.Exception.Message)" -ForegroundColor Red
}

# 3. Clear Windows Update Cache (requires admin)
if ($isAdmin) {
    Write-Host "`n3. Clearing Windows Update Cache..." -ForegroundColor Yellow
    try {
        Stop-Service wuauserv -Force -ErrorAction SilentlyContinue
        $updatePath = "C:\Windows\SoftwareDistribution\Download"
        if (Test-Path $updatePath) {
            $beforeSize = (Get-ChildItem -Path $updatePath -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
            Remove-Item -Path "$updatePath\*" -Recurse -Force -ErrorAction SilentlyContinue
            $afterSize = (Get-ChildItem -Path $updatePath -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
            $cleaned = $beforeSize - $afterSize
            Write-Host "  Cleaned Windows Update cache: $([math]::Round($cleaned/1GB, 2)) GB" -ForegroundColor Green
        }
        Start-Service wuauserv -ErrorAction SilentlyContinue
    } catch {
        Write-Host "  Error cleaning Windows Update cache: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "`n3. Skipping Windows Update Cache (requires admin)" -ForegroundColor Yellow
}

# 4. Disk Cleanup
if ($isAdmin) {
    Write-Host "`n4. Running Disk Cleanup..." -ForegroundColor Yellow
    try {
        Start-Process cleanmgr -ArgumentList "/sagerun:1" -Wait -NoNewWindow
        Write-Host "  Disk Cleanup completed" -ForegroundColor Green
    } catch {
        Write-Host "  Could not run Disk Cleanup: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "`n4. Skipping Disk Cleanup (requires admin)" -ForegroundColor Yellow
}

# 5. Optimize Drives
if ($isAdmin) {
    Write-Host "`n5. Optimizing Drives..." -ForegroundColor Yellow
    try {
        Optimize-Volume -DriveLetter C -Defragment -Verbose
        Write-Host "  Drive C: optimized" -ForegroundColor Green
    } catch {
        Write-Host "  Could not optimize drive: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "`n5. Skipping Drive Optimization (requires admin)" -ForegroundColor Yellow
}

Write-Host "`n=== OPTIMIZATION COMPLETE ===" -ForegroundColor Cyan
Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "1. Restart your computer to apply all changes" -ForegroundColor White
Write-Host "2. Check Task Manager for high CPU/Memory processes" -ForegroundColor White
Write-Host "3. Consider closing unused IBM Bob instances (7 running!)" -ForegroundColor White
Write-Host "4. Update Windows Defender definitions to reduce MsMpEng CPU usage" -ForegroundColor White

# Made with Bob
