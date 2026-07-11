# check_pr_scope.ps1
# Determines if a PR contains only src/ files or mixed directories
# Used by PR Loop V2 to prevent non-src rebase contamination

param(
    [Parameter(Mandatory=$true)]
    [int]$PrNumber
)

$ErrorActionPreference = "Stop"

try {
    # Get all files changed in the PR
    $filesJson = gh pr view $PrNumber --json files --jq '.files[].path' 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to fetch PR files: $filesJson"
        exit 1
    }
    
    # Parse file paths
    $files = $filesJson -split "`n" | Where-Object { $_ -ne "" }
    
    if ($files.Count -eq 0) {
        Write-Output "EMPTY"
        exit 0
    }
    
    # Check if ALL files are in src/
    $nonSrcFiles = $files | Where-Object { $_ -notlike "src/*" }
    
    if ($nonSrcFiles.Count -eq 0) {
        Write-Output "SRC-ONLY"
    } else {
        Write-Output "MIXED"
    }
    
    exit 0
}
catch {
    Write-Error "Error checking PR scope: $_"
    exit 1
}

# Made with Bob
