# Wave 1 Phase 0 - Execution Strategy Change

**Date**: 2026-06-14 06:13 UTC
**Status**: ⚠️ Strategy Change Required

---

## User Request

User wants to change execution strategy for remaining 10 epics (EPIC-006 to EPIC-015):

1. **Model Name Reporting**: Agents should report their model name along with cost
2. **Foreground Execution**: Run agents in foreground (not screen sessions) so user can watch

---

## Current Situation

### What We've Done (EPIC-001 to EPIC-005)
- Used **screen sessions** (background execution)
- Agents run autonomously without user visibility
- Cost reported in logs after completion
- Model name: Not explicitly reported (Bob Shell default)

### What User Wants (EPIC-006 to EPIC-015)
- **Foreground execution** (user can watch in real-time)
- **Model name + cost** reported by each agent
- Interactive visibility during execution

---

## Implementation Challenges

### Challenge 1: Foreground vs Background
**Current**: `screen -dmS p0-001 bash -l -c 'script.sh'` (background)
**Requested**: Direct execution without screen (foreground)

**Problem**: Can only run 1 epic at a time in foreground
- 10 epics × 2 minutes each = 20 minutes sequential
- vs 10 epics × 2 minutes parallel = 2 minutes total

**Options**:
- **A**: Run 1 epic at a time in foreground (20 min total, full visibility)
- **B**: Run in screen but attach to watch (2 min total, partial visibility)
- **C**: Use tmux with split panes (2 min total, full visibility for all)

### Challenge 2: Model Name Reporting
**Current**: Bob Shell doesn't explicitly report model name in logs
**Requested**: Include model name in cost report

**Solution**: Modify prompt to request model name in attempt_completion:
```
Cost: X.XX bobcoins | Balance: Y.YY bobcoins | Model: claude-sonnet-4-6
```

---

## Recommended Approach

### Option A: Sequential Foreground (Simplest)
**Pros**:
- ✅ Full visibility for each epic
- ✅ Easy to implement (no screen/tmux)
- ✅ User can watch every step

**Cons**:
- ⚠️ 20 minutes total (vs 2 minutes parallel)
- ⚠️ Must wait for each epic to complete

**Implementation**:
```bash
# Run epics one at a time
for epic in 006 007 008 009 010 011 012 013 014 015; do
    echo "=== Starting EPIC-$epic ==="
    bash _p0_$epic.sh
    echo "=== EPIC-$epic Complete ==="
done
```

### Option B: Screen with Attach (Compromise)
**Pros**:
- ✅ 2 minutes total (parallel execution)
- ✅ Can attach to watch any epic
- ✅ Proven to work (used for EPIC-001-005)

**Cons**:
- ⚠️ Can only watch 1 epic at a time
- ⚠️ Must detach/reattach to switch

**Implementation**:
```bash
# Launch all in screen
for epic in 006 007 008 009 010 011 012 013 014 015; do
    screen -dmS p0-$epic bash -l -c "_p0_$epic.sh"
done

# User can attach to watch
screen -r p0-006  # Watch EPIC-006
# Ctrl+A, D to detach
screen -r p0-007  # Watch EPIC-007
```

### Option C: Tmux Split Panes (Best of Both)
**Pros**:
- ✅ 2 minutes total (parallel execution)
- ✅ Watch all 10 epics simultaneously
- ✅ Split screen visibility

**Cons**:
- ⚠️ Requires tmux on VM
- ⚠️ Complex setup
- ⚠️ May be overwhelming (10 panes)

---

## Model Name Reporting

### Current Prompt (No Model Name)
```
Cost: 1.19 | Balance: 158.81
```

### Updated Prompt (With Model Name)
```
Cost: 1.19 bobcoins | Balance: 158.81 bobcoins | Model: claude-sonnet-4-6
```

**Implementation**: Add to Phase 0 prompt:
```
In your attempt_completion, you MUST report:
- Cost in bobcoins (e.g., "Cost: 1.19 bobcoins")
- Remaining balance (e.g., "Balance: 158.81 bobcoins")
- Model name (e.g., "Model: claude-sonnet-4-6")

Format: "Cost: X.XX bobcoins | Balance: Y.YY bobcoins | Model: <model-name>"
```

---

## Decision Required

**User, please choose**:

1. **Option A**: Sequential foreground (20 min, full visibility, simple)
2. **Option B**: Screen with attach (2 min, partial visibility, proven)
3. **Option C**: Tmux split panes (2 min, full visibility, complex)

**My Recommendation**: **Option A** (Sequential Foreground)
- Simplest to implement
- Full visibility as requested
- 20 minutes is acceptable for 10 epics
- No risk of missing output

---

## Next Steps (After Decision)

1. Update Phase 0 prompt to include model name reporting
2. Regenerate scripts with updated prompt
3. Execute using chosen strategy
4. Validate all 10 epics complete
5. Proceed to Phase 1

---

**Status**: ⚠️ BLOCKED - Awaiting user decision on execution strategy
**Current Cost**: $138.55
**Estimated Additional Cost**: ~12 bobcoins (10 epics × 1.2 each)