using Xunit;

namespace V12_Performance.Tests.Core
{
    public class W7_147_GetOcoOrderFleetTypeTests
    {
        // Standalone mirror of GetOcoOrderFleetType (pure logic, no NinjaTrader deps)
        private enum OcoFleetOrderType { Stop, Target, Unknown }

        private OcoFleetOrderType GetOcoOrderFleetType(string ocoName)
        {
            if (ocoName.StartsWith("Stop_"))
                return OcoFleetOrderType.Stop;
            if (ocoName.StartsWith("T") && ocoName.Length > 2 && ocoName[2] == '_')
                return OcoFleetOrderType.Target;
            return OcoFleetOrderType.Unknown;
        }

        [Fact]
        public void GetOcoOrderFleetType_ReturnsStop_ForStopBes()
        {
            Assert.Equal(OcoFleetOrderType.Stop, GetOcoOrderFleetType("Stop_BES"));
        }

        [Fact]
        public void GetOcoOrderFleetType_ReturnsStop_ForExactStopPrefix()
        {
            Assert.Equal(OcoFleetOrderType.Stop, GetOcoOrderFleetType("Stop_"));
        }

        [Fact]
        public void GetOcoOrderFleetType_ReturnsTarget_ForT2Bes()
        {
            Assert.Equal(OcoFleetOrderType.Target, GetOcoOrderFleetType("T2_BES"));
        }

        [Fact]
        public void GetOcoOrderFleetType_ReturnsTarget_ForT9X()
        {
            Assert.Equal(OcoFleetOrderType.Target, GetOcoOrderFleetType("T9_X"));
        }

        [Fact]
        public void GetOcoOrderFleetType_ReturnsUnknown_ForLimitBes()
        {
            Assert.Equal(OcoFleetOrderType.Unknown, GetOcoOrderFleetType("LIMIT_BES"));
        }

        [Fact]
        public void GetOcoOrderFleetType_ReturnsUnknown_ForEmptyString()
        {
            Assert.Equal(OcoFleetOrderType.Unknown, GetOcoOrderFleetType(""));
        }

        [Fact]
        public void GetOcoOrderFleetType_ReturnsUnknown_ForTXShortName()
        {
            // Length=2, not >2 -- should return Unknown
            Assert.Equal(OcoFleetOrderType.Unknown, GetOcoOrderFleetType("TX"));
        }

        [Fact]
        public void GetOcoOrderFleetType_ReturnsUnknown_WhenThirdCharIsNotUnderscore()
        {
            // Starts with T, length=3, but [2] != '_'
            Assert.Equal(OcoFleetOrderType.Unknown, GetOcoOrderFleetType("T2X"));
        }
    }

    public class W7_147_DispatchOcoFleetOrderTests
    {
        private enum OcoFleetOrderType { Stop, Target, Unknown }

        private string lastCall = "none";

        private void HandleFleetStopFill() => lastCall = "stop";
        private void HandleFleetTargetFill() => lastCall = "target";

        private void DispatchOcoFleetOrder(OcoFleetOrderType orderType, string ocoName)
        {
            if (orderType == OcoFleetOrderType.Stop)
                HandleFleetStopFill();
            else if (orderType == OcoFleetOrderType.Target)
                HandleFleetTargetFill();
            else
                lastCall = "unknown";
        }

        [Fact]
        public void DispatchOcoFleetOrder_CallsStopFill_ForStopType()
        {
            DispatchOcoFleetOrder(OcoFleetOrderType.Stop, "Stop_BES");
            Assert.Equal("stop", lastCall);
        }

        [Fact]
        public void DispatchOcoFleetOrder_CallsTargetFill_ForTargetType()
        {
            DispatchOcoFleetOrder(OcoFleetOrderType.Target, "T2_BES");
            Assert.Equal("target", lastCall);
        }

        [Fact]
        public void DispatchOcoFleetOrder_LogsUnknown_ForUnknownType()
        {
            DispatchOcoFleetOrder(OcoFleetOrderType.Unknown, "LIMIT_BES");
            Assert.Equal("unknown", lastCall);
        }
    }
}
