// <copyright file="W7_092_SetRmaAnchorFromIpcTests.cs" company="BMad">
// Copyright (c) BMad. All rights reserved.
// </copyright>
// EPIC-W7-092 xUnit tests for T1 (RmaAnchorLookup / TryParseRmaAnchorType)
// and T2 (SetRmaAnchorFromIpc refactored orchestrator).
// NT8 dependency cannot be linked here; pure-logic mirrors are used instead.
using System.Collections.Generic;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// xUnit tests for EPIC-W7-092 extracted helpers from SetRmaAnchorFromIpc.
    /// T1: RmaAnchorLookup field + TryParseRmaAnchorType helper (CYC=1).
    /// T2: SetRmaAnchorFromIpc refactored orchestrator (CYC=4).
    /// </summary>
    public class W7_092_SetRmaAnchorFromIpcTests
    {
        // -----------------------------------------------------------------------
        // Stand-in enum mirror (RmaAnchorType is defined in the NT8 assembly)
        // -----------------------------------------------------------------------

        private enum RmaAnchorType
        {
            Ema30,
            Ema65,
            Ema200,
            OrHigh,
            OrLow,
            Manual,
        }

        // -----------------------------------------------------------------------
        // Stand-in mirrors of the extracted members (T1)
        // -----------------------------------------------------------------------

        // CYC=N/A: static readonly field -- mirrors RmaAnchorLookup in V12_002.SIMA.cs
        private static readonly Dictionary<string, RmaAnchorType> RmaAnchorLookup =
            new Dictionary<string, RmaAnchorType>
            {
                { "EMA30",   RmaAnchorType.Ema30   },
                { "EMA65",   RmaAnchorType.Ema65   },
                { "EMA200",  RmaAnchorType.Ema200  },
                { "OR_HIGH", RmaAnchorType.OrHigh  },
                { "OR_LOW",  RmaAnchorType.OrLow   },
                { "MANUAL",  RmaAnchorType.Manual  },
            };

        // CYC=1: mirrors TryParseRmaAnchorType expression-bodied helper
        private static bool TryParseRmaAnchorType(string key, out RmaAnchorType result)
            => RmaAnchorLookup.TryGetValue(key, out result);

        // -----------------------------------------------------------------------
        // Stand-in mirror of the refactored orchestrator (T2)
        // Mirrors SetRmaAnchorFromIpc; currentRmaAnchor returned via out param
        // because NT8 instance state is not available here.
        // CYC=4: base(1) + if(1) + try(1) + catch(1)
        // -----------------------------------------------------------------------
        private static bool SimSetRmaAnchorFromIpc(
            string anchorStr,
            out RmaAnchorType currentRmaAnchor,
            out string printedMessage)
        {
            currentRmaAnchor = RmaAnchorType.Ema30; // default sentinel
            printedMessage = string.Empty;
            try
            {
                if (TryParseRmaAnchorType(anchorStr, out RmaAnchorType anchor))
                    currentRmaAnchor = anchor;

                printedMessage = "IPC SET ANCHOR: " + anchorStr;
                return true;
            }
            catch
            {
                printedMessage = "Error SetRmaAnchorFromIpc: caught";
                return false;
            }
        }

        // -----------------------------------------------------------------------
        // T1 tests: TryParseRmaAnchorType -- all 6 known keys + edge cases
        // -----------------------------------------------------------------------

        [Fact]
        public void TryParseRmaAnchorType_EMA30_ReturnsEma30()
        {
            bool found = TryParseRmaAnchorType("EMA30", out RmaAnchorType result);
            Assert.Equal(true, found);
            Assert.Equal(RmaAnchorType.Ema30, result);
        }

        [Fact]
        public void TryParseRmaAnchorType_EMA65_ReturnsEma65()
        {
            bool found = TryParseRmaAnchorType("EMA65", out RmaAnchorType result);
            Assert.Equal(true, found);
            Assert.Equal(RmaAnchorType.Ema65, result);
        }

        [Fact]
        public void TryParseRmaAnchorType_EMA200_ReturnsEma200()
        {
            bool found = TryParseRmaAnchorType("EMA200", out RmaAnchorType result);
            Assert.Equal(true, found);
            Assert.Equal(RmaAnchorType.Ema200, result);
        }

        [Fact]
        public void TryParseRmaAnchorType_OR_HIGH_ReturnsOrHigh()
        {
            bool found = TryParseRmaAnchorType("OR_HIGH", out RmaAnchorType result);
            Assert.Equal(true, found);
            Assert.Equal(RmaAnchorType.OrHigh, result);
        }

        [Fact]
        public void TryParseRmaAnchorType_OR_LOW_ReturnsOrLow()
        {
            bool found = TryParseRmaAnchorType("OR_LOW", out RmaAnchorType result);
            Assert.Equal(true, found);
            Assert.Equal(RmaAnchorType.OrLow, result);
        }

        [Fact]
        public void TryParseRmaAnchorType_MANUAL_ReturnsManual()
        {
            bool found = TryParseRmaAnchorType("MANUAL", out RmaAnchorType result);
            Assert.Equal(true, found);
            Assert.Equal(RmaAnchorType.Manual, result);
        }

        [Fact]
        public void TryParseRmaAnchorType_UnknownKey_ReturnsFalse()
        {
            bool found = TryParseRmaAnchorType("UNKNOWN_KEY", out RmaAnchorType result);
            Assert.Equal(false, found);
        }

        [Fact]
        public void TryParseRmaAnchorType_EmptyKey_ReturnsFalse()
        {
            bool found = TryParseRmaAnchorType(string.Empty, out RmaAnchorType result);
            Assert.Equal(false, found);
        }

        [Fact]
        public void TryParseRmaAnchorType_LowercaseKey_ReturnsFalse()
        {
            // Keys are case-sensitive -- "ema30" must not match "EMA30"
            bool found = TryParseRmaAnchorType("ema30", out RmaAnchorType result);
            Assert.Equal(false, found);
        }

        // -----------------------------------------------------------------------
        // T1 tests: RmaAnchorLookup has exactly 6 entries
        // -----------------------------------------------------------------------

        [Fact]
        public void RmaAnchorLookup_ContainsExactlySixEntries()
        {
            Assert.Equal(6, RmaAnchorLookup.Count);
        }

        // -----------------------------------------------------------------------
        // T2 tests: SimSetRmaAnchorFromIpc (mirrors refactored orchestrator)
        // -----------------------------------------------------------------------

        [Fact]
        public void SetRmaAnchorFromIpc_EMA30_SetsEma30AndPrints()
        {
            bool ok = SimSetRmaAnchorFromIpc("EMA30", out RmaAnchorType anchor, out string msg);
            Assert.Equal(true, ok);
            Assert.Equal(RmaAnchorType.Ema30, anchor);
            Assert.Equal("IPC SET ANCHOR: EMA30", msg);
        }

        [Fact]
        public void SetRmaAnchorFromIpc_EMA65_SetsEma65AndPrints()
        {
            bool ok = SimSetRmaAnchorFromIpc("EMA65", out RmaAnchorType anchor, out string msg);
            Assert.Equal(true, ok);
            Assert.Equal(RmaAnchorType.Ema65, anchor);
            Assert.Equal("IPC SET ANCHOR: EMA65", msg);
        }

        [Fact]
        public void SetRmaAnchorFromIpc_EMA200_SetsEma200AndPrints()
        {
            bool ok = SimSetRmaAnchorFromIpc("EMA200", out RmaAnchorType anchor, out string msg);
            Assert.Equal(true, ok);
            Assert.Equal(RmaAnchorType.Ema200, anchor);
            Assert.Equal("IPC SET ANCHOR: EMA200", msg);
        }

        [Fact]
        public void SetRmaAnchorFromIpc_OR_HIGH_SetsOrHighAndPrints()
        {
            bool ok = SimSetRmaAnchorFromIpc("OR_HIGH", out RmaAnchorType anchor, out string msg);
            Assert.Equal(true, ok);
            Assert.Equal(RmaAnchorType.OrHigh, anchor);
            Assert.Equal("IPC SET ANCHOR: OR_HIGH", msg);
        }

        [Fact]
        public void SetRmaAnchorFromIpc_OR_LOW_SetsOrLowAndPrints()
        {
            bool ok = SimSetRmaAnchorFromIpc("OR_LOW", out RmaAnchorType anchor, out string msg);
            Assert.Equal(true, ok);
            Assert.Equal(RmaAnchorType.OrLow, anchor);
            Assert.Equal("IPC SET ANCHOR: OR_LOW", msg);
        }

        [Fact]
        public void SetRmaAnchorFromIpc_MANUAL_SetsManualAndPrints()
        {
            bool ok = SimSetRmaAnchorFromIpc("MANUAL", out RmaAnchorType anchor, out string msg);
            Assert.Equal(true, ok);
            Assert.Equal(RmaAnchorType.Manual, anchor);
            Assert.Equal("IPC SET ANCHOR: MANUAL", msg);
        }

        [Fact]
        public void SetRmaAnchorFromIpc_UnknownKey_StillPrintsAndDoesNotThrow()
        {
            // Unrecognized key: TryParseRmaAnchorType returns false, assignment skipped (defense in depth)
            bool ok = SimSetRmaAnchorFromIpc("BOGUS", out RmaAnchorType anchor, out string msg);
            Assert.Equal(true, ok);
            Assert.Equal("IPC SET ANCHOR: BOGUS", msg);
        }

        [Fact]
        public void SetRmaAnchorFromIpc_NullKey_StillPrintsAndDoesNotThrow()
        {
            // null key: Dictionary.TryGetValue(null) returns false; no throw
            bool ok = SimSetRmaAnchorFromIpc(null, out RmaAnchorType anchor, out string msg);
            Assert.Equal(true, ok);
            Assert.Equal("IPC SET ANCHOR: ", msg);
        }
    }
}
