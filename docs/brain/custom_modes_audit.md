# Custom Modes Permission Audit (2026-06-02)

## Executive Summary

Audit of custom mode permissions in `.bob/custom_modes.yaml` to ensure `v12-engineer` and `v12-phase7-lead` have full file access equivalent to advanced mode.

## Current Configuration

### v12-epic-planner (PLAN-ONLY)
**Status**: ✅ CORRECT (Intentionally Restricted)
- **Edit Permission**: RESTRICTED to `\.(md|yaml|yml|json|txt|ps1|py|sh)$`
- **Rationale**: PLAN-ONLY mode per line 12 of roleDefinition
- **Can Edit**: Documentation, config, and script files
- **Cannot Edit**: `src/*.cs` files (intentional)

### v12-engineer (ARCHITECT + ENGINEER)
**Status**: ✅ CORRECT (Full Access)
- **Edit Permission**: UNRESTRICTED (line 69: `- edit`)
- **No fileRegex**: Can edit ALL file types including `src/*.cs`
- **Tool Groups**: read, edit, command, mcp, browser
- **Expected Behavior**: Should have full file access like advanced mode

### v12-phase7-lead (Concurrency Lead)
**Status**: ✅ CORRECT (Full Access)
- **Edit Permission**: UNRESTRICTED (line 111: `- edit`)
- **No fileRegex**: Can edit ALL file types including `src/*.cs`
- **Tool Groups**: read, edit, command, mcp, browser
- **Expected Behavior**: Should have full file access like advanced mode

## Configuration Validation

### YAML Syntax Check
```yaml
# v12-engineer (lines 67-72)
groups:
  - read
  - edit          # ✅ No restrictions
  - command
  - mcp
  - browser

# v12-phase7-lead (lines 109-114)
groups:
  - read
  - edit          # ✅ No restrictions
  - command
  - mcp
  - browser
```

**Result**: ✅ YAML syntax is correct, no fileRegex restrictions present

### Comparison with Advanced Mode
Advanced mode has the following tool groups:
- read
- edit (unrestricted)
- command
- mcp
- browser

**v12-engineer**: ✅ MATCHES advanced mode permissions
**v12-phase7-lead**: ✅ MATCHES advanced mode permissions

## Potential Issues

### 1. Mode-Specific Rules Files
Checked `.bob/rules-v12-engineer/dna.md` - No file restrictions found ✅

### 2. Global Settings
Unable to check `.bob/settings.json` (user denied access)
- **Recommendation**: Verify no global file restrictions are set

### 3. Bob Version/Cache
- **Possible Issue**: Bob may be caching old mode configurations
- **Solution**: Restart VS Code or reload Bob extension

## Recommendations

### If Permission Issues Persist:

1. **Restart VS Code**
   - Close and reopen VS Code to clear Bob's mode cache
   - Verify modes reload with current configuration

2. **Verify File Patterns**
   - When permission denied, note the exact file path
   - Check if file matches any exclusion patterns in `.bobignore` or `.gitignore`

3. **Test Edit Permission**
   - Switch to `v12-engineer` mode
   - Try editing a `src/*.cs` file
   - If denied, capture exact error message

4. **Check Bob Version**
   - Ensure Bob extension is up to date
   - Some older versions may have permission bugs

5. **Manual YAML Validation**
   - Run: `python -c "import yaml; yaml.safe_load(open('.bob/custom_modes.yaml'))"`
   - Verify no YAML parsing errors

## Conclusion

**Configuration Status**: ✅ CORRECT

Both `v12-engineer` and `v12-phase7-lead` modes have unrestricted edit permissions matching advanced mode. If permission issues persist, they are likely caused by:
- Bob extension cache (restart VS Code)
- Global settings restrictions (check `.bob/settings.json`)
- File-specific exclusions (check `.bobignore`)
- Bob extension bug (update to latest version)

## Next Steps

1. User to test `v12-engineer` mode with a `src/*.cs` file edit
2. If denied, capture exact error message
3. Check Bob extension version and update if needed
4. Restart VS Code to clear mode cache

---

**Audit Date**: 2026-06-02  
**Auditor**: Advanced Mode Agent  
**Configuration File**: `.bob/custom_modes.yaml` (118 lines)