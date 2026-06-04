# PR-Agent Installation Guide

**Status**: Configuration ready, app installation pending  
**Date**: 2026-06-03

---

## What is PR-Agent?

PR-Agent is an open-source AI-powered code review bot that automatically reviews pull requests. We've configured it with GODMODE Jane Street rules enforcement.

**Configuration File**: `.pr_agent.toml` (✅ Already configured)

---

## Installation Steps

### Option 1: GitHub Marketplace (Recommended - FREE)

1. **Go to GitHub Marketplace**:
   - Visit: https://github.com/marketplace/actions/the-pr-agent
   - Or navigate: GitHub → Marketplace → Search "PR-Agent"

2. **Install the App**:
   - Click "Use latest version" (green button)
   - Select "Only select repositories"
   - Choose: `antigravityos187-sketch/universal-or-strategy`
   - Click "Install"

3. **Verify Installation**:
   - Go to: https://github.com/antigravityos187-sketch/universal-or-strategy/settings/installations
   - You should see "The PR Agent" in the list

4. **Test on Next PR**:
   - Create any PR (e.g., PR #25 for EPIC-CCN-13)
   - Wait 2-3 minutes
   - PR-Agent will comment with:
     - Code review
     - Suggestions
     - Jane Street rule violations (labeled `[CRITICAL-JS-P0/P1/P2]`)

---

### Option 2: Self-Hosted (Advanced)

If you prefer to run PR-Agent locally or in your own CI/CD:

1. **Clone Repository**:
   ```bash
   git clone https://github.com/Codium-ai/pr-agent.git
   cd pr-agent
   ```

2. **Install Dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

3. **Configure API Keys**:
   ```bash
   # Create .env file
   OPENAI_API_KEY=your_key_here
   # OR
   ANTHROPIC_API_KEY=your_key_here
   ```

4. **Run on PR**:
   ```bash
   python pr_agent/cli.py --pr_url=https://github.com/antigravityos187-sketch/universal-or-strategy/pull/25
   ```

**Note**: Self-hosted requires API costs (OpenAI/Anthropic). GitHub Marketplace version is FREE for open-source.

---

## Current Configuration (GODMODE)

Our `.pr_agent.toml` is configured with:

### Enforcement Levels
- **P0 (CRITICAL)**: `[CRITICAL-JS-P0]` - BLOCKS MERGE
- **P1 (HIGH)**: `[CRITICAL-JS-P1]` - BLOCKS MERGE (upgraded from warning)
- **P2 (MEDIUM)**: `[CRITICAL-JS-P2]` - BLOCKS MERGE (upgraded from info)

### Key Rules Enforced
- JS-001: Result<T,E> instead of exceptions
- JS-002: Option<T> instead of null
- JS-021: NO lock() - use Actor/FSM
- JS-036: Span<T> for zero-allocation
- JS-042: Named constants (no magic numbers)
- JS-067: **Complexity ≤8** (GODMODE: Jane Street strict)
- JS-070: ASCII-only strings

### Review Scope
- **Included**: All `.cs` files in `src/`
- **Excluded**: `docs/`, `.github/`, `.bob/`, `.codex/`, `Traycerrefactor/`

---

## Verification Checklist

After installation, verify PR-Agent is working:

- [ ] App appears in: https://github.com/antigravityos187-sketch/universal-or-strategy/settings/installations
- [ ] Create test PR (or use PR #25)
- [ ] Wait 2-3 minutes for bot analysis
- [ ] Check for PR-Agent comment with:
  - [ ] Code review summary
  - [ ] Suggestions for improvements
  - [ ] Jane Street rule violations (if any)
  - [ ] Severity labels: `[CRITICAL-JS-P0]`, `[CRITICAL-JS-P1]`, `[CRITICAL-JS-P2]`

---

## Integration with Other Bots

PR-Agent works alongside:
- ✅ **CodeRabbit** (already installed)
- ✅ **Codacy** (already installed)
- ✅ **CodeScene** (already installed)
- ✅ **SonarCloud** (already installed)

All bots are configured with GODMODE Jane Street rules.

---

## Troubleshooting

### Bot Not Commenting

**Possible Causes**:
1. App not installed (check settings/installations)
2. PR too small (no code changes)
3. All files excluded by `.pr_agent.toml` ignore rules

**Solution**:
- Verify installation
- Check PR has `.cs` file changes in `src/`
- Wait 5 minutes (bot may be slow)

### Wrong Severity Labels

**Expected**: `[CRITICAL-JS-P0]`, `[CRITICAL-JS-P1]`, `[CRITICAL-JS-P2]`  
**If seeing**: `[WARNING-JS-P1]`, `[INFO-JS-P2]`

**Solution**:
- Verify `.pr_agent.toml` has GODMODE configuration
- Re-trigger bot: Close and reopen PR
- Check bot version (should be v0.36.0+)

### API Rate Limits

**Symptom**: Bot stops commenting after several PRs

**Solution**:
- GitHub Marketplace version has generous limits
- If hitting limits, consider self-hosted with your own API keys

---

## Cost

### GitHub Marketplace (Recommended)
- **FREE** for open-source repositories
- **Paid** for private repos (check marketplace pricing)

### Self-Hosted
- **FREE** software (open-source)
- **API Costs**: $0.01-0.10 per PR (OpenAI/Anthropic)

---

## Next Steps

1. **Install PR-Agent** (Option 1 recommended)
2. **Test on PR #25** (EPIC-CCN-13)
3. **Verify GODMODE labels** appear
4. **Continue with EPIC-CCN-14** (next hotspot)

---

## References

- **PR-Agent GitHub**: https://github.com/Codium-ai/pr-agent
- **Marketplace**: https://github.com/marketplace/actions/the-pr-agent
- **Documentation**: https://pr-agent-docs.codium.ai/
- **Our Config**: `.pr_agent.toml`
- **GODMODE Docs**: `docs/standards/GODMODE.md`

---

*Part of V12 GODMODE Configuration - Jane Street Cyborg Transformation*