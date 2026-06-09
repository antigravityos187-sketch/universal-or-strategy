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
    /// - Test 1-6: HasPendingEntryForAccount (entry order detection)
    /// - Test 7-11: HasActivePositionForAccount (active position detection)
    /// </summary>
    public class HandleFlatPositionUpdateTests
    {
        // ============================================================================
        // Helper 1 Tests: HasPendingEntryForAccount (6 tests)
        // ============================================================================

        [Fact]
        public void HasPendingEntryForAccount_WithPendingOrder_ReturnsTrue()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();
            var account = new MockAccount("Apex01");

            // Add entry order with Working state
            var entryOrder = new MockOrder("Entry_Long_1", MockOrderState.Working, account);
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: false);
            testHelper.AddEntryOrder("pos1", entryOrder, position);

            // Act
            bool result = testHelper.HasPendingEntryForAccount("Apex01");

            // Assert
            Assert.True(result, "Should return true when account has pending entry order");
        }

        [Fact]
        public void HasPendingEntryForAccount_WithNoOrders_ReturnsFalse()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();

            // Act
            bool result = testHelper.HasPendingEntryForAccount("Apex01");

            // Assert
            Assert.False(result, "Should return false when entryOrders is empty");
        }

        [Fact]
        public void HasPendingEntryForAccount_WithCompletedOrder_ReturnsFalse()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();
            var account = new MockAccount("Apex01");

            // Add entry order with Filled state (terminal)
            var entryOrder = new MockOrder("Entry_Long_1", MockOrderState.Filled, account);
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: true);
            testHelper.AddEntryOrder("pos1", entryOrder, position);

            // Act
            bool result = testHelper.HasPendingEntryForAccount("Apex01");

            // Assert
            Assert.False(result, "Should return false when order is in terminal state (Filled)");
        }

        [Fact]
        public void HasPendingEntryForAccount_WithDifferentAccount_ReturnsFalse()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();
            var account = new MockAccount("Apex02");

            // Add entry order for different account
            var entryOrder = new MockOrder("Entry_Long_1", MockOrderState.Working, account);
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: false);
            testHelper.AddEntryOrder("pos1", entryOrder, position);

            // Act
            bool result = testHelper.HasPendingEntryForAccount("Apex01");

            // Assert
            Assert.False(result, "Should return false when searching for different account");
        }

        [Fact]
        public void HasPendingEntryForAccount_WithMultipleAccounts_ReturnsCorrectly()
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
            bool result = testHelper.HasPendingEntryForAccount("Apex01");

            // Assert
            Assert.True(result, "Should return true for Apex01 when it has pending entry");
        }

        [Fact]
        public void HasPendingEntryForAccount_WithNullAccount_ThrowsException()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                testHelper.HasPendingEntryForAccount(null);
            });

            Assert.Contains("accountName", exception.Message);
        }

        // ============================================================================
        // Helper 2 Tests: HasActivePositionForAccount (5 tests)
        // ============================================================================

        [Fact]
        public void HasActivePositionForAccount_WithActivePosition_ReturnsTrue()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();
            var account = new MockAccount("Apex01");

            // Add active position with EntryFilled = false
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: false);
            testHelper.AddActivePosition("pos1", position);

            // Act
            bool result = testHelper.HasActivePositionForAccount("Apex01");

            // Assert
            Assert.True(result, "Should return true when account has active unfilled position");
        }

        [Fact]
        public void HasActivePositionForAccount_WithNoPositions_ReturnsFalse()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();

            // Act
            bool result = testHelper.HasActivePositionForAccount("Apex01");

            // Assert
            Assert.False(result, "Should return false when activePositions is empty");
        }

        [Fact]
        public void HasActivePositionForAccount_WithDifferentAccount_ReturnsFalse()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();
            var account = new MockAccount("Apex02");

            // Add active position for different account
            var position = testHelper.CreateMockPositionInfo(account, entryFilled: false);
            testHelper.AddActivePosition("pos1", position);

            // Act
            bool result = testHelper.HasActivePositionForAccount("Apex01");

            // Assert
            Assert.False(result, "Should return false when searching for different account");
        }

        [Fact]
        public void HasActivePositionForAccount_WithMultipleAccounts_ReturnsCorrectly()
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
            bool result = testHelper.HasActivePositionForAccount("Apex01");

            // Assert
            Assert.True(result, "Should return true for Apex01 when it has active position");
        }

        [Fact]
        public void HasActivePositionForAccount_WithNullAccount_ThrowsException()
        {
            // Arrange
            var testHelper = new HandleFlatPositionUpdateTestHelper();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                testHelper.HasActivePositionForAccount(null);
            });

            Assert.Contains("accountName", exception.Message);
        }
    }

    /// <summary>
    /// Test helper class that simulates the V12_002 strategy's internal state.
    /// Provides mock implementations of HasPendingEntryForAccount and HasActivePositionForAccount
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
        /// Mock implementation of HasPendingEntryForAccount.
        /// This simulates the logic from V12_002.Orders.Callbacks.Execution.cs lines 78-92.
        /// Will be replaced by actual method call after extraction.
        /// </summary>
        public bool HasPendingEntryForAccount(string accountName)
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
        /// Mock implementation of HasActivePositionForAccount.
        /// This simulates the logic from V12_002.Orders.Callbacks.Execution.cs lines 97-109.
        /// Will be replaced by actual method call after extraction.
        /// </summary>
        public bool HasActivePositionForAccount(string accountName)
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
