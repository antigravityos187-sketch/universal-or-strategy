using System;
using System.Collections.Concurrent;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// Unit tests for AdoptFleetOrders helper methods.
    /// EPIC-CCN-17 TDD Safety Net: Validates RouteOrderToTargetDict pure function.
    /// Tests dictionary routing logic for all 7 order classification types.
    ///
    /// Note: Since V12_002 is a NinjaTrader Strategy (not a standalone class),
    /// we test the logic pattern using a standalone helper that mirrors the extracted method.
    /// The actual method will be internal in V12_002.SIMA.Lifecycle.cs.
    /// </summary>
    public class AdoptFleetOrdersTests
    {
        // Mock dictionaries for testing
        private readonly ConcurrentDictionary<string, object> stopOrders = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> target1Orders =
            new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> target2Orders =
            new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> target3Orders =
            new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> target4Orders =
            new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> target5Orders =
            new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> entryOrders = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Test helper that mirrors RouteOrderToTargetDict logic.
        /// This allows us to test the pure function behavior in isolation.
        /// </summary>
        private ConcurrentDictionary<string, object> RouteOrderToTargetDict(
            string classification,
            string orderName,
            out string key,
            out string dictName
        )
        {
            ConcurrentDictionary<string, object> targetDict = null;
            key = null;
            dictName = null;

            switch (classification)
            {
                case "stop":
                    targetDict = stopOrders;
                    key = orderName.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase)
                        ? orderName.Substring(5)
                        : orderName.Substring(2);
                    dictName = "stopOrders";
                    break;
                case "target1":
                    targetDict = target1Orders;
                    key = orderName.Substring(3);
                    dictName = "target1Orders";
                    break;
                case "target2":
                    targetDict = target2Orders;
                    key = orderName.Substring(3);
                    dictName = "target2Orders";
                    break;
                case "target3":
                    targetDict = target3Orders;
                    key = orderName.Substring(3);
                    dictName = "target3Orders";
                    break;
                case "target4":
                    targetDict = target4Orders;
                    key = orderName.Substring(3);
                    dictName = "target4Orders";
                    break;
                case "target5":
                    targetDict = target5Orders;
                    key = orderName.Substring(3);
                    dictName = "target5Orders";
                    break;
                case "entry":
                    targetDict = entryOrders;
                    key = orderName;
                    dictName = "entryOrders";
                    break;
            }

            return targetDict;
        }

        [Fact]
        public void RouteOrderToTargetDict_StopOrder_WithStopPrefix_ReturnsStopDict()
        {
            // Arrange
            string classification = "stop";
            string orderName = "Stop_MOMO_001";

            // Act
            var result = RouteOrderToTargetDict(classification, orderName, out string key, out string dictName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("MOMO_001", key);
            Assert.Equal("stopOrders", dictName);
            Assert.Same(stopOrders, result); // Reference equality
        }

        [Fact]
        public void RouteOrderToTargetDict_StopOrder_WithSPrefix_ReturnsStopDict()
        {
            // Arrange
            string classification = "stop";
            string orderName = "S_MOMO_001";

            // Act
            var result = RouteOrderToTargetDict(classification, orderName, out string key, out string dictName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("MOMO_001", key);
            Assert.Equal("stopOrders", dictName);
            Assert.Same(stopOrders, result);
        }

        [Fact]
        public void RouteOrderToTargetDict_Target1Order_ReturnsTarget1Dict()
        {
            // Arrange
            string classification = "target1";
            string orderName = "T1_TREND_002";

            // Act
            var result = RouteOrderToTargetDict(classification, orderName, out string key, out string dictName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TREND_002", key);
            Assert.Equal("target1Orders", dictName);
            Assert.Same(target1Orders, result);
        }

        [Fact]
        public void RouteOrderToTargetDict_Target2Order_ReturnsTarget2Dict()
        {
            // Arrange
            string classification = "target2";
            string orderName = "T2_TREND_002";

            // Act
            var result = RouteOrderToTargetDict(classification, orderName, out string key, out string dictName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TREND_002", key);
            Assert.Equal("target2Orders", dictName);
            Assert.Same(target2Orders, result);
        }

        [Fact]
        public void RouteOrderToTargetDict_Target3Order_ReturnsTarget3Dict()
        {
            // Arrange
            string classification = "target3";
            string orderName = "T3_TREND_002";

            // Act
            var result = RouteOrderToTargetDict(classification, orderName, out string key, out string dictName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TREND_002", key);
            Assert.Equal("target3Orders", dictName);
            Assert.Same(target3Orders, result);
        }

        [Fact]
        public void RouteOrderToTargetDict_Target4Order_ReturnsTarget4Dict()
        {
            // Arrange
            string classification = "target4";
            string orderName = "T4_TREND_002";

            // Act
            var result = RouteOrderToTargetDict(classification, orderName, out string key, out string dictName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TREND_002", key);
            Assert.Equal("target4Orders", dictName);
            Assert.Same(target4Orders, result);
        }

        [Fact]
        public void RouteOrderToTargetDict_Target5Order_ReturnsTarget5Dict()
        {
            // Arrange
            string classification = "target5";
            string orderName = "T5_TREND_002";

            // Act
            var result = RouteOrderToTargetDict(classification, orderName, out string key, out string dictName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TREND_002", key);
            Assert.Equal("target5Orders", dictName);
            Assert.Same(target5Orders, result);
        }

        [Fact]
        public void RouteOrderToTargetDict_EntryOrder_ReturnsEntryDict()
        {
            // Arrange
            string classification = "entry";
            string orderName = "Fleet_MOMO_001";

            // Act
            var result = RouteOrderToTargetDict(classification, orderName, out string key, out string dictName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Fleet_MOMO_001", key); // Entry orders use full name
            Assert.Equal("entryOrders", dictName);
            Assert.Same(entryOrders, result);
        }

        [Fact]
        public void RouteOrderToTargetDict_InvalidClassification_ReturnsNull()
        {
            // Arrange
            string classification = "invalid";
            string orderName = "Unknown_Order";

            // Act
            var result = RouteOrderToTargetDict(classification, orderName, out string key, out string dictName);

            // Assert
            Assert.Null(result);
            Assert.Null(key);
            Assert.Null(dictName);
        }
    }
}

// Made with Bob
