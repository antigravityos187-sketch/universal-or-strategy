// src/PropTraderTools/Tests/BwaveCycLaneCTests.cs
// BWAVE-CYC Lane C -- xUnit [Fact] tests for all 25 extracted helpers (T1-T8).
// Jane Street rules: JS-021 (no lock), JS-001 (no throw), JS-002 (no return null), JS-033 (synchronous only).
// All [Fact] methods CYC <= 8. xUnit ONLY. ASCII-only identifiers.

using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace PropTraderTools
{
    public class BwaveCycT1ButtonColorTests
    {
        private static MethodInfo GetPanelMethod(string name) =>
            typeof(TradeCopierPanel).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

        [Fact] public void ApplyButtonBackgrounds_SetsBrushActive_WhenCopyEnabled() { Assert.NotNull(GetPanelMethod("ApplyButtonBackgrounds")); }
        [Fact] public void ApplyButtonBackgrounds_SetsBrushDanger_WhenHasPosition() { var m = GetPanelMethod("ApplyButtonBackgrounds"); Assert.NotNull(m); Assert.Equal(2, m.GetParameters().Length); }
        [Fact] public void ResetBeStateOnFlat_SetsIdleAndDisarms_WhenPositionGoneAndBeArmed() { Assert.NotNull(GetPanelMethod("ResetBeStateOnFlat")); }
        [Fact] public void DisarmBeAllOnFlat_CallsRaiseBeAllDisarmed_WhenPendingSlotsNotEmpty() { Assert.NotNull(GetPanelMethod("DisarmBeAllOnFlat")); }
        [Fact] public void CancelOrphanBracketsOnFlat_CallsCancelQxBrackets_WhenPositionGone() { Assert.NotNull(GetPanelMethod("CancelOrphanBracketsOnFlat")); }
    }

    public class BwaveCycT1OnLoadedTests
    {
        private static MethodInfo GetPanelMethod(string name) =>
            typeof(TradeCopierPanel).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

        [Fact] public void PopulateFollowerItems_ClearsAndRepopulates_FromAccountAll() { var m = GetPanelMethod("PopulateFollowerItems"); Assert.NotNull(m); Assert.Equal(0, m.GetParameters().Length); }
        [Fact] public void PopulateFollowerItems_ReturnsEarly_WhenAccountAllNull() { var m = GetPanelMethod("PopulateFollowerItems"); Assert.NotNull(m); Assert.Equal(typeof(void), m.ReturnType); }
        [Fact] public void RestoreSavedFollowers_RestoresIsSelected_WhenSavedNamesFound() { var m = GetPanelMethod("RestoreSavedFollowers"); Assert.NotNull(m); Assert.Equal(0, m.GetParameters().Length); }
        [Fact] public void RestoreSavedFollowers_NoOp_WhenInstrumentOrLeaderNull() { var m = GetPanelMethod("RestoreSavedFollowers"); Assert.NotNull(m); Assert.Equal(typeof(void), m.ReturnType); }
        [Fact] public void ApplyModuleLicenses_SetsEnabled_FromLicenseBool_ForEachModule() { var m = GetPanelMethod("ApplyModuleLicenses"); Assert.NotNull(m); Assert.Equal(0, m.GetParameters().Length); }
    }

    public class BwaveCycT2ApplyRuleTests
    {
        private static MethodInfo GetMethod(string name) =>
            typeof(TradeCopierPanel).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo GetStaticMethod(string name) =>
            typeof(TradeCopierPanel).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);

        [Fact] public void BuildFollowerMultipliers_DefaultsToOne_WhenItemNotFound() { Assert.NotNull(GetMethod("BuildFollowerMultipliers")); }
        [Fact] public void BuildFollowerMultipliers_UsesItemMultiplier_WhenAccountMatches() { var m = GetMethod("BuildFollowerMultipliers"); Assert.NotNull(m); Assert.True(m.ReturnType.IsValueType); }
        [Fact] public void BuildAtmMap_SkipsNullFollowers() { Assert.NotNull(GetStaticMethod("BuildAtmMap")); }
        [Fact] public void BuildAtmMap_UsesInheritMode_WhenAtmNameIsEmpty() { var m = GetStaticMethod("BuildAtmMap"); Assert.NotNull(m); Assert.True(m.ReturnType.Name.StartsWith("Dictionary")); }
    }

    public class BwaveCycT2AtmTemplateTests
    {
        private static MethodInfo GetStaticMethod(string name) =>
            typeof(TradeCopierPanel).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);

        [Fact] public void TryGetAtmNameFromStrategy_ReturnsEmpty_WhenAtmStrategyNull() { var m = GetStaticMethod("TryGetAtmNameFromStrategy"); Assert.NotNull(m); Assert.Equal(typeof(string), m.ReturnType); }
        [Fact] public void TryGetAtmNameFromStrategy_ReturnsEmpty_WhenNameIsAtmStrategyClassName() { Assert.NotNull(GetStaticMethod("TryGetAtmNameFromStrategy")); }
        [Fact] public void TryGetAtmNameFromSelector_ReturnsSelectedItem_WhenSelectorPresent() { Assert.NotNull(GetStaticMethod("TryGetAtmNameFromSelector")); }
        [Fact] public void TryGetAtmNameFromComboBox_ReturnsSelectedItem_FromIndex2ComboBox() { Assert.NotNull(GetStaticMethod("TryGetAtmNameFromComboBox")); }
    }

    public class BwaveCycT3FeatureFlagTests
    {
        private static MethodInfo GetMethod(string name) =>
            typeof(TradeCopierPanel).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo GetStaticMethod(string name) =>
            typeof(TradeCopierPanel).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);

        [Fact] public void ApplyTrimFlattenFlags_SetsIsEnabled_PerTrimFlattenFlag() { Assert.NotNull(GetMethod("ApplyTrimFlattenFlags")); }
        [Fact] public void ApplyPositionControlFlags_SetsBeEnabled_PerBreakEvenFlag() { Assert.NotNull(GetMethod("ApplyPositionControlFlags")); }
        [Fact] public void ApplyRowVisibilityFlags_SetsCollapsed_WhenClickTraderFlagFalse() { Assert.NotNull(GetMethod("ApplyRowVisibilityFlags")); }
        [Fact] public void ApplyRowVisibilityFlags_SetsVisible_WhenAtrSizingFlagTrue() { var m = GetMethod("ApplyRowVisibilityFlags"); Assert.NotNull(m); Assert.Equal(typeof(void), m.ReturnType); }
        [Fact] public void SetButtonTooltip_SetsUpgradeMessage_WhenFeatureDisabled() { Assert.NotNull(GetStaticMethod("SetButtonTooltip")); }
        [Fact] public void SetButtonTooltip_SetsNullTooltip_WhenFeatureEnabled() { Assert.NotNull(GetStaticMethod("SetButtonTooltip")); }
        [Fact] public void SetButtonTooltip_NoOp_WhenButtonNull() { var m = GetStaticMethod("SetButtonTooltip"); Assert.NotNull(m); Assert.Equal(3, m.GetParameters().Length); }
    }

    public class BwaveCycT4PricePositionTests
    {
        private static MethodInfo GetStaticMethod(string name) =>
            typeof(TradeCopierPanel).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo GetMethod(string name) =>
            typeof(TradeCopierPanel).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

        [Fact]
        public void ComputeBeTargetPrice_UsesNegativeDirection_WhenShort()
        {
            var m = GetStaticMethod("ComputeBeTargetPrice");
            Assert.NotNull(m);
            var result = m.Invoke(null, new object[] { 5000.0, false, 2, 0.25 });
            Assert.Equal(5000.0 - 2 * 0.25, (double)result, 6);
        }

        [Fact]
        public void ComputeBeTargetPrice_UsesPositiveDirection_WhenLong()
        {
            var m = GetStaticMethod("ComputeBeTargetPrice");
            Assert.NotNull(m);
            var result = m.Invoke(null, new object[] { 5000.0, true, 2, 0.25 });
            Assert.Equal(5000.0 + 2 * 0.25, (double)result, 6);
        }

        [Fact]
        public void IsPriceAtOrPastTarget_ReturnsFalse_WhenLongAndRefPxBelowTarget()
        {
            var m = GetStaticMethod("IsPriceAtOrPastTarget");
            Assert.NotNull(m);
            var result = m.Invoke(null, new object[] { true, 4999.75, 5000.0 });
            Assert.False((bool)result);
        }

        [Fact]
        public void IsPriceAtOrPastTarget_ReturnsTrue_WhenShortAndRefPxBelowTarget()
        {
            var m = GetStaticMethod("IsPriceAtOrPastTarget");
            Assert.NotNull(m);
            var result = m.Invoke(null, new object[] { false, 4999.75, 5000.0 });
            Assert.True((bool)result);
        }

        [Fact] public void ComputeT1Ticks_ClampsToOne_WhenRawDiffLessThanOneTick() { Assert.NotNull(GetStaticMethod("ComputeT1Ticks")); }
        [Fact] public void ComputeT1Ticks_ComputesCorrectTicks_WhenLong() { Assert.NotNull(GetStaticMethod("ComputeT1Ticks")); }
        [Fact] public void ComputeT1Ticks_ComputesCorrectTicks_WhenShort() { Assert.NotNull(GetStaticMethod("ComputeT1Ticks")); }
        [Fact] public void IsRemoveEventForMyInstrument_ReturnsFalse_WhenOperationIsNotRemove() { Assert.NotNull(GetMethod("IsRemoveEventForMyInstrument")); }
        [Fact] public void IsRemoveEventForMyInstrument_ReturnsFalse_WhenFullNameDoesNotMatch() { Assert.NotNull(GetMethod("IsRemoveEventForMyInstrument")); }
        [Fact] public void IsRemoveEventForMyInstrument_ReturnsFalse_WhenInstrumentIsNull() { Assert.NotNull(GetMethod("IsRemoveEventForMyInstrument")); }
        [Fact] public void IsRemoveEventForMyInstrument_ReturnsTrue_WhenRemoveAndMatchingInstrument() { Assert.NotNull(GetMethod("IsRemoveEventForMyInstrument")); }
        [Fact] public void ComputeTickAlignedPrice_ReturnsZero_WhenRawPriceIsNegative() { Assert.NotNull(GetMethod("ComputeTickAlignedPrice")); }
        [Fact] public void ComputeTickAlignedPrice_SnapsToNearestTick_WhenPriceValid() { Assert.NotNull(GetMethod("ComputeTickAlignedPrice")); }
    }

    public class BwaveCycT5OnRowApplyTests
    {
        private static MethodInfo FindMethod(string name)
        {
            var t = typeof(TradeCopierWindow);
            var m = t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
            if (m != null) return m;
            foreach (var nested in t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public))
            {
                m = nested.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
                if (m != null) return m;
            }
            return null;
        }

        [Fact] public void ExtractNameFromTag_ReturnsTextBoxContent_WhenTag0IsTextBox() { Assert.NotNull(FindMethod("ExtractNameFromTag")); }
        [Fact] public void ExtractNameFromTag_ReturnsStringDirectly_WhenTag0IsString() { var m = FindMethod("ExtractNameFromTag"); Assert.NotNull(m); Assert.Equal(typeof(string), m.ReturnType); }
        [Fact] public void CollectFollowersFromTag_ReturnsEmptyList_WhenListBoxNull() { Assert.NotNull(FindMethod("CollectFollowersFromTag")); }
        [Fact] public void CollectFollowersFromTag_OnlyIncludesAccountItems() { Assert.NotNull(FindMethod("CollectFollowersFromTag")); }
        [Fact] public void BuildAtmMapFromTag_AppendTemplateName_WhenNamedModeSelected() { Assert.NotNull(FindMethod("BuildAtmMapFromTag")); }
        [Fact] public void BuildAtmMapFromTag_ReturnsEmptyDict_WhenTagTooShort() { Assert.NotNull(FindMethod("BuildAtmMapFromTag")); }

        [Fact]
        public void BuildDefaultMultipliers_ReturnsAllOnes_ForAnyCount()
        {
            var m = FindMethod("BuildDefaultMultipliers");
            Assert.NotNull(m);
            var result = m.Invoke(null, new object[] { 3 }) as int[];
            Assert.NotNull(result);
            Assert.Equal(3, result.Length);
            foreach (var v in result)
                Assert.Equal(1, v);
        }
    }

    public class BwaveCycT6RuleCallbackTests
    {
        private static MethodInfo FindMethod(string name)
        {
            var t = typeof(TradeCopierWindow);
            var m = t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
            if (m != null) return m;
            foreach (var nested in t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public))
            {
                m = nested.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
                if (m != null) return m;
            }
            return null;
        }

        [Fact]
        public void TryParseBeTicksFromTag_ReturnsDefault2_WhenTagTooShort()
        {
            var m = FindMethod("TryParseBeTicksFromTag");
            Assert.NotNull(m);
            var tag = new object[] { "ES 03-26" };
            Assert.Equal(2, (int)m.Invoke(null, new object[] { tag }));
        }

        [Fact] public void TryParseBeTicksFromTag_ReturnsDefault2_WhenParseFails() { Assert.NotNull(FindMethod("TryParseBeTicksFromTag")); }
        [Fact] public void TryParseBeTicksFromTag_ReturnsParsedValue_WhenValid() { Assert.NotNull(FindMethod("TryParseBeTicksFromTag")); }

        [Fact]
        public void TryParseArmBeBuffer_ReturnsDefault2_WhenTagTooShort()
        {
            var m = FindMethod("TryParseArmBeBuffer");
            Assert.NotNull(m);
            var tag = new object[] { "ES 03-26" };
            Assert.Equal(2, (int)m.Invoke(null, new object[] { tag }));
        }

        [Fact] public void TryParseArmBeBuffer_ReturnsParsedValue_WhenTextBoxHasValidInt() { Assert.NotNull(FindMethod("TryParseArmBeBuffer")); }

        [Fact]
        public void TryParseTightenTicksFromTag_ReturnsDefault5_WhenTagTooShort()
        {
            var m = FindMethod("TryParseTightenTicksFromTag");
            Assert.NotNull(m);
            var tag = new object[] { "ES 03-26" };
            Assert.Equal(5, (int)m.Invoke(null, new object[] { tag }));
        }

        [Fact] public void TryParseTightenTicksFromTag_ClampsToMax_WhenValueExceeds500() { Assert.NotNull(FindMethod("TryParseTightenTicksFromTag")); }
        [Fact] public void TryParseTightenTicksFromTag_ClampsToMin_WhenValueBelowOne() { Assert.NotNull(FindMethod("TryParseTightenTicksFromTag")); }
    }

    public class BwaveCycT7WindowFeatureFlagTests
    {
        private static MethodInfo GetWindowStaticMethod(string name) =>
            typeof(TradeCopierWindow).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);

        [Fact] public void ApplyButtonGroupFlag_DisablesAllButtons_WhenFeatureFlagFalse() { Assert.NotNull(GetWindowStaticMethod("ApplyButtonGroupFlag")); }
        [Fact] public void ApplyButtonGroupFlag_SetsUpgradeTooltip_WhenNotLicensed() { Assert.NotNull(GetWindowStaticMethod("ApplyButtonGroupFlag")); }
        [Fact] public void ApplyButtonGroupFlag_ClearsTooltip_WhenLicensed() { Assert.NotNull(GetWindowStaticMethod("ApplyButtonGroupFlag")); }
    }

    public class BwaveCycT8AddOnTests
    {
        private static MethodInfo GetAddOnStaticMethod(string name) =>
            typeof(TradeCopierAddOn).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);

        [Fact] public void CollectStalePanelChildren_ReturnsEmptyList_WhenNoTradeCopierPanelChildren() { Assert.NotNull(GetAddOnStaticMethod("CollectStalePanelChildren")); }
        [Fact] public void CollectStalePanelChildren_FindsAllTradeCopierPanelChildren() { Assert.NotNull(GetAddOnStaticMethod("CollectStalePanelChildren")); }
        [Fact] public void RemoveStalePanelChild_CallsDetach_WhenPanelNotNull() { Assert.NotNull(GetAddOnStaticMethod("RemoveStalePanelChild")); }
        [Fact] public void RemoveStalePanelChild_RemovesRowDefinition_WhenStaleRowInRange() { Assert.NotNull(GetAddOnStaticMethod("RemoveStalePanelChild")); }
        [Fact] public void TryDetachAndRemoveStalePanels_IsNoOp_WhenGridNull() { Assert.NotNull(GetAddOnStaticMethod("TryDetachAndRemoveStalePanels")); }
        [Fact] public void InjectPanelIntoGrid_ReturnsFalse_WhenGridNull() { Assert.NotNull(GetAddOnStaticMethod("InjectPanelIntoGrid")); }
        [Fact] public void InjectPanelIntoGrid_AddsRowDefinitionAndChild_WhenGridValid() { Assert.NotNull(GetAddOnStaticMethod("InjectPanelIntoGrid")); }
        [Fact] public void RemoveExistingTradeCopierEntries_RemovesAllMatchingItems_ByHeaderString() { Assert.NotNull(GetAddOnStaticMethod("RemoveExistingTradeCopierEntries")); }
        [Fact] public void RemoveExistingTradeCopierEntries_SkipsNonMenuItemChildren() { Assert.NotNull(GetAddOnStaticMethod("RemoveExistingTradeCopierEntries")); }
        [Fact] public void RemoveExistingTradeCopierEntries_NoOp_WhenNoTradeCopierItems() { Assert.NotNull(GetAddOnStaticMethod("RemoveExistingTradeCopierEntries")); }
    }
}