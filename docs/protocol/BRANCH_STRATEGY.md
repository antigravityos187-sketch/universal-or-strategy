# Three-Tier Branch Model + Parallel Execution (V12.22)

## Overview
Separates changes by **architectural layer** to prevent PR noise and enable independent review cycles. Extended with **parallel execution workflow** for Bob IDE (orchestrator) + 2 Bob Shell (workers).

---

## Tier 1: Source Code Branches (`feature/src-*`, `fix/src-*`)

**Purpose**: Production code changes only

**Allowed Files:**
- ✅ `.cs` files in `src/` directory
- ✅ `.csproj` files (project structure)
- ✅ Test files in `tests/` (if test-only changes)

**Forbidden:**
- ❌ Any `docs/`, `scripts/`, `.github/` files
- ❌ Config files (`.yml`, `.json`, `.toml`)
- ❌ Markdown files
- ❌ PowerShell/Python scripts

**Review Process:**
- ✅ **PR REQUIRED** (always)
- Full Arena AI + Codacy + CodeRabbit review
- Complexity audit required
- Build + test gates enforced
- Longest review cycle (2-3 days typical)

**Execution Environment:**
- ✅ **Bob Shell** (for parallel execution)
- ❌ **Bob IDE** (orchestrator only, no direct src/ work)

**Example branches:**
- `feature/src-epic-8-extract-sima`
- `fix/src-null-reference-atm`

---

## Tier 2: Infrastructure Branches (`feature/infra-*`, `fix/infra-*`)

**Purpose**: Tooling, scripts, documentation, configs

**Allowed Files:**
- ✅ `docs/` (all markdown, images, PDFs)
- ✅ `scripts/` (PowerShell, Python, Shell)
- ✅ `.github/` (workflows, actions, templates)
- ✅ Config files (`.codacy.yml`, `.editorconfig`, etc.)
- ✅ `docs/brain/` (session notes, forensics)

**Forbidden:**
- ❌ `.cs` files in `src/`

**Review Process:**
- ✅ **PR REQUIRED** (for git history)
- Lightweight review (no complexity audit)
- Markdown linting only
- Fast-track merge (same day typical)
- No bot review unless security-sensitive

**Execution Environment:**
- ✅ **Bob Shell** (for parallel execution)
- ✅ **Bob IDE** (for orchestration tasks)

**Example branches:**
- `feature/infra-pr18-fixes`
- `fix/infra-broken-deploy-script`

---

## Tier 3: Protocol Branches (`feature/protocol-*`, `fix/protocol-*`)

**Purpose**: Agent rules, workflows, meta-configuration

**Allowed Files:**
- ✅ `.bob/`, `.agent/`, `.claude/`, `.gemini/` directories
- ✅ `AGENTS.md`, `CLAUDE.md`, `BOB.md`, `CODEX.md`
- ✅ `docs/protocol/` (protocol documentation)
- ✅ `.mcp.json`, `bob.config.yaml`

**Forbidden:**
- ❌ `.cs` files in `src/`
- ❌ General scripts (unless protocol-related)

**Review Process (HYBRID):**
- ⚠️ **PR OPTIONAL** (Director's choice)
- **Default: Direct push to main** (95% of cases)
  - Faster: 2 minutes vs 5 minutes
  - 100% reliable (zero bot interference)
  - Git history preserved via commit messages
- **Optional: Create PR** (5% of cases)
  - When seeking second opinion
  - When change is complex/controversial
  - When you want formal review record
  - Protocol Guard workflow auto-blocks bots

**Execution Environment:**
- ✅ **Bob IDE** (primary for protocol work)
- ✅ **Bob Shell** (if needed for testing)

**Workflow Options:**

**Option A: Direct Push (Recommended)**
```powershell
# Work on protocol branch
git checkout -b feature/protocol-fix-issue

# Make changes, commit
git commit -m "[PROTOCOL] Fix issue - rationale here"

# Merge directly to main
git checkout main
git merge feature/protocol-fix-issue --no-ff
git push origin main

# Delete branch
git branch -d feature/protocol-fix-issue
```

**Option B: PR Review (When Needed)**
```powershell
# Work on protocol branch
git checkout -b feature/protocol-complex-change

# Make changes, commit
git commit -m "[PROTOCOL] Complex change - needs review"

# Push and create PR
git push origin feature/protocol-complex-change
gh pr create --title "[PROTOCOL] Complex change" --body "Rationale..."

# Protocol Guard workflow will:
# - Auto-add 'protocol-only' label
# - Block bot reviews via CODEOWNERS
# - Post human-readable notice
```

**Example branches:**
- `feature/protocol-branch-guard` (direct push)
- `fix/protocol-bob-mode-enforcement` (direct push)
- `feature/protocol-major-refactor` (PR if complex)

---

## Parallel Execution Workflow (NEW)

### Architecture

**Bob IDE (VS Code)**: Orchestrator
- Runs `epic-run` or `epic-tdd` commands
- Coordinates 2 Bob Shell workers
- Monitors progress and handles F5 gates
- Does NOT execute code directly

**Bob Shell Window 1**: Worker A
- Executes EPIC-X in subgraph S1
- Autonomous execution (9 review gates)
- Reports status to orchestrator

**Bob Shell Window 2**: Worker B
- Executes EPIC-Y in subgraph S2
- Autonomous execution (9 review gates)
- Reports status to orchestrator

### Subgraph Isolation Rules

**Safe Parallel Combinations** (from `docs/architecture.md`):

| Subgraph | Files | Can Run With |
|----------|-------|--------------|
| S1: SIMA Core | `V12_002.cs` (SIMA methods) | S2, S3, S4, S5, S6, S7, S8 |
| S2: Execution Engine | `V12_002.cs` (order methods) | S1, S3, S4, S5, S6, S7, S8 |
| S3: UI & Photon IO | `V12_002.cs` (UI methods) | S1, S2, S4, S5, S6, S7, S8 |
| S4: ATM Logic | `V12_002.Atm.cs` | S1, S2, S3, S5, S6, S7, S8 |
| S5: Drawing Helpers | `V12_002.DrawingHelpers.cs` | S1, S2, S3, S4, S6, S7, S8 |
| S6: Utilities | `V12_002.Utilities.cs` | S1, S2, S3, S4, S5, S7, S8 |
| S7: State Management | `V12_002.StateManagement.cs` | S1, S2, S3, S4, S5, S6, S8 |
| S8: Configuration | `V12_002.Configuration.cs` | S1, S2, S3, S4, S5, S6, S7 |

**Conflict Detection**:
- ❌ **NEVER** run 2 EPICs in the same subgraph simultaneously
- ❌ **NEVER** run 2 EPICs that modify the same file
- ✅ **ALWAYS** verify subgraph isolation before starting parallel work

### Branch Naming for Parallel Work

**Pattern**: `feature/src-epic-{N}-{subgraph}-{description}`

**Examples**:
```
Window 1: feature/src-epic-8-s1-extract-sima
Window 2: feature/src-epic-9-s3-extract-ui
```

**Benefits**:
- Branch name encodes subgraph (conflict detection)
- Easy to see which EPICs are running in parallel
- Git history shows parallel execution clearly

### Execution Protocol

**Step 1: Director Plans Parallel Work**
```
Bob IDE (Orchestrator):
> "I want to run EPIC-8 and EPIC-9 in parallel. Verify subgraph isolation."

Orchestrator checks:
- EPIC-8 targets S1 (SIMA Core)
- EPIC-9 targets S3 (UI & Photon IO)
- No file overlap
- Safe to proceed
```

**Step 2: Launch Workers**
```
Bob Shell Window 1:
$ epic-run epic-8 "OnKeyDown + ProcessIpc extraction"

Bob Shell Window 2:
$ epic-run epic-9 "AttachPanelHandlers + OnSyncAllClick extraction"
```

**Step 3: Orchestrator Monitors**
```
Bob IDE tracks:
- Window 1: Phase 2.5 (Architectural Review)
- Window 2: Phase 4 (Tickets)
- No conflicts detected
```

**Step 4: F5 Gates (Sequential)**
```
Window 1 reaches F5 gate first:
> "Press F5 for EPIC-8 ticket-01"
Director: F5 done [BUILD_TAG_001]

Window 2 reaches F5 gate:
> "Press F5 for EPIC-9 ticket-01"
Director: F5 done [BUILD_TAG_002]
```

**Step 5: PR Coordination**
```
Window 1 completes first:
- Creates PR #8
- Runs /pr-loop to 100/100 PHS
- Waits for merge approval

Window 2 completes:
- Creates PR #9
- Runs /pr-loop to 100/100 PHS
- Waits for merge approval

Director merges in order:
1. PR #8 (EPIC-8)
2. PR #9 (EPIC-9)
```

### Merge Order Rules

**Rule 1: First-Complete, First-Merge**
- PR that reaches 100/100 PHS first gets merged first
- Other PRs rebase after merge

**Rule 2: Dependency-Aware Merging**
- If EPIC-9 depends on EPIC-8, merge EPIC-8 first
- Document dependencies in EXECUTION_GUIDE.md

**Rule 3: Conflict Resolution**
- If merge conflict detected, HALT parallel execution
- Resolve conflict manually
- Resume with rebased branches

### Performance Gains

**Sequential Execution**:
```
EPIC-8: 2 hours
EPIC-9: 2 hours
Total: 4 hours
```

**Parallel Execution**:
```
EPIC-8: 2 hours (Window 1)
EPIC-9: 2 hours (Window 2, simultaneous)
Total: 2 hours (50% time savings)
```

**Realistic Gains**:
- 2 EPICs: 40-50% time savings
- 3+ EPICs: Not recommended (cognitive overload)

---

## Workflow Examples

### Scenario 1: Pure Source Code Work (Sequential)
```
1. Create feature/src-epic-X
2. Bob Shell works here (ONLY .cs commits)
3. PR review (full bot suite)
4. Merge to main
```

### Scenario 2: Pure Source Code Work (Parallel)
```
1. Create feature/src-epic-8-s1-extract-sima (Window 1)
2. Create feature/src-epic-9-s3-extract-ui (Window 2)
3. Both Bob Shells work simultaneously
4. PR review for both (full bot suite)
5. Merge PR #8, then PR #9
```

### Scenario 3: Infrastructure + Source Code (Same Epic)
```
1. Create feature/infra-epic-X (infrastructure first)
2. Merge infra branch (fast-track)
3. Create feature/src-epic-X (source code)
4. Merge src branch (full review)
```

### Scenario 4: Protocol Fix During Source Work
```
1. Working on feature/src-epic-X (Window 1)
2. Bob makes mistake → protocol needs update
3. STOP src work in Window 1
4. Switch to Bob IDE
5. Create feature/protocol-fix-issue
6. Fix protocol, merge directly to main (no PR)
7. Resume src work in Window 1
```

### Scenario 5: Emergency Hotfix (Mixed)
```
1. Create hotfix/critical-issue
2. Mixed commits allowed BUT:
   - Separate commits: [SRC] and [INFRA] prefixes
   - Document reason in commit message
3. Create follow-up tickets to separate properly
```

---

## Benefits

**Token Efficiency:**
- Infra PRs: ~500 tokens (no bot review)
- Src PRs: ~5,000 tokens (full review)
- Protocol direct push: 0 tokens (no PR)
- Savings: 90% on infra, 100% on protocol

**Review Speed:**
- Protocol: Immediate (direct push)
- Infra: Same day merge
- Src: 2-3 day review cycle

**Parallel Execution:**
- 2 EPICs: 40-50% time savings
- Subgraph isolation prevents conflicts
- Independent review cycles

**Git History Clarity:**
- Branch name = intent obvious
- No "mixed bag" commits
- Easy to revert by layer
- Parallel work visible in branch names

**Migration Safety:**
- Protocol changes deploy immediately
- No bot configuration needed on new accounts
- CODEOWNERS + Protocol Guard travel with repo

**Jane Street Alignment:**
- Separation of concerns
- Predictable change scope
- Reduced cognitive load
- Parallel execution mirrors HFT pipeline architecture

---

## Enforcement

**Branch Guard Rule:** `.bob/rules-v12-engineer/branch-guard.md`
- Auto-blocks mixed commits
- Auto-stashes violating files
- Logs violations

**Protocol Guard (Optional PR Path):** `.github/workflows/protocol-guard.yml`
- Auto-detects protocol file changes
- Auto-adds `protocol-only` label
- Posts human-readable notice
- Blocks bot reviews via CODEOWNERS

**Subgraph Conflict Detection:** (manual, for now)
- Director verifies subgraph isolation before parallel launch
- Future: Automated conflict detection in epic-run

**Git Hooks:** (optional, not yet implemented)
- Pre-commit validation
- Branch pattern matching

**Bob Shell/IDE Isolation:**
- Bob IDE → orchestrator only (no direct src/ work)
- Bob Shell → src branches (parallel execution)
- Separate terminal windows required for parallel work

---

## Decision Matrix: When to Use PR vs Direct Push

| Change Type | Complexity | Urgency | Recommendation |
|-------------|-----------|---------|----------------|
| Protocol typo fix | Low | High | Direct push |
| Protocol rule tweak | Low | High | Direct push |
| New protocol file | Medium | Medium | Direct push (or PR if unsure) |
| Protocol refactor | High | Low | PR (seek review) |
| Protocol breaking change | High | Medium | PR (document impact) |

**Rule of Thumb**: If you're confident and it's <50 lines, direct push. If you want a second opinion, use PR.

---

## Decision Matrix: Sequential vs Parallel Execution

| Scenario | EPICs | Subgraphs | Recommendation |
|----------|-------|-----------|----------------|
| 2 independent EPICs | EPIC-8, EPIC-9 | S1, S3 | Parallel (40-50% faster) |
| 2 dependent EPICs | EPIC-8, EPIC-9 | S1, S1 | Sequential (conflict risk) |
| 3+ EPICs | EPIC-8, 9, 10 | S1, S2, S3 | Sequential (cognitive overload) |
| 1 complex EPIC | EPIC-8 | S1 | Sequential (focus required) |

**Rule of Thumb**: Parallel execution for 2 independent EPICs in different subgraphs. Sequential for everything else.

---

## Current Status

**Implemented:**
- ✅ Branch guard rule created (`.bob/rules-v12-engineer/branch-guard.md`)
- ✅ Three-tier model documented
- ✅ Protocol Guard workflow (`.github/workflows/protocol-guard.yml`)
- ✅ CODEOWNERS updated for protocol paths
- ✅ Parallel execution workflow documented
- ✅ Subgraph isolation rules defined
- ✅ Branch naming convention for parallel work

**Pending:**
- ⏳ Automated subgraph conflict detection
- ⏳ Git pre-commit hooks (optional)
- ⏳ Fine-tuning as we use it in practice

**Effective Date:** 2026-05-31 (V12.22)