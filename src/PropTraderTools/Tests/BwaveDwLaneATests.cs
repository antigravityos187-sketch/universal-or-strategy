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
        public void DetachPanel_DisarmsOwnLeaderAccount_DetachIlCallsDisarmPendingBe()
        {
            // Structural guard: verify Detach() IL contains a callvirt to DisarmPendingBe.
            // This ensures DW-C38-03 leader-scoped disarm at line 591 is compiled into Detach().
            // Full behavioral test requires live NT8 Account object (NT8-runtime dependency).
            var detach = typeof(TradeCopierPanel).GetMethod(
                "Detach",
                BindingFlags.Public | BindingFlags.Instance
            );
            Assert.NotNull(detach);

            var disarmMi = typeof(CopyEngine).GetMethod(
                "DisarmPendingBe",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            Assert.NotNull(disarmMi);

            var body = detach.GetMethodBody();
            Assert.NotNull(body);
            var il = body.GetILAsByteArray();
            Assert.NotNull(il);
            Assert.True(il.Length > 0, "Detach must have a non-empty IL body");

            bool found = false;
            var module = typeof(TradeCopierPanel).Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] != 0x6F) // callvirt opcode
                    continue;
                int token = il[i + 1] | (il[i + 2] << 8) | (il[i + 3] << 16) | (il[i + 4] << 24);
                try
                {
                    var resolved = module.ResolveMethod(token);
                    if (resolved != null && resolved.Name == "DisarmPendingBe")
                    {
                        found = true;
                        break;
                    }
                }
                catch { /* token not a valid method reference -- skip */ }
            }

            Assert.True(
                found,
                "Detach() must contain a callvirt to DisarmPendingBe (DW-C38-03: leader-scoped disarm at line 591)"
            );
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