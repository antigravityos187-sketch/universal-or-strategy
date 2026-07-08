// EPIC-W7-160 xUnit Tests — Lane FL-27 (S3_UI_IO cluster)
// Wave 7, Phase 5 — TrySendToClient + CleanupStaleClient extraction verification
// KB: xUnit only, [Fact], Assert.Equal(), ASCII-only, UTF-8 no BOM
using System.Text;
using NinjaTrader.NinjaScript.Strategies;
using Xunit;

namespace W7_160.Tests
{
    public class IpcSendLogicTests
    {
        // ---------------------------------------------------------------
        // ShouldLogBroadcast
        // ---------------------------------------------------------------

        [Fact]
        public void ShouldLogBroadcast_WhenResponseContainsSyncTargetState_ReturnsTrue()
        {
            bool result = IpcSendLogic.ShouldLogBroadcast("SYNC_TARGET_STATE|some_data");
            Assert.Equal(true, result);
        }

        [Fact]
        public void ShouldLogBroadcast_WhenResponseDoesNotContainSyncTargetState_ReturnsFalse()
        {
            bool result = IpcSendLogic.ShouldLogBroadcast("CONFIG|FLEET|COUNT:2");
            Assert.Equal(false, result);
        }

        [Fact]
        public void ShouldLogBroadcast_WhenResponseIsNull_ReturnsFalse()
        {
            bool result = IpcSendLogic.ShouldLogBroadcast(null);
            Assert.Equal(false, result);
        }

        [Fact]
        public void ShouldLogBroadcast_WhenResponseIsExactKeyword_ReturnsTrue()
        {
            bool result = IpcSendLogic.ShouldLogBroadcast("SYNC_TARGET_STATE");
            Assert.Equal(true, result);
        }

        // ---------------------------------------------------------------
        // EncodeResponseBytes — wire framing: response + "\n" in UTF-8
        // ---------------------------------------------------------------

        [Fact]
        public void EncodeResponseBytes_AppendsNewline()
        {
            byte[] bytes = IpcSendLogic.EncodeResponseBytes("OK");
            byte[] expected = Encoding.UTF8.GetBytes("OK\n");
            Assert.Equal(expected.Length, bytes.Length);
        }

        [Fact]
        public void EncodeResponseBytes_EncodesContentCorrectly()
        {
            byte[] bytes = IpcSendLogic.EncodeResponseBytes("HELLO");
            string decoded = Encoding.UTF8.GetString(bytes);
            Assert.Equal("HELLO\n", decoded);
        }

        [Fact]
        public void EncodeResponseBytes_EmptyString_ReturnsSingleNewlineByte()
        {
            byte[] bytes = IpcSendLogic.EncodeResponseBytes(string.Empty);
            Assert.Equal(1, bytes.Length);
            Assert.Equal((byte)'\n', bytes[0]);
        }

        // ---------------------------------------------------------------
        // IsSessionWritable — mirrors TrySendToClient connected-and-writable branch
        // ---------------------------------------------------------------

        [Fact]
        public void IsSessionWritable_WhenConnectedAndCanWrite_ReturnsTrue()
        {
            bool result = IpcSendLogic.IsSessionWritable(clientConnected: true, streamCanWrite: true);
            Assert.Equal(true, result);
        }

        [Fact]
        public void IsSessionWritable_WhenDisconnected_ReturnsFalse()
        {
            bool result = IpcSendLogic.IsSessionWritable(clientConnected: false, streamCanWrite: true);
            Assert.Equal(false, result);
        }

        [Fact]
        public void IsSessionWritable_WhenStreamNotWritable_ReturnsFalse()
        {
            bool result = IpcSendLogic.IsSessionWritable(clientConnected: true, streamCanWrite: false);
            Assert.Equal(false, result);
        }

        [Fact]
        public void IsSessionWritable_WhenBothFalse_ReturnsFalse()
        {
            bool result = IpcSendLogic.IsSessionWritable(clientConnected: false, streamCanWrite: false);
            Assert.Equal(false, result);
        }
    }
}
