# Mise Tool Management Setup

**V12 Photon Kernel Infrastructure Layer**  
**Last Updated**: 2026-06-03  
**Status**: Production Ready

## Overview

Mise is a polyglot tool version manager that replaces the need for multiple version managers (nvm, pyenv, rbenv, etc.). The Universal OR Strategy project uses Mise to manage all development tools in a single, declarative configuration.

## Why Mise?

- **Single Source of Truth**: All tool versions defined in `.mise.toml`
- **Automatic Activation**: Tools available when you `cd` into the project
- **Task Runner**: Built-in task system replaces Make/npm scripts
- **Cross-Platform**: Works on Windows, macOS, and Linux
- **Fast**: Written in Rust, minimal overhead

## Installation

### Windows

```powershell
# Install Mise
irm https://mise.jdx.dev/install.ps1 | iex

# Verify installation
mise --version

# Add to PowerShell profile (automatic activation)
mise activate pwsh | Out-String | Invoke-Expression
```

### macOS

```bash
# Install via Homebrew
brew install mise

# Or via curl
curl https://mise.run | sh

# Add to shell profile
echo 'eval "$(mise activate bash)"' >> ~/.bashrc
# OR for zsh
echo 'eval "$(mise activate zsh)"' >> ~/.zshrc
```

### Linux

```bash
# Install via curl
curl https://mise.run | sh

# Add to shell profile
echo 'eval "$(mise activate bash)"' >> ~/.bashrc
```

## First-Time Setup

After installing Mise, run the complete project setup:

```powershell
# Navigate to project root
cd c:/WSGTA/universal-or-strategy

# Install all mise-managed tools
mise install

# Run complete project setup (tools + dependencies)
mise run setup
```

This will:
1. Install Python 3.12, Node 20, .NET 8.0, Git, GitHub CLI, jq, yq
2. Restore .NET tools (CSharpier, dotnet-format)
3. Install Python packages (Lizard, Semgrep, requests)
4. Install Node packages (Snyk)

## Manual Tool Installation

Some tools require manual installation (not yet available via Mise plugins):

### PowerShell 7.x

**Windows**:
```powershell
winget install Microsoft.PowerShell
```

**macOS**:
```bash
brew install powershell
```

### Gitleaks (Secrets Detection)

**Windows**:
```powershell
winget install gitleaks
```

**macOS**:
```bash
brew install gitleaks
```

### CodeScene CLI

Download from: https://codescene.com/docs/tools/command-line-tool.html

### CodeRabbit CLI

```bash
curl -fsSL https://cli.coderabbit.ai/install.sh | sh
```

### Bob CLI (IBM)

Manual installation required. Contact IBM for binary.

### Graphify (Optional)

```bash
npm install -g @modelcontextprotocol/graphify
```

## Daily Workflow

### Automatic Activation

When you `cd` into the project directory, Mise automatically activates and makes all tools available:

```powershell
cd c:/WSGTA/universal-or-strategy
# Tools are now available: python, node, dotnet, gh, jq, yq
```

### Common Tasks

```powershell
# Format code
mise run format

# Build project
mise run build

# Run tests
mise run test

# Full validation (pre-push checks)
mise run validate

# Fast validation (skip slow checks)
mise run validate-fast

# Security scans
mise run security

# Complexity audit
mise run complexity

# Sync NinjaTrader hard links
mise run sync

# Development workflow (format + build + test)
mise run dev

# CI pipeline locally
mise run ci
```

### Task Aliases

Short aliases for common tasks:

```powershell
mise run v    # validate-fast
mise run f    # format
mise run b    # build
mise run t    # test
mise run s    # sync
```

## Tool Version Management

### Check Current Versions

```powershell
# List all installed tools
mise list

# Check specific tool version
python --version
node --version
dotnet --version
```

### Update Tools

```powershell
# Update all tools to latest versions
mise upgrade

# Update specific tool
mise upgrade python
```

### Pin Specific Versions

Edit `.mise.toml` to pin versions:

```toml
[tools]
python = "3.12.3"  # Pin to exact version
node = "20"        # Pin to major version (latest 20.x)
```

## VS Code Integration

Mise integrates with VS Code for seamless development:

### Settings

Add to `.vscode/settings.json`:

```json
{
  "terminal.integrated.env.windows": {
    "MISE_SHELL": "pwsh"
  },
  "mise.enable": true
}
```

### Terminal

VS Code terminals automatically activate Mise when opened in the project directory.

## CI/CD Integration

### GitHub Actions

```yaml
name: CI

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Mise
        uses: jdx/mise-action@v2
      
      - name: Run validation
        run: mise run validate
```

### Local CI Simulation

```powershell
# Run full CI pipeline locally
mise run ci
```

## Pre-Commit Hooks

Mise can run tasks as pre-commit hooks:

```bash
# Create hook file
mkdir -p .mise/hooks
cat > .mise/hooks/pre-commit << 'EOF'
#!/bin/bash
mise run format-check
mise run validate-fast
EOF

chmod +x .mise/hooks/pre-commit
```

## Troubleshooting

### Tools Not Found

```powershell
# Verify Mise is activated
mise doctor

# Reinstall tools
mise install --force
```

### Version Conflicts

```powershell
# Check which version is active
mise current

# Clear cache and reinstall
mise cache clear
mise install
```

### PowerShell Profile Issues

If tools aren't available in new terminals:

```powershell
# Check if Mise is in profile
Get-Content $PROFILE

# Add Mise activation if missing
Add-Content $PROFILE 'mise activate pwsh | Out-String | Invoke-Expression'
```

### Windows PATH Issues

Mise adds tools to PATH automatically. If tools aren't found:

```powershell
# Verify Mise shims directory is in PATH
$env:PATH -split ';' | Select-String mise

# Manually add if missing (restart terminal after)
[Environment]::SetEnvironmentVariable(
    "Path",
    "$env:USERPROFILE\.local\share\mise\shims;$env:Path",
    "User"
)
```

## Environment Variables

Mise sets project-specific environment variables (defined in `.mise.toml`):

```powershell
# Check active environment
mise env

# Variables set by V12:
# - DOTNET_CLI_TELEMETRY_OPTOUT=1
# - PYTHONUNBUFFERED=1
# - SNYK_DISABLE_ANALYTICS=1
# - PROJECT_ROOT=<project-path>
# - SCRIPTS_DIR=<project-path>/scripts
# - V12_MISE_MANAGED=true
```

## Advanced Usage

### Custom Tasks

Add custom tasks to `.mise.toml`:

```toml
[tasks.my-task]
description = "My custom task"
run = """
echo "Running custom task"
pwsh -File ./scripts/my-script.ps1
"""
```

### Task Dependencies

```toml
[tasks.deploy]
depends = ["format", "build", "test"]
run = "pwsh -File ./deploy-sync.ps1"
```

### Conditional Tasks

```toml
[tasks.windows-only]
run = "echo 'Windows task'"
condition = "os == 'windows'"
```

## Bob CLI Integration

Bob CLI automatically uses Mise-managed tools:

```json
// .bob/settings.json
{
  "environment": {
    "setup_command": "mise run setup",
    "validate_command": "mise run validate"
  }
}
```

## Migration from Other Tools

### From nvm/pyenv/rbenv

Mise replaces all version managers:

```powershell
# Before (multiple tools)
nvm use 20
pyenv local 3.12

# After (single tool)
cd project  # Mise auto-activates
```

### From Make/npm scripts

Mise tasks replace Makefiles and package.json scripts:

```powershell
# Before
make build
npm run test

# After
mise run build
mise run test
```

## Best Practices

1. **Always use `mise run` for tasks**: Ensures consistent environment
2. **Pin major versions**: Use `python = "3.12"` not `"latest"`
3. **Document custom tasks**: Add descriptions to all tasks
4. **Test locally before CI**: Run `mise run ci` before pushing
5. **Keep `.mise.toml` in version control**: Team shares same config

## V12 DNA Alignment

Mise enforces V12 architectural principles:

- **Correctness by Construction**: Tool versions pinned, no surprises
- **Zero Ambiguity**: Single source of truth for all tools
- **Reproducible Builds**: Same tools, same versions, everywhere
- **Fast Feedback**: Local validation matches CI exactly

## Resources

- **Mise Documentation**: https://mise.jdx.dev
- **GitHub**: https://github.com/jdx/mise
- **Discord**: https://discord.gg/mise

## Support

For V12-specific Mise issues:
1. Check `mise doctor` output
2. Review this document
3. Ask in project Discord/Slack
4. File issue with `mise run doctor` output attached