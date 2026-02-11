# API Verification Protocol (AVP)

## 🎯 Purpose
Enforce **documentation-first coding** for all NinjaTrader 8 NinjaScript development. Every API call, property access, or class usage MUST be verified against the indexed reference before writing code.

> [!IMPORTANT]
> This protocol directly supports the **Anti-Guesswork Pillar**. API hallucinations in live trading code = real money lost.

## 📖 The Index
**Primary Source**: `.agent/brain/context7_pilot_data.md`
**Backup Source**: Supermemory (`universal-or-v12` project → recall "NinjaTrader API")

### 🛠️ Tool-Specific Integration
#### Cursor (IDE)
- **Rules File**: Ensure `.cursorrules` (or project rules) links to `.agent/brain/context7_pilot_data.md`.
- **Composer/Tab**: When generating code in Cursor Composer or via Tab, the user should provide the index as a context reference (@-mention or "Paste Context").

#### Coex / Codex (Execution Agent)
- **Role**: The "Hands" (Gemini Flash / DeepSeek) must perform a final syntax check against the index before merging.
- **Protocol**: If the Brain (Opus) sends a deployment manifest, Coex must verify the API calls in that manifest against the index to catch logic drift.

## 🔄 The Workflow (3-Step Lookup)

### Step 1: LOOKUP Before You Code
Before writing or modifying ANY NinjaScript code that touches an NT8 API:
1. Open `.agent/brain/context7_pilot_data.md`
2. Search for the class/method/property you need
3. Confirm the **exact signature**, **parameter types**, and **gotchas**

### Step 2: CITE in Your Code Comment
When using an API that has known risks, add a one-line citation:
```csharp
// AVP: OrderId is mutable — using Name for tracking (context7_pilot_data.md §1)
```

### Step 3: VERIFY Against the Index Post-Edit
After any code change, cross-check:
- [ ] All method signatures match the index
- [ ] All property types are correct
- [ ] All known gotchas are addressed (e.g., `GetRealtimeOrder()` on hist→live)
- [ ] No `State.SetDefaults` code accesses `Instrument` (must wait for `DataLoaded`)

## ⚠️ Red Flag Patterns (Auto-Reject)
Any code containing these patterns MUST be caught and corrected:

| Pattern | Why It's Wrong | Fix |
|---|---|---|
| Using `OrderId` as dictionary key | Mutable across lifetime | Use `Order.Name` or `Order.Token` |
| Accessing `Instrument` in `SetDefaults` | Not loaded yet | Move to `State.DataLoaded` |
| Missing `GetRealtimeOrder()` | Ghost orders on hist→live | Add in `OnOrderUpdate` |
| `SetStopLoss` + `SetTrailStop` same signal | StopLoss overrides Trail | Use different `fromEntrySignal` |
| Mixing managed + unmanaged methods | Runtime crash | Pick one approach per strategy |
| UI access without `Dispatcher` | Cross-thread exception | Wrap in `Dispatcher.InvokeAsync` |
| XAML with inline event handlers | Won't load in NT8 | Use code-behind pattern |

## 📈 Index Maintenance
- **When to Update**: After discovering a new API behavior during compilation or live testing
- **How to Update**: Add new entry to `context7_pilot_data.md` under the relevant section
- **Post-Mortem Rule**: Every compilation error traced to an API misuse triggers a new entry

## 🧠 Agent/IDE Instructions
All agents and IDE sessions operating on this project MUST:
1. **Antigravity (Architect)**: Use the index to build the initial logic blueprints.
2. **Cursor (Co-pilot)**: Reference the index via the `@context` or `.cursorrules` to prevent "Predictive Hallucination."
3. **Coex / Codex (Execution)**: Verify every method signature during the final code generation phase.
4. **Recall**: Use Supermemory (`query: "NinjaTrader API"`) if the index file is not in context.
5. **Never guess**: If it's not in the index, search the web first and update the repository.
