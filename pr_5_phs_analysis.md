# PR #5 Project Health Score Analysis

**Generated**: 2026-06-06T01:38:00Z  
**PR**: #5  
**Branch**: (from gh pr checks output)

---

## Bot Check Results Summary

### ❌ FAILING (5 bots)
1. **Build & Run Pyramid Suites** - 1m10s
   - URL: https://github.com/antigravityos187-sketch/universal-or-strategy/actions/runs/27048675876/job/79839775352
   
2. **CodeFactor** - 35s
   - URL: https://www.codefactor.io/repository/github/antigravityos187-sketch/universal-or-strategy/pull/5
   
3. **SonarCloud** - 1m11s
   - URL: https://github.com/antigravityos187-sketch/universal-or-strategy/actions/runs/27048675860/job/79839775341
   
4. **Verify src/ vs non-src/ Separation** - 25s
   - URL: https://github.com/antigravityos187-sketch/universal-or-strategy/actions/runs/27048675868/job/79839775319
   
5. **codescene-delta** - 27s
   - URL: https://github.com/antigravityos187-sketch/universal-or-strategy/actions/runs/27048675861/job/79839775367

### ✅ PASSING (22 bots)
1. .github/dependabot.yml - 1s
2. Codacy Static Code Analysis - 0s
3. CodeQL - 2s
4. CodeQL (csharp, none) - 3m17s
5. CodeRabbit - 0s (Review skipped)
6. Compile NinjaScript (C# / .NET 4.8) - 33s
7. Gitar - 6m46s
8. Greptile Review - 6m36s
9. Label PR by changed files - 4s
10. SonarCloud Code Analysis - 24s
11. Test and Coverage - 48s
12. gitleaks (run 1) - 13s
13. gitleaks (run 2) - 15s
14. gitleaks (run 3) - 2s
15. lint - 32s
16. markdown-link-check - 5s
17. scan - 12s
18. update_release_draft - 4s
19. review - 28s
20. semgrep/ci - 26s
21. qlty check - 0s (No blocking issues)
22. qlty fmt - 0s (No formatting issues)
23. security/snyk - 0s (No manifest changes)

### ⏭️ SKIPPED (3 bots)
1. coverage - 0s
2. Mermaid Diagram Sync Assistant - 2s
3. Sourcery review - 0s

---

## PHS Score Calculation

**Formula**: `(Passing Bots / Total Active Bots) × 100`

**Calculation**:
- **Passing Bots**: 22
- **Failing Bots**: 5
- **Skipped Bots**: 3 (excluded from calculation)
- **Total Active Bots**: 22 + 5 = 27

**PHS Score**: `(22 / 27) × 100 = 81.48%`

---

## Comparison to Previous Score

- **Previous PHS**: 50% (reported)
- **Current PHS**: 81.48%
- **Improvement**: +31.48 percentage points ✅

---

## Critical Failures Analysis

### 1. Build & Run Pyramid Suites ❌
**Impact**: P0 - Blocks merge  
**Action Required**: Fix build/test failures

### 2. CodeFactor ❌
**Impact**: P1 - Code quality gate  
**Action Required**: Address code quality issues

### 3. SonarCloud ❌
**Impact**: P1 - Security/quality gate  
**Action Required**: Fix SonarCloud findings

### 4. Verify src/ vs non-src/ Separation ❌
**Impact**: P0 - V12 DNA violation  
**Action Required**: Enforce Three-Tier Branch Model (INFRASTRUCTURE_PROTOCOL.md)

### 5. codescene-delta ❌
**Impact**: P2 - Code health regression  
**Action Required**: Address hotspot/complexity issues

---

## Next Steps (PR Loop Protocol)

1. **Fix P0 Blockers** (Build & Separation)
   - Run `powershell -File .\scripts\build_readiness.ps1`
   - Verify branch separation compliance

2. **Address P1 Quality Gates** (CodeFactor, SonarCloud)
   - Review CodeFactor report
   - Fix SonarCloud security/quality findings

3. **Resolve P2 Issues** (codescene-delta)
   - Check CodeScene hotspot analysis
   - Refactor high-complexity areas if needed

4. **Re-run PHS Check**
   - Target: 100/100 (all bots passing)
   - Use `/pr-loop 5` to automate fixes

---

## Bot Status Legend

- ✅ **PASS**: Check completed successfully
- ❌ **FAIL**: Check failed, requires action
- ⏭️ **SKIP**: Check skipped (not counted in PHS)

---

## References

- **PR Loop Protocol**: `.bob/rules/00-pr-hygiene.md`
- **Branch Strategy**: `docs/protocol/BRANCH_STRATEGY.md`
- **Pre-Push Validation**: `scripts/pre_push_validation.ps1`