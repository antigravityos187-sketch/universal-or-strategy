// ParseAccountPositionsTests.cs
// xUnit tests for the ParseAccountPositions logic extracted from DeserializeSnapshot
// EPIC-W7-118 T1 -- account positions JSON block parsing
// Validates: returns empty dict when key absent; parses valid key-value pairs correctly

using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace V12_Performance.Tests.Core
{
    public class ParseAccountPositionsTests
    {
        // Pure-logic mirror of the extracted private helper ParseAccountPositions.
        // Parses the "AccountPositions" JSON object block from a raw JSON string.
        private static Dictionary<string, int> ParseAccountPositions(string json)
        {
            var result = new Dictionary<string, int>();
            int accountPosStart = json.IndexOf(
                "\"AccountPositions\"",
                System.StringComparison.Ordinal
            );
            if (accountPosStart >= 0)
            {
                int objStart = json.IndexOf('{', accountPosStart);
                int objEnd = json.IndexOf('}', objStart);
                if (objStart >= 0 && objEnd > objStart)
                {
                    string accountsBlock = json.Substring(objStart + 1, objEnd - objStart - 1);
                    string[] pairs = accountsBlock.Split(
                        new[] { ',' },
                        System.StringSplitOptions.RemoveEmptyEntries
                    );
                    foreach (string pair in pairs)
                    {
                        int colonIdx = pair.IndexOf(':');
                        if (colonIdx > 0)
                        {
                            string key = pair.Substring(0, colonIdx).Trim().Trim('"');
                            string valStr = pair.Substring(colonIdx + 1).Trim();
                            if (
                                int.TryParse(
                                    valStr,
                                    NumberStyles.Integer,
                                    CultureInfo.InvariantCulture,
                                    out int val
                                )
                            )
                            {
                                result[key] = val;
                            }
                        }
                    }
                }
            }

            return result;
        }

        [Fact]
        public void ParseAccountPositions_ReturnsEmpty_WhenNoAccountPositionsKey()
        {
            // JSON with no "AccountPositions" key at all
            string json = "{\"SnapshotTicks\": 1234, \"PositionSize\": 2}";
            Dictionary<string, int> result = ParseAccountPositions(json);
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void ParseAccountPositions_ParsesValidJson_ReturnsPositions()
        {
            // JSON containing an "AccountPositions" object with two accounts
            string json =
                "{\"SnapshotTicks\": 1, \"AccountPositions\": {\"Sim101\": 3, \"Sim102\": -1}}";
            Dictionary<string, int> result = ParseAccountPositions(json);
            Assert.Equal(2, result.Count);
            Assert.Equal(3, result["Sim101"]);
            Assert.Equal(-1, result["Sim102"]);
        }
    }
}
