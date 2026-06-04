<#
.SYNOPSIS
    Automated benchmark runner with baseline tracking and regression detection.

.DESCRIPTION
    Runs all benchmarks in benchmarks/ directory using BenchmarkDotNet.
    Tracks baseline in docs/benchmarks/baseline.json.
    Detects regressions >5% and zero-allocation violations.
    Integrates with V12 pr-loop workflow (Step 2.5).

.PARAMETER UpdateBaseline
    Update the baseline with current run results.

.PARAMETER Fast
    Skip slow benchmarks (run only fast benchmarks).

.PARAMETER Verbose
    Enable verbose output.

.EXAMPLE
    .\scripts\run_benchmarks.ps1
    Run all benchmarks and check for regressions.

.EXAMPLE
    .\scripts\run_benchmarks.ps1 -UpdateBaseline
    Run benchmarks and update baseline.json.

.EXAMPLE
    .\scripts\run_benchmarks.ps1 -Fast
    Run only fast benchmarks (skip slow ones).

.NOTES
    Author: V12 Performance Integration (Phase 3)
    Date: 2026-06-03
    Requires: BenchmarkDotNet, .NET 6.0+
#>

param(
    [switch]$UpdateBaseline,
    [switch]$Fast,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Paths
$RepoRoot = Split-Path -Parent $PSScriptRoot
$BenchmarkDir = Join-Path $RepoRoot "benchmarks"
$BaselineFile = Join-Path $RepoRoot "docs\benchmarks\baseline.json"
$LatestRunFile = Join-Path $RepoRoot "docs\benchmarks\latest_run.json"
$BenchmarkResultsDir = Join-Path $BenchmarkDir "BenchmarkDotNet.Artifacts\results"

# Ensure output directory exists
$OutputDir = Join-Path $RepoRoot "docs\benchmarks"
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

Write-Host "[BENCHMARK] V12 Performance Benchmark Runner" -ForegroundColor Cyan
Write-Host "[BENCHMARK] Repo: $RepoRoot" -ForegroundColor Gray
Write-Host "[BENCHMARK] Mode: $(if ($Fast) { 'Fast' } else { 'Full' })" -ForegroundColor Gray
Write-Host ""

# Step 1: Build benchmarks project
Write-Host "[BENCHMARK] Step 1: Building benchmarks project..." -ForegroundColor Yellow
Push-Location $BenchmarkDir
try {
    $projectFile = "V12_Performance.Benchmarks.csproj"
    $buildArgs = @("build", $projectFile, "-c", "Release", "--nologo")
    if (-not $Verbose) {
        $buildArgs += "--verbosity", "quiet"
    }
    
    & dotnet @buildArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Benchmark build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[BENCHMARK] Build successful" -ForegroundColor Green
} finally {
    Pop-Location
}

# Step 2: Run benchmarks
Write-Host "[BENCHMARK] Step 2: Running benchmarks..." -ForegroundColor Yellow
Push-Location $BenchmarkDir
try {
    $projectFile = "V12_Performance.Benchmarks.csproj"
    $runArgs = @("run", "--project", $projectFile, "-c", "Release", "--exporters", "json")
    
    if ($Fast) {
        # Fast mode: use shorter job
        $runArgs += "--job", "short"
        Write-Host "[BENCHMARK] Fast mode: using short job" -ForegroundColor Gray
    }
    
    if (-not $Verbose) {
        $runArgs += "--verbosity", "quiet"
    }
    
    & dotnet @runArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Benchmark run failed with exit code $LASTEXITCODE"
    }
    Write-Host "[BENCHMARK] Benchmarks completed" -ForegroundColor Green
} finally {
    Pop-Location
}

# Step 3: Parse BenchmarkDotNet JSON output
Write-Host "[BENCHMARK] Step 3: Parsing results..." -ForegroundColor Yellow

# Find the most recent JSON result file
$jsonFiles = Get-ChildItem -Path $BenchmarkResultsDir -Filter "*-report-full.json" -ErrorAction SilentlyContinue
if (-not $jsonFiles) {
    throw "No benchmark results found in $BenchmarkResultsDir"
}

$latestJson = $jsonFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Write-Host "[BENCHMARK] Parsing: $($latestJson.Name)" -ForegroundColor Gray

$benchmarkData = Get-Content $latestJson.FullName -Raw | ConvertFrom-Json

# Step 4: Extract metrics
Write-Host "[BENCHMARK] Step 4: Extracting metrics..." -ForegroundColor Yellow

$results = @{
    version = if ($env:BUILD_TAG) { $env:BUILD_TAG } else { "dev" }
    timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    benchmarks = @{}
}

foreach ($benchmark in $benchmarkData.Benchmarks) {
    $fullName = "$($benchmark.Type).$($benchmark.Method)"
    
    # Extract mean time in nanoseconds
    $meanNs = $benchmark.Statistics.Mean
    $stddevNs = $benchmark.Statistics.StandardDeviation
    
    # Extract memory allocations
    $allocatedBytes = 0
    if ($benchmark.Memory) {
        $allocatedBytes = $benchmark.Memory.BytesAllocatedPerOperation
    }
    
    $results.benchmarks[$fullName] = @{
        mean_ns = [math]::Round($meanNs, 3)
        stddev_ns = [math]::Round($stddevNs, 3)
        allocations_bytes = $allocatedBytes
    }
    
    if ($Verbose) {
        Write-Host "  $fullName : $([math]::Round($meanNs, 2)) ns, $allocatedBytes B" -ForegroundColor Gray
    }
}

Write-Host "[BENCHMARK] Extracted $($results.benchmarks.Count) benchmark results" -ForegroundColor Green

# Step 5: Save latest run
Write-Host "[BENCHMARK] Step 5: Saving latest run..." -ForegroundColor Yellow
$results | ConvertTo-Json -Depth 10 | Set-Content $LatestRunFile -Encoding UTF8
Write-Host "[BENCHMARK] Saved: $LatestRunFile" -ForegroundColor Green

# Step 6: Compare against baseline (if exists)
if (Test-Path $BaselineFile) {
    Write-Host "[BENCHMARK] Step 6: Comparing against baseline..." -ForegroundColor Yellow
    
    $baseline = Get-Content $BaselineFile -Raw | ConvertFrom-Json
    $regressions = @()
    $allocationViolations = @()
    
    foreach ($benchName in $results.benchmarks.Keys) {
        $current = $results.benchmarks[$benchName]
        $baselineEntry = $baseline.benchmarks.$benchName
        
        if (-not $baselineEntry) {
            Write-Host "  [NEW] $benchName (no baseline)" -ForegroundColor Cyan
            continue
        }
        
        # Check for performance regression (>5%)
        $baselineMean = $baselineEntry.mean_ns
        $currentMean = $current.mean_ns
        $percentChange = (($currentMean - $baselineMean) / $baselineMean) * 100
        
        if ($percentChange -gt 5.0) {
            $regressions += @{
                name = $benchName
                baseline_ns = $baselineMean
                current_ns = $currentMean
                percent_change = [math]::Round($percentChange, 2)
            }
        }
        
        # Check for allocation violations (zero-allocation mandate)
        $baselineAlloc = $baselineEntry.allocations_bytes
        $currentAlloc = $current.allocations_bytes
        
        if ($currentAlloc -gt $baselineAlloc) {
            $allocationViolations += @{
                name = $benchName
                baseline_bytes = $baselineAlloc
                current_bytes = $currentAlloc
                delta_bytes = $currentAlloc - $baselineAlloc
            }
        }
        
        # Print comparison
        $status = if ($percentChange -gt 5.0) { "REGRESSION" } elseif ($percentChange -lt -5.0) { "IMPROVEMENT" } else { "OK" }
        $color = if ($percentChange -gt 5.0) { "Red" } elseif ($percentChange -lt -5.0) { "Green" } else { "Gray" }
        
        if ($Verbose -or $status -ne "OK") {
            Write-Host "  [$status] $benchName : $([math]::Round($percentChange, 2))% ($baselineMean ns -> $currentMean ns)" -ForegroundColor $color
        }
    }
    
    # Report regressions
    if ($regressions.Count -gt 0) {
        Write-Host ""
        Write-Host "[BENCHMARK-REGRESSION] Performance regressions detected:" -ForegroundColor Red
        foreach ($reg in $regressions) {
            Write-Host "  - $($reg.name): +$($reg.percent_change)% ($($reg.baseline_ns) ns -> $($reg.current_ns) ns)" -ForegroundColor Red
        }
        Write-Host ""
        Write-Host "[BENCHMARK] FAIL: Regressions exceed 5% threshold" -ForegroundColor Red
        exit 1
    }
    
    # Report allocation violations
    if ($allocationViolations.Count -gt 0) {
        Write-Host ""
        Write-Host "[BENCHMARK-REGRESSION] Zero-allocation violations detected:" -ForegroundColor Red
        foreach ($viol in $allocationViolations) {
            Write-Host "  - $($viol.name): +$($viol.delta_bytes) B ($($viol.baseline_bytes) B -> $($viol.current_bytes) B)" -ForegroundColor Red
        }
        Write-Host ""
        Write-Host "[BENCHMARK] FAIL: Zero-allocation mandate violated" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "[BENCHMARK-PASS] All benchmarks within 5% of baseline" -ForegroundColor Green
    Write-Host "[BENCHMARK-PASS] Zero-allocation mandate maintained" -ForegroundColor Green
} else {
    Write-Host "[BENCHMARK] Step 6: No baseline found (first run)" -ForegroundColor Yellow
}

# Step 7: Update baseline (if requested)
if ($UpdateBaseline) {
    Write-Host "[BENCHMARK] Step 7: Updating baseline..." -ForegroundColor Yellow
    $results | ConvertTo-Json -Depth 10 | Set-Content $BaselineFile -Encoding UTF8
    Write-Host "[BENCHMARK] Baseline updated: $BaselineFile" -ForegroundColor Green
} else {
    Write-Host "[BENCHMARK] Step 7: Baseline not updated (use -UpdateBaseline to update)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "[BENCHMARK] Complete" -ForegroundColor Cyan
exit 0

# Made with Bob
