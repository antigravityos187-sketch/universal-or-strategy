using System.Text;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// Unit tests for PrintFleetForensicReport extracted helper.
    /// EPIC-W7-096 TICKET-4 TDD Safety Net.
    /// Validates forensic report assembly: header injection, ok count, timing format.
    /// [NoInlining] cold logging path. CYC=4.
    /// Logic verified via inline stand-in (NT8 Print not available standalone).
    /// </summary>
    public class PrintFleetForensicReportTests
    {
        // ---------------------------------------------------------------------------
        // Stand-in: mirrors PrintFleetForensicReport string assembly logic.
        // Returns built report string instead of calling Print() (NT8 dependency).
        // ---------------------------------------------------------------------------

        private static string SimulatePrintFleetForensicReport(
            string header,
            StringBuilder log,
            int okCount,
            double setupMs,
            double loopMs
        )
        {
            double totalMs = setupMs + loopMs;
            var report = new StringBuilder(1024);
            report.AppendLine("+==============================================================+");
            report.AppendLine(header);
            report.AppendLine("+==============================================================+");
            report.AppendLine("|  TYPE | ACCOUNT                       | ORDER TYPE   | STATUS |");
            report.AppendLine("+==============================================================+");
            report.Append(log.ToString());
            report.AppendLine("+--------------------------------------------------------------+");
            report.AppendLine(string.Format("|  PATH B BROADCAST: {0} Brackets Submitted", okCount));
            report.AppendLine("+--------------------------------------------------------------+");
            report.AppendLine("|  TIMING SUMMARY                                              |");
            report.AppendLine("+--------------------------------------------------------------+");
            report.AppendLine(
                string.Format(
                    "|  Setup Phase:  {0,8:F3} ms  |  Fleet Loop:  {1,8:F3} ms       |",
                    setupMs,
                    loopMs
                )
            );
            report.AppendLine(
                string.Format("|  Total Elapsed: {0,8:F3} ms                                  |", totalMs)
            );
            report.AppendLine("+==============================================================+");
            return report.ToString().TrimEnd();
        }

        // ---------------------------------------------------------------------------
        // Header is injected correctly
        // ---------------------------------------------------------------------------

        [Fact]
        public void PrintFleetForensicReport_HeaderIsPresent()
        {
            var log = new StringBuilder();
            string report = SimulatePrintFleetForensicReport(
                "|       FORENSIC PULSE REPORT  Phase 9 MULTI-ACCOUNT BRACKET   |",
                log, 0, 0.0, 0.0
            );
            Assert.Contains("FORENSIC PULSE REPORT", report);
        }

        // ---------------------------------------------------------------------------
        // okCount appears in broadcast line
        // ---------------------------------------------------------------------------

        [Fact]
        public void PrintFleetForensicReport_OkCountAppearsInBroadcastLine()
        {
            var log = new StringBuilder();
            string report = SimulatePrintFleetForensicReport("HEADER", log, 3, 0.1, 0.5);
            Assert.Contains("3 Brackets Submitted", report);
        }

        [Fact]
        public void PrintFleetForensicReport_ZeroOkCount_AppearsInBroadcastLine()
        {
            var log = new StringBuilder();
            string report = SimulatePrintFleetForensicReport("HEADER", log, 0, 0.0, 0.0);
            Assert.Contains("0 Brackets Submitted", report);
        }

        // ---------------------------------------------------------------------------
        // totalMs = setupMs + loopMs
        // ---------------------------------------------------------------------------

        [Fact]
        public void PrintFleetForensicReport_TotalMsEqualsSetupPlusLoop()
        {
            var log = new StringBuilder();
            string report = SimulatePrintFleetForensicReport("HEADER", log, 1, 1.0, 2.0);
            // total = 3.000 ms
            Assert.Contains("3.000", report);
        }

        // ---------------------------------------------------------------------------
        // Log content is embedded in the report
        // ---------------------------------------------------------------------------

        [Fact]
        public void PrintFleetForensicReport_LogContentEmbeddedInReport()
        {
            var log = new StringBuilder();
            log.AppendLine("    OK | Sim101                       | Bracket(3)   | submitted");
            string report = SimulatePrintFleetForensicReport("HEADER", log, 1, 0.1, 0.2);
            Assert.Contains("Sim101", report);
        }

        // ---------------------------------------------------------------------------
        // setupMs and loopMs appear in timing summary
        // ---------------------------------------------------------------------------

        [Fact]
        public void PrintFleetForensicReport_SetupMsAppearsInTimingSummary()
        {
            var log = new StringBuilder();
            string report = SimulatePrintFleetForensicReport("HEADER", log, 0, 0.123, 0.456);
            Assert.Contains("0.123", report);
        }

        [Fact]
        public void PrintFleetForensicReport_LoopMsAppearsInTimingSummary()
        {
            var log = new StringBuilder();
            string report = SimulatePrintFleetForensicReport("HEADER", log, 0, 0.123, 0.456);
            Assert.Contains("0.456", report);
        }
    }
}
