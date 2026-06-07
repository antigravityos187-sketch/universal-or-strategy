# [EPIC-7-QUALITY-010] File I/O Security: Path Validation

## Priority: P1 HIGH

## Labels
`security`, `file-io`, `P1`, `epic-7-quality-phase2`, `path-traversal`

## Summary
13 file I/O operations lack path validation, creating path traversal vulnerabilities and race condition risks.

## Affected Files
- [`src/V12_002.UI.Compliance.cs`](../../src/V12_002.UI.Compliance.cs)
  - Line 152: `File.WriteAllText(_csvPath, ...)` - CSV write without validation
  - Line 834: `File.WriteAllText(path, jsonPayload)` - User-provided path
- [`src/V12_002.StickyState.cs`](../../src/V12_002.StickyState.cs)
  - Line 73: `File.WriteAllText(tempPath, ...)` - Temp file write
  - Line 117: `File.ReadAllText(_stickyStatePath, ...)` - State load
  - Line 131: `File.ReadAllText(_stickyStatePath, ...)` - Backup load
  - Line 198: `File.ReadAllText(backupPath, ...)` - Backup restore
- [`src/V12_002.Lifecycle.cs`](../../src/V12_002.Lifecycle.cs)
  - Line 457-458: `Path.Combine` + `Directory.CreateDirectory` - Log directory creation
  - Line 462-463: `Directory.Exists` + `Directory.CreateDirectory` - TOCTOU vulnerability
  - Line 638-645: Multiple `Path.Combine` calls - Log path construction
  - Line 768-773: `Path.Combine` + state path construction

## Security Impact
- **Severity:** HIGH
- **Risk Categories:**
  1. **Path Traversal** - No validation that paths stay within intended directories
     - Example: User provides `../../../../Windows/System32/config.json`
  2. **TOCTOU (Time-of-Check-Time-of-Use)** - Race condition between `Directory.Exists` and `Directory.CreateDirectory`
  3. **Symlink Attacks** - No validation that paths aren't symlinks to sensitive files
  4. **Insufficient Error Handling** - File write failures may corrupt state

## V12 DNA Violation
Violates **Zero-Trust Architecture**: "Do not trust file system state" - assumes paths are safe without validation.

## Root Cause Analysis
File I/O operations trust user input and environment variables without validation:
1. **User-provided paths** (Compliance.cs:834) - No sanitization
2. **Environment-based paths** (MyDocuments) - Assumes safe location
3. **Temp file paths** - No atomic write protection
4. **Directory creation** - TOCTOU race condition

## Remediation Approach

### Phase 1: Path Validation Helper (Foundation)
Create `FileSystemHelpers.cs` with secure path operations:

```csharp
// V12.EPIC-7-QUALITY-010: Secure file I/O helpers
public static class FileSystemHelpers
{
    private static readonly string[] AllowedBasePaths = new[]
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NinjaTrader 8", "logs"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NinjaTrader 8", "state")
    };

    /// <summary>
    /// Validates that a path is within allowed directories and doesn't contain traversal sequences.
    /// </summary>
    public static bool IsPathSafe(string path, out string canonicalPath)
    {
        try
        {
            // Canonicalize path (resolves .., symlinks, etc.)
            canonicalPath = Path.GetFullPath(path);
            
            // Check for path traversal attempts
            if (canonicalPath.Contains(".."))
                return false;
            
            // Verify path is within allowed base paths
            foreach (var basePath in AllowedBasePaths)
            {
                if (canonicalPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            
            return false;
        }
        catch
        {
            canonicalPath = null;
            return false;
        }
    }

    /// <summary>
    /// Atomically writes content to a file (write to temp, then move).
    /// </summary>
    public static void WriteAllTextAtomic(string path, string content)
    {
        if (!IsPathSafe(path, out string safePath))
            throw new SecurityException($"Path validation failed: {path}");
        
        string tempPath = safePath + ".tmp";
        
        try
        {
            // Write to temp file
            File.WriteAllText(tempPath, content, Encoding.UTF8);
            
            // Atomic move (overwrites destination)
            File.Move(tempPath, safePath, overwrite: true);
        }
        catch
        {
            // Cleanup temp file on failure
            try { File.Delete(tempPath); } catch { }
            throw;
        }
    }

    /// <summary>
    /// Safely creates a directory (handles race conditions).
    /// </summary>
    public static void CreateDirectorySafe(string path)
    {
        if (!IsPathSafe(path, out string safePath))
            throw new SecurityException($"Path validation failed: {path}");
        
        try
        {
            Directory.CreateDirectory(safePath);
        }
        catch (IOException ex) when (Directory.Exists(safePath))
        {
            // Race condition: another thread created it - this is OK
            return;
        }
    }
}
```

### Phase 2: Replace Unsafe Operations

**StickyState.cs:**
```csharp
// BEFORE (Line 73):
File.WriteAllText(tempPath, jsonWithChecksum, Encoding.UTF8);

// AFTER:
FileSystemHelpers.WriteAllTextAtomic(_stickyStatePath, jsonWithChecksum);
```

**Compliance.cs:**
```csharp
// BEFORE (Line 834):
System.IO.File.WriteAllText(path, jsonPayload);

// AFTER:
if (!FileSystemHelpers.IsPathSafe(path, out string safePath))
{
    Print($"[COMPLIANCE] ERROR: Invalid export path: {path}");
    return;
}
FileSystemHelpers.WriteAllTextAtomic(safePath, jsonPayload);
```

**Lifecycle.cs:**
```csharp
// BEFORE (Lines 462-463):
if (!System.IO.Directory.Exists(logsDirInit))
    System.IO.Directory.CreateDirectory(logsDirInit);

// AFTER:
FileSystemHelpers.CreateDirectorySafe(logsDirInit);
```

### Phase 3: Additional Security Measures
1. **Whitelist allowed file extensions** - Only `.json`, `.csv`, `.v12state`
2. **Add file size limits** - Prevent disk exhaustion attacks
3. **Implement file locking** - Prevent concurrent writes
4. **Add integrity checks** - Validate file content after write

## Acceptance Criteria
- [ ] `FileSystemHelpers.cs` created with path validation
- [ ] All 13 file I/O operations use secure helpers
- [ ] Path traversal attempts blocked (unit test with `../../../` paths)
- [ ] TOCTOU race conditions eliminated
- [ ] Atomic writes implemented for all state files
- [ ] Build passes with 0 warnings
- [ ] Security test suite passes (path traversal, symlink, TOCTOU tests)

## Testing Strategy
1. **Unit Tests**:
   - Path traversal attempts (`../../../Windows/System32/config.json`)
   - Symlink attacks (create symlink to sensitive file)
   - Invalid characters in paths (`\0`, `<`, `>`, `|`)
   - Paths outside allowed directories
2. **Integration Tests**:
   - TOCTOU race condition (concurrent directory creation)
   - Atomic write verification (kill process mid-write)
   - Disk full scenario (verify cleanup)
3. **Stress Tests**:
   - 1000 concurrent file writes
   - Rapid directory creation/deletion

## Effort Estimate
**6-8 hours**
- 2h: Create `FileSystemHelpers.cs` with validation logic
- 2h: Replace all unsafe file I/O operations
- 1h: Add file size limits and extension whitelist
- 1-2h: Testing and verification

## Dependencies
- **TICKET-001** (Phase 1) - May need environment variables for allowed paths
- Blocks **TICKET-007** - State persistence error handling depends on atomic writes
- Blocks **TICKET-011** - Retry logic should use validated paths

## Blockers
- None

## References
- OWASP: [Path Traversal](https://owasp.org/www-community/attacks/Path_Traversal)
- CWE-22: Improper Limitation of a Pathname to a Restricted Directory
- CWE-367: Time-of-check Time-of-use (TOCTOU) Race Condition
- V12 DNA: [SECURITY.md](../../SECURITY.md) Lines 27-32 (Zero-Trust Reminders)

## Implementation Notes
- Use `Path.GetFullPath()` for canonicalization (resolves `..`, symlinks)
- `File.Move(..., overwrite: true)` is atomic on Windows (POSIX rename semantics)
- Consider using `FileStream` with `FileShare.None` for exclusive access
- Add `[SecuritySafeCritical]` attribute if needed for partial trust scenarios

## Jane Street Alignment
This ticket enforces Jane Street's **defensive I/O** principles:
1. **Validate all inputs** - Never trust file paths from environment or user
2. **Fail-fast on invalid state** - Throw `SecurityException` on path validation failure
3. **Atomic operations** - Prevent partial writes that corrupt state
4. **Explicit error handling** - No silent failures in file I/O

## Status
🔴 **OPEN** - Not Started

## Assigned To
_Unassigned_ (Recommend: Advanced mode)

## Created
2026-05-26T17:07:00Z