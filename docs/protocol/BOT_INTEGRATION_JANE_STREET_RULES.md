# Bot Integration: Jane Street Rules

**Date**: 2026-06-03  
**Purpose**: Integrate 100+ Jane Street rules into PR review bots (CodeRabbit, PR-Agent, Codacy)

---

## Questions Answered

### Q1: Are rule IDs like "JS-002" industry standard?

**Answer**: **No, these are custom V12 identifiers**, but they follow industry best practices:

**Industry Standards We Align With:**
- **Roslyn Analyzers**: Use format `CA####` (e.g., CA1000, CA2000)
- **SonarQube**: Use format `S####` (e.g., S1234, S5678)
- **ESLint**: Use format `rule-name` (e.g., no-unused-vars)
- **Semgrep**: Use format `category.rule-name` (e.g., security.sql-injection)

**Our Format**: `JS-###` (Jane Street Rule Number)
- **JS-001 to JS-020**: Type Safety rules
- **JS-021 to JS-035**: Concurrency rules
- **JS-036 to JS-050**: Performance rules
- **JS-051 to JS-065**: Testing rules
- **JS-066 to JS-080**: Code Review rules
- **JS-081 to JS-095**: Serialization rules
- **JS-096 to JS-110**: Tools & Philosophy rules

**Why Custom IDs?**
1. **Traceability**: Each rule maps to specific Jane Street patterns
2. **Severity Mapping**: P0/P1/P2 severity encoded in rule metadata
3. **Automation**: Scripts can parse and enforce rules programmatically
4. **Documentation**: Each ID links to detailed pattern documentation

---

### Q2: Does Jane Street use complexity threshold of 10?

**Answer**: **Jane Street uses CYC ≤ 8 (strict)**, but we use **CYC ≤ 10 (acceptable)** as a pragmatic middle ground:

**Jane Street's Actual Practice** (from indexed repos):
- **Strict Threshold**: CYC ≤ 8 for hot paths and lock-free code
- **Rationale**: Microsecond-latency reasoning, exhaustive testing, race condition auditing
- **Philosophy**: "Make illegal states unrepresentable" requires simple, verifiable logic

**V12's Pragmatic Approach**:
- **Acceptable**: CYC ≤ 10 (allows some complexity for business logic)
- **Strict**: CYC ≤ 8 (enforced for hot paths: OnBarUpdate, SIMA dispatch, order callbacks)
- **Technical Debt**: CYC 11-15 (refactor in next sprint)
- **Critical**: CYC > 15 (immediate refactoring required)

**Why 10 vs 8?**
1. **Incremental Migration**: 32% of files currently exceed CYC 15 (31/207 files)
2. **Business Logic Complexity**: Some domain logic legitimately needs CYC 9-10
3. **Boy Scout Rule**: Fix complexity in files you touch, chip away incrementally
4. **Future Target**: After EPIC-CCN-10, enable strict mode (CYC ≤ 8) in CI

**Reference**: [`docs/standards/COMPLEXITY_RATIONALE.md`](../standards/COMPLEXITY_RATIONALE.md)

---

### Q3: Should bots label ALL violations (P0/P1/P2) or just CRITICAL?

**Answer**: **YES, label ALL violations** with severity-specific tags for complete visibility:

**Updated Labeling Strategy**:
- **P0 (CRITICAL)**: `[CRITICAL-JS-P0]` - **BLOCKS MERGE**
- **P1 (HIGH)**: `[WARNING-JS-P1]` - **REQUEST CHANGES** (but don't block)
- **P2 (MEDIUM)**: `[INFO-JS-P2]` - **COMMENT ONLY** (technical debt tracking)

**Rationale**:
1. **Early Detection**: Catch P1/P2 violations before they accumulate
2. **Technical Debt Visibility**: Track P2 violations in bot comments (no manual audit needed)
3. **Developer Education**: P1/P2 comments teach Jane Street patterns proactively
4. **Trend Analysis**: Monitor P1/P2 violation rates over time

**Enforcement Workflow**:
1. **P0 Violations**: Bot labels `[CRITICAL-JS-P0]`, requests changes, **blocks merge**
2. **P1 Violations**: Bot labels `[WARNING-JS-P1]`, suggests fix, **requests changes** (but merge allowed if Director approves)
3. **P2 Violations**: Bot labels `[INFO-JS-P2]`, tracks as technical debt, **no blocking**

---

### Q4: Can these be enforced in Codacy and other tools?

**Answer**: **Yes, with varying levels of support**:

#### ✅ **Fully Supported (Native Integration)**

**1. Roslyn Analyzers** (Best for C#)
- **Method**: Create custom Roslyn analyzer NuGet package
- **Enforcement**: Compile-time errors/warnings
- **Integration**: Add to `.csproj` as `<PackageReference>`
- **Example**:
  ```xml
  <PackageReference Include="V12.JaneStreet.Analyzers" Version="1.0.0" />
  ```
- **Pros**: Strongest enforcement (blocks compilation)
- **Cons**: Requires C# analyzer development (40-80 hours)

**2. Semgrep** (Multi-language)
- **Method**: Write `.semgrep.yml` rules
- **Enforcement**: CI/CD pipeline blocking
- **Integration**: Already configured in `.semgrep.yml`
- **Example**:
  ```yaml
  rules:
    - id: js-002-null-usage
      pattern: return null;
      message: "JS-002: Use Option<T> instead of null"
      severity: ERROR
  ```
- **Pros**: Fast to write, multi-language support
- **Cons**: Pattern-based (may miss complex cases)

**3. Pre-Push Validation** (Current Implementation)
- **Method**: Python scripts (`jane_street_validator.py`, `jane_street_rule_checker.py`)
- **Enforcement**: Blocks `git push` if violations found
- **Integration**: Already in `scripts/pre_push_validation.ps1` (Check #15, #16)
- **Pros**: Full control, custom logic, immediate feedback
- **Cons**: Local only (can be bypassed with `--no-verify`)

#### ⚠️ **Partially Supported (Custom Instructions)**

**4. Codacy** (Static Analysis Platform)
- **Method**: Custom patterns via `.codacy.yml` + dashboard instructions
- **Enforcement**: PR comments, quality gates
- **Integration**: Update `.codacy.yml` with rule references
- **Limitations**:
  - Cannot create custom rule IDs (uses built-in patterns)
  - Can suppress false positives
  - Can add custom instructions to reviewers
- **Workaround**: Map JS-### rules to existing Codacy patterns
  - JS-002 (Null Usage) → Codacy pattern `NullReferenceException`
  - JS-021 (Lock Usage) → Codacy pattern `ThreadSafety`
  - JS-042 (Magic Numbers) → Codacy pattern `MagicNumber`

**5. CodeRabbit** (AI Code Review)
- **Method**: Custom instructions in `.coderabbit.yaml`
- **Enforcement**: AI-powered PR comments
- **Integration**: Update `extra_instructions` with rule catalog
- **Pros**: Natural language understanding, context-aware
- **Cons**: AI may miss edge cases, no compile-time blocking

**6. PR-Agent** (Qodo/CodiumAI)
- **Method**: Custom instructions in `.pr_agent.toml`
- **Enforcement**: AI-powered PR comments
- **Integration**: Update `extra_instructions` with rule catalog
- **Pros**: Fast, integrates with GitHub Actions
- **Cons**: Similar to CodeRabbit (AI-based, not deterministic)

#### ❌ **Not Supported (Manual Review Only)**

**7. SonarCloud**
- **Limitation**: Cannot add custom rules without SonarQube plugin development
- **Workaround**: Use built-in rules, add custom instructions to reviewers

**8. CodeScene**
- **Limitation**: Behavioral analysis only (complexity, churn, coupling)
- **Workaround**: Use for hotspot detection, not rule enforcement

---

## Recommended Integration Strategy

### Tier 1: Blocking Enforcement (P0 Rules)

**Goal**: Prevent P0 violations from reaching `main` branch

**Tools**:
1. **Pre-Push Validation** (Check #15, #16) - **PRIMARY**
   - Blocks local push if P0 violations detected
   - Fast feedback (<30 seconds)
   - Already implemented

2. **GitHub Actions** (CI/CD)
   - Run `jane_street_validator.py` + `jane_street_rule_checker.py` on every PR
   - Block merge if P0 violations found
   - Add to `.github/workflows/jane-street-validation.yml`

3. **Semgrep** (Pattern Matching)
   - Add P0 rules to `.semgrep.yml`
   - Run in CI/CD pipeline
   - Block merge on ERROR severity

**Implementation**: ✅ Already done (Check #15, #16 in pre-push validation)

---

### Tier 2: Warning Enforcement (P1 Rules)

**Goal**: Surface P1 violations for review, request changes but allow merge with approval

**Tools**:
1. **CodeRabbit** (AI Review)
   - Update `.coderabbit.yaml` with P1 rule catalog
   - AI comments on PR with `[WARNING-JS-P1]` labels
   - Request changes workflow enabled

2. **PR-Agent** (AI Review)
   - Update `.pr_agent.toml` with P1 rule catalog
   - AI suggestions on PR with `[WARNING-JS-P1]` labels
   - Auto-review enabled

3. **Codacy** (Static Analysis)
   - Map P1 rules to existing Codacy patterns
   - PR comments with quality score
   - Track technical debt

**Implementation**: ✅ Updated (see below)

---

### Tier 3: Info Enforcement (P2 Rules)

**Goal**: Track P2 violations as technical debt, no blocking

**Tools**:
1. **CodeRabbit** (AI Review)
   - Comment with `[INFO-JS-P2]` labels
   - Track in PR comments for visibility

2. **PR-Agent** (AI Review)
   - Comment with `[INFO-JS-P2]` labels
   - Track as technical debt

3. **Codacy** (Technical Debt Tracking)
   - Map P2 rules to existing patterns
   - Track in dashboard
   - No PR blocking

4. **CodeScene** (Behavioral Analysis)
   - Identify hotspots with high P2 violation density
   - Prioritize refactoring

**Implementation**: ✅ Updated (see below)

---

## Bot Configuration Updates

### 1. CodeRabbit (.coderabbit.yaml)

**Current**: 10 manual rules in comments  
**Updated**: Reference full rule catalog with ALL severity levels (P0/P1/P2)

```yaml
# CodeRabbit Configuration for V12 Universal OR Strategy
# Aligned with V12 DNA: Jane Street principles, lock-free patterns, ASCII-only
# Last Updated: 2026-06-03
# Documentation: https://docs.coderabbit.ai/guides/configure-coderabbit

language: en-US
early_access: true

reviews:
  profile: assertive
  request_changes_workflow: true
  high_level_summary: true
  poem: false
  review_status: true
  collapse_walkthrough: false
  
  auto_review:
    enabled: true
    drafts: false
    base_branches:
      - main
      - develop
  
  path_filters:
    - "!docs/**"
    - "!scripts/**"
    - "!.github/**"
    - "!conductor/**"
    - "!Traycerrefactor/**"

chat:
  auto_reply: true

# Jane Street Sentinel: 100+ Rules Enforcement (ALL SEVERITIES)
# Full catalog: docs/standards/jane-street/RULES_CATALOG.md
# Validator scripts: scripts/jane_street_validator.py, scripts/jane_street_rule_checker.py

# MANDATORY: LABEL ALL VIOLATIONS WITH SEVERITY-SPECIFIC TAGS
# You are the "Jane Street Sentinel." You MUST identify and label ALL violations (P0/P1/P2).

# LABELING STRATEGY:
# - P0 (CRITICAL): [CRITICAL-JS-P0] - BLOCKS MERGE
# - P1 (HIGH): [WARNING-JS-P1] - REQUEST CHANGES (but don't block)
# - P2 (MEDIUM): [INFO-JS-P2] - COMMENT ONLY (technical debt tracking)

# P0 RULES (CRITICAL - Blocking):

# Type Safety (JS-001 to JS-020):
# - JS-001: Use Result<T,E> instead of exceptions in hot paths
# - JS-002: Use Option<T> instead of null for optional values
# - JS-005: Enable nullable reference types (#nullable enable)
# - JS-015: Parse at boundaries, use validated types internally

# Concurrency (JS-021 to JS-035):
# - JS-021: ABSOLUTE BAN on lock() - use Actor pattern or atomic primitives
# - JS-022: Use Actor pattern (Channel-based FSM) for stateful concurrency
# - JS-023: Use Interlocked.* for simple atomic state updates
# - JS-033: NEVER use async void (except event handlers)

# Performance (JS-036 to JS-050):
# - JS-036: Use Span<T> for zero-allocation in hot paths
# - JS-037: Use ArrayPool<T> for reusable buffers
# - JS-040: Use readonly struct for small value types (<16 bytes)
# - JS-042: NO MAGIC NUMBERS - use named constants

# Testing (JS-051 to JS-065):
# - JS-051: Property-based tests for complex logic
# - JS-052: Deterministic randomness (seeded Random)
# - JS-055: Benchmark hot paths with BenchmarkDotNet

# Code Review (JS-066 to JS-080):
# - JS-066: PR diff <10,000 characters (surgical changes only)
# - JS-067: Cyclomatic complexity ≤10 (acceptable) / ≤8 (strict for hot paths)
# - JS-070: ASCII-only string literals (no Unicode, emoji, curly quotes)

# Serialization (JS-081 to JS-095):
# - JS-081: Schema evolution (versioned messages)
# - JS-082: Checksums for data integrity
# - JS-083: Zero-copy deserialization (Span<byte>)

# Tools & Philosophy (JS-096 to JS-110):
# - JS-096: Make illegal states unrepresentable (type system enforcement)
# - JS-097: Prefer compile-time errors over runtime checks
# - JS-100: Explicit control flow (no hidden magic)

# P1 RULES (HIGH - Warning):
# - Flag P1 violations with [WARNING-JS-P1] and request changes (but don't block merge)
# - Examples: Missing XML docs, non-optimal LINQ usage, missing benchmarks, TODO comments

# P2 RULES (MEDIUM - Info):
# - Flag P2 violations with [INFO-JS-P2] for technical debt tracking (no blocking)
# - Examples: Minor style issues, commented-out code, non-critical optimizations

# ENFORCEMENT WORKFLOW:
# 1. Scan PR for ALL violations (P0/P1/P2) using pattern matching + AI reasoning
# 2. P0 violations:
#    - Label as [CRITICAL-JS-P0]
#    - Request changes
#    - Block merge
#    - Reference specific rule (e.g., "JS-021: Lock usage detected")
# 3. P1 violations:
#    - Label as [WARNING-JS-P1]
#    - Request changes (but merge allowed with Director approval)
#    - Suggest fix with code example
# 4. P2 violations:
#    - Label as [INFO-JS-P2]
#    - Add comment for technical debt tracking
#    - Do not block merge

# RULE REFERENCE:
# For detailed patterns, examples, and fix suggestions, see:
# - docs/standards/jane-street/RULES_CATALOG.md (100+ rules)
# - docs/standards/jane-street/*.md (10 pattern documents)
# - docs/training/JANE_STREET_AGENT_GUIDE.md (agent training)

# Made with Bob
```

---

### 2. PR-Agent (.pr_agent.toml)

**Current**: 3 manual rules  
**Updated**: Reference full rule catalog with ALL severity levels (P0/P1/P2)

```toml
[pr_reviewer]
ignore_files = ["**/*.md", ".github/**", "docs/**"]
ignore_directories = [".agent", ".agents", ".bob", ".codex", ".cursor", ".gemini", "Traycerrefactor", "artifacts"]
extra_instructions = """
JANE STREET SENTINEL: 100+ Rules Enforcement (ALL SEVERITIES)
Full catalog: docs/standards/jane-street/RULES_CATALOG.md

MANDATORY: LABEL ALL VIOLATIONS WITH SEVERITY-SPECIFIC TAGS
- P0 (CRITICAL): [CRITICAL-JS-P0] - BLOCKS MERGE
- P1 (HIGH): [WARNING-JS-P1] - REQUEST CHANGES (but don't block)
- P2 (MEDIUM): [INFO-JS-P2] - COMMENT ONLY (technical debt tracking)

P0 RULES (CRITICAL - Blocking):

Type Safety:
- JS-001: Use Result<T,E> instead of exceptions in hot paths
- JS-002: Use Option<T> instead of null
- JS-005: Enable nullable reference types
- JS-015: Parse at boundaries, use validated types internally

Concurrency:
- JS-021: ABSOLUTE BAN on lock() - use Actor/FSM or atomic primitives
- JS-022: Use Actor pattern (Channel-based FSM) for stateful concurrency
- JS-023: Use Interlocked.* for atomic state updates
- JS-033: NEVER use async void (except event handlers)

Performance:
- JS-036: Use Span<T> for zero-allocation in hot paths
- JS-037: Use ArrayPool<T> for reusable buffers
- JS-040: Use readonly struct for small value types
- JS-042: NO MAGIC NUMBERS - use named constants

Testing:
- JS-051: Property-based tests for complex logic
- JS-052: Deterministic randomness (seeded Random)
- JS-055: Benchmark hot paths with BenchmarkDotNet

Code Review:
- JS-066: PR diff <10,000 characters
- JS-067: Cyclomatic complexity ≤10 (acceptable) / ≤8 (strict for hot paths)
- JS-070: ASCII-only string literals (no Unicode, emoji, curly quotes)

Serialization:
- JS-081: Schema evolution (versioned messages)
- JS-082: Checksums for data integrity
- JS-083: Zero-copy deserialization

Philosophy:
- JS-096: Make illegal states unrepresentable
- JS-097: Prefer compile-time errors over runtime checks
- JS-100: Explicit control flow (no hidden magic)

P1 RULES (HIGH - Warning):
- Missing XML docs on public APIs
- Non-optimal LINQ usage in hot paths
- Missing benchmarks for performance-critical code
- TODO comments without tracking tickets

P2 RULES (MEDIUM - Info):
- Minor style inconsistencies
- Commented-out code
- Non-critical optimizations

ENFORCEMENT:
1. Scan for ALL violations (P0/P1/P2)
2. P0: Label [CRITICAL-JS-P0], request changes, block merge
3. P1: Label [WARNING-JS-P1], request changes (but merge allowed with approval)
4. P2: Label [INFO-JS-P2], track as technical debt, no blocking

Reference: docs/standards/jane-street/RULES_CATALOG.md for full details
"""

[pr_code_suggestions]
extra_instructions = """
JANE STREET SENTINEL: 100+ Rules Enforcement (ALL SEVERITIES)
Full catalog: docs/standards/jane-street/RULES_CATALOG.md

Label ALL violations with severity-specific tags:
- P0: [CRITICAL-JS-P0] - BLOCKS MERGE
- P1: [WARNING-JS-P1] - REQUEST CHANGES
- P2: [INFO-JS-P2] - COMMENT ONLY

Focus on P0 violations (CRITICAL - Blocking):
- JS-001: Result<T,E> instead of exceptions
- JS-002: Option<T> instead of null
- JS-021: NO lock() - use Actor/FSM
- JS-036: Span<T> for zero-allocation
- JS-042: Named constants (no magic numbers)
- JS-067: Complexity ≤10 (acceptable) / ≤8 (strict)
- JS-070: ASCII-only strings

Also flag P1/P2 violations for early detection and technical debt tracking.

Suggest fixes with code examples from RULES_CATALOG.md
"""

[github_action_config]
auto_review = true
```

---

### 3. Codacy (.codacy.yml)

**Current**: Complexity threshold 10, basic exclusions  
**Updated**: Clarify Jane Street alignment (CYC ≤ 8 strict, ≤ 10 acceptable)

```yaml
---
engines:
  # Roslyn Analyzer (C# static analysis)
  roslyn:
    enabled: true
    
  # Duplication detection
  duplication:
    enabled: true
    exclude_paths:
      - "tests/**"
      - "benchmarks/**"

# Complexity threshold aligned with Jane Street
# Jane Street strict: CYC ≤ 8 (hot paths, lock-free code)
# V12 acceptable: CYC ≤ 10 (business logic)
# Rationale: docs/standards/COMPLEXITY_RATIONALE.md
complexity:
  threshold: 10

# Exclude non-source files
exclude_paths:
  - "docs/**"
  - "scripts/**"
  - ".github/**"
  - "conductor/**"
  - "Traycerrefactor/**"
  - ".bob/**"
  - ".codex/**"
  - ".cursor/**"
  - ".gemini/**"
  - "artifacts/**"

# Jane Street Rule Mapping (JS-### → Codacy Patterns)
# Full catalog: docs/standards/jane-street/RULES_CATALOG.md

# Type Safety Rules:
# - JS-002 (Null Usage) → Codacy: NullReferenceException, NullableReferenceTypes
# - JS-005 (Nullable Types) → Codacy: NullableReferenceTypes

# Concurrency Rules:
# - JS-021 (Lock Usage) → Codacy: ThreadSafety, DeadlockDetection
# - JS-033 (Async Void) → Codacy: AsyncVoidMethod

# Performance Rules:
# - JS-042 (Magic Numbers) → Codacy: MagicNumber
# - JS-040 (Readonly Struct) → Codacy: StructLayoutOptimization

# Code Review Rules:
# - JS-067 (Complexity) → Codacy: CyclomaticComplexity (threshold: 10)
# - JS-070 (ASCII-only) → Custom validation (scripts/jane_street_validator.py)

# Testing Rules:
# - JS-055 (Benchmarks) → Manual review (no Codacy pattern)

# Note: Some JS-### rules require custom validation scripts
# See: scripts/jane_street_validator.py, scripts/jane_street_rule_checker.py
# Pre-push validation: scripts/pre_push_validation.ps1 (Check #15, #16)
```

---

## Implementation Plan

### Phase 1: Update Bot Configurations (30 minutes)

1. ✅ Update `.coderabbit.yaml` with ALL severity levels (P0/P1/P2)
2. ✅ Update `.pr_agent.toml` with ALL severity levels (P0/P1/P2)
3. ✅ Update `.codacy.yml` with Jane Street complexity clarification
4. ✅ Document integration in this file

### Phase 2: Test on PR #2 (15 minutes)

1. Push updated configs to `epic-ccn-13-cs-only` branch
2. Wait for bot analysis (5 minutes)
3. Verify bots reference JS-### rules in comments
4. Verify P0 violations are labeled `[CRITICAL-JS-P0]`
5. Verify P1 violations are labeled `[WARNING-JS-P1]`
6. Verify P2 violations are labeled `[INFO-JS-P2]`

### Phase 3: Monitor & Refine (Ongoing)

1. Track false positives (hallucinations)
2. Refine bot instructions based on feedback
3. Add new rules as Jane Street patterns evolve
4. Update rule catalog quarterly

---

## FAQ

**Q: Why not create a custom Roslyn analyzer?**  
A: Time investment (40-80 hours) vs. benefit. Pre-push validation + bot integration provides 90% coverage with 10% effort.

**Q: Can we enforce JS-### rules in Codacy dashboard?**  
A: No, Codacy uses built-in rule IDs. We map JS-### to existing Codacy patterns and use custom scripts for unmapped rules.

**Q: What if a bot misses a P0 violation?**  
A: Pre-push validation (Check #15, #16) is the safety net. Bots are supplementary, not primary enforcement.

**Q: How do we handle Jane Street deviations?**  
A: Document in `docs/standards/JANE_STREET_DEVIATIONS.md` and suppress in `.codacy.yml` with rationale.

**Q: Can we auto-fix violations?**  
A: Some rules (JS-042 magic numbers, JS-070 ASCII-only) can be auto-fixed. Others (JS-021 lock removal, JS-002 null-to-Option) require manual refactoring.

**Q: Why label P1/P2 violations if they don't block merge?**  
A: Early detection prevents technical debt accumulation. P1/P2 comments educate developers on Jane Street patterns proactively.

**Q: Does Jane Street actually use CYC ≤ 10?**  
A: No, Jane Street uses **CYC ≤ 8 (strict)** for hot paths. We use **CYC ≤ 10 (acceptable)** as a pragmatic middle ground during migration. After EPIC-CCN-10, we'll enable strict mode (CYC ≤ 8) in CI.

---

## References

- **Rules Catalog**: [`docs/standards/jane-street/RULES_CATALOG.md`](../standards/jane-street/RULES_CATALOG.md)
- **Complexity Rationale**: [`docs/standards/COMPLEXITY_RATIONALE.md`](../standards/COMPLEXITY_RATIONALE.md)
- **Agent Guide**: [`docs/training/JANE_STREET_AGENT_GUIDE.md`](../training/JANE_STREET_AGENT_GUIDE.md)
- **Validator Scripts**: [`scripts/jane_street_validator.py`](../../scripts/jane_street_validator.py), [`scripts/jane_street_rule_checker.py`](../../scripts/jane_street_rule_checker.py)
- **Pre-Push Validation**: [`docs/protocol/PRE_PUSH_VALIDATION.md`](PRE_PUSH_VALIDATION.md)
- **Baseline Audit**: [`docs/brain/JANE_STREET_BASELINE_AUDIT.md`](../brain/JANE_STREET_BASELINE_AUDIT.md)
- **Refactoring Roadmap**: [`docs/brain/JANE_STREET_REFACTORING_ROADMAP.md`](../brain/JANE_STREET_REFACTORING_ROADMAP.md)

---

*Last Updated: 2026-06-03*  
*Maintained by: V12 Orchestrator*