# Bob Shell read_file Tool Issue on VM

## Problem

When running Bob Shell on the VM via SSH, the `read_file` tool consistently fails with "File not found" even when files demonstrably exist (verified via `ls` and `cat` shell commands).

## Evidence from EPIC-CCN-107 Test

```bash
# Shell command shows file exists:
$ ls -lah /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-107/
-rw-r--r-- 1 malhitticrypto malhitticrypto 9.1K Jun 13 01:08 00-hotspots.md

$ wc -l /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-107/00-hotspots.md
217 /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-107/00-hotspots.md

# But read_file tool fails:
[using tool read_file: docs/.../00-hotspots.md]
Error executing tool read_file: File not found
```

## Root Cause Hypothesis

1. **Path Resolution**: Bob Shell's `read_file` tool may use a different working directory than shell commands
2. **SSH Context**: Tool may not handle remote file paths correctly when invoked via `gcloud compute ssh`
3. **Caching**: Tool may cache directory listings and not see newly created files
4. **Relative vs Absolute**: Tool may require relative paths from workspace root

## Workaround

Use shell commands for verification instead of `read_file`:

```bash
# Verify file exists and show first 20 lines:
cat /path/to/file | head -20

# Or just check file exists:
ls -lah /path/to/file
```

## Impact on Phase 0 Protocol

**Temporary Protocol Adjustment**:
- Use `run_shell_command` with `cat` or `ls` for verification instead of `read_file`
- Agent should report: "Files verified via shell commands (read_file tool has VM path issue)"
- This is acceptable since files are demonstrably created and readable

## Status

- **Issue**: Confirmed on v12-test-golden-v2 VM (2026-06-13)
- **Workaround**: Use shell commands for verification
- **Impact**: Low - files are created successfully, only verification method differs
- **Action**: Document in Phase 0 completion reports