$src = "C:\Users\Mohammed Khalid\Documents\NinjaTrader 8\bin\Custom_OLD"
$dst = "C:\Users\Mohammed Khalid\Documents\NinjaTrader 8\bin\Custom"

$locales = @("de-DE","es-ES","fr-FR","it-IT","ko-KR","pt-PT","ru-RU","zh-Hans")
foreach ($loc in $locales) {
    $srcDir = Join-Path $src $loc
    $dstDir = Join-Path $dst $loc
    if (Test-Path $srcDir) {
        New-Item -ItemType Directory -Path $dstDir -Force | Out-Null
        Copy-Item "$srcDir\*" $dstDir -Force -ErrorAction SilentlyContinue
        Write-Host "Copied $loc"
    }
}
Write-Host "All locale folders restored"
