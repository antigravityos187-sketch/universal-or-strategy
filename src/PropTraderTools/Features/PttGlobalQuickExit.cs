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
        /// Execute: all-accounts Quick Exit bracket swap, skipping follower accounts.
        /// CYC=6: acc loop(1), follower guard(2), pos loop(3), null/flat continue(4),
        ///        engine?. null-check on cancel call(5), delegate(6).
        /// DW-B47-BE-FOLLOWER-SCOPE: follower accounts skipped via CopyEngine.IsFollowerAccount.
        /// B68 DW-B68-01: follower brackets cancelled via CancelQxBracketsForFollowers before ExecuteOne.
        /// JS-021: no lock. NT8-021: Account.All safe -- called from UI thread after Loaded.
        /// </summary>
        internal void Execute()
        {
            var engine = CopyEngine.Instance;                   // capture once
            foreach (Account acc in Account.All)                // (1)
            {
                if (engine != null && engine.IsFollowerAccount(acc)) continue; // (2) follower skip
                foreach (Position pos in acc.Positions)         // (3)
                {
                    if (pos == null || pos.Quantity == 0) continue;  // (4)
                    var ticks = ResolveQuickTicks(pos.Instrument);
                    engine?.CancelQxBracketsForFollowers(pos.Instrument); // B68 DW-B68-01 (5)
                    ExecuteOne(acc, pos.Instrument, ticks.t1, ticks.t2); // (6)
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
            if (engine == null) return InstrumentDefaults.GetQuickTicks(instr?.MasterInstrument?.Name ?? string.Empty);  // (1)
            return engine.GetQuickTicksForInstrument(instr);  // (2) -- returns rule ticks or InstrumentDefaults fallback
        }

        /// <summary>
        /// ExecuteOne: per-account Quick Exit bracket swap.
        /// Delegates to PttQuickExit pattern for the given account.
        /// CYC=1: straight delegation (PttQuickExit.Execute contains all guard logic).
        /// </summary>
        private void ExecuteOne(Account acc, Instrument instr, int t1Ticks, int t2Ticks)
        {
            var executor = new PttQuickExit();
            executor.Execute(acc, instr, t1Ticks, t2Ticks);
        }
    }
}
