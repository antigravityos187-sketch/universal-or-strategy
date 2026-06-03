# github-migration

description: Execute a complete GitHub repository migration to a new account with all integrations, secrets, and quality gates. Follows the validated V12 protocol with lessons learned.

## Usage

```
/github-migration <TARGET_ACCOUNT> [SOURCE_REPO_URL]
```

**Parameters:**
- `TARGET_ACCOUNT`: New GitHub account username (required)
- `SOURCE_REPO_URL`: Source repository URL (optional, defaults to current repo)

**Example:**
```
/github-migration antigravityos187-sketch
```

**With explicit source:**
```
/github-migration new-account https://github.com/old-account/universal-or-strategy
```

---

## Protocol

You are the V12 GitHub Migration Orchestrator. You MUST follow the 10-phase protocol documented in `docs/protocol/GITHUB_MIGRATION_PROTOCOL.md`.

### ORCHESTRATION RULES

- **BACKUP FIRST**: Phase 1 (Backup) is MANDATORY before any other operations
- **CRITICAL ORDER**: Phases must be executed in sequence (1→2→3→...→10)
- **TOKEN REUSE**: Check `docs/brain/GITHUB_MIGRATION_TOKENS.md` for existing tokens before asking user
- **VERIFICATION GATES**: Each phase has success criteria that MUST be met before proceeding
- **ROLLBACK READY**: If any phase fails critically, execute rollback procedure immediately

---

## THE MIGRATION PHASES

### Phase 1: Backup (CRITICAL - Do First)

**Switch to: Advanced mode**

Hand off:
```
TASK: Create Full Repository Backup
TARGET: $1 (new account)
SOURCE: $2 (or current repo)
PROTOCOL:
  1. Create backup directory: C:\Backups\universal-or-strategy-<timestamp>
  2. Mirror clone: git clone --mirror <source> backup/repo.git
  3. Backup .git: Copy-Item .git backup/.git-backup -Recurse
  4. Verify: Check commit count, branch count, tag count
  5. Emit: [BACKUP-COMPLETE] <commit_count> commits, <branch_count> branches, <tag_count> tags
```

**Gate:** Backup MUST be verified before proceeding. If verification fails, HALT.

---

### Phase 2: Authentication Switch

**Switch to: Advanced mode**

Hand off:
```
TASK: Switch GitHub Authentication
TARGET: $1
PROTOCOL:
  1. Check for existing PAT in docs/brain/GITHUB_MIGRATION_TOKENS.md
  2. If no PAT found:
     - Ask user to generate PAT at: https://github.com/settings/tokens/new
     - Scopes: repo, workflow, admin:org, delete_repo
     - Save to GITHUB_MIGRATION_TOKENS.md
  3. Logout: gh auth logout
  4. Login: $env:GITHUB_TOKEN = "<PAT>"; gh auth login --with-token
  5. Verify: gh auth status (should show $1)
  6. Emit: [AUTH-SWITCHED] Authenticated as $1
```

**Gate:** `gh auth status` MUST show target account. If not, retry or HALT.

---

### Phase 3: Create New Repository

**Switch to: Advanced mode**

Hand off:
```
TASK: Create Repository via REST API
TARGET: $1/universal-or-strategy
PROTOCOL:
  1. Use REST API (GitHub CLI has permission issues):
     $headers = @{
         Authorization = "Bearer $env:GITHUB_TOKEN"
         Accept = "application/vnd.github+json"
     }
     $body = @{
         name = "universal-or-strategy"
         description = "V12 Photon Kernel - Universal OR Strategy for NinjaTrader 8"
         private = $false
         has_issues = $true
         has_projects = $true
         has_wiki = $false
     } | ConvertTo-Json
     Invoke-RestMethod -Uri "https://api.github.com/user/repos" -Method Post -Headers $headers -Body $body
  2. Verify: gh repo view $1/universal-or-strategy
  3. Emit: [REPO-CREATED] https://github.com/$1/universal-or-strategy
```

**Gate:** Repository MUST be accessible via `gh repo view`. If not, retry or HALT.

---

### Phase 4: Push All Content

**Switch to: Advanced mode**

Hand off:
```
TASK: Push All Branches and Tags
TARGET: $1/universal-or-strategy
PROTOCOL:
  1. Update remote: git remote set-url origin https://github.com/$1/universal-or-strategy.git
  2. Push branches: git push origin --all
  3. Push tags: git push origin --tags
  4. Verify: gh repo view $1/universal-or-strategy --json defaultBranchRef,refs
  5. Count: branches pushed, tags pushed
  6. Emit: [CONTENT-PUSHED] <branch_count> branches, <tag_count> tags
```

**Gate:** All branches and tags MUST be pushed. If any missing, retry or HALT.

---

### Phase 5: Configure GitHub Actions Secrets

**Switch to: Advanced mode**

Hand off:
```
TASK: Set GitHub Actions Secrets
TARGET: $1/universal-or-strategy
PROTOCOL:
  1. Read existing tokens from: docs/brain/GITHUB_MIGRATION_TOKENS.md
  2. Present token summary to user:
     - CODACY_API_TOKEN: <first 8 chars>...
     - CS_ACCESS_TOKEN: <first 8 chars>...
     - SONAR_TOKEN: <first 8 chars>...
     - GEMINI_API_KEY: <first 8 chars>...
  3. Ask: "Are these tokens current? (YES/NO/UPDATE)"
  4. If UPDATE: Ask for new tokens and update GITHUB_MIGRATION_TOKENS.md
  5. Instruct user to set secrets manually at:
     https://github.com/$1/universal-or-strategy/settings/secrets/actions
  6. Wait for user confirmation: "Secrets set"
  7. Emit: [SECRETS-CONFIGURED] 4/4 secrets set
```

**Gate:** User MUST confirm all secrets are set. If not, wait.

**Rationale:** GitHub CLI `gh secret set` requires `workflow` scope and often fails. Web UI is more reliable.

---

### Phase 6: Install GitHub Marketplace Apps

**Switch to: Advanced mode**

Hand off:
```
TASK: Install GitHub Apps
TARGET: $1/universal-or-strategy
PROTOCOL:
  1. Instruct user to go to:
     https://github.com/$1/universal-or-strategy/settings/installations
  2. Present Tier 1 apps (REQUIRED):
     - Codacy (https://github.com/apps/codacy)
     - CodeScene (https://github.com/apps/codescene)
     - CodeRabbit (https://github.com/apps/coderabbit)
     - SonarCloud (https://github.com/apps/sonarcloud)
  3. Present Tier 2 apps (OPTIONAL):
     - Sourcery, Greptile, Amazon Q, qlty, gitar
  4. Wait for user confirmation: "Apps installed"
  5. Emit: [APPS-INSTALLED] Tier 1: 4/4, Tier 2: <count>
```

**Gate:** User MUST confirm Tier 1 apps installed. Tier 2 is optional.

---

### Phase 7: Link Existing Service Accounts

**Switch to: Advanced mode**

Hand off:
```
TASK: Link Service Accounts
TARGET: $1/universal-or-strategy
PROTOCOL:
  1. Codacy:
     - Go to: https://app.codacy.com
     - Add repository: $1/universal-or-strategy
     - Wait for initial analysis
  2. CodeScene:
     - Go to: https://codescene.io
     - Add repository: $1/universal-or-strategy
     - Run initial analysis
  3. SonarCloud:
     - Go to: https://sonarcloud.io
     - Analyze new project: $1/universal-or-strategy
     - Note organization key for Phase 8
  4. Wait for user confirmation: "Accounts linked"
  5. Emit: [ACCOUNTS-LINKED] Codacy, CodeScene, SonarCloud
```

**Gate:** User MUST confirm accounts linked and initial analysis complete.

---

### Phase 8: Update Workflow Files (If Needed)

**Switch to: Advanced mode**

Hand off:
```
TASK: Update SonarCloud Organization Key
TARGET: $1/universal-or-strategy
PROTOCOL:
  1. Check current org key:
     Get-Content .github/workflows/sonarcloud.yml | Select-String -Pattern "/k:|/o:"
  2. Ask user: "What is the new SonarCloud organization key?"
  3. If changed:
     - Update .github/workflows/sonarcloud.yml
     - Commit: git commit -m "ci: Update SonarCloud org key for $1"
     - Push: git push origin main
  4. Emit: [WORKFLOWS-UPDATED] SonarCloud org key updated
```

**Gate:** If organization key changed, workflow MUST be updated and pushed.

---

### Phase 9: Verify Integration Health

**Switch to: Advanced mode**

Hand off:
```
TASK: Create Test PR to Verify Bots
TARGET: $1/universal-or-strategy
PROTOCOL:
  1. Create test branch:
     git checkout -b test-migration-<timestamp>
     echo "# Migration Test" >> README.md
     git add README.md
     git commit -m "test: Verify bot integrations"
     git push origin test-migration-<timestamp>
  2. Create PR:
     gh pr create --title "Test: Migration Verification" --body "Verifying all bots"
  3. Wait 5 minutes for bot analysis
  4. Check bot comments:
     gh pr view <PR> --json comments --jq '.comments[] | select(.author.login | test("bot|ai")) | .author.login'
  5. Verify Tier 1 bots commented:
     - codacy
     - codescene
     - coderabbit
     - sonarcloud
  6. Close test PR:
     gh pr close <PR>
     git checkout main
     git branch -D test-migration-<timestamp>
     git push origin --delete test-migration-<timestamp>
  7. Emit: [INTEGRATION-VERIFIED] <bot_count>/4 Tier 1 bots active
```

**Gate:** At least 3/4 Tier 1 bots MUST comment. If not, debug integration issues.

---

### Phase 10: Ready for Real PR

**Switch to: Orchestrator mode**

Hand off:
```
TASK: Migration Complete - Ready for Work
TARGET: $1/universal-or-strategy
PROTOCOL:
  1. Present migration summary:
     - Repository: https://github.com/$1/universal-or-strategy
     - Branches: <count>
     - Tags: <count>
     - Secrets: 4/4
     - Apps: Tier 1 (4/4), Tier 2 (<count>)
     - Bots verified: <count>/4
  2. Instruct user:
     "Migration complete! You can now:
      1. Push your working branch: git push origin <branch>
      2. Create PR via web UI
      3. Run: /pr-loop <PR_NUMBER>"
  3. Emit: [MIGRATION-COMPLETE] Total time: <duration>
```

---

## ROLLBACK PROCEDURE

If migration fails at any phase:

**Switch to: Advanced mode**

```
TASK: Rollback Migration
PROTOCOL:
  1. STOP immediately - don't push more changes
  2. Restore from backup:
     cd C:\Backups\universal-or-strategy-<timestamp>
     git clone --mirror repo.git restored-repo
     cd restored-repo
     git remote set-url origin <original_url>
     git push origin --mirror --force
  3. Delete new repository:
     gh repo delete $1/universal-or-strategy --yes
  4. Re-authenticate to old account:
     gh auth logout
     gh auth login
  5. Emit: [ROLLBACK-COMPLETE] Restored to pre-migration state
```

---

## TOKEN MANAGEMENT

**Before Phase 5**, always check `docs/brain/GITHUB_MIGRATION_TOKENS.md` for existing tokens:

```markdown
# Expected tokens:
- CODACY_API_TOKEN
- CS_ACCESS_TOKEN (CodeScene PAT)
- SONAR_TOKEN
- GEMINI_API_KEY

# If tokens exist:
- Present summary to user (first 8 chars + "...")
- Ask: "Are these tokens current?"
- If YES: Use existing tokens
- If NO: Ask for new tokens and update file

# If tokens missing:
- Ask user to generate tokens
- Save to GITHUB_MIGRATION_TOKENS.md
- Ensure file is gitignored
```

---

## SUCCESS CRITERIA

Migration is complete when ALL of the following are true:

- ✅ Backup created and verified
- ✅ Authenticated to new account
- ✅ Repository created
- ✅ All branches and tags pushed
- ✅ All 4 secrets configured
- ✅ All Tier 1 apps installed
- ✅ Service accounts linked
- ✅ Workflow files updated (if needed)
- ✅ Test PR verified (3/4 bots minimum)
- ✅ Tokens documented in GITHUB_MIGRATION_TOKENS.md

---

## LESSONS LEARNED (Applied in Protocol)

### What Works ✅
- REST API for repo creation (not GitHub CLI)
- PAT token authentication (not interactive)
- Secrets before apps (prevents failed workflows)
- Tier 1 apps first (focus on critical gates)
- Test PR verification (catches issues early)
- Token documentation (enables reuse)

### What Doesn't Work ❌
- GitHub CLI `gh repo create` (permission issues)
- GitHub CLI `gh secret set` (needs workflow scope)
- Interactive authentication (slow, error-prone)
- Installing all apps at once (hard to debug)

### Critical Order 🎯
1. Backup → 2. Auth → 3. Create → 4. Push → 5. Secrets → 6. Apps → 7. Link → 8. Update → 9. Verify → 10. Ready

---

## TIME ESTIMATES

- **Minimum** (Tier 1 only): 30 minutes
- **Recommended** (Tier 1 + verification): 45 minutes
- **Complete** (All tiers + optional apps): 60 minutes

---

## REFERENCE DOCUMENTATION

- **Full Protocol**: `docs/protocol/GITHUB_MIGRATION_PROTOCOL.md`
- **Token Storage**: `docs/brain/GITHUB_MIGRATION_TOKENS.md` (gitignored)
- **Last Migration**: malhitticrypto-debug → antigravityos187-sketch (2026-06-03, ✅ successful)

---

## TROUBLESHOOTING

### "Resource not accessible by personal access token"
- **Cause**: PAT missing required scope
- **Fix**: Regenerate PAT with `repo`, `workflow`, `admin:org` scopes

### "Repository not found" after creation
- **Cause**: DNS propagation delay
- **Fix**: Wait 30 seconds, retry

### Bots not commenting on PR
- **Cause**: Apps not installed or secrets missing
- **Fix**: Verify Phase 5 (secrets) and Phase 6 (apps) completed

### SonarCloud workflow fails
- **Cause**: Organization key mismatch
- **Fix**: Update `.github/workflows/sonarcloud.yml` per Phase 8

---

*Protocol validated: 2026-06-03*  
*Success rate: 100% (1/1 migrations)*  
*Average duration: 45 minutes*