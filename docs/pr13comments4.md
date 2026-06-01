New changes since you last viewed
fix(P0): Apply 3 critical atomic state fixes 
9a6a84c
@codeant-ai
codeant-ai Bot
commented
10 minutes ago
CodeAnt AI is running Incremental review

greptile-apps[bot]
greptile-apps Bot reviewed 10 minutes ago
greptile-apps Bot
left a comment
backtothefutures83-oss has reached the 50-review limit for trial accounts. To continue receiving code reviews, upgrade your plan.

codefactor-io[bot]
codefactor-io Bot reviewed 10 minutes ago
src/V12_002.SIMA.Dispatch.cs
            // Calculate stop and target prices based on entry price
            // Use simple fixed offsets for now (these should ideally come from strategy parameters)
            double tickSize = Instrument.MasterInstrument.TickSize;
            // CORRECTNESS BY CONSTRUCTION: Derive from master position's ATR-based parameters
@codefactor-io
codefactor-io Bot
10 minutes ago
Single-line comment should be preceded by blank line.

Suggested change
            // CORRECTNESS BY CONSTRUCTION: Derive from master position's ATR-based parameters
            // CORRECTNESS BY CONSTRUCTION: Derive from master position's ATR-based parameters
@backtothefutures83-oss	Reply...
src/V12_002.Trailing.StopUpdate.cs
                // ATOMIC UPDATE: Use AddOrUpdate to avoid read-modify-write race condition
                pendingStopReplacements.AddOrUpdate(
                    entryName,
                    // Add factory (should not be called since TryAdd failed above)
@codefactor-io
codefactor-io Bot
10 minutes ago
Single-line comment should be preceded by blank line.

Suggested change
                    // Add factory (should not be called since TryAdd failed above)
                    // Add factory (should not be called since TryAdd failed above)
@backtothefutures83-oss	Reply...
src/V12_002.Trailing.StopUpdate.cs
                    entryName,
                    // Add factory (should not be called since TryAdd failed above)
                    key => newPending,
                    // Update factory (atomic)
@codefactor-io
codefactor-io Bot
10 minutes ago
Single-line comment should be preceded by blank line.

Suggested change
                    // Update factory (atomic)
                    // Update factory (atomic)
@backtothefutures83-oss	Reply...
src/V12_002.Trailing.StopUpdate.cs
                        var _b950Refresh = !pending.BracketRestorationNeeded
                            ? RefreshTargetSnapshot(entryName)
                            : pending.CapturedTargets;
                        var _b950Needed =
@codefactor-io
codefactor-io Bot
10 minutes ago
Variable '_b950Needed' should begin with lower-case letter.

Suggested change
                        var _b950Needed =
                        var b950Needed =
@backtothefutures83-oss	Reply...
@qodo-code-review
qodo-code-review Bot
commented
10 minutes ago
CI Feedback 🧐
A test triggered by this PR failed. Here is an AI-generated analysis of the failure:

Action: Verify src/ vs non-src/ Separation

Failed stage: Check file separation [❌]

Failed test name: ""

Failure summary:

The action failed due to an enforced PR policy violation: this PR includes a mix of src/ and
non-src/ changes.
- The workflow reports: ❌ VIOLATION: This PR contains both src/ and non-src/ files
and exits with code 1 (line 254).
- src/ files changed (5): src/V12_002.Orders.Callbacks.cs,
src/V12_002.Orders.Management.StopSync.cs, src/V12_002.PositionInfo.cs,
src/V12_002.SIMA.Dispatch.cs, src/V12_002.Trailing.StopUpdate.cs (lines 240-245).
- non-src/ file
changed (1): docs/brain/pr_13_fix_queue.md (lines 246-247).
The later git warning (fatal: No url 
found for submodule path 'AntigravityMobile' in .gitmodules, line 264) happens during post-job
cleanup and is not the primary cause of the job failure.

Relevant error logs:
@codeant-ai codeant-ai Bot removed the size:L label 10 minutes ago
@codeant-ai codeant-ai Bot added the size:L label 10 minutes ago
@codeant-ai
codeant-ai Bot
commented
9 minutes ago
CodeAnt AI Incremental review completed.

@sonarqubecloud
sonarqubecloud Bot
commented
9 minutes ago
Quality Gate Failed Quality Gate failed
Failed conditions
 C Maintainability Rating on New Code (required ≥ A)

See analysis details on SonarQube Cloud

 Catch issues before they fail your Quality Gate with our IDE extension  SonarQube for IDE

codescene-delta-analysis[bot]
codescene-delta-analysis Bot reviewed 9 minutes ago
codescene-delta-analysis Bot
left a comment
 Code Health Improved (1 files improve in Code Health)

Gates Failed
 Prevent hotspot decline (1 hotspot with Large Method)
 Enforce advisory code health rules (2 files with Large Method, Complex Method)

Our agent can fix these. Install it.

Gates Passed
 4 Quality Gates Passed

Reason for failure
View Improvements
Absence of Expected Change Pattern

universal-or-strategy/src/V12_002.Orders.Management.StopSync.cs is usually changed with: universal-or-strategy/src/V12_002.Orders.Management.Flatten.cs
Quality Gate Profile: Pay Down Tech Debt
Install CodeScene MCP: safeguard and uplift AI-generated code. Catch issues early with our IDE extension and CLI tool.

src/V12_002.SIMA.Dispatch.cs
Comment on lines +453 to +556
        private bool Dispatch_BuildFollowerOrders(
            string tradeType,
            OrderAction action,
            int quantity,
            double entryPrice,
            OrderType entryOrderType,
            Account acct,
            int accountIndex,
            string symmetryDispatchId,
            int dispatchTargetCount,
            StringBuilder dispatchLog,
            out PositionInfo fleetPos,
            out Order entry,
            out string fleetEntryName,
            out string expectedKey,
            out string ocoId,
            out int followerQty,
            out int ft1,
            out int ft2,
            out int ft3,
            out int ft4,
            out int ft5,
            out double stopPrice,
            out double t1TargetPrice,
            out double t2TargetPrice,
            out double t3TargetPrice,
            out double t4TargetPrice,
            out double t5TargetPrice
        )
        {
            // Initialize all out parameters
            fleetPos = null;
            entry = null;
            fleetEntryName = null;
            expectedKey = null;
            ocoId = null;
            followerQty = quantity;
            ft1 = ft2 = ft3 = ft4 = ft5 = 0;
            stopPrice = t1TargetPrice = t2TargetPrice = t3TargetPrice = t4TargetPrice = t5TargetPrice = 0;

            // Get or create position info for this account
            expectedKey = acct.Name;
            fleetPos = GetOrCreatePositionInfo(acct);

            // Generate unique fleet entry name
            fleetEntryName = LogBuffer.Format("{0}_{1}_{2}", symmetryDispatchId, acct.Name, accountIndex);

            // Generate OCO ID for bracket orders
            ocoId = LogBuffer.Format("{0}_{1}", action.ToString(), DateTime.UtcNow.Ticks);

            // Create entry order signal name
            string entrySig = SymmetryTrim("Entry_" + fleetEntryName, 40);

            // Create the entry order
            entry = acct.CreateOrder(
                Instrument,
                action,
                entryOrderType,
                TimeInForce.Gtc,
                followerQty,
                entryOrderType == OrderType.Limit ? entryPrice : 0,
                0,
                ocoId,
                entrySig,
                null
            );

            if (entry == null)
            {
                Print($"[DISPATCH] [X] CreateOrder returned null for {acct.Name}");
                return false;
            }

            // Calculate stop and target prices based on entry price
            // Use simple fixed offsets for now (these should ideally come from strategy parameters)
            double tickSize = Instrument.MasterInstrument.TickSize;
            // CORRECTNESS BY CONSTRUCTION: Derive from master position's ATR-based parameters
            // instead of hardcoded values to maintain master/follower symmetry
            double stopDistance = Math.Abs(masterPos.InitialStopPrice - masterPos.EntryPrice);
            double stopTicks = stopDistance / tickSize;
            double targetDistance = Math.Abs(masterPos.Target1Price - masterPos.EntryPrice);
            double targetTicks = targetDistance / tickSize;

            if (action == OrderAction.Buy)
            {
                stopPrice = entryPrice - (stopTicks * tickSize);
                t1TargetPrice = entryPrice + (targetTicks * tickSize);
            }
            else
            {
                stopPrice = entryPrice + (stopTicks * tickSize);
                t1TargetPrice = entryPrice - (targetTicks * tickSize);
            }

            // Round to tick size
            stopPrice = Instrument.MasterInstrument.RoundToTickSize(stopPrice);
            t1TargetPrice = Instrument.MasterInstrument.RoundToTickSize(t1TargetPrice);

            // Set target quantities (distribute across targets based on dispatchTargetCount)
            if (dispatchTargetCount >= 1)
                ft1 = followerQty;

            return true;
        }
@codescene-delta-analysis
codescene-delta-analysis Bot
9 minutes ago
❌ New issue: Large Method
Dispatch_BuildFollowerOrders has 81 lines, threshold = 70

Suppress

@backtothefutures83-oss	Reply...
src/V12_002.Trailing.StopUpdate.cs
                }
            }
            else if (pendingStopReplacements.TryGetValue(entryName, out var pending))
            else
@codescene-delta-analysis
codescene-delta-analysis Bot
9 minutes ago
❌ New issue: Complex Method
UpdateExistingPendingReplacement has a cyclomatic complexity of 10, threshold = 9

Suppress

@backtothefutures83-oss	Reply...
gitar-bot[bot]
gitar-bot Bot reviewed 9 minutes ago
src/V12_002.SIMA.Dispatch.cs
Comment on lines +529 to +534
            // CORRECTNESS BY CONSTRUCTION: Derive from master position's ATR-based parameters
            // instead of hardcoded values to maintain master/follower symmetry
            double stopDistance = Math.Abs(masterPos.InitialStopPrice - masterPos.EntryPrice);
            double stopTicks = stopDistance / tickSize;
            double targetDistance = Math.Abs(masterPos.Target1Price - masterPos.EntryPrice);
            double targetTicks = targetDistance / tickSize;
@gitar-bot
gitar-bot Bot
9 minutes ago
⚠️ Edge Case: No guard for zero stopDistance/targetDistance from unset masterPos
When masterPos.EntryPrice is 0 (entry not yet filled) or masterPos.InitialStopPrice == masterPos.EntryPrice (stop not yet computed), stopDistance and/or targetDistance will be 0, producing stopPrice == entryPrice and submitting a stop order at fill price (instant stop-out) or a target at fill price (instant fill). This silently places invalid bracket orders on follower accounts.

Add a guard that bails out (or falls back to a safe default) when derived distances are zero or unreasonably small.

Add a minimum-distance guard before computing tick counts to prevent zero-distance bracket orders:

double stopDistance = Math.Abs(masterPos.InitialStopPrice - masterPos.EntryPrice);
double targetDistance = Math.Abs(masterPos.Target1Price - masterPos.EntryPrice);
if (stopDistance < tickSize || targetDistance < tickSize)
{
    Print(string.Format("[DISPATCH] ABORT follower bracket: master stop/target not yet computed. stopDist={0}, targetDist={1}", stopDistance, targetDistance));
    return false;
}
double stopTicks = stopDistance / tickSize;
double targetTicks = targetDistance / tickSize;
 Apply fix
Check the box to apply the fix or reply for a change | Was this helpful? React with 👍 / 👎

@backtothefutures83-oss	Reply...
@gitar-bot
gitar-bot Bot
commented
8 minutes ago
Code Review ⚠️ Changes requested 2 resolved / 3 findings
Refactored HandleEntryOrderFilled to improve cyclomatic complexity and state atomicity, but requested changes remain due to insufficient validation for follower position stop distances and hardcoded risk parameters in the new dispatch helper.

⚠️ Edge Case: No guard for zero stopDistance/targetDistance from unset masterPos
📄 src/V12_002.SIMA.Dispatch.cs:529-534

When masterPos.EntryPrice is 0 (entry not yet filled) or masterPos.InitialStopPrice == masterPos.EntryPrice (stop not yet computed), stopDistance and/or targetDistance will be 0, producing stopPrice == entryPrice and submitting a stop order at fill price (instant stop-out) or a target at fill price (instant fill). This silently places invalid bracket orders on follower accounts.

Add a guard that bails out (or falls back to a safe default) when derived distances are zero or unreasonably small.

Add a minimum-distance guard before computing tick counts to prevent zero-distance bracket orders
✅ 2 resolved
🤖 Prompt for agents
Tip

Comment Gitar fix CI or enable auto-apply: gitar auto-apply:on

Options
Was this helpful? React with 👍 / 👎 | Gitar

@gitar-bot
gitar-bot Bot
commented
8 minutes ago
Code Review ⚠️ Changes requested 2 resolved / 3 findings
Refactored HandleEntryOrderFilled to improve cyclomatic complexity and state atomicity, but requested changes remain due to insufficient validation for follower position stop distances and hardcoded risk parameters in the new dispatch helper.

⚠️ Edge Case: No guard for zero stopDistance/targetDistance from unset masterPos
📄 src/V12_002.SIMA.Dispatch.cs:529-534

When masterPos.EntryPrice is 0 (entry not yet filled) or masterPos.InitialStopPrice == masterPos.EntryPrice (stop not yet computed), stopDistance and/or targetDistance will be 0, producing stopPrice == entryPrice and submitting a stop order at fill price (instant stop-out) or a target at fill price (instant fill). This silently places invalid bracket orders on follower accounts.

Add a guard that bails out (or falls back to a safe default) when derived distances are zero or unreasonably small.

Add a minimum-distance guard before computing tick counts to prevent zero-distance bracket orders
✅ 2 resolved
🤖 Prompt for agents
Tip

Comment Gitar fix CI or enable auto-apply: gitar auto-apply:on

Options
Was this helpful? React with 👍 / 👎 | Gitar

coderabbitai[bot]
coderabbitai Bot requested changes 4 minutes ago
coderabbitai Bot
left a comment
Actionable comments posted: 1

🤖 Prompt for all review comments with AI agents
🪄 Autofix (Beta)
ℹ️ Review info
src/V12_002.Trailing.StopUpdate.cs
Comment on lines +210 to +236
                pendingStopReplacements.AddOrUpdate(
                    entryName,
                    // Add factory (should not be called since TryAdd failed above)
                    key => newPending,
                    // Update factory (atomic)
                    (key, pending) =>
                    {
                        // Readonly struct: must create new instance to update dictionary
                        var _b950Refresh = !pending.BracketRestorationNeeded
                            ? RefreshTargetSnapshot(entryName)
                            : pending.CapturedTargets;
                        var _b950Needed =
                            !pending.BracketRestorationNeeded && _b950Refresh != null && _b950Refresh.Length > 0;

                        return new PendingStopReplacement
                        {
                            EntryName = pending.EntryName,
                            Quantity = pending.Quantity,
                            StopPrice = validatedStopPrice,
                            Direction = pending.Direction,
                            OldOrder = pending.OldOrder,
                            CreatedTime = pending.CreatedTime,
                            CapturedTargets = _b950Refresh ?? pending.CapturedTargets,
                            BracketRestorationNeeded = _b950Needed || pending.BracketRestorationNeeded,
                        };
                    }
                );
@coderabbitai
coderabbitai Bot
4 minutes ago
⚠️ Potential issue | 🟠 Major | ⚡ Quick win

🧩 Analysis chain
Fix ConcurrentDictionary AddOrUpdate add-path counter drift in UpdateExistingPendingReplacement

UpdateExistingPendingReplacement uses TryAdd(...) and, on failure, falls back to pendingStopReplacements.AddOrUpdate(entryName, key => newPending, ...). If entryName is removed between the failed TryAdd and the subsequent AddOrUpdate, AddOrUpdate can take the add path (invoking key => newPending)—inserting a new pending replacement without incrementing pendingReplacementCount or running the circuit-breaker bookkeeping, so counters/thresholds can drift from the actual dictionary size.

Proposed fix
📝 Committable suggestion
🧰 Tools
🤖 Prompt for AI Agents
@backtothefutures83-oss	Reply...
cubic-dev-ai[bot]
cubic-dev-ai Bot reviewed now
cubic-dev-ai Bot
left a comment
2 issues found across 3 files (changes from recent commits).

You’re at about 97% of the monthly reviewed-line limit. You may want to disable incremental reviews to conserve quota. Reviews will continue until that limit is exceeded. If you need help avoiding interruptions, please contact contact@cubic.dev.

Prompt for AI agents (unresolved issues)
Tip: Review your code locally with the cubic CLI to iterate faster.

Fix all with cubic | Re-trigger cubic

src/V12_002.SIMA.Dispatch.cs
            double tickSize = Instrument.MasterInstrument.TickSize;
            // CORRECTNESS BY CONSTRUCTION: Derive from master position's ATR-based parameters
            // instead of hardcoded values to maintain master/follower symmetry
            double stopDistance = Math.Abs(masterPos.InitialStopPrice - masterPos.EntryPrice);
@cubic-dev-ai
cubic-dev-ai Bot
now
P1: Add a minimum-distance guard before deriving tick counts; if master stop/target prices are still unset, zero distances can place follower stop/target orders at the entry price.

Prompt for AI agents
@backtothefutures83-oss	Reply...
src/V12_002.Trailing.StopUpdate.cs
Comment on lines +210 to +236
                pendingStopReplacements.AddOrUpdate(
                    entryName,
                    // Add factory (should not be called since TryAdd failed above)
                    key => newPending,
                    // Update factory (atomic)
                    (key, pending) =>
                    {
                        // Readonly struct: must create new instance to update dictionary
                        var _b950Refresh = !pending.BracketRestorationNeeded
                            ? RefreshTargetSnapshot(entryName)
                            : pending.CapturedTargets;
                        var _b950Needed =
                            !pending.BracketRestorationNeeded && _b950Refresh != null && _b950Refresh.Length > 0;

                        return new PendingStopReplacement
                        {
                            EntryName = pending.EntryName,
                            Quantity = pending.Quantity,
                            StopPrice = validatedStopPrice,
                            Direction = pending.Direction,
                            OldOrder = pending.OldOrder,
                            CreatedTime = pending.CreatedTime,
                            CapturedTargets = _b950Refresh ?? pending.CapturedTargets,
                            BracketRestorationNeeded = _b950Needed || pending.BracketRestorationNeeded,
                        };
                    }
                );
@cubic-dev-ai
cubic-dev-ai Bot
now
P2: AddOrUpdate can add a new pending replacement without updating pendingReplacementCount, breaking circuit-breaker accounting.

Prompt for AI agents
Suggested change
                pendingStopReplacements.AddOrUpdate(
                    entryName,
                    // Add factory (should not be called since TryAdd failed above)
                    key => newPending,
                    // Update factory (atomic)
                    (key, pending) =>
                    {
                        // Readonly struct: must create new instance to update dictionary
                        var _b950Refresh = !pending.BracketRestorationNeeded
                            ? RefreshTargetSnapshot(entryName)
                            : pending.CapturedTargets;
                        var _b950Needed =
                            !pending.BracketRestorationNeeded && _b950Refresh != null && _b950Refresh.Length > 0;
                        return new PendingStopReplacement
                        {
                            EntryName = pending.EntryName,
                            Quantity = pending.Quantity,
                            StopPrice = validatedStopPrice,
                            Direction = pending.Direction,
                            OldOrder = pending.OldOrder,
                            CreatedTime = pending.CreatedTime,
                            CapturedTargets = _b950Refresh ?? pending.CapturedTargets,
                            BracketRestorationNeeded = _b950Needed || pending.BracketRestorationNeeded,
                        };
                    }
                );
                while (true)
                {
                    if (pendingStopReplacements.TryGetValue(entryName, out var pending))
                    {
                        var _b950Refresh = !pending.BracketRestorationNeeded
                            ? RefreshTargetSnapshot(entryName)
                            : pending.CapturedTargets;
                        var _b950Needed =
                            !pending.BracketRestorationNeeded && _b950Refresh != null && _b950Refresh.Length > 0;
                        var updatedPending = new PendingStopReplacement
                        {
                            EntryName = pending.EntryName,
                            Quantity = pending.Quantity,
                            StopPrice = validatedStopPrice,
                            Direction = pending.Direction,
                            OldOrder = pending.OldOrder,
                            CreatedTime = pending.CreatedTime,
                            CapturedTargets = _b950Refresh ?? pending.CapturedTargets,
                            BracketRestorationNeeded = _b950Needed || pending.BracketRestorationNeeded,
                        };
                        if (pendingStopReplacements.TryUpdate(entryName, updatedPending, pending))
                            break;
                        continue;
                    }
                    if (pendingStopReplacements.TryAdd(entryName, newPending))
                    {
                        int currentCount = Interlocked.Increment(ref pendingReplacementCount);
                        if (currentCount >= CIRCUIT_BREAKER_THRESHOLD && !circuitBreakerActive)
                        {
                            circuitBreakerActive = true;
                            circuitBreakerActivatedTime = DateTime.Now;
                            Print(
                                string.Format(
                                    "V8.30: CIRCUIT BREAKER ACTIVATED - {0} pending replacements (threshold: {1})",
                                    currentCount,
                                    CIRCUIT_BREAKER_THRESHOLD
                                )
                            );
                        }
                        break;
                    }
                }