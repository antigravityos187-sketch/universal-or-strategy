---
description: How to verify NinjaTrader API usage before writing code
---

# API Verification Workflow

Before writing or modifying any NinjaScript code, follow these steps:

## 1. Lookup (Agent/IDE Specific)
- **Antigravity**: Open `.agent/brain/context7_pilot_data.md` directly.
- **Cursor**: Use `@context7_pilot_data.md` in Composer/Chat.
- **Coex/Codex**: Run `cat .agent/brain/context7_pilot_data.md` before generating code.

## 2. Confirm Signature
Verify the exact method signature, parameter types, and return type match your intended usage. Pay special attention to the **Red Flag Patterns** listed in `.agent/protocols/api_verification_protocol.md`.

## 3. Check Gotchas
Review the ⚠️ warnings in the index for your specific API:
- `OrderId` is mutable — don't use as key
- `Instrument` not available before `State.DataLoaded`
- `GetRealtimeOrder()` required on hist→live transition
- `SetStopLoss` overrides `SetTrailStop` for same signal
- UI access needs `Dispatcher`

## 4. Cite in Code
Add a one-line `// AVP:` comment for any API with known risks:
```
// AVP: OrderId is mutable — using Name for tracking
```

## 5. Post-Edit Verify
After code changes, confirm:
- All method signatures match the index
- All known gotchas are addressed
- No red flag patterns present

## 6. Gap Reporting
If a needed API is NOT in the index:
1. Search the web for the correct documentation
2. Add the new entry to `context7_pilot_data.md`
3. Save to Supermemory for cross-agent access
