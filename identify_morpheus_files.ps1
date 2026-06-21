# Identify Morpheus vs V12 files in src/
$ErrorActionPreference = "Stop"

Write-Host "=" * 80
Write-Host "IDENTIFYING MORPHEUS VS V12 FILES"
Write-Host "=" * 80

# Get all .cs files in src/
$allFiles = Get-ChildItem -Path "src" -Filter "*.cs" | Select-Object -ExpandProperty Name | Sort-Object

Write-Host "`nTotal .cs files in src/: $($allFiles.Count)"
Write-Host "=" * 80

# V12 Photon Kernel patterns (from architecture.md)
$v12Patterns = @(
    "V12_002.*",
    "SignalBroadcaster.cs",
    "AGENTS.md"  # Not a .cs file but listed
)

# Morpheus patterns (based on architecture.md - Morpheus Substrate section)
# Note: Morpheus is described as "Cross-Process" with Electron/Svelte/Adapters
# These would NOT be in src/ as C# files - they'd be separate projects
$morpheusPatterns = @(
    "*Morpheus*",
    "*Electron*",
    "*Svelte*",
    "*Adapter*",
    "*MPMC*",
    "*XOR*"
)

# Categorize files
$v12Files = @()
$morpheusFiles = @()
$unknownFiles = @()

foreach ($file in $allFiles) {
    $isV12 = $false
    $isMorpheus = $false
    
    # Check V12 patterns
    foreach ($pattern in $v12Patterns) {
        if ($file -like $pattern) {
            $isV12 = $true
            break
        }
    }
    
    # Check Morpheus patterns
    foreach ($pattern in $morpheusPatterns) {
        if ($file -like $pattern) {
            $isMorpheus = $true
            break
        }
    }
    
    if ($isV12) {
        $v12Files += $file
    } elseif ($isMorpheus) {
        $morpheusFiles += $file
    } else {
        $unknownFiles += $file
    }
}

Write-Host "`nV12 PHOTON KERNEL FILES ($($v12Files.Count) files):"
Write-Host "=" * 80
$v12Files | ForEach-Object { Write-Host "  $_" }

if ($morpheusFiles.Count -gt 0) {
    Write-Host "`n`nMORPHEUS SUBSTRATE FILES ($($morpheusFiles.Count) files):"
    Write-Host "=" * 80
    $morpheusFiles | ForEach-Object { Write-Host "  $_" }
} else {
    Write-Host "`n`nMORPHEUS SUBSTRATE FILES: 0"
    Write-Host "=" * 80
    Write-Host "  (No Morpheus files found in src/ - likely in separate project)"
}

if ($unknownFiles.Count -gt 0) {
    Write-Host "`n`nUNKNOWN/UNCATEGORIZED FILES ($($unknownFiles.Count) files):"
    Write-Host "=" * 80
    $unknownFiles | ForEach-Object { Write-Host "  $_" }
}

Write-Host "`n" + ("=" * 80)
Write-Host "SUMMARY:"
Write-Host "  Total files: $($allFiles.Count)"
Write-Host "  V12 Photon Kernel: $($v12Files.Count)"
Write-Host "  Morpheus Substrate: $($morpheusFiles.Count)"
Write-Host "  Unknown: $($unknownFiles.Count)"
Write-Host "=" * 80

# Based on architecture.md, ALL files in src/ should be V12
# Morpheus is a separate cross-process substrate (Electron/Svelte/etc)
Write-Host "`nCONCLUSION:"
Write-Host "Based on architecture.md, Morpheus is a SEPARATE cross-process substrate."
Write-Host "All .cs files in src/ appear to be V12 Photon Kernel components."
Write-Host "Morpheus components (Electron, Svelte, Adapters) would be in separate directories."
Write-Host "`nRecommendation: Check for Morpheus directories outside src/"
Write-Host "=" * 80

# Made with Bob
