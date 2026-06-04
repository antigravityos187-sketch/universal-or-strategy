# Jane Street Knowledge Base Sync Script
# V12 Universal OR Strategy - Jane Street Intel Pipeline
# Last Updated: 2026-06-03

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet(1, 2)]
    [int]$Tier,
    
    [Parameter(Mandatory=$false)]
    [switch]$ExtractOnly,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipFirestore,
    
    [Parameter(Mandatory=$false)]
    [switch]$TestMode,
    
    [Parameter(Mandatory=$false)]
    [int]$ParallelJobs = 4
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Configuration
$ReposDir = [System.IO.Path]::GetFullPath("$HOME/.jane-street")
$StatusFile = Join-Path $ReposDir ".sync-status.json"
$CompleteMarker = if ($TestMode) { Join-Path $ReposDir ".sync-complete-test" } else { Join-Path $ReposDir ".sync-complete" }
$Version = "2026-06-03"

# Test mode repos (small, fast to clone/index)
$TestRepos = @("time_now", "patience_diff")

# Tier 1: Core infrastructure (12 repos, ~6 hours)
$Tier1Repos = @(
    "base",
    "core",
    "core_kernel",
    "async",
    "async_kernel",
    "expect_test",
    "core_bench",
    "incremental",
    "memtrace",
    "hardcaml",
    "time_now",
    "ppx_inline_test"
)

# Tier 2: Extended ecosystem (10 repos, ~5 hours)
$Tier2Repos = @(
    "async_unix",
    "async_rpc",
    "bin_prot",
    "sexplib",
    "patience_diff",
    "re2",
    "ppx_assert",
    "ppx_bench",
    "core_profiler",
    "textutils"
)

function Write-Status {
    param([string]$Message, [string]$Color = "White")
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] $Message" -ForegroundColor $Color
}

function Initialize-SyncDirectory {
    Write-Status "Initializing Jane Street sync directory..." "Cyan"
    
    if (!(Test-Path $ReposDir)) {
        New-Item -ItemType Directory -Path $ReposDir -Force | Out-Null
        Write-Status "Created directory: $ReposDir" "Green"
    }
    
    # Initialize status file if not exists
    if (!(Test-Path $StatusFile)) {
        $initialStatus = @{
            version = $Version
            last_sync = $null
            repos = @{}
        }
        $initialStatus | ConvertTo-Json -Depth 10 | Set-Content $StatusFile
        Write-Status "Initialized status file" "Green"
    }
}

function Get-SyncStatus {
    if (Test-Path $StatusFile) {
        return Get-Content $StatusFile | ConvertFrom-Json
    }
    return $null
}

function Update-RepoStatus {
    param(
        [string]$RepoName,
        [string]$Status,
        [string]$Message = ""
    )
    
    $syncStatus = Get-SyncStatus
    
    # Convert to hashtable for proper manipulation
    $statusHash = @{
        version = $syncStatus.version
        last_sync = (Get-Date).ToString("o")
        repos = @{}
    }
    
    # Copy existing repos if they exist
    if ($syncStatus.repos) {
        foreach ($prop in $syncStatus.repos.PSObject.Properties) {
            $statusHash.repos[$prop.Name] = @{
                status = $prop.Value.status
                message = $prop.Value.message
                timestamp = $prop.Value.timestamp
            }
        }
    }
    
    # Add/update current repo
    $statusHash.repos[$RepoName] = @{
        status = $Status
        message = $Message
        timestamp = (Get-Date).ToString("o")
    }
    
    $statusHash | ConvertTo-Json -Depth 10 | Set-Content $StatusFile
}

function Clone-JaneStreetRepo {
    param([string]$RepoName)
    
    $repoPath = Join-Path $ReposDir $RepoName
    $repoUrl = "https://github.com/janestreet/$RepoName.git"
    
    Write-Status "Cloning $RepoName..." "Yellow"
    
    try {
        if (Test-Path $repoPath) {
            Write-Status "  Repository already exists, pulling latest..." "Gray"
            Push-Location $repoPath
            git pull origin master 2>&1 | Out-Null
            Pop-Location
        } else {
            git clone --depth 1 $repoUrl $repoPath 2>&1 | Out-Null
        }
        
        Write-Status "  Clone successful: $RepoName" "Green"
        Update-RepoStatus -RepoName $RepoName -Status "cloned" -Message "Successfully cloned"
        return $true
    }
    catch {
        Write-Status "  Clone failed: $RepoName - $($_.Exception.Message)" "Red"
        Update-RepoStatus -RepoName $RepoName -Status "clone_failed" -Message $_.Exception.Message
        return $false
    }
}

function Index-JaneStreetRepo {
    param([string]$RepoName)
    
    $repoPath = Join-Path $ReposDir $RepoName
    
    Write-Status "Indexing $RepoName with jCodemunch..." "Yellow"
    
    try {
        # Check if jcodemunch-mcp is available
        $jcmPath = Get-Command jcodemunch-mcp -ErrorAction SilentlyContinue
        if (!$jcmPath) {
            throw "jcodemunch-mcp not found in PATH. Install via: npm install -g jcodemunch-mcp"
        }
        
        # Run jcodemunch-mcp index_folder
        $indexCmd = "jcodemunch-mcp index_folder --path `"$repoPath`" --use-ai-summaries false"
        Write-Status "  Running: $indexCmd" "Gray"
        
        $result = Invoke-Expression $indexCmd 2>&1
        
        Write-Status "  Index successful: $RepoName" "Green"
        Update-RepoStatus -RepoName $RepoName -Status "indexed" -Message "Successfully indexed"
        return $true
    }
    catch {
        Write-Status "  Index failed: $RepoName - $($_.Exception.Message)" "Red"
        Update-RepoStatus -RepoName $RepoName -Status "index_failed" -Message $_.Exception.Message
        return $false
    }
}

function Process-Repo {
    param([string]$RepoName)
    
    $success = Clone-JaneStreetRepo -RepoName $RepoName
    if (!$success) {
        return $false
    }
    
    Start-Sleep -Seconds 2  # Rate limiting
    
    $success = Index-JaneStreetRepo -RepoName $RepoName
    return $success
}

function Sync-Repos {
    param([string[]]$Repos)
    
    $total = $Repos.Count
    $completed = 0
    $failed = 0
    
    Write-Status "Starting sync for $total repositories..." "Cyan"
    Write-Status "Estimated time: $([math]::Ceiling($total * 1.5)) minutes" "Cyan"
    
    # Process repos sequentially (simpler, more reliable)
    foreach ($repo in $Repos) {
        Write-Status "Processing: $repo" "Cyan"
        
        $success = Process-Repo -RepoName $repo
        if ($success) {
            $completed++
        } else {
            $failed++
        }
        
        Write-Status "Progress: $completed/$total completed, $failed failed" "Cyan"
    }
    
    return @{
        Total = $total
        Completed = $completed
        Failed = $failed
    }
}

function Invoke-DocExtraction {
    Write-Status "Extracting documentation from repos..." "Cyan"
    
    try {
        $extractScript = Join-Path $PSScriptRoot "extract_jane_street_docs.py"
        if (!(Test-Path $extractScript)) {
            throw "Extract script not found: $extractScript"
        }
        
        python $extractScript
        Write-Status "Documentation extraction complete" "Green"
        return $true
    }
    catch {
        Write-Status "Documentation extraction failed: $($_.Exception.Message)" "Red"
        return $false
    }
}

function Invoke-FirestoreUpload {
    Write-Status "Uploading to Firestore..." "Cyan"
    
    try {
        $uploadScript = Join-Path $PSScriptRoot "upload_jane_street_intel.py"
        if (!(Test-Path $uploadScript)) {
            throw "Upload script not found: $uploadScript"
        }
        
        python $uploadScript
        Write-Status "Firestore upload complete" "Green"
        return $true
    }
    catch {
        Write-Status "Firestore upload failed: $($_.Exception.Message)" "Red"
        Write-Status "You can retry later with: mise run jane-street-sync-extract" "Yellow"
        return $false
    }
}

# Main execution
try {
    Write-Status "=== Jane Street Knowledge Base Sync ===" "Cyan"
    Write-Status "Version: $Version" "Cyan"
    Write-Status "Target Directory: $ReposDir" "Cyan"
    
    Initialize-SyncDirectory
    
    # Determine which repos to sync
    $reposToSync = @()
    if ($TestMode) {
        $reposToSync = $TestRepos
        Write-Status "Mode: TEST MODE (2 repos: time_now, patience_diff)" "Yellow"
        Write-Status "Estimated time: 2-3 minutes" "Yellow"
    }
    elseif ($Tier -eq 1) {
        $reposToSync = $Tier1Repos
        Write-Status "Mode: Tier 1 only (12 repos)" "Cyan"
    }
    elseif ($Tier -eq 2) {
        $reposToSync = $Tier2Repos
        Write-Status "Mode: Tier 2 only (10 repos)" "Cyan"
    }
    else {
        $reposToSync = $Tier1Repos + $Tier2Repos
        Write-Status "Mode: Full sync (22 repos)" "Cyan"
    }
    
    # Clone and index repos (unless ExtractOnly)
    if (!$ExtractOnly) {
        $syncResult = Sync-Repos -Repos $reposToSync
        
        Write-Status "" "White"
        Write-Status "=== Sync Summary ===" "Cyan"
        Write-Status "Total: $($syncResult.Total)" "White"
        Write-Status "Completed: $($syncResult.Completed)" "Green"
        Write-Status "Failed: $($syncResult.Failed)" "Red"
        
        if ($syncResult.Failed -gt 0) {
            Write-Status "Some repos failed to sync. Check status with: mise run jane-street-status" "Yellow"
        }
    }
    
    # Extract documentation
    $extractSuccess = Invoke-DocExtraction
    if (!$extractSuccess) {
        Write-Status "CRITICAL: Documentation extraction failed. Halting." "Red"
        exit 1
    }
    
    # Upload to Firestore (unless skipped)
    if (!$SkipFirestore) {
        $uploadSuccess = Invoke-FirestoreUpload
        if (!$uploadSuccess) {
            Write-Status "WARNING: Firestore upload failed. You can retry later." "Yellow"
        }
    }
    
    # Create completion marker
    Set-Content -Path $CompleteMarker -Value (Get-Date).ToString("o")
    
    Write-Status "" "White"
    Write-Status "=== Sync Complete ===" "Green"
    Write-Status "Status file: $StatusFile" "White"
    Write-Status "Check status: mise run jane-street-status" "White"
}
catch {
    Write-Status "FATAL ERROR: $($_.Exception.Message)" "Red"
    Write-Status $_.ScriptStackTrace "Red"
    exit 1
}

# Made with Bob
