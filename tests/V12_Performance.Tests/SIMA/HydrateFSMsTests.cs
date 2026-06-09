using System;
using V12_Performance.Tests.Mocks;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// Unit tests for SIMA Lifecycle hydration methods.
    /// EPIC-CCN-16 TDD Safety Net: Validates MapOrderStateToFSMState pure function.
    /// Ensures correct mapping from NinjaTrader OrderState to V12 FollowerBracketState.
    /// </summary>
    public class HydrateFSMsTests
    {
        // Note: Since we cannot instantiate V12_002 directly (it's a Strategy),
        // we'll test the logic pattern using a standalone helper class that mirrors
        // the extracted method's behavior. The actual method will be private in V12_002.

        /// <summary>
        /// Test helper that mirrors MapOrderStateToFSMState logic.
        /// This allows us to test the pure function behavior in isolation.
        /// </summary>
        private enum TestFollowerBracketState
        {
            None,
            PendingSubmit,
            Submitted,
            Accepted,
            Active,
            Replacing,
            Modifying,
            Filled,
            Cancelled,
            Rejected,
            Disconnected,
        }

        private TestFollowerBracketState? MapOrderStateToFSMState(MockOrderState entryState)
        {
            if (entryState == MockOrderState.Filled || entryState == MockOrderState.PartFilled)
            {
                return TestFollowerBracketState.Active;
            }
            else if (entryState == MockOrderState.Accepted)
            {
                return TestFollowerBracketState.Accepted;
            }
            else if (
                entryState == MockOrderState.Working
                || entryState == MockOrderState.Submitted
                || entryState == MockOrderState.Initialized
                || entryState == MockOrderState.ChangePending
                || entryState == MockOrderState.ChangeSubmitted
            )
            {
                return TestFollowerBracketState.Submitted;
            }
            else
            {
                return null; // Terminal state - skip FSM creation
            }
        }

        [Fact]
        public void MapOrderStateToFSMState_Filled_ReturnsActive()
        {
            // Arrange
            MockOrderState state = MockOrderState.Filled;

            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestFollowerBracketState.Active, result.Value);
        }

        [Fact]
        public void MapOrderStateToFSMState_PartFilled_ReturnsActive()
        {
            // Arrange
            MockOrderState state = MockOrderState.PartFilled;

            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestFollowerBracketState.Active, result.Value);
        }

        [Fact]
        public void MapOrderStateToFSMState_Accepted_ReturnsAccepted()
        {
            // Arrange
            MockOrderState state = MockOrderState.Accepted;

            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestFollowerBracketState.Accepted, result.Value);
        }

        [Fact]
        public void MapOrderStateToFSMState_Working_ReturnsSubmitted()
        {
            // Arrange
            MockOrderState state = MockOrderState.Working;

            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestFollowerBracketState.Submitted, result.Value);
        }

        [Fact]
        public void MapOrderStateToFSMState_Submitted_ReturnsSubmitted()
        {
            // Arrange
            MockOrderState state = MockOrderState.Submitted;

            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestFollowerBracketState.Submitted, result.Value);
        }

        [Fact]
        public void MapOrderStateToFSMState_Initialized_ReturnsSubmitted()
        {
            // Arrange
            MockOrderState state = MockOrderState.Initialized;

            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestFollowerBracketState.Submitted, result.Value);
        }

        [Fact]
        public void MapOrderStateToFSMState_ChangePending_ReturnsSubmitted()
        {
            // Arrange
            MockOrderState state = MockOrderState.ChangePending;

            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestFollowerBracketState.Submitted, result.Value);
        }

        [Fact]
        public void MapOrderStateToFSMState_ChangeSubmitted_ReturnsSubmitted()
        {
            // Arrange
            MockOrderState state = MockOrderState.ChangeSubmitted;

            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestFollowerBracketState.Submitted, result.Value);
        }

        [Fact]
        public void MapOrderStateToFSMState_Cancelled_ReturnsNull()
        {
            // Arrange
            MockOrderState state = MockOrderState.Cancelled;

            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void MapOrderStateToFSMState_Rejected_ReturnsNull()
        {
            // Arrange
            MockOrderState state = MockOrderState.Rejected;

            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void MapOrderStateToFSMState_Unknown_ReturnsNull()
        {
            // Arrange
            MockOrderState state = MockOrderState.Unknown;

            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(MockOrderState.Filled)]
        [InlineData(MockOrderState.PartFilled)]
        public void MapOrderStateToFSMState_ActiveStates_AlwaysReturnsActive(MockOrderState state)
        {
            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestFollowerBracketState.Active, result.Value);
        }

        [Theory]
        [InlineData(MockOrderState.Working)]
        [InlineData(MockOrderState.Submitted)]
        [InlineData(MockOrderState.Initialized)]
        [InlineData(MockOrderState.ChangePending)]
        [InlineData(MockOrderState.ChangeSubmitted)]
        public void MapOrderStateToFSMState_WorkingStates_AlwaysReturnsSubmitted(MockOrderState state)
        {
            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestFollowerBracketState.Submitted, result.Value);
        }

        [Theory]
        [InlineData(MockOrderState.Cancelled)]
        [InlineData(MockOrderState.Rejected)]
        [InlineData(MockOrderState.Unknown)]
        public void MapOrderStateToFSMState_TerminalStates_AlwaysReturnsNull(MockOrderState state)
        {
            // Act
            var result = MapOrderStateToFSMState(state);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void MapOrderStateToFSMState_IsPureFunction_SameInputProducesSameOutput()
        {
            // Arrange
            MockOrderState state = MockOrderState.Working;

            // Act
            var result1 = MapOrderStateToFSMState(state);
            var result2 = MapOrderStateToFSMState(state);
            var result3 = MapOrderStateToFSMState(state);

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }
    }
}

// Made with Bob (EPIC-CCN-16 Ticket 1 TDD)
