# extract_pr_forensics.ps1 Enhancement Plan

## Current Gaps

The script uses `gh pr view` but misses several comment types:

1. **Issue Comments**: PR-level comments (not in reviews)
2. **Review Summaries**: Top-level review comments
3. **Inline Comments**: Currently parsed from text, should use structured API

## Proposed Enhancement

### Phase 1: Add Missing Comment Types (Quick Win)

**Current (line 25)**:
```powershell
gh pr view $PrNumber --json comments,reviews,statusCheckRollup
```

**Enhanced**:
```powershell
# Fetch ALL comment types in one call
gh pr view $PrNumber --json comments,reviews,reviewThreads,latestReviews
```

**New fields**:
- `reviewThreads`: Inline review comments (structured, not text-parsed)
- `latestReviews`: Review summaries (currently missed)

### Phase 2: Deduplicate Across Sources

**Problem**: Same comment might appear in multiple fields (e.g., review summary in both `reviews` and `latestReviews`)

**Solution**: Add deduplication by comment ID or body hash:
```powershell
$seenComments = @{}
foreach ($comment in $allComments) {
    $hash = ($comment.body -replace '\s', '').GetHashCode()
    if ($seenComments.ContainsKey($hash)) {
        continue  # Skip duplicate
    }
    $seenComments[$hash] = $true
    # Process comment...
}
```

### Phase 3: Remove Text Parsing (lines 189-253)

**Current**: Regex parsing of `gh pr view --comments` output
**Enhanced**: Use structured `reviewThreads` field instead

**Benefits**:
- More reliable (no regex brittleness)
- Captures all metadata (line numbers, file paths, comment IDs)
- Faster (no text parsing overhead)

## Implementation Steps

1. **Test on PR #25** (known to have 53 comments):
   ```powershell
   gh pr view 25 --json comments,reviews,reviewThreads,latestReviews | ConvertFrom-Json
   ```

2. **Count comments per field**:
   - `comments`: Issue comments
   - `reviews[].body`: Review summaries
   - `reviewThreads[].comments[]`: Inline comments
   - `latestReviews[].body`: Latest review summaries

3. **Update script** (lines 20-253):
   - Replace line 25 with enhanced JSON fetch
   - Add deduplication logic
   - Remove text parsing (lines 189-253)
   - Add structured inline comment processing

4. **Verify coverage**:
   - Run on PR #25
   - Confirm 53 comments extracted (30 review + 10 issue + 13 summaries)
   - Compare against manual GitHub UI count

## Expected Outcome

**Before**: ~40-45 comments extracted (misses issue comments + summaries)
**After**: 53 comments extracted (100% coverage)

**Token Savings**: ~500 tokens per PR (no text parsing overhead)

## Integration with pr-loop

**Step 1 (line 22 in pr-loop command)**:
```
1. Run: powershell -File .\scripts\extract_pr_forensics.ps1 -PrNumber $1
```

**No changes needed** - enhancement is transparent to pr-loop workflow.

## Timeline

- **Phase 1**: 30 minutes (add missing fields)
- **Phase 2**: 15 minutes (deduplication)
- **Phase 3**: 45 minutes (remove text parsing)
- **Total**: ~90 minutes

## Risk Assessment

- **Low Risk**: Changes are additive (won't break existing functionality)
- **High Reward**: 100% comment coverage + faster execution
- **Rollback**: Keep current version as `extract_pr_forensics_v1.ps1` backup