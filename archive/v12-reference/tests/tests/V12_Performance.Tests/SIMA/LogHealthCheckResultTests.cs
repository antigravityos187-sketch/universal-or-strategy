using System.Text;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// xUnit tests for EPIC-W7-001 Phase 5 (T1-T6) -- LogHealthCheckResult helpers.
    /// T1: IsAccountTrulyFlat, T2: HasAnyActiveState, T3: BuildHealthCheckSkipReason,
    /// T4: LogHealthCheck_TrulyFlat, T5: LogHealthCheck_FlatWithActiveState,
    /// T6: LogHealthCheckResult integration.
    /// NT8 prevents direct class instantiation; logic is verified via standalone mirrors.
    /// </summary>
    public class LogHealthCheckResultTests
    {
        // -----------------------------------------------------------------------
        // Stand-in mirrors (NT8 prevents direct class instantiation)
        // -----------------------------------------------------------------------

        private static bool SimIsAccountTrulyFlat(
            bool brokerFlat,
            bool hasActiveFsm,
            bool hasActivePosition,
            bool hasDispatchPending
        )
        {
            return brokerFlat && !hasActiveFsm && !hasActivePosition && !hasDispatchPending;
        }

        private static bool SimHasAnyActiveState(
            bool hasActiveFsm,
            bool hasActivePosition,
            bool hasDispatchPending
        )
        {
            return hasActiveFsm || hasActivePosition || hasDispatchPending;
        }

        private static string SimBuildHealthCheckSkipReason(
            bool hasActiveFsm,
            bool hasDispatchPending,
            bool hasActivePosition
        )
        {
            if (hasActiveFsm)
                return "FSM active";
            if (hasDispatchPending)
                return "dispatch pending";
            return "activePos present";
        }

        private static void SimLogHealthCheck_TrulyFlat(string accountName, StringBuilder dispatchLog)
        {
            dispatchLog.AppendLine(
                string.Format(
                    "[DISPATCH] H-13: {0} broker flat, no FSM/position/dispatch -- no action",
                    accountName
                )
            );
        }

        private static void SimLogHealthCheck_FlatWithActiveState(
            string accountName,
            string skipReason,
            StringBuilder dispatchLog
        )
        {
            dispatchLog.AppendLine(
                string.Format(
                    "[DISPATCH] H-13 SKIP: {0} Flat but {1} -- not resetting",
                    accountName,
                    skipReason
                )
            );
        }

        private static void SimLogHealthCheckResult(
            string accountName,
            bool brokerFlat,
            bool hasActiveFsm,
            bool hasActivePosition,
            bool hasDispatchPending,
            StringBuilder dispatchLog
        )
        {
            if (SimIsAccountTrulyFlat(brokerFlat, hasActiveFsm, hasActivePosition, hasDispatchPending))
            {
                SimLogHealthCheck_TrulyFlat(accountName, dispatchLog);
            }
            else if (brokerFlat && SimHasAnyActiveState(hasActiveFsm, hasActivePosition, hasDispatchPending))
            {
                string reason = SimBuildHealthCheckSkipReason(hasActiveFsm, hasDispatchPending, hasActivePosition);
                SimLogHealthCheck_FlatWithActiveState(accountName, reason, dispatchLog);
            }
        }

        // -----------------------------------------------------------------------
        // T1: IsAccountTrulyFlat (5 tests)
        // -----------------------------------------------------------------------

        [Fact]
        public void IsAccountTrulyFlat_TrulyFlat_ReturnsTrue()
        {
            bool result = SimIsAccountTrulyFlat(
                brokerFlat: true,
                hasActiveFsm: false,
                hasActivePosition: false,
                hasDispatchPending: false
            );
            Assert.Equal(true, result);
        }

        [Fact]
        public void IsAccountTrulyFlat_BrokerNotFlat_ReturnsFalse()
        {
            bool result = SimIsAccountTrulyFlat(
                brokerFlat: false,
                hasActiveFsm: false,
                hasActivePosition: false,
                hasDispatchPending: false
            );
            Assert.Equal(false, result);
        }

        [Fact]
        public void IsAccountTrulyFlat_HasActiveFsm_ReturnsFalse()
        {
            bool result = SimIsAccountTrulyFlat(
                brokerFlat: true,
                hasActiveFsm: true,
                hasActivePosition: false,
                hasDispatchPending: false
            );
            Assert.Equal(false, result);
        }

        [Fact]
        public void IsAccountTrulyFlat_HasActivePosition_ReturnsFalse()
        {
            bool result = SimIsAccountTrulyFlat(
                brokerFlat: true,
                hasActiveFsm: false,
                hasActivePosition: true,
                hasDispatchPending: false
            );
            Assert.Equal(false, result);
        }

        [Fact]
        public void IsAccountTrulyFlat_HasDispatchPending_ReturnsFalse()
        {
            bool result = SimIsAccountTrulyFlat(
                brokerFlat: true,
                hasActiveFsm: false,
                hasActivePosition: false,
                hasDispatchPending: true
            );
            Assert.Equal(false, result);
        }

        // -----------------------------------------------------------------------
        // T2: HasAnyActiveState (4 tests)
        // -----------------------------------------------------------------------

        [Fact]
        public void HasAnyActiveState_AllFalse_ReturnsFalse()
        {
            bool result = SimHasAnyActiveState(
                hasActiveFsm: false,
                hasActivePosition: false,
                hasDispatchPending: false
            );
            Assert.Equal(false, result);
        }

        [Fact]
        public void HasAnyActiveState_FsmOnly_ReturnsTrue()
        {
            bool result = SimHasAnyActiveState(
                hasActiveFsm: true,
                hasActivePosition: false,
                hasDispatchPending: false
            );
            Assert.Equal(true, result);
        }

        [Fact]
        public void HasAnyActiveState_PositionOnly_ReturnsTrue()
        {
            bool result = SimHasAnyActiveState(
                hasActiveFsm: false,
                hasActivePosition: true,
                hasDispatchPending: false
            );
            Assert.Equal(true, result);
        }

        [Fact]
        public void HasAnyActiveState_DispatchOnly_ReturnsTrue()
        {
            bool result = SimHasAnyActiveState(
                hasActiveFsm: false,
                hasActivePosition: false,
                hasDispatchPending: true
            );
            Assert.Equal(true, result);
        }

        // -----------------------------------------------------------------------
        // T3: BuildHealthCheckSkipReason (3 tests)
        // -----------------------------------------------------------------------

        [Fact]
        public void BuildHealthCheckSkipReason_FsmActive_ReturnsFsmActive()
        {
            string result = SimBuildHealthCheckSkipReason(
                hasActiveFsm: true,
                hasDispatchPending: false,
                hasActivePosition: false
            );
            Assert.Equal("FSM active", result);
        }

        [Fact]
        public void BuildHealthCheckSkipReason_DispatchPending_ReturnsDispatchPending()
        {
            string result = SimBuildHealthCheckSkipReason(
                hasActiveFsm: false,
                hasDispatchPending: true,
                hasActivePosition: false
            );
            Assert.Equal("dispatch pending", result);
        }

        [Fact]
        public void BuildHealthCheckSkipReason_ActivePositionOnly_ReturnsActivePosPresent()
        {
            string result = SimBuildHealthCheckSkipReason(
                hasActiveFsm: false,
                hasDispatchPending: false,
                hasActivePosition: true
            );
            Assert.Equal("activePos present", result);
        }

        // -----------------------------------------------------------------------
        // T4: LogHealthCheck_TrulyFlat (1 test)
        // -----------------------------------------------------------------------

        [Fact]
        public void LogHealthCheck_TrulyFlat_AppendsCorrectLine()
        {
            var sb = new StringBuilder();
            SimLogHealthCheck_TrulyFlat("TestAcct", sb);
            Assert.Equal(
                "[DISPATCH] H-13: TestAcct broker flat, no FSM/position/dispatch -- no action",
                sb.ToString().TrimEnd()
            );
        }

        // -----------------------------------------------------------------------
        // T5: LogHealthCheck_FlatWithActiveState (1 test)
        // -----------------------------------------------------------------------

        [Fact]
        public void LogHealthCheck_FlatWithActiveState_AppendsCorrectLine()
        {
            var sb = new StringBuilder();
            SimLogHealthCheck_FlatWithActiveState("TestAcct", "FSM active", sb);
            Assert.Equal(
                "[DISPATCH] H-13 SKIP: TestAcct Flat but FSM active -- not resetting",
                sb.ToString().TrimEnd()
            );
        }

        // -----------------------------------------------------------------------
        // T6: LogHealthCheckResult integration (3 tests)
        // -----------------------------------------------------------------------

        [Fact]
        public void LogHealthCheckResult_TrulyFlat_AppendsTrulyFlatMessage()
        {
            var sb = new StringBuilder();
            SimLogHealthCheckResult("TestAcct", true, false, false, false, sb);
            Assert.Equal(
                "[DISPATCH] H-13: TestAcct broker flat, no FSM/position/dispatch -- no action",
                sb.ToString().TrimEnd()
            );
        }

        [Fact]
        public void LogHealthCheckResult_FlatWithFsmActive_AppendsSkipMessage()
        {
            var sb = new StringBuilder();
            SimLogHealthCheckResult("TestAcct", true, true, false, false, sb);
            Assert.Equal(
                "[DISPATCH] H-13 SKIP: TestAcct Flat but FSM active -- not resetting",
                sb.ToString().TrimEnd()
            );
        }

        [Fact]
        public void LogHealthCheckResult_NotFlat_NoActionNoOutput()
        {
            var sb = new StringBuilder();
            SimLogHealthCheckResult("TestAcct", false, false, false, false, sb);
            Assert.Equal(string.Empty, sb.ToString());
        }
    }
}
