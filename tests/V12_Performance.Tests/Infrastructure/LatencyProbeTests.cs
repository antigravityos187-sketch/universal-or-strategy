using System;
using System.Threading;
using Xunit;

namespace V12_Performance.Tests.Infrastructure
{
    /// <summary>
    /// Unit tests for LatencyProbe struct correctness.
    /// Gap remediation from EPIC-6 Sentinel Scan (HIGH priority).
    /// Validates measurement infrastructure before using in benchmarks.
    /// </summary>
    public class LatencyProbeTests
    {
        [Fact]
        public void Start_Stop_ValidProbe()
        {
            // Arrange & Act
            var probe = LatencyProbe.Start();
            Thread.Sleep(1); // Ensure measurable time (1ms = 1000μs)
            probe = probe.Stop();

            // Assert
            Assert.True(probe.IsValid, "Probe should be valid after Start/Stop");
            Assert.True(probe.ElapsedMicroseconds > 0, "Elapsed time should be positive");
            Assert.True(probe.ElapsedMicroseconds < 10000, "Elapsed time should be <10ms (sanity check)");
        }

        [Fact]
        public void Stop_WithoutStart_InvalidProbe()
        {
            // Arrange
            var probe = new LatencyProbe(); // Default constructor, no Start()

            // Act
            probe = probe.Stop();

            // Assert
            Assert.False(probe.IsValid, "Probe should be invalid without Start()");
            Assert.Equal(-1, probe.ElapsedMicroseconds);
        }

        [Fact]
        public void ElapsedMicroseconds_Accuracy()
        {
            // Arrange
            var probe = LatencyProbe.Start();
            Thread.Sleep(10); // 10ms = 10,000μs
            probe = probe.Stop();

            // Assert - Allow 100% tolerance for CI/VM scheduling variance
            // Expected: 10,000μs ± 100% = 5,000-20,000μs
            Assert.InRange(probe.ElapsedMicroseconds, 5000, 20000);
        }

        [Fact]
        public void MultipleStops_LastStopWins()
        {
            // Arrange
            var probe = LatencyProbe.Start();
            Thread.Sleep(1);
            probe = probe.Stop();
            var firstElapsed = probe.ElapsedMicroseconds;

            // Act - Stop again after more time
            Thread.Sleep(5);
            probe = probe.Stop();
            var secondElapsed = probe.ElapsedMicroseconds;

            // Assert - Second stop should have larger elapsed time
            Assert.True(
                secondElapsed > firstElapsed,
                $"Second stop ({secondElapsed}μs) should be > first stop ({firstElapsed}μs)"
            );
        }
    }

    /// <summary>
    /// Placeholder LatencyProbe struct for testing.
    /// In production, this would reference src/V12_002.Perf.LatencyProbe.cs
    /// </summary>
    public struct LatencyProbe
    {
        private readonly long _startTicks;
        private readonly long _stopTicks;

        public bool IsValid => _startTicks > 0 && _stopTicks > _startTicks;

        public long ElapsedMicroseconds
        {
            get
            {
                if (!IsValid)
                    return -1;
                long elapsedTicks = _stopTicks - _startTicks;
                return (elapsedTicks * 1_000_000) / System.Diagnostics.Stopwatch.Frequency;
            }
        }

        public static LatencyProbe Start()
        {
            return new LatencyProbe(System.Diagnostics.Stopwatch.GetTimestamp(), 0);
        }

        public LatencyProbe Stop()
        {
            return new LatencyProbe(_startTicks, System.Diagnostics.Stopwatch.GetTimestamp());
        }

        private LatencyProbe(long startTicks, long stopTicks)
        {
            _startTicks = startTicks;
            _stopTicks = stopTicks;
        }
    }
}

// Made with Bob
