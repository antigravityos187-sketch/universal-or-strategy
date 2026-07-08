# Fix Complexity Threshold: 15 → 8
# Systematic replacement across all documentation and scripts

$ErrorActionPreference = "Stop"

Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host "Complexity Threshold Fix: 15 -> 8" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host ""

# Files to fix (excluding AGENTS.md - already fixed manually)
$filesToFix = @(
    # Phase scripts (currently deployed on VM)
    "_p1_*.sh",
    "_p1_5_*.sh", 
    "_p2_*.sh",
    
    # Script generators
    "scripts/wave2/generate_phase*.py",
    "scripts/phase_*_mcp*.py",
    
    # Documentation
    "docs/workflow/*.md",
    "docs/protocol/*.md",
    "docs/brain/*.md",
    "docs/standards/*.md",
    "docs/standards/jane-street/*.md",
    
    # Building blocks
    "building-blocks/**/*.md",
    
    # Plugins
    "plugins/**/*.md",
    
    # Root docs
    "*.md"
)

# Patterns to replace
$patterns = @(
    @{Find = 'CYC ≤ 15'; Replace = 'CYC ≤ 8'; Desc = 'CYC threshold symbol'}
    @{Find = 'CYC <= 15'; Replace = 'CYC <= 8'; Desc = 'CYC threshold ASCII'}
    @{Find = 'complexity <= 15'; Replace = 'complexity <= 8'; Desc = 'complexity threshold'}
    @{Find = 'threshold 15'; Replace = 'threshold 8'; Desc = 'threshold value'}
    @{Find = 'threshold: 15'; Replace = 'threshold: 8'; Desc = 'threshold colon'}
    @{Find = 'CYC >15'; Replace = 'CYC >8'; Desc = 'CYC greater than'}
    @{Find = 'CYC > 15'; Replace = 'CYC > 8'; Desc = 'CYC greater than spaced'}
    @{Find = 'complexity >15'; Replace = 'complexity >8'; Desc = 'complexity greater than'}
    @{Find = 'complexity > 15'; Replace = 'complexity > 8'; Desc = 'complexity greater spaced'}
    @{Find = 'Target complexity <= 15'; Replace = 'Target complexity <= 8'; Desc = 'Target complexity'}
    @{Find = 'target complexity <= 15'; Replace = 'target complexity <= 8'; Desc = 'target complexity lowercase'}
    @{Find = 'exceeding threshold 15'; Replace = 'exceeding threshold 8'; Desc = 'exceeding threshold'}
    @{Find = 'exceeds threshold 15'; Replace = 'exceeds threshold 8'; Desc = 'exceeds threshold'}
    @{Find = 'threshold of 15'; Replace = 'threshold of 8'; Desc = 'threshold of'}
    @{Find = 'threshold (15)'; Replace = 'threshold (8)'; Desc = 'threshold parens'}
    @{Find = 'threshold=15'; Replace = 'threshold=8'; Desc = 'threshold equals'}
)

$totalFiles = 0
$totalReplacements = 0

foreach ($pattern in $filesToFix) {
    $files = Get-ChildItem -Path . -Filter $pattern -Recurse -File -ErrorAction SilentlyContinue
    
    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }
        
        $originalContent = $content
        $fileReplacements = 0
        
        foreach ($p in $patterns) {
            if ($content -match [regex]::Escape($p.Find)) {
                $content = $content -replace [regex]::Escape($p.Find), $p.Replace
                $fileReplacements++
            }
        }
        
        if ($content -ne $originalContent) {
            Set-Content -Path $file.FullName -Value $content -NoNewline
            $totalFiles++
            $totalReplacements += $fileReplacements
            Write-Host "[OK] Fixed: $($file.FullName) ($fileReplacements patterns)" -ForegroundColor Green
        }
    }
}

Write-Host ""
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host "Summary" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host "Files modified: $totalFiles" -ForegroundColor Yellow
Write-Host "Total replacements: $totalReplacements" -ForegroundColor Yellow
Write-Host ""
Write-Host "[SUCCESS] Threshold fix complete!" -ForegroundColor Green

# Made with Bob
