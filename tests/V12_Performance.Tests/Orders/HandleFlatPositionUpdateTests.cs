using System;
using System.Collections.Generic;
using V12_Performance.Tests.Mocks;
using Xunit;

namespace V12_Performance.Tests.Orders
{
    /// <summary>
    /// TDD tests for HandleFlatPositionUpdate helper methods (EPIC-CCN-18 Ticket 1).
    /// Tests the 2 extracted boolean helper methods that reduce HandleFlatPositionUpdate from CYC 37 to 23.
    ///
    /// Test Strategy:
    /// - Test helper method logic directly using mock dictionaries
    /// - Validate account matching and state detection
    /// - No NT8 runtime required - pure unit tests
    ///
    /// Coverage:
    /// - Test 1-6: HasPendingEntryOrderForAccount (entry order detection)
    /// - Test 7-11: HasUnfilledPositionForAccount (active position detection)
    /// </summary>
    public class HandleFlatPositionUpdateTests
    {
        // ============================================================================
        // Helper 1 Tests: HasPendingEntryOrderForAccount (6 tests)
        // ============================================================================

        [Fact]
        public void HasPendingEntryOrderForAccount_WithPendingOrder_ReturnsTrue()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();
            var account = new MockAccount("Apex01");

            // Add entry order with Working state
            var entryOrder = new MockOrder("Entry_Long_1", MockOrderState.Working, account);
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: false);
            testHelper.AddEntryOrder("pos1", entryOrder, position);

            // Act
            bool result = testHelper.HasPendingEntryOrderForAccount("Apex01");

            // Assert
            Assert.True(result, "Should return true when account has pending entry order");
        }

        [Fact]
        public void HasPendingEntryOrderForAccount_WithNoOrders_ReturnsFalse()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();

            // Act
            bool result = testHelper.HasPendingEntryOrderForAccount("Apex01");

            // Assert
            Assert.False(result, "Should return false when entryOrders is empty");
        }

        [Fact]
        public void HasPendingEntryOrderForAccount_WithCompletedOrder_ReturnsFalse()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();
            var account = new MockAccount("Apex01");

            // Add entry order with Filled state (terminal)
            var entryOrder = new MockOrder("Entry_Long_1", MockOrderState.Filled, account);
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: true);
            testHelper.AddEntryOrder("pos1", entryOrder, position);

            // Act
            bool result = testHelper.HasPendingEntryOrderForAccount("Apex01");

            // Assert
            Assert.False(result, "Should return false when order is in terminal state (Filled)");
        }

        [Fact]
        public void HasPendingEntryOrderForAccount_WithDifferentAccount_ReturnsFalse()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();
            var account = new MockAccount("Apex02");

            // Add entry order for different account
            var entryOrder = new MockOrder("Entry_Long_1", MockOrderState.Working, account);
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: false);
            testHelper.AddEntryOrder("pos1", entryOrder, position);

            // Act
            bool result = testHelper.HasPendingEntryOrderForAccount("Apex01");

            // Assert
            Assert.False(result, "Should return false when searching for different account");
        }

        [Fact]
        public void HasPendingEntryOrderForAccount_WithMultipleAccounts_ReturnsCorrectly()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();
            var account1 = new MockAccount("Apex01");
            var account2 = new MockAccount("Apex02");

            // Add entry orders for both accounts
            var entryOrder1 = new MockOrder("Entry_Long_1", MockOrderState.Working, account1);
            var position1 = testHelper.CreateMockPositionInfo(account1, entryFilled: false);
            testHelper.AddEntryOrder("pos1", entryOrder1, position1);

            var entryOrder2 = new MockOrder("Entry_Long_2", MockOrderState.Working, account2);
            var position2 = testHelper.CreateMockPositionInfo(account2, entryFilled: false);
            testHelper.AddEntryOrder("pos2", entryOrder2, position2);

            // Act
            bool result = testHelper.HasPendingEntryOrderForAccount("Apex01");

            // Assert
            Assert.True(result, "Should return true for Apex01 when it has pending entry");
        }

        [Fact]
        public void HasPendingEntryOrderForAccount_WithNullAccount_ThrowsException()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                testHelper.HasPendingEntryOrderForAccount(null);
            });

            Assert.Contains("accountName", exception.Message);
        }

        // ============================================================================
        // Helper 2 Tests: HasUnfilledPositionForAccount (5 tests)
        // ============================================================================

        [Fact]
        public void HasUnfilledPositionForAccount_WithActivePosition_ReturnsTrue()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();
            var account = new MockAccount("Apex01");

            // Add active position with EntryFilled = false
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: false);
            testHelper.AddActivePosition("pos1", position);

            // Act
            bool result = testHelper.HasUnfilledPositionForAccount("Apex01");

            // Assert
            Assert.True(result, "Should return true when account has active unfilled position");
        }

        [Fact]
        public void HasUnfilledPositionForAccount_WithNoPositions_ReturnsFalse()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();

            // Act
            bool result = testHelper.HasUnfilledPositionForAccount("Apex01");

            // Assert
            Assert.False(result, "Should return false when activePositions is empty");
        }

        [Fact]
        public void HasUnfilledPositionForAccount_WithDifferentAccount_ReturnsFalse()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();
            var account = new MockAccount("Apex02");

            // Add active position for different account
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: false);
            testHelper.AddActivePosition("pos1", position);

            // Act
            bool result = testHelper.HasUnfilledPositionForAccount("Apex01");

            // Assert
            Assert.False(result, "Should return false when searching for different account");
        }

        [Fact]
        public void HasUnfilledPositionForAccount_WithMultipleAccounts_ReturnsCorrectly()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();
            var account1 = new MockAccount("Apex01");
            var account2 = new MockAccount("Apex02");

            // Add active positions for both accounts
            var position1 = testHelper.CreateMockPositionInfo(account1, entryFilled: false);
            testHelper.AddActivePosition("pos1", position1);

            var position2 = testHelper.CreateMockPositionInfo(account2, entryFilled: false);
            testHelper.AddActivePosition("pos2", position2);

            // Act
            bool result = testHelper.HasUnfilledPositionForAccount("Apex01");

            // Assert
            Assert.True(result, "Should return true for Apex01 when it has active position");
        }

        [Fact]
        public void HasUnfilledPositionForAccount_WithNullAccount_ThrowsException()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                testHelper.HasUnfilledPositionForAccount(null);
            });

            Assert.Contains("accountName", exception.Message);
        // ============================================================================
        // Helper 3 Tests: CancelOrphanedOrdersForPosition (8 tests)
        // ============================================================================

        [Fact]
        public void CancelOrphanedOrdersForPosition_WithWorkingStopOrder_CancelsStop()
        {
            // Arrange
            var testHelper = new CancellationTestHelper();
            var account = new MockAccount("Apex01");
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: true);
            string posKey = "pos1";

            // Add working stop order
            var stopOrder = new MockOrder("Stop_1", MockOrderState.Working, account);
            testHelper.AddStopOrder(posKey, stopOrder);

            // Act
            testHelper.CancelOrphanedOrdersForPosition(posKey, position);

            // Assert
            Assert.Equal(1, testHelper.CancelCallCount);
            Assert.Contains(stopOrder, testHelper.CancelledOrders);
        }

        [Fact]
        public void CancelOrphanedOrdersForPosition_WithAcceptedStopOrder_CancelsStop()
        {
            // Arrange
            var testHelper = new CancellationTestHelper();
            var account = new MockAccount("Apex01");
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: true);
            string posKey = "pos1";

            // Add accepted stop order
            var stopOrder = new MockOrder("Stop_1", MockOrderState.Accepted, account);
            testHelper.AddStopOrder(posKey, stopOrder);

            // Act
            testHelper.CancelOrphanedOrdersForPosition(posKey, position);

            // Assert
            Assert.Equal(1, testHelper.CancelCallCount);
            Assert.Contains(stopOrder, testHelper.CancelledOrders);
        }

        [Fact]
        public void CancelOrphanedOrdersForPosition_WithFilledStopOrder_DoesNotCancel()
        {
            // Arrange
            var testHelper = new CancellationTestHelper();
            var account = new MockAccount("Apex01");
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: true);
            string posKey = "pos1";

            // Add filled stop order (terminal state)
            var stopOrder = new MockOrder("Stop_1", MockOrderState.Filled, account);
            testHelper.AddStopOrder(posKey, stopOrder);

            // Act
            testHelper.CancelOrphanedOrdersForPosition(posKey, position);

            // Assert
            Assert.Equal(0, testHelper.CancelCallCount);
        }

        [Fact]
        public void CancelOrphanedOrdersForPosition_WithNoStopOrder_DoesNotThrow()
        {
            // Arrange
            var testHelper = new CancellationTestHelper();
            var account = new MockAccount("Apex01");
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: true);
            string posKey = "pos1";

            // Act & Assert (no exception)
            testHelper.CancelOrphanedOrdersForPosition(posKey, position);
            Assert.Equal(0, testHelper.CancelCallCount);
        }

        [Fact]
        public void CancelOrphanedOrdersForPosition_WithWorkingTargetOrders_CancelsAll()
        {
            // Arrange
            var testHelper = new CancellationTestHelper();
            var account = new MockAccount("Apex01");
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: true);
            string posKey = "pos1";

            // Add all 5 target orders with Working state
            for (int t = 1; t <= 5; t++)
            {
                var targetOrder = new MockOrder($"Target_{t}", MockOrderState.Working, account);
                testHelper.AddTargetOrder(posKey, t, targetOrder);
            }

            // Act
            testHelper.CancelOrphanedOrdersForPosition(posKey, position);

            // Assert
            Assert.Equal(5, testHelper.CancelCallCount);
        }

        [Fact]
        public void CancelOrphanedOrdersForPosition_WithAcceptedTargetOrders_CancelsAll()
        {
            // Arrange
            var testHelper = new CancellationTestHelper();
            var account = new MockAccount("Apex01");
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: true);
            string posKey = "pos1";

            // Add all 5 target orders with Accepted state
            for (int t = 1; t <= 5; t++)
            {
                var targetOrder = new MockOrder($"Target_{t}", MockOrderState.Accepted, account);
                testHelper.AddTargetOrder(posKey, t, targetOrder);
            }

            // Act
            testHelper.CancelOrphanedOrdersForPosition(posKey, position);

            // Assert
            Assert.Equal(5, testHelper.CancelCallCount);
        }

        [Fact]
        public void CancelOrphanedOrdersForPosition_WithFilledTargetOrders_DoesNotCancel()
        {
            // Arrange
            var testHelper = new CancellationTestHelper();
            var account = new MockAccount("Apex01");
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: true);
            string posKey = "pos1";

            // Add all 5 target orders with Filled state (terminal)
            for (int t = 1; t <= 5; t++)
            {
                var targetOrder = new MockOrder($"Target_{t}", MockOrderState.Filled, account);
                testHelper.AddTargetOrder(posKey, t, targetOrder);
            }

            // Act
            testHelper.CancelOrphanedOrdersForPosition(posKey, position);

            // Assert
            Assert.Equal(0, testHelper.CancelCallCount);
        }

        [Fact]
        public void CancelOrphanedOrdersForPosition_WithMissingTargetOrders_DoesNotThrow()
        {
            // Arrange
            var testHelper = new CancellationTestHelper();
            var account = new MockAccount("Apex01");
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: true);
            string posKey = "pos1";

            // Act & Assert (no exception, no target orders exist)
            testHelper.CancelOrphanedOrdersForPosition(posKey, position);
            Assert.Equal(0, testHelper.CancelCallCount);
        }
    }

    /// <summary>
    /// Test helper for CancelOrphanedOrdersForPosition tests.
    /// Simulates stop/target order dictionaries and tracks cancellation calls.
    /// </summary>
    internal class CancellationTestHelper
    {
        private readonly Dictionary<string, MockOrder> _stopOrders;
        private readonly Dictionary<int, Dictionary<string, MockOrder>> _targetOrders;
        public List<MockOrder> CancelledOrders { get; }
        public int CancelCallCount => CancelledOrders.Count;

        public CancellationTestHelper()
        {
            _stopOrders = new Dictionary<string, MockOrder>();
            _targetOrders = new Dictionary<int, Dictionary<string, MockOrder>>();
            CancelledOrders = new List<MockOrder>();

            // Initialize 5 target dictionaries
            for (int t = 1; t <= 5; t++)
            {
                _targetOrders[t] = new Dictionary<string, MockOrder>();
            }
        }

        public void AddStopOrder(string posKey, MockOrder order)
        {
            _stopOrders[posKey] = order;
        }

        public void AddTargetOrder(string posKey, int targetNum, MockOrder order)
        {
            if (targetNum < 1 || targetNum > 5)
                throw new ArgumentOutOfRangeException(nameof(targetNum), "Target number must be 1-5");

            _targetOrders[targetNum][posKey] = order;
        }

        public MockPositionInfo CreateMockPositionInfo(MockAccount account, bool entryFilled)
        {
            return new MockPositionInfo
            {
                ExecutingAccount = account,
                EntryFilled = entryFilled,
                RemainingContracts = entryFilled ? 0 : 1,
            };
        }

        /// <summary>
        /// Mock implementation of CancelOrphanedOrdersForPosition.
        /// This simulates the logic from V12_002.Orders.Callbacks.Execution.cs lines 118-140.
        /// Will be replaced by actual method call after extraction.
        /// </summary>
        public void CancelOrphanedOrdersForPosition(string posKey, MockPositionInfo pos)
        {
            // Cancel stop order if active
            if (_stopOrders.TryGetValue(posKey, out var stopOrder))
            {
                if (
                    stopOrder != null
                    && (
                        stopOrder.OrderState == MockOrderState.Working
                        || stopOrder.OrderState == MockOrderState.Accepted
                    )
                )
                {
                    CancelledOrders.Add(stopOrder);
                }
            }

            // Cancel all 5 target orders if active
            for (int tNum = 1; tNum <= 5; tNum++)
            {
                var tDict = _targetOrders[tNum];
                if (tDict != null && tDict.TryGetValue(posKey, out var tOrder))
                {
                    if (
                        tOrder != null
                        && (
                            tOrder.OrderState == MockOrderState.Working
                            || tOrder.OrderState == MockOrderState.Accepted
                        )
                    )
                    {
                        CancelledOrders.Add(tOrder);
                    }
                }
            }
        }
    }
        }
    }

    /// <summary>
    /// Test helper class that simulates the V12_002 strategy's internal state.
    /// Provides mock implementations of HasPendingEntryOrderForAccount and HasUnfilledPositionForAccount
    /// that will be replaced by the actual extracted methods after implementation.
    /// </summary>
    internal class HandleFlatPositionUpdateTestHelper
    {
        private readonly Dictionary<string, KeyValuePair<MockOrder, MockPositionInfo>> _entryOrders;
        private readonly Dictionary<string, MockPositionInfo> _activePositions;

        public HandleFlatPositionUpdateTestHelper()
        {
            _entryOrders = new Dictionary<string, KeyValuePair<MockOrder, MockPositionInfo>>();
            _activePositions = new Dictionary<string, MockPositionInfo>();
        }

        public void AddEntryOrder(string key, MockOrder order, MockPositionInfo position)
        {
            _entryOrders[key] = new KeyValuePair<MockOrder, MockPositionInfo>(order, position);
        }

        public void AddActivePosition(string key, MockPositionInfo position)
        {
            _activePositions[key] = position;
        }

        public MockPositionInfo CreateMockPositionInfo(MockAccount account, bool entryFilled)
        {
            return new MockPositionInfo
            {
                ExecutingAccount = account,
                EntryFilled = entryFilled,
                RemainingContracts = entryFilled ? 0 : 1,
            };
        }

        /// <summary>
        /// Mock implementation of HasPendingEntryOrderForAccount.
        /// This simulates the logic from V12_002.Orders.Callbacks.Execution.cs lines 78-92.
        /// Will be replaced by actual method call after extraction.
        /// </summary>
        public bool HasPendingEntryOrderForAccount(string accountName)
        {
            if (accountName == null)
                throw new ArgumentNullException(nameof(accountName));

            foreach (var kvp in _entryOrders)
            {
                var ord = kvp.Value.Key;
                var pos = kvp.Value.Value;

                if (
                    ord != null
                    && !IsOrderTerminal(ord.OrderState)
                    && pos != null
                    && pos.ExecutingAccount != null
                    && pos.ExecutingAccount.Name == accountName
                )
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Mock implementation of HasUnfilledPositionForAccount.
        /// This simulates the logic from V12_002.Orders.Callbacks.Execution.cs lines 97-109.
        /// Will be replaced by actual method call after extraction.
        /// </summary>
        public bool HasUnfilledPositionForAccount(string accountName)
        {
            if (accountName == null)
                throw new ArgumentNullException(nameof(accountName));

            foreach (var kvp in _activePositions)
            {
                if (
                    kvp.Value.ExecutingAccount != null
                    && kvp.Value.ExecutingAccount.Name == accountName
                    && !kvp.Value.EntryFilled
                )
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsOrderTerminal(MockOrderState state)
        {
            return state == MockOrderState.Filled
                || state == MockOrderState.Cancelled
                || state == MockOrderState.Rejected;
        }
    }

    /// <summary>
    /// Mock PositionInfo for testing.
    /// Simulates the internal PositionInfo class from V12_002.
    /// </summary>
    internal class MockPositionInfo
    {
        public MockAccount ExecutingAccount { get; set; }
        public bool EntryFilled { get; set; }
        public int RemainingContracts { get; set; }
    }
}

// Made with Bob (EPIC-CCN-18 Ticket 1 TDD Infrastructure)
