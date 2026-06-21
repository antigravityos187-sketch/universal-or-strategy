Write-Host "=== SYSTEM CLEANUP ANALYSIS ===" -ForegroundColor Cyan
Write-Host ""

# 1. Disk Space Analysis
Write-Host "1. DISK SPACE ANALYSIS" -ForegroundColor Yellow
Get-PSDrive -PSProvider FileSystem | Where-Object {$_.Used -gt 0} | Select-Object Name, @{Name="Used(GB)";Expression={[math]::Round($_.Used/1GB,2)}}, @{Name="Free(GB)";Expression={[math]::Round($_.Free/1GB,2)}}, @{Name="Total(GB)";Expression={[math]::Round(($_.Used+$_.Free)/1GB,2)}}, @{Name="Free%";Expression={[math]::Round(($_.Free/($_.Used+$_.Free))*100,1)}} | Format-Table -AutoSize

# 2. Top CPU Processes
Write-Host "`n2. TOP 10 CPU-INTENSIVE PROCESSES" -ForegroundColor Yellow
Get-Process | Sort-Object CPU -Descending | Select-Object -First 10 Name, @{Name="CPU(s)";Expression={[math]::Round($_.CPU,2)}}, @{Name="Memory(MB)";Expression={[math]::Round($_.WorkingSet/1MB,2)}} | Format-Table -AutoSize

# 3. Top Memory Processes
Write-Host "`n3. TOP 10 MEMORY-INTENSIVE PROCESSES" -ForegroundColor Yellow
Get-Process | Sort-Object WorkingSet -Descending | Select-Object -First 10 Name, @{Name="Memory(MB)";Expression={[math]::Round($_.WorkingSet/1MB,2)}}, @{Name="CPU(s)";Expression={[math]::Round($_.CPU,2)}} | Format-Table -AutoSize

# 4. Temp Files Size
Write-Host "`n4. TEMPORARY FILES ANALYSIS" -ForegroundColor Yellow
$tempPaths = @(
    "$env:TEMP",
    "$env:LOCALAPPDATA\Temp",
    "C:\Windows\Temp"
)
foreach ($path in $tempPaths) {
    if (Test-Path $path) {
        $size = (Get-ChildItem -Path $path -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
        $sizeGB = [math]::Round($size/1GB, 2)
        Write-Host "$path : $sizeGB GB"
    }
}

# 5. Large Directories in User Profile
Write-Host "`n5. LARGEST DIRECTORIES IN USER PROFILE (>1GB)" -ForegroundColor Yellow
Get-ChildItem -Path $env:USERPROFILE -Directory -ErrorAction SilentlyContinue | ForEach-Object {
    $size = (Get-ChildItem -Path $_.FullName -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
    [PSCustomObject]@{
        Path = $_.Name
        SizeGB = [math]::Round($size/1GB, 2)
    }
} | Where-Object {$_.SizeGB -gt 1} | Sort-Object SizeGB -Descending | Format-Table -AutoSize

# 6. Windows Update Cache
Write-Host "`n6. WINDOWS UPDATE CACHE" -ForegroundColor Yellow
if (Test-Path "C:\Windows\SoftwareDistribution\Download") {
    $size = (Get-ChildItem -Path "C:\Windows\SoftwareDistribution\Download" -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
    $sizeGB = [math]::Round($size/1GB, 2)
    Write-Host "Windows Update Cache: $sizeGB GB"
}

# 7. Recycle Bin Size
Write-Host "`n7. RECYCLE BIN SIZE" -ForegroundColor Yellow
$shell = New-Object -ComObject Shell.Application
$recycleBin = $shell.Namespace(0xA)
$size = ($recycleBin.Items() | Measure-Object -Property Size -Sum).Sum
$sizeGB = [math]::Round($size/1GB, 2)
Write-Host "Recycle Bin: $sizeGB GB"

# 8. Startup Programs
Write-Host "`n8. STARTUP PROGRAMS" -ForegroundColor Yellow
Get-CimInstance Win32_StartupCommand | Select-Object Name, Command, Location | Format-Table -AutoSize

Write-Host "`n=== ANALYSIS COMPLETE ===" -ForegroundColor Cyan
Write-Host "`nRecommendations will be provided based on these results." -ForegroundColor Green

# Made with Bob
