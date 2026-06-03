# Integration Brainstorm: Jane Street + Source Code Context Skill

**Date**: 2026-06-03  
**Purpose**: Integrate Jane Street GitHub insights and Source Code Context patterns into V12 workflows

---

## Executive Summary

We have two powerful resources to integrate:
1. **Jane Street GitHub** - Production HFT code patterns, testing, and architecture
2. **Source Code Context Skill** - Session-aware code navigation using jcodemunch-mcp

**Key Insight**: Both resources emphasize **correctness by construction** and **cognitive simplicity** - perfectly aligned with V12 DNA.

---

## Part 1: Jane Street Integration Strategy

### Current Status
✅ **Already Analyzed**: See [`docs/brain/jane-street-analysis.md`](docs/brain/jane-street-analysis.md)

### Top 3 Repos for V12

#### 1. **core_bench** (Micro-benchmarking) - HIGHEST PRIORITY
**Why**: Jane Street's production benchmarking tool for microsecond-latency measurements

**V12 Integration Points**:
- Replace ad-hoc benchmarking in `benchmarks/V12_Performance.Benchmarks.csproj`
- Add statistical rigor (p50, p99, variance) instead of "run 3 times and average"
- Integrate into pr-loop as **Step 2.5: Performance Regression Check**

**Proposed Workflow**:
```
pr-loop Step 2.5 (NEW):
1. Run benchmarks on current branch
2. Compare to baseline (main branch)
3. If regression > 5%: HALT and report
4. If improvement > 10%: Document in PR description
```

**Implementation**:
- Create `scripts/benchmark_regression.ps1`
- Add to `.mise.toml` as `mise run benchmark`
- Integrate into `pre_push_validation.ps1` as Check #15 (warning-only)

---

#### 2. **async** (Asynchronous Execution) - HIGH PRIORITY
**Why**: Jane Street's core concurrency primitive - similar to V12's Actor/FSM pattern

**V12 Integration Points**:
- **Validation**: Compare Jane Street's async patterns to our `Enqueue` model
- **Error Handling**: Extract patterns for graceful degradation
- **Testing**: Learn how Jane Street tests async code

**Proposed Analysis**:
1. Index `async` repo with jcodemunch: `mise run index-jane-street --repo=async`
2. Search for error handling patterns: `search_symbols(query="error", repo="async")`
3. Document findings in `docs/standards/JANE_STREET_ASYNC_PATTERNS.md`
4. Update `AGENTS.md` with async validation checklist

**Key Questions to Answer**:
- How does Jane Street handle async timeouts?
- What's their pattern for cancellation?
- How do they test race conditions?

---

#### 3. **patience_diff** (Better Diff Algorithm) - MEDIUM PRIORITY
**Why**: Better than standard diff for code (preserves structure)

**V12 Integration Points**:
- Improve `apply_diff` tool accuracy
- Better conflict resolution in pr-loop
- Cleaner diff generation for PRs

**Proposed Enhancement**:
- Study patience_diff algorithm
- Evaluate if we can use it via external tool or need to reimplement
- Test on EPIC-CCN-13 PR to see if it reduces diff noise

---

### Jane Street Coding Standards Extraction

**Goal**: Create a checklist for V12 DNA alignment

**Method**:
1. Index Jane Street `core` repo (their standard library)
2. Extract patterns:
   - Naming conventions
   - Error handling (Result types vs exceptions)
   - Type safety patterns
   - Documentation standards
3. Create `docs/standards/JANE_STREET_CHECKLIST.md`
4. Add to pr-loop Step 1 (Forensics) as DNA validation

**Example Checklist Items**:
- [ ] No exceptions thrown (use Result<T, Error>)
- [ ] All state transitions explicit (no implicit state)
- [ ] Function names describe intent, not implementation
- [ ] No magic numbers (all constants named)
- [ ] Complexity ≤15 (cognitive simplicity)

---

## Part 2: Source Code Context Skill Integration

### Core Patterns to Adopt

#### Pattern 1: **Opening Move Protocol** (`plan_turn`)
**What**: Session-aware routing based on confidence level

**Current Problem**: Agents often read too many files or not enough files

**Solution**:
```
plan_turn(repo, query, model) → confidence + recommended files
├─ High → Direct to symbols (max 2 supplementary)
├─ Medium → Explore files (max 5 supplementary)
└─ Low → Report "doesn't exist", don't search further
```

**Integration Points**:

1. **epic-run Phase 0** (Hotspot Analysis):
```diff
+ 1. Run: mise run plan-turn --query="$2"
+ 2. If confidence=LOW: Report "Feature doesn't exist" and HALT
+ 3. If confidence=HIGH: Proceed directly to recommended files
  4. Run: mise run hotspots
```

2. **v12-engineer mode** (Before any code read):
```diff
+ 1. Run: plan_turn(repo=".", query="$TASK", model="claude-sonnet-4-6")
+ 2. Read ONLY recommended files (no exploratory reads)
  3. Proceed with implementation
```

**Benefits**:
- 71% fewer tokens (per jcodemunch docs)
- No wasted reads on non-existent features
- Faster task completion

---

#### Pattern 2: **Negative Evidence Tracking**
**What**: Log failed searches to avoid repeating them

**Current Problem**: Agents search for the same non-existent code multiple times

**Solution**:
```
search_symbols(query) → no results
→ Log to docs/brain/negative_evidence.md
→ Never search again for same query
```

**Integration Points**:

1. **pr-loop Step 1.5** (NEW - Negative Evidence Check):
```
Before fixing issues:
1. Check docs/brain/negative_evidence.md
2. If issue mentions non-existent code: Mark as HALLUCINATION
3. Skip fix attempt
```

2. **v12-epic-planner** (Before planning):
```
Before creating implementation plan:
1. Check negative_evidence.md for related features
2. If feature doesn't exist: Flag in plan as "NEW FEATURE" (not refactor)
```

**File Format** (`docs/brain/negative_evidence.md`):
```markdown
# Negative Evidence Log

## 2026-06-03
- ❌ CSRF protection middleware (searched: "csrf", "xsrf", "token validation")
- ❌ Rate limiting (searched: "rate limit", "throttle", "request limit")

## 2026-06-02
- ❌ Caching layer (searched: "cache", "redis", "memcached")
```

---

#### Pattern 3: **Session Snapshot Recovery**
**What**: ~200 token summary for context continuity after compaction

**Current Problem**: After long sessions, agents lose context and re-read files

**Solution**:
```
get_session_snapshot() → ~200 token summary
├─ Focus files (top 10)
├─ Key searches (top 5)
├─ Edited files (all)
└─ Dead ends (failed searches)
```

**Integration Points**:

1. **epic-run Phase 5.5** (NEW - Between Tickets):
```
After completing ticket-XX:
1. Run: get_session_snapshot()
2. Save to: docs/brain/$EPIC/session-snapshot-XX.md
3. Next ticket reads snapshot before starting
```

2. **pr-loop Step 3.5** (NEW - After Push):
```
After pushing fixes:
1. Run: get_session_snapshot()
2. Save to: docs/brain/pr_$PR_session.md
3. If PHS < 100 and retry needed: Read snapshot first
```

**Benefits**:
- Faster context restoration (200 tokens vs 5000+ tokens)
- No duplicate file reads
- Maintains continuity across long sessions

---

#### Pattern 4: **Budget-Aware Exploration**
**What**: Stop exploring when token budget is exhausted

**Current Problem**: Agents sometimes exceed context limits

**Solution**:
```
IF _meta.budget_warning:
  STOP exploring
  Work with what you have
```

**Integration Points**:

1. **v12-epic-planner** (During analysis):
```
While analyzing codebase:
1. Check _meta.budget_warning after each search
2. If warning: Stop analysis, work with current findings
3. Document in plan: "Analysis limited by budget"
```

2. **v12-engineer** (During implementation):
```
While reading context:
1. Check _meta.budget_warning after each file read
2. If warning: Proceed with implementation using current context
3. Flag in commit message: "Limited context due to budget"
```

---

#### Pattern 5: **Post-Edit Cache Invalidation**
**What**: Invalidate jcodemunch caches after editing files

**Current Problem**: Stale search results after code changes

**Solution**:
```
After editing files:
  register_edit(files) → invalidate jcodemunch caches
```

**Integration Points**:

1. **epic-tdd Step 4** (After Commit):
```diff
  1. mise run commit-tdd --epic=$1 --ticket=$2
  2. git push
+ 3. Run: register_edit(files=<edited_files>)
```

2. **pr-loop Step 2** (After Local Repair):
```diff
  PART C: Validation
  3. Run formatters: mise run format
  4. Run FULL local validation: mise run validate
+ 5. Run: register_edit(files=<fixed_files>)
  6. If ALL checks pass: emit [LOCAL-READY]
```

---

## Part 3: Slash Command Integration Plan

### Priority 1: epic-run Enhancements

**Phase 0 (Hotspot Analysis) - ADD**:
```diff
+ Step 0.5: Plan Turn (Confidence Check)
+ **Switch to: Advanced mode**
+ 
+ Hand off:
+ ```
+ TASK: Run Plan Turn for Epic
+ PROTOCOL:
+   1. Run: plan_turn(repo=".", query="$2", model="claude-sonnet-4-6")
+   2. If confidence=LOW:
+      - Emit: [CONFIDENCE-LOW] Feature likely doesn't exist
+      - Ask Director: "Confidence is LOW. Proceed anyway? (YES/NO)"
+   3. If confidence=MEDIUM:
+      - Emit: [CONFIDENCE-MEDIUM] Explore recommended files
+      - Proceed to hotspot analysis
+   4. If confidence=HIGH:
+      - Emit: [CONFIDENCE-HIGH] Direct to recommended symbols
+      - Skip hotspot analysis, use recommended files
+ ```
```

**Phase 5.5 (Between Tickets) - ADD**:
```diff
+ Step 5.5: Session Snapshot (Context Continuity)
+ **Switch to: Advanced mode**
+ 
+ Hand off:
+ ```
+ TASK: Create Session Snapshot
+ PROTOCOL:
+   1. Run: get_session_snapshot(max_files=10, max_searches=5)
+   2. Save to: docs/brain/$EPIC/session-snapshot-XX.md
+   3. Emit: [SNAPSHOT-SAVED] Ready for next ticket
+ ```
```

---

### Priority 2: pr-loop Enhancements

**Step 1.5 (Negative Evidence Check) - ADD**:
```diff
+ Step 1.5: Negative Evidence Check (NEW)
+ **Switch to: Advanced mode**
+ 
+ Hand off:
+ ```
+ TASK: Check Negative Evidence Before Fixes
+ PR: $1
+ PROTOCOL:
+   1. Read: docs/brain/negative_evidence.md
+   2. Read: docs/brain/pr_$1_fix_queue.md
+   3. For each VALID-FIX issue:
+      - Check if issue mentions code in negative_evidence.md
+      - If YES: Reclassify as HALLUCINATION
+      - Update fix_queue.md
+   4. Emit: [NEGATIVE-EVIDENCE-CHECK] X issues reclassified
+ ```
```

**Step 2 (Local Repair) - ENHANCE**:
```diff
  PART C: Validation
  3. Run formatters: mise run format
  4. Run FULL local validation: mise run validate
+ 5. Run: register_edit(files=<fixed_files>)
  6. If ALL checks pass: emit [LOCAL-READY]
```

**Step 3.5 (Session Snapshot) - ADD**:
```diff
+ Step 3.5: Session Snapshot (After Push)
+ **Switch to: Advanced mode**
+ 
+ Hand off:
+ ```
+ TASK: Save Session State
+ PR: $1
+ PROTOCOL:
+   1. Run: get_session_snapshot()
+   2. Save to: docs/brain/pr_$1_session.md
+   3. If PHS < 100: Next iteration reads snapshot first
+   4. Emit: [SESSION-SAVED]
+ ```
```

---

### Priority 3: epic-tdd Enhancements

**Step 2 (Local Verification) - ENHANCE**:
```diff
  1. mise run sync
  2. mise run validate
+ 3. Run: register_edit(files=<edited_files>)
```

**Step 4 (Commit & Push) - ENHANCE**:
```diff
  1. mise run commit-tdd --epic=$1 --ticket=$2
  2. git push
+ 3. Run: register_edit(files=<committed_files>)
```

---

## Part 4: New Mise Tasks

Add to `.mise.toml`:

```toml
# === JANE STREET INTEGRATION ===

[tasks.index-jane-street]
description = "Index a Jane Street repository for analysis"
run = """
jcodemunch index-repo --url=https://github.com/janestreet/$REPO
"""

[tasks.benchmark]
description = "Run performance benchmarks (Jane Street style)"
run = """
dotnet run --project benchmarks/V12_Performance.Benchmarks.csproj --configuration Release
python scripts/benchmark_analysis.py
"""

[tasks.benchmark-regression]
description = "Check for performance regressions vs main"
run = """
python scripts/benchmark_regression.ps1
"""

# === SOURCE CODE CONTEXT INTEGRATION ===

[tasks.plan-turn]
description = "Run plan_turn for session-aware routing"
run = """
# Requires jcodemunch-mcp
# Usage: mise run plan-turn --query="task description"
"""

[tasks.session-snapshot]
description = "Create session snapshot for context continuity"
run = """
# Requires jcodemunch-mcp
# Usage: mise run session-snapshot --output=docs/brain/snapshot.md
"""

[tasks.register-edit]
description = "Invalidate jcodemunch caches after editing files"
run = """
# Requires jcodemunch-mcp
# Usage: mise run register-edit --files="file1.cs,file2.cs"
"""

[tasks.negative-evidence-check]
description = "Check if issue mentions non-existent code"
run = """
python scripts/negative_evidence_check.py --issue="$ISSUE"
"""
```

---

## Part 5: New Scripts to Create

### 1. `scripts/benchmark_regression.ps1`
**Purpose**: Detect performance regressions using Jane Street-style statistical analysis

**Features**:
- Run benchmarks on current branch
- Compare to baseline (main branch)
- Calculate p50, p99, variance
- Report regressions > 5%
- Document improvements > 10%

**Integration**: pr-loop Step 2.5 (warning-only)

---

### 2. `scripts/negative_evidence_check.py`
**Purpose**: Check if bot issues mention non-existent code

**Features**:
- Read `docs/brain/negative_evidence.md`
- Parse bot issue descriptions
- Match against negative evidence log
- Reclassify as HALLUCINATION if match found

**Integration**: pr-loop Step 1.5

---

### 3. `scripts/jane_street_validator.py`
**Purpose**: Validate code against Jane Street patterns

**Features**:
- Check for exceptions (should use Result<T, Error>)
- Check for implicit state transitions
- Check for magic numbers
- Check for complexity > 15
- Generate checklist report

**Integration**: pr-loop Step 2 (validation)

---

## Part 6: Documentation to Create

### 1. `docs/standards/JANE_STREET_CHECKLIST.md`
**Purpose**: DNA validation checklist based on Jane Street patterns

**Sections**:
- Error Handling (no exceptions)
- State Management (explicit transitions)
- Naming Conventions (intent over implementation)
- Type Safety (make illegal states unrepresentable)
- Complexity (≤15 per function)

---

### 2. `docs/standards/JANE_STREET_ASYNC_PATTERNS.md`
**Purpose**: Document Jane Street's async patterns for V12 validation

**Sections**:
- Timeout handling
- Cancellation patterns
- Error propagation
- Testing strategies
- Race condition prevention

---

### 3. `docs/brain/negative_evidence.md`
**Purpose**: Log of failed searches to avoid repetition

**Format**:
```markdown
# Negative Evidence Log

## 2026-06-03
- ❌ Feature X (searched: "query1", "query2")
- ❌ Feature Y (searched: "query3")
```

---

## Part 7: Implementation Roadmap

### Phase 1: Quick Wins (This Week)
**Goal**: Adopt Source Code Context patterns

1. ✅ Add `plan_turn` to epic-run Phase 0
2. ✅ Add `register_edit` to epic-tdd Step 4
3. ✅ Add `get_session_snapshot` to epic-run Phase 5.5
4. ✅ Create `docs/brain/negative_evidence.md`
5. ✅ Add negative evidence check to pr-loop Step 1.5

**Estimated Effort**: 4 hours
**Impact**: 30% fewer wasted file reads

---

### Phase 2: Jane Street Exploration (Next Week)
**Goal**: Index and analyze Jane Street repos

1. Index `core_bench` with jcodemunch
2. Index `async` with jcodemunch
3. Index `patience_diff` with jcodemunch
4. Create `docs/standards/JANE_STREET_CHECKLIST.md`
5. Create `docs/standards/JANE_STREET_ASYNC_PATTERNS.md`

**Estimated Effort**: 8 hours
**Impact**: Validation framework for V12 DNA

---

### Phase 3: Performance Integration (2 Weeks)
**Goal**: Add Jane Street-style benchmarking

1. Create `scripts/benchmark_regression.ps1`
2. Add to pr-loop as Step 2.5 (warning-only)
3. Test on EPIC-CCN-14 through EPIC-CCN-22
4. Promote to blocking check if reliable

**Estimated Effort**: 12 hours
**Impact**: Catch performance regressions early

---

### Phase 4: Advanced Integration (1 Month)
**Goal**: Full workflow integration

1. Implement budget-aware exploration in v12-epic-planner
2. Implement session continuity across long epics
3. Create `scripts/jane_street_validator.py`
4. Add Jane Street validation to pr-loop Step 2

**Estimated Effort**: 20 hours
**Impact**: Production-grade quality gates

---

## Part 8: Success Metrics

### Quantitative
- **Token Efficiency**: 30% reduction in file reads (via plan_turn)
- **False Positives**: 50% reduction in hallucination fixes (via negative evidence)
- **Context Continuity**: 80% reduction in duplicate reads (via session snapshots)
- **Performance Regressions**: 0 regressions shipped (via benchmark checks)

### Qualitative
- **V12 DNA Alignment**: All code passes Jane Street checklist
- **Agent Efficiency**: Agents complete tasks faster with fewer retries
- **Code Quality**: Complexity stays ≤15, no exceptions, explicit state
- **Developer Experience**: Fewer false alarms, clearer feedback

---

## Part 9: Risks & Mitigations

### Risk 1: jcodemunch-mcp Dependency
**Risk**: Heavy reliance on jcodemunch-mcp for new patterns
**Mitigation**: 
- Keep fallback to native tools (grep, read_file)
- Document jcodemunch-mcp setup in MISE_SETUP.md
- Test all workflows with and without jcodemunch

### Risk 2: Jane Street Patterns Not Directly Applicable
**Risk**: OCaml patterns may not translate to C#
**Mitigation**:
- Focus on principles, not syntax
- Adapt patterns to C# idioms
- Validate with Director before enforcing

### Risk 3: Performance Overhead
**Risk**: Benchmark checks may slow down pr-loop
**Mitigation**:
- Start as warning-only (Step 2.5)
- Optimize benchmark suite for speed
- Only run on src/ changes, skip docs/scripts

### Risk 4: Negative Evidence False Negatives
**Risk**: May miss legitimate features due to overly broad negative evidence
**Mitigation**:
- Require exact query match (not substring)
- Expire entries after 30 days
- Allow Director override

---

## Part 10: Next Steps (Brainstorming Questions)

### For Director Review:

1. **Priority Order**: Do you agree with Phase 1 → Phase 2 → Phase 3 → Phase 4?
2. **Jane Street Repos**: Should we index all 3 (core_bench, async, patience_diff) or start with just core_bench?
3. **Performance Checks**: Should benchmark regression be warning-only or blocking from day 1?
4. **Negative Evidence**: Should we auto-expire entries after 30 days or keep forever?
5. **Session Snapshots**: Should we save snapshots after every ticket or only on long epics (>5 tickets)?
6. **Budget Limits**: What token budget should trigger "stop exploring" warning? (Current: none)
7. **Integration Scope**: Should we integrate into ALL slash commands or just epic-run + pr-loop?

### Technical Questions:

1. **jcodemunch Setup**: Do we need to document jcodemunch-mcp installation in MISE_SETUP.md?
2. **Benchmark Baseline**: Should we store baseline benchmarks in git or regenerate from main?
3. **Negative Evidence Format**: Markdown or JSON for easier parsing?
4. **Session Snapshot Storage**: One file per epic or one file per ticket?
5. **Jane Street Validation**: Should it be a separate check or integrated into existing lint check?

---

## Summary

### What We're Integrating:
1. **Jane Street Patterns**: Correctness by construction, micro-benchmarking, async patterns
2. **Source Code Context**: Session-aware routing, negative evidence, budget awareness

### Where We're Integrating:
1. **epic-run**: plan_turn (Phase 0), session snapshots (Phase 5.5)
2. **pr-loop**: negative evidence (Step 1.5), register_edit (Step 2), session snapshots (Step 3.5)
3. **epic-tdd**: register_edit (Step 2, Step 4)

### Why We're Integrating:
1. **Efficiency**: 30% fewer wasted file reads
2. **Quality**: Jane Street-grade validation
3. **Reliability**: Catch regressions early
4. **Continuity**: Maintain context across long sessions

### When We're Integrating:
- **Phase 1** (This Week): Source Code Context patterns
- **Phase 2** (Next Week): Jane Street exploration
- **Phase 3** (2 Weeks): Performance integration
- **Phase 4** (1 Month): Advanced integration

---

**Status**: READY FOR DIRECTOR REVIEW  
**Next Action**: Discuss priorities and answer brainstorming questions