# GitHub Account Migration Protocol

**Version**: 1.0  
**Last Updated**: 2026-05-25  
**Status**: MANDATORY for all GitHub account migrations

## Critical Lesson Learned

**NEVER use GitHub forks for account migrations.** Forks have security restrictions that prevent automated PR creation, even for the fork owner.

## The Problem

When migrating from Account A to Account B:
- ❌ **Fork approach**: GitHub's fork security model blocks automated PR creation via API/CLI
- ❌ **Error**: "must be a collaborator" even though you own the fork
- ❌ **Impact**: Breaks PR Loop V2 automation

## The Solution

### Option 1: Personal Access Token (FASTEST ✅)

**Use this for quick migrations when you want to keep the fork.**

1. **Create PAT** on new account:
   - Go to: https://github.com/settings/tokens/new
   - **Required Scopes**:
     - ✅ `repo` (full control of private repositories)
     - ✅ `workflow` (update GitHub Action workflows) - **CRITICAL for PR creation**
     - ✅ `admin:org` (read:org minimum)
   - Generate token (starts with `ghp_...`)
   - **SAVE IMMEDIATELY** - token shown only once

2. **⚠️ CRITICAL: Remove Old Account Token**:
   ```powershell
   # Check current authentication
   gh auth status
   
   # If showing old account, clear environment variable
   [System.Environment]::SetEnvironmentVariable('GITHUB_TOKEN', $null, 'User')
   [System.Environment]::SetEnvironmentVariable('GITHUB_TOKEN', $null, 'Process')
   
   # Verify old token removed
   gh auth status  # Should show error or different account
   ```

3. **Configure GitHub CLI with NEW token**:
   ```powershell
   # Set persistently (survives terminal restarts)
   [System.Environment]::SetEnvironmentVariable('GITHUB_TOKEN', 'ghp_NEW_TOKEN_HERE', 'User')
   
   # Set for current session
   $env:GITHUB_TOKEN = "ghp_NEW_TOKEN_HERE"
   
   # Verify authentication
   gh auth status --show-token
   # Should show: ✓ Logged in to github.com account NEW_ACCOUNT
   ```

4. **Test PR creation**:
   ```powershell
   gh pr create --repo <new-account>/<repo> --head <branch> --base main --title "..." --body "..."
   ```

5. **Automation works immediately** ✅

### Option 2: Standalone Repository (CLEANEST)

**Use this for permanent migrations or when you want full control.**

1. **Delete the fork** (local code is safe):
   ```powershell
   gh repo delete <new-account>/<repo> --yes
   ```

2. **Create fresh repo** (not a fork):
   ```powershell
   gh repo create <new-account>/<repo> --private --description "..."
   ```

3. **Push all branches**:
   ```powershell
   git remote add new https://github.com/<new-account>/<repo>.git
   git push new --all
   git push new --tags
   ```

4. **GitHub Apps auto-work** (they're account-level, not repo-level)

5. **Add secrets** to new repo:
   - ANTHROPIC_API_KEY
   - OPENAI_API_KEY
   - GREPTILE_API_KEY
   - GOOGLE_API_KEY

## Migration Checklist

### Pre-Migration
- [ ] Audit GitHub Apps installed on old account
- [ ] Document all repository secrets
- [ ] List active workflows in `.github/workflows/`
- [ ] Note any external service integrations (Codacy, Snyk, etc.)
- [ ] **Save old account token** to `docs/brain/GITHUB_MIGRATION_TOKENS.md` (keep for old account access)

### During Migration
- [ ] Choose Option 1 (PAT) or Option 2 (Standalone)
- [ ] If Option 1: Create PAT with **ALL required scopes** (`repo`, `workflow`, `admin:org`)
- [ ] **⚠️ CRITICAL: Remove old account token from environment** (see Option 1, Step 2)
- [ ] Configure GitHub CLI with NEW token (persistent + session)
- [ ] Verify authentication shows NEW account: `gh auth status`
- [ ] If Option 2: Delete fork, create fresh repo
- [ ] Push all branches to new account

### Post-Migration
- [ ] Add repository secrets to new repo
- [ ] Verify GitHub Apps are active (check PR checks)
- [ ] Test automated PR creation: `gh pr create ...`
- [ ] Confirm all workflows trigger correctly
- [ ] Update local git remotes if needed
- [ ] Document new token in `docs/brain/GITHUB_MIGRATION_TOKENS.md`

## Automation Integration

### For Bob CLI / Orchestrator

When migrating accounts, always:
1. Check if target repo is a fork: `gh repo view <owner>/<repo> --json isFork`
2. If fork detected, prompt user: "Fork detected. Use PAT (faster) or convert to standalone (cleaner)?"
3. If PAT chosen, request token and set `$env:GH_TOKEN`
4. If standalone chosen, execute delete → create → push workflow

### For PR Loop V2

Before creating PR:
```powershell
# Check if we have a PAT configured
if (-not $env:GH_TOKEN) {
    # Check if repo is a fork
    $isFork = (gh repo view <owner>/<repo> --json isFork | ConvertFrom-Json).isFork
    if ($isFork) {
        Write-Error "Fork detected. Set GH_TOKEN or convert to standalone repo."
        exit 1
    }
}
```

## Security Notes

### Personal Access Tokens
- ✅ Store securely (never commit to git)
- ✅ Use minimal scopes (`repo` only for PR creation)
- ✅ Rotate regularly (every 90 days)
- ✅ Revoke immediately if compromised: https://github.com/settings/tokens

### Repository Secrets
- ✅ Never expose in logs or PR descriptions
- ✅ Use GitHub Secrets for CI/CD workflows
- ✅ Rotate API keys after migration
- ✅ Audit secret access regularly

## Troubleshooting

### "Resource not accessible by personal access token" error
**Cause**: Token missing required scopes (usually `workflow`) OR authenticated with wrong account
**Symptoms**:
- `gh auth status` shows old account name
- PR creation fails with GraphQL error
- Token has `repo` scope but still fails

**Fix**:
```powershell
# 1. Check current authentication
gh auth status --show-token

# 2. If showing wrong account, remove old token
[System.Environment]::SetEnvironmentVariable('GITHUB_TOKEN', $null, 'User')
$env:GITHUB_TOKEN = $null

# 3. Set new token with ALL required scopes
[System.Environment]::SetEnvironmentVariable('GITHUB_TOKEN', 'ghp_NEW_TOKEN', 'User')
$env:GITHUB_TOKEN = "ghp_NEW_TOKEN"

# 4. Verify correct account
gh auth status
# Should show: ✓ Logged in to github.com account NEW_ACCOUNT
```

**Prevention**: Always remove old account tokens before setting new ones during migration.

### "must be a collaborator" error
**Cause**: Trying to create PR on a fork without PAT
**Fix**: Use Option 1 (PAT) or Option 2 (Standalone)

### "permission denied" on push
**Cause**: Git credential cache has old account credentials
**Fix**:
```powershell
"protocol=https`nhost=github.com`n" | git credential-manager erase
git config --global credential.helper "!gh auth git-credential"
```

### GitHub Apps not triggering
**Cause**: Apps are installed on old account, not new account
**Fix**: Install apps on new account (they're account-level, auto-work on all repos)

### Token authentication works but reverts to old account
**Cause**: `GITHUB_TOKEN` environment variable not set persistently
**Fix**:
```powershell
# Set at User level (persists across terminal sessions)
[System.Environment]::SetEnvironmentVariable('GITHUB_TOKEN', 'ghp_NEW_TOKEN', 'User')

# Restart terminal or reload environment
# Verify persistence
gh auth status
```

## References

- GitHub Fork Security: https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks
- Personal Access Tokens: https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token
- GitHub CLI Authentication: https://cli.github.com/manual/gh_auth_login

## Dynamic Username Detection Pattern

**Problem**: Hardcoded GitHub usernames in scripts and config files break when migrating accounts.

**Solution**: Always extract username dynamically from `git remote get-url origin`.

### Implementation Pattern

```powershell
# Extract repository URL from git remote
$GitRemote = git remote get-url origin
$RepoUrl = $GitRemote -replace '\.git$', '' -replace 'git@github\.com:', 'https://github.com/'

# Now use $RepoUrl for constructing URLs
$PrUrl = "$RepoUrl/pull/$PrNumber"
```

### Helper Script

Use [`scripts/get_github_username.ps1`](../../scripts/get_github_username.ps1) for reusable username extraction:

```powershell
# Get username dynamically
$username = & .\scripts\get_github_username.ps1

# Use in URLs
$codacyUrl = "https://app.codacy.com/gh/$username/universal-or-strategy/dashboard"
```

### Files Requiring Manual Updates

These files cannot use dynamic detection and must be updated manually during migrations:

1. **`.github/dependabot.yml`** (Lines 13, 26)
   - Field: `reviewers`
   - Pattern: `- "username"`
   - Note: Add comment `# TODO: Make this dynamic via git remote parsing`

2. **`AGENTS.md`** (Lines 252, 293)
   - Codacy dashboard URLs
   - Pattern: `https://app.codacy.com/gh/USERNAME/universal-or-strategy/...`
   - Note: Add comment `# Note: Update this URL when GitHub account changes`

### Files Using Dynamic Detection

These files automatically adapt to account changes:

- ✅ [`scripts/extract_pr_forensics.ps1`](../../scripts/extract_pr_forensics.ps1) - PR URL construction
- ✅ [`scripts/get_github_username.ps1`](../../scripts/get_github_username.ps1) - Helper function

### Migration Checklist Addition

When migrating accounts, add this step:

- [ ] Update hardcoded usernames in `.github/dependabot.yml` and `AGENTS.md`
- [ ] Verify dynamic scripts still work: `.\scripts\get_github_username.ps1`

## Version History

- **1.2** (2026-06-04): Added critical token conflict prevention
  - **CRITICAL**: Added Step 2 to Option 1 - Remove old account token before setting new one
  - Updated PAT scopes to include `workflow` (required for PR creation)
  - Added troubleshooting section for "Resource not accessible by personal access token" error
  - Updated migration checklist with token removal step
  - Added persistent token configuration instructions
  - Documented token reversion issue and fix
- **1.1** (2026-05-25): Added dynamic username detection pattern
  - Created `scripts/get_github_username.ps1` helper
  - Updated `scripts/extract_pr_forensics.ps1` to use dynamic URLs
  - Documented files requiring manual updates vs. dynamic detection
- **1.0** (2026-05-25): Initial protocol based on PR #11 migration experience
  - Documented fork limitation
  - Added PAT and standalone options
  - Created automation integration guidelines