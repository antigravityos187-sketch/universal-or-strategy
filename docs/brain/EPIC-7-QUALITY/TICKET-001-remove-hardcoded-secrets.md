# [EPIC-7-QUALITY-001] Security: Remove Hardcoded Secrets and Rotate Tokens

## Priority: P0 CRITICAL

## Labels
`security`, `P0`, `epic-7-quality`, `technical-debt`

## Summary
36 hardcoded secrets detected across 9 files requiring immediate token rotation and migration to environment variables.

## Affected Files
- `.mcp.json` (API tokens)
- `.bob/mcp.json` (API tokens)
- `docs/brain/EPIC-4-STICKY-STATE-IPC/` (Bearer tokens in documentation)
- Configuration files with embedded credentials

## Security Impact
- **Severity:** CRITICAL
- **Risk:** Exposed API tokens in version control
- **Compliance:** Violates V12 DNA security protocols

## Required Actions
1. **Immediate:** Rotate all exposed API tokens
   - Anthropic API keys
   - GitHub tokens
   - MCP server credentials
2. **Migration:** Move all secrets to `.env` (use `.env.example` as template)
3. **Cleanup:** Remove hardcoded secrets from all files
4. **Verification:** Run Gitleaks scan to confirm no secrets remain
5. **Documentation:** Update setup guides to reference environment variables

## Acceptance Criteria
- [ ] All API tokens rotated
- [ ] Secrets moved to `.env` (gitignored)
- [ ] `.env.example` updated with placeholder values
- [ ] Gitleaks scan passes (0 secrets detected)
- [ ] Documentation updated
- [ ] No hardcoded secrets in any committed files

## Effort Estimate
Medium (8-12 hours)

## References
- Audit: `docs/brain/DEFERRED_WORK_AUDIT.md` (Lines 45-89)
- PRs affected: #1, #2, #6, #8
- V12 DNA: Security-first architecture

## Status
🔴 **OPEN** - Not Started

## Assigned To
_Unassigned_

## Created
2026-05-24T04:13:00Z