# V12 GitHub PR Creation via REST API
# Reliable alternative to gh CLI when token scope issues occur

param(
    [Parameter(Mandatory=$true)]
    [string]$Title,
    
    [Parameter(Mandatory=$true)]
    [string]$Body,
    
    [Parameter(Mandatory=$true)]
    [string]$Head,
    
    [Parameter(Mandatory=$false)]
    [string]$Base = "main",
    
    [Parameter(Mandatory=$false)]
    [string]$Token = $env:GITHUB_TOKEN,
    
    [Parameter(Mandatory=$false)]
    [string]$Repo = "antigravityos187-sketch/universal-or-strategy"
)

$ErrorActionPreference = "Stop"

# Validate token
if (-not $Token) {
    Write-Host "ERROR: GITHUB_TOKEN not set" -ForegroundColor Red
    Write-Host "Set token: `$env:GITHUB_TOKEN = 'ghp_...'" -ForegroundColor Yellow
    exit 1
}

# Create PR via REST API
$headers = @{
    Authorization = "Bearer $Token"
    Accept = "application/vnd.github+json"
}

$bodyJson = @{
    title = $Title
    body = $Body
    head = $Head
    base = $Base
} | ConvertTo-Json

try {
    Write-Host "Creating PR: $Title" -ForegroundColor Cyan
    Write-Host "  Head: $Head" -ForegroundColor Gray
    Write-Host "  Base: $Base" -ForegroundColor Gray
    
    $response = Invoke-RestMethod `
        -Uri "https://api.github.com/repos/$Repo/pulls" `
        -Method Post `
        -Headers $headers `
        -Body $bodyJson
    
    Write-Host ""
    Write-Host "✓ PR Created Successfully" -ForegroundColor Green
    Write-Host "  Number: #$($response.number)" -ForegroundColor White
    Write-Host "  URL: $($response.html_url)" -ForegroundColor Cyan
    Write-Host "  State: $($response.state)" -ForegroundColor White
    Write-Host ""
    
    # Return PR number for scripting
    return $response.number
}
catch {
    Write-Host ""
    Write-Host "ERROR: Failed to create PR" -ForegroundColor Red
    Write-Host "  Message: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    exit 1
}

# Made with Bob
