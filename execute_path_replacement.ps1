$searchPath = "C:\WSGTA\universal-or-strategy"

# Define the replacements map (Old -> New)
$replacements = @{
    # Standard Windows Path
    [regex]::Escape("C:\Users\Mohammed Khalid\OneDrive\Desktop\WSGTA\Github\universal-or-strategy")        = "C:\WSGTA\universal-or-strategy"
    
    # JSON Escaped Path (Double Backslash)
    [regex]::Escape("C:\\Users\\Mohammed Khalid\\OneDrive\\Desktop\\WSGTA\\Github\\universal-or-strategy") = "C:\\WSGTA\\universal-or-strategy"

    # Forward Slash Path
    [regex]::Escape("C:/Users/Mohammed Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy")        = "C:/WSGTA/universal-or-strategy"

    # Shell Path (Git Bash style)
    [regex]::Escape("//c/Users/Mohammed Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy")       = "//c/WSGTA/universal-or-strategy"
}

# Get all files, excluding .git
$files = Get-ChildItem -Path $searchPath -Recurse -File | Where-Object { $_.FullName -notmatch "\\.git\\" -and $_.Name -ne "execute_path_replacement.ps1" -and $_.Name -ne "audit_path_references.ps1" -and $_.Name -ne "audit_path_references_v2.ps1" }

$totalFilesModified = 0

foreach ($file in $files) {
    try {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction Stop
        $newContent = $content
        $modified = $false

        foreach ($old in $replacements.Keys) {
            $new = $replacements[$old]
            if ($newContent -match $old) {
                $newContent = $newContent -replace $old, $new
                $modified = $true
            }
        }

        if ($modified) {
            Set-Content -Path $file.FullName -Value $newContent -NoNewline -Encoding UTF8
            Write-Host "Modified: $($file.FullName)"
            $totalFilesModified++
        }
    }
    catch {
        # Skip binary files or locked files
        # Write-Warning "Could not process $($file.FullName): $_"
    }
}

Write-Host "Total files modified: $totalFilesModified"
