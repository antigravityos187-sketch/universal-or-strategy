# Bob IDE - Mise Integration Guide
**Date**: 2026-06-03  
**Version**: 1.0.0  
**Status**: ✅ COMPLETE

---

## Overview

This document describes the complete integration between **Bob CLI** (IBM's AI-powered development assistant) and **Mise** (unified tool version manager). This integration ensures Bob CLI can leverage all Mise-managed tools automatically.

---

## Architecture

### Integration Components

```
┌─────────────────────────────────────────────────────────────┐
│                      Bob CLI (v12-engineer)                  │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │          .bob/settings.json                        │    │
│  │  - Mise enabled: true                              │    │
│  │  - Bridge config: .bob/mise.json                   │    │
│  │  - Auto-activate: true                             │    │
│  └────────────────┬───────────────────────────────────┘    │
│                   │                                          │
│                   ▼                                          │
│  ┌────────────────────────────────────────────────────┐    │
│  │          .bob/mise.json (Bridge)                   │    │
│  │  - Environment commands                            │    │
│  │  - Tool paths                                      │    │
│  │  - Quality gates                                   │    │
│  │  - V12 DNA constraints                             │    │
│  └────────────────┬───────────────────────────────────┘    │
└───────────────────┼──────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│                    Mise Tool Manager                         │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │          .mise.toml                                │    │
│  │  - Tool versions (Python 3.12, Node 20, etc.)     │    │
│  │  - Environment variables                           │    │
│  │  - Tasks (validate, format, build, etc.)          │    │
│  │  - Bob environment (BOB_PROJECT_ROOT, etc.)       │    │
│  └────────────────┬───────────────────────────────────┘    │
│                   │                                          │
│                   ▼                                          │
│  ┌────────────────────────────────────────────────────┐    │
│  │     Managed Tools (18 total)                       │    │
│  │  - Python 3.12, Node 20, .NET 8.0                 │    │
│  │  - Git, gh CLI, jq, yq                            │    │
│  │  - CSharpier, Lizard, Semgrep, Snyk              │    │
│  │  - Gitleaks, CodeScene, CodeRabbit               │    │
│  │  - Graphify, jCodemunch, PowerShell 7.x          │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

---

## Installation

### Prerequisites

1. **Mise** installed and activated:
   ```bash
   # Install Mise
   curl https://mise.run | sh
   
   # Activate in shell (add to ~/.bashrc or ~/.zshrc)
   eval "$(mise activate bash)"  # or zsh, fish, etc.
   ```

2. **Bob CLI** installed:
   ```bash
   # Download from IBM (manual binary installation)
   # Place in PATH or use full path
   ```

3. **Project tools** installed:
   ```bash
   # Run Mise setup
   mise run setup
   ```

### Integration Setup

The integration is **pre-configured** in this repository. No manual setup required!

**Files Created**:
- ✅ `.bob/mise.json` - Bridge configuration
- ✅ `.bob/settings.json` - Mise integration enabled
- ✅ `.mise.toml` - Bob environment variables added
- ✅ `.vscode/extensions.json` - Bob IDE extension recommended

---

## Configuration Files

### 1. `.bob/mise.json` (Bridge Configuration)

**Purpose**: Maps Mise commands and tools to Bob CLI

**Key Sections**:

#### Environment Commands
```json
{
  "environment": {
    "setup_command": "mise run setup",
    "validate_command": "mise run validate",
    "format_command": "mise run format",
    "build_command": "mise run build",
    "test_command": "mise run test"
  }
}
```

#### Tool Paths
```json
{
  "tools": {
    "python": {
      "command": "mise which python",
      "version_command": "python --version",
      "expected_version": "3.12"
    },
    "dotnet": {
      "command": "mise which dotnet",
      "version_command": "dotnet --version",
      "expected_version": "8.0"
    }
  }
}
```

#### Quality Gates
```json
{
  "quality_gates": {
    "pre_commit": [
      "mise run format-check",
      "mise run complexity"
    ],
    "pre_push": [
      "mise run validate-fast"
    ],
    "pre_pr": [
      "mise run validate",
      "mise run pr-hygiene"
    ]
  }
}
```

#### V12 DNA Constraints
```json
{
  "v12_dna": {
    "ascii_only": true,
    "lock_free": true,
    "complexity_threshold": 15,
    "diff_limit": 10000
  }
}
```

### 2. `.bob/settings.json` (Bob Configuration)

**Mise Integration Section**:
```json
{
  "mise": {
    "enabled": true,
    "bridge_config": ".bob/mise.json",
    "auto_activate": true,
    "quality_gates": {
      "pre_commit": true,
      "pre_push": true,
      "pre_pr": true
    },
    "tool_paths": {
      "python": "mise which python",
      "node": "mise which node",
      "dotnet": "mise which dotnet",
      "git": "mise which git"
    }
  }
}
```

### 3. `.mise.toml` (Mise Configuration)

**Bob Environment Variables**:
```toml
[env]
# Bob CLI Integration
BOB_PROJECT_ROOT = "{{config_root}}"
BOB_SETTINGS_PATH = "{{config_root}}/.bob/settings.json"
BOB_MODES_PATH = "{{config_root}}/.bob/custom_modes.yaml"
BOB_MISE_BRIDGE = "{{config_root}}/.bob/mise.json"
```

**Bob Task**:
```toml
[tasks.bob-engineer]
description = "Run Bob CLI in v12-engineer mode"
run = "bob --mode v12-engineer"
```

### 4. `.vscode/extensions.json` (VS Code)

**Recommended Extensions**:
```json
{
  "recommendations": [
    "ibm.bob",           // Bob IDE
    "jdx.mise",          // Mise integration
    "csharpier.csharpier-vscode",  // C# formatting
    "ms-dotnettools.csharp"        // C# language support
  ]
}
```

---

## Usage

### Running Bob with Mise Tools

#### Method 1: Direct Bob CLI
```bash
# Bob automatically uses Mise-managed tools
bob --mode v12-engineer
```

#### Method 2: Via Mise Task
```bash
# Shortcut via Mise
mise run bob-engineer
```

#### Method 3: VS Code Integration
1. Open Command Palette (`Ctrl+Shift+P`)
2. Type "Bob: Start Session"
3. Select `v12-engineer` mode
4. Bob automatically detects Mise tools

### Tool Resolution

When Bob needs a tool (e.g., Python), it:

1. **Checks `.bob/mise.json`** for tool path
2. **Runs `mise which python`** to get Mise-managed path
3. **Uses that path** for all Python operations
4. **Validates version** against expected version

**Example Flow**:
```
Bob needs Python
  ↓
Reads .bob/mise.json → "python": { "command": "mise which python" }
  ↓
Executes: mise which python
  ↓
Returns: /home/user/.local/share/mise/installs/python/3.12/bin/python
  ↓
Bob uses this Python for all operations
```

### Quality Gates

Bob automatically runs quality gates at key points:

#### Pre-Commit
```bash
# Bob runs before every commit:
mise run format-check
mise run complexity
```

#### Pre-Push
```bash
# Bob runs before every push:
mise run validate-fast
```

#### Pre-PR
```bash
# Bob runs before creating PR:
mise run validate
mise run pr-hygiene
```

---

## Verification

### 1. Check Integration Status

```bash
# Verify Mise is active
mise doctor

# Verify Bob can find Mise
bob --version
mise --version

# Check tool paths
mise which python
mise which dotnet
mise which git
```

### 2. Test Bob-Mise Integration

```bash
# Run Bob with Mise tools
mise run bob-engineer

# Bob should automatically:
# - Detect Mise-managed tools
# - Use correct versions
# - Run quality gates
```

### 3. Verify Quality Gates

```bash
# Make a test change
echo "// test" >> src/V12_002.cs

# Try to commit (Bob should run format-check)
git add src/V12_002.cs
git commit -m "test"

# Bob should:
# 1. Run mise run format-check
# 2. Run mise run complexity
# 3. Block commit if checks fail
```

---

## Troubleshooting

### Issue: Bob Can't Find Mise Tools

**Symptom**: Bob reports "python not found" or similar

**Solution**:
```bash
# 1. Verify Mise is activated
eval "$(mise activate bash)"

# 2. Verify tools are installed
mise install

# 3. Check tool paths
mise which python
mise which dotnet

# 4. Restart Bob
bob --restart
```

### Issue: Quality Gates Not Running

**Symptom**: Bob doesn't run pre-commit checks

**Solution**:
```bash
# 1. Check .bob/settings.json
cat .bob/settings.json | grep -A 5 "quality_gates"

# 2. Verify quality_gates are enabled
# Should show: "pre_commit": true

# 3. Check .bob/mise.json
cat .bob/mise.json | grep -A 10 "quality_gates"

# 4. Manually test gates
mise run format-check
mise run complexity
```

### Issue: Wrong Tool Version

**Symptom**: Bob uses system Python instead of Mise Python

**Solution**:
```bash
# 1. Check Mise version
mise which python
python --version  # Should match Mise version

# 2. Verify .mise.toml
cat .mise.toml | grep python

# 3. Reinstall if needed
mise install python@3.12

# 4. Clear Bob cache
bob --clear-cache
```

### Issue: Bob IDE Extension Not Working

**Symptom**: VS Code doesn't show Bob features

**Solution**:
1. **Install Extension**:
   - Open Extensions (`Ctrl+Shift+X`)
   - Search "IBM Bob"
   - Click Install

2. **Reload VS Code**:
   - `Ctrl+Shift+P` → "Reload Window"

3. **Check Extension Settings**:
   - `Ctrl+,` → Search "Bob"
   - Verify "Bob: Enabled" is checked

4. **Verify `.vscode/extensions.json`**:
   ```bash
   cat .vscode/extensions.json
   # Should include "ibm.bob"
   ```

---

## Advanced Configuration

### Custom Tool Paths

Add custom tools to `.bob/mise.json`:

```json
{
  "tools": {
    "custom_tool": {
      "command": "mise which custom_tool",
      "version_command": "custom_tool --version",
      "expected_version": "1.0.0"
    }
  }
}
```

### Custom Quality Gates

Add custom gates to `.bob/mise.json`:

```json
{
  "quality_gates": {
    "pre_commit": [
      "mise run format-check",
      "mise run complexity",
      "mise run custom-check"  // Your custom check
    ]
  }
}
```

Then add the task to `.mise.toml`:

```toml
[tasks.custom-check]
description = "Run custom quality check"
run = "python scripts/custom_check.py"
```

### Environment-Specific Configuration

Use Mise profiles for different environments:

```toml
# .mise.toml
[env]
BOB_ENV = "development"

[env._.production]
BOB_ENV = "production"
```

Activate profile:
```bash
mise use --profile production
```

---

## Integration Benefits

### For Bob CLI

✅ **Consistent Tool Versions**: Always uses Mise-managed versions  
✅ **Automatic Tool Discovery**: No manual PATH configuration  
✅ **Quality Gate Enforcement**: Automatic pre-commit/push checks  
✅ **V12 DNA Compliance**: Built-in constraint validation  
✅ **Cross-Platform**: Works on Windows, macOS, Linux

### For Developers

✅ **One-Command Setup**: `mise run setup` installs everything  
✅ **Reproducible Environment**: Same tools across all machines  
✅ **Fast Tool Switching**: Mise handles version management  
✅ **IDE Integration**: VS Code extensions auto-configured  
✅ **Documentation**: All tools documented in one place

### For V12 Project

✅ **Enforced Standards**: CYC ≤ 15, ASCII-only, lock-free  
✅ **Automated Validation**: 13 quality checks before push  
✅ **Tool Consistency**: All agents use same tool versions  
✅ **Audit Trail**: Tool versions tracked in `.mise.toml`  
✅ **Zero Configuration**: Works out of the box

---

## Maintenance

### Updating Tool Versions

```bash
# Update a specific tool
mise use python@3.13

# Update all tools
mise upgrade

# Verify updates
mise run doctor
```

### Adding New Tools

1. **Add to `.mise.toml`**:
   ```toml
   [tools]
   new_tool = "latest"
   ```

2. **Add to `.bob/mise.json`**:
   ```json
   {
     "tools": {
       "new_tool": {
         "command": "mise which new_tool",
         "version_command": "new_tool --version",
         "expected_version": "latest"
       }
     }
   }
   ```

3. **Install**:
   ```bash
   mise install
   ```

4. **Verify**:
   ```bash
   mise run doctor
   ```

### Removing Tools

1. **Remove from `.mise.toml`**
2. **Remove from `.bob/mise.json`**
3. **Uninstall**:
   ```bash
   mise uninstall tool_name
   ```

---

## References

- **Mise Documentation**: https://mise.jdx.dev/
- **Bob CLI Documentation**: (IBM internal)
- **V12 Tool Audit**: [`docs/protocol/MISE_TOOL_AUDIT.md`](./MISE_TOOL_AUDIT.md)
- **Mise Implementation**: [`docs/protocol/MISE_IMPLEMENTATION_SUMMARY.md`](./MISE_IMPLEMENTATION_SUMMARY.md)
- **AGENTS.md**: [`AGENTS.md`](../../AGENTS.md)

---

## Changelog

### Version 1.0.0 (2026-06-03)
- ✅ Initial integration complete
- ✅ `.bob/mise.json` bridge created
- ✅ `.bob/settings.json` updated with Mise section
- ✅ `.mise.toml` updated with Bob environment variables
- ✅ `.vscode/extensions.json` created with Bob IDE recommendation
- ✅ `mise run bob-engineer` task added
- ✅ Documentation complete

---

**Status**: ✅ **PRODUCTION READY**  
**Last Updated**: 2026-06-03  
**Maintained By**: V12 Infrastructure Team