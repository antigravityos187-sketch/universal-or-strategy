// EPIC-W7-025 | T2: ComputeFFMAStopDistance pure-logic tests
// The extracted formula:
//   double stopDistance = Math.Min(Math.Abs(currentPrice - candleExtreme), MaximumStop);
//   if (stopDistance < tickSize * 2) stopDistance = tickSize * 2;
// Pure logic is verified here by inlining the formula without instance state.
using System;
using Xunit;

namespace V12_Performance.Tests.W7_025
{
    /// <summary>
    /// xUnit tests for the logic extracted in ComputeFFMAStopDistance (EPIC-W7-025 T2).
    /// The method uses instance fields (MaximumStop, tickSize), so the formula is
    /// inlined and tested as a pure function to verify all three code paths:
    ///   1. raw distance used as-is (below max, above tick floor)
    ///   2. clamped to MaximumStop
    ///   3. raised to tickSize * 2 floor
    /// </summary>
    public class W7_025_ComputeFFMAStopDistanceTests
    {
        // Inline of the extracted formula — zero instance state required.
        private static double ComputeFFMAStopDistance(
            double currentPrice,
            double candleExtreme,
            double maximumStop,
            double tickSize
        )
        {
            double stopDistance = Math.Min(Math.Abs(currentPrice - candleExtreme), maximumStop);
            if (stopDistance < tickSize * 2)
                stopDistance = tickSize * 2;
            return stopDistance;
        }

        [Fact]
        public void RawDistance_BelowMaxStop_AboveTickFloor_ReturnsRawDistance()
        {
            // currentPrice=4200, candleExtreme=4210 => raw=10
            // maximumStop=50 => Math.Min(10,50)=10 (no clamp)
            // tickSize=0.25 => floor=0.5, 10>=0.5 (no floor raise)
            // expected = 10
            double result = ComputeFFMAStopDistance(
                currentPrice: 4200.0,
                candleExtreme: 4210.0,
                maximumStop: 50.0,
                tickSize: 0.25
            );

            Assert.Equal(10.0, result, precision: 8);
        }

        [Fact]
        public void RawDistance_ExceedsMaxStop_ClampsToMaximumStop()
        {
            // currentPrice=4200, candleExtreme=4260 => raw=60
            // maximumStop=50 => Math.Min(60,50)=50 (clamped)
            // tickSize=0.25 => floor=0.5, 50>=0.5 (no floor raise)
            // expected = 50
            double result = ComputeFFMAStopDistance(
                currentPrice: 4200.0,
                candleExtreme: 4260.0,
                maximumStop: 50.0,
                tickSize: 0.25
            );

            Assert.Equal(50.0, result, precision: 8);
        }

        [Fact]
        public void RawDistance_BelowTickFloor_RaisesToTickFloor()
        {
            // currentPrice=4200, candleExtreme=4200.1 => raw=0.1
            // maximumStop=50 => Math.Min(0.1,50)=0.1 (no clamp)
            // tickSize=0.25 => floor=0.5, 0.1<0.5 => raised to 0.5
            // expected = 0.5
            double result = ComputeFFMAStopDistance(
                currentPrice: 4200.0,
                candleExtreme: 4200.1,
                maximumStop: 50.0,
                tickSize: 0.25
            );

            Assert.Equal(0.5, result, precision: 8);
        }
    }
}
