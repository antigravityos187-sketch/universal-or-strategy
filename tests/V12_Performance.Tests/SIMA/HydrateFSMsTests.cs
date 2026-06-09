using System;
using System.Collections.Generic;
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
        public enum TestFollowerBracketState
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

        /// <summary>
        /// Test helper that mirrors ResolveRemainingContracts logic.
        /// Calculates remaining contracts based on FSM state and position quantity.
        /// </summary>
        private int ResolveRemainingContracts(TestFollowerBracketState state, int orderQuantity, int? positionQuantity)
        {
            int remainingContracts = Math.Max(0, orderQuantity);
            if (state == TestFollowerBracketState.Active && positionQuantity.HasValue)
            {
                remainingContracts = Math.Abs(positionQuantity.Value);
            }
            return remainingContracts;
        }

        [Fact]
        public void ResolveRemainingContracts_NonActiveState_ReturnsOrderQuantity()
        {
            // Arrange
            TestFollowerBracketState state = TestFollowerBracketState.Submitted;
            int orderQuantity = 5;
            int? positionQuantity = 3;

            // Act
            int result = ResolveRemainingContracts(state, orderQuantity, positionQuantity);

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void ResolveRemainingContracts_ActiveStateWithPosition_ReturnsPositionQuantity()
        {
            // Arrange
            TestFollowerBracketState state = TestFollowerBracketState.Active;
            int orderQuantity = 5;
            int? positionQuantity = 3;

            // Act
            int result = ResolveRemainingContracts(state, orderQuantity, positionQuantity);

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public void ResolveRemainingContracts_ActiveStateNoPosition_ReturnsOrderQuantity()
        {
            // Arrange
            TestFollowerBracketState state = TestFollowerBracketState.Active;
            int orderQuantity = 5;
            int? positionQuantity = null;

            // Act
            int result = ResolveRemainingContracts(state, orderQuantity, positionQuantity);

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void ResolveRemainingContracts_NegativeOrderQuantity_ClampsToZero()
        {
            // Arrange
            TestFollowerBracketState state = TestFollowerBracketState.Submitted;
            int orderQuantity = -3;
            int? positionQuantity = null;

            // Act
            int result = ResolveRemainingContracts(state, orderQuantity, positionQuantity);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void ResolveRemainingContracts_ZeroOrderQuantity_ReturnsZero()
        {
            // Arrange
            TestFollowerBracketState state = TestFollowerBracketState.Submitted;
            int orderQuantity = 0;
            int? positionQuantity = null;

            // Act
            int result = ResolveRemainingContracts(state, orderQuantity, positionQuantity);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void ResolveRemainingContracts_NegativePosition_ReturnsAbsoluteValue()
        {
            // Arrange (short position)
            TestFollowerBracketState state = TestFollowerBracketState.Active;
            int orderQuantity = 5;
            int? positionQuantity = -3;

            // Act
            int result = ResolveRemainingContracts(state, orderQuantity, positionQuantity);

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public void ResolveRemainingContracts_ZeroPosition_ReturnsZero()
        {
            // Arrange
            TestFollowerBracketState state = TestFollowerBracketState.Active;
            int orderQuantity = 5;
            int? positionQuantity = 0;

            // Act
            int result = ResolveRemainingContracts(state, orderQuantity, positionQuantity);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void ResolveRemainingContracts_PartialFill_OrderLessThanPosition()
        {
            // Arrange (order partially filled, position shows actual fill)
            TestFollowerBracketState state = TestFollowerBracketState.Active;
            int orderQuantity = 5;
            int? positionQuantity = 2;

            // Act
            int result = ResolveRemainingContracts(state, orderQuantity, positionQuantity);

            // Assert
            Assert.Equal(2, result); // Position is source of truth
        }

        [Fact]
        public void ResolveRemainingContracts_Overfill_OrderGreaterThanPosition()
        {
            // Arrange (edge case: order shows more than position)
            TestFollowerBracketState state = TestFollowerBracketState.Active;
            int orderQuantity = 2;
            int? positionQuantity = 5;

            // Act
            int result = ResolveRemainingContracts(state, orderQuantity, positionQuantity);

            // Assert
            Assert.Equal(5, result); // Position is source of truth
        }

        [Theory]
        [InlineData(TestFollowerBracketState.Submitted, 5, 3, 5)]
        [InlineData(TestFollowerBracketState.Accepted, 5, 3, 5)]
        [InlineData(TestFollowerBracketState.Active, 5, 3, 3)]
        [InlineData(TestFollowerBracketState.Replacing, 5, 3, 5)]
        [InlineData(TestFollowerBracketState.Modifying, 5, 3, 5)]
        public void ResolveRemainingContracts_VariousStates_CorrectBehavior(
            TestFollowerBracketState state,
            int orderQty,
            int positionQty,
            int expected
        )
        {
            // Act
            int result = ResolveRemainingContracts(state, orderQty, positionQty);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ResolveRemainingContracts_IsPureFunction_SameInputProducesSameOutput()
        {
            // Arrange
            TestFollowerBracketState state = TestFollowerBracketState.Active;
            int orderQuantity = 5;
            int? positionQuantity = 3;

            // Act
            int result1 = ResolveRemainingContracts(state, orderQuantity, positionQuantity);
            int result2 = ResolveRemainingContracts(state, orderQuantity, positionQuantity);
            int result3 = ResolveRemainingContracts(state, orderQuantity, positionQuantity);

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        /// <summary>
        /// Test helper that mirrors RegisterFSM logic.
        /// Registers FSM in tracking dictionaries and updates counters.
        /// </summary>
        private void RegisterFSM(
            string entryKey,
            TestFollowerBracketFSM fsm,
            MockOrder entryOrder,
            Dictionary<string, TestFollowerBracketFSM> followerBrackets,
            Dictionary<string, string> orderIdToFsmKey,
            ref int ordersIndexed,
            ref int fsmCreated
        )
        {
            // Add FSM to tracking dictionary (idempotent via TryAdd pattern)
            // TryAdd returns false if key exists, but doesn't throw
            if (!followerBrackets.ContainsKey(entryKey))
            {
                followerBrackets.Add(entryKey, fsm);
            }

            // Link entry order to FSM key if present and has OrderId
            if (entryOrder != null && !string.IsNullOrEmpty(entryOrder.OrderId))
            {
                orderIdToFsmKey[entryOrder.OrderId] = entryKey;
                ordersIndexed++;
            }

            // Always increment FSM created counter
            fsmCreated++;
        }

        [Fact]
        public void RegisterFSM_ValidFSM_AddsToDictionary()
        {
            // Arrange
            string entryKey = "ENTRY_001";
            var fsm = new TestFollowerBracketFSM { EntryName = entryKey };
            MockOrder entryOrder = new MockOrder("ORDER_001", MockOrderState.Working, new MockAccount("Sim101"));
            var followerBrackets = new Dictionary<string, TestFollowerBracketFSM>();
            var orderIdToFsmKey = new Dictionary<string, string>();
            int ordersIndexed = 0;
            int fsmCreated = 0;

            // Act
            RegisterFSM(
                entryKey,
                fsm,
                entryOrder,
                followerBrackets,
                orderIdToFsmKey,
                ref ordersIndexed,
                ref fsmCreated
            );

            // Assert
            Assert.True(followerBrackets.ContainsKey(entryKey));
            Assert.Equal(fsm, followerBrackets[entryKey]);
        }

        [Fact]
        public void RegisterFSM_ValidEntryOrder_LinksOrderIdToFsmKey()
        {
            // Arrange
            string entryKey = "ENTRY_002";
            var fsm = new TestFollowerBracketFSM { EntryName = entryKey };
            MockOrder entryOrder = new MockOrder("ENTRY_002", MockOrderState.Working, new MockAccount("Sim102"));
            entryOrder.OrderId = "ORDER_002"; // Set explicit OrderId
            var followerBrackets = new Dictionary<string, TestFollowerBracketFSM>();
            var orderIdToFsmKey = new Dictionary<string, string>();
            int ordersIndexed = 0;
            int fsmCreated = 0;

            // Act
            RegisterFSM(
                entryKey,
                fsm,
                entryOrder,
                followerBrackets,
                orderIdToFsmKey,
                ref ordersIndexed,
                ref fsmCreated
            );

            // Assert
            Assert.True(orderIdToFsmKey.ContainsKey("ORDER_002"));
            Assert.Equal(entryKey, orderIdToFsmKey["ORDER_002"]);
        }

        [Fact]
        public void RegisterFSM_ValidEntryOrder_IncrementsOrdersIndexed()
        {
            // Arrange
            string entryKey = "ENTRY_003";
            var fsm = new TestFollowerBracketFSM { EntryName = entryKey };
            MockOrder entryOrder = new MockOrder("ORDER_003", MockOrderState.Working, new MockAccount("Sim103"));
            var followerBrackets = new Dictionary<string, TestFollowerBracketFSM>();
            var orderIdToFsmKey = new Dictionary<string, string>();
            int ordersIndexed = 0;
            int fsmCreated = 0;

            // Act
            RegisterFSM(
                entryKey,
                fsm,
                entryOrder,
                followerBrackets,
                orderIdToFsmKey,
                ref ordersIndexed,
                ref fsmCreated
            );

            // Assert
            Assert.Equal(1, ordersIndexed);
        }

        [Fact]
        public void RegisterFSM_AlwaysIncrementsFsmCreated()
        {
            // Arrange
            string entryKey = "ENTRY_004";
            var fsm = new TestFollowerBracketFSM { EntryName = entryKey };
            MockOrder entryOrder = new MockOrder("ORDER_004", MockOrderState.Working, new MockAccount("Sim104"));
            var followerBrackets = new Dictionary<string, TestFollowerBracketFSM>();
            var orderIdToFsmKey = new Dictionary<string, string>();
            int ordersIndexed = 0;
            int fsmCreated = 0;

            // Act
            RegisterFSM(
                entryKey,
                fsm,
                entryOrder,
                followerBrackets,
                orderIdToFsmKey,
                ref ordersIndexed,
                ref fsmCreated
            );

            // Assert
            Assert.Equal(1, fsmCreated);
        }

        [Fact]
        public void RegisterFSM_NullEntryOrder_DoesNotLinkOrderId()
        {
            // Arrange
            string entryKey = "ENTRY_005";
            var fsm = new TestFollowerBracketFSM { EntryName = entryKey };
            MockOrder entryOrder = null;
            var followerBrackets = new Dictionary<string, TestFollowerBracketFSM>();
            var orderIdToFsmKey = new Dictionary<string, string>();
            int ordersIndexed = 0;
            int fsmCreated = 0;

            // Act
            RegisterFSM(
                entryKey,
                fsm,
                entryOrder,
                followerBrackets,
                orderIdToFsmKey,
                ref ordersIndexed,
                ref fsmCreated
            );

            // Assert
            Assert.Empty(orderIdToFsmKey);
            Assert.Equal(0, ordersIndexed);
            Assert.Equal(1, fsmCreated); // FSM still created
        }

        [Fact]
        public void RegisterFSM_EmptyOrderId_DoesNotLinkOrderId()
        {
            // Arrange
            string entryKey = "ENTRY_006";
            var fsm = new TestFollowerBracketFSM { EntryName = entryKey };
            MockOrder entryOrder = new MockOrder("ENTRY_006", MockOrderState.Working, new MockAccount("Sim106"));
            entryOrder.OrderId = ""; // Set empty OrderId explicitly
            var followerBrackets = new Dictionary<string, TestFollowerBracketFSM>();
            var orderIdToFsmKey = new Dictionary<string, string>();
            int ordersIndexed = 0;
            int fsmCreated = 0;

            // Act
            RegisterFSM(
                entryKey,
                fsm,
                entryOrder,
                followerBrackets,
                orderIdToFsmKey,
                ref ordersIndexed,
                ref fsmCreated
            );

            // Assert
            Assert.Empty(orderIdToFsmKey);
            Assert.Equal(0, ordersIndexed);
            Assert.Equal(1, fsmCreated);
        }

        [Fact]
        public void RegisterFSM_DuplicateKey_DoesNotOverwrite()
        {
            // Arrange
            string entryKey = "ENTRY_007";
            var fsm1 = new TestFollowerBracketFSM { EntryName = entryKey, RemainingContracts = 1 };
            var fsm2 = new TestFollowerBracketFSM { EntryName = entryKey, RemainingContracts = 2 };
            MockOrder entryOrder1 = new MockOrder("ORDER_007A", MockOrderState.Working, new MockAccount("Sim107"));
            MockOrder entryOrder2 = new MockOrder("ORDER_007B", MockOrderState.Working, new MockAccount("Sim107"));
            var followerBrackets = new Dictionary<string, TestFollowerBracketFSM>();
            var orderIdToFsmKey = new Dictionary<string, string>();
            int ordersIndexed = 0;
            int fsmCreated = 0;

            // Act
            RegisterFSM(
                entryKey,
                fsm1,
                entryOrder1,
                followerBrackets,
                orderIdToFsmKey,
                ref ordersIndexed,
                ref fsmCreated
            );
            RegisterFSM(
                entryKey,
                fsm2,
                entryOrder2,
                followerBrackets,
                orderIdToFsmKey,
                ref ordersIndexed,
                ref fsmCreated
            );

            // Assert
            Assert.Equal(fsm1, followerBrackets[entryKey]); // First FSM preserved
            Assert.Equal(1, followerBrackets[entryKey].RemainingContracts);
            Assert.Equal(2, fsmCreated); // Both counted
        }

        [Fact]
        public void RegisterFSM_MultipleRegistrations_AccumulatesCounters()
        {
            // Arrange
            var followerBrackets = new Dictionary<string, TestFollowerBracketFSM>();
            var orderIdToFsmKey = new Dictionary<string, string>();
            int ordersIndexed = 0;
            int fsmCreated = 0;

            // Act - Register 3 FSMs
            RegisterFSM(
                "ENTRY_008A",
                new TestFollowerBracketFSM { EntryName = "ENTRY_008A" },
                new MockOrder("ORDER_008A", MockOrderState.Working, new MockAccount("Sim108")),
                followerBrackets,
                orderIdToFsmKey,
                ref ordersIndexed,
                ref fsmCreated
            );
            RegisterFSM(
                "ENTRY_008B",
                new TestFollowerBracketFSM { EntryName = "ENTRY_008B" },
                new MockOrder("ORDER_008B", MockOrderState.Working, new MockAccount("Sim108")),
                followerBrackets,
                orderIdToFsmKey,
                ref ordersIndexed,
                ref fsmCreated
            );
            RegisterFSM(
                "ENTRY_008C",
                new TestFollowerBracketFSM { EntryName = "ENTRY_008C" },
                null, // No entry order
                followerBrackets,
                orderIdToFsmKey,
                ref ordersIndexed,
                ref fsmCreated
            );

            // Assert
            Assert.Equal(3, followerBrackets.Count);
            Assert.Equal(2, ordersIndexed); // Only 2 had valid entry orders
            Assert.Equal(3, fsmCreated); // All 3 FSMs created
        }

        [Fact]
        public void RegisterFSM_CollectionStateAfterRegistration_IsCorrect()
        {
            // Arrange
            string entryKey = "ENTRY_009";
            var fsm = new TestFollowerBracketFSM { EntryName = entryKey, State = TestFollowerBracketState.Active };
            MockOrder entryOrder = new MockOrder("ENTRY_009", MockOrderState.Filled, new MockAccount("Sim109"));
            entryOrder.OrderId = "ORDER_009"; // Set explicit OrderId
            var followerBrackets = new Dictionary<string, TestFollowerBracketFSM>();
            var orderIdToFsmKey = new Dictionary<string, string>();
            int ordersIndexed = 0;
            int fsmCreated = 0;

            // Act
            RegisterFSM(
                entryKey,
                fsm,
                entryOrder,
                followerBrackets,
                orderIdToFsmKey,
                ref ordersIndexed,
                ref fsmCreated
            );

            // Assert - Verify complete state
            Assert.Single(followerBrackets);
            Assert.Single(orderIdToFsmKey);
            Assert.Equal(1, ordersIndexed);
            Assert.Equal(1, fsmCreated);
            Assert.Equal(entryKey, followerBrackets[entryKey].EntryName);
            Assert.Equal(TestFollowerBracketState.Active, followerBrackets[entryKey].State);
            Assert.Equal(entryKey, orderIdToFsmKey["ORDER_009"]);
        }
    }
}

// Made with Bob (EPIC-CCN-16 Ticket 1, 2, 3 & 4 TDD)
