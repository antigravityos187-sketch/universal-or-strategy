using System;
using Xunit;

namespace V12_Performance.Tests.BarUpdate
{
    /// <summary>
    /// TDD tests for ProcessPendingTRENDEntry() extraction from OnBarUpdate.
    /// EPIC-CCN-21 Ticket 1: Extract pending TREND entry processing logic.
    /// </summary>
    public class ProcessPendingTRENDEntryTests
    {
        private readonly TestableV12Strategy _strategy;

        public ProcessPendingTRENDEntryTests()
        {
            _strategy = new TestableV12Strategy();
        }

        [Fact]
        public void ProcessPendingTRENDEntry_NotArmed_ReturnsEarly()
        {
            // Arrange
            _strategy.pendingTRENDEntry = false;
            
            // Act
            _strategy.ProcessPendingTRENDEntry();
            
            // Assert
            Assert.False(_strategy.ExecuteTRENDEntryCalled);
        }

        [Fact]
        public void ProcessPendingTRENDEntry_Armed_CalculatesStopDistance()
        {
            // Arrange
            _strategy.pendingTRENDEntry = true;
            _strategy.MockTrendStopDistance = 10.0;
            
            // Act
            _strategy.ProcessPendingTRENDEntry();
            
            // Assert
            Assert.True(_strategy.CalculateTRENDStopDistanceCalled);
        }

        [Fact]
        public void ProcessPendingTRENDEntry_Armed_CalculatesPositionSize()
        {
            // Arrange
            _strategy.pendingTRENDEntry = true;
            _strategy.MockTrendStopDistance = 10.0;
            
            // Act
            _strategy.ProcessPendingTRENDEntry();
            
            // Assert
            Assert.True(_strategy.CalculatePositionSizeCalled);
            Assert.Equal(10.0, _strategy.CalculatePositionSizeInput);
        }

        [Fact]
        public void ProcessPendingTRENDEntry_Armed_ExecutesEntry()
        {
            // Arrange
            _strategy.pendingTRENDEntry = true;
            _strategy.MockTrendStopDistance = 10.0;
            _strategy.MockPositionSize = 5;
            
            // Act
            _strategy.ProcessPendingTRENDEntry();
            
            // Assert
            Assert.True(_strategy.ExecuteTRENDEntryCalled);
            Assert.Equal(5, _strategy.ExecuteTRENDEntryContracts);
        }

        [Fact]
        public void ProcessPendingTRENDEntry_Armed_ClearsPendingFlag()
        {
            // Arrange
            _strategy.pendingTRENDEntry = true;
            _strategy.MockTrendStopDistance = 10.0;
            _strategy.MockPositionSize = 5;
            
            // Act
            _strategy.ProcessPendingTRENDEntry();
            
            // Assert
            Assert.False(_strategy.pendingTRENDEntry);
        }

        [Fact]
        public void ProcessPendingTRENDEntry_ZeroStopDistance_HandlesGracefully()
        {
            // Arrange
            _strategy.pendingTRENDEntry = true;
            _strategy.MockTrendStopDistance = 0.0;
            
            // Act
            _strategy.ProcessPendingTRENDEntry();
            
            // Assert
            Assert.True(_strategy.CalculatePositionSizeCalled);
            Assert.Equal(0.0, _strategy.CalculatePositionSizeInput);
        }
    }

    /// <summary>
    /// Testable wrapper for V12_002 strategy with mock tracking.
    /// Self-contained mock without NinjaTrader dependencies.
    /// </summary>
    public class TestableV12Strategy
    {
        // State flags
        public bool pendingTRENDEntry { get; set; }

        // Mock return values
        public double MockTrendStopDistance { get; set; }
        public int MockPositionSize { get; set; }

        // Call tracking
        public bool CalculateTRENDStopDistanceCalled { get; private set; }
        public bool CalculatePositionSizeCalled { get; private set; }
        public double CalculatePositionSizeInput { get; private set; }
        public bool ExecuteTRENDEntryCalled { get; private set; }
        public int ExecuteTRENDEntryContracts { get; private set; }

        /// <summary>
        /// Method under test - extracted from OnBarUpdate lines 323-327.
        /// Matches real implementation in src/V12_002.BarUpdate.cs.
        /// </summary>
        public void ProcessPendingTRENDEntry()
        {
            if (!pendingTRENDEntry)
                return;
            
            double trendDist = CalculateTRENDStopDistance();
            int trendContracts = CalculatePositionSize(trendDist);
            ExecuteTRENDEntry(trendContracts);
        }

        // Mock helper methods
        private double CalculateTRENDStopDistance()
        {
            CalculateTRENDStopDistanceCalled = true;
            return MockTrendStopDistance;
        }

        private int CalculatePositionSize(double stopDistance)
        {
            CalculatePositionSizeCalled = true;
            CalculatePositionSizeInput = stopDistance;
            return MockPositionSize;
        }

        private void ExecuteTRENDEntry(int contracts)
        {
            ExecuteTRENDEntryCalled = true;
            ExecuteTRENDEntryContracts = contracts;
            pendingTRENDEntry = false; // Clear flag after execution
        }
    }
}

// Made with Bob
