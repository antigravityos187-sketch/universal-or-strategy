using Xunit;

namespace V12_Performance.Tests.Core
{
    /// <summary>
    /// Unit tests for ConvertYCoordToPrice helper logic.
    /// EPIC-W7-046 T2: Coordinate conversion extracted from HandleChartClick_ConvertPrice.
    /// Tests pure function: clamps yInPanel to [0, effectivePriceHeight], then converts
    /// Y pixel coordinate to price via linear interpolation.
    /// Jane Street: xUnit [Fact] + Assert.Equal(); deterministic; test each helper independently.
    /// </summary>
    public class ConvertYCoordToPriceTests
    {
        // -----------------------------------------------------------------------
        // Mirror of the extracted private helper -- pure function, no NT8 deps.
        // Signature mirrors: private double ConvertYCoordToPrice(double yInPanel,
        //     double effectivePriceHeight, double maxPrice, double priceRange)
        // -----------------------------------------------------------------------
        private static double ConvertYCoordToPrice(
            double yInPanel,
            double effectivePriceHeight,
            double maxPrice,
            double priceRange
        )
        {
            if (yInPanel < 0)
                yInPanel = 0;
            if (yInPanel > effectivePriceHeight)
                yInPanel = effectivePriceHeight;
            return maxPrice - (yInPanel / effectivePriceHeight) * priceRange;
        }

        // -----------------------------------------------------------------------
        // Y=0 => maxPrice (top of chart)
        // -----------------------------------------------------------------------

        [Fact]
        public void AtTopOfChart_ReturnsMaxPrice()
        {
            // yInPanel=0 => ratio=0 => price = maxPrice - 0 = maxPrice
            double result = ConvertYCoordToPrice(0.0, 400.0, 5000.0, 200.0);
            Assert.Equal(5000.0, result);
        }

        // -----------------------------------------------------------------------
        // Y=effectivePriceHeight => minPrice (bottom of chart)
        // -----------------------------------------------------------------------

        [Fact]
        public void AtBottomOfChart_ReturnsMinPrice()
        {
            // yInPanel=400 => ratio=1 => price = maxPrice - priceRange = minPrice
            double result = ConvertYCoordToPrice(400.0, 400.0, 5000.0, 200.0);
            Assert.Equal(4800.0, result);
        }

        // -----------------------------------------------------------------------
        // Y at midpoint => midPrice
        // -----------------------------------------------------------------------

        [Fact]
        public void AtMidpoint_ReturnsMidPrice()
        {
            // yInPanel=200, effectivePriceHeight=400 => ratio=0.5 => price = 5000 - 0.5*200 = 4900
            double result = ConvertYCoordToPrice(200.0, 400.0, 5000.0, 200.0);
            Assert.Equal(4900.0, result);
        }

        // -----------------------------------------------------------------------
        // Clamp: Y < 0 => clamps to 0 => returns maxPrice
        // -----------------------------------------------------------------------

        [Fact]
        public void NegativeY_ClampsToZero_ReturnsMaxPrice()
        {
            // yInPanel=-50 clamps to 0 => price = maxPrice
            double result = ConvertYCoordToPrice(-50.0, 400.0, 5000.0, 200.0);
            Assert.Equal(5000.0, result);
        }

        // -----------------------------------------------------------------------
        // Clamp: Y > effectivePriceHeight => clamps to effectivePriceHeight => returns minPrice
        // -----------------------------------------------------------------------

        [Fact]
        public void YExceedsHeight_ClampsToHeight_ReturnsMinPrice()
        {
            // yInPanel=600 clamps to 400 => ratio=1 => price = minPrice
            double result = ConvertYCoordToPrice(600.0, 400.0, 5000.0, 200.0);
            Assert.Equal(4800.0, result);
        }
    }
}

// Made with Bob (EPIC-W7-046 T2)
