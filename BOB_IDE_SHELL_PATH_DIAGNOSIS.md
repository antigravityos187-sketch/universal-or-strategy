# Bob IDE Shell PATH Diagnosis

## Problem Statement

Bob IDE's `execute_command` tool cannot find basic Unix commands like `ls`, `bash`, `python3`, etc.

## Root Cause Analysis

### What We Know

1. **Shell Used**: Bob IDE's `execute_command` uses `/bin/sh`
2. **Error Pattern**: All commands return "command not found"
3. **Commands Affected**: `ls`, `bash`, `python3`, `find`, `grep`, `wc`, `seq`, `which`, `cat`, `head`, `tail`

### Why This Happens

Bob IDE's `execute_command` tool spawns a **non-interactive, non-login shell** (`/bin/sh`), which means:

1. **Does NOT source** `~/.bashrc` (only for interactive shells)
2. **Does NOT source** `~/.bash_profile` or `~/.profile` (only for login shells)
3. **Does NOT inherit** the PATH from your terminal session
4. **Uses minimal PATH**: Likely just `/bin` or even empty

### The PATH Problem

Standard Unix commands are located in:
- `/usr/local/sbin`
- `/usr/local/bin`
- `/usr/sbin`
- `/usr/bin`
- `/sbin`
- `/bin`

But `/bin/sh` in non-interactive mode doesn't have these in its PATH.

## Solutions (Ranked by Permanence)

### Solution 1: Fix Bob IDE Configuration (BEST)

Bob IDE likely has a configuration file that controls the shell environment for `execute_command`.

**Files to Check**:
```
~/.bob/settings.json
~/.bob/config.json
.bob/settings.json (workspace-level)
bob.config.yaml (workspace-level)
```

**What to Add**:
```json
{
  "terminal.integrated.env.linux": {
    "PATH": "/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin"
  }
}
```

OR if Bob IDE uses VSCode-style settings:
```json
{
  "terminal.integrated.shellArgs.linux": ["-l"]
}
```

The `-l` flag makes the shell a login shell, which sources `~/.profile`.

### Solution 2: System-Wide Environment (GOOD)

Set PATH in `/etc/environment` (affects ALL processes):

```bash
# Add to /etc/environment
PATH="/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin"
```

**Pros**: Works for all non-interactive shells
**Cons**: Requires root access, system-wide change

### Solution 3: Shell Wrapper Script (WORKAROUND)

Create a wrapper script that sets PATH before executing commands:

```bash
# Create /usr/local/bin/bob-shell-wrapper
#!/bin/sh
export PATH="/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin"
exec "$@"
```

Then configure Bob IDE to use this wrapper instead of `/bin/sh`.

### Solution 4: ENV File for sh (PARTIAL)

The POSIX shell (`/bin/sh`) respects the `ENV` environment variable:

```bash
# Create ~/.shrc
export PATH="/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin"

# Set ENV in Bob IDE config
ENV="$HOME/.shrc"
```

**Problem**: Bob IDE needs to set `ENV` before spawning the shell.

### Solution 5: Symlink Commands to /bin (HACKY)

```bash
# Symlink all commands to /bin (where /bin/sh can find them)
ln -s /usr/bin/ls /bin/ls
ln -s /usr/bin/python3 /bin/python3
# ... etc for all commands
```

**Pros**: Works immediately
**Cons**: Messy, doesn't scale, may break system

## Recommended Action Plan

### Step 1: Check Bob IDE Configuration

Look for Bob IDE's configuration files:

```bash
# Check for Bob IDE config
ls -la ~/.bob/
ls -la .bob/
cat bob.config.yaml 2>/dev/null
cat .bob/settings.json 2>/dev/null
```

### Step 2: Check Current Shell Behavior

Test what `/bin/sh` sees:

```bash
# In a regular terminal (not Bob IDE):
/bin/sh -c 'echo $PATH'
/bin/sh -c 'which ls'
```

If this returns empty PATH or "command not found", the system's `/bin/sh` is broken.

### Step 3: Verify System PATH

```bash
# Check system-wide environment
cat /etc/environment
cat /etc/profile
```

### Step 4: Test ENV Variable

```bash
# Create test ENV file
echo 'export PATH="/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin"' > ~/.shrc

# Test if sh respects it
ENV=~/.shrc /bin/sh -c 'echo $PATH'
ENV=~/.shrc /bin/sh -c 'which ls'
```

## What to Do Next

1. **Check if Bob IDE has a config file** that controls shell environment
2. **Test `/bin/sh` behavior** in a regular terminal
3. **Verify system PATH** is set correctly
4. **Try ENV variable approach** if Bob IDE supports it

## Questions to Answer

1. Does Bob IDE have a configuration file for shell environment?
2. What does `/bin/sh -c 'echo $PATH'` return in a regular terminal?
3. Is `/etc/environment` properly configured?
4. Can we set `ENV` variable for Bob IDE's shell spawning?

## Temporary Workaround

Until we fix the root cause, use absolute paths in commands:

```bash
# Instead of: ls -la
/usr/bin/ls -la

# Instead of: python3 script.py
/usr/bin/python3 script.py

# Instead of: find . -name "*.md"
/usr/bin/find . -name "*.md"
```

This is ugly but will work immediately.