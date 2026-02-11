# MISSION BRIEF: [V12-UI-001] Section 1 - Dynamic Dashboard & Mode Panels

## 🎯 Primary Objective
Refactor the side panel of the Universal OR Strategy V12.11 to replace redundant mode buttons with a dynamic **Mode Config Container**. The UI must automatically toggle visibility of sub-panels based on the selected configuration mode (ORB, RMA, FFMA, MOMO, TREND, RETEST).

## 🛠 Target Component
- **File**: [V12StandardPanel_V12_001.cs](file:///C:/WSGTA/universal-or-strategy/V12StandardPanel_V12_001.cs)
- **Class**: `V12StandardPanel`

## 📋 Technical Requirements

### 1. The Container System
- Create a `modeConfigContainer` (WPF Grid or StackPanel) that sits inside the side panel.
- Implement a `Dictionary<string, FrameworkElement> modePanels` to store the sub-panels.

### 2. Mode-Specific Panels
Build panels for each major mode with the following controls:
- **ORB**: Labels for "OR LONG" / "OR SHORT" status.
- **RMA**: ON/OFF Toggle for "Chart Click Mode".
- **MOMO**: ON/OFF Toggle for "Momo Trigger".
- **FFMA**: "Manual Entry" button + "Strategy Enable" toggle.
- **TREND/RETEST**: Type selection (STD or RMA).

### 3. Visibility Logic
- In `SelectConfigMode(string mode, Button clickedBtn)`, update the logic to:
  1. Hide all panels in the container.
  2. Show the panel corresponding to the `mode` string.
  3. Ensure the selection persists across UI refreshes.

### 4. Style & Safety
- **Visuals**: Use the existing Blue/Gold theme constants.
- **Safety**: Do NOT modify the trade execution logic in `UniversalORStrategyV12.cs`. This is a pure UI/Control refactor.
- **Protocol**: If a logic conflict arises, use the `ask_deepseek` tool (OpenRouter) to resolve it.

## 🚀 Execution Steps
1. **Initialize Team**: Move into `delegate` mode.
2. **Analysis**: Read the existing `SelectConfigMode` method to understand how buttons are currently highlighted.
3. **Drafting**: Use `delegation_bridge` to draft the container layout.
4. **Implementation**: Apply changes to `V12StandardPanel_V12_001.cs`.
5. **Verification**: Confirm no compilation errors.

## ⚠️ Critical Constraints
- **Main Branch Safety**: All work MUST be done on a `dev/` branch.
- **No Deletion**: Do not delete the old button logic until the container logic is verified as functional parity.

---
*Created by Antigravity (Architect) - Session ID: 520*
