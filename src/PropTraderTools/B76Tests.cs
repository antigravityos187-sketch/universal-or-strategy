// src/PropTraderTools/B76Tests.cs
// B76-LaneA: HOTFIX-B76-FLATTEN-GUARD-01 v2 + HOTFIX-B76-FLATTEN-RACE-01 + HOTFIX-B76-POSSTATE-DEDUP-01
//            + HOTFIX-B76-ATM-TPL-CLASSNAME
// 12 xUnit [Fact] tests: T_B76_01 through T_B76_12.
// JS-021: no lock. JS-001: no throw. JS-002: no return null. JS-033: no async void.
// xUnit only -- no NUnit, no MSTest. ASCII identifiers only.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Xunit;

namespace PropTraderTools
{
    public sealed class B76Tests
    {
        // ======================================================================
        // TICKET-B76-1: FlattenOneAccount in-flight guard + race guard
        // HOTFIX-B76-FLATTEN-GUARD-01 v2 + HOTFIX-B76-FLATTEN-RACE-01
        // ======================================================================

        // T_B76_01: FlattenOneAccount method exists as private instance method on CopyEngine.
        // Confirms the method was not accidentally removed or renamed.
        [Fact]
        public void T_B76_01_FlattenOneAccount_MethodExists_PrivateInstance()
        {
            var mi = typeof(CopyEngine).GetMethod(
                "FlattenOneAccount",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            Assert.NotNull(mi);
            Assert.Equal(typeof(void), mi.ReturnType);

            var ps = mi.GetParameters();
            Assert.Equal(2, ps.Length);
            Assert.Equal(typeof(NinjaTrader.Cbi.Account), ps[0].ParameterType);
            Assert.Equal(typeof(NinjaTrader.Cbi.Instrument), ps[1].ParameterType);
        }

        // T_B76_02: FlattenOneAccount compiled body contains the string literal for the in-flight guard.
        // Verifies HOTFIX-B76-FLATTEN-GUARD-01 v2 is compiled into the method.
        // IL assertion: scans ldstr tokens within the same assembly module (same-module ResolveString is stable).
        // Roslyn version dependency: valid for net8.0 target. Review on toolchain upgrade. See DW-C39-12.
        [Fact]
        public void T_B76_02_FlattenOneAccount_ContainsInFlightSkipString()
        {
            var mi = typeof(CopyEngine).GetMethod(
                "FlattenOneAccount",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            Assert.NotNull(mi);

            var body = mi.GetMethodBody();
            Assert.NotNull(body);
            var il = body.GetILAsByteArray();
            Assert.NotNull(il);
            Assert.True(il.Length > 0, "FlattenOneAccount must have a non-empty IL body");

            // IL assertion: scan for ldstr opcode anywhere in method body (not at a fixed offset). See DW-C39-12.
            bool found = false;
            var module = typeof(CopyEngine).Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] == 0x72) // ldstr
                {
                    int token =
                        il[i + 1] | (il[i + 2] << 8) | (il[i + 3] << 16) | (il[i + 4] << 24);
                    try
                    {
                        var s = module.ResolveString(token);
                        if (s != null && s.Contains("flat-guard: in-flight skip"))
                        {
                            found = true;
                            break;
                        }
                    }
                    catch
                    { /* token not a valid string reference -- skip */
                    }
                }
            }

            Assert.True(
                found,
                "FlattenOneAccount must contain string literal 'flat-guard: in-flight skip' (HOTFIX-B76-FLATTEN-GUARD-01 v2)"
            );
        }

        // T_B76_03: FlattenOneAccount compiled body contains the string literal for the race skip.
        // Verifies HOTFIX-B76-FLATTEN-RACE-01 is compiled into the method.
        // IL assertion: scans ldstr tokens within the same assembly module (same-module ResolveString is stable).
        // Roslyn version dependency: valid for net8.0 target. Review on toolchain upgrade. See DW-C39-12.
        [Fact]
        public void T_B76_03_FlattenOneAccount_ContainsRaceSkipString()
        {
            var mi = typeof(CopyEngine).GetMethod(
                "FlattenOneAccount",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            Assert.NotNull(mi);

            var body = mi.GetMethodBody();
            Assert.NotNull(body);
            var il = body.GetILAsByteArray();
            Assert.NotNull(il);

            // IL assertion: scan for ldstr opcode anywhere in method body (not at a fixed offset). See DW-C39-12.
            bool found = false;
            var module = typeof(CopyEngine).Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] == 0x72) // ldstr
                {
                    int token =
                        il[i + 1] | (il[i + 2] << 8) | (il[i + 3] << 16) | (il[i + 4] << 24);
                    try
                    {
                        var s = module.ResolveString(token);
                        if (s != null && s.Contains("flat-race skip"))
                        {
                            found = true;
                            break;
                        }
                    }
                    catch { }
                }
            }

            Assert.True(
                found,
                "FlattenOneAccount must contain string literal 'flat-race skip' (HOTFIX-B76-FLATTEN-RACE-01)"
            );
        }

        // T_B76_04: FlattenOneAccount IL contains at least 2 FindPosition call sites.
        // The first call is the initial position check; the second is the post-cancel re-read.
        // Proves the race guard (posAfterCancel) is compiled into the method.
        // IL assertion: uses name+declaring-type resolution via helper (stable). See DW-C39-12.
        // Assertion uses count >= 2 (not exact) to avoid fragility.
        [Fact]
        public void T_B76_04_FlattenOneAccount_HasAtLeastTwoFindPositionCallSites()
        {
            var mi = typeof(CopyEngine).GetMethod(
                "FlattenOneAccount",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            Assert.NotNull(mi);

            var body = mi.GetMethodBody();
            Assert.NotNull(body);
            var il = body.GetILAsByteArray();
            Assert.NotNull(il);

            var module = typeof(CopyEngine).Module;
            int callCount = CollectCallSiteOffsets(il, module, "FindPosition").Count;

            Assert.True(
                callCount >= 2,
                $"FlattenOneAccount must contain at least 2 FindPosition call sites (pre-cancel + post-cancel re-read). Found: {callCount}"
            );
        }

        // T_B76_05: FlattenOneAccount IL: CancelAllAccountOrders call site appears before the second
        // FindPosition call site. Proves the cancel-then-re-read sequence is correct.
        // IL assertion: uses name+declaring-type resolution via helper (stable). See DW-C39-12.
        // Offset ordering assertion verifies sequencing invariant, not exact byte offsets.
        [Fact]
        public void T_B76_05_FlattenOneAccount_CancelBeforeSecondFindPosition_InIL()
        {
            var mi = typeof(CopyEngine).GetMethod(
                "FlattenOneAccount",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            Assert.NotNull(mi);

            var body = mi.GetMethodBody();
            Assert.NotNull(body);
            var il = body.GetILAsByteArray();
            Assert.NotNull(il);

            var module = typeof(CopyEngine).Module;
            var findPosOffsets = CollectCallSiteOffsets(il, module, "FindPosition");
            int cancelOffset = FindFirstCallSiteOffset(il, module, "CancelAllAccountOrders");

            Assert.True(
                findPosOffsets.Count >= 2,
                "FlattenOneAccount must have at least 2 FindPosition call sites"
            );
            Assert.True(cancelOffset >= 0, "FlattenOneAccount must call CancelAllAccountOrders");

            int secondFindPosOffset = findPosOffsets[1];
            Assert.True(
                cancelOffset < secondFindPosOffset,
                $"CancelAllAccountOrders offset ({cancelOffset}) must be before second FindPosition offset ({secondFindPosOffset})"
            );
        }

        // T_B76_06: FlattenOneAccount IL has at least 5 local variables.
        // Expected locals: foreach loop var (o), pos, posAfterCancel, action, order.
        // Proves the full method body is compiled (not a stub).
        // IL assertion: local count >= 5 (lower bound only, not exact). See DW-C39-12.
        // Roslyn version dependency: valid for net8.0 target. Review on toolchain upgrade. See DW-C39-12.
        [Fact]
        public void T_B76_06_FlattenOneAccount_HasAtLeastFiveLocals()
        {
            var mi = typeof(CopyEngine).GetMethod(
                "FlattenOneAccount",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            Assert.NotNull(mi);

            var body = mi.GetMethodBody();
            Assert.NotNull(body);

            // IL assertion: local count lower bound -- compiler may allocate more locals for temporaries. See DW-C39-12.
            int localCount = body.LocalVariables.Count;
            Assert.True(
                localCount >= 5,
                $"FlattenOneAccount must have at least 5 local variables (loop var, pos, posAfterCancel, action, order). Found: {localCount}"
            );
        }

        // ======================================================================
        // TICKET-B76-2: PositionStateChanged dedup -- _lastHasPos + Interlocked.Exchange
        // HOTFIX-B76-POSSTATE-DEDUP-01
        // ======================================================================

        // T_B76_07: CopyEngine has field _lastHasPos of type ConcurrentDictionary<string, int[]>.
        // Verifies the dedup dictionary is declared and accessible via reflection.
        [Fact]
        public void T_B76_07_CopyEngine_HasLastHasPosField_ConcurrentDictionaryStringIntArray()
        {
            var fi = typeof(CopyEngine).GetField(
                "_lastHasPos",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            Assert.NotNull(fi);
            Assert.Equal(typeof(ConcurrentDictionary<string, int[]>), fi.FieldType);

            var instance = CopyEngine.Instance;
            var value = fi.GetValue(instance);
            Assert.NotNull(value);
            Assert.IsType<ConcurrentDictionary<string, int[]>>(value);
        }

        // T_B76_08: TryFirePositionState IL contains an Interlocked.Exchange call site.
        // Verifies the CAS dedup is compiled into TryFirePositionState.
        // DW-C39-11: raw token comparison replaced with stable name+declaring-type check.
        // Interlocked.Exchange is in mscorlib/System.Runtime (cross-assembly). The call site in
        // CopyEngine emits a MemberRef token, not the MethodDef token from the declaring assembly,
        // so token equality never holds across assembly boundaries. Use name+type resolution instead.
        [Fact]
        public void T_B76_08_TryFirePositionState_IL_ContainsInterlockedExchangeCallSite()
        {
            var mi = typeof(CopyEngine).GetMethod(
                "TryFirePositionState",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            Assert.NotNull(mi);

            var body = mi.GetMethodBody();
            Assert.NotNull(body);
            var il = body.GetILAsByteArray();
            Assert.NotNull(il);
            Assert.True(il.Length > 0, "TryFirePositionState must have a non-empty IL body");

            var module = typeof(CopyEngine).Module;
            bool foundExchange = false;
            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] == 0x28 || il[i] == 0x6F) // call or callvirt
                {
                    int token =
                        il[i + 1] | (il[i + 2] << 8) | (il[i + 3] << 16) | (il[i + 4] << 24);
                    try
                    {
                        // DW-C39-11: resolve by name + declaring type (stable across assembly boundaries).
                        // Raw token comparison fails for cross-assembly MemberRef tokens.
                        var mb = module.ResolveMethod(token) as MethodBase;
                        if (
                            mb != null
                            && mb.Name == "Exchange"
                            && mb.DeclaringType == typeof(System.Threading.Interlocked)
                        )
                        {
                            foundExchange = true;
                            break;
                        }
                    }
                    catch
                    { /* token resolves to a non-method or is not resolvable in this context -- skip */
                    }
                }
            }

            Assert.True(
                foundExchange,
                "TryFirePositionState must call Interlocked.Exchange(ref int, int) -- HOTFIX-B76-POSSTATE-DEDUP-01"
            );
        }

        // T_B76_09: TryFirePositionState is a private instance method on CopyEngine.
        // Accessibility check: not public, not static.
        [Fact]
        public void T_B76_09_TryFirePositionState_IsPrivateInstanceMethod()
        {
            var mi = typeof(CopyEngine).GetMethod(
                "TryFirePositionState",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            Assert.NotNull(mi);

            var publicMi = typeof(CopyEngine).GetMethod(
                "TryFirePositionState",
                BindingFlags.Public | BindingFlags.Instance
            );
            Assert.Null(publicMi);

            Assert.False(
                mi.IsStatic,
                "TryFirePositionState must be an instance method (not static)"
            );
            Assert.True(mi.IsPrivate, "TryFirePositionState must be private");
        }

        // ======================================================================
        // TICKET-B76-3: GetLeaderAtmTemplateName class-name guard
        // HOTFIX-B76-ATM-TPL-CLASSNAME
        // ======================================================================

        // T_B76_10: GetLeaderAtmTemplateName(null) returns string.Empty.
        // Regression guard -- null-chart path must continue to return empty string.
        [Fact]
        public void T_B76_10_GetLeaderAtmTemplateName_NullChart_ReturnsStringEmpty()
        {
            var mi = typeof(TradeCopierPanel).GetMethod(
                "GetLeaderAtmTemplateName",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public
            );
            Assert.NotNull(mi);

            var result = mi.Invoke(null, new object[] { null });
            Assert.Equal(string.Empty, result);
            Assert.NotNull(result);
        }

        // T_B76_11: GetLeaderAtmTemplateName method body contains the string literal "AtmStrategy"
        // used as the class-name guard comparison.
        // Confirms HOTFIX-B76-ATM-TPL-CLASSNAME is compiled into the method.
        // IL assertion: scans ldstr tokens anywhere in method body (not at a fixed offset).
        // Same-assembly ResolveString is stable. Roslyn version dependency: valid for net8.0 target.
        // Review on toolchain upgrade. See DW-C39-12.
        [Fact]
        public void T_B76_11_GetLeaderAtmTemplateName_IL_ContainsAtmStrategyClassNameGuardString()
        {
            var mi = typeof(TradeCopierPanel).GetMethod(
                "GetLeaderAtmTemplateName",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public
            );
            Assert.NotNull(mi);

            var body = mi.GetMethodBody();
            Assert.NotNull(body);
            var il = body.GetILAsByteArray();
            Assert.NotNull(il);
            Assert.True(il.Length > 0, "GetLeaderAtmTemplateName must have a non-empty IL body");

            // IL assertion: scan for ldstr opcode anywhere in method body (not at a fixed offset). See DW-C39-12.
            bool found = false;
            var module = typeof(TradeCopierPanel).Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] == 0x72) // ldstr
                {
                    int token =
                        il[i + 1] | (il[i + 2] << 8) | (il[i + 3] << 16) | (il[i + 4] << 24);
                    try
                    {
                        var s = module.ResolveString(token);
                        if (s == "AtmStrategy")
                        {
                            found = true;
                            break;
                        }
                    }
                    catch { }
                }
            }

            Assert.True(
                found,
                "GetLeaderAtmTemplateName must contain string literal \"AtmStrategy\" (HOTFIX-B76-ATM-TPL-CLASSNAME class-name guard)"
            );
        }

        // T_B76_12: GetLeaderAtmTemplateName is an internal static method on TradeCopierPanel.
        // Accessibility check: internal (assembly-accessible), static, returns string.
        [Fact]
        public void T_B76_12_GetLeaderAtmTemplateName_IsInternalStaticOnTradeCopierPanel()
        {
            // internal is visible as Assembly-scope; try NonPublic first, then Public (same assembly)
            var mi = typeof(TradeCopierPanel).GetMethod(
                "GetLeaderAtmTemplateName",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            if (mi == null)
            {
                mi = typeof(TradeCopierPanel).GetMethod(
                    "GetLeaderAtmTemplateName",
                    BindingFlags.Public | BindingFlags.Static
                );
            }

            Assert.NotNull(mi);
            Assert.True(mi.IsStatic, "GetLeaderAtmTemplateName must be static");
            Assert.Equal(typeof(string), mi.ReturnType);

            var ps = mi.GetParameters();
            Assert.Single(ps);
            Assert.Equal("currentChart", ps[0].Name);
        }

        // CollectCallSiteOffsets: returns all IL byte offsets of call/callvirt instructions that
        // resolve to a method named `methodName` on `CopyEngine`. DW-C39-12 helper.
        // CYC: 1(base)+1(for)+1(if-opcode)+1(try-catch)+1(if-name-match) = 5
        private static System.Collections.Generic.List<int> CollectCallSiteOffsets(
            byte[] il,
            Module module,
            string methodName
        )
        {
            var offsets = new System.Collections.Generic.List<int>();
            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] != 0x28 && il[i] != 0x6F)
                    continue;
                int token = il[i + 1] | (il[i + 2] << 8) | (il[i + 3] << 16) | (il[i + 4] << 24);
                try
                {
                    var mb = module.ResolveMethod(token) as MethodBase;
                    if (mb != null && mb.Name == methodName && mb.DeclaringType == typeof(CopyEngine))
                        offsets.Add(i);
                }
                catch { }
            }
            return offsets;
        }

        // FindFirstCallSiteOffset: returns the first IL byte offset of a call/callvirt that
        // resolves to `methodName` on `CopyEngine`, or -1 if not found. DW-C39-12 helper.
        // CYC: 1(base)+1(for)+1(if-opcode)+1(try-catch)+1(if-name-match) = 5
        private static int FindFirstCallSiteOffset(byte[] il, Module module, string methodName)
        {
            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] != 0x28 && il[i] != 0x6F)
                    continue;
                int token = il[i + 1] | (il[i + 2] << 8) | (il[i + 3] << 16) | (il[i + 4] << 24);
                try
                {
                    var mb = module.ResolveMethod(token) as MethodBase;
                    if (mb != null && mb.Name == methodName && mb.DeclaringType == typeof(CopyEngine))
                        return i;
                }
                catch { }
            }
            return -1;
        }
    }
}
