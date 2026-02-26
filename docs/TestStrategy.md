# Test Strategy — universal-or-strategy (V12)

**Date:** 2026-02-15  
**Owner:** Project manager (post–TestSprite handoff)  
**Status:** Decided

---

## 1. Context

- **Codebase:** NinjaTrader 8 C# strategy — SIMA multi-account copy-trading (V12).
- **Communication:** TCP IPC on port 5001 (custom protocol). **No HTTP REST API.**
- **TestSprite run (2026-02-15):** 0/5 backend tests passed; all failures due to **architecture mismatch** (tests expect HTTP; app exposes only TCP IPC). See `testsprite_tests/testsprite-mcp-test-report.md`.

---

## 2. Decision

| Option | Choice | Rationale |
|--------|--------|------------|
| **A** – Add HTTP API for TestSprite | **Rejected** | Adds surface area and a second protocol solely for testing; not aligned with current design. |
| **B** – TCP/IPC client tests | **Deferred** | Viable later for E2E/automation; not required for current quality gate. |
| **C** – C# unit/integration tests | **Primary** | Strategy and execution logic are in-process; NUnit/xUnit against strategy/ATR/rounding/fleet logic is the right fit. |
| **D** – Defer TestSprite for backend | **Accepted** | TestSprite backend tests assume HTTP; do not re-run until either an HTTP API exists or tests are rewritten for TCP/IPC. |

**Summary:** Use **Option C** as the main testing approach. **Defer** TestSprite backend runs (Option D). Leave **Option B** open for future TCP-based automation if needed.

---

## 3. Next steps (owners)

1. **Dev:** Add or extend a C# test project (e.g. NUnit or xUnit) in the solution.
   - Target: strategy/ATR/rounding (e.g. stop distance ceiling), MOMO/FFMA mode behavior, and fleet/account logic where testable in-process.
   - Path: `C:\WSGTA\universal-or-strategy` (use consistently in solution and CI).
2. **Dev/PM:** Document in README or runbook that:
   - Port 5001 is TCP IPC only; TestSprite backend tests are not run against this service unless an HTTP layer or TCP test suite is introduced.
3. **PM:** Consider Option B (TCP client tests) only if/when automated E2E tests against the live TCP service are required.

---

## 4. References

| File | Purpose |
|------|--------|
| `HANDOFF_PROJECT_MANAGER.md` | Handoff that triggered this decision. |
| `testsprite_tests/testsprite-mcp-test-report.md` | Full TestSprite report and failure analysis. |
| `testsprite_tests/tmp/config.json` | TestSprite config (backend, port 5001). Do not re-run backend tests until strategy matches (HTTP or TCP tests). |

---

*End of test strategy. Project manager has documented the decision and next steps.*
