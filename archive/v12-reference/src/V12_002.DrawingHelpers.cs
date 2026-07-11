// Build 971: V12_002 DrawingHelpers -- Drawing, Helpers regions
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class V12_002 : Strategy
    {
        #region Drawing - Box Instead of Rays

        private void DrawORBox()
        {
            if (sessionHigh == double.MinValue || sessionLow == double.MaxValue)
                return;
            if (orStartDateTime == DateTime.MinValue || orEndDateTime == DateTime.MinValue)
                return;

            try
            {
                DateTime orStartInZone = ConvertToSelectedTimeZone(orStartDateTime);
                DateTime sessionEndInZone = GetSessionEndInZone(orStartInZone);
                TimeZoneInfo targetZone = ResolveTimeZone(SelectedTimeZone);
                DateTime boxEndTime = TimeZoneInfo.ConvertTime(sessionEndInZone, targetZone, TimeZoneInfo.Local);
                DrawBoxWithOptionalMidLine(boxEndTime);
            }
            catch (Exception ex)
            {
                Print("ERROR DrawORBox: " + ex.Message);
            }
        }

        private DateTime GetSessionEndInZone(DateTime orStartInZone)
        {
            TimeSpan sessionStartTime = SessionStart.TimeOfDay;
            TimeSpan sessionEndTime = SessionEnd.TimeOfDay;
            bool sessionCrossesMidnight = sessionEndTime < sessionStartTime;
            DateTime baseEnd = new DateTime(
                orStartInZone.Year,
                orStartInZone.Month,
                orStartInZone.Day,
                sessionEndTime.Hours,
                sessionEndTime.Minutes,
                sessionEndTime.Seconds,
                DateTimeKind.Unspecified
            );
            return sessionCrossesMidnight ? baseEnd.AddDays(1) : baseEnd;
        }

        private static TimeZoneInfo ResolveTimeZone(string selectedTimeZone)
        {
            switch (selectedTimeZone)
            {
                case "Eastern":
                    return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                case "Central":
                    return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                case "Mountain":
                    return TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                case "Pacific":
                    return TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                case "UTC":
                    return TimeZoneInfo.Utc;
                default:
                    return TimeZoneInfo.Local;
            }
        }

        private void DrawBoxWithOptionalMidLine(DateTime boxEndTime)
        {
            Draw.Rectangle(
                this,
                "ORBox",
                false,
                orStartDateTime,
                sessionHigh,
                boxEndTime,
                sessionLow,
                Brushes.DodgerBlue,
                Brushes.DodgerBlue,
                BoxOpacity
            );

            if (ShowMidLine)
            {
                Draw.Line(
                    this,
                    "ORMid",
                    false,
                    orStartDateTime,
                    sessionMid,
                    boxEndTime,
                    sessionMid,
                    Brushes.Yellow,
                    DashStyleHelper.Dash,
                    1
                );
            }
        }

        private void ResetOR()
        {
            sessionHigh = double.MinValue;
            sessionLow = double.MaxValue;
            sessionMid = 0;
            sessionRange = 0;
            isInORWindow = false;
            orComplete = false;
            retestFiredThisSession = false; // V12.1101E [B-2]: Reset RETEST latch at session start
            orStartDateTime = DateTime.MinValue;
            orEndDateTime = DateTime.MinValue;
            sessionStartDateTime = DateTime.MinValue;
            orStartBarIndex = 0;
            orEndBarIndex = 0;

            RemoveDrawObjects();
        }

        #endregion

        #region Helpers

        private DateTime ConvertToSelectedTimeZone(DateTime localTime)
        {
            try
            {
                TimeZoneInfo targetZone;
                switch (SelectedTimeZone)
                {
                    case "Eastern":
                        targetZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                        break;
                    case "Central":
                        targetZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                        break;
                    case "Mountain":
                        targetZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                        break;
                    case "Pacific":
                        targetZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                        break;
                    case "UTC":
                        targetZone = TimeZoneInfo.Utc;
                        break;
                    default:
                        return localTime;
                }

                return TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, targetZone);
            }
            catch (Exception ex)
            {
                Print("ERROR ConvertToSelectedTimeZone: " + ex.Message);
                return localTime;
            }
        }

        private void RemoveDrawObjects()
        {
            RemoveDrawObject("ORBox");
            RemoveDrawObject("ORMid");
        }

        // V12.1101Q [FIX-DRAW]: Ultimate fallback helper using 'object' to bypass namespace issues.
        private object GetDrawObject(string tag)
        {
            if (DrawObjects == null)
                return null;
            foreach (var o in DrawObjects)
            {
                if (o.Tag == tag)
                    return o;
            }
            return null;
        }

        // Build 940 [FIX-1]: Stable OCO hash -- FNV-1a non-crypto hash, consistent across NT8 restarts and platforms.
        // Used for OCO Group IDs to satisfy SonarCloud security hotspots while maintaining stability.
        private string GetStableHash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "00000000";
            uint hash = 2166136261;
            foreach (char c in input)
            {
                hash = (hash ^ c) * 16777619;
            }
            return hash.ToString("X8").ToUpperInvariant();
        }

        #endregion
    }
}
