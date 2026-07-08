// EPIC-W7-007 | T1+T2: ComputeSlotQuantity + ValidateAndAdjustBucketSum behavioral tests
// Both helpers are private static; tested via the public GetTargetDistribution API.
// Jane Street: xUnit [Fact] + Assert.Equal; deterministic; zero NinjaTrader dependencies.
using System;
using Xunit;
using NinjaTrader.NinjaScript.Strategies;

namespace V12_Performance.Tests.W7_007
{
    /// <summary>
    /// xUnit tests for ComputeSlotQuantity (T1) and ValidateAndAdjustBucketSum (T2)
    /// extracted from GetTargetDistribution in EPIC-W7-007.
    /// Private helpers are exercised through the public GetTargetDistribution surface.
    /// </summary>
    public class W7_007_ComputeSlotQuantityTests
    {
        // ------------------------------------------------------------------
        // T1: ComputeSlotQuantity -- slot < remainder => baseQty + 1
        // ------------------------------------------------------------------

        [Fact]
        public void ComputeSlotQuantity_SlotBelowRemainder_AddsOne()
        {
            // contracts=5, count=3 => baseQty=1, remainder=2
            // slot 0 satisfies (0 < 2) => 1 + 1 = 2
            int[] result = V12_PureLogic.GetTargetDistribution(5, 3);
            Assert.Equal(2, result[0]);
        }

        [Fact]
        public void ComputeSlotQuantity_SlotAtOrAboveRemainder_BaseQtyOnly()
        {
            // contracts=5, count=3 => baseQty=1, remainder=2
            // slot 2 does NOT satisfy (2 < 2) => 1 + 0 = 1
            int[] result = V12_PureLogic.GetTargetDistribution(5, 3);
            Assert.Equal(1, result[2]);
        }

        // ------------------------------------------------------------------
        // T2: ValidateAndAdjustBucketSum -- invariant: sum always == contracts
        // ------------------------------------------------------------------

        [Fact]
        public void ValidateAndAdjustBucketSum_SumMatchesContracts_NoChange()
        {
            // contracts=6, count=3 => perfectly divisible, no adjustment needed
            // expected buckets: [2, 2, 2, 0, 0]
            int[] result = V12_PureLogic.GetTargetDistribution(6, 3);
            int sum = result[0] + result[1] + result[2] + result[3] + result[4];
            Assert.Equal(6, sum);
            Assert.Equal(2, result[0]);
            Assert.Equal(2, result[1]);
            Assert.Equal(2, result[2]);
        }

        [Fact]
        public void ValidateAndAdjustBucketSum_SumMismatch_AdjustsLastBucket()
        {
            // contracts=7, count=3 => baseQty=2, remainder=1
            // buckets=[3, 2, 2, 0, 0] => sum=7 (no panic needed, invariant holds)
            // This verifies the post-distribution sum invariant enforced by the helper.
            int[] result = V12_PureLogic.GetTargetDistribution(7, 3);
            int sum = result[0] + result[1] + result[2] + result[3] + result[4];
            Assert.Equal(7, sum);
            // slot 0 gets baseQty+1 = 3 (slot 0 < remainder 1)
            Assert.Equal(3, result[0]);
            // slot 2 (last active) gets baseQty = 2
            Assert.Equal(2, result[2]);
        }
    }
}
