# Fix Phase 1 scripts to use bash -l (login shell) for PATH loading
# This ensures bob command is available in screen sessions

$scripts = @(
    "_p1_107.sh",
    "_p1_108.sh",
    "_p1_109.sh",
    "_p1_110.sh",
    "_p1_111.sh",
    "_p1_112.sh",
    "_p1_113.sh",
    "_p1_114.sh",
    "_p1_115.sh"
)

foreach ($script in $scripts) {
    Write-Host "Fixing $script..."
    
    # Read content
    $content = Get-Content $script -Raw
    
    # Replace: bob --yolo --chat-mode plan
    # With: bash -l -c 'bob --yolo --chat-mode plan ...'
    $content = $content -replace 'bob --yolo --chat-mode plan "\$\(cat /tmp/phase1_msg_(\d+)\.txt\)"', 'bash -l -c ''bob --yolo --chat-mode plan "$(cat /tmp/phase1_msg_$1.txt)"'''
    
    # Write back
    Set-Content -Path $script -Value $content -NoNewline
    
    Write-Host "[OK] Fixed $script"
}

Write-Host ""
Write-Host "[OK] Fixed all 9 Phase 1 scripts"

# Made with Bob
