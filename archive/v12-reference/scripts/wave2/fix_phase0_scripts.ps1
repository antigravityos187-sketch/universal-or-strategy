# Fix Phase 0 scripts: Replace run_shell_command with execute_command
# Minimal change - only fix the tool name in instructions

$epics = 107..115

foreach ($epic in $epics) {
    $scriptPath = "_p0_$epic.sh"
    
    if (Test-Path $scriptPath) {
        Write-Host "Fixing $scriptPath..."
        
        # Read content
        $content = Get-Content $scriptPath -Raw
        
        # Replace run_shell_command with execute_command in the message
        $content = $content -replace 'run_shell_command', 'execute_command'
        
        # Add note about cwd parameter (insert after the execute_command explanation)
        $content = $content -replace '(✅ ALWAYS use execute_command tool)', '$1 with cwd parameter set to /home/malhitticrypto/universal-or-strategy'
        
        # Write back
        $content | Out-File -FilePath $scriptPath -Encoding UTF8 -NoNewline
        
        Write-Host "✓ Fixed $scriptPath"
    } else {
        Write-Host "⚠ $scriptPath not found"
    }
}

Write-Host "`n✅ All scripts fixed"
Write-Host "Changes made:"
Write-Host "  - Replaced 'run_shell_command' with 'execute_command' throughout"
Write-Host "  - Added cwd parameter note to instructions"

# Made with Bob
