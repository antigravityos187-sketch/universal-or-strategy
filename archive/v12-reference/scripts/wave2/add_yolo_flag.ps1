# Add --yolo flag to all Phase 0 scripts

$epics = 107, 108, 109, 110, 111, 112, 113, 114, 115

foreach ($epic in $epics) {
    $script = "_p0_$epic.sh"
    if (Test-Path $script) {
        $content = Get-Content $script -Raw
        $content = $content -replace 'bob --chat-mode', 'bob --yolo --chat-mode'
        Set-Content $script $content -NoNewline
        Write-Host "✅ Fixed $script" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "All 9 scripts updated with --yolo flag" -ForegroundColor Cyan
Write-Host "Files will now persist on disk!" -ForegroundColor Cyan

# Made with Bob
