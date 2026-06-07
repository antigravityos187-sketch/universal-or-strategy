# Secrets Audit Report - EPIC-7-QUALITY-001

**Date:** 2026-05-24T04:23:00Z  
**Severity:** P0 CRITICAL  
**Status:** IN PROGRESS

## Executive Summary

Gitleaks scan detected **14 secrets** across the repository:
- **4 CRITICAL secrets in active files** (require immediate action)
- **10 test fixture secrets in infrastructure/paperclip** (false positives - can be excluded)

### Critical Findings

#### 1. `.env` File Committed to Git History (P0 CRITICAL)
**Commit:** 75b4e911ef2f4cfd40d99b6b9d5bbe5bf3e490bf  
**Date:** 2026-05-18T23:19:50Z

**Exposed Secrets:**
- `LANGSMITH_API_KEY`: `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`
- `CONTEXT7_API_KEY`: `ctx7sk-faca8e7b-48b4-4f7c-b7ca-ccc9424bef17`
- `PINECONE_API_KEY`: `pcsk_rciXF_PkmYQ3exL4pgKwJtthYR1gHyyLgJjx8RefAm53H6sgtWDjF4hoBXSH8E44PTFED`
- `BRAINTRUST_API_KEY`: `sk-XucAZsl9Qu6ru3eQiURKaghIXGj1QEKs7z5cBlkUyvGlolXG`

**Impact:** All 4 API keys are now permanently exposed in git history and must be rotated immediately.

#### 2. `.bob/mcp.json` - Greptile Bearer Token (P0 CRITICAL)
**File:** `.bob/mcp.json:7`  
**Secret:** `Bearer vob20OZM949/QgQ/IPxtzrU7lJDMGEFuFvwQ8D0UxO3lJ2CG`

**Status:** Currently active in working directory. Must be migrated to environment variable.

#### 3. Documentation Files - Greptile Bearer Token (P0 CRITICAL)
**File:** `docs/brain/REAPER-EXPANSION/GREPTILE-FIX-SUMMARY.md:41`  
**Secret:** `Bearer GKZ5piB2DLIr22NtSOF/afDCpA6MT3YiAjpsEkbI6Fx88DK9`

**Status:** Documentation example showing hardcoded token. Must be redacted.

#### 4. Documentation Files - User Path PII (P2 MEDIUM)
**File:** `docs/brain/REAPER-EXPANSION/RESUME-PROMPT.md:17`  
**Path:** `C:\Users\Mohammed Khalid\AppData\Roaming\Code\User\mcp.json`

**Status:** Leaks developer username. Should use placeholder like `%APPDATA%\Code\User\mcp.json`.

### Test Fixture Secrets (False Positives)

The following secrets are in `infrastructure/paperclip/` test files and are **intentional test fixtures**:
- `infrastructure/paperclip/docs/deploy/secrets.md:394` - UUID test fixture
- `infrastructure/paperclip/server/src/__tests__/redaction.test.ts:68` - JWT test fixture
- `infrastructure/paperclip/server/src/__tests__/heartbeat-active-run-output-watchdog.test.ts:267` - JWT test fixture
- `infrastructure/paperclip/server/src/__tests__/secrets-service.test.ts:1107,1223` - AWS ARN test fixtures

**Recommendation:** Add `infrastructure/paperclip/` to `.gitleaks.toml` allowlist.

## Migration Plan

### Phase 1: Immediate Token Rotation (USER ACTION REQUIRED)

**The following tokens MUST be rotated by the user:**

1. **LangSmith API Key**
   - Service: https://smith.langchain.com/
   - Action: Generate new API key
   - Environment Variable: `LANGSMITH_API_KEY`

2. **Context7 API Key**
   - Service: https://context7.com/
   - Action: Revoke `ctx7sk-faca8e7b-48b4-4f7c-b7ca-ccc9424bef17` and generate new key
   - Environment Variable: `CONTEXT7_API_KEY`

3. **Pinecone API Key**
   - Service: https://www.pinecone.io/
   - Action: Revoke `pcsk_rciXF_...` and generate new key
   - Environment Variable: `PINECONE_API_KEY`

4. **Braintrust API Key**
   - Service: https://www.braintrustdata.com/
   - Action: Revoke `sk-XucAZsl9...` and generate new key
   - Environment Variable: `BRAINTRUST_API_KEY`

5. **Greptile Bearer Token**
   - Service: https://greptile.com/
   - Action: Revoke both exposed tokens:
     - `vob20OZM949/QgQ/IPxtzrU7lJDMGEFuFvwQ8D0UxO3lJ2CG`
     - `GKZ5piB2DLIr22NtSOF/afDCpA6MT3YiAjpsEkbI6Fx88DK9`
   - Environment Variable: `GREPTILE_API_KEY`

### Phase 2: Code Migration (AUTOMATED)

1. **Update `.bob/mcp.json`**
   - Replace hardcoded Bearer token with environment variable reference
   - Add support for reading from `GREPTILE_API_KEY`

2. **Redact Documentation Files**
   - Replace hardcoded tokens with `<REDACTED>` or `YOUR_TOKEN_HERE`
   - Replace user-specific paths with placeholders

3. **Delete `.env` from Working Directory**
   - File is already gitignored
   - User will create new `.env` from `.env.example` with rotated keys

4. **Update `.env.example`**
   - Add all required environment variables with placeholder values
   - Add clear comments explaining each variable

5. **Update `.gitleaks.toml`**
   - Add allowlist for `infrastructure/paperclip/` test fixtures

### Phase 3: Documentation Updates

1. **Create Setup Guide**
   - Document environment variable setup process
   - Provide instructions for creating `.env` from `.env.example`
   - List all required API keys and where to obtain them

2. **Update Existing Documentation**
   - Update any references to hardcoded secrets
   - Add security best practices section

### Phase 4: Verification

1. **Run Gitleaks Scan**
   - Target: 0 secrets in active files (excluding test fixtures)
   - Verify allowlist works correctly

2. **Build Verification**
   - Ensure applications can read from environment variables
   - Test MCP server connections with new tokens

## Security Recommendations

1. **Git History Cleanup** (Optional but Recommended)
   - Consider using `git filter-repo` or BFG Repo-Cleaner to remove `.env` from history
   - This requires force-push and coordination with all collaborators

2. **Pre-commit Hooks**
   - Install Gitleaks pre-commit hook to prevent future secret commits
   - Add to `.git/hooks/pre-commit`

3. **Secrets Management**
   - Consider using a secrets manager (AWS Secrets Manager, Azure Key Vault, etc.) for production
   - Use GitHub Actions Secrets for CI/CD workflows

4. **Regular Audits**
   - Run Gitleaks scan monthly
   - Review `.env.example` to ensure it stays in sync with actual requirements

## Next Steps

1. **User:** Rotate all exposed API tokens (Phase 1)
2. **Agent:** Migrate code to use environment variables (Phase 2)
3. **Agent:** Update documentation (Phase 3)
4. **Agent:** Run verification (Phase 4)
5. **User:** Test with new tokens and verify build

---

**Report Generated:** 2026-05-24T04:23:00Z  
**Tool:** Gitleaks v8.x  
**Scan Coverage:** 158 commits, ~102.78 MB