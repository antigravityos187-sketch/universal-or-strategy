// PTT-COPIER-B41 -- PttGlobalQuickExit.cs
// Quick Exit: all-accounts bracket swap (global scope).
// B41: operates on Account.All x Positions -- every account, every instrument with a non-flat position.
// Jane Street rules: JS-001 (no throw), JS-002 (no return null), JS-021 (no lock), JS-033 (no async void).
// NT8-003: volatile int (NOT volatile double). NT8-021: Account.All in Loaded handler, not constructor.

using System;
using System.Threading;
using NinjaTrader.Cbi;

namespace PropTraderTools
{
    /// <summary>
    /// PttGlobalQuickExit: all-accounts Quick Exit bracket swap.
    /// Button scope: Account.All x every non-flat position.
    /// Works without CopyRule -- InstrumentDefaults provides fallback ticks.
    /// </summary>
    internal sealed class PttGlobalQuickExit
    {
        /// <summary>
        /// Execute: all-accounts Quick Exit bracket swap, skipping follower accounts in the leader loop.
        /// CYC=8: acc loop(1), follower guard(2), pos loop(3), null/flat continue(4),
        ///        rule null-check(5), follower foreach(6), follower null continue(7), delegate(8).
        /// HOTFIX-QUICK-T3-01: snapshot target orders before cancel to determine N (targetCount).
        /// Pass targets snapshot to ExecuteOne for N-bracket submission.
        /// DW-B47-BE-FOLLOWER-SCOPE: follower accounts skipped in leader loop via IsFollowerAccount.
        /// B78 DW-B63-01: capture leaderStop BEFORE calling ExecuteOne (which cancels leader brackets).
        ///   Pass leaderStop + targets.Count to follower ExecuteOne so follower resolves the correct
        ///   stop price and target count even when its own ATM brackets have not yet arrived.
        /// JS-021: no lock. NT8-021: Account.All safe -- called from UI thread after Loaded.
        /// </summary>
        internal void Execute()
        {
            NinjaTrader.Code.Output.Process(
                "[PTT-QX-ALL] GlobalQuickExit fired",
                NinjaTrader.NinjaScript.PrintTo.OutputTab1
            );
            var engine = CopyEngine.Instance; // capture once
            foreach (Account acc in Account.All) // (1)
            {
                if (engine != null && engine.IsFollowerAccount(acc))
                    continue; // (2) follower skip
                foreach (Position pos in acc.Positions) // (3)
                {
                    if (pos == null || pos.Quantity == 0)
                        continue; // (4)
                    var targets = SnapshotTargetOrders(acc, pos.Instrument);
                    // B78 DW-B63-01: snapshot leader stop BEFORE ExecuteOne cancels leader brackets.
                    double leaderStop = PttQuickExit.SnapshotStopPrice(acc, pos.Instrument);
                    var ticks = ResolveQuickTicks(pos.Instrument);
                    NinjaTrader.Code.Output.Process(
                        "[PTT-QX-ALL] leader: "
                            + acc.Name
                            + " "
                            + pos.Instrument.FullName
                            + " qty="
                            + pos.Quantity
                            + " t1="
                            + ticks.t1
                            + " stop="
                            + leaderStop,
                        NinjaTrader.NinjaScript.PrintTo.OutputTab1
                    );
                    ExecuteOne(acc, pos.Instrument, ticks.t1, targets);
                    // B71 DW-B71-04: place PTT-QX on every follower that has an open position
                    var rule = engine?.FindRule(pos.Instrument); // (5)
                    if (rule != null) // (5 guard)
                        foreach (var follower in rule.Value.FollowerAccounts) // (6)
                        {
                            if (follower == null)
                                continue; // (7)
                            var followerTargets = SnapshotTargetOrders(follower, pos.Instrument);
                            NinjaTrader.Code.Output.Process(
                                "[PTT-QX-ALL] follower: "
                                    + follower.Name
                                    + " "
                                    + pos.Instrument.FullName
                                    + " leaderStop="
                                    + leaderStop
                                    + " leaderTargets="
                                    + targets.Count,
                                NinjaTrader.NinjaScript.PrintTo.OutputTab1
                            );
                            ExecuteOne(
                                follower,
                                pos.Instrument,
                                ticks.t1,
                                followerTargets,
                                skipIfFollower: false,
                                leaderStop: leaderStop,
                                leaderTargetCount: targets.Count
                            );
                        }
                }
            }
        }

        /// <summary>
        /// ResolveQuickTicks: returns (T1, T2) from CopyRule if found, else InstrumentDefaults.
        /// CYC=2: engine null guard(1), rule found check(2).
        /// </summary>
        private static (int t1, int t2) ResolveQuickTicks(Instrument instr)
        {
            var engine = CopyEngine.Instance;
            if (engine == null)
                return InstrumentDefaults.GetQuickTicks(
                    instr?.MasterInstrument?.Name ?? string.Empty
                ); // (1)
            int t1 = engine.GlobalQuickAllT1; // HOTFIX-QUICKALL-SINGLETON-01: use shared singleton value
            int t2 = t1 * 2;
            return (t1, t2);
        }

        /// <summary>
        /// ExecuteOne: per-account Quick Exit bracket swap.
        /// HOTFIX-QUICK-T3-01: accepts targets snapshot for N-bracket submission.
        /// B78 DW-B63-01: leaderStop + leaderTargetCount forwarded to PttQuickExit.Execute.
        /// DW-B79-03: pre-cancel follower ATM+PTT-* brackets BEFORE constructing PttQuickExit
        ///   so the follower account is clean when PttQuickExit.Execute runs its own cancel step.
        ///   Mirrors the leader path: cancel first, then submit PTT-QX.
        ///   Only fires on the follower path (skipIfFollower=false).
        ///   Leader path (skipIfFollower=true) unchanged -- leader's own ATM brackets are
        ///   already Working and cancelled by PttQuickExit.Execute's internal snapshot logic.
        /// CYC=2: follower guard(1) + delegate(2).
        /// JS-021: no lock. JS-001: no throw. JS-002: void. JS-033: synchronous void. ASCII-only.
        /// </summary>
        private void ExecuteOne(
            Account acc,
            Instrument instr,
            int t1Ticks,
            System.Collections.Generic.List<(double Price, int Qty)> targets,
            bool skipIfFollower = true,
            double leaderStop = 0,
            int leaderTargetCount = 0
        )
        {
            // DW-B79-03: pre-cancel follower ATM + prior PTT-* brackets BEFORE PttQuickExit snapshot.
            // When follower ATM brackets exist in any cancellable state (Working/Accepted/Submitted/
            // Initialized/TriggerPending), this cancel fires first -- identical to what the leader
            // path does naturally (leader ATM brackets are always Working at QX-ALL fire time and
            // cancelled by PttQuickExit.Execute's BuildQxSnapshot/CancelQxBrackets).
            // After this call, follower brackets enter CancelSubmitted (excluded from
            // BuildQxSnapshot's stateOk) -- PttQuickExit's internal cancel is a no-op.
            // NT8 sim confirms the cancel before PTT-QX Submit completes, preventing the conflict.
            if (!skipIfFollower) // (1)
            {
                NinjaTrader.Code.Output.Process(
                    "[PTT-QX-GUARD] pre-cancel follower brackets: "
                        + (acc != null ? acc.Name : "NULL"),
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1
                );
                CopyEngine.Instance?.CancelQxBrackets(acc, instr);
            }
            var executor = new PttQuickExit(); // (2)
            executor.Execute(
                acc,
                instr,
                t1Ticks,
                targets,
                skipIfFollower,
                leaderStop,
                leaderTargetCount
            );
        }

        /// <summary>
        /// SnapshotTargetOrders: returns list of (LimitPrice, Quantity) for active target orders
        /// on acc for instr. Covers ATM targets (Target1-Target9), PTT-QX-T* targets, and
        /// PTT-BE-Target-* targets (re-arm after prior BE). Reference: CopyEngine.MoveStopToBreakEven Step A.
        /// CYC=4: null guard(1), foreach(2), stateOk(3), isTarget(4). JS-002: returns list (never null).
        /// </summary>
        private static System.Collections.Generic.List<(
            double Price,
            int Qty
        )> SnapshotTargetOrders(Account acc, NinjaTrader.Cbi.Instrument instr)
        {
            var result = new System.Collections.Generic.List<(double Price, int Qty)>();
            if (acc == null || instr == null)
                return result; // (1)
            foreach (NinjaTrader.Cbi.Order o in acc.Orders) // (2)
            {
                if (o == null)
                    continue;
                bool stateOk =
                    o.OrderState == NinjaTrader.Cbi.OrderState.Working
                    || o.OrderState == NinjaTrader.Cbi.OrderState.Accepted; // (3)
                bool instrOk = o.Instrument != null && o.Instrument.FullName == instr.FullName;
                if (!stateOk || !instrOk || o.OrderType != NinjaTrader.Cbi.OrderType.Limit)
                    continue;
                bool isTarget =
                    !string.IsNullOrEmpty(o.Name)
                    && ( // (4)
                        (
                            o.Name.StartsWith("Target", StringComparison.Ordinal)
                            && o.Name.Length > 6
                            && char.IsDigit(o.Name[6])
                        )
                        || (
                            o.Name.StartsWith("PTT-QX-T", StringComparison.Ordinal)
                            && o.Name.Length > 8
                            && char.IsDigit(o.Name[8])
                        )
                        || o.Name.StartsWith("PTT-BE-Target-", StringComparison.Ordinal)
                    );
                if (!isTarget)
                    continue;
                result.Add((o.LimitPrice, o.Quantity));
            }
            return result;
        }
    }
}
