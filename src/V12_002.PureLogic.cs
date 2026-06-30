// V12.1102Z: Pure Logic Engine (Zero-Allocation / No-NinjaTrader Dependencies)
// Contains: Mathematical kernels extracted for Unit Testing
using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// V12_002.PureLogic contains the deterministic mathematical nuclei of the strategy.
    /// Extracted here to allow NUnit testing without the NinjaTrader Strategy runtime.
    /// </summary>
    public static class V12_PureLogic
    {
        /// <summary>
        /// IS-01: Iron Shield Target Distribution [V12.BEYOND-BUG]
        /// Deterministically divides contracts into a bucketed distribution.
        /// </summary>
        public static int[] GetTargetDistribution(int contracts, int targetCount)
        {
            if (contracts <= 0)
            {
                return new int[5];
            }

            // Clamp count to [1, 5]
            int count = Math.Max(1, Math.Min(5, targetCount));

            int[] buckets = new int[5];
            int baseQty = contracts / count;
            int remainder = contracts % count;

            // Distribute base and remainder (scalp preference: extras go to T1 first)
            for (int i = 0; i < count; i++)
            {
                buckets[i] = ComputeSlotQuantity(baseQty, i, remainder);
            }

            ValidateAndAdjustBucketSum(buckets, contracts, count);

            return buckets;
        }

        /// <summary>
        /// Computes the integer quantity for a single distribution slot.
        /// Slots below the remainder index receive one extra contract (scalp preference).
        /// </summary>
        private static int ComputeSlotQuantity(int baseQty, int slot, int remainder)
        {
            return baseQty + (slot < remainder ? 1 : 0);
        }

        /// <summary>
        /// Audits post-distribution bucket sum and applies panic-correction for integer-division edge cases.
        /// </summary>
        private static void ValidateAndAdjustBucketSum(int[] buckets, int contracts, int count)
        {
            int sum = buckets.Sum();
            if (sum != contracts)
            {
                buckets[count - 1] += (contracts - sum);
            }
        }

        /// <summary>
        /// V12.30: Core Sizing Logic Kernel
        /// Deterministically calculates quantity based on risk budget and stop points.
        /// </summary>
        public static int CalculatePositionSize(
            double stopPoints,
            double maxRiskAmount,
            double slippageCushionPoints,
            double pointValue,
            int minContracts,
            int maxContracts
        )
        {
            if (double.IsNaN(stopPoints) || stopPoints <= 0 || pointValue <= 0)
            {
                return Math.Max(1, minContracts);
            }

            double stopDollars = stopPoints * pointValue;
            double slippageCushionDollars = slippageCushionPoints * pointValue;
            double effectiveRisk = maxRiskAmount - slippageCushionDollars;

            if (effectiveRisk <= 0)
            {
                return Math.Max(1, minContracts);
            }

            int contracts;
            try
            {
                contracts = checked((int)Math.Floor(effectiveRisk / stopDollars));
            }
            catch (OverflowException)
            {
                contracts = maxContracts;
            }

            return Math.Max(minContracts, Math.Min(contracts, maxContracts));
        }

        /// <summary>
        /// V12.30: ATR Stop Distance Logic Kernel
        /// </summary>
        public static double CalculateATRStopDistance(double atr, double atrMultiplier, double minStop, double maxStop)
        {
            if (atr <= 0)
            {
                return minStop;
            }

            double rawStop = atr * atrMultiplier;
            double ceilingStop = Math.Ceiling(rawStop);
            return Math.Max(minStop, Math.Min(ceilingStop, maxStop));
        }

        // W7-041 T-1: Pure-logic kernel for AuditStopQuantityAndLog decision paths.
        // Returns (auditLine, sumLine) so xUnit tests assert on output without a Print() sink.
        // CYC=4 (base + null guard + mismatch branch + sum mismatch branch).

        /// <summary>
        /// W7-041 T-1: Stop quantity audit verdict kernel.
        /// auditLine is "[STOP_AUDIT] MISMATCH ..." when stopOrderPresent and stopQty != totalContracts,
        /// otherwise "[STOP_AUDIT] OK ...".
        /// sumLine is "[BRACKET_WARN] ..." when targetSum != totalContracts, otherwise empty.
        /// </summary>
        public static (string auditLine, string sumLine) AuditStopQuantityVerdict(
            string entryName,
            bool stopOrderPresent,
            int stopQty,
            int totalContracts,
            int nonRunnerLimitQty,
            int runnerQty
        )
        {
            string auditLine;
            if (stopOrderPresent && stopQty != totalContracts)
            {
                auditLine = string.Format(
                    "[STOP_AUDIT] MISMATCH {0}: StopQty={1} Total={2}",
                    entryName,
                    stopQty,
                    totalContracts
                );
            }
            else
            {
                auditLine = string.Format(
                    "[STOP_AUDIT] OK {0}: StopQty={1} NonRunnerLimits={2} RunnerQty={3}",
                    entryName,
                    totalContracts,
                    nonRunnerLimitQty,
                    runnerQty
                );
            }

            int targetSum = nonRunnerLimitQty + runnerQty;
            string sumLine =
                targetSum != totalContracts
                    ? string.Format(
                        "[BRACKET_WARN] Target sum mismatch for {0}: targets={1} totalContracts={2}. Distribution may have lost contracts.",
                        entryName,
                        targetSum,
                        totalContracts
                    )
                    : string.Empty;

            return (auditLine, sumLine);
        }

        // W7-041 T-2: Pure-logic kernel for BuildAndPrintBracketSummary decision paths.
        // CYC=5 (base + isFollowerSubmit + for-loop + targetQty<=0 skip + isRunnerSlot).

        /// <summary>
        /// W7-041 T-2: Bracket summary string builder kernel.
        /// targetQties[i] = qty for slot i+1; runnerSlots[i] = true when runner; targetPrices[i] = limit price.
        /// Returns (followerLine, bracketLine) where followerLine is empty when isFollowerSubmit=false.
        /// </summary>
        public static (string followerLine, string bracketLine) BuildBracketSummaryLines(
            string entryName,
            bool isFollowerSubmit,
            double target1Price,
            double validatedStopPrice,
            bool isRmaTrade,
            int[] targetQties,
            bool[] runnerSlots,
            double[] targetPrices
        )
        {
            string followerLine = string.Empty;
            if (isFollowerSubmit)
            {
                followerLine = string.Format(
                    "[938-BRACKET] Follower bracket submitted: {0} T1={1:F2} Stop={2:F2}",
                    entryName,
                    target1Price,
                    validatedStopPrice
                );
            }

            string tradeType = isRmaTrade ? "RMA" : "OR";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendFormat("{0} BRACKET V12.1101E: Stop@{1:F2}", tradeType, validatedStopPrice);
            for (int i = 0; i < 5; i++)
            {
                int qty = targetQties != null && i < targetQties.Length ? targetQties[i] : 0;
                if (qty <= 0)
                    continue;

                bool isRunner = runnerSlots != null && i < runnerSlots.Length && runnerSlots[i];
                if (isRunner)
                    sb.AppendFormat(" | T{0}:{1}@trail", i + 1, qty);
                else
                {
                    double price = targetPrices != null && i < targetPrices.Length ? targetPrices[i] : 0.0;
                    sb.AppendFormat(" | T{0}:{1}@{2:F2}", i + 1, qty, price);
                }
            }

            return (followerLine, sb.ToString());
        }
    }

    /// <summary>
    /// EPIC-W7-160: Pure-logic kernels for IPC send routing decisions.
    /// Extracted for xUnit testing without TCP/NetworkStream dependencies.
    /// </summary>
    public static class IpcSendLogic
    {
        /// <summary>
        /// Returns true when a response payload should trigger a diagnostic broadcast log.
        /// Mirrors the branch: if (response.Contains("SYNC_TARGET_STATE"))
        /// </summary>
        public static bool ShouldLogBroadcast(string response)
        {
            return response != null && response.Contains("SYNC_TARGET_STATE");
        }

        /// <summary>
        /// Encodes a response string to UTF-8 bytes with a trailing newline,
        /// matching the wire framing used in SendResponseToRemote.
        /// </summary>
        public static byte[] EncodeResponseBytes(string response)
        {
            return System.Text.Encoding.UTF8.GetBytes(response + "\n");
        }

        /// <summary>
        /// Returns true when the client state allows a TCP write.
        /// Mirrors the branch: if (session.Client.Connected and session.Stream.CanWrite)
        /// </summary>
        public static bool IsSessionWritable(bool clientConnected, bool streamCanWrite)
        {
            return clientConnected && streamCanWrite;
        }
    }
}
