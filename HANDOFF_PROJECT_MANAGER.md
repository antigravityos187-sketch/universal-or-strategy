# Handoff: Project Manager — TestSprite & universal-or-strategy

**Date:** 2026-02-15  
**From:** Previous AI session  
**To:** Project manager (human or agent) taking over this task

---

## 1. Project context

- **Repo:** universal-or-strategy  
- **Location:** `C:\WSGTA\universal-or-strategy` (no longer under OneDrive)  
- **Type:** NinjaTrader 8 C# strategy — SIMA multi-account copy-trading engine (V12).  
- **Notable:** TCP IPC on port 5001 (custom protocol); no HTTP REST API.

---

## 2. What was done

- TestSprite was **bootstrapped** for this repo (backend, port 5001, scope: codebase).  
- **Backend test plan** was generated; **test execution** was run.  
- **Report** written: `testsprite_tests\testsprite-mcp-test-report.md`.  
- **Result:** 0/5 tests passed — all failures due to **architecture mismatch**: tests expect HTTP REST; app exposes only TCP IPC.

---

## 3. Task for project manager

**Primary:** Decide how testing should work for this codebase and drive the next steps.

**Options to evaluate:**

| Option | Description | Owner / Next step |
|--------|-------------|-------------------|
| **A** | Add a thin HTTP API (e.g. ASP.NET Core) that exposes strategy/ATR/fleet state for TestSprite. | Dev: design endpoints; align with TestSprite test plan. |
| **B** | Use TCP/IPC client tests instead of HTTP (TestSprite or other tooling). | Dev/QA: define IPC test cases and automate. |
| **C** | Rely on C# unit/integration tests (NUnit/xUnit) against strategy logic only. | Dev: add or extend tests in solution. |
| **D** | Defer automated “backend” testing; keep TestSprite for a future HTTP service only. | PM: document decision and update test strategy. |

**Additional:**

- Ensure **path** `C:\WSGTA\universal-or-strategy` is used consistently (docs, CI, TestSprite config).  
- If the strategy/service on port 5001 is run elsewhere, update TestSprite config and docs.  
- Re-run TestSprite only after either (1) an HTTP API is in place and running, or (2) tests are changed to TCP/IPC or removed.

---

## 4. Key files

| File | Purpose |
|------|--------|
| `testsprite_tests\testsprite-mcp-test-report.md` | Full TestSprite run report and recommendations. |
| `testsprite_tests\tmp\config.json` | TestSprite config (backend, port 5001). |
| `testsprite_tests\tmp\code_summary.yaml` | Code summary used for test plan. |
| `testsprite_tests\testsprite_backend_test_plan.json` | Generated backend test plan. |

---

## 5. Suggested next actions (for PM)

1. Read `testsprite_tests\testsprite-mcp-test-report.md`.  
2. Choose Option A, B, C, or D (or a combination) and document in a short **test strategy** (e.g. `docs/TestStrategy.md` or a ticket).  
3. Assign owners and deadlines for the chosen option(s).  
4. If Option A: specify which endpoints to add and how they map to the existing test plan.  
5. Update any runbooks or README that reference project path or TestSprite.

---

## 7. Audit & Hardening (2026-02-15) - CURRENT PRIORITY

**Context:** Pre-market hardening for Sunday 18:00 EST open.
**Status:** Round 1 Independent Audits complete (Gemini, Codex 5.3, Cursor). Claude Pending.

**Achievements:**
1.  **Restored `Properties.cs`**: [UniversalORStrategyV12_002_Dev.Properties.cs](file:///C:/WSGTA/universal-or-strategy/UniversalORStrategyV12_002_Dev.Properties.cs) was missing and has been reconstructed from strategy defaults.
2.  **Consolidated Audit Log**: [CONSOLIDATED_AUDIT.md](file:///C:/WSGTA/universal-or-strategy/CONSOLIDATED_AUDIT.md) contains all critical findings.

**Top Implementation Priorities for the next Project Director:**
1.  **FFMA Sizing Fix**: Guard against division by zero (Close == Low edge case).
2.  **MOMO Order Type**: Switch `StopLimit` to `StopMarket` (or add slippage buffer).
3.  **RETEST Mode Bug**: Fix typo where `isTrendRmaMode` is used instead of `isRetestRmaMode`.
4.  **Properties Verification**: Final spot-check of the reconstructed infrastructure.

*Note: The next session should start with the "Director Spawn Prompt" provided in the chat.*
