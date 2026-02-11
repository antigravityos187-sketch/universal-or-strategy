$base = "C:\WSGTA\universal-or-strategy"
$files = @(
    "UniversalORStrategyV12_002_Dev.cs",
    "UniversalORStrategyV12_002_Dev.Entries.cs",
    "UniversalORStrategyV12_002_Dev.Orders.cs",
    "UniversalORStrategyV12_002_Dev.UI.cs",
    "UniversalORStrategyV12_002_Dev.SIMA.cs",
    "UniversalORStrategyV12_002_Dev.REAPER.cs"
)

foreach ($f in $files) {
    $path = Join-Path $base $f
    if (Test-Path $path) {
        $content = Get-Content $path
        $lineCount = $content.Count
        $firstLine = $content[0]
        $hasPartial = ($content | Select-String "public partial class" | Measure-Object).Count -gt 0
        Write-Host "$f : $lineCount lines | partial: $hasPartial | first: $($firstLine.Substring(0, [Math]::Min(70, $firstLine.Length)))"
    } else {
        Write-Host "$f : NOT FOUND"
    }
}
