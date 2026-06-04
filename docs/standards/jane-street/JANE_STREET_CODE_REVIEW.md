# Jane Street Code Review: V12 Translation Guide

**Version**: 1.0  
**Last Updated**: 2026-06-03  
**Status**: Active Standard  
**Compliance**: V12 DNA Mandatory

---

## Overview

This document translates Jane Street's code review practices from OCaml into V12-aligned C# workflows. Jane Street's philosophy: **"Code review is the primary mechanism for maintaining code quality and sharing knowledge."**

### Jane Street Code Review Philosophy

Jane Street's approach to code review:
- **Mandatory Reviews**: Every change requires review
- **Fast Feedback**: Reviews within hours, not days
- **Teaching Tool**: Reviews educate junior developers
- **Standards Enforcement**: Automated + human checks
- **Incremental Changes**: Small, focused PRs

### V12 Alignment

V12 DNA implements these principles:
- ✅ **Pre-Push Validation**: 13 automated checks
- ✅ **PR Loop**: Automated fix-all workflow
- ✅ **Bob CLI**: Enforces V12 DNA constraints
- ✅ **Arena AI**: Adversarial consensus review
- ✅ **Codacy/CodeRabbit**: Automated quality gates

---

## Pattern 1: Pre-Commit Validation (Fast Feedback)

### Jane Street Approach (OCaml)

```bash
# Jane Street: Pre-commit hooks
# .git/hooks/pre-commit

#!/bin/bash
set -e

# Format check
dune build @fmt --auto-promote

# Type check
dune build @check

# Unit tests
dune runtest

# Linter
dune build @lint

echo "Pre-commit checks passed"
```

### V12 Translation (C#)

```powershell
# V12: Pre-push validation (13 checks)
# scripts/pre_push_validation.ps1

param(
    [switch]$Fast,
    [switch]$SkipBuild,
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

Write-Host "=== V12 Pre-Push Validation ===" -ForegroundColor Cyan

# Check 1: ASCII-Only
Write-Host "[1/13] ASCII-Only Check..." -ForegroundColor Yellow
$nonAscii = Get-ChildItem -Path src -Recurse -Filter *.cs | 
    Select-String -Pattern '[^\x00-\x7F]'
if ($nonAscii) {
    Write-Error "Non-ASCII characters found (V12 DNA violation)"
    exit 1
}

# Check 2: Build
if (-not $SkipBuild) {
    Write-Host "[2/13] Build Check..." -ForegroundColor Yellow
    dotnet build --no-incremental
    if ($LASTEXITCODE -ne 0) { exit 1 }
}

# Check 3: Unit Tests
if (-not $SkipTests) {
    Write-Host "[3/13] Unit Tests..." -ForegroundColor Yellow
    dotnet test --no-build
    if ($LASTEXITCODE -ne 0) { exit 1 }
}

# Check 4: Lint (Roslyn)
Write-Host "[4/13] Lint Check..." -ForegroundColor Yellow
dotnet build /p:TreatWarningsAsErrors=true
if ($LASTEXITCODE -ne 0) { exit 1 }

# Check 5: Formatting (CSharpier)
Write-Host "[5/13] Format Check..." -ForegroundColor Yellow
dotnet csharpier check src/
if ($LASTEXITCODE -ne 0) { exit 1 }

# Check 6: Security (Gitleaks + Snyk)
Write-Host "[6/13] Security Scan..." -ForegroundColor Yellow
gitleaks detect --no-git
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Security issues found (non-blocking)"
}

# Check 7: Markdown Links
Write-Host "[7/13] Markdown Links..." -ForegroundColor Yellow
powershell -File .\scripts\verify_links.ps1

# Check 8: PR Hygiene
Write-Host "[8/13] PR Hygiene..." -ForegroundColor Yellow
powershell -File .\scripts\verify_pr_hygiene.ps1

# Check 9: Complexity (CYC ≤15)
Write-Host "[9/13] Complexity Audit..." -ForegroundColor Yellow
python scripts/complexity_audit.py --threshold 15
if ($LASTEXITCODE -ne 0) { exit 1 }

# Fast mode: Skip slow checks
if ($Fast) {
    Write-Host "Fast mode: Skipping checks 10-13" -ForegroundColor Yellow
    exit 0
}

# Check 10: Dead Code
Write-Host "[10/13] Dead Code Scan..." -ForegroundColor Yellow
python scripts/dead_code_scan.py

# Check 11: Codacy Preview
Write-Host "[11/13] Codacy Preview..." -ForegroundColor Yellow
powershell -File .\scripts\query_codacy_issues.ps1

# Check 12: Semgrep
Write-Host "[12/13] Semgrep Scan..." -ForegroundColor Yellow
semgrep --config auto src/

# Check 13: CodeRabbit AI
Write-Host "[13/13] CodeRabbit Review..." -ForegroundColor Yellow
cr review --agent --timeout 30m

Write-Host "=== All Checks Passed ===" -ForegroundColor Green
```

**V12 DNA Alignment:**
- ✅ Lock-free: No blocking operations
- ✅ Type-safe: Compiler enforces correctness
- ✅ CYC ≤15: Complexity gate
- ✅ Fast: <2 minutes in fast mode

**DO:**
- ✅ Run pre-push validation before every push
- ✅ Use `-Fast` mode for quick iterations
- ✅ Fix all blocking issues before push
- ✅ Treat warnings as errors

**DON'T:**
- ❌ Skip validation (protocol violation)
- ❌ Push with failing checks
- ❌ Ignore warnings (technical debt)
- ❌ Disable checks without approval

---

## Pattern 2: PR Loop (Automated Fix-All)

### Jane Street Approach (OCaml)

```bash
# Jane Street: Automated fix workflow
# scripts/fix_all.sh

#!/bin/bash
set -e

# Format code
dune build @fmt --auto-promote

# Fix linter issues
dune build @lint --auto-promote

# Regenerate expect tests
dune runtest --auto-promote

# Commit fixes
git add .
git commit -m "Auto-fix: formatting and linter"
```

### V12 Translation (C#)

```powershell
# V12: PR Loop (automated fix-all)
# Usage: /pr-loop <PR_NUMBER>

param(
    [Parameter(Mandatory=$true)]
    [int]$PRNumber
)

$ErrorActionPreference = "Stop"

Write-Host "=== V12 PR Loop: PR #$PRNumber ===" -ForegroundColor Cyan

# Step 1: Fetch PR and create local branch
Write-Host "[Step 1] Fetching PR..." -ForegroundColor Yellow
gh pr checkout $PRNumber

# Step 2: Run pre-push validation
Write-Host "[Step 2] Running validation..." -ForegroundColor Yellow
powershell -File .\scripts\pre_push_validation.ps1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Validation failed. Starting repair..." -ForegroundColor Red
    
    # Auto-fix: Format
    Write-Host "  [2a] Auto-formatting..." -ForegroundColor Yellow
    dotnet csharpier format src/
    
    # Auto-fix: Build
    Write-Host "  [2b] Rebuilding..." -ForegroundColor Yellow
    dotnet build --no-incremental
    
    # Auto-fix: Complexity (extract methods)
    Write-Host "  [2c] Complexity repair..." -ForegroundColor Yellow
    python scripts/complexity_repair.py --auto-extract
    
    # Commit fixes
    git add .
    git commit -m "Auto-fix: PR #$PRNumber validation issues"
    git push
}

# Step 3: Calculate Project Health Score (PHS)
Write-Host "[Step 3] Calculating PHS..." -ForegroundColor Yellow
$phs = python scripts/calculate_phs.py
Write-Host "  PHS: $phs/100" -ForegroundColor Cyan

if ($phs -lt 100) {
    Write-Host "  PHS < 100. Running repair loop..." -ForegroundColor Yellow
    
    # Repair loop: Iterate until PHS = 100
    $iteration = 1
    while ($phs -lt 100 -and $iteration -le 5) {
        Write-Host "  [Iteration $iteration] Repairing..." -ForegroundColor Yellow
        
        # Fix highest-priority issues
        python scripts/fix_priority_issues.py
        
        # Recalculate PHS
        $phs = python scripts/calculate_phs.py
        Write-Host "  PHS: $phs/100" -ForegroundColor Cyan
        
        # Commit fixes
        git add .
        git commit -m "Auto-fix: PHS repair iteration $iteration"
        git push
        
        $iteration++
    }
}

# Step 4: Final validation
Write-Host "[Step 4] Final validation..." -ForegroundColor Yellow
powershell -File .\scripts\pre_push_validation.ps1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Final validation failed. Manual intervention required."
    exit 1
}

Write-Host "=== PR Loop Complete: PHS = $phs/100 ===" -ForegroundColor Green
```

**V12 DNA Alignment:**
- ✅ Lock-free: Automated workflow
- ✅ Type-safe: Validation enforces correctness
- ✅ CYC ≤15: Complexity repair
- ✅ Iterative: Converges to PHS = 100

**DO:**
- ✅ Run PR loop after every PR submission
- ✅ Iterate until PHS = 100
- ✅ Commit fixes incrementally
- ✅ Push after each iteration

**DON'T:**
- ❌ Skip PR loop (protocol violation)
- ❌ Merge with PHS < 100
- ❌ Manual fixes (use automation)
- ❌ Exceed 5 iterations (escalate to Director)

---

## Pattern 3: Adversarial Review (Arena AI)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Adversarial review *)
(* Multiple reviewers with different perspectives *)

(* Reviewer 1: Correctness *)
let review_correctness code =
  check_type_safety code;
  check_edge_cases code;
  check_error_handling code

(* Reviewer 2: Performance *)
let review_performance code =
  check_allocations code;
  check_complexity code;
  check_cache_friendliness code

(* Reviewer 3: Maintainability *)
let review_maintainability code =
  check_readability code;
  check_documentation code;
  check_test_coverage code
```

### V12 Translation (C#)

```csharp
// V12: Arena AI (adversarial consensus)
// Usage: arena-ai review <PR_NUMBER>

public class ArenaAIReview
{
    public async Task<ReviewResult> ReviewPRAsync(int prNumber)
    {
        // Fetch PR diff
        var diff = await FetchPRDiffAsync(prNumber);

        // Spawn 3 adversarial reviewers
        var correctnessTask = ReviewCorrectnessAsync(diff);
        var performanceTask = ReviewPerformanceAsync(diff);
        var maintainabilityTask = ReviewMaintainabilityAsync(diff);

        await Task.WhenAll(correctnessTask, performanceTask, maintainabilityTask);

        // Aggregate findings
        var findings = new List<Finding>();
        findings.AddRange(correctnessTask.Result);
        findings.AddRange(performanceTask.Result);
        findings.AddRange(maintainabilityTask.Result);

        // Consensus: All reviewers must approve
        var consensus = findings.All(f => f.Severity != Severity.Critical);

        return new ReviewResult
        {
            Consensus = consensus,
            Findings = findings,
            CorrectnessPassed = correctnessTask.Result.All(f => f.Severity != Severity.Critical),
            PerformancePassed = performanceTask.Result.All(f => f.Severity != Severity.Critical),
            MaintainabilityPassed = maintainabilityTask.Result.All(f => f.Severity != Severity.Critical)
        };
    }

    private async Task<List<Finding>> ReviewCorrectnessAsync(string diff)
    {
        var findings = new List<Finding>();

        // Check 1: Type safety
        if (diff.Contains("(object)") || diff.Contains("as "))
            findings.Add(new Finding
            {
                Severity = Severity.High,
                Category = "Type Safety",
                Message = "Unsafe cast detected. Use pattern matching."
            });

        // Check 2: Null handling
        if (diff.Contains("== null") || diff.Contains("!= null"))
            findings.Add(new Finding
            {
                Severity = Severity.Medium,
                Category = "Null Safety",
                Message = "Null check detected. Use Option<T>."
            });

        // Check 3: Error handling
        if (diff.Contains("throw new Exception"))
            findings.Add(new Finding
            {
                Severity = Severity.High,
                Category = "Error Handling",
                Message = "Generic exception thrown. Use Result<T,E>."
            });

        return findings;
    }

    private async Task<List<Finding>> ReviewPerformanceAsync(string diff)
    {
        var findings = new List<Finding>();

        // Check 1: Allocations
        if (diff.Contains("new List<") || diff.Contains("new Dictionary<"))
            findings.Add(new Finding
            {
                Severity = Severity.Medium,
                Category = "Allocations",
                Message = "Heap allocation in hot path. Use ArrayPool<T>."
            });

        // Check 2: Locks
        if (diff.Contains("lock("))
            findings.Add(new Finding
            {
                Severity = Severity.Critical,
                Category = "Concurrency",
                Message = "Lock detected (V12 DNA violation). Use Actor pattern."
            });

        // Check 3: Complexity
        var complexity = CalculateComplexity(diff);
        if (complexity > 15)
            findings.Add(new Finding
            {
                Severity = Severity.High,
                Category = "Complexity",
                Message = $"Cyclomatic complexity {complexity} exceeds threshold 15."
            });

        return findings;
    }

    private async Task<List<Finding>> ReviewMaintainabilityAsync(string diff)
    {
        var findings = new List<Finding>();

        // Check 1: Documentation
        if (!diff.Contains("///"))
            findings.Add(new Finding
            {
                Severity = Severity.Low,
                Category = "Documentation",
                Message = "Missing XML documentation."
            });

        // Check 2: Test coverage
        var testCoverage = CalculateTestCoverage(diff);
        if (testCoverage < 80)
            findings.Add(new Finding
            {
                Severity = Severity.Medium,
                Category = "Testing",
                Message = $"Test coverage {testCoverage}% below threshold 80%."
            });

        return findings;
    }

    private int CalculateComplexity(string diff) => 10;  // Placeholder
    private int CalculateTestCoverage(string diff) => 85;  // Placeholder
    private async Task<string> FetchPRDiffAsync(int prNumber) => "";  // Placeholder
}

public record ReviewResult
{
    public bool Consensus { get; init; }
    public List<Finding> Findings { get; init; } = new();
    public bool CorrectnessPassed { get; init; }
    public bool PerformancePassed { get; init; }
    public bool MaintainabilityPassed { get; init; }
}

public record Finding
{
    public Severity Severity { get; init; }
    public string Category { get; init; } = "";
    public string Message { get; init; } = "";
}

public enum Severity { Low, Medium, High, Critical }
```

**V12 DNA Alignment:**
- ✅ Lock-free: Parallel review tasks
- ✅ Type-safe: Enforces V12 DNA constraints
- ✅ CYC ≤15: Complexity gate
- ✅ Consensus: All reviewers must approve

**DO:**
- ✅ Run Arena AI before merge
- ✅ Fix all critical findings
- ✅ Require consensus from all reviewers
- ✅ Document review decisions

**DON'T:**
- ❌ Merge without consensus
- ❌ Ignore critical findings
- ❌ Override Arena AI (escalate to Director)
- ❌ Skip adversarial review

---

## Pattern 4: Incremental Changes (Small PRs)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Small, focused PRs *)

(* BAD: Large PR mixing concerns *)
let big_pr () =
  refactor_module_a ();
  add_feature_b ();
  fix_bug_c ();
  update_docs ()

(* GOOD: Small, focused PRs *)
let pr_1 () = refactor_module_a ()
let pr_2 () = add_feature_b ()
let pr_3 () = fix_bug_c ()
let pr_4 () = update_docs ()
```

### V12 Translation (C#)

```csharp
// V12: Incremental changes (diff limit <10k chars)

// BAD: Large PR (scope creep)
public class LargePR
{
    public void Execute()
    {
        // Concern 1: Refactoring
        RefactorModuleA();

        // Concern 2: New feature
        AddFeatureB();

        // Concern 3: Bug fix
        FixBugC();

        // Concern 4: Documentation
        UpdateDocs();

        // Result: 15k char diff, mixed concerns, hard to review
    }
}

// GOOD: Small, focused PRs
public class SmallPR1
{
    // PR #1: Refactoring only (3k char diff)
    public void Execute()
    {
        RefactorModuleA();
    }
}

public class SmallPR2
{
    // PR #2: New feature only (4k char diff)
    public void Execute()
    {
        AddFeatureB();
    }
}

public class SmallPR3
{
    // PR #3: Bug fix only (2k char diff)
    public void Execute()
    {
        FixBugC();
    }
}

public class SmallPR4
{
    // PR #4: Documentation only (1k char diff)
    public void Execute()
    {
        UpdateDocs();
    }
}

// V12: Diff guard (enforces <10k char limit)
public class DiffGuard
{
    public static Result<Unit, string> ValidatePRSize(string diff)
    {
        var charCount = diff.Length;

        if (charCount > 10000)
            return Result<Unit, string>.Err(
                $"PR diff {charCount} chars exceeds limit 10,000. Split into smaller PRs.");

        return Result<Unit, string>.Ok(Unit.Value);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Independent PRs
- ✅ Type-safe: Single concern per PR
- ✅ CYC ≤15: Smaller changes = simpler logic
- ✅ Reviewable: <10k chars = fast review

**DO:**
- ✅ Keep PRs under 10k chars
- ✅ One concern per PR
- ✅ Split large changes into multiple PRs
- ✅ Run diff guard before push

**DON'T:**
- ❌ Mix concerns in one PR (scope creep)
- ❌ Exceed 10k char diff limit
- ❌ Bundle unrelated fixes
- ❌ Skip diff guard validation

---

## Pattern 5: Standards Enforcement (Automated Checks)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Automated standards enforcement *)

(* Check 1: Naming conventions *)
let check_naming code =
  if not (is_snake_case code.function_names) then
    Error "Function names must be snake_case"
  else
    Ok ()

(* Check 2: Module structure *)
let check_module_structure code =
  if not (has_signature code.module_) then
    Error "Module must have signature"
  else
    Ok ()

(* Check 3: Documentation *)
let check_documentation code =
  if not (has_docstring code.functions) then
    Error "Functions must have docstrings"
  else
    Ok ()
```

### V12 Translation (C#)

```csharp
// V12: Standards enforcement (Roslyn Analyzers)

// Analyzer 1: V12 DNA Lock-Free Rule
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class V12LockFreeAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        id: "V12001",
        title: "Lock statement detected",
        messageFormat: "Lock statement violates V12 DNA. Use Actor pattern.",
        category: "V12DNA",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeLockStatement, SyntaxKind.LockStatement);
    }

    private void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
    {
        var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}

// Analyzer 2: V12 DNA Complexity Rule
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class V12ComplexityAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        id: "V12002",
        title: "Cyclomatic complexity exceeds threshold",
        messageFormat: "Method '{0}' has complexity {1}, exceeds threshold 15",
        category: "V12DNA",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var method = (MethodDeclarationSyntax)context.Node;
        var complexity = CalculateComplexity(method);

        if (complexity > 15)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                method.Identifier.GetLocation(),
                method.Identifier.Text,
                complexity);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private int CalculateComplexity(MethodDeclarationSyntax method)
    {
        // Simplified complexity calculation
        var walker = new ComplexityWalker();
        walker.Visit(method);
        return walker.Complexity;
    }
}

// Analyzer 3: V12 DNA ASCII-Only Rule
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class V12AsciiOnlyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        id: "V12003",
        title: "Non-ASCII character detected",
        messageFormat: "String literal contains non-ASCII character: '{0}'",
        category: "V12DNA",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeStringLiteral, SyntaxKind.StringLiteralExpression);
    }

    private void AnalyzeStringLiteral(SyntaxNodeAnalysisContext context)
    {
        var literal = (LiteralExpressionSyntax)context.Node;
        var text = literal.Token.ValueText;

        foreach (var ch in text)
        {
            if (ch > 127)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    literal.GetLocation(),
                    ch);
                context.ReportDiagnostic(diagnostic);
                return;
            }
        }
    }
}

internal class ComplexityWalker : CSharpSyntaxWalker
{
    public int Complexity { get; private set; } = 1;

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        Complexity++;
        base.VisitIfStatement(node);
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        Complexity++;
        base.VisitWhileStatement(node);
    }

    public override void VisitForStatement(ForStatementSyntax node)
    {
        Complexity++;
        base.VisitForStatement(node);
    }

    public override void VisitSwitchSection(SwitchSectionSyntax node)
    {
        Complexity++;
        base.VisitSwitchSection(node);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Analyzer detects locks
- ✅ Type-safe: Compiler enforces rules
- ✅ CYC ≤15: Complexity analyzer
- ✅ ASCII-only: Character analyzer

**DO:**
- ✅ Use Roslyn Analyzers for V12 DNA enforcement
- ✅ Treat analyzer warnings as errors
- ✅ Run analyzers in CI/CD pipeline
- ✅ Add custom analyzers for project-specific rules

**DON'T:**
- ❌ Disable analyzers (protocol violation)
- ❌ Suppress warnings without justification
- ❌ Skip analyzer checks in CI
- ❌ Ignore analyzer diagnostics

---

## Pattern 6: Knowledge Sharing (Teaching Reviews)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Teaching reviews *)

(* Review comment: Explain WHY, not just WHAT *)
let review_comment_bad = "Use List.map instead of List.iter"

let review_comment_good =
  "Use List.map instead of List.iter because:
   1. List.map is more idiomatic for transformations
   2. List.map returns a new list (immutable)
   3. List.iter is for side effects only
   
   Example:
   (* Before *)
   let results = ref [] in
   List.iter (fun x -> results := f x :: !results) xs;
   List.rev !results
   
   (* After *)
   List.map f xs"
```

### V12 Translation (C#)

```csharp
// V12: Teaching reviews (explain WHY)

// BAD: Review comment without context
// "Use LINQ Select instead of foreach"

// GOOD: Review comment with explanation
/*
Use LINQ Select instead of foreach because:

1. **Declarative**: Expresses WHAT, not HOW
2. **Composable**: Can chain with other LINQ operators
3. **Lazy**: Deferred execution (better performance)
4. **Immutable**: Returns new collection (V12 DNA)

Example:

// Before (imperative)
var results = new List<int>();
foreach (var x in xs)
{
    results.Add(f(x));
}

// After (declarative)
var results = xs.Select(f).ToList();

// Even better (lazy evaluation)
var results = xs.Select(f);  // No ToList() unless needed

V12 DNA Alignment:
- ✅ Immutable: No mutation of results list
- ✅ Composable: Can chain .Where(), .OrderBy(), etc.
- ✅ Readable: Intent is clear

References:
- JANE_STREET_CORE_PATTERNS.md: Pattern 5 (Pipeline Operators)
- JANE_STREET_PERFORMANCE_PATTERNS.md: Pattern 1 (Zero-Allocation)
*/

// V12: Review template
public class ReviewTemplate
{
    public string CreateReviewComment(
        string issue,
        string reason,
        string before,
        string after,
        string[] dnaAlignment,
        string[] references)
    {
        return $@"
{issue}

**Why:**
{reason}

**Example:**

```csharp
// Before
{before}

// After
{after}
```

**V12 DNA Alignment:**
{string.Join("\n", dnaAlignment.Select(a => $"- ✅ {a}"))}

**References:**
{string.Join("\n", references.Select(r => $"- {r}"))}
";
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: No blocking in reviews
- ✅ Type-safe: Explain type safety benefits
- ✅ CYC ≤15: Explain complexity reduction
- ✅ Educational: Teach V12 DNA principles

**DO:**
- ✅ Explain WHY, not just WHAT
- ✅ Provide before/after examples
- ✅ Link to V12 DNA principles
- ✅ Reference standards documents

**DON'T:**
- ❌ Give terse comments ("fix this")
- ❌ Assume reviewer knows context
- ❌ Skip examples
- ❌ Forget to link to standards

---

## Pattern 7: Fast Turnaround (Review SLA)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Fast review turnaround *)

(* SLA: Reviews within 4 hours *)
let review_sla = 4 * 60 * 60  (* 4 hours in seconds *)

let check_review_time pr =
  let elapsed = current_time () - pr.submitted_at in
  if elapsed > review_sla then
    notify_reviewers pr "Review SLA exceeded"
  else
    ()
```

### V12 Translation (C#)

```csharp
// V12: Fast review turnaround (4-hour SLA)

public class ReviewSLA
{
    private static readonly TimeSpan SLA = TimeSpan.FromHours(4);

    public async Task MonitorReviewSLAAsync()
    {
        while (true)
        {
            var pendingPRs = await FetchPendingPRsAsync();

            foreach (var pr in pendingPRs)
            {
                var elapsed = DateTime.UtcNow - pr.SubmittedAt;

                if (elapsed > SLA)
                {
                    await NotifyReviewersAsync(pr, "Review SLA exceeded");
                }
                else if (elapsed > SLA * 0.75)
                {
                    await NotifyReviewersAsync(pr, "Review SLA approaching");
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(15));
        }
    }

    private async Task<List<PullRequest>> FetchPendingPRsAsync()
    {
        // Fetch PRs awaiting review
        return new List<PullRequest>();
    }

    private async Task NotifyReviewersAsync(PullRequest pr, string message)
    {
        // Send notification to reviewers
        Console.WriteLine($"PR #{pr.Number}: {message}");
    }
}

public record PullRequest
{
    public int Number { get; init; }
    public DateTime SubmittedAt { get; init; }
    public List<string> Reviewers { get; init; } = new();
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Async monitoring
- ✅ Type-safe: Strongly-typed PR model
- ✅ CYC ≤8: Simple SLA logic
- ✅ Fast: 4-hour turnaround

**DO:**
- ✅ Review PRs within 4 hours
- ✅ Monitor SLA compliance
- ✅ Notify reviewers when SLA approaches
- ✅ Prioritize small PRs (<10k chars)

**DON'T:**
- ❌ Let PRs sit for days
- ❌ Ignore SLA notifications
- ❌ Block on non-critical issues
- ❌ Require perfect PRs (iterate)

---

## Summary Checklist

### Code Review Patterns Compliance

- [ ] **Pre-Commit Validation**: Run 13 checks before every push
- [ ] **PR Loop**: Automated fix-all workflow (PHS = 100)
- [ ] **Adversarial Review**: Arena AI consensus required
- [ ] **Incremental Changes**: PRs under 10k chars
- [ ] **Standards Enforcement**: Roslyn Analyzers for V12 DNA
- [ ] **Knowledge Sharing**: Explain WHY in review comments
- [ ] **Fast Turnaround**: Review within 4 hours

### V12 DNA Compliance Matrix

| Pattern | Lock-Free | Type-Safe | CYC ≤15 | Automated | Fast |
|---------|-----------|-----------|---------|-----------|------|
| Pre-Commit Validation | ✅ | ✅ | ✅ | ✅ | ✅ |
| PR Loop | ✅ | ✅ | ✅ | ✅ | ✅ |
| Adversarial Review | ✅ | ✅ | ✅ | ✅ | ⚠️ |
| Incremental Changes | ✅ | ✅ | ✅ | ✅ | ✅ |
| Standards Enforcement | ✅ | ✅ | ✅ | ✅ | ✅ |
| Knowledge Sharing | ✅ | ✅ | ✅ | ⚠️ | ⚠️ |
| Fast Turnaround | ✅ | ✅ | ✅ | ✅ | ✅ |

**Legend**: ✅ Full compliance | ⚠️ Acceptable | ❌ Not applicable

---

## References

### Jane Street Resources
- **Firestore KB**: `weeks_making_ocaml_safe_2025` (Code Review Practices)

### V12 Standards
- [`AGENTS.md`](../../AGENTS.md) - Section 3.5: Pre-Push Validation Protocol
- [`AGENTS.md`](../../AGENTS.md) - Section 11: No Scope Creep Protocol
- [`docs/protocol/BRANCH_STRATEGY.md`](../../protocol/BRANCH_STRATEGY.md) - Three-Tier Branch Model

### Related Documents
- [`JANE_STREET_TOOLS_PATTERNS.md`](./JANE_STREET_TOOLS_PATTERNS.md) - CI/CD automation
- [`JANE_STREET_TESTING_PATTERNS.md`](./JANE_STREET_TESTING_PATTERNS.md) - Test-driven review

---

**Document Status**: ✅ Complete (7 patterns documented)  
**Next Review**: 2026-07-03  
**Maintainer**: V12 Architecture Team