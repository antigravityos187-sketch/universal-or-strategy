# Jane Street Knowledge Base Sync Protocol

**Version**: 2026-06-03  
**Status**: Infrastructure Ready (Testing Pending)  
**Owner**: V12 Universal OR Strategy

## Overview

The Jane Street Sync system provides reproducible infrastructure for cloning, indexing, and extracting knowledge from 22 Jane Street OCaml repositories. This creates a searchable knowledge base aligned with V12 DNA principles (correctness by construction, lock-free patterns, microsecond-latency optimization).

### Purpose

- **Knowledge Extraction**: Mine Jane Street's open-source repos for HFT patterns, testing standards, and architectural principles
- **RAG Integration**: Feed extracted intel into Firestore for agent query via `query_kb.py`
- **Reproducibility**: Mise-managed tasks ensure consistent setup across environments
- **Incremental Sync**: Idempotent design allows safe re-runs and partial updates

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Mise Task Layer                          │
│  jane-street-sync, jane-street-sync-tier1/tier2, etc.       │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│              jane_street_sync.ps1                           │
│  Orchestration: Clone → Index → Extract → Upload            │
└────┬────────────────────┬────────────────────┬──────────────┘
     │                    │                    │
     ▼                    ▼                    ▼
┌─────────┐      ┌──────────────┐    ┌─────────────────┐
│  Git    │      │ jCodemunch   │    │ extract_jane_   │
│  Clone  │      │ index_folder │    │ street_docs.py  │
└─────────┘      └──────────────┘    └────────┬────────┘
                                              │
                                              ▼
                                     ┌─────────────────┐
                                     │ upload_jane_    │
                                     │ street_intel.py │
                                     └────────┬────────┘
                                              │
                                              ▼
                                     ┌─────────────────┐
                                     │   Firestore     │
                                     │  jane-street-   │
                                     │  repo-intel     │
                                     └─────────────────┘
```

## Prerequisites

### Required Tools

1. **jCodemunch MCP Server**
   ```bash
   npm install -g jcodemunch-mcp
   ```

2. **Python 3.12** (via Mise)
   ```bash
   mise install python@3.12
   ```

3. **Git** (via Mise)
   ```bash
   mise install core:git
   ```

4. **Firebase Credentials**
   - Place `firebase-credentials.json` in project root
   - Same credentials used by `scripts/query_kb.py`

### Verification

```bash
# Check all prerequisites
mise run doctor

# Verify jCodemunch
jcodemunch --version

# Verify Firestore access
python scripts/query_kb.py "test"
```

## Repository Tiers

### Tier 1: Core Infrastructure (12 repos, ~6 hours)

| Repository | Purpose | Priority |
|------------|---------|----------|
| `base` | Foundation types, utilities | P0 |
| `core` | Standard library replacement | P0 |
| `core_kernel` | Portable core subset | P0 |
| `async` | Cooperative concurrency | P1 |
| `async_kernel` | Async core primitives | P1 |
| `expect_test` | Inline testing framework | P1 |
| `core_bench` | Microbenchmarking | P2 |
| `incremental` | Self-adjusting computation | P2 |
| `memtrace` | Memory profiling | P2 |
| `hardcaml` | Hardware design DSL | P3 |
| `time_now` | Fast time access | P3 |
| `ppx_inline_test` | Test PPX rewriter | P3 |

**Estimated Time**: 6 hours (4 parallel jobs)

### Tier 2: Extended Ecosystem (10 repos, ~5 hours)

| Repository | Purpose | Priority |
|------------|---------|----------|
| `async_unix` | Unix-specific async | P1 |
| `async_rpc` | RPC framework | P1 |
| `bin_prot` | Binary protocol | P2 |
| `sexplib` | S-expression library | P2 |
| `patience_diff` | Diff algorithm | P3 |
| `re2` | Regular expressions | P3 |
| `ppx_assert` | Assert PPX | P3 |
| `ppx_bench` | Benchmark PPX | P3 |
| `core_profiler` | Profiling tools | P3 |
| `textutils` | Text utilities | P3 |

**Estimated Time**: 5 hours (4 parallel jobs)

## Usage

### Full Sync (All 22 Repos)

```bash
# Complete pipeline: clone + index + extract + upload
mise run jane-street-sync

# Estimated time: 11 hours
# Output: ~/.jane-street/ with 22 indexed repos
```

### Tier-Based Sync

```bash
# Tier 1 only (core infrastructure)
mise run jane-street-sync-tier1

# Tier 2 only (extended ecosystem)
mise run jane-street-sync-tier2

# Combine both for full coverage
mise run jane-street-sync-tier1
mise run jane-street-sync-tier2
```

### Extract-Only Mode

```bash
# Skip cloning/indexing, only extract docs
# (Requires repos already cloned and indexed)
mise run jane-street-sync-extract
```

### Check Sync Status

```bash
# View sync progress and repo status
mise run jane-street-status

# Output: JSON table with status per repo
```

## Output Structure

### Directory Layout

```
~/.jane-street/
├── .sync-status.json          # Sync metadata
├── .sync-complete             # Completion marker
├── base/
│   ├── .git/                  # Git repo
│   ├── src/                   # Source code
│   ├── .jcodemunch/           # jCodemunch index
│   └── extracted-docs.json    # Extracted intel
├── core/
│   └── ...
└── [20 more repos]
```

### Sync Status Format

```json
{
  "version": "2026-06-03",
  "last_sync": "2026-06-03T12:00:00Z",
  "repos": {
    "base": {
      "status": "indexed",
      "message": "Successfully indexed",
      "timestamp": "2026-06-03T12:30:00Z"
    },
    "core": {
      "status": "clone_failed",
      "message": "Network timeout",
      "timestamp": "2026-06-03T12:45:00Z"
    }
  }
}
```

### Extracted Docs Format

```json
{
  "repo": "core_bench",
  "extracted_at": "2026-06-03T13:00:00Z",
  "readme": "# Core_bench\n\nMicrobenchmarking library...",
  "design": "##  Design Principles\n\n...",
  "comments": [
    {
      "file": "src/bench.ml",
      "comment": "This module provides..."
    }
  ],
  "commits": [
    {
      "hash": "abc12345",
      "author": "Jane Street Developer",
      "date": "2026-05-15T10:30:00Z",
      "message": "Optimize allocation patterns"
    }
  ],
  "metadata": {
    "total_files": 127,
    "ocaml_files": 89,
    "markdown_files": 12,
    "size_kb": 2048.5
  }
}
```

## Error Handling

### Common Issues

1. **jCodemunch Not Found**
   ```
   ERROR: jcodemunch not found in PATH
   Solution: npm install -g jcodemunch-mcp
   ```

2. **Git Clone Timeout**
   ```
   Clone failed: Network timeout
   Solution: Re-run sync (idempotent), check network
   ```

3. **Firestore Upload Failed**
   ```
   WARNING: Firestore upload failed
   Solution: mise run jane-street-sync-extract (retry upload only)
   ```

4. **Index Corruption**
   ```
   Index failed: Corrupted cache
   Solution: Delete ~/.jane-street/<repo>/.jcodemunch/, re-run
   ```

### Recovery Procedures

**Partial Failure Recovery**:
```bash
# Check what failed
mise run jane-street-status

# Re-run sync (only processes failed repos)
mise run jane-street-sync
```

**Complete Reset**:
```bash
# Nuclear option: delete everything and restart
rm -rf ~/.jane-street/
mise run jane-street-sync
```

**Extract-Only Recovery**:
```bash
# If cloning/indexing succeeded but upload failed
mise run jane-street-sync-extract
```

## Performance Optimization

### Parallel Processing

- **Default**: 4 parallel jobs
- **Custom**: Modify `$ParallelJobs` in `jane_street_sync.ps1`
- **Memory**: Each job uses ~500MB RAM during indexing

### Rate Limiting

- **Git Clones**: 2-second delay between repos
- **jCodemunch**: No artificial limits (tool handles internally)
- **Firestore**: 500 writes/second (per Firestore limits)

### Disk Usage

- **Per Repo**: ~50-200MB (source + index)
- **Total**: ~3-5GB for all 22 repos
- **Cleanup**: Safe to delete `~/.jane-street/` after Firestore upload

## Integration with V12 Workflow

### Agent Query Integration

```bash
# Query extracted Jane Street knowledge
python scripts/query_kb.py "async patterns"
python scripts/query_kb.py "testing strategies"
python scripts/query_kb.py "memory allocation"
```

### Bob CLI Integration

```bash
# Bob can query Jane Street KB during architectural decisions
bob --mode v12-engineer
# Within Bob session:
/query-kb "lock-free data structures"
```

### Epic Planning Integration

- **Before EPIC**: Query relevant Jane Street patterns
- **During Design**: Reference extracted design docs
- **Code Review**: Validate against Jane Street standards

## Maintenance

### Regular Updates

```bash
# Monthly sync to get latest commits
mise run jane-street-sync

# The sync is idempotent - safe to re-run
```

### Version Management

- **Sync Version**: Tracked in `JANE_STREET_VERSION` env var
- **Schema Changes**: Update version when changing extraction format
- **Backward Compatibility**: Maintain for 2 versions

### Monitoring

```bash
# Check sync health
mise run jane-street-status

# Verify Firestore collection
python scripts/query_kb.py ""  # List all docs
```

## Troubleshooting

### Debug Mode

```bash
# Enable verbose logging
$env:JANE_STREET_DEBUG = "true"
mise run jane-street-sync-tier1
```

### Manual Intervention

```bash
# Clone single repo manually
git clone https://github.com/janestreet/base.git ~/.jane-street/base

# Index single repo manually
jcodemunch index_folder --path ~/.jane-street/base

# Extract single repo manually
python scripts/extract_jane_street_docs.py  # Processes all indexed repos
```

### Log Analysis

```bash
# PowerShell logs include timestamps and color coding
# Look for patterns:
# [ERROR] - Critical failures requiring intervention
# [WARN]  - Non-critical issues, sync continues
# [INFO]  - Progress updates
# [DEBUG] - Detailed operation info
```

## Security Considerations

### Credentials

- **Firestore**: Uses service account JSON (same as `query_kb.py`)
- **Git**: Public repos, no authentication required
- **Storage**: Local `~/.jane-street/` directory (user-owned)

### Data Privacy

- **Source Code**: Public Jane Street repositories only
- **Extraction**: README, DESIGN, comments, commit messages (public data)
- **Upload**: Firestore collection `jane-street-repo-intel` (project-scoped)

### Rate Limiting

- **GitHub**: Respects rate limits via 2-second delays
- **Firestore**: Stays within 500 writes/second limit
- **jCodemunch**: Uses tool's built-in throttling

## Success Criteria

### Infrastructure Complete ✅

- [x] `.mise.toml` updated with sync tasks
- [x] `scripts/jane_street_sync.ps1` orchestration script
- [x] `scripts/extract_jane_street_docs.py` extraction logic
- [x] `scripts/upload_jane_street_intel.py` Firestore integration
- [x] `docs/protocol/JANE_STREET_SYNC.md` documentation

### Testing Pending (Subtask 0.2)

- [ ] `mise run jane-street-sync-tier1` completes without errors
- [ ] All 12 Tier 1 repos cloned and indexed
- [ ] `~/.jane-street/.sync-status.json` shows "indexed" status
- [ ] Extracted docs saved to JSON files
- [ ] Firestore upload successful
- [ ] Query integration verified

## Next Steps

1. **Subtask 0.2**: Test Tier 1 sync (12 repos)
2. **Subtask 0.3**: Test full sync (22 repos)
3. **Subtask 0.4**: Integrate with Bob CLI query system
4. **Subtask 0.5**: Create agent training materials

---

**Note**: This infrastructure is ready for testing. Do NOT run the actual sync yet - wait for Subtask 0.2 testing phase.