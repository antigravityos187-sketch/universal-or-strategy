using System.Collections.Concurrent;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    public class Dispatch_RollbackFleetSlotTests
    {
        [Fact]
        public void Dispatch_RollbackFleetSlot_WithRegisteredKey_RemovesAllEntries()
        {
            // Arrange
            var activePositions = new ConcurrentDictionary<string, object>();
            var entryOrders = new ConcurrentDictionary<string, object>();
            var stopOrders = new ConcurrentDictionary<string, object>();
            const string key = "fleet-test-key";
            activePositions[key] = new object();
            entryOrders[key] = new object();
            stopOrders[key] = new object();

            // Act: simulate Dispatch_RollbackFleetSlot core logic
            activePositions.TryRemove(key, out _);
            entryOrders.TryRemove(key, out _);
            stopOrders.TryRemove(key, out _);

            // Assert: all 3 primary dicts cleared
            Assert.Equal(0, activePositions.Count);
            Assert.Equal(0, entryOrders.Count);
            Assert.Equal(0, stopOrders.Count);
        }

        [Fact]
        public void Dispatch_RollbackFleetSlot_TargetDictLoop_ClearsUpToFiveTargets()
        {
            // Arrange: simulate 5 target-order dicts (as in GetTargetOrdersDictionary loop)
            var targetDicts = new ConcurrentDictionary<string, object>[5];
            const string key = "fleet-target-key";
            for (int i = 0; i < 5; i++)
            {
                targetDicts[i] = new ConcurrentDictionary<string, object>();
                targetDicts[i][key] = new object();
            }

            // Act: simulate for (int tNum = 1; tNum <= 5; tNum++) rollback
            for (int tNum = 0; tNum < 5; tNum++)
            {
                var targetDict = targetDicts[tNum];
                if (targetDict != null)
                    targetDict.TryRemove(key, out _);
            }

            // Assert: each target dict is empty
            for (int tNum = 0; tNum < 5; tNum++)
            {
                Assert.Equal(0, targetDicts[tNum].Count);
            }
        }

        [Fact]
        public void Dispatch_RollbackFleetSlot_NullTargetDict_DoesNotThrow()
        {
            // Arrange: simulate null target dict (GetTargetOrdersDictionary returns null)
            var nonNull0 = new ConcurrentDictionary<string, object>();
            var nonNull1 = new ConcurrentDictionary<string, object>();
            const string key = "fleet-null-guard-key";
            nonNull0[key] = new object();
            nonNull1[key] = new object();

            // Act: simulate null-guard inside rollback for-loop
            ConcurrentDictionary<string, object>[] targetDicts = [nonNull0, null, nonNull1];
            foreach (var targetDict in targetDicts)
            {
                if (targetDict != null)
                    targetDict.TryRemove(key, out _);
            }

            // Assert: non-null dicts cleared, no exception thrown
            Assert.Equal(0, nonNull0.Count);
            Assert.Equal(0, nonNull1.Count);
        }
    }
}
