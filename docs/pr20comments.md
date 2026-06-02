Src/epic posinfo ticket 01
#20
Open
backtothefutures83-oss
wants to merge 6 commits into
main
from
src/epic-posinfo-ticket-01
+50
-110
Lines changed: 50 additions & 110 deletions
Conversation0 (0)
Commits6 (6)
Checks3 (3)
Files changed1 (1)
Conversation
@backtothefutures83-oss
Owner
backtothefutures83-oss
commented
7 minutes ago
• 
User description
Mission Context
Build Tag:
Mission:

Files Changed
src/... —
Pre-Flight Checklist
Mandatory Gates (ALL must pass before merge)
 ASCII Gate: python check_ascii.py src/ — zero non-ASCII in C# strings
 Lock-Free Audit: grep -r "lock(" src/ — zero matches in strategy files
 Lint Pass: powershell -File .\scripts\lint.ps1 — LINT PASS confirmed
 Build Readiness: powershell -File .\scripts\build_readiness.ps1 — Build PASS
 AMAL Gate: python scripts/amal_harness.py — PASSED (Allocated = 0 B)
 Bob Shell Audit: Used v12-engineer mode with checkpointing: true
 Deploy Sync: powershell -File .\deploy-sync.ps1 — hard links re-established
 BUILD_TAG Banner: Verified in NinjaTrader Output window after F5 compile
Architecture Review
 No new lock() statements introduced
 All state mutations use Enqueue() actor model or Interlocked primitives
 _simaToggleSem released in finally blocks (if touched)
 No emoji, curly quotes, or em-dashes in Print() or string literals
Test Results
AMAL Benchmark Summary:
[paste AMAL output: Allocated = 0 B, Mean Latency < Baseline]
Agent Audit & Checkpoint
Bob Checkpoint ID:

 Gemini Standards Auditor review posted
 SonarCloud quality gate: PASSED
 No new P0/P1 SonarCloud issues introduced
Summary by Sourcery
Refine position target accessors to more concise conditional expressions while preserving zero-allocation behavior and target handling semantics.

Enhancements:

Replace switch-based target accessors with expression-bodied conditional chains for contracts, prices, filled status, and filled quantities.
Simplify target mutation methods to use bounded conditional logic instead of switch statements for marking targets filled and updating filled quantities.
Summary by cubic
Refactored target accessors in PositionInfo to expression-bodied members to cut code size and keep zero-allocation hot paths. Behavior and call sites remain unchanged, aligning with EPIC-POSINFO Ticket 01.

Refactors
Rewrote getters with ternary expression-bodied members; setters use simple if/else.
Reduced lines from 124 to 51 (~59%).
Preserved zero allocation and CYC ≤ 15.
No API changes; 32 call sites unchanged.
Written for commit 8095854. Summary will update on new commits.

Review in cubic

CodeAnt-AI Description
Simplify target position accessors without changing how target data is read or updated

What Changed
Replaced several target lookup blocks with shorter direct checks for target 1 through 5
Kept the same behavior for reading target contracts, prices, filled status, and filled quantity
Kept invalid target numbers ignored when marking fills or saving filled quantity
Impact
✅ Same target position behavior
✅ Clearer maintenance of position info
✅ No added memory use in target lookups

💡 Usage Guide
Summary by CodeRabbit
Refactor
Internal code optimization and simplification of helper methods to improve maintainability.
Note: This release contains no user-facing changes or new features.

backtothefutures83-oss added 6 commits 43 minutes ago
EPIC-POSINFO Phase 2: Analysis and Approach documents 
3977c5d
Merge branch 'main' of https://github.com/backtothefutures83-oss/univ… 
c1a2c71
Merge branch 'main' of https://github.com/backtothefutures83-oss/univ… 
1e2902f
Merge branch 'main' of https://github.com/backtothefutures83-oss/univ… 
77e8802
Merge branch 'main' of https://github.com/backtothefutures83-oss/univ… 
31be883
refactor: Convert switch-based accessors to expression-bodied members 
8095854
@qodo-code-review
qodo-code-review Bot
commented
6 minutes ago
Qodo reviews are paused for this user.
Troubleshooting steps vary by plan Learn more →

On a Teams plan?
Reviews resume once this user has a paid seat and their Git account is linked in Qodo.
Link Git account →

Using GitHub Enterprise Server, GitLab Self-Managed, or Bitbucket Data Center?
These require an Enterprise plan - Contact us
Contact us →

@codeant-ai
codeant-ai Bot
commented
6 minutes ago
CodeAnt AI is reviewing your PR.

@sourcery-ai
sourcery-ai Bot
commented
6 minutes ago
• 
Reviewer's Guide
Refactors target access helper methods in PositionInfo to use expression-bodied ternary chains and if/else branches instead of switch statements, preserving behavior while keeping zero-allocation hot paths explicit.

File-Level Changes
Change	Details	Files
Refactored target accessors and mutators from switch statements to expression-bodied methods and if/else chains while preserving existing semantics and bounds checks.	
Rewrote GetTargetContracts, GetTargetPrice, IsTargetFilled, and GetTargetFilledQuantity as expression-bodied methods using chained conditional operators instead of switch blocks.
Updated MarkTargetFilled and SetTargetFilledQuantity to replace switch statements with explicit if/else chains after validating target number range.
Preserved existing default/fallback return values and safety checks (invalid target numbers, non-negative filled quantities) to maintain current behavior in zero-allocation hot paths.
src/V12_002.PositionInfo.cs
Tips and commands
greptile-apps[bot]
greptile-apps Bot reviewed 6 minutes ago
greptile-apps Bot
left a comment
backtothefutures83-oss has reached the 50-review limit for trial accounts. To continue receiving code reviews, upgrade your plan.

@coderabbitai
coderabbitai Bot
commented
6 minutes ago
• 
Review Change Stack

Walkthrough
This PR refactors PositionInfo target accessor methods in src/V12_002.PositionInfo.cs. Read-only getters (GetTargetContracts, GetTargetPrice, IsTargetFilled) are converted from switch statements to ternary chains. State mutation helpers (MarkTargetFilled, GetTargetFilledQuantity, SetTargetFilledQuantity) are simplified to use conditional assignments instead of switch branching, reducing lines of code from 110 to 50.

Changes
PositionInfo Target Accessor Refactoring

Layer / File(s)	Summary
Target state getters
src/V12_002.PositionInfo.cs	GetTargetContracts, GetTargetPrice, and IsTargetFilled are converted from switch statements to expression-bodied ternary chains that directly return the matching pos.T* fields for target numbers 1–5, with default values for other inputs.
Target mutation and quantity helpers
src/V12_002.PositionInfo.cs	MarkTargetFilled simplified from switch-with-break to bounds check plus if/else assignments; GetTargetFilledQuantity and SetTargetFilledQuantity use ternary/conditional selection instead of switch, preserving filledQuantity clamping via Math.Max(0, filledQuantity).
Estimated code review effort
🎯 2 (Simple) | ⏱️ ~8 minutes

Possibly related PRs
backtothefutures83-oss/universal-or-strategy#14: Both modify PositionInfo.cs target-handling code including MarkTargetFilled and SetTargetFilledQuantity refactoring with bounds/guards.
backtothefutures83-oss/universal-or-strategy#15: Both refactor PositionInfo target/filled switch-based logic and related structures in the same file.
Suggested labels
refactor, code-quality, size:M

🚥 Pre-merge checks | ✅ 2 | ❌ 3
✨ Finishing Touches
Comment @coderabbitai help to get the list of available commands and usage tips.

amazon-q-developer[bot]
amazon-q-developer Bot reviewed 6 minutes ago
amazon-q-developer Bot
left a comment
Review Summary
This PR successfully refactors 5 helper methods from switch statements to ternary/if-else chains, reducing code from 129 lines to 69 lines. The changes maintain functional equivalence:

GetTargetContracts, GetTargetPrice, IsTargetFilled, and GetTargetFilledQuantity converted to expression-bodied members with ternary chains
MarkTargetFilled and SetTargetFilledQuantity simplified from switch statements to if-else chains
All refactored methods preserve the original logic, return values, and edge case handling. No critical issues found that would block merge.

You can now have the agent implement changes and create commits directly on your pull request's source branch. Simply comment with /q followed by your request in natural language to ask the agent to make changes.

codescene-delta-analysis[bot]
codescene-delta-analysis Bot approved these changes 6 minutes ago
codescene-delta-analysis Bot
left a comment
 Code Health Improved (1 files improve in Code Health)

Our agent can fix these. Install it.

Gates Passed
 6 Quality Gates Passed

View Improvements
Quality Gate Profile: Pay Down Tech Debt
Install CodeScene MCP: safeguard and uplift AI-generated code. Catch issues early with our IDE extension and CLI tool.

@github-actions
github-actions Bot
commented
6 minutes ago
Failed to generate code suggestions for PR

@codeant-ai codeant-ai Bot added the size:M label 6 minutes ago
@gitar-bot
gitar-bot Bot
commented
5 minutes ago
• 
Code Review ✅ Approved
Options
Was this helpful? React with 👍 / 👎 | Gitar

sourcery-ai[bot]
sourcery-ai Bot reviewed 5 minutes ago
sourcery-ai Bot
left a comment
Hey - I've found 1 issue, and left some high level feedback:

The chained ternary expressions in the GetTarget*/IsTargetFilled helpers are harder to read than the previous switch; consider using C# switch expressions for a more maintainable zero-allocation mapping if language version allows.
For MarkTargetFilled and SetTargetFilledQuantity, you can keep branch structure compact while preserving readability by using a switch/switch expression instead of a tall if/else if ladder.
Prompt for AI Agents
Sourcery is free for open source - if you like our reviews please consider sharing them ✨
Help me be more useful! Please click 👍 or 👎 on each comment and I'll use the feedback to improve your reviews.
src/V12_002.PositionInfo.cs
Comment on lines +277 to +278
        private int GetTargetContracts(PositionInfo pos, int targetNumber) =>
            targetNumber == 1 ? pos.T1Contracts
@sourcery-ai
sourcery-ai Bot
5 minutes ago
suggestion: The chained ternary accessors reduce readability and are harder to visually scan than the original switch-based form.

This nested ternary style is harder to read and modify, especially when scanning for a specific target or extending to more targets later. Since these are hot paths, a switch or C# switch expression should give comparable JIT optimization while keeping the logic clearer. Please prefer one of those forms here.

@backtothefutures83-oss	Reply...
gemini-code-assist[bot]
gemini-code-assist Bot reviewed 5 minutes ago
gemini-code-assist Bot
left a comment
Code Review
This pull request refactors several switch-based accessor and setter methods in src/V12_002.PositionInfo.cs into nested ternary operators and if-else chains. The review feedback correctly points out that these changes degrade performance from O(1) to O(N) in hot paths, violate the repository's style guide regarding dense one-liners, and introduce potential NullReferenceException risks due to a lack of null-guards on the pos parameter. The reviewer recommends utilizing C# 8.0 switch expressions with null-guards to resolve these issues elegantly.

Important

The consumer version of Gemini Code Assist on GitHub is being sunset. Starting June 18, 2026, new organization installations will be blocked, and all code review activity will officially cease on July 17, 2026.
For more details on the timeline and next steps, please review the Help Documentation.

src/V12_002.PositionInfo.cs
Comment on lines +277 to +283
        private int GetTargetContracts(PositionInfo pos, int targetNumber) =>
            targetNumber == 1 ? pos.T1Contracts
            : targetNumber == 2 ? pos.T2Contracts
            : targetNumber == 3 ? pos.T3Contracts
            : targetNumber == 4 ? pos.T4Contracts
            : targetNumber == 5 ? pos.T5Contracts
            : 0;
@gemini-code-assist
gemini-code-assist Bot
5 minutes ago
high

Performance & Style Improvement\n\n1. Performance (O(1) vs O(N)): Replacing the switch statement with nested ternary operators degrades performance. In C#, a switch statement/expression on contiguous integers compiles to a highly efficient jump table (O(1) complexity). A nested ternary chain compiles to sequential conditional branches (O(N) complexity), which is less efficient in hot paths.\n2. Style Guide Adherence: The nested ternary chain is dense and hard to read, violating the repository style guide's mandate to avoid dense one-liners and prioritize 'Metabolic Elegance' (Line 16).\n3. Defensive Programming: There is no null check on pos before accessing its properties, which can lead to a NullReferenceException if pos is null.\n\nUsing a C# 8.0 switch expression with a null-guard solves all three issues elegantly.
        private int GetTargetContracts(PositionInfo pos, int targetNumber) =>\n            pos == null ? 0 : targetNumber switch\n            {\n                1 => pos.T1Contracts,\n                2 => pos.T2Contracts,\n                3 => pos.T3Contracts,\n                4 => pos.T4Contracts,\n                5 => pos.T5Contracts,\n                _ => 0\n            };
References
@backtothefutures83-oss	Reply...
src/V12_002.PositionInfo.cs
Comment on lines +285 to +291
        private double GetTargetPrice(PositionInfo pos, int targetNumber) =>
            targetNumber == 1 ? pos.Target1Price
            : targetNumber == 2 ? pos.Target2Price
            : targetNumber == 3 ? pos.Target3Price
            : targetNumber == 4 ? pos.Target4Price
            : targetNumber == 5 ? pos.Target5Price
            : 0.0;
@gemini-code-assist
gemini-code-assist Bot
5 minutes ago
high

Performance & Style Improvement\n\n1. Performance (O(1) vs O(N)): Replacing the switch statement with nested ternary operators degrades performance. In C#, a switch statement/expression on contiguous integers compiles to a highly efficient jump table (O(1) complexity). A nested ternary chain compiles to sequential conditional branches (O(N) complexity), which is less efficient in hot paths.\n2. Style Guide Adherence: The nested ternary chain is dense and hard to read, violating the repository style guide's mandate to avoid dense one-liners and prioritize 'Metabolic Elegance' (Line 16).\n3. Defensive Programming: There is no null check on pos before accessing its properties, which can lead to a NullReferenceException if pos is null.\n\nUsing a C# 8.0 switch expression with a null-guard solves all three issues elegantly.
        private double GetTargetPrice(PositionInfo pos, int targetNumber) =>\n            pos == null ? 0.0 : targetNumber switch\n            {\n                1 => pos.Target1Price,\n                2 => pos.Target2Price,\n                3 => pos.Target3Price,\n                4 => pos.Target4Price,\n                5 => pos.Target5Price,\n                _ => 0.0\n            };
References
@backtothefutures83-oss	Reply...
src/V12_002.PositionInfo.cs
Comment on lines +293 to +299
        private bool IsTargetFilled(PositionInfo pos, int targetNumber) =>
            targetNumber == 1 ? pos.T1Filled
            : targetNumber == 2 ? pos.T2Filled
            : targetNumber == 3 ? pos.T3Filled
            : targetNumber == 4 ? pos.T4Filled
            : targetNumber == 5 ? pos.T5Filled
            : false;
@gemini-code-assist
gemini-code-assist Bot
5 minutes ago
high

Performance & Style Improvement\n\n1. Performance (O(1) vs O(N)): Replacing the switch statement with nested ternary operators degrades performance. In C#, a switch statement/expression on contiguous integers compiles to a highly efficient jump table (O(1) complexity). A nested ternary chain compiles to sequential conditional branches (O(N) complexity), which is less efficient in hot paths.\n2. Style Guide Adherence: The nested ternary chain is dense and hard to read, violating the repository style guide's mandate to avoid dense one-liners and prioritize 'Metabolic Elegance' (Line 16).\n3. Defensive Programming: There is no null check on pos before accessing its properties, which can lead to a NullReferenceException if pos is null.\n\nUsing a C# 8.0 switch expression with a null-guard solves all three issues elegantly.
        private bool IsTargetFilled(PositionInfo pos, int targetNumber) =>\n            pos != null && (targetNumber switch\n            {\n                1 => pos.T1Filled,\n                2 => pos.T2Filled,\n                3 => pos.T3Filled,\n                4 => pos.T4Filled,\n                5 => pos.T5Filled,\n                _ => false\n            });
References
@backtothefutures83-oss	Reply...
src/V12_002.PositionInfo.cs
Comment on lines 301 to 315
        private void MarkTargetFilled(PositionInfo pos, int targetNumber)
        {
            if (targetNumber < 1 || targetNumber > 5)
                return;
            switch (targetNumber)
            {
                case 1:
                    pos.T1Filled = true;
                    break;
                case 2:
                    pos.T2Filled = true;
                    break;
                case 3:
                    pos.T3Filled = true;
                    break;
                case 4:
                    pos.T4Filled = true;
                    break;
                case 5:
                    pos.T5Filled = true;
                    break;
            }
            if (targetNumber == 1)
                pos.T1Filled = true;
            else if (targetNumber == 2)
                pos.T2Filled = true;
            else if (targetNumber == 3)
                pos.T3Filled = true;
            else if (targetNumber == 4)
                pos.T4Filled = true;
            else if (targetNumber == 5)
                pos.T5Filled = true;
        }
@gemini-code-assist
gemini-code-assist Bot
5 minutes ago
high

Performance & Safety Improvement\n\n1. Performance (O(1) vs O(N)): Replacing the switch statement with an if-else chain degrades performance. In C#, a switch statement on contiguous integers compiles to a highly efficient jump table (O(1) complexity). An if-else chain compiles to sequential conditional branches (O(N) complexity), which is less efficient in hot paths.\n2. Defensive Programming: There is no null check on pos before accessing its properties, which can lead to a NullReferenceException if pos is null.\n\nReverting to a switch statement with a null-guard solves both issues.
        private void MarkTargetFilled(PositionInfo pos, int targetNumber)\n        {\n            if (pos == null || targetNumber < 1 || targetNumber > 5)\n                return;\n            switch (targetNumber)\n            {\n                case 1: pos.T1Filled = true; break;\n                case 2: pos.T2Filled = true; break;\n                case 3: pos.T3Filled = true; break;\n                case 4: pos.T4Filled = true; break;\n                case 5: pos.T5Filled = true; break;\n            }\n        }
@backtothefutures83-oss	Reply...
src/V12_002.PositionInfo.cs
Comment on lines +317 to +323
        private int GetTargetFilledQuantity(PositionInfo pos, int targetNumber) =>
            targetNumber == 1 ? pos.T1FilledQuantity
            : targetNumber == 2 ? pos.T2FilledQuantity
            : targetNumber == 3 ? pos.T3FilledQuantity
            : targetNumber == 4 ? pos.T4FilledQuantity
            : targetNumber == 5 ? pos.T5FilledQuantity
            : 0;
@gemini-code-assist
gemini-code-assist Bot
5 minutes ago
high

Performance & Style Improvement\n\n1. Performance (O(1) vs O(N)): Replacing the switch statement with nested ternary operators degrades performance. In C#, a switch statement/expression on contiguous integers compiles to a highly efficient jump table (O(1) complexity). A nested ternary chain compiles to sequential conditional branches (O(N) complexity), which is less efficient in hot paths.\n2. Style Guide Adherence: The nested ternary chain is dense and hard to read, violating the repository style guide's mandate to avoid dense one-liners and prioritize 'Metabolic Elegance' (Line 16).\n3. Defensive Programming: There is no null check on pos before accessing its properties, which can lead to a NullReferenceException if pos is null.\n\nUsing a C# 8.0 switch expression with a null-guard solves all three issues elegantly.
        private int GetTargetFilledQuantity(PositionInfo pos, int targetNumber) =>\n            pos == null ? 0 : targetNumber switch\n            {\n                1 => pos.T1FilledQuantity,\n                2 => pos.T2FilledQuantity,\n                3 => pos.T3FilledQuantity,\n                4 => pos.T4FilledQuantity,\n                5 => pos.T5FilledQuantity,\n                _ => 0\n            };
References
@backtothefutures83-oss	Reply...
src/V12_002.PositionInfo.cs
Comment on lines 325 to 340
        private void SetTargetFilledQuantity(PositionInfo pos, int targetNumber, int filledQuantity)
        {
            if (targetNumber < 1 || targetNumber > 5)
                return;
            int safeQty = Math.Max(0, filledQuantity);
            switch (targetNumber)
            {
                case 1:
                    pos.T1FilledQuantity = safeQty;
                    break;
                case 2:
                    pos.T2FilledQuantity = safeQty;
                    break;
                case 3:
                    pos.T3FilledQuantity = safeQty;
                    break;
                case 4:
                    pos.T4FilledQuantity = safeQty;
                    break;
                case 5:
                    pos.T5FilledQuantity = safeQty;
                    break;
            }
            if (targetNumber == 1)
                pos.T1FilledQuantity = safeQty;
            else if (targetNumber == 2)
                pos.T2FilledQuantity = safeQty;
            else if (targetNumber == 3)
                pos.T3FilledQuantity = safeQty;
            else if (targetNumber == 4)
                pos.T4FilledQuantity = safeQty;
            else if (targetNumber == 5)
                pos.T5FilledQuantity = safeQty;
        }
@gemini-code-assist
gemini-code-assist Bot
5 minutes ago
high

Performance & Safety Improvement\n\n1. Performance (O(1) vs O(N)): Replacing the switch statement with an if-else chain degrades performance. In C#, a switch statement on contiguous integers compiles to a highly efficient jump table (O(1) complexity). An if-else chain compiles to sequential conditional branches (O(N) complexity), which is less efficient in hot paths.\n2. Defensive Programming: There is no null check on pos before accessing its properties, which can lead to a NullReferenceException if pos is null.\n\nReverting to a switch statement with a null-guard solves both issues.
        private void SetTargetFilledQuantity(PositionInfo pos, int targetNumber, int filledQuantity)\n        {\n            if (pos == null || targetNumber < 1 || targetNumber > 5)\n                return;\n            int safeQty = Math.Max(0, filledQuantity);\n            switch (targetNumber)\n            {\n                case 1: pos.T1FilledQuantity = safeQty; break;\n                case 2: pos.T2FilledQuantity = safeQty; break;\n                case 3: pos.T3FilledQuantity = safeQty; break;\n                case 4: pos.T4FilledQuantity = safeQty; break;\n                case 5: pos.T5FilledQuantity = safeQty; break;\n            }\n        }
@backtothefutures83-oss	Reply...
@sonarqubecloud
sonarqubecloud Bot
commented
5 minutes ago
Quality Gate Failed Quality Gate failed
Failed conditions
 D Maintainability Rating on New Code (required ≥ A)

See analysis details on SonarQube Cloud

 Catch issues before they fail your Quality Gate with our IDE extension  SonarQube for IDE

codacy-production[bot]
codacy-production Bot reviewed 5 minutes ago
codacy-production Bot
left a comment
Pull Request Overview
This PR cannot be properly reviewed because the code changes are not present in the diff. Although the metadata indicates the project is up to standards, this is likely an artifact of the empty file list.

All acceptance criteria, particularly those regarding zero-allocation performance and cyclomatic complexity, remain unverified. The lack of mission context and the omission of AMAL benchmark data are major gaps for a refactor of this type. Merging is blocked until the implementation is provided and the mandatory pre-flight checklist is completed.

About this PR
The AMAL Benchmark Summary is missing data. The claims of zero-allocation performance and latency maintenance must be backed by benchmark results before approval.
The pre-flight checklist (ASCII, Lock-Free, and Lint gates) is completely unchecked. These gates are mandatory for ensuring the integrity of the refactor.
The PR contains no code changes or visible diff. The proposed refactoring of PositionInfo target accessors cannot be evaluated or verified without the source code.
Mission Context and Build Tag fields are empty. Please populate these to provide necessary operational context for the changes.
Test suggestions
 Verify target contract accessor returns correct values for valid indices (1-5)
 Verify target price accessor returns correct values for valid indices (1-5)
 Verify filled status accessor returns correct values for valid indices (1-5)
 Verify MarkTargetFilled correctly updates state for valid indices
 Verify SaveTargetFilledQuantity correctly updates state for valid indices
 Verify invalid target indices (outside 1-5) are handled without memory allocation
Prompt proposal for missing tests
TIP Improve review quality by adding custom instructions
TIP How was this review? Give us feedback

@codeant-ai
codeant-ai Bot
commented
4 minutes ago
CodeAnt AI finished reviewing your PR.

@codacy-production
codacy-production Bot
commented
3 minutes ago
Up to standards ✅
🟢 Issues 5 minor
🟢 Metrics 0 complexity · 0 duplication
AI Reviewer: first review requested successfully. AI can make mistakes. Always validate suggestions.

Run reviewer

TIP This summary will be updated as you push new changes.

coderabbitai[bot]
coderabbitai Bot requested changes 3 minutes ago
coderabbitai Bot
left a comment
Actionable comments posted: 2

🤖 Prompt for all review comments with AI agents
🪄 Autofix (Beta)
ℹ️ Review info
src/V12_002.PositionInfo.cs
Comment on lines +277 to +339
        private int GetTargetContracts(PositionInfo pos, int targetNumber) =>
            targetNumber == 1 ? pos.T1Contracts
            : targetNumber == 2 ? pos.T2Contracts
            : targetNumber == 3 ? pos.T3Contracts
            : targetNumber == 4 ? pos.T4Contracts
            : targetNumber == 5 ? pos.T5Contracts
            : 0;

        private double GetTargetPrice(PositionInfo pos, int targetNumber) =>
            targetNumber == 1 ? pos.Target1Price
            : targetNumber == 2 ? pos.Target2Price
            : targetNumber == 3 ? pos.Target3Price
            : targetNumber == 4 ? pos.Target4Price
            : targetNumber == 5 ? pos.Target5Price
            : 0.0;

        private bool IsTargetFilled(PositionInfo pos, int targetNumber) =>
            targetNumber == 1 ? pos.T1Filled
            : targetNumber == 2 ? pos.T2Filled
            : targetNumber == 3 ? pos.T3Filled
            : targetNumber == 4 ? pos.T4Filled
            : targetNumber == 5 ? pos.T5Filled
            : false;

        private void MarkTargetFilled(PositionInfo pos, int targetNumber)
        {
            if (targetNumber < 1 || targetNumber > 5)
                return;
            switch (targetNumber)
            {
                case 1:
                    pos.T1Filled = true;
                    break;
                case 2:
                    pos.T2Filled = true;
                    break;
                case 3:
                    pos.T3Filled = true;
                    break;
                case 4:
                    pos.T4Filled = true;
                    break;
                case 5:
                    pos.T5Filled = true;
                    break;
            }
            if (targetNumber == 1)
                pos.T1Filled = true;
            else if (targetNumber == 2)
                pos.T2Filled = true;
            else if (targetNumber == 3)
                pos.T3Filled = true;
            else if (targetNumber == 4)
                pos.T4Filled = true;
            else if (targetNumber == 5)
                pos.T5Filled = true;
        }

        private int GetTargetFilledQuantity(PositionInfo pos, int targetNumber)
        {
            switch (targetNumber)
            {
                case 1:
                    return pos.T1FilledQuantity;
                case 2:
                    return pos.T2FilledQuantity;
                case 3:
                    return pos.T3FilledQuantity;
                case 4:
                    return pos.T4FilledQuantity;
                case 5:
                    return pos.T5FilledQuantity;
                default:
                    return 0;
            }
        }
        private int GetTargetFilledQuantity(PositionInfo pos, int targetNumber) =>
            targetNumber == 1 ? pos.T1FilledQuantity
            : targetNumber == 2 ? pos.T2FilledQuantity
            : targetNumber == 3 ? pos.T3FilledQuantity
            : targetNumber == 4 ? pos.T4FilledQuantity
            : targetNumber == 5 ? pos.T5FilledQuantity
            : 0;

        private void SetTargetFilledQuantity(PositionInfo pos, int targetNumber, int filledQuantity)
        {
            if (targetNumber < 1 || targetNumber > 5)
                return;
            int safeQty = Math.Max(0, filledQuantity);
            switch (targetNumber)
            {
                case 1:
                    pos.T1FilledQuantity = safeQty;
                    break;
                case 2:
                    pos.T2FilledQuantity = safeQty;
                    break;
                case 3:
                    pos.T3FilledQuantity = safeQty;
                    break;
                case 4:
                    pos.T4FilledQuantity = safeQty;
                    break;
                case 5:
                    pos.T5FilledQuantity = safeQty;
                    break;
            }
            if (targetNumber == 1)
                pos.T1FilledQuantity = safeQty;
            else if (targetNumber == 2)
                pos.T2FilledQuantity = safeQty;
            else if (targetNumber == 3)
                pos.T3FilledQuantity = safeQty;
            else if (targetNumber == 4)
                pos.T4FilledQuantity = safeQty;
            else if (targetNumber == 5)
                pos.T5FilledQuantity = safeQty;
@coderabbitai
coderabbitai Bot
3 minutes ago
⚠️ Potential issue | 🟠 Major

🧩 Analysis chain
Block merge: missing required /loop-critic (P4) and /forensics artifacts

/loop-critic output for the src/V12_002.PositionInfo.cs change (lines 277-339) is not attached.
/forensics evidence is required even though src/V12_002.PositionInfo.cs is ASCII-only and lock(stateLock) appears only in comments in src/V12_002.SIMA.cs / src/V12_002.Orders.Management.StopSync.cs, with no actual lock statements shown in those matches.
🧰 Tools
🤖 Prompt for AI Agents
@backtothefutures83-oss	Reply...
src/V12_002.PositionInfo.cs
Comment on lines +305 to +314
            if (targetNumber == 1)
                pos.T1Filled = true;
            else if (targetNumber == 2)
                pos.T2Filled = true;
            else if (targetNumber == 3)
                pos.T3Filled = true;
            else if (targetNumber == 4)
                pos.T4Filled = true;
            else if (targetNumber == 5)
                pos.T5Filled = true;
@coderabbitai
coderabbitai Bot
3 minutes ago
⚠️ Potential issue | 🟡 Minor | ⚡ Quick win

Add braces in the touched mutators to clear SA1503.

These branches are functionally fine, but the current form introduces repeated StyleCop warnings in the modified block. Wrapping each branch now keeps the file clean and avoids accidental logic drift when more statements get added later.

Proposed fix
Also applies to: 330-339

🧰 Tools