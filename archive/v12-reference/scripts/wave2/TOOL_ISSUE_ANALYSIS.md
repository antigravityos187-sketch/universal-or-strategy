# Bob Shell Tool Issue Analysis - Phase 0 Mode

## Actual Test Results

### EPIC-CCN-107 Test (Original)

**What Actually Happened**:
1. Agent tried `write_to_file` tool - appeared to execute
2. Agent tried `read_file` to verify - **FAILED** with "File not found"
3. Agent fell back to `run_shell_command` with `cat >` - **SUCCESS**
4. Shell verification showed file exists with 217 lines
5. Agent tried `read_file` again - **STILL FAILED** with "File not found"

**Conclusion**: 
- `write_to_file` tool status: **UNKNOWN** (never verified to work)
- `read_file` tool status: **BROKEN** (can't see files that demonstrably exist)

### test_write_then_read.sh Test

**What Happened**:
1. Agent called `write_to_file` for test.md
2. Agent called `read_file` immediately after - **FAILED** with "File not found"
3. Agent reported: "write_to_file did NOT persist the file"

**Conclusion**: Either `write_to_file` doesn't work, OR `read_file` can't see newly created files.

## Root Cause Hypothesis

The `read_file` tool in Bob Shell appears to have a **caching or path resolution issue** when running in non-interactive mode via SSH. Possible causes:

1. **Working Directory Mismatch**: `read_file` may use a different working directory than shell commands
2. **File System Caching**: Tool may cache directory listings and not see new files
3. **Path Resolution Bug**: Tool may not correctly resolve relative paths in SSH context
4. **Timing Issue**: Tool may execute before file system sync completes

## Working Solution

Since `run_shell_command` with `cat` works reliably, the Phase 0 protocol should be updated:

### Current Protocol (Broken)
```
1. write_to_file docs/brain/EPIC-{ID}/00-hotspots.md
2. read_file docs/brain/EPIC-{ID}/00-hotspots.md (VERIFY)
3. write_to_file docs/brain/EPIC-{ID}/manifest.json
4. read_file docs/brain/EPIC-{ID}/manifest.json (VERIFY)
5. attempt_completion after BOTH read_file calls succeed
```

### Updated Protocol (Working)
```
1. run_shell_command: cat > docs/brain/EPIC-{ID}/00-hotspots.md << 'EOF' ... EOF
2. run_shell_command: cat docs/brain/EPIC-{ID}/00-hotspots.md | head -20 (VERIFY)
3. run_shell_command: cat > docs/brain/EPIC-{ID}/manifest.json << 'EOF' ... EOF
4. run_shell_command: cat docs/brain/EPIC-{ID}/manifest.json | head -20 (VERIFY)
5. attempt_completion after BOTH shell verifications succeed
```

## Impact Assessment

**Severity**: Medium
- Files ARE being created successfully
- Verification method is different but reliable
- No data loss or corruption

**Workaround**: Use shell commands for file I/O instead of Bob tools

**Long-term Fix Needed**: Bob Shell team needs to investigate why `read_file` fails in SSH/non-interactive context

## Recommendation

For Wave 2 Phase 0 launch:
1. Update phase0 message template to use shell commands
2. Document this as known limitation
3. Files will be created and verified successfully
4. Report issue to Bob Shell team for future fix