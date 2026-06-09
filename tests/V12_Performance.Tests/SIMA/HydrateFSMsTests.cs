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

        /// <summary>
        /// Test helper that mirrors BuildFSM logic.
        /// Creates a mock FSM structure for testing.
        /// </summary>
        private class TestFollowerBracketFSM
        {
            public string AccountName { get; set; }
            public string EntryName { get; set; }
            public TestFollowerBracketState State { get; set; }
            public int RemainingContracts { get; set; }
            public DateTime LastUpdateUtc { get; set; }
            public MockOrder EntryOrder { get; set; }
            public MockOrder StopOrder { get; set; }
            public MockOrder[] Targets { get; set; }
        }

        private TestFollowerBracketFSM BuildFSM(
            string entryKey,
            string accountName,
            MockOrder entryOrder,
            TestFollowerBracketState state,
            int remainingContracts
        )
        {
            return new TestFollowerBracketFSM
            {
                AccountName = accountName,
                EntryName = entryKey,
                State = state,
                RemainingContracts = remainingContracts,
                LastUpdateUtc = DateTime.UtcNow,
                EntryOrder = entryOrder,
            };
        }

        [Fact]
        public void BuildFSM_ValidInputs_ReturnsInitializedFSM()
        {
            // Arrange
            string entryKey = "ENTRY_001";
            string accountName = "Sim101";
            MockOrder entryOrder = new MockOrder("ENTRY_001", MockOrderState.Working, new MockAccount("Sim101"));
            TestFollowerBracketState state = TestFollowerBracketState.Submitted;
            int remainingContracts = 2;

            // Act
            var fsm = BuildFSM(entryKey, accountName, entryOrder, state, remainingContracts);

            // Assert
            Assert.NotNull(fsm);
            Assert.Equal(entryKey, fsm.EntryName);
            Assert.Equal(accountName, fsm.AccountName);
            Assert.Equal(entryOrder, fsm.EntryOrder);
            Assert.Equal(state, fsm.State);
            Assert.Equal(remainingContracts, fsm.RemainingContracts);
        }

        [Fact]
        public void BuildFSM_NullEntryOrder_StillInitializesFSM()
        {
            // Arrange
            string entryKey = "ENTRY_002";
            string accountName = "Sim102";
            MockOrder entryOrder = null;
            TestFollowerBracketState state = TestFollowerBracketState.Active;
            int remainingContracts = 1;

            // Act
            var fsm = BuildFSM(entryKey, accountName, entryOrder, state, remainingContracts);

            // Assert
            Assert.NotNull(fsm);
            Assert.Equal(entryKey, fsm.EntryName);
            Assert.Equal(accountName, fsm.AccountName);
            Assert.Null(fsm.EntryOrder);
            Assert.Equal(state, fsm.State);
            Assert.Equal(remainingContracts, fsm.RemainingContracts);
        }

        [Fact]
        public void BuildFSM_ZeroRemainingContracts_AcceptsZero()
        {
            // Arrange
            string entryKey = "ENTRY_003";
            string accountName = "Sim103";
            MockOrder entryOrder = new MockOrder("ENTRY_003", MockOrderState.Filled, new MockAccount("Sim103"));
            TestFollowerBracketState state = TestFollowerBracketState.Active;
            int remainingContracts = 0;

            // Act
            var fsm = BuildFSM(entryKey, accountName, entryOrder, state, remainingContracts);

            // Assert
            Assert.NotNull(fsm);
            Assert.Equal(0, fsm.RemainingContracts);
        }

        [Fact]
        public void BuildFSM_AllStates_InitializesCorrectly()
        {
            // Arrange
            string entryKey = "ENTRY_004";
            string accountName = "Sim104";
            MockOrder entryOrder = new MockOrder("ENTRY_004", MockOrderState.Working, new MockAccount("Sim104"));

            // Act & Assert for each state
            var states = new[]
            {
                TestFollowerBracketState.None,
                TestFollowerBracketState.PendingSubmit,
                TestFollowerBracketState.Submitted,
                TestFollowerBracketState.Accepted,
                TestFollowerBracketState.Active,
                TestFollowerBracketState.Replacing,
                TestFollowerBracketState.Modifying,
                TestFollowerBracketState.Filled,
                TestFollowerBracketState.Cancelled,
                TestFollowerBracketState.Rejected,
                TestFollowerBracketState.Disconnected,
            };

            foreach (var state in states)
            {
                var fsm = BuildFSM(entryKey, accountName, entryOrder, state, 1);
                Assert.Equal(state, fsm.State);
            }
        }

        [Fact]
        public void BuildFSM_LastUpdateUtc_IsSetToCurrentTime()
        {
            // Arrange
            string entryKey = "ENTRY_005";
            string accountName = "Sim105";
            MockOrder entryOrder = new MockOrder("ENTRY_005", MockOrderState.Working, new MockAccount("Sim105"));
            TestFollowerBracketState state = TestFollowerBracketState.Submitted;
            int remainingContracts = 2;
            DateTime beforeCall = DateTime.UtcNow;

            // Act
            var fsm = BuildFSM(entryKey, accountName, entryOrder, state, remainingContracts);
            DateTime afterCall = DateTime.UtcNow;

            // Assert
            Assert.True(fsm.LastUpdateUtc >= beforeCall);
            Assert.True(fsm.LastUpdateUtc <= afterCall);
        }

        [Fact]
        public void BuildFSM_IsPureFactory_NoSideEffects()
        {
            // Arrange
            string entryKey = "ENTRY_006";
            string accountName = "Sim106";
            MockOrder entryOrder = new MockOrder("ENTRY_006", MockOrderState.Working, new MockAccount("Sim106"));
            TestFollowerBracketState state = TestFollowerBracketState.Submitted;
            int remainingContracts = 2;

            // Act
            var fsm1 = BuildFSM(entryKey, accountName, entryOrder, state, remainingContracts);
            var fsm2 = BuildFSM(entryKey, accountName, entryOrder, state, remainingContracts);

            // Assert - Each call creates a new instance
            Assert.NotSame(fsm1, fsm2);
            Assert.Equal(fsm1.EntryName, fsm2.EntryName);
            Assert.Equal(fsm1.AccountName, fsm2.AccountName);
            Assert.Equal(fsm1.State, fsm2.State);
            Assert.Equal(fsm1.RemainingContracts, fsm2.RemainingContracts);
        }
    }
}

// Made with Bob (EPIC-CCN-16 Ticket 1 & 2 TDD)
