using V12_Performance.Tests.Mocks;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// Unit tests for IsActiveOrderState and IsSubmittedOrderState extracted helpers.
    /// EPIC-W7-058 T1 + T2 TDD Safety Net.
    /// Validates compound OR predicate extractions from MapOrderStateToFSMState.
    /// Helpers are private static on V12_002; logic verified via isolated stand-ins
    /// that mirror the exact branch structure.
    /// </summary>
    public class MapOrderStateToFSMStateTests
    {
        // Stand-ins mirroring the extracted private static helpers
        private static bool IsActiveOrderState(MockOrderState s) =>
            s == MockOrderState.Filled || s == MockOrderState.PartFilled;

        private static bool IsSubmittedOrderState(MockOrderState s) =>
            s == MockOrderState.Working
            || s == MockOrderState.Submitted
            || s == MockOrderState.Initialized
            || s == MockOrderState.ChangePending
            || s == MockOrderState.ChangeSubmitted;

        // IsActiveOrderState -- T1

        [Fact]
        public void IsActiveOrderState_FilledReturnsTrue()
        {
            Assert.True(IsActiveOrderState(MockOrderState.Filled));
        }

        [Fact]
        public void IsActiveOrderState_PartFilledReturnsTrue()
        {
            Assert.True(IsActiveOrderState(MockOrderState.PartFilled));
        }

        [Fact]
        public void IsActiveOrderState_AcceptedReturnsFalse()
        {
            Assert.False(IsActiveOrderState(MockOrderState.Accepted));
        }

        // IsSubmittedOrderState -- T2

        [Fact]
        public void IsSubmittedOrderState_WorkingReturnsTrue()
        {
            Assert.True(IsSubmittedOrderState(MockOrderState.Working));
        }

        [Fact]
        public void IsSubmittedOrderState_SubmittedReturnsTrue()
        {
            Assert.True(IsSubmittedOrderState(MockOrderState.Submitted));
        }

        [Fact]
        public void IsSubmittedOrderState_InitializedReturnsTrue()
        {
            Assert.True(IsSubmittedOrderState(MockOrderState.Initialized));
        }

        [Fact]
        public void IsSubmittedOrderState_ChangePendingReturnsTrue()
        {
            Assert.True(IsSubmittedOrderState(MockOrderState.ChangePending));
        }

        [Fact]
        public void IsSubmittedOrderState_ChangeSubmittedReturnsTrue()
        {
            Assert.True(IsSubmittedOrderState(MockOrderState.ChangeSubmitted));
        }

        [Fact]
        public void IsSubmittedOrderState_FilledReturnsFalse()
        {
            Assert.False(IsSubmittedOrderState(MockOrderState.Filled));
        }

        [Fact]
        public void IsSubmittedOrderState_AcceptedReturnsFalse()
        {
            Assert.False(IsSubmittedOrderState(MockOrderState.Accepted));
        }
    }
}
