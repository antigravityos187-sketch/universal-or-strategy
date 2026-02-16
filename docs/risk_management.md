# Risk Management & Sizing Logic

The V12 strategy implements a rigorous, automated risk management framework to protect equity across the entire fleet.

## 📏 Auto-Sizing Engine (V12.30+)

The strategy calculates position size dynamically based on current market volatility (ATR) and a fixed dollar risk cap.

### The Math
1. **Stop Distance**: `currentATR * StopMultiplier`
2. **Ceiling Rounding**: The stop distance is rounded **UP** to the nearest whole point. 
   > [!TIP]
   > Rounding up ensures that our dollar risk calculation is conservative.
3. **Contract Calculation**: `MaxRiskAmount / (CeilingStop * PointValue)`
4. **Floor Rounding**: The quantity is rounded **DOWN** to the nearest integer.
   > [!IMPORTANT]
   > Rounding down guarantees that we never exceed the `MaxRiskAmount` in dollar terms.

## 🎯 4-Target Distribution

Trades are split into four logical "Exit Clusters" to balance quick scalps with long-running trends.

| Target | Purpose | Default Split |
| :--- | :--- | :--- |
| **T1** | Quick Scalp Anchor | Fixed 1.0 Point Profit |
| **T2** | Standard Target | ATR or OR-Range Relative |
| **T3** | Extended Target | ATR or OR-Range Relative |
| **T4** | The Runner | Trailing Stop Only |

### Priority Fill Model
For accounts with small contract sizes (e.g., < 4 contracts), the strategy uses a **Priority Fill** model:
1. **T1 (Anchor)** is always filled first.
2. **T4 (Runner)** is protected as the second priority.
3. **T2/T3** are allocated only if sufficient contracts remain.

## 🔒 Compliance & Consistency

### Consistency Lock
Prevents "Revenge Trading" or over-leveraging. If a fleet account hits its `MaxDailyProfitCap`, it is automatically locked from new entries for the remainder of the session.

### Reaper Desync Protection
If the Reaper Audit detects that a fleet account's position differs from the expected Master state, it can:
- **Warn**: Log a high-severity alert to the panel.
- **Flatten**: Kill all orders and positions on the desynced account to prevent runaway risk.
