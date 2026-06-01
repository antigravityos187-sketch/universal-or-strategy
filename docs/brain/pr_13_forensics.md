# PR #13 Forensics Report
Generated: 2026-06-01 13:09:28

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 20 |
| VALID Issues | 20 |
| HALLUCINATIONS | 0 |
| INFRA-NOISE | 0 |
| P0 (Critical) | 7 |
| P1 (High) | 13 |
| P2 (Medium) | 0 |

## VALID Issues (Priority Order)

### [P0] CRITICAL - coderabbitai
**Source:** review  
**Timestamp:** 2026-06-01T17:46:36Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```


> [!CAUTION]
> Some comments are outside the diff and canÔÇÖt be posted inline due to platform limitations.
> 
> 
> 
> <details>
> <summary>ÔÜá´©Å Outside diff range comments (1)</summary><blockquote>
> 
> <details>
> <summary>src/V12_002.Orders.Callbacks.cs (1)</summary><blockquote>
> 
> `340-356`: _ÔÜá´©Å Potential issue_ | _­ƒƒá Major_ | _­ƒÅù´©Å Heavy lift_
> 
> **Commit `EntryFilled`/`InitialTargetCount` only after targets and stop are recalculated (success path).**
> 
> On the success pa
```

### [P0] CRITICAL - cubic-dev-ai
**Source:** review  
**Timestamp:** 2026-06-01T17:25:14Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```
**No issues found** across 1 file

<sub>YouÔÇÖre at about 95% of the monthly reviewed-line limit. You may want to [disable incremental reviews](https://www.cubic.dev/ai-review) to conserve quota. Reviews will continue until that limit is exceeded. If you need help avoiding interruptions, please contact contact@cubic.dev.</sub>

<sub>**Tip**: cubic could auto-approve low-risk PRs like this, if it thinks it's safe to merge. [Learn more](https://docs.cubic.dev/ai-review/ai-review-settings?utm_sourc
```

### [P0] CRITICAL - coderabbitai
**Source:** review  
**Timestamp:** 2026-06-01T18:40:03Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```
**Actionable comments posted: 5**

<details>
<summary>­ƒñû Prompt for all review comments with AI agents</summary>

```
Verify each finding against current code. Fix only still-valid issues, skip the
rest with a brief reason, keep changes minimal, and validate.

Inline comments:
In `@src/V12_002.PositionInfo.cs`:
- Around line 407-425: PendingStopReplacement is declared as a readonly struct
but defines mutable auto-properties and is constructed via object initializers
in Trailing.StopUpdate.cs a
```

### [P0] CRITICAL - cubic-dev-ai
**Source:** review  
**Timestamp:** 2026-06-01T19:45:49Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```
**2 issues found across 3 files (changes from recent commits).**

<sub>YouÔÇÖre at about 97% of the monthly reviewed-line limit. You may want to [disable incremental reviews](https://www.cubic.dev/ai-review) to conserve quota. Reviews will continue until that limit is exceeded. If you need help avoiding interruptions, please contact contact@cubic.dev.</sub>

<details>
<summary>Prompt for AI agents (unresolved issues)</summary>

```text

Check if these issues are valid ÔÇö if so, understand the r
```

### [P0] CRITICAL - cubic-dev-ai
**Source:** review  
**Timestamp:** 2026-06-01T18:43:24Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```
**2 issues found across 4 files (changes from recent commits).**

<sub>YouÔÇÖre at about 96% of the monthly reviewed-line limit. You may want to [disable incremental reviews](https://www.cubic.dev/ai-review) to conserve quota. Reviews will continue until that limit is exceeded. If you need help avoiding interruptions, please contact contact@cubic.dev.</sub>

<details>
<summary>Prompt for AI agents (unresolved issues)</summary>

```text

Check if these issues are valid ÔÇö if so, understand the r
```

### [P0] CRITICAL - codacy-production
**Source:** review  
**Timestamp:** 2026-06-01T17:23:33Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```
### Pull Request Overview

The refactoring of `HandleEntryOrderFilled` effectively achieves the goal of reducing cyclomatic complexity from 35 to 12. Codacy analysis indicates the changes are 'Up to Standards', although one new static analysis issue was introduced.

The primary risk in this PR is the extraction of complex price-guard and target-recalculation logic without accompanying unit tests. While the code structure is improved, there is no verification that the 12.0 point stop cap or RMA A
```

### [P0] CRITICAL - gitar-bot
**Source:** comment  
**Timestamp:** 2026-06-01T20:06:47Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13#issuecomment-4596070202

**Excerpt:**
```
<details open>
<summary><b>Code Review</b> <kbd>ÔÜá´©Å Changes requested</kbd> <kbd>2 resolved / 3 findings</kbd></summary>

Refactored HandleEntryOrderFilled to improve cyclomatic complexity and state atomicity, but requested changes remain due to insufficient validation for follower position stop distances and hardcoded risk parameters in the new dispatch helper.

<details>
<summary>ÔÜá´©Å <b>Edge Case:</b> No guard for zero stopDistance/targetDistance from unset masterPos</summary>

<kbd>­ƒôä
```

### [P1] REVIEW - codescene-delta-analysis
**Source:** review  
**Timestamp:** 2026-06-01T19:37:39Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```

[//]: # (cs-code-health)
[![](https://codescene.io/imgs/svg/pass1.svg)](#) Code Health Improved `(1 files improve in Code Health)`

**Gates Failed**
[![](https://codescene.io/imgs/svg/x1.svg)](#) Prevent hotspot decline `(1 hotspot with Large Method)`
[![](https://codescene.io/imgs/svg/x1.svg)](#) Enforce advisory code health rules `(2 files with Large Method, Complex Method)`

Our agent can fix these. [Install it.](https://codescene.io/docs/developer-tools/pr-refactoring-agent.html)

**Gates P
```

### [P1] REVIEW - amazon-q-developer
**Source:** review  
**Timestamp:** 2026-06-01T17:22:20Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```
## Review Summary

This PR successfully extracts `HandleEntryOrderFilled` logic into two helper methods, achieving the stated goal of reducing cyclomatic complexity from 35 to 12 (66% reduction). The refactoring is clean with no logic changes introduced.

**Verification:**
Ô£à Code extraction preserves original behavior
Ô£à No new defects introduced
Ô£à Complexity reduction achieved as documented
Ô£à All build and verification checks passing

The refactoring follows sound software engineering pr
```

### [P1] REVIEW - greptile-apps
**Source:** review  
**Timestamp:** 2026-06-01T19:35:50Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```
`backtothefutures83-oss` has reached the 50-review limit for trial accounts. To continue receiving code reviews, [upgrade your plan](https://app.greptile.com/review/github).
```

### [P1] REVIEW - codescene-delta-analysis
**Source:** review  
**Timestamp:** 2026-06-01T20:00:43Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```

[//]: # (cs-code-health)
[![](https://codescene.io/imgs/svg/pass1.svg)](#) Code Health Improved `(1 files improve in Code Health)`

**Gates Failed**
[![](https://codescene.io/imgs/svg/x1.svg)](#) Prevent hotspot decline `(1 hotspot with Large Method)`
[![](https://codescene.io/imgs/svg/x1.svg)](#) Enforce advisory code health rules `(2 files with Large Method, Complex Method)`

Our agent can fix these. [Install it.](https://codescene.io/docs/developer-tools/pr-refactoring-agent.html)

**Gates P
```

### [P1] REVIEW - greptile-apps
**Source:** review  
**Timestamp:** 2026-06-01T20:00:00Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```
`backtothefutures83-oss` has reached the 50-review limit for trial accounts. To continue receiving code reviews, [upgrade your plan](https://app.greptile.com/review/github).
```

### [P1] REVIEW - coderabbitai
**Source:** review  
**Timestamp:** 2026-06-01T19:42:37Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```
**Actionable comments posted: 1**

<details>
<summary>­ƒñû Prompt for all review comments with AI agents</summary>

```
Verify each finding against current code. Fix only still-valid issues, skip the
rest with a brief reason, keep changes minimal, and validate.

Inline comments:
In `@src/V12_002.Trailing.StopUpdate.cs`:
- Around line 210-236: Update UpdateExistingPendingReplacement to ensure the Add
path in pendingStopReplacements.AddOrUpdate performs the same
counter/circuit-breaker bookkeeping
```

### [P1] REVIEW - codescene-delta-analysis
**Source:** review  
**Timestamp:** 2026-06-01T17:35:43Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```

[//]: # (cs-code-health)
[![](https://codescene.io/imgs/svg/pass1.svg)](#) Code Health Improved `(1 files improve in Code Health)`



Our agent can fix these. [Install it.](https://codescene.io/docs/developer-tools/pr-refactoring-agent.html)

**Gates Passed**
[![](https://codescene.io/imgs/svg/pass1.svg)](#) 6 Quality Gates Passed


<details><summary>View Improvements</summary>

| File | Code Health Impact | Categories Improved |
|-|----|--|
| V12_002.Orders.Callbacks.cs | 4.31 ÔåÆ 4.47 | Compl
```

### [P1] REVIEW - sourcery-ai
**Source:** review  
**Timestamp:** 2026-06-01T17:23:25Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```
Hey - I've left some high level feedback:

- In `ValidateAndPrepareEntryFill` the `Order order` parameter is never used; consider removing it to keep the method signature minimal and avoid confusion about its purpose.
- The `ValidateAndPrepareEntryFill` return value is used in a negated form at the call site (`if (!ValidateAndPrepareEntryFill(...))`), which makes the control flow slightly harder to follow; consider inverting the return semantics or renaming the method to better reflect that `fal
```

### [P1] REVIEW - coderabbitai
**Source:** review  
**Timestamp:** 2026-06-01T17:23:56Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```
**Actionable comments posted: 1**

<details>
<summary>­ƒñû Prompt for all review comments with AI agents</summary>

```
Verify each finding against current code. Fix only still-valid issues, skip the
rest with a brief reason, keep changes minimal, and validate.

Inline comments:
In `@src/V12_002.Orders.Callbacks.cs`:
- Around line 320-328: The method ValidateAndPrepareEntryFill has an unused
parameter order; remove the order parameter from the method signature and from
any related overloads (the
```

### [P1] REVIEW - gemini-code-assist
**Source:** review  
**Timestamp:** 2026-06-01T17:22:49Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```
## Code Review

This pull request refactors the order callback logic in `src/V12_002.Orders.Callbacks.cs` by extracting entry fill validation and target/stop recalculation into two dedicated helper methods: `ValidateAndPrepareEntryFill` and `RecalculateTargetsAndStop`. The review feedback recommends removing the unused `order` parameter from `ValidateAndPrepareEntryFill` to simplify its signature, and adding defensive null checks for the `pos` parameter in both new methods to prevent potential `
```

### [P1] REVIEW - codescene-delta-analysis
**Source:** review  
**Timestamp:** 2026-06-01T17:22:23Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```

[//]: # (cs-code-health)
[![](https://codescene.io/imgs/svg/pass1.svg)](#) Code Health Improved `(1 files improve in Code Health)`



Our agent can fix these. [Install it.](https://codescene.io/docs/developer-tools/pr-refactoring-agent.html)

**Gates Passed**
[![](https://codescene.io/imgs/svg/pass1.svg)](#) 6 Quality Gates Passed


<details><summary>View Improvements</summary>

| File | Code Health Impact | Categories Improved |
|-|----|--|
| V12_002.Orders.Callbacks.cs | 4.31 ÔåÆ 4.47 | Compl
```

### [P1] REVIEW - codescene-delta-analysis
**Source:** review  
**Timestamp:** 2026-06-01T18:32:43Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```

[//]: # (cs-code-health)
[![](https://codescene.io/imgs/svg/pass1.svg)](#) Code Health Improved `(1 files improve in Code Health)`

**Gates Failed**
[![](https://codescene.io/imgs/svg/x1.svg)](#) Prevent hotspot decline `(1 hotspot with Large Method)`
[![](https://codescene.io/imgs/svg/x1.svg)](#) Enforce advisory code health rules `(2 files with Large Method, Complex Method)`

Our agent can fix these. [Install it.](https://codescene.io/docs/developer-tools/pr-refactoring-agent.html)

**Gates P
```

### [P1] REVIEW - codescene-delta-analysis
**Source:** review  
**Timestamp:** 2026-06-01T17:53:55Z  
**URL:** https://github.com/backtothefutures83-oss/universal-or-strategy/pull/13

**Excerpt:**
```

[//]: # (cs-code-health)
[![](https://codescene.io/imgs/svg/pass1.svg)](#) Code Health Improved `(1 files improve in Code Health)`



Our agent can fix these. [Install it.](https://codescene.io/docs/developer-tools/pr-refactoring-agent.html)

**Gates Passed**
[![](https://codescene.io/imgs/svg/pass1.svg)](#) 6 Quality Gates Passed


<details><summary>View Improvements</summary>

| File | Code Health Impact | Categories Improved |
|-|----|--|
| V12_002.Orders.Callbacks.cs | 4.31 ÔåÆ 4.47 | Compl
```

