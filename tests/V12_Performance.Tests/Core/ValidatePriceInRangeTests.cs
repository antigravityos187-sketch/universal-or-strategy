// ValidatePriceInRangeTests.cs
// xUnit tests for the ValidatePriceInRange logic extracted from HandleChartClick_ConvertPrice
// EPIC-W7-046 T3 -- post-round range guard
// Validates: returns false if clickPrice < minPrice - priceRange OR > maxPrice + priceRange

using Xunit;

namespace V12_Performance.Tests.Core
{
    public class ValidatePriceInRangeTests
    {
        // Pure-logic mirror of the extracted helper.
        // Bounds: [minPrice - priceRange, maxPrice + priceRange]
        private static bool ValidatePriceInRange(
            double clickPrice,
            double minPrice,
            double maxPrice,
            double priceRange
        )
        {
            if (clickPrice < minPrice - priceRange || clickPrice > maxPrice + priceRange)
                return false;
            return true;
        }

        [Fact]
        public void PriceInRange_ReturnsTrue()
        {
            // minPrice=90, maxPrice=110, priceRange=20 => bounds [70, 130]
            // clickPrice=100 is inside => true
            bool result = ValidatePriceInRange(100.0, 90.0, 110.0, 20.0);
            Assert.Equal(true, result);
        }

        [Fact]
        public void PriceBelowLowerBound_ReturnsFalse()
        {
            // lower bound = 90 - 20 = 70; clickPrice=65 < 70 => false
            bool result = ValidatePriceInRange(65.0, 90.0, 110.0, 20.0);
            Assert.Equal(false, result);
        }

        [Fact]
        public void PriceAboveUpperBound_ReturnsFalse()
        {
            // upper bound = 110 + 20 = 130; clickPrice=135 > 130 => false
            bool result = ValidatePriceInRange(135.0, 90.0, 110.0, 20.0);
            Assert.Equal(false, result);
        }

        [Fact]
        public void PriceAtLowerBoundary_ReturnsTrue()
        {
            // clickPrice exactly at lower bound = 70; 70 < 70 is false => in range => true
            bool result = ValidatePriceInRange(70.0, 90.0, 110.0, 20.0);
            Assert.Equal(true, result);
        }

        [Fact]
        public void PriceAtUpperBoundary_ReturnsTrue()
        {
            // clickPrice exactly at upper bound = 130; 130 > 130 is false => in range => true
            bool result = ValidatePriceInRange(130.0, 90.0, 110.0, 20.0);
            Assert.Equal(true, result);
        }

        [Fact]
        public void PriceJustBelowLowerBound_ReturnsFalse()
        {
            // lower bound = 70; clickPrice=69.99 < 70 => false
            bool result = ValidatePriceInRange(69.99, 90.0, 110.0, 20.0);
            Assert.Equal(false, result);
        }
    }
}
