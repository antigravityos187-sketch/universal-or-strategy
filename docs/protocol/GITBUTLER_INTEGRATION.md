# GitButler Integration Protocol (V12.25)

**Status**: PROPOSED (Pending Director Approval)
**Effective Date**: TBD
**Scope**: ALL agents, ALL custom modes, 4 main commands

## 1. Overview

GitButler is a Git client that provides visual branch management and "virtual branches" for easier file staging and commit organization. This protocol mandates GitButler integration for all V12 autonomous workflows to reduce friction when moving files between branches.

## 2. Why GitButler?

**Problem**: Current workflow requires manual `git add src/` and careful staging to avoid PR pollution.

**GitButler Benefits**:
- **Visual File Staging**: Drag-and-drop files between virtual branches
- **Antifriction**: Easy to move files around without command-line errors
- **Branch Isolation**: Virtual branches prevent accidental cross-contamination
- **Undo-Friendly**: Easy to unstage files before commit

## 3. Mandatory Integration Points

### 3.1. Four Main Commands

**MANDATORY** for:
1. `/pr-loop` - Step 3 (Global Push & Monitor)
2. `/epic-run` - Ticket Loop Step D (Auto-commit)
3. `/epic-tdd` - Step 4 (Commit & Push)
4. `/autonomous-refactor` - All commit operations

**Integration Requirement**:
- Before `git add src/`, agents MUST verify GitButler is active
- If GitButler not available, HALT and instruct user to install
- Use GitButler CLI or API for staging operations

### 3.2. All Custom Modes

**MANDATORY** for:
- `v12-engineer` - All src/ modifications
- `v12-epic-planner` - Documentation commits
- `advanced` - Infrastructure commits
- `orchestrator` - Delegation to other modes (verify GitButler available)

### 3.3. All Agents

**MANDATORY** for:
- **Bob CLI** (`v12-engineer`) - Primary src/ engineer
- **Codex CLI** (`codex-rescue`) - Logic hardening
- **Gemini CLI** (`yolo`) - Utility tasks
- **Jules AI** - GitHub workflows
- **Advanced Mode** - All code modifications

## 4. GitButler Workflow Integration

### 4.1. Pre-Commit Hook Enhancement

**Current**: `.git/hooks/pre-commit` blocks non-.cs files on epic branches

**Enhanced with GitButler**:
```bash
#!/bin/bash
# Check if GitButler is active
if ! command -v gitbutler &> /dev/null; then
    echo "ERROR: GitButler not installed"
    echo "Install: https://gitbutler.com/downloads"
    exit 1
fi

# Verify virtual branch is active
if ! gitbutler branch list | grep -q "active"; then
    echo "ERROR: No active GitButler virtual branch"
    echo "Create a virtual branch before committing"
    exit 1
fi

# Existing pre-commit logic...
```

### 4.2. Command Integration Examples

#### `/pr-loop` Step 3 Enhancement

**Before**:
```powershell
git add src/ && git commit -m "fix: PHS Perfection Loop - PR #$1" && git push
```

**After (with GitButler)**:
```powershell
# Verify GitButler active
gitbutler branch list | grep "active" || { echo "ERROR: GitButler not active"; exit 1; }

# Stage files via GitButler (visual or CLI)
gitbutler stage src/

# Commit via GitButler
gitbutler commit -m "fix: PHS Perfection Loop - PR #$1"

# Push via GitButler
gitbutler push
```

#### `/epic-run` Ticket Loop Step D Enhancement

**Before**:
```powershell
git add src/
git commit -m "[$1] ticket-XX: [short description] -- CYC [before]->[after] [BUILD_TAG]"
```

**After (with GitButler)**:
```powershell
# Stage via GitButler
gitbutler stage src/

# Commit via GitButler with metadata
gitbutler commit -m "[$1] ticket-XX: [short description] -- CYC [before]->[after] [BUILD_TAG]"
```

### 4.3. Bob CLI Integration

**File**: `.bob/settings.json`

**Add**:
```json
{
  "gitbutler": {
    "enabled": true,
    "require_virtual_branch": true,
    "auto_stage_src": true,
    "block_non_gitbutler_commits": true
  }
}
```

**Enforcement**: Bob CLI MUST check `gitbutler.enabled` before any `git add` operation.

## 5. Installation & Setup

### 5.1. GitButler Installation

**Windows**:
```powershell
# Download from https://gitbutler.com/downloads
# Or via Chocolatey
choco install gitbutler
```

**macOS**:
```bash
brew install gitbutler
```

**Linux**:
```bash
# Download AppImage from https://gitbutler.com/downloads
```

### 5.2. Repository Setup

**One-time setup per repository**:
```bash
# Initialize GitButler in repo
cd c:/WSGTA/universal-or-strategy
gitbutler init

# Create default virtual branch for src/ work
gitbutler branch create "src-work" --pattern "src/**/*.cs"

# Create virtual branch for infrastructure work
gitbutler branch create "infra-work" --pattern "docs/**,scripts/**,.bob/**"
```

### 5.3. Agent Configuration

**File**: `AGENTS.md` (update Section 1.5)

**Add**:
```markdown
## 1.6. GitButler Integration (MANDATORY)

ALL agents MUST verify GitButler is active before any commit operation:

```bash
# Pre-commit check
if ! gitbutler branch list | grep -q "active"; then
    echo "ERROR: GitButler not active. Run: gitbutler branch create <name>"
    exit 1
fi
```

**Virtual Branch Strategy**:
- `src-work`: For epic-* branches (src/ only)
- `infra-work`: For main branch (infrastructure only)
- `mixed-work`: For mixed PRs (rare, requires approval)
```

## 6. Enforcement Mechanisms

### 6.1. Pre-Push Validation Enhancement

**File**: `scripts/pre_push_validation.ps1`

**Add Check #18: GitButler Active**:
```powershell
# Check 18: GitButler Active (MANDATORY for epic branches)
$branch = git branch --show-current
if ($branch -match "^epic-") {
    $gitbutlerActive = gitbutler branch list | Select-String "active"
    if (-not $gitbutlerActive) {
        Write-CheckResult "GitButler Active" $false "GitButler not active on epic branch"
        $failedChecks++
    } else {
        Write-CheckResult "GitButler Active" $true "GitButler virtual branch active"
    }
}
```

### 6.2. Bob CLI Pre-Commit Hook

**File**: `.bob/hooks/pre-commit.sh`

**Add**:
```bash
#!/bin/bash
# GitButler enforcement for Bob CLI

if [ "$BOB_MODE" = "v12-engineer" ]; then
    if ! gitbutler branch list | grep -q "active"; then
        echo "[BOB ERROR] GitButler not active"
        echo "Run: gitbutler branch create src-work --pattern 'src/**/*.cs'"
        exit 1
    fi
fi
```

### 6.3. Command Validation

**All 4 main commands MUST include**:
```markdown
## GitButler Validation (MANDATORY)

Before executing any commit operation:
1. Verify GitButler installed: `gitbutler --version`
2. Verify virtual branch active: `gitbutler branch list | grep active`
3. If not active: HALT and instruct user to create virtual branch
```

## 7. Migration Plan

### Phase 1: Installation (Week 1)
- [ ] Install GitButler on all development machines
- [ ] Initialize GitButler in universal-or-strategy repo
- [ ] Create default virtual branches (src-work, infra-work)

### Phase 2: Command Updates (Week 2)
- [ ] Update `/pr-loop` with GitButler integration
- [ ] Update `/epic-run` with GitButler integration
- [ ] Update `/epic-tdd` with GitButler integration
- [ ] Update `/autonomous-refactor` with GitButler integration

### Phase 3: Agent Updates (Week 3)
- [ ] Update Bob CLI settings.json
- [ ] Update AGENTS.md with GitButler requirements
- [ ] Add GitButler checks to pre-push validation
- [ ] Update all custom mode prompts

### Phase 4: Enforcement (Week 4)
- [ ] Enable GitButler blocking in pre-commit hooks
- [ ] Add Check #18 to pre-push validation
- [ ] Test with EPIC-CCN-14 (first epic under new protocol)
- [ ] Document lessons learned

## 8. Rollback Plan

If GitButler causes issues:
1. Disable `gitbutler.enabled` in `.bob/settings.json`
2. Remove Check #18 from pre-push validation
3. Revert to manual `git add src/` workflow
4. Document failure reasons in `docs/brain/GITBUTLER_FAILURE_ANALYSIS.md`

## 9. Success Metrics

**After 4 weeks**:
- Zero PR pollution incidents (currently 1 per epic)
- 50% reduction in "wrong files staged" errors
- 100% agent compliance with GitButler protocol
- Zero rollbacks due to GitButler issues

## 10. References

- **GitButler Docs**: https://docs.gitbutler.com
- **V12 Three-Tier Branch Model**: `docs/protocol/BRANCH_STRATEGY.md`
- **PR Pollution Fix**: `docs/brain/GIT_ADD_COMPREHENSIVE_FIX.md`
- **Pre-Commit Hooks**: `.git/hooks/pre-commit`, `.git/hooks/pre-commit.ps1`

---

**Status**: PROPOSED - Awaiting Director approval to begin Phase 1
**Next Action**: Director must approve migration plan and set start date