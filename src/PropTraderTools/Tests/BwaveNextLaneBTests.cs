// BWAVE-NEXT Lane B T2 tests -- DW-NEW-08 Option D structural verification.
// xUnit only -- JS-051. No lock() -- JS-021. No async void -- JS-033.
// Structural reflection tests only -- no live NT8 Account required.
using System;
using System.Collections.Concurrent;
using System.Reflection;
using NinjaTrader.NinjaScript;
using Xunit;

namespace PropTraderTools.Tests
{
    public class BwaveNextLaneBTests
    {
        private static readonly Type EngineType = typeof(CopyEngine);
        private const BindingFlags Priv = BindingFlags.NonPublic | BindingFlags.Instance;

        [Fact]
        public void DrainThenDispatch_MethodExists_WithExpectedSignature()
        {
            // Structural: verify DrainThenDispatch method exists with correct signature (6 params)
            var method = EngineType.GetMethod("DrainThenDispatch", Priv);
            Assert.NotNull(method);
            Assert.Equal(typeof(void), method.ReturnType);
            var parms = method.GetParameters();
            Assert.Equal(6, parms.Length);
            Assert.Equal(typeof(NinjaTrader.Cbi.Account), parms[0].ParameterType);
            Assert.Equal(typeof(NinjaTrader.Cbi.Instrument), parms[1].ParameterType);
            Assert.Equal(typeof(int), parms[2].ParameterType);
            Assert.Equal(typeof(double), parms[3].ParameterType);
            Assert.Equal(typeof(NinjaTrader.Cbi.OrderAction), parms[4].ParameterType);
            Assert.Equal(typeof(NinjaTrader.Cbi.OrderType), parms[5].ParameterType);

            // Structural: verify _pendingDispatchDrains field is readonly ConcurrentDictionary<,>
            var field = EngineType.GetField("_pendingDispatchDrains", Priv);
            Assert.NotNull(field);
            Assert.True(field.IsInitOnly, "_pendingDispatchDrains must be readonly (JS-008)");
            Assert.Equal(typeof(ConcurrentDictionary<,>), field.FieldType.GetGenericTypeDefinition());

            // Structural: verify PendingDispatchDrain nested type is sealed
            var drainType = EngineType.GetNestedType("PendingDispatchDrain", BindingFlags.NonPublic);
            Assert.NotNull(drainType);
            Assert.True(drainType.IsSealed, "PendingDispatchDrain must be sealed");

            // Structural: verify PendingCancelCount is a plain int field (not property)
            // so Interlocked.Decrement can take its reference
            var countField = drainType.GetField(
                "PendingCancelCount",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(countField);
            Assert.Equal(typeof(int), countField.FieldType);
            Assert.False(countField.IsInitOnly, "PendingCancelCount must NOT be readonly -- Interlocked needs ref");
        }

        [Fact]
        public void OnDrainCancelAck_MethodExists_WithExpectedSignature()
        {
            // Structural: verify OnDrainCancelAck exists as private void (string acctKey)
            var ackMethod = EngineType.GetMethod("OnDrainCancelAck", Priv);
            Assert.NotNull(ackMethod);
            Assert.Equal(typeof(void), ackMethod.ReturnType);
            var ackParms = ackMethod.GetParameters();
            Assert.Single(ackParms);
            Assert.Equal(typeof(string), ackParms[0].ParameterType);

            // Structural: verify SubmitDrainedEntry exists as private void (string acctKey)
            var submitMethod = EngineType.GetMethod("SubmitDrainedEntry", Priv);
            Assert.NotNull(submitMethod);
            Assert.Equal(typeof(void), submitMethod.ReturnType);
            var submitParms = submitMethod.GetParameters();
            Assert.Single(submitParms);
            Assert.Equal(typeof(string), submitParms[0].ParameterType);

            // Structural: verify TryDrainWatchdog exists as private void with 0 parameters
            var watchdog = EngineType.GetMethod("TryDrainWatchdog", Priv);
            Assert.NotNull(watchdog);
            Assert.Equal(typeof(void), watchdog.ReturnType);
            Assert.Empty(watchdog.GetParameters());
        }

        [Fact]
        public void DrainWatchdog_MethodExists_WithExpectedSignature()
        {
            // Structural: verify PendingDispatchDrain.TimestampTicks property exists and is long
            var drainType = EngineType.GetNestedType("PendingDispatchDrain", BindingFlags.NonPublic);
            Assert.NotNull(drainType);
            var prop = drainType.GetProperty(
                "TimestampTicks",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(prop);
            Assert.Equal(typeof(long), prop.PropertyType);

            // Structural: verify _pendingDispatchDrains key type is string (StringComparer.Ordinal)
            var field = EngineType.GetField("_pendingDispatchDrains", Priv);
            Assert.NotNull(field);
            Assert.Equal(typeof(string), field.FieldType.GetGenericArguments()[0]);

            // Structural: verify TryDrainWatchdog has 0 parameters and returns void
            var watchdog = EngineType.GetMethod("TryDrainWatchdog", Priv);
            Assert.NotNull(watchdog);
            Assert.Empty(watchdog.GetParameters());
            Assert.Equal(typeof(void), watchdog.ReturnType);

            // Structural: verify PendingDispatchDrain has no public constructor
            var ctors = drainType.GetConstructors(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            Assert.NotEmpty(ctors);
            Assert.All(ctors, c => Assert.False(c.IsPublic));
        }
    }
}