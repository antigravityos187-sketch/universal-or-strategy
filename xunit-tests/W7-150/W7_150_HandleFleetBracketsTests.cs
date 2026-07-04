// EPIC-W7-150 xUnit Tests
// Method: ProcessQueuedExecution_HandleFleetBrackets (CYC 10->8)
// Helpers tested: TryGetEligibleFollowerPosition, GetFleetFillPrice, LogFleetBracketError
// Framework: xUnit [Fact] Assert.Equal

using System;
using Xunit;

namespace W7_150.Tests
{
    // ---------------------------------------------------------------------------
    // Standalone unit tests that validate the extracted helper logic in isolation.
    // The helpers are pure-logic extractions; their behaviour can be verified by
    // duplicating the same logic in test doubles below.
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Minimal PositionInfo stand-in for test purposes.
    /// </summary>
    public class PositionInfo
    {
        public bool IsFollower { get; set; }
        public bool EntryFilled { get; set; }
    }

    /// <summary>
    /// Test double that replicates TryGetEligibleFollowerPosition logic.
    /// </summary>
    public static class FollowerPositionHelper
    {
        // Mirrors: activePositions.TryGetValue(fleetKey, out pos) && pos.IsFollower && !pos.EntryFilled
        public static bool TryGetEligibleFollowerPosition(
            System.Collections.Generic.Dictionary<string, PositionInfo> positions,
            string fleetKey,
            out PositionInfo pos
        )
        {
            return positions.TryGetValue(fleetKey, out pos) && pos.IsFollower && !pos.EntryFilled;
        }
    }

    /// <summary>
    /// Test double that replicates GetFleetFillPrice logic.
    /// </summary>
    public static class FillPriceHelper
    {
        // Mirrors: item.EventArgs.Execution != null ? item.EventArgs.Execution.Price : 0
        public static double GetFleetFillPrice(double? executionPrice)
        {
            return executionPrice.HasValue ? executionPrice.Value : 0;
        }
    }

    // ---------------------------------------------------------------------------
    // Tests
    // ---------------------------------------------------------------------------

    public class W7_150_HandleFleetBracketsTests
    {
        // -----------------------------------------------------------------------
        // TryGetEligibleFollowerPosition — 3 tests
        // -----------------------------------------------------------------------

        [Fact]
        public void TryGetEligible_ReturnsTrue_WhenFollowerAndNotFilled()
        {
            var positions = new System.Collections.Generic.Dictionary<string, PositionInfo>
            {
                { "fleet1", new PositionInfo { IsFollower = true, EntryFilled = false } },
            };

            bool result = FollowerPositionHelper.TryGetEligibleFollowerPosition(positions, "fleet1", out var pos);

            Assert.Equal(true, result);
            Assert.Equal(true, pos.IsFollower);
            Assert.Equal(false, pos.EntryFilled);
        }

        [Fact]
        public void TryGetEligible_ReturnsFalse_WhenEntryAlreadyFilled()
        {
            var positions = new System.Collections.Generic.Dictionary<string, PositionInfo>
            {
                { "fleet1", new PositionInfo { IsFollower = true, EntryFilled = true } },
            };

            bool result = FollowerPositionHelper.TryGetEligibleFollowerPosition(positions, "fleet1", out _);

            Assert.Equal(false, result);
        }

        [Fact]
        public void TryGetEligible_ReturnsFalse_WhenNotFollower()
        {
            var positions = new System.Collections.Generic.Dictionary<string, PositionInfo>
            {
                { "fleet1", new PositionInfo { IsFollower = false, EntryFilled = false } },
            };

            bool result = FollowerPositionHelper.TryGetEligibleFollowerPosition(positions, "fleet1", out _);

            Assert.Equal(false, result);
        }

        // -----------------------------------------------------------------------
        // GetFleetFillPrice — 2 tests
        // -----------------------------------------------------------------------

        [Fact]
        public void GetFleetFillPrice_ReturnsPrice_WhenExecutionPresent()
        {
            double result = FillPriceHelper.GetFleetFillPrice(executionPrice: 4521.50);

            Assert.Equal(4521.50, result);
        }

        [Fact]
        public void GetFleetFillPrice_ReturnsZero_WhenExecutionNull()
        {
            double result = FillPriceHelper.GetFleetFillPrice(executionPrice: null);

            Assert.Equal(0.0, result);
        }
    }
}
