# Getting Started - Autonomous Refactoring Building Block

## Quick Start (5 Minutes)

### Prerequisites Check

```bash
# 1. Verify Git version (≥2.35 for worktree support)
git --version

# 2. Verify PowerShell version (≥7.0)
$PSVersionTable.PSVersion

# 3. Verify .NET SDK (≥8.0)
dotnet --version

# 4. Verify Bob CLI installed
bob --version

# 5. Verify Python (≥3.12 for Jane Street KB)
python --version
```

### Installation

```bash
# 1. Clone repository
git clone https://github.com/your-org/universal-or-strategy
cd universal-or-strategy

# 2. Install dependencies
dotnet restore
pip install -r requirements.txt

# 3. Configure Jane Street KB
# (Set FIRESTORE_PROJECT_ID in .env)
cp .env.example .env
# Edit .env and add your Firestore credentials

# 4. Verify setup
python scripts/query_kb.py "test"
```

## Your First Epic (15 Minutes)

### Step 1: Identify Target Method

```bash
# Run complexity audit
python scripts/complexity_audit.py

# Output shows methods with CYC >15:
# src/V12_002.SIMA.Lifecycle.cs::AdoptFleetOrders (CYC 37)
# src/V12_002.Orders.cs::ProcessOrderQueue (CYC 28)
# ...
```

### Step 2: Start Epic

```bash
# Open Bob CLI in v12-engineer mode
bob

# In Bob CLI:
/epic-run EPIC-CCN-17 "AdoptFleetOrders CYC 37→8"
```

### Step 3: Monitor Progress

Bob CLI will guide you through 6 phases:

```
[Phase 0] Hotspot Analysis... ✓
[Phase 1] Intake... ✓
[Phase 2] Plan... ✓
[Phase 3] Validate... ✓
[Phase 4] Tickets... ✓
[Phase 5] Execute Ticket 1...
  → Extraction plan ready
  → Executing...
  → Calling /pr-loop...
  → PHS 100/100 ✓
  → [F5-GATE] Press F5 in NinjaTrader
```

### Step 4: F5 Verification

1. Open NinjaTrader 8
2. Press F5 (or click Compile)
3. Look for BUILD_TAG banner in output
4. Return to Bob CLI and type: `F5 done [BUILD_TAG]`

### Step 5: Review Results

```bash
# Check completion report
cat docs/brain/EPIC-CCN-17/05-completion-report.md

# Verify CYC reduction
python scripts/complexity_audit.py | grep AdoptFleetOrders
# Before: CYC 37
# After: CYC 8 ✓
```

**Congratulations!** You've completed your first autonomous epic. 🎉

## Parallel Execution (30 Minutes Setup)

### Step 1: Create Worktrees

```bash
# Run automated setup
powershell -File .\scripts\setup_parallel_epic_workflow.ps1

# This creates:
# - C:\WSGTA\universal-or-epic-cluster-1 (SIMA)
# - C:\WSGTA\universal-or-epic-cluster-2 (Orders)
# - C:\WSGTA\universal-or-epic-cluster-3 (Lifecycle)
```

### Step 2: Configure Auto-Approval

```bash
# Enable YOLO mode in all worktrees
powershell -File .\scripts\create_worktree_auto_approval.ps1

# Verifies .bob/settings.json exists in each worktree
```

### Step 3: Launch Parallel Execution

Open 3 Bob CLI windows:

**Window 1 (SIMA Cluster)**:
```bash
cd C:\WSGTA\universal-or-epic-cluster-1
bob
/epic-run EPIC-CCN-19 "Method A CYC 20→8"
```

**Window 2 (Orders Cluster)**:
```bash
cd C:\WSGTA\universal-or-epic-cluster-2
bob
/epic-run EPIC-CCN-20 "Method B CYC 18→8"
```

**Window 3 (Lifecycle Cluster)**:
```bash
cd C:\WSGTA\universal-or-epic-cluster-3
bob
/epic-run EPIC-CCN-21 "Method C CYC 16→8"
```

### Step 4: Batch F5 Verification

When all 3 reach F5 gate:

1. Press F5 in NinjaTrader (tests all 3 together)
2. Verify all 3 BUILD_TAGs appear
3. Confirm in each Bob CLI window

**Time Savings**: 15 minutes (vs 45 minutes sequential)

## Understanding the Loops

### Local Loop (Innermost)

**What**: Verifies single file changes  
**When**: Called by PR Loop during fixes  
**Goal**: 5/5 checks passing

```bash
# Manual invocation (for testing)
/local-loop src/V12_002.SIMA.Lifecycle.cs
```

**Output**:
```
[LOCAL-LOOP] src/V12_002.SIMA.Lifecycle.cs
✓ ASCII-only
✓ Build
✓ Tests
✓ Lint
✓ Format
Score: 5/5 ✓
```

### PR Loop (Inner)

**What**: Achieves 100/100 PHS  
**When**: Called by Epic Run after each ticket  
**Goal**: Zero quality violations

```bash
# Manual invocation
/pr-loop 42
```

**Phases**:
1. Extract bot forensics
2. Categorize issues (VALID/HALLUCINATION/NOISE)
3. Fix VALID issues (calls Local Loop)
4. Push and monitor
5. Repeat until PHS = 100/100

**Output**:
```
[PR-LOOP] PR #42
Iteration 1: PHS 85/100 (15 VALID issues)
  → Fixing P0 issues...
  → Local Loop: 5/5 ✓
  → Pushing...
Iteration 2: PHS 95/100 (5 VALID issues)
  → Fixing P1 issues...
  → Local Loop: 5/5 ✓
  → Pushing...
Iteration 3: PHS 100/100 ✓
[PHS-PERFECT] Ready for merge
```

### Epic Run (Middle)

**What**: Reduces method CYC to ≤8  
**When**: Called by Epic Loop or manually  
**Goal**: Single method refactored

```bash
# Manual invocation
/epic-run EPIC-CCN-17 "AdoptFleetOrders CYC 37→8"
```

**Phases**:
0. Hotspot Analysis (CodeScene)
1. Intake (scope definition)
2. Plan (extraction strategy)
3. Validate (triple-agent audit)
4. Tickets (task breakdown)
5. Execute (calls PR Loop per ticket)
6. Review (final verification)

**Output**:
```
[EPIC-RUN] EPIC-CCN-17
Phase 0: Hotspot Analysis ✓
Phase 1: Intake ✓
Phase 2: Plan ✓
Phase 3: Validate ✓
Phase 4: Tickets (3 tickets) ✓
Phase 5: Execute
  Ticket 1: CYC 37→17 ✓
  Ticket 2: CYC 17→10 ✓
  Ticket 3: CYC 10→8 ✓
Phase 6: Review ✓
[EPIC-COMPLETE] CYC 37→8 (78% reduction)
```

### Epic Loop (Outer)

**What**: Processes all 165 epics  
**When**: Top-level orchestration  
**Goal**: All methods CYC ≤8

```bash
# Manual invocation
/epic-loop 19 168
```

**Execution**:
- Calls Epic Run for each epic
- Monitors progress
- Handles failures
- Reports metrics

**Output**:
```
[EPIC-LOOP] Processing 150 epics (19-168)
Epic 19: ✓ (CYC 20→8)
Epic 20: ✓ (CYC 18→7)
Epic 21: ✓ (CYC 16→8)
...
Progress: 3/150 (2%)
Estimated completion: 6 weeks
```

## Common Workflows

### Daily Workflow (Parallel Execution)

**Morning** (2-3 hours):
```bash
# Start 3 epics in parallel (no F5)
# Window 1: EPIC-CCN-X
# Window 2: EPIC-CCN-Y
# Window 3: EPIC-CCN-Z
```

**Lunch** (15 minutes):
```bash
# Batch F5 verification
# Test all 3 together
# Confirm all 3 BUILD_TAGs
```

**Afternoon** (2-3 hours):
```bash
# Start next 3 epics in parallel (no F5)
# Window 1: EPIC-CCN-X+1
# Window 2: EPIC-CCN-Y+1
# Window 3: EPIC-CCN-Z+1
```

**Evening** (15 minutes):
```bash
# Batch F5 verification
# Test all 3 together
# Confirm all 3 BUILD_TAGs
```

**Daily Output**: 6 tickets = 2 epics completed

### Weekly Workflow

**Monday-Friday**: Execute 2 epics/day (10 total)  
**Weekend**: Merge branches, update roadmap

**Weekly Output**: 10 epics completed

### Monthly Workflow

**Weeks 1-4**: Execute 40 epics  
**Week 5**: Retrospective, adjust strategy

**Monthly Output**: 40 epics completed

## Troubleshooting

### Epic Fails at F5 Gate

**Symptom**: Compilation errors after pressing F5

**Diagnosis**:
```bash
# Check hard link sync
powershell -File .\deploy-sync.ps1

# Verify file count
# Expected: 81/81 files synced
```

**Fix**:
```bash
# Re-sync hard links
powershell -File .\deploy-sync.ps1

# Re-run F5
# If still fails, check error message
```

### PR Loop Stuck at <100 PHS

**Symptom**: PHS oscillates between 85-95 after 5 iterations

**Diagnosis**:
```bash
# Check forensics report
cat docs/brain/pr_X_forensics.md

# Look for:
# - High HALLUCINATION count (bot errors)
# - INFRA-NOISE (not code issues)
# - VALID issues that conflict with Jane Street
```

**Fix**:
```bash
# Option 1: Manual override (if issues are low-priority)
# Director approves <100 merge

# Option 2: Suppress false positives
# Add to .codacy.yml exclude_paths

# Option 3: Fix remaining VALID issues manually
```

### Worktree Merge Conflicts

**Symptom**: Git reports conflicts when merging cluster branches

**Diagnosis**:
```bash
# Check which files conflict
git status

# Expected: Only non-src files (docs, scripts)
# Unexpected: src/ files (indicates isolation failure)
```

**Fix**:
```bash
# For non-src conflicts: Accept incoming changes
git checkout --theirs docs/brain/EPIC_STATUS.md
git add docs/brain/EPIC_STATUS.md

# For src/ conflicts: HALT and investigate
# This should never happen with proper file isolation
```

### Jane Street KB Query Fails

**Symptom**: `python scripts/query_kb.py` returns empty results

**Diagnosis**:
```bash
# Check Firestore credentials
cat .env | grep FIRESTORE

# Verify network connectivity
ping firestore.googleapis.com
```

**Fix**:
```bash
# Re-configure credentials
cp .env.example .env
# Edit .env with correct FIRESTORE_PROJECT_ID

# Test connection
python scripts/query_kb.py "test"
```

## Best Practices

### Before Starting

1. ✅ Run complexity audit (identify targets)
2. ✅ Verify GODMODE (all quality gates passing)
3. ✅ Sync hard links (`deploy-sync.ps1`)
4. ✅ Query Jane Street KB (load patterns)
5. ✅ Review epic roadmap (check dependencies)

### During Execution

1. ✅ Monitor Bob CLI output (watch for errors)
2. ✅ Review extraction plans (verify CYC targets)
3. ✅ Check PR Loop iterations (watch for oscillation)
4. ✅ Verify F5 BUILD_TAGs (confirm correct version)
5. ✅ Update manifest.json (track progress)

### After Completion

1. ✅ Run complexity audit (verify CYC reduction)
2. ✅ Run pre-push validation (verify quality)
3. ✅ Sync hard links (update NinjaTrader)
4. ✅ Update epic roadmap (mark complete)
5. ✅ Document lessons learned (retrospective)

## Performance Tips

### Speed Up Local Loop

```bash
# Use incremental builds
dotnet build --no-restore

# Skip slow tests (if safe)
dotnet test --filter "Category!=Slow"

# Parallelize checks
# (Already done in pre_push_validation.ps1)
```

### Speed Up PR Loop

```bash
# Use fast mode (skip slow checks)
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# Cache bot responses
# (Forensics extraction is idempotent)

# Batch fixes (fix all P0 together)
# (Already done in PR Loop V2)
```

### Speed Up Epic Run

```bash
# Use parallel ticket execution
# (Requires independent tickets)

# Skip redundant validations
# (Trust Local Loop results)

# Batch F5 verification
# (Test multiple tickets together)
```

### Speed Up Epic Loop

```bash
# Use parallel execution (3 clusters)
# (64% faster than sequential)

# Optimize cluster assignment
# (Balance complexity across clusters)

# Batch merge operations
# (Merge all 3 clusters weekly)
```

## Metrics & Monitoring

### Track Progress

```bash
# Epic completion rate
cat epic_roadmap.json | jq '.epics[] | select(.status=="completed") | .id'

# Average CYC reduction
python scripts/complexity_audit.py --trend

# Time per epic
cat docs/brain/EPIC_STATUS.md | grep "Duration"

# Quality score trend
cat docs/brain/PHS_HISTORY.md
```

### Monitor Health

```bash
# Jane Street violations (should stay at 347 P0)
cat docs/standards/JANE_STREET_DEVIATIONS.md | grep "P0"

# Build time trend (should stay <30s)
cat .bob/notes/build-times.log

# Test coverage (target 80%)
dotnet test --collect:"XPlat Code Coverage"

# Dead code count (should decrease)
python scripts/dead_code_scan.py
```

## Next Steps

### After First Epic

1. Review completion report
2. Understand CYC reduction strategy
3. Identify patterns for next epic
4. Update personal notes

### After First Week

1. Analyze velocity (epics per day)
2. Identify bottlenecks (F5 gate, PR loop)
3. Optimize workflow (parallel execution)
4. Document lessons learned

### After First Month

1. Calculate time savings (vs sequential)
2. Measure quality improvement (PHS trend)
3. Assess Jane Street alignment (violation count)
4. Plan next phase (EPIC-CCN-169+)

## Resources

### Documentation

- **README**: Overview and benefits
- **ARCHITECTURE**: System design and loops
- **GETTING_STARTED**: This guide
- **Workflow Guide**: `docs/workflow/PARALLEL_EPIC_EXECUTION.md`
- **Skill Reference**: `plugins/parallel-epic-execution/SKILL.md`

### Scripts

- **Setup**: `scripts/setup_parallel_epic_workflow.ps1`
- **Auto-Approval**: `scripts/create_worktree_auto_approval.ps1`
- **Complexity Audit**: `scripts/complexity_audit.py`
- **Pre-Push Validation**: `scripts/pre_push_validation.ps1`
- **Jane Street KB**: `scripts/query_kb.py`

### Commands

- **Epic Loop**: `/epic-loop [start] [end]`
- **Epic Run**: `/epic-run EPIC-ID "description"`
- **PR Loop**: `/pr-loop PR_NUMBER`
- **Local Loop**: `/local-loop FILE_PATH`

### Support

- **GitHub**: [universal-or-strategy](https://github.com/your-org/universal-or-strategy)
- **Slack**: #v12-epic-workflow
- **Email**: v12-architecture-team@your-org.com

## FAQ

### Q: How long does one epic take?

**A**: 15-25 minutes on average (6 phases + F5 gate)

### Q: Can I run epics in parallel?

**A**: Yes! Use 3 clusters (SIMA/Orders/Lifecycle) for 64% time savings

### Q: What if F5 fails?

**A**: Check `deploy-sync.ps1` output, verify hard links, re-run F5

### Q: What if PR Loop gets stuck?

**A**: Use manual override gate after 5 iterations, approve <100 merge

### Q: How do I resume after failure?

**A**: Check `manifest.json`, identify last completed phase, resume from next

### Q: Can I skip phases?

**A**: No. All 6 phases are mandatory for correctness

### Q: What's the Jane Street KB?

**A**: Firestore knowledge base with HFT correctness patterns

### Q: Why CYC ≤8 instead of ≤15?

**A**: Jane Street ultra-alignment (cognitive simplicity for HFT)

### Q: Can I use this for other projects?

**A**: Yes! The building block is project-agnostic (just configure targets)

### Q: How do I contribute?

**A**: See CONTRIBUTING.md (coming soon)

---

**Ready to start?** Run your first epic now:

```bash
bob
/epic-run EPIC-CCN-17 "AdoptFleetOrders CYC 37→8"
```

Good luck! 🚀