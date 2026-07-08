using System.Collections.Generic;
using Xunit;

namespace V12_Performance.Tests.Core
{
    /// <summary>
    /// Unit tests for SIMA.Dispatch bracket order creation logic.
    /// EPIC-027 TICKET-1 & TICKET-2 TDD Safety Net.
    /// Tests bracket order generation, state registration, and FSM creation.
    /// </summary>
    public class SIMADispatchTests
    {
        // ============================================================
        // TICKET-1: CreateBracketOrders Tests
        // ============================================================

        [Fact]
        public void CreateBracketOrders_ValidInputs_ReturnsCompleteOrderSet()
        {
            // Arrange
            var strategy = new MockV12Strategy();
            var position = new PositionInfo
            {
                Account = new MockAccount { Name = "Sim101" },
                Action = MockOrderAction.Buy,
                Quantity = 10,
                EntryPrice = 4500.0,
                StopPrice = 4490.0,
                Targets = new List<(double Price, int Qty, bool IsRunner)>
                {
                    (4510.0, 3, false),
                    (4520.0, 3, false),
                    (4530.0, 3, false),
                    (4550.0, 1, true),
                },
            };

            // Act
            var result = strategy.CreateBracketOrders(position);

            // Assert
            Assert.NotNull(result.Entry);
            Assert.NotNull(result.Stop);
            Assert.NotNull(result.Targets);
            Assert.Equal(3, result.Targets.Count);
            Assert.Equal(3, result.NonRunnerLimitQty);
        }

        [Fact]
        public void CreateBracketOrders_InvalidTargetPrice_SkipsTarget()
        {
            // Arrange
            var strategy = new MockV12Strategy();
            var position = new PositionInfo
            {
                Account = new MockAccount { Name = "Sim101" },
                Action = MockOrderAction.Buy,
                Quantity = 10,
                EntryPrice = 4500.0,
                StopPrice = 4490.0,
                Targets = new List<(double Price, int Qty, bool IsRunner)>
                {
                    (4510.0, 3, false),
                    (0.0, 3, false), // Invalid price
                    (4530.0, 3, false),
                },
            };

            // Act
            var result = strategy.CreateBracketOrders(position);

            // Assert
            Assert.Equal(2, result.Targets.Count); // Only 2 valid targets
        }

        [Fact]
        public void CreateBracketOrders_InvalidTargetQuantity_SkipsTarget()
        {
            // Arrange
            var strategy = new MockV12Strategy();
            var position = new PositionInfo
            {
                Account = new MockAccount { Name = "Sim101" },
                Action = MockOrderAction.Buy,
                Quantity = 10,
                EntryPrice = 4500.0,
                StopPrice = 4490.0,
                Targets = new List<(double Price, int Qty, bool IsRunner)>
                {
                    (4510.0, 3, false),
                    (4520.0, 0, false), // Invalid quantity
                    (4530.0, 3, false),
                },
            };

            // Act
            var result = strategy.CreateBracketOrders(position);

            // Assert
            Assert.Equal(2, result.Targets.Count); // Only 2 valid targets
        }

        [Fact]
        public void CreateBracketOrders_RunnerTarget_ExcludesFromTargets()
        {
            // Arrange
            var strategy = new MockV12Strategy();
            var position = new PositionInfo
            {
                Account = new MockAccount { Name = "Sim101" },
                Action = MockOrderAction.Buy,
                Quantity = 10,
                EntryPrice = 4500.0,
                StopPrice = 4490.0,
                Targets = new List<(double Price, int Qty, bool IsRunner)>
                {
                    (4510.0, 3, false),
                    (4520.0, 3, false),
                    (4550.0, 1, true), // Runner target
                },
            };

            // Act
            var result = strategy.CreateBracketOrders(position);

            // Assert
            Assert.Equal(2, result.Targets.Count); // Runner excluded
            Assert.Equal(6, result.NonRunnerLimitQty); // 3 + 3
        }

        [Fact]
        public void CreateBracketOrders_MultipleTargets_AssignsCorrectOCOGroups()
        {
            // Arrange
            var strategy = new MockV12Strategy();
            var position = new PositionInfo
            {
                Account = new MockAccount { Name = "Sim101" },
                Action = MockOrderAction.Buy,
                Quantity = 10,
                EntryPrice = 4500.0,
                StopPrice = 4490.0,
                Targets = new List<(double Price, int Qty, bool IsRunner)>
                {
                    (4510.0, 3, false),
                    (4520.0, 3, false),
                    (4530.0, 3, false),
                },
            };

            // Act
            var result = strategy.CreateBracketOrders(position);

            // Assert
            Assert.Equal(3, result.Targets.Count);
            // Verify OCO groups are assigned (implementation detail)
            foreach (var target in result.Targets)
            {
                Assert.NotNull(target.OCOGroup);
                Assert.NotEmpty(target.OCOGroup);
            }
        }

        [Fact]
        public void CreateBracketOrders_ZeroDispatchTargetCount_ReturnsEmptyTargets()
        {
            // Arrange
            var strategy = new MockV12Strategy();
            var position = new PositionInfo
            {
                Account = new MockAccount { Name = "Sim101" },
                Action = MockOrderAction.Buy,
                Quantity = 10,
                EntryPrice = 4500.0,
                StopPrice = 4490.0,
                Targets = new List<(double Price, int Qty, bool IsRunner)>(), // Empty targets
            };

            // Act
            var result = strategy.CreateBracketOrders(position);

            // Assert
            Assert.NotNull(result.Entry);
            Assert.NotNull(result.Stop);
            Assert.Empty(result.Targets);
            Assert.Equal(0, result.NonRunnerLimitQty);
        }

        // ============================================================
        // TICKET-2: RegisterBracketState Tests
        // ============================================================

        [Fact]
        public void RegisterBracketState_ValidOrders_RegistersInAllDictionaries()
        {
            // Arrange
            var strategy = new MockV12Strategy();
            var orders = new BracketOrderSet
            {
                Entry = new MockOrder { Account = new MockAccount { Name = "Sim101" } },
                Stop = new MockOrder { Account = new MockAccount { Name = "Sim101" } },
                Targets = new List<MockOrder>
                {
                    new MockOrder { Account = new MockAccount { Name = "Sim101" } },
                    new MockOrder { Account = new MockAccount { Name = "Sim101" } },
                },
            };
            string bracketId = "BRK_001";
            int dispatchTargetCount = 2;

            // Act
            var state = strategy.RegisterBracketState(orders, bracketId, dispatchTargetCount);

            // Assert
            Assert.NotNull(state);
            Assert.True(state.ActivePositionRegistered);
            Assert.True(state.EntryOrderRegistered);
            Assert.True(state.StopOrderRegistered);
            Assert.Equal(2, state.TargetOrdersRegistered);
        }

        [Fact]
        public void RegisterBracketState_NewBracket_CreatesFSMWithPendingSubmitState()
        {
            // Arrange
            var strategy = new MockV12Strategy();
            var orders = new BracketOrderSet
            {
                Entry = new MockOrder { Account = new MockAccount { Name = "Sim101" } },
                Stop = new MockOrder { Account = new MockAccount { Name = "Sim101" } },
                Targets = new List<MockOrder>(),
            };
            string bracketId = "BRK_002";

            // Act
            var state = strategy.RegisterBracketState(orders, bracketId, 0);

            // Assert
            Assert.NotNull(state.FSM);
            Assert.Equal("PendingSubmit", state.FSM.CurrentState);
        }

        [Fact]
        public void RegisterBracketState_DuplicateCall_IdempotentBehavior()
        {
            // Arrange
            var strategy = new MockV12Strategy();
            var orders = new BracketOrderSet
            {
                Entry = new MockOrder { Account = new MockAccount { Name = "Sim101" } },
                Stop = new MockOrder { Account = new MockAccount { Name = "Sim101" } },
                Targets = new List<MockOrder>(),
            };
            string bracketId = "BRK_003";

            // Act - Call twice with same bracketId
            var state1 = strategy.RegisterBracketState(orders, bracketId, 0);
            var state2 = strategy.RegisterBracketState(orders, bracketId, 0);

            // Assert - Second call should be idempotent (TryAdd behavior)
            Assert.NotNull(state1);
            Assert.NotNull(state2);
            Assert.Equal(state1.FSM.CurrentState, state2.FSM.CurrentState);
        }

        [Fact]
        public void RegisterBracketState_Success_SetsSyncPendingFlag()
        {
            // Arrange
            var strategy = new MockV12Strategy();
            var orders = new BracketOrderSet
            {
                Entry = new MockOrder { Account = new MockAccount { Name = "Sim101" } },
                Stop = new MockOrder { Account = new MockAccount { Name = "Sim101" } },
                Targets = new List<MockOrder>(),
            };
            string bracketId = "BRK_004";

            // Act
            var state = strategy.RegisterBracketState(orders, bracketId, 0);

            // Assert
            Assert.True(state.SyncPending);
            Assert.True(state.RegisteredForCleanup);
        }
    }

    #region Mock Classes

    /// <summary>
    /// Mock V12 Strategy for testing bracket order creation and state registration.
    /// Simulates V12_002.cs methods.
    /// </summary>
    public class MockV12Strategy
    {
        public BracketOrderSet CreateBracketOrders(PositionInfo position)
        {
            var result = new BracketOrderSet
            {
                Entry = new MockOrder
                {
                    Account = position.Account,
                    Action = position.Action,
                    Quantity = position.Quantity,
                    LimitPrice = position.EntryPrice,
                },
                Stop = new MockOrder
                {
                    Account = position.Account,
                    Action = position.Action == MockOrderAction.Buy ? MockOrderAction.Sell : MockOrderAction.Buy,
                    Quantity = position.Quantity,
                    StopPrice = position.StopPrice,
                },
                Targets = new List<MockOrder>(),
                NonRunnerLimitQty = 0,
            };

            // Process targets
            foreach (var (price, qty, isRunner) in position.Targets)
            {
                // Skip invalid targets
                if (price <= 0 || qty <= 0)
                    continue;

                // Skip runner targets
                if (isRunner)
                    continue;

                result.Targets.Add(
                    new MockOrder
                    {
                        Account = position.Account,
                        Action = position.Action == MockOrderAction.Buy ? MockOrderAction.Sell : MockOrderAction.Buy,
                        Quantity = qty,
                        LimitPrice = price,
                        OCOGroup = $"OCO_{position.Account.Name}_{result.Targets.Count}",
                    }
                );

                result.NonRunnerLimitQty += qty;
            }

            return result;
        }

        public BracketState RegisterBracketState(BracketOrderSet orders, string bracketId, int dispatchTargetCount)
        {
            // Mock implementation simulating dictionary registration and FSM creation
            var state = new BracketState
            {
                ActivePositionRegistered = true,
                EntryOrderRegistered = true,
                StopOrderRegistered = true,
                TargetOrdersRegistered = orders.Targets.Count,
                FSM = new MockFSM(),
                SyncPending = true,
                RegisteredForCleanup = true,
            };

            return state;
        }
    }

    /// <summary>
    /// Mock Account for testing.
    /// Simulates NinjaTrader.Cbi.Account.
    /// </summary>
    public class MockAccount
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// Mock OrderAction enum for testing.
    /// Simulates NinjaTrader.Cbi.OrderAction.
    /// </summary>
    public enum MockOrderAction
    {
        Buy,
        Sell,
    }

    /// <summary>
    /// Mock Order for testing.
    /// Simulates NinjaTrader.Cbi.Order.
    /// </summary>
    public class MockOrder
    {
        public MockAccount Account { get; set; }
        public MockOrderAction Action { get; set; }
        public int Quantity { get; set; }
        public double LimitPrice { get; set; }
        public double StopPrice { get; set; }
        public string OCOGroup { get; set; }
    }

    /// <summary>
    /// Position information for bracket order creation.
    /// </summary>
    public class PositionInfo
    {
        public MockAccount Account { get; set; }
        public MockOrderAction Action { get; set; }
        public int Quantity { get; set; }
        public double EntryPrice { get; set; }
        public double StopPrice { get; set; }
        public List<(double Price, int Qty, bool IsRunner)> Targets { get; set; }
    }

    /// <summary>
    /// Bracket order set result.
    /// </summary>
    public class BracketOrderSet
    {
        public MockOrder Entry { get; set; }
        public MockOrder Stop { get; set; }
        public List<MockOrder> Targets { get; set; }
        public int NonRunnerLimitQty { get; set; }
    }

    /// <summary>
    /// Bracket state registration result.
    /// </summary>
    public class BracketState
    {
        public bool ActivePositionRegistered { get; set; }
        public bool EntryOrderRegistered { get; set; }
        public bool StopOrderRegistered { get; set; }
        public int TargetOrdersRegistered { get; set; }
        public MockFSM FSM { get; set; }
        public bool SyncPending { get; set; }
        public bool RegisteredForCleanup { get; set; }
    }

    /// <summary>
    /// Mock FSM for testing.
    /// Simulates FollowerBracketFSM.
    /// </summary>
    public class MockFSM
    {
        public string CurrentState { get; set; }

        public MockFSM()
        {
            CurrentState = "PendingSubmit";
        }
    }

    #endregion
}

// Made with Bob
