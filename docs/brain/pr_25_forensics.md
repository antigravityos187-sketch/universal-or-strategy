# PR #25 Forensics Report
Generated: 2026-06-03 08:44:14

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 38 |
| VALID Issues | 38 |
| HALLUCINATIONS | 0 |
| INFRA-NOISE | 0 |
| P0 (Critical) | 6 |
| P1 (High) | 32 |
| P2 (Medium) | 0 |

## VALID Issues (Priority Order)

### [P0] CRITICAL - gemini-code-assist[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:21Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345434712

**Excerpt:**
```
scripts/pre_push_validation.ps1:485 - ![medium](https://www.gstatic.com/codereviewagent/medium-priority.svg)

If `cs delta` fails with a non-zero exit code due to an actual error (such as an invalid/expired `CS_ACCESS_TOKEN` or network failure), the `else` block will execute and report a `PASS` with the message `'No staged changes to analyze'`. This can silently mask configuration or authentication errors. Consider checking if there a
```

### [P0] CRITICAL - codacy-production
**Source:** review  
**Timestamp:** 2026-06-03T01:38:51Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25

**Excerpt:**
```
### Pull Request Overview

While the PR successfully reduces cyclomatic complexity in `MonitorRmaProximity` from 32 to 7, it fails to meet the project's 'Mandatory Braces' standard as explicitly claimed in the description. Several control structures in `src/V12_002.Entries.RMA.cs` lack curly braces, which the Quality agent flagged as potential maintenance risks. 

Furthermore, there is a high-severity logic error in the `epic_planner.py` tool: the severity scoring logic incorrectly penalizes cle
```

### [P0] CRITICAL - coderabbitai
**Source:** review  
**Timestamp:** 2026-06-03T01:43:14Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25

**Excerpt:**
```
**Actionable comments posted: 1**

> [!CAUTION]
> Some comments are outside the diff and canÔÇÖt be posted inline due to platform limitations.
> 
> 
> 
> <details>
> <summary>ÔÜá´©Å Outside diff range comments (1)</summary><blockquote>
> 
> <details>
> <summary>.bob/commands/epic-tdd.md (1)</summary><blockquote>
> 
> `68-85`: _ÔÜá´©Å Potential issue_ | _­ƒƒá Major_ | _ÔÜí Quick win_
> 
> **Restore mandatory `/epic-scan` in Gate 2.3.**
> 
> Gate 2.3 currently replaces the formal slash-command ver
```

### [P0] CRITICAL - codescene-delta-analysis
**Source:** review  
**Timestamp:** 2026-06-03T01:37:58Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25

**Excerpt:**
```

[//]: # (cs-code-health)
[![](https://codescene.io/imgs/svg/pass1.svg)](#) Code Health Improved `(1 files improve in Code Health)`

**Gates Failed**
[![](https://codescene.io/imgs/svg/x1.svg)](#) Prevent hotspot decline `(1 hotspot with Overall Code Complexity)`
[![](https://codescene.io/imgs/svg/x1.svg)](#) New code is healthy `(1 new file with code health below 10.00)`
[![](https://codescene.io/imgs/svg/x1.svg)](#) Enforce critical code health rules `(1 file with Bumpy Road Ahead, Deep, Neste
```

### [P0] CRITICAL - gitar-bot
**Source:** comment  
**Timestamp:** 2026-06-03T01:37:11Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#issuecomment-4608350945

**Excerpt:**
```
<details>
<summary><b>CI failed</b>: The build failed due to a type mismatch in the test suite and a violation of the PR separation policy, alongside some infrastructure issues with the runner environment.</summary>

### Overview
Multiple critical failures were identified in this CI run, primarily driven by a code refactoring error, a policy violation regarding PR contents, and isolated environment issues where the runner failed to detect a git repository.

### Failures

#### Build Violation: PR
```

### [P0] CRITICAL - amazon-q-developer
**Source:** review  
**Timestamp:** 2026-06-03T01:37:28Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25

**Excerpt:**
```
## Review Summary

I've completed the review of PR #25. The changes successfully achieve the stated goals of reducing complexity through well-structured refactoring.

### Key Changes Reviewed

**1. `src/V12_002.Entries.RMA.cs` - MonitorRmaProximity Refactoring Ô£à**
- Successfully extracted 4 helper methods from `MonitorRmaProximity`
- Complexity reduced from 32 ÔåÆ 7 (78% reduction), meeting the CYC Ôëñ 15 threshold
- Clear orchestrator pattern with focused helper methods:
  - `ShouldMonitorOrd
```

### [P1] INLINE - gitar-bot[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:39Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435695

**Excerpt:**
```
scripts/epic_planner.py:112 - <details>
<summary>­ƒÆí <b>Quality:</b> epic_number assigned before sort, so rank != epic number</summary>

In scripts/epic_planner.py `generate_epic_roadmap`, `epic_number` is computed as `f'EPIC-CCN-{12 + idx}'` using the pre-sort enumeration index (lines 98-109). The list is then re-sorted by `composite_score` descending (line 135) and printed with a separate `idx` rank in `print_roadmap`. As a
```

### [P1] INLINE - qltysh[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:40Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435821

**Excerpt:**
```
.github/workflows/codescene-quality-gate.yml:22 - credential persistence through GitHub Actions artifacts <i>[zizmor:zizmor/artipacked]</i><a href="https://qlty.sh/gh/backtothefutures83-oss/projects/universal-or-strategy/issues/zizmor/zizmor/artipacked/issues/cddab412145969f0f91eec5f4f050bd6"></a>
```

### [P1] INLINE - gitar-bot[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:40Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435818

**Excerpt:**
```
.github/workflows/codescene-quality-gate.yml:56 - <details>
<summary>­ƒÆí <b>Quality:</b> CI delta step builds pr_changes.diff but never uses it</summary>

In .github/workflows/codescene-quality-gate.yml the delta step writes `git diff origin/${{ github.base_ref }}...HEAD > pr_changes.diff` (line 44) but then runs `cs delta --output-format json` (line 47) which never references pr_changes.diff, making the diff file dead work. Additionally, the ga
```

### [P1] INLINE - gitar-bot[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:38Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435582

**Excerpt:**
```
scripts/epic_planner.py:44 - <details>
<summary>ÔÜá´©Å <b>Bug:</b> Non-ASCII emoji in epic_planner.py can crash on Windows</summary>

scripts/epic_planner.py still contains two emoji characters despite commit 416d557 ("Remove Unicode from epic_planner.py (ASCII compliance)"):

- Line 44: `print(f"\U0001F50D Reviewing {file_path} with CodeScene CLI...")`
- Line 166: `print(f" \U0001F4BE Roadmap saved to: {output_path}")`

Impa
```

### [P1] INLINE - sourcery-ai[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:36Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435509

**Excerpt:**
```
.github/workflows/codescene-quality-gate.yml:55 - **issue:** jq expression assumes `.review` is always an array, which may break on missing/empty review data

This jq expression, `[.review[] | select(.indication >= 3)] | length`, will error if `.review` is `null` or missing (e.g., schema changes or partial responses). To make the step resilient, guard with a default: `[ (.review // [])[] | select(.indication >= 3) ] | length`.
```

### [P1] INLINE - sourcery-ai[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:36Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435507

**Excerpt:**
```
scripts/epic_planner.py:184 - **suggestion (bug_risk):** Argument parsing for `--top-n` can raise IndexError or ValueError on malformed input

This logic assumes `--top-n` is followed by a valid integer. If `--top-n` is last, `sys.argv[idx + 1]` will raise `IndexError`, and nonÔÇænumeric input will raise `ValueError`. For a CLI used repeatedly, consider validating `idx + 1 < len(sys.argv)` and handling `ValueError` to emit a c
```

### [P1] INLINE - sourcery-ai[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:36Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435512

**Excerpt:**
```
.bob/commands/epic-run.md:52 - **nitpick (typo):** Consider clarifying whether "target" should be singular or plural in this sentence.

Since "epic $1" is singular, consider using "target" instead of "targets" if there is only one target per epic: `Confirm epic $1 target: $2.`

```suggestion
**GATE 0:**
> "Top 5 refactoring candidates: [list with scores]. Confirm epic $1 target: $2. Reply YES to proceed or ADJUST to change targ
```

### [P1] INLINE - qltysh[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:40Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435833

**Excerpt:**
```
.github/workflows/codescene-quality-gate.yml:27 - Found 2 issues:<br/><br/>1. code injection via template expansion <i>[zizmor:zizmor/template-injection]</i><a href="https://qlty.sh/gh/backtothefutures83-oss/projects/universal-or-strategy/issues/zizmor/zizmor/template-injection/issues/0a2b4270deaf444dfb7a62872a3815e5"></a><br/><br/>
2. code injection via template expansion <i>[zizmor:zizmor/template-injection]</i><a href="https://qlty.sh/gh/backt
```

### [P1] INLINE - qltysh[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:40Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435862

**Excerpt:**
```
.github/workflows/codescene-quality-gate.yml:157 - code injection via template expansion <i>[zizmor:zizmor/template-injection]</i><a href="https://qlty.sh/gh/backtothefutures83-oss/projects/universal-or-strategy/issues/zizmor/zizmor/template-injection/issues/6b9614bf951a0e4d0fcde23460f61c70"></a>
```

### [P1] INLINE - qltysh[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:40Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435857

**Excerpt:**
```
.github/workflows/codescene-quality-gate.yml:156 - code injection via template expansion <i>[zizmor:zizmor/template-injection]</i><a href="https://qlty.sh/gh/backtothefutures83-oss/projects/universal-or-strategy/issues/zizmor/zizmor/template-injection/issues/c4285228ab7af621eb3c7a7819ed80eb"></a>
```

### [P1] INLINE - qltysh[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:40Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435871

**Excerpt:**
```
docs/protocol/CODESCENE_INTEGRATION.md:1 - Incorrect formatting, autoformat by running <code>qlty fmt</code>\. <i>[markdownlint:fmt]</i><a href="https://qlty.sh/gh/backtothefutures83-oss/projects/universal-or-strategy/issues/markdownlint/fmt/issues/f71f1a07981e294614f75e47490d1286"></a>
```

### [P1] INLINE - qltysh[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:40Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435866

**Excerpt:**
```
.github/workflows/codescene-quality-gate.yml:158 - code injection via template expansion <i>[zizmor:zizmor/template-injection]</i><a href="https://qlty.sh/gh/backtothefutures83-oss/projects/universal-or-strategy/issues/zizmor/zizmor/template-injection/issues/8a46208d83d66cf88d4aba0af83d0d61"></a>
```

### [P1] INLINE - qltysh[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:40Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435845

**Excerpt:**
```
.github/workflows/codescene-quality-gate.yml:92 - code injection via template expansion <i>[zizmor:zizmor/template-injection]</i><a href="https://qlty.sh/gh/backtothefutures83-oss/projects/universal-or-strategy/issues/zizmor/zizmor/template-injection/issues/4cc6be8fe856e81d53c1c70d4bcae9b3"></a>
```

### [P1] INLINE - qltysh[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:40Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435838

**Excerpt:**
```
.github/workflows/codescene-quality-gate.yml:44 - code injection via template expansion <i>[zizmor:zizmor/template-injection]</i><a href="https://qlty.sh/gh/backtothefutures83-oss/projects/universal-or-strategy/issues/zizmor/zizmor/template-injection/issues/ce584b71d4de34f8d110fb011a4a6bb7"></a>
```

### [P1] INLINE - qltysh[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:40Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435855

**Excerpt:**
```
.github/workflows/codescene-quality-gate.yml:94 - code injection via template expansion <i>[zizmor:zizmor/template-injection]</i><a href="https://qlty.sh/gh/backtothefutures83-oss/projects/universal-or-strategy/issues/zizmor/zizmor/template-injection/issues/810b9f1465a52346b4d42482f141f1f6"></a>
```

### [P1] INLINE - qltysh[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:40Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435849

**Excerpt:**
```
.github/workflows/codescene-quality-gate.yml:93 - code injection via template expansion <i>[zizmor:zizmor/template-injection]</i><a href="https://qlty.sh/gh/backtothefutures83-oss/projects/universal-or-strategy/issues/zizmor/zizmor/template-injection/issues/0faec43e04f269489267441f0efe93b7"></a>
```

### [P1] INLINE - codescene-delta-analysis[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:37:56Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345433412

**Excerpt:**
```
scripts/epic_planner.py:214 - 
[//]: # (cs-code-health)
ÔØî New issue: [**Bumpy Road Ahead**](https://codescene.io/projects/80699/delta?repo-id=1583022&review-id=25&biomarker=Bumpy+Road+Ahead&filename=scripts%2Fepic_planner.py&method=main)
main has 2 blocks with nested conditional logic. Any nesting of 2 or deeper is considered. Threshold is 2 blocks per function

<sub>[Suppress](https://codescene.io/projects/80699/delta?repo-
```

### [P1] INLINE - codescene-delta-analysis[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:37:56Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345433400

**Excerpt:**
```
scripts/epic_planner.py:21 - 
[//]: # (cs-code-health)
ÔØî New issue: [**Complex Conditional**](https://codescene.io/projects/80699/delta?repo-id=1583022&review-id=25&biomarker=Complex+Conditional&filename=scripts%2Fepic_planner.py&method=load_env)
load_env has 1 complex conditionals with 2 branches, threshold = 2

<sub>[Suppress](https://codescene.io/projects/80699/delta?repo-id=1583022&review-id=25&biomarker=Complex+Conditi
```

### [P1] INLINE - codescene-delta-analysis[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:37:56Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345433444

**Excerpt:**
```
scripts/epic_planner.py:137 - 
[//]: # (cs-code-health)
ÔØî New issue: [**Deep, Nested Complexity**](https://codescene.io/projects/80699/delta?repo-id=1583022&review-id=25&biomarker=Deep%2C+Nested+Complexity&filename=scripts%2Fepic_planner.py&method=generate_epic_roadmap)
generate_epic_roadmap has a nested complexity depth of 5, threshold = 4

<sub>[Suppress](https://codescene.io/projects/80699/delta?repo-id=1583022&review-id=
```

### [P1] COMPLEXITY - codescene-delta-analysis[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:37:56Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345433429

**Excerpt:**
```
scripts/epic_planner.py:1 - 
[//]: # (cs-code-health)
ÔØî New issue: [**Overall Code Complexity**](https://codescene.io/projects/80699/delta?repo-id=1583022&review-id=25&biomarker=Overall+Code+Complexity&filename=scripts%2Fepic_planner.py&method=)
This module has a mean cyclomatic complexity of 4.88 across 8 functions. The mean complexity threshold is 4

<sub>[Suppress](https://codescene.io/projects/80699/delta?repo-id=15830
```

### [P1] REVIEW - gemini-code-assist
**Source:** review  
**Timestamp:** 2026-06-03T01:38:21Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25

**Excerpt:**
```
## Code Review

This pull request introduces CodeScene integration across the project, including a new integration protocol document, updates to the local pre-push validation script and command workflows, and a new `epic_planner.py` script. Additionally, `MonitorRmaProximity` in `V12_002.Entries.RMA.cs` is refactored into smaller helper methods to reduce cyclomatic complexity. The code review identified several issues: a potential `NullReferenceException` in the RMA monitor, falsy checks in Pyth
```

### [P1] REVIEW - greptile-apps
**Source:** review  
**Timestamp:** 2026-06-03T01:36:54Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25

**Excerpt:**
```
`backtothefutures83-oss` has reached the 50-review limit for trial accounts. To continue receiving code reviews, [upgrade your plan](https://app.greptile.com/review/github).
```

### [P1] COMPLEXITY - codescene-delta-analysis[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:37:55Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345433395

**Excerpt:**
```
scripts/epic_planner.py:214 - 
[//]: # (cs-code-health)
ÔØî New issue: [**Complex Method**](https://codescene.io/projects/80699/delta?repo-id=1583022&review-id=25&biomarker=Complex+Method&filename=scripts%2Fepic_planner.py&method=main)
main has a cyclomatic complexity of 10, threshold = 9

<sub>[Suppress](https://codescene.io/projects/80699/delta?repo-id=1583022&review-id=25&biomarker=Complex+Method&filename=scripts%2Fepic_pla
```

### [P1] REVIEW - sourcery-ai
**Source:** review  
**Timestamp:** 2026-06-03T01:38:36Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25

**Excerpt:**
```
Hey - I've found 4 issues, and left some high level feedback:

- The new `epic_planner.py` CLI expects a subcommand (`generate`/`review`), but the docs and .bob commands invoke `python scripts/epic_planner.py` with no arguments; either update the script to support a sensible default or adjust the documented commands to include the required subcommand and options.
- The new helper methods (e.g. `CalculateProximityDistance`, `HandleProximityExit`) introduce single-line `if` statements without brac
```

### [P1] INLINE - gemini-code-assist[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:21Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345434704

**Excerpt:**
```
scripts/epic_planner.py:148 - ![medium](https://www.gstatic.com/codereviewagent/medium-priority.svg)

If the CodeScene score is `0`, `epic['codescene_score']` evaluates to `False` and will display as `'N/A'` instead of `'0.0'`. Explicitly check if the score is not `None`.

```suggestion
        health = f"{epic['codescene_score']:.1f}" if epic['codescene_score'] is not None else "N/A"
```
```

### [P1] INLINE - gemini-code-assist[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:21Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345434699

**Excerpt:**
```
scripts/epic_planner.py:76 - ![high](https://www.gstatic.com/codereviewagent/high-priority.svg)

In Python, `0` is falsy. If the CodeScene score is `0` (representing the worst possible code health), `codescene.get('score')` will evaluate to `False`, resulting in a `code_health_penalty` of `0` instead of `30.0`. You should explicitly check if the score is not `None`.

```suggestion
    if codescene and codescene.get('score') i
```

### [P1] INLINE - sourcery-ai[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:36Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345435504

**Excerpt:**
```
scripts/epic_planner.py:107 - **suggestion:** Composite score implementation doesnÔÇÖt match the documented 40/30/20/10 weighting

The docstring for `calculate_composite_score` specifies a 40/30/20/10 weighting, but the code instead applies raw factors (hotspot * 0.4, code health * 3.0, severity * 2.0, churn * 0.5) without normalization, so the effective weights donÔÇÖt match the documentation. Please either normalize to the d
```

### [P1] INLINE - gemini-code-assist[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:21Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345434710

**Excerpt:**
```
scripts/epic_planner.py:59 - ![medium](https://www.gstatic.com/codereviewagent/medium-priority.svg)

If the CodeScene CLI (`cs`) is not installed or not available in the system's `PATH`, `subprocess.run` will raise a `FileNotFoundError` (which is an `OSError`, not a `CalledProcessError`). This will cause the script to crash. You should catch `OSError` or `FileNotFoundError` to handle this gracefully.

```python
    except sub
```

### [P1] INLINE - codescene-delta-analysis[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:37:57Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345433480

**Excerpt:**
```
scripts/pre_push_validation.ps1:438 - 
[//]: # (cs-code-health)
ÔØî Getting worse: [**Deep, Global Nested Complexity**](https://codescene.io/projects/80699/delta?repo-id=1583022&review-id=25&biomarker=Deep%2C+Global+Nested+Complexity&filename=scripts%2Fpre_push_validation.ps1&method=)
The global code outside of functions increases in nested complexity depth from 5 to 6, threshold = 4

<sub>[Suppress](https://codescene.io/projects/8069
```

### [P1] COMPLEXITY - codescene-delta-analysis[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:37:57Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345433466

**Excerpt:**
```
scripts/pre_push_validation.ps1:438 - 
[//]: # (cs-code-health)
ÔØî Getting worse: [**Global Conditionals**](https://codescene.io/projects/80699/delta?repo-id=1583022&review-id=25&biomarker=Global+Conditionals&filename=scripts%2Fpre_push_validation.ps1&method=)
The global code outside of functions increases in cyclomatic complexity from 63 to 72, threshold = 10

<sub>[Suppress](https://codescene.io/projects/80699/delta?repo-id=1583022
```

### [P1] INLINE - gemini-code-assist[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:38:21Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345434697

**Excerpt:**
```
src/V12_002.Entries.RMA.cs:426 - ![high](https://www.gstatic.com/codereviewagent/high-priority.svg)

If `activePositions.TryGetValue` succeeds but the retrieved `pos` is `null`, accessing `pos.IsRMATrade` will throw a `NullReferenceException`. To ensure robust defensive programming, add a null check for `pos` before accessing its properties.

```c#
            if (!activePositions.TryGetValue(entryName, out pos) || pos == null ||
```

### [P1] COMPLEXITY - codescene-delta-analysis[bot]
**Source:** inline  
**Timestamp:** 2026-06-03T01:37:57Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/25#discussion_r3345433492

**Excerpt:**
```
src/V12_002.Entries.RMA.cs:382 - 
[//]: # (cs-code-health)
ÔØî New issue: [**Overall Code Complexity**](https://codescene.io/projects/80699/delta?repo-id=1583022&review-id=25&biomarker=Overall+Code+Complexity&filename=src%2FV12_002.Entries.RMA.cs&method=)
This module has a mean cyclomatic complexity of 5.20 across 10 functions. The mean complexity threshold is 4

<sub>[Suppress](https://codescene.io/projects/80699/delta?repo-id=1
```

