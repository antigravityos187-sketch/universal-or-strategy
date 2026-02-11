# V12.16 Modularization Script - Remove extracted code blocks from main file
# Removes: Entries (1094-1261, 1414-2526), Orders (2528-4843, 5033-5083), UI (4845-5031, 5085-6776)
# Replaces each block with a comment marker

param(
    [string]$Phase = "all"  # "entries", "orders", "ui", or "all"
)

$file = "C:\WSGTA\universal-or-strategy\UniversalORStrategyV12_002_Dev.cs"
$lines = Get-Content $file -Encoding UTF8
Write-Host "Original line count: $($lines.Count)"

# Define removal ranges (1-indexed) and their replacement comments
# Format: @{ Start = N; End = M; Comment = "..." }
$removals = @()

if ($Phase -eq "entries" -or $Phase -eq "all") {
    $removals += @{ Start = 1094; End = 1261; Comment = "        // V12.16: FFMA entry logic moved to Entries.cs" }
    $removals += @{ Start = 1414; End = 2526; Comment = "        // V12.16: OR, RMA, MOMO, TREND, RETEST entry logic moved to Entries.cs" }
}

if ($Phase -eq "orders" -or $Phase -eq "all") {
    $removals += @{ Start = 2528; End = 4843; Comment = "        // V12.16: Order Management, Trailing Stops, Position Sync moved to Orders.cs" }
    $removals += @{ Start = 5033; End = 5083; Comment = "        // V12.16: Stop Management Helpers moved to Orders.cs" }
}

if ($Phase -eq "ui" -or $Phase -eq "all") {
    $removals += @{ Start = 4845; End = 5031; Comment = "        // V12.16: UI handlers moved to UI.cs" }
    $removals += @{ Start = 5085; End = 6776; Comment = "        // V12.16: IPC, Compliance, Position Sizing moved to UI.cs" }
}

# Sort removals in REVERSE order (highest line numbers first) to preserve indices
$removals = $removals | Sort-Object { $_.Start } -Descending

foreach ($removal in $removals) {
    $startIdx = $removal.Start - 1  # Convert to 0-indexed
    $endIdx = $removal.End - 1

    # Verify boundaries
    Write-Host "Removing lines $($removal.Start)-$($removal.End) ($($removal.End - $removal.Start + 1) lines)"
    Write-Host "  First line: $($lines[$startIdx].Substring(0, [Math]::Min(80, $lines[$startIdx].Length)))"
    Write-Host "  Last line:  $($lines[$endIdx].Substring(0, [Math]::Min(80, $lines[$endIdx].Length)))"

    # Build new array: before + comment + after
    $before = $lines[0..($startIdx - 1)]
    $comment = @($removal.Comment, "")
    $after = if ($endIdx -lt $lines.Count - 1) { $lines[($endIdx + 1)..($lines.Count - 1)] } else { @() }

    $lines = $before + $comment + $after
}

# Write result
Set-Content $file -Value $lines -Encoding UTF8
Write-Host "New line count: $($lines.Count)"
Write-Host "Lines removed: $($removals | ForEach-Object { $_.End - $_.Start + 1 } | Measure-Object -Sum | Select-Object -ExpandProperty Sum)"
