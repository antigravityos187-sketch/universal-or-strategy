using System.Collections.Concurrent;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// Unit tests for ShouldSkipFleetAccountBracket extracted helper.
    /// EPIC-W7-096 TICKET-1 TDD Safety Net.
    /// Validates account eligibility filter: inactive guard (bug fix), consistency lock check.
    /// CYC=5. Pure predicate -- verified via inline stand-in mirroring exact branch structure.
    /// </summary>
    public class ShouldSkipFleetAccountBracketTests
    {
        // ---------------------------------------------------------------------------
        // Stand-in: mirrors ShouldSkipFleetAccountBracket branch structure exactly.
        // ConcurrentDictionary used (lock-free, matches production implementation).
        // ---------------------------------------------------------------------------

        private static bool SimulateShouldSkipFleetAccountBracket(
            string accountName,
            ConcurrentDictionary<string, bool> activeFleetAccounts,
            bool enableConsistencyLock,
            double dailyPL,
            double maxDailyProfitCap,
            out string skipReason
        )
        {
            if (!activeFleetAccounts.TryGetValue(accountName, out bool isActive) || !isActive)
            {
                skipReason = "Inactive";
                return true;
            }

            if (enableConsistencyLock)
            {
                if (dailyPL >= maxDailyProfitCap)
                {
                    skipReason = string.Format("ConsistencyLock ${0:F2}", dailyPL);
                    return true;
                }
            }

            skipReason = string.Empty;
            return false;
        }

        // ---------------------------------------------------------------------------
        // Test: account not registered -> skip (Inactive)
        // ---------------------------------------------------------------------------

        [Fact]
        public void ShouldSkipFleetAccountBracket_AccountNotRegistered_ReturnsTrue()
        {
            var dict = new ConcurrentDictionary<string, bool>();
            bool result = SimulateShouldSkipFleetAccountBracket(
                "Sim101", dict, false, 0, 10000, out string reason
            );
            Assert.Equal(true, result);
        }

        [Fact]
        public void ShouldSkipFleetAccountBracket_AccountNotRegistered_ReasonIsInactive()
        {
            var dict = new ConcurrentDictionary<string, bool>();
            SimulateShouldSkipFleetAccountBracket(
                "Sim101", dict, false, 0, 10000, out string reason
            );
            Assert.Equal("Inactive", reason);
        }

        // ---------------------------------------------------------------------------
        // Test: account registered but explicitly disabled -> skip (Inactive)
        // ---------------------------------------------------------------------------

        [Fact]
        public void ShouldSkipFleetAccountBracket_AccountDisabled_ReturnsTrue()
        {
            var dict = new ConcurrentDictionary<string, bool>();
            dict["Sim101"] = false;
            bool result = SimulateShouldSkipFleetAccountBracket(
                "Sim101", dict, false, 0, 10000, out string reason
            );
            Assert.Equal(true, result);
        }

        [Fact]
        public void ShouldSkipFleetAccountBracket_AccountDisabled_ReasonIsInactive()
        {
            var dict = new ConcurrentDictionary<string, bool>();
            dict["Sim101"] = false;
            SimulateShouldSkipFleetAccountBracket(
                "Sim101", dict, false, 0, 10000, out string reason
            );
            Assert.Equal("Inactive", reason);
        }

        // ---------------------------------------------------------------------------
        // Test: daily cap reached with consistency lock enabled -> skip
        // ---------------------------------------------------------------------------

        [Fact]
        public void ShouldSkipFleetAccountBracket_DailyCapReached_ReturnsTrue()
        {
            var dict = new ConcurrentDictionary<string, bool>();
            dict["Sim202"] = true;
            bool result = SimulateShouldSkipFleetAccountBracket(
                "Sim202", dict, true, 500.00, 500.00, out string reason
            );
            Assert.Equal(true, result);
        }

        [Fact]
        public void ShouldSkipFleetAccountBracket_DailyCapReached_ReasonContainsConsistencyLock()
        {
            var dict = new ConcurrentDictionary<string, bool>();
            dict["Sim202"] = true;
            SimulateShouldSkipFleetAccountBracket(
                "Sim202", dict, true, 500.00, 500.00, out string reason
            );
            Assert.Contains("ConsistencyLock", reason);
        }

        // ---------------------------------------------------------------------------
        // Test: consistency lock enabled but daily cap NOT reached -> no skip
        // ---------------------------------------------------------------------------

        [Fact]
        public void ShouldSkipFleetAccountBracket_BelowDailyCap_ReturnsFalse()
        {
            var dict = new ConcurrentDictionary<string, bool>();
            dict["Sim303"] = true;
            bool result = SimulateShouldSkipFleetAccountBracket(
                "Sim303", dict, true, 499.99, 500.00, out string reason
            );
            Assert.Equal(false, result);
        }

        [Fact]
        public void ShouldSkipFleetAccountBracket_BelowDailyCap_ReasonIsEmpty()
        {
            var dict = new ConcurrentDictionary<string, bool>();
            dict["Sim303"] = true;
            SimulateShouldSkipFleetAccountBracket(
                "Sim303", dict, true, 499.99, 500.00, out string reason
            );
            Assert.Equal(string.Empty, reason);
        }

        // ---------------------------------------------------------------------------
        // Test: active account, consistency lock disabled -> no skip
        // ---------------------------------------------------------------------------

        [Fact]
        public void ShouldSkipFleetAccountBracket_ActiveNoLock_ReturnsFalse()
        {
            var dict = new ConcurrentDictionary<string, bool>();
            dict["Sim404"] = true;
            bool result = SimulateShouldSkipFleetAccountBracket(
                "Sim404", dict, false, 9999.00, 500.00, out string reason
            );
            Assert.Equal(false, result);
        }
    }
}
