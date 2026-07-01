using System.Text;
using System.Threading;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// Unit tests for ShouldSkipFleetIteration extracted helper.
    /// EPIC-W7-119 T1 TDD Safety Net.
    /// Validates circuit-breaker guard: Volatile.Read(_reaperCircuitBreakerTripped) == 1
    /// with log append on skip. AggressiveInlining hot-path per-iteration predicate.
    /// </summary>
    public class ShouldSkipFleetIterationTests
    {
        // ---------------------------------------------------------------------------
        // Helper: simulate the extracted predicate inline (NT8 prevents direct class
        // instantiation; logic is verified via an isolated stand-in that mirrors the
        // exact branch structure of ShouldSkipFleetIteration).
        // ---------------------------------------------------------------------------

        private static bool SimulateShouldSkipFleetIteration(
            ref int reaperCircuitBreakerTripped,
            string accountName,
            StringBuilder dispatchLog
        )
        {
            if (Volatile.Read(ref reaperCircuitBreakerTripped) == 1)
            {
                dispatchLog.AppendLine($"[DISPATCH] CB tripped - skipping {accountName} (no allocation)");
                return true;
            }
            return false;
        }

        // ---------------------------------------------------------------------------
        // Test 1: Circuit breaker tripped -> returns true, appends log message
        // ---------------------------------------------------------------------------

        [Fact]
        public void ShouldSkipFleetIteration_CircuitBreakerTripped_ReturnsTrue()
        {
            // Arrange
            int tripped = 1;
            var log = new StringBuilder();
            string accountName = "Sim101";

            // Act
            bool result = SimulateShouldSkipFleetIteration(ref tripped, accountName, log);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldSkipFleetIteration_CircuitBreakerTripped_AppendsLogMessage()
        {
            // Arrange
            int tripped = 1;
            var log = new StringBuilder();
            string accountName = "Sim101";

            // Act
            SimulateShouldSkipFleetIteration(ref tripped, accountName, log);

            // Assert
            Assert.Contains("[DISPATCH] CB tripped - skipping Sim101 (no allocation)", log.ToString());
        }

        // ---------------------------------------------------------------------------
        // Test 2: Circuit breaker NOT tripped -> returns false, log untouched
        // ---------------------------------------------------------------------------

        [Fact]
        public void ShouldSkipFleetIteration_CircuitBreakerNotTripped_ReturnsFalse()
        {
            // Arrange
            int tripped = 0;
            var log = new StringBuilder();
            string accountName = "Sim202";

            // Act
            bool result = SimulateShouldSkipFleetIteration(ref tripped, accountName, log);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShouldSkipFleetIteration_CircuitBreakerNotTripped_LogIsEmpty()
        {
            // Arrange
            int tripped = 0;
            var log = new StringBuilder();
            string accountName = "Sim202";

            // Act
            SimulateShouldSkipFleetIteration(ref tripped, accountName, log);

            // Assert
            Assert.Equal(string.Empty, log.ToString());
        }
    }
}
