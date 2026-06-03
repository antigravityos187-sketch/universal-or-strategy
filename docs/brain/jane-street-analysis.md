# Jane Street Repository Analysis + PR Comment Extraction

## Part 1: Jane Street Repos - Relevance to V12

### High-Value Repos (Direct Applicability)

#### 1. **core_bench** (56 stars) - HIGHEST PRIORITY
**Language**: OCaml
**Purpose**: Micro-benchmarking library
**V12 Use Case**: 
- Replace ad-hoc benchmarking in `benchmarks/V12_Performance.Benchmarks.csproj`
- Learn statistical analysis patterns (p50, p99, variance)
- Integrate into pr-loop Step 2.5 (Performance Regression Check)

**Why Useful**: 
- Jane Street's production benchmarking tool
- Handles microsecond-latency measurements
- Statistical rigor (not just "run 3 times and average")

---

#### 2. **async** (256 stars) - HIGH PRIORITY
**Language**: OCaml
**Purpose**: Asynchronous execution library
**V12 Use Case**:
- Compare to V12's Actor/FSM pattern
- Validate our `Enqueue` model aligns with Jane Street's async primitives
- Extract error handling patterns

**Why Useful**:
- Jane Street's core concurrency primitive
- Similar to V12's lock-free Actor pattern
- Production-tested at HFT scale

---

#### 3. **patience_diff** (23 stars) - MEDIUM PRIORITY
**Language**: OCaml
**Purpose**: Patience diff algorithm implementation
**V12 Use Case**:
- Improve `apply_diff` tool accuracy
- Better conflict resolution in pr-loop
- Cleaner diff generation for PRs

**Why Useful**:
- Better than standard diff for code (preserves structure)
- Used in patdiff (250 stars)
- Could improve Bob CLI's surgical edits

---

#### 4. **hardcaml** (1049 stars) - LOW PRIORITY (Conceptual)
**Language**: OCaml
**Purpose**: Hardware design in OCaml
**V12 Use Case**:
- **Conceptual only**: "Make illegal states unrepresentable" at hardware level
- FSM design patterns
- Type-safe state machines

**Why Useful**:
- Extreme example of "correctness by construction"
- FSM patterns applicable to V12's Actor model
- Not directly usable (hardware focus), but philosophically aligned

---

#### 5. **core** (1285 stars) - MEDIUM PRIORITY (Reference)
**Language**: OCaml
**Purpose**: Standard library replacement
**V12 Use Case**:
- Study Jane Street's coding standards
- Extract naming conventions
- Understand their "no exceptions" philosophy

**Why Useful**:
- Jane Street's DNA in code form
- Shows how they enforce "make illegal states unrepresentable"
- Reference for V12 DNA alignment

---

### Medium-Value Repos (Indirect Applicability)

#### 6. **sexplib** (176 stars)
**Purpose**: S-expression serialization
**V12 Use Case**: Structured logging patterns (alternative to JSON)

#### 7. **re2** (47 stars)
**Purpose**: OCaml bindings for Google's RE2 regex engine
**V12 Use Case**: Fast regex for `search_files` tool

#### 8. **rpc_parallel** (50 stars)
**Purpose**: Type-safe parallel RPC
**V12 Use Case**: Multi-agent coordination patterns

---

### Low-Value Repos (Not Applicable)

- **learn-ocaml-workshop** (701 stars): Educational, not production code
- **install-ocaml** (97 stars): Setup instructions
- **jenga** (90 stars): Build system (we use dotnet)
- **ocaml_plugin** (59 stars): Archived

---

## Part 2: Source Code Context Skill Analysis

### What the Skill Offers

**Core Innovation**: Session-aware code navigation using jcodemunch-mcp.

**Key Patterns**:

1. **Opening Move Protocol**
```
plan_turn(repo, query, model) → confidence + recommended files
├─ High → Direct to symbols (max 2 supplementary)
├─ Medium → Explore files (max 5 supplementary)
└─ Low → Report "doesn't exist", don't search further
```

2. **Negative Evidence Tracking**
```
search_symbols(query) → no results
→ Log to negative_evidence.md
→ Never search again for same query
```

3. **Session Snapshot Recovery**
```
get_session_snapshot() → ~200 token summary
├─ Focus files (top 10)
├─ Key searches (top 5)
├─ Edited files (all)
└─ Dead ends (failed searches)
```

4. **Budget-Aware Exploration**
```
IF _meta.budget_warning:
  STOP exploring
  Work with what you have
```

5. **Post-Edit Cache Invalidation**
```
After editing files:
  register_edit(files) → invalidate jcodemunch caches
```

### What We Can Adopt

**Immediate Wins**:
1. ✅ Add `plan_turn` to epic-run Phase 0
2. ✅ Add `register_edit` to epic-tdd Step 4
3. ✅ Add `get_session_snapshot` to epic-run Phase 5.5

**Medium-Term**:
1. Negative evidence tracking in pr-loop
2. Budget-aware exploration in v12-epic-planner
3. Session continuity across long epics

---

## Part 3: GitHub PR Comment Extraction

### Can We Get 100% of PR Comments?

**Short Answer**: YES, via GitHub API (not crawler).

### GitHub API Coverage

#### 1. **Review Comments** (Code-level)
**Endpoint**: `GET /repos/{owner}/{repo}/pulls/{pull_number}/comments`
**Coverage**: 100% including:
- ✅ Line-level comments
- ✅ Dropdown/collapsed comments
- ✅ Resolved comments
- ✅ Outdated comments (from old commits)
- ✅ Suggested changes
- ✅ Comment threads (replies)

**Example**:
```powershell
gh api repos/backtothefutures83-oss/universal-or-strategy/pulls/23/comments
```

#### 2. **Issue Comments** (PR-level)
**Endpoint**: `GET /repos/{owner}/{repo}/issues/{issue_number}/comments`
**Coverage**: 100% including:
- ✅ Top-level PR comments
- ✅ Bot comments (CodeFactor, Codacy, CodeScene)
- ✅ Hidden/minimized comments

**Example**:
```powershell
gh api repos/backtothefutures83-oss/universal-or-strategy/issues/23/comments
```

#### 3. **Review Summaries**
**Endpoint**: `GET /repos/{owner}/{repo}/pulls/{pull_number}/reviews`
**Coverage**: 100% including:
- ✅ Approve/Request Changes/Comment reviews
- ✅ Review body text
- ✅ Submitted/Pending/Dismissed reviews

---

### Crawler vs. API

| Aspect | Crawler (Selenium/Puppeteer) | GitHub API |
|--------|------------------------------|------------|
| **Coverage** | 95% (may miss dynamic content) | 100% (authoritative source) |
| **Reliability** | Fragile (breaks on UI changes) | Stable (versioned API) |
| **Rate Limits** | IP-based (strict) | Token-based (5000/hour) |
| **Authentication** | Cookie-based (complex) | PAT token (simple) |
| **Dropdown Comments** | ❌ Requires JS execution | ✅ Included in API |
| **Resolved Comments** | ❌ Hidden by default | ✅ Included in API |
| **Performance** | Slow (browser overhead) | Fast (JSON response) |

**Verdict**: Use GitHub API, not crawler.

---

### Implementation: Enhanced `extract_pr_forensics.ps1`

**Current**: Only reads bot comments from PR description.
**Proposed**: Read ALL comments via GitHub API.

```powershell
# New function in extract_pr_forensics.ps1

function Get-AllPRComments {
    param([int]$PrNumber)
    
    # 1. Review comments (code-level)
    $reviewComments = gh api "repos/$env:GITHUB_REPOSITORY/pulls/$PrNumber/comments" | ConvertFrom-Json
    
    # 2. Issue comments (PR-level)
    $issueComments = gh api "repos/$env:GITHUB_REPOSITORY/issues/$PrNumber/comments" | ConvertFrom-Json
    
    # 3. Review summaries
    $reviews = gh api "repos/$env:GITHUB_REPOSITORY/pulls/$PrNumber/reviews" | ConvertFrom-Json
    
    # Merge and categorize
    $allComments = @()
    
    foreach ($comment in $reviewComments) {
        $allComments += @{
            Type = "ReviewComment"
            Author = $comment.user.login
            Body = $comment.body
            Path = $comment.path
            Line = $comment.line
            CreatedAt = $comment.created_at
            IsResolved = $comment.in_reply_to_id -ne $null
        }
    }
    
    foreach ($comment in $issueComments) {
        $allComments += @{
            Type = "IssueComment"
            Author = $comment.user.login
            Body = $comment.body
            CreatedAt = $comment.created_at
        }
    }
    
    foreach ($review in $reviews) {
        $allComments += @{
            Type = "Review"
            Author = $review.user.login
            State = $review.state  # APPROVED, CHANGES_REQUESTED, COMMENTED
            Body = $review.body
            CreatedAt = $review.submitted_at
        }
    }
    
    return $allComments
}
```

---

### Use Cases for Full Comment Extraction

#### 1. **pr-loop Step 1: Bot Forensics**
**Current**: Manually read screenshots
**Proposed**: Auto-extract all bot comments

```powershell
$comments = Get-AllPRComments -PrNumber 23
$botComments = $comments | Where-Object { $_.Author -in @("codefactor-io", "codacy", "codescene") }

foreach ($comment in $botComments) {
    # Parse comment body for issues
    # Categorize as VALID/HALLUCINATION/INFRA-NOISE
    # Write to pr_23_forensics.md
}
```

#### 2. **Director Feedback Tracking**
**Use Case**: Track which issues Director approved/rejected

```powershell
$directorComments = $comments | Where-Object { $_.Author -eq "malhitticrypto-debug" }
# Extract: "Approved", "Fix this first", "Skip this"
```

#### 3. **Hallucination Log**
**Use Case**: Track bot comments that were false positives

```powershell
$resolvedComments = $comments | Where-Object { $_.IsResolved -eq $true }
# If resolved without code change → likely hallucination
```

---

## Part 4: Recommended Integration Priority

### Phase 1: Quick Wins (This Week)
1. ✅ **Adopt Source Code Context patterns**:
   - Add `plan_turn` to epic-run Phase 0
   - Add `register_edit` to epic-tdd Step 4
   - Add `get_session_snapshot` to epic-run Phase 5.5

2. ✅ **Enhance PR comment extraction**:
   - Update `extract_pr_forensics.ps1` to use GitHub API
   - Extract ALL comments (not just bot comments)
   - Test on PR #25

### Phase 2: Jane Street Exploration (Next Week)
1. **Index core_bench**: Learn micro-benchmarking patterns
2. **Index async**: Compare to V12 Actor model
3. **Index patience_diff**: Improve apply_diff tool

### Phase 3: Advanced Integration (2 Weeks)
1. Implement Performance Regression Check (pr-loop Step 2.5)
2. Implement Negative Evidence Tracking (pr-loop Step 1.5)
3. Implement Incremental Validation (epic-tdd Step 2.5)

---

## Summary

### Jane Street Repos - Top 3
1. **core_bench**: Micro-benchmarking (direct use)
2. **async**: Async patterns (validation)
3. **patience_diff**: Better diffs (tool improvement)

### Source Code Context Skill - Top 3 Patterns
1. **plan_turn**: Session-aware routing
2. **register_edit**: Cache invalidation
3. **get_session_snapshot**: Context recovery

### GitHub PR Comments - Answer
**YES**, 100% extraction via GitHub API (not crawler).
- ✅ Dropdown comments
- ✅ Resolved comments
- ✅ Bot comments
- ✅ Review summaries

**Next Step**: Enhance `extract_pr_forensics.ps1` to use GitHub API.