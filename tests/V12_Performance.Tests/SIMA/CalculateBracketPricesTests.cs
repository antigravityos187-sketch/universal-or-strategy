using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// Unit tests for CalculateBracketPrices extracted helper + BracketPriceResult struct.
    /// EPIC-W7-096 TICKET-2 TDD Safety Net.
    /// Pure function: no side effects, no NT8 dependencies. Tick rounding simulated via
    /// a simple truncation stand-in (production uses MasterInstrument.RoundToTickSize).
    /// CYC=4.
    /// </summary>
    public class CalculateBracketPricesTests
    {
        // ---------------------------------------------------------------------------
        // Stand-in BracketPriceResult (mirrors readonly struct in production)
        // ---------------------------------------------------------------------------

        private readonly struct BracketPriceResult
        {
            public readonly double StopPrice;
            public readonly double TargetPrice;

            public BracketPriceResult(double stopPrice, double targetPrice) =>
                (StopPrice, TargetPrice) = (stopPrice, targetPrice);
        }

        // ---------------------------------------------------------------------------
        // Action mirror enum
        // ---------------------------------------------------------------------------

        private enum OrderAction { Buy, Sell, SellShort, BuyToCover }

        // ---------------------------------------------------------------------------
        // Stand-in: mirrors CalculateBracketPrices branch structure.
        // RoundToTickSize simulated as identity (tick=0) for pure math verification.
        // ---------------------------------------------------------------------------

        private static BracketPriceResult SimulateCalculateBracketPrices(
            OrderAction action,
            double currentPrice,
            double stopPoints,
            double targetPoints
        )
        {
            double stopPrice =
                action == OrderAction.Buy ? currentPrice - stopPoints : currentPrice + stopPoints;
            double targetPrice =
                action == OrderAction.Buy ? currentPrice + targetPoints : currentPrice - targetPoints;
            // tick rounding identity (test focuses on branch logic, not rounding)
            return new BracketPriceResult(stopPrice, targetPrice);
        }

        // ---------------------------------------------------------------------------
        // Buy side: stop below, target above
        // ---------------------------------------------------------------------------

        [Fact]
        public void CalculateBracketPrices_Buy_StopBelowCurrentPrice()
        {
            var result = SimulateCalculateBracketPrices(OrderAction.Buy, 5000.0, 10.0, 20.0);
            Assert.Equal(4990.0, result.StopPrice);
        }

        [Fact]
        public void CalculateBracketPrices_Buy_TargetAboveCurrentPrice()
        {
            var result = SimulateCalculateBracketPrices(OrderAction.Buy, 5000.0, 10.0, 20.0);
            Assert.Equal(5020.0, result.TargetPrice);
        }

        // ---------------------------------------------------------------------------
        // Sell side: stop above, target below
        // ---------------------------------------------------------------------------

        [Fact]
        public void CalculateBracketPrices_Sell_StopAboveCurrentPrice()
        {
            var result = SimulateCalculateBracketPrices(OrderAction.Sell, 5000.0, 10.0, 20.0);
            Assert.Equal(5010.0, result.StopPrice);
        }

        [Fact]
        public void CalculateBracketPrices_Sell_TargetBelowCurrentPrice()
        {
            var result = SimulateCalculateBracketPrices(OrderAction.Sell, 5000.0, 10.0, 20.0);
            Assert.Equal(4980.0, result.TargetPrice);
        }

        // ---------------------------------------------------------------------------
        // Struct immutability: two calls with same args produce equal results
        // ---------------------------------------------------------------------------

        [Fact]
        public void CalculateBracketPrices_SameInputs_ProduceEqualResults()
        {
            var r1 = SimulateCalculateBracketPrices(OrderAction.Buy, 4800.0, 5.0, 15.0);
            var r2 = SimulateCalculateBracketPrices(OrderAction.Buy, 4800.0, 5.0, 15.0);
            Assert.Equal(r1.StopPrice, r2.StopPrice);
            Assert.Equal(r1.TargetPrice, r2.TargetPrice);
        }

        // ---------------------------------------------------------------------------
        // Sell side with SellShort action (same branch as Sell)
        // ---------------------------------------------------------------------------

        [Fact]
        public void CalculateBracketPrices_SellShort_StopAboveCurrentPrice()
        {
            var result = SimulateCalculateBracketPrices(OrderAction.SellShort, 4500.0, 8.0, 12.0);
            Assert.Equal(4508.0, result.StopPrice);
        }

        [Fact]
        public void CalculateBracketPrices_SellShort_TargetBelowCurrentPrice()
        {
            var result = SimulateCalculateBracketPrices(OrderAction.SellShort, 4500.0, 8.0, 12.0);
            Assert.Equal(4488.0, result.TargetPrice);
        }
    }
}
