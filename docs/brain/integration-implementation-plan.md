# Integration Implementation Plan - FINAL

**Date**: 2026-06-03  
**Status**: APPROVED BY DIRECTOR  
**Scope**: Jane Street + Source Code Context → ALL V12 workflows

---

## Director Decisions (Confirmed)

✅ **Phase Order**: Phase 1 → 2 → 3 → 4  
✅ **Jane Street Repos**: ALL 8 relevant repos (see below)  
✅ **Performance Checks**: BLOCKING from day 1  
✅ **Negative Evidence**: Keep forever (no expiration)  
✅ **Session Snapshots**: After EVERY ticket  
✅ **Budget Limits**: None (no token limits currently)  
✅ **Integration Scope**: ALL slash commands  

---

## Technical Recommendations (Bob's Analysis)

### 1. jcodemunch Setup
**Recommendation**: ✅ YES - Document in MISE_SETUP.md

**Rationale**:
- jcodemunch-mcp is critical for Source Code Context patterns
- Mise should manage jcodemunch as a tool
- Setup must be reproducible across all agents

**Action**: Add jcodemunch to `.mise.toml` + document in `MISE_SETUP.md`

---

### 2. Benchmark Baseline
**Recommendation**: ✅ Store in git (not regenerate)

**Rationale**:
- **Stability**: Regenerating from main can drift if main changes
- **Reproducibility**: Git-stored baselines are immutable
- **Speed**: No need to checkout main and rebuild
- **Audit Trail**: Git history shows when baselines were updated

**Implementation**:
```
benchmarks/baselines/
├── main-2026-06-03.json      # Baseline snapshot
├── main-2026-06-10.json      # Updated after major changes
└── README.md                 # How to update baselines
```

**Update Protocol**:
- Update baseline after merging performance improvements
- Never update for regressions (that's what we're catching)
- Store as JSON with metadata (commit SHA, date, machine specs)

---

### 3. Negative Evidence Format
**Recommendation**: ✅ JSON (not Markdown)

**Rationale**:
- **Performance**: Faster parsing (no regex needed)
- **Agent Preference**: Agents work better with structured data
- **Queryability**: Easy to search/filter programmatically
- **Validation**: JSON schema can enforce structure

**Format**:
```json
{
  "version": "1.0",
  "last_updated": "2026-06-03T18:40:00Z",
  "entries": [
    {
      "feature": "CSRF protection middleware",
      "queries": ["csrf", "xsrf", "token validation"],
      "searched_at": "2026-06-03T10:30:00Z",
      "searched_by": "v12-engineer",
      "files_searched": ["src/", "middleware/"],
      "confidence": "high"
    }
  ]
}
```

**File**: `docs/brain/negative_evidence.json`

---

### 4. Session Snapshot Storage
**Recommendation**: ✅ Per ticket (not per epic)

**Rationale**:
- **Granularity**: Each ticket is a distinct context
- **Recovery**: Can resume from any ticket if session crashes
- **Debugging**: Easier to trace which ticket caused issues
- **Size**: Per-ticket files stay small (~200 tokens each)

**Structure**:
```
docs/brain/EPIC-CCN-14/
├── session-snapshot-01.md    # After ticket 1
├── session-snapshot-02.md    # After ticket 2
├── session-snapshot-03.md    # After ticket 3
└── session-snapshot-final.md # After all tickets
```

---

### 5. Jane Street Validation
**Recommendation**: ✅ Separate check (not integrated into lint)

**Rationale**:
- **Clarity**: Jane Street validation is DNA-level, lint is syntax-level
- **Flexibility**: Can disable Jane Street check without affecting lint
- **Reporting**: Separate reports for DNA vs style issues
- **Evolution**: Jane Street patterns may change independently of lint rules

**Implementation**: Add as Check #16 in `pre_push_validation.ps1`

```powershell
# Check #16: Jane Street DNA Validation
Write-Host "Running Jane Street DNA validation..." -ForegroundColor Cyan
$janeStreetResult = python scripts/jane_street_validator.py
if ($janeStreetResult -ne 0) {
    Write-Host "❌ Jane Street DNA validation failed" -ForegroundColor Red
    $failedChecks++
}
```

---

## Jane Street Repos - Complete List (8 Relevant)

Based on analysis in `docs/brain/jane-street-analysis.md`:

### Tier 1: Direct Use (3 repos)
1. **core_bench** (56 stars) - Micro-benchmarking
2. **async** (256 stars) - Async patterns
3. **patience_diff** (23 stars) - Better diffs

### Tier 2: Validation & Reference (2 repos)
4. **core** (1285 stars) - Standard library (coding standards)
5. **hardcaml** (1049 stars) - FSM patterns (conceptual)

### Tier 3: Indirect Use (3 repos)
6. **sexplib** (176 stars) - Structured logging
7. **re2** (47 stars) - Fast regex
8. **rpc_parallel** (50 stars) - Multi-agent coordination

**Total**: 8 repos to index

**Excluded** (not relevant):
- learn-ocaml-workshop (educational)
- install-ocaml (setup only)
- jenga (build system, we use dotnet)
- ocaml_plugin (archived)

---

## Implementation Plan - FINAL

### Phase 1: Quick Wins (This Week - 4 hours)

#### Task 1.1: Source Code Context Patterns
**Files to Modify**:
- `.bob/commands/epic-run.md` - Add Phase 0.5, Phase 5.5
- `.bob/commands/pr-loop.md` - Add Step 1.5, Step 2 enhancement, Step 3.5
- `.bob/commands/epic-tdd.md` - Add Step 2 enhancement, Step 4 enhancement
- `.bob/commands/github-migration.md` - Add session snapshots

**New Files**:
- `docs/brain/negative_evidence.json` - Empty template
- `scripts/negative_evidence_check.py` - Validation script

**Mise Tasks**:
```toml
[tasks.plan-turn]
description = "Run plan_turn for session-aware routing"
run = "echo 'Requires jcodemunch-mcp - call via MCP tool'"

[tasks.session-snapshot]
description = "Create session snapshot for context continuity"
run = "echo 'Requires jcodemunch-mcp - call via MCP tool'"

[tasks.register-edit]
description = "Invalidate jcodemunch caches after editing files"
run = "echo 'Requires jcodemunch-mcp - call via MCP tool'"

[tasks.negative-evidence-check]
description = "Check if issue mentions non-existent code"
run = "python scripts/negative_evidence_check.py --issue=$ISSUE"
```

**Deliverables**:
- ✅ 4 slash commands updated
- ✅ 2 new files created
- ✅ 4 new Mise tasks added

---

### Phase 2: Jane Street Exploration (Next Week - 8 hours)

#### Task 2.1: Index All 8 Jane Street Repos
**Command**:
```bash
# Tier 1 (Direct Use)
mise run index-jane-street --repo=core_bench
mise run index-jane-street --repo=async
mise run index-jane-street --repo=patience_diff

# Tier 2 (Validation)
mise run index-jane-street --repo=core
mise run index-jane-street --repo=hardcaml

# Tier 3 (Indirect)
mise run index-jane-street --repo=sexplib
mise run index-jane-street --repo=re2
mise run index-jane-street --repo=rpc_parallel
```

**Mise Task**:
```toml
[tasks.index-jane-street]
description = "Index a Jane Street repository for analysis"
run = """
# Requires jcodemunch-mcp
# Usage: mise run index-jane-street --repo=core_bench
echo "Indexing Jane Street repo: $REPO"
# Call jcodemunch index_repo via MCP
"""
```

#### Task 2.2: Extract Patterns
**New Files**:
- `docs/standards/JANE_STREET_CHECKLIST.md` - DNA validation checklist
- `docs/standards/JANE_STREET_ASYNC_PATTERNS.md` - Async patterns
- `docs/standards/JANE_STREET_BENCHMARKING.md` - Benchmarking patterns
- `docs/standards/JANE_STREET_FSM_PATTERNS.md` - FSM patterns from hardcaml

**Analysis Queries** (via jcodemunch):
```
# core_bench
search_symbols(repo="core_bench", query="benchmark")
search_symbols(repo="core_bench", query="statistical")

# async
search_symbols(repo="async", query="error")
search_symbols(repo="async", query="timeout")
search_symbols(repo="async", query="cancel")

# core
search_symbols(repo="core", query="result")
search_symbols(repo="core", query="option")
```

**Deliverables**:
- ✅ 8 repos indexed
- ✅ 4 pattern documents created
- ✅ 1 Mise task added

---

### Phase 3: Performance Integration (2 Weeks - 12 hours)

#### Task 3.1: Benchmark Infrastructure
**New Files**:
- `scripts/benchmark_regression.ps1` - Regression detection
- `scripts/benchmark_analysis.py` - Statistical analysis
- `benchmarks/baselines/main-2026-06-03.json` - Initial baseline
- `benchmarks/baselines/README.md` - Update protocol

**Benchmark Baseline Format**:
```json
{
  "version": "1.0",
  "commit_sha": "abc123",
  "created_at": "2026-06-03T18:40:00Z",
  "machine": {
    "os": "Windows 11",
    "cpu": "Intel i9-12900K",
    "ram": "64GB"
  },
  "benchmarks": {
    "BarUpdate": {
      "p50": 1.2,
      "p99": 2.5,
      "mean": 1.5,
      "stddev": 0.3,
      "unit": "microseconds"
    }
  }
}
```

**Mise Tasks**:
```toml
[tasks.benchmark]
description = "Run performance benchmarks (Jane Street style)"
run = """
dotnet run --project benchmarks/V12_Performance.Benchmarks.csproj --configuration Release
python scripts/benchmark_analysis.py
"""

[tasks.benchmark-regression]
description = "Check for performance regressions vs baseline (BLOCKING)"
run = """
pwsh -File ./scripts/benchmark_regression.ps1
"""

[tasks.benchmark-baseline-update]
description = "Update benchmark baseline (after performance improvements)"
run = """
mise run benchmark
cp benchmarks/results/latest.json benchmarks/baselines/main-$(date +%Y-%m-%d).json
git add benchmarks/baselines/
git commit -m "chore: Update benchmark baseline"
"""
```

#### Task 3.2: Integrate into pr-loop
**Modify**: `.bob/commands/pr-loop.md`

**Add Step 2.5** (NEW - Performance Regression Check):
```markdown
### Step 2.5: Performance Regression Check (BLOCKING)

**Switch to: Advanced mode**

Hand off:
```
TASK: Check Performance Regressions
PR: $1
PROTOCOL:
  1. Run: mise run benchmark-regression
  2. If regression > 5%: HALT and report
  3. If improvement > 10%: Document in PR description
  4. Emit: [PERF-CHECK] Pass/Fail
```

**Gate**: BLOCKING - Must pass before proceeding to Step 3
```

**Deliverables**:
- ✅ 4 new files created
- ✅ 3 new Mise tasks added
- ✅ pr-loop Step 2.5 added (BLOCKING)

---

### Phase 4: Advanced Integration (1 Month - 20 hours)

#### Task 4.1: Jane Street Validator
**New File**: `scripts/jane_street_validator.py`

**Checks**:
1. No exceptions thrown (use Result<T, Error> pattern)
2. All state transitions explicit (no implicit state)
3. Function names describe intent, not implementation
4. No magic numbers (all constants named)
5. Complexity ≤15 (cognitive simplicity)
6. No locks (Actor/FSM pattern only)
7. ASCII-only (no Unicode)
8. Mandatory braces (explicit control flow)

**Output Format**:
```json
{
  "version": "1.0",
  "checked_at": "2026-06-03T18:40:00Z",
  "files_checked": 207,
  "violations": [
    {
      "file": "src/V12_002.cs",
      "line": 123,
      "rule": "no_exceptions",
      "severity": "error",
      "message": "Exception thrown: use Result<T, Error> instead"
    }
  ],
  "summary": {
    "errors": 5,
    "warnings": 12,
    "passed": true
  }
}
```

#### Task 4.2: Integrate into pre_push_validation.ps1
**Add Check #16**:
```powershell
# Check #16: Jane Street DNA Validation (BLOCKING)
Write-Host "`n[16/16] Jane Street DNA Validation..." -ForegroundColor Cyan
$janeStreetResult = python scripts/jane_street_validator.py
if ($janeStreetResult -ne 0) {
    Write-Host "❌ Jane Street DNA validation failed" -ForegroundColor Red
    Write-Host "   Run: python scripts/jane_street_validator.py --verbose" -ForegroundColor Yellow
    $failedChecks++
} else {
    Write-Host "✅ Jane Street DNA validation passed" -ForegroundColor Green
}
```

#### Task 4.3: Budget-Aware Exploration
**Modify**: `.bob/custom_modes.yaml` (v12-epic-planner mode)

**Add Budget Check**:
```yaml
v12-epic-planner:
  rules:
    - Check _meta.budget_warning after each jcodemunch call
    - If warning: Stop analysis, work with current findings
    - Document in plan: "Analysis limited by budget"
```

#### Task 4.4: Session Continuity
**Modify**: `.bob/commands/epic-run.md`

**Enhance Phase 5.5**:
```markdown
### Phase 5.5: Session Snapshot (Context Continuity)

**Switch to: Advanced mode**

Hand off:
```
TASK: Create Session Snapshot + Load Previous
PROTOCOL:
  1. If previous snapshot exists:
     - Read: docs/brain/$EPIC/session-snapshot-XX.md
     - Restore context (focus files, key searches, dead ends)
  2. Run: get_session_snapshot(max_files=10, max_searches=5)
  3. Save to: docs/brain/$EPIC/session-snapshot-YY.md
  4. Emit: [SNAPSHOT-SAVED] Ready for next ticket
```
```

**Deliverables**:
- ✅ 1 new script created (jane_street_validator.py)
- ✅ Check #16 added to pre_push_validation.ps1
- ✅ Budget-aware exploration added to v12-epic-planner
- ✅ Session continuity enhanced in epic-run

---

## New Mise Tasks - Complete List

Add to `.mise.toml`:

```toml
# === JANE STREET INTEGRATION ===

[tasks.index-jane-street]
description = "Index a Jane Street repository for analysis"
run = """
# Requires jcodemunch-mcp
# Usage: mise run index-jane-street --repo=core_bench
echo "Indexing Jane Street repo: $REPO"
"""

[tasks.benchmark]
description = "Run performance benchmarks (Jane Street style)"
run = """
dotnet run --project benchmarks/V12_Performance.Benchmarks.csproj --configuration Release
python scripts/benchmark_analysis.py
"""

[tasks.benchmark-regression]
description = "Check for performance regressions vs baseline (BLOCKING)"
run = """
pwsh -File ./scripts/benchmark_regression.ps1
"""

[tasks.benchmark-baseline-update]
description = "Update benchmark baseline (after performance improvements)"
run = """
mise run benchmark
cp benchmarks/results/latest.json benchmarks/baselines/main-$(date +%Y-%m-%d).json
git add benchmarks/baselines/
git commit -m "chore: Update benchmark baseline"
"""

[tasks.jane-street-validate]
description = "Validate code against Jane Street DNA patterns (BLOCKING)"
run = """
python scripts/jane_street_validator.py
"""

# === SOURCE CODE CONTEXT INTEGRATION ===

[tasks.plan-turn]
description = "Run plan_turn for session-aware routing"
run = """
# Requires jcodemunch-mcp
# Usage: mise run plan-turn --query="task description"
echo "Running plan_turn..."
"""

[tasks.session-snapshot]
description = "Create session snapshot for context continuity"
run = """
# Requires jcodemunch-mcp
# Usage: mise run session-snapshot --output=docs/brain/snapshot.md
echo "Creating session snapshot..."
"""

[tasks.register-edit]
description = "Invalidate jcodemunch caches after editing files"
run = """
# Requires jcodemunch-mcp
# Usage: mise run register-edit --files="file1.cs,file2.cs"
echo "Registering edits..."
"""

[tasks.negative-evidence-check]
description = "Check if issue mentions non-existent code"
run = """
python scripts/negative_evidence_check.py --issue=$ISSUE
"""
```

---

## Documentation Updates

### 1. Update MISE_SETUP.md
**Add Section**: jcodemunch-mcp Setup

```markdown
## jcodemunch-mcp Setup

jcodemunch-mcp is required for Source Code Context patterns (plan_turn, session snapshots, register_edit).

### Installation

**Via npm**:
```bash
npm install -g @jcodemunch/mcp-server
```

**Via Mise** (recommended):
```bash
mise use npm:@jcodemunch/mcp-server@latest
```

### Configuration

Add to `.mcp.json`:
```json
{
  "mcpServers": {
    "jcodemunch": {
      "command": "jcodemunch-mcp",
      "args": []
    }
  }
}
```

### Verification

```bash
# Check if jcodemunch-mcp is available
mise run doctor

# Test jcodemunch-mcp
jcodemunch-mcp --version
```
```

### 2. Update AGENTS.md
**Add Section 1.6**: Jane Street Integration

```markdown
## 1.6. Jane Street Integration

ALL agents MUST follow Jane Street patterns for V12 DNA alignment.

### Indexed Repos (8 total)

**Tier 1** (Direct Use):
- core_bench - Micro-benchmarking
- async - Async patterns
- patience_diff - Better diffs

**Tier 2** (Validation):
- core - Coding standards
- hardcaml - FSM patterns

**Tier 3** (Indirect):
- sexplib - Structured logging
- re2 - Fast regex
- rpc_parallel - Multi-agent coordination

### DNA Checklist

Before every commit, verify:
- [ ] No exceptions (use Result<T, Error>)
- [ ] Explicit state transitions
- [ ] Intent-based naming
- [ ] No magic numbers
- [ ] Complexity ≤15
- [ ] No locks (Actor/FSM only)
- [ ] ASCII-only
- [ ] Mandatory braces

**Validation**: `mise run jane-street-validate`
```

---

## Success Metrics - Updated

### Quantitative (Measurable)
- **Token Efficiency**: 30% reduction in file reads (via plan_turn)
- **False Positives**: 50% reduction in hallucination fixes (via negative evidence)
- **Context Continuity**: 80% reduction in duplicate reads (via session snapshots)
- **Performance Regressions**: 0 regressions shipped (via benchmark checks - BLOCKING)
- **DNA Compliance**: 100% pass rate on Jane Street validator (BLOCKING)

### Qualitative (Observable)
- **V12 DNA Alignment**: All code passes Jane Street checklist
- **Agent Efficiency**: Agents complete tasks faster with fewer retries
- **Code Quality**: Complexity stays ≤15, no exceptions, explicit state
- **Developer Experience**: Fewer false alarms, clearer feedback
- **Benchmark Rigor**: Statistical analysis (p50, p99, variance) instead of "run 3 times"

---

## Timeline - FINAL

### Week 1 (Phase 1): June 3-7, 2026
- ✅ Monday: Update 4 slash commands (epic-run, pr-loop, epic-tdd, github-migration)
- ✅ Tuesday: Create negative_evidence.json + validation script
- ✅ Wednesday: Add 4 Mise tasks (plan-turn, session-snapshot, register-edit, negative-evidence-check)
- ✅ Thursday: Test on PR #2 (current PR)
- ✅ Friday: Document in MISE_SETUP.md + AGENTS.md

**Deliverable**: Source Code Context patterns integrated

---

### Week 2 (Phase 2): June 10-14, 2026
- ✅ Monday: Index Tier 1 repos (core_bench, async, patience_diff)
- ✅ Tuesday: Index Tier 2 repos (core, hardcaml)
- ✅ Wednesday: Index Tier 3 repos (sexplib, re2, rpc_parallel)
- ✅ Thursday: Extract patterns, create 4 standards docs
- ✅ Friday: Review with Director, adjust as needed

**Deliverable**: 8 Jane Street repos indexed + 4 pattern documents

---

### Weeks 3-4 (Phase 3): June 17-28, 2026
- ✅ Week 3: Build benchmark infrastructure (regression.ps1, analysis.py, baselines)
- ✅ Week 4: Integrate into pr-loop Step 2.5 (BLOCKING), test on EPIC-CCN-14

**Deliverable**: Performance regression checks (BLOCKING)

---

### Weeks 5-8 (Phase 4): July 1-26, 2026
- ✅ Week 5: Create jane_street_validator.py
- ✅ Week 6: Integrate as Check #16 (BLOCKING)
- ✅ Week 7: Add budget-aware exploration to v12-epic-planner
- ✅ Week 8: Enhance session continuity in epic-run

**Deliverable**: Full Jane Street DNA validation + advanced patterns

---

## Risk Mitigation - Updated

### Risk 1: Performance Check Too Strict
**Risk**: 5% regression threshold may be too sensitive
**Mitigation**:
- Start with 10% threshold for first 2 weeks
- Collect data on false positives
- Adjust to 5% after validation period
- Allow Director override for known regressions

### Risk 2: Jane Street Patterns Not C#-Compatible
**Risk**: OCaml patterns may not translate directly
**Mitigation**:
- Focus on principles, not syntax
- Adapt to C# idioms (e.g., Result<T, Error> → Result<T> with exceptions)
- Validate with Director before enforcing
- Document deviations in JANE_STREET_DEVIATIONS.md

### Risk 3: jcodemunch-mcp Dependency
**Risk**: Heavy reliance on external tool
**Mitigation**:
- Keep fallback to native tools (grep, read_file)
- Document setup thoroughly in MISE_SETUP.md
- Test all workflows with and without jcodemunch
- Add to Mise for version management

### Risk 4: Benchmark Baseline Drift
**Risk**: Baselines may become stale
**Mitigation**:
- Update baseline after merging performance improvements
- Never update for regressions
- Store metadata (commit SHA, date, machine specs)
- Review baselines monthly

---

## Next Steps - Immediate Actions

### 1. Create Implementation Task
**Delegate to**: Advanced mode

**Task**: Implement Phase 1 (Source Code Context patterns)

**Scope**:
- Update 4 slash commands
- Create negative_evidence.json
- Create negative_evidence_check.py
- Add 4 Mise tasks
- Update MISE_SETUP.md
- Update AGENTS.md

**Estimated Time**: 4 hours

---

### 2. Test on Current PR
**After Phase 1 complete**:
- Run pr-loop on PR #2 with new patterns
- Verify negative evidence check works
- Verify session snapshots work
- Document any issues

---

### 3. Begin Phase 2
**After Phase 1 tested**:
- Start indexing Jane Street repos
- Extract patterns
- Create standards documents

---

## Summary

### What We're Building:
1. **Source Code Context**: Session-aware routing, negative evidence, snapshots
2. **Jane Street Integration**: 8 repos indexed, DNA validation, benchmarking
3. **Performance Gates**: BLOCKING regression checks with statistical rigor
4. **Quality Gates**: BLOCKING Jane Street DNA validation

### Where It Goes:
1. **ALL slash commands**: epic-run, pr-loop, epic-tdd, github-migration
2. **Mise tasks**: 9 new tasks for workflow integration
3. **Pre-push validation**: Check #16 (Jane Street DNA - BLOCKING)
4. **Documentation**: 6 new/updated docs

### Why It Matters:
1. **Efficiency**: 30% fewer wasted file reads
2. **Quality**: Jane Street-grade validation
3. **Reliability**: Catch regressions before merge
4. **Continuity**: Maintain context across long sessions

### When It Happens:
- **Phase 1**: This week (4 hours)
- **Phase 2**: Next week (8 hours)
- **Phase 3**: Weeks 3-4 (12 hours)
- **Phase 4**: Weeks 5-8 (20 hours)

---

**Status**: READY FOR IMPLEMENTATION  
**Next Action**: Delegate Phase 1 to Advanced mode