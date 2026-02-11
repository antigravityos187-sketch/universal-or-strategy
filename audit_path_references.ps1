$searchPath = "C:\WSGTA\universal-or-strategy"
$oldPath = "C:\Users\Mohammed Khalid\OneDrive\Desktop\WSGTA\Github\universal-or-strategy"

Get-ChildItem -Path $searchPath -Recurse -File | Where-Object { $_.FullName -notmatch "\\.git\\" } | ForEach-Object {
    $file = $_.FullName
    try {
        $content = Get-Content -Path $file -Raw -ErrorAction Stop
        if ($content -match [regex]::Escape($oldPath)) {
            Write-Output "Found in: $file"
        }
    }
    catch {
        # Ignore read errors
    }
}
