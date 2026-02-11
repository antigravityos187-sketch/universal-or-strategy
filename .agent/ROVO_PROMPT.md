# PROMPT FOR ROVO (Project Director)

**Mission**: Fix V12 Compilation Errors
**Context**: We have manually deployed `UniversalORStrategyV12_Dev.cs` and `V12StandardPanel_V12_001_Dev.cs` to the local NinjaTrader `bin/Custom` folders. However, we are hitting compilation errors.

**Input Errors**:
```csv
NinjaScript File,Error,Code,Line,Column
UniversalORStrategyV12_Dev.cs,The type or namespace name 'SignalBroadcaster' could not be found,CS0246,5187,58
UniversalORStrategyV12_Dev.cs,The type or namespace name 'ATR'/'RSI' could not be found,CS0246,66,17
V12StandardPanel_V12_001_Dev.cs,The name 'indicator' does not exist in the current context,CS0103,3069,11
NinjaTrader\Vendor.cs,The name 'indicator' does not exist in the current context,CS0103,216,11
UniversalORStrategyV12_Dev.cs,'Draw' does not contain a definition for 'HorizontalLine'/'Rectangle',CS0117,967,26
```

**Analysis & Instructions**:
1.  **Missing File**: `SignalBroadcaster` error (CS0246) indicates `SignalBroadcaster.cs` was NOT deployed.
    *   **Action**: Locate `C:\WSGTA\universal-or-strategy\SignalBroadcaster.cs` and deploy it to `...\bin\Custom\AddOns` or `...\bin\Custom\Strategies`.
2.  **Panel Scope Bug**: The Panel code references an `indicator` variable (likely a copy-paste from a different context).
    *   **Action**: In `V12StandardPanel_V12_001_Dev.cs`, identify what `indicator` refers to. It should probably be `this` (if it inherits from Indicator) or a reference to the parent strategy. Replace `indicator.` with `this.` or removing the prefix if valid.
3.  **Namespace Issues**: `ATR`, `RSI`, `EMA` errors suggest missing `using` directives or a conflict.
    *   **Action**: Ensure `using NinjaTrader.NinjaScript.Indicators;` is present in `UniversalORStrategyV12_Dev.cs`. Check if `ATR` is being used as a type or a variable name.
4.  **Drawing Tools**: `Draw.HorizontalLine` error.
    *   **Action**: Verify `using NinjaTrader.NinjaScript.DrawingTools;`. If `Draw` is being called on an object, check the object type.

**Goal**:
Provide the corrected code blocks or file operations to resolve these errors and get a successful build.
