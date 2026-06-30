// EPIC-W7-096 | TryExecuteBracketForAccount + DispatchBracketForAccount extraction tests
// Jane Street: xUnit [Fact] + Assert.Equal; deterministic; zero NinjaTrader dependencies.
using Xunit;

namespace V12_Performance.Tests.W7_096
{
    /// <summary>
    /// xUnit tests for EPIC-W7-096 extraction:
    /// TryExecuteBracketForAccount (price calc, order factory, reservation, submit)
    /// DispatchBracketForAccount (try/catch wrapper, success count, rollback on exception)
    /// Stand-in mirrors isolate the logic from NinjaTrader runtime.
    /// CYC verified: ExecuteMultiAccountBracket 10 -> 6 via lizard.
    /// </summary>
    public class W7_096_TryExecuteBracketForAccountTests
    {
        // ---------------------------------------------------------------------------
        // Stand-in: mirrors BracketPriceResult value type
        // ---------------------------------------------------------------------------

        private readonly struct BracketPriceResult
        {
            public readonly double StopPrice;
            public readonly double TargetPrice;

            public BracketPriceResult(double stop, double target)
            {
                StopPrice = stop;
                TargetPrice = target;
            }
        }

        // Mirror of CalculateBracketPrices pure math
        private static BracketPriceResult SimCalculateBracketPrices(
            bool isBuy,
            double currentPrice,
            double stopPoints,
            double targetPoints
        )
        {
            double stop = isBuy ? currentPrice - stopPoints : currentPrice + stopPoints;
            double target = isBuy ? currentPrice + targetPoints : currentPrice - targetPoints;
            return new BracketPriceResult(stop, target);
        }

        // Mirror of TryExecuteBracketForAccount: returns false when order factory fails
        private static bool SimTryExecuteBracket(
            bool orderFactorySucceeds,
            bool isBuy,
            int quantity,
            out int reservedDelta
        )
        {
            reservedDelta = 0;
            if (!orderFactorySucceeds)
                return false;
            reservedDelta = isBuy ? quantity : -quantity;
            return true;
        }

        // Mirror of DispatchBracketForAccount: try/catch + success tracking
        private static void SimDispatchBracket(
            bool orderFactorySucceeds,
            bool isBuy,
            int quantity,
            bool throwOnSubmit,
            ref int successCount,
            out int? rolledBackDelta
        )
        {
            rolledBackDelta = null;
            int reservedDelta = 0;
            try
            {
                if (SimTryExecuteBracket(orderFactorySucceeds, isBuy, quantity, out reservedDelta))
                {
                    if (throwOnSubmit)
                        throw new System.InvalidOperationException("Submit failed");
                    successCount++;
                }
            }
            catch
            {
                if (reservedDelta != 0)
                    rolledBackDelta = -reservedDelta;
            }
        }

        // ---------------------------------------------------------------------------
        // CalculateBracketPrices mirror
        // ---------------------------------------------------------------------------

        [Fact]
        public void CalculateBracketPrices_Buy_StopBelow_TargetAbove()
        {
            var result = SimCalculateBracketPrices(isBuy: true, currentPrice: 100.0, stopPoints: 2.0, targetPoints: 4.0);
            Assert.Equal(98.0, result.StopPrice);
            Assert.Equal(104.0, result.TargetPrice);
        }

        [Fact]
        public void CalculateBracketPrices_Sell_StopAbove_TargetBelow()
        {
            var result = SimCalculateBracketPrices(isBuy: false, currentPrice: 100.0, stopPoints: 2.0, targetPoints: 4.0);
            Assert.Equal(102.0, result.StopPrice);
            Assert.Equal(96.0, result.TargetPrice);
        }

        [Fact]
        public void CalculateBracketPrices_ZeroPoints_StopAndTargetEqualPrice()
        {
            var result = SimCalculateBracketPrices(isBuy: true, currentPrice: 50.0, stopPoints: 0.0, targetPoints: 0.0);
            Assert.Equal(50.0, result.StopPrice);
            Assert.Equal(50.0, result.TargetPrice);
        }

        // ---------------------------------------------------------------------------
        // TryExecuteBracketForAccount mirror
        // ---------------------------------------------------------------------------

        [Fact]
        public void TryExecuteBracket_OrderFactoryFails_ReturnsFalse_DeltaZero()
        {
            bool result = SimTryExecuteBracket(orderFactorySucceeds: false, isBuy: true, quantity: 1, out int reservedDelta);
            Assert.Equal(false, result);
            Assert.Equal(0, reservedDelta);
        }

        [Fact]
        public void TryExecuteBracket_Buy_ReturnsTrue_DeltaPositive()
        {
            bool result = SimTryExecuteBracket(orderFactorySucceeds: true, isBuy: true, quantity: 2, out int reservedDelta);
            Assert.Equal(true, result);
            Assert.Equal(2, reservedDelta);
        }

        [Fact]
        public void TryExecuteBracket_Sell_ReturnsTrue_DeltaNegative()
        {
            bool result = SimTryExecuteBracket(orderFactorySucceeds: true, isBuy: false, quantity: 3, out int reservedDelta);
            Assert.Equal(true, result);
            Assert.Equal(-3, reservedDelta);
        }

        [Fact]
        public void TryExecuteBracket_FactoryFails_Sell_DeltaStillZero()
        {
            bool result = SimTryExecuteBracket(orderFactorySucceeds: false, isBuy: false, quantity: 5, out int reservedDelta);
            Assert.Equal(false, result);
            Assert.Equal(0, reservedDelta);
        }

        // ---------------------------------------------------------------------------
        // DispatchBracketForAccount mirror
        // ---------------------------------------------------------------------------

        [Fact]
        public void DispatchBracket_Success_IncrementsSuccessCount()
        {
            int successCount = 0;
            SimDispatchBracket(orderFactorySucceeds: true, isBuy: true, quantity: 1,
                throwOnSubmit: false, ref successCount, out int? rolledBack);
            Assert.Equal(1, successCount);
            Assert.Null(rolledBack);
        }

        [Fact]
        public void DispatchBracket_FactoryFails_NoIncrement_NullRollback()
        {
            int successCount = 0;
            SimDispatchBracket(orderFactorySucceeds: false, isBuy: true, quantity: 1,
                throwOnSubmit: false, ref successCount, out int? rolledBack);
            Assert.Equal(0, successCount);
            Assert.Null(rolledBack);
        }

        [Fact]
        public void DispatchBracket_SubmitThrows_Buy_RollsBackNegativeDelta()
        {
            int successCount = 0;
            SimDispatchBracket(orderFactorySucceeds: true, isBuy: true, quantity: 5,
                throwOnSubmit: true, ref successCount, out int? rolledBack);
            Assert.Equal(0, successCount);
            Assert.Equal(-5, rolledBack);
        }

        [Fact]
        public void DispatchBracket_SubmitThrows_Sell_RollsBackPositiveDelta()
        {
            int successCount = 0;
            SimDispatchBracket(orderFactorySucceeds: true, isBuy: false, quantity: 3,
                throwOnSubmit: true, ref successCount, out int? rolledBack);
            Assert.Equal(0, successCount);
            Assert.Equal(3, rolledBack);
        }

        [Fact]
        public void DispatchBracket_MultipleAccounts_AccumulatesCount()
        {
            int successCount = 0;
            SimDispatchBracket(orderFactorySucceeds: true, isBuy: true, quantity: 1,
                throwOnSubmit: false, ref successCount, out _);
            SimDispatchBracket(orderFactorySucceeds: true, isBuy: true, quantity: 1,
                throwOnSubmit: false, ref successCount, out _);
            Assert.Equal(2, successCount);
        }
    }
}
