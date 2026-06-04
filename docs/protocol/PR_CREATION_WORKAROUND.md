# PR Creation Workaround

## Issue

GitHub CLI `gh pr create` uses GraphQL API, which **does not support PR creation with classic Personal Access Tokens**, even with full `repo` scope.

**Error**: `GraphQL: Resource not accessible by personal access token (createPullRequest)`

## Root Cause

- Classic PATs can only create PRs via REST API
- GitHub CLI uses GraphQL API by default
- Fine-grained PATs support GraphQL, but have other limitations

## Workaround Options

### Option 1: Manual PR Creation (CURRENT)

**Use GitHub Web UI**:
1. Push branch: `git push origin <branch>`
2. Navigate to: `https://github.com/antigravityos187-sketch/universal-or-strategy/pull/new/<branch>`
3. Fill in title and body
4. Click "Create pull request"

**Pros**: Always works, no token issues
**Cons**: Requires manual action

### Option 2: REST API via PowerShell

```powershell
$headers = @{
    Authorization = "Bearer $env:GITHUB_TOKEN"
    Accept = "application/vnd.github+json"
}
$body = @{
    title = "PR Title"
    body = "PR Body"
    head = "branch-name"
    base = "main"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://api.github.com/repos/antigravityos187-sketch/universal-or-strategy/pulls" -Method Post -Headers $headers -Body $body
```

**Pros**: Scriptable, works with classic PAT
**Cons**: More complex than `gh pr create`

### Option 3: Fine-Grained PAT (NOT RECOMMENDED)

Fine-grained PATs support GraphQL but have limitations:
- Shorter expiration (max 1 year)
- More complex permission model
- May not work with all GitHub Apps

## Recommendation

**Use Option 1 (Manual)** for now. The PR creation step is already a manual gate in the workflow, so this doesn't add significant overhead.

**Future**: Implement Option 2 (REST API) in a PowerShell script for full automation.

## Current Status

- ✅ GitHub CLI authenticated as `antigravityos187-sketch`
- ✅ PAT has full scopes (`repo`, `workflow`, etc.)
- ❌ Cannot create PRs via `gh pr create` (GraphQL limitation)
- ✅ Can push branches, view PRs, comment on PRs
- ✅ Manual PR creation works

## Updated Workflow

**Phase 6 of epic-run.md**:
1. Push branch: `git push origin <branch>`
2. **MANUAL**: Director creates PR via web UI
3. Extract PR number from URL
4. Run: `/pr-loop <PR_NUMBER>`