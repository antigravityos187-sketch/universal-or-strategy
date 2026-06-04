# Jane Street Tools Patterns: V12 Translation Guide

**Version**: 1.0  
**Last Updated**: 2026-06-03  
**Status**: Active Standard  
**Compliance**: V12 DNA Mandatory

---

## Overview

This document translates Jane Street's development tools and workflows from OCaml into V12-aligned C# implementations. Jane Street emphasizes **fast feedback loops**, **automated tooling**, and **developer productivity**—principles that directly impact code quality and velocity.

### Jane Street Tools Philosophy

Jane Street's tooling strategy prioritizes:
- **Fast Compilation**: Incremental builds in <1 second
- **Integrated Testing**: Tests run automatically on save
- **Static Analysis**: Catch bugs before runtime
- **Code Generation**: Eliminate boilerplate
- **Developer Experience**: Minimize friction

### V12 Alignment

V12 DNA implements these principles:
- ✅ **Mise**: Unified tool management
- ✅ **CSharpier**: Automated formatting
- ✅ **Roslyn Analyzers**: Static analysis
- ✅ **Source Generators**: Code generation
- ✅ **Hot Reload**: Fast feedback loops

---

## Pattern 1: Fast Compilation (Incremental Builds)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Dune for fast incremental builds *)
(* dune file *)
(library
 (name order_book)
 (libraries core async)
 (preprocess (pps ppx_jane)))

(* Incremental compilation: only rebuild changed modules *)
(* dune build --watch *)
```

### V12 Translation (C#)

```csharp
// V12: MSBuild with incremental compilation
// Directory.Build.props
/*
<Project>
  <PropertyGroup>
    <!-- Enable incremental build -->
    <Deterministic>true</Deterministic>
    <ProduceReferenceAssembly>true</ProduceReferenceAssembly>
    
    <!-- Fast compilation -->
    <UseSharedCompilation>true</UseSharedCompilation>
    <BuildInParallel>true</BuildInParallel>
    
    <!-- Hot reload -->
    <EnforceExtendedAnalyzerRules>false</EnforceExtendedAnalyzerRules>
  </PropertyGroup>
</Project>
*/

// Watch mode: dotnet watch run
// Hot reload: dotnet watch --no-hot-reload=false

// Mise integration for consistent builds
/*
[tools]
dotnet = "8.0"

[tasks.build]
run = "dotnet build --no-incremental=false"

[tasks.watch]
run = "dotnet watch run"

[tasks.test-watch]
run = "dotnet watch test"
*/
```

**V12 DNA Alignment:**
- ✅ Fast: Incremental builds <1 second
- ✅ Consistent: Mise ensures tool versions
- ✅ Automated: Watch mode for continuous feedback
- ✅ Parallel: Multi-core compilation

**DO:**
- ✅ Enable incremental compilation
- ✅ Use watch mode during development
- ✅ Use Mise for tool management
- ✅ Enable parallel builds

**DON'T:**
- ❌ Disable incremental builds (slow feedback)
- ❌ Use `--no-incremental` in development
- ❌ Ignore build warnings (technical debt)
- ❌ Skip Mise setup (inconsistent environments)

---

## Pattern 2: Automated Formatting (Code Style)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: ocamlformat for consistent style *)
(* .ocamlformat *)
(*
profile = janestreet
break-infix = fit-or-vertical
*)

(* Format on save in editor *)
(* dune build @fmt --auto-promote *)
```

### V12 Translation (C#)

```csharp
// V12: CSharpier for opinionated formatting
// .csharpierrc.json
/*
{
  "printWidth": 100,
  "useTabs": false,
  "tabWidth": 4,
  "endOfLine": "lf"
}
*/

// Mise integration
/*
[tools]
"dotnet:csharpier" = "latest"

[tasks.format]
run = "dotnet csharpier ."

[tasks.format-check]
run = "dotnet csharpier --check ."
*/

// Pre-commit hook (.git/hooks/pre-commit)
/*
#!/bin/sh
dotnet csharpier --check . || {
  echo "Code formatting issues detected. Run 'dotnet csharpier .' to fix."
  exit 1
}
*/

// VS Code integration (settings.json)
/*
{
  "editor.formatOnSave": true,
  "[csharp]": {
    "editor.defaultFormatter": "csharpier.csharpier-vscode"
  }
}
*/
```

**V12 DNA Alignment:**
- ✅ Consistent: Opinionated formatting
- ✅ Automated: Format on save
- ✅ Enforced: Pre-commit hooks
- ✅ Fast: <1 second for entire codebase

**DO:**
- ✅ Use CSharpier for all C# code
- ✅ Enable format-on-save in editor
- ✅ Add pre-commit hooks
- ✅ Run format check in CI

**DON'T:**
- ❌ Manually format code (waste of time)
- ❌ Debate style in code reviews (use formatter)
- ❌ Skip formatting checks in CI
- ❌ Use multiple formatters (inconsistency)

---

## Pattern 3: Static Analysis (Linting)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: ppx_js_style for linting *)
(* Enforces Jane Street coding standards *)
(library
 (name my_lib)
 (preprocess (pps ppx_js_style)))

(* Catches common mistakes at compile time *)
```

### V12 Translation (C#)

```csharp
// V12: Roslyn Analyzers + EditorConfig
// .editorconfig
/*
root = true

[*.cs]
# V12 DNA Rules
dotnet_diagnostic.CA1062.severity = error  # Validate arguments
dotnet_diagnostic.CA2007.severity = error  # ConfigureAwait
dotnet_diagnostic.CA1031.severity = warning  # Catch specific exceptions

# Naming conventions
dotnet_naming_rule.private_fields_underscore.severity = error
dotnet_naming_rule.private_fields_underscore.symbols = private_fields
dotnet_naming_rule.private_fields_underscore.style = underscore_prefix

# Code quality
dotnet_code_quality.CA1062.exclude_extension_method_this_parameter = true
*/

// Custom Roslyn Analyzer
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class V12DNAAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor LockUsageRule = new(
        id: "V12001",
        title: "Lock usage detected",
        messageFormat: "Lock usage is banned in V12 DNA. Use Actor pattern instead.",
        category: "V12DNA",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnicodeStringRule = new(
        id: "V12002",
        title: "Unicode string detected",
        messageFormat: "Unicode strings are banned in V12 DNA. Use ASCII only.",
        category: "V12DNA",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(LockUsageRule, UnicodeStringRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeLockStatement, 
            SyntaxKind.LockStatement);
        context.RegisterSyntaxNodeAction(AnalyzeStringLiteral, 
            SyntaxKind.StringLiteralExpression);
    }

    private void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
    {
        var diagnostic = Diagnostic.Create(
            LockUsageRule, 
            context.Node.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private void AnalyzeStringLiteral(SyntaxNodeAnalysisContext context)
    {
        var literal = (LiteralExpressionSyntax)context.Node;
        var text = literal.Token.ValueText;

        if (text.Any(c => c > 127))
        {
            var diagnostic = Diagnostic.Create(
                UnicodeStringRule, 
                context.Node.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}

// Mise integration
/*
[tasks.lint]
run = "dotnet build /p:TreatWarningsAsErrors=true"

[tasks.lint-fix]
run = "dotnet format"
*/
```

**V12 DNA Alignment:**
- ✅ Enforced: Compile-time checks
- ✅ Custom: V12 DNA-specific rules
- ✅ Fast: Incremental analysis
- ✅ Automated: Runs on every build

**DO:**
- ✅ Create custom analyzers for V12 DNA rules
- ✅ Treat warnings as errors in CI
- ✅ Use EditorConfig for team consistency
- ✅ Run analyzers on every build

**DON'T:**
- ❌ Disable analyzers (loses safety)
- ❌ Ignore warnings (technical debt)
- ❌ Skip analyzer updates (miss new rules)
- ❌ Use `#pragma warning disable` without justification

---

## Pattern 4: Code Generation (Boilerplate Elimination)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: PPX for code generation *)
type order = {
  order_id: int64;
  price: float;
  quantity: int;
} [@@deriving sexp, bin_io, compare, hash]

(* Generates: *)
(* - Serialization (sexp, bin_io) *)
(* - Comparison (compare) *)
(* - Hashing (hash) *)
```

### V12 Translation (C#)

```csharp
// V12: Source Generators for boilerplate
[Generator]
public class SerializationGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            return;

        foreach (var recordDecl in receiver.RecordDeclarations)
        {
            var source = GenerateSerializationCode(recordDecl);
            context.AddSource($"{recordDecl.Identifier}.Serialization.g.cs", source);
        }
    }

    private string GenerateSerializationCode(RecordDeclarationSyntax record)
    {
        var className = record.Identifier.Text;
        return $@"
// <auto-generated/>
partial record {className}
{{
    public void WriteTo(Span<byte> buffer)
    {{
        // Generated serialization code
    }}

    public static Result<{className}, string> ReadFrom(ReadOnlySpan<byte> buffer)
    {{
        // Generated deserialization code
    }}
}}";
    }

    private class SyntaxReceiver : ISyntaxReceiver
    {
        public List<RecordDeclarationSyntax> RecordDeclarations { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is RecordDeclarationSyntax record &&
                record.AttributeLists.Any(al => 
                    al.Attributes.Any(a => a.Name.ToString() == "Serializable")))
            {
                RecordDeclarations.Add(record);
            }
        }
    }
}

// Usage: Attribute-driven generation
[Serializable]
public partial record Order(long OrderId, double Price, int Quantity);

// Generated code provides:
// - WriteTo(Span<byte>)
// - ReadFrom(ReadOnlySpan<byte>)
// - GetHashCode()
// - Equals()
```

**V12 DNA Alignment:**
- ✅ Zero-runtime cost: Generated at compile time
- ✅ Type-safe: Compiler-enforced
- ✅ Maintainable: Single source of truth
- ✅ Fast: Incremental generation

**DO:**
- ✅ Use source generators for repetitive code
- ✅ Generate serialization, equality, hashing
- ✅ Use attributes to drive generation
- ✅ Test generated code

**DON'T:**
- ❌ Generate complex logic (keep it simple)
- ❌ Ignore generated code in reviews
- ❌ Use reflection at runtime (use source generators)
- ❌ Generate code manually (use tools)

---

## Pattern 5: Continuous Testing (Test Automation)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Inline tests with ppx_inline_test *)
let%test "order matching" =
  let book = OrderBook.empty in
  let book = OrderBook.add book buy_order in
  let book = OrderBook.add book sell_order in
  let matches = OrderBook.match_orders book in
  List.length matches = 1

(* Tests run automatically on file save *)
(* dune runtest --watch *)
```

### V12 Translation (C#)

```csharp
// V12: Continuous testing with dotnet watch
// Mise integration
/*
[tasks.test-watch]
run = "dotnet watch test --filter 'FullyQualifiedName~UnitTests'"

[tasks.test-fast]
run = "dotnet test --filter 'Category=Fast'"

[tasks.test-all]
run = "dotnet test"
*/

// Fast test categorization
[Trait("Category", "Fast")]
public class OrderBookFastTests
{
    [Fact]
    public void AddOrder()
    {
        var book = OrderBook.Empty.Add(new Order(1, 100.0, 10));
        Assert.Equal(1, book.Size);
    }
}

[Trait("Category", "Slow")]
public class OrderBookSlowTests
{
    [Fact]
    public async Task StressTest()
    {
        // Slow test (>1 second)
    }
}

// VS Code tasks.json
/*
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "test-watch",
      "command": "dotnet",
      "args": ["watch", "test", "--filter", "Category=Fast"],
      "type": "shell",
      "isBackground": true,
      "problemMatcher": "$msCompile"
    }
  ]
}
*/

// Pre-push hook (.git/hooks/pre-push)
/*
#!/bin/sh
dotnet test --filter 'Category=Fast' || {
  echo "Fast tests failed. Fix before pushing."
  exit 1
}
*/
```

**V12 DNA Alignment:**
- ✅ Fast: Tests run in <1 second
- ✅ Automated: Run on file save
- ✅ Categorized: Fast vs slow tests
- ✅ Enforced: Pre-push hooks

**DO:**
- ✅ Use watch mode during development
- ✅ Categorize tests by speed
- ✅ Run fast tests on every save
- ✅ Add pre-push hooks

**DON'T:**
- ❌ Run slow tests in watch mode
- ❌ Skip tests before pushing
- ❌ Write slow tests (keep them <10ms)
- ❌ Ignore test failures

---

## Pattern 6: Developer Productivity (IDE Integration)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Merlin for IDE integration *)
(* .merlin *)
(*
S src
B _build/default/src
PKG core async
*)

(* Provides: *)
(* - Type inference on hover *)
(* - Jump to definition *)
(* - Auto-completion *)
(* - Error highlighting *)
```

### V12 Translation (C#)

```csharp
// V12: OmniSharp + VS Code integration
// .vscode/settings.json
/*
{
  "omnisharp.enableRoslynAnalyzers": true,
  "omnisharp.enableEditorConfigSupport": true,
  "omnisharp.enableImportCompletion": true,
  
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll": true,
    "source.organizeImports": true
  },
  
  "csharp.inlayHints.parameters.enabled": true,
  "csharp.inlayHints.types.enabled": true,
  
  "files.watcherExclude": {
    "**/.git/objects/**": true,
    "**/bin/**": true,
    "**/obj/**": true
  }
}
*/

// .vscode/launch.json
/*
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (console)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net8.0/V12.dll",
      "args": [],
      "cwd": "${workspaceFolder}",
      "console": "internalConsole",
      "stopAtEntry": false
    }
  ]
}
*/

// .vscode/extensions.json
/*
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "csharpier.csharpier-vscode",
    "eamodio.gitlens",
    "github.copilot"
  ]
}
*/
```

**V12 DNA Alignment:**
- ✅ Fast: IntelliSense in <100ms
- ✅ Accurate: Roslyn-powered
- ✅ Integrated: Format, lint, test in editor
- ✅ Consistent: Team-wide settings

**DO:**
- ✅ Use OmniSharp for C# development
- ✅ Enable inlay hints for type inference
- ✅ Configure format-on-save
- ✅ Share VS Code settings in repo

**DON'T:**
- ❌ Use outdated IDE extensions
- ❌ Disable IntelliSense (loses productivity)
- ❌ Ignore IDE warnings (technical debt)
- ❌ Skip IDE configuration (inconsistent experience)

---

## Pattern 7: Build Automation (CI/CD)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Automated CI with dune *)
(* .github/workflows/ci.yml *)
(*
name: CI
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: ocaml/setup-ocaml@v2
      - run: opam install . --deps-only
      - run: dune build
      - run: dune runtest
*)
```

### V12 Translation (C#)

```csharp
// V12: GitHub Actions with Mise
// .github/workflows/ci.yml
/*
name: V12 CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Mise
        uses: jdx/mise-action@v2
        with:
          install: true
      
      - name: Restore dependencies
        run: mise run restore
      
      - name: Build
        run: mise run build
      
      - name: Run fast tests
        run: mise run test-fast
      
      - name: Run linters
        run: mise run lint
      
      - name: Check formatting
        run: mise run format-check
      
      - name: Run complexity audit
        run: mise run complexity
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage.xml

  security-scan:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Run Snyk
        uses: snyk/actions/dotnet@master
        env:
          SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
      
      - name: Run Gitleaks
        uses: gitleaks/gitleaks-action@v2
*/

// Mise tasks for CI
/*
[tasks.ci]
depends = ["restore", "build", "test-fast", "lint", "format-check", "complexity"]

[tasks.restore]
run = "dotnet restore"

[tasks.build]
run = "dotnet build --no-restore"

[tasks.test-fast]
run = "dotnet test --no-build --filter 'Category=Fast'"

[tasks.lint]
run = "dotnet build /p:TreatWarningsAsErrors=true"

[tasks.format-check]
run = "dotnet csharpier --check ."

[tasks.complexity]
run = "python scripts/complexity_audit.py"
*/
```

**V12 DNA Alignment:**
- ✅ Fast: CI runs in <5 minutes
- ✅ Comprehensive: Build, test, lint, format
- ✅ Consistent: Mise ensures reproducibility
- ✅ Automated: Runs on every push

**DO:**
- ✅ Use Mise for CI reproducibility
- ✅ Run fast tests in CI
- ✅ Cache dependencies
- ✅ Fail fast on errors

**DON'T:**
- ❌ Run slow tests in CI (use nightly builds)
- ❌ Skip linting in CI
- ❌ Ignore CI failures
- ❌ Use different tools in CI vs local

---

## Summary Checklist

### Tools Patterns Compliance

- [ ] **Fast Compilation**: Use incremental builds with Mise
- [ ] **Automated Formatting**: Use CSharpier with pre-commit hooks
- [ ] **Static Analysis**: Use Roslyn Analyzers with custom V12 DNA rules
- [ ] **Code Generation**: Use Source Generators for boilerplate
- [ ] **Continuous Testing**: Use watch mode with fast test categorization
- [ ] **IDE Integration**: Use OmniSharp with team-wide VS Code settings
- [ ] **Build Automation**: Use GitHub Actions with Mise for reproducibility

### V12 DNA Compliance Matrix

| Pattern | Fast | Automated | Consistent | Enforced |
|---------|------|-----------|------------|----------|
| Fast Compilation | ✅ | ✅ | ✅ | ✅ |
| Automated Formatting | ✅ | ✅ | ✅ | ✅ |
| Static Analysis | ✅ | ✅ | ✅ | ✅ |
| Code Generation | ✅ | ✅ | ✅ | ✅ |
| Continuous Testing | ✅ | ✅ | ✅ | ✅ |
| IDE Integration | ✅ | ⚠️ | ✅ | ⚠️ |
| Build Automation | ✅ | ✅ | ✅ | ✅ |

**Legend**: ✅ Full compliance | ⚠️ Acceptable | ❌ Not applicable

---

## References

### Jane Street Resources
- **Firestore KB**: `henry_tools_for_traders_2025` (Building Tools for Traders)
- **Firestore KB**: `jane_street_trading_billions_2023` (Production Engineering When Trading Billions)

### V12 Standards
- [`AGENTS.md`](../../AGENTS.md) - Section 1.5: Agent Tool Requirements (Mise Integration)
- [`docs/protocol/MISE_IMPLEMENTATION_SUMMARY.md`](../../protocol/MISE_IMPLEMENTATION_SUMMARY.md)
- [`docs/protocol/BOB_MISE_INTEGRATION.md`](../../protocol/BOB_MISE_INTEGRATION.md)

### Related Documents
- [`JANE_STREET_TESTING_PATTERNS.md`](./JANE_STREET_TESTING_PATTERNS.md) - Testing automation
- [`JANE_STREET_CODE_REVIEW.md`](./JANE_STREET_CODE_REVIEW.md) - Code review tools

### External Resources
- **Mise**: https://mise.jdx.dev/
- **CSharpier**: https://csharpier.com/
- **Roslyn Analyzers**: https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/

---

**Document Status**: ✅ Complete (7 patterns documented)  
**Next Review**: 2026-07-03  
**Maintainer**: V12 Architecture Team