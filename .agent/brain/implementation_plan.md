# Task 3: Account Performance Metrics - Implementation Plan

## Goal Description
Enhance the existing performance tracking system to provide a comprehensive "Health Dashboard" for the SIMA fleet. This includes tracking detailed metrics, enforcing dynamic Apex consistency rules, and persisting historical data for performance review.

## User Review Required
> [!IMPORTANT]
> **Consistency & Payout Warnings**: All violations (30% rule, trailing drawdown proximity, payout eligibility) will be **UI Warnings only**. This ensures you are alerted to risks without the strategy interfering with your execution.

> [!TIP]
> **Apex Payout Tracking**: Focused on **50k Apex Rithmic Accounts** ($2,600 min profit, 10 trading days, $2,500 trailing drawdown limit).

> [!IMPORTANT]
> **UI Account Selector**: The dashboard will feature a **Manual Dropdown Selector** in the chart overlay. By default, it will "Auto-Focus" on the account with the highest risk (lowest drawdown buffer or consistency violation), but Mo can manually override this selection.

> [!NOTE]
> **Data Storage**: Metrics will be saved in `Documents/NinjaTrader 8/SIMA_Logs/DailySummaries.csv` for historical tracking.

## Proposed Changes

---

### [Component] UniversalORStrategyV12.cs

#### [MODIFY] [UniversalORStrategyV12.cs](file:///c:/Users/Mohammed%20Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy/UniversalORStrategyV12.cs)
- **Enhanced Tracking Variables**:
    - Add `ConcurrentDictionary<string, int> accountTradeCount` to track trades per account.
    - Add `ConcurrentDictionary<string, double> accountMaxDrawdown` to track daily peak-to-trough drawdown.
- **Improved Consistency Logic**:
    - Update `ExecuteSmartDispatchEntry` to use a dynamic consistency cap: `Math.Min(MaxDailyProfitCap, accountTotalProfit[acct.Name] * (ConsistencyThreshold / 100.0))`.
- **Apex Payout Tracking**:
    - Add `ConcurrentDictionary<string, int> accountDaysTraded` to track unique days with >= 1 trade.
    - Implement `bool IsEligibleForPayout(string accountName)` and `string GetPayoutWarning(string accountName)`.
    - Payout rules to track:
        - **Minimum Days**: 10 trading days (Warning if < 10).
        - **Profit Threshold**: Minimum $2,600 (for 50k Rithmic) (Warning if < Threshold).
        - **Consistency**: 30% check (Warning if 1 day > 30% of total).
        - **Drawdown**: Proximity alert (Warning if current equity is within $200 of the $2,500 Trailing Drawdown limit).
- **Historical Logging**:
    - Implement `SaveDailySessionSummary()` called in `OnBarUpdate` at session end.
    - Append Account Name, Date, Realized PnL, Trade Count, and Max Drawdown to a CSV.
- **Metric Collection**:
    - Update `OnAccountExecutionUpdate` to increment `accountTradeCount` on entries.
    - Update `LogApexPerformance` to include these new fields in the JSON.

---

### [Component] V12StandardPanel.cs (or Strategy UI section)

#### [MODIFY] [UniversalORStrategyV12.cs](file:///c:/Users/Mohammed%20Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy/UniversalORStrategyV12.cs)
- **UI Performance Hub**:
    - Add a "FLEET HEALTH" section to the main grid.
    - Display "Aggregated Fleet P/L" at the top.
    - Add a small list/ticker showing the top 3 and bottom 3 accounts by P/L.
    - Add a "System Status" indicator (Green if all accounts within consistency limits, Red if any locked).
    - Add a **Manual Account Dropdown** to the overlay (WPF ComboBox) to allow overriding the "Auto-Focus" at-risk account.

---

### [Component] LegacyChartButtonsIndicator.cs [NEW]

#### [NEW] [LegacyChartButtonsIndicator.cs](file:///c:/Users/Mohammed%20Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy/LegacyChartButtonsIndicator.cs)
- **Standalone Artifact**: Create a new indicator that purely handles the creation of the graphical WPF buttons (LONG, SHORT, FLATTEN, etc.) on the chart.
- **Methods to Migrate**:
    - `CreateUI` (Legacy version)
    - `CreateDoubleHorizonDashboard`
    - Button helpers: `CreateMainButton`, `CreateSubButton`, `CreateModeButton`, `CreateMgmtButton`, `CreateTelemetryLabel`, `CreateCleanButton`, `CreateBuyButton`, `CreateSellButton`, `CreateDashedButton`, `CreateOrangeButton`, `CreateCleanModeButton`, `CreateTelemetryButton`, `CreateStackedTelemetryPair`.
    - Event handlers: `OnFFMAButtonClick`, `OnMOMOButtonClick`, `OnBreakevenButtonClick`, `OnMinimizeToggle`, `OnCloseOverlay`, `OnBreakevenPlus2Click`, `OnTrail1PtClick`, `OnTrail2PtClick`, `OnStopOffClick`, `OnTrim25Click`, `OnTrim50Click`.
    - Dropdown logic: `CreateDropdownPanel`, `ToggleT1Dropdown`, `ToggleT2Dropdown`, `ToggleT3Dropdown`, `ToggleRunnerDropdown`.
    - Drag/Resize handlers: `OnDragStart`, `OnDragMove`, `OnDragEnd`, `OnResizeStart`, `OnResizeMove`, `OnResizeEnd`.
- **IPC Bridge**: This indicator will act as a "Remote Control" pointing to `127.0.0.1:5000`. It will use a `TcpClient` to send commands (e.g., `FLATTEN`, `LONG`, `SHORT`) that the Strategy IPC Server already processes.

---

### [Component] UniversalORStrategyV12.cs

#### [MODIFY] [UniversalORStrategyV12.cs](file:///c:/Users/Mohammed%20Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy/UniversalORStrategyV12.cs)
- **Cleanup**: Remove all migated WPF button creation methods and their associated UI variables (`overlayContainer`, `mainBorder`, `contentViewbox`, `uiCreated`, etc.).
- **Preserved Logic**: Keep `ProcessIpcCommands` and `HandleClient` intact, as they serve both the new indicator and the V12 Standard Panel.
- **Optimization**: Significant reduction in file size (~2000+ lines of UI code removed) to improve NinjaTrader compilation speed.

---

### [Component] Apex Dashboard Overrides

#### [MODIFY] [UniversalORStrategyV12.cs](file:///c:/Users/Mohammed%20Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy/UniversalORStrategyV12.cs)
- **Manual Account Selector**: Implement the WPF `ComboBox` override in the chart overlay dashboard code (the *new* dashboard, not the legacy one being removed).
- **At-Risk Focus Logic**: Implement the auto-selection of the high-risk account while allowing the `ManualOverride` flag to persist the user's manual choice.

---

### [Component] Workspace Hygiene (Archiving)

#### [MODIFY] Root Directory & [archived-versions/](file:///c:/Users/Mohammed%20Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy/archived-versions)
- **Archive Legacy Versions**: Move all files matching `UniversalORStrategyV[1-9]*.cs` and `UniversalORSlaveV[1-9]*.cs` from the root directory into `archived-versions/`.
- **Archive Legacy Support Files**: Identify and move old `.ps1`, `.bak.cs`, and `.disabled` files that relate to versions prior to V10.
- **Maintain Current Baseline**: Ensure only `UniversalORStrategyV12.cs`, `UniversalOR_V9_MasterHub.cs`, and active UI indicators (V12StandardPanel.cs, etc.) remain in the root.

---

### [Component] Savings Dashboard (ROI Visualizer) [NEW]

#### [NEW] [Dashboard Artifact](file:///c:/Users/Mohammed%20Khalid/OneDrive/Desktop/WSGTA/Github/universal-or-strategy/.agent/brain/savings_dashboard.html)
- **Goal**: Provide Mo with a visual representation of the money saved by using the "Execution Desk" (OpenRouter Free Pool / Gemini Flash).
- **Core Logic**:
    - Parse `.agent/state/delegation_history.json`.
    - For each event: `Savings = Hypothetical_Sonnet_Cost - Actual_Cost`.
    - `Actual_Cost` is $0.00 for Free Pool models.
- **UI Elements**:
    - **Total Lifetime Savings**: A large, green ticker showing total USD saved.
    - **Efficiency Ratio**: A percentage showing how much cheaper the current system is vs. full-price Opus development.
    - **Model Breakdown**: A pie chart showing the distribution of tasks across the free model pool.
- **Integration**: The dashboard will be generated as a single-file HTML artifact that can be opened in any browser or displayed within the IDE.

---

## Task 8: Expert Tooling - TestSprite Integration

### Goal
Integrate TestSprite as an AI-powered testing layer for the "Execution Desk" to catch code errors before expensive tokens are used for debugging.

### Proposed Changes
1.  **Automated Validation Gate**:
    - Before ANY C# compilation or deployment, TestSprite (via MCP) will scan the delta for syntax and logic errors.
    - If errors are found, TestSprite provides the "Fix Log" to the current agent.
2.  **Token Savings Logic**:
    - Instead of Opus troubleshooting a missing semicolon or a logic loop, the "Free Task" (Gemini Flash) requests a TestSprite scan.
    - Result: Opus only sees *working* code or *explicit* fix instructions, cutting total reasoning time by 30-50%.

### Dashboard Linkage
- New metric: **"Bugs Caught by TestSprite"** logic added to the Savings Dashboard logic (hypothetical fix cost saved).

---

## Task 9: Context Optimization - Bloat Prevention

### Goal
Prevent "AI Context Bloat" (the 1M+ line diff issue) by strictly controlling what file data can be read by agents. This protects the Windows bridge from crashing and saves tokens.

### Proposed Changes
1.  **Ignore Rule Standardization**:
    - [NEW] Create `.claudeignore` in root.
    - [NEW] Create `.cursorignore` in root.
    - Path Exclusions: `bin/`, `obj/`, `.git/`, `.vs/`, `*.csv` (except templates), `*.zip`, `*.bak`.
2.  **Massive File Relocation**:
    - Audit the workspace for any file > 500KB.
    - Move non-code large files to `.agent/state/large_data/` or `archived-versions/data/`.
3.  **Knowledge Vault Update**:
    - Add **Lesson 9: The 1M Line Context Trap**.
    - Mandate a `dir` scan before any "Read all files" command.

### Verification Plan
- Run a `diff` mock in Cursor/CLI.
- Verify the diff count is < 5,000 lines.

---

## Verification Plan


### Automated Tests
- **Internal Logic Test**: I will create a temporary test method `VerifyConsistencyLogic()` that simulates a total profit and a daily profit to ensure the dynamic cap is calculated correctly.

### Manual Verification
1. **PnL Accuracy**: Compare the "Fleet P/L" displayed in the strategy UI with the sum of "Realized PnL" in the NinjaTrader Account tab for all Apex accounts.
2. **Consistency Lock**: Set `MaxDailyProfitCap` to a very low value (e.g., $10) and verify that the strategy prints `[DISPATCH] 🔒 SKIPPING ...` after the first trade fills.
3. **Log Check**: Verify that `SIMA_Logs/DailySummaries.csv` is created and correctly populated after a simulated session reset.
4. **Clean Root**: Verify that the root directory is significantly leaner and only contains V10+ active components.

