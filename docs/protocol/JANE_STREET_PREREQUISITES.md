# Jane Street Sync Prerequisites Installation Guide
**V12 Universal OR Strategy**  
**Last Updated**: 2026-06-03  
**Status**: Required before running Subtask 0.2

---

## Overview

This guide walks through installing the required tools for the Jane Street Knowledge Base sync pipeline.

**Estimated Time**: 15-20 minutes  
**Skill Level**: Intermediate (command-line experience required)

---

## Prerequisites Checklist

- [ ] Mise (tool version manager)
- [ ] Node.js 20+ (via Mise)
- [ ] jCodemunch MCP (npm package)
- [ ] Python 3.12+ (via Mise)
- [ ] Firebase credentials (for Firestore upload)

---

## Step 1: Install Mise (5 minutes)

### Windows (PowerShell)

```powershell
# Install Mise
irm https://mise.jdx.dev/install.ps1 | iex

# Activate in current shell
mise activate powershell | Out-String | Invoke-Expression

# Verify installation
mise --version
# Expected: mise 2024.x.x or later
```

### macOS/Linux (Bash/Zsh)

```bash
# Install Mise
curl https://mise.run | sh

# Add to shell profile (choose one)
echo 'eval "$(mise activate bash)"' >> ~/.bashrc   # Bash
echo 'eval "$(mise activate zsh)"' >> ~/.zshrc    # Zsh

# Reload shell
source ~/.bashrc  # or ~/.zshrc

# Verify installation
mise --version
```

### Troubleshooting

**Issue**: `mise: command not found` after installation  
**Fix**: Restart your terminal or manually activate:
```powershell
# Windows
mise activate powershell | Out-String | Invoke-Expression

# macOS/Linux
eval "$(mise activate bash)"  # or zsh
```

---

## Step 2: Install Core Tools via Mise (5 minutes)

```bash
# Navigate to project root
cd c:/WSGTA/universal-or-strategy

# Install all Mise-managed tools (Python, Node, .NET, Git, etc.)
mise install

# Verify installations
mise run doctor
```

**Expected Output**:
```
Core Runtimes:
Python 3.12.x
node v20.x.x
.NET SDK 8.0.x
git version 2.x.x
```

---

## Step 3: Install jCodemunch MCP (5 minutes)

### Automated Installation (Recommended)

```bash
# Use Mise task (installs via npm)
mise run jcodemunch-install
```

### Manual Installation

```bash
# Install globally via npm
npm install -g jcodemunch-mcp

# Verify installation
jcodemunch --version
# Expected: jcodemunch-mcp v1.x.x
```

### Verify jCodemunch Works

```bash
# Test basic command
jcodemunch list_repos

# Expected output (if no repos indexed yet):
# No repositories indexed yet.
```

### Troubleshooting

**Issue**: `npm: command not found`  
**Fix**: Ensure Node.js is installed via Mise:
```bash
mise install node@20
node --version
npm --version
```

**Issue**: `jcodemunch: command not found` after npm install  
**Fix**: Add npm global bin to PATH:
```powershell
# Windows - Add to PATH
$env:PATH += ";$env:APPDATA\npm"

# macOS/Linux - Add to shell profile
echo 'export PATH="$HOME/.npm-global/bin:$PATH"' >> ~/.bashrc
```

---

## Step 4: Install Python Dependencies (2 minutes)

```bash
# Install Python packages (includes firebase-admin)
pip install -r requirements.txt

# Verify firebase-admin installed
python -c "import firebase_admin; print('Firebase Admin SDK installed')"
```

---

## Step 5: Configure Firebase Credentials (3 minutes)

### Obtain Credentials

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Select your project (or create one)
3. Navigate to: **Project Settings** → **Service Accounts**
4. Click **Generate New Private Key**
5. Save the JSON file as `firebase-credentials.json`

### Place Credentials

```bash
# Copy to project root
cp ~/Downloads/firebase-credentials.json c:/WSGTA/universal-or-strategy/

# Verify file exists
ls firebase-credentials.json
```

### Test Firestore Connection

```bash
# Query existing knowledge base (should return results or "no docs found")
python scripts/query_kb.py "test"
```

**Expected Output** (if KB is empty):
```
No documents found matching query: test
```

**Expected Output** (if KB has data):
```
Found 3 documents matching 'test':
...
```

---

## Step 6: Verify Complete Setup (2 minutes)

Run the comprehensive verification:

```bash
# Full system check
mise run doctor

# Verify Jane Street tasks registered
mise tasks | Select-String "jane-street"
```

**Expected Output**:
```
jane-street-sync              Sync Jane Street knowledge base
jane-street-sync-tier1        Sync Tier 1 repos only
jane-street-sync-tier2        Sync Tier 2 repos only
jane-street-sync-extract      Extract docs to Firestore
jane-street-sync-test         Test sync on 2 small repos
jane-street-sync-test-dry     Test sync without Firestore upload
jane-street-status            Check Jane Street sync status
```

---

## Verification Checklist

Run these commands to confirm everything is installed:

```bash
# 1. Mise
mise --version
# ✅ Should show version 2024.x.x or later

# 2. Node.js
node --version
# ✅ Should show v20.x.x

# 3. Python
python --version
# ✅ Should show 3.12.x

# 4. jCodemunch
jcodemunch --version
# ✅ Should show jcodemunch-mcp v1.x.x

# 5. Firebase Admin SDK
python -c "import firebase_admin; print('OK')"
# ✅ Should print "OK"

# 6. Firebase Credentials
test -f firebase-credentials.json && echo "OK" || echo "MISSING"
# ✅ Should print "OK"

# 7. Mise Tasks
mise tasks | grep jane-street | wc -l
# ✅ Should show 7 (number of jane-street tasks)
```

---

## Common Issues & Solutions

### Issue: Mise not activating automatically

**Symptom**: `mise: command not found` in new terminal sessions

**Solution**: Add to shell profile:
```powershell
# Windows PowerShell - Add to $PROFILE
mise activate powershell | Out-String | Invoke-Expression

# macOS/Linux - Add to ~/.bashrc or ~/.zshrc
eval "$(mise activate bash)"  # or zsh
```

---

### Issue: jCodemunch installation fails

**Symptom**: `npm ERR! code EACCES` or permission errors

**Solution**: Configure npm to use user directory:
```bash
# Create npm global directory
mkdir ~/.npm-global

# Configure npm
npm config set prefix '~/.npm-global'

# Add to PATH (add to shell profile for persistence)
export PATH=~/.npm-global/bin:$PATH

# Retry installation
npm install -g jcodemunch-mcp
```

---

### Issue: Firebase credentials not found

**Symptom**: `Credentials not found at firebase-credentials.json`

**Solution**: Verify file location and permissions:
```bash
# Check file exists in project root
ls -la firebase-credentials.json

# Verify it's valid JSON
python -c "import json; json.load(open('firebase-credentials.json'))"

# Check file permissions (should be readable)
chmod 600 firebase-credentials.json
```

---

### Issue: Python firebase-admin import fails

**Symptom**: `ModuleNotFoundError: No module named 'firebase_admin'`

**Solution**: Reinstall Python dependencies:
```bash
# Ensure using Mise-managed Python
which python
# Should show: ~/.local/share/mise/installs/python/3.12.x/bin/python

# Reinstall dependencies
pip install --force-reinstall -r requirements.txt
```

---

## Next Steps

Once all prerequisites are installed and verified:

1. ✅ **Run Test Sync**: `mise run jane-street-sync-test-dry`
2. ✅ **Review Test Results**: Check `docs/protocol/JANE_STREET_SYNC_TEST_RESULTS.md`
3. ✅ **Proceed to Phase 1**: Full 22-repo sync (if tests pass)

---

## Support

**Documentation**:
- Mise: https://mise.jdx.dev/
- jCodemunch: https://github.com/jcodemunch/jcodemunch-mcp
- Firebase Admin SDK: https://firebase.google.com/docs/admin/setup

**Troubleshooting**:
- Check `docs/protocol/JANE_STREET_SYNC_TEST_RESULTS.md` for known issues
- Review Mise logs: `mise doctor`
- Test individual components before full sync

---

**Last Updated**: 2026-06-03  
**Maintainer**: V12 Infrastructure Team  
**Status**: Required for Subtask 0.2