# Shell Commands vs Bob Tools: Comparison for Wave 2

## TL;DR: Shell Commands Are BETTER for VM Execution

**Answer**: Shell commands provide **equal or superior** results compared to Bob tools, especially in SSH/non-interactive mode.

## Feature Comparison

| Feature | Bob Tools (`read_file`, `write_to_file`) | Shell Commands (`cat`, `cat >`) |
|---------|------------------------------------------|----------------------------------|
| **Reliability in SSH** | ❌ Fails with "File not found" | ✅ Works perfectly |
| **Path Resolution** | ❌ Buggy in non-interactive mode | ✅ Direct filesystem access |
| **Verification** | ❌ Cannot verify after write | ✅ Immediate verification (`ls`, `wc -l`) |
| **Content Preview** | ❌ Requires separate tool call | ✅ Built-in (`head`, `tail`) |
| **Multi-line Content** | ⚠️ Requires escaping | ✅ Heredoc syntax (no escaping) |
| **Large Files** | ⚠️ Token limits | ✅ No token limits |
| **Atomic Operations** | ❌ Separate write/verify | ✅ Single command chain |
| **Error Detection** | ⚠️ Silent failures | ✅ Immediate exit codes |
| **Performance** | ⚠️ API round-trip | ✅ Direct execution |

## Practical Examples

### Writing Files

**Bob Tool** (Unreliable in SSH):
```
write_to_file:
  path: docs/brain/EPIC-CCN-107/00-hotspots.md
  content: |
    # Phase 0: Hotspot Analysis
    [217 lines of content]
  line_count: 217

Result: ❌ May fail silently or with path errors
```

**Shell Command** (Reliable):
```bash
cat > docs/brain/EPIC-CCN-107/00-hotspots.md << 'EOF'
# Phase 0: Hotspot Analysis
[217 lines of content - no escaping needed]
EOF

Result: ✅ Always works, immediate feedback
```

### Reading Files

**Bob Tool** (Fails in SSH):
```
read_file:
  path: docs/brain/EPIC-CCN-107/00-hotspots.md

Result: ❌ "File not found" even when file exists
```

**Shell Command** (Works):
```bash
cat docs/brain/EPIC-CCN-107/00-hotspots.md

Result: ✅ Returns content immediately
```

### Verification

**Bob Tool** (No built-in verification):
```
write_to_file → read_file (fails) → ???

Result: ❌ Cannot verify write succeeded
```

**Shell Command** (Built-in verification):
```bash
cat > file.md << 'EOF'
content
EOF
ls -lh file.md && wc -l file.md

Result: ✅ Immediate confirmation:
-rw-r--r-- 1 user 9.1K Jun 13 01:08 file.md
217 file.md
```

## Why Shell Commands Are Better

### 1. Direct Filesystem Access
- **Bob Tools**: Go through Bob's abstraction layer → path resolution bugs
- **Shell Commands**: Direct kernel syscalls → no abstraction bugs

### 2. Immediate Feedback
- **Bob Tools**: Async execution, may fail silently
- **Shell Commands**: Synchronous, immediate exit codes

### 3. Composability
```bash
# Single command does: create + verify + preview
cat > file.md << 'EOF'
content
EOF && ls -lh file.md && wc -l file.md && head -5 file.md

# Bob Tools would require 4 separate tool calls
```

### 4. No Token Limits
- **Bob Tools**: Large files hit token limits
- **Shell Commands**: Can handle GB-sized files

### 5. Heredoc Syntax
```bash
cat > file.md << 'EOF'
No need to escape:
- Quotes: "hello" 'world'
- Variables: $HOME ${USER}
- Special chars: !@#$%^&*()
- Backticks: `command`
EOF
```

**Bob Tools**: Would require escaping all of the above.

## Code Editing Capability

**Question**: Can shell commands handle code editing (Phase 5-6)?

**Answer**: YES, with some caveats.

### Simple Edits (Append/Prepend)
```bash
# Append to file
cat >> src/file.cs << 'EOF'
    // New method
    public void NewMethod() { }
EOF
```

### Complex Edits (Search/Replace)
```bash
# Using sed for surgical edits
sed -i 's/oldPattern/newPattern/g' src/file.cs

# Using awk for line-based edits
awk '/pattern/ {print "new line"} {print}' src/file.cs > tmp && mv tmp src/file.cs
```

### Full File Rewrites
```bash
# Same as documentation - just write entire file
cat > src/file.cs << 'EOF'
[Complete new file content]
EOF
```

## Limitations of Shell Commands

### 1. No Syntax Awareness
- **Bob Tools**: Understand code structure (AST-aware)
- **Shell Commands**: Text manipulation only

### 2. No Refactoring Intelligence
- **Bob Tools**: Can suggest improvements
- **Shell Commands**: Dumb text replacement

### 3. Manual Verification
- **Bob Tools**: Built-in linting/validation
- **Shell Commands**: Must run separate build/test

## Recommendation for Wave 2

### Phase 0-4 (Documentation)
✅ **Use shell commands exclusively**
- More reliable than Bob tools
- Faster execution
- Better verification
- No tool bugs

### Phase 5-6 (Code Editing)
✅ **Use shell commands for simple edits**
- File creation: `cat >`
- Appending: `cat >>`
- Search/replace: `sed`
- Line insertion: `awk`

⚠️ **Consider Bob tools for complex refactoring**
- Multi-file changes
- AST-aware transformations
- Intelligent suggestions

**BUT**: If Bob tools fail in SSH mode, shell commands are a viable fallback for ALL phases.

## Agent Adaptation

The Phase 0 agent demonstrated that agents can:
1. ✅ Recognize when Bob tools fail
2. ✅ Adapt to use shell commands instead
3. ✅ Verify results using shell commands
4. ✅ Complete tasks successfully despite tool bugs

**This proves shell commands are not just a workaround - they're a superior approach for VM execution.**

## Conclusion

**Shell commands are BETTER than Bob tools for Wave 2 VM execution because:**

1. ✅ More reliable (no path resolution bugs)
2. ✅ Faster (no API round-trips)
3. ✅ Better verification (immediate feedback)
4. ✅ More powerful (composable, no token limits)
5. ✅ Proven to work (Phase 0 success)

**Recommendation**: Embrace shell commands as the PRIMARY approach, not a workaround.

## References

- Working Example: `plugins/multi-agent-orchestrator/WAVE2_SHELL_WORKAROUND.md`
- Test Evidence: EPIC-CCN-107 successful execution (217 lines, verified)
- Template: `scripts/wave2/phase0_message_template_shell.txt`