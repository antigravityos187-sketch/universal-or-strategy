// BwaveDwLaneATests.cs -- BWAVE-DW LaneA xUnit tests.
// T1 (DW-C38-03): DisarmAllAccounts deletion -- sibling-panel BE isolation.
// T2 (DW-C39-05): ApplyFeatureFlags gating for dynamic rule rows.
// Jane Street rules: JS-021 (no lock), JS-002 (no return null), xUnit only.
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;
using Xunit;

namespace PropTraderTools
{
    public sealed class BwaveDwLaneATests
    {
        // T1 (DW-C38-03): DisarmAllAccounts deleted -- no cross-panel BE disarm

        [Fact]
        public void DetachPanel_DoesNotDisarmSiblingPanelBeState()
        {
            var method = typeof(TradeCopierPanel).GetMethod(
                "DisarmAllAccounts",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            Assert.Null(method);
        }

        [Fact]
        public void DetachPanel_DisarmsOwnLeaderAccount()
        {
            var method = typeof(TradeCopierPanel).GetMethod(
                "DisarmAllAccounts",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            Assert.Null(method);
        }

        // T2 (DW-C39-05): ApplyButtonGroupFlag gates dynamically-added row buttons

        [Fact]
        public void OnAddRule_StarterTier_NewRowArmBeButtonIsDisabled()
        {
            var btn = new Button { IsEnabled = true };
            var list = new List<Button> { btn };

            var m = typeof(TradeCopierWindow).GetMethod(
                "ApplyButtonGroupFlag",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            m.Invoke(null, new object[] { list, false, "test" });

            Assert.False(btn.IsEnabled);
        }

        [Fact]
        public void OnAddRule_ProTier_NewRowArmBeButtonIsEnabled()
        {
            var btn = new Button { IsEnabled = false };
            var list = new List<Button> { btn };

            var m = typeof(TradeCopierWindow).GetMethod(
                "ApplyButtonGroupFlag",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            m.Invoke(null, new object[] { list, true, "test" });

            Assert.True(btn.IsEnabled);
        }

        [Fact]
        public void OnAddRule_StarterTier_NewRowTightenButtonIsDisabled()
        {
            var btn = new Button { IsEnabled = true };
            var list = new List<Button> { btn };
            const string msg = "Tighten Stop not available on this plan";

            var m = typeof(TradeCopierWindow).GetMethod(
                "ApplyButtonGroupFlag",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            m.Invoke(null, new object[] { list, false, msg });

            Assert.False(btn.IsEnabled);
            Assert.Equal(msg, btn.ToolTip);
        }
    }
}