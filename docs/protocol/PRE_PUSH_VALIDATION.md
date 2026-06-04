# Pre-Push Validation Protocol

**Version**: V12.22 (Phase 4 - Jane Street Integration)  
**Last Updated**: 2026-06-03

## Overview

The Pre-Push Validation Suite runs 16 automated checks locally before every GitHub push to catch issues early and maintain V12 DNA standards.

## Usage

**Fast mode** (skip slow checks 9, 10, 12, 13, 15, 16):
```powershell
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

**Full mode** (all 16 checks):
```powershell
powershell -File .\scripts\pre_push_validation.ps1
```

**Skip specific checks**:
```powershell
powershell -File .\scripts\pre_push_validation.ps1 -SkipBuild -SkipTests
```

## Check Catalog

| # | Check | Tool | Threshold | Blocking? | Fast Mode |
|---|-------|------|-----------|-----------|-----------|
| 1 | ASCII-Only | PowerShell | Zero non-ASCII | ✅ YES | ✅ Run |
| 2 | Build | dotnet build | Zero errors | ✅ YES | ✅ Run |
| 3 | Unit Tests | dotnet test | 100% pass | ✅ YES | ✅ Run |
| 4 | Lint | Roslyn | Zero violations | ✅ YES | ✅ Run |
| 5 | Formatting | CSharpier | Zero issues | ✅ YES | ✅ Run |
| 6 | Security | Gitleaks + Snyk | Zero secrets | ⚠️ WARNING | ✅ Run |
| 7 | Markdown Links | verify_links.ps1 | Zero broken | ⚠️ WARNING | ✅ Run |
| 8 | PR Hygiene | verify_pr_hygiene.ps1 | Diff <10k | ✅ YES | ✅ Run |
| 9 | Complexity | complexity_audit.py | CYC ≤ 15 | ✅ YES | ❌ Skip |
| 10 | Dead Code | dead_code_scan.py | Zero dead methods | ⚠️ WARNING | ❌ Skip |
| 11 | Codacy Preview | query_codacy_issues.ps1 | Zero errors | ⚠️ WARNING | ✅ Run |
| 12 | Semgrep | semgrep CLI | Zero findings | ⚠️ WARNING | ❌ Skip |
| 13 | CodeRabbit AI | coderabbit CLI | Zero critical/high | ⚠️ WARNING* | ❌ Skip |
| 14 | CodeScene Delta | cs CLI | No degradation | ⚠️ WARNING | ✅ Run |
| 15 | Jane Street Patterns | jane_street_validator.py | Zero P0 | ✅ YES | ❌ Skip |
| 16 | Jane Street Rules | jane_street_rule_checker.py | Zero P0 | ✅ YES | ✅ Run |

**\*CodeRabbit**: WARNING mode during 2-week validation period (ends 2026-06-09). Will become BLOCKING after validation.

## Check Details

### 1. ASCII-Only Compliance (V12 DNA Mandate)

**Purpose**: Enforce ASCII-only strings (no Unicode/emoji)

**Implementation**:
```powershell
Get-ChildItem -Path "src" -Filter "*.cs" -Recurse | ForEach-Object {
    $content = [System.IO.File]::ReadAllBytes($_.FullName)
    foreach ($byte in $content) {
        if ($byte -gt 127) { # Non-ASCII detected }
    }
}
```

**Rationale**: Jane Street alignment - predictable byte-level behavior, no encoding surprises

---

### 2. Build Compilation

**Purpose**: Verify code compiles without errors

**Command**: `dotnet build Linting.csproj --nologo --verbosity quiet`

**Exit Code**: 0 = success, non-zero = failure

---

### 3. Unit Tests

**Purpose**: Ensure all tests pass

**Command**: `dotnet test Testing.csproj --no-build --nologo --verbosity quiet`

**Current Coverage**: 1 test file (`V12_Performance.Tests/Core/FSMActorTests.cs`)

**Action Required**: Add TDD tests for EPIC-8 through EPIC-14 extractions

---

### 4. Roslyn Linting

**Purpose**: Catch code quality violations

**Command**: `powershell -File .\scripts\lint.ps1`

**Analyzers**: Roslyn C# analyzers configured in `.editorconfig`

---

### 5. CSharpier Formatting (V12 DNA - Curly Braces Mandate)

**Purpose**: Enforce consistent formatting and curly braces

**Command**: `dotnet csharpier check src/`

**Auto-fix**: `dotnet csharpier format src/`

**Why Mandatory**:
- Automatically adds missing braces (V12 DNA requirement)
- Fixes line ending inconsistencies (CRLF/LF)
- Prevents whitespace mutation in diffs
- Fast: <1 second for entire codebase

---

### 6. Security Scans

**6a. Gitleaks** (secrets detection):
```powershell
gitleaks detect --no-git --verbose
```

**6b. Snyk** (vulnerability scanning):
```powershell
snyk test --severity-threshold=high
```

**Note**: Both are WARNING-level (non-blocking) but should be addressed

---

### 7. Markdown Link Validation

**Purpose**: Catch broken documentation links

**Command**: `powershell -File .\scripts\verify_links.ps1`

**Scope**: All `.md` files in `docs/`

---

### 8. PR Hygiene

**Purpose**: Enforce diff size limits and commit structure

**Command**: `powershell -File .\scripts\verify_pr_hygiene.ps1`

**Thresholds**:
- Diff size: <10,000 characters in `src/`
- Commit structure: Logical, atomic commits

**Rationale**: Large diffs are hard to review and hide logic changes

---

### 9. Complexity Threshold (CYC ≤ 15)

**Purpose**: Enforce Jane Street-aligned cognitive simplicity

**Command**: `python scripts/complexity_audit.py --threshold 15 --fail-on-violation`

**Threshold**: CYC ≤ 15 (Jane Street aligned)

**Rationale**:
- Jane Street's HFT systems prioritize cognitive simplicity
- Functions with CYC >15 are harder to:
  - Reason about under microsecond latency constraints
  - Test exhaustively (exponential path growth)
  - Audit for race conditions in lock-free code
- V12 DNA mandates: "Make illegal states unrepresentable" - requires simple, verifiable logic

**Lizard Tool** (used by Codacy) has hardcoded threshold 8:
- Too conservative for HFT hot-path co-location
- Treat Lizard warnings (CYC 9-13) as technical debt visibility, not blockers
- Track in EPIC-CCN-10 backlog for future refactoring to CCN 10

---

### 10. Dead Code Detection

**Purpose**: Identify unused private methods

**Command**: `python scripts/dead_code_scan.py`

**Status**: WARNING-level (non-blocking)

**Action**: Review and remove or document why kept

---

### 11. Codacy Issue Preview

**Purpose**: Preview Codacy issues before PR submission

**Command**: `powershell -File .\scripts\query_codacy_issues.ps1`

**Requirements**:
- `$env:CODACY_API_TOKEN` must be set
- Branch must be pushed to GitHub

**API Endpoint**: `https://app.codacy.com/api/v3`

**Rate Limit**: 100 requests/hour

**Output**: `codacy_warnings.json` (gitignored)

---

### 12. Semgrep Security Scan

**Purpose**: Detect security anti-patterns

**Command**: `semgrep --config auto --json src/`

**Status**: WARNING-level (non-blocking)

**Findings**: Logged for review, not blocking

---

### 13. CodeRabbit AI Review

**Purpose**: AI-powered code review for quality and security

**Command**: `coderabbit review --agent --type uncommitted`

**Installation**:
```bash
curl -fsSL https://cli.coderabbit.ai/install.sh | sh
# or
brew install coderabbit
```

**Authentication**: `cr auth login` (browser-based) or `cr auth login --api-key` (CI/CD)

**Review Mode**: `--agent` (structured JSON for automation) or `--plain` (human-readable)

**Timeout**: 30 minutes max (background execution)

**Output**: `coderabbit_review.json` (gitignored)

**Pricing**: Free tier (limited) or Usage-based Add-on ($0.25/file, unlimited)

**Validation Period**: WARNING mode until 2026-06-09, then BLOCKING

**Thresholds**:
- **Blocking** (after validation): Critical or High severity findings
- **Warning** (current): All findings logged, none blocking

---

### 14. CodeScene Delta Analysis

**Purpose**: Detect code health degradation in staged changes

**Command**: `cs delta --staged --output-format json`

**Requirements**:
- CodeScene CLI installed
- `$env:CS_ACCESS_TOKEN` set

**Installation**:
```powershell
Invoke-WebRequest -Uri 'https://downloads.codescene.io/enterprise/cli/install-cs-tool.ps1' -OutFile install-cs-tool.ps1
.\install-cs-tool.ps1
```

**Metrics**:
- Code Health Score (0-10)
- Hotspot Detection (complexity + churn)
- Change Coupling (files that change together)

**Output**: `codescene_delta.json` (gitignored)

---

### 15. Jane Street Pattern Validation (NEW - Phase 4)

**Purpose**: Automated anti-pattern detection aligned with Jane Street standards

**Command**: `python scripts/jane_street_validator.py src/ --json`

**Severity Levels**:

#### P0 (CRITICAL - Blocking)
- `LOCK_USAGE`: `lock()` usage detected - use Actor/FSM pattern
- `NULLABLE_WITHOUT_CHECK`: Nullable reference used without null check
- `MUTABLE_SHARED_STATE`: Mutable shared state detected
- `UNICODE_IN_STRING`: Non-ASCII character detected in string literal

**Fix Suggestions**:
- `lock()` → "Use Channel<T> for producer/consumer or Interlocked for atomic ops"
- Nullable → "Use Option<T> or Result<T,E> pattern"
- Mutable state → "Use readonly/const or Actor pattern for shared state"
- Unicode → "Replace with ASCII equivalent or use escape sequence"

#### P1 (HIGH - Warning)
- `MAGIC_NUMBER`: Magic number detected
- `EXCEPTION_CONTROL_FLOW`: Exception used for control flow
- `NESTED_LOOPS_DEEP`: Nested loops >2 levels detected
- `LONG_METHOD`: Method exceeds 50 lines

**Fix Suggestions**:
- Magic number → "Extract to const: private const int MAX_RETRIES = 3;"
- Exception → "Use Result<T,E> pattern instead of exceptions"
- Nested loops → "Extract inner loops to separate methods"
- Long method → "Split into smaller, focused methods"

#### P2 (MEDIUM - Info)
- `MISSING_XML_DOCS`: Missing XML documentation on public API
- `TODO_COMMENT`: TODO comment detected (technical debt)
- `COMMENTED_CODE`: Commented-out code detected

**Fix Suggestions**:
- Missing docs → "Add /// <summary> documentation"
- TODO → "Create issue or remove comment"
- Commented code → "Remove or explain why commented"

**Usage**:
```bash
# JSON output (for automation)
python scripts/jane_street_validator.py src/ --json

# Human-readable with fix suggestions
python scripts/jane_street_validator.py src/ --fix-suggestions
```

**Exit Codes**:
- 0 = No P0 violations (P1/P2 may exist)
- 1 = P0 violations detected (blocking)

**Integration**: Runs in full mode only (skipped in `-Fast` mode)

---

### 16. Jane Street Rule Enforcement (NEW - Phase 5)

**Purpose**: Automated rule enforcement from comprehensive Jane Street rules catalog

**Command**: `python scripts/jane_street_rule_checker.py src/ --severity P0`

**Rule Categories**:

#### P0 (CRITICAL - Blocking)
- **Type Safety**: Use Result<T,E> instead of exceptions, Option<T> instead of null
- **Concurrency**: No lock() usage, use Actor/FSM pattern
- **Performance**: Zero-allocation in hot path, cache-friendly data structures
- **Code Quality**: ASCII-only, no magic numbers, explicit control flow

**Example Rules**:
- `JS-001`: Use Result<T,E> for error handling
- `JS-002`: Use Option<T> for missing values
- `JS-021`: No lock() - use Channel<T> or Interlocked
- `JS-042`: Extract magic numbers to named constants
- `JS-055`: ASCII-only string literals

#### P1 (HIGH - Warning)
- **Testing**: Property-based tests, deterministic randomness
- **Code Review**: Small PRs (<10k diff), fast feedback loops
- **Serialization**: Schema evolution, checksums, zero-copy

#### P2 (MEDIUM - Info)
- **Documentation**: XML docs on public APIs
- **Technical Debt**: Track TODO comments
- **Code Hygiene**: No commented-out code

**Usage**:
```bash
# Check all src/ files (P0 only)
python scripts/jane_street_rule_checker.py src/ --severity P0

# Check specific file
python scripts/jane_street_rule_checker.py src/V12_002.cs

# Include all severities
python scripts/jane_street_rule_checker.py src/ --severity ALL

# JSON output for automation
python scripts/jane_street_rule_checker.py src/ --json
```

**Exit Codes**:
- 0 = No P0 violations (P1/P2 may exist)
- 1 = P0 violations detected (blocking)

**Output Format**:
```
[P0] JS-042: Magic Number Detected
  File: src/V12_002.cs
  Line: 1234
  Message: Magic number '3' should be extracted to a named constant
  Fix: private const int MAX_RETRIES = 3;
```

**Integration**:
- Runs in both fast and full modes
- Complements Check #15 (Jane Street Patterns validator)
- Check #15 = anti-pattern detection (lock usage, nullable refs)
- Check #16 = rule enforcement (Result<T,E>, Option<T>, magic numbers)

**Reference**: See `docs/standards/jane-street/RULES_CATALOG.md` for complete rule catalog (100+ rules)

---

## Enforcement

### Bob CLI Integration

Bob CLI automatically runs `-Fast` mode before every commit:
```yaml
# .bob/settings.json
"pre_commit_validation": {
  "enabled": true,
  "command": "powershell -File .\\scripts\\pre_push_validation.ps1 -Fast"
}
```

### PR Loop Integration

The PR Loop runs FULL mode in Step 2 (Local Repair):
```bash
/pr-loop <PR_NUMBER>
# Step 2: powershell -File .\scripts\pre_push_validation.ps1
```

### Epic Run Integration

Epic workflows run FULL mode in Step C (Verification):
```bash
/epic-validate
# Runs: powershell -File .\scripts\pre_push_validation.ps1
```

### Manual TDD Workflow

Developers must run FULL mode in Step 2:
```bash
# Step 1: Write test
# Step 2: powershell -File .\scripts\pre_push_validation.ps1
# Step 3: Implement feature
# Step 4: Verify test passes
```

---

## Test Quality Audit

**Current Status**: 1 test file (`tests/V12_Performance.Tests/Core/FSMActorTests.cs`)

**Coverage**:
- ✅ Tests FSM/Actor Enqueue model (lock-free correctness)
- ✅ Validates atomic state transitions
- ❌ **Coverage Gap**: No tests for complexity-extracted methods (45 methods with CYC > 20)

**Action Required**: Add TDD tests for EPIC-8 through EPIC-14 extractions

---

## Troubleshooting

### CSharpier Not Installed
```powershell
dotnet tool install -g csharpier
```

### Python Not Found
Install Python 3.12 via Mise:
```bash
mise install python@3.12
```

### Codacy API Token Missing
```powershell
$env:CODACY_API_TOKEN = "<your-token>"
# Add to .env for persistence
```

### CodeRabbit Not Installed
```bash
curl -fsSL https://cli.coderabbit.ai/install.sh | sh
cr auth login
```

### CodeScene CLI Not Installed
```powershell
Invoke-WebRequest -Uri 'https://downloads.codescene.io/enterprise/cli/install-cs-tool.ps1' -OutFile install-cs-tool.ps1
.\install-cs-tool.ps1
$env:CS_ACCESS_TOKEN = "<your-token>"
```

---

## Related Documentation

- [AGENTS.md](../../AGENTS.md) - Agent hierarchy and tool requirements
- [MISE_IMPLEMENTATION_SUMMARY.md](MISE_IMPLEMENTATION_SUMMARY.md) - Mise tool integration
- [COMPLEXITY_RATIONALE.md](../standards/COMPLEXITY_RATIONALE.md) - Why CYC ≤ 15
- [JANE_STREET_PHILOSOPHY.md](../standards/jane-street/JANE_STREET_PHILOSOPHY.md) - Jane Street principles
- [BOB_MISE_INTEGRATION.md](BOB_MISE_INTEGRATION.md) - Bob CLI + Mise integration

---

**Made with Bob** 🤖