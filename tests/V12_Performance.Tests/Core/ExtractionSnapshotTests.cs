using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace V12_Performance.Tests.Core
{
    /// <summary>
    /// Example snapshot tests using Verify framework (Jane Street expect test pattern).
    /// Demonstrates before/after state capture for epic extractions.
    /// </summary>
    [UsesVerify]
    public class ExtractionSnapshotTests
    {
        /// <summary>
        /// Example: Capture extraction state before refactoring.
        /// In real epic workflow, this would capture:
        /// - Method name, CYC, LOC
        /// - Caller list
        /// - Complexity metrics
        /// </summary>
        [Fact]
        public Task CaptureBeforeState_Example()
        {
            var state = new ExtractionState
            {
                EpicId = "EPIC-CCN-1",
                MethodName = "HydrateFSMsFromWorkingOrders",
                FilePath = "src/V12_002.cs",
                CYC = 71,
                LOC = 450,
                Callers = new[] { "OnStateChange", "OnExecutionUpdate", "AdoptFleetOrders" },
                ComplexityScore = 100.0,
            };

            return Verify(state).UseDirectory("snapshots").UseFileName("EPIC-CCN-1_before");
        }

        /// <summary>
        /// Example: Capture extraction state after refactoring.
        /// Compare .verified.txt diff to see what changed.
        /// </summary>
        [Fact]
        public Task CaptureAfterState_Example()
        {
            var state = new ExtractionState
            {
                EpicId = "EPIC-CCN-1",
                MethodName = "HydrateFSMsFromWorkingOrders",
                FilePath = "src/V12_002.cs",
                CYC = 8, // Reduced from 71
                LOC = 120, // Reduced from 450
                Callers = new[] { "OnStateChange", "OnExecutionUpdate", "AdoptFleetOrders" },
                ComplexityScore = 15.0, // Reduced from 100.0
                ExtractedMethods = new[]
                {
                    "ValidateOrderState",
                    "InitializeFSMFromOrder",
                    "ConfigureFSMBehavior",
                    "RegisterFSMCallbacks",
                },
            };

            return Verify(state).UseDirectory("snapshots").UseFileName("EPIC-CCN-1_after");
        }

        /// <summary>
        /// Example: Snapshot test with scrubbing (remove non-deterministic fields).
        /// </summary>
        [Fact]
        public Task CaptureWithScrubbing_Example()
        {
            var state = new ExtractionState
            {
                EpicId = "EPIC-CCN-2",
                MethodName = "ProcessIpcCommands",
                FilePath = "src/V12_002.UI.IPC.cs",
                CYC = 14,
                LOC = 180,
                Callers = new[] { "HandleIncomingIpcLine_TriggerProcessing" },
                Timestamp = System.DateTime.UtcNow, // Non-deterministic
            };

            var settings = new VerifySettings();
            settings.ScrubMembers("Timestamp"); // Remove timestamp from snapshot

            return Verify(state, settings).UseDirectory("snapshots").UseFileName("EPIC-CCN-2_final");
        }
    }

    /// <summary>
    /// Data model for extraction state snapshots.
    /// </summary>
    public class ExtractionState
    {
        public string EpicId { get; set; }
        public string MethodName { get; set; }
        public string FilePath { get; set; }
        public int CYC { get; set; }
        public int LOC { get; set; }
        public string[] Callers { get; set; }
        public double ComplexityScore { get; set; }
        public string[] ExtractedMethods { get; set; }
        public System.DateTime? Timestamp { get; set; }
    }
}

/*
 * USAGE IN EPIC WORKFLOW:
 *
 * Phase 5 (Before Extraction):
 * 1. Run: dotnet test --filter "FullyQualifiedName~CaptureBeforeState"
 * 2. Commit .verified.txt file
 *
 * Phase 5 (After Extraction):
 * 1. Run: dotnet test --filter "FullyQualifiedName~CaptureAfterState"
 * 2. Review diff in .verified.txt
 * 3. Verify CYC reduction, LOC reduction, extracted methods
 * 4. Commit updated .verified.txt
 *
 * JANE STREET ALIGNMENT:
 * - Expect tests capture complex output (state snapshots)
 * - Regression detection via git diff
 * - Scrubbing removes non-deterministic fields
 * - Snapshots live in tests/ directory (co-located with code)
 */

// Made with Bob
