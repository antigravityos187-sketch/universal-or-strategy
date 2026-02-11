$searchPath = "C:\WSGTA\universal-or-strategy"
$term = "OneDrive"

Get-ChildItem -Path $searchPath -Recurse -File | Where-Object { $_.FullName -notmatch "\\.git\\" -and $_.Name -ne "execute_path_replacement.ps1" -and $_.Name -ne "audit_path_references.ps1" -and $_.Name -ne "audit_path_references_v2.ps1" } | ForEach-Object {
    $file = $_.FullName
    try {
        $content = Get-Content -Path $file -Raw -ErrorAction Stop
        if ($content -match $term) {
            Write-Output "Found '$term' in: $file"
        }
    }
    catch {
        # Ignore
    }
}
